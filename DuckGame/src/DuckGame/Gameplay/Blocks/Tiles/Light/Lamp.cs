using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Lights", EditorItemType.Lighting)]
[BaggedProperty("isInDemo", true)]
public class Lamp : Thing
{
	private SpriteThing _shade;

	private List<LightOccluder> _occluders = new List<LightOccluder>();

	public Lamp(float xpos, float ypos)
		: base(xpos, ypos)
	{
		graphic = new Sprite("lamp");
		center = new Vec2(7f, 28f);
		_collisionSize = new Vec2(16f, 16f);
		_collisionOffset = new Vec2(-8f, -15f);
		base.depth = 0.9f;
		base.hugWalls = WallHug.Floor;
		base.layer = Layer.Game;
	}

	public override void Initialize()
	{
		if (!(Level.current is Editor))
		{
			_occluders.Add(new LightOccluder(position + new Vec2(-7f, -16f), position + new Vec2(-3f, -28f), new Color(1f, 0.7f, 0.7f)));
			_occluders.Add(new LightOccluder(position + new Vec2(7f, -16f), position + new Vec2(3f, -28f), new Color(1f, 0.7f, 0.7f)));
			Level.Add(new PointLight(base.x, base.y - 24f, new Color(255, 255, 180), 100f, _occluders));
			_shade = new SpriteThing(base.x, base.y, new Sprite("lampShade"));
			_shade.center = center;
			_shade.layer = Layer.Foreground;
			Level.Add(_shade);
		}
	}
}
