using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

[BaggedProperty("canSpawn", false)]
[BaggedProperty("isFatal", false)]
public class TapedGun : Gun
{
    public StateBinding _gun1Binding = new StateBinding(nameof(gun1));

    public StateBinding _gun2Binding = new StateBinding(nameof(gun2));

    private Holdable _gun1;

    private Holdable _gun2;

    private int tapeDepth;

    public bool taping = true;

    private Sprite _tape;

    private bool _firstCalc = true;

    public override bool immobilizeOwner
    {
        get
        {
            if (gun1 == null || !gun1.immobilizeOwner)
            {
                if (gun2 != null)
                {
                    return gun2.immobilizeOwner;
                }
                return false;
            }
            return true;
        }
        set
        {
            _immobilizeOwner = value;
        }
    }

    public Holdable gun1
    {
        get
        {
            return _gun1;
        }
        set
        {
            if (_gun1 != null)
            {
                _gun1.owner = null;
                _gun1.enablePhysics = true;
                _gun1.tape = null;
            }
            _gun1 = value;
            if (_gun1 != null)
            {
                _gun1.owner = this;
                _gun1.enablePhysics = false;
                _gun1.tape = this;
            }
            UpdateGunOwners();
        }
    }

    public Holdable gun2
    {
        get
        {
            return _gun2;
        }
        set
        {
            if (_gun2 != null)
            {
                _gun2.owner = null;
                _gun2.enablePhysics = true;
                _gun2.tape = null;
            }
            _gun2 = value;
            if (_gun2 != null)
            {
                _gun2.owner = this;
                _gun2.enablePhysics = false;
                _gun2.tape = this;
            }
            UpdateGunOwners();
        }
    }

    public override bool action
    {
        get
        {
            if (taping)
            {
                return false;
            }
            return base.action;
        }
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk binaryClassChunk = base.Serialize();
        binaryClassChunk.AddProperty("gun1", (gun1 != null) ? gun1.Serialize() : null);
        binaryClassChunk.AddProperty("gun2", (gun2 != null) ? gun2.Serialize() : null);
        return binaryClassChunk;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        BinaryClassChunk g1 = node.GetProperty<BinaryClassChunk>("gun1");
        if (g1 != null)
        {
            gun1 = Thing.LoadThing(g1, chance: false) as Holdable;
            if (gun1 != null)
            {
                gun1.tape = this;
            }
        }
        BinaryClassChunk g2 = node.GetProperty<BinaryClassChunk>("gun2");
        if (g2 != null)
        {
            gun2 = Thing.LoadThing(g2, chance: false) as Holdable;
            if (gun2 != null)
            {
                gun2.tape = this;
            }
        }
        return base.Deserialize(node);
    }

    public override void Initialize()
    {
        if (gun1 != null)
        {
            base.level.AddThing(gun1);
        }
        if (gun2 != null)
        {
            base.level.AddThing(gun2);
        }
        base.Initialize();
    }

    public TapedGun(float xval, float yval)
        : base(xval, yval)
    {
        _ammoType = new ATDefault();
        ammo = 99;
        _type = "gun";
        graphic = new Sprite("tinyGun");
        _barrelOffsetTL = new Vector2(20f, 15f);
        _fireSound = "littleGun";
        _kickForce = 0f;
        _fireRumble = RumbleIntensity.Kick;
        _tape = new Sprite("tapePiece");
        _tape.CenterOrigin();
    }

    public void UpdateChildren()
    {
        if (gun2 != null)
        {
            gun2.UpdateTaped(this);
        }
        if (gun1 == null)
        {
            return;
        }
        gun1.UpdateTaped(this);
        if (gun1.removeFromLevel || (!taping && gun2 != null && gun2.removeFromLevel))
        {
            if (!gun1.removeFromLevel)
            {
                gun1.owner = owner;
            }
            if (gun2 != null && !gun2.removeFromLevel)
            {
                gun2.owner = owner;
            }
            gun1.tape = null;
            if (gun2 != null)
            {
                gun2.tape = null;
            }
            gun1 = null;
            gun2 = null;
            Level.Remove(this);
        }
    }

    public void PreUpdatePositioning()
    {
        if (gun1 != null)
        {
            gun1.PreUpdateTapedPositioning(this);
        }
        if (gun2 != null)
        {
            gun2.PreUpdateTapedPositioning(this);
        }
    }

    public void UpdatePositioning()
    {
        ammo = 0;
        buoyancy = 0f;
        onlyFloatInLava = false;
        flammable = 0f;
        weight = 0f;
        heat = 0f;
        base.bouncy = 0f;
        kick = 0f;
        base.dontCrush = true;
        float splitSize = ((gun1 is TapedGun || gun2 is TapedGun) ? 4f : 3f);
        float height = 0f;
        if (gun1 != null)
        {
            height += gun1.height;
            gun1.tape = this;
        }
        if (gun2 != null)
        {
            height += gun2.height;
            taping = false;
            gun2.tape = this;
        }
        splitSize = height / 6f;
        height -= splitSize * 2f;
        if (gun1 != null)
        {
            if (gun1.angleMul != 1f)
            {
                angleMul = gun1.angleMul;
            }
            if (gun1.addVerticalTapeOffset)
            {
                gun1.Position = Offset(new Vector2(0f, splitSize) + gun1.tapedOffset);
            }
            else
            {
                gun1.Position = Offset(gun1.tapedOffset);
            }
            gun1.Depth = base.Depth - 8;
            if (taping)
            {
                gun1.AngleDegrees = base.AngleDegrees + (float)(90 * offDir);
                gun1.offDir = offDir;
            }
            else
            {
                gun1.AngleDegrees = base.AngleDegrees - 180f;
                gun1.offDir = (sbyte)(-offDir);
            }
            gun1.grounded = base.grounded;
            gun1.hSpeed = hSpeed;
            gun1.vSpeed = vSpeed;
            gun1.lastGrounded = lastGrounded;
            gun1.clip = new HashSet<MaterialThing>(base.clip);
            if (gun1 is TapedGun)
            {
                (gun1 as TapedGun).tapeDepth = tapeDepth + 1;
                (gun1 as TapedGun).UpdatePositioning();
            }
            if (!gun1.removeFromLevel)
            {
                ammo += ((!(gun1 is Gun)) ? 1 : (gun1 as Gun).ammo);
                buoyancy += gun1.buoyancy;
                if (gun1.onlyFloatInLava)
                {
                    onlyFloatInLava = true;
                }
                flammable += gun1.flammable;
                weight = Math.Max(gun1.weight, weight);
                heat += gun1.heat / 2f;
                if (!gun1.dontCrush && gun1.weight >= 5f)
                {
                    base.dontCrush = false;
                }
            }
            gun1.UpdateTapedPositioning(this);
            if (!(gun1 is Gun))
            {
                float val = 1f - ((float)Math.Sin(Maths.DegToRad(gun1.AngleDegrees + 90f)) + 1f) / 2f;
                gun1._extraOffset.Y = val * (gun1.collisionOffset.Y + gun1.collisionSize.Y + gun1.collisionOffset.Y);
            }
            if (gun1 is Gun)
            {
                kick += (gun1 as Gun).kick;
            }
            if (gun1.bouncy > base.bouncy)
            {
                base.bouncy = gun1.bouncy;
            }
        }
        if (gun2 != null)
        {
            if (gun2.angleMul != 1f)
            {
                angleMul = gun2.angleMul;
            }
            if (gun2.addVerticalTapeOffset)
            {
                gun2.Position = Offset(new Vector2(0f, 0f - splitSize) + gun2.tapedOffset);
            }
            else
            {
                gun2.Position = Offset(gun2.tapedOffset);
            }
            gun2.Depth = base.Depth - 4;
            gun2.AngleDegrees = base.AngleDegrees;
            gun2.offDir = offDir;
            gun2.grounded = base.grounded;
            gun2.hSpeed = hSpeed;
            gun2.vSpeed = vSpeed;
            gun2.lastGrounded = lastGrounded;
            gun2.clip = new HashSet<MaterialThing>(base.clip);
            if (gun2 is TapedGun)
            {
                (gun2 as TapedGun).tapeDepth = tapeDepth + 1;
                (gun2 as TapedGun).UpdatePositioning();
            }
            if (!gun2.removeFromLevel)
            {
                ammo += ((!(gun2 is Gun)) ? 1 : (gun2 as Gun).ammo);
                buoyancy += gun2.buoyancy;
                if (gun2.onlyFloatInLava)
                {
                    onlyFloatInLava = true;
                }
                flammable += gun2.flammable;
                weight = Math.Max(gun2.weight, weight);
                heat += gun2.heat / 2f;
                if (!gun2.dontCrush && gun2.weight >= 5f)
                {
                    base.dontCrush = false;
                }
            }
            gun2.UpdateTapedPositioning(this);
            if (!(gun2 is Gun))
            {
                float val2 = 1f - ((float)Math.Sin(Maths.DegToRad(gun2.AngleDegrees + 90f)) + 1f) / 2f;
                gun2._extraOffset.Y = val2 * (gun2.collisionOffset.Y + gun2.collisionSize.Y + gun2.collisionOffset.Y);
            }
            if (gun2 is Gun)
            {
                kick += (gun2 as Gun).kick;
            }
            if (gun2.bouncy > base.bouncy)
            {
                base.bouncy = gun2.bouncy;
            }
        }
        if (ammo > 100)
        {
            ammo = 100;
        }
        if (weight > 8f)
        {
            weight = 8f;
        }
        Center = new Vector2(16f, 16f);
        if (gun1 != null && gun2 != null)
        {
            if (_firstCalc)
            {
                float val3 = 1f - ((float)Math.Sin(Maths.DegToRad(gun1.AngleDegrees + 90f)) + 1f) / 2f;
                gun1._extraOffset.Y = val3 * (gun1.collisionOffset.Y + gun1.collisionSize.Y + gun1.collisionOffset.Y);
                val3 = 1f - ((float)Math.Sin(Maths.DegToRad(gun2.AngleDegrees + 90f)) + 1f) / 2f;
                gun2._extraOffset.Y = val3 * (gun2.collisionOffset.Y + gun2.collisionSize.Y + gun2.collisionOffset.Y);
                float highest = Math.Min(gun1.top - gun1._extraOffset.Y, gun2.top - gun2._extraOffset.Y);
                float lowest = Math.Max(gun1.bottom - gun1._extraOffset.Y, gun2.bottom - gun2._extraOffset.Y);
                float highDif = base.Y - highest;
                collisionOffset = new Vector2(-6f, 0f - highDif);
                collisionSize = new Vector2(12f, lowest - highest);
                _firstCalc = false;
            }
        }
        else
        {
            collisionOffset = new Vector2(-6f, 0f - height / 2f);
            collisionSize = new Vector2(12f, height);
        }
    }

    public override void Terminate()
    {
        if (gun1 != null)
        {
            Level.Remove(gun1);
        }
        if (gun2 != null)
        {
            Level.Remove(gun2);
        }
        base.Terminate();
    }

    public void UpdateSubActions(bool pAction)
    {
        UpdatePositioning();
        if (gun1 != null)
        {
            gun1.triggerAction = pAction;
            gun1.UpdateAction();
        }
        if (gun2 != null)
        {
            gun2.triggerAction = pAction;
            gun2.UpdateAction();
        }
    }

    private void UpdateGunOwners()
    {
        if (base.duck != null)
        {
            if (taping)
            {
                base.duck.resetAction = true;
            }
            if (gun1 != null)
            {
                gun1.owner = base.duck;
            }
            if (gun2 != null)
            {
                gun2.owner = base.duck;
            }
        }
        else
        {
            if (gun1 != null)
            {
                gun1.owner = this;
            }
            if (gun2 != null)
            {
                gun2.owner = this;
            }
        }
    }

    public override void Update()
    {
        UpdateGunOwners();
        if (base.isServerForObject)
        {
            if (gun1 != null && !gun1.isServerForObject)
            {
                Fondle(gun1);
            }
            if (gun2 != null && !gun2.isServerForObject)
            {
                Fondle(gun2);
            }
        }
        try
        {
            if (base.isServerForObject && taping && base.duck != null && base.duck.inputProfile != null && base.duck.inputProfile.Pressed("SHOOT"))
            {
                Holdable g = Level.current.NearestThingFilter<Holdable>(Position, (Thing t) => t.owner == null && t != this && t != gun1 && !(t is Equipment) && !(t is RagdollPart) && !(t is TapedGun) && (t as Holdable).tapeable && (gun1 == null || gun1.CanTapeTo(t)));
                if (Distance(g) < 16f)
                {
                    Level.Add(SmallSmoke.New(Position.X, Position.Y));
                    Level.Add(SmallSmoke.New(Position.X, Position.Y));
                    SFX.PlaySynchronized("equip", 0.8f);
                    Thing.ExtraFondle(g, connection);
                    gun2 = g;
                    gun2.owner = base.duck;
                    taping = false;
                    if (base.duck != null)
                    {
                        base.duck.resetAction = true;
                    }
                    Holdable monster = gun1.BecomeTapedMonster(this);
                    if (monster != null)
                    {
                        Thing.Fondle(monster, DuckNetwork.localConnection);
                        monster.Position = Position;
                        Level.Add(monster);
                        if (base.duck != null)
                        {
                            base.duck.GiveHoldable(monster);
                        }
                        Thing.Fondle(this, DuckNetwork.localConnection);
                        Thing.Fondle(gun1, DuckNetwork.localConnection);
                        Thing.Fondle(gun2, DuckNetwork.localConnection);
                        Level.Remove(gun1);
                        Level.Remove(gun2);
                        Level.Remove(this);
                    }
                    else if (gun1.tapedIndexPreference >= 0 && gun1.tapedIndexPreference != 0)
                    {
                        Holdable g2 = gun2;
                        gun2 = gun1;
                        gun1 = g2;
                    }
                    else if (gun2.tapedIndexPreference >= 0 && gun2.tapedIndexPreference != 1)
                    {
                        Holdable g3 = gun1;
                        gun1 = gun2;
                        gun2 = g3;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            DevConsole.Log(DCSection.General, "Duct Tape exception TapedGun.Update:");
            DevConsole.Log(DCSection.General, ex.ToString());
        }
        PreUpdatePositioning();
        base.Update();
        UpdatePositioning();
        UpdateChildren();
        DoFloat();
    }

    public override void Draw()
    {
        if (base.duck != null && taping)
        {
            base.duck.resetAction = true;
        }
        UpdatePositioning();
        _tape.Depth = base.Depth + 16;
        _tape.AngleDegrees = base.AngleDegrees;
        _tape.flipH = offDir < 0;
        new Vector2(0f, base.bottom - base.top);
        if (gun2 != null)
        {
            Vector2 pos = gun2.Offset(new Vector2(0f, 0f - collisionOffset.Y / 2f));
            Graphics.Draw(_tape, pos.X, pos.Y);
        }
        else
        {
            Graphics.Draw(_tape, Position.X, Position.Y);
        }
        if (base.level == null || Duck.renderingIcon)
        {
            if (gun1 != null)
            {
                gun1.Draw();
            }
            if (gun2 != null)
            {
                gun2.Draw();
            }
        }
    }

    public override void Burn(Vector2 firePosition, Thing litBy)
    {
        try
        {
            if (gun1 != null && gun1.flammable > 0f)
            {
                gun1.Burn(firePosition, litBy);
            }
            if (gun2 != null && gun2.flammable > 0f)
            {
                gun2.Burn(firePosition, litBy);
            }
        }
        catch (Exception ex)
        {
            DevConsole.Log(DCSection.General, "Duct Tape exception TapedGun.Burn:");
            DevConsole.Log(DCSection.General, ex.ToString());
        }
    }

    public override void DoHeatUp(float val, Vector2 location)
    {
        try
        {
            if (gun1 != null)
            {
                gun1.DoHeatUp(val, location);
            }
            if (gun2 != null)
            {
                gun2.DoHeatUp(val, location);
            }
        }
        catch (Exception ex)
        {
            DevConsole.Log(DCSection.General, "Duct Tape exception TapedGun.DoHeatUp:");
            DevConsole.Log(DCSection.General, ex.ToString());
        }
    }

    public override void OnPressAction()
    {
        base.OnPressAction();
    }

    public override void Fire()
    {
    }

    public override void Impact(MaterialThing with, ImpactedFrom from, bool solidImpact)
    {
        try
        {
            if (gun1 != null)
            {
                gun1.hSpeed = hSpeed;
                gun1.vSpeed = vSpeed;
                gun1.Impact(with, from, solidImpact);
            }
            if (gun2 != null)
            {
                gun2.hSpeed = hSpeed;
                gun2.vSpeed = vSpeed;
                gun2.Impact(with, from, solidImpact);
            }
            base.Impact(with, from, solidImpact);
        }
        catch (Exception ex)
        {
            DevConsole.Log(DCSection.General, "Duct Tape exception TapedGun.Impact:");
            DevConsole.Log(DCSection.General, ex.ToString());
        }
    }
}
