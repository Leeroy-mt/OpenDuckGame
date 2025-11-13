using System.Collections.Generic;

namespace DuckGame;

public class CompareHoldablePriorities : IComparer<Holdable>
{
	private Duck _duck;

	public CompareHoldablePriorities(Duck d)
	{
		_duck = d;
	}

	public int Compare(Holdable h1, Holdable h2)
	{
		if (h1 == h2)
		{
			return 0;
		}
		if (h1 is CTFPresent)
		{
			return -1;
		}
		if (h2 is CTFPresent)
		{
			return 1;
		}
		if (h1 is TrappedDuck)
		{
			return -1;
		}
		if (h2 is TrappedDuck)
		{
			return 1;
		}
		if (h1 is Equipment && _duck.HasEquipment(h1 as Equipment))
		{
			return 1;
		}
		if (h2 is Equipment && _duck.HasEquipment(h2 as Equipment))
		{
			return -1;
		}
		if (h1.PickupPriority() == h2.PickupPriority())
		{
			if ((h1.position - _duck.position).length - (h2.position - _duck.position).length < -2f)
			{
				return -1;
			}
			return 1;
		}
		if (h1.PickupPriority() < h2.PickupPriority())
		{
			return -1;
		}
		return 1;
	}
}
