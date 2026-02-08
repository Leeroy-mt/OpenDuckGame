using Microsoft.Xna.Framework;

namespace DuckGame;

/// <summary>
/// Represents a transformable component.
/// </summary>
public abstract class Transform
{
    #region Public Fields
    public float AngleValue;

    public Vector2 CenterValue;
    #endregion

    #region Public Properties
    public float X
    {
        get => Position.X;
        set => Position = new(value, Position.Y);
    }

    public float Y
    {
        get => Position.Y;
        set => Position = new(Position.X, value);
    }

    public float Z { get; set; }

    public float CenterX
    {
        get => CenterValue.X;
        set => CenterValue.X = value;
    }

    public float CenterY
    {
        get => CenterValue.Y;
        set => CenterValue.Y = value;
    }

    public float ScaleX
    {
        get => Scale.X;
        set => Scale = new(value, Scale.Y);
    }

    public float ScaleY
    {
        get => Scale.Y;
        set => Scale = new(Scale.X, value);
    }

    public virtual float Angle
    {
        get => AngleValue;
        set => AngleValue = value;
    }

    public float AngleDegrees
    {
        get => Maths.RadToDeg(Angle);
        set => Angle = Maths.DegToRad(value);
    }

    public float Alpha { get; set; } = 1;

    public Depth Depth { get; set; } = new(0);

    public Vector2 Position { get; set; }

    public virtual Vector2 Center
    {
        get => CenterValue;
        set => CenterValue = value;
    }

    public Vector2 Scale { get; set; } = new(1);
    #endregion
}