using Microsoft.Xna.Framework;

namespace DuckGame;

public class NMFlowerPoof : NMEvent
{
    public Vector2 position;

    public NMFlowerPoof()
    {
    }

    public NMFlowerPoof(Vector2 pPosition)
    {
        position = pPosition;
    }

    public override void Activate()
    {
        Flower.PoofEffect(position);
        base.Activate();
    }
}
