using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class BeamParticle : Thing
{
    public float _wave;

    public float _sinVal;

    private bool _inverse;

    private float _size;

    private Color _color;

    public BeamParticle(float xpos, float ypos, float spd, bool inverse, Color c)
        : base(xpos, ypos)
    {
        base.Depth = 0.9f;
        vSpeed = Rando.Float(-0.5f, -1.5f);
        base.Y += Rando.Float(10f);
        _inverse = inverse;
        _size = 0.5f + Rando.Float(0.8f);
        _color = c;
    }

    public override void Update()
    {
        _wave += 0.1f;
        _sinVal = (float)Math.Sin(_wave);
        base.Y += vSpeed;
        if (_sinVal < -0.8f && base.Depth > 0f)
        {
            base.Depth = -0.8f;
        }
        else if (_sinVal > 0.8f && base.Depth < 0f)
        {
            base.Depth = 0.8f;
        }
        if (base.Y < -20f)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void Draw()
    {
        Vector2 pos = Position + new Vector2(16f * _sinVal * (_inverse ? (-1f) : 1f), 0f);
        Graphics.DrawRect(pos - new Vector2(_size, _size), pos + new Vector2(_size, _size), _color * 0.4f, base.Depth);
        base.Draw();
    }
}
