using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[EditorGroup("Stuff|Pipes")]
[BaggedProperty("isOnlineCapable", true)]
public class PipeGreen : PipeTileset
{
    public PipeGreen(float x, float y)
        : base(x, y, "travelPipesGreen")
    {
        _editorName = "Green Pipe";
        editorTooltip = "Ducks who travel through Green pipes are said to be industrious and savvy.";
        pipeDepth = 0.91f;
    }

    protected override Dictionary<Direction, PipeTileset> GetNeighbors()
    {
        Dictionary<Direction, PipeTileset> neighbors = new Dictionary<Direction, PipeTileset>();
        PipeTileset up = (from x in Level.CheckPointAll<PipeGreen>(base.X, base.Y - 16f)
                          where x.@group == @group
                          select x).FirstOrDefault();
        if (up != null)
        {
            neighbors[Direction.Up] = up;
        }
        PipeTileset down = (from x in Level.CheckPointAll<PipeGreen>(base.X, base.Y + 16f)
                            where x.@group == @group
                            select x).FirstOrDefault();
        if (down != null)
        {
            neighbors[Direction.Down] = down;
        }
        PipeTileset left = (from x in Level.CheckPointAll<PipeGreen>(base.X - 16f, base.Y)
                            where x.@group == @group
                            select x).FirstOrDefault();
        if (left != null)
        {
            neighbors[Direction.Left] = left;
        }
        PipeTileset right = (from x in Level.CheckPointAll<PipeGreen>(base.X + 16f, base.Y)
                             where x.@group == @group
                             select x).FirstOrDefault();
        if (right != null)
        {
            neighbors[Direction.Right] = right;
        }
        return neighbors;
    }
}
