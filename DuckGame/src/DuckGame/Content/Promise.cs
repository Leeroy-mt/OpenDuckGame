using System;
using System.Threading;

namespace DuckGame;

public class Promise
{
    protected bool _finished;

    protected Delegate _delegate;

    public bool Finished
    {
        get
        {
            lock (this)
            {
                return _finished;
            }
        }
        protected set
        {
            lock (this)
            {
                _finished = value;
            }
        }
    }

    protected Promise(Delegate d)
    {
        _delegate = d;
    }

    public Promise(Action action)
        : this((Delegate)action)
    {
    }

    public virtual void Execute()
    {
        _delegate.Method.Invoke(_delegate.Target, null);
        Finished = true;
    }

    public void WaitForComplete(uint waitMs = 13u, uint maxAttempts = 0u)
    {
        while (!Finished)
        {
            Thread.Sleep((int)waitMs);
            if (maxAttempts != 0 && --maxAttempts == 0)
            {
                break;
            }
        }
    }
}
public class Promise<T> : Promise
{
    public T Result { get; private set; }

    public Promise(Func<T> function)
        : base(function)
    {
    }

    public override void Execute()
    {
        Result = (T)_delegate.Method.Invoke(_delegate.Target, null);
        base.Finished = true;
    }
}
