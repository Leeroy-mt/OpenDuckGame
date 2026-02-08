using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Guns|Explosives")]
public class GrenadeLauncher : Gun
{
    public StateBinding _fireAngleState = new StateBinding(nameof(_fireAngle));

    public StateBinding _aimAngleState = new StateBinding(nameof(_aimAngle));

    public StateBinding _aimWaitState = new StateBinding(nameof(_aimWait));

    public StateBinding _aimingState = new StateBinding(nameof(_aiming));

    public StateBinding _cooldownState = new StateBinding(nameof(_cooldown));

    public float _fireAngle;

    public float _aimAngle;

    public float _aimWait;

    public bool _aiming;

    public float _cooldown;

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

    public GrenadeLauncher(float xval, float yval)
        : base(xval, yval)
    {
        wideBarrel = true;
        ammo = 6;
        _type = "gun";
        graphic = new Sprite("grenadeLauncher");
        Center = new Vector2(16f, 16f);
        collisionOffset = new Vector2(-6f, -4f);
        collisionSize = new Vector2(16f, 7f);
        _barrelOffsetTL = new Vector2(28f, 14f);
        _fireSound = "pistol";
        _kickForce = 3f;
        _fireRumble = RumbleIntensity.Light;
        _holdOffset = new Vector2(4f, 0f);
        _ammoType = new ATGrenade();
        _fireSound = "deepMachineGun";
        _bulletColor = Color.White;
        editorTooltip = "Delivers a fun & exciting present to a long distance friend. Hold fire to adjust arc.";
    }

    public override void Update()
    {
        base.Update();
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
        if (_cooldown == 0f)
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
        if (_cooldown == 0f && ammo > 0)
        {
            _aiming = false;
            Fire();
            _cooldown = 1f;
            Angle = 0f;
            _fireAngle = 0f;
        }
    }
}
