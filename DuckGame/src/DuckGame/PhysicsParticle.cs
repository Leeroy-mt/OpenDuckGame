using System;

namespace DuckGame;

public abstract class PhysicsParticle : Thing
{
	public ushort netIndex;

	public byte updateOrder = byte.MaxValue;

	public bool netRemove;

	public bool _grounded;

	protected float _spinAngle;

	protected string _bounceSound = "";

	protected float _bounceEfficiency = 0.5f;

	protected float _gravMult = 1f;

	protected float _sticky;

	protected bool _foreverGrounded;

	protected float _stickDir;

	public bool gotMessage;

	public bool needsSynchronization;

	protected bool _hit;

	protected bool _touchedFloor;

	private float _framesAlive;

	private bool _waitForNoCollide;

	protected float _airFriction = 0.03f;

	protected float _life = 1f;

	public Vec2 lerpPos = Vec2.Zero;

	public new Vec2 lerpSpeed = Vec2.Zero;

	private static Map<byte, Type> _netParticleTypes = new Map<byte, Type>();

	private static byte _netParticleTypeIndex = 0;

	protected Vec2 netLerpPosition = Vec2.Zero;

	public bool customGravity;

	public bool onlyDieWhenGrounded;

	public float spinAngle
	{
		get
		{
			return _spinAngle;
		}
		set
		{
			_spinAngle = value;
		}
	}

	public float life
	{
		get
		{
			return _life;
		}
		set
		{
			_life = value;
		}
	}

	public PhysicsParticle(float xpos, float ypos)
		: base(xpos, ypos)
	{
	}

	public void LerpPosition(Vec2 pos)
	{
		lerpPos = pos;
	}

	public void LerpSpeed(Vec2 speed)
	{
		lerpSpeed = speed;
	}

	public static void RegisterNetParticleType(Type pType)
	{
		if (!_netParticleTypes.ContainsValue(pType))
		{
			_netParticleTypes[_netParticleTypeIndex] = pType;
			_netParticleTypeIndex++;
		}
	}

	public static byte TypeToNetTypeIndex(Type pType)
	{
		if (_netParticleTypes.ContainsValue(pType))
		{
			return _netParticleTypes.Get(pType);
		}
		return byte.MaxValue;
	}

	public static Type NetTypeToTypeIndex(byte pNetType)
	{
		if (pNetType >= 254)
		{
			return null;
		}
		if (_netParticleTypes.ContainsKey(pNetType))
		{
			return _netParticleTypes.Get(pNetType);
		}
		return null;
	}

	public virtual void NetSerialize(BitBuffer b)
	{
		b.Write((short)base.x);
		b.Write((short)base.y);
	}

	public virtual void NetDeserialize(BitBuffer d)
	{
		float xpos = d.ReadShort();
		float ypos = d.ReadShort();
		netLerpPosition = new Vec2(xpos, ypos);
	}

	public override void ResetProperties()
	{
		_life = 1f;
		_grounded = false;
		_spinAngle = 0f;
		_foreverGrounded = false;
		base.alpha = 1f;
		_airFriction = 0.03f;
		vSpeed = 0f;
		hSpeed = 0f;
		_framesAlive = 0f;
		_waitForNoCollide = false;
		base.globalIndex = Thing.GetGlobalIndex();
		gotMessage = false;
		isLocal = true;
		netIndex = 0;
		updateOrder = byte.MaxValue;
		netRemove = false;
		base.ResetProperties();
	}

	public override void Update()
	{
		if (!isLocal)
		{
			Vec2 me = position;
			Vec2 targ = netLerpPosition;
			if ((me - targ).lengthSq > 2048f || (me - targ).lengthSq < 1f)
			{
				position = targ;
			}
			else
			{
				position = Lerp.Vec2Smooth(me, targ, 0.5f);
			}
			return;
		}
		if (Network.isActive && (base.y < Level.current.highestPoint - 200f || base.y > Level.current.lowestPoint + 200f))
		{
			Level.Remove(this);
			return;
		}
		_hit = false;
		_touchedFloor = false;
		_framesAlive += 1f;
		if (!onlyDieWhenGrounded || _grounded || _framesAlive > 400f)
		{
			_life -= 0.005f;
			if (_life < 0f)
			{
				base.alpha -= 0.1f;
				if (base.alpha < 0f)
				{
					Level.Remove(this);
				}
			}
		}
		if (_foreverGrounded)
		{
			_grounded = true;
			if (Rando.Float(250f) < 1f - _sticky)
			{
				_foreverGrounded = false;
				_grounded = false;
				hSpeed = (0f - _stickDir) * Rando.Float(0.8f);
			}
		}
		if (!_grounded)
		{
			if (hSpeed > 0f)
			{
				hSpeed -= _airFriction;
			}
			if (hSpeed < 0f)
			{
				hSpeed += _airFriction;
			}
			if (hSpeed < _airFriction && hSpeed > 0f - _airFriction)
			{
				hSpeed = 0f;
			}
			if (vSpeed < 4f)
			{
				vSpeed += 0.1f * _gravMult;
			}
			if (float.IsNaN(hSpeed))
			{
				hSpeed = 0f;
			}
			_spinAngle -= 10 * Math.Sign(hSpeed);
			Thing col = Level.CheckPoint<Block>(base.x + hSpeed, base.y + vSpeed);
			if (col != null && _framesAlive < 2f)
			{
				_waitForNoCollide = true;
			}
			if (col != null && _waitForNoCollide)
			{
				col = null;
			}
			else if (col == null && _waitForNoCollide)
			{
				_waitForNoCollide = false;
			}
			if (col != null)
			{
				_touchedFloor = true;
				if (_bounceSound != "" && (Math.Abs(vSpeed) > 1f || Math.Abs(hSpeed) > 1f))
				{
					SFX.Play(_bounceSound, 0.5f, -0.1f + Rando.Float(0.2f));
				}
				if (vSpeed > 0f && col.top > base.y)
				{
					vSpeed = 0f - vSpeed * _bounceEfficiency;
					_hit = true;
					if (Math.Abs(vSpeed) < 0.5f)
					{
						vSpeed = 0f;
						_grounded = true;
					}
				}
				else if (vSpeed < 0f && col.bottom < base.y)
				{
					vSpeed = 0f - vSpeed * _bounceEfficiency;
					_hit = true;
				}
				if (hSpeed > 0f && col.left > base.x)
				{
					hSpeed = 0f - hSpeed * _bounceEfficiency;
					_hit = true;
					if (_sticky > 0f && Rando.Float(1f) < _sticky)
					{
						hSpeed = 0f;
						vSpeed = 0f;
						_foreverGrounded = true;
						_stickDir = 1f;
					}
				}
				else if (hSpeed < 0f && col.right < base.x)
				{
					hSpeed = 0f - hSpeed * _bounceEfficiency;
					_hit = true;
					if (_sticky > 0f && Rando.Float(1f) < _sticky)
					{
						hSpeed = 0f;
						vSpeed = 0f;
						_foreverGrounded = true;
						_stickDir = -1f;
					}
				}
				if (!_hit)
				{
					_grounded = true;
				}
			}
			else
			{
				base.x += hSpeed;
				base.y += vSpeed;
			}
		}
		if (_spinAngle > 360f)
		{
			_spinAngle -= 360f;
		}
		if (_spinAngle < 0f)
		{
			_spinAngle += 360f;
		}
	}
}
