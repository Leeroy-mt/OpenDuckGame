namespace DuckGame;

public class UIChangingText : UIText
{
    private FieldBinding _field;

    private FieldBinding _filterBinding;

    public string defaultSizeString = "ON OFF  ";

    public override string text
    {
        get
        {
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
        }
    }

    public UIChangingText(float wide, float high, FieldBinding field, FieldBinding filterBinding)
        : base("ON OFF  ", Color.White)
    {
        _field = field;
        _filterBinding = filterBinding;
    }

    public override void Draw()
    {
        _font.scale = base.scale;
        _font.alpha = base.alpha;
        float textWidth = _font.GetWidth(defaultSizeString);
        float xOffset = 0f;
        xOffset = (((base.align & UIAlign.Left) > UIAlign.Center) ? (0f - base.width / 2f) : (((base.align & UIAlign.Right) <= UIAlign.Center) ? ((0f - textWidth) / 2f) : (base.width / 2f - textWidth)));
        float yOffset = 0f;
        yOffset = (((base.align & UIAlign.Top) > UIAlign.Center) ? (0f - base.height / 2f) : (((base.align & UIAlign.Bottom) <= UIAlign.Center) ? ((0f - _font.height) / 2f) : (base.height / 2f - _font.height)));
        string t = text;
        while (t.Length < 8)
        {
            t = " " + t;
        }
        _font.colorOverride = (UIMenu.disabledDraw ? Colors.BlueGray : default(Color));
        _font.Draw(t, base.x + xOffset, base.y + yOffset, Color.White, base.depth);
    }
}
