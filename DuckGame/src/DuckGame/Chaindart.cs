using System;

namespace DuckGame;

[EditorGroup("Guns|Machine Guns")]
[BaggedProperty("isSuperWeapon", true)]
public class Chaindart : Gun
{
	public StateBinding _fireWaitBinding = new StateBinding("_fireWait");

	public StateBinding _spinBinding = new StateBinding("_spin");

	public StateBinding _spinningBinding = new StateBinding("_spinning");

	public StateBinding _burnLifeBinding = new StateBinding("_burnLife");

	public float _burnLife = 1f;

	public float _burnWait;

	private SpriteMap _burned;

	private SpriteMap _tip;

	private SpriteMap _sprite;

	public float _spin;

	private ChaingunBullet _bullets;

	private ChaingunBullet _topBullet;

	private Sound _spinUp;

	private Sound _spinDown;

	private int bulletsTillRemove = 10;

	private int numHanging = 10;

	private bool _spinning;

	private float spinAmount;

	private bool burntOut;

	public Chaindart(float xval, float yval)
		: base(xval, yval)
	{
		ammo = 100;
		_ammoType = new ATDart();
		_ammoType.range = 170f;
		_ammoType.accuracy = 0.5f;
		wideBarrel = true;
		barrelInsertOffset = new Vec2(0f, 0f);
		_type = "gun";
		_sprite = new SpriteMap("dartchain", 38, 18);
		graphic = _sprite;
		center = new Vec2(14f, 9f);
		collisionOffset = new Vec2(-8f, -3f);
		collisionSize = new Vec2(24f, 10f);
		_burned = new SpriteMap("dartchain_burned", 38, 18);
		graphic = _sprite;
		_tip = new SpriteMap("dartchain_tip", 38, 18);
		_barrelOffsetTL = new Vec2(38f, 8f);
		_fireSound = "pistolFire";
		_fullAuto = true;
		_fireWait = 0.7f;
		_kickForce = 1f;
		_fireRumble = RumbleIntensity.Kick;
		weight = 4f;
		_spinUp = SFX.Get("chaingunSpinUp");
		_spinDown = SFX.Get("chaingunSpinDown");
		_holdOffset = new Vec2(4f, 2f);
		flammable = 0.8f;
		physicsMaterial = PhysicsMaterial.Plastic;
		editorTooltip = "Like a chaingun, but for babies. Fires safety-capped sponge darts.";
	}

	public override void Initialize()
	{
		base.Initialize();
		_bullets = new ChaingunBullet(base.x, base.y, dart: true);
		_bullets.parentThing = this;
		_topBullet = _bullets;
		float add = 0.1f;
		ChaingunBullet lastBullet = null;
		for (int i = 0; i < 9; i++)
		{
			ChaingunBullet b = new ChaingunBullet(base.x, base.y, dart: true);
			b.parentThing = _bullets;
			_bullets = b;
			b.waveAdd = add;
			add += 0.4f;
			if (i == 0)
			{
				_topBullet.childThing = b;
			}
			else
			{
				lastBullet.childThing = b;
			}
			lastBullet = b;
		}
	}

	public override void Terminate()
	{
	}

	public override void OnPressAction()
	{
		if (burntOut)
		{
			SFX.Play("dartStick", 0.5f, -0.1f + Rando.Float(0.2f));
		}
		else
		{
			base.OnPressAction();
		}
	}

	public override void OnHoldAction()
	{
		if (!burntOut)
		{
			if (!_spinning)
			{
				_spinning = true;
				_spinDown.Volume = 0f;
				_spinDown.Stop();
				_spinUp.Volume = 1f;
				_spinUp.Play();
			}
			if (_spin < 1f)
			{
				_spin += 0.04f;
				return;
			}
			_spin = 1f;
			base.OnHoldAction();
		}
	}

	public override void OnReleaseAction()
	{
		if (_spinning)
		{
			_spinning = false;
			_spinUp.Volume = 0f;
			_spinUp.Stop();
			if (_spin > 0.9f)
			{
				_spinDown.Volume = 1f;
				_spinDown.Play();
			}
		}
	}

	public override void UpdateOnFire()
	{
		if (base.onFire)
		{
			_burnWait -= 0.01f;
			if (_burnWait < 0f)
			{
				Level.Add(SmallFire.New(22f, 0f, 0f, 0f, shortLife: false, this, canMultiply: false, this));
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
			Vec2 smokePos = Offset(new Vec2(10f, 0f));
			Level.Add(SmallSmoke.New(smokePos.x, smokePos.y));
			_onFire = false;
			flammable = 0f;
			burntOut = true;
		}
		if (_topBullet != null)
		{
			_topBullet.DoUpdate();
			int newHanging = (int)((float)ammo / (float)bulletsTillRemove);
			if (newHanging < numHanging)
			{
				_topBullet = _topBullet.childThing as ChaingunBullet;
				if (_topBullet != null)
				{
					_topBullet.parentThing = this;
				}
			}
			numHanging = newHanging;
		}
		if (base.isServerForObject)
		{
			FluidPuddle pudd = Level.CheckPoint<FluidPuddle>(base.barrelPosition.x, base.barrelPosition.y);
			if (pudd != null && pudd.data.heat > 0.5f)
			{
				OnBurn(base.barrelPosition, pudd);
			}
		}
		_fireWait = 0.65f + Maths.NormalizeSection(_barrelHeat, 5f, 9f) * 3f + Rando.Float(0.25f);
		if (_barrelHeat > 10f)
		{
			_barrelHeat = 10f;
		}
		_barrelHeat -= 0.005f;
		if (_barrelHeat < 0f)
		{
			_barrelHeat = 0f;
		}
		if (!burntOut)
		{
			_sprite.speed = _spin;
			_tip.speed = _spin;
			if (_spin > 0f)
			{
				_spin -= 0.01f;
			}
			else
			{
				_spin = 0f;
			}
			spinAmount += _spin;
			barrelInsertOffset = new Vec2(0f, 2f + (float)Math.Sin(spinAmount / 9f * 3.14f) * 2f);
		}
		base.Update();
		if (_topBullet != null)
		{
			if (!graphic.flipH)
			{
				_topBullet.chainOffset = new Vec2(1f, 5f);
			}
			else
			{
				_topBullet.chainOffset = new Vec2(-1f, 5f);
			}
		}
	}

	public override void Fire()
	{
		if (burnt >= 1f || burntOut)
		{
			SFX.Play("dartStick", 0.5f, -0.1f + Rando.Float(0.2f));
		}
		else
		{
			base.Fire();
		}
	}

	protected override bool OnBurn(Vec2 firePosition, Thing litBy)
	{
		if (!base.onFire)
		{
			SFX.Play("ignite", 1f, -0.3f + Rando.Float(0.3f));
		}
		base.onFire = true;
		return true;
	}

	protected override void PlayFireSound()
	{
		SFX.PlaySynchronized("dartGunFire", 0.7f, -0.1f + Rando.Float(0.2f));
	}

	public override void Draw()
	{
		Material m = Graphics.material;
		if (burntOut)
		{
			graphic = _burned;
			base.Draw();
		}
		else
		{
			base.Draw();
			Graphics.material = base.material;
			_tip.flipH = graphic.flipH;
			_tip.center = graphic.center;
			_tip.depth = base.depth + 1;
			_tip.alpha = Math.Min(_barrelHeat * 1.5f / 10f, 1f);
			_tip.angle = angle;
			Graphics.Draw(_tip, base.x, base.y);
			Graphics.material = m;
		}
		if (_topBullet != null)
		{
			_topBullet.material = base.material;
			_topBullet.DoDraw();
		}
	}
}
