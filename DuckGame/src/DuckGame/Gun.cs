using System;
using System.Collections.Generic;

namespace DuckGame;

public abstract class Gun : Holdable
{
	protected AmmoType _ammoType;

	public StateBinding _ammoBinding = new StateBinding("netAmmo");

	public StateBinding _waitBinding = new StateBinding("_wait");

	public StateBinding _loadedBinding = new StateBinding("loaded");

	public StateBinding _bulletFireIndexBinding = new StateBinding("bulletFireIndex");

	public StateBinding _infiniteAmmoValBinding = new StateBinding("infiniteAmmoVal");

	public byte bulletFireIndex;

	public bool wideBarrel;

	public Vec2 barrelInsertOffset;

	public float kick;

	protected float _kickForce = 3f;

	protected RumbleIntensity _fireRumble;

	public int ammo;

	public bool firing;

	public bool tamping;

	public float tampPos;

	public float loseAccuracy;

	public float _accuracyLost;

	public float maxAccuracyLost;

	protected Color _bulletColor = Color.White;

	protected bool _lowerOnFire = true;

	public bool receivingPress;

	public bool hasFireEvents;

	public bool onlyFireAction;

	protected float _fireSoundPitch;

	public EditorProperty<bool> infinite;

	public bool infiniteAmmoVal;

	protected string _bio = "No Info.";

	protected Vec2 _barrelOffsetTL;

	protected Vec2 _laserOffsetTL;

	protected float _barrelAngleOffset;

	public bool plugged;

	protected string _fireSound = "pistol";

	protected string _clickSound = "click";

	public float _wait;

	public float _fireWait = 1f;

	public bool loaded = true;

	private Sprite _bayonetSprite;

	private Sprite _tapeSprite;

	private bool _laserInit;

	protected bool _fullAuto;

	protected int _numBulletsPerFire = 1;

	protected bool _manualLoad;

	public bool laserSight;

	protected SpriteMap _flare;

	protected float _flareAlpha;

	private SpriteMap _barrelSmoke;

	public float _barrelHeat;

	protected float _smokeWait;

	protected float _smokeAngle;

	protected float _smokeFlatten;

	private SinWave _accuracyWave = 0.3f;

	private SpriteMap _clickPuff;

	private Tex2D _laserTex;

	public bool isFatal = true;

	protected Vec2 _wallPoint;

	protected Sprite _sightHit;

	private bool _doPuff;

	public byte _framesSinceThrown;

	public bool explode;

	public List<Bullet> firedBullets = new List<Bullet>();

	private Material _additiveMaterial;

	public AmmoType ammoType => _ammoType;

	public sbyte netAmmo
	{
		get
		{
			return (sbyte)ammo;
		}
		set
		{
			ammo = value;
		}
	}

	public bool lowerOnFire => _lowerOnFire;

	public string bio => _bio;

	public Vec2 barrelPosition => Offset(barrelOffset);

	public Vec2 barrelOffset => _barrelOffsetTL - center + _extraOffset;

	public Vec2 laserOffset => _laserOffsetTL - center;

	public Vec2 barrelVector => Offset(barrelOffset) - Offset(barrelOffset + new Vec2(-1f, 0f));

	public override float angle
	{
		get
		{
			return _angle + (float)_accuracyWave * (_accuracyLost * 0.5f);
		}
		set
		{
			_angle = value;
		}
	}

	public float barrelAngleOffset => _barrelAngleOffset;

	public float barrelAngle => Maths.DegToRad(Maths.PointDirection(Vec2.Zero, barrelVector) + _barrelAngleOffset * (float)offDir);

	public bool fullAuto => _fullAuto;

	public bool CanSpawnInfinite()
	{
		if (!(this is FlareGun) && !(this is QuadLaser) && !(this is RomanCandle) && !(this is Matchbox) && !(this is FireCrackers))
		{
			return !(this is NetGun);
		}
		return false;
	}

	public bool CanSpin()
	{
		return weight <= 5f;
	}

	public override void EditorPropertyChanged(object property)
	{
		infiniteAmmoVal = infinite.value;
		UpdateMaterial();
	}

	public virtual void OnNetworkBulletsFired(Vec2 pos)
	{
	}

	public override void UpdateMaterial()
	{
		if (infinite.value)
		{
			infiniteAmmoVal = true;
		}
		if (infiniteAmmoVal)
		{
			if (base.material == null)
			{
				base.material = new MaterialGold(this);
			}
		}
		else
		{
			base.UpdateMaterial();
		}
	}

	public Gun(float xval, float yval)
		: base(xval, yval)
	{
		_flare = new SpriteMap("smallFlare", 11, 10);
		_flare.center = new Vec2(0f, 5f);
		_barrelSmoke = new SpriteMap("barrelSmoke", 8, 8);
		_barrelSmoke.center = new Vec2(1f, 8f);
		_barrelSmoke.ClearAnimations();
		_barrelSmoke.AddAnimation("puff", 1f, false, 0, 1, 2);
		_barrelSmoke.AddAnimation("loop", 1f, true, 3, 4, 5, 6, 7, 8);
		_barrelSmoke.AddAnimation("finish", 1f, false, 9, 10, 11, 12);
		_barrelSmoke.SetAnimation("puff");
		_barrelSmoke.speed = 0f;
		_translucent = true;
		physicsMaterial = PhysicsMaterial.Metal;
		_dontCrush = true;
		_clickPuff = new SpriteMap("clickPuff", 16, 16);
		_clickPuff.AddAnimation("puff", 0.3f, false, 0, 1, 2, 3);
		_clickPuff.center = new Vec2(0f, 12f);
		_sightHit = new Sprite("laserSightHit");
		_sightHit.CenterOrigin();
		base.depth = -0.1f;
		infinite = new EditorProperty<bool>(val: false, this);
		infinite._tooltip = "Makes gun have infinite ammo.";
		base.collideSounds.Add("smallMetalCollide");
		base.impactVolume = 0.3f;
		holsterAngle = 90f;
		coolingFactor = 0.002f;
	}

	public void DoAmmoClick()
	{
		_doPuff = true;
		_clickPuff.frame = 0;
		_clickPuff.SetAnimation("puff");
		_barrelHeat = 0f;
		_barrelSmoke.SetAnimation("finish");
		SFX.Play(_clickSound);
		for (int i = 0; i < 2; i++)
		{
			SmallSmoke smallSmoke = SmallSmoke.New(barrelPosition.x, barrelPosition.y);
			smallSmoke.scale = new Vec2(0.3f, 0.3f);
			smallSmoke.hSpeed = Rando.Float(-0.1f, 0.1f);
			smallSmoke.vSpeed = 0f - Rando.Float(0.05f, 0.2f);
			smallSmoke.alpha = 0.6f;
			Level.Add(smallSmoke);
		}
	}

	public override void HeatUp(Vec2 location)
	{
		if (_ammoType != null && _ammoType.combustable && ammo > 0 && heat > 1f && Rando.Float(1f) > 0.8f)
		{
			byte num = bulletFireIndex;
			heat -= 0.05f;
			PressAction();
			if (num != bulletFireIndex && Rando.Float(1f) > 0.4f)
			{
				SFX.Play("bulletPop", Rando.Float(0.5f, 1f), Rando.Float(-1f, 1f));
			}
		}
	}

	public override void DoUpdate()
	{
		if (laserSight && _laserTex == null)
		{
			_additiveMaterial = new Material("Shaders/basicAdd");
			_laserTex = Content.Load<Tex2D>("pointerLaser");
		}
		base.DoUpdate();
	}

	public override void Update()
	{
		if (infiniteAmmoVal)
		{
			ammo = 99;
		}
		if (TeamSelect2.Enabled("INFAMMO"))
		{
			infinite.value = true;
			infiniteAmmoVal = true;
		}
		base.Update();
		if (_clickPuff.finished)
		{
			_doPuff = false;
		}
		_accuracyLost = Maths.CountDown(_accuracyLost, 0.015f);
		if (_flareAlpha > 0f)
		{
			_flareAlpha -= 0.5f;
		}
		else
		{
			_flareAlpha = 0f;
		}
		if (_barrelHeat > 0f)
		{
			_barrelHeat -= 0.01f;
		}
		else
		{
			_barrelHeat = 0f;
		}
		if (_barrelHeat > 10f)
		{
			_barrelHeat = 10f;
		}
		if (_smokeWait > 0f)
		{
			_smokeWait -= 0.1f;
		}
		else
		{
			if (_barrelHeat > 0.1f && _barrelSmoke.speed == 0f)
			{
				_barrelSmoke.SetAnimation("puff");
				_barrelSmoke.speed = 0.1f;
			}
			if (_barrelSmoke.speed > 0f && _barrelSmoke.currentAnimation == "puff" && _barrelSmoke.finished)
			{
				_barrelSmoke.SetAnimation("loop");
			}
			if (_barrelSmoke.speed > 0f && _barrelSmoke.currentAnimation == "loop" && _barrelSmoke.frame == 5 && _barrelHeat < 0.1f)
			{
				_barrelSmoke.SetAnimation("finish");
			}
		}
		if (_smokeWait > 0f && _barrelSmoke.speed > 0f)
		{
			_barrelSmoke.SetAnimation("finish");
		}
		if (_barrelSmoke.currentAnimation == "finish" && _barrelSmoke.finished)
		{
			_barrelSmoke.speed = 0f;
		}
		if (owner != null)
		{
			if (owner.hSpeed > 0.1f)
			{
				_smokeAngle -= 0.1f;
			}
			else if (owner.hSpeed < -0.1f)
			{
				_smokeAngle += 0.1f;
			}
			if (_smokeAngle > 0.4f)
			{
				_smokeAngle = 0.4f;
			}
			if (_smokeAngle < -0.4f)
			{
				_smokeAngle = -0.4f;
			}
			if (owner.vSpeed > 0.1f)
			{
				_smokeFlatten -= 0.1f;
			}
			else if (owner.vSpeed < -0.1f)
			{
				_smokeFlatten += 0.1f;
			}
			if (_smokeFlatten > 0.5f)
			{
				_smokeFlatten = 0.5f;
			}
			if (_smokeFlatten < -0.5f)
			{
				_smokeFlatten = -0.5f;
			}
			_framesSinceThrown = 0;
		}
		else
		{
			_framesSinceThrown++;
			if (_framesSinceThrown > 25)
			{
				_framesSinceThrown = 25;
			}
		}
		if (!(this is Sword) && owner == null && CanSpin() && Level.current.simulatePhysics)
		{
			bool spinning = false;
			bool againstWall = false;
			if ((Math.Abs(hSpeed) + Math.Abs(vSpeed) > 2f || !base.grounded) && gravMultiplier > 0f && !againstWall && !_grounded)
			{
				if (offDir > 0)
				{
					base.angleDegrees += (Math.Abs(hSpeed * 2f) + Math.Abs(vSpeed)) * 1f + 5f;
				}
				else
				{
					base.angleDegrees -= (Math.Abs(hSpeed * 2f) + Math.Abs(vSpeed)) * 1f + 5f;
				}
				spinning = true;
			}
			if (!spinning || againstWall)
			{
				base.angleDegrees %= 360f;
				if (base.angleDegrees < 0f)
				{
					base.angleDegrees += 360f;
				}
				if (againstWall)
				{
					if (Math.Abs(base.angleDegrees - 90f) < Math.Abs(base.angleDegrees + 90f))
					{
						base.angleDegrees = Lerp.Float(base.angleDegrees, 90f, 16f);
					}
					else
					{
						base.angleDegrees = Lerp.Float(-90f, 0f, 16f);
					}
				}
				else if (base.angleDegrees > 90f && base.angleDegrees < 270f)
				{
					base.angleDegrees = Lerp.Float(base.angleDegrees, 180f, 14f);
				}
				else
				{
					if (base.angleDegrees > 180f)
					{
						base.angleDegrees -= 360f;
					}
					else if (base.angleDegrees < -180f)
					{
						base.angleDegrees += 360f;
					}
					base.angleDegrees = Lerp.Float(base.angleDegrees, 0f, 14f);
				}
			}
		}
		float val = 1f - ((float)Math.Sin(Maths.DegToRad(base.angleDegrees + 90f)) + 1f) / 2f;
		if (_owner == null)
		{
			_extraOffset.y = val * (_collisionOffset.y + _collisionSize.y + _collisionOffset.y);
		}
		else
		{
			_extraOffset.y = 0f;
		}
		if (owner == null || (owner.hSpeed > -0.1f && owner.hSpeed < 0.1f))
		{
			if (_smokeAngle >= 0.1f)
			{
				_smokeAngle -= 0.1f;
			}
			else if (_smokeAngle <= -0.1f)
			{
				_smokeAngle += 0.1f;
			}
			else
			{
				_smokeAngle = 0f;
			}
		}
		if (owner == null || (owner.vSpeed > -0.1f && owner.vSpeed < 0.1f))
		{
			if (_smokeFlatten >= 0.1f)
			{
				_smokeFlatten -= 0.1f;
			}
			else if (_smokeFlatten <= -0.1f)
			{
				_smokeFlatten += 0.1f;
			}
			else
			{
				_smokeFlatten = 0f;
			}
		}
		if (kick > 0f)
		{
			kick -= 0.2f;
		}
		else
		{
			kick = 0f;
		}
		if (owner == null)
		{
			if (ammo <= 0 && (base.alpha < 0.99f || (base.grounded && Math.Abs(hSpeed) + Math.Abs(vSpeed) < 0.3f)))
			{
				canPickUp = false;
				base.alpha -= 10.2f;
				weight = 0.01f;
			}
			if ((double)base.alpha < 0.0)
			{
				Level.Remove(this);
			}
		}
		if (owner != null && owner.graphic != null)
		{
			graphic.flipH = owner.graphic.flipH;
		}
		if (_wait > 0f)
		{
			_wait -= 0.15f;
		}
		if (_wait < 0f)
		{
			_wait = 0f;
		}
	}

	public override void Terminate()
	{
		if (!(Level.current is Editor))
		{
			Level.Add(SmallSmoke.New(base.x, base.y));
			Level.Add(SmallSmoke.New(base.x + 4f, base.y));
			Level.Add(SmallSmoke.New(base.x - 4f, base.y));
			Level.Add(SmallSmoke.New(base.x, base.y + 4f));
			Level.Add(SmallSmoke.New(base.x, base.y - 4f));
		}
		base.Terminate();
	}

	public override void PressAction()
	{
		if (base.isServerForObject && ((TeamSelect2.Enabled("GUNEXPL") && ammo <= 0) || explode))
		{
			if (base.duck != null)
			{
				if (this is Warpgun)
				{
					base.duck.Kill(new DTImpale(this));
				}
				else
				{
					base.duck.ThrowItem();
				}
				Level.Remove(this);
				for (int repeat = 0; repeat < 1; repeat++)
				{
					ExplosionPart explosionPart = new ExplosionPart(base.x - 8f + Rando.Float(16f), base.y - 8f + Rando.Float(16f));
					explosionPart.xscale *= 0.7f;
					explosionPart.yscale *= 0.7f;
					Level.Add(explosionPart);
				}
				SFX.Play("explode");
				List<Bullet> firedBullets = new List<Bullet>();
				for (int i = 0; i < 12; i++)
				{
					float dir = (float)i * 30f - 10f + Rando.Float(20f);
					ATShrapnel shrap = new ATShrapnel();
					shrap.range = 25f + Rando.Float(10f);
					Bullet bullet = new Bullet(base.x + (float)(Math.Cos(Maths.DegToRad(dir)) * 8.0), base.y - (float)(Math.Sin(Maths.DegToRad(dir)) * 8.0), shrap, dir);
					bullet.firedFrom = this;
					firedBullets.Add(bullet);
					Level.Add(bullet);
				}
				if (Network.isActive)
				{
					Send.Message(new NMExplodingProp(firedBullets), NetMessagePriority.ReliableOrdered);
					firedBullets.Clear();
				}
			}
		}
		else
		{
			base.PressAction();
		}
	}

	public override void OnPressAction()
	{
		if (!_fullAuto)
		{
			if (base.isServerForObject)
			{
				_fireActivated = true;
			}
			Fire();
		}
	}

	public override void OnHoldAction()
	{
		if (_fullAuto)
		{
			Fire();
		}
	}

	public virtual void ApplyKick()
	{
		if (owner == null || !base.isServerForObject)
		{
			return;
		}
		if (_kickForce != 0f)
		{
			Duck duckOwner = owner as Duck;
			Thing kick = owner;
			if (duckOwner != null && duckOwner._trapped != null)
			{
				kick = duckOwner._trapped;
			}
			if (duckOwner != null && duckOwner.ragdoll != null && duckOwner.ragdoll.part2 != null && duckOwner.ragdoll.part1 != null && duckOwner.ragdoll.part3 != null)
			{
				Vec2 dir = -barrelVector * (_kickForce / 2f);
				duckOwner.ragdoll.part1.hSpeed += dir.x;
				duckOwner.ragdoll.part1.vSpeed += dir.y;
				duckOwner.ragdoll.part2.hSpeed += dir.x;
				duckOwner.ragdoll.part2.vSpeed += dir.y;
				duckOwner.ragdoll.part3.hSpeed += dir.x;
				duckOwner.ragdoll.part3.vSpeed += dir.y;
			}
			else
			{
				Vec2 dir2 = -barrelVector * _kickForce;
				if (Math.Sign(kick.hSpeed) != Math.Sign(dir2.x) || Math.Abs(dir2.x) > Math.Abs(kick.hSpeed))
				{
					kick.hSpeed = dir2.x;
				}
				if (duckOwner != null)
				{
					if (duckOwner.crouch)
					{
						duckOwner.sliding = true;
					}
					kick.vSpeed += dir2.y - _kickForce * 0.333f;
				}
				else
				{
					kick.vSpeed += dir2.y - _kickForce * 0.333f;
				}
			}
		}
		this.kick = 1f;
	}

	public virtual void Fire()
	{
		if (!loaded)
		{
			return;
		}
		if (ammo > 0 && _wait == 0f)
		{
			firedBullets.Clear();
			if (base.duck != null)
			{
				RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(_fireRumble, RumbleDuration.Pulse, RumbleFalloff.None));
			}
			ApplyKick();
			for (int i = 0; i < _numBulletsPerFire; i++)
			{
				float accuracy = _ammoType.accuracy;
				_ammoType.accuracy *= 1f - _accuracyLost;
				_ammoType.bulletColor = _bulletColor;
				float shootAngle = base.angleDegrees;
				if (offDir < 0)
				{
					shootAngle += 180f;
					shootAngle -= _ammoType.barrelAngleDegrees;
				}
				else
				{
					shootAngle += _ammoType.barrelAngleDegrees;
				}
				if (!receivingPress)
				{
					if (_ammoType is ATDart)
					{
						if (base.isServerForObject)
						{
							Vec2 pos = Offset(barrelOffset);
							Dart d = new Dart(pos.x, pos.y, owner as Duck, 0f - shootAngle);
							Fondle(d);
							if (base.onFire || _barrelHeat > 6f)
							{
								Level.Add(SmallFire.New(0f, 0f, 0f, 0f, shortLife: false, d, canMultiply: true, this));
								d.burning = true;
								d.onFire = true;
								Burn(position, this);
							}
							Vec2 travelDir = Maths.AngleToVec(Maths.DegToRad(0f - shootAngle));
							d.hSpeed = travelDir.x * 10f;
							d.vSpeed = travelDir.y * 10f;
							d.vSpeed -= Rando.Float(2f);
							Level.Add(d);
						}
					}
					else
					{
						Bullet b = _ammoType.FireBullet(Offset(barrelOffset), owner, shootAngle, this);
						if (Network.isActive && base.isServerForObject)
						{
							firedBullets.Add(b);
							if (base.duck != null && base.duck.profile.connection != null)
							{
								b.connection = base.duck.profile.connection;
							}
						}
						if (base.isServerForObject && (this is LaserRifle || this is PewPewLaser || this is Phaser))
						{
							Global.data.laserBulletsFired.valueInt++;
						}
					}
				}
				bulletFireIndex++;
				_ammoType.accuracy = accuracy;
				_barrelHeat += 0.3f;
			}
			_smokeWait = 3f;
			loaded = false;
			_flareAlpha = 1.5f;
			if (!_manualLoad)
			{
				Reload();
			}
			firing = true;
			_wait = _fireWait;
			PlayFireSound();
			if (owner == null)
			{
				Vec2 fly = barrelVector * Rando.Float(1f, 3f);
				fly.y += Rando.Float(2f);
				hSpeed -= fly.x;
				vSpeed -= fly.y;
			}
			_accuracyLost += loseAccuracy;
			if (_accuracyLost > maxAccuracyLost)
			{
				_accuracyLost = maxAccuracyLost;
			}
		}
		else if (ammo <= 0 && _wait == 0f)
		{
			firedBullets.Clear();
			DoAmmoClick();
			_wait = _fireWait;
		}
	}

	protected virtual void PlayFireSound()
	{
		SFX.Play(_fireSound, 1f, -0.1f + Rando.Float(0.2f) + _fireSoundPitch);
	}

	public void PopShell(bool isMessage = false)
	{
		if ((base.isServerForObject || isMessage) && _ammoType != null)
		{
			_ammoType.PopShell(base.x, base.y, -offDir);
			if (!isMessage)
			{
				Send.Message(new NMPopShell(this), NetMessagePriority.UnreliableUnordered);
			}
		}
	}

	public virtual void Reload(bool shell = true)
	{
		if (ammo != 0)
		{
			if (shell)
			{
				PopShell();
			}
			ammo--;
		}
		loaded = true;
	}

	public override void Draw()
	{
		if (laserSight && base.held)
		{
			ATTracer tracer = new ATTracer();
			tracer.range = 2000f;
			float a = base.angleDegrees;
			a *= -1f;
			if (offDir < 0)
			{
				a += 180f;
			}
			Vec2 pos = Offset(laserOffset);
			tracer.penetration = 0.4f;
			Bullet b = new Bullet(pos.x, pos.y, tracer, a, owner, rbound: false, -1f, tracer: true);
			_wallPoint = b.end;
			_laserInit = true;
		}
		Material obj = Graphics.material;
		if (graphic != null)
		{
			if (owner != null && owner.graphic != null)
			{
				graphic.flipH = owner.graphic.flipH;
			}
			else
			{
				graphic.flipH = offDir <= 0;
			}
		}
		if (_doPuff)
		{
			_clickPuff.alpha = 0.6f;
			_clickPuff.angle = angle + _smokeAngle;
			_clickPuff.flipH = offDir < 0;
			Draw(_clickPuff, barrelOffset);
		}
		if (!VirtualTransition.active && Graphics.material == null)
		{
			Graphics.material = base.material;
		}
		base.Draw();
		Graphics.material = null;
		if (_flareAlpha > 0f)
		{
			Draw(_flare, barrelOffset);
		}
		if (_barrelSmoke.speed > 0f && !base.raised)
		{
			_barrelSmoke.alpha = 0.7f;
			_barrelSmoke.angle = _smokeAngle;
			_barrelSmoke.flipH = offDir < 0;
			if (offDir > 0 && base.angleDegrees > 90f && base.angleDegrees < 270f)
			{
				_barrelSmoke.flipH = true;
			}
			if (offDir < 0 && base.angleDegrees > 90f && base.angleDegrees < 270f)
			{
				_barrelSmoke.flipH = false;
			}
			_barrelSmoke.yscale = 1f - _smokeFlatten;
			DrawIgnoreAngle(_barrelSmoke, barrelOffset);
		}
		if (!Options.Data.fireGlow)
		{
			DrawGlow();
		}
		Graphics.material = obj;
		_ = DevConsole.showCollision;
	}

	public override void DrawGlow()
	{
		if (laserSight && base.held && _laserTex != null && _laserInit)
		{
			float alpha = 1f;
			if (!Options.Data.fireGlow)
			{
				alpha = 0.4f;
			}
			Vec2 startPos = Offset(laserOffset);
			float length = (startPos - _wallPoint).length;
			float range = 100f;
			if (ammoType != null)
			{
				range = ammoType.range;
			}
			Vec2 travel = (_wallPoint - startPos).normalized;
			Vec2 endPos = startPos + travel * Math.Min(range, length);
			Graphics.DrawTexturedLine(_laserTex, startPos, endPos, Color.Red * alpha, 0.5f, base.depth - 1);
			if (length > range)
			{
				for (int i = 1; i < 4; i++)
				{
					Graphics.DrawTexturedLine(_laserTex, endPos, endPos + travel * 2f, Color.Red * (1f - (float)i * 0.2f) * alpha, 0.5f, base.depth - 1);
					endPos += travel * 2f;
				}
			}
			if (_sightHit != null && length < range)
			{
				_sightHit.alpha = alpha;
				_sightHit.color = Color.Red * alpha;
				Graphics.Draw(_sightHit, _wallPoint.x, _wallPoint.y);
			}
		}
		base.DrawGlow();
	}
}
