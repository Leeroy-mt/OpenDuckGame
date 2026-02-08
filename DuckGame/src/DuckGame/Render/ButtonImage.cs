using Microsoft.Xna.Framework;

namespace DuckGame;

public class ButtonImage : Sprite
{
    private BitmapFont _font;

    private Sprite _keySprite;

    private string _keyString;

    public ButtonImage(char key)
    {
        _font = new BitmapFont("tinyNumbers", 4, 5);
        _keySprite = new Sprite("buttons/genericButton");
        int num = key;
        _keyString = num.ToString() ?? "";
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
        _font.Draw(_keyString, Position + new Vector2((float)_keySprite.width * base.Scale.X / 2f - _font.GetWidth(_keyString) / 2f, 4f * base.Scale.Y), new Color(20, 32, 34), base.Depth + 2);
    }
}
