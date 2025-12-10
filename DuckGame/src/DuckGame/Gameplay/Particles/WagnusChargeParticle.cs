using System;

namespace DuckGame;

public class WagnusChargeParticle : Thing, IFactory
{
    private static int kMaxWagCharge = 64;

    private static WagnusChargeParticle[] _sparks = new WagnusChargeParticle[kMaxWagCharge];

    private static int _lastActiveWagCharge = 0;

    private Thing _target;

    private float life = 1f;

    public static WagnusChargeParticle New(float xpos, float ypos, Thing target)
    {
        WagnusChargeParticle spark = null;
        if (_sparks[_lastActiveWagCharge] == null)
        {
            spark = new WagnusChargeParticle();
            _sparks[_lastActiveWagCharge] = spark;
        }
        else
        {
            spark = _sparks[_lastActiveWagCharge];
        }
        _lastActiveWagCharge = (_lastActiveWagCharge + 1) % kMaxWagCharge;
        spark.ResetProperties();
        spark.Init(xpos, ypos, target);
        spark.globalIndex = Thing.GetGlobalIndex();
        return spark;
    }

    private WagnusChargeParticle()
    {
    }

    private void Init(float xpos, float ypos, Thing target)
    {
        hSpeed = Rando.Float(-1f, 1f);
        vSpeed = Rando.Float(-1f, 1f);
        position.x = xpos;
        position.y = ypos;
        base.depth = 0.9f;
        life = 1f;
        _target = target;
        base.alpha = 1f;
    }

    public override void Update()
    {
        Vec2 travel = position - _target.position;
        float len = travel.lengthSq;
        if (len < 64f || len > 4096f)
        {
            base.alpha -= 0.08f;
        }
        hSpeed = Lerp.Float(hSpeed, (0f - travel.x) * 0.7f, 0.15f);
        vSpeed = Lerp.Float(vSpeed, (0f - travel.y) * 0.7f, 0.15f);
        position.x += hSpeed;
        position.y += vSpeed;
        position.x = Lerp.Float(position.x, _target.x, 0.16f);
        position.y = Lerp.Float(position.y, _target.y, 0.16f);
        hSpeed *= Math.Min(1f, len / 128f + 0.25f);
        vSpeed *= Math.Min(1f, len / 128f + 0.25f);
        life -= 0.02f;
        if (life < 0f)
        {
            base.alpha -= 0.08f;
        }
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
        Graphics.DrawLine(col: new Color(147, 64, 221) * base.alpha, p1: position, p2: end, width: 1f, depth: base.depth);
    }
}
