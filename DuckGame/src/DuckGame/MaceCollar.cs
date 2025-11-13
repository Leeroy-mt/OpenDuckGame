namespace DuckGame;

[EditorGroup("Equipment")]
[BaggedProperty("isOnlineCapable", false)]
public class MaceCollar : ChokeCollar
{
	public MaceCollar(float xpos, float ypos)
		: base(xpos, ypos)
	{
		editorTooltip = "A heavy ball & chain that can be swung with great force. For profit!";
	}

	public override void Initialize()
	{
		if (!(Level.current is Editor))
		{
			_ball = new WeightBall(base.x, base.y, this, this, isMace: true);
			ReturnItemToWorld(_ball);
			Level.Add(_ball);
		}
	}
}
