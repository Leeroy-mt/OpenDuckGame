using System;

namespace DuckGame;

public abstract class Holdable : PhysicsObject
{
    public StateBinding _triggerHeldBinding = new StateBinding(nameof(_triggerHeld));

    public StateBinding _canPickUpBinding = new StateBinding(nameof(canPickUp));

    public float raiseSpeed = 0.2f;

    protected Vec2 _prevHoverPos;

    private ItemSpawner _hoverSpawner;

    protected Vec2 _lastReceivedPosition = Vec2.Zero;

    protected bool _immobilizeOwner;

    public bool _keepRaised;

    protected bool _canRaise = true;

    public bool hoverRaise = true;

    public bool ignoreHands;

    public bool hideRightWing;

    public bool hideLeftWing;

    public float angleMul = 1f;

    public Vec2 holsterOffset = Vec2.Zero;

    public float holsterAngle;

    public bool holsterable = true;

    protected bool _hasTrigger = true;

    public bool canStore = true;

    public bool canPickUp = true;

    public Vec2 handOffset;

    public Vec2 _holdOffset;

    public Vec2 _extraOffset = Vec2.Zero;

    public float handAngle;

    public bool handFlip;

    public bool triggerAction;

    public Duck _equippedDuck;

    protected bool _raised;

    public bool _triggerHeld;

    public bool tapeable = true;

    private Duck _disarmedFrom;

    private DateTime _disarmTime = DateTime.MinValue;

    public Depth _oldDepth;

    public bool _hasOldDepth;

    protected int _equippedDepth = 4;

    protected bool _fireActivated;

    public TapedGun tape;

    public int tapedIndexPreference = -1;

    public bool addVerticalTapeOffset = true;

    public float charThreshold = 100f;

    public override Thing netOwner
    {
        get
        {
            return owner;
        }
        set
        {
            _prevOwner = owner;
            _lastThrownBy = _owner;
            owner = value;
        }
    }

    public ItemSpawner hoverSpawner
    {
        get
        {
            return _hoverSpawner;
        }
        set
        {
            if (_hoverSpawner != null && value == null)
            {
                gravMultiplier = 1f;
            }
            else if (_hoverSpawner == null && value != null)
            {
                gravMultiplier = 0f;
            }
            if (value != null && _hoverSpawner != value)
            {
                _prevHoverPos = position;
            }
            _hoverSpawner = value;
        }
    }

    public override Vec2 netPosition
    {
        get
        {
            if (owner != null && !(this is DrumSet))
            {
                return new Vec2(-10000f, -8999f);
            }
            if (hoverSpawner == null)
            {
                return position;
            }
            return hoverSpawner.position + new Vec2(0f, -8f);
        }
        set
        {
            Math.Abs(value.x);
            _ = 1000f;
            if (value.x > -9000f)
            {
                if (hoverSpawner == null || _lastReceivedPosition != value || (_lastReceivedPosition - position).length > 25f)
                {
                    position = value;
                }
                _lastReceivedPosition = value;
            }
        }
    }

    public new Duck duck => _owner as Duck;

    public virtual bool immobilizeOwner
    {
        get
        {
            return _immobilizeOwner;
        }
        set
        {
            _immobilizeOwner = value;
        }
    }

    public bool keepRaised => _keepRaised;

    public bool canRaise => _canRaise;

    public bool hasTrigger => _hasTrigger;

    public Vec2 holdOffset => new Vec2(_holdOffset.x * (float)offDir, _holdOffset.y);

    public override Vec2 center
    {
        get
        {
            return _center + _extraOffset;
        }
        set
        {
            _center = value;
        }
    }

    public override bool action
    {
        get
        {
            if (_owner == null || _owner.owner == this || (_owner is Duck && !(_owner as Duck).Held(this, ignorePowerHolster: true)) || !_owner.action)
            {
                return triggerAction;
            }
            return true;
        }
    }

    public Duck equippedDuck => _equippedDuck;

    public bool raised
    {
        get
        {
            return _raised;
        }
        set
        {
            _raised = value;
        }
    }

    public Duck disarmedFrom
    {
        get
        {
            return _disarmedFrom;
        }
        set
        {
            _disarmedFrom = value;
        }
    }

    public DateTime disarmTime
    {
        get
        {
            return _disarmTime;
        }
        set
        {
            _disarmTime = value;
        }
    }

    public int equippedDepth => _equippedDepth;

    public Thing tapedCompatriot
    {
        get
        {
            if (tape != null)
            {
                if (tape.gun1 == this && tape.gun2 != this)
                {
                    return tape.gun2;
                }
                if (tape.gun2 == this && tape.gun1 != this)
                {
                    return tape.gun1;
                }
            }
            return null;
        }
    }

    public virtual Vec2 tapedOffset => Vec2.Zero;

    public bool tapedIsGun1
    {
        get
        {
            if (tape != null)
            {
                return tape.gun1 == this;
            }
            return false;
        }
    }

    public bool tapedIsGun2
    {
        get
        {
            if (tape != null)
            {
                return tape.gun2 == this;
            }
            return false;
        }
    }

    public override Thing owner
    {
        get
        {
            return _owner;
        }
        set
        {
            if (_owner != value)
            {
                if (owner is TapedGun)
                {
                    _prevOwner = owner.prevOwner;
                }
                else
                {
                    _prevOwner = _owner;
                }
            }
            _lastThrownBy = ((_prevOwner != null) ? _prevOwner : _owner);
            _owner = value;
            if (_owner == null)
            {
                solid = true;
                enablePhysics = true;
            }
            else if (_owner == null)
            {
                solid = false;
                enablePhysics = false;
            }
        }
    }

    public bool held
    {
        get
        {
            if (duck != null)
            {
                return duck.Held(this);
            }
            return false;
        }
    }

    public virtual bool HolsterActivate(Holster pHolster)
    {
        return false;
    }

    public virtual void HolsterUpdate(Holster pHolster)
    {
    }

    public override Vec2 OffsetLocal(Vec2 pos)
    {
        Vec2 offset = pos * base.scale - _extraOffset;
        if (offDir < 0)
        {
            offset.x *= -1f;
        }
        return offset.Rotate(angle, new Vec2(0f, 0f));
    }

    public override Vec2 ReverseOffsetLocal(Vec2 pos)
    {
        return (pos * base.scale - _extraOffset).Rotate(0f - angle, new Vec2(0f, 0f));
    }

    public virtual bool CanTapeTo(Thing pThing)
    {
        return true;
    }

    public Holdable()
    {
    }

    public Holdable(float xpos, float ypos)
        : base(xpos, ypos)
    {
    }

    public virtual void UpdateTaped(TapedGun pTaped)
    {
    }

    public virtual void PreUpdateTapedPositioning(TapedGun pTaped)
    {
    }

    public virtual void UpdateTapedPositioning(TapedGun pTaped)
    {
    }

    /// <summary>
    /// Override this to define a special taped object that two taped objects should turn into
    /// </summary>
    /// <param name="pTaped">The taped gun responsible. Make sure you check gun1 and gun2 to make sure it's the combination you're expecting (two swords = long sword for example)</param>
    /// <returns></returns>
    public virtual Holdable BecomeTapedMonster(TapedGun pTaped)
    {
        return null;
    }

    public virtual void Thrown()
    {
        angle = 0f;
    }

    public virtual void CheckIfHoldObstructed()
    {
        if (!(owner is Duck duckOwner))
        {
            return;
        }
        if (offDir > 0)
        {
            Block hit = Level.CheckLine<Block>(new Vec2(duckOwner.x, base.y), new Vec2(base.right, base.y));
            if (hit is Door && ((hit as Door)._jam == 1f || (hit as Door)._jam == -1f))
            {
                hit = null;
            }
            duckOwner.holdObstructed = hit != null;
        }
        else
        {
            Block hit2 = Level.CheckLine<Block>(new Vec2(base.left, base.y), new Vec2(duckOwner.x, base.y));
            if (hit2 is Door && ((hit2 as Door)._jam == 1f || (hit2 as Door)._jam == -1f))
            {
                hit2 = null;
            }
            duckOwner.holdObstructed = hit2 != null;
        }
    }

    public virtual void ReturnToWorld()
    {
    }

    public virtual int PickupPriority()
    {
        int priority = 0;
        if (this is CTFPresent)
        {
            return 0;
        }
        if (this is Gun)
        {
            return 1;
        }
        if (this is TeamHat)
        {
            return 5;
        }
        if (this is Banana || this is BananaCluster)
        {
            return 4;
        }
        if (this is Hat && !(this is TinfoilHat))
        {
            return 3;
        }
        if (this is Equipment)
        {
            return 2;
        }
        return 3;
    }

    public virtual void UpdateAction()
    {
        if (!base.isServerForObject)
        {
            return;
        }
        bool didPress = false;
        bool didRelease = false;
        if (action)
        {
            if (!_triggerHeld)
            {
                PressAction();
                didPress = true;
            }
            else
            {
                HoldAction();
            }
        }
        else if (_triggerHeld)
        {
            ReleaseAction();
            didRelease = true;
        }
        if (this is Gun && (didPress || _fireActivated))
        {
            (this as Gun).bulletFireIndex++;
            (this as Gun).plugged = false;
        }
        if (Network.isActive && base.isServerForObject && this is Gun)
        {
            if (didPress || (this as Gun).firedBullets.Count > 0 || _fireActivated)
            {
                Send.Message(new NMFireGun(this as Gun, (this as Gun).firedBullets, (this as Gun).bulletFireIndex, !_fireActivated && !didPress && (didRelease ? true : false), (byte)((duck != null) ? duck.netProfileIndex : 4), !didPress && !didRelease), NetMessagePriority.Urgent);
            }
            (this as Gun).firedBullets.Clear();
        }
        _fireActivated = false;
    }

    public virtual void UpdateMaterial()
    {
        if (base.material == null && burnt >= charThreshold)
        {
            base.material = new MaterialCharred();
            SFX.Play("flameExplode");
            for (int i = 0; i < 3; i++)
            {
                Level.Add(SmallSmoke.New(base.x + Rando.Float(-2f, 2f), base.y + Rando.Float(-2f, 2f)));
            }
        }
        else if (base.material == null && heat > 0.1f && physicsMaterial == PhysicsMaterial.Metal)
        {
            base.material = new MaterialRedHot(this);
        }
        else if (base.material == null && heat < -0.1f)
        {
            base.material = new MaterialFrozen(this);
        }
        if (base.material is MaterialRedHot)
        {
            if (heat < 0.1f)
            {
                base.material = null;
            }
            else
            {
                (base.material as MaterialRedHot).intensity = Math.Min(heat - 0.1f, 1f);
            }
        }
        else if (base.material is MaterialFrozen)
        {
            if (heat > -0.1f)
            {
                base.material = null;
            }
            else
            {
                (base.material as MaterialFrozen).intensity = Math.Min(Math.Abs(heat) - 0.1f, 1f);
            }
        }
    }

    public override void Update()
    {
        UpdateMaterial();
        if (owner != null)
        {
            if (!_hasOldDepth)
            {
                _oldDepth = base.depth;
                _hasOldDepth = true;
            }
            Thing ownerThing = owner;
            if (owner is Duck && (owner as Duck)._trapped != null)
            {
                ownerThing = (owner as Duck)._trapped;
            }
            if (duck == null || duck.holdObject == this || this is Equipment)
            {
                base.depth = ownerThing.depth + ((_equippedDuck != null) ? _equippedDepth : 9);
            }
            if ((duck == null || duck.holdObject == this) && !(ownerThing is TapedGun))
            {
                offDir = ownerThing.offDir;
            }
            if (owner is Duck duckOwner)
            {
                if (Network.isActive && duckOwner.Held(this, ignorePowerHolster: true) && !(this is Vine) && (!(this is Equipment) || (this as Equipment).equippedDuck == null) && duckOwner.holdObject != this && !(duckOwner.holdObject is TapedGun) && base.isServerForObject)
                {
                    duckOwner.ObjectThrown(this);
                    return;
                }
                _responsibleProfile = duckOwner.profile;
                duckOwner.UpdateHoldPosition(updateLerp: false);
            }
            _sleeping = false;
            if ((duck == null || duck.Held(this, ignorePowerHolster: true) || this is Equipment) && tape == null)
            {
                base.grounded = false;
            }
            if (duck == null || (duck.holdObject is TapedGun && !(this is Equipment)))
            {
                UpdateAction();
            }
            base.solidImpacting.Clear();
            base.impacting.Clear();
            triggerAction = false;
        }
        else
        {
            if (_hasOldDepth)
            {
                base.depth = _oldDepth;
                _hasOldDepth = false;
            }
            if (owner == null || owner is TapedGun)
            {
                UpdateAction();
            }
            base.Update();
            DoFloat();
            triggerAction = false;
        }
    }

    public void UpdateHoldPositioning()
    {
    }

    public virtual void PressAction()
    {
        _triggerHeld = true;
        OnPressAction();
        HoldAction();
    }

    public virtual void OnPressAction()
    {
    }

    public void HoldAction()
    {
        _triggerHeld = true;
        OnHoldAction();
    }

    public virtual void OnHoldAction()
    {
    }

    public void ReleaseAction()
    {
        _triggerHeld = false;
        OnReleaseAction();
    }

    public virtual void OnReleaseAction()
    {
    }
}
