using Microsoft.Xna.Framework;

namespace DuckGame;

public class LightOccluder
{
    public Vector2 p1;

    public Vector2 p2;

    public Color color;

    public LightOccluder(Vector2 p, Vector2 pp, Color c)
    {
        p1 = p;
        p2 = pp;
        color = c;
    }
}
