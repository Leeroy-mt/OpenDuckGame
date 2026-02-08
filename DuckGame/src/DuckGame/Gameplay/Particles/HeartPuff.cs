using Microsoft.Xna.Framework;

namespace DuckGame;

public class HeartPuff : Thing
{
    private SpriteMap _sprite;

    public HeartPuff(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("heartpuff", 16, 16);
        _sprite.AddAnimation("wither", 0.35f, false, 0, 1, 2, 3, 4);
        _sprite.SetAnimation("wither");
        Center = new Vector2(5f, 16f);
        base.Alpha = 0.6f;
        base.Depth = 0.9f;
        graphic = _sprite;
        _sprite.color = Color.Green;
    }

    public override void Update()
    {
        if (base.anchor != null && base.anchor.thing != null)
        {
            flipHorizontal = base.anchor.thing.offDir < 0;
            if (flipHorizontal)
            {
                Center = new Vector2(10f, 16f);
            }
            else
            {
                Center = new Vector2(5f, 16f);
            }
            Angle = base.anchor.thing.Angle;
        }
        if (_sprite.finished)
        {
            Level.Remove(this);
        }
        base.Update();
    }
}
