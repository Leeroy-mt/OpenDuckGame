using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Arcade", EditorItemType.Arcade)]
[BaggedProperty("isInDemo", true)]
public class ArcadeScreen : Thing
{
	private PointLight _light;

	private List<LightOccluder> _occluders = new List<LightOccluder>();

	public ArcadeScreen(float xpos, float ypos)
		: base(xpos, ypos)
	{
		center = new Vec2(5f, 4f);
		_collisionSize = new Vec2(16f, 24f);
		_collisionOffset = new Vec2(-8f, -22f);
		base.depth = 0.9f;
		base.hugWalls = WallHug.Ceiling;
		base.layer = Layer.Game;
	}

	public override void Initialize()
	{
		if (!(Level.current is Editor))
		{
			_occluders.Add(new LightOccluder(position + new Vec2(-7f, -8f), position + new Vec2(7f, -8f), new Color(0.7f, 0.7f, 0.7f)));
			_light = new PointLight(base.x + 1f, base.y - 7f, new Color(100, 130, 180), 30f, _occluders);
			Level.Add(_light);
		}
	}

	public override void Update()
	{
		_light.visible = visible;
		base.Update();
	}
}
