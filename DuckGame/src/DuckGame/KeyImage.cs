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
		_keySprite.position = position;
		_keySprite.alpha = base.alpha;
		_keySprite.color = base.color;
		_keySprite.depth = base.depth;
		_keySprite.scale = base.scale;
		_keySprite.Draw();
		_font.scale = base.scale;
		_font.Draw(_keyString, position + new Vec2((float)_keySprite.width * _keySprite.scale.x / 2f - _font.GetWidth(_keyString) / 2f - 1f, 2f * _keySprite.scale.y), new Color(20, 32, 34), base.depth + 2);
	}
}
