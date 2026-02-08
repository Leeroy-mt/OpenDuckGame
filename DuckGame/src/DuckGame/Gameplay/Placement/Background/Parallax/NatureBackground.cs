using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Background|Parallax")]
[BaggedProperty("isInDemo", true)]
[BaggedProperty("previewPriority", true)]
public class NatureBackground : BackgroundUpdater
{
    public NatureBackground(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("backgroundIcons", 16, 16)
        {
            frame = 0
        };
        Center = new Vector2(8f, 8f);
        _collisionSize = new Vector2(16f, 16f);
        _collisionOffset = new Vector2(-8f, -8f);
        base.Depth = 0.9f;
        base.layer = Layer.Foreground;
        _visibleInGame = false;
        _editorName = "Nature BG";
    }

    public override void Initialize()
    {
        if (Level.current != null && !(Level.current is Editor))
        {
            backgroundColor = new Color(129, 182, 218);
            Level.current.backgroundColor = backgroundColor;
            _parallax = new ParallaxBackground("background/forest5", 0f, 0f, 3);
            float speed = 0.4f;
            Sprite s = new Sprite("background/cloud1");
            s.Depth = -0.9f;
            s.Position = new Vector2(50f, 60f);
            _parallax.AddZoneSprite(s, 6, 0.72f, speed, moving: true);
            s = new Sprite("background/cloud4");
            s.Depth = -0.9f;
            s.Position = new Vector2(200f, 95f);
            _parallax.AddZoneSprite(s, 5, 0.72f, speed, moving: true);
            s = new Sprite("background/cloud2");
            s.Depth = -0.9f;
            s.Position = new Vector2(170f, 72f);
            _parallax.AddZoneSprite(s, 8, 0.82f, speed, moving: true);
            s = new Sprite("background/cloud5");
            s.Depth = -0.9f;
            s.Position = new Vector2(30f, 45f);
            _parallax.AddZoneSprite(s, 4, 0.82f, speed, moving: true);
            s = new Sprite("background/cloud3");
            s.Depth = -0.9f;
            s.Position = new Vector2(150f, 30f);
            _parallax.AddZoneSprite(s, 7, 0.91f, speed, moving: true);
            int zoneOffset = 1;
            float cloudSpeed = 0.1f;
            _parallax.AddZone(0, 0.75f, cloudSpeed);
            _parallax.AddZone(1, 0.8f, cloudSpeed);
            _parallax.AddZone(2, 0.8f, cloudSpeed);
            _parallax.AddZone(3, 0.8f, cloudSpeed);
            _parallax.AddZone(4, 0.85f, cloudSpeed);
            _parallax.AddZone(12 + zoneOffset, 0.65f, speed);
            _parallax.AddZone(13 + zoneOffset, 0.65f, speed);
            _parallax.AddZone(14 + zoneOffset, 0.65f, speed);
            _parallax.AddZone(15 + zoneOffset, 0.6f, speed);
            _parallax.AddZone(16 + zoneOffset, 0.6f, speed);
            _parallax.AddZone(17 + zoneOffset, 0.6f, speed);
            _parallax.AddZone(18 + zoneOffset, 0.6f, speed);
            _parallax.AddZone(19 + zoneOffset, 0.6f, speed);
            _parallax.AddZone(20 + zoneOffset, 0.6f, speed);
            _parallax.AddZone(21 + zoneOffset, 0.6f, speed);
            _parallax.AddZone(22 + zoneOffset, 0.6f, speed);
            _parallax.AddZone(23 + zoneOffset, 0.55f, speed);
            _parallax.AddZone(24 + zoneOffset, 0.5f, speed);
            _parallax.AddZone(25 + zoneOffset, 0.45f, speed);
            _parallax.AddZone(26 + zoneOffset, 0.4f, speed);
            _parallax.AddZone(27 + zoneOffset, 0.35f, speed);
            _parallax.AddZone(28 + zoneOffset, 0.3f, speed);
            _parallax.AddZone(29 + zoneOffset, 0.25f, speed);
            Level.Add(_parallax);
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
