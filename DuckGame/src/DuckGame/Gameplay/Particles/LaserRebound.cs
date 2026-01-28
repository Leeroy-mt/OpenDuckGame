namespace DuckGame;

public class LaserRebound : Thing
{
    private Tex2D _rebound = Content.Load<Tex2D>("laserRebound");

    public LaserRebound(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite(_rebound);
        base.Depth = 0.9f;
        Center = new Vec2(4f, 4f);
    }

    public override void Update()
    {
        base.Alpha -= 0.07f;
        if (base.Alpha <= 0f)
        {
            Level.Remove(this);
        }
    }
}
