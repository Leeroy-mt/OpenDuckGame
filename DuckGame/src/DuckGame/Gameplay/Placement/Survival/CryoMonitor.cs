namespace DuckGame;

[EditorGroup("survival")]
[BaggedProperty("isOnlineCapable", false)]
public class CryoMonitor : Thing
{
    public CryoMonitor(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new("survival/cryoMonitor");
        center = new(graphic.w / 2, graphic.h / 2);
        _collisionSize = new(32);
        _collisionOffset = new(-16);
        depth = 0.9f;
        layer = Layer.Foreground;
    }
}
