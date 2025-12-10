using System;

namespace DuckGame;

public class ChallengeTimer : Timer
{
    private float _time;

    private bool _active;

    public override TimeSpan elapsed
    {
        get
        {
            TimeSpan span = new TimeSpan(0, 0, 0, 0, (int)(_time * 1000f));
            if (_maxTime.TotalSeconds == 0.0 || span < _maxTime)
            {
                return span;
            }
            return _maxTime;
        }
    }

    public ChallengeTimer(TimeSpan max = default(TimeSpan))
    {
        _maxTime = max;
    }

    public void Update()
    {
        if (_active)
        {
            _time += Maths.IncFrameTimer();
        }
    }

    public override void Start()
    {
        _active = true;
    }

    public override void Stop()
    {
        _active = false;
    }

    public override void Reset()
    {
        _time = 0f;
        _active = false;
    }

    public override void Restart()
    {
        _time = 0f;
    }
}
