using Microsoft.Xna.Framework;

namespace DuckGame;

public class NMDuckPhysicsState : NMPhysicsState
{
    public int inputState;

    public ushort holding;

    public NMDuckPhysicsState()
    {
    }

    public NMDuckPhysicsState(Vector2 Position, Vector2 Velocity, ushort ObjectID, int ClientFrame, int InputState, ushort Holding)
        : base(Position, Velocity, ObjectID, ClientFrame)
    {
        inputState = InputState;
        holding = Holding;
    }
}
