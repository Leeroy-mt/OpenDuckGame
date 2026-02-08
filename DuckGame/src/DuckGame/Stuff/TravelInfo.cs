using Microsoft.Xna.Framework;

namespace DuckGame;

public class TravelInfo
{
    public Vector2 p1;

    public Vector2 p2;

    public float length;

    public TravelInfo(Vector2 point1, Vector2 point2, float len)
    {
        p1 = point1;
        p2 = point2;
        length = len;
    }
}
