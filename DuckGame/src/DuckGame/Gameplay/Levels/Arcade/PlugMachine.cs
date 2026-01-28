namespace DuckGame;

[EditorGroup("Special|Arcade", EditorItemType.Arcade)]
[BaggedProperty("isOnlineCapable", false)]
public class PlugMachine : Thing
{
    private SpriteMap _sprite;

    private float _hoverFade;

    private Sprite _hoverSprite;

    private Sprite _duckSprite;

    private SpriteMap _ledStrip;

    private SpriteMap _screen;

    public bool hover;

    private Thing _lighting;

    private DustSparkleEffect _dust;

    public override Vec2 cameraPosition => Position + new Vec2(-16f, 0f);

    public PlugMachine(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("arcade/plug_machine", 38, 36);
        _sprite.AddAnimation("idle", 0.5f, true, 0, 1, 2, 3);
        _sprite.SetAnimation("idle");
        _screen = new SpriteMap("arcade/plug_machine_monitor", 11, 8);
        _screen.AddAnimation("idle", 0.2f, true, 0, 1, 2);
        _screen.SetAnimation("idle");
        graphic = _sprite;
        base.Depth = -0.5f;
        Center = new Vec2(_sprite.width / 2, _sprite.h / 2);
        _collisionSize = new Vec2(16f, 15f);
        _collisionOffset = new Vec2(-8f, 2f);
        _hoverSprite = new Sprite("arcade/plug_hover");
        _duckSprite = new Sprite("arcade/plug_duck");
        _ledStrip = new SpriteMap("arcade/led_strip", 14, 1);
        _ledStrip.AddAnimation("idle", 0.3f, true, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13);
        _ledStrip.SetAnimation("idle");
        base.hugWalls = WallHug.Floor;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor) && base.level != null && !base.level.bareInitialize)
        {
            _dust = new DustSparkleEffect(base.X - 34f, base.Y - 40f, wide: false, lit: false);
            Level.Add(_dust);
            _dust.Depth = base.Depth - 2;
            _lighting = new ArcadeScreen(base.X, base.Y);
            Level.Add(_lighting);
        }
    }

    public override void Update()
    {
        Vec2 testPos = Position + new Vec2(-20f, 0f);
        Duck d = Level.Nearest<Duck>(testPos);
        if (d != null)
        {
            if (d.grounded && (d.Position - testPos).Length() < 16f)
            {
                _hoverFade = Lerp.Float(_hoverFade, 1f, 0.2f);
                hover = true;
            }
            else
            {
                _hoverFade = Lerp.Float(_hoverFade, 0f, 0.2f);
                hover = false;
            }
        }
        _dust.fade = 0.7f;
        DustSparkleEffect dust = _dust;
        bool flag = (_lighting.visible = visible);
        dust.visible = flag;
    }

    public override void Draw()
    {
        graphic.color = Color.White;
        if (!(Level.current is Editor))
        {
            Vec2 offset = new Vec2(-24f, -8f);
            _duckSprite.Depth = base.Depth + 16;
            Graphics.Draw(_duckSprite, base.X + offset.X, base.Y + offset.Y);
            _ledStrip.Alpha = 1f;
            _ledStrip.Depth = base.Depth + 10;
            Graphics.Draw(_ledStrip, base.X - 16f, base.Y + 9f);
            _ledStrip.Alpha = 0.25f;
            _ledStrip.Depth = base.Depth + 10;
            Graphics.Draw(_ledStrip, base.X - 16f, base.Y + 10f);
            _screen.Depth = base.Depth + 5;
            Graphics.Draw(_screen, base.X - 9f, base.Y - 7f);
            _hoverSprite.Alpha = Lerp.Float(_hoverSprite.Alpha, _hoverFade, 0.05f);
            if (_hoverSprite.Alpha > 0.01f)
            {
                _hoverSprite.Depth = base.Depth + 6;
                Graphics.Draw(_hoverSprite, base.X + offset.X - 1f, base.Y + offset.Y - 1f);
            }
        }
        base.Draw();
    }
}
