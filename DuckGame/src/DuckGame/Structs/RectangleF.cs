using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[Serializable]
public struct RectangleF
{
    #region Public Fields

    public float X, Y;

    public float Width, Height;

    #endregion

    #region Public Properties

    public float Left
    {
        get => X;
        set => X = value;
    }

    public float Right
    {
        get => X + Width;
        set => X = value - Width;
    }

    public float Top
    {
        get => Y;
        set => Y = value;
    }

    public float Bottom
    {
        get => Y + Height;
        set => Y = value - Height;
    }

    public float Aspect => Width / Height;

    public Vector2 LeftTop => new(X, Y);

    public Vector2 RightTop => new(X + Width, Y);

    public Vector2 LeftBottom => new(X, Y + Height);

    public Vector2 RightBottom => new(X + Width, Y + Height);

    public Vector2 Size => new(Width, Height);

    public Vector2 Center
    {
        get => new(X + Width / 2, Y + Height / 2);
        set
        {
            X = value.X - Width / 2;
            Y = value.Y - Height / 2;
        }
    }

    #endregion

    #region Constructors

    public RectangleF(
        float x,
        float y,
        float width,
        float height
        )
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public RectangleF(
        Vector2 tl,
        Vector2 br
        )
    {
        if (tl.X > br.X)
            (tl.X, br.X) = (br.X, tl.X);
        if (tl.Y > br.Y)
            (tl.Y, br.Y) = (br.Y, tl.Y);

        X = tl.X;
        Y = tl.Y;
        Width = br.X - tl.X;
        Height = br.Y - tl.Y;
    }

    #endregion

    #region Public Methods

    public bool Contains(Vector2 position)
    {
        if (position.X >= X && position.Y >= Y && position.X <= X + Width)
        {
            return position.Y <= Y + Height;
        }
        return false;
    }

    public RectangleF Shift(Vector2 offset)
    {
        return new(X + offset.X, Y + offset.Y, Width, Height);
    }

    public Vector4 ToVector4()
    {
        return new(X, Y, Width, Height);
    }

    #endregion

    #region Implicit Operators

    public static implicit operator Microsoft.Xna.Framework.Rectangle(RectangleF r)
    {
        return new Microsoft.Xna.Framework.Rectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
    }

    public static implicit operator RectangleF(Microsoft.Xna.Framework.Rectangle r)
    {
        return new RectangleF(r.X, r.Y, r.Width, r.Height);
    }

    #endregion
}
