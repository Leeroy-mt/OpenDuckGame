using System.Collections.Generic;

namespace DuckGame;

public class GrenadeBullet : Bullet
{
    private float _isVolatile = 1f;

    public GrenadeBullet(float xval, float yval, AmmoType type, float ang = -1f, Thing owner = null, bool rbound = false, float distance = -1f, bool tracer = false, bool network = true)
        : base(xval, yval, type, ang, owner, rbound, distance, tracer, network)
    {
    }

    protected override void OnHit(bool destroyed)
    {
        if (!destroyed || !isLocal)
        {
            return;
        }
        for (int repeat = 0; repeat < 1; repeat++)
        {
            ExplosionPart explosionPart = new ExplosionPart(base.x - 8f + Rando.Float(16f), base.y - 8f + Rando.Float(16f));
            explosionPart.xscale *= 0.7f;
            explosionPart.yscale *= 0.7f;
            Level.Add(explosionPart);
        }
        SFX.Play("explode");
        RumbleManager.AddRumbleEvent(position, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
        foreach (TV item in Level.CheckCircleAll<TV>(position, 20f))
        {
            item.Destroy(new DTImpact(this));
        }
        List<Bullet> firedBullets = new List<Bullet>();
        Vec2 bPos = position;
        bPos -= travelDirNormalized;
        for (int i = 0; i < 12; i++)
        {
            float dir = (float)i * 30f - 10f + Rando.Float(20f);
            ATGrenadeLauncherShrapnel shrap = new ATGrenadeLauncherShrapnel();
            shrap.range = 25f + Rando.Float(10f);
            Bullet bullet = new Bullet(bPos.x, bPos.y, shrap, dir);
            bullet.firedFrom = this;
            firedBullets.Add(bullet);
            Level.Add(bullet);
        }
        if (Network.isActive && isLocal)
        {
            Send.Message(new NMFireGun(null, firedBullets, 0, rel: false, 4), NetMessagePriority.ReliableOrdered);
            firedBullets.Clear();
        }
        foreach (Window w in Level.CheckCircleAll<Window>(position, 20f))
        {
            if (Level.CheckLine<Block>(position, w.position, w) == null)
            {
                w.Destroy(new DTImpact(this));
            }
        }
    }

    protected override void Rebound(Vec2 pos, float dir, float rng)
    {
        GrenadeBullet obj = ammo.GetBullet(pos.x, pos.y, null, 0f - dir, base.firedFrom, rng, _tracer) as GrenadeBullet;
        obj._teleporter = _teleporter;
        obj._isVolatile = _isVolatile;
        obj.isLocal = isLocal;
        obj.lastReboundSource = lastReboundSource;
        obj.connection = base.connection;
        reboundCalled = true;
        Level.Add(obj);
        SFX.Play("grenadeBounce", 0.8f, Rando.Float(-0.1f, 0.1f));
    }

    public override void Update()
    {
        _isVolatile -= 0.06f;
        if (_isVolatile <= 0f)
        {
            rebound = false;
        }
        base.Update();
    }
}
