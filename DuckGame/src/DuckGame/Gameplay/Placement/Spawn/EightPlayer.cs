using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Spawns")]
public class EightPlayer : Thing
{
    public EditorProperty<bool> eightPlayerOnly = new EditorProperty<bool>(val: false);

    public EightPlayer(float x, float y)
        : base(x, y)
    {
        _editorName = "Eight Player";
        graphic = new Sprite("eight_player");
        Center = new Vector2(8f, 8f);
        base.Depth = 0.55f;
        _visibleInGame = false;
        editorTooltip = "Place in a level to make it an 8 Player map!";
        eightPlayerOnly._tooltip = "If true, this map will not appear when less than 5 players are present in the game.";
        solid = false;
        _collisionSize = new Vector2(0f, 0f);
    }
}
