using System;

namespace DuckGame;

public class MultiBeamParticle : Thing
{
    public float _wave;

    public float _sinVal;

    private bool _inverse;

    private float _size;

    private Color _color;

    public MultiBeamParticle(float xpos, float ypos, float spd, bool inverse, Color c)
        : base(xpos, ypos)
    {
        base.depth = 0.9f;
        vSpeed = Rando.Float(-0.5f, -1.5f);
        base.y += Rando.Float(10f);
        _inverse = inverse;
        _size = 0.5f + Rando.Float(0.8f);
        _color = c;
        base.layer = Layer.Background;
    }

    public override void Update()
    {
        _wave += 0.1f;
        _sinVal = (float)Math.Sin(_wave);
        base.y += vSpeed;
        if (_sinVal < -0.8f && base.depth > 0f)
        {
            base.depth = -0.8f;
        }
        else if (_sinVal > 0.8f && base.depth < 0f)
        {
            base.depth = 0.8f;
        }
        if (base.y < 0f)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void Draw()
    {
        Vec2 pos = position + new Vec2(16f * _sinVal * (_inverse ? (-1f) : 1f), 0f);
        Graphics.DrawRect(pos - new Vec2(_size, _size), pos + new Vec2(_size, _size), _color * 0.4f, base.depth);
        base.Draw();
    }
}
