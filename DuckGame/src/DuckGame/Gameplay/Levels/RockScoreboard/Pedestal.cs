using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class Pedestal : Thing
{
    private Team _team;

    private SpriteMap _sprite;

    private Sprite _scoreCard;

    private BitmapFont _font;

    private Sprite _trophy;

    private List<Duck> _ducks = new List<Duck>();

    public Pedestal(float xpos, float ypos, Team team, int place, bool smallMode)
        : base(xpos, ypos)
    {
        _team = team;
        if (smallMode)
        {
            _sprite = new SpriteMap("rockThrow/placePedastalsSmall", 27, 45);
        }
        else
        {
            _sprite = new SpriteMap("rockThrow/placePedastals", 38, 45);
        }
        _sprite.frame = place;
        Center = new Vector2(_sprite.w / 2, _sprite.h);
        graphic = _sprite;
        base.Depth = 0.062f;
        _scoreCard = new Sprite("rockThrow/scoreCard");
        _font = new BitmapFont("biosFont", 8);
        _scoreCard.CenterOrigin();
        _trophy = new Sprite("trophy");
        _trophy.CenterOrigin();
        if (Network.isServer)
        {
            int i = 0;
            foreach (Profile p in team.activeProfiles)
            {
                float wide = (team.activeProfiles.Count - 1) * 10;
                Duck duck = new Duck(xpos - wide / 2f + (float)(i * 10), GetYOffset() - 15f, p)
                {
                    Depth = 0.06f
                };
                Level.Add(duck);
                if (place == 0)
                {
                    Trophy t = new Trophy(duck.X, duck.Y);
                    Level.Add(t);
                    if (!Network.isActive)
                    {
                        duck.Fondle(t);
                        duck.GiveHoldable(t);
                    }
                }
                i++;
            }
        }
        Level.Add(new Platform(xpos - 17f, GetYOffset(), 34f, 16f));
        Level.Add(new Block(-6f, GetYOffset() - 100f, 6f, 200f));
        Level.Add(new Block(320f, GetYOffset() - 100f, 6f, 200f));
        Level.Add(new Block(-20f, 155f, 600f, 100f));
    }

    public override void Update()
    {
    }

    public float GetYOffset()
    {
        float ypos = base.Y - 45f;
        if (_sprite.frame == 1)
        {
            ypos = base.Y - 28f;
        }
        else if (_sprite.frame == 2)
        {
            ypos = base.Y - 19f;
        }
        else if (_sprite.frame == 3)
        {
            ypos = base.Y - 12f;
        }
        return ypos;
    }

    public override void Draw()
    {
        base.Depth = -0.5f;
        base.Draw();
        _ = _team.activeProfiles.Count;
        if (_sprite.frame == 0)
        {
            _trophy.Depth = base.Depth + 1;
            Graphics.Draw(_trophy, base.X, base.Y - 14f);
        }
        _scoreCard.Depth = 1f;
        Graphics.Draw(_scoreCard, base.X, base.Y + 2f);
        string score = Change.ToString(_team.score);
        _font.Draw(score, base.X - _font.GetWidth(score) / 2f, base.Y, Color.DarkSlateGray, _scoreCard.Depth + 1);
    }
}
