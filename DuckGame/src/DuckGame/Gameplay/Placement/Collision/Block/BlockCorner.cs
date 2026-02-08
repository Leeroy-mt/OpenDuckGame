using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class BlockCorner(Vector2 c, Block b, bool wall = false)
{
    public Vector2 corner = c;

    public Block block = b;

    public bool wallCorner = wall;

    public List<BlockCorner> testedCorners = [];

    public BlockCorner Copy() =>
        new(corner, block, wallCorner);
}
