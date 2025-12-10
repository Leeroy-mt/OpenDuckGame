namespace DuckGame;

[EditorGroup("survival")]
[BaggedProperty("isOnlineCapable", false)]
public class CryoTube : Thing
{
    private CryoPlug _plug;

    public CryoTube(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new("survival/cryoTube");
        center = new(16, 15);
        _collisionSize = new(18, 32);
        _collisionOffset = new(-9, -16);
        depth = 0.9f;
        hugWalls = WallHug.Floor;
    }

    public override void Initialize()
    {
        _plug = new(x - 20, y);
        Level.Add(_plug);
        _plug.AttachTo(this);
    }

    public override void Terminate() =>
        Level.Remove(_plug);
}
