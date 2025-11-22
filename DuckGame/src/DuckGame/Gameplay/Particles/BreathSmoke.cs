using System;

namespace DuckGame;

public class BreathSmoke : Thing
{
	private static int kMaxObjects = 64;

	private static BreathSmoke[] _objects = new BreathSmoke[kMaxObjects];

	private static int _lastActiveObject = 0;

	public static bool shortlife = false;

	private float _orbitInc = Rando.Float(5f);

	private SpriteMap _sprite2;

	private SpriteMap _sprite;

	private SpriteMap _orbiter;

	private float _life = 1f;

	private float _rotSpeed = Rando.Float(0.05f, 0.15f);

	private float _distPulseSpeed = Rando.Float(0.05f, 0.15f);

	private float _distPulse = Rando.Float(5f);

	private float s1 = 1f;

	private float s2 = 1f;

	private float lifeTake = 0.05f;

	public SpriteMap sprite => _sprite;

	public static BreathSmoke New(float xpos, float ypos, float depth = 0.8f, float scaleMul = 1f)
	{
		BreathSmoke obj = null;
		if (_objects[_lastActiveObject] == null)
		{
			obj = new BreathSmoke();
			_objects[_lastActiveObject] = obj;
		}
		else
		{
			obj = _objects[_lastActiveObject];
		}
		_lastActiveObject = (_lastActiveObject + 1) % kMaxObjects;
		obj.Init(xpos, ypos);
		obj.ResetProperties();
		obj._sprite.globalIndex = Thing.GetGlobalIndex();
		obj.globalIndex = Thing.GetGlobalIndex();
		obj.depth = depth;
		obj.s1 *= scaleMul;
		obj.s2 *= scaleMul;
		if (shortlife)
		{
			obj.lifeTake = 0.14f;
		}
		return obj;
	}

	public static BreathSmoke New(float xpos, float ypos)
	{
		BreathSmoke obj = null;
		if (_objects[_lastActiveObject] == null)
		{
			obj = new BreathSmoke();
			_objects[_lastActiveObject] = obj;
		}
		else
		{
			obj = _objects[_lastActiveObject];
		}
		_lastActiveObject = (_lastActiveObject + 1) % kMaxObjects;
		obj.Init(xpos, ypos);
		obj.ResetProperties();
		obj._sprite.globalIndex = Thing.GetGlobalIndex();
		obj.globalIndex = Thing.GetGlobalIndex();
		obj.depth = 0.8f;
		return obj;
	}

	private BreathSmoke()
	{
		_sprite = new SpriteMap("tinySmokeTestFront", 16, 16);
		int off = Rando.Int(3) * 4;
		_sprite.AddAnimation("idle", 0.1f, true, 2 + off);
		_sprite.AddAnimation("puff", Rando.Float(0.08f, 0.12f), false, 2 + off, 1 + off, off);
		_orbiter = new SpriteMap("tinySmokeTestFront", 16, 16);
		off = Rando.Int(3) * 4;
		_orbiter.AddAnimation("idle", 0.1f, true, 2 + off);
		_orbiter.AddAnimation("puff", Rando.Float(0.08f, 0.12f), false, 2 + off, 1 + off, off);
		_sprite2 = new SpriteMap("tinySmokeTestBack", 16, 16);
		_sprite2.currentAnimation = null;
		_orbiter.currentAnimation = null;
		center = new Vec2(8f, 8f);
	}

	private void Init(float xpos, float ypos)
	{
		_orbitInc += 0.2f;
		_life = 1f;
		position.x = xpos;
		position.y = ypos;
		_sprite.SetAnimation("idle");
		_sprite.frame = 0;
		_orbiter.imageIndex = _sprite.imageIndex;
		_sprite2.imageIndex = _sprite.imageIndex;
		_sprite.angleDegrees = Rando.Float(360f);
		_orbiter.angleDegrees = Rando.Float(360f);
		s1 = Rando.Float(0.6f, 1f);
		s2 = Rando.Float(0.6f, 1f);
		hSpeed = Rando.Float(-0.15f, 0.15f);
		vSpeed = Rando.Float(-0.1f, -0.05f);
		_life += Rando.Float(0.2f);
		_sprite.color = Color.White;
		base.depth = 0.8f;
		base.alpha = 0.15f;
		base.layer = Layer.Game;
	}

	public override void Initialize()
	{
	}

	public override void Update()
	{
		base.xscale = 1f;
		base.yscale = base.xscale;
		_orbitInc += _rotSpeed;
		_distPulse += _distPulseSpeed;
		base.alpha -= 0.003f;
		vSpeed -= 0.01f;
		hSpeed *= 0.95f;
		if (_sprite.currentAnimation != "puff")
		{
			_sprite.SetAnimation("puff");
		}
		if (base.alpha < 0f)
		{
			Level.Remove(this);
		}
		base.x += hSpeed;
		base.y += vSpeed;
	}

	public override void Draw()
	{
		float distPulse = (float)Math.Sin(_distPulse);
		float xOff = (0f - (float)Math.Sin(_orbitInc) * distPulse) * s1;
		float yOff = (float)Math.Cos(_orbitInc) * distPulse * s1;
		_sprite.imageIndex = _sprite.imageIndex;
		_sprite.depth = base.depth;
		_sprite.scale = new Vec2(s1);
		_sprite.center = center;
		_sprite.alpha = base.alpha;
		_sprite.color = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)(base.alpha * 255f));
		_sprite.color = Color.White * base.alpha;
		Graphics.Draw(_sprite, base.x + xOff, base.y + yOff);
		_sprite2.frame = 0;
		_sprite2.imageIndex = _sprite.imageIndex;
		_sprite2.angle = _sprite.angle;
		_sprite2.depth = -0.5f;
		_sprite2.scale = _sprite.scale;
		_sprite2.center = center;
		Rando.Float(0.2f);
		_sprite2.color = _sprite.color;
		Graphics.Draw(_sprite2, base.x + xOff, base.y + yOff);
	}
}
