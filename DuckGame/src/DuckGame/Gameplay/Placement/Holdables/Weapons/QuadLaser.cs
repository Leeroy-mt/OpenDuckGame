namespace DuckGame;

[EditorGroup("Guns|Lasers")]
public class QuadLaser : Gun
{
    public QuadLaser(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 3;
        _ammoType = new AT9mm();
        _type = "gun";
        graphic = new Sprite("quadLaser");
        Center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-8f, -3f);
        collisionSize = new Vec2(16f, 8f);
        _barrelOffsetTL = new Vec2(20f, 8f);
        _fireSound = "pistolFire";
        _kickForce = 3f;
        _fireRumble = RumbleIntensity.Kick;
        loseAccuracy = 0.1f;
        maxAccuracyLost = 0.6f;
        _holdOffset = new Vec2(2f, -2f);
        _bio = "Stop moving...";
        _editorName = "Quad Laser";
        editorTooltip = "Shoots a slow-moving science block of doom that passes through walls.";
    }

    public override void OnPressAction()
    {
        if (ammo <= 0)
        {
            return;
        }
        Vec2 barrel = Offset(base.barrelOffset);
        if (base.isServerForObject)
        {
            QuadLaserBullet b = new QuadLaserBullet(barrel.X, barrel.Y, base.barrelVector);
            b.killThingType = GetType();
            Level.Add(b);
            if (base.duck != null)
            {
                RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(_fireRumble, RumbleDuration.Pulse, RumbleFalloff.None));
                base.duck.hSpeed = (0f - base.barrelVector.X) * 8f;
                base.duck.vSpeed = (0f - base.barrelVector.Y) * 4f - 2f;
                b.responsibleProfile = base.duck.profile;
            }
        }
        ammo--;
        SFX.Play("laserBlast");
    }
}
