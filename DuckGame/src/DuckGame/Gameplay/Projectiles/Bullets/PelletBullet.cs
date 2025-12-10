namespace DuckGame;

public class PelletBullet : Bullet
{
    private float _isVolatile = 1f;

    public PelletBullet(float xval, float yval, AmmoType type, float ang = -1f, Thing owner = null, bool rbound = false, float distance = -1f, bool tracer = false, bool network = true)
        : base(xval, yval, type, ang, owner, rbound, distance, tracer, network)
    {
    }

    protected override void Rebound(Vec2 pos, float dir, float rng)
    {
        PelletBullet obj = ammo.GetBullet(pos.x, pos.y, null, 0f - dir, base.firedFrom, rng, _tracer) as PelletBullet;
        obj._teleporter = _teleporter;
        obj._isVolatile = _isVolatile;
        obj.isLocal = isLocal;
        obj.lastReboundSource = lastReboundSource;
        obj.connection = base.connection;
        reboundCalled = true;
        Level.Add(obj);
        SFX.Play("littleRic", 0.8f, Rando.Float(-0.15f, 0.15f));
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
