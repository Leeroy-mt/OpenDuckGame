using System;
using System.Collections.Generic;
using System.Reflection;

namespace DuckGame;

public static class DG
{
    #region Public Fields
    public static int MaxPlayers = 8;

    public static int MaxSpectators = 4;
    #endregion

    #region Private Fields
    static bool _drmFailure = false;

    static bool _devBuild = false;

    static bool _betaBuild = true;

    static bool _pressBuild = true;

    static int _versionHigh = 7717;

    static int _versionLow = 16376;

    static int _versionMajor = 1;

    static ulong _localID = 0;
    #endregion

    #region Public Properties
    public static bool drmFailure => _drmFailure;

    public static bool devBuild => _devBuild;

    public static bool betaBuild => _betaBuild;

    public static bool pressBuild => _pressBuild;

    public static bool buildExpired => false;

    public static bool isHalloween
    {
        get
        {
            DateTime now = MonoMain.GetLocalTime();
            if (now.Month == 10)
            {
                if (now.Day != 28 && now.Day != 29 && now.Day != 30)
                    return now.Day == 31;
                return true;
            }
            return false;
        }
    }

    public static int versionMajor => _versionMajor;

    public static int versionLow => _versionLow;

    public static int versionHigh => _versionHigh;

    public static int di => NetworkDebugger.currentIndex;

    public static ulong localID
    {
        get
        {
            if (NetworkDebugger.enabled)
                return (ulong)(1330 + NetworkDebugger.currentIndex);
            if (Steam.user != null)
                return Steam.user.id;
            return _localID;
        }
    }

    public static string version => MakeVersionString(_versionMajor, _versionHigh, _versionLow);

    public static string platform
    {
        get
        {
            var os = Environment.OSVersion;
            var version = Environment.OSVersion.Version;
            string name = "Windows Mystery Edition";

            switch (version.Major)
            {
                case <= 4:
                    name = $"Windows NT {version.Major}.{version.Minor}";
                    break;
                case 5:
                    name = version.Minor switch
                    {
                        0 => "Windows 2000",
                        1 => "Windows XP",
                        //2 => "Windows Server 2003",
                        _ => os.VersionString
                    };
                    break;
                case 6:
                    switch (version.Minor)
                    {
                        case 0:
                            name = version.Build switch
                            {
                                //2423 => "Windows Home Server",
                                >= 6000 and <= 6003 => "Windows Vista",
                                _ => os.VersionString
                            };
                            break;
                        case 1:
                            name = version.Build switch
                            {
                                >= 6429 and <= 7601 => "Windows 7",
                                //7657 => "Windows Home Server 2011",
                                _ => os.VersionString
                            };
                            break;
                        case 2:
                            name = "Windows 8";
                            break;
                        case 3:
                            name = "Windows 8.1";
                            break;
                    }
                    break;
                case 10:
                    name = version.Build switch
                    {
                        >= 9841 and <= 19045 => "Windows 10",
                        //20348 => "Windows Server 2022",
                        >= 22000 and <= 26200 => "Windows 11",
                        _ => os.VersionString
                    };
                    break;
            }

            if (Program.wineVersion != null)
                name = $"{name} (Linux Wine v{Program.wineVersion})";
            return name;
        }
    }

    public static Assembly[] assemblies => ModLoader.modAssemblyArray;
    #endregion

    #region Public Methods
    public static void SetVersion(string v) =>
        _localID = (ulong)Rando.Long();

    public static bool InitializeDRM() =>
        true;

    public static string MakeVersionString(int pMajor, int pHigh, int pLow) =>
        $"1.{pMajor}.{pHigh}.{pLow}";

    public static string GetCrashWindowString(Exception pException, ModConfiguration pModConfig, string pLogMessage) =>
        GetCrashWindowString(pException, pModConfig.name, pLogMessage);

    public static string GetCrashWindowString(Exception pException, Assembly pAssembly, string pLogMessage) =>
        GetCrashWindowString(pException, (pAssembly != null) ? pAssembly.GetName().Name : null, pLogMessage);

    public static string GetCrashWindowString(Exception pException, string pAssemblyName, string pLogMessage)
    {
        string version = versionMajor.ToString() + versionHigh + versionLow,
               mods = "",
               assembly = pAssemblyName ?? "DuckGame",
               exception = pException?.GetType().ToString() ?? "Unknown";
        try
        {
            foreach (Mod m in ModLoader.allMods)
                if (m is not CoreMod && m.configuration.loaded)
                    mods = mods + m.configuration.workshopID + ",";
        }
        catch (Exception ex)
        {
            exception = "SENDFAIL " + ex.ToString();
        }
        string b64log = CrashWindow.CrashWindow.Base64Encode((pLogMessage == "") ? "none" : pLogMessage);
        return $" -pVersion {version} -pMods {CrashWindow.CrashWindow.Base64Encode((mods == "") ? "none" : mods)} -pAssembly {CrashWindow.CrashWindow.Base64Encode(assembly)} -pException {CrashWindow.CrashWindow.Base64Encode(exception)} -pLogMessage {b64log}";
    }

    public static string Reduced(this string str, int pMaxLength)
    {
        if (str.Length > pMaxLength)
            return string.Concat(str.AsSpan(0, pMaxLength - 2), "..");
        return str;
    }

    public static string Padded(this string str, int pMinLength)
    {
        while (str.Length < pMinLength)
            str += " ";
        return str;
    }

    public static IEnumerable<T> Reverse<T>(T[] pArray)
    {
        List<T> reversed = [];
        for (int i = pArray.Length - 1; i >= 0; i--)
            reversed.Add(pArray[i]);
        return reversed;
    }
    #endregion
}