using Microsoft.Xna.Framework;

namespace DuckGame;

public class PathNodeLink
{
    public Thing link;

    public Thing owner;

    public float distance;

    public bool oneWay;

    public bool gap;

    public Vector2 position => owner.Position;
}
