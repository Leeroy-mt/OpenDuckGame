namespace DuckGame;

public class UIStringEntry : UIText
{
    private bool _directionalPassword;

    public UIStringEntry(bool directional, string textVal, Color c, UIAlign al = UIAlign.Center, float heightAdd = 0f, InputProfile controlProfile = null)
        : base(textVal, c, al, heightAdd, controlProfile)
    {
        _directionalPassword = directional;
    }

    public override void Draw()
    {
        if (_directionalPassword && _text != "  NONE")
        {
            _collisionSize.x = 48f;
            float textWidth = _text.Length * 8;
            float xOffset = 0f;
            xOffset = (((base.align & UIAlign.Left) > UIAlign.Center) ? (0f - base.width / 2f) : (((base.align & UIAlign.Right) <= UIAlign.Center) ? ((0f - textWidth) / 2f) : (base.width / 2f - textWidth)));
            xOffset -= 8f;
            float yOffset = 0f;
            yOffset = (((base.align & UIAlign.Top) > UIAlign.Center) ? (0f - base.height / 2f) : (((base.align & UIAlign.Bottom) <= UIAlign.Center) ? ((0f - _font.height) / 2f) : (base.height / 2f - _font.height)));
            Graphics.DrawPassword(_text, new Vec2(base.x + xOffset, base.y + yOffset), _color, base.depth);
            return;
        }
        if (_text.Length > 10)
        {
            _text = _text.Substring(0, 8) + "..";
        }
        _collisionSize.x = 48f;
        float textWidth2 = _text.Length * 8;
        float xOffset2 = 0f;
        xOffset2 = (((base.align & UIAlign.Left) > UIAlign.Center) ? (0f - base.width / 2f) : (((base.align & UIAlign.Right) <= UIAlign.Center) ? ((0f - textWidth2) / 2f) : (base.width / 2f - textWidth2)));
        xOffset2 -= 8f;
        float yOffset2 = 0f;
        yOffset2 = (((base.align & UIAlign.Top) > UIAlign.Center) ? (0f - base.height / 2f) : (((base.align & UIAlign.Bottom) <= UIAlign.Center) ? ((0f - _font.height) / 2f) : (base.height / 2f - _font.height)));
        Graphics.DrawString(_text, new Vec2(base.x + xOffset2, base.y + yOffset2), _color, base.depth);
    }
}
