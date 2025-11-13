using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class AIStateFindGun : AIState
{
	private int _refresh;

	private Thing _target;

	public override AIState Update(Duck duck, DuckAI ai)
	{
		if (duck.holdObject == null || !(duck.holdObject is Gun))
		{
			duck.ThrowItem();
			if (_target == null)
			{
				List<Thing> guns = Level.current.things[typeof(Gun)].Where((Thing x) => (x as Gun).ammo > 0 && (x as Gun).owner == null).ToList();
				if (AI.Nearest(duck.position, guns) is Gun nearest)
				{
					_target = nearest;
					ai.SetTarget(nearest.position);
				}
				else
				{
					List<Thing> boxes = Level.current.things[typeof(ItemBox)].Where((Thing x) => !(x as ItemBox)._hit).ToList();
					if (!(AI.Nearest(duck.position, boxes) is ItemBox nearestBox))
					{
						return new AIStateWait(Rando.Float(0.8f, 1f));
					}
					_target = nearestBox;
					ai.SetTarget(nearestBox.position + new Vec2(0f, 32f));
				}
			}
			else if (_target is ItemBox)
			{
				if (Math.Abs(_target.x - duck.x) < 8f)
				{
					ai.locomotion.Jump(15);
					return new AIStateWait(Rando.Float(0.8f, 1f));
				}
			}
			else if (_target.owner != null && _target.owner != duck)
			{
				_target = null;
			}
			else if ((_target.position - duck.position).length < 18f)
			{
				ai.Press("GRAB");
			}
			else
			{
				_refresh++;
				if (_refresh > 10 && ai.canRefresh)
				{
					_target = null;
					ai.canRefresh = false;
				}
			}
			return this;
		}
		return null;
	}
}
