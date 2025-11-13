using System;
using System.Collections.Generic;

namespace DuckGame;

public class NGeneratorRule
{
	private Func<bool> _check;

	public NGeneratorRule(Func<bool> pCheck)
	{
		_check = pCheck;
	}

	public static int Count(HashSet<Thing> pThings, Func<Thing, bool> pCheck)
	{
		int count = 0;
		foreach (Thing t in pThings)
		{
			if (pCheck(t))
			{
				count++;
			}
		}
		return count;
	}

	public virtual bool Check()
	{
		return _check();
	}
}
