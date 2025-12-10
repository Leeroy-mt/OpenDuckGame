using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DbMon.NET;

/// <summary>
/// This class captures all strings passed to <c>OutputDebugString</c> when
/// the application is not debugged.	
/// </summary>
/// <remarks>	
/// This class is a port of Microsofts Visual Studio's C++ example "dbmon", which
/// can be found at <c>http://msdn.microsoft.com/library/default.asp?url=/library/en-us/vcsample98/html/vcsmpdbmon.asp</c>.
/// </remarks>
/// <remarks>
/// 	<code>
/// 		public static void Main(string[] args) {
/// 			DebugMonitor.Start();
/// 			DebugMonitor.OnOutputDebugString += new OnOutputDebugStringHandler(OnOutputDebugString);
/// 			Console.WriteLine("Press 'Enter' to exit.");
/// 			Console.ReadLine();
/// 			DebugMonitor.Stop();
/// 		}
///
/// 		private static void OnOutputDebugString(int pid, string text) {
/// 			Console.WriteLine(DateTime.Now + ": " + text);
/// 		}
/// 	</code>
/// </remarks>
public sealed class DebugMonitor
{
    private struct SECURITY_DESCRIPTOR
    {
        public byte revision;

        public byte size;

        public short control;

        public IntPtr owner;

        public IntPtr group;

        public IntPtr sacl;

        public IntPtr dacl;
    }

    private struct SECURITY_ATTRIBUTES
    {
        public int nLength;

        public IntPtr lpSecurityDescriptor;

        public int bInheritHandle;
    }

    [Flags]
    private enum PageProtection : uint
    {
        NoAccess = 1u,
        Readonly = 2u,
        ReadWrite = 4u,
        WriteCopy = 8u,
        Execute = 0x10u,
        ExecuteRead = 0x20u,
        ExecuteReadWrite = 0x40u,
        ExecuteWriteCopy = 0x80u,
        Guard = 0x100u,
        NoCache = 0x200u,
        WriteCombine = 0x400u
    }

    private const int WAIT_OBJECT_0 = 0;

    private const uint INFINITE = uint.MaxValue;

    private const int ERROR_ALREADY_EXISTS = 183;

    private const uint SECURITY_DESCRIPTOR_REVISION = 1u;

    private const uint SECTION_MAP_READ = 4u;

    /// <summary>
    /// Event handle for slot 'DBWIN_BUFFER_READY'
    /// </summary>
    private static IntPtr m_AckEvent = IntPtr.Zero;

    /// <summary>
    /// Event handle for slot 'DBWIN_DATA_READY'
    /// </summary>
    private static IntPtr m_ReadyEvent = IntPtr.Zero;

    /// <summary>
    /// Handle for our shared file
    /// </summary>
    private static IntPtr m_SharedFile = IntPtr.Zero;

    /// <summary>
    /// Handle for our shared memory
    /// </summary>
    private static IntPtr m_SharedMem = IntPtr.Zero;

    /// <summary>
    /// Our capturing thread
    /// </summary>
    private static Thread m_Capturer = null;

    /// <summary>
    /// Our synchronization root
    /// </summary>
    private static readonly Lock m_SyncRoot = new();

    /// <summary>
    /// Mutex for singleton check
    /// </summary>
    private static Mutex m_Mutex = null;

    /// <summary>
    /// Fired if an application calls <c>OutputDebugString</c>
    /// </summary>
    public static event OnOutputDebugStringHandler OnOutputDebugString;

    /// <summary>
    /// Private constructor so no one can create a instance
    /// of this static class
    /// </summary>
    private DebugMonitor()
    {
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool InitializeSecurityDescriptor(ref SECURITY_DESCRIPTOR sd, uint dwRevision);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool SetSecurityDescriptorDacl(ref SECURITY_DESCRIPTOR sd, bool daclPresent, IntPtr dacl, bool daclDefaulted);

    [DllImport("kernel32.dll")]
    private static extern IntPtr CreateEvent(ref SECURITY_ATTRIBUTES sa, bool bManualReset, bool bInitialState, string lpName);

    [DllImport("kernel32.dll")]
    private static extern bool PulseEvent(IntPtr hEvent);

    [DllImport("kernel32.dll")]
    private static extern bool SetEvent(IntPtr hEvent);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateFileMapping(IntPtr hFile, ref SECURITY_ATTRIBUTES lpFileMappingAttributes, PageProtection flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hHandle);

    [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
    private static extern int WaitForSingleObject(IntPtr handle, uint milliseconds);

    /// <summary>
    /// Starts this debug monitor
    /// </summary>
    public static void Start()
    {
        lock (m_SyncRoot)
        {
            if (m_Capturer != null)
            {
                throw new ApplicationException("This DebugMonitor is already started.");
            }
            if (!Environment.OSVersion.ToString().Contains("Microsoft"))
            {
                throw new NotSupportedException("This DebugMonitor is only supported on Microsoft operating systems.");
            }
            m_Mutex = new Mutex(initiallyOwned: false, typeof(DebugMonitor).Namespace, out bool createdNew);
            if (!createdNew)
            {
                throw new ApplicationException("There is already an instance of 'DbMon.NET' running.");
            }
            SECURITY_DESCRIPTOR sd = default;
            if (!InitializeSecurityDescriptor(ref sd, 1u))
            {
                throw CreateApplicationException("Failed to initializes the security descriptor.");
            }
            if (!SetSecurityDescriptorDacl(ref sd, daclPresent: true, IntPtr.Zero, daclDefaulted: false))
            {
                throw CreateApplicationException("Failed to initializes the security descriptor");
            }
            SECURITY_ATTRIBUTES sa = default;
            m_AckEvent = CreateEvent(ref sa, bManualReset: false, bInitialState: false, "DBWIN_BUFFER_READY");
            if (m_AckEvent == IntPtr.Zero)
            {
                throw CreateApplicationException("Failed to create event 'DBWIN_BUFFER_READY'");
            }
            m_ReadyEvent = CreateEvent(ref sa, bManualReset: false, bInitialState: false, "DBWIN_DATA_READY");
            if (m_ReadyEvent == IntPtr.Zero)
            {
                throw CreateApplicationException("Failed to create event 'DBWIN_DATA_READY'");
            }
            m_SharedFile = CreateFileMapping(new IntPtr(-1), ref sa, PageProtection.ReadWrite, 0u, 4096u, "DBWIN_BUFFER");
            if (m_SharedFile == IntPtr.Zero)
            {
                throw CreateApplicationException("Failed to create a file mapping to slot 'DBWIN_BUFFER'");
            }
            m_SharedMem = MapViewOfFile(m_SharedFile, 4u, 0u, 0u, 512u);
            if (m_SharedMem == IntPtr.Zero)
            {
                throw CreateApplicationException("Failed to create a mapping view for slot 'DBWIN_BUFFER'");
            }
            m_Capturer = new Thread(Capture);
            m_Capturer.Start();
        }
    }

    /// <summary>
    /// Captures 
    /// </summary>
    private static void Capture()
    {
        try
        {
            IntPtr pString = new(m_SharedMem.ToInt32() + Marshal.SizeOf<int>());
            while (true)
            {
                SetEvent(m_AckEvent);
                int ret = WaitForSingleObject(m_ReadyEvent, uint.MaxValue);
                if (m_Capturer != null)
                {
                    if (ret == 0)
                    {
                        FireOnOutputDebugString(Marshal.ReadInt32(m_SharedMem), Marshal.PtrToStringAnsi(pString));
                    }
                    continue;
                }
                break;
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            Dispose();
        }
    }

    private static void FireOnOutputDebugString(int pid, string text)
    {
        if (OnOutputDebugString == null)
        {
            return;
        }
        try
        {
            OnOutputDebugString(pid, text);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An 'OnOutputDebugString' handler failed to execute: " + ex.ToString());
        }
    }

    /// <summary>
    /// Dispose all resources
    /// </summary>
    private static void Dispose()
    {
        if (m_AckEvent != IntPtr.Zero)
        {
            if (!CloseHandle(m_AckEvent))
            {
                throw CreateApplicationException("Failed to close handle for 'AckEvent'");
            }
            m_AckEvent = IntPtr.Zero;
        }
        if (m_ReadyEvent != IntPtr.Zero)
        {
            if (!CloseHandle(m_ReadyEvent))
            {
                throw CreateApplicationException("Failed to close handle for 'ReadyEvent'");
            }
            m_ReadyEvent = IntPtr.Zero;
        }
        if (m_SharedFile != IntPtr.Zero)
        {
            if (!CloseHandle(m_SharedFile))
            {
                throw CreateApplicationException("Failed to close handle for 'SharedFile'");
            }
            m_SharedFile = IntPtr.Zero;
        }
        if (m_SharedMem != IntPtr.Zero)
        {
            if (!UnmapViewOfFile(m_SharedMem))
            {
                throw CreateApplicationException("Failed to unmap view for slot 'DBWIN_BUFFER'");
            }
            m_SharedMem = IntPtr.Zero;
        }

        m_Mutex?.Close();
        m_Mutex = null;
    }

    /// <summary>
    /// Stops this debug monitor. This call we block the executing thread
    /// until this debug monitor is stopped.
    /// </summary>
    public static void Stop()
    {
        lock (m_SyncRoot)
        {
            if (m_Capturer == null)
            {
                throw new ObjectDisposedException("DebugMonitor", "This DebugMonitor is not running.");
            }
            m_Capturer = null;
            PulseEvent(m_ReadyEvent);
            while (m_AckEvent != IntPtr.Zero)
            {
            }
        }
    }

    /// <summary>
    /// Helper to create a new application exception, which has automaticly the 
    /// last win 32 error code appended.
    /// </summary>
    /// <param name="text">text</param>
    private static ApplicationException CreateApplicationException(string text)
    {
        if (text == null || text.Length < 1)
        {
            throw new ArgumentNullException(nameof(text), "'text' may not be empty or null.");
        }
        return new ApplicationException($"{text}. Last Win32 Error was {Marshal.GetLastWin32Error()}");
    }
}
