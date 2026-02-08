using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[EditorGroup("Stuff")]
public class FunBeam : MaterialThing
{
    protected SpriteMap _beam;

    protected Vector2 _prev = Vector2.Zero;

    protected Vector2 _endPoint = Vector2.Zero;

    public bool enabled = true;

    public FunBeam(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _beam = new SpriteMap("funBeam", 16, 16);
        _beam.ClearAnimations();
        _beam.AddAnimation("idle", 1f, true, 0, 1, 2, 3, 4, 5, 6, 7);
        _beam.SetAnimation("idle");
        _beam.speed = 0.2f;
        _beam.Alpha = 0.3f;
        _beam.Center = new Vector2(0f, 8f);
        graphic = new Sprite("funBeamer");
        Center = new Vector2(9f, 8f);
        collisionOffset = new Vector2(-2f, -5f);
        collisionSize = new Vector2(4f, 10f);
        base.Depth = -0.5f;
        _editorName = "Fun Beam";
        editorTooltip = "Place 2 generators near each other to create a beam that triggers weapons passing through.";
        base.hugWalls = WallHug.Left;
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (enabled && with is Gun g && !(g is Sword) && !(g is SledgeHammer))
        {
            g.PressAction();
        }
    }

    public override void Draw()
    {
        if (Editor.editorDraw)
        {
            return;
        }
        if (enabled && GetType() == typeof(FunBeam))
        {
            if (_prev != Position)
            {
                _endPoint = Vector2.Zero;
                for (int i = 0; i < 32; i++)
                {
                    Thing t = Level.CheckLine<Block>(Position + new Vector2(4 + i * 16, 0f), Position + new Vector2((i + 1) * 16 - 6, 0f));
                    if (t != null)
                    {
                        _endPoint = new Vector2(t.left - 2f, base.Y);
                        break;
                    }
                }
                _prev = Position;
            }
            if (_endPoint != Vector2.Zero)
            {
                graphic.flipH = true;
                graphic.Depth = base.Depth;
                Graphics.Draw(graphic, _endPoint.X, _endPoint.Y);
                graphic.flipH = false;
                _beam.Depth = base.Depth - 2;
                float dist = _endPoint.X - base.X;
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
                    Graphics.Draw(_beam, base.X + (float)(j * 16), base.Y);
                }
                collisionOffset = new Vector2(-1f, -4f);
                collisionSize = new Vector2(dist, 8f);
            }
            else
            {
                collisionOffset = new Vector2(-1f, -5f);
                collisionSize = new Vector2(4f, 10f);
            }
        }
        base.Draw();
    }
}
