using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Details|Terrain")]
[BaggedProperty("isInDemo", true)]
[BaggedProperty("previewPriority", true)]
public class TreeTop : Thing
{
    private Sprite _treeInside;

    public TreeTop(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("treeTop");
        _treeInside = new Sprite("treeTopInside");
        _treeInside.Center = new Vector2(24f, 24f);
        _treeInside.Alpha = 0.8f;
        _treeInside.Depth = 0.9f;
        Center = new Vector2(24f, 24f);
        _collisionSize = new Vector2(16f, 16f);
        _collisionOffset = new Vector2(-8f, -8f);
        base.Depth = 0.9f;
        base.hugWalls = WallHug.Left | WallHug.Right | WallHug.Ceiling | WallHug.Floor;
    }

    public override void Draw()
    {
        graphic.flipH = offDir <= 0;
        base.Draw();
    }
}
