using System;

namespace DuckGame;

public class RhythmMode
{
    private static Sprite _bar;

    private static Sprite _ball;

    private static float _pos;

    private static float _soundPos;

    public static bool inTime
    {
        get
        {
            if (!(_pos < 0.25f))
            {
                return _pos > 0.75f;
            }
            return true;
        }
    }

    public static void Tick(float pos)
    {
        _pos = pos;
    }

    public static void TickSound(float pos)
    {
        if (pos < _soundPos)
        {
            SFX.Play("metronome");
        }
        _soundPos = pos;
    }

    public static void Draw()
    {
        if (_bar == null)
        {
            _bar = new Sprite("rhythmBar");
        }
        if (_ball == null)
        {
            _ball = new Sprite("rhythmBall");
            _ball.CenterOrigin();
        }
        Vec2 barPos = new Vec2(Layer.HUD.camera.width / 2f - (float)(_bar.w / 2), 10f);
        Graphics.Draw(_bar, barPos.X, barPos.Y);
        for (int i = 0; i < 5; i++)
        {
            float xpos = barPos.X + 2f + (float)(i * (_bar.w / 4) + 1) + _pos * ((float)_bar.w / 4f);
            float distance = Maths.Clamp((xpos - barPos.X) / ((float)_bar.w - 2f), 0f, 1f);
            _ball.Alpha = ((float)Math.Sin((double)distance * (Math.PI * 2.0) - Math.PI / 2.0) + 1f) / 2f;
            if (((i == 1 && _pos > 0.5f) || (i == 2 && _pos <= 0.5f)) && inTime)
            {
                _ball.Scale = new Vec2(2f, 2f);
            }
            else
            {
                _ball.Scale = new Vec2(1f, 1f);
            }
            Graphics.Draw(_ball, xpos, barPos.Y + 4f);
        }
    }
}
