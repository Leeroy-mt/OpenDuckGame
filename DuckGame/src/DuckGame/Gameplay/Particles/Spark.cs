namespace DuckGame;

public class Spark : PhysicsParticle
{
    private static int kMaxSparks = 64;

    private static Spark[] _sparks = new Spark[kMaxSparks];

    private static int _lastActiveSpark = 0;

    private float _killSpeed = 0.03f;

    public Color _color;

    public float _width = 0.5f;

    public static Spark New(float xpos, float ypos, Vec2 hitAngle, float killSpeed = 0.02f)
    {
        Spark spark = null;
        if (_sparks[_lastActiveSpark] == null)
        {
            spark = new Spark();
            _sparks[_lastActiveSpark] = spark;
        }
        else
        {
            spark = _sparks[_lastActiveSpark];
        }
        _lastActiveSpark = (_lastActiveSpark + 1) % kMaxSparks;
        spark.ResetProperties();
        spark.Init(xpos, ypos, hitAngle, killSpeed);
        spark.globalIndex = Thing.GetGlobalIndex();
        return spark;
    }

    private Spark()
        : base(0f, 0f)
    {
    }

    private void Init(float xpos, float ypos, Vec2 hitAngle, float killSpeed = 0.02f)
    {
        X = xpos;
        Y = ypos;
        hSpeed = (0f - hitAngle.X) * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(-1f, 1f);
        vSpeed = (0f - hitAngle.Y) * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(2f);
        _bounceEfficiency = 0.6f;
        base.Depth = 0.9f;
        _killSpeed = killSpeed;
        _color = new Color(byte.MaxValue, (byte)Rando.Int(180, 255), (byte)0);
        _width = 0.5f;
    }

    public override void Update()
    {
        base.Alpha -= _killSpeed;
        if (base.Alpha < 0f)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void Draw()
    {
        Vec2 dir = base.velocity.Normalized;
        float speed = base.velocity.Length() * 2f;
        Vec2 end = Position + dir * speed;
        Vec2 intersect;
        Block touch = Level.CheckLine<Block>(Position, end, out intersect);
        Graphics.DrawLine(Position, (touch != null) ? intersect : end, _color * base.Alpha, _width, base.Depth);
    }
}
