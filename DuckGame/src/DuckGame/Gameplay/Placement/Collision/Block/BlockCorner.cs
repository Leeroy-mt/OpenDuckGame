using System.Collections.Generic;

namespace DuckGame;

public class BlockCorner(Vec2 c, Block b, bool wall = false)
{
    public Vec2 corner = c;

    public Block block = b;

    public bool wallCorner = wall;

    public List<BlockCorner> testedCorners = [];

    public BlockCorner Copy() =>
        new(corner, block, wallCorner);
}
