namespace DuckGame;

public class Platform : MaterialThing, IPlatform
{
    public Platform(float x, float y)
        : base(x, y)
    {
        collisionSize = new(16, 16);
        thickness = 10;
    }

    public Platform(float x, float y, float wid, float hi)
        : base(x, y)
    {
        collisionSize = new(wid, hi);
        thickness = 10;
    }
}
