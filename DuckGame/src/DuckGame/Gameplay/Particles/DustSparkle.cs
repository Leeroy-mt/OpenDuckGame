using Microsoft.Xna.Framework;

namespace DuckGame;

public class DustSparkle
{
    public Vector2 position;

    public Vector2 velocity;

    public float alpha;

    public float sin;

    public DustSparkle(Vector2 pos, Vector2 vel)
    {
        position = pos;
        velocity = vel;
        sin = Rando.Float(6f);
    }
}
