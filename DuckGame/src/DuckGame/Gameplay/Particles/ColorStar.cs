namespace DuckGame;

public class ColorStar : PhysicsParticle
{
    private float maxSize;

    public ColorStar(float xpos, float ypos, Vec2 dir, Color pColor)
        : base(xpos, ypos)
    {
        graphic = new Sprite("colorStar");
        graphic.CenterOrigin();
        Center = new Vec2(graphic.width / 2, graphic.height / 2);
        base.ScaleX = (base.ScaleY = 0.9f);
        hSpeed = dir.X;
        vSpeed = dir.Y;
        maxSize = 0.1f;
        graphic.color = pColor;
        _gravMult = 3f;
    }

    public override void Update()
    {
        base.ScaleX = Lerp.Float(base.ScaleX, maxSize, 0.04f);
        base.ScaleY = base.ScaleX;
        if (base.ScaleX <= maxSize)
        {
            Level.Remove(this);
        }
        base.Update();
    }
}
