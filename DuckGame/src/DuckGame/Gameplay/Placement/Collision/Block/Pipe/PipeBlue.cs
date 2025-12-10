using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[EditorGroup("Stuff|Pipes")]
[BaggedProperty("isOnlineCapable", true)]
public class PipeBlue : PipeTileset
{
    public PipeBlue(float x, float y)
        : base(x, y, "travelPipesBlue")
    {
        _editorName = "Blue Pipe";
        editorTooltip = "Ducks who travel through Blue pipes sometimes wish they'd never come out the other side.";
        pipeDepth = 0.95f;
    }

    protected override Dictionary<Direction, PipeTileset> GetNeighbors()
    {
        Dictionary<Direction, PipeTileset> neighbors = new Dictionary<Direction, PipeTileset>();
        PipeTileset up = (from x in Level.CheckPointAll<PipeBlue>(base.x, base.y - 16f)
                          where x.@group == @group
                          select x).FirstOrDefault();
        if (up != null)
        {
            neighbors[Direction.Up] = up;
        }
        PipeTileset down = (from x in Level.CheckPointAll<PipeBlue>(base.x, base.y + 16f)
                            where x.@group == @group
                            select x).FirstOrDefault();
        if (down != null)
        {
            neighbors[Direction.Down] = down;
        }
        PipeTileset left = (from x in Level.CheckPointAll<PipeBlue>(base.x - 16f, base.y)
                            where x.@group == @group
                            select x).FirstOrDefault();
        if (left != null)
        {
            neighbors[Direction.Left] = left;
        }
        PipeTileset right = (from x in Level.CheckPointAll<PipeBlue>(base.x + 16f, base.y)
                             where x.@group == @group
                             select x).FirstOrDefault();
        if (right != null)
        {
            neighbors[Direction.Right] = right;
        }
        return neighbors;
    }
}
