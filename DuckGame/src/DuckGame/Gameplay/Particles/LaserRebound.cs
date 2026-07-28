using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class LaserRebound : Thing
{
    Texture2D _rebound = Content.Load<Texture2D>("laserRebound");

    public LaserRebound(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite(_rebound);
        base.Depth = 0.9f;
        Center = new Vector2(4f, 4f);
    }

    public override void Update()
    {
        base.Alpha -= 0.07f;
        if (base.Alpha <= 0f)
        {
            Level.Remove(this);
        }
    }
}
