using System;
using System.Collections.Generic;
using System.Reflection;
using CrashWindow;

namespace DuckGame;

public static class DG
{
	private static int _versionHigh = 7717;

	private static int _versionLow = 16376;

	private static int _versionMajor = 1;

	private static ulong _localID = 0uL;

	public static int MaxPlayers = 8;

	public static int MaxSpectators = 4;

	private static bool _drmFailure = false;

	private static bool _devBuild = false;

	private static bool _betaBuild = true;

	private static bool _pressBuild = true;

	public static string version => MakeVersionString(_versionMajor, _versionHigh, _versionLow);

	public static int versionHigh => _versionHigh;

	public static int versionLow => _versionLow;

	public static int versionMajor => _versionMajor;

	public static bool isHalloween
	{
		get
		{
			DateTime now = MonoMain.GetLocalTime();
			if (now.Month == 10)
			{
				if (now.Day != 28 && now.Day != 29 && now.Day != 30)
				{
					return now.Day == 31;
				}
				return true;
			}
			return false;
		}
	}

	public static Assembly[] assemblies => ModLoader.modAssemblyArray;

	public static string platform
	{
		get
		{
			string ver = Environment.OSVersion.ToString();
			string name = "Windows Mystery Edition";
			if (ver.Contains("5.0"))
			{
				name = "Windows 2000";
			}
			else if (ver.Contains("5.1"))
			{
				name = "Windows XP";
			}
			else if (ver.Contains("5.2"))
			{
				name = "Windows XP 64-Bit Edition";
			}
			else if (ver.Contains("6.0"))
			{
				name = "Windows Vista";
			}
			else if (ver.Contains("6.1"))
			{
				name = "Windows 7";
			}
			else if (ver.Contains("6.2"))
			{
				name = "Windows 8";
			}
			else if (ver.Contains("6.3"))
			{
				name = "Windows 8.1";
			}
			else if (ver.Contains("10.0"))
			{
				name = "Windows 10";
			}
			if (Program.wineVersion != null)
			{
				name = name + " (Linux Wine v" + Program.wineVersion + ")";
			}
			return name;
		}
	}

	public static ulong localID
	{
		get
		{
			if (NetworkDebugger.enabled)
			{
				return (ulong)(1330 + NetworkDebugger.currentIndex);
			}
			if (Steam.user != null)
			{
				return Steam.user.id;
			}
			return _localID;
		}
	}

	public static bool drmFailure => _drmFailure;

	public static bool devBuild => _devBuild;

	public static bool betaBuild => _betaBuild;

	public static bool pressBuild => _pressBuild;

	public static bool buildExpired => false;

	public static int di => NetworkDebugger.currentIndex;

	public static string MakeVersionString(int pMajor, int pHigh, int pLow)
	{
		return "1." + pMajor + "." + pHigh + "." + pLow;
	}

	public static void SetVersion(string v)
	{
		_localID = (ulong)Rando.Long();
	}

	public static bool InitializeDRM()
	{
		return true;
	}

	public static string GetCrashWindowString(Exception pException, ModConfiguration pModConfig, string pLogMessage)
	{
		return GetCrashWindowString(pException, pModConfig.name, pLogMessage);
	}

	public static string GetCrashWindowString(Exception pException, Assembly pAssembly, string pLogMessage)
	{
		return GetCrashWindowString(pException, (pAssembly != null) ? pAssembly.GetName().Name : null, pLogMessage);
	}

	public static string GetCrashWindowString(Exception pException, string pAssemblyName, string pLogMessage)
	{
		string version = versionMajor.ToString() + versionHigh + versionLow;
		string mods = "";
		string assembly = ((pAssemblyName != null) ? pAssemblyName : "DuckGame");
		string exception = ((pException != null) ? pException.GetType().ToString() : "Unknown");
		try
		{
			foreach (Mod m in ModLoader.allMods)
			{
				if (!(m is CoreMod) && m.configuration.loaded)
				{
					mods = mods + m.configuration.workshopID + ",";
				}
			}
		}
		catch (Exception ex)
		{
			exception = "SENDFAIL " + ex.ToString();
		}
		string b64log = global::CrashWindow.CrashWindow.Base64Encode((pLogMessage == "") ? "none" : pLogMessage);
		return " -pVersion " + version + " -pMods " + global::CrashWindow.CrashWindow.Base64Encode((mods == "") ? "none" : mods) + " -pAssembly " + global::CrashWindow.CrashWindow.Base64Encode(assembly) + " -pException " + global::CrashWindow.CrashWindow.Base64Encode(exception) + " -pLogMessage " + b64log;
	}

	public static string Reduced(this string str, int pMaxLength)
	{
		if (str.Length > pMaxLength)
		{
			return str.Substring(0, pMaxLength - 2) + "..";
		}
		return str;
	}

	public static string Padded(this string str, int pMinLength)
	{
		while (str.Length < pMinLength)
		{
			str += " ";
		}
		return str;
	}

	public static IEnumerable<T> Reverse<T>(T[] pArray)
	{
		List<T> reversed = new List<T>();
		for (int i = pArray.Length - 1; i >= 0; i--)
		{
			reversed.Add(pArray[i]);
		}
		return reversed;
	}
}
