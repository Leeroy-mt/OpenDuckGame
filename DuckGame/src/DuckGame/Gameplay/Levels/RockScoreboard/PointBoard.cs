namespace DuckGame;

public class PointBoard : Thing
{
    private BitmapFont _font;

    private Sprite _scoreCard;

    private Team _team;

    private Thing _stick;

    public PointBoard(Thing rock, Team t)
        : base(rock.x + 24, rock.y)
    {
        _scoreCard = new("rockThrow/scoreCard");
        _font = new("biosFont", 8);
        _team = t;
        _scoreCard.CenterOrigin();
        collisionOffset = new(-8, -6);
        collisionSize = new(16, 13);
        center = new(_scoreCard.w / 2, _scoreCard.h / 2);
        _stick = rock;
        depth = -0.1f;
    }

    public override void Update() =>
        (x, y) = (_stick.x + 24, _stick.y);

    public override void Draw()
    {
        _scoreCard.depth = depth;
        Graphics.Draw(_scoreCard, x, y);
        if (_team == null)
        {
            string score = "X";
            _font.Draw(score, x - _font.GetWidth(score) / 2, y - 2, Color.DarkSlateGray, _scoreCard.depth + 1);
        }
        else
        {
            string score2 = Change.ToString(_team.score);
            _font.Draw(score2, x - _font.GetWidth(score2) / 2, y - 2, Color.DarkSlateGray, _scoreCard.depth + 1);
        }
    }
}
