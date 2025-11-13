namespace DuckGame;

[EditorGroup("Arcade|Cameras", EditorItemType.ArcadeNew)]
[BaggedProperty("previewPriority", true)]
internal class CameraMover : Thing
{
	public EditorProperty<float> SpeedX = new EditorProperty<float>(0f, null, -10f, 10f, 0.25f);

	public EditorProperty<float> SpeedY = new EditorProperty<float>(0f, null, -10f, 10f, 0.25f);

	public EditorProperty<float> MoveDelay = new EditorProperty<float>(0f, null, 0f, 120f, 0.25f);

	public CameraMover(float xPos, float yPos)
		: base(xPos, yPos)
	{
		graphic = new Sprite("cameraMover");
		center = new Vec2(8f, 8f);
		collisionSize = new Vec2(8f, 8f);
		collisionOffset = new Vec2(-4f, -4f);
	}

	public override void Initialize()
	{
		if (!(Level.current is Editor))
		{
			base.alpha = 0f;
		}
		base.Initialize();
	}
}
