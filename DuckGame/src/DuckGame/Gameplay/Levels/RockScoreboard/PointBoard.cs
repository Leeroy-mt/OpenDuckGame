namespace DuckGame;

public class PointBoard : Thing
{
    private BitmapFont _font;

    private Sprite _scoreCard;

    private Team _team;

    private Thing _stick;

    public PointBoard(Thing rock, Team t)
        : base(rock.X + 24, rock.Y)
    {
        _scoreCard = new("rockThrow/scoreCard");
        _font = new("biosFont", 8);
        _team = t;
        _scoreCard.CenterOrigin();
        collisionOffset = new(-8, -6);
        collisionSize = new(16, 13);
        Center = new(_scoreCard.w / 2, _scoreCard.h / 2);
        _stick = rock;
        Depth = -0.1f;
    }

    public override void Update() =>
        (X, Y) = (_stick.X + 24, _stick.Y);

    public override void Draw()
    {
        _scoreCard.Depth = Depth;
        Graphics.Draw(_scoreCard, X, Y);
        if (_team == null)
        {
            string score = "X";
            _font.Draw(score, X - _font.GetWidth(score) / 2, Y - 2, Color.DarkSlateGray, _scoreCard.Depth + 1);
        }
        else
        {
            string score2 = Change.ToString(_team.score);
            _font.Draw(score2, X - _font.GetWidth(score2) / 2, Y - 2, Color.DarkSlateGray, _scoreCard.Depth + 1);
        }
    }
}
