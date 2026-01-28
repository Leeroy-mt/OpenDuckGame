using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MagBullet : Bullet
{
    private Texture2D _beem;

    private float _thickness;

    public MagBullet(float xval, float yval, AmmoType type, float ang = -1f, Thing owner = null, bool rbound = false, float distance = -1f, bool tracer = false, bool network = false)
        : base(xval, yval, type, ang, owner, rbound, distance, tracer, network)
    {
        _thickness = type.bulletThickness;
        _beem = Content.Load<Texture2D>("magBeam");
    }

    public override void Draw()
    {
        if (_tracer || !(_bulletDistance > 0.1f))
        {
            return;
        }
        float length = (drawStart - drawEnd).Length();
        float dist = 0f;
        float incs = 1f / (length / 8f);
        float alph = 0f;
        float drawLength = 8f;
        while (true)
        {
            bool doBreak = false;
            if (dist + drawLength > length)
            {
                drawLength = length - Maths.Clamp(dist, 0f, 99f);
                doBreak = true;
            }
            alph += incs;
            Graphics.DrawTexturedLine(_beem, drawStart + travelDirNormalized * dist, drawStart + travelDirNormalized * (dist + drawLength), Color.White * alph, _thickness, 0.6f);
            if (!doBreak)
            {
                dist += 8f;
                continue;
            }
            break;
        }
    }

    protected override void OnHit(bool destroyed)
    {
        if (!destroyed)
        {
            return;
        }
        ExplosionPart explosionPart = new ExplosionPart(base.X, base.Y);
        explosionPart.ScaleX *= 0.7f;
        explosionPart.ScaleY *= 0.7f;
        Level.Add(explosionPart);
        SFX.Play("magPop", 0.7f, Rando.Float(-0.5f, -0.3f));
        if (!isLocal)
        {
            return;
        }
        Thing bulletOwner = owner;
        foreach (MaterialThing t in Level.CheckCircleAll<MaterialThing>(Position, 14f))
        {
            if (t != bulletOwner)
            {
                Thing.SuperFondle(t, DuckNetwork.localConnection);
                t.Destroy(new DTShot(this));
            }
        }
    }

    protected override void Rebound(Vec2 pos, float dir, float rng)
    {
        MagBullet bullet = new MagBullet(pos.X, pos.Y, ammo, dir, null, rebound, rng);
        bullet._teleporter = _teleporter;
        bullet.firedFrom = base.firedFrom;
        bullet.lastReboundSource = lastReboundSource;
        bullet.connection = base.connection;
        bullet.isLocal = isLocal;
        reboundCalled = true;
        Level.current.AddThing(bullet);
        Level.current.AddThing(new LaserRebound(pos.X, pos.Y));
    }
}
