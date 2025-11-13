namespace DuckGame;

[EditorGroup("survival")]
[BaggedProperty("isOnlineCapable", false)]
public class CryoTube : Thing
{
	private CryoPlug _plug;

	public CryoTube(float xpos, float ypos)
		: base(xpos, ypos)
	{
		graphic = new Sprite("survival/cryoTube");
		center = new Vec2(16f, 15f);
		_collisionSize = new Vec2(18f, 32f);
		_collisionOffset = new Vec2(-9f, -16f);
		base.depth = 0.9f;
		base.hugWalls = WallHug.Floor;
	}

	public override void Initialize()
	{
		_plug = new CryoPlug(base.x - 20f, base.y);
		Level.Add(_plug);
		_plug.AttachTo(this);
	}

	public override void Terminate()
	{
		Level.Remove(_plug);
	}
}
