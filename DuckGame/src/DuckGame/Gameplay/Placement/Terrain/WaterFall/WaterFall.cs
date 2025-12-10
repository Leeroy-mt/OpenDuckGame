namespace DuckGame;

[EditorGroup("Details|Terrain")]
public class WaterFall : Thing
{
    public WaterFall(float xpos, float ypos)
        : base(xpos, ypos)
    {
        SpriteMap flow = new SpriteMap("waterFall", 16, 32);
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
