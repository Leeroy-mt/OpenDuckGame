namespace DuckGame;

[EditorGroup("Details|Terrain")]
[BaggedProperty("isInDemo", true)]
public class TreeTopDead : Thing
{
    private Sprite _treeInside;

    public TreeTopDead(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("treeTopDead");
        _treeInside = new Sprite("treeTopInsideDead");
        _treeInside.Center = new Vec2(24f, 24f);
        _treeInside.Alpha = 0.8f;
        _treeInside.Depth = 0.9f;
        Center = new Vec2(24f, 24f);
        _collisionSize = new Vec2(16f, 16f);
        _collisionOffset = new Vec2(-8f, -8f);
        base.Depth = 0.9f;
        base.hugWalls = WallHug.Left | WallHug.Right | WallHug.Ceiling | WallHug.Floor;
    }

    public override void Draw()
    {
        graphic.flipH = offDir <= 0;
        base.Draw();
    }
}
