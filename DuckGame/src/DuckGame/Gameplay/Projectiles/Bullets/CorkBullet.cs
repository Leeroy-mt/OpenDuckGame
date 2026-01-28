namespace DuckGame;

public class CorkBullet : Bullet
{
    private Sprite _cork;

    private CorkObject _corkObject;

    public CorkBullet(float xval, float yval, AmmoType type, float ang = -1f, Thing owner = null, bool rbound = false, float distance = -1f, bool tracer = false, bool network = false)
        : base(xval, yval, type, ang, owner, rbound, distance, tracer, network)
    {
        _cork = new Sprite("cork");
        _cork.Center = new Vec2(3f, 2.5f);
    }

    public override void Update()
    {
        base.Update();
        if (doneTravelling && _corkObject == null)
        {
            _corkObject = new CorkObject(drawEnd.X - travelDirNormalized.X * 4f, drawEnd.Y - travelDirNormalized.Y * 4f, base.firedFrom);
            if (base.firedFrom != null && base.firedFrom is CorkGun)
            {
                (base.firedFrom as CorkGun).corkObject = _corkObject;
            }
            Level.Add(_corkObject);
            Level.Remove(this);
        }
    }

    public override void Draw()
    {
        Graphics.Draw(_cork, drawEnd.X, drawEnd.Y);
        base.Draw();
    }
}
