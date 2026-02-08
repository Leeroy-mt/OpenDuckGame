using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[EditorGroup("Equipment")]
[BaggedProperty("isFatal", false)]
public class DuctTape : Equipment
{
    public DuctTape(float xval, float yval)
        : base(xval, yval)
    {
        _type = "gun";
        graphic = new Sprite("tape");
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-5f, -5f);
        collisionSize = new Vector2(10f, 10f);
        wearable = false;
        _editorName = "Tape";
        editorTooltip = "Taping things together is always a good time!";
    }

    public override void PressAction()
    {
        try
        {
            if (!base.isServerForObject)
            {
                return;
            }
            Holdable gun1 = Level.current.NearestThingFilter<Holdable>(Position, (Thing t) => t.owner == null && t != this && !(t is Equipment) && !(t is RagdollPart) && !(t is TapedGun) && (t as Holdable).tapeable);
            if (Distance(gun1) < 16f)
            {
                Level.Add(SmallSmoke.New(Position.X, Position.Y));
                Level.Add(SmallSmoke.New(Position.X, Position.Y));
                SFX.PlaySynchronized("equip", 0.8f);
                TapedGun taped = new TapedGun(0f, 0f);
                Thing.ExtraFondle(gun1, connection);
                taped.gun1 = gun1;
                gun1.owner = base.duck;
                Level.Add(taped);
                if (base.duck != null && base.held)
                {
                    base.duck.resetAction = true;
                    base.duck.GiveHoldable(taped);
                }
                Level.Remove(this);
            }
        }
        catch (Exception ex)
        {
            DevConsole.Log(DCSection.General, "Duct Tape exception DuctTape.PressAction:");
            DevConsole.Log(DCSection.General, ex.ToString());
        }
    }
}
