using System;

namespace DuckGame;

public class Net : PhysicsObject
{
	private SpriteMap _sprite;

	protected new Duck _owner;

	public Net(float xpos, float ypos, Duck owner)
		: base(xpos, ypos)
	{
		_sprite = new SpriteMap("net", 16, 16);
		graphic = _sprite;
		center = new Vec2(8f, 7f);
		collisionOffset = new Vec2(-6f, -5f);
		collisionSize = new Vec2(12f, 12f);
		base.depth = -0.5f;
		thickness = 2f;
		weight = 1f;
		_owner = owner;
		_impactThreshold = 0.01f;
	}

	public override void Update()
	{
		if (Math.Abs(hSpeed) + Math.Abs(vSpeed) > 0.1f)
		{
			base.angleDegrees = 0f - Maths.PointDirection(Vec2.Zero, new Vec2(hSpeed, vSpeed));
		}
		if (base.grounded && Math.Abs(vSpeed) + Math.Abs(hSpeed) <= 0f)
		{
			base.alpha -= 0.2f;
		}
		if (base.alpha <= 0f)
		{
			Level.Remove(this);
		}
		if (!base.onFire && Level.CheckRect<SmallFire>(position + new Vec2(-4f, -4f), position + new Vec2(4f, 4f), this) != null)
		{
			base.onFire = true;
			Level.Add(SmallFire.New(0f, 0f, 0f, 0f, shortLife: false, this, canMultiply: true, this));
		}
		base.Update();
	}

	public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
	{
		if (Network.isActive && connection != DuckNetwork.localConnection)
		{
			return;
		}
		if (with is Duck { inNet: false, dead: false } d)
		{
			d.Netted(this);
			if (d._trapped != null)
			{
				for (int i = 0; i < 4; i++)
				{
					SmallSmoke smallSmoke = SmallSmoke.New(d._trapped.x + Rando.Float(-4f, 4f), d._trapped.y + Rando.Float(-4f, 4f));
					smallSmoke.hSpeed += d._trapped.hSpeed * Rando.Float(0.3f, 0.5f);
					smallSmoke.vSpeed -= Rando.Float(0.1f, 0.2f);
					Level.Add(smallSmoke);
				}
			}
			if (Recorder.currentRecording != null)
			{
				Recorder.currentRecording.LogBonus();
			}
		}
		else
		{
			if (!(with is RagdollPart p) || p.doll.captureDuck == null || p.doll.captureDuck.dead)
			{
				return;
			}
			Duck d2 = p.doll.captureDuck;
			Fondle(p.doll);
			p.doll.Unragdoll();
			d2.Netted(this);
			if (d2._trapped != null)
			{
				for (int j = 0; j < 4; j++)
				{
					SmallSmoke smallSmoke2 = SmallSmoke.New(d2._trapped.x + Rando.Float(-4f, 4f), d2._trapped.y + Rando.Float(-4f, 4f));
					smallSmoke2.hSpeed += d2._trapped.hSpeed * Rando.Float(0.3f, 0.5f);
					smallSmoke2.vSpeed -= Rando.Float(0.1f, 0.2f);
					Level.Add(smallSmoke2);
				}
			}
			if (Recorder.currentRecording != null)
			{
				Recorder.currentRecording.LogBonus();
			}
		}
	}
}
