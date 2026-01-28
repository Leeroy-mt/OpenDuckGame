using System;

namespace DuckGame;

public class Fluid : PhysicsParticle
{
    public static FluidData Lava = new FluidData(0f, new Color(255, 89, 5).ToVector4(), 0f, "lava", 1f, 0.8f);

    public static FluidData Gas = new FluidData(0f, new Color(246, 198, 55).ToVector4(), 1f, "gas");

    public static FluidData Water = new FluidData(0f, new Color(0, 150, 249).ToVector4(), 0f, "water");

    public static FluidData Ketchup = new FluidData(0f, Color.Red.ToVector4() * 0.8f, 0.4f, "water");

    public static FluidData Poo = new FluidData(0f, Color.SaddleBrown.ToVector4() * 0.8f, 0.5f, "water");

    private Fluid _stream;

    private Fluid _child;

    private bool _firstHit;

    private float _thickness;

    public FluidData data;

    private float _thickMult;

    private SmallFire _fire;

    public SpriteMap _glob;

    private float startThick;

    private float live = 1f;

    public Fluid child
    {
        get
        {
            return _child;
        }
        set
        {
            _child = value;
        }
    }

    public SmallFire fire
    {
        get
        {
            return _fire;
        }
        set
        {
            _fire = value;
        }
    }

    public Fluid(float xpos, float ypos, Vec2 hitAngle, FluidData dat, Fluid stream = null, float thickMult = 1f)
        : base(xpos, ypos)
    {
        hSpeed = (0f - hitAngle.X) * 2f * (Rando.Float(1f) + 0.3f);
        vSpeed = (0f - hitAngle.Y) * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(2f);
        hSpeed = hitAngle.X;
        vSpeed = hitAngle.Y;
        _bounceEfficiency = 0.6f;
        _stream = stream;
        if (stream != null)
        {
            stream.child = this;
        }
        base.Alpha = 1f;
        _gravMult = 2f;
        base.Depth = -0.5f;
        data = dat;
        _thickMult = thickMult;
        _thickness = Maths.Clamp(data.amount * 600f, 0.2f, 8f) * _thickMult;
        startThick = _thickness;
        _glob = new SpriteMap("bigGlob", 8, 8);
    }

    public override void Update()
    {
        if (_fire != null)
        {
            _fire.Position = Position;
        }
        _life = 1f;
        if (_thickness < 4f || Math.Abs(vSpeed) < 1.5f)
        {
            live -= 0.01f;
        }
        _thickness = Lerp.FloatSmooth(startThick, 0.1f, 1f - live);
        if (live < 0f || (_grounded && Math.Abs(vSpeed) < 0.1f))
        {
            Level.Remove(this);
            active = false;
            FluidPuddle p = null;
            foreach (FluidPuddle puddle in Level.current.things[typeof(FluidPuddle)])
            {
                if (base.X > puddle.left && base.X < puddle.right && Math.Abs(puddle.Y - base.Y) < 10f)
                {
                    p = puddle;
                    break;
                }
            }
            if (p == null)
            {
                Vec2 hitPos;
                Block b = Level.CheckLine<AutoBlock>(Position + new Vec2(0f, -8f), Position + new Vec2(0f, 16f), out hitPos);
                if (b != null && hitPos.Y == b.top)
                {
                    p = new FluidPuddle(hitPos.X, hitPos.Y, b);
                    Level.Add(p);
                }
            }
            p?.Feed(data);
            return;
        }
        base.Update();
        if (_touchedFloor && !_firstHit)
        {
            _firstHit = true;
            hSpeed += Rando.Float(-1f, 1f);
            hSpeed *= Rando.Float(-1f, 1.5f);
            vSpeed *= Rando.Float(0.3f, 1f);
        }
        if (_stream != null)
        {
            float hDif = Math.Abs(hSpeed - _stream.hSpeed);
            if (Math.Abs(base.X - _stream.X) * hDif > 40f || Math.Abs(vSpeed - _stream.vSpeed) > 1.9f || hDif > 1.9f)
            {
                BreakStream();
            }
        }
    }

    public void BreakStream()
    {
        if (_child != null)
        {
            _child._stream = null;
        }
        _child = null;
        if (_stream != null)
        {
            _stream._child = null;
        }
        _stream = null;
    }

    public override void Draw()
    {
        if (_stream != null)
        {
            Graphics.currentDrawIndex++;
            Graphics.DrawLine(Position, _stream.Position, new Color(data.color) * base.Alpha, _thickness, base.Depth);
        }
        else if (_child == null)
        {
            if (_thickness > 4f)
            {
                _glob.Depth = base.Depth;
                _glob.frame = 2;
                _glob.color = new Color(data.color) * base.Alpha;
                _glob.CenterOrigin();
                _glob.Angle = Maths.DegToRad(0f - Maths.PointDirection(Position, Position + base.velocity) + 90f);
                Graphics.Draw(_glob, base.X, base.Y);
            }
            else
            {
                Graphics.DrawRect(Position - new Vec2(_thickness / 2f, _thickness / 2f), Position + new Vec2(_thickness / 2f, _thickness / 2f), new Color(data.color) * base.Alpha, base.Depth);
            }
        }
    }
}
