using System;
using System.Collections.Generic;

namespace DuckGame;

[BaggedProperty("canSpawn", false)]
public class RCCar : Holdable, IPlatform
{
	public StateBinding _controllerBinding = new StateBinding("_controller");

	public StateBinding _signalBinding = new StateBinding("receivingSignal");

	public StateBinding _idleSpeedBinding = new CompressedFloatBinding("_idleSpeed", 1f, 4);

	private SpriteMap _sprite;

	private float _tilt;

	private float _maxSpeed = 6f;

	private SinWaveManualUpdate _wave = new SinWaveManualUpdate(0.1f);

	private float _waveMult;

	private Sprite _wheel;

	public bool moveLeft;

	public bool moveRight;

	public bool jump;

	private bool _receivingSignal;

	private int _inc;

	public float _idleSpeed;

	public RCController _controller;

	private ConstantSound _idle = new ConstantSound("rcDrive");

	public bool receivingSignal
	{
		get
		{
			return _receivingSignal;
		}
		set
		{
			if (_receivingSignal != value && !destroyed)
			{
				if (value)
				{
					SFX.Play("rcConnect", 0.5f);
				}
				else
				{
					SFX.Play("rcDisconnect", 0.5f);
				}
			}
			_receivingSignal = value;
		}
	}

	public RCCar(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_sprite = new SpriteMap("rcBody", 32, 32);
		_sprite.AddAnimation("idle", 1f, true, default(int));
		_sprite.AddAnimation("beep", 0.2f, true, 0, 1);
		graphic = _sprite;
		center = new Vec2(16f, 24f);
		collisionOffset = new Vec2(-8f, 0f);
		collisionSize = new Vec2(16f, 11f);
		base.depth = -0.5f;
		_editorName = "RC Car";
		thickness = 2f;
		weight = 5f;
		flammable = 0.3f;
		_wheel = new Sprite("rcWheel");
		_wheel.center = new Vec2(4f, 4f);
		weight = 0.5f;
		physicsMaterial = PhysicsMaterial.Metal;
	}

	public override void Initialize()
	{
	}

	public override void Terminate()
	{
		_idle.Kill();
		_idle.lerpVolume = 0f;
	}

	protected override bool OnDestroy(DestroyType type = null)
	{
		RumbleManager.AddRumbleEvent(position, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
		if (!base.isServerForObject)
		{
			return false;
		}
		ATRCShrapnel shrap = new ATRCShrapnel();
		shrap.MakeNetEffect(position);
		List<Bullet> firedBullets = new List<Bullet>();
		for (int i = 0; i < 20; i++)
		{
			float dir = (float)i * 18f - 5f + Rando.Float(10f);
			shrap = new ATRCShrapnel();
			shrap.range = 55f + Rando.Float(14f);
			Bullet bullet = new Bullet(base.x + (float)(Math.Cos(Maths.DegToRad(dir)) * 6.0), base.y - (float)(Math.Sin(Maths.DegToRad(dir)) * 6.0), shrap, dir);
			bullet.firedFrom = this;
			firedBullets.Add(bullet);
			Level.Add(bullet);
		}
		if (Network.isActive)
		{
			Send.Message(new NMFireGun(null, firedBullets, 0, rel: false, 4), NetMessagePriority.ReliableOrdered);
			firedBullets.Clear();
		}
		Level.Remove(this);
		if (Level.current.camera is FollowCam cam)
		{
			cam.Remove(this);
		}
		if (Recorder.currentRecording != null)
		{
			Recorder.currentRecording.LogBonus();
		}
		return true;
	}

	public override bool Hit(Bullet bullet, Vec2 hitPos)
	{
		if (bullet.isLocal && owner == null)
		{
			Thing.Fondle(this, DuckNetwork.localConnection);
		}
		if (bullet.isLocal)
		{
			Destroy(new DTShot(bullet));
		}
		return false;
	}

	public override void Update()
	{
		if (_controller == null && !(Level.current is Editor) && base.isServerForObject)
		{
			_controller = new RCController(base.x, base.y, this);
			Level.Add(_controller);
		}
		_wave.Update();
		base.Update();
		_sprite.currentAnimation = (_receivingSignal ? "beep" : "idle");
		_idle.lerpVolume = Math.Min(_idleSpeed * 10f, 0.7f);
		if (base._destroyed)
		{
			_idle.lerpVolume = 0f;
			_idle.lerpSpeed = 1f;
		}
		_idle.pitch = 0.5f + _idleSpeed * 0.5f;
		if (moveLeft)
		{
			if (base.isServerForObject)
			{
				if (hSpeed > 0f - _maxSpeed)
				{
					hSpeed -= 0.4f;
				}
				else
				{
					hSpeed = 0f - _maxSpeed;
				}
				offDir = -1;
			}
			_idleSpeed += 0.03f;
			_inc++;
		}
		if (moveRight)
		{
			if (base.isServerForObject)
			{
				if (hSpeed < _maxSpeed)
				{
					hSpeed += 0.4f;
				}
				else
				{
					hSpeed = _maxSpeed;
				}
				offDir = 1;
			}
			_idleSpeed += 0.03f;
			_inc++;
		}
		if (_idleSpeed > 0.1f)
		{
			_inc = 0;
			Level.Add(SmallSmoke.New(base.x - (float)(offDir * 10), base.y));
		}
		if (!moveLeft && !moveRight)
		{
			_idleSpeed -= 0.03f;
		}
		if (_idleSpeed > 1f)
		{
			_idleSpeed = 1f;
		}
		if (_idleSpeed < 0f)
		{
			_idleSpeed = 0f;
		}
		if (jump && base.grounded)
		{
			vSpeed -= 4.8f;
		}
		_tilt = MathHelper.Lerp(_tilt, 0f - hSpeed, 0.4f);
		_waveMult = MathHelper.Lerp(_waveMult, 0f - hSpeed, 0.1f);
		base.angleDegrees = _tilt * 2f + _wave.value * (_waveMult * (_maxSpeed - Math.Abs(hSpeed)));
		if (base.isServerForObject && base.isOffBottomOfLevel && !destroyed)
		{
			Destroy(new DTFall());
		}
	}

	public override void Draw()
	{
		if (owner == null)
		{
			_sprite.flipH = !((float)offDir >= 0f);
		}
		base.Draw();
		Graphics.Draw(_wheel, base.x - 7f, base.y + 9f);
		Graphics.Draw(_wheel, base.x + 7f, base.y + 9f);
	}
}
