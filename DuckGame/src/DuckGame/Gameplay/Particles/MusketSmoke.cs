using Microsoft.Xna.Framework;

namespace DuckGame;

public class MusketSmoke : Thing
{
    private float _angleInc;

    private float _scaleInc;

    private float _fade;

    public Vector2 move;

    public Vector2 fly;

    private float _fastGrow;

    private Sprite _backgroundSmoke;

    public MusketSmoke(float xpos, float ypos)
        : base(xpos, ypos)
    {
        base.ScaleX = 0.15f + Rando.Float(0.15f);
        base.ScaleY = base.ScaleX;
        Angle = Maths.DegToRad(Rando.Float(360f));
        _fastGrow = 0.6f + Rando.Float(0.3f);
        _angleInc = Maths.DegToRad(-1f + Rando.Float(2f));
        _scaleInc = 0.001f + Rando.Float(0.001f);
        _fade = 0.0015f + Rando.Float(0.001f);
        move.X = -0.1f + Rando.Float(0.2f);
        move.Y = -0.1f + Rando.Float(0.2f);
        GraphicList glist = new GraphicList();
        Sprite smoke = new Sprite("smoke");
        smoke.Depth = 1f;
        smoke.CenterOrigin();
        glist.Add(smoke);
        Sprite backSmoke = new Sprite("smokeBack");
        backSmoke.Depth = -0.1f;
        backSmoke.CenterOrigin();
        glist.Add(backSmoke);
        graphic = glist;
        Center = new Vector2(0f, 0f);
        base.Depth = 1f;
        _backgroundSmoke = new Sprite("smokeBack");
    }

    public override void Update()
    {
        Angle += _angleInc;
        base.ScaleX += _scaleInc;
        if (_fastGrow > 0f)
        {
            _fastGrow -= 0.05f;
            base.ScaleX += 0.05f;
        }
        if (fly.X > 0.01f || fly.X < -0.01f)
        {
            base.X += fly.X;
            fly.X *= 0.9f;
        }
        if (fly.Y > 0.01f || fly.Y < -0.01f)
        {
            base.Y += fly.Y;
            fly.Y *= 0.9f;
        }
        base.ScaleY = base.ScaleX;
        base.X += move.X;
        base.Y += move.Y;
        base.ScaleX -= 0.005f;
        if (base.ScaleX < 0.1f)
        {
            Level.Remove(this);
        }
    }
}
