using System;

namespace DuckGame;

[BaggedProperty("canSpawn", false)]
public class Ragdoll : Thing
{
    public bool inSleepingBag;

    public StateBinding _positionBinding = new InterpolatedVec2Binding(nameof(position));

    public StateBinding _part1Binding = new StateBinding(nameof(part1));

    public StateBinding _part2Binding = new StateBinding(nameof(part2));

    public StateBinding _part3Binding = new StateBinding(nameof(part3));

    public StateBinding _tongueStuckBinding = new StateBinding(nameof(tongueStuck));

    public StateBinding _sleepingBagHealthBinding = new StateBinding(nameof(sleepingBagHealth));

    public byte sleepingBagHealth;

    public StateBinding _physicsStateBinding = new RagdollFlagBinding();

    public Vec2 tongueStuck = Vec2.Zero;

    public Thing tongueStuckThing;

    private bool _zekeBear;

    public RagdollPart _part1;

    public RagdollPart _part2;

    public RagdollPart _part3;

    private Duck _theDuck;

    protected Thing _zapper;

    private float _zap;

    public DuckPersona persona;

    public bool _slide;

    public float _timeSinceNudge;

    public float partSep = 6f;

    public int npi;

    public int tongueShakes;

    private bool _didSmoke;

    public bool jetting;

    private bool _wasZapping;

    public bool _makeActive;

    private float sleepingBagTimer;

    public Thing holdingOwner
    {
        get
        {
            if (_part1 != null && _part1.owner != null)
            {
                return _part1.owner;
            }
            if (_part2 != null && _part2.owner != null)
            {
                return _part2.owner;
            }
            if (_part3 != null && _part3.owner != null)
            {
                return _part3.owner;
            }
            return null;
        }
    }

    public RagdollPart part1
    {
        get
        {
            return _part1;
        }
        set
        {
            _part1 = value;
            if (_part1 != null)
            {
                _part1.doll = this;
                _part1.part = 0;
            }
        }
    }

    public RagdollPart part2
    {
        get
        {
            return _part2;
        }
        set
        {
            _part2 = value;
            if (_part2 != null)
            {
                _part2.doll = this;
                _part2.part = 2;
            }
        }
    }

    public RagdollPart part3
    {
        get
        {
            return _part3;
        }
        set
        {
            _part3 = value;
            if (_part3 != null)
            {
                _part3.doll = this;
                _part3.part = 1;
            }
        }
    }

    public Duck _duck
    {
        get
        {
            return _theDuck;
        }
        set
        {
            _theDuck = value;
        }
    }

    public override bool visible
    {
        get
        {
            return base.visible;
        }
        set
        {
            if (!visible && value)
            {
                _makeActive = false;
                if (_part1 != null)
                {
                    _part1.owner = null;
                    _part1.framesSinceGrounded = 99;
                }
                if (_part2 != null)
                {
                    _part2.owner = null;
                    _part2.framesSinceGrounded = 99;
                }
                if (_part3 != null)
                {
                    _part3.owner = null;
                    _part3.framesSinceGrounded = 99;
                }
            }
            base.visible = value;
            if (_part1 != null)
            {
                _part1.visible = value;
            }
            if (_part2 != null)
            {
                _part2.visible = value;
            }
            if (_part3 != null)
            {
                _part3.visible = value;
            }
        }
    }

    public override bool active
    {
        get
        {
            return base.active;
        }
        set
        {
            base.active = value;
            if (_part1 != null)
            {
                _part1.active = value;
            }
            if (_part2 != null)
            {
                _part2.active = value;
            }
            if (_part3 != null)
            {
                _part3.active = value;
            }
        }
    }

    public override bool enablePhysics
    {
        get
        {
            return base.enablePhysics;
        }
        set
        {
            base.enablePhysics = value;
            if (_part1 != null)
            {
                _part1.enablePhysics = value;
            }
            if (_part2 != null)
            {
                _part2.enablePhysics = value;
            }
            if (_part3 != null)
            {
                _part3.enablePhysics = value;
            }
        }
    }

    public override bool solid
    {
        get
        {
            return base.solid;
        }
        set
        {
            base.solid = value;
            if (_part1 != null)
            {
                _part1.solid = value;
            }
            if (_part2 != null)
            {
                _part2.solid = value;
            }
            if (_part3 != null)
            {
                _part3.solid = value;
            }
        }
    }

    public override NetworkConnection connection
    {
        get
        {
            return base.connection;
        }
        set
        {
            base.connection = value;
            if (_part1 != null)
            {
                _part1.connection = value;
            }
            if (_part2 != null)
            {
                _part2.connection = value;
            }
            if (_part3 != null)
            {
                _part3.connection = value;
            }
        }
    }

    public override NetIndex8 authority
    {
        get
        {
            return base.authority;
        }
        set
        {
            base.authority = value;
            if (_part1 != null)
            {
                _part1.authority = value;
            }
            if (_part2 != null)
            {
                _part2.authority = value;
            }
            if (_part3 != null)
            {
                _part3.authority = value;
            }
        }
    }

    public Duck captureDuck
    {
        get
        {
            return _duck;
        }
        set
        {
            _duck = value;
            if (_duck != null)
            {
                if (_part1 != null)
                {
                    _part1.part = 0;
                }
                if (_part2 != null)
                {
                    _part2.part = 2;
                }
                if (_part3 != null)
                {
                    _part3.part = 1;
                }
            }
        }
    }

    public bool makeActive
    {
        get
        {
            return _makeActive;
        }
        set
        {
            _makeActive = value;
        }
    }

    private bool isInPipe
    {
        get
        {
            if ((part1 == null || !part1.inPipe) && (part2 == null || !part2.inPipe) && (part3 == null || !part3.inPipe) && !inPipe)
            {
                if (captureDuck != null)
                {
                    return captureDuck.inPipe;
                }
                return false;
            }
            return true;
        }
    }

    public void MakeZekeBear()
    {
        if (_part1 != null)
        {
            _part1.MakeZekeBear();
        }
        if (_part2 != null)
        {
            _part2.MakeZekeBear();
        }
        if (_part3 != null)
        {
            _part3.MakeZekeBear();
        }
        _zekeBear = true;
    }

    public void Zap(Thing zapper)
    {
        _zapper = zapper;
        _zap = 1f;
    }

    public void Extinguish()
    {
        if (part1 != null)
        {
            part1.Extinquish();
        }
        if (part2 != null)
        {
            part2.Extinquish();
        }
        if (part3 != null)
        {
            part3.Extinquish();
        }
    }

    public Ragdoll(float xpos, float ypos, Duck who, bool slide, float degrees, int off, Vec2 v, DuckPersona p = null)
        : base(xpos, ypos)
    {
        _duck = who;
        _slide = slide;
        offDir = (sbyte)off;
        base.angleDegrees = degrees;
        base.velocity = v;
        persona = p;
    }

    public bool PartHeld()
    {
        if (_part1 == null || _part2 == null || _part3 == null)
        {
            return false;
        }
        if (_part1.owner != null || _part2.owner != null || _part3.owner != null)
        {
            return true;
        }
        return false;
    }

    public Profile PartHeldProfile()
    {
        if (_part1 == null || _part2 == null || _part3 == null)
        {
            return null;
        }
        if (_part1.duck != null)
        {
            return _part1.duck.profile;
        }
        if (_part2.duck != null)
        {
            return _part2.duck.profile;
        }
        if (_part3.duck != null)
        {
            return _part3.duck.profile;
        }
        return null;
    }

    public void SortOutParts(float xpos, float ypos, Duck who, bool slide, float degrees, int off, Vec2 v)
    {
        _duck = who;
        _slide = slide;
        offDir = (sbyte)off;
        base.angleDegrees = degrees;
        base.velocity = v;
        _makeActive = false;
        RunInit();
    }

    public void Organize()
    {
        Vec2 vec = Maths.AngleToVec(angle);
        if (_part1 == null)
        {
            _part1 = new RagdollPart(base.x - vec.x * partSep, base.y - vec.y * partSep, 0, (_duck != null) ? _duck.persona : persona, offDir, this);
            if (Network.isActive && !GhostManager.inGhostLoop)
            {
                GhostManager.context.MakeGhost(_part1);
            }
            _part2 = new RagdollPart(base.x, base.y, 2, (_duck != null) ? _duck.persona : persona, offDir, this);
            if (Network.isActive && !GhostManager.inGhostLoop)
            {
                GhostManager.context.MakeGhost(_part2);
            }
            _part3 = new RagdollPart(base.x + vec.x * partSep, base.y + vec.y * partSep, 1, (_duck != null) ? _duck.persona : persona, offDir, this);
            if (Network.isActive && !GhostManager.inGhostLoop)
            {
                GhostManager.context.MakeGhost(_part3);
            }
            Level.Add(_part1);
            Level.Add(_part2);
            Level.Add(_part3);
        }
        else
        {
            _part1.SortOutDetails(base.x - vec.x * partSep, base.y - vec.y * partSep, 0, (_duck != null) ? _duck.persona : persona, offDir, this);
            _part2.SortOutDetails(base.x, base.y, 2, (_duck != null) ? _duck.persona : persona, offDir, this);
            _part3.SortOutDetails(base.x + vec.x * partSep, base.y + vec.y * partSep, 1, (_duck != null) ? _duck.persona : persona, offDir, this);
        }
        _part1.joint = _part2;
        _part3.joint = _part2;
        _part1.connect = _part3;
        _part3.connect = _part1;
        _part1.framesSinceGrounded = 99;
        _part2.framesSinceGrounded = 99;
        _part3.framesSinceGrounded = 99;
        if (_duck != null)
        {
            if (!(Level.current is GameLevel) || !(Level.current as GameLevel).isRandom)
            {
                _duck.ReturnItemToWorld(_part1);
                _duck.ReturnItemToWorld(_part2);
                _duck.ReturnItemToWorld(_part3);
            }
            _part3.depth = new Depth(_duck.depth.value);
            _part1.depth = _part3.depth - 1;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public void RunInit()
    {
        Organize();
        if (Network.isActive && !GhostManager.inGhostLoop)
        {
            GhostManager.context.MakeGhost(this);
        }
        if (Math.Abs(hSpeed) < 0.2f)
        {
            hSpeed = NetRand.Float(0.3f, 1f) * (float)((NetRand.Float(1f) >= 0.5f) ? 1 : (-1));
        }
        float mul1 = (_slide ? 1f : 1.05f);
        float mul2 = (_slide ? 1f : 0.95f);
        _part1.hSpeed = hSpeed * mul1;
        _part1.vSpeed = vSpeed;
        _part2.hSpeed = hSpeed;
        _part2.vSpeed = vSpeed;
        _part3.hSpeed = hSpeed * mul2;
        _part3.vSpeed = vSpeed;
        _part1.enablePhysics = false;
        _part2.enablePhysics = false;
        _part3.enablePhysics = false;
        _part1.Update();
        _part2.Update();
        _part3.Update();
        _part1.enablePhysics = true;
        _part2.enablePhysics = true;
        _part3.enablePhysics = true;
        if (Network.isActive)
        {
            Thing.Fondle(this, DuckNetwork.localConnection);
            Thing.Fondle(_part1, DuckNetwork.localConnection);
            Thing.Fondle(_part2, DuckNetwork.localConnection);
            Thing.Fondle(_part3, DuckNetwork.localConnection);
        }
        if (_duck != null && _duck.onFire)
        {
            _part1.Burn(_part1.position, _duck.lastBurnedBy);
            _part2.Burn(_part2.position, _duck.lastBurnedBy);
        }
    }

    public void LightOnFire()
    {
        if (_duck != null)
        {
            _part1.Burn(_part1.position, _duck.lastBurnedBy);
            _part2.Burn(_part2.position, _duck.lastBurnedBy);
        }
    }

    public void Unragdoll()
    {
        if (isInPipe)
        {
            return;
        }
        bool num = _duck.HasEquipment(typeof(FancyShoes));
        _duck.visible = true;
        if (Network.isActive)
        {
            _part2.UpdateLastReasonablePosition(_part2.position);
            _duck.position = _part2._lastReasonablePosition;
        }
        else
        {
            _duck.position = _part2.position;
        }
        if (!num)
        {
            _duck.position.y -= 20f;
        }
        _duck.hSpeed = _part2.hSpeed;
        _duck.immobilized = false;
        _duck.enablePhysics = true;
        _duck._jumpValid = 0;
        _duck._lastHoldItem = null;
        _makeActive = false;
        _part2.ReturnItemToWorld(_duck);
        if (Network.isActive)
        {
            active = false;
            visible = false;
            owner = null;
            if (base.y > -1000f)
            {
                base.y = -9999f;
                _part1.y = -9999f;
                _part2.y = -9999f;
                _part3.y = -9999f;
            }
            _part1.owner = null;
            _part2.owner = null;
            _part3.owner = null;
            if (_duck.isServerForObject)
            {
                Thing.Fondle(this, _duck.connection);
                Thing.Fondle(_part1, _duck.connection);
                Thing.Fondle(_part2, _duck.connection);
                Thing.Fondle(_part3, _duck.connection);
            }
        }
        else
        {
            Level.Remove(this);
        }
        _duck.ragdoll = null;
        if (!num)
        {
            _duck.vSpeed = -2f;
        }
        else
        {
            _duck.vSpeed = _part2.vSpeed;
        }
    }

    public void Shot(Bullet bullet)
    {
        if (_duck != null && !_duck.dead)
        {
            _duck.position = _part2.position;
            _duck.Kill(new DTShot(bullet));
            _duck.y -= 5000f;
        }
    }

    public void Killed(DestroyType t)
    {
        if (_duck != null && !_duck.dead && t != null)
        {
            _duck.position = _part2.position;
            _duck.Destroy(t);
            _duck.y -= 5000f;
        }
    }

    public void LitOnFire(Thing litBy)
    {
        if (_duck != null && !_duck.onFire)
        {
            _duck.Burn(position, litBy);
        }
    }

    public override void Terminate()
    {
        if (_part1 != null && _part2 != null && _part3 != null)
        {
            Level.Remove(_part1);
            Level.Remove(_part2);
            Level.Remove(_part3);
        }
    }

    public void Solve(PhysicsObject body1, PhysicsObject body2, float dist)
    {
        Vec2 axis = body2.position - body1.position;
        float currentDistance = axis.length;
        if (currentDistance < 0.0001f)
        {
            currentDistance = 0.0001f;
        }
        Vec2 unitAxis = axis * (1f / currentDistance);
        Vec2 vel1 = new Vec2(body1.hSpeed, body1.vSpeed);
        Vec2 vel2 = new Vec2(body2.hSpeed, body2.vSpeed);
        float num = Vec2.Dot(vel2 - vel1, unitAxis);
        float relDist = currentDistance - dist;
        float invMass1 = 2.1f;
        float invMass2 = 2.1f;
        if (body1 == part1 && jetting)
        {
            invMass1 = 6f;
        }
        else if (body2 == part1 && jetting)
        {
            invMass2 = 6f;
        }
        float impulse = (num + relDist) / (invMass1 + invMass2);
        Vec2 impulseVector = unitAxis * impulse;
        vel1 += impulseVector * invMass1;
        vel2 -= impulseVector * invMass2;
        if (body1.owner == null)
        {
            body1.hSpeed = vel1.x;
            body1.vSpeed = vel1.y;
        }
        if (body2.owner == null)
        {
            body2.hSpeed = vel2.x;
            body2.vSpeed = vel2.y;
        }
    }

    public override bool ShouldUpdate()
    {
        return false;
    }

    public void ProcessInput(InputProfile input)
    {
    }

    public float SpecialSolve(PhysicsObject b1, PhysicsObject b2, float dist)
    {
        Thing body1 = ((b1.owner != null) ? b1.owner : b1);
        Thing body2 = ((b2.owner != null) ? b2.owner : b2);
        Vec2 axis = b2.position - b1.position;
        float currentDistance = axis.length;
        if (currentDistance < 0.0001f)
        {
            currentDistance = 0.0001f;
        }
        if (currentDistance < dist)
        {
            return 0f;
        }
        Vec2 unitAxis = axis * (1f / currentDistance);
        Vec2 vel1 = new Vec2(body1.hSpeed, body1.vSpeed);
        Vec2 vel2 = new Vec2(body2.hSpeed, body2.vSpeed);
        float num = Vec2.Dot(vel2 - vel1, unitAxis);
        float relDist = currentDistance - dist;
        float invMass1 = 2.5f;
        float invMass2 = 2.1f;
        if (body1 is ChainLink && !(body2 is ChainLink))
        {
            invMass1 = 10f;
            invMass2 = 0f;
        }
        else if (body2 is ChainLink && !(body1 is ChainLink))
        {
            invMass1 = 0f;
            invMass2 = 10f;
        }
        else if (body1 is ChainLink && body2 is ChainLink)
        {
            invMass1 = 10f;
            invMass2 = 10f;
        }
        if (body1 is RagdollPart)
        {
            invMass1 = ((!_zekeBear) ? 10f : 4f);
        }
        else if (body2 is RagdollPart)
        {
            invMass2 = ((!_zekeBear) ? 10f : 4f);
        }
        float impulse = (num + relDist) / (invMass1 + invMass2);
        Vec2 impulseVector = unitAxis * impulse;
        vel1 += impulseVector * invMass1;
        vel2 -= impulseVector * invMass2;
        body1.hSpeed = vel1.x;
        body1.vSpeed = vel1.y;
        body2.hSpeed = vel2.x;
        body2.vSpeed = vel2.y;
        if (body1 is ChainLink && (body2.position - body1.position).length > dist * 12f)
        {
            body1.position = position;
        }
        if (body2 is ChainLink && (body2.position - body1.position).length > dist * 12f)
        {
            body2.position = position;
        }
        return impulse;
    }

    public float SpecialSolve(PhysicsObject b1, Vec2 stuck, float dist)
    {
        Thing body1 = ((b1.owner != null) ? b1.owner : b1);
        Vec2 axis = stuck - b1.position;
        float currentDistance = axis.length;
        if (currentDistance < 0.0001f)
        {
            currentDistance = 0.0001f;
        }
        if (currentDistance < dist)
        {
            return 0f;
        }
        Vec2 unitAxis = axis * (1f / currentDistance);
        Vec2 vel1 = new Vec2(body1.hSpeed, body1.vSpeed);
        Vec2 vec = new Vec2(0f, 0f);
        float num = Vec2.Dot(vec - vel1, unitAxis);
        float relDist = currentDistance - dist;
        float invMass1 = 2.5f;
        float invMass2 = 2.1f;
        if (body1 is RagdollPart)
        {
            invMass1 = ((!_zekeBear) ? 10f : 4f);
        }
        float impulse = (num + relDist) / (invMass1 + invMass2);
        Vec2 impulseVector = unitAxis * impulse;
        vel1 += impulseVector * invMass1;
        _ = vec - impulseVector * invMass2;
        body1.hSpeed = vel1.x;
        body1.vSpeed = vel1.y;
        return impulse;
    }

    public override void Update()
    {
        if (base.removeFromLevel || (base.isOffBottomOfLevel && captureDuck != null && captureDuck.dead))
        {
            return;
        }
        _timeSinceNudge += 0.07f;
        if (_part1 == null || _part2 == null || _part3 == null)
        {
            return;
        }
        if (_zap > 0f)
        {
            _part1.vSpeed += Rando.Float(-1f, 0.5f);
            _part1.hSpeed += Rando.Float(-0.5f, 0.5f);
            _part2.vSpeed += Rando.Float(-1f, 0.5f);
            _part2.hSpeed += Rando.Float(-0.5f, 0.5f);
            _part3.vSpeed += Rando.Float(-1f, 0.5f);
            _part3.hSpeed += Rando.Float(-0.5f, 0.5f);
            _part1.x += Rando.Int(-2, 2);
            _part1.y += Rando.Int(-2, 2);
            _part2.x += Rando.Int(-2, 2);
            _part2.y += Rando.Int(-2, 2);
            _part3.x += Rando.Int(-2, 2);
            _part3.y += Rando.Int(-2, 2);
            _zap -= 0.05f;
            _wasZapping = true;
        }
        else if (_wasZapping)
        {
            _wasZapping = false;
            if (captureDuck != null)
            {
                if (captureDuck.dead)
                {
                    captureDuck.Ressurect();
                }
                else
                {
                    captureDuck.Kill(new DTElectrocute(_zapper));
                }
                return;
            }
        }
        if (captureDuck != null && captureDuck.inputProfile != null && captureDuck.isServerForObject)
        {
            if (captureDuck.inputProfile.Pressed("JUMP") && captureDuck.HasEquipment(typeof(Jetpack)))
            {
                captureDuck.GetEquipment(typeof(Jetpack)).PressAction();
            }
            if (captureDuck.inputProfile.Released("JUMP") && captureDuck.HasEquipment(typeof(Jetpack)))
            {
                captureDuck.GetEquipment(typeof(Jetpack)).ReleaseAction();
            }
        }
        partSep = 6f;
        if (_zekeBear)
        {
            partSep = 4f;
        }
        if ((_part1.position - _part3.position).length > partSep * 5f)
        {
            if (_part1.owner != null)
            {
                _part2.position = (_part3.position = _part1.position);
            }
            else if (_part3.owner != null)
            {
                _part1.position = (_part2.position = _part3.position);
            }
            else
            {
                _part1.position = (_part3.position = _part2.position);
            }
            RagdollPart ragdollPart = _part1;
            RagdollPart ragdollPart2 = _part2;
            float num = (_part3.vSpeed = 0f);
            float num3 = (ragdollPart2.vSpeed = num);
            ragdollPart.vSpeed = num3;
            RagdollPart ragdollPart3 = _part1;
            RagdollPart ragdollPart4 = _part2;
            num = (_part3.hSpeed = 0f);
            num3 = (ragdollPart4.hSpeed = num);
            ragdollPart3.hSpeed = num3;
            Solve(_part1, _part2, partSep);
            Solve(_part2, _part3, partSep);
            Solve(_part1, _part3, partSep * 2f);
        }
        Solve(_part1, _part2, partSep);
        Solve(_part2, _part3, partSep);
        Solve(_part1, _part3, partSep * 2f);
        if (_part1.owner is Duck && _part3.owner is Duck)
        {
            SpecialSolve(_part3, _part1.owner as Duck, 16f);
            SpecialSolve(_part1, _part3.owner as Duck, 16f);
        }
        if (tongueStuck != Vec2.Zero && captureDuck != null)
        {
            Vec2 t = tongueStuck + new Vec2(captureDuck.offDir * -4, -6f);
            if (_part1.owner is Duck)
            {
                SpecialSolve(_part3, _part1.owner as Duck, 16f);
                SpecialSolve(_part1, t, 16f);
            }
            if (_part3.owner is Duck)
            {
                SpecialSolve(_part1, _part3.owner as Duck, 16f);
                SpecialSolve(_part3, t, 16f);
            }
            if ((part1.position - t).length > 4f)
            {
                SpecialSolve(_part1, t, 4f);
                _ = (t - part1.position).normalized;
                if ((part1.position - t).length > 12f)
                {
                    part1.position = Lerp.Vec2Smooth(part1.position, t, 0.2f);
                }
            }
        }
        position = (_part1.position + _part2.position + _part3.position) / 3f;
        if (_duck == null || !(_zap <= 0f))
        {
            return;
        }
        if (_duck.eyesClosed)
        {
            _part1.frame = 20;
        }
        if (!_duck.fancyShoes || _duck.framesSinceRagdoll >= 1)
        {
            UpdateInput();
        }
        bool wellAndTrulyKilled = false;
        if (base.isServerForObject)
        {
            if (base.isOffBottomOfLevel && !_duck.dead)
            {
                wellAndTrulyKilled = _duck.Kill(new DTFall());
                _duck.profile.stats.fallDeaths++;
            }
            jetting = false;
        }
        if (wellAndTrulyKilled)
        {
            _duck.y = _part2.y - 9999f;
            _duck.x = _part2.x;
        }
    }

    public override void Draw()
    {
    }

    public bool TryingToControl()
    {
        if (captureDuck == null)
        {
            return false;
        }
        if (!captureDuck.inputProfile.Pressed("LEFT") && !captureDuck.inputProfile.Pressed("RIGHT") && !captureDuck.inputProfile.Pressed("UP") && !captureDuck.inputProfile.Pressed("RAGDOLL"))
        {
            return captureDuck.inputProfile.Pressed("JUMP");
        }
        return true;
    }

    public void UpdateInput()
    {
        sleepingBagTimer -= Maths.IncFrameTimer();
        if (sleepingBagTimer < 0f && sleepingBagHealth > 20)
        {
            sleepingBagHealth -= 4;
            sleepingBagTimer = 1f;
        }
        if (!_duck.dead)
        {
            if (_duck.HasEquipment(typeof(FancyShoes)) && !jetting)
            {
                if (captureDuck.inputProfile.Pressed("RIGHT"))
                {
                    Vec2 velVec = (_part1.position - _part2.position).Rotate((float)Math.PI / 2f, Vec2.Zero);
                    part1.velocity += velVec * 0.2f;
                    velVec = (_part3.position - _part2.position).Rotate((float)Math.PI / 2f, Vec2.Zero);
                    part3.velocity += velVec * 0.2f;
                }
                else if (captureDuck.inputProfile.Pressed("LEFT"))
                {
                    Vec2 velVec2 = (_part1.position - _part2.position).Rotate((float)Math.PI / 2f, Vec2.Zero);
                    part1.velocity += velVec2 * -0.2f;
                    velVec2 = (_part3.position - _part2.position).Rotate((float)Math.PI / 2f, Vec2.Zero);
                    part3.velocity += velVec2 * -0.2f;
                }
            }
            else if (_timeSinceNudge > 1f && !jetting)
            {
                if (captureDuck.inputProfile.Pressed("LEFT"))
                {
                    float randy = NetRand.Float(-2f, 2f);
                    _part1.vSpeed += randy;
                    _part3.vSpeed += NetRand.Float(-2f, 2f);
                    _part2.hSpeed += NetRand.Float(-2f, -1.2f);
                    _part2.vSpeed -= NetRand.Float(1f, 1.5f);
                    _timeSinceNudge = 0f;
                    ShakeOutOfSleepingBag();
                }
                else if (captureDuck.inputProfile.Pressed("RIGHT"))
                {
                    _part1.vSpeed += NetRand.Float(-2f, 2f);
                    _part3.vSpeed += NetRand.Float(-2f, 2f);
                    _part2.hSpeed += NetRand.Float(1.2f, 2f);
                    _part2.vSpeed -= NetRand.Float(1f, 1.5f);
                    _timeSinceNudge = 0f;
                    ShakeOutOfSleepingBag();
                }
                else if (captureDuck.inputProfile.Pressed("UP"))
                {
                    _part1.vSpeed += NetRand.Float(-2f, 1f);
                    _part3.vSpeed += NetRand.Float(-2f, 1f);
                    _part2.vSpeed -= NetRand.Float(1.5f, 2f);
                    _timeSinceNudge = 0f;
                    ShakeOutOfSleepingBag();
                }
            }
        }
        bool canRise = false;
        if (captureDuck.HasEquipment(typeof(FancyShoes)) && Math.Abs(_part1.x - _part3.x) < 9f && _part1.y < _part3.y)
        {
            canRise = true;
        }
        if (tongueStuckThing != null && tongueStuckThing.removeFromLevel)
        {
            tongueStuck = Vec2.Zero;
            if (Network.isActive)
            {
                Thing.Fondle(this, DuckNetwork.localConnection);
            }
            _makeActive = true;
        }
        if (_duck.dead || (!captureDuck.inputProfile.Pressed("RAGDOLL") && !captureDuck.inputProfile.Pressed("JUMP")) || (!(_part1.framesSinceGrounded < 5 || _part2.framesSinceGrounded < 5 || _part3.framesSinceGrounded < 5 || _part1.doFloat || part2.doFloat || _part3.doFloat || canRise) && _part1.owner == null && _part2.owner == null && _part3.owner == null))
        {
            return;
        }
        if (inSleepingBag)
        {
            if (_timeSinceNudge > 1f)
            {
                _part1.vSpeed += NetRand.Float(-2f, 1f);
                _part3.vSpeed += NetRand.Float(-2f, 1f);
                _part2.vSpeed -= NetRand.Float(1.5f, 2f);
                _timeSinceNudge = 0f;
                ShakeOutOfSleepingBag();
            }
        }
        else if (!_part1.held && !_part2.held && !_part3.held && (tongueStuck == Vec2.Zero || tongueShakes > 5) && base.isServerForObject)
        {
            tongueStuck = Vec2.Zero;
            if (Network.isActive)
            {
                Thing.Fondle(this, DuckNetwork.localConnection);
            }
            _makeActive = true;
        }
    }

    public void ShakeOutOfSleepingBag()
    {
        tongueShakes++;
        if (_part1 != null && _part1.owner == null && _part2 != null && _part2.owner == null && _part3 != null && _part3.owner == null && tongueStuck != Vec2.Zero && tongueShakes > 5)
        {
            tongueStuck = Vec2.Zero;
        }
        if (sleepingBagHealth < 0 || captureDuck == null)
        {
            return;
        }
        sleepingBagHealth = (byte)Math.Max(sleepingBagHealth - 5, 0);
        if (sleepingBagHealth == 0)
        {
            if (inSleepingBag && captureDuck.isServerForObject)
            {
                _makeActive = true;
            }
            inSleepingBag = false;
        }
    }

    public void UpdateUnragdolling()
    {
        if (!isInPipe && captureDuck != null && captureDuck.isServerForObject && _makeActive)
        {
            Unragdoll();
        }
    }
}
