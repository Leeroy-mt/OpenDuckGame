using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[EditorGroup("Guns|Explosives")]
public class GrenadeCannon : Gun
{
    public StateBinding _fireAngleState = new StateBinding(nameof(_fireAngle));

    public StateBinding _aimAngleState = new StateBinding(nameof(_aimAngle));

    public StateBinding _aimWaitState = new StateBinding(nameof(_aimWait));

    public StateBinding _aimingState = new StateBinding(nameof(_aiming));

    public StateBinding _cooldownState = new StateBinding(nameof(_cooldown));

    public bool _doLoad;

    public bool _doneLoad;

    public float _timer = 1.2f;

    public float _fireAngle;

    public float _aimAngle;

    public float _aimWait;

    public bool _aiming;

    public float _cooldown;

    private SpriteMap _sprite;

    public override float Angle
    {
        get
        {
            return base.Angle + _aimAngle;
        }
        set
        {
            AngleValue = value;
        }
    }

    public GrenadeCannon(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 4;
        _type = "gun";
        _sprite = new SpriteMap("grenadecannon", 26, 12);
        _sprite.AddAnimation("idle4", 0.4f, false, default(int));
        _sprite.AddAnimation("load4", 0.4f, false, 1, 2, 3, 4);
        _sprite.AddAnimation("idle3", 0.4f, false, 5);
        _sprite.AddAnimation("load3", 0.4f, false, 6, 7, 8, 9);
        _sprite.AddAnimation("idle2", 0.4f, false, 10);
        _sprite.AddAnimation("load2", 0.4f, false, 11, 12, 13, 14);
        _sprite.AddAnimation("idle1", 0.4f, false, 15);
        _sprite.AddAnimation("load1", 0.4f, false, 16, 17, 18, 19);
        _sprite.AddAnimation("idle0", 0.4f, false, 20);
        _sprite.SetAnimation("idle4");
        graphic = _sprite;
        Center = new Vector2(11f, 7f);
        collisionOffset = new Vector2(-6f, -4f);
        collisionSize = new Vector2(16f, 8f);
        _barrelOffsetTL = new Vector2(22f, 6f);
        _laserOffsetTL = new Vector2(22f, 6f);
        _fireSound = "pistol";
        _kickForce = 3f;
        _fireRumble = RumbleIntensity.Light;
        _holdOffset = new Vector2(2f, 0f);
        _ammoType = new ATGrenade();
        _fireSound = "deepMachineGun";
        _bulletColor = Color.White;
        editorTooltip = "An unstable weapon filled with explosions. Spits fire if the trigger is held too long.";
    }

    public override void Update()
    {
        base.Update();
        if (_doLoad && _sprite.finished)
        {
            Level.Add(new GrenadePin(base.X, base.Y)
            {
                hSpeed = (float)(-offDir) * (1.5f + Rando.Float(0.5f)),
                vSpeed = -2f
            });
            SFX.Play("pullPin");
            _doneLoad = true;
            _doLoad = false;
        }
        if (_doneLoad)
        {
            _timer -= 0.01f;
        }
        if (_timer <= 0f)
        {
            _timer = 1.2f;
            _doneLoad = false;
            _doLoad = false;
            if (base.isServerForObject)
            {
                Vector2 pos = Offset(base.barrelOffset);
                ammo--;
                Vector2 travelDir = Maths.AngleToVec(base.barrelAngle + Rando.Float(-0.1f, 0.1f));
                for (int i = 0; i < 12; i++)
                {
                    Level.Add(SmallFire.New(pos.X, pos.Y, travelDir.X * Rando.Float(3.5f, 5f) + Rando.Float(-2f, 2f), travelDir.Y * Rando.Float(3.5f, 5f) + Rando.Float(-2f, 2f)));
                }
                for (int j = 0; j < 6; j++)
                {
                    Level.Add(SmallSmoke.New(pos.X + Rando.Float(-2f, 2f), pos.Y + Rando.Float(-2f, 2f)));
                }
                _sprite.SetAnimation("idle" + Math.Min(ammo, 4));
                kick = 1f;
                _aiming = false;
                _cooldown = 1f;
                _fireAngle = 0f;
                if (owner != null)
                {
                    owner.hSpeed -= travelDir.X * 4f;
                    owner.vSpeed -= travelDir.Y * 4f;
                    if (owner is Duck { crouch: not false } duckOwner)
                    {
                        duckOwner.sliding = true;
                    }
                }
                else
                {
                    hSpeed -= travelDir.X * 4f;
                    vSpeed -= travelDir.Y * 4f;
                }
            }
        }
        if (_doneLoad && _aiming)
        {
            laserSight = true;
        }
        if (_aiming && _aimWait <= 0f && _fireAngle < 90f)
        {
            _fireAngle += 3f;
        }
        if (_aimWait > 0f)
        {
            _aimWait -= 0.9f;
        }
        if ((double)_cooldown > 0.0)
        {
            _cooldown -= 0.1f;
        }
        else
        {
            _cooldown = 0f;
        }
        if (owner != null)
        {
            _aimAngle = 0f - Maths.DegToRad(_fireAngle);
            if (offDir < 0)
            {
                _aimAngle = 0f - _aimAngle;
            }
        }
        else
        {
            _aimWait = 0f;
            _aiming = false;
            _aimAngle = 0f;
            _fireAngle = 0f;
        }
        if (_raised)
        {
            _aimAngle = 0f;
        }
    }

    public override void OnPressAction()
    {
        if (!_doneLoad && !_doLoad)
        {
            _sprite.SetAnimation("load" + Math.Min(ammo, 4));
            _doLoad = true;
        }
        if (_doneLoad && _cooldown == 0f)
        {
            if (ammo > 0)
            {
                _aiming = true;
                _aimWait = 1f;
            }
            else
            {
                SFX.Play("click");
            }
        }
    }

    public override void OnReleaseAction()
    {
        if (_doneLoad && _cooldown == 0f && _aiming && ammo > 0)
        {
            _aiming = false;
            ammo--;
            kick = 1f;
            if (!receivingPress && base.isServerForObject)
            {
                Vector2 pos = Offset(base.barrelOffset);
                float radians = base.barrelAngle + Rando.Float(-0.1f, 0.1f);
                CannonGrenade g = new CannonGrenade(pos.X, pos.Y)
                {
                    _pin = false,
                    _timer = _timer
                };
                Fondle(g);
                Vector2 travelDir = Maths.AngleToVec(radians);
                g.hSpeed = travelDir.X * 10f;
                g.vSpeed = travelDir.Y * 10f;
                Level.Add(g);
                _timer = 1.2f;
                _doneLoad = false;
                _doLoad = false;
                _sprite.SetAnimation("idle" + Math.Min(ammo, 4));
            }
            _cooldown = 1f;
            Angle = 0f;
            _fireAngle = 0f;
        }
    }

    public override void Fire()
    {
    }
}
