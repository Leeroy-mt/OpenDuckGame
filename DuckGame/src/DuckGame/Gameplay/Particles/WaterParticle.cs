namespace DuckGame;

public class WaterParticle : Thing
{
    public WaterParticle(float xpos, float ypos, Vec2 hitAngle)
        : base(xpos, ypos)
    {
        hSpeed = (0f - hitAngle.x) * 2f * (Rando.Float(1f) + 0.3f);
    }

    public override void Update()
    {
        vSpeed += 0.1f;
        hSpeed *= 0.9f;
        base.x += hSpeed;
        base.y += vSpeed;
        base.alpha -= 0.06f;
        if (base.alpha < 0f)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void Draw()
    {
        Graphics.DrawRect(position, position + new Vec2(1f, 1f), Color.LightBlue * base.alpha, base.depth);
    }
}
