using System.Collections.Generic;

namespace DuckGame;

public class UIMultiToggle : UIText
{
    private FieldBinding _field;

    private List<string> _captions;

    private bool _compressed;

    public void SetFieldBinding(FieldBinding f)
    {
        _field = f;
    }

    public UIMultiToggle(float wide, float high, FieldBinding field, List<string> captions, bool compressed = false)
        : base("AAAAAAAAA", Color.White)
    {
        _field = field;
        _captions = captions;
        _compressed = compressed;
    }

    public override void Draw()
    {
        _font.scale = base.scale;
        _font.alpha = base.alpha;
        int val = (int)_field.value;
        string drawText = "";
        if (_compressed && val < _captions.Count)
        {
            drawText = _captions[val];
        }
        else
        {
            int num = 0;
            foreach (string s in _captions)
            {
                if (num != 0)
                {
                    drawText += " ";
                }
                drawText = ((num != val) ? (drawText + "|GRAY|") : (drawText + "|WHITE|"));
                drawText += s;
                num++;
            }
        }
        Vec2 sssss = _font.scale;
        if (specialScale != 0f)
        {
            _font.scale = new Vec2(specialScale);
        }
        float textWidth = _font.GetWidth(drawText);
        float xOffset = 0f;
        xOffset = (((base.align & UIAlign.Left) > UIAlign.Center) ? (0f - base.width / 2f) : (((base.align & UIAlign.Right) <= UIAlign.Center) ? ((0f - textWidth) / 2f) : (base.width / 2f - textWidth)));
        float yOffset = 0f;
        yOffset = (((base.align & UIAlign.Top) > UIAlign.Center) ? (0f - base.height / 2f) : (((base.align & UIAlign.Bottom) <= UIAlign.Center) ? ((0f - _font.height) / 2f) : (base.height / 2f - _font.height)));
        _font.Draw(drawText, base.x + xOffset, base.y + yOffset, Color.White, base.depth);
        _font.scale = sssss;
    }
}
