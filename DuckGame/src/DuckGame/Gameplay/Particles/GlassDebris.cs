using Microsoft.Xna.Framework;

namespace DuckGame;

public class GlassDebris : PhysicsParticle
{
    private SpriteMap _sprite;

    public GlassDebris(bool rotate, float xpos, float ypos, float h, float v, int f, int tint = 0)
        : base(xpos, ypos)
    {
        hSpeed = h;
        vSpeed = v;
        _sprite = new SpriteMap("windowDebris", 8, 8);
        _sprite.frame = Rando.Int(7);
        _sprite.color = Window.windowColors[tint] * 0.6f;
        graphic = _sprite;
        Center = new Vector2(4f, 4f);
        _bounceEfficiency = 0.3f;
        if (rotate)
        {
            Angle -= 1.57f;
        }
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
}
