using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class UIText : UIComponent
{
    #region Public Fields

    public int minLength;

    public float specialScale;

    public BitmapFont _font;

    #endregion

    #region Protected Fields

    protected string _text;

    protected Color _color;

    protected Func<string> _textFunc;

    #endregion

    #region Private Fields

    float _heightAdd;

    InputProfile _controlProfile;

    #endregion

    #region Public Properties

    public virtual string text
    {
        get
        {
            if (_textFunc != null)
                text = _textFunc();
            return _text;
        }
        set
        {
            _text = value;
            if (minLength > 0)
                while (_text.Length < minLength)
                    _text = " " + _text;
            _collisionSize = new Vector2(_font.GetWidth(_text), _font.height + _heightAdd);
        }
    }

    public float scaleVal
    {
        set => _font.Scale = new Vector2(value);
    }

    #endregion

    #region Public Constructors

    public UIText(string textVal, Color c, UIAlign al = UIAlign.Center, float heightAdd = 0, InputProfile controlProfile = null)
        : base(0, 0, 0, 0)
    {
        _heightAdd = heightAdd;
        _font = new BitmapFont("biosFontUI", 8, 7);
        text = textVal;
        _color = c;
        align = al;
        _controlProfile = controlProfile;
    }

    public UIText(Func<string> textFunc, Color c, UIAlign al = UIAlign.Center, float heightAdd = 0, InputProfile controlProfile = null)
        : base(0, 0, 0, 0)
    {
        _heightAdd = heightAdd;
        _font = new BitmapFont("biosFontUI", 8, 7);
        _textFunc = textFunc;
        text = _textFunc();
        _color = c;
        align = al;
        _controlProfile = controlProfile;
    }

    #endregion

    #region Public Methods

    public override void Draw()
    {
        _font.Scale = Scale;
        _font.Alpha = Alpha;
        float textWidth = _font.GetWidth(text);
        float xOffset = ((align & UIAlign.Left) > UIAlign.Center) ? (-width / 2) : (((align & UIAlign.Right) <= UIAlign.Center) ? (-textWidth / 2) : (width / 2 - textWidth));
        float yOffset = ((align & UIAlign.Top) > UIAlign.Center) ? (-height / 2) : (((align & UIAlign.Bottom) <= UIAlign.Center) ? (-_font.height / 2) : (height / 2 - _font.height));
        if (specialScale != 0)
        {
            Vector2 s = _font.Scale;
            _font.Scale = new Vector2(specialScale);
            _font.Draw(text, X + xOffset, Y + yOffset, UIMenu.disabledDraw ? Colors.BlueGray : _color, Depth, _controlProfile);
            _font.Scale = s;
        }
        else
            _font.Draw(text, X + xOffset, Y + yOffset, UIMenu.disabledDraw ? Colors.BlueGray : _color, Depth, _controlProfile);
        base.Draw();
    }

    public void SetFont(BitmapFont f)
    {
        if (f != null)
            _font = f;
        _collisionSize = new Vector2(_font.GetWidth(text), _font.height + _heightAdd);
    }

    #endregion
}
