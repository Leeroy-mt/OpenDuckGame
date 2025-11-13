using System;
using System.Collections.Generic;
using System.Reflection;

namespace DuckGame;

public class Script
{
	private static Profile _activeProfile;

	private static int _currentPosition;

	private static List<List<Profile>> _positions;

	private static PropertyInfo _activeProfileProperty;

	private static DuckNews _activeNewsStory;

	private static Dictionary<string, MethodInfo> _availableFunctions = new Dictionary<string, MethodInfo>();

	private static List<string> _highlightRatings = new List<string>
	{
		"Ughhhh...", "I fell asleep", "Really Boring", "Kinda Boring", "Not Terribly Exciting", "About Average", "Mildly Entertaining", "Pretty Exciting", "Awesome", "Super Awesome",
		"Heart Stopping Action", "Insane Non-stop Insanity"
	};

	public static Profile activeProfile
	{
		get
		{
			return _activeProfile;
		}
		set
		{
			_activeProfile = value;
		}
	}

	public static int currentPosition
	{
		get
		{
			return _currentPosition;
		}
		set
		{
			_currentPosition = value;
		}
	}

	public static List<List<Profile>> positions
	{
		get
		{
			return _positions;
		}
		set
		{
			_positions = value;
		}
	}

	public static DuckNews activeNewsStory
	{
		get
		{
			return _activeNewsStory;
		}
		set
		{
			_activeNewsStory = value;
		}
	}

	public static MethodInfo GetMethod(string name)
	{
		MethodInfo info = null;
		_availableFunctions.TryGetValue(name, out info);
		return info;
	}

	public static object CallMethod(string name, object value)
	{
		MethodInfo info = GetMethod(name);
		if (info != null)
		{
			return info.Invoke(null, (value == null) ? null : new object[1] { value });
		}
		return null;
	}

	public static void Initialize()
	{
		_activeProfile = Profiles.DefaultPlayer1;
		_activeProfileProperty = typeof(Script).GetProperty("activeProfile", BindingFlags.Static | BindingFlags.Public);
		MethodInfo[] methods = typeof(Script).GetMethods(BindingFlags.Static | BindingFlags.Public);
		foreach (MethodInfo p in methods)
		{
			_availableFunctions[p.Name] = p;
		}
	}

	public static int profileScore()
	{
		return activeProfile.endOfRoundStats.GetProfileScore();
	}

	public static int negProfileScore()
	{
		return -activeProfile.endOfRoundStats.GetProfileScore();
	}

	public static ScriptObject stat(string statName)
	{
		PropertyInfo prop = typeof(ProfileStats).GetProperty(statName);
		if (prop != null)
		{
			return new ScriptObject
			{
				obj = activeProfile.endOfRoundStats,
				objectProperty = prop
			};
		}
		return null;
	}

	public static ScriptObject statNegative(string statName)
	{
		PropertyInfo prop = typeof(ProfileStats).GetProperty(statName);
		if (prop != null)
		{
			return new ScriptObject
			{
				obj = activeProfile.endOfRoundStats,
				objectProperty = prop,
				negative = true
			};
		}
		return null;
	}

	public static string coolnessString()
	{
		return activeProfile.endOfRoundStats.GetCoolnessString();
	}

	public static ScriptObject prevStat(string statName)
	{
		PropertyInfo prop = typeof(ProfileStats).GetProperty(statName);
		if (prop != null)
		{
			return new ScriptObject
			{
				obj = activeProfile.prevStats,
				objectProperty = prop
			};
		}
		return null;
	}

	public static string previousTitleOwner(string name)
	{
		DuckTitle t = DuckTitle.GetTitle(name);
		if (t != null)
		{
			return t.previousOwner;
		}
		return "";
	}

	public static float sin(float val)
	{
		return (float)Math.Sin(val);
	}

	public static float cos(float val)
	{
		return (float)Math.Cos(val);
	}

	public static float round(float val)
	{
		return (float)Math.Round(val);
	}

	public static float toFloat(int val)
	{
		return val;
	}

	public static int place()
	{
		return currentPosition;
	}

	public static float random()
	{
		return Rando.Float(1f);
	}

	public static string winner()
	{
		return Results.winner.name;
	}

	public static string RatingsString(int wow)
	{
		if (wow > Global.data.highestNewsCast)
		{
			Global.data.highestNewsCast = wow;
		}
		int min = 60;
		int max = 250 + (int)((float)Global.data.highestNewsCast * Rando.Float(0.1f, 0.25f));
		if (wow < min)
		{
			wow = min;
		}
		if (wow > max)
		{
			wow = max;
		}
		wow -= min;
		float val = (float)wow / (float)(max - min);
		return _highlightRatings[(int)Math.Round(val * (float)(_highlightRatings.Count - 1))];
	}

	public static string highlightRating()
	{
		float coolPoints = 0f;
		List<Recording> highlights = Highlights.GetHighlights();
		foreach (Recording r in highlights)
		{
			coolPoints += r.highlightScore;
		}
		coolPoints /= (float)highlights.Count;
		coolPoints *= 1.5f;
		return RatingsString((int)coolPoints);
	}

	public static float floatVALUE()
	{
		if (_activeNewsStory != null && _activeNewsStory.valueCalculation != null)
		{
			object res = _activeNewsStory.valueCalculation.result;
			if (res != null)
			{
				return Change.ToSingle(res);
			}
		}
		return 0f;
	}

	public static float floatVALUE2()
	{
		if (_activeNewsStory != null && _activeNewsStory.valueCalculation != null)
		{
			object res = _activeNewsStory.valueCalculation2.result;
			if (res != null)
			{
				return Change.ToSingle(res);
			}
		}
		return 0f;
	}

	public static int numInPlace(int p)
	{
		if (positions == null || p < 0 || p >= positions.Count)
		{
			return 0;
		}
		return positions[positions.Count - 1 - p].Count;
	}

	public static bool skippedNewscast()
	{
		return HighlightLevel.didSkip;
	}

	public static bool hasPurchaseInfo()
	{
		return Main.foundPurchaseInfo;
	}

	public static bool doesNotHavePurchaseInfo()
	{
		return !Main.foundPurchaseInfo;
	}

	public static bool isDemo()
	{
		return Main.isDemo;
	}

	public static bool isNotDemo()
	{
		return !Main.isDemo;
	}

	public static float greatest(string val)
	{
		float greatestVal = -99999f;
		foreach (Profile item in Profiles.active)
		{
			_ = item;
			float curVal = -99999f;
			ScriptObject scriptObject = stat(val);
			if (scriptObject != null)
			{
				curVal = Change.ToSingle(scriptObject.objectProperty.GetValue(scriptObject.obj, null)) * (float)((!scriptObject.negative) ? 1 : (-1));
			}
			if (curVal > greatestVal)
			{
				greatestVal = curVal;
			}
		}
		return greatestVal;
	}

	public static bool hasGreatest(string val)
	{
		float greatestVal = -999999f;
		Profile greatestProfile = null;
		foreach (Profile p in Profiles.active)
		{
			float curVal = -999999f;
			Profile storeProfile = activeProfile;
			activeProfile = p;
			if (_activeNewsStory != null && val == "VALUE")
			{
				object res = _activeNewsStory.valueCalculation.result;
				if (res != null)
				{
					curVal = Change.ToSingle(res);
				}
			}
			else if (val != "VALUE")
			{
				ScriptObject scriptObject = stat(val);
				if (scriptObject != null)
				{
					curVal = Change.ToSingle(scriptObject.objectProperty.GetValue(scriptObject.obj, null)) * (float)((!scriptObject.negative) ? 1 : (-1));
				}
			}
			activeProfile = storeProfile;
			if (curVal > greatestVal)
			{
				greatestVal = curVal;
				greatestProfile = p;
			}
		}
		return greatestProfile == activeProfile;
	}
}
