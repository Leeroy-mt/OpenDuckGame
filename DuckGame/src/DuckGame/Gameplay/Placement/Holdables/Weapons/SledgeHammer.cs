using System;

namespace DuckGame;

[EditorGroup("Guns|Melee")]
public class SledgeHammer : Gun
{
    public StateBinding _swingBinding = new StateBinding("_swing");

    private SpriteMap _sprite;

    private SpriteMap _sledgeSwing;

    private Vec2 _offset;

    private float _swing;

    private float _swingLast;

    private float _swingVelocity;

    private float _swingForce;

    private bool _pressed;

    private float _lastSpeed;

    private int _lastDir;

    private float _fullSwing;

    private float _sparkWait;

    private bool _swung;

    private bool _drawOnce;

    private bool _held;

    private PhysicsObject _lastOwner;

    private float _hPull;

    public SledgeHammer(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 4;
        _ammoType = new ATLaser();
        _ammoType.range = 170f;
        _ammoType.accuracy = 0.8f;
        _type = "gun";
        _sprite = new SpriteMap("sledgeHammer", 32, 32);
        _sledgeSwing = new SpriteMap("sledgeSwing", 32, 32);
        _sledgeSwing.AddAnimation("swing", 0.8f, false, 0, 1, 2, 3, 4, 5);
        _sledgeSwing.currentAnimation = "swing";
        _sledgeSwing.speed = 0f;
        _sledgeSwing.center = new Vec2(16f, 16f);
        graphic = _sprite;
        center = new Vec2(16f, 14f);
        collisionOffset = new Vec2(-2f, 0f);
        collisionSize = new Vec2(4f, 18f);
        _barrelOffsetTL = new Vec2(16f, 28f);
        _fireSound = "smg";
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 3f;
        weight = 9f;
        _dontCrush = false;
        base.collideSounds.Add("rockHitGround2");
        holsterAngle = 180f;
        holsterOffset = new Vec2(11f, 0f);
        editorTooltip = "For big nails.";
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (with is IPlatform)
        {
            for (int i = 0; i < 4; i++)
            {
                Level.Add(Spark.New(base.barrelPosition.x + Rando.Float(-6f, 6f), base.barrelPosition.y + Rando.Float(-3f, 3f), -MaterialThing.ImpactVector(from)));
            }
        }
    }

    public override void CheckIfHoldObstructed()
    {
        if (owner is Duck duckOwner)
        {
            duckOwner.holdObstructed = false;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void ReturnToWorld()
    {
        collisionOffset = new Vec2(-2f, 0f);
        collisionSize = new Vec2(4f, 18f);
        _sprite.frame = 0;
        _swing = 0f;
        _swingForce = 0f;
        _pressed = false;
        _swung = false;
        _fullSwing = 0f;
        _swingVelocity = 0f;
    }

    public override void Update()
    {
        if (_lastOwner != null && owner == null)
        {
            _lastOwner.frictionMod = 0f;
            _lastOwner = null;
            _swing = 0f;
            _swingVelocity = 0f;
        }
        if (base.duck != null)
        {
            if (base.duck.ragdoll != null)
            {
                holsterAngle = 0f;
                holsterOffset = new Vec2(0f, 0f);
                center = new Vec2(16f, 22f);
                collisionOffset = new Vec2(-6f, 6f);
                collisionSize = new Vec2(12f, 12f);
                return;
            }
            holsterAngle = 180f;
            center = new Vec2(16f, 14f);
            graphic.center = center;
            if (base.duck.sliding)
            {
                holsterOffset = new Vec2(4f, 8f);
            }
            else
            {
                holsterOffset = new Vec2(11f, 0f);
            }
        }
        collisionOffset = new Vec2(-2f, 0f);
        collisionSize = new Vec2(4f, 18f);
        if (_swing > 0f)
        {
            collisionOffset = new Vec2(-9999f, 0f);
            collisionSize = new Vec2(4f, 18f);
        }
        _swingVelocity = Maths.LerpTowards(_swingVelocity, _swingForce, 0.1f);
        Duck duckOwner = owner as Duck;
        if (base.isServerForObject)
        {
            _swing += _swingVelocity;
            float dif = _swing - _swingLast;
            _swingLast = _swing;
            if (_swing > 1f)
            {
                _swing = 1f;
            }
            if (_swing < 0f)
            {
                _swing = 0f;
            }
            _sprite.flipH = false;
            _sprite.flipV = false;
            if (duckOwner != null && base.held)
            {
                float spd = duckOwner.hSpeed;
                _hPull = Maths.LerpTowards(_hPull, duckOwner.hSpeed, 0.15f);
                if (Math.Abs(duckOwner.hSpeed) < 0.1f)
                {
                    _hPull = 0f;
                }
                float weightMod = Math.Abs(_hPull) / 2.5f;
                if (weightMod > 1f)
                {
                    weightMod = 1f;
                }
                weight = 8f - weightMod * 3f;
                if ((double)weight <= 5.0)
                {
                    weight = 5.1f;
                }
                float fricDif = Math.Abs(duckOwner.hSpeed - _hPull);
                duckOwner.frictionMod = 0f;
                if (duckOwner.hSpeed > 0f && _hPull > duckOwner.hSpeed)
                {
                    duckOwner.frictionMod = (0f - fricDif) * 1.8f;
                }
                if (duckOwner.hSpeed < 0f && _hPull < duckOwner.hSpeed)
                {
                    duckOwner.frictionMod = (0f - fricDif) * 1.8f;
                }
                _lastDir = duckOwner.offDir;
                _lastSpeed = spd;
                if (_swing != 0f && dif > 0f)
                {
                    duckOwner.hSpeed += (float)duckOwner.offDir * (dif * 3f) * base.weightMultiplier;
                    duckOwner.vSpeed -= dif * 2f * base.weightMultiplier;
                }
            }
        }
        if (_sparkWait > 0f)
        {
            _sparkWait -= 0.1f;
        }
        else
        {
            _sparkWait = 0f;
        }
        if (duckOwner != null && base.held && _sparkWait == 0f && _swing == 0f && duckOwner.Held(this, ignorePowerHolster: true))
        {
            if (duckOwner.grounded && duckOwner.offDir > 0 && duckOwner.hSpeed > 1f)
            {
                _sparkWait = 0.25f;
                Level.Add(Spark.New(base.x - 22f, base.y + 6f, new Vec2(0f, 0.5f)));
            }
            else if (duckOwner.grounded && duckOwner.offDir < 0 && duckOwner.hSpeed < -1f)
            {
                _sparkWait = 0.25f;
                Level.Add(Spark.New(base.x + 22f, base.y + 6f, new Vec2(0f, 0.5f)));
            }
        }
        if (_swing < 0.5f)
        {
            float norm = _swing * 2f;
            _sprite.imageIndex = (int)(norm * 10f);
            _sprite.angle = 1.2f - norm * 1.5f;
            _sprite.yscale = 1f - norm * 0.1f;
        }
        else if (_swing >= 0.5f)
        {
            float norm2 = (_swing - 0.5f) * 2f;
            _sprite.imageIndex = 10 - (int)(norm2 * 10f);
            _sprite.angle = -0.3f - norm2 * 1.5f;
            _sprite.yscale = 1f - (1f - norm2) * 0.1f;
            _fullSwing += 0.16f;
            if (!_swung)
            {
                _swung = true;
                if (base.duck != null && base.isServerForObject)
                {
                    Level.Add(new ForceWave(base.x + (float)offDir * 4f + owner.hSpeed, base.y + 8f, offDir, 0.15f, 4f + Math.Abs(owner.hSpeed), owner.vSpeed, base.duck));
                }
            }
        }
        if (_swing == 1f)
        {
            _pressed = false;
        }
        if (_swing == 1f && !_pressed && _fullSwing > 1f)
        {
            _swingForce = -0.08f;
            _fullSwing = 0f;
        }
        if (_sledgeSwing.finished)
        {
            _sledgeSwing.speed = 0f;
        }
        _lastOwner = owner as PhysicsObject;
        bool didAction = false;
        if (base.duck != null && base.held)
        {
            didAction = (base.duck.Held(this, ignorePowerHolster: true) ? base.duck.action : triggerAction);
            if (didAction && !_held && _swing == 0f)
            {
                RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(RumbleIntensity.Kick, RumbleDuration.Pulse, RumbleFalloff.Short));
                _fullSwing = 0f;
                duckOwner._disarmDisable = 30;
                duckOwner.crippleTimer = 1f;
                _sledgeSwing.speed = 1f;
                _sledgeSwing.frame = 0;
                _swingForce = 0.6f;
                _pressed = true;
                _swung = false;
                _held = true;
            }
            if (!base.duck.action)
            {
                _pressed = false;
                _held = false;
            }
        }
        handOffset = new Vec2(_swing * 3f, 0f - _swing * 4f);
        handAngle = 1.4f + (_sprite.angle * 0.5f - 1f);
        if (duckOwner != null && duckOwner.offDir < 0)
        {
            _sprite.angle = 0f - _sprite.angle;
            handAngle = 0f - handAngle;
        }
        base.Update();
    }

    public override void Draw()
    {
        if (base.duck != null && base.duck.ragdoll != null)
        {
            base.Draw();
        }
        else if (owner != null && _drawOnce)
        {
            _offset = new Vec2((float)offDir * -6f + _swing * 5f * (float)offDir, -3f + _swing * 5f);
            Vec2 pos = position + _offset;
            graphic.position = pos;
            graphic.depth = base.depth;
            graphic.Draw();
            Duck duckOwner = owner as Duck;
            if (_sledgeSwing.speed > 0f)
            {
                if (duckOwner != null)
                {
                    _sledgeSwing.flipH = duckOwner.offDir <= 0;
                }
                _sledgeSwing.position = position;
                _sledgeSwing.depth = base.depth + 1;
                _sledgeSwing.Draw();
            }
        }
        else
        {
            base.Draw();
            _drawOnce = true;
        }
    }

    public override void OnPressAction()
    {
    }

    public override void OnReleaseAction()
    {
    }

    public override void Fire()
    {
    }
}
