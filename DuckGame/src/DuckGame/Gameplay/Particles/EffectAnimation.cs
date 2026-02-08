using Microsoft.Xna.Framework;

namespace DuckGame;

public class EffectAnimation : Thing
{
    protected SpriteMap _sprite;

    public Color color = Color.White;

    public EffectAnimation(Vector2 pos, SpriteMap spr, float deep)
        : base(pos.X, pos.Y)
    {
        base.Depth = deep;
        _sprite = spr;
        _sprite.CenterOrigin();
        base.layer = Layer.Foreground;
    }

    public override void Update()
    {
        if (_sprite.finished)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void Draw()
    {
        _sprite.Scale = base.Scale;
        _sprite.Alpha = base.Alpha;
        _sprite.color = color;
        _sprite.Depth = base.Depth;
        _sprite.flipH = flipHorizontal;
        Graphics.Draw(_sprite, base.X, base.Y);
        base.Draw();
    }
}
