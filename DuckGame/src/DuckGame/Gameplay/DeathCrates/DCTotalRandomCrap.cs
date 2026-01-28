using System;

namespace DuckGame;

public class DCTotalRandomCrap : DeathCrateSetting
{
    public DCTotalRandomCrap()
    {
        likelyhood = 0.25f;
    }

    public override void Activate(DeathCrate c, bool server = true)
    {
        float cx = c.X;
        float cy = c.Y - 2f;
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
                PhysicsObject randomItem = ItemBoxRandom.GetRandomItem();
                randomItem.Position = c.Position;
                float norm = (float)j / 7f;
                randomItem.hSpeed = (-15f + norm * 30f) * Rando.Float(0.5f, 1f);
                randomItem.vSpeed = Rando.Float(-10f, 10f);
                Level.Add(randomItem);
            }
            Level.Remove(c);
        }
        Graphics.FlashScreen();
        SFX.Play("explode");
        RumbleManager.AddRumbleEvent(c.Position, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
    }
}
