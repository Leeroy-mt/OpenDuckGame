using System;

namespace DuckGame;

public class DCHighEnergySurprise : DeathCrateSetting
{
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
            EnergyScimitar energyScimitar = new EnergyScimitar(c.X, c.Y - 8f);
            Level.Add(energyScimitar);
            energyScimitar.StartFlying(TileConnection.Left);
            EnergyScimitar energyScimitar2 = new EnergyScimitar(c.X, c.Y - 8f);
            Level.Add(energyScimitar2);
            energyScimitar2.StartFlying(TileConnection.Right);
            EnergyScimitar energyScimitar3 = new EnergyScimitar(c.X, c.Y - 8f);
            Level.Add(energyScimitar3);
            energyScimitar3.StartFlying(TileConnection.Up);
            EnergyScimitar energyScimitar4 = new EnergyScimitar(c.X, c.Y - 8f);
            Level.Add(energyScimitar4);
            energyScimitar4.StartFlying(TileConnection.Down);
            Level.Remove(c);
        }
        Graphics.FlashScreen();
        SFX.Play("explode");
        RumbleManager.AddRumbleEvent(c.Position, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
    }
}
