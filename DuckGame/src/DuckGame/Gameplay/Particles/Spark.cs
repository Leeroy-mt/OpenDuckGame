namespace DuckGame;

public class Spark : PhysicsParticle, IFactory
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
        position.x = xpos;
        position.y = ypos;
        hSpeed = (0f - hitAngle.x) * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(-1f, 1f);
        vSpeed = (0f - hitAngle.y) * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(2f);
        _bounceEfficiency = 0.6f;
        base.depth = 0.9f;
        _killSpeed = killSpeed;
        _color = new Color(byte.MaxValue, (byte)Rando.Int(180, 255), (byte)0);
        _width = 0.5f;
    }

    public override void Update()
    {
        base.alpha -= _killSpeed;
        if (base.alpha < 0f)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void Draw()
    {
        Vec2 dir = base.velocity.normalized;
        float speed = base.velocity.length * 2f;
        Vec2 end = position + dir * speed;
        Vec2 intersect;
        Block touch = Level.CheckLine<Block>(position, end, out intersect);
        Graphics.DrawLine(position, (touch != null) ? intersect : end, _color * base.alpha, _width, base.depth);
    }
}
