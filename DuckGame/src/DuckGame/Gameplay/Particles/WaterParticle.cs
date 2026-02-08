using Microsoft.Xna.Framework;

namespace DuckGame;

public class WaterParticle : Thing
{
    public WaterParticle(float xpos, float ypos, Vector2 hitAngle)
        : base(xpos, ypos)
    {
        hSpeed = (0f - hitAngle.X) * 2f * (Rando.Float(1f) + 0.3f);
    }

    public override void Update()
    {
        vSpeed += 0.1f;
        hSpeed *= 0.9f;
        base.X += hSpeed;
        base.Y += vSpeed;
        base.Alpha -= 0.06f;
        if (base.Alpha < 0f)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void Draw()
    {
        Graphics.DrawRect(Position, Position + new Vector2(1f, 1f), Color.LightBlue * base.Alpha, base.Depth);
    }
}
