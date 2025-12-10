using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[EditorGroup("Arcade", EditorItemType.ArcadeNew)]
[BaggedProperty("isOnlineCapable", false)]
public class GoalType : Thing
{
    public enum Special
    {
        None,
        Survival,
        Suicide,
        Butterfingers
    }

    public enum Result
    {
        None,
        Win,
        Lose
    }

    public EditorProperty<bool> Penalize_Misses = new EditorProperty<bool>(val: false);

    public EditorProperty<Special> Mode = new EditorProperty<Special>(Special.None);

    private HashSet<Thing> _trackedObjects = new HashSet<Thing>();

    private HashSet<Thing> _finishedObjects = new HashSet<Thing>();

    private ChallengeMode challenge;

    public Type contains { get; set; }

    public int numObjectsRemaining
    {
        get
        {
            UpdateTrackedObjects();
            return _trackedObjects.Count;
        }
    }

    public GoalType()
    {
        graphic = new Sprite("swirl");
        center = new Vec2(8f, 8f);
        collisionSize = new Vec2(16f, 16f);
        collisionOffset = new Vec2(-8f, -8f);
        _canFlip = false;
        _visibleInGame = false;
        Penalize_Misses._tooltip = "If true, points will be lost for every 'Destroy' object that falls off the level.";
        Mode._tooltip = "Special win/lose conditions. Butterfingers should be paired with an Equipper!";
        editorTooltip = "A special Target goal for your challenge (Destroying Crates, for example).";
    }

    public void UpdateTrackedObjects()
    {
        if (!(contains != null))
        {
            return;
        }
        foreach (Thing t in base.level.things[contains])
        {
            _trackedObjects.Add(t);
        }
    }

    public Result Check()
    {
        if (Mode.value == Special.Survival)
        {
            foreach (Duck d in Level.current.things[typeof(Duck)])
            {
                if (!(d is TargetDuck) && d.dead)
                {
                    return Result.Lose;
                }
            }
            return Result.Win;
        }
        if (Mode.value == Special.Suicide)
        {
            foreach (Duck d2 in Level.current.things[typeof(Duck)])
            {
                if (!(d2 is TargetDuck) && d2.dead)
                {
                    return Result.Win;
                }
            }
            return Result.None;
        }
        if (Mode.value == Special.Butterfingers)
        {
            foreach (Duck item in Level.current.things[typeof(Duck)])
            {
                if (item.holdObject == null)
                {
                    return Result.Lose;
                }
            }
            return Result.Win;
        }
        return Result.Win;
    }

    public override void Update()
    {
        if (contains != null)
        {
            UpdateTrackedObjects();
            for (int i = 0; i < _trackedObjects.Count; i++)
            {
                Thing t = _trackedObjects.ElementAt(i);
                if (_finishedObjects.Contains(t) || !(t is PhysicsObject) || !ChallengeLevel.running)
                {
                    continue;
                }
                if ((t as PhysicsObject).destroyed || (t as PhysicsObject)._ruined)
                {
                    ChallengeLevel.targetsShot++;
                    _finishedObjects.Add(t);
                    _trackedObjects.Remove(t);
                    i--;
                }
                else
                {
                    if (t.level != null)
                    {
                        continue;
                    }
                    if (Penalize_Misses.value)
                    {
                        if (ChallengeLevel.targetsShot > 0)
                        {
                            ChallengeLevel.targetsShot--;
                            SFX.Play("badBeep", 1f, 0.4f);
                        }
                    }
                    else
                    {
                        ChallengeLevel.targetsShot++;
                        _finishedObjects.Add(t);
                    }
                    _trackedObjects.Remove(t);
                    i--;
                }
            }
        }
        base.Update();
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk binaryClassChunk = base.Serialize();
        binaryClassChunk.AddProperty("contains", Editor.SerializeTypeName(contains));
        return binaryClassChunk;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        contains = Editor.DeSerializeTypeName(node.GetProperty<string>("contains"));
        return true;
    }

    public override ContextMenu GetContextMenu()
    {
        FieldBinding binding = new FieldBinding(this, "contains");
        EditorGroupMenu obj = base.GetContextMenu() as EditorGroupMenu;
        EditorGroupMenu contain = new EditorGroupMenu(obj);
        contain.InitializeTypelist(typeof(PhysicsObject), binding);
        contain.text = "Destroy";
        contain.tooltip = "Type of object that player needs to destroy (Counts as a 'Target' in challenge settings.)";
        obj.AddItem(contain);
        return obj;
    }
}
