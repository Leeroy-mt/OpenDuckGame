namespace DuckGame;

[EditorGroup("Special|Arcade", EditorItemType.Arcade)]
[BaggedProperty("isOnlineCapable", false)]
public class PrizeTable : Thing
{
    private SpriteMap _sprite;

    private Sprite _outline;

    private float _hoverFade;

    private SpriteMap _light;

    private Sprite _fixture;

    private Sprite _prizes;

    private Sprite _hoverSprite;

    public bool hoverChancyChallenge;

    private DustSparkleEffect _dust;

    public bool hover;

    public bool _unlocked = true;

    private ArcadeTableLight _lighting;

    private bool _hasEligibleChallenges;

    public override bool visible
    {
        get
        {
            return base.visible;
        }
        set
        {
            base.visible = value;
            _dust.visible = base.visible;
        }
    }

    public PrizeTable(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("arcade/prizeCounter", 69, 30);
        graphic = _sprite;
        base.Depth = -0.5f;
        _outline = new Sprite("arcade/prizeCounterOutline");
        _outline.Depth = base.Depth + 1;
        _outline.CenterOrigin();
        Center = new Vec2(_sprite.width / 2, _sprite.h / 2);
        _collisionSize = new Vec2(16f, 15f);
        _collisionOffset = new Vec2(-8f, 0f);
        _light = new SpriteMap("arcade/prizeLights", 107, 55);
        _fixture = new Sprite("arcade/bigFixture");
        _prizes = new Sprite("arcade/prizes");
        _hoverSprite = new Sprite("arcade/chancyHover");
        base.hugWalls = WallHug.Floor;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            _dust = new DustSparkleEffect(base.X - 54f, base.Y - 40f, wide: true, lit: true);
            Level.Add(_dust);
            _dust.Depth = base.Depth - 2;
            _lighting = new ArcadeTableLight(base.X, base.Y - 43f);
            Level.Add(_lighting);
        }
    }

    public override void Update()
    {
        if (Profiles.active.Count == 0)
        {
            return;
        }
        _hasEligibleChallenges = Challenges.GetEligibleChancyChallenges(Profiles.active[0]).Count > 0;
        Duck d = Level.Nearest<Duck>(base.X, base.Y);
        if (d != null)
        {
            if (d.grounded && (d.Position - Position).Length() < 20f)
            {
                _hoverFade = Lerp.Float(_hoverFade, 1f, 0.1f);
                hover = true;
            }
            else
            {
                _hoverFade = Lerp.Float(_hoverFade, 0f, 0.1f);
                hover = false;
            }
        }
        if (_hasEligibleChallenges)
        {
            Vec2 hoverOffset = new Vec2(40f, 0f);
            d = Level.Nearest<Duck>(base.X + hoverOffset.X, base.Y + hoverOffset.Y);
            if (d != null)
            {
                if (d.grounded && (d.Position - (Position + hoverOffset)).Length() < 20f)
                {
                    hoverChancyChallenge = true;
                }
                else
                {
                    hoverChancyChallenge = false;
                }
            }
        }
        _dust.fade = 0.5f;
        _dust.visible = _unlocked && visible;
    }

    public override void Draw()
    {
        _light.Depth = base.Depth - 9;
        _prizes.Depth = base.Depth - 7;
        Graphics.Draw(_prizes, base.X - 28f, base.Y - 33f);
        if (_unlocked)
        {
            graphic.color = Color.White;
        }
        else
        {
            graphic.color = Color.Black;
        }
        Graphics.Draw(_light, base.X - 53f, base.Y - 40f);
        if (Chancy.atCounter && !(Level.current is Editor))
        {
            Vec2 offset = new Vec2(32f, -15f);
            Chancy.body.flipH = true;
            if (_hasEligibleChallenges)
            {
                offset = new Vec2(42f, -10f);
                Chancy.body.flipH = false;
            }
            Chancy.body.Depth = base.Depth - 6;
            Graphics.Draw(Chancy.body, base.X + offset.X, base.Y + offset.Y);
            if (hoverChancyChallenge)
            {
                _hoverSprite.Alpha = Lerp.Float(_hoverSprite.Alpha, 1f, 0.05f);
            }
            else
            {
                _hoverSprite.Alpha = Lerp.Float(_hoverSprite.Alpha, 0f, 0.05f);
            }
            if (_hoverSprite.Alpha > 0.01f)
            {
                _hoverSprite.Depth = 0f;
                _hoverSprite.flipH = Chancy.body.flipH;
                if (_hoverSprite.flipH)
                {
                    Graphics.Draw(_hoverSprite, base.X + offset.X + 1f, base.Y + offset.Y - 1f);
                }
                else
                {
                    Graphics.Draw(_hoverSprite, base.X + offset.X - 1f, base.Y + offset.Y - 1f);
                }
            }
        }
        base.Draw();
        if (_hoverFade > 0f)
        {
            _outline.Alpha = _hoverFade;
            Graphics.Draw(_outline, base.X + 1f, base.Y);
        }
    }
}
