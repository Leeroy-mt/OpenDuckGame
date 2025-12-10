using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class LaserBulletOrange : LaserBullet
{
    private int _travels;

    private bool _exploded;

    public LaserBulletOrange(float xval, float yval, AmmoType type, float ang = -1f, Thing owner = null, bool rbound = false, float distance = -1f, bool tracer = false, bool network = false)
        : base(xval, yval, type, ang, owner, rbound, distance, tracer, network)
    {
        _thickness = type.bulletThickness;
        _beem = Content.Load<Texture2D>("laserBeemOrange");
    }

    protected override void CheckTravelPath(Vec2 pStart, Vec2 pEnd)
    {
        if (_thickness > 1f && _travels > 0)
        {
            for (int i = 0; i < 10; i++)
            {
                Vec2 vec = pStart + (pEnd - pStart) * ((float)i / 10f);
                if (ATMissile.DestroyRadius(vec, 16f, this, pExplode: true) > 0 && !_exploded)
                {
                    _exploded = true;
                    SFX.Play("explode");
                }
                foreach (PhysicsObject o in Level.CheckCircleAll<PhysicsObject>(vec, 16f))
                {
                    if (!(o is Gun) && !(o is Equipment))
                    {
                        o.Destroy(new DTIncinerate(this));
                    }
                }
            }
        }
        _travels++;
    }
}
