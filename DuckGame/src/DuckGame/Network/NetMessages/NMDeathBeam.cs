using Microsoft.Xna.Framework;

namespace DuckGame;

public class NMDeathBeam : NMEvent
{
    public HugeLaser laser;

    public Vector2 position;

    public Vector2 target;

    public NMDeathBeam()
    {
    }

    public NMDeathBeam(HugeLaser pLaser, Vector2 pPosition, Vector2 pTarget)
    {
        position = pPosition;
        target = pTarget;
        laser = pLaser;
    }

    public override void Activate()
    {
        Level.Add(new DeathBeam(position, target)
        {
            isLocal = false
        });
        if (laser != null)
        {
            laser.PostFireLogic();
        }
        base.Activate();
    }
}
