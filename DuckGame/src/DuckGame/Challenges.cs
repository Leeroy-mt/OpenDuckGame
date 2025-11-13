using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuckGame;

public class Challenges
{
	public static int valueBronze = 15;

	public static int valueSilver = 5;

	public static int valueGold = 5;

	public static int valuePlatinum = 12;

	private static Dictionary<string, ChallengeData> _challenges = new Dictionary<string, ChallengeData>();

	private static List<ChallengeData> _challengesInArcade;

	public static Dictionary<string, ChallengeData> challenges => _challenges;

	public static List<ChallengeData> challengesInArcade
	{
		get
		{
			if (_challengesInArcade == null)
			{
				_challengesInArcade = new List<ChallengeData>();
				ArcadeLevel arcade = new ArcadeLevel(Content.GetLevelID("arcade"));
				arcade.bareInitialize = true;
				arcade.InitializeMachines();
				if (arcade != null)
				{
					foreach (ArcadeMachine m in arcade._challenges)
					{
						if (m is ImportMachine)
						{
							continue;
						}
						foreach (string challenge in m.data.challenges)
						{
							ChallengeData d = GetChallenge(challenge);
							if (d != null)
							{
								challengesInArcade.Add(d);
							}
						}
					}
					foreach (ChallengeData dat in GetAllChancyChallenges(_challengesInArcade))
					{
						_challengesInArcade.Add(dat);
					}
				}
			}
			return _challengesInArcade;
		}
	}

	public static ChallengeData LoadChallengeData(string pLevel)
	{
		if (pLevel == null)
		{
			return null;
		}
		ChallengeData data = null;
		if (_challenges.TryGetValue(pLevel, out data))
		{
			return data;
		}
		LevelData doc = Content.GetLevel(pLevel);
		if (doc == null)
		{
			doc = DuckFile.LoadLevel(pLevel);
		}
		if (doc != null)
		{
			string guid = doc.metaData.guid;
			foreach (BinaryClassChunk elly in doc.objects.objects)
			{
				string typeString = elly.GetProperty<string>("type");
				ChallengeMode m = null;
				try
				{
					if (typeString != null && typeString.Contains("DuckGame.ChallengeMode,"))
					{
						m = Thing.LoadThing(elly, chance: false) as ChallengeMode;
					}
					else if (typeString != null && typeString.Contains("DuckGame.ChallengeModeNew,"))
					{
						m = Thing.LoadThing(elly, chance: false) as ChallengeModeNew;
					}
					if (m != null)
					{
						m.challenge.fileName = pLevel;
						m.challenge.levelID = guid;
						m.challenge.preview = doc.previewData.preview;
						_challenges.Add(doc.metaData.guid, m.challenge);
						return m.challenge;
					}
				}
				catch (Exception)
				{
				}
			}
		}
		return null;
	}

	public static void Initialize()
	{
		foreach (string challenge in Content.GetLevels("challenge", LevelLocation.Content, pRecursive: true, pOnline: false, pEightPlayer: false))
		{
			MonoMain.currentActionQueue.Enqueue(new LoadingAction(delegate
			{
				LoadChallengeData(challenge);
			}));
		}
	}

	public static void LoadElementsFromNode(string pName, DXMLNode pNode)
	{
		foreach (DXMLNode e in pNode.Elements("challengeSaveData"))
		{
			ChallengeSaveData dat = new ChallengeSaveData();
			dat.LegacyDeserialize(e);
			if (dat.trophy == TrophyType.Developer)
			{
				Options.Data.gotDevMedal = true;
			}
			dat.challenge = pName;
			Profile p = Profiles.Get(dat.profileID);
			if (p != null && !p.challengeData.ContainsKey(pName))
			{
				p.challengeData.Add(pName, dat);
				dat.profileID = p.id;
			}
		}
	}

	public static void InitializeChallengeData()
	{
		string[] files = DuckFile.GetFiles(DuckFile.challengeDirectory);
		foreach (string save in files)
		{
			DuckXML doc = DuckFile.LoadDuckXML(save);
			if (doc != null)
			{
				string name = Path.GetFileNameWithoutExtension(save);
				DXMLNode data = doc.Element("Data");
				if (data != null)
				{
					LoadElementsFromNode(name, data);
				}
			}
		}
	}

	public static int GetNumTrophies(Profile p)
	{
		int trophies = 0;
		foreach (KeyValuePair<string, ChallengeData> challenge in _challenges)
		{
			ChallengeSaveData dat = p.GetSaveData(challenge.Value.levelID, canBeNull: true);
			if (dat != null && dat.trophy != TrophyType.Baseline)
			{
				trophies++;
			}
		}
		return trophies;
	}

	public static ChallengeSaveData GetSaveData(string guid, Profile p, bool canBeNull = false)
	{
		return p.GetSaveData(guid, canBeNull);
	}

	public static List<ChallengeSaveData> GetAllSaveData(Profile p)
	{
		List<ChallengeSaveData> datList = new List<ChallengeSaveData>();
		foreach (KeyValuePair<string, ChallengeSaveData> challengeDatum in p.challengeData)
		{
			datList.Add(challengeDatum.Value);
		}
		return datList;
	}

	public static List<ChallengeSaveData> GetAllSaveData()
	{
		List<ChallengeSaveData> datList = new List<ChallengeSaveData>();
		foreach (Profile item in Profiles.all)
		{
			foreach (KeyValuePair<string, ChallengeSaveData> challengeDatum in item.challengeData)
			{
				datList.Add(challengeDatum.Value);
			}
		}
		return datList;
	}

	public static ChallengeData GetChallenge(string name)
	{
		ChallengeData dat = null;
		if (!_challenges.TryGetValue(name, out dat))
		{
			return LoadChallengeData(name);
		}
		return dat;
	}

	public static List<ChallengeData> GetEligibleChancyChallenges(Profile p)
	{
		List<ChallengeData> data = new List<ChallengeData>();
		foreach (KeyValuePair<string, ChallengeData> d in challenges)
		{
			if (!(d.Value.requirement != "") || !d.Value.CheckRequirement(p))
			{
				continue;
			}
			if (d.Value.prevchal != "")
			{
				ChallengeData dat = GetChallenge(d.Value.prevchal);
				ChallengeSaveData save = p.GetSaveData(dat.levelID, canBeNull: true);
				if (save != null && save.trophy > TrophyType.Baseline)
				{
					data.Add(d.Value);
				}
			}
			else
			{
				data.Add(d.Value);
			}
		}
		return data;
	}

	public static List<ChallengeData> GetAllChancyChallenges(List<ChallengeData> available = null)
	{
		List<ChallengeData> data = new List<ChallengeData>();
		foreach (KeyValuePair<string, ChallengeData> d in challenges)
		{
			if (d.Value.requirement != "" && (d.Value.prevchal == null || d.Value.prevchal == "" || available == null || available.FirstOrDefault((ChallengeData x) => x != null && x.fileName == d.Value.prevchal) != null))
			{
				data.Add(d.Value);
			}
		}
		return data;
	}

	public static List<ChallengeData> GetEligibleIncompleteChancyChallenges(Profile p)
	{
		List<ChallengeData> eligibleChancyChallenges = GetEligibleChancyChallenges(p);
		List<ChallengeData> incomplete = new List<ChallengeData>();
		foreach (ChallengeData d in eligibleChancyChallenges)
		{
			ChallengeSaveData s = p.GetSaveData(d.levelID, canBeNull: true);
			if (s == null || s.trophy < TrophyType.Bronze)
			{
				incomplete.Add(d);
			}
		}
		return incomplete;
	}

	public static float GetChallengeSkillIndex()
	{
		int val = 0;
		int max = 0;
		List<ChallengeData> challengesInArcade = new List<ChallengeData>();
		ArcadeLevel arcade = Level.current as ArcadeLevel;
		if (arcade == null)
		{
			arcade = ArcadeLevel.currentArcade;
		}
		if (arcade != null)
		{
			foreach (ArcadeMachine challenge in arcade._challenges)
			{
				foreach (string s in challenge.data.challenges)
				{
					challengesInArcade.Add(GetChallenge(s));
				}
			}
			foreach (ChallengeData dat in GetAllChancyChallenges(challengesInArcade))
			{
				challengesInArcade.Add(dat);
			}
			foreach (KeyValuePair<string, ChallengeData> dat2 in _challenges)
			{
				if (challengesInArcade.Contains(dat2.Value))
				{
					max += 4;
					ChallengeSaveData saveDat = Profiles.active[0].GetSaveData(dat2.Value.levelID, canBeNull: true);
					if (saveDat != null)
					{
						val = (int)(val + saveDat.trophy);
					}
				}
			}
			return (float)val / (float)max;
		}
		return 0f;
	}

	public static int GetTicketCount(Profile p)
	{
		int max = 0;
		foreach (KeyValuePair<string, ChallengeData> dat in _challenges)
		{
			if (!challengesInArcade.Contains(dat.Value))
			{
				continue;
			}
			ChallengeSaveData saveDat = p.GetSaveData(dat.Value.levelID, canBeNull: true);
			if (saveDat != null)
			{
				if (saveDat.trophy >= TrophyType.Bronze)
				{
					max += valueBronze;
				}
				if (saveDat.trophy >= TrophyType.Silver)
				{
					max += valueSilver;
				}
				if (saveDat.trophy >= TrophyType.Gold)
				{
					max += valueGold;
				}
				if (saveDat.trophy >= TrophyType.Platinum)
				{
					max += valuePlatinum;
				}
			}
		}
		foreach (UnlockData dat2 in Unlocks.GetUnlocks(UnlockType.Any))
		{
			if (dat2.ProfileUnlocked(p))
			{
				max -= dat2.cost;
			}
		}
		return max;
	}
}
