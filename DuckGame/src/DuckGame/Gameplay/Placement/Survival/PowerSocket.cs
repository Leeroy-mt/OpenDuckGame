using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("survival")]
[BaggedProperty("isOnlineCapable", false)]
public class PowerSocket : Thing
{
    public PowerSocket(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("survival/cryoSocket");
        Center = new Vector2(8f, 8f);
        _collisionSize = new Vector2(14f, 14f);
        _collisionOffset = new Vector2(-7f, -7f);
        base.Depth = -0.9f;
    }
}
