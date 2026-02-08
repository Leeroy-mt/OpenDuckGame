using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Special", EditorItemType.Arcade)]
[BaggedProperty("isOnlineCapable", false)]
public class EnableWallJump : Thing
{
    public EnableWallJump()
    {
        graphic = new Sprite("swirl");
        Center = new Vector2(8f, 8f);
        collisionSize = new Vector2(16f, 16f);
        collisionOffset = new Vector2(-8f, -8f);
        _canFlip = false;
        _visibleInGame = false;
    }
}
