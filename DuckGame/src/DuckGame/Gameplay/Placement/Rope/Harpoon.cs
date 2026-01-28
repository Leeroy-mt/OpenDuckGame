namespace DuckGame;

public class Harpoon : Thing
{
    public bool _inGun = true;

    public bool _stuck;

    private float _hangGrav = 0.1f;

    private float _hangPull;

    public Thing _belongsTo;

    public bool noisy = true;

    public ISwing swingOwner => _owner as ISwing;

    public bool inGun => _inGun;

    public bool stuck => _stuck;

    public override NetworkConnection connection
    {
        get
        {
            if (_belongsTo != null)
            {
                return _belongsTo.connection;
            }
            return base.connection;
        }
    }

    public override NetIndex8 authority
    {
        get
        {
            if (_belongsTo != null)
            {
                return _belongsTo.authority;
            }
            return base.authority;
        }
    }

    public Harpoon(Thing belongsTo = null)
    {
        _belongsTo = belongsTo;
        owner = belongsTo;
        graphic = new Sprite("hook");
        Center = new Vec2(3f, 3f);
        collisionOffset = new Vec2(-5f, -1.5f);
        collisionSize = new Vec2(10f, 5f);
    }

    public override void Update()
    {
        if (base.isServerForObject)
        {
            if (!_stuck)
            {
                base.Update();
            }
            else if (swingOwner != null)
            {
                Thing piece = swingOwner.GetRopeParent(this);
            }
            if (_owner is Grapple && _inGun)
            {
                Grapple g = _owner as Grapple;
                Position = g.barrelPosition;
                base.Depth = g.Depth - 1;
                hSpeed = 0f;
                vSpeed = 0f;
                graphic.flipH = (float)g.offDir < 0f;
            }
        }
    }

    public void Latch(Vec2 point)
    {
        _inGun = false;
        Position = point;
        _stuck = true;
    }

    public void SetStuckPoint(Vec2 pPoint)
    {
        _inGun = false;
        Position = pPoint;
        _stuck = true;
    }

    public void Fire(Vec2 point, Vec2 travel)
    {
        if (!_inGun)
        {
            return;
        }
        _inGun = false;
        Position = point + travel * -2f;
        _stuck = true;
        if (noisy)
        {
            SFX.Play("grappleHook", 0.5f);
            for (int i = 0; i < 6; i++)
            {
                Level.Add(Spark.New(point.X - travel.X * 2f, point.Y - travel.Y * 2f, travel));
            }
            for (int j = 0; j < 1; j++)
            {
                Level.Add(SmallSmoke.New(point.X + Rando.Float(-2f, 2f), point.Y + Rando.Float(-2f, 2f)));
            }
        }
    }

    public void Return()
    {
        if (!_inGun)
        {
            _inGun = true;
            hSpeed = 0f;
            vSpeed = 0f;
            _stuck = false;
        }
    }

    public override void Draw()
    {
        if (!inGun && noisy)
        {
            base.Draw();
        }
    }
}
