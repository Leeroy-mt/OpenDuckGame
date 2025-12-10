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
        _keySprite.position = position;
        _keySprite.alpha = base.alpha;
        _keySprite.color = base.color;
        _keySprite.depth = base.depth;
        _keySprite.scale = base.scale;
        _keySprite.Draw();
        _font.scale = base.scale;
        _font.Draw(_keyString, position + new Vec2((float)_keySprite.width * base.scale.x / 2f - _font.GetWidth(_keyString) / 2f, 4f * base.scale.y), new Color(20, 32, 34), base.depth + 2);
    }
}
