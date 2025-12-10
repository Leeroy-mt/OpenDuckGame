using System;

namespace DuckGame;

public class DCPresentExplosion : DeathCrateSetting
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
            for (int j = 0; j < 8; j++)
            {
                Present present = new Present(c.x, c.y);
                float norm = (float)j / 7f;
                present.hSpeed = (-15f + norm * 30f) * Rando.Float(0.5f, 1f);
                present.vSpeed = Rando.Float(-3f, -11f);
                Level.Add(present);
            }
            for (int k = 0; k < 4; k++)
            {
                Flower flower = new Flower(c.x, c.y);
                float norm2 = (float)k / 3f;
                flower.hSpeed = (-10f + norm2 * 20f) * Rando.Float(0.5f, 1f);
                flower.vSpeed = Rando.Float(-3f, -11f);
                Level.Add(flower);
            }
            Level.Remove(c);
        }
        Graphics.FlashScreen();
        SFX.Play("harp");
    }
}
