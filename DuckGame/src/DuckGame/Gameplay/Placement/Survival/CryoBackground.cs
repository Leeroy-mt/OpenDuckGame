namespace DuckGame;

[EditorGroup("survival")]
[BaggedProperty("isOnlineCapable", false)]
public class CryoBackground : Thing
{
    public CryoBackground(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new("survival/cryoBackground");
        center = new(graphic.w / 2, graphic.h / 2);
        _collisionSize = new(32);
        _collisionOffset = new(-16);
        depth = 0.9f;
        layer = Layer.Background;
    }
}
