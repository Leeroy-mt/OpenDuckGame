using System;
using System.Collections.Generic;

namespace DuckGame;

public class GrenadeExplosion : Thing
{
	private int _explodeFrames = -1;

	public GrenadeExplosion(float xpos, float ypos)
		: base(xpos, ypos)
	{
	}

	public override void Update()
	{
		if (_explodeFrames < 0)
		{
			float cx = base.x;
			float cy = base.y - 2f;
			Level.Add(new ExplosionPart(cx, cy));
			int num = 6;
			if (Graphics.effectsLevel < 2)
			{
				num = 3;
			}
			for (int i = 0; i < num; i++)
			{
				float dir = (float)i * 60f + Rando.Float(-10f, 10f);
				float dist = Rando.Float(12f, 20f);
				Level.Add(new ExplosionPart(cx + (float)(Math.Cos(Maths.DegToRad(dir)) * (double)dist), cy - (float)(Math.Sin(Maths.DegToRad(dir)) * (double)dist)));
			}
			_explodeFrames = 4;
			return;
		}
		_explodeFrames--;
		if (_explodeFrames != 0)
		{
			return;
		}
		float cx2 = base.x;
		float cy2 = base.y - 2f;
		List<Bullet> firedBullets = new List<Bullet>();
		for (int j = 0; j < 20; j++)
		{
			float dir2 = (float)j * 18f - 5f + Rando.Float(10f);
			ATPropExplosion shrap = new ATPropExplosion();
			shrap.range = 60f + Rando.Float(18f);
			Bullet bullet = new Bullet(cx2 + (float)(Math.Cos(Maths.DegToRad(dir2)) * 6.0), cy2 - (float)(Math.Sin(Maths.DegToRad(dir2)) * 6.0), shrap, dir2);
			bullet.firedFrom = this;
			firedBullets.Add(bullet);
			Level.Add(bullet);
		}
		if (Network.isActive)
		{
			Send.Message(new NMExplodingProp(firedBullets), NetMessagePriority.ReliableOrdered);
			firedBullets.Clear();
		}
		if (Options.Data.flashing)
		{
			Graphics.flashAdd = 1.3f;
			Layer.Game.darken = 1.3f;
		}
		foreach (Window w in Level.CheckCircleAll<Window>(position, 40f))
		{
			if (Level.CheckLine<Block>(position, w.position, w) == null)
			{
				w.Destroy(new DTImpact(this));
			}
		}
		SFX.Play("explode");
		Level.Remove(this);
	}
}
