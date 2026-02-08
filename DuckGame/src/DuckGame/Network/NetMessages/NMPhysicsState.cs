using Microsoft.Xna.Framework;

namespace DuckGame;

public class NMPhysicsState : NMObjectMessage
{
    public Vector2 position;

    public Vector2 velocity;

    public int clientFrame;

    public NMPhysicsState()
    {
    }

    public NMPhysicsState(Vector2 Position, Vector2 Velocity, ushort ObjectID, int ClientFrame)
        : base(ObjectID)
    {
        position = Position;
        velocity = Velocity;
        clientFrame = ClientFrame;
    }
}
