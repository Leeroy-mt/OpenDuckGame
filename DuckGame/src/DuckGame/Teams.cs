using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public static class Teams
{
	private static TeamsCore _core;

	public static int kCustomOffset = 5000;

	public static int kCustomSpread = 2000;

	public static TeamsCore core
	{
		get
		{
			return _core;
		}
		set
		{
			_core = value;
		}
	}

	public static SpriteMap hats => _core.hats;

	public static Team Player1 => _core.teams[0];

	public static Team Player2 => _core.teams[1];

	public static Team Player3 => _core.teams[2];

	public static Team Player4 => _core.teams[3];

	public static Team Player5 => _core.teams[4];

	public static Team Player6 => _core.teams[5];

	public static Team Player7 => _core.teams[6];

	public static Team Player8 => _core.teams[7];

	public static Team NullTeam => _core.nullTeam;

	public static int numTeams => _core.all.Count;

	public static List<Team> all => _core.all;

	public static List<Team> allStock => _core.allStock;

	public static List<Team> allRandomized
	{
		get
		{
			List<Team> teams = new List<Team>();
			teams.AddRange(all);
			List<Team> randomizeTeams = new List<Team>();
			while (teams.Count > 0)
			{
				int index = Rando.Int(teams.Count - 1);
				randomizeTeams.Add(teams[index]);
				teams.RemoveAt(index);
			}
			return randomizeTeams;
		}
	}

	public static List<Team> active
	{
		get
		{
			List<Team> teams = new List<Team>();
			foreach (Team t in all)
			{
				if (t.activeProfiles.Where((Profile x) => x.slotType != SlotType.Spectator).Count() > 0)
				{
					teams.Add(t);
				}
			}
			return teams;
		}
	}

	public static List<Team> winning
	{
		get
		{
			List<Team> teams = new List<Team>();
			foreach (Team t in all)
			{
				if (t.activeProfiles.Count > 0)
				{
					if (teams.Count == 0 || t.score > teams[0].score)
					{
						teams.Clear();
						teams.Add(t);
					}
					else if (teams.Count != 0 && t.score == teams[0].score)
					{
						teams.Add(t);
					}
				}
			}
			return teams;
		}
	}

	public static Team GetTeam(string name)
	{
		Team t = _core.all.FirstOrDefault((Team x) => x.name == name);
		if (t == null)
		{
			return _core.teams[8];
		}
		return t;
	}

	public static int IndexOf(Team t)
	{
		if (Network.isActive && _core.extraTeams.Contains(t))
		{
			return DuckNetwork.localProfile.customTeamIndexOffset + _core.extraTeams.IndexOf(t);
		}
		if (t.owner != null)
		{
			return t.owner.IndexOfCustomTeam(t);
		}
		return _core.all.IndexOf(t);
	}

	public static Team ParseFromIndex(ushort pIndex)
	{
		Team t = null;
		try
		{
			if (pIndex >= 0)
			{
				if (pIndex < kCustomOffset)
				{
					t = all[pIndex];
				}
				else
				{
					int profileIndex = (pIndex - kCustomOffset) / kCustomSpread;
					if (profileIndex >= 0 && profileIndex < DuckNetwork.profilesFixedOrder.Count)
					{
						int realTeamIndex = (pIndex - kCustomOffset) % kCustomSpread;
						t = DuckNetwork.profilesFixedOrder[profileIndex].GetCustomTeam((ushort)realTeamIndex);
					}
				}
			}
		}
		catch (Exception)
		{
		}
		return t;
	}

	public static int CurrentGameTeamIndex(Team t)
	{
		List<Team> teams = new List<Team>();
		foreach (Team tm in active)
		{
			if (tm.activeProfiles.Count > 1)
			{
				teams.Add(tm);
			}
		}
		return teams.IndexOf(t);
	}

	public static void AddExtraTeam(Team t)
	{
		_core.extraTeams.Add(t);
	}

	public static void Initialize()
	{
		if (_core == null)
		{
			_core = new TeamsCore();
			_core.Initialize();
		}
	}

	public static void PostInitialize()
	{
		foreach (Team deserializedTeam in Team.deserializedTeams)
		{
			AddExtraTeam(deserializedTeam);
		}
	}
}
