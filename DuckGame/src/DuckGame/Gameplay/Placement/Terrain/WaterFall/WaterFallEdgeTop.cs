using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Details|Terrain")]
public class WaterFallEdgeTop : Thing
{
    public WaterFallEdgeTop(float xpos, float ypos)
        : base(xpos, ypos)
    {
        SpriteMap flow = new SpriteMap("waterFallEdgeTop", 16, 20);
        graphic = flow;
        Center = new Vector2(8f, 12f);
        _collisionSize = new Vector2(8f, 8f);
        _collisionOffset = new Vector2(-8f, -8f);
        base.layer = Layer.Foreground;
        base.Depth = 0.9f;
        base.Alpha = 0.8f;
    }

    public override void Draw()
    {
        (graphic as SpriteMap).frame = (int)((float)Graphics.frame / 3f % 4f);
        graphic.flipH = offDir <= 0;
        base.Draw();
    }
}
