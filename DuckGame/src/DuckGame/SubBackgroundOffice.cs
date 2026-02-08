using Microsoft.Xna.Framework;

namespace DuckGame;

public class SubBackgroundOffice : SubBackgroundTile
{
    public SubBackgroundOffice(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("officeSubBackground", 32, 32, calculateTransparency: true);
        _opacityFromGraphic = true;
        Center = new Vector2(24, 16);
        collisionSize = new Vector2(32);
        collisionOffset = new Vector2(-16);
    }
}
