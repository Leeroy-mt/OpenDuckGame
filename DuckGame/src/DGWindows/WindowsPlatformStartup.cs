using DuckGame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DGWindows;

internal class WindowsPlatformStartup
{
    public enum MachineType : ushort
    {
        IMAGE_FILE_MACHINE_UNKNOWN = 0,
        IMAGE_FILE_MACHINE_AM33 = 467,
        IMAGE_FILE_MACHINE_AMD64 = 34404,
        IMAGE_FILE_MACHINE_ARM = 448,
        IMAGE_FILE_MACHINE_EBC = 3772,
        IMAGE_FILE_MACHINE_I386 = 332,
        IMAGE_FILE_MACHINE_IA64 = 512,
        IMAGE_FILE_MACHINE_M32R = 36929,
        IMAGE_FILE_MACHINE_MIPS16 = 614,
        IMAGE_FILE_MACHINE_MIPSFPU = 870,
        IMAGE_FILE_MACHINE_MIPSFPU16 = 1126,
        IMAGE_FILE_MACHINE_POWERPC = 496,
        IMAGE_FILE_MACHINE_POWERPCFP = 497,
        IMAGE_FILE_MACHINE_R4000 = 358,
        IMAGE_FILE_MACHINE_SH3 = 418,
        IMAGE_FILE_MACHINE_SH3DSP = 419,
        IMAGE_FILE_MACHINE_SH4 = 422,
        IMAGE_FILE_MACHINE_SH5 = 424,
        IMAGE_FILE_MACHINE_THUMB = 450,
        IMAGE_FILE_MACHINE_WCEMIPSV2 = 361
    }

    private delegate IntPtr wine_version_delegate();

    public class Native
    {
        public struct ModuleInformation
        {
            public IntPtr lpBaseOfDll;

            public uint SizeOfImage;

            public IntPtr EntryPoint;
        }

        internal enum ModuleFilter
        {
            ListModulesDefault,
            ListModules32Bit,
            ListModules64Bit,
            ListModulesAll
        }

        [DllImport("psapi.dll")]
        public static extern bool EnumProcessModulesEx(IntPtr hProcess, [In][Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] IntPtr[] lphModule, int cb, [MarshalAs(UnmanagedType.U4)] out int lpcbNeeded, uint dwFilterFlag);

        [DllImport("psapi.dll")]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] uint nSize);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out ModuleInformation lpmodinfo, uint cb);
    }

    public class Module(string modulePath, IntPtr baseAddress, uint size)
    {
        public string ModulePath { get; set; } = modulePath;

        public IntPtr BaseAddress { get; set; } = baseAddress;

        public uint Size { get; set; } = size;
    }

    public struct DEVMODE
    {
        private const int CCHDEVICENAME = 32;

        private const int CCHFORMNAME = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;

        public short dmSpecVersion;

        public short dmDriverVersion;

        public short dmSize;

        public short dmDriverExtra;

        public int dmFields;

        public int dmPositionX;

        public int dmPositionY;

        public ScreenOrientation dmDisplayOrientation;

        public int dmDisplayFixedOutput;

        public short dmColor;

        public short dmDuplex;

        public short dmYResolution;

        public short dmTTOption;

        public short dmCollate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;

        public short dmLogPixels;

        public int dmBitsPerPel;

        public int dmPelsWidth;

        public int dmPelsHeight;

        public int dmDisplayFlags;

        public int dmDisplayFrequency;

        public int dmICMMethod;

        public int dmICMIntent;

        public int dmMediaType;

        public int dmDitherType;

        public int dmReserved1;

        public int dmReserved2;

        public int dmPanningWidth;

        public int dmPanningHeight;
    }

    public static List<string> _moduleDependencies =
    [
        "ntdll.dll", "MSCOREE.DLL", "KERNEL32.dll", "KERNELBASE.dll", "ADVAPI32.dll", "msvcrt.dll", "sechost.dll", "RPCRT4.dll", "mscoreei.dll", "SHLWAPI.dll",
        "kernel.appcore.dll", "VERSION.dll", "clr.dll", "USER32.dll", "win32u.dll", "VCRUNTIME140_CLR0400.dll", "ucrtbase_clr0400.dll", "GDI32.dll", "gdi32full.dll", "msvcp_win.dll",
        "ucrtbase.dll", "IMM32.DLL", "mscorlib.ni.dll", "ole32.dll", "combase.dll", "bcryptPrimitives.dll", "clrjit.dll", "OLEAUT32.dll", "System.ni.dll", "System.Core.ni.dll",
        "CRYPTSP.dll", "rsaenh.dll", "bcrypt.dll", "CRYPTBASE.dll", "Steam.dll", "MSVCR100.dll", "MSVCP100.dll", "steam_api.dll", "SHELL32.dll", "System.Drawing.ni.dll",
        "System.Windows.Forms.ni.dll", "windows.storage.dll", "Wldp.dll", "SHCORE.dll", "steamclient.dll", "WS2_32.dll", "CRYPT32.dll", "imagehlp.dll", "WINMM.dll", "PSAPI.DLL",
        "IPHLPAPI.DLL", "SETUPAPI.dll", "cfgmgr32.dll", "tier0_s.dll", "vstdlib_s.dll", "MSWSOCK.dll", "Secur32.dll", "SSPICLI.DLL", "gameoverlayrenderer.dll", "Microsoft.Xna.Framework.dll",
        "WindowsCodecs.dll", "X3DAudio1_7.dll", "Microsoft.Xna.Framework.Graphics.dll", "d3d9.dll", "d3dx9_41.dll", "dwmapi.dll", "uxtheme.dll", "nvldumd.dll", "msasn1.dll", "cryptnet.dll",
        "WINTRUST.DLL", "nvd3dum.dll", "System.Configuration.ni.dll", "System.Xml.ni.dll", "profapi.dll", "comctl32.dll", "gdiplus.dll", "DWrite.dll", "MSCTF.dll", "gpapi.dll",
        "dxcore.dll", "textinputframework.dll", "CoreMessaging.dll", "CoreUIComponents.dll", "wintypes.dll", "ntmarta.dll", "Oleacc.dll", "Accessibility.ni.dll", "clbcatq.dll", "sxs.dll",
        "MMDevApi.dll", "DEVOBJ.dll", "AUDIOSES.DLL", "powrprof.dll", "UMPDC.dll", "Windows.UI.dll", "InputHost.dll", "WindowManagementAPI.dll", "twinapi.appcore.dll", "PROPSYS.dll",
        "xinput1_3.dll", "DGInput.dll", "DINPUT8.dll", "HID.DLL", "amsi.dll", "USERENV.dll", "MpOav.dll", "MPCLIENT.DLL", "wbemprox.dll", "wbemcomn.dll",
        "wbemsvc.dll", "fastprox.dll", "System.IO.Compression.ni.dll", "winmmbase.dll", "wdmaud.drv", "ksuser.dll", "AVRT.dll", "msacm32.drv", "MSACM32.dll", "midimap.dll",
        "resourcepolicyclient.dll", "System.Speech.ni.dll", "sapi.dll", "msdmo.dll"
    ];

    private static string kWineVersion = null;

    public static List<string> assemblyLoadStrings = [];

    private const int ENUM_CURRENT_SETTINGS = -1;

    private const int ENUM_REGISTRY_SETTINGS = -2;

    public static int displayRefreshRate;

    public static bool IsRunningWine => kWineVersion != null;

    public static string WineVersion => kWineVersion;

    public static MachineType GetDllMachineType(string dllPath)
    {
        FileStream fileStream = new(dllPath, FileMode.Open, FileAccess.Read);
        BinaryReader br = new(fileStream);
        fileStream.Seek(60L, SeekOrigin.Begin);
        int peOffset = br.ReadInt32();
        fileStream.Seek(peOffset, SeekOrigin.Begin);
        if (br.ReadUInt32() != 17744)
        {
            throw new Exception("Can't find PE header");
        }
        MachineType machineType = (MachineType)br.ReadUInt16();
        br.Close();
        fileStream.Close();
        return machineType;
    }

    [DllImport("kernel32.dll")]
    private static extern uint GetLastError();

    [DllImport("kernel32.dll")]
    private static extern uint SetErrorMode(uint mode);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetDllDirectory(string lpPathName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern uint SearchPath(string lpPath, string lpFileName, string lpExtension, int nBufferLength, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpBuffer, out IntPtr lpFilePart);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr LoadLibrary(string dllToLoad);

    public static string CheckLibraryError(string pLibrary)
    {
        if (LoadLibrary(pLibrary) == IntPtr.Zero)
        {
            switch (GetLastError())
            {
                case 126u:
                    return null;
                default:
                    try
                    {
                        StringBuilder buffer = new(255);
                        _ = SearchPath(null, pLibrary, null, 255, buffer, out nint ptr);
                        return buffer.ToString();
                    }
                    catch (Exception)
                    {
                        return pLibrary;
                    }
                case 0u:
                    break;
            }
        }
        return null;
    }

    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    private static void Main(string[] args)
    {
        try
        {
            IntPtr ntDll = LoadLibrary("ntdll.dll");
            if (ntDll != IntPtr.Zero)
            {
                IntPtr pwine_get_version = GetProcAddress(ntDll, "wine_get_version");
                if (pwine_get_version != IntPtr.Zero)
                {
                    kWineVersion = "unknown";
                    IntPtr wineVersionPtr = Marshal.GetDelegateForFunctionPointer<wine_version_delegate>(pwine_get_version)();
                    if (wineVersionPtr != IntPtr.Zero)
                    {
                        kWineVersion = Marshal.PtrToStringAnsi(wineVersionPtr);
                    }
                }
            }
        }
        catch (Exception)
        {
        }
        AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
        Program.Main(args);
    }

    public static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
    {
        if (File.Exists("CrashWindow.exe"))
        {
            try
            {
                Program.HandleGameCrash(e.ExceptionObject as Exception);
            }
            catch (Exception pException)
            {
                string errorLine = e.ExceptionObject.ToString();
                errorLine = ProcessErrorLine(errorLine, e.ExceptionObject as Exception);
                StreamWriter streamWriter = new("ducklog.txt", append: true);
                streamWriter.WriteLine(errorLine);
                streamWriter.Close();
                Process.Start("CrashWindow.exe", "-modResponsible 0 -modDisabled 0 -modName none -source " + (e.ExceptionObject as Exception).Source + " -commandLine \"none\" -executable \"" + Application.ExecutablePath + "\" " + GetCrashWindowString(pException, null, errorLine));
            }
        }
    }

    public static string ProcessErrorLine(string pLine, Exception pException)
    {
        if (pException is FileNotFoundException)
        {
            if (pException.Message.Contains("Microsoft.Xna.Framework"))
            {
                pLine = "It seems like the XNA Framework is not installed! Info on getting XNA can be found here:\nhttps://steamcommunity.com/app/312530/discussions/1/2997675206112505333/\n\n" + pLine;
            }
            else if (pException.Message.Contains("Steam.dll"))
            {
                pLine = "It seems like you may be missing Microsoft's VC++ 2015 Redists! Please download and install them from Microsoft:\nhttps://www.microsoft.com/en-us/download/details.aspx?id=52685\n\n" + pLine;
            }
        }
        else if (pException is BadImageFormatException || pException is FileLoadException)
        {
            string dll = "";
            try
            {
                foreach (string s in BadFormatExceptionAssembly())
                {
                    dll = dll + s + "\n";
                }
            }
            catch (Exception)
            {
            }
            pLine = "One or more DLL files failed to load. This usually means the file is 64-bit, but it's supposed to be 32-bit:\n" + dll + "\n\nThere may be an issue with your .NET Framework installation, or with the location/version of some of your Windows DLL files... Please check the 'System.BadImageFormatException' section on the DG common issues page:\nhttps://steamcommunity.com/app/312530/discussions/1/2997675206112505333/\n\n" + pLine;
        }
        else if (pException is OutOfMemoryException || pException.ToString().Contains("System.OutOfMemoryException"))
        {
            pLine = "Duck Game ran out of memory! It's possible that you have *TOO MANY MODS* installed. Try unsubscribing from some mods, or disable some mods through the options menu by running DG with the '-nomods' launch option:\nhttps://www.microsoft.com/en-ca/download/details.aspx?id=20914\n\n" + pLine;
        }
        return pLine;
    }

    public static string GetCrashWindowString(Exception pException, string pAssemblyName, string pLogMessage)
    {
        string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        string mods = "";
        string assembly = pAssemblyName ?? "DuckGame";
        string exception = pException?.GetType().ToString() ?? "Unknown";
        string b64log = CrashWindow.CrashWindow.Base64Encode((pLogMessage == "") ? "none" : pLogMessage);
        return $" -pVersion {version} -pMods {CrashWindow.CrashWindow.Base64Encode((mods == "") ? "none" : mods)} -pAssembly {CrashWindow.CrashWindow.Base64Encode(assembly)} -pException {CrashWindow.CrashWindow.Base64Encode(exception)} -pLogMessage {b64log}";
    }

    public static List<string> BadFormatExceptionAssembly()
    {
        List<string> problems = [];
        _ = SetErrorMode(0u);
        foreach (string s in _moduleDependencies)
        {
            try
            {
                string st = CheckLibraryError(s);
                if (st != null)
                {
                    problems.Add(st);
                }
            }
            catch (Exception)
            {
                return problems;
            }
        }
        if (problems.Count == 0)
        {
            problems.Add("(unknown dll)");
        }
        return problems;
    }

    public static List<Module> CollectModules(Process process)
    {
        List<Module> collectedModules = [];
        IntPtr[] modulePointers = [];
        if (!Native.EnumProcessModulesEx(process.Handle, modulePointers, 0, out int bytesNeeded, 3u))
        {
            return collectedModules;
        }
        int totalNumberofModules = bytesNeeded / IntPtr.Size;
        modulePointers = new IntPtr[totalNumberofModules];
        if (Native.EnumProcessModulesEx(process.Handle, modulePointers, bytesNeeded, out _, 3u))
        {
            for (int index = 0; index < totalNumberofModules; index++)
            {
                StringBuilder moduleFilePath = new(1024);
                _ = Native.GetModuleFileNameEx(process.Handle, modulePointers[index], moduleFilePath, (uint)moduleFilePath.Capacity);
                Native.GetModuleInformation(process.Handle, modulePointers[index], out var moduleInformation, (uint)(IntPtr.Size * modulePointers.Length));
                Module module = new(moduleFilePath.ToString(), moduleInformation.lpBaseOfDll, moduleInformation.SizeOfImage);
                collectedModules.Add(module);
            }
        }
        return collectedModules;
    }

    public static void AssemblyLoad(object sender, AssemblyLoadEventArgs args)
    {
        assemblyLoadStrings.Add(args.LoadedAssembly.FullName + ": " + args.LoadedAssembly.GetName().ProcessorArchitecture);
        if ((args.LoadedAssembly.FullName.Contains("HarmonySharedState") || args.LoadedAssembly.FullName.Contains("HarmonyLoader")) && ModLoader.loadingOldMod != null)
        {
            ModLoader.FailWithHarmonyException();
        }
    }

    [DllImport("user32.dll")]
    public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
}
