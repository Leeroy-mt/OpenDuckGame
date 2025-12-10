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
        : base(pos.x, pos.y)
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
        base.alpha -= fade;
        if (base.alpha < 0f)
        {
            Level.Remove(this);
        }
        base.x += _move.x * _moveSpeed;
        base.y += _move.y * _moveSpeed;
    }

    public override void Draw()
    {
        Graphics.DrawLine(position, position + _target, _color * base.alpha, _thickness, 0.9f);
    }
}
