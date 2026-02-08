using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Background")]
public class BackgroundSpace : BackgroundTile
{
    public BackgroundSpace(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("spaceBackground", 16, 16, calculateTransparency: true);
        _opacityFromGraphic = true;
        Center = new Vector2(8f, 8f);
        collisionSize = new Vector2(16f, 16f);
        collisionOffset = new Vector2(-8f, -8f);
        _editorName = "Space";
    }
}
