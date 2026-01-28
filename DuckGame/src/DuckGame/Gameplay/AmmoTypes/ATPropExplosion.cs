namespace DuckGame;

public class ATPropExplosion : ATShrapnel
{
    public override void MakeNetEffect(Vec2 pos, bool fromNetwork = false)
    {
        for (int repeat = 0; repeat < 1; repeat++)
        {
            ExplosionPart explosionPart = new ExplosionPart(pos.X - 8f + Rando.Float(16f), pos.Y - 8f + Rando.Float(16f));
            explosionPart.ScaleX *= 0.7f;
            explosionPart.ScaleY *= 0.7f;
            Level.Add(explosionPart);
        }
        SFX.Play("explode");
        Graphics.FlashScreen();
    }
}
