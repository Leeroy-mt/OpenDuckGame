using Microsoft.Xna.Framework;

namespace DuckGame;

public class TrailPiece
{
    internal Vector2 position;

    internal Vector2 p1;

    internal Vector2 p2;

    internal Vector2 scale = new Vector2(1f, 1f);

    internal float wide = 1f;

    internal TrailPiece(float _x, float _y, float _width, Vector2 _p1, Vector2 _p2)
    {
        position.X = _x;
        position.Y = _y;
        wide = _width;
        p1 = _p1;
        p2 = _p2;
    }
}
