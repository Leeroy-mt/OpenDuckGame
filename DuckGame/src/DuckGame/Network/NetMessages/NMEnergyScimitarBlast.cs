using Microsoft.Xna.Framework;

namespace DuckGame;

public class NMEnergyScimitarBlast : NMEvent
{
    public Vector2 position;

    public Vector2 target;

    public NMEnergyScimitarBlast(Vector2 pPosition, Vector2 pTarget)
    {
        position = pPosition;
        target = pTarget;
    }

    public NMEnergyScimitarBlast()
    {
    }

    public override void Activate()
    {
        if (Level.current != null)
        {
            Level.Add(new EnergyScimitarBlast(position, target));
        }
    }
}
