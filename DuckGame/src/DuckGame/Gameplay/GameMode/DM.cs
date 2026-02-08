using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class DM : GameMode
{
    private int waitFrames = 3;

    public DM(bool validityTest = false, bool editorTestMode = false)
        : base(validityTest, editorTestMode)
    {
    }

    protected override void Initialize()
    {
    }

    protected override void Start()
    {
    }

    protected override void Update()
    {
        List<Team> liveTeams = new List<Team>();
        foreach (Team t in Teams.all)
        {
            foreach (Profile p in t.activeProfiles)
            {
                if (p.duck == null || p.duck.dead)
                {
                    continue;
                }
                if (p.duck.converted != null && p.duck.converted.profile.team != p.team)
                {
                    if (!liveTeams.Contains(p.duck.converted.profile.team))
                    {
                        liveTeams.Add(p.duck.converted.profile.team);
                    }
                }
                else if (!liveTeams.Contains(t))
                {
                    liveTeams.Add(t);
                }
            }
        }
        if (liveTeams.Count <= 1)
        {
            EndMatch();
        }
        else
        {
            _matchOver = false;
            _roundEndWait = 1f;
        }
        base.Update();
    }

    protected override List<Duck> AssignSpawns()
    {
        return (from sp in Spawn.SpawnPlayers()
                orderby sp.X
                select sp).ToList();
    }

    protected override Level GetNextLevel()
    {
        if (_editorTestMode)
        {
            return new GameLevel((Level.current as GameLevel).levelInputString, 0, validityTest: false, editorTestMode: true);
        }
        return new GameLevel(Deathmatch.RandomLevelString(GameMode.previousLevel));
    }

    protected override List<Profile> AddPoints()
    {
        if (TeamSelect2.KillsForPoints)
        {
            return new List<Profile>();
        }
        List<Profile> winners = new List<Profile>();
        List<Team> convertedTeams = new List<Team>();
        List<Team> liveTeams = new List<Team>();
        foreach (Team t in Teams.all)
        {
            foreach (Profile p in t.activeProfiles)
            {
                if (p == null || p.duck == null || p.duck.dead)
                {
                    continue;
                }
                if (p.duck.converted != null && p.duck.converted.profile != null && p.duck.converted.profile.team != p.team)
                {
                    if (!liveTeams.Contains(p.duck.converted.profile.team))
                    {
                        liveTeams.Add(p.duck.converted.profile.team);
                    }
                    if (!convertedTeams.Contains(p.duck.profile.team))
                    {
                        convertedTeams.Add(p.duck.profile.team);
                    }
                }
                else if (!liveTeams.Contains(t))
                {
                    liveTeams.Add(t);
                }
                break;
            }
        }
        if (liveTeams.Count <= 1 && liveTeams.Count > 0)
        {
            liveTeams.AddRange(convertedTeams);
            GameMode.lastWinners.Clear();
            Profile realProfile = null;
            foreach (Team item in liveTeams)
            {
                foreach (Profile p2 in item.activeProfiles)
                {
                    if (p2 != null && p2.duck != null && !p2.duck.dead)
                    {
                        winners.Add(p2);
                        Profile apply = p2;
                        if (p2.duck.converted != null)
                        {
                            apply = (realProfile = p2.duck.converted.profile);
                        }
                        GameMode.lastWinners.Add(p2);
                        if (apply != null)
                        {
                            PlusOne plusOne = new PlusOne(0f, 0f, apply, temp: false, _editorTestMode);
                            plusOne.anchor = p2.duck;
                            plusOne.anchor.offset = new Vector2(0f, -16f);
                            Level.Add(plusOne);
                        }
                    }
                }
            }
            if (Network.isActive && Network.isServer)
            {
                Send.Message(new NMAssignWin(GameMode.lastWinners, realProfile));
            }
            liveTeams.First().score++;
        }
        return winners;
    }
}
