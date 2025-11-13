using System;
using System.Collections.Generic;

namespace DuckGame;

public class Unlocks
{
	public static int bronzeTotalTickets;

	public static int silverTotalTickets;

	public static int goldTotalTickets;

	public static int platinumTotalTickets;

	private static List<UnlockData> _unlocks = new List<UnlockData>();

	private static List<UnlockData> _allUnlocks = new List<UnlockData>();

	public static Dictionary<string, byte> modifierToByte = new Dictionary<string, byte>();

	public static List<UnlockData> unlocks => new List<UnlockData>(_unlocks);

	public static List<UnlockData> allUnlocks => new List<UnlockData>(_allUnlocks);

	public static void CalculateTreeValues()
	{
		ArcadeLevel arcadeLevel = new ArcadeLevel(Content.GetLevelID("arcade"));
		arcadeLevel.InitializeMachines();
		int numChallenges = 0;
		foreach (ArcadeMachine m in arcadeLevel._challenges)
		{
			numChallenges += m.data.challenges.Count;
		}
		numChallenges += Challenges.GetAllChancyChallenges().Count;
		int bronzeValue = numChallenges * Challenges.valueBronze;
		int silverValue = numChallenges * Challenges.valueSilver;
		int goldValue = numChallenges * Challenges.valueGold;
		int platinumValue = numChallenges * Challenges.valuePlatinum;
		int bronzeTotal = bronzeValue;
		int silverTotal = bronzeValue + silverValue;
		int goldTotal = bronzeValue + silverValue + goldValue;
		int num = bronzeValue + silverValue + goldValue + platinumValue;
		bronzeTotalTickets = bronzeTotal;
		silverTotalTickets = silverTotal;
		goldTotalTickets = goldTotal;
		platinumTotalTickets = num;
		int numCheap = 0;
		int numNormal = 0;
		int numHigh = 0;
		int numRidiculous = 0;
		foreach (UnlockData unlock in GetUnlocks(UnlockType.Any))
		{
			if (unlock.priceTier == UnlockPrice.Cheap)
			{
				numCheap++;
			}
			else if (unlock.priceTier == UnlockPrice.Normal)
			{
				numNormal++;
			}
			else if (unlock.priceTier == UnlockPrice.High)
			{
				numHigh++;
			}
			else if (unlock.priceTier == UnlockPrice.Ridiculous)
			{
				numRidiculous++;
			}
		}
		int num2 = (int)Math.Round((float)goldTotal * 0.1f);
		int normalTickets = (int)Math.Round((float)goldTotal * 0.3f);
		int highTickets = (int)Math.Round((float)goldTotal * 0.4f);
		int ridiculousTickets = (int)Math.Round((float)goldTotal * 0.2f);
		int costPerCheap = (int)Math.Round((float)num2 / (float)numCheap);
		int costPerNormal = (int)Math.Round((float)normalTickets / (float)numNormal);
		int costPerHigh = (int)Math.Round((float)highTickets / (float)numHigh);
		int costPerRidiculous = (int)Math.Round((float)ridiculousTickets / (float)numRidiculous);
		while (costPerCheap * numCheap + costPerNormal * numNormal + costPerHigh * numHigh + costPerRidiculous * numRidiculous > goldTotal)
		{
			costPerRidiculous--;
		}
		for (; costPerCheap * numCheap + costPerNormal * numNormal + costPerHigh * numHigh + costPerRidiculous * numRidiculous < goldTotal; costPerRidiculous++)
		{
		}
		foreach (UnlockData unlock2 in GetUnlocks(UnlockType.Any))
		{
			if (unlock2.priceTier == UnlockPrice.Cheap)
			{
				unlock2.cost = costPerCheap;
			}
			else if (unlock2.priceTier == UnlockPrice.Normal)
			{
				unlock2.cost = costPerNormal;
			}
			else if (unlock2.priceTier == UnlockPrice.High)
			{
				unlock2.cost = costPerHigh;
			}
			else if (unlock2.priceTier == UnlockPrice.Ridiculous)
			{
				unlock2.cost = costPerRidiculous;
			}
			else if (unlock2.priceTier == UnlockPrice.Chancy)
			{
				unlock2.cost = platinumValue;
			}
		}
	}

	public static bool IsUnlocked(string unlock, Profile pro = null)
	{
		foreach (Profile p in Profiles.all)
		{
			if ((pro == null || p == pro) && p.unlocks.Contains(unlock))
			{
				return true;
			}
		}
		return false;
	}

	public static List<UnlockData> GetTreeLayer(int layer)
	{
		if (layer == 0)
		{
			return new List<UnlockData>(_unlocks);
		}
		int curLayer = 0;
		List<UnlockData> dat = new List<UnlockData>();
		List<UnlockData> cur = new List<UnlockData>(_unlocks);
		for (int i = 0; i < cur.Count; i++)
		{
			if (cur[i].children.Count > 0)
			{
				foreach (UnlockData udat in cur[i].children)
				{
					if (!dat.Contains(udat))
					{
						dat.Add(udat);
					}
				}
			}
			if (i == cur.Count - 1)
			{
				if (curLayer == layer - 1)
				{
					return dat;
				}
				cur = new List<UnlockData>(dat);
				dat.Clear();
				curLayer++;
				i = -1;
			}
		}
		return null;
	}

	public static List<UnlockData> GetUnlocks(UnlockType type)
	{
		if (type == UnlockType.Any)
		{
			return new List<UnlockData>(_allUnlocks);
		}
		List<UnlockData> dat = new List<UnlockData>();
		foreach (UnlockData d in _allUnlocks)
		{
			if (d.type == type)
			{
				dat.Add(d);
			}
		}
		return dat;
	}

	public static UnlockData GetUnlock(string id)
	{
		foreach (UnlockData d in _allUnlocks)
		{
			if (d.id == id)
			{
				return d;
			}
		}
		return null;
	}

	public static void Initialize()
	{
		UnlockData key = new UnlockData
		{
			name = "Chancy's Key",
			id = "BASEMENTKEY",
			longDescription = "The key to the basement, where some tricky machines are!",
			type = UnlockType.Special,
			cost = 180,
			description = "You'll need this for the basement.",
			icon = 24,
			priceTier = UnlockPrice.Ridiculous,
			layer = 3
		};
		_allUnlocks.Add(key);
		UnlockData lowGravityUnlock = new UnlockData
		{
			name = "Moon Gravity",
			id = "MOOGRAV",
			type = UnlockType.Modifier,
			cost = 15,
			description = "Ain't it time|CONCERNED| you got higher?",
			longDescription = "Gravity is greatly reduced. Ducks jump higher and throw things further.",
			icon = 3,
			priceTier = UnlockPrice.Cheap
		};
		_unlocks.Add(lowGravityUnlock);
		_allUnlocks.Add(lowGravityUnlock);
		UnlockData helmetUnlock = new UnlockData
		{
			name = "Start With Helmet",
			id = "HELMY",
			longDescription = "Ducks start the round with a helmet.",
			type = UnlockType.Modifier,
			cost = 20,
			description = "|CONCERNED|You're tired of being crushed?",
			icon = 11,
			priceTier = UnlockPrice.Cheap
		};
		lowGravityUnlock.AddChild(helmetUnlock);
		_allUnlocks.Add(helmetUnlock);
		UnlockData explodingCrate = new UnlockData
		{
			name = "Exploding Props",
			id = "EXPLODEYCRATES",
			longDescription = "Props, such as rocks and crates, will explode when shot.",
			type = UnlockType.Modifier,
			cost = 30,
			description = "Watch  |CONCERNED|where you shoot, OK?.",
			icon = 6,
			priceTier = UnlockPrice.Normal
		};
		lowGravityUnlock.AddChild(explodingCrate);
		_allUnlocks.Add(explodingCrate);
		UnlockData infiniteAmmo = new UnlockData
		{
			name = "Ammo, Infinite",
			shortName = "Infinite Ammo",
			id = "INFAMMO",
			longDescription = "Guns will spawn as golden guns, and will not run out of ammo.",
			type = UnlockType.Modifier,
			cost = 35,
			description = "Just shoot. Don't even aim.",
			icon = 13,
			priceTier = UnlockPrice.High
		};
		explodingCrate.AddChild(infiniteAmmo);
		_allUnlocks.Add(infiniteAmmo);
		UnlockData explodingGun = new UnlockData
		{
			name = "Empty Guns Explode",
			id = "GUNEXPL",
			longDescription = "If you press fire after your gun is empty, it will explode in your hands and kill you.",
			type = UnlockType.Modifier,
			cost = 40,
			description = "Hey, |CONCERNED|watch your ammo count.",
			icon = 17,
			priceTier = UnlockPrice.Normal
		};
		key.AddChild(explodingGun);
		_allUnlocks.Add(explodingGun);
		UnlockData hatPack2 = new UnlockData
		{
			name = "Hat Pack 2",
			id = "HATTY2",
			type = UnlockType.Hat,
			cost = 40,
			description = "More hats!",
			icon = 15,
			priceTier = UnlockPrice.High
		};
		key.AddChild(hatPack2);
		_allUnlocks.Add(hatPack2);
		UnlockData hatPack3 = new UnlockData
		{
			name = "Hat Pack 1",
			id = "HATTY1",
			type = UnlockType.Hat,
			cost = 20,
			description = "Some hats.",
			icon = 14,
			priceTier = UnlockPrice.Normal
		};
		_unlocks.Add(hatPack3);
		_allUnlocks.Add(hatPack3);
		UnlockData winnerPresent = new UnlockData
		{
			name = "Presents for Winners",
			id = "WINPRES",
			longDescription = "The winners of every round get a present for the next round.",
			type = UnlockType.Modifier,
			cost = 25,
			description = "It's probably just a rock.",
			icon = 12,
			priceTier = UnlockPrice.Normal
		};
		hatPack3.AddChild(winnerPresent);
		_allUnlocks.Add(winnerPresent);
		UnlockData startWithShoes = new UnlockData
		{
			name = "Start With Shoes",
			id = "SHOESTAR",
			longDescription = "Ducks start the round with shoes.",
			type = UnlockType.Modifier,
			cost = 20,
			description = "The most stylin' unlock around.",
			icon = 5,
			priceTier = UnlockPrice.Cheap
		};
		hatPack3.AddChild(startWithShoes);
		_allUnlocks.Add(startWithShoes);
		UnlockData qwopMode = new UnlockData
		{
			name = "QWOP Mode",
			id = "QWOPPY",
			longDescription = "Alternate left and right triggers to move. If you screw up, you fall over.",
			type = UnlockType.Modifier,
			cost = 35,
			description = "Practically impossible.",
			icon = 8,
			priceTier = UnlockPrice.High,
			onlineEnabled = false
		};
		winnerPresent.AddChild(qwopMode);
		_allUnlocks.Add(qwopMode);
		infiniteAmmo.AddChild(key);
		qwopMode.AddChild(key);
		UnlockData jetpack = new UnlockData
		{
			name = "Start With Jetpack",
			id = "JETTY",
			longDescription = "Ducks start the round with a jetpack.",
			type = UnlockType.Modifier,
			cost = 50,
			description = "You love it.",
			icon = 1,
			priceTier = UnlockPrice.High
		};
		key.AddChild(jetpack);
		_allUnlocks.Add(jetpack);
		UnlockData explodingCorpse = new UnlockData
		{
			name = "Live Grenade On Death",
			shortName = "Grenade On Death",
			id = "CORPSEBLOW",
			longDescription = "When killed, a duck will drop a live grenade.",
			type = UnlockType.Modifier,
			cost = 45,
			description = "Makes death deadly. |CONCERNED|err...",
			icon = 7,
			priceTier = UnlockPrice.Normal
		};
		key.AddChild(explodingCorpse);
		_allUnlocks.Add(explodingCorpse);
		UnlockData ultimate = new UnlockData
		{
			name = "Ultimate Champion",
			id = "ULTIMATE",
			longDescription = "A nifty hat, very expensive. Only the very best can afford this hat!",
			type = UnlockType.Hat,
			cost = 180,
			description = "A hat |RED|only|WHITE| for the very best!",
			icon = 19,
			priceTier = UnlockPrice.Chancy
		};
		explodingCorpse.AddChild(ultimate);
		explodingGun.AddChild(ultimate);
		hatPack2.AddChild(ultimate);
		jetpack.AddChild(ultimate);
		_allUnlocks.Add(ultimate);
		byte idx = 0;
		foreach (UnlockData u in _allUnlocks)
		{
			if (u.type == UnlockType.Modifier)
			{
				modifierToByte[u.id] = idx;
				idx++;
			}
		}
	}
}
