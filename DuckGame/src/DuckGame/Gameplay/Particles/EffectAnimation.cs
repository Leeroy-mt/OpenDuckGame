namespace DuckGame;

public class EffectAnimation : Thing
{
    protected SpriteMap _sprite;

    public Color color = Color.White;

    public EffectAnimation(Vec2 pos, SpriteMap spr, float deep)
        : base(pos.x, pos.y)
    {
        base.depth = deep;
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
        _sprite.scale = base.scale;
        _sprite.alpha = base.alpha;
        _sprite.color = color;
        _sprite.depth = base.depth;
        _sprite.flipH = flipHorizontal;
        Graphics.Draw(_sprite, base.x, base.y);
        base.Draw();
    }
}
