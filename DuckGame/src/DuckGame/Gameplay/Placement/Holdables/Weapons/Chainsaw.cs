using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Guns|Melee")]
[BaggedProperty("isSuperWeapon", true)]
public class Chainsaw : Gun
{
    public class ChainsawFlagBinding : StateFlagBase
    {
        public override ushort ushortValue
        {
            get
            {
                _value = 0;
                Chainsaw obj = _thing as Chainsaw;
                if (obj._flooded)
                {
                    _value |= 4;
                }
                if (obj._started)
                {
                    _value |= 2;
                }
                if (obj._throttle)
                {
                    _value |= 1;
                }
                return _value;
            }
            set
            {
                _value = value;
                Chainsaw obj = _thing as Chainsaw;
                obj._flooded = (_value & 4) != 0;
                obj._started = (_value & 2) != 0;
                obj._throttle = (_value & 1) != 0;
            }
        }

        public ChainsawFlagBinding(GhostPriority p = GhostPriority.Normal)
            : base(p, 3)
        {
        }
    }

    public StateBinding _angleOffsetBinding = new StateBinding(nameof(_hold));

    public StateBinding _throwSpinBinding = new StateBinding(nameof(_throwSpin));

    public StateBinding _gasBinding = new StateBinding(nameof(_gas));

    public StateBinding _floodBinding = new StateBinding(nameof(_flood));

    public StateBinding _chainsawStateBinding = new ChainsawFlagBinding();

    public EditorProperty<bool> souped = new EditorProperty<bool>(val: false);

    private float _hold;

    private bool _shing;

    private static bool _playedShing;

    public float _throwSpin;

    private int _framesExisting;

    private int _hitWait;

    private SpriteMap _swordSwing;

    private SpriteMap _sprite;

    private float _rotSway;

    public bool _started;

    private int _pullState = -1;

    private float _animRot;

    private float _upWait;

    private float _engineSpin;

    private float _bladeSpin;

    private float _engineResistance = 1f;

    private SinWave _idleWave = 0.6f;

    private SinWave _spinWave = 1f;

    private bool _puffClick;

    private float _warmUp;

    public bool _flooded;

    private int _gasDripFrames;

    public float _flood;

    private bool _releasePull;

    public float _gas = 1f;

    private bool _struggling;

    private bool _throttle;

    private float _throttleWait;

    private bool _releasedSincePull;

    private int _skipDebris;

    private bool _resetDuck;

    private int _skipSpark;

    private LoopingSound _sound = new LoopingSound("chainsawIdle", 0f, 0f, "chainsawIdleMulti");

    private LoopingSound _bladeSound = new LoopingSound("chainsawBladeLoop", 0f, 0f, "chainsawBladeLoopMulti");

    private LoopingSound _bladeSoundLow = new LoopingSound("chainsawBladeLoopLow", 0f, 0f, "chainsawBladeLoopLowMulti");

    private bool _smokeFlipper;

    private float _fireTrailWait;

    private bool _skipSmoke;

    private Vec2 _idleOffset = Vec2.Zero;

    public override float Angle
    {
        get
        {
            return base.Angle + _hold * (float)offDir + _animRot * (float)offDir + _rotSway * (float)offDir;
        }
        set
        {
            AngleValue = value;
        }
    }

    public Vec2 barrelStartPos => Position + (Offset(base.barrelOffset) - Position).Normalized * 2f;

    public override Vec2 tapedOffset
    {
        get
        {
            if (!base.tapedIsGun1)
            {
                return Vec2.Zero;
            }
            return new Vec2(6f, 0f);
        }
    }

    public bool throttle => _throttle;

    public Chainsaw(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 4;
        _ammoType = new ATLaser();
        _ammoType.range = 170f;
        _ammoType.accuracy = 0.8f;
        _type = "gun";
        _sprite = new SpriteMap("chainsaw", 29, 13);
        graphic = _sprite;
        Center = new Vec2(8f, 7f);
        collisionOffset = new Vec2(-8f, -6f);
        collisionSize = new Vec2(20f, 11f);
        _barrelOffsetTL = new Vec2(27f, 8f);
        _fireSound = "smg";
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 3f;
        _fireRumble = RumbleIntensity.Kick;
        _holdOffset = new Vec2(-4f, 4f);
        weight = 5f;
        physicsMaterial = PhysicsMaterial.Metal;
        _swordSwing = new SpriteMap("swordSwipe", 32, 32);
        _swordSwing.AddAnimation("swing", 0.6f, false, 0, 1, 1, 2);
        _swordSwing.currentAnimation = "swing";
        _swordSwing.speed = 0f;
        _swordSwing.Center = new Vec2(9f, 25f);
        throwSpeedMultiplier = 0.5f;
        _bouncy = 0.5f;
        _impactThreshold = 0.3f;
        base.collideSounds.Add("landTV");
        holsterAngle = -10f;
        editorTooltip = "The perfect tool for cutting wood or carving decorative ice sculptures.";
    }

    public override void Initialize()
    {
        _sprite = new SpriteMap("chainsaw", 29, 13);
        if ((bool)souped)
        {
            _sprite = new SpriteMap("turbochainsaw", 29, 13);
        }
        graphic = _sprite;
        base.Initialize();
    }

    public override void Terminate()
    {
        _sound.Kill();
        _bladeSound.Kill();
        _bladeSoundLow.Kill();
    }

    public void Shing(Thing wall)
    {
        if (_shing)
        {
            return;
        }
        _struggling = true;
        _shing = true;
        if (!_playedShing)
        {
            _playedShing = true;
            SFX.Play("chainsawClash", Rando.Float(0.4f, 0.55f), Rando.Float(-0.2f, 0.2f), Rando.Float(-0.1f, 0.1f));
        }
        Vec2 vec = (Position - base.barrelPosition).Normalized;
        Vec2 start = base.barrelPosition;
        for (int i = 0; i < 6; i++)
        {
            Level.Add(Spark.New(start.X, start.Y, new Vec2(Rando.Float(-1f, 1f), Rando.Float(-1f, 1f))));
            start += vec * 4f;
        }
        _swordSwing.speed = 0f;
        if (Recorder.currentRecording != null)
        {
            Recorder.currentRecording.LogAction(7);
        }
        if (base.duck == null)
        {
            return;
        }
        Duck d = base.duck;
        RumbleManager.AddRumbleEvent(d.profile, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Pulse, RumbleFalloff.None));
        if (wall.bottom < d.top)
        {
            d.vSpeed += 2f;
            return;
        }
        if (d.sliding)
        {
            d.sliding = false;
        }
        if (wall.X > d.X)
        {
            d.hSpeed -= 5f;
        }
        else
        {
            d.hSpeed += 5f;
        }
        d.vSpeed -= 2f;
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        SFX.Play("ting");
        return base.Hit(bullet, hitPos);
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (owner == null && with is Block)
        {
            Shing(with);
            if (base.totalImpactPower > 3f)
            {
                _started = false;
            }
        }
    }

    public override void ReturnToWorld()
    {
        _throwSpin = 90f;
    }

    public void PullEngine()
    {
        float pitch = (souped ? 0.3f : 0f);
        if (!_flooded && _gas > 0f && (_warmUp > 0.5f || _engineResistance < 1f))
        {
            SFX.Play("chainsawFire");
            _started = true;
            _engineSpin = 1.5f;
            for (int i = 0; i < 2; i++)
            {
                Level.Add(SmallSmoke.New(base.X + (float)(offDir * 4), base.Y + 5f));
            }
            _flooded = false;
            _flood = 0f;
        }
        else
        {
            if (_flooded && _gas > 0f)
            {
                SFX.Play("chainsawFlooded", 0.9f, Rando.Float(-0.2f, 0.2f));
                _engineSpin = 1.6f;
            }
            else
            {
                if (_gas == 0f || Rando.Float(1f) > 0.3f)
                {
                    SFX.Play("chainsawPull", 1f, pitch);
                }
                else
                {
                    SFX.Play("chainsawFire", 1f, pitch);
                }
                _engineSpin = 0.8f;
            }
            if (Rando.Float(1f) > 0.8f)
            {
                _flooded = false;
                _flood = 0f;
            }
        }
        _engineResistance -= 0.5f;
        if (_gas > 0f)
        {
            int num = (_flooded ? 4 : 2);
            for (int j = 0; j < num; j++)
            {
                Level.Add(SmallSmoke.New(base.X + (float)(offDir * 4), base.Y + 5f));
            }
        }
    }

    public override void PreUpdateTapedPositioning(TapedGun pTaped)
    {
        base.UpdateTapedPositioning(pTaped);
        if (pTaped.gun1 == this)
        {
            if (pTaped.duck != null && pTaped.duck.crouch)
            {
                pTaped._holdOffset = new Vec2(0f, -3f);
                pTaped.handOffset = new Vec2(0f, -3f);
            }
            else
            {
                pTaped._holdOffset = Vec2.Zero;
                pTaped.handOffset = new Vec2(0f, 0f);
            }
        }
    }

    public override void UpdateTapedPositioning(TapedGun pTaped)
    {
        base.UpdateTapedPositioning(pTaped);
        if (pTaped.gun1 == this)
        {
            offDir = pTaped.offDir;
            if (offDir < 0)
            {
                base.AngleDegrees -= 200f;
            }
            else
            {
                base.AngleDegrees -= 160f;
            }
        }
    }

    public override void Update()
    {
        base.Update();
        float speedMultiplier = 1f;
        if ((bool)souped)
        {
            speedMultiplier = 1.3f;
        }
        if (_swordSwing.finished)
        {
            _swordSwing.speed = 0f;
        }
        if (_hitWait > 0)
        {
            _hitWait--;
        }
        _framesExisting++;
        if (_framesExisting > 100)
        {
            _framesExisting = 100;
        }
        float pitch = (souped ? 0.3f : 0f);
        _sound.lerpVolume = ((_started && !_throttle) ? 0.6f : 0f);
        _sound.pitch = pitch;
        if (base.isServerForObject && base.duck != null)
        {
            RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(_engineSpin / 4f / 12f + (_started ? 0.02f : 0f), 0.05f, 0f));
        }
        if (_started)
        {
            _warmUp += 0.001f;
            if (_warmUp > 1f)
            {
                _warmUp = 1f;
            }
            if (!_puffClick && (float)_idleWave > 0.9f)
            {
                _skipSmoke = !_skipSmoke;
                if (_throttle || !_skipSmoke)
                {
                    Level.Add(SmallSmoke.New(base.X + (float)(offDir * 4), base.Y + 5f, _smokeFlipper ? (-0.1f) : 0.8f, 0.7f));
                    _smokeFlipper = !_smokeFlipper;
                    _puffClick = true;
                }
            }
            else if (_puffClick && (float)_idleWave < 0f)
            {
                _puffClick = false;
            }
            if (_pullState < 0)
            {
                float extraShake = 1f + Maths.NormalizeSection(_engineSpin, 1f, 2f) * 2f;
                float wave = _idleWave;
                if (extraShake > 1f)
                {
                    wave = _spinWave;
                }
                handOffset = Lerp.Vec2Smooth(handOffset, new Vec2(0f, 2f + wave * extraShake), 0.23f);
                _holdOffset = Lerp.Vec2Smooth(_holdOffset, new Vec2(1f, 2f + wave * extraShake), 0.23f);
                float extraShake2 = Maths.NormalizeSection(_engineSpin, 1f, 2f) * 3f;
                _rotSway = _idleWave.normalized * extraShake2 * 0.03f;
            }
            else
            {
                _rotSway = 0f;
            }
            if (!infinite.value)
            {
                _gas -= 3E-05f;
                if (_throttle)
                {
                    _gas -= 0.0002f;
                }
                if (_gas < 0f)
                {
                    _gas = 0f;
                    _started = false;
                    _throttle = false;
                }
            }
            if (_triggerHeld)
            {
                if (_releasedSincePull)
                {
                    if (!_throttle)
                    {
                        _throttle = true;
                        SFX.Play("chainsawBladeRevUp", 0.5f, pitch);
                    }
                    _engineSpin = Lerp.FloatSmooth(_engineSpin, 4f, 0.1f);
                }
            }
            else
            {
                if (_throttle)
                {
                    _throttle = false;
                    if (_engineSpin > 1.7f)
                    {
                        SFX.Play("chainsawBladeRevDown", 0.5f, pitch);
                    }
                }
                _engineSpin = Lerp.FloatSmooth(_engineSpin, 0f, 0.1f);
                _releasedSincePull = true;
            }
        }
        else
        {
            _warmUp -= 0.001f;
            if (_warmUp < 0f)
            {
                _warmUp = 0f;
            }
            _releasedSincePull = false;
            _throttle = false;
        }
        _bladeSound.lerpSpeed = 0.1f;
        _throttleWait = Lerp.Float(_throttleWait, _throttle ? 1f : 0f, 0.07f);
        _bladeSound.lerpVolume = ((_throttleWait > 0.96f) ? 0.6f : 0f);
        if (_struggling)
        {
            _bladeSound.lerpVolume = 0f;
        }
        _bladeSoundLow.lerpVolume = ((_throttleWait > 0.96f && _struggling) ? 0.6f : 0f);
        _bladeSound.pitch = pitch;
        _bladeSoundLow.pitch = pitch;
        if (owner == null)
        {
            collisionOffset = new Vec2(-8f, -6f);
            collisionSize = new Vec2(13f, 11f);
        }
        else if (base.duck != null && (base.duck.sliding || base.duck.crouch))
        {
            collisionOffset = new Vec2(-8f, -6f);
            collisionSize = new Vec2(6f, 11f);
        }
        else
        {
            collisionOffset = new Vec2(-8f, -6f);
            collisionSize = new Vec2(10f, 11f);
        }
        if (owner != null)
        {
            _resetDuck = false;
            if (_pullState == -1)
            {
                if (!_started)
                {
                    handOffset = Lerp.Vec2Smooth(handOffset, new Vec2(0f, 2f), 0.25f);
                    _holdOffset = Lerp.Vec2Smooth(_holdOffset, new Vec2(1f, 2f), 0.23f);
                }
                _upWait = 0f;
            }
            else if (_pullState == 0)
            {
                _animRot = Lerp.FloatSmooth(_animRot, -0.4f, 0.15f);
                handOffset = Lerp.Vec2Smooth(handOffset, new Vec2(-2f, -2f), 0.25f);
                _holdOffset = Lerp.Vec2Smooth(_holdOffset, new Vec2(-4f, 4f), 0.23f);
                if (_animRot <= -0.35f)
                {
                    _animRot = -0.4f;
                    _pullState = 1;
                    PullEngine();
                }
                _upWait = 0f;
            }
            else if (_pullState == 1)
            {
                _releasePull = false;
                _holdOffset = Lerp.Vec2Smooth(_holdOffset, new Vec2(2f, 3f), 0.23f);
                handOffset = Lerp.Vec2Smooth(handOffset, new Vec2(-4f, -2f), 0.23f);
                _animRot = Lerp.FloatSmooth(_animRot, -0.5f, 0.07f);
                if (_animRot < -0.45f)
                {
                    _animRot = -0.5f;
                    _pullState = 2;
                }
                _upWait = 0f;
            }
            else if (_pullState == 2)
            {
                if (_releasePull || !_triggerHeld)
                {
                    _releasePull = true;
                    if (_started)
                    {
                        handOffset = Lerp.Vec2Smooth(handOffset, new Vec2(0f, 2f + _idleWave.normalized), 0.23f);
                        _holdOffset = Lerp.Vec2Smooth(_holdOffset, new Vec2(1f, 2f + _idleWave.normalized), 0.23f);
                        _animRot = Lerp.FloatSmooth(_animRot, 0f, 0.1f);
                        if (_animRot > -0.07f)
                        {
                            _animRot = 0f;
                            _pullState = -1;
                        }
                    }
                    else
                    {
                        _holdOffset = Lerp.Vec2Smooth(_holdOffset, new Vec2(-4f, 4f), 0.24f);
                        handOffset = Lerp.Vec2Smooth(handOffset, new Vec2(-2f, -2f), 0.24f);
                        _animRot = Lerp.FloatSmooth(_animRot, -0.4f, 0.12f);
                        if (_animRot > -0.44f)
                        {
                            _releasePull = false;
                            _animRot = -0.4f;
                            _pullState = 3;
                            _holdOffset = new Vec2(-4f, 4f);
                            handOffset = new Vec2(-2f, -2f);
                        }
                    }
                }
                _upWait = 0f;
            }
            else if (_pullState == 3)
            {
                _releasePull = false;
                _upWait += 0.1f;
                if (_upWait > 6f)
                {
                    _pullState = -1;
                }
            }
            _bladeSpin += _engineSpin;
            while (_bladeSpin >= 1f)
            {
                _bladeSpin -= 1f;
                int f = _sprite.frame + 1;
                if (f > 15)
                {
                    f = 0;
                }
                _sprite.frame = f;
            }
            _engineSpin = Lerp.FloatSmooth(_engineSpin, 0f, 0.1f);
            _engineResistance = Lerp.FloatSmooth(_engineResistance, 1f, 0.01f);
            _hold = -0.4f;
            Center = new Vec2(8f, 7f);
            _framesSinceThrown = 0;
        }
        else
        {
            _rotSway = 0f;
            _shing = false;
            _animRot = Lerp.FloatSmooth(_animRot, 0f, 0.18f);
            if (_framesSinceThrown == 1)
            {
                _throwSpin = base.AngleDegrees;
            }
            _hold = 0f;
            base.AngleDegrees = _throwSpin;
            Center = new Vec2(8f, 7f);
            bool spinning = false;
            bool againstWall = false;
            if ((Math.Abs(hSpeed) + Math.Abs(vSpeed) > 2f || !base.grounded) && gravMultiplier > 0f)
            {
                if (!base.grounded && Level.CheckRect<Block>(Position + new Vec2(-8f, -6f), Position + new Vec2(8f, -2f)) != null)
                {
                    againstWall = true;
                }
                if (!againstWall && !_grounded && Level.CheckPoint<IPlatform>(Position + new Vec2(0f, 8f)) == null)
                {
                    if (offDir > 0)
                    {
                        _throwSpin += (Math.Abs(hSpeed) + Math.Abs(vSpeed)) * 1f + 5f;
                    }
                    else
                    {
                        _throwSpin -= (Math.Abs(hSpeed) + Math.Abs(vSpeed)) * 1f + 5f;
                    }
                    spinning = true;
                }
            }
            if (!spinning || againstWall)
            {
                _throwSpin %= 360f;
                if (_throwSpin < 0f)
                {
                    _throwSpin += 360f;
                }
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
                }
                else if (_throwSpin > 90f && _throwSpin < 270f)
                {
                    _throwSpin = Lerp.Float(_throwSpin, 180f, 14f);
                }
                else
                {
                    if (_throwSpin > 180f)
                    {
                        _throwSpin -= 360f;
                    }
                    else if (_throwSpin < -180f)
                    {
                        _throwSpin += 360f;
                    }
                    _throwSpin = Lerp.Float(_throwSpin, 0f, 14f);
                }
            }
        }
        if (Math.Abs(base.AngleDegrees) > 90f && Math.Abs(base.AngleDegrees) < 270f && !infinite.value)
        {
            if (base.isServerForObject)
            {
                _flood += 0.005f;
                if (_flood > 1f)
                {
                    _flooded = true;
                    _started = false;
                }
            }
            _gasDripFrames++;
            if (_gas > 0f && _flooded && _gasDripFrames > 2)
            {
                FluidData dat = Fluid.Gas;
                dat.amount = 0.003f;
                _gas -= 0.005f;
                if (_gas < 0f)
                {
                    _gas = 0f;
                }
                Level.Add(new Fluid(base.X, base.Y, Vec2.Zero, dat));
                _gasDripFrames = 0;
            }
            if (_gas <= 0f && base.isServerForObject)
            {
                _started = false;
            }
        }
        else if (base.isServerForObject)
        {
            _flood -= 0.008f;
            if (_flood < 0f)
            {
                _flood = 0f;
            }
        }
        if (base.duck != null)
        {
            base.duck.frictionMult = 1f;
            if (_skipSpark > 0)
            {
                _skipSpark++;
                if (_skipSpark > 2)
                {
                    _skipSpark = 0;
                }
            }
            if (base.duck.sliding && _throttle && !base.tapedIsGun2 && _skipSpark == 0)
            {
                if (Level.CheckLine<Block>(barrelStartPos + new Vec2(0f, 8f), base.barrelPosition + new Vec2(0f, 8f)) != null)
                {
                    _skipSpark = 1;
                    Vec2 pos = Position + base.barrelVector * 5f;
                    for (int i = 0; i < 2; i++)
                    {
                        Level.Add(Spark.New(pos.X, pos.Y, new Vec2((float)offDir * Rando.Float(0f, 2f), Rando.Float(0.5f, 1.5f))));
                        pos += base.barrelVector * 2f;
                        _fireTrailWait -= 0.5f;
                        if ((bool)souped && _fireTrailWait <= 0f)
                        {
                            _fireTrailWait = 1f;
                            SmallFire smallFire = SmallFire.New(pos.X, pos.Y, (float)offDir * Rando.Float(0f, 2f), Rando.Float(0.5f, 1.5f));
                            smallFire.waitToHurt = Rando.Float(1f, 2f);
                            smallFire.whoWait = owner as Duck;
                            Level.Add(smallFire);
                        }
                    }
                    if (offDir > 0 && owner.hSpeed < (float)(offDir * 6) * speedMultiplier)
                    {
                        owner.hSpeed = (float)(offDir * 6) * speedMultiplier;
                    }
                    else if (offDir < 0 && owner.hSpeed > (float)(offDir * 6) * speedMultiplier)
                    {
                        owner.hSpeed = (float)(offDir * 6) * speedMultiplier;
                    }
                }
                else if (offDir > 0 && owner.hSpeed < (float)(offDir * 3) * speedMultiplier)
                {
                    owner.hSpeed = (float)(offDir * 3) * speedMultiplier;
                }
                else if (offDir < 0 && owner.hSpeed > (float)(offDir * 3) * speedMultiplier)
                {
                    owner.hSpeed = (float)(offDir * 3) * speedMultiplier;
                }
            }
            if (_pullState == -1)
            {
                if (!_throttle)
                {
                    _animRot = MathHelper.Lerp(_animRot, 0.3f, 0.2f);
                    handOffset = Lerp.Vec2Smooth(handOffset, new Vec2(-2f, 2f), 0.25f);
                    _holdOffset = Lerp.Vec2Smooth(_holdOffset, new Vec2(-3f, 4f), 0.23f);
                }
                else if (_shing)
                {
                    _animRot = MathHelper.Lerp(_animRot, -1.8f, 0.4f);
                    handOffset = Lerp.Vec2Smooth(handOffset, new Vec2(1f, 0f), 0.25f);
                    _holdOffset = Lerp.Vec2Smooth(_holdOffset, new Vec2(1f, 2f), 0.23f);
                    if (_animRot < -1.5f)
                    {
                        _shing = false;
                    }
                }
                else if (base.duck.crouch)
                {
                    if (tape != null)
                    {
                        if (tape.gun1 == this)
                        {
                            _animRot = MathHelper.Lerp(_animRot, 0.2f, 0.2f);
                        }
                        else
                        {
                            _animRot = MathHelper.Lerp(_animRot, 0.4f, 0.2f);
                        }
                    }
                    else
                    {
                        _animRot = MathHelper.Lerp(_animRot, 0.4f, 0.2f);
                    }
                    handOffset = Lerp.Vec2Smooth(handOffset, new Vec2(1f, 0f), 0.25f);
                    _holdOffset = Lerp.Vec2Smooth(_holdOffset, new Vec2(1f, 2f), 0.23f);
                }
                else if (base.duck.inputProfile.Down("UP"))
                {
                    if (tape != null)
                    {
                        if (tape.gun1 == this)
                        {
                            _animRot = MathHelper.Lerp(_animRot, -0.4f, 0.2f);
                        }
                        else
                        {
                            _animRot = MathHelper.Lerp(_animRot, -0.6f, 0.2f);
                        }
                    }
                    else
                    {
                        _animRot = MathHelper.Lerp(_animRot, -0.9f, 0.2f);
                    }
                    handOffset = Lerp.Vec2Smooth(handOffset, new Vec2(1f, 0f), 0.25f);
                    _holdOffset = Lerp.Vec2Smooth(_holdOffset, new Vec2(1f, 2f), 0.23f);
                }
                else
                {
                    _animRot = MathHelper.Lerp(_animRot, 0f, 0.2f);
                    handOffset = Lerp.Vec2Smooth(handOffset, new Vec2(1f, 0f), 0.25f);
                    _holdOffset = Lerp.Vec2Smooth(_holdOffset, new Vec2(1f, 2f), 0.23f);
                }
            }
        }
        else if (!_resetDuck && base.prevOwner != null)
        {
            if (base.prevOwner is PhysicsObject t)
            {
                t.frictionMult = 1f;
            }
            _resetDuck = true;
        }
        if (_skipDebris > 0)
        {
            _skipDebris++;
        }
        if (_skipDebris > 3)
        {
            _skipDebris = 0;
        }
        _struggling = false;
        if (owner != null && _started && _throttle && !_shing)
        {
            (Offset(base.barrelOffset) - Position).Normalize();
            Offset(base.barrelOffset);
            IEnumerable<IAmADuck> hit = Level.CheckLineAll<IAmADuck>(barrelStartPos, base.barrelPosition);
            Block wallHit = Level.CheckLine<Block>(barrelStartPos, base.barrelPosition);
            if (owner != null)
            {
                foreach (MaterialThing t2 in Level.CheckLineAll<MaterialThing>(barrelStartPos, base.barrelPosition))
                {
                    if (!t2.Hurt((t2 is Door) ? 1.8f : 0.5f))
                    {
                        continue;
                    }
                    if (base.duck != null && base.duck.sliding && t2 is Door && (t2 as Door)._jammed)
                    {
                        t2.Destroy(new DTImpale(this));
                        continue;
                    }
                    _struggling = true;
                    if (base.duck != null)
                    {
                        base.duck.frictionMult = 4f;
                    }
                    if (_skipDebris != 0)
                    {
                        continue;
                    }
                    _skipDebris = 1;
                    Vec2 point = Collision.LinePoint(barrelStartPos, base.barrelPosition, t2.rectangle);
                    if (point != Vec2.Zero)
                    {
                        point += base.barrelVector * Rando.Float(0f, 3f);
                        Vec2 dir = -base.barrelVector.Rotate(Rando.Float(-0.2f, 0.2f), Vec2.Zero);
                        if (t2.physicsMaterial == PhysicsMaterial.Wood)
                        {
                            WoodDebris woodDebris = WoodDebris.New(point.X, point.Y);
                            woodDebris.hSpeed = dir.X * 3f;
                            woodDebris.vSpeed = dir.Y * 3f;
                            Level.Add(woodDebris);
                        }
                        else if (t2.physicsMaterial == PhysicsMaterial.Metal)
                        {
                            Spark spark = Spark.New(point.X, point.Y, Vec2.Zero);
                            spark.hSpeed = dir.X * 3f;
                            spark.vSpeed = dir.Y * 3f;
                            Level.Add(spark);
                        }
                        else if (t2.physicsMaterial == PhysicsMaterial.Glass)
                        {
                            Level.Add(new GlassParticle(point.X, point.Y, Vec2.Zero)
                            {
                                hSpeed = dir.X * 3f,
                                vSpeed = dir.Y * 3f
                            });
                        }
                    }
                }
            }
            bool clashed = false;
            if (wallHit != null && !(wallHit is Door))
            {
                Shing(wallHit);
                if (wallHit is Window)
                {
                    wallHit.Destroy(new DTImpact(this));
                }
            }
            else
            {
                foreach (Sword s in Level.current.things[typeof(Sword)])
                {
                    if (s.owner != null && s.crouchStance && !s.jabStance && Collision.LineIntersect(barrelStartPos, base.barrelPosition, s.barrelStartPos, s.barrelPosition))
                    {
                        Shing(s);
                        s.Shing();
                        s.owner.hSpeed += (float)offDir * 3f;
                        s.owner.vSpeed -= 2f;
                        base.duck.hSpeed += (float)(-offDir) * 3f;
                        base.duck.vSpeed -= 2f;
                        if (s.duck != null)
                        {
                            s.duck.crippleTimer = 1f;
                        }
                        base.duck.crippleTimer = 1f;
                        clashed = true;
                    }
                }
                if (!clashed)
                {
                    Thing ignore = null;
                    if (base.duck != null)
                    {
                        ignore = base.duck.GetEquipment(typeof(Helmet));
                    }
                    QuadLaserBullet laserHit = Level.CheckLine<QuadLaserBullet>(Position, base.barrelPosition);
                    if (laserHit != null)
                    {
                        Shing(laserHit);
                        Vec2 travel = laserHit.travel;
                        float mag = travel.Length();
                        float mul = 1f;
                        if (offDir > 0 && travel.X < 0f)
                        {
                            mul = 1.5f;
                        }
                        else if (offDir < 0 && travel.X > 0f)
                        {
                            mul = 1.5f;
                        }
                        travel = ((offDir <= 0) ? new Vec2((0f - mag) * mul, 0f) : new Vec2(mag * mul, 0f));
                        laserHit.travel = travel;
                    }
                    else
                    {
                        Helmet helmetHit = Level.CheckLine<Helmet>(barrelStartPos, base.barrelPosition, ignore);
                        if (helmetHit != null && helmetHit.equippedDuck != null && helmetHit.owner != null)
                        {
                            Shing(helmetHit);
                            if (helmetHit.owner != null)
                            {
                                helmetHit.owner.hSpeed += (float)offDir * 3f;
                                helmetHit.owner.vSpeed -= 2f;
                                if (helmetHit.duck != null)
                                {
                                    helmetHit.duck.crippleTimer = 1f;
                                }
                            }
                            helmetHit.Hurt(0.53f);
                            clashed = true;
                        }
                        else
                        {
                            if (base.duck != null)
                            {
                                ignore = base.duck.GetEquipment(typeof(ChestPlate));
                            }
                            ChestPlate chestHit = Level.CheckLine<ChestPlate>(barrelStartPos, base.barrelPosition, ignore);
                            if (chestHit != null && chestHit.equippedDuck != null && chestHit.owner != null)
                            {
                                Shing(chestHit);
                                if (chestHit.owner != null)
                                {
                                    chestHit.owner.hSpeed += (float)offDir * 3f;
                                    chestHit.owner.vSpeed -= 2f;
                                    if (chestHit.duck != null)
                                    {
                                        chestHit.duck.crippleTimer = 1f;
                                    }
                                }
                                chestHit.Hurt(0.53f);
                                clashed = true;
                            }
                        }
                    }
                }
            }
            if (!clashed)
            {
                foreach (Chainsaw s2 in Level.current.things[typeof(Chainsaw)])
                {
                    if (s2 != this && s2.owner != null && base.duck != null && s2 != base.tapedCompatriot && Collision.LineIntersect(barrelStartPos, base.barrelPosition, s2.barrelStartPos, s2.barrelPosition))
                    {
                        Shing(s2);
                        s2.Shing(this);
                        s2.owner.hSpeed += (float)offDir * 2f;
                        s2.owner.vSpeed -= 1.5f;
                        base.duck.hSpeed += (float)(-offDir) * 2f;
                        base.duck.vSpeed -= 1.5f;
                        if (s2.duck != null)
                        {
                            s2.duck.crippleTimer = 1f;
                        }
                        base.duck.crippleTimer = 1f;
                        clashed = true;
                        if (Recorder.currentRecording != null)
                        {
                            Recorder.currentRecording.LogBonus();
                        }
                    }
                }
            }
            if (!clashed)
            {
                foreach (IAmADuck d in hit)
                {
                    if (d == base.duck)
                    {
                        continue;
                    }
                    if (d is Duck && base.duck != null)
                    {
                        RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.Short));
                    }
                    if (d is MaterialThing realThing)
                    {
                        realThing.velocity += new Vec2((float)offDir * 0.8f, -0.8f);
                        realThing.Destroy(new DTImpale(this));
                        if (base.duck != null)
                        {
                            base.duck._timeSinceChainKill = 0;
                        }
                    }
                }
            }
        }
        _sound.Update();
        _bladeSound.Update();
        _bladeSoundLow.Update();
    }

    public override void HolsterUpdate(Holster pHolster)
    {
        holsterOffset = Vec2.Zero;
        if (pHolster is PowerHolster)
        {
            if (base.duck != null && base.duck.sliding)
            {
                holsterAngle = 90f;
                holsterOffset = new Vec2(6f, 0f);
            }
            else
            {
                holsterAngle = -10f;
            }
        }
        else
        {
            holsterAngle = 90f;
        }
        _flood = 0f;
    }

    public override void Draw()
    {
        _playedShing = false;
        if (_swordSwing.speed > 0f)
        {
            if (base.duck != null)
            {
                _swordSwing.flipH = base.duck.offDir <= 0;
            }
            _swordSwing.Alpha = 0.4f;
            _swordSwing.Position = Position;
            _swordSwing.Depth = base.Depth + 1;
            _swordSwing.Draw();
        }
        if (base.duck != null && (_pullState == 1 || _pullState == 2))
        {
            Graphics.DrawLine(Offset(new Vec2(-2f, -2f)), base.duck.armPosition + new Vec2(handOffset.X * (float)offDir, handOffset.Y), Color.White, 1f, base.duck.Depth + 11 - 1);
        }
        if ((base.duck == null || tape != null) && _started)
        {
            _idleOffset = Lerp.Vec2Smooth(handOffset, new Vec2(0f, 2f + _idleWave.normalized), 0.23f);
        }
        else
        {
            _idleOffset = Vec2.Zero;
        }
        Position += _idleOffset;
        base.Draw();
        Position -= _idleOffset;
    }

    public override void OnPressAction()
    {
        if (!_started)
        {
            if (base.duck != null)
            {
                RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(_fireRumble, RumbleDuration.Pulse, RumbleFalloff.None));
            }
            if (_pullState == -1)
            {
                _pullState = 0;
            }
            else if (_pullState == 3)
            {
                _pullState = 1;
                PullEngine();
            }
        }
    }

    public override void Fire()
    {
    }
}
