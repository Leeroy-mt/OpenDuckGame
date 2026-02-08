using Microsoft.Xna.Framework;

namespace DuckGame;

public class LevelInfo : Card
{
    private string _name;

    private string _description;

    private Sprite _sprite;

    private Tex2D _image;

    protected bool _large;

    private const float largeCardWidth = 96f;

    private const float largeCardHeight = 62f;

    private const float smallCardWidth = 71f;

    private const float smallCardHeight = 48f;

    private static BitmapFont _font = new BitmapFont("biosFont", 8);

    public const float cardSpacing = 4f;

    public string name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
        }
    }

    public string description
    {
        get
        {
            return _description;
        }
        set
        {
            _description = value;
        }
    }

    public Tex2D image
    {
        get
        {
            return _image;
        }
        set
        {
            _image = value;
            _sprite = new Sprite(_image);
        }
    }

    public bool large
    {
        get
        {
            return _large;
        }
        set
        {
            _large = value;
        }
    }

    public override float width
    {
        get
        {
            if (!_large)
            {
                return 71f;
            }
            return 96f;
        }
    }

    public override float height
    {
        get
        {
            if (!_large)
            {
                return 48f;
            }
            return 62f;
        }
    }

    public LevelInfo(bool large = true, string text = null)
    {
        _large = large;
        _specialText = text;
    }

    public override void Draw(Vector2 position, bool selected, float alpha)
    {
        Graphics.DrawRect(position, position + new Vector2(width, height), new Color(25, 38, 41) * alpha, 0.9f);
        if (selected)
        {
            Graphics.DrawRect(position + new Vector2(-1f, 0f), position + new Vector2(width + 1f, height), Color.White * alpha, 0.97f, filled: false);
        }
        if (_specialText != null)
        {
            _font.Scale = new Vector2(0.5f, 0.5f);
            _font.Draw(_specialText, position.X + width / 2f - _font.GetWidth(_specialText) / 2f, position.Y + height / 2f - 3f, Color.White * alpha, 0.95f);
            return;
        }
        _font.Scale = new Vector2(0.5f, 0.5f);
        _font.Draw(_name, position.X + 3f, position.Y + height - 6f, Color.White * alpha, 0.95f);
        Sprite sprite = _sprite;
        float xscale = (_sprite.ScaleY = width / (float)_sprite.width);
        sprite.ScaleX = xscale;
        _sprite.Depth = 0.95f;
        _sprite.Alpha = alpha;
        Graphics.Draw(_sprite, position.X, position.Y);
    }
}
