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
        Center = new(16, 15);
        _collisionSize = new(18, 32);
        _collisionOffset = new(-9, -16);
        Depth = 0.9f;
        hugWalls = WallHug.Floor;
    }

    public override void Initialize()
    {
        _plug = new(X - 20, Y);
        Level.Add(_plug);
        _plug.AttachTo(this);
    }

    public override void Terminate() =>
        Level.Remove(_plug);
}
