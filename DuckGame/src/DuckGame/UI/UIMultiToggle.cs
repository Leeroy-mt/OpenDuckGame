using System.Collections.Generic;

namespace DuckGame;

public class UIMultiToggle(float wide, float high, FieldBinding field, List<string> captions, bool compressed = false) 
    : UIText("AAAAAAAAA", Color.White)
{
    #region Private Fields

    bool _compressed = compressed;

    FieldBinding _field = field;

    List<string> _captions = captions;

    #endregion

    #region Public Methods

    public override void Draw()
    {
        _font.Scale = Scale;
        _font.Alpha = Alpha;
        int val = (int)_field.value;
        string drawText = "";
        if (_compressed && val < _captions.Count)
            drawText = _captions[val];
        else
        {
            int num = 0;
            foreach (string s in _captions)
            {
                if (num != 0)
                    drawText += " ";
                drawText = num != val ? $"{drawText}|GRAY|" : $"{drawText}|WHITE|";
                drawText += s;
                num++;
            }
        }
        Vec2 scale = _font.Scale;
        if (specialScale != 0)
            _font.Scale = new Vec2(specialScale);
        float textWidth = _font.GetWidth(drawText);
        float xOffset = ((align & UIAlign.Left) > UIAlign.Center) ? -width / 2 : (((align & UIAlign.Right) <= UIAlign.Center) ? -textWidth / 2 : (width / 2 - textWidth));
        float yOffset = ((align & UIAlign.Top) > UIAlign.Center) ? -height / 2 : (((align & UIAlign.Bottom) <= UIAlign.Center) ? -_font.height / 2 : (height / 2 - _font.height));
        _font.Draw(drawText, X + xOffset, Y + yOffset, Color.White, Depth);
        _font.Scale = scale;
    }

    public void SetFieldBinding(FieldBinding f)
    {
        _field = f;
    }

    #endregion
}
