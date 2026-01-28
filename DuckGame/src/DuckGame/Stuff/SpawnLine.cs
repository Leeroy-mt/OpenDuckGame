namespace DuckGame;

public class SpawnLine : Thing
{
    private float _moveSpeed;

    private float _thickness;

    private Color _color;

    public SpawnLine(float xpos, float ypos, int dir, float moveSpeed, Color color, float thickness)
        : base(xpos, ypos)
    {
        _moveSpeed = moveSpeed;
        _color = color;
        _thickness = thickness;
        offDir = (sbyte)dir;
        base.layer = Layer.Foreground;
        base.Depth = 0.9f;
    }

    public override void Update()
    {
        base.Alpha -= 0.03f;
        if (base.Alpha < 0f)
        {
            Level.Remove(this);
        }
        base.X += _moveSpeed;
    }

    public override void Draw()
    {
        Graphics.DrawLine(Position, Position + new Vec2(0f, -1200f), _color * base.Alpha, _thickness, 0.9f);
    }
}
