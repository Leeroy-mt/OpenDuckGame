using DbMon.NET;
using DGWindows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace DuckGame;

/// <summary>
/// The main class.
/// </summary>
public static class Program
{
    #region Constants
    const uint WM_CLOSE = 16u;

    const string kCleanupString = "C:\\gamedev\\duckgame_try2\\duckgame\\DuckGame\\src\\";
    #endregion

    #region Public Fields
    public static bool alternateSaveLocation;

    public static bool isLinux;

    public static bool crashed;

    public static bool gameLoadedSuccessfully;

    public static int constructorsLoaded;

    public static int thingTypes;

    public static int steamBuildID;

    public static string commandLine = "";

    public static string wineVersion;

    public static Assembly crashAssembly;
    #endregion

    #region Private Fields
    static bool enteredMain;

    static bool _showedError;

    static bool _attemptingResolve;

    static string steamInitializeError = "";

    static Main main;

    static List<Func<string>> _extraExceptionDetailsMinimal =
    [
        () => "Date: " + DateTime.UtcNow.ToString(DateTimeFormatInfo.InvariantInfo),
        () => "Version: " + DG.version,
        () => "Platform: " + DG.platform + " (Steam Build " + steamBuildID + ")",
        () => "Command Line: " + commandLine
    ];
    #endregion

    #region Public Methods
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        if (args.Contains("-linux") || (WindowsPlatformStartup.IsRunningWine && !args.Contains("-nolinux")))
        {
            wineVersion = WindowsPlatformStartup.WineVersion;
            isLinux = true;
            MonoMain.enableThreadedLoading = false;
        }
        else
        {
            AppDomain.CurrentDomain.AssemblyLoad += WindowsPlatformStartup.AssemblyLoad;
        }
        Application.ThreadException += UnhandledThreadExceptionTrapper;
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        if (Environment.Is64BitProcess)
        {
            if (File.Exists("Windows-x64\\Steamworks.NET.dll"))
                Assembly.LoadFrom("Windows-x64\\Steamworks.NET.dll");
        }
        else
        {
            if (File.Exists("Windows-x86\\Steamworks.NET.dll"))
                Assembly.LoadFrom("Windows-x86\\Steamworks.NET.dll");
        }

        try
        {
            DoMain(args);
        }
        catch (Exception pException)
        {
            HandleGameCrash(pException);
        }
    }

    public static void HandleGameCrash(Exception pException)
    {
        if (!File.Exists("CrashWindow.exe"))
        {
            return;
        }
        if (pException is ThreadAbortException)
        {
            ThreadAbortException a = pException as ThreadAbortException;
            if (a.ExceptionState is Exception)
            {
                pException = a.ExceptionState as Exception;
            }
        }
        bool modRelated = false;
        try
        {
            if (Network.isActive)
            {
                for (int i = 0; i < 5; i++)
                {
                    Send.ImmediateUnreliableBroadcast(new NMClientCrashed());
                    Send.ImmediateUnreliableBroadcast(new NMClientCrashed());
                    Steam.Update();
                    Thread.Sleep(16);
                }
                crashed = true;
            }
        }
        catch (Exception)
        {
        }
        string error = "";
        int crashPoint = 0;
        try
        {
            try
            {
                error = MonoMain.GetExceptionString(pException);
            }
            catch (Exception)
            {
                try
                {
                    error = GetExceptionStringMinimal(pException);
                }
                catch (Exception)
                {
                    error = pException.ToString();
                }
            }
            try
            {
                if (pException is UnauthorizedAccessException && !DuckFile.appdataSave)
                {
                    error = "This crash may be due to your save being located in the Documents folder. If the problem persists, try moving your DuckGame save files from (" + DuckFile.oldSaveLocation + "DuckGame/) to (" + DuckFile.newSaveLocation + "DuckGame/).\n\n" + error;
                }
                else
                {
                    try
                    {
                        if (pException is OutOfMemoryException || pException.ToString().Contains("System.OutOfMemoryException"))
                        {
                            MonoMain.CalculateModMemoryOffendersList();
                            error = MonoMain.modMemoryOffendersString + error;
                        }
                    }
                    catch (Exception)
                    {
                    }
                    error = WindowsPlatformStartup.ProcessErrorLine(error, pException);
                }
            }
            catch (Exception)
            {
            }
            try
            {
                WriteToLog(error);
            }
            catch (Exception ex6)
            {
                error = error + "Writing your crash to the log failed with exception " + ex6.Message + "!\n";
            }
            crashPoint = 1;
            Exception ex7 = pException;
            string modName = "";
            bool successfullyDisabled = false;
            Assembly modAssembly = crashAssembly;
            ModConfiguration config = null;
            try
            {
                if (pException is ModException)
                {
                    config = (pException as ModException).mod;
                    if (config != null)
                    {
                        modName = config.name;
                        modAssembly = config.assembly;
                    }
                }
                else
                {
                    crashPoint = 2;
                    try
                    {
                        foreach (Mod m in ModLoader.allMods)
                        {
                            if (m is CoreMod || m.configuration == null || !(m.configuration.assembly != null) || !(m.configuration.assembly != Assembly.GetExecutingAssembly()))
                            {
                                continue;
                            }
                            bool isCause = (crashAssembly == null && m.configuration.assembly == ex7.TargetSite.DeclaringType.Assembly) || m.configuration.assembly == crashAssembly;
                            if (!isCause)
                            {
                                Type[] types = m.configuration.assembly.GetTypes();
                                foreach (Type t in types)
                                {
                                    if (pException.StackTrace.Contains(t.ToString()))
                                    {
                                        isCause = true;
                                        break;
                                    }
                                    if (pException.InnerException != null && pException.InnerException.StackTrace.Contains(t.ToString()))
                                    {
                                        isCause = true;
                                        break;
                                    }
                                }
                            }
                            if (!isCause)
                            {
                                continue;
                            }
                            modAssembly = m.configuration.assembly;
                            modRelated = true;
                            modName = m.configuration.name;
                            if (!MonoMain.modDebugging)
                            {
                                if (!gameLoadedSuccessfully || (Options.Data.disableModOnCrash && (DateTime.Now - MonoMain.startTime).TotalMinutes < 2.0))
                                {
                                    m.configuration.Disable();
                                }
                                successfullyDisabled = true;
                            }
                        }
                        _ = pException.InnerException;
                    }
                    catch (Exception ex8)
                    {
                        error = error + "Finding if crash was Mod related failed with exception " + ex8.Message + "!\n But, No matter, here's the actual exception message for the crash:\n";
                    }
                }
            }
            catch (Exception)
            {
            }
            crashPoint = 4;
            if (modAssembly == null)
            {
                modAssembly = crashAssembly;
            }
            try
            {
                crashPoint = 5;
                if (main != null && main.Window != null)
                {
                    SendMessage(main.Window.Handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
            }
            catch (Exception)
            {
            }
            crashPoint = 6;
            if (File.Exists("CrashWindow.exe"))
            {
                try
                {
                    if (config != null)
                    {
                        Process.Start("CrashWindow.exe", "-modResponsible " + (modRelated ? "1" : "0") + " -modDisabled " + ((gameLoadedSuccessfully && !Options.Data.disableModOnCrash) ? "2" : (successfullyDisabled ? "1" : "0")) + " -modName " + modName + " -source " + ex7.Source + " -commandLine \"" + commandLine + "\" -executable \"" + Application.ExecutablePath + "\" " + DG.GetCrashWindowString(pException, config, error));
                    }
                    else
                    {
                        Process.Start("CrashWindow.exe", "-modResponsible " + (modRelated ? "1" : "0") + " -modDisabled " + ((gameLoadedSuccessfully && !Options.Data.disableModOnCrash) ? "2" : (successfullyDisabled ? "1" : "0")) + " -modName " + modName + " -source " + ex7.Source + " -commandLine \"" + commandLine + "\" -executable \"" + Application.ExecutablePath + "\" " + DG.GetCrashWindowString(pException, modAssembly, error));
                    }
                }
                catch (Exception ex11)
                {
                    WriteToLog("Opening CrashWindow failed with error: " + ex11.ToString() + "\n");
                }
            }
            //MonoMain.instance.Exit(); //soft exit

            SDL3.SDL.SDL_DestroyWindow(MonoMain.instance.Window.Handle); //hard exit
            Environment.Exit(1); //hard exit
        }
        catch (Exception ex12)
        {
            try
            {
                WriteToLog("Crash catcher failed (crashpoint " + crashPoint + ") with exception: " + ex12.Message + "\n But Also: \n" + error);
            }
            catch (Exception)
            {
                StreamWriter streamWriter = new("ducklog.txt", append: true);
                streamWriter.WriteLine("Failed to write exception to log: " + ex12.Message + "\n");
                streamWriter.Close();
            }
        }
    }

    public static void WriteToLog(string s)
    {
        try
        {
            StreamWriter streamWriter = new("ducklog.txt", append: true);
            streamWriter.WriteLine(s + "\n");
            streamWriter.Close();
        }
        catch (Exception ex)
        {
            StreamWriter streamWriter2 = new("ducklog.txt", append: true);
            streamWriter2.WriteLine(ex.ToString() + "\n");
            streamWriter2.Close();
        }
    }

    public static void MakeNetLog()
    {
        StreamWriter file = new("netlog.txt", append: false);
        foreach (DCLine d in DevConsole.core.lines)
            file.WriteLine(d.timestamp.ToLongTimeString() + " " + RemoveColorTags(d.SectionString()) + " " + RemoveColorTags(d.line) + "\n");
        foreach (DCLine d2 in DevConsole.core.pendingLines)
            file.WriteLine(d2.timestamp.ToLongTimeString() + " " + RemoveColorTags(d2.SectionString()) + " " + RemoveColorTags(d2.line) + "\n");
        file.WriteLine("\n");
        file.Close();
    }

    public static void LogLine(string line)
    {
        try
        {
            StreamWriter streamWriter = new("ducklog.txt", append: true);
            streamWriter.WriteLine(line + "\n");
            streamWriter.Close();
        }
        catch
        {
        }
    }

    public static void RemotePlayConnected() =>
        Windows_Audio.forceMode = AudioMode.DirectSound;

    public static void UnhandledThreadExceptionTrapper(object sender, ThreadExceptionEventArgs e) =>
        HandleGameCrash(e.Exception);

    public static string ProcessExceptionString(Exception e)
    {
        string error;
        if (e is ModException)
        {
            ModException m = e as ModException;
            error = m.Message + "\n" + m.exception.ToString();
            e = m.exception;
        }
        else
        {
            error = e.ToString();
        }
        try
        {
            if (e is UnauthorizedAccessException)
            {
                UnauthorizedAccessException u = e as UnauthorizedAccessException;
                int pathStart = u.Message.IndexOf(':') - 1;
                if (pathStart > 0)
                {
                    int pathEnd = u.Message.LastIndexOf('\'');
                    if (pathEnd > 0)
                    {
                        FileAttributes attr = new FileInfo(u.Message[pathStart..pathEnd]).Attributes;
                        string details = "(File is ";
                        int num = 0;
                        foreach (FileAttributes f in Enum.GetValues<FileAttributes>())
                        {
                            if ((attr & f) > 0)
                            {
                                if (num > 0)
                                {
                                    details += ",";
                                }
                                details += f;
                                num++;
                            }
                        }
                        details += ") ";
                        error = details + error;
                    }
                }
            }
        }
        catch (Exception)
        {
        }
        return error;
    }

    public static string GetExceptionStringMinimal(object e)
    {
        string error = (e as Exception).ToString() + "\r\n";
        error = error.Replace(kCleanupString, "");
        foreach (Func<string> func in _extraExceptionDetailsMinimal)
        {
            string add = "FIELD FAILED";
            try
            {
                add = func();
            }
            catch
            {
            }
            error += "\r\n";
            error += add;
        }
        return error;
    }

    public static string RemoveColorTags(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '|')
            {
                int startIndex = i;
                for (i++; i < s.Length && s[i] != '|'; i++)
                {
                }
                if (i < s.Length && s[i] == '|')
                {
                    s = s.Remove(startIndex, i - startIndex + 1);
                    i = -1;
                }
            }
        }
        return s;
    }

    public static Assembly ModResolve(object sender, ResolveEventArgs args)  =>
        ManagedContent.ResolveModAssembly(sender, args);

    public static Assembly Resolve(object sender, ResolveEventArgs args)
    {
        if (!enteredMain)
            return null;
        if (args.Name.StartsWith("Steam,"))
            return Assembly.GetAssembly(typeof(Steam));
        if (!_attemptingResolve)
        {
            bool modResolutionFailure = false;
            if (enteredMain)
            {
                _attemptingResolve = true;
                Assembly resolved = null;
                try
                {
                    resolved = ModResolve(sender, args);
                }
                catch (Exception)
                {
                }
                _attemptingResolve = false;
                if (resolved != null)
                {
                    return resolved;
                }
                modResolutionFailure = true;
            }
            if (!_showedError && (!ModLoader.runningModloadCode || MonoMain.modDebugging) && !modResolutionFailure)
            {
                _showedError = true;
                string errorLine = "Failed to resolve assembly:\n" + args.Name + "\n";
                if (args.Name.Contains("Microsoft.Xna.Framework"))
                {
                    errorLine += "(You may need to install the XNA redistributables!)\n";
                }
                StreamWriter streamWriter = new("ducklog.txt", append: true);
                streamWriter.WriteLine(errorLine);
                streamWriter.Close();
                Process.Start("CrashWindow.exe", "-modResponsible 0 -modDisabled 0 -exceptionString \"" + errorLine.Replace("\n", "|NEWLINE|").Replace("\r", "|NEWLINE2|") + "\" -source Duck Game -commandLine \"\" -executable \"" + Application.ExecutablePath + "\"");
            }
        }
        return null;
    }
    #endregion

    #region Private Methods
    static void OnProcessExit(object sender, EventArgs e) =>
        main?.KillEverything();

    static void DoMain(string[] args)
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        MonoMain.startTime = DateTime.Now;
        for (int i = 0; i < args.Length; i++)
        {
            commandLine += args[i];
            if (i != args.Length - 1)
            {
                commandLine += " ";
            }
        }
        bool _testDependencies = false;
        for (int j = 0; j < args.Length; j++)
        {
            if (args[j] == "+connect_lobby")
            {
                j++;
                if (args.Length > j)
                {
                    try
                    {
                        DuckGame.Main.connectID = Convert.ToUInt64(args[j], CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        throw new Exception("+connect_lobby format exception (" + args[j] + ")");
                    }
                }
            }
            else if (args[j] == "+password")
            {
                j++;
                if (args.Length > j)
                {
                    MonoMain.lobbyPassword = args[j];
                }
            }
            else if (args[j] == "-debug")
            {
                _testDependencies = true;
            }
            else if (args[j] == "-windowedFullscreen")
            {
                MonoMain.forceFullscreenMode = 1;
            }
            else if (args[j] == "-oldschoolFullscreen")
            {
                MonoMain.forceFullscreenMode = 2;
            }
            else if (args[j] == "-nothreading")
            {
                MonoMain.enableThreadedLoading = false;
            }
            else if (args[j] == "-defaultcontrols")
            {
                MonoMain.defaultControls = true;
            }
            else if (args[j] == "-olddefaults")
            {
                MonoMain.oldDefaultControls = true;
            }
            else if (args[j] == "-nofullscreen")
            {
                MonoMain.noFullscreen = true;
            }
            else if (args[j] == "-nosteam")
            {
                MonoMain.disableSteam = true;
            }
            else if (args[j] == "-steam")
            {
                MonoMain.launchedFromSteam = true;
            }
            else if (args[j] == "-loopdebug")
            {
                MonoMain.infiniteLoopDebug = true;
            }
            else if (args[j] == "-nomods")
            {
                MonoMain.nomodsMode = true;
            }
            else if (args[j] == "-linux")
            {
                if (MonoMain.audioModeOverride == AudioMode.None)
                {
                    MonoMain.audioModeOverride = AudioMode.Wave;
                }
            }
            else if (args[j] == "-disableModding")
            {
                MonoMain.moddingEnabled = false;
            }
            else if (args[j] == "-nointro")
            {
                MonoMain.noIntro = true;
            }
            else if (args[j] == "-startineditor")
            {
                MonoMain.startInEditor = true;
            }
            else if (args[j] == "-moddebug")
            {
                MonoMain.modDebugging = true;
            }
            else if (args[j] == "-downloadmods")
            {
                MonoMain.downloadWorkshopMods = true;
            }
            else if (args[j] == "-editsave")
            {
                MonoMain.editSave = true;
            }
            else if (args[j] == "-nodinput")
            {
                MonoMain.disableDirectInput = true;
            }
            else if (args[j] == "-dinputNoTimeout")
            {
                MonoMain.dinputNoTimeout = true;
            }
            else if (args[j] == "-ignoreLegacyLoad")
            {
                ModLoader.ignoreLegacyLoad = true;
            }
            else if (args[j] == "-nocloud")
            {
                Cloud.nocloud = true;
            }
            else if (args[j] == "-cloudnoload")
            {
                Cloud.downloadEnabled = false;
            }
            else if (args[j] == "-cloudnosave")
            {
                Cloud.uploadEnabled = false;
            }
            else if (args[j] == "-netdebug")
            {
                MonoMain.networkDebugger = true;
            }
            else if (args[j] == "-altaudio")
            {
                MonoMain.audioModeOverride = AudioMode.Wave;
            }
            else if (args[j] == "-directaudio")
            {
                MonoMain.audioModeOverride = AudioMode.DirectSound;
            }
            else if (args[j] == "-oldangles")
            {
                MonoMain.oldAngles = true;
            }
            else if (args[j] == "-nohidef")
            {
                MonoMain.noHidef = true;
            }
            else if (args[j] == "-logFileOperations")
            {
                MonoMain.logFileOperations = true;
            }
            else if (args[j] == "-logLevelOperations")
            {
                MonoMain.logLevelOperations = true;
            }
            else if (args[j] == "-recoversave")
            {
                MonoMain.recoversave = true;
            }
            else if (args[j] == "-notimeout")
            {
                MonoMain.noConnectionTimeout = true;
            }
            else if (args[j] == "-command")
            {
                j++;
                if (j < args.Length)
                {
                    DevConsole.startupCommands.Add(args[j]);
                }
            }
            else if (args[j] == "-sdl2")
            {
                Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL2");
            }
            else if (args[j] == "-nostart")
            {
                MonoMain.NoStart = true;
            }
            else
            {
                if (args[j] == "-nolaunch")
                {
                    MessageBox.Show("-nolaunch Command Line Option activated! Cancelling launch!");
                    return;
                }
                if (args[j] == "-alternateSaveLocation")
                {
                    alternateSaveLocation = true;
                }
            }
        }
        try
        {
            if (MonoMain.audioModeOverride == AudioMode.None && Environment.OSVersion.Version.Major < 6)
            {
                MonoMain.audioModeOverride = AudioMode.Wave;
            }
        }
        catch (Exception)
        {
        }
        if (_testDependencies)
        {
            try
            {
                DebugMonitor.OnOutputDebugString += OnOutputDebugStringHandler;
                DebugMonitor.Start();
            }
            catch (Exception ex3)
            {
                steamInitializeError = "SteamAPI deep debug failed with exception:" + ex3.Message + "\nTry running Duck Game as administrator for more debug info.";
            }
        }
        enteredMain = true;
        if (!MonoMain.disableSteam)
        {
            if (MonoMain.breakSteam || !Steam.InitializeCore())
            {
                LogLine("Steam INIT Failed!");
            }
            else
            {
                Steam.Initialize();
            }
        }
        try
        {
            if (Steam.IsInitialized())
            {
                steamBuildID = Steam.GetGameBuildID();
                Steam.RemotePlay += RemotePlayConnected;
                if (!Steam.IsLoggedIn() || !Steam.Authorize())
                {
                    MonoMain.steamConnectionCheckFail = true;
                }
            }
            else
            {
                steamBuildID = -1;
            }
        }
        catch (Exception)
        {
        }
        DevConsole.Log("Starting Duck Game (" + DG.platform + ")...");
        main = new Main();
        main.Run();
    }

    static void OnOutputDebugStringHandler(int pid, string text) =>
        steamInitializeError = steamInitializeError + text + "\n";

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    #endregion
}
