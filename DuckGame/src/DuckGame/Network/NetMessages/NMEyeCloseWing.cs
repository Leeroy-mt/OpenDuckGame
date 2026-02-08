using Microsoft.Xna.Framework;

namespace DuckGame;

public class NMEyeCloseWing : NMEvent
{
    public Vector2 position;

    public Duck closer;

    public Duck closee;

    public NMEyeCloseWing()
    {
    }

    public NMEyeCloseWing(Vector2 pPosition, Duck pCloser, Duck pClosee)
    {
        position = pPosition;
        closer = pCloser;
        closee = pClosee;
    }

    public override void Activate()
    {
        if (closer != null && closee != null && closee.ragdoll != null && closee.ragdoll.part1 != null)
        {
            Level.Add(new EyeCloseWing((closee.ragdoll.part1.Angle < 0f) ? (position.X - 4f) : (position.X - 11f), position.Y + 7f, (closee.ragdoll.part1.Angle < 0f) ? 1 : (-1), closer._spriteArms, closer, closee));
        }
        base.Activate();
    }
}
