using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace DuckGame;

public class UnlockableHat : Unlockable
{
    private Team _team;

    private TeamHat _hat;

    private DuckPersona _persona;

    private Cape _cape;

    public UnlockableHat(string identifier, Team t, Func<bool> condition, string nam, string desc, string achieve = "")
        : this(canHint: true, identifier, t, condition, nam, desc, achieve)
    {
    }

    public UnlockableHat(bool canHint, string identifier, Team t, Func<bool> condition, string nam, string desc, string achieve = "")
        : base(identifier, condition, nam, desc, achieve)
    {
        allowHints = canHint;
        _team = t;
        _persona = Persona.all.ElementAt(Rando.Int(3));
        _showScreen = true;
    }

    public override void Initialize()
    {
        if (_team != null)
        {
            _team.locked = true;
        }
    }

    protected override void Unlock()
    {
        if (_team != null)
        {
            _team.locked = false;
        }
    }

    protected override void Lock()
    {
        if (_team != null)
        {
            _team.locked = true;
        }
    }

    public override void Draw(float x, float y, Depth depth)
    {
        if (_team != null)
        {
            y -= 9f;
            float ypos = y + 8f;
            _persona.sprite.Depth = depth;
            _persona.sprite.color = Color.White;
            Graphics.Draw(_persona.sprite, 0, x, ypos);
            _persona.armSprite.frame = _persona.sprite.imageIndex;
            _persona.armSprite.Scale = new Vector2(1f, 1f);
            _persona.armSprite.Depth = depth + 4;
            Graphics.Draw(_persona.armSprite, x - 3f, ypos + 6f);
            Vector2 offset = DuckRig.GetHatPoint(_persona.sprite.imageIndex);
            _team.hat.Depth = depth + 2;
            _team.hat.Center = new Vector2(16f, 16f) + _team.hatOffset;
            Graphics.Draw(_team.hat, _team.hat.frame, x + offset.X, ypos + offset.Y);
            if (_team.hat.texture.textureName == "hats/devhat" && _cape == null)
            {
                _hat = new TeamHat(x + offset.X, ypos + offset.Y + 5f, Teams.GetTeam("CAPTAIN"));
                _cape = new Cape(x + offset.X, ypos + offset.Y, _hat);
                _cape.SetCapeTexture(Content.Load<Tex2D>("hats/devCape"));
            }
            if (_team.hat.texture.textureName == "hats/moonwalker" && _cape == null)
            {
                _hat = new TeamHat(x + offset.X, ypos + offset.Y + 5f, Teams.GetTeam("MOONWALK"));
                _cape = new Cape(x + offset.X, ypos + offset.Y, _hat);
                _cape.SetCapeTexture(Content.Load<Tex2D>("hats/moonCape"));
            }
            if (_team.hat.texture.textureName == "hats/royalty" && _cape == null)
            {
                _hat = new TeamHat(x + offset.X, ypos + offset.Y + 5f, Teams.GetTeam("MAJESTY"));
                _cape = new Cape(x + offset.X, ypos + offset.Y, _hat);
                _cape.SetCapeTexture(Content.Load<Tex2D>("hats/royalCape"));
            }
            if (_cape != null)
            {
                _hat.Position = new Vector2(x + offset.X, ypos + offset.Y + 5f);
                _cape.Depth = depth + 2;
                _cape.Update();
                _cape.Draw();
            }
        }
    }
}
