using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class Spawn
{
    private static bool runningSecondSpawn;

    private static SpawnPoint AttemptTeamSpawn(Team team, List<SpawnPoint> usedSpawns, List<Duck> spawned)
    {
        _ = Level.current;
        List<TeamSpawn> validTeamSpawns = new List<TeamSpawn>();
        foreach (TeamSpawn s in Level.current.things[typeof(TeamSpawn)])
        {
            if (!usedSpawns.Contains(s) && (!s.eightPlayerOnly.value || GameLevel.NumberOfDucks > 4))
            {
                validTeamSpawns.Add(s);
            }
        }
        if (validTeamSpawns.Count > 0)
        {
            TeamSpawn teamSpawn = validTeamSpawns[Rando.Int(validTeamSpawns.Count - 1)];
            usedSpawns.Add(teamSpawn);
            for (int i = 0; i < team.numMembers; i++)
            {
                Vector2 pos = teamSpawn.Position;
                if (team.numMembers == 2)
                {
                    float spread = 18.823528f;
                    pos.X = teamSpawn.Position.X - 16f + spread * (float)i;
                }
                else if (team.numMembers == 3)
                {
                    float spread2 = 9.411764f;
                    pos.X = teamSpawn.Position.X - 16f + spread2 * (float)i;
                }
                Duck player = new Duck(pos.X, pos.Y - 7f, team.activeProfiles[i]);
                player.offDir = teamSpawn.offDir;
                spawned.Add(player);
            }
            return teamSpawn;
        }
        return null;
    }

    private static SpawnPoint AttemptFreeSpawn(Profile profile, List<SpawnPoint> usedSpawns, List<Duck> spawned)
    {
        List<SpawnPoint> validFreeSpawns = new List<SpawnPoint>();
        foreach (FreeSpawn s in Level.current.things[typeof(FreeSpawn)])
        {
            if (s.secondSpawn.value == runningSecondSpawn && !usedSpawns.Contains(s) && (!s.eightPlayerOnly.value || GameLevel.NumberOfDucks > 4))
            {
                validFreeSpawns.Add(s);
            }
        }
        if (validFreeSpawns.Count == 0)
        {
            foreach (FreeSpawn s2 in Level.current.things[typeof(FreeSpawn)])
            {
                if (!usedSpawns.Contains(s2) && (!s2.eightPlayerOnly.value || GameLevel.NumberOfDucks > 4))
                {
                    validFreeSpawns.Add(s2);
                }
            }
        }
        if (validFreeSpawns.Count == 0)
        {
            return null;
        }
        SpawnPoint freeSpawn = validFreeSpawns[Rando.Int(validFreeSpawns.Count - 1)];
        usedSpawns.Add(freeSpawn);
        Duck player = new Duck(freeSpawn.X, freeSpawn.Y - 7f, profile);
        player.offDir = freeSpawn.offDir;
        spawned.Add(player);
        runningSecondSpawn = !runningSecondSpawn;
        return freeSpawn;
    }

    private static SpawnPoint AttemptCTFSpawn(Profile profile, List<SpawnPoint> usedSpawns, List<Duck> spawned, bool red)
    {
        int t = (red ? 1 : 2);
        List<SpawnPoint> validFreeSpawns = new List<SpawnPoint>();
        foreach (FreeSpawn s in Level.current.things[typeof(FreeSpawn)])
        {
            if (!usedSpawns.Contains(s) && (int)s.spawnType == t)
            {
                validFreeSpawns.Add(s);
            }
        }
        if (validFreeSpawns.Count == 0)
        {
            return null;
        }
        SpawnPoint freeSpawn = validFreeSpawns[Rando.Int(validFreeSpawns.Count - 1)];
        usedSpawns.Add(freeSpawn);
        Duck player = new Duck(freeSpawn.X, freeSpawn.Y - 7f, profile);
        player.offDir = freeSpawn.offDir;
        spawned.Add(player);
        return freeSpawn;
    }

    private static SpawnPoint AttemptAnySpawn(Profile profile, List<SpawnPoint> usedSpawns, List<Duck> spawned)
    {
        List<SpawnPoint> validFreeSpawns = new List<SpawnPoint>();
        foreach (SpawnPoint s in Level.current.things[typeof(SpawnPoint)])
        {
            if (!usedSpawns.Contains(s))
            {
                validFreeSpawns.Add(s);
            }
        }
        if (validFreeSpawns.Count == 0)
        {
            if (usedSpawns.Count <= 0)
            {
                return null;
            }
            validFreeSpawns.AddRange(usedSpawns);
        }
        SpawnPoint freeSpawn = validFreeSpawns[Rando.Int(validFreeSpawns.Count - 1)];
        usedSpawns.Add(freeSpawn);
        Duck player = new Duck(freeSpawn.X, freeSpawn.Y - 7f, profile);
        player.offDir = freeSpawn.offDir;
        spawned.Add(player);
        return freeSpawn;
    }

    public static List<Duck> SpawnPlayers()
    {
        return SpawnPlayers(recordStats: true);
    }

    public static List<Duck> SpawnPlayers(bool recordStats)
    {
        List<Duck> spawns = new List<Duck>();
        List<SpawnPoint> usedSpawns = new List<SpawnPoint>();
        List<Team> teams = Teams.allRandomized;
        if (GameMode.showdown)
        {
            List<Team> newTeams = new List<Team>();
            int highest = 0;
            foreach (Team t in teams)
            {
                if (t.score > highest)
                {
                    highest = t.score;
                }
            }
            foreach (Team t2 in teams)
            {
                if (t2.score == highest)
                {
                    newTeams.Add(t2);
                }
            }
            teams = newTeams;
        }
        int num = 0;
        foreach (Team item in teams)
        {
            if (item.activeProfiles.Count() != 0)
            {
                num++;
            }
        }
        GameLevel.NumberOfDucks = num;
        foreach (Team t3 in teams)
        {
            if (t3.activeProfiles.Count() == 0)
            {
                continue;
            }
            if (recordStats)
            {
                foreach (Profile activeProfile in t3.activeProfiles)
                {
                    activeProfile.stats.timesSpawned++;
                }
            }
            if (t3.activeProfiles.Count() == 1)
            {
                SpawnPoint point = AttemptFreeSpawn(t3.activeProfiles[0], usedSpawns, spawns);
                if (point == null)
                {
                    point = AttemptTeamSpawn(t3, usedSpawns, spawns);
                    if (point == null)
                    {
                        usedSpawns.Clear();
                        point = AttemptFreeSpawn(t3.activeProfiles[0], usedSpawns, spawns);
                        if (point == null)
                        {
                            usedSpawns.Clear();
                            point = AttemptTeamSpawn(t3, usedSpawns, spawns);
                        }
                    }
                }
                if (point == null)
                {
                    return spawns;
                }
                continue;
            }
            SpawnPoint point2 = AttemptTeamSpawn(t3, usedSpawns, spawns);
            if (point2 != null)
            {
                continue;
            }
            foreach (Profile profile in t3.activeProfiles)
            {
                point2 = AttemptFreeSpawn(profile, usedSpawns, spawns);
                if (point2 == null)
                {
                    usedSpawns.Clear();
                    point2 = AttemptFreeSpawn(profile, usedSpawns, spawns);
                }
                if (point2 == null)
                {
                    return spawns;
                }
            }
        }
        List<Duck> movedDucks = new List<Duck>();
        foreach (Duck d in spawns)
        {
            Duck d2 = spawns.FirstOrDefault((Duck x) => x != d && x.Position == d.Position);
            if (d2 != null && !movedDucks.Contains(d2) && !movedDucks.Contains(d))
            {
                d.X += 4f;
                d2.X -= 4f;
                movedDucks.Add(d);
                movedDucks.Add(d2);
            }
        }
        return spawns;
    }

    public static List<Duck> SpawnCTF()
    {
        List<Duck> spawns = new List<Duck>();
        List<SpawnPoint> usedSpawns = new List<SpawnPoint>();
        List<Team> all = Teams.all;
        int index = 0;
        foreach (Team t in all)
        {
            if (t.activeProfiles.Count() == 0)
            {
                continue;
            }
            foreach (Profile activeProfile in t.activeProfiles)
            {
                activeProfile.stats.timesSpawned++;
            }
            foreach (Profile activeProfile2 in t.activeProfiles)
            {
                AttemptCTFSpawn(activeProfile2, usedSpawns, spawns, index == 0);
            }
            index++;
        }
        return spawns;
    }
}
