namespace DuckGame;

public class CorkObject : PhysicsObject, ISwing, IPullBack
{
	private Thing _gun;

	public Sprite _ropeSprite;

	private Rope _rope;

	private Harpoon _sticker;

	public CorkObject(float pX, float pY, Thing pOwner)
		: base(pX, pY)
	{
		graphic = new Sprite("cork");
		_collisionSize = new Vec2(4f, 4f);
		_collisionOffset = new Vec2(-2f, -3f);
		center = new Vec2(3f, 3f);
		_gun = pOwner;
		weight = 0.1f;
		base.bouncy = 0.5f;
		airFrictionMult = 0f;
		_ropeSprite = new Sprite("grappleWire");
		_ropeSprite.center = new Vec2(8f, 0f);
	}

	public Rope GetRopeParent(Thing child)
	{
		for (Rope t = _rope; t != null; t = t.attach2 as Rope)
		{
			if (t.attach2 == child)
			{
				return t;
			}
		}
		return null;
	}

	public override void Initialize()
	{
		if (_gun != null)
		{
			_sticker = new Harpoon(this);
			base.level.AddThing(_sticker);
			_sticker.SetStuckPoint(_gun.position);
			_rope = new Rope(base.x, base.y, null, _sticker, this, vine: false, _ropeSprite, this);
			Level.Add(_rope);
		}
		base.Initialize();
	}

	public override void Terminate()
	{
		if (_sticker != null)
		{
			Level.Remove(_sticker);
		}
		if (_rope != null)
		{
			_rope.RemoveRope();
		}
		base.Terminate();
	}

	public float WindUp(float pAmount)
	{
		if (pAmount > 0f && _rope.startLength > 0f)
		{
			_rope.Pull(0f - pAmount);
			_rope.startLength -= pAmount;
			return _rope.startLength;
		}
		return 100f;
	}

	public override void Update()
	{
		if (_rope != null)
		{
			if (!base.grounded)
			{
				specialFrictionMod = 0f;
			}
			else
			{
				specialFrictionMod = 1f;
			}
			_rope.position = position;
			_rope.SetServer(base.isServerForObject);
			Vec2 travel = _rope.attach1.position - _rope.attach2.position;
			bool physics = true;
			if (_rope.properLength < 0f)
			{
				Rope rope = _rope;
				float startLength = (_rope.properLength = 100f);
				rope.startLength = startLength;
				physics = false;
			}
			if (travel.length > _rope.properLength)
			{
				travel = travel.normalized;
				_ = position;
				Vec2 start = position;
				Vec2 p2 = _rope.attach2.position + travel * _rope.properLength;
				Level.CheckRay<Block>(start, p2, out var _);
				if (physics)
				{
					hSpeed = p2.x - position.x;
					vSpeed = p2.y - position.y;
					gravMultiplier = 0f;
					float prevSpec = specialFrictionMod;
					specialFrictionMod = 0f;
					airFrictionMult = 0f;
					Vec2 lastPos = base.lastPosition;
					UpdatePhysics();
					gravMultiplier = 1f;
					specialFrictionMod = prevSpec;
					Vec2 dif = p2 - lastPos;
					if (dif.length > 32f)
					{
						position = p2;
					}
					else if (dif.length > 6f)
					{
						hSpeed = Rando.Float(-2f, 2f);
						vSpeed = Rando.Float(-2f, 2f);
					}
					else
					{
						hSpeed = dif.x;
						vSpeed = dif.y;
					}
				}
				else
				{
					position = p2;
				}
			}
			_sticker.SetStuckPoint((_gun as Gun).barrelPosition);
		}
		base.Update();
	}
}
