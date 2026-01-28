namespace DuckGame;

public class KeyImage : Sprite
{
    private FancyBitmapFont _font;

    private Sprite _keySprite;

    private string _keyString;

    public KeyImage(char key)
    {
        _font = new FancyBitmapFont("smallFont");
        _keySprite = new Sprite("buttons/keyboard/key");
        _keyString = key.ToString() ?? "";
        _texture = _keySprite.texture;
    }

    public override void Draw()
    {
        _keySprite.Position = Position;
        _keySprite.Alpha = base.Alpha;
        _keySprite.color = base.color;
        _keySprite.Depth = base.Depth;
        _keySprite.Scale = base.Scale;
        _keySprite.Draw();
        _font.Scale = base.Scale;
        _font.Draw(_keyString, Position + new Vec2((float)_keySprite.width * _keySprite.Scale.X / 2f - _font.GetWidth(_keyString) / 2f - 1f, 2f * _keySprite.Scale.Y), new Color(20, 32, 34), base.Depth + 2);
    }
}
