using System;

namespace DuckGame;

[EditorGroup("Details|Terrain")]
public class RockWall : Block, IBigStupidWall
{
    private Sprite _wall;

    public RockWall(float xpos, float ypos, Type c = null)
        : base(xpos, ypos)
    {
        graphic = new Sprite("laserSpawner");
        Center = new Vec2(8f, 8f);
        collisionSize = new Vec2(12f, 12f);
        collisionOffset = new Vec2(-6f, -6f);
        base.Depth = -0.6f;
        base.hugWalls = WallHug.None;
        base.layer = Layer.Foreground;
        physicsMaterial = PhysicsMaterial.Metal;
        _visibleInGame = true;
        _wall = new Sprite("rockWall");
        _wall.Center = new Vec2(_wall.w - 4, _wall.h / 2);
        editorTooltip = "Adds an infinite vertical rock wall.";
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            collisionSize = new Vec2(64f, 4096f);
            collisionOffset = new Vec2(-61f, -700f);
        }
        base.Initialize();
    }

    public override void Draw()
    {
        _wall.flipH = flipHorizontal;
        if (!(Level.current is Editor))
        {
            Graphics.Draw(_wall, base.X, base.Y);
            if (Level.current.topLeft.Y < base.Y - 500f)
            {
                Graphics.Draw(_wall, base.X, base.Y - (float)_wall.h);
            }
            if (Level.current.bottomRight.Y > base.Y + 500f)
            {
                Graphics.Draw(_wall, base.X, base.Y + (float)_wall.h);
            }
        }
        else
        {
            Graphics.DrawLine(Position, Position + new Vec2(flipHorizontal ? 16 : (-16), 0f), Color.Red);
            base.Draw();
        }
    }
}
