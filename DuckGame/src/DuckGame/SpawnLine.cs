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
		base.depth = 0.9f;
	}

	public override void Update()
	{
		base.alpha -= 0.03f;
		if (base.alpha < 0f)
		{
			Level.Remove(this);
		}
		base.x += _moveSpeed;
	}

	public override void Draw()
	{
		Graphics.DrawLine(position, position + new Vec2(0f, -1200f), _color * base.alpha, _thickness, 0.9f);
	}
}
