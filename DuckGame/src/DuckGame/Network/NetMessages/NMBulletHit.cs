namespace DuckGame;

public class NMBulletHit : NMEvent
{
    private float x;

    private float y;

    public NMBulletHit()
    {
    }

    public NMBulletHit(Vec2 pos)
    {
        x = pos.X;
        y = pos.Y;
    }

    public override void Activate()
    {
    }
}
