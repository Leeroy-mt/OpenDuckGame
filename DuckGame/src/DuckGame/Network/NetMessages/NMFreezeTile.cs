using Microsoft.Xna.Framework;

namespace DuckGame;

public class NMFreezeTile : NMEvent
{
    public short x;

    public short y;

    public NMFreezeTile()
    {
    }

    public NMFreezeTile(Vector2 pPosition)
    {
        x = (short)pPosition.X;
        y = (short)pPosition.Y;
    }

    public override void Activate()
    {
        Level.CheckPoint<SnowTileset>(new Vector2(x, y))?.Freeze(pServer: false, pNetMessage: true);
        base.Activate();
    }
}
