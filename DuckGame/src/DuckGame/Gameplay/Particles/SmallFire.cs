namespace DuckGame;

public class SmallFire : PhysicsParticle, ITeleport
{
	public static int kMaxObjects = 256;

	public float waitToHurt;

	public Duck whoWait;

	private SpriteMap _sprite;

	private SpriteMap _airFire;

	private float _airFireScale;

	private float _spinSpeed;

	private bool _multiplied;

	private byte _groundLife = 125;

	private Vec2 _stickOffset;

	private MaterialThing _stick;

	private Thing _firedFrom;

	private static bool kAlternate = false;

	private bool _alternate;

	private bool _alternateb;

	public bool doFloat;

	private int _fireID;

	private bool _canMultiply = true;

	private bool didRemove;

	public byte groundLife
	{
		get
		{
			return _groundLife;
		}
		set
		{
			_groundLife = value;
		}
	}

	public Vec2 stickOffset
	{
		get
		{
			return _stickOffset;
		}
		set
		{
			_stickOffset = value;
		}
	}

	public MaterialThing stick
	{
		get
		{
			return _stick;
		}
		set
		{
			_stick = value;
		}
	}

	public Thing firedFrom => _firedFrom;

	public int fireID => _fireID;

	public static SmallFire New(float xpos, float ypos, float hspeed, float vspeed, bool shortLife = false, MaterialThing stick = null, bool canMultiply = true, Thing firedFrom = null, bool network = false)
	{
		SmallFire obj = null;
		if (Network.isActive)
		{
			obj = new SmallFire();
		}
		else if (Level.core.firePool[Level.core.firePoolIndex] == null)
		{
			obj = new SmallFire();
			Level.core.firePool[Level.core.firePoolIndex] = obj;
		}
		else
		{
			obj = Level.core.firePool[Level.core.firePoolIndex];
		}
		Level.core.firePoolIndex = (Level.core.firePoolIndex + 1) % kMaxObjects;
		if (obj != null)
		{
			obj.ResetProperties();
			obj.Init(xpos, ypos, hspeed, vspeed, shortLife, stick, canMultiply);
			obj._sprite.globalIndex = Thing.GetGlobalIndex();
			obj._airFire.globalIndex = Thing.GetGlobalIndex();
			obj._firedFrom = firedFrom;
			obj.needsSynchronization = true;
			obj.isLocal = !network;
			if (Network.isActive && !network)
			{
				GhostManager.context.particleManager.AddLocalParticle(obj);
			}
			if (float.IsNaN(obj.position.x) || float.IsNaN(obj.position.y))
			{
				if (obj.stick != null)
				{
					obj.position.x = 0f;
					obj.position.y = 0f;
				}
				else
				{
					obj.position.x = Vec2.NetMin.x;
					obj.position.y = Vec2.NetMin.y;
				}
			}
		}
		return obj;
	}

	public override void NetSerialize(BitBuffer b)
	{
		if (stick != null && stick.ghostObject != null)
		{
			b.Write(val: true);
			b.Write((ushort)(int)stick.ghostObject.ghostObjectIndex);
			b.Write((sbyte)stickOffset.x);
			b.Write((sbyte)stickOffset.y);
		}
		else
		{
			b.Write(val: false);
			b.Write((short)base.x);
			b.Write((short)base.y);
		}
	}

	public override void NetDeserialize(BitBuffer d)
	{
		if (d.ReadBool())
		{
			GhostObject attach = GhostManager.context.GetGhost(d.ReadUShort());
			if (attach != null && attach.thing != null)
			{
				stick = attach.thing as MaterialThing;
			}
			int xOffset = d.ReadSByte();
			int yOffset = d.ReadSByte();
			stickOffset = new Vec2(xOffset, yOffset);
			UpdateStick();
			hSpeed = 0f;
			vSpeed = 0f;
		}
		else
		{
			float xpos = d.ReadShort();
			float ypos = d.ReadShort();
			netLerpPosition = new Vec2(xpos, ypos);
		}
	}

	private SmallFire()
		: base(0f, 0f)
	{
		_bounceEfficiency = 0.2f;
		_sprite = new SpriteMap("smallFire", 16, 16);
		_sprite.AddAnimation("burn", 0.2f + Rando.Float(0.2f), true, 0, 1, 2, 3, 4);
		graphic = _sprite;
		center = new Vec2(8f, 14f);
		_airFire = new SpriteMap("airFire", 16, 16);
		_airFire.AddAnimation("burn", 0.2f + Rando.Float(0.2f), true, 0, 1, 2, 1);
		_airFire.center = new Vec2(8f, 8f);
		_collisionSize = new Vec2(12f, 12f);
		_collisionOffset = new Vec2(-6f, -6f);
	}

	private void Init(float xpos, float ypos, float hspeed, float vspeed, bool shortLife = false, MaterialThing stick = null, bool canMultiply = true)
	{
		if (xpos == 0f && ypos == 0f && stick == null)
		{
			xpos = Vec2.NetMin.x;
			ypos = Vec2.NetMin.y;
		}
		position.x = xpos;
		position.y = ypos;
		_airFireScale = 0f;
		_multiplied = false;
		_groundLife = 125;
		doFloat = false;
		hSpeed = hspeed;
		vSpeed = vspeed;
		_sprite.SetAnimation("burn");
		_sprite.imageIndex = Rando.Int(4);
		float num = (base.yscale = 0.8f + Rando.Float(0.6f));
		base.xscale = num;
		base.angleDegrees = -10f + Rando.Float(20f);
		_airFire.SetAnimation("burn");
		_airFire.imageIndex = Rando.Int(2);
		SpriteMap airFire = _airFire;
		num = (_airFire.yscale = 0f);
		airFire.xscale = num;
		_spinSpeed = 0.1f + Rando.Float(0.1f);
		_airFire.color = Color.Orange * (0.8f + Rando.Float(0.2f));
		_gravMult = 0.7f;
		_sticky = 0.6f;
		_life = 100f;
		if (Network.isActive)
		{
			_sticky = 0f;
		}
		_fireID = FireManager.GetFireID();
		needsSynchronization = true;
		if (shortLife)
		{
			_groundLife = 31;
		}
		base.depth = 0.6f;
		_stick = stick;
		_stickOffset = new Vec2(xpos, ypos);
		UpdateStick();
		_alternate = kAlternate;
		kAlternate = !kAlternate;
		_canMultiply = canMultiply;
	}

	public void UpdateStick()
	{
		if (_stick != null)
		{
			position = _stick.Offset(_stickOffset);
		}
	}

	public void SuckLife(float l)
	{
		_life -= l;
	}

	public override void Removed()
	{
		if (Network.isActive && !didRemove && isLocal && GhostManager.context != null)
		{
			didRemove = true;
			GhostManager.context.particleManager.RemoveParticle(this);
		}
		base.Removed();
	}

	public override void Update()
	{
		if (waitToHurt > 0f)
		{
			waitToHurt -= Maths.IncFrameTimer();
		}
		else
		{
			whoWait = null;
		}
		if (!isLocal)
		{
			if (_stick != null)
			{
				UpdateStick();
			}
			else
			{
				base.Update();
			}
			return;
		}
		if (_airFireScale < 1.2f)
		{
			_airFireScale += 0.15f;
		}
		if (_grounded && _stick == null)
		{
			_airFireScale -= 0.3f;
			if (_airFireScale < 0.9f)
			{
				_airFireScale = 0.9f;
			}
			_spinSpeed -= 0.01f;
			if (_spinSpeed < 0.05f)
			{
				_spinSpeed = 0.05f;
			}
		}
		if (_grounded)
		{
			if (_groundLife <= 0)
			{
				base.alpha -= 0.04f;
				if (base.alpha < 0f)
				{
					Level.Remove(this);
				}
			}
			else
			{
				_groundLife--;
			}
		}
		if (base.y > Level.current.bottomRight.y + 200f)
		{
			Level.Remove(this);
		}
		SpriteMap airFire = _airFire;
		float num = (_airFire.yscale = _airFireScale);
		airFire.xscale = num;
		_airFire.depth = base.depth - 1;
		_airFire.alpha = 0.5f;
		_airFire.angle += hSpeed * _spinSpeed;
		if (isLocal && _canMultiply && !_multiplied && Rando.Float(310f) < 1f && base.y > base.level.topLeft.y - 500f)
		{
			Level.Add(New(base.x, base.y, -0.5f + Rando.Float(1f), 0f - (0.5f + Rando.Float(0.5f))));
			_multiplied = true;
		}
		if (_stick == null)
		{
			if (base.level != null && base.y < base.level.topLeft.y - 1500f)
			{
				Level.Remove(this);
			}
			base.Update();
		}
		else
		{
			_grounded = true;
			if (_stick.destroyed)
			{
				_stick = null;
				_grounded = false;
			}
			else
			{
				UpdateStick();
				stick.UpdateFirePosition(this);
				if (!_stick.onFire || _stick.removeFromLevel || _stick.alpha < 0.01f)
				{
					Level.Add(SmallSmoke.New(base.x, base.y));
					Level.Remove(this);
				}
			}
		}
		_alternateb = !_alternateb;
		if (_alternateb)
		{
			_alternate = !_alternate;
		}
	}

	public override void Draw()
	{
		base.Draw();
	}
}
