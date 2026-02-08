using Microsoft.Xna.Framework;

namespace DuckGame;

public class NMBonk : NMEvent
{
    public Vector2 position;

    public Vector2 velocity;

    public NMBonk()
    {
    }

    public NMBonk(Vector2 pPosition, Vector2 pVelocity)
    {
        position = pPosition;
        velocity = pVelocity;
    }

    public override void Activate()
    {
        Duck.MakeStars(position, velocity);
        base.Activate();
    }
}
