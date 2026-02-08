using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class GinormoScreen : Thing
{
    private BitmapFont _font;

    public static Vector2 GetSize(bool pSmall)
    {
        return new Vector2(185f, 103f);
    }

    public GinormoScreen(float xpos, float ypos, BoardMode mode)
        : base(xpos, ypos)
    {
        base.layer = Layer.Foreground;
        base.Depth = 0f;
        _font = new BitmapFont("biosFont", 8);
        _collisionSize = new Vector2(184f, 102f);
        List<Team> teams = new List<Team>();
        int index = 0;
        foreach (Team t in Teams.all)
        {
            if (t.activeProfiles.Count > 0)
            {
                teams.Add(t);
            }
        }
        teams.Sort((Team a, Team b) => (a.score != b.score) ? ((a.score < b.score) ? 1 : (-1)) : 0);
        bool smallMode = teams.Count > 4;
        foreach (Team t2 in teams)
        {
            float ySlot = base.Y + 2f + (float)((smallMode ? 12 : 25) * index);
            if (Graphics.aspect > 0.59f)
            {
                ySlot += 10f;
            }
            GinormoCard c = new GinormoCard((float)index * 1f, new Vector2(300f, ySlot), new Vector2(base.X + (float)((mode == BoardMode.Points) ? 2 : 2), ySlot), t2, mode, index, smallMode);
            Level.current.AddThing(c);
            index++;
        }
    }

    public override void Draw()
    {
    }
}
