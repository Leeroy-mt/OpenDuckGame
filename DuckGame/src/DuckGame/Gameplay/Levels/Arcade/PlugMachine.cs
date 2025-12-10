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

    public override Vec2 cameraPosition => position + new Vec2(-16f, 0f);

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
        base.depth = -0.5f;
        center = new Vec2(_sprite.width / 2, _sprite.h / 2);
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
            _dust = new DustSparkleEffect(base.x - 34f, base.y - 40f, wide: false, lit: false);
            Level.Add(_dust);
            _dust.depth = base.depth - 2;
            _lighting = new ArcadeScreen(base.x, base.y);
            Level.Add(_lighting);
        }
    }

    public override void Update()
    {
        Vec2 testPos = position + new Vec2(-20f, 0f);
        Duck d = Level.Nearest<Duck>(testPos);
        if (d != null)
        {
            if (d.grounded && (d.position - testPos).length < 16f)
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
            _duckSprite.depth = base.depth + 16;
            Graphics.Draw(_duckSprite, base.x + offset.x, base.y + offset.y);
            _ledStrip.alpha = 1f;
            _ledStrip.depth = base.depth + 10;
            Graphics.Draw(_ledStrip, base.x - 16f, base.y + 9f);
            _ledStrip.alpha = 0.25f;
            _ledStrip.depth = base.depth + 10;
            Graphics.Draw(_ledStrip, base.x - 16f, base.y + 10f);
            _screen.depth = base.depth + 5;
            Graphics.Draw(_screen, base.x - 9f, base.y - 7f);
            _hoverSprite.alpha = Lerp.Float(_hoverSprite.alpha, _hoverFade, 0.05f);
            if (_hoverSprite.alpha > 0.01f)
            {
                _hoverSprite.depth = base.depth + 6;
                Graphics.Draw(_hoverSprite, base.x + offset.x - 1f, base.y + offset.y - 1f);
            }
        }
        base.Draw();
    }
}
