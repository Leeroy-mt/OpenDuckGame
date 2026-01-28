using System;

namespace DuckGame;

public class SnowFallParticle : PhysicsParticle
{
    private float _sin;

    private float _moveSpeed = 0.1f;

    private float _sinSize = 0.1f;

    private float _drift;

    private float _size;

    public SnowFallParticle(float xpos, float ypos, Vec2 startVel, bool big = false)
        : base(xpos, ypos)
    {
        _gravMult = 0.5f;
        _sin = Rando.Float(7f);
        _moveSpeed = Rando.Float(0.005f, 0.02f);
        _sinSize = Rando.Float(8f, 16f);
        _size = Rando.Float(0.2f, 0.6f);
        if (big)
        {
            _size = Rando.Float(0.8f, 1f);
        }
        base.life = Rando.Float(0.1f, 0.2f);
        onlyDieWhenGrounded = true;
        base.velocity = startVel;
    }

    public override void Update()
    {
        base.Update();
        if (vSpeed > 1f)
        {
            vSpeed = 1f;
        }
        if (!_grounded)
        {
            float sinOff = (float)Math.Sin(_sin) * _sinSize;
            _sin += _moveSpeed;
            base.X += Rando.Float(-0.3f, 0.3f);
            base.X += sinOff / 60f;
        }
    }

    public override void Draw()
    {
        _ = base.Z / 200f;
        float size = _size;
        Graphics.DrawRect(Position + new Vec2(0f - size, 0f - size), Position + new Vec2(size, size), Color.White * base.Alpha, 0.1f);
    }
}
