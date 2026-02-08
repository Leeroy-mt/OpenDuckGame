using Microsoft.Xna.Framework;

namespace DuckGame;

public class WetEnterEffect : Thing
{
    private SpriteMap _sprite;

    public WetEnterEffect(float xpos, float ypos, Vector2 dir, Thing attach)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("wetEnter", 16, 16);
        _sprite.AddAnimation("splash", 0.45f, false, 0, 1);
        _sprite.SetAnimation("splash");
        Center = new Vector2(0f, 7f);
        graphic = _sprite;
        base.Depth = 0.7f;
        base.Alpha = 0.6f;
        Angle = Maths.DegToRad(0f - Maths.PointDirection(Vector2.Zero, dir));
        base.anchor = new Anchor(attach);
        base.anchor.offset = new Vector2(xpos, ypos) - attach.Position;
    }

    public override void Update()
    {
        if (_sprite.finished)
        {
            Level.Remove(this);
        }
    }

    public override void Draw()
    {
        base.Draw();
    }
}
