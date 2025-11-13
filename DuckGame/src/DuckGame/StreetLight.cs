using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details|Lights", EditorItemType.Normal)]
[BaggedProperty("isInDemo", true)]
public class StreetLight : Thing
{
	private SpriteThing _shade;

	private List<LightOccluder> _occluders = new List<LightOccluder>();

	public StreetLight(float xpos, float ypos)
		: base(xpos, ypos)
	{
		graphic = new Sprite("streetLight");
		center = new Vec2(6f, 54f);
		_collisionSize = new Vec2(8f, 8f);
		_collisionOffset = new Vec2(-4f, -2f);
		base.depth = 0.9f;
		base.hugWalls = WallHug.Floor;
		base.layer = Layer.Game;
	}

	public override void Initialize()
	{
		if (!(Level.current is Editor))
		{
			if (flipHorizontal)
			{
				Vec2 lightPos = new Vec2(base.x - 16f, base.y - 32f - 20f);
				_occluders.Add(new LightOccluder(lightPos + new Vec2(8f, 5f), lightPos + new Vec2(-1f, -4f), new Color(0.4f, 0.4f, 0.4f)));
				_occluders.Add(new LightOccluder(lightPos + new Vec2(1f, -4f), lightPos + new Vec2(-8f, 5f), new Color(0.4f, 0.4f, 0.4f)));
				Level.Add(new PointLight(lightPos.x, lightPos.y + 1f, new Color(247, 198, 120), 200f, _occluders));
			}
			else
			{
				Vec2 lightPos2 = new Vec2(base.x + 16f, base.y - 32f - 20f);
				_occluders.Add(new LightOccluder(lightPos2 + new Vec2(-8f, 5f), lightPos2 + new Vec2(1f, -4f), new Color(0.4f, 0.4f, 0.4f)));
				_occluders.Add(new LightOccluder(lightPos2 + new Vec2(-1f, -4f), lightPos2 + new Vec2(8f, 5f), new Color(0.4f, 0.4f, 0.4f)));
				Level.Add(new PointLight(lightPos2.x, lightPos2.y + 1f, new Color(247, 198, 120), 200f, _occluders));
			}
		}
	}

	public override void Draw()
	{
		graphic.flipH = flipHorizontal;
		base.Draw();
	}
}
