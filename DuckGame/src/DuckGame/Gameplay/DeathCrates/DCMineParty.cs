using System;

namespace DuckGame;

public class DCMineParty : DeathCrateSetting
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
            for (int j = 0; j < 10; j++)
            {
                Mine mine = new Mine(c.x, c.y);
                float norm = (float)j / 9f;
                mine.hSpeed = (-20f + norm * 40f) * Rando.Float(0.5f, 1f);
                mine.vSpeed = Rando.Float(-3f, -11f);
                mine.PressAction();
                mine.Arm();
                Level.Add(mine);
            }
            Level.Remove(c);
        }
        Graphics.FlashScreen();
        SFX.Play("explode");
        RumbleManager.AddRumbleEvent(c.position, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
    }
}
