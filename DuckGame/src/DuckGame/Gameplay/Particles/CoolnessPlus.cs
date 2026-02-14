using Microsoft.Xna.Framework;

namespace DuckGame;

public class CoolnessPlus : Thing
{
    private BitmapFont _font;

    private Profile _profile;

    private float _wait = 1f;

    private int _change;

    public float wait
    {
        get
        {
            return _wait;
        }
        set
        {
            _wait = value;
        }
    }

    public int change
    {
        get
        {
            return _change;
        }
        set
        {
            _change = value;
            _wait = 1f;
        }
    }

    public CoolnessPlus(float xpos, float ypos, Duck d, int c)
        : base(xpos, ypos)
    {
        _font = new BitmapFont("biosFont", 8);
        change = c;
        base.anchor = d;
        base.anchor.offset = new Vector2(-0f, -24f);
        _profile = d.profile;
    }

    public override void Initialize()
    {
        SFX.Play("scoreDing", 0.8f);
    }

    public override void Update()
    {
        _wait -= 0.01f;
        if (_wait < 0f)
        {
            Level.Remove(this);
        }
    }

    public override void Draw()
    {
        Position = base.anchor.position;
        string text = "";
        if (change > 0)
        {
            text = "+";
        }
        text += change;
        float xposit = base.X - _font.GetWidth(text) / 2f;
        _font.Draw(text, xposit - 1f, base.Y - 1f, Color.Black, 0.8f);
        _font.Draw(text, xposit + 1f, base.Y - 1f, Color.Black, 0.8f);
        _font.Draw(text, xposit - 1f, base.Y + 1f, Color.Black, 0.8f);
        _font.Draw(text, xposit + 1f, base.Y + 1f, Color.Black, 0.8f);
        Color c = new Color((byte)_profile.persona.color.X, (byte)_profile.persona.color.Y, (byte)_profile.persona.color.Z);
        _font.Draw(text, xposit, base.Y, c, 0.9f);
    }
}
