using Microsoft.Xna.Framework;

namespace DuckGame;

public class Card
{
    protected string _specialText;

    private static BitmapFont _font = new BitmapFont("biosFont", 8);

    public string specialText
    {
        get
        {
            return _specialText;
        }
        set
        {
            _specialText = value;
        }
    }

    public virtual float width => 71f;

    public virtual float height => 12f;

    public Card(string text)
    {
        _specialText = text;
    }

    public Card()
    {
    }

    public virtual void Draw(Vector2 position, bool selected, float alpha)
    {
        Graphics.DrawRect(position, position + new Vector2(width, height), new Color(25, 38, 41) * alpha, 0.9f);
        if (selected)
        {
            Graphics.DrawRect(position + new Vector2(-1f, 0f), position + new Vector2(width + 1f, height), Color.White * alpha, 0.97f, filled: false);
        }
        _font.Scale = new Vector2(0.5f, 0.5f);
        _font.Draw(_specialText, position.X + 4f, position.Y + 4f, Color.White * alpha, 0.98f);
    }
}
