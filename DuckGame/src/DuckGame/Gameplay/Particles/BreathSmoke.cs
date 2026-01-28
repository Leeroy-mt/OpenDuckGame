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
        obj.Depth = depth;
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
        obj.Depth = 0.8f;
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
        Center = new Vec2(8f, 8f);
    }

    private void Init(float xpos, float ypos)
    {
        _orbitInc += 0.2f;
        _life = 1;
        X = xpos;
        Y = ypos;
        _sprite.SetAnimation("idle");
        _sprite.frame = 0;
        _orbiter.imageIndex = _sprite.imageIndex;
        _sprite2.imageIndex = _sprite.imageIndex;
        _sprite.AngleDegrees = Rando.Float(360);
        _orbiter.AngleDegrees = Rando.Float(360);
        s1 = Rando.Float(0.6f, 1f);
        s2 = Rando.Float(0.6f, 1f);
        hSpeed = Rando.Float(-0.15f, 0.15f);
        vSpeed = Rando.Float(-0.1f, -0.05f);
        _life += Rando.Float(0.2f);
        _sprite.color = Color.White;
        Depth = 0.8f;
        Alpha = 0.15f;
        layer = Layer.Game;
    }

    public override void Initialize()
    {
    }

    public override void Update()
    {
        base.ScaleX = 1f;
        base.ScaleY = base.ScaleX;
        _orbitInc += _rotSpeed;
        _distPulse += _distPulseSpeed;
        base.Alpha -= 0.003f;
        vSpeed -= 0.01f;
        hSpeed *= 0.95f;
        if (_sprite.currentAnimation != "puff")
        {
            _sprite.SetAnimation("puff");
        }
        if (base.Alpha < 0f)
        {
            Level.Remove(this);
        }
        base.X += hSpeed;
        base.Y += vSpeed;
    }

    public override void Draw()
    {
        float distPulse = (float)Math.Sin(_distPulse);
        float xOff = (0f - (float)Math.Sin(_orbitInc) * distPulse) * s1;
        float yOff = (float)Math.Cos(_orbitInc) * distPulse * s1;
        _sprite.imageIndex = _sprite.imageIndex;
        _sprite.Depth = base.Depth;
        _sprite.Scale = new Vec2(s1);
        _sprite.Center = Center;
        _sprite.Alpha = base.Alpha;
        _sprite.color = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)(base.Alpha * 255f));
        _sprite.color = Color.White * base.Alpha;
        Graphics.Draw(_sprite, base.X + xOff, base.Y + yOff);
        _sprite2.frame = 0;
        _sprite2.imageIndex = _sprite.imageIndex;
        _sprite2.Angle = _sprite.Angle;
        _sprite2.Depth = -0.5f;
        _sprite2.Scale = _sprite.Scale;
        _sprite2.Center = Center;
        Rando.Float(0.2f);
        _sprite2.color = _sprite.color;
        Graphics.Draw(_sprite2, base.X + xOff, base.Y + yOff);
    }
}
