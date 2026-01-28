namespace DuckGame;

public class GlassParticle : PhysicsParticle
{
    private int _tint;

    public GlassParticle(float xpos, float ypos, Vec2 hitAngle, int tint = -1)
        : base(xpos, ypos)
    {
        hSpeed = (0f - hitAngle.X) * 2f * (Rando.Float(1f) + 0.3f);
        vSpeed = (0f - hitAngle.Y) * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(2f);
        _bounceEfficiency = 0.6f;
        _tint = tint;
    }

    public override void Update()
    {
        base.Alpha -= 0.01f;
        if (base.Alpha < 0f)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void Draw()
    {
        Graphics.DrawRect(Position, Position + new Vec2(1f, 1f), ((_tint > 0) ? Window.windowColors[_tint] : Color.LightBlue) * base.Alpha, base.Depth);
    }
}
