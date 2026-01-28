using System.Collections.Generic;

namespace DuckGame;

public class SequenceItem
{
    #region Public Fields

    public static List<SequenceItem> sequenceItems = [];

    public bool waitTillOrder;
    public bool isValid = true;
    public bool randomMode;
    public bool _resetLikelyhood = true;

    public int order;
    public int timesActivated;
    public int likelyhood;

    #endregion

    #region Private Fields

    bool _finished;
    bool _activated;
    bool _loop;

    SequenceItemType _type;

    Thing _thing;

    #endregion

    #region Public Properties

    public bool finished => _finished;

    public bool activated => _activated;

    public bool loop
    {
        get => _loop;
        set => _loop = value;
    }

    public SequenceItemType type
    {
        get => _type;
        set => _type = value;
    }

    #endregion

    public SequenceItem(Thing t)
    {
        _thing = t;
    }

    #region Public Methods

    public static bool IsFinished()
    {
        bool fin = true;
        foreach (ISequenceItem item in Level.current.things[typeof(ISequenceItem)])
        {
            SequenceItem t = (item as Thing).sequence;
            if (t != null && !t._finished && t.isValid)
            {
                fin = false;
                break;
            }
        }
        return fin;
    }

    public static bool IsFinished(SequenceItemType tp)
    {
        bool fin = true;
        foreach (ISequenceItem item in Level.current.things[typeof(ISequenceItem)])
        {
            SequenceItem t = (item as Thing).sequence;
            if (t != null && t.type == tp && !t._finished && t.isValid)
            {
                fin = false;
                break;
            }
        }
        return fin;
    }

    public virtual void Finished()
    {
        _finished = true;
        if (order >= 0)
            CheckSequence();
    }

    public virtual void OnActivate() { }

    public void Reset()
    {
        _activated = false;
        _finished = false;
    }

    public void BeginRandomSequence()
    {
        List<int> orders = [];
        foreach (ISequenceItem item in Level.current.things[typeof(ISequenceItem)])
        {
            SequenceItem t = (item as Thing).sequence;
            t._finished = false;
            t._activated = false;
            if (t.order != order && !orders.Contains(t.order))
                orders.Add(t.order);
        }
        if (orders.Count == 0)
            orders.Add(order);
        int pick = Rando.ChooseInt([.. orders]);
        foreach (ISequenceItem item2 in Level.current.things[typeof(ISequenceItem)])
        {
            SequenceItem t2 = (item2 as Thing).sequence;
            if (t2.order == pick)
                t2.Activate();
        }
    }

    public void Activate()
    {
        if (!_activated)
        {
            if (_resetLikelyhood)
                likelyhood = 0;
            _activated = true;
            _thing.OnSequenceActivate();
            OnActivate();
            timesActivated++;
        }
    }

    #endregion

    #region Private Methods

    bool SequenceFinished()
    {
        foreach (ISequenceItem item in Level.current.things[typeof(ISequenceItem)])
        {
            SequenceItem t = (item as Thing).sequence;
            if (t.order == order && !t._finished)
                return false;
        }
        return true;
    }

    void CheckSequence()
    {
        if (randomMode)
            return;
        List<SequenceItem> things = [];
        int minNext = 9999999;
        int realOrder = order;
        if (loop && SequenceFinished())
        {
            realOrder = -1;
            foreach (ISequenceItem item in Level.current.things[typeof(ISequenceItem)])
            {
                SequenceItem sequence = (item as Thing).sequence;
                sequence._activated = false;
                sequence._finished = false;
            }
        }
        bool notFinished = false;
        foreach (ISequenceItem seq in Level.current.things[typeof(ISequenceItem)])
        {
            SequenceItem t = (seq as Thing).sequence;
            if ((t == this && !loop) || ((seq is Window || seq is Door) && !t.isValid))
                continue;
            if (!t._activated && t.order > realOrder)
            {
                if (t.order == minNext)
                    things.Add(t);
                else if (t.order < minNext)
                {
                    things.Clear();
                    things.Add(t);
                    minNext = t.order;
                }
            }
            if (t.order == realOrder && !t._finished)
            {
                things.Clear();
                notFinished = true;
                break;
            }
        }
        if (!notFinished && ChallengeLevel.random)
        {
            BeginRandomSequence();
            return;
        }
        foreach (SequenceItem item2 in things)
            item2.Activate();
    }

    #endregion
}
