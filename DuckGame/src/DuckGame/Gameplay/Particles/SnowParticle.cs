using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class SnowParticle : WeatherParticle
{
    public SnowParticle(Vec2 pos)
        : base(pos)
    {
        velocity = new Vec2(Rando.Float(-0.5f, 0.5f), Rando.Float(0.5f, 1f));
        zSpeed = Rando.Float(-0.1f, 0.1f);
    }

    public override void Draw()
    {
        Vec2 pos = position;
        Vec3 newPos = new Vec3(pos.X, z, pos.Y);
        newPos = new Viewport(0, 0, (int)Layer.HUD.width, (int)Layer.HUD.height).Project(newPos, Layer.Game.projection, Layer.Game.view, Matrix.Identity);
        position = new Vec2(newPos.x, newPos.y);
        float znorm = z / 200f;
        float size = 0.3f + znorm * 0.3f;
        Graphics.DrawRect(position + new Vec2(0f - size, 0f - size), position + new Vec2(size, size), Color.White * alpha, -0.02f + znorm * 0.1f);
        position = pos;
    }

    public override void Update()
    {
        if (!die)
        {
            position += velocity;
            z += zSpeed;
        }
        else
        {
            alpha -= 0.04f;
        }
    }
}
