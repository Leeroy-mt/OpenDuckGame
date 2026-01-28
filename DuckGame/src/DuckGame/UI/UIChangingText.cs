namespace DuckGame;

public class UIChangingText : UIText
{
    #region Public Fields

    public string defaultSizeString = "ON OFF  ";

    #endregion

    #region Private Fields

    FieldBinding _field;

    FieldBinding _filterBinding;

    #endregion

    #region Public Properties

    public override string text
    {
        get => _text;
        set
        {
            _text = value;
            if (minLength > 0)
                while (_text.Length < minLength)
                    _text = " " + _text;
        }
    }

    #endregion

    #region Public Constructors

    public UIChangingText(float wide, float high, FieldBinding field, FieldBinding filterBinding)
        : base("ON OFF", Color.White)
    {
        _field = field;
        _filterBinding = filterBinding;
    }

    #endregion

    #region Public Methods

    public override void Draw()
    {
        _font.Scale = Scale;
        _font.Alpha = Alpha;
        float textWidth = _font.GetWidth(defaultSizeString);
        float xOffset = ((align & UIAlign.Left) > UIAlign.Center) ? (-width / 2) : (((align & UIAlign.Right) <= UIAlign.Center) ? (-textWidth / 2) : (width / 2 - textWidth));
        float yOffset = ((align & UIAlign.Top) > UIAlign.Center) ? (-height / 2) : (((align & UIAlign.Bottom) <= UIAlign.Center) ? (-_font.height / 2) : (height / 2 - _font.height));
        string t = text;
        while (t.Length < 8)
            t = " " + t;
        _font.colorOverride = UIMenu.disabledDraw ? Colors.BlueGray : default;
        _font.Draw(t, X + xOffset, Y + yOffset, Color.White, Depth);
    }

    #endregion
}
