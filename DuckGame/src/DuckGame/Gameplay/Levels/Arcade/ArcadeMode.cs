using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Special", EditorItemType.Arcade)]
[BaggedProperty("isOnlineCapable", false)]
public class ArcadeMode : Thing
{
    public ArcadeMode()
    {
        graphic = new Sprite("arcadeIcon");
        Center = new Vector2(8f, 8f);
        _collisionSize = new Vector2(16f, 16f);
        _collisionOffset = new Vector2(-8f, -8f);
        base.Depth = 0.9f;
        base.layer = Layer.Foreground;
        _visibleInGame = false;
        _editorName = "Arcade";
        _canFlip = false;
        _canHaveChance = false;
    }

    public override void Initialize()
    {
    }

    public override void Update()
    {
    }
}
