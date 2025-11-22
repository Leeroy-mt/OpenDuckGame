using System;

namespace DuckGame;

[EditorGroup("Stuff")]
public class VerticalFunBeam : FunBeam
{
	public VerticalFunBeam(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_editorName = "Fun Beam Vertical";
		editorTooltip = "Ever seen a fun beam? Now try tilting your head sideways.";
		base.hugWalls = WallHug.Ceiling;
		base.angleDegrees = 90f;
		collisionOffset = new Vec2(-5f, -2f);
		collisionSize = new Vec2(10f, 4f);
		_placementCost += 2;
	}

	public override void Draw()
	{
		if (Editor.editorDraw)
		{
			return;
		}
		if (enabled)
		{
			if (_prev != position)
			{
				_endPoint = Vec2.Zero;
				for (int i = 0; i < 32; i++)
				{
					Thing t = Level.CheckLine<Block>(position + new Vec2(0f, 4 + i * 16), position + new Vec2(0f, (i + 1) * 16 - 6));
					if (t != null)
					{
						_endPoint = new Vec2(base.x, t.top - 2f);
						break;
					}
				}
				_prev = position;
			}
			if (_endPoint != Vec2.Zero)
			{
				graphic.flipH = true;
				graphic.depth = base.depth;
				graphic.angleDegrees = 90f;
				Graphics.Draw(graphic, _endPoint.x, _endPoint.y);
				graphic.flipH = false;
				_beam.depth = base.depth - 2;
				float dist = _endPoint.y - base.y;
				int numReq = (int)Math.Ceiling(dist / 16f);
				for (int j = 0; j < numReq; j++)
				{
					if (j == numReq - 1)
					{
						_beam.cutWidth = 16 - (int)(dist % 16f);
					}
					else
					{
						_beam.cutWidth = 0;
					}
					_beam.angleDegrees = 90f;
					Graphics.Draw(_beam, base.x, base.y + (float)(j * 16));
				}
				collisionOffset = new Vec2(-4f, -1f);
				collisionSize = new Vec2(8f, dist);
			}
			else
			{
				collisionOffset = new Vec2(-5f, -1f);
				collisionSize = new Vec2(10f, 4f);
			}
		}
		base.Draw();
	}
}
