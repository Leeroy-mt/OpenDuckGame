using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class CTF : GameMode
{
    public static bool hasWinner;

    public static bool winner;

    protected override void Initialize()
    {
        hasWinner = false;
    }

    protected override void Start()
    {
    }

    public static void CaptureFlag(bool team)
    {
        hasWinner = true;
        winner = team;
    }

    protected override void Update()
    {
        if (!_matchOver)
        {
            List<Team> liveTeams = new List<Team>();
            int teamIndex = 0;
            foreach (Team t in Teams.all)
            {
                if (t.activeProfiles.Count() == 0)
                {
                    continue;
                }
                foreach (Profile p in t.activeProfiles)
                {
                    if (p.duck == null)
                    {
                        continue;
                    }
                    p.duck.ctfTeamIndex = teamIndex;
                    if (!p.duck.dead)
                    {
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
                        continue;
                    }
                    p.duck.Position = p.duck.respawnPos;
                    if (Level.current.camera is FollowCam)
                    {
                        (Level.current.camera as FollowCam).Add(p.duck);
                    }
                    p.duck.respawnTime += 0.016f;
                    if (p.duck.respawnTime > 1.5f)
                    {
                        p.duck.respawnTime = 0f;
                        p.duck.dead = false;
                        if (p.duck.ragdoll != null)
                        {
                            p.duck.ragdoll.Unragdoll();
                        }
                        p.duck.Position = p.duck.respawnPos;
                        p.duck.isGhost = true;
                        p.duck.immobilized = false;
                        p.duck.crouch = false;
                        p.duck.sliding = false;
                        p.duck._cooked = null;
                        p.duck.onFire = false;
                        p.duck.unfocus = 1f;
                        if (p.duck._trapped != null)
                        {
                            Level.Remove(p.duck._trapped);
                        }
                        p.duck._trapped = null;
                        if (Level.current.camera is FollowCam)
                        {
                            (Level.current.camera as FollowCam).Add(p.duck);
                        }
                        Level.Add(p.duck);
                    }
                }
                teamIndex++;
            }
            if (hasWinner)
            {
                EndMatch();
            }
        }
        base.Update();
    }

    protected override List<Duck> AssignSpawns()
    {
        return (from sp in Spawn.SpawnCTF()
                orderby sp.X
                select sp).ToList();
    }

    protected override Level GetNextLevel()
    {
        return new CTFLevel(Deathmatch.RandomLevelString(GameMode.previousLevel, "ctf"));
    }

    protected override List<Profile> AddPoints()
    {
        List<Profile> winners = new List<Profile>();
        List<Team> convertedTeams = new List<Team>();
        List<Team> liveTeams = new List<Team>();
        int winIndex = ((!winner) ? 1 : 0);
        int teamIndex = 0;
        foreach (Team t in Teams.all)
        {
            if (t.activeProfiles.Count() == 0)
            {
                continue;
            }
            foreach (Profile p in t.activeProfiles)
            {
                if (p.duck == null || teamIndex != winIndex)
                {
                    continue;
                }
                if (p.duck.converted != null && p.duck.converted.profile.team != p.team)
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
            teamIndex++;
        }
        if (liveTeams.Count <= 1 && liveTeams.Count > 0)
        {
            liveTeams.AddRange(convertedTeams);
            foreach (Team item in liveTeams)
            {
                foreach (Profile p2 in item.activeProfiles)
                {
                    if (p2.duck != null && !p2.duck.dead)
                    {
                        winners.Add(p2);
                        p2.stats.lastWon = DateTime.Now;
                        p2.stats.matchesWon++;
                        Profile realProfile = p2;
                        if (p2.duck.converted != null)
                        {
                            realProfile = p2.duck.converted.profile;
                        }
                        PlusOne plusOne = new PlusOne(0f, 0f, realProfile);
                        plusOne.anchor = p2.duck;
                        plusOne.anchor.offset = new Vec2(0f, -16f);
                        Level.Add(plusOne);
                    }
                }
            }
            if (Network.isActive && Network.isServer)
            {
                Send.Message(new NMAssignWin(winners, null));
            }
            liveTeams.First().score++;
        }
        return winners;
    }
}
