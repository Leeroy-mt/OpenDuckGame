using Microsoft.Xna.Framework;

namespace DuckGame;

public class UIStringEntry(bool directional, string textVal, Color c, UIAlign al = UIAlign.Center, float heightAdd = 0, InputProfile controlProfile = null)
    : UIText(textVal, c, al, heightAdd, controlProfile)
{
    #region Private Fields

    bool _directionalPassword = directional;

    #endregion

    #region Public Methods

    public override void Draw()
    {
        if (_directionalPassword && _text != "  NONE")
        {
            _collisionSize.X = 48;
            float textWidth = _text.Length * 8;
            float xOffset = (((align & UIAlign.Left) > UIAlign.Center) ? (-width / 2) : (((align & UIAlign.Right) <= UIAlign.Center) ? ((-textWidth) / 2) : (width / 2 - textWidth)));
            xOffset -= 8;
            float yOffset = (((align & UIAlign.Top) > UIAlign.Center) ? (-height / 2) : (((align & UIAlign.Bottom) <= UIAlign.Center) ? ((-_font.height) / 2) : (height / 2 - _font.height)));
            Graphics.DrawPassword(_text, new Vector2(X + xOffset, Y + yOffset), _color, Depth);
            return;
        }
        if (_text.Length > 10)
            _text = $"{_text[..8]}..";
        _collisionSize.X = 48;
        float textWidth2 = _text.Length * 8;
        float xOffset2 = ((align & UIAlign.Left) > UIAlign.Center) ? (-width / 2) : (((align & UIAlign.Right) <= UIAlign.Center) ? (-textWidth2 / 2) : (width / 2 - textWidth2));
        xOffset2 -= 8;
        float yOffset2 = ((align & UIAlign.Top) > UIAlign.Center) ? (-height / 2) : (((align & UIAlign.Bottom) <= UIAlign.Center) ? (-_font.height / 2) : (height / 2 - _font.height));
        Graphics.DrawString(_text, new Vector2(X + xOffset2, Y + yOffset2), _color, Depth);
    }

    #endregion
}
