using Microsoft.Xna.Framework;

namespace DuckGame;

public class PhysicsRopeSection : PhysicsObject
{
    public Vector2 tempPos;

    public Vector2 calcPos;

    public new Vector2 velocity;

    public Vector2 accel;

    public PhysicsRope rope;

    public PhysicsRopeSection(float xpos, float ypos, PhysicsRope r)
        : base(xpos, ypos)
    {
        tempPos = Position;
        collisionSize = new Vector2(4f, 4f);
        collisionOffset = new Vector2(-2f, -2f);
        weight = 0.1f;
        updatePhysics = false;
        rope = r;
    }
}
