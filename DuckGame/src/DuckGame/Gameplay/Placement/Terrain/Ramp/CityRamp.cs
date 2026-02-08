using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Details|Terrain")]
public class CityRamp : IceWedge
{
    public CityRamp(float xpos, float ypos, int dir)
        : base(xpos, ypos, dir)
    {
        _canFlipVert = true;
        graphic = new SpriteMap("cityWedge", 17, 17);
        base.hugWalls = WallHug.Left | WallHug.Right | WallHug.Floor;
        Center = new Vector2(8f, 14f);
        collisionSize = new Vector2(14f, 8f);
        collisionOffset = new Vector2(-7f, -6f);
        _editorName = "Ramp";
        base.Depth = -0.9f;
    }
}
