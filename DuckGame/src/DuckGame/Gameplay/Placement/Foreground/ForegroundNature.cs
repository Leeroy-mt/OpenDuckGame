namespace DuckGame;

[EditorGroup("Background")]
[BaggedProperty("isInDemo", true)]
public class ForegroundNature : ForegroundTile
{
    public ForegroundNature(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("foregroundNature", 16, 16);
        Center = new(8);
        collisionSize = new(16);
        collisionOffset = new(-8);
        layer = Layer.Foreground;
        _editorName = "Foliage";
    }
}
