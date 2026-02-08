using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Arcade|Targets", EditorItemType.ArcadeNew)]
[BaggedProperty("canSpawn", false)]
public class TriggerVolume : MaterialThing, ISequenceItem
{
    public EditorProperty<int> Order = new EditorProperty<int>(-1, null, -1f, 256f, 1f, "RANDOM");

    public EditorProperty<int> Wide = new EditorProperty<int>(16, null, 16f, 1024f, 1f);

    public EditorProperty<int> High = new EditorProperty<int>(16, null, 16f, 1024f, 1f);

    public EditorProperty<bool> Untouch = new EditorProperty<bool>(val: false);

    public EditorProperty<bool> Ducks_Only = new EditorProperty<bool>(val: true);

    public EditorProperty<bool> Is_Goody = new EditorProperty<bool>(val: false);

    private HashSet<PhysicsObject> _touching = new HashSet<PhysicsObject>();

    private bool _hidden;

    public override void EditorPropertyChanged(object property)
    {
        if (!(Level.current is Editor))
        {
            Center = new Vector2(8f, 8f);
            collisionSize = new Vector2((int)Wide, (int)High);
            collisionOffset = new Vector2(-((int)Wide / 2), -((int)High / 2));
        }
    }

    public TriggerVolume(float xpos, float ypos)
        : base(xpos, ypos)
    {
        base.sequence = new SequenceItem(this);
        base.sequence.type = SequenceItemType.Goody;
        enablePhysics = false;
        _impactThreshold = 1E-06f;
        graphic = new SpriteMap("challenge/goody", 16, 16);
        Center = new Vector2(8f, 8f);
        (graphic as SpriteMap).frame = 3;
        collisionOffset = new Vector2(-4f, -4f);
        collisionSize = new Vector2(8f, 8f);
        _contextMenuFilter.Add("Sequence");
        _editorName = "Trigger Volume";
        editorTooltip = "Pretty much an invisible Goody that you can resize.";
        Untouch._tooltip = "If enabled, the volume only triggers when a Duck leaves it.";
        Ducks_Only._tooltip = "If enabled, only Ducks trigger this volume. Otherwise all physics objects do.";
        Is_Goody._tooltip = "If enabled, this volume will count as a collected Goody when triggered.";
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (_hidden || !(with is PhysicsObject) || (Ducks_Only.value && !(with is Duck) && !(with is RagdollPart) && !(with is TrappedDuck)) || with.destroyed || Level.current is Editor)
        {
            return;
        }
        if (Untouch.value)
        {
            _touching.Add(with as PhysicsObject);
        }
        else if (!_touching.Contains(with as PhysicsObject))
        {
            _sequence.Finished();
            if (ChallengeLevel.running && Is_Goody.value)
            {
                ChallengeLevel.goodiesGot++;
            }
            _touching.Add(with as PhysicsObject);
        }
    }

    public override void Update()
    {
        bool broke = false;
        foreach (PhysicsObject o in _touching)
        {
            if (Collision.Rect(base.rectangle, o.rectangle))
            {
                continue;
            }
            if (Untouch.value)
            {
                _sequence.Finished();
                if (ChallengeLevel.running && Is_Goody.value)
                {
                    ChallengeLevel.goodiesGot++;
                }
            }
            broke = true;
        }
        if (broke)
        {
            _touching.Clear();
        }
        base.Update();
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            base.sequence.order = Order.value;
            if (base.sequence.order == -1)
            {
                base.sequence.order = Rando.Int(256);
            }
            base.sequence.waitTillOrder = true;
            Center = new Vector2(8f, 8f);
            collisionSize = new Vector2((int)Wide, (int)High);
            collisionOffset = new Vector2(-((int)Wide / 2), -((int)High / 2));
        }
        if (!(Level.current is Editor) && base.sequence.waitTillOrder && base.sequence.order != 0)
        {
            visible = false;
            _hidden = true;
        }
        base.Initialize();
    }

    public override void OnSequenceActivate()
    {
        if (base.sequence.waitTillOrder)
        {
            visible = true;
            _hidden = false;
        }
        base.OnSequenceActivate();
    }

    public override void Draw()
    {
        if (Level.current is Editor)
        {
            base.Draw();
            if (!Editor.editorDraw)
            {
                float wid = Wide.value;
                float hig = High.value;
                Graphics.DrawRect(Position + new Vector2((0f - wid) / 2f, (0f - hig) / 2f), Position + new Vector2(wid / 2f, hig / 2f), Colors.DGGreen * 0.5f, 1f, filled: false);
            }
        }
    }
}
