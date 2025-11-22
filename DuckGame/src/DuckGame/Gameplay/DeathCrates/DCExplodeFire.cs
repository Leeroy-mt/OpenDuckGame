using System;

namespace DuckGame;

public class DCExplodeFire : DeathCrateSetting
{
	public override void Activate(DeathCrate c, bool server = true)
	{
		float cx = c.x;
		float cy = c.y - 2f;
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
		if (server)
		{
			for (int j = 0; j < 16; j++)
			{
				Level.Add(SmallFire.New(c.x - 6f + Rando.Float(12f), c.y - 8f + Rando.Float(4f), -6f + Rando.Float(12f), 2f - Rando.Float(8.5f), shortLife: false, null, canMultiply: true, c));
			}
			Level.Remove(c);
		}
		Graphics.FlashScreen();
		SFX.Play("explode");
		RumbleManager.AddRumbleEvent(c.position, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
	}
}
