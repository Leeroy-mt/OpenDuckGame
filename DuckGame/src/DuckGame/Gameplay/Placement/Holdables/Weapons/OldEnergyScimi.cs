using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[BaggedProperty("canSpawn", false)]
public class OldEnergyScimi : Sword
{
    private class RagdollDrag
    {
        public RagdollPart part;

        public Vec2 offset;
    }

    public StateBinding _glowBinding = new StateBinding(nameof(_glow));

    private MaterialEnergyBlade _bladeMaterial;

    private Sprite _blade;

    private Sprite _bladeTrail;

    private List<EnergyBlocker> _walls = new List<EnergyBlocker>();

    private Platform _platform;

    private Sprite _whiteGlow;

    private Sprite _warpLine;

    public Color properColor = new Color(178, 220, 239);

    public Color swordColor;

    private ConstantSound _hum;

    private float _timeTillPulse;

    private List<RagdollDrag> _drag = new List<RagdollDrag>();

    private bool _airFly;

    private float _airFlyDir;

    private bool _canAirFly = true;

    private bool _airFlyVertical;

    private float _upFlyTime;

    private bool _stuck;

    private float _glow;

    private bool _longCharge;

    private float _angleWhoom;

    private bool _thrownDown;

    private bool _thrownUp;

    private float slowWait;

    private bool _slowV;

    private Duck _revertVMaxDuck;

    private float _vmaxReversion = 1f;

    private bool _playedChargeUp;

    private float _unchargeWait;

    private float _lastAngleHum;

    private float _timeSincePickedUp = 10f;

    private bool _didOwnerSwitchLogic;

    public List<WarpLine> warpLines = new List<WarpLine>();

    public override Vec2 barrelStartPos
    {
        get
        {
            if (_stuck)
            {
                return position - (Offset(base.barrelOffset) - position).normalized * -5f;
            }
            if (owner == null)
            {
                return position - (Offset(base.barrelOffset) - position).normalized * 6f;
            }
            if (_slamStance)
            {
                return position + (Offset(base.barrelOffset) - position).normalized * 12f;
            }
            return position + (Offset(base.barrelOffset) - position).normalized * 2f;
        }
    }

    public override DestroyType destroyType
    {
        get
        {
            if (_airFly)
            {
                return new DTImpale(this);
            }
            return new DTIncinerate(this);
        }
    }

    public float angleWhoom => _angleWhoom;

    public OldEnergyScimi(float pX, float pY)
        : base(pX, pY)
    {
        graphic = new Sprite("energyScimiHilt");
        center = new Vec2(6f, 26f);
        collisionOffset = new Vec2(-2f, -24f);
        collisionSize = new Vec2(4f, 28f);
        _blade = new Sprite("energyScimiBlade");
        _bladeTrail = new Sprite("energyScimiBladeTrail");
        _whiteGlow = new Sprite("whiteGlow");
        _whiteGlow.center = new Vec2(16f, 28f);
        _whiteGlow.xscale = 0.8f;
        _whiteGlow.yscale = 1.4f;
        thickness = 0.01f;
        _impactThreshold = 0.5f;
        centerHeld = new Vec2(6f, 29f);
        centerUnheld = new Vec2(6f, 16f);
        _bladeMaterial = new MaterialEnergyBlade(this);
        additionalHoldOffset = new Vec2(0f, -3f);
        _swingSound = null;
        _enforceJabSwing = false;
        _allowJabMotion = false;
        _clashWithWalls = false;
        swordColor = properColor;
        _warpLine = new Sprite("warpLine2");
    }

    public override void Initialize()
    {
        for (int i = 0; i < 6; i++)
        {
            EnergyBlocker b = new EnergyBlocker(this);
            b.collisionSize = new Vec2(6f, 6f);
            b.center = new Vec2(3f, 3f);
            b.collisionOffset = new Vec2(-3f, -3f);
            _walls.Add(b);
            Level.Add(b);
        }
        _platform = new Platform(0f, 0f, 20f, 8f);
        _platform.solid = false;
        _platform.enablePhysics = false;
        _platform.center = new Vec2(10f, 4f);
        _platform.collisionOffset = new Vec2(-10f, -2f);
        _platform.thickness = 0.01f;
        Level.Add(_platform);
        _hum = new ConstantSound("scimiHum");
        _hum.volume = 0f;
        _hum.lerpSpeed = 1f;
        base.Initialize();
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        return false;
    }

    public override void Terminate()
    {
        foreach (EnergyBlocker wall in _walls)
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
        }
    }

    public override void RestoreCollisionSize(bool pHeld = false)
    {
        if (pHeld)
        {
            collisionOffset = new Vec2(-4f, 0f);
            collisionSize = new Vec2(4f, 4f);
            if (_crouchStance && !_jabStance)
            {
                collisionOffset = new Vec2(-2f, -19f);
                collisionSize = new Vec2(4f, 16f);
                thickness = 3f;
            }
        }
        else
        {
            collisionOffset = new Vec2(-2f, -24f);
            collisionSize = new Vec2(4f, 28f);
            if (_wasLifted)
            {
                collisionOffset = new Vec2(-4f, -2f);
                collisionSize = new Vec2(8f, 4f);
            }
        }
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (!_wasLifted || owner != null)
        {
            return;
        }
        if (with is Block || (with is IPlatform && from == ImpactedFrom.Bottom && vSpeed > 0f))
        {
            Shing();
            if (_framesSinceThrown > 5)
            {
                _framesSinceThrown = 25;
            }
        }
        else
        {
            if (!_airFly || !(with is RagdollPart) || _drag.FirstOrDefault((RagdollDrag x) => x.part == with) != null)
            {
                return;
            }
            RagdollPart p = with as RagdollPart;
            if (p.doll != null)
            {
                if (p.doll.part1 != null)
                {
                    _drag.Add(new RagdollDrag
                    {
                        part = p.doll.part1,
                        offset = position - p.doll.part1.position
                    });
                }
                if (p.doll.part2 != null)
                {
                    _drag.Add(new RagdollDrag
                    {
                        part = p.doll.part2,
                        offset = position - p.doll.part2.position
                    });
                }
                if (p.doll.part3 != null)
                {
                    _drag.Add(new RagdollDrag
                    {
                        part = p.doll.part3,
                        offset = position - p.doll.part3.position
                    });
                }
            }
        }
    }

    protected override void PerformAirSpin()
    {
        if (!enablePhysics)
        {
            return;
        }
        if (_canAirFly && !_airFly && _framesSinceThrown < 15)
        {
            _upFlyTime = 0f;
            if (Math.Abs(hSpeed) > 2f)
            {
                if (Level.CheckLine<Block>(position + new Vec2(-16f, 0f), position + new Vec2(16f, 0f)) == null)
                {
                    _airFly = true;
                    _airFlyDir = Math.Sign(hSpeed);
                }
                else
                {
                    _canAirFly = false;
                }
                _airFlyVertical = false;
            }
            else if (Math.Abs(vSpeed) > 2f)
            {
                if (Level.CheckLine<Block>(position + new Vec2(0f, -16f), position + new Vec2(0f, 16f)) == null)
                {
                    _airFly = true;
                    _airFlyDir = Math.Sign(vSpeed);
                }
                else
                {
                    _canAirFly = false;
                }
                _airFlyVertical = true;
            }
        }
        if (_airFly)
        {
            hMax = 18f;
            if (_airFlyVertical)
            {
                _upFlyTime += Maths.IncFrameTimer();
                if (_upFlyTime > 2f && _airFlyDir < 0f)
                {
                    _airFlyDir = 1f;
                }
                if (_airFlyDir > 0f)
                {
                    _throwSpin = 90f;
                }
                else if (_airFlyDir < 0f)
                {
                    _throwSpin = 270f;
                }
                vSpeed = _airFlyDir * 18f;
                hSpeed = 0f;
            }
            else
            {
                if (_airFlyDir > 0f)
                {
                    _throwSpin = 0f;
                }
                else if (_airFlyDir < 0f)
                {
                    _throwSpin = 180f;
                }
                vSpeed = 0f;
                hSpeed = _airFlyDir * 18f;
            }
            base.angleDegrees = 90f + _throwSpin;
            gravMultiplier = 0f;
        }
        else
        {
            hMax = 12f;
            vMax = 8f;
            base.PerformAirSpin();
        }
    }

    public override void Shing()
    {
        gravMultiplier = 1f;
        ClearDrag();
        Pulse();
        if (_airFly)
        {
            bool hit = false;
            if (_airFlyVertical)
            {
                Block b = Level.CheckLine<Block>(position, position + new Vec2(0f, vSpeed));
                if (b != null)
                {
                    hit = true;
                    base.clip.Add(b);
                    if (vSpeed > 0f)
                    {
                        base.y = b.top - 18f;
                        _throwSpin = 90f;
                    }
                    else
                    {
                        base.y = b.bottom + 18f;
                        _throwSpin = 270f;
                    }
                }
            }
            else
            {
                Block b2 = Level.CheckLine<Block>(position, position + new Vec2(hSpeed, 0f));
                if (b2 != null)
                {
                    hit = true;
                    base.clip.Add(b2);
                    if (hSpeed > 0f)
                    {
                        base.x = b2.left - 18f;
                        _throwSpin = 0f;
                    }
                    else
                    {
                        base.x = b2.right + 18f;
                        _throwSpin = 180f;
                    }
                }
            }
            if (hit)
            {
                _longCharge = true;
                _stuck = true;
                enablePhysics = false;
                hSpeed = 0f;
                vSpeed = 0f;
            }
        }
        _airFly = false;
        base.Shing();
    }

    protected override void QuadLaserHit(QuadLaserBullet pBullet)
    {
        if (base.isServerForObject)
        {
            Fondle(pBullet);
            EnergyScimitarBlast b = new EnergyScimitarBlast(pBullet.position, new Vec2(offDir * 2000, 0f));
            Level.Add(b);
            Level.Remove(pBullet);
            if (Network.isActive)
            {
                Send.Message(new NMEnergyScimitarBlast(b.position, b._target));
            }
        }
    }

    protected override void UpdateCrouchStance()
    {
        if (!_crouchStance)
        {
            _hold = -0.3f;
            handOffset = new Vec2(_addOffsetX + 4f, _addOffsetY);
            _holdOffset = new Vec2(2f + _addOffsetX, 6f + _addOffsetY) + additionalHoldOffset;
        }
        else if (base.duck != null && base.duck.sliding)
        {
            _hold = 2.24159f;
            if (handFlip)
            {
                _holdOffset = new Vec2(-4f + _addOffsetX, -3f + _addOffsetY) + additionalHoldOffset;
            }
            else
            {
                _holdOffset = new Vec2(2f + _addOffsetX, -3f + _addOffsetY) + additionalHoldOffset;
            }
            handOffset = new Vec2(3f + _addOffsetX, -6f + _addOffsetY);
        }
        else
        {
            _hold = 2.5415902f;
            if (handFlip)
            {
                _holdOffset = new Vec2(-4f + _addOffsetX, -7f + _addOffsetY) + additionalHoldOffset;
            }
            else
            {
                _holdOffset = new Vec2(2f + _addOffsetX, -7f + _addOffsetY) + additionalHoldOffset;
            }
            handOffset = new Vec2(3f + _addOffsetX, -10f + _addOffsetY);
        }
    }

    protected override void UpdateJabPullback()
    {
        handFlip = true;
        if (base.duck != null && base.duck.sliding)
        {
            _swing = MathHelper.Lerp(_swing, -4.2f, 0.36f);
        }
        else
        {
            _swing = MathHelper.Lerp(_swing, -4.8f, 0.36f);
        }
        _addOffsetX = MathHelper.Lerp(_addOffsetX, -2f, 0.45f);
        if (_addOffsetX < -12f)
        {
            _addOffsetX = -12f;
        }
    }

    protected override void UpdateSlamPullback()
    {
        _swing = MathHelper.Lerp(_swing, 0.8f, 0.8f);
        _addOffsetX = MathHelper.Lerp(_addOffsetX, -5f, 0.45f);
        if (_addOffsetX < -4.6f)
        {
            _addOffsetX = -5f;
        }
        _addOffsetY = MathHelper.Lerp(_addOffsetY, 6f, 0.35f);
        if (_addOffsetX < -5.5f)
        {
            _addOffsetY = -6f;
        }
    }

    public void ClearDrag()
    {
        int num = 1;
        foreach (RagdollDrag d in _drag)
        {
            if (d.part.doll != null && d.part.doll.captureDuck != null && d.part.doll.captureDuck._cooked == null)
            {
                if (!_airFlyVertical)
                {
                    d.part.position = Offset(new Vec2(-10f, 10f));
                }
                else if (_airFlyDir < 0f)
                {
                    d.part.position = Offset(new Vec2(0f, 0f));
                }
                else
                {
                    d.part.position = Offset(new Vec2(0f, 20f));
                }
                d.part.doll.position = d.part.position;
                d.part.doll.captureDuck.position = d.part.position;
                d.part.doll.captureDuck.OnKill(new DTIncinerate(d.part.doll.captureDuck));
                if (d.part.doll.captureDuck._cooked != null)
                {
                    d.part.doll.captureDuck._cooked.vSpeed = -(2 + num);
                }
                num++;
            }
        }
        _drag.Clear();
    }

    public override void Thrown()
    {
        if (base.isServerForObject && base.duck != null)
        {
            if (base.duck.inputProfile.Down("DOWN"))
            {
                base.x = base.duck.x;
                _thrownDown = true;
                if (!base.duck.grounded)
                {
                    base.duck.vSpeed -= 8f;
                }
            }
            else if (base.duck.inputProfile.Down("UP"))
            {
                base.x = base.duck.x;
                _thrownUp = true;
            }
        }
        base.Thrown();
    }

    protected override void OnSwing()
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
                    start = base.duck.position,
                    end = base.duck.position + new Vec2(0f, -80f),
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
                    start = base.duck.position + new Vec2(-offDir * 16, 4f),
                    end = base.duck.position + new Vec2(offDir * 62, 4f),
                    lerp = 0f,
                    wide = 20f
                });
            }
            slowWait = 0.085f;
        }
    }

    protected override void ResetTrailHistory()
    {
        _lastAngleHum = angle;
        base.ResetTrailHistory();
    }

    public override void Update()
    {
        float max = Math.Min(_angleWhoom, 0.5f) * 38f;
        if (base.isServerForObject)
        {
            _stickWait -= Maths.IncFrameTimer();
            if (_thrownDown)
            {
                vSpeed = 4f;
                base.y += 10f;
                _thrownDown = false;
            }
            if (_thrownUp)
            {
                vSpeed = -4f;
                base.y -= 6f;
                _thrownUp = false;
            }
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
                d.part.position = position - d.offset;
                d.part.hSpeed = 0f;
                d.part.vSpeed = 0f;
            }
            _timeSincePickedUp += Maths.IncFrameTimer();
            if (base.grounded)
            {
                _canAirFly = true;
            }
            _timeTillPulse -= Maths.IncFrameTimer();
            if (owner != null)
            {
                _canAirFly = true;
                ClearDrag();
                if (base.prevOwner == null && !_didOwnerSwitchLogic)
                {
                    _didOwnerSwitchLogic = true;
                    _timeSincePickedUp = 0f;
                    foreach (PhysicsObject item in Level.CheckCircleAll<PhysicsObject>(position, 16f))
                    {
                        item.sleeping = false;
                    }
                }
                float len = 20f + max;
                Vec2 pos = position + OffsetLocal(new Vec2(0f, -2f));
                foreach (EnergyBlocker wall in _walls)
                {
                    pos += OffsetLocal(new Vec2(0f, (0f - len) / (float)_walls.Count));
                    wall.position = pos;
                    wall.solid = _glow > 0.5f;
                }
            }
            else
            {
                _didOwnerSwitchLogic = false;
                Vec2 pos2 = position + OffsetLocal(new Vec2(0f, _stuck ? (-25) : (-14)));
                foreach (EnergyBlocker wall2 in _walls)
                {
                    pos2 += OffsetLocal(new Vec2(0f, 18f / (float)_walls.Count));
                    wall2.position = pos2;
                    wall2.solid = _glow > 0.5f;
                }
            }
            if (base.duck != null && _timeSincePickedUp > 0.4f && base.held && _swinging && Level.CheckLine<Block>(position, position + new Vec2(offDir * 16, 0f)) != null)
            {
                base.duck.Swear();
                _ = angle;
                base.duck.ThrowItem();
                _airFly = true;
                _airFlyDir = offDir;
                hSpeed = offDir * 16;
                Shing();
                base.angleDegrees = _throwSpin + 90f;
                ResetTrailHistory();
            }
        }
        float glowMul = Math.Min(_glow, 1f);
        float dif = Math.Min(Math.Abs(_lastAngleHum - angle), 1f);
        _angleWhoom = Lerp.FloatSmooth(_angleWhoom, dif, 0.2f);
        _hum.volume = Lerp.FloatSmooth(_hum.volume, Math.Min((Math.Min(Math.Abs(hSpeed) + Math.Abs(vSpeed), 5f) / 10f + dif * 2f + 0.15f + glowMul * 0.1f) * _glow, 0.75f), 0.2f);
        if (base.level != null)
        {
            float maxDist = 800f;
            float minDist = 400f;
            float dist = Math.Min(Math.Max((base.level.camera.position - position).length, minDist) - minDist, maxDist);
            float atten = 1f - dist / maxDist;
            _hum.volume *= atten;
            if (base.isServerForObject && (base.x < base.level.topLeft.x - 1000f || base.x > base.level.bottomRight.x + 1000f))
            {
                Level.Remove(this);
            }
        }
        _extraOffset = new Vec2(0f, 0f - max);
        _barrelOffsetTL = new Vec2(4f, 3f - max);
        _lastAngleHum = angle;
        if (_glow > 1f)
        {
            _glow *= 0.85f;
        }
        if (base.held || _airFly)
        {
            _stuck = false;
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
            if (base.duck != null && !_swinging && !_crouchStance)
            {
                glowLerp = 0f;
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
        if (_glow > 0.1f)
        {
            _stayVolatile = true;
            _volatile = true;
            heat += 0.006f;
        }
        else
        {
            _stayVolatile = false;
            _volatile = false;
            if (heat > 0f)
            {
                heat -= 0.01f;
            }
        }
        base.Update();
        _platform.solid = false;
        _platform.enablePhysics = false;
        _platform.position = new Vec2(-99999f, -99999f);
        if (_stuck)
        {
            if (Math.Abs(barrelStartPos.y - base.barrelPosition.y) < 6f)
            {
                _platform.solid = true;
                _platform.enablePhysics = true;
                _platform.position = Offset(new Vec2(0f, -10f));
            }
            center = new Vec2(6f, 29f);
        }
    }

    public override void DrawGlow()
    {
        _whiteGlow.angle = angle;
        _whiteGlow.color = swordColor;
        _whiteGlow.alpha = _glow * 0.5f;
        Graphics.Draw(_whiteGlow, base.x, base.y, base.depth - 2);
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

    public override void Draw()
    {
        base.Draw();
        Sword._playedShing = true;
        _ = DevConsole.showCollision;
        float max = Math.Min(_angleWhoom, 0.5f) * 1.5f;
        Graphics.material = _bladeMaterial;
        _bladeMaterial.glow = 0.25f + _glow * 0.75f;
        _blade.center = center;
        _bladeTrail.center = center;
        _blade.angle = graphic.angle;
        _blade.flipH = graphic.flipH;
        _bladeTrail.flipH = _blade.flipH;
        _blade.color = Color.Lerp(Color.White, Color.Red, heat);
        swordColor = Color.Lerp(properColor, Color.Red, heat);
        if (_glow > 1f)
        {
            _blade.scale = new Vec2(1f + (_glow - 1f) * 0.03f, 1f);
        }
        else
        {
            _blade.scale = new Vec2(1f);
        }
        _bladeTrail.yscale = _blade.yscale + max;
        Graphics.Draw(_blade, base.x, base.y, base.depth - 1);
        Graphics.material = null;
        base.alpha = 1f;
        _ = position;
        _ = base.depth;
        _bladeTrail.color = swordColor;
        graphic.color = Color.White;
        if (!(_glow > 0.5f))
        {
            return;
        }
        float rlAngle = angle;
        _ = _angle;
        float alph = 1f;
        Vec2 drawPos = position;
        _ = position;
        for (int i = 0; i < 8; i++)
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
                _bladeTrail.angle = rlAngle;
                _bladeTrail.alpha = Math.Min(Math.Max((_hum.volume - 0.1f) * 4f, 0f), 1f) * 0.7f;
                Graphics.Draw(_bladeTrail, drawPos.x, drawPos.y, base.depth - 2);
            }
            alph -= 0.15f;
        }
    }
}
