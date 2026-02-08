using Microsoft.Xna.Framework;

namespace DuckGame;

public class GhostPack : Jetpack
{
    public GhostPack(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("jetpack", 16, 16);
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-5f, -5f);
        collisionSize = new Vector2(11f, 12f);
        _offset = new Vector2(-3f, 3f);
        thickness = 0.1f;
    }

    public override void Draw()
    {
        _heat = 0.01f;
        if (_equippedDuck != null)
        {
            base.Depth = -0.5f;
            Vector2 off = _offset;
            if (base.duck.offDir < 0)
            {
                off.X *= -1f;
            }
            Position = base.duck.Position + off;
        }
        else
        {
            base.Depth = 0f;
        }
    }
}
