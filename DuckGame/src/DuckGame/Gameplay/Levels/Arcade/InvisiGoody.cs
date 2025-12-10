namespace DuckGame;

[EditorGroup("Special|Goodies", EditorItemType.Arcade)]
[BaggedProperty("isOnlineCapable", false)]
public class InvisiGoody : Goody
{
    public EditorProperty<bool> valid;

    public EditorProperty<bool> sound;

    public EditorProperty<int> size;

    public override void EditorPropertyChanged(object property)
    {
        UpdateHeight();
        base.sequence.isValid = valid.value;
        if (sound.value)
        {
            collectSound = "goody";
        }
        else
        {
            collectSound = "";
        }
    }

    public void UpdateHeight()
    {
        float s = size.value;
        center = new Vec2(8f, 8f);
        collisionSize = new Vec2(s * 16f);
        collisionOffset = new Vec2((0f - s * 16f) / 2f);
        base.scale = new Vec2(s);
    }

    public InvisiGoody(float xpos, float ypos)
        : base(xpos, ypos, new Sprite("swirl"))
    {
        _visibleInGame = false;
        base.sequence.isValid = false;
        size = new EditorProperty<int>(1, this, 1f, 16f, 1f);
        valid = new EditorProperty<bool>(val: false, this);
        sound = new EditorProperty<bool>(val: false, this);
    }
}
