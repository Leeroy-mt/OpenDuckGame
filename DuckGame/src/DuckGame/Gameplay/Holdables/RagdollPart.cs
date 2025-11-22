using System;
using System.Collections.Generic;

namespace DuckGame;

[BaggedProperty("canSpawn", false)]
[BaggedProperty("isInDemo", true)]
public class RagdollPart : Holdable, IAmADuck
{
	public StateBinding _dollBinding = new StateBinding("_doll");

	public StateBinding _connectBinding = new StateBinding("connect");

	public StateBinding _jointBinding = new StateBinding("_joint");

	public StateBinding _partBinding = new StateBinding("netPart", 2);

	public StateBinding _framesSinceGroundedBinding = new StateBinding("framesSinceGrounded", 4);

	private bool _setting;

	public float extraGravMultiplier = 1f;

	public Vec2 _lastReasonablePosition;

	private SpriteMap _sprite;

	private SpriteMap _quackSprite;

	public RagdollPart _joint;

	public RagdollPart connect;

	public int clipFrames;

	private DuckPersona _prevPersona;

	private int _part;

	public float addWeight;

	private bool _zekeBear;

	private DuckPersona _rlPersona;

	public Ragdoll _doll;

	private SpriteMap _campDuck;

	private bool extinguishing;

	private bool _setSkipClip;

	private int _ownTime;

	private Vec2 _stickLerp;

	private Vec2 _stickSlowLerp;

	public override NetworkConnection connection
	{
		get
		{
			if (_doll != null)
			{
				return _doll.connection;
			}
			return base.connection;
		}
		set
		{
			if (!_setting)
			{
				_setting = true;
				if (_doll != null)
				{
					_doll.connection = value;
				}
				base.connection = value;
				_setting = false;
			}
		}
	}

	public override NetIndex8 authority
	{
		get
		{
			if (_doll != null)
			{
				return _doll.authority;
			}
			return base.authority;
		}
		set
		{
			if (!_setting)
			{
				_setting = true;
				if (_doll != null)
				{
					_doll.authority = value;
				}
				base.authority = value;
				_setting = false;
			}
		}
	}

	public override float currentGravity => PhysicsObject.gravity * gravMultiplier * floatMultiplier * extraGravMultiplier;

	public byte netPart
	{
		get
		{
			return (byte)_part;
		}
		set
		{
			part = value;
		}
	}

	public RagdollPart joint
	{
		get
		{
			return _joint;
		}
		set
		{
			_joint = value;
		}
	}

	public int part
	{
		get
		{
			return _part;
		}
		set
		{
			int prevPart = _part;
			_part = value;
			if (_doll != null && _doll._duck != null)
			{
				_persona = _doll._duck.persona;
			}
			if (part == 0)
			{
				center = new Vec2(16f, 13f);
			}
			else if (part == 1)
			{
				center = new Vec2(16f, 13f);
			}
			else if (part == 3)
			{
				center = new Vec2(6f, 8f);
			}
			else
			{
				center = new Vec2(8f, 8f);
			}
			if (part == 0 || part == 1)
			{
				if (part == 0)
				{
					collisionOffset = new Vec2(-4f, -5f);
					collisionSize = new Vec2(8f, 10f);
				}
				else
				{
					collisionOffset = new Vec2(-4f, -5f);
					collisionSize = new Vec2(8f, 10f);
				}
			}
			else
			{
				collisionOffset = new Vec2(-1f, -1f);
				collisionSize = new Vec2(2f, 2f);
			}
			if (_persona != null && (_prevPersona != _persona || prevPart != _part))
			{
				_quackSprite = _persona.quackSprite.CloneMap();
				_sprite = _persona.sprite.CloneMap();
				_quackSprite.frame = 18;
				_sprite.frame = 18;
				if (prevPart != _part || graphic == null)
				{
					graphic = _sprite;
				}
				_quackSprite.frame = ((_part == 0) ? 18 : 19);
				_sprite.frame = ((_part == 0) ? 18 : 19);
				if (doll != null && doll.captureDuck != null && doll.captureDuck.eyesClosed)
				{
					_quackSprite.frame = ((_part == 0) ? 20 : 19);
					_sprite.frame = ((_part == 0) ? 20 : 19);
				}
				_prevPersona = _persona;
			}
		}
	}

	public override float weight
	{
		get
		{
			return _weight + addWeight;
		}
		set
		{
			_weight = value;
		}
	}

	public DuckPersona _persona
	{
		get
		{
			return _rlPersona;
		}
		set
		{
			_rlPersona = value;
		}
	}

	public Ragdoll doll
	{
		get
		{
			return _doll;
		}
		set
		{
			_doll = value;
		}
	}

	public override void Zap(Thing zapper)
	{
		if (_doll != null)
		{
			_doll.Zap(zapper);
		}
		base.Zap(zapper);
	}

	public void MakeZekeBear()
	{
		_quackSprite = new SpriteMap("teddy", 32, 32);
		_sprite = _quackSprite;
		_quackSprite.frame = 0;
		_sprite.frame = 0;
		graphic = _sprite;
		_quackSprite.frame = ((_part != 0) ? 1 : 0);
		_sprite.frame = ((_part != 0) ? 1 : 0);
		if (part == 0)
		{
			center = new Vec2(16f, 16f);
		}
		else if (part == 1)
		{
			center = new Vec2(16f, 13f);
		}
		else if (part == 3)
		{
			center = new Vec2(6f, 8f);
		}
		else
		{
			center = new Vec2(8f, 8f);
		}
		_zekeBear = true;
	}

	public RagdollPart(float xpos, float ypos, int p, DuckPersona persona, int off, Ragdoll doll)
		: base(xpos, ypos)
	{
		if (persona == null)
		{
			persona = Persona.Duck1;
		}
		_sprite = new SpriteMap("crate", 16, 16);
		_campDuck = new SpriteMap("campduck", 32, 32);
		graphic = _sprite;
		_editorName = "Crate";
		thickness = 0.5f;
		weight = 0.05f;
		base.bouncy = 0.6f;
		_holdOffset = new Vec2(2f, 0f);
		flammable = 0.3f;
		tapeable = false;
		SortOutDetails(xpos, ypos, p, persona, off, doll);
	}

	public override void Extinquish()
	{
		if (!extinguishing)
		{
			extinguishing = true;
			if (doll != null && doll.captureDuck != null)
			{
				doll.captureDuck.Extinquish();
			}
			base.Extinquish();
			extinguishing = false;
		}
	}

	public void SortOutDetails(float xpos, float ypos, int p, DuckPersona persona, int off, Ragdoll doll)
	{
		base.x = xpos;
		base.y = ypos;
		hSpeed = 0f;
		vSpeed = 0f;
		_part = part;
		offDir = (sbyte)off;
		airFrictionMult = 0.3f;
		_persona = persona;
		_doll = doll;
		part = p;
	}

	public override void OnTeleport()
	{
		position.x += Math.Sign(hSpeed) * 8;
		doll.part1.position = position;
		doll.part2.position = position;
		doll.part3.position = position;
		doll.part1.hSpeed = hSpeed;
		doll.part2.hSpeed = hSpeed;
		doll.part3.hSpeed = hSpeed;
	}

	public override bool Hit(Bullet bullet, Vec2 hitPos)
	{
		if (_doll == null)
		{
			return false;
		}
		if (bullet.isLocal && owner == null)
		{
			Thing.Fondle(_doll, DuckNetwork.localConnection);
		}
		if (bullet.isLocal && _doll.captureDuck != null)
		{
			Duck d = _doll.captureDuck;
			Equipment chest = d.GetEquipment(typeof(ChestPlate));
			if (chest != null && Collision.Point(hitPos, chest))
			{
				chest.UnEquip();
				SFX.Play("ting2");
				d.Unequip(chest);
				chest.hSpeed = bullet.travelDirNormalized.x;
				chest.vSpeed = -2f;
				chest.Destroy(new DTShot(bullet));
				chest.solid = false;
				return true;
			}
			Equipment head = d.GetEquipment(typeof(Helmet));
			if (head != null && Collision.Point(hitPos, head))
			{
				head.UnEquip();
				SFX.Play("ting2");
				d.Unequip(head);
				head.hSpeed = bullet.travelDirNormalized.x;
				head.vSpeed = -2f;
				head.Destroy(new DTShot(bullet));
				head.solid = false;
				return true;
			}
		}
		Feather feather = Feather.New(0f, 0f, _persona);
		feather.hSpeed = (0f - bullet.travelDirNormalized.x) * (1f + Rando.Float(1f));
		feather.vSpeed = 0f - Rando.Float(2f);
		feather.position = hitPos;
		Level.Add(feather);
		if (bullet.isLocal)
		{
			hSpeed += bullet.travelDirNormalized.x * bullet.ammo.impactPower;
			vSpeed += bullet.travelDirNormalized.y * bullet.ammo.impactPower;
			SFX.Play("thwip", 1f, Rando.Float(-0.1f, 0.1f));
			_doll.Shot(bullet);
		}
		return base.Hit(bullet, hitPos);
	}

	protected override bool OnDestroy(DestroyType type = null)
	{
		if (_doll == null)
		{
			return false;
		}
		if (type is DTIncinerate)
		{
			if (_doll.removeFromLevel || _doll.captureDuck == null || !_doll.captureDuck.dead)
			{
				if (_doll.captureDuck != null)
				{
					_doll.captureDuck.Kill(type);
					return true;
				}
				return false;
			}
			CookedDuck cooked = new CookedDuck(_doll.x, _doll.y);
			Level.Add(SmallSmoke.New(_doll.x + Rando.Float(-4f, 4f), _doll.y + Rando.Float(-4f, 4f)));
			Level.Add(SmallSmoke.New(_doll.x + Rando.Float(-4f, 4f), _doll.y + Rando.Float(-4f, 4f)));
			Level.Add(SmallSmoke.New(_doll.x + Rando.Float(-4f, 4f), _doll.y + Rando.Float(-4f, 4f)));
			ReturnItemToWorld(cooked);
			cooked.vSpeed = vSpeed - 2f;
			cooked.hSpeed = hSpeed;
			Level.Add(cooked);
			SFX.Play("ignite", 1f, -0.3f + Rando.Float(0.3f));
			Level.Remove(_doll);
			_doll.captureDuck._cooked = cooked;
		}
		if (!destroyed)
		{
			_doll.Killed(type);
		}
		return false;
	}

	public override void ExitHit(Bullet bullet, Vec2 exitPos)
	{
	}

	public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
	{
		if (_doll.captureDuck != null && !_doll.captureDuck.dead && (part == 0 || part == 1) && base.totalImpactPower > 5f && (float)_doll.captureDuck.quack < 0.25f)
		{
			_doll.captureDuck.Swear();
			float rumb = Math.Min(base.totalImpactPower * 0.01f, 1f);
			if (rumb > 0.05f)
			{
				RumbleManager.AddRumbleEvent(_doll.captureDuck.profile, new RumbleEvent(rumb, 0.05f, 0.6f));
			}
		}
		base.OnSolidImpact(with, from);
	}

	public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
	{
		if (_doll == null || !base.isServerForObject || !with.isServerForObject || with is RagdollPart || with is FeatherVolume || with == owner || with == _doll.holdingOwner || with == _doll.captureDuck)
		{
			return;
		}
		if (with is Duck)
		{
			Holdable h = (with as Duck)._lastHoldItem;
			if ((with as Duck)._timeSinceThrow < 15 && (h == _doll.part1 || h == _doll.part2 || h == _doll.part3))
			{
				return;
			}
		}
		if (_doll.captureDuck != null)
		{
			Vec2 prevPos = _doll.captureDuck.position;
			_doll.captureDuck.collisionOffset = collisionOffset;
			_doll.captureDuck.collisionSize = collisionSize;
			_doll.captureDuck.position = position;
			_doll.captureDuck.OnSoftImpact(with, from);
			_doll.captureDuck.position = prevPos;
		}
	}

	public void UpdateLastReasonablePosition(Vec2 pPosition)
	{
		if (pPosition.y > -7000f && pPosition.y < Level.activeLevel.lowestPoint + 400f)
		{
			_lastReasonablePosition = pPosition;
		}
	}

	public override void Update()
	{
		if (_doll == null || (base.y > Level.activeLevel.lowestPoint + 1000f && base.isOffBottomOfLevel))
		{
			return;
		}
		UpdateLastReasonablePosition(position);
		if (clipFrames > 0)
		{
			clipFrames--;
		}
		if (owner != null && _doll != null && !doll.inSleepingBag)
		{
			_ownTime++;
			if (_ownTime > 20)
			{
				_doll.ShakeOutOfSleepingBag();
				_ownTime = 0;
			}
		}
		if (_doll.captureDuck != null)
		{
			if (_zekeBear)
			{
				if (_part == 0)
				{
					base.depth = _doll.captureDuck.depth + 2;
				}
				else
				{
					base.depth = _doll.captureDuck.depth;
				}
			}
			else if (_part == 0)
			{
				base.depth = _doll.captureDuck.depth - 10;
				if (_doll.part3 != null)
				{
					base.depth = _doll.part3.depth - 10;
				}
			}
			else
			{
				base.depth = _doll.captureDuck.depth;
			}
			canPickUp = true;
			if (_doll.captureDuck.HasEquipment(typeof(ChokeCollar)) && _part != 0)
			{
				canPickUp = false;
			}
		}
		if (_joint != null && connect != null)
		{
			if (owner == null && base.prevOwner != null)
			{
				base.clip.Add(base.prevOwner as PhysicsObject);
				connect.clip.Add(base.prevOwner as PhysicsObject);
				_joint.clipFrames = 12;
				_joint.clipThing = _prevOwner;
				_prevOwner = null;
			}
			if (owner != null)
			{
				_joint.clipFrames = 0;
				_joint.depth = base.depth;
			}
			if (owner != null || _joint.owner != null)
			{
				weight = 0.1f;
			}
			else
			{
				weight = 4f;
			}
			if (_zekeBear)
			{
				if (owner != null || _joint.owner != null)
				{
					weight = 0.1f;
				}
				else
				{
					weight = 0.2f;
				}
			}
			if (_joint.clipFrames > 0)
			{
				skipClip = true;
				_setSkipClip = true;
			}
			else if (_setSkipClip)
			{
				skipClip = false;
				_setSkipClip = false;
			}
		}
		if (_part > 1)
		{
			canPickUp = false;
		}
		base.Update();
		if (_doll.captureDuck != null && _doll.captureDuck.HasEquipment(typeof(FancyShoes)) && _part == 0 && _doll.captureDuck.holdObject != null)
		{
			_doll.captureDuck.holdObject.position = Offset(new Vec2(3f, 5f) + _doll.captureDuck.holdObject.holdOffset);
			_doll.captureDuck.holdObject.angle = angle;
			if (_doll.captureDuck.holdObject != null && _doll.captureDuck.isServerForObject)
			{
				_doll.captureDuck.holdObject.isLocal = isLocal;
				_doll.captureDuck.holdObject.UpdateAction();
			}
		}
		if (doll != null && doll.part3 != null)
		{
			offDir = doll.part3.offDir;
			if (doll.captureDuck != null && enablePhysics)
			{
				doll.captureDuck.offDir = offDir;
				if (owner != null)
				{
					doll.captureDuck._prevOwner = owner;
				}
			}
		}
		FluidPuddle p = Level.CheckPoint<FluidPuddle>(position + new Vec2(0f, 4f));
		if (p != null)
		{
			if (base.y + 4f - p.top > 8f)
			{
				gravMultiplier = -0.5f;
				base.grounded = false;
			}
			else
			{
				if (base.y + 4f - p.top < 3f)
				{
					gravMultiplier = 0.2f;
					base.grounded = true;
				}
				else if (base.y + 4f - p.top > 4f)
				{
					gravMultiplier = -0.2f;
					base.grounded = true;
				}
				base.grounded = true;
			}
		}
		else
		{
			gravMultiplier = 1f;
		}
		if (_joint != null)
		{
			if (_doll.captureDuck != null && _doll.captureDuck.IsQuacking())
			{
				graphic = _quackSprite;
			}
			else
			{
				graphic = _sprite;
			}
			if (base.isServerForObject)
			{
				if (offDir < 0)
				{
					base.angleDegrees = 0f - Maths.PointDirection(position, _joint.position) + 180f + 90f;
				}
				else
				{
					base.angleDegrees = 0f - Maths.PointDirection(position, _joint.position) - 90f;
				}
			}
		}
		if (_part == 3 && connect != null)
		{
			base.angleDegrees = 0f - Maths.PointDirection(position, connect.position) + 180f;
			base.depth = connect.depth + 2;
		}
		visible = _part != 2;
	}

	protected override bool OnBurn(Vec2 firePosition, Thing litBy)
	{
		if (!_onFire)
		{
			SFX.Play("ignite", 1f, -0.3f + Rando.Float(0.3f));
			for (int i = 0; i < 2; i++)
			{
				Level.Add(SmallFire.New(-3f + Rando.Float(6f), -2f + Rando.Float(2f), 0f, 0f, shortLife: false, this));
			}
			_onFire = true;
			_doll.LitOnFire(litBy);
		}
		return true;
	}

	public override void Draw()
	{
		addWeight = 0f;
		extraGravMultiplier = 1f;
		if (_part == 2 || _joint == null)
		{
			return;
		}
		Vec2 pos = position;
		Vec2 gap = position - _joint.position;
		float dist = gap.length;
		if (dist > 8f)
		{
			dist = 8f;
		}
		position = _joint.position + gap.normalized * dist;
		if (_part == 0 && _doll != null && _doll.captureDuck != null && (_doll.captureDuck.quack > 0 || (doll != null && doll.tongueStuck != Vec2.Zero)))
		{
			Vec2 rs = _doll.captureDuck.tounge;
			_stickLerp = Lerp.Vec2Smooth(_stickLerp, rs, 0.2f);
			_stickSlowLerp = Lerp.Vec2Smooth(_stickSlowLerp, rs, 0.1f);
			Vec2 stick = _stickLerp;
			Vec2 facing = Maths.AngleToVec(angle);
			if (offDir < 0)
			{
				stick *= Maths.Clamp(1f - (facing - stick * -1f).length, 0f, 1f);
			}
			else
			{
				stick *= Maths.Clamp(1f - (facing - stick).length, 0f, 1f);
			}
			stick.y *= -1f;
			Vec2 stick2 = _stickSlowLerp;
			stick2.y *= -1f;
			float len = stick.length;
			_ = 0.5f;
			bool tongueStuck = false;
			if (doll != null && doll.tongueStuck != Vec2.Zero)
			{
				tongueStuck = true;
				len = 1f;
			}
			if (len > 0.05f || tongueStuck)
			{
				Vec2 offsetVec = (position - _joint.position).normalized;
				Vec2 mouthPos = position - offsetVec * 3f;
				if (tongueStuck)
				{
					stick = (doll.tongueStuck - mouthPos) / 6f;
					stick2 = (doll.tongueStuck - mouthPos) / 6f / 2f;
					stick2 = (Offset(new Vec2((doll.tongueStuck - mouthPos).length / 2f, 2f)) - mouthPos) / 6f;
				}
				List<Vec2> list = Curve.Bezier(8, mouthPos, mouthPos + stick2 * 6f, mouthPos + stick * 6f);
				Vec2 prev = Vec2.Zero;
				float lenMul = 1f;
				foreach (Vec2 p in list)
				{
					if (prev != Vec2.Zero)
					{
						Vec2 dir = prev - p;
						Graphics.DrawTexturedLine(Graphics.tounge.texture, prev + dir.normalized * 0.4f, p, new Color(223, 30, 30), 0.15f * lenMul, base.depth + 1);
						Graphics.DrawTexturedLine(Graphics.tounge.texture, prev + dir.normalized * 0.4f, p - dir.normalized * 0.4f, Color.Black, 0.3f * lenMul, base.depth - 1);
					}
					lenMul -= 0.1f;
					prev = p;
					if (doll != null && doll.captureDuck != null)
					{
						doll.captureDuck.tongueCheck = p;
					}
				}
				if (_graphic != null && _graphic == _quackSprite)
				{
					SpriteMap spr = graphic as SpriteMap;
					if (doll != null && doll.inSleepingBag)
					{
						_graphic = _campDuck;
					}
					if (_offDir < 0)
					{
						_graphic.flipH = true;
					}
					else
					{
						_graphic.flipH = false;
					}
					_graphic.position = position;
					_graphic.alpha = base.alpha;
					_graphic.angle = angle;
					_graphic.depth = base.depth + 4;
					_graphic.scale = base.scale;
					_graphic.center = center;
					if (_graphic == _campDuck)
					{
						(_graphic as SpriteMap).frame = 4;
						_graphic.Draw();
					}
					else
					{
						(_graphic as SpriteMap).frame += 36;
						_graphic.Draw();
						(_graphic as SpriteMap).frame -= 36;
					}
					_graphic = spr;
				}
			}
			else if (doll != null && doll.captureDuck != null)
			{
				doll.captureDuck.tongueCheck = Vec2.Zero;
			}
		}
		else if (doll != null && doll.captureDuck != null)
		{
			doll.captureDuck.tongueCheck = Vec2.Zero;
		}
		SpriteMap s = graphic as SpriteMap;
		if (s != null && doll != null && doll.inSleepingBag)
		{
			if (s.frame == 18)
			{
				_campDuck.frame = 0;
			}
			else if (s.frame == 19)
			{
				_campDuck.frame = 1;
			}
			else if (s.frame == 20)
			{
				_campDuck.frame = 2;
			}
			if (s == _quackSprite && s.frame == 18)
			{
				_campDuck.frame = 3;
			}
			graphic = _campDuck;
		}
		float ang = base.angleDegrees;
		if (offDir < 0)
		{
			base.angleDegrees = 0f - Maths.PointDirection(position, _joint.position) + 180f + 90f;
		}
		else
		{
			base.angleDegrees = 0f - Maths.PointDirection(position, _joint.position) - 90f;
		}
		base.Draw();
		base.angleDegrees = ang;
		graphic = s;
		position = pos;
	}
}
