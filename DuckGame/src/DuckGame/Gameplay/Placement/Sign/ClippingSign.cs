using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Details|Signs")]
public class ClippingSign : Thing
{
    public EditorProperty<int> style;

    private SpriteMap _sprite;

    public override void EditorPropertyChanged(object property)
    {
        (graphic as SpriteMap).frame = style.value;
    }

    public ClippingSign(float xpos, float ypos)
        : base(xpos, ypos)
    {
        style = new EditorProperty<int>(0, this, 0f, 2f, 1f);
        _sprite = new SpriteMap("noClippingSign", 32, 32);
        graphic = _sprite;
        Center = new Vector2(16f, 24f);
        _collisionSize = new Vector2(16f, 16f);
        _collisionOffset = new Vector2(-8f, -8f);
        base.Depth = -0.5f;
        _editorName = "No Clipping";
        editorTooltip = "I mean it!!";
        _canFlip = false;
        base.hugWalls = WallHug.Floor;
    }

    public override void Draw()
    {
        base.Draw();
    }
}
