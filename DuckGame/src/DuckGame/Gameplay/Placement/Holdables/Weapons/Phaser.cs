namespace DuckGame;

[EditorGroup("Guns|Lasers")]
public class Phaser : Gun
{
    private float _charge;

    private int _chargeLevel;

    private float _chargeFade;

    private SinWave _chargeWaver = 0.4f;

    private SpriteMap _phaserCharge;

    public Phaser(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 30;
        _ammoType = new ATPhaser();
        _type = "gun";
        graphic = new Sprite("phaser");
        center = new Vec2(7f, 4f);
        collisionOffset = new Vec2(-7f, -4f);
        collisionSize = new Vec2(15f, 9f);
        _barrelOffsetTL = new Vec2(14f, 3f);
        _fireSound = "laserRifle";
        _fullAuto = false;
        _fireWait = 0f;
        _kickForce = 0.5f;
        _holdOffset = new Vec2(0f, 0f);
        _flare = new SpriteMap("laserFlare", 16, 16);
        _flare.center = new Vec2(0f, 8f);
        _phaserCharge = new SpriteMap("phaserCharge", 8, 8);
        _phaserCharge.frame = 1;
        editorTooltip = "Like a laser, only...phasery? Hold the trigger to charge a more powerful shot.";
    }

    public override void Update()
    {
        if (owner == null || ammo <= 0)
        {
            _charge = 0f;
            _chargeLevel = 0;
        }
        _chargeFade = Lerp.Float(_chargeFade, (float)_chargeLevel / 3f, 0.06f);
        base.Update();
    }

    public override void OnPressAction()
    {
    }

    public override void Draw()
    {
        base.Draw();
        if (_chargeFade > 0.01f)
        {
            float a = base.alpha;
            base.alpha = (_chargeFade * 0.6f + _chargeFade * _chargeWaver.normalized * 0.4f) * 0.8f;
            Draw(_phaserCharge, new Vec2(3f + _chargeFade * (float)_chargeWaver * 0.5f, -4f), -1);
            base.alpha = a;
        }
    }

    public override void OnHoldAction()
    {
        if (ammo > 0)
        {
            _charge += 0.03f;
            if (_charge > 1f)
            {
                _charge = 1f;
            }
            if (_chargeLevel == 0)
            {
                _chargeLevel = 1;
            }
            else if (_charge > 0.4f && _chargeLevel == 1)
            {
                _chargeLevel = 2;
                SFX.Play("phaserCharge02", 0.5f);
            }
            else if (_charge > 0.8f && _chargeLevel == 2)
            {
                _chargeLevel = 3;
                SFX.Play("phaserCharge03", 0.6f);
            }
        }
    }

    public override void OnReleaseAction()
    {
        if (ammo <= 0)
        {
            return;
        }
        if (owner != null)
        {
            _ammoType.range = (float)_chargeLevel * 80f;
            _ammoType.bulletThickness = 0.2f + _charge * 0.4f;
            _ammoType.penetration = _chargeLevel;
            _ammoType.accuracy = 0.4f + _charge * 0.5f;
            _ammoType.bulletSpeed = 8f + _charge * 10f;
            if (_chargeLevel == 1)
            {
                if (base.duck != null)
                {
                    RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(RumbleIntensity.Kick, RumbleDuration.Pulse, RumbleFalloff.None));
                }
                _fireSound = "phaserSmall";
            }
            else if (_chargeLevel == 2)
            {
                if (base.duck != null)
                {
                    RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.None));
                }
                _fireSound = "phaserMedium";
            }
            else if (_chargeLevel == 3)
            {
                if (base.duck != null)
                {
                    RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.Short));
                }
                _fireSound = "phaserLarge";
            }
            Fire();
            _charge = 0f;
            _chargeLevel = 0;
        }
        base.OnReleaseAction();
    }
}
