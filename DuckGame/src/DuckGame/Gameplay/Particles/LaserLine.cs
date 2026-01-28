namespace DuckGame;

public class LaserLine : Thing
{
    private float _moveSpeed;

    private float _thickness;

    private Color _color;

    private Vec2 _move;

    private Vec2 _target;

    private float fade = 0.06f;

    public LaserLine(Vec2 pos, Vec2 target, Vec2 moveVector, float moveSpeed, Color color, float thickness, float f = 0.06f)
        : base(pos.X, pos.Y)
    {
        _moveSpeed = moveSpeed;
        _color = color;
        _thickness = thickness;
        _move = moveVector;
        _target = target;
        fade = f;
    }

    public override void Update()
    {
        base.Alpha -= fade;
        if (base.Alpha < 0f)
        {
            Level.Remove(this);
        }
        base.X += _move.X * _moveSpeed;
        base.Y += _move.Y * _moveSpeed;
    }

    public override void Draw()
    {
        Graphics.DrawLine(Position, Position + _target, _color * base.Alpha, _thickness, 0.9f);
    }
}
