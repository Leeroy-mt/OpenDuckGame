using System.Collections.Generic;

namespace DuckGame;

public class TourneyGroup
{
    public List<Team> players = new List<Team>();

    public List<bool> assigned = new List<bool>();

    public TourneyGroup next;

    public int groupIndex;

    public int depth;

    public void AddPlayer(Team p, bool ass = false)
    {
        players.Add(p);
        assigned.Add(ass);
    }
}
