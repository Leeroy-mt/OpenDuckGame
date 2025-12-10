using System;

namespace DuckGame;

public class ConfettiParticle : PhysicsParticle, IFactory
{
    private static int kMaxSparks = 64;

    private static ConfettiParticle[] _sparks = new ConfettiParticle[kMaxSparks];

    private static int _lastActiveSpark = 0;

    private float _killSpeed = 0.03f;

    public Color _color;

    public float _width = 0.5f;

    private bool _stringConfetti;

    private static int _confettiNumber;

    private new float life = 1f;

    private float sin;

    private float sinMult;

    public static ConfettiParticle New(float xpos, float ypos, Vec2 hitAngle, float killSpeed = 0.02f, bool lineType = false)
    {
        ConfettiParticle spark = null;
        if (_sparks[_lastActiveSpark] == null)
        {
            spark = new ConfettiParticle();
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
        spark._stringConfetti = lineType;
        return spark;
    }

    public ConfettiParticle()
        : base(0f, 0f)
    {
    }

    public void Init(float xpos, float ypos, Vec2 hitAngle, float killSpeed = 0.02f)
    {
        position.x = xpos;
        position.y = ypos;
        hSpeed = (0f - hitAngle.x) * 1.5f * Rando.Float(-2f, 2f);
        vSpeed = (0f - hitAngle.y) * 2f * (Rando.Float(1f) - 0.3f) - Rando.Float(1f);
        hSpeed *= 1.5f;
        vSpeed *= 1.5f;
        _bounceEfficiency = 0.1f;
        base.depth = 0.9f;
        _killSpeed = killSpeed;
        _color = Color.RainbowColors[_confettiNumber % Color.RainbowColors.Count];
        _confettiNumber++;
        _width = 1f;
        life = Rando.Float(0.8f, 1f);
        sin = Rando.Float(3.14f);
        _gravMult = 0.3f;
        sinMult = 0f;
        onlyDieWhenGrounded = true;
    }

    public override void Update()
    {
        hSpeed *= 0.95f;
        vSpeed *= 0.96f;
        life -= 0.03f;
        if (life <= 0f)
        {
            sinMult += 0.02f;
            if (sinMult > 1f)
            {
                sinMult = 1f;
            }
            if (!_grounded && Math.Abs(hSpeed) < 0.2f)
            {
                sin += 0.2f;
                base.x += (float)Math.Sin(sin) * 0.5f * sinMult;
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        if (_stringConfetti)
        {
            Vec2 dir = base.velocity.normalized;
            float speed = base.velocity.length * (3f + sinMult * 3f);
            Vec2 end = position + dir * speed;
            Vec2 intersect;
            Block touch = Level.CheckLine<Block>(position, end, out intersect);
            Graphics.DrawLine(position, (touch != null) ? intersect : end, _color * base.alpha, _width, base.depth);
        }
        else
        {
            Graphics.DrawRect(position + new Vec2(-1f, -1f), position + new Vec2(1f, 1f), _color * base.alpha, base.depth);
        }
    }
}
