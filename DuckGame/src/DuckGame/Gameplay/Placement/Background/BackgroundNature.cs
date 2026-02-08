using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Background")]
public class BackgroundNature : BackgroundTile
{
    public BackgroundNature(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("natureBackground", 16, 16);
        _opacityFromGraphic = true;
        Center = new Vector2(8f, 8f);
        collisionSize = new Vector2(16f, 16f);
        collisionOffset = new Vector2(-8f, -8f);
        _editorName = "Nature";
    }
}
