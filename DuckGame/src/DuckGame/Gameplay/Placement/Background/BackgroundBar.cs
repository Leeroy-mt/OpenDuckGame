namespace DuckGame;

[EditorGroup("Background")]
public class BackgroundBar : BackgroundTile
{
    public BackgroundBar(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("barback", 16, 16, calculateTransparency: true);
        _opacityFromGraphic = true;
        Center = new Vec2(8f, 8f);
        collisionSize = new Vec2(16f, 16f);
        collisionOffset = new Vec2(-8f, -8f);
        _editorName = "BAR";
    }
}
