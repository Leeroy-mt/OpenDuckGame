using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class Party
{
	private static Dictionary<Profile, int> _drinks = new Dictionary<Profile, int>();

	private static Dictionary<Profile, List<PartyPerks>> _perks = new Dictionary<Profile, List<PartyPerks>>();

	public static void AddDrink(Profile p, int num)
	{
		if (!_drinks.ContainsKey(p))
		{
			_drinks[p] = 0;
		}
		_drinks[p] += num;
	}

	public static void AddPerk(Profile p, PartyPerks perk)
	{
		if (!_perks.ContainsKey(p))
		{
			_perks[p] = new List<PartyPerks>();
		}
		if (!_perks[p].Contains(perk))
		{
			_perks[p].Add(perk);
		}
	}

	public static bool HasPerk(Profile p, PartyPerks perk)
	{
		if (!TeamSelect2.partyMode)
		{
			return false;
		}
		if (_perks.ContainsKey(p) && _perks[p].Contains(perk))
		{
			return true;
		}
		return false;
	}

	public static void AddRandomPerk(Profile p)
	{
		IEnumerable<PartyPerks> perks = Enum.GetValues(typeof(PartyPerks)).Cast<PartyPerks>();
		AddPerk(p, perks.ElementAt(Rando.Int(perks.Count() - 1)));
	}

	public static int GetDrinks(Profile p)
	{
		if (_drinks.ContainsKey(p))
		{
			return _drinks[p];
		}
		return 0;
	}

	public static List<PartyPerks> GetPerks(Profile p)
	{
		if (_perks.ContainsKey(p))
		{
			return _perks[p];
		}
		return new List<PartyPerks>();
	}

	public static void Clear()
	{
		List<Profile> pros = new List<Profile>();
		foreach (KeyValuePair<Profile, int> drink in _drinks)
		{
			pros.Add(drink.Key);
		}
		foreach (Profile p in pros)
		{
			_drinks[p] = 0;
		}
		pros.Clear();
		foreach (KeyValuePair<Profile, List<PartyPerks>> perk in _perks)
		{
			pros.Add(perk.Key);
		}
		foreach (Profile p2 in pros)
		{
			_perks[p2].Clear();
		}
	}
}
