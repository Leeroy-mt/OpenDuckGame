using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Spawns")]
[BaggedProperty("isInDemo", false)]
[BaggedProperty("previewPriority", false)]
public class ItemBoxRandom : ItemBox
{
    public ItemBoxRandom(float xpos, float ypos)
        : base(xpos, ypos)
    {
        editorTooltip = "Spawns a random object each time it's used. Recharges after a short duration.";
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Draw()
    {
        _sprite.frame += 2;
        base.Draw();
        _sprite.frame -= 2;
    }

    public static PhysicsObject GetRandomItem()
    {
        List<Type> things = ItemBox.GetPhysicsObjects(Editor.Placeables);
        things.RemoveAll((Type t) => t == typeof(LavaBarrel) || t == typeof(Grapple) || t == typeof(Slag) || t == typeof(Holster));
        Type contains;
        if (Rando.Int(10000) == 0)
        {
            contains = typeof(PositronShooter);
            Options.Data.specialTimes = 100;
        }
        else
        {
            if (Options.Data.specialTimes > 0)
            {
                things.Add(typeof(PositronShooter));
                things.Add(typeof(PositronShooter));
                Options.Data.specialTimes--;
            }
            contains = things[Rando.Int(things.Count - 1)];
        }
        PhysicsObject newThing = Editor.CreateThing(contains) as PhysicsObject;
        if (Rando.Int(1000) == 1 && newThing is Gun && (newThing as Gun).CanSpawnInfinite())
        {
            (newThing as Gun).infiniteAmmoVal = true;
            (newThing as Gun).infinite.value = true;
        }
        if (newThing is Rock && Rando.Int(1000000) == 0)
        {
            newThing = Editor.CreateThing(typeof(SpawnedGoldRock)) as PhysicsObject;
        }
        return newThing;
    }

    public override PhysicsObject GetSpawnItem()
    {
        PhysicsObject p = GetRandomItem();
        base.contains = p.GetType();
        return p;
    }

    public override void DrawHoverInfo()
    {
    }
}
