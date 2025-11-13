using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[EditorGroup("Stuff|Pipes")]
[BaggedProperty("isOnlineCapable", true)]
public class PipeRed : PipeTileset
{
	public PipeRed(float x, float y)
		: base(x, y, "travelPipes")
	{
		_editorName = "Red Pipe";
		editorTooltip = "Ducks who travel through Red pipes are said to have good hearts.";
		pipeDepth = 0.93f;
	}

	protected override Dictionary<Direction, PipeTileset> GetNeighbors()
	{
		Dictionary<Direction, PipeTileset> neighbors = new Dictionary<Direction, PipeTileset>();
		PipeTileset up = (from x in Level.CheckPointAll<PipeRed>(base.x, base.y - 16f)
			where x.@group == @group
			select x).FirstOrDefault();
		if (up != null)
		{
			neighbors[Direction.Up] = up;
		}
		PipeTileset down = (from x in Level.CheckPointAll<PipeRed>(base.x, base.y + 16f)
			where x.@group == @group
			select x).FirstOrDefault();
		if (down != null)
		{
			neighbors[Direction.Down] = down;
		}
		PipeTileset left = (from x in Level.CheckPointAll<PipeRed>(base.x - 16f, base.y)
			where x.@group == @group
			select x).FirstOrDefault();
		if (left != null)
		{
			neighbors[Direction.Left] = left;
		}
		PipeTileset right = (from x in Level.CheckPointAll<PipeRed>(base.x + 16f, base.y)
			where x.@group == @group
			select x).FirstOrDefault();
		if (right != null)
		{
			neighbors[Direction.Right] = right;
		}
		return neighbors;
	}
}
