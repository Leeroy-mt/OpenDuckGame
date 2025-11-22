namespace DuckGame;

[EditorGroup("Background|Parallax")]
[BaggedProperty("isInDemo", false)]
public class SnowBackground : BackgroundUpdater
{
	public SnowBackground(float xpos, float ypos)
		: base(xpos, ypos)
	{
		graphic = new SpriteMap("backgroundIcons", 16, 16)
		{
			frame = 7
		};
		center = new Vec2(8f, 8f);
		_collisionSize = new Vec2(16f, 16f);
		_collisionOffset = new Vec2(-8f, -8f);
		base.depth = 0.9f;
		base.layer = Layer.Foreground;
		_visibleInGame = false;
		_editorName = "Snow BG";
	}

	public override void Initialize()
	{
		if (!(Level.current is Editor))
		{
			backgroundColor = new Color(148, 178, 210);
			Level.current.backgroundColor = backgroundColor;
			_parallax = new ParallaxBackground("background/snowSky", 0f, 0f, 3);
			float speed = 0.2f;
			_parallax.AddZone(0, 0.9f, speed);
			_parallax.AddZone(1, 0.9f, speed);
			_parallax.AddZone(2, 0.9f, speed);
			_parallax.AddZone(3, 0.9f, speed);
			_parallax.AddZone(4, 0.9f, speed);
			_parallax.AddZone(5, 0.9f, speed);
			_parallax.AddZone(6, 0.9f, speed);
			_parallax.AddZone(7, 0.9f, speed);
			_parallax.AddZone(8, 0.9f, speed);
			_parallax.AddZone(9, 0.9f, speed);
			_parallax.AddZone(10, 0.8f, speed);
			_parallax.AddZone(11, 0.7f, speed);
			_parallax.AddZone(12, 0.6f, speed);
			_parallax.AddZone(13, 0.5f, speed);
			_parallax.AddZone(14, 0.4f, speed);
			_parallax.AddZone(15, 0.3f, speed);
			Sprite s = null;
			Vec2 offzet = new Vec2(0f, -12f);
			s = new Sprite("background/bigBerg1_reflection");
			s.depth = -0.9f;
			s.position = new Vec2(-30f, 113f) + offzet;
			_parallax.AddZoneSprite(s, 12, 0f, 0f, moving: true);
			s = new Sprite("background/bigBerg1");
			s.depth = -0.8f;
			s.position = new Vec2(-31f, 50f) + offzet;
			_parallax.AddZoneSprite(s, 12, 0f, 0f, moving: true);
			s = new Sprite("background/bigBerg2_reflection");
			s.depth = -0.9f;
			s.position = new Vec2(210f, 108f) + offzet;
			_parallax.AddZoneSprite(s, 12, 0f, 0f, moving: true);
			s = new Sprite("background/bigBerg2");
			s.depth = -0.8f;
			s.position = new Vec2(211f, 52f) + offzet;
			_parallax.AddZoneSprite(s, 12, 0f, 0f, moving: true);
			s = new Sprite("background/berg1_reflection");
			s.depth = -0.9f;
			s.position = new Vec2(119f, 131f) + offzet;
			_parallax.AddZoneSprite(s, 13, 0f, 0f, moving: true);
			s = new Sprite("background/berg1");
			s.depth = -0.8f;
			s.position = new Vec2(121f, 114f) + offzet;
			_parallax.AddZoneSprite(s, 13, 0f, 0f, moving: true);
			offzet = new Vec2(-30f, -20f);
			s = new Sprite("background/berg2_reflection");
			s.depth = -0.9f;
			s.position = new Vec2(69f, 153f) + offzet;
			_parallax.AddZoneSprite(s, 14, 0f, 0f, moving: true);
			s = new Sprite("background/berg2");
			s.depth = -0.8f;
			s.position = new Vec2(71f, 154f) + offzet;
			_parallax.AddZoneSprite(s, 14, 0f, 0f, moving: true);
			offzet = new Vec2(200f, 2f);
			s = new Sprite("background/berg3_reflection");
			s.depth = -0.9f;
			s.position = new Vec2(70f, 153f) + offzet;
			_parallax.AddZoneSprite(s, 15, 0f, 0f, moving: true);
			s = new Sprite("background/berg3");
			s.depth = -0.8f;
			s.position = new Vec2(71f, 154f) + offzet;
			_parallax.AddZoneSprite(s, 15, 0f, 0f, moving: true);
			Level.Add(_parallax);
			if (base.level != null)
			{
				base.level.cold = true;
			}
		}
	}

	public override void Update()
	{
		base.Update();
	}

	public override void Terminate()
	{
		Level.Remove(_parallax);
	}
}
