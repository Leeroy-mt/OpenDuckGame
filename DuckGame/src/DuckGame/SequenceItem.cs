using System.Collections.Generic;

namespace DuckGame;

public class SequenceItem
{
    public static List<SequenceItem> sequenceItems = new List<SequenceItem>();

    public int order;

    private bool _finished;

    private bool _activated;

    private Thing _thing;

    private SequenceItemType _type;

    private bool _loop;

    public bool waitTillOrder;

    public bool isValid = true;

    public int timesActivated;

    public int likelyhood;

    public bool randomMode;

    public bool _resetLikelyhood = true;

    public bool finished => _finished;

    public bool activated => _activated;

    public SequenceItemType type
    {
        get
        {
            return _type;
        }
        set
        {
            _type = value;
        }
    }

    public bool loop
    {
        get
        {
            return _loop;
        }
        set
        {
            _loop = value;
        }
    }

    public SequenceItem(Thing t)
    {
        _thing = t;
    }

    public virtual void Finished()
    {
        _finished = true;
        if (order >= 0)
        {
            CheckSequence();
        }
    }

    public void Reset()
    {
        _activated = false;
        _finished = false;
    }

    public void BeginRandomSequence()
    {
        List<int> orders = new List<int>();
        foreach (ISequenceItem item in Level.current.things[typeof(ISequenceItem)])
        {
            SequenceItem t = (item as Thing).sequence;
            t._finished = false;
            t._activated = false;
            if (t.order != order && !orders.Contains(t.order))
            {
                orders.Add(t.order);
            }
        }
        if (orders.Count == 0)
        {
            orders.Add(order);
        }
        int pick = Rando.ChooseInt(orders.ToArray());
        foreach (ISequenceItem item2 in Level.current.things[typeof(ISequenceItem)])
        {
            SequenceItem t2 = (item2 as Thing).sequence;
            if (t2.order == pick)
            {
                t2.Activate();
            }
        }
    }

    private bool SequenceFinished()
    {
        foreach (ISequenceItem item in Level.current.things[typeof(ISequenceItem)])
        {
            SequenceItem t = (item as Thing).sequence;
            if (t.order == order && !t._finished)
            {
                return false;
            }
        }
        return true;
    }

    private void CheckSequence()
    {
        if (randomMode)
        {
            return;
        }
        List<SequenceItem> things = new List<SequenceItem>();
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
            {
                continue;
            }
            if (!t._activated && t.order > realOrder)
            {
                if (t.order == minNext)
                {
                    things.Add(t);
                }
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
        {
            item2.Activate();
        }
    }

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

    public void Activate()
    {
        if (!_activated)
        {
            if (_resetLikelyhood)
            {
                likelyhood = 0;
            }
            _activated = true;
            _thing.OnSequenceActivate();
            OnActivate();
            timesActivated++;
        }
    }

    public virtual void OnActivate()
    {
    }
}
