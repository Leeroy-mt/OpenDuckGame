using System;

namespace DuckGame;

public class UIProgressBar : UIComponent
{
    #region Private Fields

    float _step;

    Vec2 _barSize;

    FieldBinding _field;

    #endregion

    #region Public Constructors

    public UIProgressBar(float wide, float high, FieldBinding field, float increment, Color c = default)
        : base(0, 0, 0, 0)
    {
        _field = field;
        _barSize = new Vec2(wide, high);
        _collisionSize = _barSize;
        _step = increment;
    }

    #endregion

    #region Public Methods

    public override void Draw()
    {
        float sizeX = _barSize.X * Scale.X;
        float sizeY = _barSize.Y * Scale.Y;
        int numParts = (int)Math.Ceiling((_field.max - _field.min) / _step);
        for (int i = 0; i < numParts; i++)
        {
            Vec2 tl = Position - new Vec2(halfWidth, sizeY / 2) + new Vec2(i * (int)Math.Round(sizeX / numParts), 0);
            Vec2 br = Position - new Vec2(halfWidth, -sizeY / 2) + new Vec2(((i + 1) * (int)Math.Round(sizeX / numParts)) - 1, 0);
            if ((align & UIAlign.Center) > UIAlign.Center)
            {
                tl.X += halfWidth - sizeX / 2;
                br.X += halfWidth - sizeX / 2;
            }
            else if ((align & UIAlign.Right) > UIAlign.Center)
            {
                tl.X += width - sizeX;
                br.X += width - sizeX;
            }
            if (tl.X == br.X)
                br.X += 1;
            float value = (float)_field.value;
            Graphics.DrawRect(tl, br, (value > i * _step) ? Color.White : new Color(70, 70, 70), Depth);
        }
    }

    #endregion
}
