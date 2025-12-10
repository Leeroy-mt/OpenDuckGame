namespace DuckGame;

public class DizzyStar : PhysicsParticle, IFactory
{
    private float maxSize;

    public DizzyStar(float xpos, float ypos, Vec2 dir)
        : base(xpos, ypos)
    {
        graphic = new Sprite("dizzyStar");
        graphic.CenterOrigin();
        base.xscale = (base.yscale = Rando.Float(0.7f, 1.3f));
        hSpeed = dir.x;
        vSpeed = dir.y;
        maxSize = 0.1f;
    }

    public override void Update()
    {
        base.xscale = Lerp.Float(base.xscale, maxSize, 0.04f);
        base.yscale = base.xscale;
        if (base.xscale <= maxSize)
        {
            Level.Remove(this);
        }
        base.Update();
    }
}
