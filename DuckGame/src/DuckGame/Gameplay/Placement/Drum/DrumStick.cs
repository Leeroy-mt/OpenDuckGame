namespace DuckGame;

public class DrumStick : Thing
{
    private float _startY;

    public DrumStick(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("drumset/drumStick");
        Center = new Vec2(graphic.w / 2, graphic.h / 2);
        _startY = ypos;
        vSpeed = -3f;
    }

    public override void Update()
    {
        Angle += 0.6f;
        base.Y += vSpeed;
        vSpeed += 0.2f;
        if (base.Y > _startY)
        {
            Level.Remove(this);
        }
    }
}
