namespace DuckGame;

public class DCIonCannon : DeathCrateSetting
{
    public override void Activate(DeathCrate c, bool server = true)
    {
        float x = c.X;
        float cy = c.Y - 2f;
        Level.Add(new ExplosionPart(x, cy));
        Level.Add(new IonCannon(new Vec2(c.X, c.Y + 3000f), new Vec2(c.X, c.Y - 3000f))
        {
            serverVersion = server
        });
        Graphics.FlashScreen();
        SFX.Play("laserBlast");
        if (server)
        {
            Level.Remove(c);
        }
    }
}
