namespace DuckGame;

[EditorGroup("Guns|Misc")]
[BaggedProperty("isFatal", false)]
public class DartGun : Gun
{
	public StateBinding _burnLifeBinding = new StateBinding("_burnLife");

	private SpriteMap _sprite;

	public float _burnLife = 1f;

	public float _burnWait;

	public bool burntOut;

	public DartGun(float xval, float yval)
		: base(xval, yval)
	{
		ammo = 12;
		_ammoType = new ATLaser();
		_ammoType.range = 170f;
		_ammoType.accuracy = 0.8f;
		wideBarrel = true;
		barrelInsertOffset = new Vec2(-3f, -1f);
		_type = "gun";
		_sprite = new SpriteMap("dartgun", 32, 32);
		graphic = _sprite;
		center = new Vec2(16f, 16f);
		collisionOffset = new Vec2(-8f, -4f);
		collisionSize = new Vec2(16f, 9f);
		_barrelOffsetTL = new Vec2(29f, 14f);
		_fireSound = "smg";
		_fullAuto = true;
		_fireWait = 1f;
		_kickForce = 1f;
		_fireRumble = RumbleIntensity.Kick;
		flammable = 0.8f;
		_barrelAngleOffset = 8f;
		physicsMaterial = PhysicsMaterial.Plastic;
		editorTooltip = "Shoots like a gun, feels like a gentle breeze.";
		isFatal = false;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	public override void UpdateFirePosition(SmallFire f)
	{
		f.position = Offset(new Vec2(10f, 0f));
	}

	public override void UpdateOnFire()
	{
		if (base.onFire)
		{
			_burnWait -= 0.01f;
			if (_burnWait < 0f)
			{
				Level.Add(SmallFire.New(10f, 0f, 0f, 0f, shortLife: false, this, canMultiply: false, this));
				_burnWait = 1f;
			}
			if (burnt < 1f)
			{
				burnt += 0.001f;
			}
		}
	}

	public override void Update()
	{
		if (!burntOut && burnt >= 1f)
		{
			_sprite.frame = 1;
			Vec2 smokePos = Offset(new Vec2(10f, 0f));
			Level.Add(SmallSmoke.New(smokePos.x, smokePos.y));
			_onFire = false;
			flammable = 0f;
			burntOut = true;
		}
		base.Update();
	}

	protected override bool OnBurn(Vec2 firePosition, Thing litBy)
	{
		base.onFire = true;
		return true;
	}

	public override void Draw()
	{
		base.Draw();
	}

	public override void OnPressAction()
	{
		if (ammo > 0)
		{
			if (_burnLife <= 0f)
			{
				SFX.Play("dartStick", 0.5f, -0.1f + Rando.Float(0.2f));
				return;
			}
			ammo--;
			SFX.Play("dartGunFire", 0.5f, -0.1f + Rando.Float(0.2f));
			kick = 1f;
			if (!receivingPress && base.isServerForObject)
			{
				Vec2 pos = Offset(base.barrelOffset + new Vec2(-8f, 0f));
				float fireAngle = base.barrelAngle + Rando.Float(-0.05f, 0.05f);
				Dart d = new Dart(pos.x, pos.y, owner as Duck, 0f - fireAngle);
				Fondle(d);
				if (base.onFire)
				{
					Level.Add(SmallFire.New(0f, 0f, 0f, 0f, shortLife: false, d, canMultiply: true, this));
					d.burning = true;
					d.onFire = true;
				}
				_barrelHeat += 0.015f;
				Vec2 travelDir = Maths.AngleToVec(fireAngle);
				d.hSpeed = travelDir.x * 10f;
				d.vSpeed = travelDir.y * 10f;
				Level.Add(d);
			}
		}
		else
		{
			DoAmmoClick();
		}
	}

	public override void Fire()
	{
	}
}
