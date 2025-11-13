namespace DuckGame;

public class Feather : Thing
{
	private static int kMaxObjects = 64;

	private static Feather[] _objects = new Feather[kMaxObjects];

	private static int _lastActiveObject = 0;

	private SpriteMap _sprite;

	private bool _rested;

	public static Feather New(float xpos, float ypos, DuckPersona who)
	{
		if (who == null)
		{
			who = Persona.Duck1;
		}
		Feather obj = null;
		if (NetworkDebugger.enabled)
		{
			obj = new Feather();
		}
		else if (_objects[_lastActiveObject] == null)
		{
			obj = new Feather();
			_objects[_lastActiveObject] = obj;
		}
		else
		{
			obj = _objects[_lastActiveObject];
		}
		Level.Remove(obj);
		_lastActiveObject = (_lastActiveObject + 1) % kMaxObjects;
		obj.Init(xpos, ypos, who);
		obj.ResetProperties();
		obj._sprite.globalIndex = Thing.GetGlobalIndex();
		obj.globalIndex = Thing.GetGlobalIndex();
		return obj;
	}

	private Feather()
	{
		_sprite = new SpriteMap("feather", 12, 4);
		_sprite.speed = 0.3f;
		_sprite.AddAnimation("feather", 1f, true, 0, 1, 2, 3);
		graphic = _sprite;
		center = new Vec2(6f, 1f);
	}

	private void Init(float xpos, float ypos, DuckPersona who)
	{
		position.x = xpos;
		position.y = ypos;
		base.alpha = 1f;
		hSpeed = -3f + Rando.Float(6f);
		vSpeed = -1f + (-1f + Rando.Float(2f));
		_sprite = who.featherSprite.CloneMap();
		_sprite.SetAnimation("feather");
		_sprite.frame = Rando.Int(3);
		if (Rando.Double() > 0.5)
		{
			_sprite.flipH = true;
		}
		else
		{
			_sprite.flipH = false;
		}
		graphic = _sprite;
		_rested = false;
	}

	public override void Update()
	{
		if (_rested)
		{
			return;
		}
		if (hSpeed > 0f)
		{
			hSpeed -= 0.1f;
		}
		if (hSpeed < 0f)
		{
			hSpeed += 0.1f;
		}
		if ((double)hSpeed < 0.1 && hSpeed > -0.1f)
		{
			hSpeed = 0f;
		}
		if (vSpeed < 1f)
		{
			vSpeed += 0.06f;
		}
		if (vSpeed < 0f)
		{
			_sprite.speed = 0f;
			if (Level.CheckPoint<Block>(base.x, base.y - 7f) != null)
			{
				vSpeed = 0f;
			}
		}
		else if (Level.CheckPoint<IPlatform>(base.x, base.y + 3f) is Thing col)
		{
			vSpeed = 0f;
			_sprite.speed = 0f;
			if (col is Block)
			{
				_rested = true;
			}
		}
		else
		{
			_sprite.speed = 0.3f;
		}
		base.x += hSpeed;
		base.y += vSpeed;
	}
}
