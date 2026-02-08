using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[EditorGroup("Arcade|Targets", EditorItemType.ArcadeNew)]
public class RandomControllerNew : Thing
{
    public EditorProperty<int> Max_Up = new EditorProperty<int>(1, null, 0f, 32f, 1f, "NO LIMIT");

    public EditorProperty<float> Delay = new EditorProperty<float>(0f, null, 0f, 100f, 0.05f);

    public EditorProperty<bool> Continuous = new EditorProperty<bool>(val: true);

    public EditorProperty<int> Group = new EditorProperty<int>(0, null, -1f, 99f, 1f, "ALL");

    public EditorProperty<bool> Ordered_Groups = new EditorProperty<bool>(val: false, "GROUP");

    public EditorProperty<bool> Group_Wait = new EditorProperty<bool>(val: false, "GROUP");

    public EditorProperty<SequenceItemType> Type = new EditorProperty<SequenceItemType>(SequenceItemType.Activator, "TYPE");

    private int _originalMaxUp;

    private SequenceItem _lastUp;

    private bool _started;

    private float _waitCount;

    private int _totalUp;

    private List<SequenceItem> _up = new List<SequenceItem>();

    private int _sequenceNumber;

    private bool _hadFutureItems;

    private int _activationCycle;

    private bool _finished;

    private HashSet<int> _processedSequences = new HashSet<int>();

    public RandomControllerNew()
    {
        graphic = new Sprite("swirl");
        Center = new Vector2(8f, 8f);
        collisionSize = new Vector2(16f, 16f);
        collisionOffset = new Vector2(-8f, -8f);
        _canFlip = false;
        _visibleInGame = false;
        _editorName = "Random Controller";
        editorTooltip = "Allows you to make it so Targets/Goodies appear randomly.";
        Max_Up._tooltip = "How many Targets/Goodies can be active at once.";
        Delay._tooltip = "The delay (in seconds) before new Targets/Goodies popup.";
        Continuous._tooltip = "If true, Targets/Goodies will keep popping up forever. Otherwise, each will appear only once.";
        Group._tooltip = "Which sequence group this controller activates.";
        Ordered_Groups._tooltip = "If true, Targets/Goodie Order Groups will run sequentially. Otherwise groups will appear randomly.";
        Group_Wait._tooltip = "If true, each Target/Goodie Order Group will wait until the previous group is finished before appearing.";
        Type._tooltip = "Selects which sort of Sequence this controller activates.";
    }

    public override void Initialize()
    {
        _originalMaxUp = Max_Up.value;
        if (Group.value > 0)
        {
            _sequenceNumber = Group.value;
        }
        base.Initialize();
    }

    public override void Update()
    {
        if (_finished || !base.isServerForObject || !Level.current.simulatePhysics)
        {
            return;
        }
        if (!_started)
        {
            if (!Ordered_Groups.value)
            {
                List<Thing> sequences = Level.current.things[typeof(ISequenceItem)].ToList();
                if (sequences.Count > 0)
                {
                    _sequenceNumber = sequences[ChallengeRando.Int(sequences.Count - 1)].sequence.order;
                }
            }
            PopUpItems();
            _started = true;
        }
        if (_up.Count > 0)
        {
            for (int i = 0; i < _up.Count; i++)
            {
                if (_up[i].finished)
                {
                    _lastUp = _up[i];
                    _up[i].Reset();
                    _up.Remove(_up[i]);
                    if (!Continuous.value)
                    {
                        _lastUp.isValid = false;
                    }
                    else if (!_hadFutureItems)
                    {
                        _lastUp.timesActivated = 0;
                    }
                    i--;
                }
            }
        }
        if ((!Group_Wait.value || _up.Count == 0) && (_up.Count < (int)Max_Up || (int)Max_Up == 0))
        {
            _waitCount += Maths.IncFrameTimer();
            if (_waitCount >= Delay.value)
            {
                PopUpItems();
                _waitCount = 0f;
            }
        }
    }

    private void PopUpItems()
    {
        bool foundNoSequences = false;
        int numToPop = Max_Up.value;
        if (numToPop == 0)
        {
            numToPop = 9999;
        }
        while (_up.Count < numToPop)
        {
            List<Thing> sequences = null;
            sequences = ((Group.value < 0) ? ((Type.value != SequenceItemType.ALL) ? Level.current.things[typeof(ISequenceItem)].Where((Thing x) => x.sequence.type == Type.value).ToList() : Level.current.things[typeof(ISequenceItem)].ToList()) : ((Type.value != SequenceItemType.ALL) ? Level.current.things[typeof(ISequenceItem)].Where((Thing x) => x.sequence.type == Type.value && x.sequence.order == Group.value).ToList() : Level.current.things[typeof(ISequenceItem)].Where((Thing x) => x.sequence.order == Group.value).ToList()));
            IEnumerable<Thing> future = null;
            IEnumerable<Thing> current = null;
            while (true)
            {
                future = sequences.Where((Thing v) => v.sequence.isValid && v.sequence.order != _sequenceNumber && v.sequence.timesActivated <= _activationCycle);
                current = sequences.Where((Thing v) => v.sequence.isValid && v.sequence.order == _sequenceNumber);
                if (future.Count() > 0)
                {
                    _hadFutureItems = true;
                }
                if (!_hadFutureItems)
                {
                    break;
                }
                current = current.Where((Thing v) => v.sequence.timesActivated == _activationCycle);
                if (current.Count() == 0)
                {
                    if (!Ordered_Groups.value)
                    {
                        IEnumerable<Thing> otherSequences = sequences.Where((Thing x) => x.sequence.order != _sequenceNumber);
                        if (!Continuous.value)
                        {
                            otherSequences = otherSequences.Where((Thing x) => x.sequence.timesActivated <= _activationCycle);
                        }
                        else
                        {
                            foreach (Thing item2 in otherSequences)
                            {
                                item2.sequence.timesActivated = 0;
                            }
                        }
                        if (otherSequences.Count() > 0)
                        {
                            otherSequences = otherSequences.OrderBy((Thing x) => x.sequence.likelyhood + ChallengeRando.Int(8));
                            _sequenceNumber = otherSequences.ElementAt(0).sequence.order;
                            Max_Up.value = _originalMaxUp;
                            continue;
                        }
                    }
                    if (future.Count() == 0)
                    {
                        if (!Continuous.value)
                        {
                            _finished = true;
                            return;
                        }
                        if (Group.value < 0)
                        {
                            _sequenceNumber = 0;
                        }
                        _activationCycle++;
                        Max_Up.value = _originalMaxUp;
                    }
                    else
                    {
                        if (Group.value < 0)
                        {
                            _sequenceNumber++;
                        }
                        Max_Up.value = _originalMaxUp;
                    }
                    continue;
                }
                if (Group_Wait.value && _up.Count == 0)
                {
                    Max_Up.value = current.Count();
                }
                break;
            }
            int total = 0;
            List<SequenceItem> validSequences = new List<SequenceItem>();
            sequences = current.ToList();
            bool foundUnactivatedTarget = false;
            while (sequences.Count() > 0)
            {
                Thing t = sequences[ChallengeRando.Int(0, sequences.Count - 1)];
                sequences.Remove(t);
                SequenceItem item = t.sequence;
                if ((!item.activated || item.finished) && item.isValid)
                {
                    foundUnactivatedTarget = true;
                    if (item != _lastUp || foundNoSequences)
                    {
                        item.likelyhood++;
                        total += item.likelyhood;
                        validSequences.Add(item);
                    }
                }
            }
            if (!foundUnactivatedTarget)
            {
                break;
            }
            if (total == 0)
            {
                total = 1;
            }
            float rand = ChallengeRando.Float(1f);
            float build = 0f;
            if (validSequences.Count == 0)
            {
                foundNoSequences = true;
            }
            bool added = false;
            foreach (SequenceItem i in validSequences)
            {
                float span = (float)i.likelyhood / (float)total;
                if (rand > build && rand < build + span)
                {
                    i.randomMode = true;
                    i.Activate();
                    _totalUp++;
                    added = true;
                    _up.Add(i);
                    break;
                }
                build += span;
            }
            if (added && Max_Up.value == 0)
            {
                break;
            }
        }
    }
}
