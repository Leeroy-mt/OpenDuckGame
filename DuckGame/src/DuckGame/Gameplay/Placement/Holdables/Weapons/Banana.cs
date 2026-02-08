using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[EditorGroup("Guns|Explosives")]
[BaggedProperty("isFatal", false)]
public class Banana : Gun
{
    public StateBinding _bananaStateBinding = new BananaFlagBinding();

    private SpriteMap _sprite;

    public bool _pin = true;

    public bool _thrown;

    private bool _fade;

    private bool _splatted;

    public bool pin => _pin;

    public override float Angle
    {
        get
        {
            if (owner != null)
            {
                if (offDir > 0)
                {
                    return base.Angle + (float)Math.PI / 2f;
                }
                return base.Angle - (float)Math.PI / 2f;
            }
            return base.Angle;
        }
        set
        {
            AngleValue = value;
        }
    }

    public Banana(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 1;
        _ammoType = new ATShrapnel();
        _type = "gun";
        _sprite = new SpriteMap("banana", 16, 16);
        graphic = _sprite;
        Center = new Vector2(8f, 13f);
        collisionOffset = new Vector2(-6f, -3f);
        collisionSize = new Vector2(12f, 5f);
        _fireRumble = RumbleIntensity.Kick;
        _holdOffset = new Vector2(-1f, 2f);
        base.bouncy = 0.4f;
        friction = 0.05f;
        physicsMaterial = PhysicsMaterial.Rubber;
        editorTooltip = "A tactically placed banana peel can cause major injuries.";
        isFatal = false;
    }

    public override void Update()
    {
        base.Update();
        if (_thrown && owner == null)
        {
            _thrown = false;
            if (Math.Abs(hSpeed) + Math.Abs(vSpeed) > 0.4f)
            {
                base.AngleDegrees = 180f;
            }
        }
        if (!_pin && owner == null && !_fade)
        {
            _sprite.frame = 2;
            weight = 0.1f;
        }
        if (_fade)
        {
            base.Alpha -= 0.1f;
            if (base.Alpha <= 0f)
            {
                Level.Remove(this);
                base.Alpha = 0f;
            }
        }
        if (!_pin && owner == null)
        {
            canPickUp = false;
        }
        if (!_pin && _grounded && !_fade)
        {
            if (!_splatted)
            {
                _splatted = true;
                if (Network.isActive)
                {
                    if (base.isServerForObject)
                    {
                        NetSoundEffect.Play("bananaSplat");
                    }
                }
                else
                {
                    SFX.Play("smallSplat", 1f, Rando.Float(-0.2f, 0.2f));
                }
            }
            base.AngleDegrees = 0f;
            canPickUp = false;
            foreach (Duck o in Level.CheckLineAll<Duck>(new Vector2(base.X - 5f, base.Y + 2f), new Vector2(base.X + 5f, base.Y + 2f)))
            {
                if (!o.grounded || o.crouch || o.sliding || o.bottom > base.bottom + 2f || !o.isServerForObject || !(Math.Abs(o.hSpeed) > 2.5f))
                {
                    continue;
                }
                RumbleManager.AddRumbleEvent(o.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.None));
                o.Fondle(this);
                if (Network.isActive)
                {
                    if (base.isServerForObject)
                    {
                        NetSoundEffect.Play("bananaSlip");
                    }
                    if (Teams.active.Count > 1 && Rando.Int(100) == 1 && o.connection == DuckNetwork.localConnection)
                    {
                        DuckNetwork.GiveXP("Banana Man", 0, 5, 4, 20, 30, 40);
                    }
                }
                else
                {
                    SFX.Play("slip", 1f, Rando.Float(-0.2f, 0.2f));
                }
                if (o.hSpeed < 0f)
                {
                    o.hSpeed -= 1.5f;
                }
                else
                {
                    o.hSpeed += 1.5f;
                }
                o.vSpeed -= 2.5f;
                hSpeed = (0f - o.hSpeed) * 0.4f;
                friction = 0.05f;
                weight = 0.01f;
                o.crippleTimer = 1.5f;
                PhysicsObject thing = o.holdObject;
                if (thing != null)
                {
                    o.ThrowItem(throwWithForce: false);
                    thing.vSpeed -= 4f;
                    thing.hSpeed = o.hSpeed * 0.8f;
                    thing.clip.Add(o);
                    o.clip.Add(thing);
                }
                o.GoRagdoll();
                if (o.ragdoll != null && o.ragdoll.part1 != null && o.ragdoll.part2 != null && o.ragdoll.part3 != null)
                {
                    if (thing != null)
                    {
                        o.ragdoll.part1.clip.Add(thing);
                        o.ragdoll.part2.clip.Add(thing);
                        o.ragdoll.part3.clip.Add(thing);
                        thing.clip.Add(o.ragdoll.part1);
                        thing.clip.Add(o.ragdoll.part2);
                        thing.clip.Add(o.ragdoll.part3);
                    }
                    o.ragdoll.part1.hSpeed *= 0.5f;
                    o.ragdoll.part3.hSpeed *= 1.5f;
                }
                _sprite.frame = 3;
                _fade = true;
                Level.Add(new BananaSlip(base.X, base.Y + 2f, o.offDir > 0));
            }
        }
        if (_triggerHeld)
        {
            if (base.duck != null)
            {
                base.duck.quack = 20;
                if (offDir > 0)
                {
                    handAngle = -1.0995574f;
                    handOffset = new Vector2(8f, -1f);
                    _holdOffset = new Vector2(-1f, 10f);
                }
                else
                {
                    handAngle = 1.0995574f;
                    handOffset = new Vector2(8f, -1f);
                    _holdOffset = new Vector2(-1f, 10f);
                }
            }
        }
        else
        {
            handAngle = 0f;
            handOffset = new Vector2(0f, 0f);
            _holdOffset = new Vector2(-1f, 2f);
        }
    }

    public override void HeatUp(Vector2 location)
    {
    }

    public void EatBanana()
    {
        _sprite.frame = 1;
        _pin = false;
        _holdOffset = new Vector2(-2f, 3f);
        collisionOffset = new Vector2(-4f, -2f);
        collisionSize = new Vector2(8f, 4f);
        weight = 0.01f;
        if (base.duck != null)
        {
            RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(_fireRumble, RumbleDuration.Pulse, RumbleFalloff.None));
        }
        if (Network.isActive)
        {
            if (base.isServerForObject)
            {
                NetSoundEffect.Play("bananaEat");
            }
        }
        else
        {
            SFX.Play("smallSplat", 1f, Rando.Float(-0.6f, 0.6f));
        }
        base.bouncy = 0f;
        friction = 0.3f;
    }

    public override void OnPressAction()
    {
        if (pin)
        {
            EatBanana();
        }
    }

    public override void OnHoldAction()
    {
    }

    public override void OnReleaseAction()
    {
    }
}
