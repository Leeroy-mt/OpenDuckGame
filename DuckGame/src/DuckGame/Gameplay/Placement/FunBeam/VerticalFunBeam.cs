using Microsoft.Xna.Framework;
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
        base.AngleDegrees = 90f;
        collisionOffset = new Vector2(-5f, -2f);
        collisionSize = new Vector2(10f, 4f);
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
            if (_prev != Position)
            {
                _endPoint = Vector2.Zero;
                for (int i = 0; i < 32; i++)
                {
                    Thing t = Level.CheckLine<Block>(Position + new Vector2(0f, 4 + i * 16), Position + new Vector2(0f, (i + 1) * 16 - 6));
                    if (t != null)
                    {
                        _endPoint = new Vector2(base.X, t.top - 2f);
                        break;
                    }
                }
                _prev = Position;
            }
            if (_endPoint != Vector2.Zero)
            {
                graphic.flipH = true;
                graphic.Depth = base.Depth;
                graphic.AngleDegrees = 90f;
                Graphics.Draw(graphic, _endPoint.X, _endPoint.Y);
                graphic.flipH = false;
                _beam.Depth = base.Depth - 2;
                float dist = _endPoint.Y - base.Y;
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
                    _beam.AngleDegrees = 90f;
                    Graphics.Draw(_beam, base.X, base.Y + (float)(j * 16));
                }
                collisionOffset = new Vector2(-4f, -1f);
                collisionSize = new Vector2(8f, dist);
            }
            else
            {
                collisionOffset = new Vector2(-5f, -1f);
                collisionSize = new Vector2(10f, 4f);
            }
        }
        base.Draw();
    }
}
