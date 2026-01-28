using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DuckGame;

public abstract class PhysicsObject : MaterialThing, ITeleport
{
    public delegate void NetAction();

    public StateBinding _positionBinding = new InterpolatedVec2Binding(nameof(netPosition), 10000);

    public StateBinding _velocityBinding = new CompressedVec2Binding(GhostPriority.High, nameof(netVelocity), 20, isvelocity: true);

    public StateBinding _angleBinding = new CompressedFloatBinding(GhostPriority.High, nameof(AngleValue), 0f, 16, isRot: true, doLerp: true);

    public StateBinding _offDirBinding = new StateBinding(GhostPriority.High, nameof(_offDir));

    public StateBinding _ownerBinding = new StateBinding(GhostPriority.High, nameof(netOwner));

    public StateBinding _physicsStateBinding = new PhysicsFlagBinding(GhostPriority.High);

    public StateBinding _burntBinding = new CompressedFloatBinding(nameof(burnt), 1f, 8);

    public StateBinding _collideSoundBinding = new NetSoundBinding(nameof(_netCollideSound));

    public bool isSpawned;

    private const short positionMax = 8191;

    public Vec2 cameraPositionOverride = Vec2.Zero;

    public float vMax = 8f;

    public float hMax = 12f;

    protected Holdable _holdObject;

    public bool sliding;

    public bool crouch;

    public bool disableCrouch;

    public float friction = 0.1f;

    public float frictionMod;

    public float frictionMult = 1f;

    public float airFrictionMult = 1f;

    public float throwSpeedMultiplier = 1f;

    public static float gravity = 0.2f;

    protected bool _skipAutoPlatforms;

    protected bool _skipPlatforms;

    public bool wasHeldByPlayer;

    protected bool duck;

    public float gravMultiplier = 1f;

    public float floatMultiplier = 0.4f;

    private MaterialThing _collideLeft;

    private MaterialThing _collideRight;

    private MaterialThing _collideTop;

    private MaterialThing _collideBottom;

    private MaterialThing _wallCollideLeft;

    private MaterialThing _wallCollideRight;

    protected bool _inPhysicsLoop;

    protected Vec2 _lastPosition = Vec2.Zero;

    protected Vec2 _lastVelocity = Vec2.Zero;

    public bool inRewindLoop;

    public bool predict;

    private List<MaterialThing> _hitThings;

    private List<Duck> _hitDucks;

    public Vec2 velocityBeforeFriction = Vec2.Zero;

    private bool _initedNetSounds;

    public bool skipClip;

    private FluidData _curFluid;

    protected FluidPuddle _curPuddle;

    public bool removedFromFall;

    public DateTime lastGrounded;

    public byte framesSinceGrounded = 99;

    public bool _sleeping;

    public bool doFloat;

    private static Comparison<MaterialThing> XHspeedPositive = SortCollisionXHspeedPositive;

    private static Comparison<MaterialThing> XHspeedNegative = SortCollisionXHspeedNegative;

    private static Comparison<MaterialThing> YVspeedPositive = SortCollisionYVspeedPositive;

    private static Comparison<MaterialThing> YVspeedNegative = SortCollisionYVspeedNegative;

    public bool platformSkip;

    public float specialFrictionMod = 1f;

    private Predicate<MaterialThing> _collisionPred;

    private bool firstCheck;

    private bool _awaken = true;

    private bool modifiedGravForFloat;

    public bool modFric;

    public bool updatePhysics = true;

    public bool didSpawn;

    public bool spawnAnimation;

    private MaterialGrid _gridMaterial;

    private Material _oldMaterial;

    private bool _oldMaterialSet;

    public short netVelocityX
    {
        get
        {
            return (short)Math.Round(hSpeed * 1000f);
        }
        set
        {
            hSpeed = (float)value / 1000f;
        }
    }

    public short netVelocityY
    {
        get
        {
            return (short)Math.Round(vSpeed * 1000f);
        }
        set
        {
            vSpeed = (float)value / 1000f;
        }
    }

    public byte netAngle
    {
        get
        {
            float deg = base.AngleDegrees;
            if (deg < 0f)
            {
                deg = Math.Abs(deg) + 180f;
            }
            return (byte)Math.Round(deg % 360f / 2f);
        }
        set
        {
            base.AngleDegrees = (float)(int)value * 2f;
        }
    }

    public virtual Vec2 netVelocity
    {
        get
        {
            return base.velocity;
        }
        set
        {
            base.velocity = value;
        }
    }

    public short netPositionX
    {
        get
        {
            return (short)Maths.Clamp((int)Math.Round(X * 4), -32768, 32767);
        }
        set
        {
            X = (float)value / 4f;
        }
    }

    public short netPositionY
    {
        get
        {
            return (short)Maths.Clamp((int)Math.Round(Y * 4), -32768, 32767);
        }
        set
        {
            Y = (float)value / 4f;
        }
    }

    public virtual Thing netOwner
    {
        get
        {
            return owner;
        }
        set
        {
            owner = value;
        }
    }

    public override Vec2 netPosition
    {
        get
        {
            _ = Position.X;
            _ = -1000f;
            return Position;
        }
        set
        {
            _ = value.X;
            _ = -1000f;
            Position = value;
        }
    }

    public override Vec2 cameraPosition
    {
        get
        {
            if (!(cameraPositionOverride != Vec2.Zero))
            {
                return base.cameraPosition;
            }
            return cameraPositionOverride;
        }
    }

    public Thing clipThing
    {
        get
        {
            if (base.clip.Count == 0)
            {
                return null;
            }
            return base.clip.ElementAt(0);
        }
        set
        {
            if (value != null && value != this)
            {
                base.clip.Clear();
                base.clip.Add(value as MaterialThing);
            }
            else
            {
                base.clip.Clear();
            }
        }
    }

    public Thing impactingThing
    {
        get
        {
            if (base.impacting.Count == 0)
            {
                return null;
            }
            return base.impacting.ElementAt(0);
        }
        set
        {
            if (value != null && value != this)
            {
                base.impacting.Clear();
                base.impacting.Add(value as MaterialThing);
            }
            else
            {
                base.impacting.Clear();
            }
        }
    }

    public virtual Holdable holdObject
    {
        get
        {
            return _holdObject;
        }
        set
        {
            _holdObject = value;
        }
    }

    public Gun gun => holdObject as Gun;

    public float currentFriction => (friction + frictionMod) * frictionMult;

    public virtual float currentGravity => gravity * gravMultiplier * floatMultiplier;

    public MaterialThing collideLeft => _collideLeft;

    public MaterialThing collideRight => _collideRight;

    public MaterialThing collideTop => _collideTop;

    public MaterialThing collideBottom => _collideBottom;

    public MaterialThing wallCollideLeft => _wallCollideLeft;

    public MaterialThing wallCollideRight => _wallCollideRight;

    public override float impactPowerV => base.impactPowerV - ((vSpeed > 0f) ? (currentGravity * base.weightMultiplierInvTotal) : 0f);

    public override float hSpeed
    {
        get
        {
            if (!_inPhysicsLoop)
            {
                return _hSpeed;
            }
            return lastHSpeed;
        }
        set
        {
            _hSpeed = value;
        }
    }

    public override float vSpeed
    {
        get
        {
            if (!_inPhysicsLoop)
            {
                return _vSpeed;
            }
            return lastVSpeed;
        }
        set
        {
            _vSpeed = value;
        }
    }

    public Vec2 lastPosition => _lastPosition;

    public Vec2 lastVelocity => _lastVelocity;

    public bool ownerIsLocalController
    {
        get
        {
            if (owner != null && owner.responsibleProfile != null && owner.responsibleProfile.localPlayer)
            {
                return true;
            }
            return false;
        }
    }

    public virtual float holdWeightMultiplier
    {
        get
        {
            if (holdObject != null)
            {
                return holdObject.weightMultiplier;
            }
            return 1f;
        }
    }

    public virtual float holdWeightMultiplierSmall
    {
        get
        {
            if (holdObject != null)
            {
                return holdObject.weightMultiplierSmall;
            }
            return 1f;
        }
    }

    public bool sleeping
    {
        get
        {
            return _sleeping;
        }
        set
        {
            if (_sleeping && !value)
            {
                _sleeping = value;
                foreach (PhysicsObject item in Level.CheckLineAll<PhysicsObject>(base.topLeft + new Vec2(0f, -4f), base.topRight + new Vec2(0f, -4f)))
                {
                    item.sleeping = false;
                }
            }
            _sleeping = value;
        }
    }

    public PhysicsObject()
        : this(0f, 0f)
    {
    }

    public PhysicsObject(float xval, float yval)
        : base(xval, yval)
    {
        syncPriority = GhostPriority.Normal;
        if (TeamSelect2.Enabled("MOOGRAV"))
        {
            gravity = 0.1f;
        }
        else
        {
            gravity = 0.2f;
        }
        _physicsIndex = Thing.GetGlobalIndex();
        _ghostType = Editor.IDToType[GetType()];
        _placementCost += 6;
        _hitThings = new List<MaterialThing>();
        _hitDucks = new List<Duck>();
    }

    public override void DoInitialize()
    {
        base.DoInitialize();
    }

    public override void Initialize()
    {
        _grounded = true;
    }

    public override bool ShouldUpdate()
    {
        return false;
    }

    public static int SortCollisionXHspeedPositive(MaterialThing t1, MaterialThing t2)
    {
        float val1 = t1.X + (float)((t1 is Block) ? (-10000) : 0);
        float val2 = t2.X + (float)((t2 is Block) ? (-10000) : 0);
        if (val1 > val2)
        {
            return 1;
        }
        if (val1 < val2)
        {
            return -1;
        }
        return 0;
    }

    public static int SortCollisionXHspeedNegative(MaterialThing t1, MaterialThing t2)
    {
        float val1 = 0f - t1.X + (float)((t1 is Block) ? 10000 : 0);
        float val2 = 0f - t2.X + (float)((t2 is Block) ? 10000 : 0);
        if (val1 > val2)
        {
            return 1;
        }
        if (val1 < val2)
        {
            return -1;
        }
        return 0;
    }

    public static int SortCollisionYVspeedPositive(MaterialThing t1, MaterialThing t2)
    {
        float val1 = t1.Y + (float)((t1 is Block) ? 10000 : 0);
        float val2 = t2.Y + (float)((t2 is Block) ? 10000 : 0);
        if (val1 > val2)
        {
            return 1;
        }
        if (val1 < val2)
        {
            return -1;
        }
        return 0;
    }

    public static int SortCollisionYVspeedNegative(MaterialThing t1, MaterialThing t2)
    {
        float val1 = 0f - t1.Y + (float)((t1 is Block) ? (-10000) : 0);
        float val2 = 0f - t2.Y + (float)((t2 is Block) ? (-10000) : 0);
        if (val1 > val2)
        {
            return 1;
        }
        if (val1 < val2)
        {
            return -1;
        }
        return 0;
    }

    /// <summary>
    /// Called when the object is shot out of a pipe/cannon/etc
    /// </summary>
    /// <param name="pFrom">The thing this object was ejected from</param>
    public virtual void Ejected(Thing pFrom)
    {
    }

    public virtual void UpdatePhysics()
    {
        if (framesSinceGrounded > 10)
        {
            framesSinceGrounded = 10;
        }
        _lastPosition = Position;
        _lastVelocity = base.velocity;
        base.Update();
        if (!solid || !enablePhysics || (base.level != null && !base.level.simulatePhysics))
        {
            lastGrounded = DateTime.Now;
            if (!solid)
            {
                base.solidImpacting.Clear();
                base.impacting.Clear();
            }
            return;
        }
        if (_collisionPred == null)
        {
            _collisionPred = (MaterialThing thing) => thing == null || !Collision.Rect(base.topLeft, base.bottomRight, thing);
        }
        _collideLeft = null;
        _collideRight = null;
        _collideTop = null;
        _collideBottom = null;
        _wallCollideLeft = null;
        _wallCollideRight = null;
        _curPuddle = null;
        if (!skipClip)
        {
            base.clip.RemoveWhere(_collisionPred);
            base.impacting.RemoveWhere(_collisionPred);
        }
        if (_sleeping)
        {
            if (hSpeed == 0f && vSpeed == 0f && !(heat > 0f) && !_awaken)
            {
                return;
            }
            _sleeping = false;
            _awaken = false;
        }
        if (!skipClip)
        {
            base.solidImpacting.RemoveWhere(_collisionPred);
        }
        float fric = currentFriction;
        if (sliding || crouch)
        {
            fric *= 0.28f;
        }
        fric *= specialFrictionMod;
        if (owner is Duck)
        {
            gravMultiplier = 1f;
        }
        if (hSpeed > 0f - fric && hSpeed < fric)
        {
            hSpeed = 0f;
        }
        if (duck)
        {
            if (hSpeed > 0f)
            {
                hSpeed -= fric;
            }
            if (hSpeed < 0f)
            {
                hSpeed += fric;
            }
        }
        else if (base.grounded)
        {
            if (hSpeed > 0f)
            {
                hSpeed -= fric;
            }
            if (hSpeed < 0f)
            {
                hSpeed += fric;
            }
        }
        else
        {
            if (base.isServerForObject && base.Y > Level.current.lowestPoint + 500f)
            {
                removedFromFall = true;
                if (this is Duck || this is RagdollPart || this is TrappedDuck)
                {
                    return;
                }
                Level.Remove(this);
            }
            if (hSpeed > 0f)
            {
                hSpeed -= fric * 0.7f * airFrictionMult;
            }
            if (hSpeed < 0f)
            {
                hSpeed += fric * 0.7f * airFrictionMult;
            }
        }
        if (hSpeed > hMax)
        {
            hSpeed = hMax;
        }
        if (hSpeed < 0f - hMax)
        {
            hSpeed = 0f - hMax;
        }
        Vec2 tl = base.topLeft + new Vec2(0f, 0.5f);
        Vec2 br = base.bottomRight + new Vec2(0f, -0.5f);
        lastHSpeed = hSpeed;
        float realX = 0f;
        bool hasRealX = false;
        if (hSpeed != 0f)
        {
            int cycles = (int)Math.Ceiling(Math.Abs(hSpeed) / 4f);
            float oldHSpeed = hSpeed;
            if (hSpeed < 0f)
            {
                tl.X += hSpeed;
                br.X -= 2f;
            }
            else
            {
                br.X += hSpeed;
                tl.X += 2f;
            }
            _hitThings.Clear();
            Level.CheckRectAll(tl, br, _hitThings);
            if (Network.isActive && !base.isServerForObject && Math.Abs(hSpeed) > 0.5f)
            {
                _hitDucks.Clear();
                Level.CheckRectAll(tl + new Vec2(hSpeed * 2f, 0f), br + new Vec2(hSpeed * 2f, 0f), _hitDucks);
                foreach (Duck d in _hitDucks)
                {
                    if (hSpeed > 0f)
                    {
                        d.Impact(this, ImpactedFrom.Left, solidImpact: true);
                    }
                    else if (hSpeed < 0f)
                    {
                        d.Impact(this, ImpactedFrom.Right, solidImpact: true);
                    }
                }
            }
            if (hSpeed > 0f)
            {
                DGList.Sort(_hitThings, XHspeedPositive);
            }
            else
            {
                DGList.Sort(_hitThings, XHspeedNegative);
            }
            for (int i = 0; i < cycles; i++)
            {
                float speedAdd = hSpeed / (float)cycles;
                if (speedAdd == 0f || Math.Sign(speedAdd) != Math.Sign(oldHSpeed))
                {
                    break;
                }
                base.X += speedAdd;
                _inPhysicsLoop = true;
                bool solidImpact = false;
                foreach (MaterialThing t in _hitThings)
                {
                    if (t == this || base.clip.Contains(t) || t.clip.Contains(this) || !t.solid || (base.planeOfExistence != 4 && t.planeOfExistence != base.planeOfExistence) || (solidImpact && !(t is Block)))
                    {
                        continue;
                    }
                    Vec2 prevPos = Position;
                    bool touch = false;
                    if (t.left <= base.right && t.left > base.left)
                    {
                        touch = true;
                        if (hSpeed > 0f)
                        {
                            _collideRight = t;
                            if (t is Block)
                            {
                                _wallCollideRight = t;
                                solidImpact = true;
                            }
                            t.Impact(this, ImpactedFrom.Left, solidImpact: true);
                            Impact(t, ImpactedFrom.Right, solidImpact: true);
                        }
                    }
                    if (t.right >= base.left && t.right < base.right)
                    {
                        touch = true;
                        if (hSpeed < 0f)
                        {
                            _collideLeft = t;
                            if (t is Block)
                            {
                                _wallCollideLeft = t;
                                solidImpact = true;
                            }
                            t.Impact(this, ImpactedFrom.Right, solidImpact: true);
                            Impact(t, ImpactedFrom.Left, solidImpact: true);
                        }
                    }
                    if (t is IBigStupidWall && (prevPos - Position).Length() > 64f)
                    {
                        Position = prevPos;
                    }
                    if (touch)
                    {
                        t.Touch(this);
                        Touch(t);
                    }
                }
                _inPhysicsLoop = false;
            }
        }
        if (hasRealX)
        {
            base.X = realX;
        }
        if (vSpeed > vMax)
        {
            vSpeed = vMax;
        }
        if (vSpeed < 0f - vMax)
        {
            vSpeed = 0f - vMax;
        }
        vSpeed += currentGravity;
        if (vSpeed < 0f)
        {
            base.grounded = false;
        }
        base.grounded = false;
        framesSinceGrounded++;
        if (!(vSpeed > 0f))
        {
            Math.Floor(vSpeed);
        }
        else
        {
            Math.Ceiling(vSpeed);
        }
        tl = base.topLeft + new Vec2(0.5f, 0f);
        br = base.bottomRight + new Vec2(-0.5f, 0f);
        float floaterTop = -9999f;
        bool touchedWater = false;
        float oldVSpeed = vSpeed;
        lastVSpeed = vSpeed;
        if (vSpeed < 0f)
        {
            tl.Y += vSpeed;
            br.Y -= 2f;
        }
        else
        {
            br.Y += vSpeed;
            tl.Y += 2f;
        }
        _hitThings.Clear();
        Level.CheckRectAll(tl, br, _hitThings);
        if (vSpeed > 0f)
        {
            DGList.Sort(_hitThings, YVspeedPositive);
        }
        else
        {
            DGList.Sort(_hitThings, YVspeedNegative);
        }
        _ = base.top;
        _ = base.bottom;
        if (this is Duck asDuck)
        {
            if (asDuck.inputProfile.Down("DOWN"))
            {
                _ = asDuck._jumpValid > 0;
            }
            else
                _ = 0;
        }
        int vCycles = (int)Math.Ceiling(Math.Abs(vSpeed) / 4f);
        for (int i2 = 0; i2 < vCycles; i2++)
        {
            float vSpeedAdd = vSpeed / (float)vCycles;
            if (vSpeedAdd == 0f || Math.Sign(vSpeedAdd) != Math.Sign(oldVSpeed))
            {
                break;
            }
            base.Y += vSpeedAdd;
            _inPhysicsLoop = true;
            for (int iObject = 0; iObject < _hitThings.Count; iObject++)
            {
                MaterialThing t2 = _hitThings[iObject];
                if (t2 is FluidPuddle)
                {
                    touchedWater = true;
                    _curPuddle = t2 as FluidPuddle;
                    if (t2.top < base.bottom - 2f && t2.collisionSize.Y > 2f)
                    {
                        floaterTop = t2.top;
                    }
                }
                if (t2 == this || base.clip.Contains(t2) || t2.clip.Contains(this) || !t2.solid || (base.planeOfExistence != 4 && t2.planeOfExistence != base.planeOfExistence))
                {
                    continue;
                }
                Vec2 prevPos2 = Position;
                bool touch2 = false;
                if (t2.bottom >= base.top && t2.top < base.top)
                {
                    touch2 = true;
                    if (vSpeed < 0f)
                    {
                        _ = base.Y;
                        _collideTop = t2;
                        t2.Impact(this, ImpactedFrom.Bottom, solidImpact: true);
                        Impact(t2, ImpactedFrom.Top, solidImpact: true);
                    }
                }
                if (t2.top <= base.bottom && t2.bottom > base.bottom)
                {
                    touch2 = true;
                    if (vSpeed > 0f)
                    {
                        _ = this is EnergyScimitar;
                        _collideBottom = t2;
                        t2.Impact(this, ImpactedFrom.Top, solidImpact: true);
                        Impact(t2, ImpactedFrom.Bottom, solidImpact: true);
                    }
                }
                if (t2 is IBigStupidWall && (prevPos2 - Position).Length() > 64f)
                {
                    Position = prevPos2;
                }
                if (touch2)
                {
                    t2.Touch(this);
                    Touch(t2);
                }
            }
            _inPhysicsLoop = false;
        }
        if (base.grounded)
        {
            lastGrounded = DateTime.Now;
            framesSinceGrounded = 0;
            if (!doFloat && hSpeed == 0f && vSpeed == 0f && !(_collideBottom is PhysicsObject) && (_collideBottom is Block || _collideBottom is IPlatform) && (!(_collideBottom is ItemBox) || (_collideBottom as ItemBox).canBounce))
            {
                _sleeping = true;
            }
        }
        if (floaterTop > -999f)
        {
            if (!doFloat && vSpeed > 1f)
            {
                Level.Add(new WaterSplash(base.X, floaterTop - 3f, _curFluid));
                SFX.Play("largeSplash", Rando.Float(0.6f, 0.7f), Rando.Float(-0.7f, -0.2f));
            }
            doFloat = true;
        }
        else
        {
            doFloat = false;
        }
        if (_curPuddle != null)
        {
            _curFluid = _curPuddle.data;
            if (base.onFire && _curFluid.flammable <= 0.5f && _curFluid.heat <= 0.5f)
            {
                Extinquish();
            }
            else if (_curFluid.heat > 0.5f)
            {
                if (flammable > 0f && base.isServerForObject)
                {
                    bool wasOnFire = base.onFire;
                    Burn(Position, this);
                    if (this is Duck && (this as Duck).onFire && !wasOnFire)
                    {
                        (this as Duck).ThrowItem();
                    }
                }
                DoHeatUp(0.015f, Position);
            }
            else
            {
                DoHeatUp(-0.05f, Position);
            }
        }
        if (doFloat)
        {
            if (this is Duck && (this as Duck).crouch)
            {
                if (floatMultiplier > 0.98f)
                {
                    vSpeed *= 0.8f;
                }
                floatMultiplier = 0.8f;
            }
            else
            {
                if (floatMultiplier > 0.98f)
                {
                    vSpeed *= 0.4f;
                }
                vSpeed *= 0.95f;
                floatMultiplier = 0.4f;
            }
        }
        else
        {
            if (touchedWater && oldVSpeed > 1f && Math.Abs(vSpeed) < 0.01f)
            {
                Level.Add(new WaterSplash(base.X, base.bottom, _curFluid));
                SFX.Play("littleSplash", Rando.Float(0.8f, 0.9f), Rando.Float(-0.2f, 0.2f));
            }
            floatMultiplier = 1f;
        }
        Recorder.LogVelocity(Math.Abs(hSpeed) + Math.Abs(vSpeed));
        if (!_sleeping)
        {
            if (modFric)
            {
                modFric = false;
            }
            else
            {
                specialFrictionMod = 1f;
            }
        }
    }

    public void DoFloat(bool lavaOnly = false)
    {
        onlyFloatInLava = lavaOnly;
        DoFloat();
    }

    public void DoFloat()
    {
        if (buoyancy > 0f)
        {
            FluidPuddle p = Level.CheckPoint<FluidPuddle>(Position + new Vec2(0f, 4f));
            if (p != null)
            {
                if (onlyFloatInLava && p.data.heat < 0.5f)
                {
                    return;
                }
                if (base.Y + 4f - p.top > 8f)
                {
                    modifiedGravForFloat = true;
                    gravMultiplier = -0.5f;
                    base.grounded = false;
                    return;
                }
                if (base.Y + 4f - p.top < 3f)
                {
                    modifiedGravForFloat = true;
                    gravMultiplier = 0.2f;
                    base.grounded = true;
                }
                else if (base.Y + 4f - p.top > 4f)
                {
                    gravMultiplier = -0.2f;
                    base.grounded = true;
                }
                base.grounded = true;
            }
            else
            {
                gravMultiplier = 1f;
            }
        }
        else if (modifiedGravForFloat)
        {
            gravMultiplier = 1f;
        }
    }

    public override void Impact(MaterialThing with, ImpactedFrom from, bool solidImpact)
    {
        bool wasBlock = true;
        bool wasSolid = false;
        if (with is Block)
        {
            wasSolid = true;
            with.SolidImpact(this, from);
            if (with.destroyed)
            {
                return;
            }
            if (from == ImpactedFrom.Right)
            {
                base.X = with.left + (base.X - base.right);
                SolidImpact(with, from);
                if (hSpeed > (0f - hSpeed) * base.bouncy)
                {
                    hSpeed = (0f - hSpeed) * base.bouncy;
                    if (Math.Abs(hSpeed) < 0.1f)
                    {
                        hSpeed = 0f;
                    }
                }
            }
            if (from == ImpactedFrom.Left)
            {
                base.X = with.right + (base.X - base.left);
                SolidImpact(with, from);
                if (hSpeed < (0f - hSpeed) * base.bouncy)
                {
                    hSpeed = (0f - hSpeed) * base.bouncy;
                    if (Math.Abs(hSpeed) < 0.1f)
                    {
                        hSpeed = 0f;
                    }
                }
            }
            if (from == ImpactedFrom.Top)
            {
                base.Y = with.bottom + (base.Y - base.top) + 1f;
                SolidImpact(with, from);
                if (vSpeed < (0f - vSpeed) * base.bouncy)
                {
                    vSpeed = (0f - vSpeed) * base.bouncy;
                    if (Math.Abs(vSpeed) < 0.1f)
                    {
                        vSpeed = 0f;
                    }
                }
            }
            if (from == ImpactedFrom.Bottom)
            {
                base.Y = with.top + (base.Y - base.bottom);
                SolidImpact(with, from);
                if (vSpeed > (0f - vSpeed) * base.bouncy)
                {
                    vSpeed = (0f - vSpeed) * base.bouncy;
                    if (Math.Abs(vSpeed) < 0.1f)
                    {
                        vSpeed = 0f;
                    }
                }
                base.grounded = true;
            }
        }
        else if (with is IPlatform)
        {
            wasSolid = false;
            if (from == ImpactedFrom.Bottom)
            {
                if ((with is PhysicsObject && (!with.grounded || !(Math.Abs(with.vSpeed) < 0.3f))) || !(with.top + (vSpeed + 2f) > base.bottom) || _skipPlatforms || (_skipAutoPlatforms && with is AutoPlatform))
                {
                    return;
                }
                with.SolidImpact(this, ImpactedFrom.Top);
                if (with.destroyed)
                {
                    return;
                }
                base.Y = with.top + (base.Y - base.bottom);
                SolidImpact(with, from);
                if (vSpeed > (0f - vSpeed) * base.bouncy)
                {
                    vSpeed = (0f - vSpeed) * base.bouncy;
                    if (Math.Abs(vSpeed) < 0.1f)
                    {
                        vSpeed = 0f;
                    }
                }
                base.grounded = true;
            }
        }
        else
        {
            wasBlock = false;
        }
        if (wasBlock)
        {
            if (!wasSolid && !base.impacting.Contains(with))
            {
                base.Impact(with, from, solidImpact);
                base.impacting.Add(with);
            }
            else if (wasSolid && !base.solidImpacting.Contains(with))
            {
                base.Impact(with, from, solidImpact);
                base.solidImpacting.Add(with);
            }
        }
        else
        {
            base.Impact(with, from, solidImpact);
        }
    }

    public override void Update()
    {
        if (Network.isActive && !_initedNetSounds)
        {
            _initedNetSounds = true;
            List<string> s = base.collideSounds.GetList(ImpactedFrom.Bottom);
            if (s != null)
            {
                _netCollideSound = new NetSoundEffect(s.ToArray());
                _netCollideSound.volume = _impactVolume;
            }
        }
        if (updatePhysics)
        {
            UpdatePhysics();
        }
    }

    public override void DoDraw()
    {
        if (!Content.renderingToTarget && spawnAnimation)
        {
            if (_gridMaterial == null)
            {
                _gridMaterial = new MaterialGrid(this);
            }
            if (!_gridMaterial.finished)
            {
                if (!_oldMaterialSet)
                {
                    _oldMaterial = base.material;
                    _oldMaterialSet = true;
                }
                base.material = _gridMaterial;
            }
            else
            {
                if (_oldMaterialSet)
                {
                    base.material = _oldMaterial;
                }
                spawnAnimation = false;
            }
        }
        base.DoDraw();
    }

    public override void Draw()
    {
        if (graphic != null)
        {
            graphic.flipH = offDir <= 0;
        }
        base.Draw();
    }

    public override void NetworkUpdate()
    {
    }

    protected void SyncNetworkAction(NetAction pAction)
    {
        if (!Network.isActive)
        {
            pAction();
        }
        else if (base.isServerForObject)
        {
            CheckForNetworkActionAttribute(pAction.Method);
            pAction();
            Send.Message(new NMRunNetworkAction(this, Editor.NetworkActionIndex(GetType(), pAction.Method)));
        }
    }

    protected void SyncNetworkAction<T>(Action<T> pMethod, T pParameter)
    {
        if (!Network.isActive)
        {
            pMethod(pParameter);
        }
        else if (base.isServerForObject)
        {
            CheckForNetworkActionAttribute(pMethod.Method);
            pMethod(pParameter);
            Send.Message(new NMRunNetworkActionParameters(this, pMethod.Method, new object[1] { pParameter }));
        }
    }

    protected void SyncNetworkAction<T, T2>(Action<T, T2> pMethod, T pParameter, T2 pParameter2)
    {
        if (!Network.isActive)
        {
            pMethod(pParameter, pParameter2);
        }
        else if (base.isServerForObject)
        {
            CheckForNetworkActionAttribute(pMethod.Method);
            pMethod(pParameter, pParameter2);
            Send.Message(new NMRunNetworkActionParameters(this, pMethod.Method, new object[2] { pParameter, pParameter2 }));
        }
    }

    protected void SyncNetworkAction<T, T2, T3>(Action<T, T2, T3> pMethod, T pParameter, T2 pParameter2, T3 pParameter3)
    {
        if (!Network.isActive)
        {
            pMethod(pParameter, pParameter2, pParameter3);
        }
        else if (base.isServerForObject)
        {
            CheckForNetworkActionAttribute(pMethod.Method);
            pMethod(pParameter, pParameter2, pParameter3);
            Send.Message(new NMRunNetworkActionParameters(this, pMethod.Method, new object[3] { pParameter, pParameter2, pParameter3 }));
        }
    }

    protected void SyncNetworkAction<T, T2, T3, T4>(Action<T, T2, T3, T4> pMethod, T pParameter, T2 pParameter2, T3 pParameter3, T4 pParameter4)
    {
        if (!Network.isActive)
        {
            pMethod(pParameter, pParameter2, pParameter3, pParameter4);
        }
        else if (base.isServerForObject)
        {
            CheckForNetworkActionAttribute(pMethod.Method);
            pMethod(pParameter, pParameter2, pParameter3, pParameter4);
            Send.Message(new NMRunNetworkActionParameters(this, pMethod.Method, new object[4] { pParameter, pParameter2, pParameter3, pParameter4 }));
        }
    }

    protected void SyncNetworkAction<T, T2, T3, T4, T5>(Action<T, T2, T3, T4, T5> pMethod, T pParameter, T2 pParameter2, T3 pParameter3, T4 pParameter4, T5 pParameter5)
    {
        if (!Network.isActive)
        {
            pMethod(pParameter, pParameter2, pParameter3, pParameter4, pParameter5);
        }
        else if (base.isServerForObject)
        {
            CheckForNetworkActionAttribute(pMethod.Method);
            pMethod(pParameter, pParameter2, pParameter3, pParameter4, pParameter5);
            Send.Message(new NMRunNetworkActionParameters(this, pMethod.Method, new object[5] { pParameter, pParameter2, pParameter3, pParameter4, pParameter5 }));
        }
    }

    private void CheckForNetworkActionAttribute(MethodInfo pMethod)
    {
        if (!pMethod.GetCustomAttributes(typeof(NetworkAction), inherit: false).Any())
        {
            throw new Exception("SyncNetworkAction can only be used for functions with the [NetworkAction] attribute defined.");
        }
    }
}
