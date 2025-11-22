using System;

namespace DuckGame;

public class DCNetCity : DeathCrateSetting
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
				float dir2 = (float)j * 22.5f + Rando.Float(-8f, 8f);
				float dist2 = Rando.Float(8f, 14f);
				Level.Add(new Net(c.x, c.y, null)
				{
					hSpeed = (float)Math.Cos(Maths.DegToRad(dir2)) * dist2,
					vSpeed = (float)(0.0 - Math.Sin(Maths.DegToRad(dir2))) * dist2
				});
			}
			Level.Remove(c);
		}
		Graphics.FlashScreen();
		SFX.Play("explode");
		RumbleManager.AddRumbleEvent(c.position, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
	}
}
