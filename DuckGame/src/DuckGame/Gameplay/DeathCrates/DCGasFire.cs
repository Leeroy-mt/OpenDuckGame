namespace DuckGame;

public class DCGasFire : DeathCrateSetting
{
    public override void Activate(DeathCrate c, bool server = true)
    {
        float x = c.x;
        float cy = c.y - 2f;
        Level.Add(new ExplosionPart(x, cy));
        if (server)
        {
            Level.Add(new YellowBarrel(c.x, c.y)
            {
                vSpeed = -3f
            });
            Grenade grenade = new Grenade(c.x, c.y);
            grenade.PressAction();
            grenade.hSpeed = -1f;
            grenade.vSpeed = -2f;
            Level.Add(grenade);
            Grenade grenade2 = new Grenade(c.x, c.y);
            grenade2.PressAction();
            grenade2.hSpeed = 1f;
            grenade2.vSpeed = -2f;
            Level.Add(grenade2);
            Level.Remove(c);
        }
        Level.Add(new MusketSmoke(c.x, c.y));
    }
}
