using System;
using System.Diagnostics;

namespace DuckGame;

public class Timer
{
    private Stopwatch _timer = new Stopwatch();

    private TimeSpan _subtract;

    protected TimeSpan _maxTime;

    public virtual TimeSpan elapsed
    {
        get
        {
            if (_maxTime.TotalSeconds == 0.0 || _timer.Elapsed - _subtract < _maxTime)
            {
                return _timer.Elapsed - _subtract;
            }
            return _maxTime;
        }
    }

    public TimeSpan maxTime
    {
        get
        {
            return _maxTime;
        }
        set
        {
            _maxTime = value;
        }
    }

    public Timer(TimeSpan max = default(TimeSpan))
    {
        _maxTime = max;
    }

    public virtual void Start()
    {
        _timer.Start();
    }

    public virtual void Stop()
    {
        _timer.Stop();
    }

    public virtual void Reset()
    {
        _timer.Reset();
    }

    public virtual void Restart()
    {
        _timer.Restart();
    }

    public virtual void Subtract(TimeSpan s)
    {
        _subtract += s;
    }
}
