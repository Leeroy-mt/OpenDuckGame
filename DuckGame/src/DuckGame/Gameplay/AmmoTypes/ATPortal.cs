namespace DuckGame;

public class ATPortal : AmmoType
{
    public bool angleShot = true;

    private PortalGun _ownerGun;

    public ATPortal(PortalGun OwnerGun)
    {
        accuracy = 0.75f;
        range = 250f;
        penetration = 1f;
        bulletSpeed = 20f;
        rebound = true;
        bulletThickness = 0.3f;
        _ownerGun = OwnerGun;
    }

    public override Bullet FireBullet(Vec2 position, Thing owner = null, float angle = 0f, Thing firedFrom = null)
    {
        angle *= -1f;
        Bullet bullet = new PortalBullet(position.X, position.Y, this, angle, _ownerGun, rebound, -1f, bulletThickness);
        bullet.firedFrom = firedFrom;
        Level.current.AddThing(bullet);
        return bullet;
    }
}
