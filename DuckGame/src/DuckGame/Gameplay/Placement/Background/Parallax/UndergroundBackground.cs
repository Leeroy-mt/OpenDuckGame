namespace DuckGame;

[EditorGroup("Background|Parallax")]
public class UndergroundBackground : BackgroundUpdater
{
    private float _speedMult;

    private bool _moving;

    private UndergroundRocksBackground _undergroundRocks;

    private UndergroundSkyBackground _skyline;

    public UndergroundBackground(float xpos, float ypos, bool moving = false, float speedMult = 1f)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("backgroundIcons", 16, 16)
        {
            frame = 4
        };
        Center = new Vec2(8f, 8f);
        _collisionSize = new Vec2(16f, 16f);
        _collisionOffset = new Vec2(-8f, -8f);
        base.Depth = 0.9f;
        base.layer = Layer.Foreground;
        _visibleInGame = false;
        _speedMult = speedMult;
        _moving = moving;
        _editorName = "Bunker BG";
        _yParallax = false;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            backgroundColor = new Color(0, 0, 0);
            Level.current.backgroundColor = backgroundColor;
            _parallax = new ParallaxBackground("background/underground", 0f, 0f, 5);
            float speed = 0.9f * _speedMult;
            _parallax.AddZone(11, 1f, speed);
            _parallax.AddZone(12, 1f, speed);
            _parallax.AddZone(13, 1f, speed);
            _parallax.AddZone(14, 0.98f, speed);
            _parallax.AddZone(15, 0.97f, speed);
            _parallax.AddZone(16, 0.75f, speed);
            _parallax.AddZone(17, 0.75f, speed);
            _parallax.AddZone(18, 0.75f, speed);
            _parallax.AddZone(19, 0.75f, speed);
            _parallax.AddZone(20, 0.75f, speed);
            Level.Add(_parallax);
            _parallax.X -= 340f;
            _parallax.restrictBottom = false;
            _undergroundRocks = new UndergroundRocksBackground(base.X, base.Y);
            Level.Add(_undergroundRocks);
            _skyline = new UndergroundSkyBackground(base.X, base.Y);
            Level.Add(_skyline);
        }
    }

    public override void Update()
    {
        Vec2 vec = new Vec2(0f, 10f);
        Matrix m = Level.current.camera.getMatrix();
        int yScissor = (int)Vec2.Transform(vec, m).Y;
        if (yScissor < 0)
        {
            yScissor = 0;
        }
        if (yScissor > Resolution.current.y)
        {
            yScissor = Resolution.current.y;
        }
        float yMul = (float)Resolution.current.y / (float)Graphics.height;
        Vec2 wallScissor = BackgroundUpdater.GetWallScissor();
        _undergroundRocks.scissor = new Rectangle((int)wallScissor.X, (float)yScissor * yMul, (int)wallScissor.Y, Resolution.current.y - yScissor);
        Vec2 vec2 = new Vec2(0f, -10f);
        m = Level.current.camera.getMatrix();
        yScissor = (int)(Vec2.Transform(vec2, m).Y * yMul);
        if (yScissor < 0)
        {
            yScissor = 0;
        }
        if ((float)yScissor > Resolution.size.Y)
        {
            yScissor = (int)Resolution.size.Y;
        }
        _skyline.scissor = new Rectangle((int)wallScissor.X, 0f, (int)wallScissor.Y, yScissor);
        base.Update();
    }

    public override void Terminate()
    {
        Level.Remove(_parallax);
    }
}
