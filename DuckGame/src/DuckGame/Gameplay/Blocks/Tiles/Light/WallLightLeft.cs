using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Arcade", EditorItemType.Lighting)]
[BaggedProperty("isInDemo", true)]
public class WallLightLeft : Thing
{
	private PointLight _light;

	private SpriteThing _shade;

	private List<LightOccluder> _occluders = new List<LightOccluder>();

	public WallLightLeft(float xpos, float ypos)
		: base(xpos, ypos)
	{
		graphic = new Sprite("wallLight");
		center = new Vec2(8f, 8f);
		_collisionSize = new Vec2(5f, 16f);
		_collisionOffset = new Vec2(-7f, -8f);
		base.depth = 0.9f;
		base.hugWalls = WallHug.Left;
		base.layer = Layer.Game;
	}

	public override void Initialize()
	{
		if (!(Level.current is Editor))
		{
			_occluders.Add(new LightOccluder(base.topLeft + new Vec2(-2f, 0f), base.topRight, new Color(1f, 0.8f, 0.8f)));
			_occluders.Add(new LightOccluder(base.bottomLeft + new Vec2(-2f, 0f), base.bottomRight, new Color(1f, 0.8f, 0.8f)));
			_light = new PointLight(base.x - 5f, base.y, new Color(255, 255, 190), 100f, _occluders);
			Level.Add(_light);
			_shade = new SpriteThing(base.x, base.y, new Sprite("wallLight"));
			_shade.center = center;
			_shade.layer = Layer.Foreground;
			Level.Add(_shade);
		}
	}

	public override void Update()
	{
		_light.visible = visible;
		base.Update();
	}

	public override void Draw()
	{
		base.Draw();
	}
}
