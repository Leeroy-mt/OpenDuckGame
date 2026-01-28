using System;
using System.Collections.Generic;

namespace DuckGame;

public class LoadingAction
{
    private bool actionInvoked;

    public bool flag;

    public bool waiting;

    public object context;

    public Queue<LoadingAction> actions = new();

    public Action action;

    public Func<bool> waitAction;

    public LoadingAction()
    {
    }

    public LoadingAction(Action pAction, Func<bool> pWaitAction = null)
    {
        action = pAction;
        waitAction = pWaitAction;
    }

    public bool Invoke()
    {
        MonoMain.currentActionQueue = actions;
        if (!actionInvoked)
        {
            actionInvoked = true;
            action();
        }
        if (actions.Count > 0)
        {
            LoadingAction a = actions.Peek();
            if (a.Invoke())
            {
                actions.Dequeue();
                return false;
            }
            if (a.waiting)
            {
                waiting = true;
                return false;
            }
        }
        if (actions.Count > 0)
        {
            return false;
        }
        if (waitAction != null)
        {
            waiting = true;
            return waitAction();
        }
        waiting = false;
        return true;
    }

    public static implicit operator LoadingAction(Action pAction)
    {
        return new LoadingAction(pAction);
    }
}
