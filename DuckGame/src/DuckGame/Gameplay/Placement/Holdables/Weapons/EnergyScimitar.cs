using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[EditorGroup("Guns|Melee")]
public class EnergyScimitar : Gun
{
    public enum Stance
    {
        None,
        Drag,
        SwingUp,
        SwingDown,
        Intermediate
    }

    public class Blocker : MaterialThing
    {
        private EnergyScimitar _parent;

        public Blocker(EnergyScimitar pParent)
            : base(0f, 0f)
        {
            thickness = 100f;
            _editorCanModify = false;
            visible = false;
            _parent = pParent;
            weight = 0.01f;
        }

        public override bool Hit(Bullet bullet, Vec2 hitPos)
        {
            if (!_solid)
            {
                return false;
            }
            if (_parent != null)
            {
                _parent.Shing();
            }
            if (bullet.ammo is ATLaser && bullet.ammo.canBeReflected)
            {
                bullet.reboundOnce = true;
                return true;
            }
            return base.Hit(bullet, hitPos);
        }
    }

    private class ScimiPlatform : Platform
    {
        public EnergyScimitar scimitar;

        public ScimiPlatform(float x, float y, float wid, float hi, EnergyScimitar pScimitar)
            : base(x, y, wid, hi)
        {
            scimitar = pScimitar;
        }
    }

    private class RagdollDrag
    {
        public RagdollPart part;

        public Vec2 offset;
    }

    public StateBinding _throwSpinBinding = new StateBinding(doLerp: true, nameof(_throwSpin));

    public StateBinding _stanceBinding = new StateBinding(doLerp: true, nameof(stanceInt));

    public StateBinding _lerpedAngleBinding = new StateBinding(doLerp: true, nameof(_lerpedAngle));

    public StateBinding _swordAngleBinding = new StateBinding(doLerp: true, nameof(_swordAngle));

    public StateBinding _stuckBinding = new StateBinding(nameof(stuck));

    public StateBinding _airFlyBinding = new StateBinding(doLerp: true, nameof(_airFly));

    public StateBinding _airFlyDirBinding = new StateBinding(doLerp: true, nameof(_airFlyAngle));

    public StateBinding _wasLiftedBinding = new StateBinding(nameof(_wasLifted));

    private bool _stuck;

    public float _swordAngle;

    public float _lerpedAngle;

    public bool dragSpeedBonus;

    protected Vec2 centerHeld = new Vec2(6f, 26f);

    protected Vec2 centerUnheld = new Vec2(4f, 22f);

    public Stance _stance = Stance.SwingUp;

    private bool _stanceReady;

    private float _dragAngle = -110f;

    private float _dragAngleDangle = -145f;

    private bool _swordFlip;

    private bool _stanceHeld;

    private float _stanceCounter;

    private float _swingDif;

    private int _timeSinceDragJump = 11111;

    private float _lerpBoost;

    private bool _goIntermediate;

    private bool _blocking;

    public bool _spikeDrag;

    public float _dragRand;

    private int _timeSincePress;

    public StateBinding _glowBinding = new StateBinding(nameof(_glow));

    private MaterialEnergyBlade _bladeMaterial;

    private Sprite _blade;

    private Sprite _bladeTrail;

    private List<Blocker> _walls = new List<Blocker>();

    private Platform _platform;

    private Sprite _whiteGlow;

    private Sprite _warpLine;

    public Color properColor = new Color(178, 220, 239);

    public Color swordColor;

    private LoopingSound _hum;

    private float _timeTillPulse;

    private List<RagdollDrag> _drag = new List<RagdollDrag>();

    public float _throwSpin;

    public bool _airFly;

    public float _airFlyAngle;

    public bool _canAirFly = true;

    private float _upFlyTime;

    public float _airFlySpeed = 14f;

    private int timeSinceReversal;

    private bool _wasLifted;

    private bool skipThrowMove;

    public MaterialThing _stuckInto;

    private float _glow;

    private bool _longCharge;

    private float _angleWhoom;

    private float slowWait;

    private bool _slowV;

    private Duck _revertVMaxDuck;

    private float _vmaxReversion = 1f;

    protected float[] _lastAngles = new float[8];

    protected Vec2[] _lastPositions = new Vec2[8];

    protected int _lastIndex;

    protected int _lastSize;

    private bool _playedChargeUp;

    private float _unchargeWait;

    private float _lastAngleHum;

    private float _timeSincePickedUp = 10f;

    private bool _didOwnerSwitchLogic = true;

    private float _stickWait;

    private float _timeSinceBlast = 99f;

    private int _glowTime;

    private Vec2 _lastBarrelStartPos;

    private Vec2 _lastBarrelPos;

    private float _humAmount;

    public List<WarpLine> warpLines = new List<WarpLine>();

    public bool stuck
    {
        get
        {
            if (base.isServerForObject)
            {
                return _stuckInto != null;
            }
            return _stuck;
        }
        set
        {
            _stuck = value;
        }
    }

    public int stanceInt
    {
        get
        {
            return (int)_stance;
        }
        set
        {
            _stance = (Stance)value;
        }
    }

    public override float Angle
    {
        get
        {
            if (owner is WireMount)
            {
                return AngleValue;
            }
            if (!base.held && owner != null)
            {
                return AngleValue + (float)Math.PI / 2f * (float)offDir;
            }
            return Maths.DegToRad(_lerpedAngle) * (float)offDir + Maths.DegToRad(_throwSpin);
        }
        set
        {
            AngleValue = value;
        }
    }

    public Vec2 barrelStartPos => Position + (Offset(base.barrelOffset) - Position).Normalized * 2f;

    public float angleWhoom => _angleWhoom;

    public override bool CanTapeTo(Thing pThing)
    {
        if (pThing is TapedSword || pThing is Sword)
        {
            return false;
        }
        return true;
    }

    public override void CheckIfHoldObstructed()
    {
        if (owner is Duck duckOwner)
        {
            duckOwner.holdObstructed = false;
        }
    }

    public override void PressAction()
    {
        if (owner is WireMount)
        {
            WireMount m = owner as WireMount;
            foreach (Block b in Level.CheckRectAll<Block>(Position + new Vec2(-8f, -8f), Position + new Vec2(8f, 8f)))
            {
                base.clip.Add(b);
            }
            float degrees = 0f - base.AngleDegrees - 90f - 180f;
            owner = null;
            m._containedThing = null;
            StartFlying(degrees);
        }
        base.PressAction();
    }

    private void UpdateStance()
    {
        _timeSincePress++;
        _timeSinceDragJump++;
        if (base.duck == null || !base.held)
        {
            _stance = Stance.None;
            _swordAngle = 0f;
            _lerpedAngle = ((owner == null) ? (_wasLifted ? 90 : 0) : 0);
            _swordFlip = offDir < 0;
            _framesSinceThrown++;
            Center = centerUnheld;
            collisionOffset = new Vec2(-2f, -16f);
            collisionSize = new Vec2(4f, 26f);
            if (_wasLifted)
            {
                if (_airFly)
                {
                    if (vSpeed < -4f)
                    {
                        collisionOffset = new Vec2(0f, -4f);
                        collisionSize = new Vec2(6f, 8f);
                    }
                    else if (vSpeed > 4f)
                    {
                        collisionOffset = new Vec2(-5f, -4f);
                        collisionSize = new Vec2(6f, 8f);
                    }
                    else
                    {
                        collisionOffset = new Vec2(-4f, 0f);
                        collisionSize = new Vec2(8f, 6f);
                    }
                }
                else
                {
                    Center = new Vec2(6f, 22f);
                    collisionOffset = new Vec2(-4f, -3f);
                    collisionSize = new Vec2(8f, 6f);
                }
            }
            if (owner != null || stuck || !_wasLifted)
            {
                return;
            }
            bool spinning = false;
            bool againstWall = false;
            if (_airFly)
            {
                PerformAirSpin();
                spinning = true;
            }
            else if (Math.Abs(hSpeed) + Math.Abs(vSpeed) > 2f || !base.grounded)
            {
                if (!base.grounded && Level.CheckRect<Block>(Position + new Vec2(-6f, -6f), Position + new Vec2(6f, -2f)) != null)
                {
                    againstWall = true;
                }
                if (!againstWall && !_grounded && (Level.CheckPoint<IPlatform>(Position + new Vec2(0f, 8f)) == null || vSpeed < 0f || _airFly))
                {
                    PerformAirSpin();
                    spinning = true;
                }
            }
            if (!(!spinning || againstWall) || _airFly)
            {
                return;
            }
            _throwSpin %= 360f;
            if (againstWall)
            {
                if (Math.Abs(_throwSpin - 90f) < Math.Abs(_throwSpin + 90f))
                {
                    _throwSpin = Lerp.Float(_throwSpin, 90f, 16f);
                }
                else
                {
                    _throwSpin = Lerp.Float(-90f, 0f, 16f);
                }
                return;
            }
            if (_throwSpin > 90f && _throwSpin < 270f)
            {
                _throwSpin = Lerp.Float(_throwSpin, 180f, 14f);
                return;
            }
            if (_throwSpin > 180f)
            {
                _throwSpin -= 360f;
            }
            else if (_throwSpin < -180f)
            {
                _throwSpin += 360f;
            }
            _throwSpin = Lerp.Float(_throwSpin, 0f, 14f);
            return;
        }
        if (_stance == Stance.None)
        {
            _stance = Stance.SwingUp;
        }
        _framesSinceThrown = 0;
        Center = centerHeld;
        collisionOffset = new Vec2(-4f, 0f);
        collisionSize = new Vec2(4f, 4f);
        _throwSpin = 0f;
        _wasLifted = true;
        _blocking = base.duck.crouch && Math.Abs(base.duck.hSpeed) < 2f;
        if (base.duck.inputProfile.Pressed("UP") && !base.duck.inputProfile.Pressed("JUMP") && (_stance == Stance.Drag || _stance == Stance.Intermediate) && !base.duck.sliding)
        {
            _stance = Stance.SwingUp;
        }
        if (base.duck.crouch && !base.duck.sliding && base.duck.inputProfile.Pressed("LEFT"))
        {
            base.duck.offDir = -1;
        }
        else if (base.duck.crouch && !base.duck.sliding && base.duck.inputProfile.Pressed("RIGHT"))
        {
            base.duck.offDir = 1;
        }
        bool droop = Level.CheckLine<IPlatform>(new Vec2(owner.Position.X, owner.bottom) + new Vec2(-offDir * 16, -10f), new Vec2(owner.Position.X, owner.bottom) + new Vec2(-offDir * 16, 2f)) == null;
        _spikeDrag = base.duck.grounded && !droop && Level.CheckLine<Spikes>(new Vec2(owner.Position.X, owner.bottom) + new Vec2(-offDir * 16, -10f), new Vec2(owner.Position.X, owner.bottom) + new Vec2(-offDir * 16, 2f)) != null;
        _dragRand = Lerp.FloatSmooth(_dragRand, 0f, 0.1f);
        if (_dragRand > 1f)
        {
            _dragRand = 1f;
        }
        dragSpeedBonus = _stance == Stance.Drag && !droop && _stanceReady;
        if (_spikeDrag && dragSpeedBonus)
        {
            _dragRand += Rando.Float(Math.Abs(base.duck.hSpeed)) * 0.1f;
            if (Rando.Int(30) == 0 && _dragRand > 0.1f)
            {
                base.duck.Swear();
            }
        }
        if (!base.duck.grounded && base.duck.inputProfile.Pressed("DOWN") && _stance != Stance.Drag)
        {
            _goIntermediate = true;
        }
        if (_stance == Stance.Drag && base.duck._hovering)
        {
            _stance = Stance.SwingDown;
            base.duck.vSpeed = -6f;
            base.duck._hovering = false;
            _timeSinceDragJump = 100;
        }
        if (base.duck.inputProfile.Pressed("DOWN") && base.duck.grounded)
        {
            _stance = Stance.Drag;
        }
        if (Math.Abs(base.duck.hSpeed) > 1f)
        {
            if (dragSpeedBonus)
            {
                Spark spark = Spark.New(base.barrelPosition.X, base.barrelPosition.Y - 6f, new Vec2(Rando.Float(-1f, 1f), Rando.Float(-1f, 1f)));
                spark._color = swordColor;
                spark._width = 1f;
                _glow = 0.3f;
                Level.Add(spark);
            }
            if (_stance == Stance.Drag && base.duck.grounded)
            {
                if (base.duck.sliding || base.duck.crouch)
                {
                    base.duck.tilt = 0f;
                    base.duck.verticalOffset = 0f;
                }
                else
                {
                    float val = base.duck.hSpeed - (float)Math.Sign(base.duck.hSpeed);
                    if (Math.Sign(val) != Math.Sign(base.duck.hSpeed))
                    {
                        val = 0f;
                    }
                    base.duck.tilt = val;
                    base.duck.verticalOffset = Math.Abs(val);
                }
            }
        }
        if (_stance == Stance.SwingDown && base.duck.inputProfile.Pressed("JUMP"))
        {
            _stance = Stance.SwingUp;
        }
        if (_goIntermediate && _stanceReady)
        {
            _stance = Stance.Intermediate;
            _goIntermediate = false;
        }
        handAngle = 0f;
        _holdOffset = new Vec2(0f, 0f);
        handOffset = new Vec2(0f, 0f);
        handFlip = false;
        if (_stance == Stance.Intermediate)
        {
            _swordAngle = -60f;
            handAngle = Maths.DegToRad(_swordAngle - 90f) * (float)offDir;
            _holdOffset = new Vec2(0f, 2f);
            _swordFlip = offDir > 0;
            if (base.duck.grounded)
            {
                _stance = Stance.Drag;
            }
        }
        else if (_stance == Stance.Drag)
        {
            if (base.duck._hovering)
            {
                _swordAngle = -190f;
                handAngle = Maths.DegToRad(_swordAngle - 90f) * (float)offDir;
                _holdOffset = new Vec2(0f, 2f);
                _swordFlip = offDir > 0;
            }
            else
            {
                _swordAngle = ((!droop) ? _dragAngle : _dragAngleDangle);
                if (base.duck.crouch)
                {
                    _swordAngle += 10f;
                }
                if (base.duck.sliding)
                {
                    _swordAngle += 10f;
                }
                handAngle = Maths.DegToRad(_swordAngle - 90f) * (float)offDir;
                _holdOffset = new Vec2(0f, 2f);
                _swordFlip = offDir > 0;
            }
        }
        else if (_stance == Stance.SwingUp)
        {
            if (base.duck._hovering)
            {
                _swordAngle = -25f;
                _swordFlip = offDir < 0;
            }
            else if (_blocking)
            {
                _swordAngle = 130f;
                handAngle = Maths.DegToRad(_swordAngle) * (float)offDir;
                _holdOffset = new Vec2(-22f, 0f);
                handOffset = new Vec2(7f, -7f);
                _swordFlip = offDir >= 0;
                handFlip = true;
            }
            else
            {
                _swordAngle = 25f;
                handAngle = Maths.DegToRad(_swordAngle) * (float)offDir;
                _holdOffset = new Vec2(-2f + _swingDif * 0.55f, -4f);
                handOffset = new Vec2(2f + _swingDif * 0.35f, -3f);
                _swordFlip = offDir < 0;
            }
        }
        else if (_stance == Stance.SwingDown)
        {
            if (base.duck._hovering)
            {
                _swordAngle = 40f;
                _holdOffset = new Vec2(0f, 8f);
                handOffset = new Vec2(2f, 0f);
                _swordFlip = offDir < 0;
            }
            else if (_blocking)
            {
                _swordAngle = 45f;
                handAngle = Maths.DegToRad(_swordAngle) * (float)offDir;
                _holdOffset = new Vec2(3f, -3f);
                handOffset = new Vec2(7f, 0f);
                _swordFlip = offDir < 0;
            }
            else
            {
                _swordAngle = 80f;
                handAngle = Maths.DegToRad(_swordAngle) * (float)offDir;
                _holdOffset = new Vec2(0f, -2f - _swingDif * 0.55f);
                handOffset = new Vec2(0f + _swingDif * 0.35f, 3f);
                _swordFlip = offDir < 0;
            }
        }
        _lerpedAngle = Lerp.FloatSmooth(_lerpedAngle, _swordAngle, 0.25f + _lerpBoost);
        _stanceReady = Math.Abs(_lerpedAngle - _swordAngle) < 25f;
        _stanceCounter += Maths.IncFrameTimer();
        _swingDif = Math.Min(Math.Abs(_lerpedAngle - _swordAngle), 35f);
        if (_timeSincePress > 25)
        {
            _swingDif *= 0.25f;
        }
        _lerpBoost = Lerp.FloatSmooth(_lerpBoost, 0f, 0.1f);
    }

    public override void Ejected(Thing pFrom)
    {
        ResetTrailHistory();
        if (pFrom is SpawnCannon)
        {
            StartFlying((pFrom as SpawnCannon).direction);
            return;
        }
        if (vSpeed < -0.1f)
        {
            StartFlying(TileConnection.Up);
        }
        if (vSpeed > 0.1f)
        {
            StartFlying(TileConnection.Down);
        }
        if (hSpeed < -0.1f)
        {
            StartFlying(TileConnection.Left);
        }
        if (hSpeed > 0.1f)
        {
            StartFlying(TileConnection.Right);
        }
    }

    public override void OnPressAction()
    {
        if (!base.isServerForObject || base.duck == null || receivingPress)
        {
            return;
        }
        _goIntermediate = false;
        if (_timeSincePress > 3)
        {
            _timeSincePress = 0;
        }
        if (_stance == Stance.Intermediate)
        {
            _stance = Stance.SwingDown;
        }
        if (!_stanceReady)
        {
            return;
        }
        if (_stance == Stance.Drag || _stance == Stance.SwingUp)
        {
            if (_stance == Stance.Drag && base.duck != null && (!base.duck.grounded || (!base.duck.crouch && !base.duck.sliding)))
            {
                base.duck.hSpeed = base.duck.offDir * 9;
                base.duck.vSpeed = -2f;
                base.duck._disarmDisable = 5;
            }
            else if (_stance == Stance.Drag && (base.duck.crouch || base.duck.sliding))
            {
                _lerpBoost = 0.4f;
            }
            _stance = Stance.SwingDown;
        }
        else if (_stance == Stance.SwingDown)
        {
            _stance = Stance.SwingUp;
        }
    }

    public override void Fire()
    {
        _stanceHeld = true;
        _stanceCounter = 0f;
    }

    public EnergyScimitar(float pX, float pY)
        : base(pX, pY)
    {
        graphic = new Sprite("energyScimiHilt");
        Center = new Vec2(6f, 26f);
        collisionOffset = new Vec2(-2f, -24f);
        collisionSize = new Vec2(4f, 28f);
        _blade = new Sprite("energyScimiBlade");
        _bladeTrail = new Sprite("energyScimiBladeTrail");
        _whiteGlow = new Sprite("whiteGlow");
        _whiteGlow.Center = new Vec2(16f, 28f);
        _whiteGlow.ScaleX = 0.8f;
        _whiteGlow.ScaleY = 1.4f;
        _fullAuto = true;
        _bouncy = 0.5f;
        _impactThreshold = 0.3f;
        ammo = 99999;
        _ammoType = new ATLaser();
        thickness = 0.01f;
        _impactThreshold = 0.5f;
        _bladeMaterial = new MaterialEnergyBlade(this);
        swordColor = properColor;
        _warpLine = new Sprite("warpLine2");
        editorTooltip = "How do you invent a sword? It uses modern technology.";
    }

    public override void Initialize()
    {
        _platform = new ScimiPlatform(0f, 0f, 20f, 8f, this);
        _platform.solid = false;
        _platform.enablePhysics = false;
        _platform.Center = new Vec2(10f, 4f);
        _platform.collisionOffset = new Vec2(-10f, -2f);
        _platform.thickness = 0.01f;
        Level.Add(_platform);
        _hum = new LoopingSound("scimiHum");
        _hum.volume = 0f;
        _humAmount = 0f;
        _hum.lerpSpeed = 1f;
        base.Initialize();
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        return false;
    }

    public override void Terminate()
    {
        if (_hum != null)
        {
            _hum.Kill();
        }
        foreach (Blocker wall in _walls)
        {
            Level.Remove(wall);
        }
        _walls.Clear();
        if (_platform != null)
        {
            Level.Remove(_platform);
        }
        base.Terminate();
    }

    public void Pulse()
    {
        if (_timeTillPulse < 0f)
        {
            _timeTillPulse = 0.2f;
            SFX.Play("scimiSurge", 0.8f, Rando.Float(-0.2f, 0.2f));
            _glow = 12f;
            Vec2 vec = (Position - base.barrelPosition).Normalized;
            Vec2 start = base.barrelPosition;
            for (int i = 0; i < 6; i++)
            {
                Spark spark = Spark.New(start.X, start.Y, new Vec2(Rando.Float(-1f, 1f), Rando.Float(-1f, 1f)));
                spark._color = swordColor;
                spark._width = 1f;
                Level.Add(spark);
                start += vec * 4f;
            }
        }
    }

    public override bool Destroy(DestroyType type = null)
    {
        return base.Destroy(type);
    }

    public override void DoTerminate()
    {
        base.DoTerminate();
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (owner != null || !base.isServerForObject)
        {
            return;
        }
        if (_airFly && with is EnergyScimitar && (with as EnergyScimitar)._airFly)
        {
            Fondle(with);
            Vec2 travel = Maths.AngleToVec(Maths.DegToRad(_airFlyAngle));
            hSpeed = (lastHSpeed = (0f - travel.X) * 3f);
            vSpeed = (lastVSpeed = (0f - travel.Y) * 3f);
            Vec2 wtravel = Maths.AngleToVec(Maths.DegToRad((with as EnergyScimitar)._airFlyAngle));
            with.hSpeed = (with.lastHSpeed = (0f - wtravel.X) * 3f);
            with.vSpeed = (with.lastVSpeed = (0f - wtravel.Y) * 3f);
            Shing();
            (with as EnergyScimitar).Shing();
        }
        else if (_airFly && with is PhysicsObject && !(with is Gun) && !(with is Equipment) && !(with is Duck) && !(with is RagdollPart))
        {
            with.Destroy(new DTIncinerate(this));
        }
        else if (_airFly && with is Duck && (with != _prevOwner || _framesSinceThrown > 15))
        {
            with.Destroy(new DTImpale(this));
            Duck d = with as Duck;
            if (d.ragdoll != null && d.ragdoll.part2 != null && _drag.FirstOrDefault((RagdollDrag x) => x.part == with) == null)
            {
                _drag.Add(new RagdollDrag
                {
                    part = d.ragdoll.part2,
                    offset = Position - d.ragdoll.part2.Position
                });
            }
        }
        else if (with is Block || (with is IPlatform && from == ImpactedFrom.Bottom && vSpeed > 0f))
        {
            if (!(with is Nubber))
            {
                Shing();
                if (_framesSinceThrown > 5)
                {
                    _framesSinceThrown = 25;
                }
            }
        }
        else
        {
            if (!_airFly || !(with is RagdollPart))
            {
                return;
            }
            RagdollPart p = with as RagdollPart;
            if ((p.doll == null || p.doll.captureDuck == null || p.doll.captureDuck != _prevOwner || _framesSinceThrown > 15) && _drag.FirstOrDefault((RagdollDrag x) => x.part == with) == null && p.doll != null)
            {
                if (p.doll.part1 != null)
                {
                    _drag.Add(new RagdollDrag
                    {
                        part = p.doll.part1,
                        offset = Position - p.doll.part1.Position
                    });
                    p.doll.part1.owner = this;
                }
                if (p.doll.part2 != null)
                {
                    _drag.Add(new RagdollDrag
                    {
                        part = p.doll.part2,
                        offset = Position - p.doll.part2.Position
                    });
                    p.doll.part2.owner = this;
                }
                if (p.doll.part3 != null)
                {
                    _drag.Add(new RagdollDrag
                    {
                        part = p.doll.part3,
                        offset = Position - p.doll.part3.Position
                    });
                    p.doll.part3.owner = this;
                }
            }
        }
    }

    public Vec2 TravelThroughAir(float pMult = 1f)
    {
        Vec2 trav = Maths.AngleToVec(Maths.DegToRad(_airFlyAngle));
        Position += trav * _airFlySpeed * pMult;
        return trav;
    }

    public void ReverseFlyDirection()
    {
        if (timeSinceReversal > 10)
        {
            timeSinceReversal = 0;
            _airFlyAngle += 180f;
            offDir *= -1;
            TravelThroughAir();
            PerformAirSpin();
        }
    }

    private void UpdateAirDirection()
    {
        if (offDir < 0)
        {
            _throwSpin = 0f - _airFlyAngle + 180f;
        }
        else
        {
            _throwSpin = 0f - _airFlyAngle;
        }
    }

    protected void PerformAirSpin()
    {
        timeSinceReversal++;
        if (!enablePhysics)
        {
            return;
        }
        if (_airFly)
        {
            Vec2 flyVector = Maths.AngleToVec(Maths.DegToRad(_airFlyAngle));
            offDir = (sbyte)((!(flyVector.X < -0.1f)) ? 1 : (-1));
            if (Math.Abs(flyVector.X) < 0.2f && flyVector.Y < 0f)
            {
                _upFlyTime += Maths.IncFrameTimer();
                if (_upFlyTime > 2f)
                {
                    ReverseFlyDirection();
                }
            }
            UpdateAirDirection();
            if (!skipThrowMove)
            {
                hSpeed = flyVector.X * _airFlySpeed;
                vSpeed = flyVector.Y * _airFlySpeed;
            }
            _impactThreshold = 0.01f;
            _bouncy = 0f;
            hMax = _airFlySpeed;
            vMax = _airFlySpeed;
            _lerpedAngle = 90f;
            gravMultiplier = 0f;
        }
        else
        {
            _impactThreshold = 0.3f;
            _bouncy = 0.5f;
            hMax = 12f;
            if (hSpeed > 0f)
            {
                _throwSpin += (Math.Abs(hSpeed) + Math.Abs(vSpeed)) * 2f + 4f;
            }
            else
            {
                _throwSpin -= (Math.Abs(hSpeed) + Math.Abs(vSpeed)) * 2f + 4f;
            }
        }
    }

    public override void ReturnToWorld()
    {
        if (!_airFly)
        {
            _throwSpin = 90f;
        }
    }

    public void StartFlying(TileConnection pDirection, bool pThrown = false)
    {
        switch (pDirection)
        {
            case TileConnection.Left:
                StartFlying(180f, pThrown);
                break;
            case TileConnection.Right:
                StartFlying(0f, pThrown);
                break;
            case TileConnection.Up:
                StartFlying(90f, pThrown);
                break;
            case TileConnection.Down:
                StartFlying(270f, pThrown);
                break;
        }
    }

    public void StartFlying(float pAngleDegrees, bool pThrown = false)
    {
        if (owner == null)
        {
            _wasLifted = true;
            _airFly = true;
            _upFlyTime = 0f;
            _airFlyAngle = pAngleDegrees;
            if (pThrown)
            {
                TravelThroughAir(-0.5f);
            }
            skipThrowMove = true;
            PerformAirSpin();
            skipThrowMove = false;
            UpdateStance();
            ResetTrailHistory();
        }
    }

    public override void Thrown()
    {
        if (base.duck == null)
        {
            return;
        }
        base.X = base.duck.X;
        Depth oldDepth = (base.Depth = -0.1f);
        _oldDepth = oldDepth;
        if (!base.isServerForObject || base.duck == null || base.duck.destroyed || !_canAirFly || _airFly)
        {
            return;
        }
        _upFlyTime = 0f;
        if (!base.duck.inputProfile.Down("GRAB"))
        {
            return;
        }
        if ((base.duck.inputProfile.Down("LEFT") && base.duck.offDir < 0) || (base.duck.inputProfile.Down("RIGHT") && base.duck.offDir > 0))
        {
            base.Y = base.duck.Y;
            if (_stance == Stance.Drag)
            {
                base.Y += 6f;
            }
            skipThrowMove = true;
            TileConnection dir = (base.duck.inputProfile.Down("LEFT") ? TileConnection.Left : TileConnection.Right);
            owner = null;
            StartFlying(dir, pThrown: true);
            skipThrowMove = false;
        }
        else if ((base.duck.inputProfile.Down("UP") || base.duck.inputProfile.Down("DOWN")) && (base.duck.inputProfile.Down("UP") || !base.duck.grounded))
        {
            int airFlyDir = 1;
            if (base.duck.inputProfile.Down("UP"))
            {
                airFlyDir = -1;
            }
            base.X = base.duck.X + (float)base.duck.offDir * -2f;
            if (airFlyDir == 1 && !base.duck.grounded)
            {
                base.duck.vSpeed -= 8f;
            }
            skipThrowMove = true;
            TileConnection dir2 = TileConnection.Down;
            if (airFlyDir < 0)
            {
                dir2 = TileConnection.Up;
            }
            owner = null;
            StartFlying(dir2, pThrown: true);
            skipThrowMove = false;
        }
    }

    private void UpdateStuck()
    {
        if (stuck)
        {
            UpdateAirDirection();
        }
    }

    public void Shing()
    {
        if (stuck)
        {
            return;
        }
        gravMultiplier = 1f;
        ClearDrag();
        Pulse();
        _timeSinceBlast = 0f;
        if (_airFly && base.isServerForObject)
        {
            Vec2 travel = TravelThroughAir(-0.5f);
            Vec2 hitPos = Vec2.Zero;
            MaterialThing hit = Level.CheckRay<Block>(Position, Position + travel * _airFlySpeed, out hitPos);
            if (hit != _platform)
            {
                if (hit is ScimiPlatform)
                {
                    hSpeed = (lastHSpeed = (0f - travel.X) * 3f);
                    vSpeed = (lastHSpeed = (0f - travel.Y) * 3f);
                }
                else
                {
                    if (hit != null)
                    {
                        base.clip.Add(hit);
                        Position = hitPos - travel * 16f;
                        UpdateAirDirection();
                    }
                    if (hit != null)
                    {
                        _stuckInto = hit;
                        _longCharge = true;
                        enablePhysics = false;
                        hSpeed = 0f;
                        vSpeed = 0f;
                        lastHSpeed = _hSpeed;
                        lastVSpeed = _vSpeed;
                        base.Depth = -0.55f;
                    }
                    else
                    {
                        vSpeed = (0f - vSpeed) * 0.25f;
                        hSpeed = (0f - hSpeed) * 0.25f;
                    }
                }
            }
        }
        _airFly = false;
    }

    protected void QuadLaserHit(QuadLaserBullet pBullet)
    {
        if (base.isServerForObject)
        {
            Fondle(pBullet);
            EnergyScimitarBlast b = new EnergyScimitarBlast(pBullet.Position, new Vec2(offDir * 2000, 0f));
            Level.Add(b);
            Level.Remove(pBullet);
            if (Network.isActive)
            {
                Send.Message(new NMEnergyScimitarBlast(b.Position, b._target));
            }
        }
    }

    public override void OnTeleport()
    {
        ResetTrailHistory();
        base.OnTeleport();
    }

    public void ClearDrag()
    {
        int num = 1;
        foreach (RagdollDrag d in _drag)
        {
            if (d.part.doll != null && d.part.doll.captureDuck != null && d.part.doll.captureDuck._cooked == null)
            {
                d.part.Position = Offset(Maths.AngleToVec(Maths.DegToRad(_airFlyAngle)) * 8f);
                d.part.doll.Position = d.part.Position;
                d.part.doll.captureDuck.Position = d.part.Position;
                d.part.doll.captureDuck.Cook();
                d.part.doll.captureDuck.Kill(new DTIncinerate(this));
                if (d.part.doll.captureDuck._cooked != null)
                {
                    d.part.doll.captureDuck._cooked.vSpeed = -(2 + num);
                }
                num++;
            }
        }
        _drag.Clear();
    }

    protected void OnSwing()
    {
        if (base.duck != null && base.isServerForObject)
        {
            if (base.duck._hovering)
            {
                _revertVMaxDuck = base.duck;
                _vmaxReversion = base.duck.vMax;
                base.duck.vMax = 13f;
                base.duck.vSpeed = -13f;
                _slowV = true;
                warpLines.Add(new WarpLine
                {
                    start = base.duck.Position,
                    end = base.duck.Position + new Vec2(0f, -80f),
                    lerp = 0f,
                    wide = 24f
                });
            }
            else
            {
                base.duck.hSpeed = (float)offDir * 11.25f;
                base.duck.vSpeed = -1f;
                _slowV = false;
                warpLines.Add(new WarpLine
                {
                    start = base.duck.Position + new Vec2(-offDir * 16, 4f),
                    end = base.duck.Position + new Vec2(offDir * 62, 4f),
                    lerp = 0f,
                    wide = 20f
                });
            }
            slowWait = 0.085f;
        }
    }

    protected virtual void ResetTrailHistory()
    {
        _lastAngles = new float[8];
        _lastPositions = new Vec2[8];
        _lastIndex = 0;
        _lastSize = 0;
    }

    protected int historyIndex(int idx)
    {
        return (_lastIndex + idx + 1) & 7;
    }

    protected void addHistory(float angle, Vec2 position)
    {
        _lastAngles[_lastIndex] = angle;
        _lastPositions[_lastIndex] = position;
        _lastIndex = (_lastIndex - 1) & 7;
        _lastSize++;
    }

    public override bool Sprung(Thing pSpringer)
    {
        StartFlying(0f - pSpringer.AngleDegrees - 90f - 180f);
        return false;
    }

    public override void Update()
    {
        if (_hum != null)
        {
            _hum.Update();
        }
        if (!base.isServerForObject)
        {
            UpdateStuck();
        }
        _skipAutoPlatforms = _airFly;
        _skipPlatforms = _airFly;
        if (_airFly || _stuckInto != null)
        {
            base.Depth = -0.55f;
        }
        else
        {
            base.Depth = -0.1f;
        }
        if (_stuckInto != null && _stuckInto is Door && Math.Abs((_stuckInto as Door)._open) > 0.5f)
        {
            _stuckInto.Fondle(this);
            enablePhysics = true;
            _stuckInto = null;
        }
        ammo = 999;
        UpdateStance();
        if (_glow < 0.4f)
        {
            _glowTime = 0;
        }
        if (base.duck != null && (_glow > 0.4f || (_glowTime > 0 && _glowTime < 4)))
        {
            _glowTime++;
            foreach (Bullet b in Level.current.things[typeof(Bullet)])
            {
                Vec2 check1 = barrelStartPos + OffsetLocal(new Vec2(8f, 0f));
                Vec2 check2 = base.barrelPosition;
                bool col = Collision.LineIntersect(check1 + base.velocity, check2 + base.velocity, b.start, b.start + b.travelDirNormalized * b.bulletSpeed);
                if (!col)
                {
                    col = Collision.LineIntersect(check1 + base.velocity * 0.5f, check2 + base.velocity * 0.5f, b.start, b.start + b.travelDirNormalized * b.bulletSpeed);
                }
                if (col && b.lastReboundSource != this)
                {
                    b.lastReboundSource = this;
                    Bullet newBullet = b.ReverseTravel();
                    if (newBullet != null)
                    {
                        newBullet.owner = base.duck;
                    }
                    Pulse();
                }
            }
        }
        _timeSinceBlast += Maths.IncFrameTimer();
        float max = Math.Min(_angleWhoom, 0.5f) * 38f;
        if (base.isServerForObject)
        {
            _stickWait -= Maths.IncFrameTimer();
            if (base.duck != null && slowWait > 0f)
            {
                slowWait -= Maths.IncFrameTimer();
                if (slowWait <= 0f)
                {
                    if (_revertVMaxDuck != null)
                    {
                        _revertVMaxDuck.vMax = _vmaxReversion;
                        _revertVMaxDuck = null;
                    }
                    if (_slowV)
                    {
                        base.duck.vSpeed *= 0.25f;
                    }
                    else
                    {
                        base.duck.hSpeed *= 0.25f;
                    }
                }
            }
            handFlip = false;
            foreach (RagdollDrag d in _drag)
            {
                d.part.Position = Position - d.offset;
                d.part.hSpeed = 0f;
                d.part.vSpeed = 0f;
            }
            _timeSincePickedUp += Maths.IncFrameTimer();
            if (_stance == Stance.Drag && base.duck != null)
            {
                _glow = ((Math.Abs(base.duck.hSpeed) > 1f) ? 0.35f : 0f);
            }
            if (base.grounded)
            {
                _canAirFly = true;
            }
            _timeTillPulse -= Maths.IncFrameTimer();
            if (owner != null)
            {
                gravMultiplier = 1f;
                if (_glow > 0.4f && _timeSincePickedUp > 0.25f && base.duck != null)
                {
                    Vec2 startStart = barrelStartPos;
                    Vec2 startEnd = barrelStartPos + new Vec2(base.duck.hSpeed * 2f, base.duck.vSpeed);
                    Vec2 endStart = base.barrelPosition;
                    Vec2 endEnd = base.barrelPosition + new Vec2(base.duck.hSpeed * 2f, base.duck.vSpeed);
                    foreach (EnergyScimitar e in Level.current.things[typeof(EnergyScimitar)])
                    {
                        if (e == this || e.owner == base.duck)
                        {
                            continue;
                        }
                        if (e.owner == null && e._airFly && e.offDir != base.duck.offDir && ((Math.Abs(e.hSpeed) > 2f && Collision.Line(barrelStartPos, base.barrelPosition, new Rectangle(e.X + e.hSpeed, e.Y - 8f, Math.Abs(e.hSpeed), 16f))) || (Math.Abs(e.vSpeed) > 2f && Collision.Line(barrelStartPos, base.barrelPosition, new Rectangle(e.X - 8f, e.Y + e.vSpeed, 16f, Math.Abs(e.vSpeed))))))
                        {
                            Fondle(e);
                            e.ReverseFlyDirection();
                            Shing();
                        }
                        if (!(e.owner is Duck) || !(e._glow > 0.4f) || (!Collision.LineIntersect(barrelStartPos, base.barrelPosition, e.barrelStartPos, e.barrelPosition) && !Collision.LineIntersect(startStart, startEnd, e.barrelStartPos, e.barrelPosition) && !Collision.LineIntersect(endStart, endEnd, e.barrelStartPos, e.barrelPosition)) || !(_timeSinceBlast > 0.15f))
                        {
                            continue;
                        }
                        Duck otherDuck = e.owner as Duck;
                        base.duck.X -= base.duck.hSpeed;
                        otherDuck.X -= otherDuck.hSpeed;
                        _timeSinceBlast = 0f;
                        otherDuck.hSpeed = (float)offDir * 5f;
                        otherDuck.vSpeed = -4f;
                        base.duck.hSpeed = (float)(-offDir) * 5f;
                        base.duck.vSpeed = -4f;
                        base.duck.hSpeed *= 2f;
                        base.duck.UpdatePhysics();
                        base.duck.hSpeed /= 2f;
                        otherDuck.hSpeed *= 2f;
                        otherDuck.UpdatePhysics();
                        otherDuck.hSpeed /= 2f;
                        base.duck.swordInvincibility = 10;
                        otherDuck.swordInvincibility = 10;
                        Shing();
                        e.Shing();
                        if (base.isServerForObject && owner != null && otherDuck != null)
                        {
                            EnergyScimitarBlast b2 = new EnergyScimitarBlast((otherDuck.Position + owner.Position) / 2f + new Vec2(0f, -16f), new Vec2(0f, -2000f));
                            if (Network.isActive)
                            {
                                Send.Message(new NMEnergyScimitarBlast(b2.Position, b2._target));
                            }
                            EnergyScimitarBlast thing = new EnergyScimitarBlast((otherDuck.Position + owner.Position) / 2f + new Vec2(0f, 16f), new Vec2(0f, 2000f));
                            if (Network.isActive)
                            {
                                Send.Message(new NMEnergyScimitarBlast(b2.Position, b2._target));
                            }
                            Level.Add(b2);
                            Level.Add(thing);
                        }
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        Vec2 p = Lerp.Vec2Smooth(barrelStartPos, _lastBarrelStartPos, (float)i / 7f);
                        Vec2 barrelEnd = Lerp.Vec2Smooth(base.barrelPosition, _lastBarrelPos, (float)i / 7f);
                        QuadLaserBullet laserHit = Level.CheckLine<QuadLaserBullet>(p, barrelEnd);
                        if (laserHit != null)
                        {
                            QuadLaserHit(laserHit);
                        }
                        foreach (MaterialThing m in Level.CheckLineAll<MaterialThing>(p, barrelEnd))
                        {
                            if (m != base.duck && m != this && m.owner != base.duck && !(m is Gun) && !(m is Equipment) && (m is PhysicsObject || m is Icicles) && (!(m is Duck) || (m as Duck).swordInvincibility <= 0))
                            {
                                if (!m.isServerForObject)
                                {
                                    Thing.SuperFondle(m, DuckNetwork.localConnection);
                                }
                                m.Destroy(new DTIncinerate(this));
                                if (m is Duck && base.duck != null)
                                {
                                    base.duck._disarmDisable = 5;
                                }
                            }
                        }
                    }
                }
                _canAirFly = true;
                ClearDrag();
                if (!_didOwnerSwitchLogic)
                {
                    _didOwnerSwitchLogic = true;
                    _timeSincePickedUp = 0f;
                    foreach (PhysicsObject item in Level.CheckCircleAll<PhysicsObject>(Position, 16f))
                    {
                        item.sleeping = false;
                    }
                }
                float len = 24f + max;
                Vec2 pos = Position + OffsetLocal(new Vec2(0f, 4f));
                foreach (Blocker wall in _walls)
                {
                    pos += OffsetLocal(new Vec2(0f, (0f - len) / (float)_walls.Count));
                    wall.Position = pos;
                    float inc = 1f - Math.Min(_stanceCounter / 0.25f, 1f);
                    wall.collisionSize = new Vec2(6f + inc * 8f, 6f);
                    wall.collisionOffset = new Vec2(-3f - inc * 4f, -3f);
                }
            }
            else
            {
                if (stuck)
                {
                    _didOwnerSwitchLogic = false;
                }
                Vec2 pos2 = Position + OffsetLocal(new Vec2(0f, (_stuckInto != null) ? (-25) : (-14)));
                foreach (Blocker wall2 in _walls)
                {
                    pos2 += OffsetLocal(new Vec2(0f, 18f / (float)_walls.Count));
                    wall2.Position = pos2;
                    wall2.solid = _stanceCounter < 0.15f;
                }
            }
            _lastBarrelPos = base.barrelPosition;
            _lastBarrelStartPos = barrelStartPos;
        }
        else
        {
            _didOwnerSwitchLogic = false;
        }
        float glowMul = Math.Min(_glow, 1f);
        float dif = Math.Min(Math.Abs(_lastAngleHum - Angle), 1f);
        _angleWhoom = Lerp.FloatSmooth(_angleWhoom, dif, 0.2f);
        _humAmount = Lerp.FloatSmooth(_humAmount, Math.Min((Math.Min(Math.Abs(hSpeed) + Math.Abs(vSpeed), 5f) / 10f + dif * 2f + 0.25f + glowMul * 0.3f) * _glow, 0.75f), 0.2f);
        _humAmount = Math.Min(_humAmount + _dragRand * 0.2f, 1f);
        if (_hum != null)
        {
            _hum.volume = _humAmount;
        }
        if (base.level != null)
        {
            float maxDist = 800f;
            float minDist = 400f;
            float dist = Math.Min(Math.Max((base.level.camera.position - Position).Length(), minDist) - minDist, maxDist);
            float atten = 1f - dist / maxDist;
            if (_hum != null)
            {
                _hum.volume *= atten;
            }
            if (base.isServerForObject && visible && (base.X < base.level.topLeft.X - 1000f || base.X > base.level.bottomRight.X + 1000f) && owner == null && !inPipe)
            {
                Level.Remove(this);
            }
        }
        _extraOffset = new Vec2(0f, 0f - max);
        _barrelOffsetTL = new Vec2(4f, 3f - max);
        _lastAngleHum = Angle;
        if (_glow > 1f)
        {
            _glow *= 0.85f;
        }
        if (owner != null)
        {
            _airFly = false;
        }
        if (base.held || _airFly)
        {
            _stuckInto = null;
            if (_longCharge)
            {
                _unchargeWait = 0.5f;
            }
            else
            {
                _unchargeWait = 0.1f;
            }
            _longCharge = false;
            if (!_playedChargeUp && owner != null)
            {
                _playedChargeUp = true;
                SFX.Play("laserChargeShort", 1f, Rando.Float(-0.1f, 0.1f));
            }
            float glowLerp = 1f;
            if (!_stanceReady || _airFly)
            {
                glowLerp = 1f;
                _glow = 1.5f;
            }
            else
            {
                glowLerp = 0f;
            }
            if (_stance == Stance.Drag && base.duck != null)
            {
                _glow = ((Math.Abs(base.duck.hSpeed) > 1f) ? 0.35f : 0f);
            }
            _glow = Lerp.Float(_glow, glowLerp, 0.1f);
        }
        else
        {
            _unchargeWait -= Maths.IncFrameTimer();
            if (_unchargeWait < 0f)
            {
                if (_playedChargeUp && owner == null)
                {
                    _playedChargeUp = false;
                    SFX.Play("laserUnchargeShort", 1f, Rando.Float(-0.1f, 0.1f));
                }
                _glow = Lerp.Float(_glow, 0f, 0.2f);
            }
        }
        _ = _glow;
        _ = 0.1f;
        base.Update();
        _platform.solid = false;
        _platform.enablePhysics = false;
        _platform.Position = new Vec2(-99999f, -99999f);
        if (stuck)
        {
            if (Math.Abs(barrelStartPos.Y - base.barrelPosition.Y) < 6f)
            {
                _platform.solid = true;
                _platform.enablePhysics = true;
                _platform.Position = Offset(new Vec2(0f, -10f));
            }
            Center = new Vec2(6f, 29f);
        }
    }

    public override void DrawGlow()
    {
        if (inPipe)
        {
            return;
        }
        _whiteGlow.Angle = Angle;
        _whiteGlow.color = swordColor;
        _whiteGlow.Alpha = _glow * 0.5f;
        Graphics.Draw(_whiteGlow, base.X, base.Y, base.Depth - 2);
        Color c = swordColor;
        foreach (WarpLine l in warpLines)
        {
            Vec2 vec = l.start - l.end;
            _ = l.end - l.start;
            float lerp1 = Math.Min(l.lerp, 0.5f) / 0.5f;
            float lerp2 = Math.Max((l.lerp - 0.5f) * 2f, 0f);
            Graphics.DrawTexturedLine(_warpLine.texture, l.start - vec * (lerp1 * 0.5f), l.start, c * (1f - lerp2), l.wide / 32f, 0.9f);
            Graphics.DrawTexturedLine(_warpLine.texture, l.start - vec * (lerp1 * 0.5f), l.start - vec * (lerp1 * 1f), c * (1f - lerp2), l.wide / 32f, 0.9f);
            l.lerp += 0.13f;
        }
        warpLines.RemoveAll((WarpLine v) => v.lerp >= 1f);
        base.DrawGlow();
    }

    public override void EditorUpdate()
    {
        _lerpedAngle = Maths.RadToDeg(AngleValue);
        base.EditorUpdate();
    }

    public override void Draw()
    {
        if (inPipe)
        {
            return;
        }
        base.Draw();
        if (DevConsole.showCollision)
        {
            foreach (Blocker wall in _walls)
            {
                Graphics.DrawRect(wall.rectangle, Color.Red, base.Depth + 10);
            }
        }
        float max = Math.Min(_angleWhoom, 0.5f) * 1.5f;
        Graphics.material = _bladeMaterial;
        _bladeMaterial.glow = 0.25f + _glow * 0.75f;
        _blade.Center = Center;
        _bladeTrail.Center = Center;
        _blade.Angle = graphic.Angle;
        _blade.flipH = _swordFlip;
        _bladeTrail.flipH = _blade.flipH;
        _blade.Alpha = base.Alpha;
        _blade.color = Color.Lerp(Color.White, Color.Red, heat);
        swordColor = Color.Lerp(properColor, Color.Red, heat);
        if (_glow > 1f)
        {
            _blade.Scale = new Vec2(1f + (_glow - 1f) * 0.03f, 1f);
        }
        else
        {
            _blade.Scale = new Vec2(1f);
        }
        _bladeTrail.ScaleY = _blade.ScaleY + max;
        Graphics.Draw(_blade, base.X, base.Y, base.Depth - 1);
        Graphics.material = null;
        _ = Position;
        _ = base.Depth;
        _bladeTrail.color = swordColor;
        graphic.color = Color.White;
        if (_glow > 0.5f)
        {
            float rlAngle = Angle;
            _ = AngleValue;
            float alph = 1f;
            Vec2 drawPos = Position;
            _ = Position;
            for (int i = 0; i < 7; i++)
            {
                Vec2 prevPosLock = Vec2.Zero;
                float prevAngLock = 0f;
                for (int j = 0; j < 4; j++)
                {
                    if (_lastSize <= i)
                    {
                        break;
                    }
                    int idx = historyIndex(i);
                    if (j == 0)
                    {
                        prevPosLock = drawPos;
                        prevAngLock = rlAngle;
                    }
                    rlAngle = Lerp.FloatSmooth(prevAngLock, _lastAngles[idx], 0.25f * (float)j);
                    drawPos = Lerp.Vec2Smooth(prevPosLock, _lastPositions[idx], 0.25f * (float)j);
                    if (owner != null)
                    {
                        drawPos += owner.velocity * 0.5f;
                    }
                    _bladeTrail.Angle = rlAngle;
                    _bladeTrail.Alpha = Math.Min(Math.Max((_humAmount - 0.1f) * 4f, 0f), 1f) * 0.7f;
                    Graphics.Draw(_bladeTrail, drawPos.X, drawPos.Y, base.Depth - 2);
                }
                alph -= 0.15f;
            }
        }
        addHistory(Angle, Position);
        if (_lastSize > 2)
        {
            int cur = historyIndex(0);
            int prev = historyIndex(2);
            float newAngle = (_lastAngles[cur] + _lastAngles[prev]) / 2f;
            Vec2 newPosition = (_lastPositions[cur] + _lastPositions[prev]) / 2f;
            addHistory(newAngle, newPosition);
        }
        if (_lastSize > 8)
        {
            _lastSize = 8;
        }
    }
}
