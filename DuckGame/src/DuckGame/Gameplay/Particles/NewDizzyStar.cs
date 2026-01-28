namespace DuckGame;

public class NewDizzyStar : PhysicsParticle
{
    private float maxSize;

    public NewDizzyStar(float xpos, float ypos, Vec2 dir, Color pColor)
        : base(xpos, ypos)
    {
        graphic = new Sprite("colorStarLarge");
        graphic.CenterOrigin();
        Center = new Vec2(graphic.width / 2, graphic.height / 2);
        base.ScaleX = (base.ScaleY = 0.7f);
        hSpeed = dir.X;
        vSpeed = dir.Y;
        maxSize = 0.05f;
        graphic.color = pColor;
        _gravMult = 1.5f;
    }

    public override void Update()
    {
        base.ScaleX = Lerp.Float(base.ScaleX, maxSize, 0.025f);
        base.ScaleY = base.ScaleX;
        if (base.ScaleX <= maxSize)
        {
            Level.Remove(this);
        }
        base.Update();
    }
}
