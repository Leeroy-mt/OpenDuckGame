using Microsoft.Xna.Framework;

namespace DuckGame;

public class NMPresentOpen : NMEvent
{
    public Vector2 position;

    public byte frame;

    public NMPresentOpen()
    {
    }

    public NMPresentOpen(Vector2 pPosition, byte pFrame)
    {
        position = pPosition;
        frame = pFrame;
    }

    public override void Activate()
    {
        Present.OpenEffect(position, frame, pIsNetMessage: true);
        base.Activate();
    }
}
