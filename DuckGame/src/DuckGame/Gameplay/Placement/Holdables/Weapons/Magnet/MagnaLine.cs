using System;

namespace DuckGame;

public class MagnaLine : Thing
{
    private Gun _attach;

    private float _length;

    private float _startLength;

    private float _move = (float)Math.PI / 2f;

    public bool show;

    public float dist;

    public float _alphaFade;

    public MagnaLine(float xpos, float ypos, Gun attach, float length, float percent)
        : base(xpos, ypos)
    {
        _attach = attach;
        _length = length;
        _startLength = length;
        _move = (float)Math.PI / 2f * percent;
        base.Alpha = 0f;
    }

    public override void Update()
    {
    }

    public override void Draw()
    {
        _move = Lerp.Float(_move, 0f, 0.04f);
        if (_move <= 0.01f)
        {
            _move += (float)Math.PI / 2f;
        }
        if (_length > dist)
        {
            show = false;
        }
        _alphaFade = Lerp.Float(_alphaFade, show ? 1f : 0f, 0.1f);
        _length = _startLength * (float)Math.Sin(_move);
        base.Alpha = (1f - _length / _startLength) * _alphaFade;
        if (!(base.Alpha < 0.01f))
        {
            Position = _attach.barrelPosition + _attach.barrelVector * _length;
            Vec2 off = _attach.barrelVector.Rotate(Maths.DegToRad(90f), Vec2.Zero);
            Graphics.DrawLine(Position + off * 7f, Position - off * 7f, Color.Blue * base.Alpha, 1f + (1f - _length / _startLength) * 4f, 0.9f);
        }
    }
}
