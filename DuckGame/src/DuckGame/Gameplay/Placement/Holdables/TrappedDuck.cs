using System;
using System.Collections.Generic;

namespace DuckGame;

public class TrappedDuck : Holdable, IPlatform, IAmADuck
{
    public StateBinding _duckOwnerBinding = new StateBinding(nameof(_duckOwner));

    public Duck _duckOwner;

    public float _trapTime = 1f;

    public float _shakeMult;

    private float _shakeInc;

    public byte funNum;

    public bool infinite;

    private bool extinguishing;

    private float jumpCountdown;

    private bool _prevVisible;

    private int framesInvisible;

    private Vec2 _stickLerp;

    private Vec2 _stickSlowLerp;

    public Duck captureDuck => _duckOwner;

    public override bool visible
    {
        get
        {
            return base.visible;
        }
        set
        {
            if (value && _trapTime < 0f)
            {
                _trapTime = 1f;
                owner = null;
            }
            base.visible = value;
        }
    }

    public TrappedDuck(float xpos, float ypos, Duck duckowner)
        : base(xpos, ypos)
    {
        center = new Vec2(16f, 16f);
        collisionOffset = new Vec2(-8f, -8f);
        collisionSize = new Vec2(16f, 16f);
        base.depth = -0.5f;
        thickness = 0.5f;
        weight = 5f;
        flammable = 1f;
        burnSpeed = 0f;
        _duckOwner = duckowner;
        tapeable = false;
        InitializeStuff();
    }

    public void InitializeStuff()
    {
        _trapTime = 1f;
    }

    protected override bool OnBurn(Vec2 firePosition, Thing litBy)
    {
        if (_duckOwner != null)
        {
            _duckOwner.Burn(firePosition, litBy);
        }
        return base.OnBurn(firePosition, litBy);
    }

    public override void Extinquish()
    {
        if (!extinguishing)
        {
            extinguishing = true;
            if (_duckOwner != null)
            {
                _duckOwner.Extinquish();
            }
            base.Extinquish();
            extinguishing = false;
        }
    }

    public override void Terminate()
    {
        base.Terminate();
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (_duckOwner == null)
        {
            return false;
        }
        if (!destroyed)
        {
            _duckOwner.hSpeed = hSpeed;
            bool wasKilled = type != null;
            if (!wasKilled && jumpCountdown > 0.01f)
            {
                _duckOwner.vSpeed = Duck.JumpSpeed;
            }
            else
            {
                _duckOwner.vSpeed = (wasKilled ? (vSpeed - 1f) : (-3f));
            }
            _duckOwner.x = base.x;
            _duckOwner.y = base.y - 10f;
            for (int i = 0; i < 4; i++)
            {
                SmallSmoke smallSmoke = SmallSmoke.New(base.x + Rando.Float(-4f, 4f), base.y + Rando.Float(-4f, 4f));
                smallSmoke.hSpeed += hSpeed * Rando.Float(0.3f, 0.5f);
                smallSmoke.vSpeed -= Rando.Float(0.1f, 0.2f);
                Level.Add(smallSmoke);
            }
            if (base.duck != null)
            {
                if (base.held)
                {
                    if (base.duck.holdObject == this)
                    {
                        base.duck.holdObject = null;
                    }
                }
                else if (base.duck.holstered == this)
                {
                    base.duck.holstered = null;
                }
            }
            if (Network.isActive)
            {
                if (!wasKilled)
                {
                    _duckOwner.Fondle(this);
                    authority += 30;
                }
                active = false;
                visible = false;
                owner = null;
            }
            else
            {
                Level.Remove(this);
            }
            if (_duckOwner.owner == this)
            {
                _duckOwner.owner = null;
            }
            if (wasKilled && !_duckOwner.killingNet)
            {
                _duckOwner.killingNet = true;
                _duckOwner.Destroy(type);
            }
            _duckOwner._trapped = null;
        }
        return true;
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        if (bullet.isLocal && (_duckOwner == null || !_duckOwner.HitArmor(bullet, hitPos)))
        {
            OnDestroy(new DTShot(bullet));
        }
        return base.Hit(bullet, hitPos);
    }

    public override void ExitHit(Bullet bullet, Vec2 exitPos)
    {
    }

    public override void InactiveUpdate()
    {
        if (base.isServerForObject)
        {
            base.y = -9999f;
            visible = false;
        }
    }

    public override void Update()
    {
        if (Network.isActive && _prevVisible && !visible)
        {
            for (int i = 0; i < 4; i++)
            {
                SmallSmoke smallSmoke = SmallSmoke.New(base.x + Rando.Float(-4f, 4f), base.y + Rando.Float(-4f, 4f));
                smallSmoke.hSpeed += hSpeed * Rando.Float(0.3f, 0.5f);
                smallSmoke.vSpeed -= Rando.Float(0.1f, 0.2f);
                Level.Add(smallSmoke);
            }
        }
        if (_duckOwner == null)
        {
            return;
        }
        _framesSinceTransfer++;
        base.Update();
        if (base.isOffBottomOfLevel)
        {
            OnDestroy(new DTFall());
        }
        jumpCountdown -= Maths.IncFrameTimer();
        _prevVisible = visible;
        _shakeInc += 0.8f;
        _shakeMult = Lerp.Float(_shakeMult, 0f, 0.05f);
        if (Network.isActive && _duckOwner._trapped == this && !_duckOwner.isServerForObject && _duckOwner.inputProfile.Pressed("JUMP"))
        {
            _shakeMult = 1f;
        }
        if (_duckOwner.isServerForObject && _duckOwner._trapped == this)
        {
            if (!visible && owner == null)
            {
                framesInvisible++;
                if (framesInvisible > 30)
                {
                    framesInvisible = 0;
                    base.y = -9999f;
                }
            }
            if (!infinite)
            {
                _duckOwner.profile.stats.timeInNet += Maths.IncFrameTimer();
                if (_duckOwner.inputProfile.Pressed("JUMP"))
                {
                    _shakeMult = 1f;
                    _trapTime -= 0.007f;
                    jumpCountdown = 0.25f;
                }
                if (base.grounded && _duckOwner.inputProfile.Pressed("JUMP"))
                {
                    _shakeMult = 1f;
                    _trapTime -= 0.028f;
                    if (owner == null)
                    {
                        if (Math.Abs(hSpeed) < 1f && _framesSinceTransfer > 30)
                        {
                            _duckOwner.Fondle(this);
                        }
                        vSpeed -= Rando.Float(0.8f, 1.1f);
                        if (_duckOwner.inputProfile.Down("LEFT") && hSpeed > -1f)
                        {
                            hSpeed -= Rando.Float(0.6f, 0.8f);
                        }
                        if (_duckOwner.inputProfile.Down("RIGHT") && hSpeed < 1f)
                        {
                            hSpeed += Rando.Float(0.6f, 0.8f);
                        }
                    }
                }
                if (_duckOwner.inputProfile.Pressed("JUMP") && _duckOwner.HasEquipment(typeof(Jetpack)))
                {
                    _duckOwner.GetEquipment(typeof(Jetpack)).PressAction();
                }
                if (_duckOwner.inputProfile.Released("JUMP") && _duckOwner.HasEquipment(typeof(Jetpack)))
                {
                    _duckOwner.GetEquipment(typeof(Jetpack)).ReleaseAction();
                }
                _trapTime -= 0.0028f;
                if ((_trapTime <= 0f || _duckOwner.dead) && !inPipe)
                {
                    OnDestroy();
                }
            }
            _duckOwner.UpdateSkeleton();
            weight = 5f;
        }
        if (_duckOwner._trapped == this)
        {
            _duckOwner.position = position;
        }
        if (owner == null)
        {
            base.depth = _duckOwner.depth - 10;
        }
    }

    public override void Draw()
    {
        if (_duckOwner == null)
        {
            return;
        }
        _duckOwner._sprite.SetAnimation("netted");
        _duckOwner._sprite.imageIndex = 14;
        _duckOwner._spriteQuack.frame = _duckOwner._sprite.frame;
        _duckOwner._sprite.depth = base.depth;
        _duckOwner._spriteQuack.depth = base.depth;
        if (Network.isActive)
        {
            _duckOwner.DrawConnectionIndicators();
        }
        float shakeOffset = 0f;
        if (owner != null)
        {
            shakeOffset = (float)Math.Sin(_shakeInc) * _shakeMult * 1f;
        }
        if (_duckOwner.quack > 0)
        {
            Vec2 rs = _duckOwner.tounge;
            if (!_duckOwner._spriteQuack.flipH && rs.x < 0f)
            {
                rs.x = 0f;
            }
            if (_duckOwner._spriteQuack.flipH && rs.x > 0f)
            {
                rs.x = 0f;
            }
            if (rs.y < -0.3f)
            {
                rs.y = -0.3f;
            }
            if (rs.y > 0.4f)
            {
                rs.y = 0.4f;
            }
            _stickLerp = Lerp.Vec2Smooth(_stickLerp, rs, 0.2f);
            _stickSlowLerp = Lerp.Vec2Smooth(_stickSlowLerp, rs, 0.1f);
            Vec2 stick = _stickLerp;
            stick.y *= -1f;
            Vec2 stick2 = _stickSlowLerp;
            stick2.y *= -1f;
            int additionalFrame = 0;
            float length = stick.length;
            if (length > 0.5f)
            {
                additionalFrame = 72;
            }
            Graphics.Draw(_duckOwner._spriteQuack, _duckOwner._sprite.imageIndex + additionalFrame, base.x + shakeOffset, base.y - 8f);
            if (length > 0.05f)
            {
                Vec2 mouthPos = position + new Vec2(shakeOffset + (float)((!_duckOwner._spriteQuack.flipH) ? 1 : (-1)), -2f);
                List<Vec2> list = Curve.Bezier(8, mouthPos, mouthPos + stick2 * 6f, mouthPos + stick * 6f);
                Vec2 prev = Vec2.Zero;
                float lenMul = 1f;
                foreach (Vec2 p in list)
                {
                    if (prev != Vec2.Zero)
                    {
                        Vec2 dir = prev - p;
                        Graphics.DrawTexturedLine(Graphics.tounge.texture, prev + dir.normalized * 0.4f, p, new Color(223, 30, 30), 0.15f * lenMul, base.depth + 1);
                        Graphics.DrawTexturedLine(Graphics.tounge.texture, prev + dir.normalized * 0.4f, p - dir.normalized * 0.4f, Color.Black, 0.3f * lenMul, base.depth - 1);
                    }
                    lenMul -= 0.1f;
                    prev = p;
                }
                if (_duckOwner._spriteQuack != null)
                {
                    _duckOwner._spriteQuack.alpha = base.alpha;
                    _duckOwner._spriteQuack.angle = angle;
                    _duckOwner._spriteQuack.depth = base.depth + 2;
                    _duckOwner._spriteQuack.scale = base.scale;
                    _duckOwner._spriteQuack.frame += 36;
                    _duckOwner._spriteQuack.Draw();
                    _duckOwner._spriteQuack.frame -= 36;
                }
            }
        }
        else
        {
            Graphics.Draw(_duckOwner._sprite, base.x + shakeOffset, base.y - 8f);
        }
        base.Draw();
    }
}
