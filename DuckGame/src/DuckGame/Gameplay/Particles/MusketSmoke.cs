namespace DuckGame;

public class MusketSmoke : Thing
{
    private float _angleInc;

    private float _scaleInc;

    private float _fade;

    public Vec2 move;

    public Vec2 fly;

    private float _fastGrow;

    private Sprite _backgroundSmoke;

    public MusketSmoke(float xpos, float ypos)
        : base(xpos, ypos)
    {
        base.xscale = 0.15f + Rando.Float(0.15f);
        base.yscale = base.xscale;
        angle = Maths.DegToRad(Rando.Float(360f));
        _fastGrow = 0.6f + Rando.Float(0.3f);
        _angleInc = Maths.DegToRad(-1f + Rando.Float(2f));
        _scaleInc = 0.001f + Rando.Float(0.001f);
        _fade = 0.0015f + Rando.Float(0.001f);
        move.x = -0.1f + Rando.Float(0.2f);
        move.y = -0.1f + Rando.Float(0.2f);
        GraphicList glist = new GraphicList();
        Sprite smoke = new Sprite("smoke");
        smoke.depth = 1f;
        smoke.CenterOrigin();
        glist.Add(smoke);
        Sprite backSmoke = new Sprite("smokeBack");
        backSmoke.depth = -0.1f;
        backSmoke.CenterOrigin();
        glist.Add(backSmoke);
        graphic = glist;
        center = new Vec2(0f, 0f);
        base.depth = 1f;
        _backgroundSmoke = new Sprite("smokeBack");
    }

    public override void Update()
    {
        angle += _angleInc;
        base.xscale += _scaleInc;
        if (_fastGrow > 0f)
        {
            _fastGrow -= 0.05f;
            base.xscale += 0.05f;
        }
        if (fly.x > 0.01f || fly.x < -0.01f)
        {
            base.x += fly.x;
            fly.x *= 0.9f;
        }
        if (fly.y > 0.01f || fly.y < -0.01f)
        {
            base.y += fly.y;
            fly.y *= 0.9f;
        }
        base.yscale = base.xscale;
        base.x += move.x;
        base.y += move.y;
        base.xscale -= 0.005f;
        if (base.xscale < 0.1f)
        {
            Level.Remove(this);
        }
    }
}
