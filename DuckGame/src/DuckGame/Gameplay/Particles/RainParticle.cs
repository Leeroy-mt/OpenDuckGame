using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class RainParticle : WeatherParticle
{
    public static SpriteMap splash;

    private Vector2 _prevPos;

    private float _frame;

    public RainParticle(Vector2 pos)
        : base(pos)
    {
        velocity = new Vector2(Rando.Float(-1.2f, -1.4f), Rando.Float(3.3f, 4f));
        zSpeed = Rando.Float(-0.1f, 0.1f);
    }

    public override void Draw()
    {
        if (die)
        {
            int frame = (int)_frame;
            if (frame > 3)
            {
                frame = 3;
            }
            splash.frame = frame;
            _frame += 0.3f;
            if (_frame >= 3.9f)
            {
                alpha = 0f;
            }
            Vector2 pos = position;
            Vec3 newPos = new Vec3(pos.X, z, pos.Y);
            newPos = new Viewport(0, 0, (int)Layer.HUD.width, (int)Layer.HUD.height).Project(newPos, Layer.Game.projection, Layer.Game.view, Matrix.Identity);
            position = new Vector2(newPos.x, newPos.y);
            float znorm = z / 200f;
            splash.Depth = -0.02f + znorm * 0.1f;
            splash.color = Color.White * 0.8f;
            Graphics.Draw(splash, position.X - 6f, position.Y - 6f);
            position = pos;
        }
        else
        {
            Vector2 pos2 = position;
            Vec3 newPos2 = new Vec3(pos2.X, z, pos2.Y);
            newPos2 = new Viewport(0, 0, (int)Layer.HUD.width, (int)Layer.HUD.height).Project(newPos2, Layer.Game.projection, Layer.Game.view, Matrix.Identity);
            position = new Vector2(newPos2.x, newPos2.y);
            float znorm2 = z / 200f;
            Graphics.DrawLine(position, _prevPos, Color.White * 0.8f, 1f, -0.02f + znorm2 * 0.1f);
            _prevPos = position;
            position = pos2;
        }
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
            alpha -= 0.01f;
        }
    }
}
