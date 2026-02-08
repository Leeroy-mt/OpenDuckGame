using Microsoft.Xna.Framework;

namespace DuckGame;

public class NMPistolExplode : NMEvent
{
    public Vector2 position;

    public NMPistolExplode()
    {
    }

    public NMPistolExplode(Vector2 pPosition)
    {
        position = pPosition;
    }

    public override void Activate()
    {
        DuelingPistol.ExplodeEffect(position);
        base.Activate();
    }
}
