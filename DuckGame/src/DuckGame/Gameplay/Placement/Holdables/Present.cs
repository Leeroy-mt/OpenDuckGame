using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Stuff|Props")]
public class Present : Holdable, IPlatform
{
    private SpriteMap _sprite;

    private Type _contains;

    public Present(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("presents", 16, 16);
        _sprite.frame = Rando.Int(0, 7);
        graphic = _sprite;
        center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-7f, -4f);
        collisionSize = new Vec2(14f, 11f);
        base.depth = -0.5f;
        thickness = 0f;
        weight = 3f;
        flammable = 0.3f;
        charThreshold = 0.5f;
        base.collideSounds.Add("presentLand");
        editorTooltip = "You never know what you'll find inside! Spawns a random item once.";
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (type is DTIncinerate && base.isServerForObject)
        {
            SFX.Play("flameExplode");
            for (int i = 0; i < 3; i++)
            {
                Level.Add(SmallSmoke.New(base.x + Rando.Float(-2f, 2f), base.y + Rando.Float(-2f, 2f)));
            }
            Holdable h = SpawnPresent(null);
            if (h != null)
            {
                h.velocity = Rando.Vec2(-1f, 1f, -2f, 0f);
            }
            Level.Remove(this);
        }
        return base.OnDestroy(type);
    }

    public override void Initialize()
    {
        List<Type> things = ItemBox.GetPhysicsObjects(Editor.Placeables);
        things.RemoveAll((Type t) => t == typeof(Present) || t == typeof(LavaBarrel) || t == typeof(Grapple));
        _contains = things[Rando.Int(things.Count - 1)];
    }

    public static void OpenEffect(Vec2 pPosition, int pFrame, bool pIsNetMessage)
    {
        Level.Add(new OpenPresent(pPosition.x, pPosition.y, pFrame));
        for (int i = 0; i < 4; i++)
        {
            Level.Add(SmallSmoke.New(pPosition.x + Rando.Float(-2f, 2f), pPosition.y + Rando.Float(-2f, 2f)));
        }
        SFX.Play("harp", 0.8f);
        if (!pIsNetMessage)
        {
            Send.Message(new NMPresentOpen(pPosition, (byte)pFrame));
        }
    }

    public Holdable SpawnPresent(Thing pOwner)
    {
        if (base.isServerForObject)
        {
            if (_contains == null)
            {
                Initialize();
            }
            Holdable newThing = Editor.CreateThing(_contains) as Holdable;
            if (newThing != null)
            {
                Duck d = pOwner as Duck;
                if (Rando.Int(500) == 1 && newThing is Gun && (newThing as Gun).CanSpawnInfinite())
                {
                    (newThing as Gun).infiniteAmmoVal = true;
                    (newThing as Gun).infinite.value = true;
                }
                if (pOwner != null)
                {
                    newThing.x = pOwner.x;
                    newThing.y = pOwner.y;
                }
                else
                {
                    newThing.x = base.x;
                    newThing.y = base.y;
                }
                Level.Add(newThing);
                if (d != null)
                {
                    d.GiveHoldable(newThing);
                    d.resetAction = true;
                }
            }
            return newThing;
        }
        return null;
    }

    public override void OnPressAction()
    {
        if (owner != null && base.isServerForObject)
        {
            Thing o = owner;
            Duck d = base.duck;
            if (d != null)
            {
                d.profile.stats.presentsOpened++;
                Global.data.presentsOpened.valueInt++;
                base.duck.ThrowItem();
            }
            Level.Remove(this);
            OpenEffect(position, _sprite.frame, pIsNetMessage: false);
            SpawnPresent(o);
        }
    }
}
