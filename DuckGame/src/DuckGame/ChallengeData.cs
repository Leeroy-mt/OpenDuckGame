using System;
using System.Collections.Generic;

namespace DuckGame;

public class ChallengeData : Serializable
{
	private List<ChallengeTrophy> _trophies;

	private string _fileName = "";

	private string _levelID = "";

	public string preview;

	public ChallengeSaveData saveData = new ChallengeSaveData();

	public bool countTargets;

	public bool countGoodies;

	public string name = "BURGER CHALLENGE";

	public string description = "";

	public string goal = "";

	public string prefix = "";

	public string reward = "";

	public string prevchal = "";

	public string requirement = "";

	public int icon;

	private bool _updating;

	public List<ChallengeTrophy> trophies => _trophies;

	public string fileName
	{
		get
		{
			return _fileName;
		}
		set
		{
			_fileName = value;
		}
	}

	public string levelID
	{
		get
		{
			return _levelID;
		}
		set
		{
			_levelID = value;
		}
	}

	public bool hasTimeRequirements
	{
		get
		{
			if (_trophies[0].goodies > 0 || _trophies[0].targets > 0)
			{
				return true;
			}
			for (int i = 1; i < _trophies.Count; i++)
			{
				if (_trophies[i].timeRequirement > 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void LoadSaveData()
	{
	}

	public string GetNameForDisplay()
	{
		return name.ToUpperInvariant();
	}

	public bool CheckRequirement(Profile p)
	{
		return CheckRequirement(p, requirement);
	}

	public static bool CheckRequirement(Profile p, string req)
	{
		TrophyType reqType = TrophyType.Baseline;
		if (req.Length > 1)
		{
			if (req[0] == 'B')
			{
				reqType = TrophyType.Bronze;
			}
			if (req[0] == 'S')
			{
				reqType = TrophyType.Silver;
			}
			if (req[0] == 'G')
			{
				reqType = TrophyType.Gold;
			}
			if (req[0] == 'P')
			{
				reqType = TrophyType.Platinum;
			}
			if (reqType == TrophyType.Baseline)
			{
				return true;
			}
			string parseNum = req.Substring(1, req.Length - 1);
			int number = 0;
			try
			{
				number = Convert.ToInt32(parseNum);
			}
			catch
			{
				return true;
			}
			if (number == 0)
			{
				return true;
			}
			int has = 0;
			foreach (ChallengeSaveData allSaveDatum in Challenges.GetAllSaveData(p))
			{
				if (allSaveDatum.trophy >= reqType)
				{
					has++;
				}
			}
			return has >= number;
		}
		return true;
	}

	public int GetRequirementValue()
	{
		TrophyType reqType = TrophyType.Baseline;
		if (requirement.Length > 1)
		{
			if (requirement[0] == 'B')
			{
				reqType = TrophyType.Bronze;
			}
			if (requirement[0] == 'S')
			{
				reqType = TrophyType.Silver;
			}
			if (requirement[0] == 'G')
			{
				reqType = TrophyType.Gold;
			}
			if (requirement[0] == 'P')
			{
				reqType = TrophyType.Platinum;
			}
			if (requirement[0] == 'D')
			{
				reqType = TrophyType.Developer;
			}
			if (reqType == TrophyType.Baseline)
			{
				return 0;
			}
			string parseNum = requirement.Substring(1, requirement.Length - 1);
			int number = 0;
			try
			{
				number = Convert.ToInt32(parseNum);
			}
			catch
			{
				return 0;
			}
			if (number == 0)
			{
				return 0;
			}
			return number * (int)reqType;
		}
		return 0;
	}

	public ChallengeData()
	{
		_trophies = new List<ChallengeTrophy>
		{
			new ChallengeTrophy(this)
			{
				type = TrophyType.Baseline
			},
			new ChallengeTrophy(this)
			{
				type = TrophyType.Bronze
			},
			new ChallengeTrophy(this)
			{
				type = TrophyType.Silver
			},
			new ChallengeTrophy(this)
			{
				type = TrophyType.Gold
			},
			new ChallengeTrophy(this)
			{
				type = TrophyType.Platinum
			},
			new ChallengeTrophy(this)
			{
				type = TrophyType.Developer
			}
		};
	}

	public void Update()
	{
		if (!_updating)
		{
			_updating = true;
			_updating = false;
		}
	}

	public BinaryClassChunk Serialize()
	{
		BinaryClassChunk element = new BinaryClassChunk();
		SerializeField(element, "name");
		SerializeField(element, "description");
		SerializeField(element, "goal");
		SerializeField(element, "reward");
		SerializeField(element, "requirement");
		SerializeField(element, "icon");
		SerializeField(element, "countGoodies");
		SerializeField(element, "countTargets");
		SerializeField(element, "prefix");
		SerializeField(element, "prevchal");
		foreach (ChallengeTrophy t in _trophies)
		{
			element.AddProperty("trophy", t.Serialize());
		}
		return element;
	}

	public bool Deserialize(BinaryClassChunk node)
	{
		DeserializeField(node, "name");
		DeserializeField(node, "description");
		DeserializeField(node, "goal");
		DeserializeField(node, "reward");
		DeserializeField(node, "requirement");
		DeserializeField(node, "icon");
		DeserializeField(node, "countGoodies");
		DeserializeField(node, "countTargets");
		DeserializeField(node, "prefix");
		DeserializeField(node, "prevchal");
		List<BinaryClassChunk> properties = node.GetProperties<BinaryClassChunk>("trophy");
		int idx = 0;
		foreach (BinaryClassChunk e in properties)
		{
			ChallengeTrophy item = new ChallengeTrophy(this);
			item.Deserialize(e);
			_trophies[idx] = item;
			idx++;
		}
		return true;
	}

	public DXMLNode LegacySerialize()
	{
		DXMLNode element = new DXMLNode("challengeData");
		LegacySerializeField(element, "name");
		LegacySerializeField(element, "description");
		LegacySerializeField(element, "goal");
		LegacySerializeField(element, "reward");
		LegacySerializeField(element, "requirement");
		LegacySerializeField(element, "icon");
		LegacySerializeField(element, "countGoodies");
		LegacySerializeField(element, "countTargets");
		LegacySerializeField(element, "prefix");
		LegacySerializeField(element, "prevchal");
		foreach (ChallengeTrophy t in _trophies)
		{
			element.Add(t.LegacySerialize());
		}
		return element;
	}

	public bool LegacyDeserialize(DXMLNode node)
	{
		LegacyDeserializeField(node, "name");
		LegacyDeserializeField(node, "description");
		LegacyDeserializeField(node, "goal");
		LegacyDeserializeField(node, "reward");
		LegacyDeserializeField(node, "requirement");
		LegacyDeserializeField(node, "icon");
		LegacyDeserializeField(node, "countGoodies");
		LegacyDeserializeField(node, "countTargets");
		LegacyDeserializeField(node, "prefix");
		LegacyDeserializeField(node, "prevchal");
		LevelData dat = DuckFile.LoadLevel(Content.path + "levels/" + prevchal + ".lev");
		if (dat != null)
		{
			prevchal = dat.metaData.guid;
		}
		int idx = 0;
		foreach (DXMLNode e in node.Elements("challengeTrophy"))
		{
			ChallengeTrophy item = new ChallengeTrophy(this);
			item.LegacyDeserialize(e);
			_trophies[idx] = item;
			idx++;
		}
		return true;
	}
}
