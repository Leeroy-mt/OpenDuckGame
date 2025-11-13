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
		base.depth = -0.5f;
		_outline = new Sprite("arcade/prizeCounterOutline");
		_outline.depth = base.depth + 1;
		_outline.CenterOrigin();
		center = new Vec2(_sprite.width / 2, _sprite.h / 2);
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
			_dust = new DustSparkleEffect(base.x - 54f, base.y - 40f, wide: true, lit: true);
			Level.Add(_dust);
			_dust.depth = base.depth - 2;
			_lighting = new ArcadeTableLight(base.x, base.y - 43f);
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
		Duck d = Level.Nearest<Duck>(base.x, base.y);
		if (d != null)
		{
			if (d.grounded && (d.position - position).length < 20f)
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
			d = Level.Nearest<Duck>(base.x + hoverOffset.x, base.y + hoverOffset.y);
			if (d != null)
			{
				if (d.grounded && (d.position - (position + hoverOffset)).length < 20f)
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
		_light.depth = base.depth - 9;
		_prizes.depth = base.depth - 7;
		Graphics.Draw(_prizes, base.x - 28f, base.y - 33f);
		if (_unlocked)
		{
			graphic.color = Color.White;
		}
		else
		{
			graphic.color = Color.Black;
		}
		Graphics.Draw(_light, base.x - 53f, base.y - 40f);
		if (Chancy.atCounter && !(Level.current is Editor))
		{
			Vec2 offset = new Vec2(32f, -15f);
			Chancy.body.flipH = true;
			if (_hasEligibleChallenges)
			{
				offset = new Vec2(42f, -10f);
				Chancy.body.flipH = false;
			}
			Chancy.body.depth = base.depth - 6;
			Graphics.Draw(Chancy.body, base.x + offset.x, base.y + offset.y);
			if (hoverChancyChallenge)
			{
				_hoverSprite.alpha = Lerp.Float(_hoverSprite.alpha, 1f, 0.05f);
			}
			else
			{
				_hoverSprite.alpha = Lerp.Float(_hoverSprite.alpha, 0f, 0.05f);
			}
			if (_hoverSprite.alpha > 0.01f)
			{
				_hoverSprite.depth = 0f;
				_hoverSprite.flipH = Chancy.body.flipH;
				if (_hoverSprite.flipH)
				{
					Graphics.Draw(_hoverSprite, base.x + offset.x + 1f, base.y + offset.y - 1f);
				}
				else
				{
					Graphics.Draw(_hoverSprite, base.x + offset.x - 1f, base.y + offset.y - 1f);
				}
			}
		}
		base.Draw();
		if (_hoverFade > 0f)
		{
			_outline.alpha = _hoverFade;
			Graphics.Draw(_outline, base.x + 1f, base.y);
		}
	}
}
