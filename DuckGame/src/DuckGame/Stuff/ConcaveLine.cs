using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class ConcaveLine
{
    public Vector2 p1;

    public Vector2 p2;

    public int index;

    public List<ConcaveLine> intersects = new List<ConcaveLine>();

    public ConcaveLine(Vector2 p1val, Vector2 p2val)
    {
        p1 = p1val;
        p2 = p2val;
    }
}
