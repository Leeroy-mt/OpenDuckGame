using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Details|Terrain")]
public class WaterFallEdge : Thing
{
    public WaterFallEdge(float xpos, float ypos)
        : base(xpos, ypos)
    {
        SpriteMap flow = new SpriteMap("waterFallEdge", 16, 16);
        graphic = flow;
        Center = new Vector2(8f, 8f);
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
