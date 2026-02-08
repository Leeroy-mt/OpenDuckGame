using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Details|Signs")]
public class MallardBillboard : MaterialThing, IPlatform
{
    private SpriteMap _sprite;

    public MallardBillboard(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("billboard", 217, 126);
        graphic = _sprite;
        Center = new Vector2(126f, 77f);
        _collisionSize = new Vector2(167f, 6f);
        _collisionOffset = new Vector2(-84f, -2f);
        base.editorOffset = new Vector2(0f, 40f);
        base.Depth = -0.5f;
        _editorName = "Mallard Billboard";
        thickness = 0.2f;
        base.hugWalls = WallHug.Floor;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Draw()
    {
        _sprite.frame = ((offDir <= 0) ? 1 : 0);
        base.Draw();
    }
}
