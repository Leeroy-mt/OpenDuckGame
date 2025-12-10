namespace DuckGame;

public class NewDizzyStar : PhysicsParticle, IFactory
{
    private float maxSize;

    public NewDizzyStar(float xpos, float ypos, Vec2 dir, Color pColor)
        : base(xpos, ypos)
    {
        graphic = new Sprite("colorStarLarge");
        graphic.CenterOrigin();
        center = new Vec2(graphic.width / 2, graphic.height / 2);
        base.xscale = (base.yscale = 0.7f);
        hSpeed = dir.x;
        vSpeed = dir.y;
        maxSize = 0.05f;
        graphic.color = pColor;
        _gravMult = 1.5f;
    }

    public override void Update()
    {
        base.xscale = Lerp.Float(base.xscale, maxSize, 0.025f);
        base.yscale = base.xscale;
        if (base.xscale <= maxSize)
        {
            Level.Remove(this);
        }
        base.Update();
    }
}
