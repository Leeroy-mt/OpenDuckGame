namespace DuckGame;

[EditorGroup("Background|Parallax")]
public class OfficeBackground : BackgroundUpdater
{
    public OfficeBackground(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("backgroundIcons", 16, 16)
        {
            frame = 1
        };
        center = new Vec2(8f, 8f);
        _collisionSize = new Vec2(16f, 16f);
        _collisionOffset = new Vec2(-8f, -8f);
        base.depth = 0.9f;
        base.layer = Layer.Foreground;
        _visibleInGame = false;
        _editorName = "Office BG";
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            backgroundColor = new Color(25, 38, 41);
            Level.current.backgroundColor = backgroundColor;
            _parallax = new ParallaxBackground("background/office", 0f, 0f, 3);
            if (_parallax.definition == null)
            {
                float speed = 0.4f;
                _parallax.AddZone(0, 0f, 0f - speed, moving: true);
                _parallax.AddZone(1, 0f, 0f - speed, moving: true);
                _parallax.AddZone(2, 0f, 0f - speed, moving: true);
                _parallax.AddZone(3, 0.2f, 0f - speed, moving: true);
                _parallax.AddZone(4, 0.2f, 0f - speed, moving: true);
                _parallax.AddZone(5, 0.4f, 0f - speed, moving: true);
                _parallax.AddZone(6, 0.8f, speed);
                _parallax.AddZone(7, 0.8f, speed);
                _parallax.AddZone(8, 0.8f, speed);
                _parallax.AddZone(9, 0.8f, speed);
                Sprite s = new Sprite("background/officeBuilding01");
                s.depth = -0.9f;
                s.position = new Vec2(100f, 100f);
                _parallax.AddZoneSprite(s, 15, 0.6f, speed);
                s = new Sprite("background/officeBuilding01Porch");
                s.depth = -0.9f;
                s.position = new Vec2(84f, 160f);
                _parallax.AddZoneSprite(s, 16, 0.6f, speed);
                s = new Sprite("background/officeBuilding02");
                s.depth = -0.9f;
                s.position = new Vec2(300f, 120f);
                _parallax.AddZoneSprite(s, 17, 0.6f, speed);
                _parallax.AddZone(19, 0.6f, speed);
                _parallax.AddZone(20, 0.6f, speed);
                _parallax.AddZone(21, 0.6f, speed);
                _parallax.AddZone(22, 0.6f, speed);
                _parallax.AddZone(23, 0.6f, speed);
                _parallax.AddZone(24, 0.5f, speed);
                _parallax.AddZone(25, 0.4f, speed);
                _parallax.AddZone(26, 0.3f, speed);
                _parallax.AddZone(27, 0.2f, speed);
                _parallax.AddZone(28, 0.1f, speed);
                _parallax.AddZone(29, 0f, speed);
            }
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
