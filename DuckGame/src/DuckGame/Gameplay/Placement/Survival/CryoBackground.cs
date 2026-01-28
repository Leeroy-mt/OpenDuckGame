namespace DuckGame;

[EditorGroup("survival")]
[BaggedProperty("isOnlineCapable", false)]
public class CryoBackground : Thing
{
    public CryoBackground(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new("survival/cryoBackground");
        Center = new(graphic.w / 2, graphic.h / 2);
        _collisionSize = new(32);
        _collisionOffset = new(-16);
        Depth = 0.9f;
        layer = Layer.Background;
    }
}
