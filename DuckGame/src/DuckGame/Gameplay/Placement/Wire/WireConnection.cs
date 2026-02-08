using Microsoft.Xna.Framework;

namespace DuckGame;

public class WireConnection
{
    public Vector2 position;

    public WireConnection up;

    public WireConnection down;

    public WireConnection left;

    public WireConnection right;

    public bool wireRight;

    public bool wireLeft;

    public bool wireUp;

    public bool wireDown;
}
