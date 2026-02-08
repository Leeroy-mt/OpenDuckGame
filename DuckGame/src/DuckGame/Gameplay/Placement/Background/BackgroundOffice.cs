using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Background")]
public class BackgroundOffice : BackgroundTile
{
    public BackgroundOffice(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("officeBackground", 16, 16, calculateTransparency: true);
        _opacityFromGraphic = true;
        Center = new Vector2(8f, 8f);
        collisionSize = new Vector2(16f, 16f);
        collisionOffset = new Vector2(-8f, -8f);
        _editorName = "Office";
    }
}
