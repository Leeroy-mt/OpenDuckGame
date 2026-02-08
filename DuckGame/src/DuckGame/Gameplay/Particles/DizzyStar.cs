using Microsoft.Xna.Framework;

namespace DuckGame;

public class DizzyStar : PhysicsParticle
{
    private float maxSize;

    public DizzyStar(float xpos, float ypos, Vector2 dir)
        : base(xpos, ypos)
    {
        graphic = new Sprite("dizzyStar");
        graphic.CenterOrigin();
        base.ScaleX = (base.ScaleY = Rando.Float(0.7f, 1.3f));
        hSpeed = dir.X;
        vSpeed = dir.Y;
        maxSize = 0.1f;
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
