using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[EditorGroup("Special", EditorItemType.Arcade)]
[BaggedProperty("isOnlineCapable", false)]
public class RandomController : Thing
{
    public EditorProperty<int> max_up = new EditorProperty<int>(1, null, 1f, 32f, 1f);

    public EditorProperty<float> wait = new EditorProperty<float>(0f, null, 0f, 100f);

    private float _waitCount;

    private bool _started;

    private int _totalUp;

    private List<SequenceItem> _up = new List<SequenceItem>();

    private SequenceItem _lastUp;

    public static bool isRand;

    public RandomController()
    {
        graphic = new Sprite("swirl");
        center = new Vec2(8f, 8f);
        collisionSize = new Vec2(16f, 16f);
        collisionOffset = new Vec2(-8f, -8f);
        _canFlip = false;
        _visibleInGame = false;
    }

    public override void Update()
    {
        if (!Level.current.simulatePhysics)
        {
            return;
        }
        if (!_started)
        {
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
                    i--;
                }
            }
        }
        if (_up.Count >= (int)max_up)
        {
            return;
        }
        if (_up.Count == 0)
        {
            PopUpItems();
            _waitCount = 0f;
            return;
        }
        _waitCount += Maths.IncFrameTimer();
        if (_waitCount >= wait.value)
        {
            _waitCount = 0f;
            PopUpItems();
        }
    }

    private void PopUpItems()
    {
        bool foundNoSequences = false;
        while (_up.Count < max_up.value)
        {
            List<Thing> sequences = Level.current.things[typeof(ISequenceItem)].ToList();
            sequences.RemoveAll((Thing v) => !v.sequence.isValid);
            if (_up.Count >= sequences.Count)
            {
                break;
            }
            int total = 0;
            List<SequenceItem> validSequences = new List<SequenceItem>();
            bool foundUnactivatedTarget = false;
            while (sequences.Count > 0)
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
            foreach (SequenceItem i in validSequences)
            {
                float span = (float)i.likelyhood / (float)total;
                if (rand > build && rand < build + span)
                {
                    i.randomMode = true;
                    isRand = true;
                    i.Activate();
                    isRand = false;
                    _totalUp++;
                    _up.Add(i);
                    break;
                }
                build += span;
            }
        }
    }
}
