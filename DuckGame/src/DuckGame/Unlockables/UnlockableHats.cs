using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UnlockableHats : Unlockable
{
    private List<Team> _teams;

    private DuckPersona[] _persona = new DuckPersona[4];

    public UnlockableHats(string identifier, List<Team> t, Func<bool> condition, string nam, string desc, string achieve = "")
        : this(canHint: true, identifier, t, condition, nam, desc, achieve)
    {
    }

    public UnlockableHats(bool canHint, string identifier, List<Team> t, Func<bool> condition, string nam, string desc, string achieve = "")
        : base(identifier, condition, nam, desc, achieve)
    {
        allowHints = canHint;
        _teams = t;
        _showScreen = true;
        _persona[0] = Persona.all.ElementAt(Rando.Int(3));
        _persona[1] = Persona.all.ElementAt(Rando.Int(3));
        _persona[2] = Persona.all.ElementAt(Rando.Int(3));
        _persona[3] = Persona.all.ElementAt(Rando.Int(3));
    }

    public override void Initialize()
    {
        foreach (Team t in _teams)
        {
            if (t != null)
            {
                t.locked = true;
            }
        }
    }

    protected override void Unlock()
    {
        foreach (Team t in _teams)
        {
            if (t != null)
            {
                t.locked = false;
            }
        }
    }

    protected override void Lock()
    {
        foreach (Team t in _teams)
        {
            if (t != null)
            {
                t.locked = true;
            }
        }
    }

    public override void Draw(float x, float y, Depth depth)
    {
        y -= 9f;
        float xOff = 9f;
        if (_teams.Count == 3)
        {
            xOff = 18f;
        }
        int idx = 0;
        foreach (Team t in _teams)
        {
            if (t != null && idx < 8)
            {
                float ypos = y + 12f;
                _persona[idx].sprite.depth = depth;
                _persona[idx].sprite.color = Color.White;
                Graphics.Draw(_persona[idx].sprite, 0, x - xOff + (float)(idx * 18), ypos);
                _persona[idx].armSprite.frame = _persona[idx].sprite.imageIndex;
                _persona[idx].armSprite.scale = new Vec2(1f, 1f);
                _persona[idx].armSprite.depth = depth + 4;
                Graphics.Draw(_persona[idx].armSprite, x - xOff + (float)(idx * 18) - 3f, ypos + 6f);
                Vec2 offset = DuckRig.GetHatPoint(_persona[idx].sprite.imageIndex);
                t.hat.depth = depth + 2;
                t.hat.center = new Vec2(16f, 16f) + t.hatOffset;
                Graphics.Draw(t.hat, t.hat.frame, x - xOff + (float)(idx * 18) + offset.x, ypos + offset.y);
            }
            idx++;
        }
    }
}
