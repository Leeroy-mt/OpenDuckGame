namespace DuckGame;

[EditorGroup("Details|Signs")]
public class UpSign : Thing
{
    private SpriteMap _sprite;

    public UpSign(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("upSign", 32, 32);
        graphic = _sprite;
        Center = new Vec2(16f, 24f);
        _collisionSize = new Vec2(16f, 16f);
        _collisionOffset = new Vec2(-8f, -8f);
        base.Depth = -0.5f;
        _editorName = "Up Sign";
        base.hugWalls = WallHug.Floor;
    }

    public override void Draw()
    {
        _sprite.frame = ((offDir <= 0) ? 1 : 0);
        base.Draw();
    }
}
