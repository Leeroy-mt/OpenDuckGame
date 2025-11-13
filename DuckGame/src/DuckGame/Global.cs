using System.Collections.Generic;

namespace DuckGame;

public static class Global
{
	public static HashSet<string> boughtHats = new HashSet<string>();

	private static List<string> _achievementList = new List<string>
	{
		"play100", "chancy", "play1000", "kill1000", "endurance", "online10", "basement", "editor", "drawbreaker", "fire",
		"crate", "book", "mine", "laser", "finish50", "myboy", "jukebox", "kingme"
	};

	private static Dictionary<string, bool> _achievementStatus = new Dictionary<string, bool>();

	private static GlobalData _data = new GlobalData();

	private static bool loadCalled = false;

	public static DXMLNode _customLoadDoc = null;

	public static GlobalData data
	{
		get
		{
			return _data;
		}
		set
		{
			_data = value;
		}
	}

	public static string globalFileName => DuckFile.optionsDirectory + "/global.dat";

	public static bool HasAchievement(string pAchievement)
	{
		bool res = false;
		if (!_achievementStatus.TryGetValue(pAchievement, out res))
		{
			return Steam.GetAchievement(pAchievement);
		}
		return res;
	}

	public static void GiveAchievement(string pAchievement)
	{
		if (!HasAchievement(pAchievement))
		{
			Steam.SetAchievement(pAchievement);
			_achievementStatus[pAchievement] = true;
		}
	}

	public static void Initialize()
	{
		foreach (string s in _achievementList)
		{
			_achievementStatus[s] = Steam.GetAchievement(s);
		}
		data.unlockListIndex = Rando.Int(500);
		data.flag = 0;
		Load();
	}

	public static void Kill(Duck d, DestroyType type)
	{
		if (d.team != null && d.team.name == "SWACK")
		{
			data.killsAsSwack++;
		}
	}

	public static void WinLevel(Team t)
	{
	}

	public static void WinMatch(Team t)
	{
		if (!_data.hatWins.ContainsKey(t.name))
		{
			_data.hatWins[t.name] = 0;
		}
		_data.hatWins[t.name]++;
	}

	public static void PlayCustomLevel(string lev)
	{
		if (!_data.customMapPlayCount.ContainsKey(lev))
		{
			_data.customMapPlayCount[lev] = 0;
		}
		_data.customMapPlayCount[lev]++;
	}

	public static void Save()
	{
		if (!loadCalled)
		{
			if (MonoMain.logFileOperations)
			{
				DevConsole.Log(DCSection.General, "Global.Save() skipped (loadCalled == false)");
			}
			return;
		}
		if (MonoMain.logFileOperations)
		{
			DevConsole.Log(DCSection.General, "Global.Save()");
		}
		DuckXML doc = new DuckXML();
		DXMLNode data = new DXMLNode("GlobalData");
		_data.boughtHats = "";
		foreach (string s in boughtHats)
		{
			GlobalData globalData = _data;
			globalData.boughtHats = globalData.boughtHats + s + "|";
		}
		data.Add(_data.Serialize());
		doc.Add(data);
		string fileName = globalFileName;
		DuckFile.SaveDuckXML(doc, fileName);
	}

	public static void Load()
	{
		if (MonoMain.logFileOperations)
		{
			DevConsole.Log(DCSection.General, "Global.Load()");
		}
		loadCalled = true;
		string fileName = globalFileName;
		DXMLNode doc = _customLoadDoc;
		if (doc == null)
		{
			doc = DuckFile.LoadDuckXML(fileName);
		}
		if (doc != null)
		{
			new Profile("");
			IEnumerable<DXMLNode> root = doc.Elements("GlobalData");
			if (root != null)
			{
				foreach (DXMLNode element in root.Elements())
				{
					if (element.Name == "Global")
					{
						_data.Deserialize(element);
						break;
					}
				}
			}
		}
		string[] array = _data.boughtHats.Split('|');
		foreach (string s in array)
		{
			if (s != "")
			{
				boughtHats.Add(s);
			}
		}
	}
}
