namespace DuckGame;

[EditorGroup("Details|Terrain")]
public class WaterFallEdge : Thing
{
    public WaterFallEdge(float xpos, float ypos)
        : base(xpos, ypos)
    {
        SpriteMap flow = new SpriteMap("waterFallEdge", 16, 16);
        graphic = flow;
        center = new Vec2(8f, 8f);
        _collisionSize = new Vec2(8f, 8f);
        _collisionOffset = new Vec2(-8f, -8f);
        base.layer = Layer.Foreground;
        base.depth = 0.9f;
        base.alpha = 0.8f;
    }

    public override void Draw()
    {
        (graphic as SpriteMap).frame = (int)((float)Graphics.frame / 3f % 4f);
        graphic.flipH = offDir <= 0;
        base.Draw();
    }
}
