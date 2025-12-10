using System;

namespace DuckGame;

public class UIText : UIComponent
{
    protected Color _color;

    public BitmapFont _font;

    protected string _text;

    protected Func<string> _textFunc;

    public int minLength;

    private float _heightAdd;

    private InputProfile _controlProfile;

    public float specialScale;

    public virtual string text
    {
        get
        {
            if (_textFunc != null)
            {
                text = _textFunc();
            }
            return _text;
        }
        set
        {
            _text = value;
            if (minLength > 0)
            {
                while (_text.Length < minLength)
                {
                    _text = " " + _text;
                }
            }
            _collisionSize = new Vec2(_font.GetWidth(_text), _font.height + _heightAdd);
        }
    }

    public float scaleVal
    {
        set
        {
            _font.scale = new Vec2(value);
        }
    }

    public void SetFont(BitmapFont f)
    {
        if (f != null)
        {
            _font = f;
        }
        _collisionSize = new Vec2(_font.GetWidth(text), _font.height + _heightAdd);
    }

    public UIText(string textVal, Color c, UIAlign al = UIAlign.Center, float heightAdd = 0f, InputProfile controlProfile = null)
        : base(0f, 0f, 0f, 0f)
    {
        _heightAdd = heightAdd;
        _font = new BitmapFont("biosFontUI", 8, 7);
        text = textVal;
        _color = c;
        base.align = al;
        _controlProfile = controlProfile;
    }

    public UIText(Func<string> textFunc, Color c, UIAlign al = UIAlign.Center, float heightAdd = 0f, InputProfile controlProfile = null)
        : base(0f, 0f, 0f, 0f)
    {
        _heightAdd = heightAdd;
        _font = new BitmapFont("biosFontUI", 8, 7);
        _textFunc = textFunc;
        text = _textFunc();
        _color = c;
        base.align = al;
        _controlProfile = controlProfile;
    }

    public override void Draw()
    {
        _font.scale = base.scale;
        _font.alpha = base.alpha;
        float textWidth = _font.GetWidth(text);
        float xOffset = 0f;
        xOffset = (((base.align & UIAlign.Left) > UIAlign.Center) ? (0f - base.width / 2f) : (((base.align & UIAlign.Right) <= UIAlign.Center) ? ((0f - textWidth) / 2f) : (base.width / 2f - textWidth)));
        float yOffset = 0f;
        yOffset = (((base.align & UIAlign.Top) > UIAlign.Center) ? (0f - base.height / 2f) : (((base.align & UIAlign.Bottom) <= UIAlign.Center) ? ((0f - _font.height) / 2f) : (base.height / 2f - _font.height)));
        if (specialScale != 0f)
        {
            Vec2 s = _font.scale;
            _font.scale = new Vec2(specialScale);
            _font.Draw(text, base.x + xOffset, base.y + yOffset, UIMenu.disabledDraw ? Colors.BlueGray : _color, base.depth, _controlProfile);
            _font.scale = s;
        }
        else
        {
            _font.Draw(text, base.x + xOffset, base.y + yOffset, UIMenu.disabledDraw ? Colors.BlueGray : _color, base.depth, _controlProfile);
        }
        base.Draw();
    }
}
