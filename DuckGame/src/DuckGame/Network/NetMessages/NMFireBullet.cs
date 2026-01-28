namespace DuckGame;

public class NMFireBullet : NMEvent
{
    public float range;

    public float speed;

    public float angle;

    public AmmoType typeInstance;

    public NMFireBullet()
    {
    }

    public NMFireBullet(float varRange, float varSpeed, float varAngle)
    {
        range = varRange;
        speed = varSpeed;
        angle = varAngle;
    }

    public void DoActivate(Vec2 position, Profile owner)
    {
        typeInstance.rangeVariation = 0f;
        typeInstance.accuracy = 1f;
        typeInstance.bulletSpeed = speed;
        typeInstance.speedVariation = 0f;
        Bullet bullet = typeInstance.GetBullet(position.X, position.Y, owner?.duck, 0f - angle, null, range, tracer: false, network: false);
        bullet.isLocal = false;
        bullet.connection = base.connection;
        Level.current.AddThing(bullet);
    }
}
