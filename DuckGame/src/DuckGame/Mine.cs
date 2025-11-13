using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Guns|Explosives")]
[BaggedProperty("isFatal", false)]
public class Mine : Gun
{
	public StateBinding _mineBinding = new MineFlagBinding();

	private SpriteMap _sprite;

	public bool _pin = true;

	public bool blownUp;

	public float _timer = 1.2f;

	public bool _armed;

	public bool _clicked;

	public float addWeight;

	public int _framesSinceArm;

	public float _holdingWeight;

	public bool _thrown;

	private Sprite _mineFlash;

	private Dictionary<Duck, float> _ducksOnMine = new Dictionary<Duck, float>();

	public List<PhysicsObject> previousThings = new List<PhysicsObject>();

	private float prevAngle;

	public Duck duckWhoThrew;

	public bool pin => _pin;

	public Dictionary<Duck, float> ducksOnMine => _ducksOnMine;

	public Mine(float xval, float yval)
		: base(xval, yval)
	{
		ammo = 1;
		_ammoType = new ATShrapnel();
		_type = "gun";
		_sprite = new SpriteMap("mine", 18, 16);
		_sprite.AddAnimation("pickup", 1f, true, default(int));
		_sprite.AddAnimation("idle", 0.05f, true, 1, 2);
		_sprite.SetAnimation("pickup");
		graphic = _sprite;
		center = new Vec2(9f, 8f);
		collisionOffset = new Vec2(-5f, -5f);
		collisionSize = new Vec2(10f, 9f);
		_mineFlash = new Sprite("mineFlash");
		_mineFlash.CenterOrigin();
		_mineFlash.alpha = 0f;
		base.bouncy = 0f;
		friction = 0.2f;
		editorTooltip = "Once placed in position, explodes if any curious Ducks walk by.";
	}

	public void Arm()
	{
		if (_armed)
		{
			return;
		}
		_holdingWeight = 0f;
		_armed = true;
		if (base.isServerForObject)
		{
			if (Network.isActive)
			{
				NetSoundEffect.Play("minePullPin");
			}
			else
			{
				SFX.Play("pullPin");
			}
		}
	}

	protected override bool OnDestroy(DestroyType type = null)
	{
		if (!_pin)
		{
			BlowUp();
			return true;
		}
		return false;
	}

	public void UpdatePinState()
	{
		if (!_pin)
		{
			canPickUp = false;
			_sprite.SetAnimation("idle");
			collisionOffset = new Vec2(-6f, -2f);
			collisionSize = new Vec2(12f, 3f);
			base.depth = 0.8f;
			_hasOldDepth = false;
			thickness = 1f;
			center = new Vec2(9f, 14f);
		}
		else
		{
			canPickUp = true;
			_sprite.SetAnimation("pickup");
			collisionOffset = new Vec2(-5f, -4f);
			collisionSize = new Vec2(10f, 8f);
			thickness = -1f;
		}
	}

	public override void Update()
	{
		if (!pin)
		{
			collisionOffset = new Vec2(-6f, -2f);
			collisionSize = new Vec2(12f, 3f);
		}
		base.Update();
		if (!pin && Math.Abs(prevAngle - angle) > 0.1f)
		{
			Vec2 colSizeWide = new Vec2(14f, 3f);
			Vec2 colOffsetWide = new Vec2(-7f, -2f);
			Vec2 colSizeTall = new Vec2(4f, 14f);
			Vec2 colOffsetTall = new Vec2(-2f, -7f);
			float norm = (float)Math.Abs(Math.Sin(angle));
			collisionSize = colSizeWide * (1f - norm) + colSizeTall * norm;
			collisionOffset = colOffsetWide * (1f - norm) + colOffsetTall * norm;
			prevAngle = angle;
		}
		UpdatePinState();
		if (_sprite.imageIndex == 2)
		{
			_mineFlash.alpha = Lerp.Float(_mineFlash.alpha, 0.4f, 0.08f);
		}
		else
		{
			_mineFlash.alpha = Lerp.Float(_mineFlash.alpha, 0f, 0.08f);
		}
		if (_armed)
		{
			_sprite.speed = 2f;
		}
		if (_thrown && owner == null)
		{
			_thrown = false;
			if (Math.Abs(hSpeed) + Math.Abs(vSpeed) > 0.4f)
			{
				base.angleDegrees = 180f;
			}
		}
		if (_armed)
		{
			_framesSinceArm++;
		}
		if (!_pin && _grounded && (!_armed || _framesSinceArm > 4))
		{
			canPickUp = false;
			float holdWeight = addWeight;
			IEnumerable<PhysicsObject> col = Level.CheckLineAll<PhysicsObject>(new Vec2(base.x - 6f, base.y - 3f), new Vec2(base.x + 6f, base.y - 3f));
			List<Duck> ducks = new List<Duck>();
			Duck stepDuck = null;
			bool hadServerThing = false;
			bool removedServerThing = false;
			foreach (PhysicsObject t in previousThings)
			{
				if (t.isServerForObject)
				{
					hadServerThing = true;
				}
				bool has = false;
				foreach (PhysicsObject item in col)
				{
					if (item == t)
					{
						has = true;
						break;
					}
				}
				if (!has && t.isServerForObject)
				{
					removedServerThing = true;
				}
			}
			previousThings.Clear();
			foreach (PhysicsObject o in col)
			{
				if (o == this || o.owner != null || (o is Holdable && (!(o as Holdable).canPickUp || (o as Holdable).hoverSpawner != null)) || Math.Abs(o.bottom - base.bottom) > 6f)
				{
					continue;
				}
				if (o.isServerForObject)
				{
					hadServerThing = true;
				}
				previousThings.Add(o);
				if (o is Duck || o is TrappedDuck || o is RagdollPart)
				{
					holdWeight += 5f;
					Duck d = o as Duck;
					if (o is TrappedDuck)
					{
						d = (o as TrappedDuck).captureDuck;
					}
					else if (o is RagdollPart && (o as RagdollPart).doll != null)
					{
						d = (o as RagdollPart).doll.captureDuck;
					}
					if (d != null)
					{
						stepDuck = d;
						if (!_ducksOnMine.ContainsKey(d))
						{
							_ducksOnMine[d] = 0f;
						}
						_ducksOnMine[d] += Maths.IncFrameTimer();
						ducks.Add(d);
					}
				}
				else
				{
					holdWeight += o.weight;
				}
				foreach (PhysicsObject previousThing in previousThings)
				{
					_ = previousThing;
				}
			}
			List<Duck> remove = new List<Duck>();
			foreach (KeyValuePair<Duck, float> pair in _ducksOnMine)
			{
				if (!ducks.Contains(pair.Key))
				{
					remove.Add(pair.Key);
				}
				else
				{
					pair.Key.profile.stats.timeSpentOnMines += Maths.IncFrameTimer();
				}
			}
			foreach (Duck d2 in remove)
			{
				_ducksOnMine.Remove(d2);
			}
			if (holdWeight < _holdingWeight && hadServerThing && removedServerThing)
			{
				Thing.Fondle(this, DuckNetwork.localConnection);
				if (!_armed)
				{
					Arm();
				}
				else
				{
					_timer = -1f;
				}
			}
			if (_armed && holdWeight > _holdingWeight)
			{
				if (!_clicked && stepDuck != null)
				{
					stepDuck.profile.stats.minesSteppedOn++;
				}
				_clicked = true;
				SFX.Play("doubleBeep");
			}
			_holdingWeight = holdWeight;
		}
		if (_timer < 0f && base.isServerForObject)
		{
			_timer = 1f;
			BlowUp();
		}
		addWeight = 0f;
	}

	public void BlowUp()
	{
		if (blownUp)
		{
			return;
		}
		MakeBlowUpHappen(position);
		blownUp = true;
		if (!base.isServerForObject)
		{
			return;
		}
		foreach (PhysicsObject p in Level.CheckCircleAll<PhysicsObject>(position, 22f))
		{
			if (p != this)
			{
				Vec2 dir = p.position - position;
				float mul = 1f - Math.Min(dir.length, 22f) / 22f;
				float len = mul * 4f;
				dir.Normalize();
				p.hSpeed += len * dir.x;
				p.vSpeed += -5f * mul;
				p.sleeping = false;
				Fondle(p);
			}
		}
		float cx = position.x;
		float cy = position.y;
		for (int i = 0; i < 20; i++)
		{
			float dir2 = (float)i * 18f - 5f + Rando.Float(10f);
			ATShrapnel shrap = new ATShrapnel();
			shrap.range = 60f + Rando.Float(18f);
			Bullet bullet = new Bullet(cx, cy, shrap, dir2);
			bullet.firedFrom = this;
			firedBullets.Add(bullet);
			Level.Add(bullet);
		}
		bulletFireIndex += 20;
		if (Network.isActive && base.isServerForObject)
		{
			Send.Message(new NMFireGun(this, firedBullets, bulletFireIndex, rel: false, 4), NetMessagePriority.ReliableOrdered);
			firedBullets.Clear();
		}
		if (Recorder.currentRecording != null)
		{
			Recorder.currentRecording.LogBonus();
		}
		Level.Remove(this);
	}

	public void MakeBlowUpHappen(Vec2 pos)
	{
		if (!blownUp)
		{
			blownUp = true;
			SFX.Play("explode");
			RumbleManager.AddRumbleEvent(pos, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
			Graphics.FlashScreen();
			float cx = pos.x;
			float cy = pos.y;
			Level.Add(new ExplosionPart(cx, cy));
			int num = 6;
			if (Graphics.effectsLevel < 2)
			{
				num = 3;
			}
			for (int i = 0; i < num; i++)
			{
				float dir = (float)i * 60f + Rando.Float(-10f, 10f);
				float dist = Rando.Float(12f, 20f);
				Level.Add(new ExplosionPart(cx + (float)(Math.Cos(Maths.DegToRad(dir)) * (double)dist), cy - (float)(Math.Sin(Maths.DegToRad(dir)) * (double)dist)));
			}
		}
	}

	public override void OnNetworkBulletsFired(Vec2 pos)
	{
		MakeBlowUpHappen(pos);
		base.OnNetworkBulletsFired(pos);
	}

	public override bool Hit(Bullet bullet, Vec2 hitPos)
	{
		if (bullet.isLocal && owner == null && !canPickUp && _timer > 0f)
		{
			Thing.Fondle(this, DuckNetwork.localConnection);
			BlowUp();
		}
		return false;
	}

	public override void Draw()
	{
		Material obj = Graphics.material;
		Graphics.material = null;
		if (_mineFlash.alpha > 0.01f)
		{
			Graphics.Draw(_mineFlash, base.x, base.y - 3f);
		}
		Graphics.material = obj;
		base.Draw();
	}

	public override void OnPressAction()
	{
		if (!base.isServerForObject)
		{
			return;
		}
		if (owner == null)
		{
			_pin = false;
			if (heat > 0.5f)
			{
				BlowUp();
			}
		}
		if (_pin)
		{
			_pin = false;
			UpdatePinState();
			if (owner is Duck duckOwner)
			{
				duckWhoThrew = duckOwner;
				_holdingWeight = 5f;
				duckOwner.doThrow = true;
				_responsibleProfile = duckOwner.profile;
			}
			else
			{
				Arm();
			}
			_thrown = true;
		}
	}
}
