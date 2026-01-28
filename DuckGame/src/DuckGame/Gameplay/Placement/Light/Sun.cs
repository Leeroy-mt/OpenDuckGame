namespace DuckGame;

[EditorGroup("Details|Lights", EditorItemType.Lighting)]
[BaggedProperty("isInDemo", true)]
public class Sun : Thing
{
    public Sun(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("officeLight");
        Center = new Vec2(16f, 3f);
        _collisionSize = new Vec2(30f, 6f);
        _collisionOffset = new Vec2(-15f, -3f);
        base.Depth = 0.9f;
        base.hugWalls = WallHug.Ceiling;
        base.layer = Layer.Game;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            Level.Add(new SunLight(base.X, base.Y - 1f, new Color(255, 255, 255), 100f));
            Level.Remove(this);
        }
    }
}
