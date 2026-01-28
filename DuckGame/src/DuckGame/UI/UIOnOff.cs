namespace DuckGame;

public class UIOnOff(float wide, float high, FieldBinding field, FieldBinding filterBinding)
    : UIText("ON OFF", Color.White)
{
    #region Private Fields

    FieldBinding _field = field;

    FieldBinding _filterBinding = filterBinding;

    #endregion

    #region Public Methods

    public override void Draw()
    {
        _font.Scale = Scale;
        _font.Alpha = Alpha;
        string display = "ON OFF";
        float textWidth = _font.GetWidth(display);
        float xOffset = ((align & UIAlign.Left) > UIAlign.Center) ? (-width / 2) : (((align & UIAlign.Right) <= UIAlign.Center) ? (-textWidth / 2) : (width / 2 - textWidth));
        float yOffset = ((align & UIAlign.Top) > UIAlign.Center) ? (-height / 2) : (((align & UIAlign.Bottom) <= UIAlign.Center) ? (-_font.height / 2) : (height / 2 - _font.height));
        bool val = (bool)_field.value;
        if (_filterBinding != null)
        {
            if (!(bool)_filterBinding.value)
                _font.Draw("   ANY", X + xOffset, Y + yOffset, Color.White, Depth);
            else if (val)
                _font.Draw("    ON", X + xOffset, Y + yOffset, Color.White, Depth);
            else
                _font.Draw("   OFF", X + xOffset, Y + yOffset, Color.White, Depth);
        }
        else
        {
            _font.Draw("ON", X + xOffset, Y + yOffset, val ? Color.White : new Color(70, 70, 70), Depth);
            _font.Draw("   OFF", X + xOffset, Y + yOffset, !val ? Color.White : new Color(70, 70, 70), Depth);
        }
    }

    #endregion
}
