namespace DuckGame;

public class NMMeltTile : NMEvent
{
    public short x;

    public short y;

    public NMMeltTile()
    {
    }

    public NMMeltTile(Vec2 pPosition)
    {
        x = (short)pPosition.X;
        y = (short)pPosition.Y;
    }

    public override void Activate()
    {
        Level.CheckPoint<SnowTileset>(new Vec2(x, y))?.Melt(pServer: false, pNetMessage: true);
        base.Activate();
    }
}
