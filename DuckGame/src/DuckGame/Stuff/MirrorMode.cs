using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Arcade", EditorItemType.ArcadeNew)]
[BaggedProperty("previewPriority", true)]
internal class MirrorMode : Thing
{
    public enum Setting
    {
        Horizontal,
        Vertical,
        Both
    }

    private SpriteMap _sprite;

    public EditorProperty<Setting> mode;

    public override void EditorPropertyChanged(object property)
    {
        _sprite.frame = (int)mode.value;
        base.EditorPropertyChanged(property);
    }

    public MirrorMode(float pX, float pY)
    {
        _sprite = new SpriteMap("mirrorMode", 16, 16);
        graphic = _sprite;
        mode = new EditorProperty<Setting>(Setting.Vertical, this);
        mode.value = Setting.Vertical;
        collisionSize = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-4f, -4f);
        Center = new Vector2(8f, 8f);
        base.editorOffset = new Vector2(8f, 8f);
        _visibleInGame = false;
    }
}
