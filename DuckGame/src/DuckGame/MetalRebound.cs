namespace DuckGame;

public class MetalRebound : Thing
{
	private static int kMaxObjects = 32;

	private static MetalRebound[] _objects = new MetalRebound[kMaxObjects];

	private static int _lastActiveObject = 0;

	private SpriteMap _sprite;

	public static MetalRebound New(float xpos, float ypos, int offDir)
	{
		MetalRebound obj = null;
		if (_objects[_lastActiveObject] == null)
		{
			obj = new MetalRebound();
			_objects[_lastActiveObject] = obj;
		}
		else
		{
			obj = _objects[_lastActiveObject];
		}
		_lastActiveObject = (_lastActiveObject + 1) % kMaxObjects;
		obj.Init(xpos, ypos, offDir);
		obj.ResetProperties();
		return obj;
	}

	public MetalRebound()
	{
		_sprite = new SpriteMap("metalRebound", 16, 16);
		graphic = _sprite;
	}

	private void Init(float xpos, float ypos, int offDir)
	{
		position.x = xpos;
		position.y = ypos;
		base.alpha = 1f;
		_sprite.frame = Rando.Int(3);
		_sprite.flipH = offDir < 0;
		center = new Vec2(16f, 8f);
	}

	public override void Update()
	{
		base.alpha -= 0.1f;
		if (base.alpha < 0f)
		{
			Level.Remove(this);
		}
	}
}
