using System;
using System.Collections.Generic;

namespace DuckGame;

public class ProfileStats : DataClass
{
    private List<string> _hotnessStrings = new List<string> { "Absolute Zero", "Icy Moon", "Antarctica", "Ice Cube", "Ice Cream", "Coffee", "Fire", "A Volcanic Eruption", "The Sun" };

    private Dictionary<string, int> _timesKilledBy = new Dictionary<string, int>();

    public string currentTitle { get; set; }

    public int kills { get; set; }

    public int suicides { get; set; }

    public int timesKilled { get; set; }

    public int matchesWon { get; set; }

    public int trophiesSinceLastWinCounter { get; set; }

    public int trophiesSinceLastWin { get; set; }

    public int timesSpawned { get; set; }

    public int trophiesWon { get; set; }

    public int gamesPlayed { get; set; }

    public int fallDeaths { get; set; }

    public int timesSwore { get; set; }

    public int bulletsFired { get; set; }

    public int bulletsThatHit { get; set; }

    public int trickShots { get; set; }

    public DateTime lastPlayed { get; set; }

    public DateTime lastWon { get; set; }

    public DateTime lastKillTime { get; set; }

    public int coolness { get; set; }

    public int unarmedDucksShot { get; set; }

    public int killsFromTheGrave { get; set; }

    public int timesNetted { get; set; }

    public float timeInNet { get; set; }

    public int loyalFans { get; set; }

    public int unloyalFans { get; set; }

    public float timeUnderMindControl { get; set; }

    public int timesMindControlled { get; set; }

    public float timeOnFire { get; set; }

    public int timesLitOnFire { get; set; }

    public float airTime { get; set; }

    public int timesJumped { get; set; }

    public int disarms { get; set; }

    public int timesDisarmed { get; set; }

    public int quacks { get; set; }

    public float timeWithMouthOpen { get; set; }

    public float timeSpentOnMines { get; set; }

    public int minesSteppedOn { get; set; }

    public float timeSpentReloadingOldTimeyWeapons { get; set; }

    public int presentsOpened { get; set; }

    public int respectGivenToDead { get; set; }

    public int funeralsPerformed { get; set; }

    public int funeralsRecieved { get; set; }

    public float timePreaching { get; set; }

    public int conversions { get; set; }

    public int timesConverted { get; set; }

    public float GetStatCalculation(StatInfo info)
    {
        return info switch
        {
            StatInfo.KillDeathRatio => kills / timesKilled,
            StatInfo.Coolness => coolness,
            StatInfo.ProfileScore => GetProfileScore(),
            _ => 0f,
        };
    }

    public int GetProfileScore()
    {
        return (int)Math.Round(Maths.Clamp(CalculateProfileScore() * 0.3f * 250f, -50f, 200f));
    }

    public string GetCoolnessString()
    {
        int val = (int)Math.Floor((float)(Maths.Clamp(GetProfileScore(), -50, 200) + 50) / 250f * 8.99f);
        return _hotnessStrings[val];
    }

    public void LogKill(Profile p)
    {
        string name = "";
        if (p != null)
        {
            name = p.name;
        }
        if (!_timesKilledBy.ContainsKey(name))
        {
            _timesKilledBy[name] = 0;
        }
        _timesKilledBy[name]++;
    }

    public ProfileStats()
    {
        lastPlayed = DateTime.Now;
        lastWon = DateTime.MinValue;
        currentTitle = "";
        _nodeName = "Stats";
    }

    public int GetFans()
    {
        if (loyalFans < 0)
        {
            loyalFans = 0;
        }
        if (unloyalFans < 0)
        {
            unloyalFans = 0;
        }
        return loyalFans + unloyalFans;
    }

    public bool TryFanTransfer(Profile to, float awesomeness, bool loyal)
    {
        if (unloyalFans > 0 && !loyal)
        {
            unloyalFans--;
            return true;
        }
        if (loyalFans > 0 && Rando.Float(3f) < awesomeness)
        {
            MakeFanUnloyal();
            if (loyal)
            {
                return true;
            }
        }
        return false;
    }

    public void MakeFanLoyal()
    {
        unloyalFans--;
        loyalFans++;
    }

    public void MakeFanUnloyal()
    {
        unloyalFans++;
        loyalFans--;
    }

    public bool FanConsidersLeaving(float awfulness, bool loyal)
    {
        if (unloyalFans > 0 && !loyal)
        {
            unloyalFans--;
            return true;
        }
        if (loyalFans > 0 && Rando.Float(3f) < Math.Abs(awfulness))
        {
            MakeFanUnloyal();
            if (loyal)
            {
                return true;
            }
        }
        return false;
    }

    public float CalculateProfileScore(bool log = false)
    {
        List<StatContribution> stat = new List<StatContribution>();
        float score = 0f;
        float negContrib = 0f;
        float posContrib = 0f;
        float contribution = 0f;
        if (timesSpawned > 0)
        {
            contribution = (float)matchesWon / (float)timesSpawned * 0.4f;
        }
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "MAT",
            amount = contribution
        });
        if (gamesPlayed > 0)
        {
            contribution = (float)trophiesWon / (float)gamesPlayed * 0.4f;
        }
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "WON",
            amount = contribution
        });
        int killed = timesKilled;
        if (killed < 1)
        {
            killed = 1;
        }
        contribution = (float)Math.Log(1f + (float)kills / (float)killed) * 0.4f;
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "KDR",
            amount = contribution
        });
        contribution = (float)Maths.Clamp((DateTime.Now - lastPlayed).Days, 0, 60) / 60f * 0.5f;
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "LVE",
            amount = contribution
        });
        contribution = (float)Math.Log(1f + (float)quacks * 0.0001f) * 0.4f;
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "CHR",
            amount = contribution
        });
        contribution = (float)Math.Log(0.75f + (float)coolness * 0.025f);
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "COO",
            amount = contribution
        });
        contribution = (float)Math.Log(1f + (float)bulletsFired * 0.0001f);
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "SHT",
            amount = contribution
        });
        if (bulletsFired > 0)
        {
            contribution = -0.1f + (float)bulletsThatHit / (float)bulletsFired * 0.2f;
        }
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "ACC",
            amount = contribution
        });
        contribution = (float)Math.Log(1f + (float)disarms * 0.0005f) * 0.5f;
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "DSM",
            amount = contribution
        });
        contribution = 0f - (float)(Math.Log(1f + (float)(timesLitOnFire + timesMindControlled + timesNetted + timesDisarmed + minesSteppedOn + fallDeaths) * 0.0005f) * 0.5);
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "BAD",
            amount = contribution
        });
        contribution = (0f - (float)Maths.Clamp((DateTime.Now - lastWon).Days, 0, 60) / 60f) * 0.3f;
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "LOS",
            amount = contribution
        });
        contribution = (float)Math.Log(1f + (float)timesJumped * 0.0001f) * 0.2f;
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "JMP",
            amount = contribution
        });
        contribution = (float)Math.Log(1f + timeWithMouthOpen * 0.001f) * 0.5f;
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "MTH",
            amount = contribution
        });
        contribution = (float)Math.Log(1f + (float)timesSwore) * 0.5f;
        score += contribution;
        if (contribution > 0f)
        {
            posContrib += contribution;
        }
        else if (contribution < 0f)
        {
            negContrib += contribution;
        }
        stat.Add(new StatContribution
        {
            name = "SWR",
            amount = contribution
        });
        if (log && score != 0f)
        {
            foreach (StatContribution c in stat)
            {
                float percent = 0f;
                if (c.amount != 0f)
                {
                    percent = ((!(c.amount > 0f)) ? (Math.Abs(c.amount) / Math.Abs(negContrib) * (Math.Abs(negContrib) / (posContrib + Math.Abs(negContrib)))) : (Math.Abs(c.amount) / Math.Abs(posContrib) * (posContrib / (posContrib + Math.Abs(negContrib)))));
                }
                if (c.amount < 0f)
                {
                    percent = 0f - percent;
                }
                float red = 0.5f;
                float green = 0.5f;
                if (percent < 0f)
                {
                    red += Math.Abs(percent) * 0.5f;
                    green -= Math.Abs(percent) * 0.5f;
                }
                else
                {
                    green += Math.Abs(percent) * 0.5f;
                    red -= Math.Abs(percent) * 0.5f;
                }
                DevConsole.Log(c.name + ": " + (percent * 100f).ToString("0.000") + "%", new Color(red, green, 0f), 1f);
            }
        }
        return score;
    }
}
