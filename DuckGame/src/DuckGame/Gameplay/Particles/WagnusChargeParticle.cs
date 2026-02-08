using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class WagnusChargeParticle : Thing
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
        X = xpos;
        Y = ypos;
        base.Depth = 0.9f;
        life = 1f;
        _target = target;
        base.Alpha = 1f;
    }

    public override void Update()
    {
        Vector2 travel = Position - _target.Position;
        float len = travel.LengthSquared();
        if (len < 64f || len > 4096f)
        {
            base.Alpha -= 0.08f;
        }
        hSpeed = Lerp.Float(hSpeed, (0f - travel.X) * 0.7f, 0.15f);
        vSpeed = Lerp.Float(vSpeed, (0f - travel.Y) * 0.7f, 0.15f);
        X += hSpeed;
        Y += vSpeed;
        X = Lerp.Float(Position.X, _target.X, 0.16f);
        Y = Lerp.Float(Position.Y, _target.Y, 0.16f);
        hSpeed *= Math.Min(1f, len / 128f + 0.25f);
        vSpeed *= Math.Min(1f, len / 128f + 0.25f);
        life -= 0.02f;
        if (life < 0f)
        {
            base.Alpha -= 0.08f;
        }
        if (base.Alpha < 0f)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void Draw()
    {
        Vector2 dir = Vector2.Normalize(velocity);
        float speed = base.velocity.Length() * 2f;
        Vector2 end = Position + dir * speed;
        Graphics.DrawLine(col: new Color(147, 64, 221) * base.Alpha, p1: Position, p2: end, width: 1f, depth: base.Depth);
    }
}
