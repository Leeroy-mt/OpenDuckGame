namespace DuckGame;

public class PointBoard : Thing
{
	private BitmapFont _font;

	private Sprite _scoreCard;

	private Team _team;

	private Thing _stick;

	public PointBoard(Thing rock, Team t)
		: base(rock.x + 24f, rock.y)
	{
		_scoreCard = new Sprite("rockThrow/scoreCard");
		_font = new BitmapFont("biosFont", 8);
		_team = t;
		_scoreCard.CenterOrigin();
		collisionOffset = new Vec2(-8f, -6f);
		collisionSize = new Vec2(16f, 13f);
		center = new Vec2(_scoreCard.w / 2, _scoreCard.h / 2);
		_stick = rock;
		base.depth = -0.1f;
	}

	public override void Update()
	{
		base.x = _stick.x + 24f;
		base.y = _stick.y;
	}

	public override void Draw()
	{
		_scoreCard.depth = base.depth;
		Graphics.Draw(_scoreCard, base.x, base.y);
		if (_team == null)
		{
			string score = "X";
			_font.Draw(score, base.x - _font.GetWidth(score) / 2f, base.y - 2f, Color.DarkSlateGray, _scoreCard.depth + 1);
		}
		else
		{
			string score2 = Change.ToString(_team.score);
			_font.Draw(score2, base.x - _font.GetWidth(score2) / 2f, base.y - 2f, Color.DarkSlateGray, _scoreCard.depth + 1);
		}
	}
}
