using Microsoft.Xna.Framework;
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

    private Vector2 _stickLerp;

    private Vector2 _stickSlowLerp;

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
        Center = new Vector2(16f, 16f);
        collisionOffset = new Vector2(-8f, -8f);
        collisionSize = new Vector2(16f, 16f);
        base.Depth = -0.5f;
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

    protected override bool OnBurn(Vector2 firePosition, Thing litBy)
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
            _duckOwner.X = base.X;
            _duckOwner.Y = base.Y - 10f;
            for (int i = 0; i < 4; i++)
            {
                SmallSmoke smallSmoke = SmallSmoke.New(base.X + Rando.Float(-4f, 4f), base.Y + Rando.Float(-4f, 4f));
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

    public override bool Hit(Bullet bullet, Vector2 hitPos)
    {
        if (bullet.isLocal && (_duckOwner == null || !_duckOwner.HitArmor(bullet, hitPos)))
        {
            OnDestroy(new DTShot(bullet));
        }
        return base.Hit(bullet, hitPos);
    }

    public override void ExitHit(Bullet bullet, Vector2 exitPos)
    {
    }

    public override void InactiveUpdate()
    {
        if (base.isServerForObject)
        {
            base.Y = -9999f;
            visible = false;
        }
    }

    public override void Update()
    {
        if (Network.isActive && _prevVisible && !visible)
        {
            for (int i = 0; i < 4; i++)
            {
                SmallSmoke smallSmoke = SmallSmoke.New(base.X + Rando.Float(-4f, 4f), base.Y + Rando.Float(-4f, 4f));
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
                    base.Y = -9999f;
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
            _duckOwner.Position = Position;
        }
        if (owner == null)
        {
            base.Depth = _duckOwner.Depth - 10;
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
        _duckOwner._sprite.Depth = base.Depth;
        _duckOwner._spriteQuack.Depth = base.Depth;
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
            Vector2 rs = _duckOwner.tounge;
            if (!_duckOwner._spriteQuack.flipH && rs.X < 0f)
            {
                rs.X = 0f;
            }
            if (_duckOwner._spriteQuack.flipH && rs.X > 0f)
            {
                rs.X = 0f;
            }
            if (rs.Y < -0.3f)
            {
                rs.Y = -0.3f;
            }
            if (rs.Y > 0.4f)
            {
                rs.Y = 0.4f;
            }
            _stickLerp = Lerp.Vec2Smooth(_stickLerp, rs, 0.2f);
            _stickSlowLerp = Lerp.Vec2Smooth(_stickSlowLerp, rs, 0.1f);
            Vector2 stick = _stickLerp;
            stick.Y *= -1f;
            Vector2 stick2 = _stickSlowLerp;
            stick2.Y *= -1f;
            int additionalFrame = 0;
            float length = stick.Length();
            if (length > 0.5f)
            {
                additionalFrame = 72;
            }
            Graphics.Draw(_duckOwner._spriteQuack, _duckOwner._sprite.imageIndex + additionalFrame, base.X + shakeOffset, base.Y - 8f);
            if (length > 0.05f)
            {
                Vector2 mouthPos = Position + new Vector2(shakeOffset + (float)((!_duckOwner._spriteQuack.flipH) ? 1 : (-1)), -2f);
                List<Vector2> list = Curve.Bezier(8, mouthPos, mouthPos + stick2 * 6f, mouthPos + stick * 6f);
                Vector2 prev = Vector2.Zero;
                float lenMul = 1f;
                foreach (Vector2 p in list)
                {
                    if (prev != Vector2.Zero)
                    {
                        Vector2 dir = prev - p;
                        Graphics.DrawTexturedLine(Graphics.tounge.texture, prev + Vector2.Normalize(dir) * 0.4f, p, new Color(223, 30, 30), 0.15f * lenMul, base.Depth + 1);
                        Graphics.DrawTexturedLine(Graphics.tounge.texture, prev + Vector2.Normalize(dir) * 0.4f, p - Vector2.Normalize(dir) * 0.4f, Color.Black, 0.3f * lenMul, base.Depth - 1);
                    }
                    lenMul -= 0.1f;
                    prev = p;
                }
                if (_duckOwner._spriteQuack != null)
                {
                    _duckOwner._spriteQuack.Alpha = base.Alpha;
                    _duckOwner._spriteQuack.Angle = Angle;
                    _duckOwner._spriteQuack.Depth = base.Depth + 2;
                    _duckOwner._spriteQuack.Scale = base.Scale;
                    _duckOwner._spriteQuack.frame += 36;
                    _duckOwner._spriteQuack.Draw();
                    _duckOwner._spriteQuack.frame -= 36;
                }
            }
        }
        else
        {
            Graphics.Draw(_duckOwner._sprite, base.X + shakeOffset, base.Y - 8f);
        }
        base.Draw();
    }
}
