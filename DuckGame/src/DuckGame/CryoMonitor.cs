namespace DuckGame;

[EditorGroup("survival")]
[BaggedProperty("isOnlineCapable", false)]
public class CryoMonitor : Thing
{
	public CryoMonitor(float xpos, float ypos)
		: base(xpos, ypos)
	{
		graphic = new Sprite("survival/cryoMonitor");
		center = new Vec2(graphic.w / 2, graphic.h / 2);
		_collisionSize = new Vec2(32f, 32f);
		_collisionOffset = new Vec2(-16f, -16f);
		base.depth = 0.9f;
		base.layer = Layer.Foreground;
	}
}
