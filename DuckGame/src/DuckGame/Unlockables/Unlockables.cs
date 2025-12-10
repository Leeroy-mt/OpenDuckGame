using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class Unlockables
{
    private static bool wawa = false;

    private static List<Unlockable> _unlocks = new List<Unlockable>();

    private static HashSet<Unlockable> _pendingUnlocks = new HashSet<Unlockable>();

    public static List<Unlockable> lockedItems => _unlocks.Where((Unlockable x) => !x.CheckCondition() && x.allowHints && x.description != null && x.description != "").ToList();

    public static void Initialize()
    {
        _unlocks.Add(new UnlockableHats(canHint: false, "hatpack1", new List<Team>
        {
            Teams.GetTeam("BAWB"),
            Teams.GetTeam("Frank"),
            Teams.GetTeam("Meeee")
        }, () => Unlocks.IsUnlocked("HATTY1"), "Hat Pack 1", "Check out these nifty cool hats."));
        _unlocks.Add(new UnlockableHats(canHint: false, "hatpack2", new List<Team>
        {
            Teams.GetTeam("Pulpy"),
            Teams.GetTeam("Joey"),
            Teams.GetTeam("Cowboys")
        }, () => Unlocks.IsUnlocked("HATTY2"), "Hat Pack 2", "More cool hats! WOW!"));
        bool futz = false;
        _unlocks.Add(new UnlockableAchievement(canHint: false, "gamerduck", () => Global.data.timesSpawned > ((!futz) ? 99 : 0), "Duck Gamer", "Spawn 100 times.", "play100"));
        _unlocks.Add(new UnlockableHat("chancyHat", Teams.GetTeam("Chancy"), () => Unlocks.IsUnlocked("ULTIMATE"), "Chancy", "Get platinum on all challenges", "chancy"));
        _unlocks.Add(new UnlockableAchievement(canHint: false, "ritual", () => Global.data.timesSpawned > (futz ? 1 : 999), "Ritual", "Spawn 1000 times.", "play1000"));
        _unlocks.Add(new UnlockableHat("skully", Teams.GetTeam("SKULLY"), () => Global.data.kills > (futz ? 3 : 999), "SKULLY", "Kill 1000 Ducks", "kill1000"));
        _unlocks.Add(new UnlockableAchievement(canHint: false, "endurance", () => Global.data.longestMatchPlayed > (futz ? 5 : 49), "Endurance", "Play through a 50 point match", "endurance"));
        _unlocks.Add(new UnlockableAchievement(canHint: false, "outgoing", () => Global.data.onlineWins > ((!futz) ? 9 : 0), "Outgoing", "Win 10 online matches", "online10"));
        _unlocks.Add(new UnlockableAchievement(canHint: false, "basement", () => Unlocks.IsUnlocked("BASEMENTKEY"), "Basement Dweller", "Unlock the basement", "basement"));
        _unlocks.Add(new UnlockableAchievement(canHint: false, "poweruser", () => Global.data.customMapPlayCount.Count > ((!futz) ? 9 : 0), "Power User", "Play on 10 different custom maps", "editor"));
        _unlocks.Add(new UnlockableAchievement(canHint: false, "drawbreaker", () => Global.data.drawsPlayed > ((!futz) ? 9 : 0), "Draw Breaker", "Break 10 draws", "drawbreaker"));
        _unlocks.Add(new UnlockableAchievement(canHint: false, "hotstuff", () => Profiles.MostTimeOnFire() > (float)(futz ? 2 : 899), "Hot Stuff", "Spend 15 minutes on fire with any one profile", "fire"));
        _unlocks.Add(new UnlockableAchievement(canHint: false, "myboy", () => Profiles.experienceProfile != null && Profiles.experienceProfile.numLittleMen > 0, "That's My Boy", "Raise a little man.", "myboy"));
        _unlocks.Add(new UnlockableAchievement(canHint: false, "jukebox", () => Profiles.experienceProfile != null && Profiles.experienceProfile.numLittleMen > 7, "Jukebox Hero", "Raise eight little men.", "jukebox"));
        _unlocks.Add(new UnlockableAchievement(canHint: false, "kingme", () => Profiles.experienceProfile != null && Profiles.experienceProfile.xp >= DuckNetwork.GetLevel(999).xpRequired, "King Me", "Level up all the way.", "kingme"));
        _unlocks.Add(new UnlockableHat("ballz", Teams.GetTeam("BALLZ"), () => Global.data.ducksCrushed > ((!futz) ? 49 : 0), "BALLZ", "Crush 50 Ducks", "crate"));
        _unlocks.Add(new UnlockableHat("hearts", Teams.GetTeam("Hearts"), () => Global.data.matchesPlayed > ((!futz) ? 49 : 0), "<3", "Finish 50 whole matches.", "finish50"));
        _unlocks.Add(new UnlockableHat("swackHat", Teams.GetTeam("SWACK"), () => Global.data.matchesPlayed > 0, "SWACK", "Play through a match"));
        _unlocks.Add(new UnlockableHat("BRODUCK", Teams.GetTeam("BRODUCK"), () => Global.data.strafeDistance > (futz ? 0.25f : 10f), "BRODUCK", "Strafe 10 Kilometers"));
        _unlocks.Add(new UnlockableHat("astropal", Teams.GetTeam("astropal"), () => Global.data.jetFuelUsed > (futz ? 5f : 200f), "ASTROPAL", "Burn 200 gallons of rocket fuel."));
        _unlocks.Add(new UnlockableHat("eggpal", Teams.GetTeam("eggpal"), () => Global.data.winsAsSwack > ((!futz) ? 4 : 0), "EGGPAL", "Win 5 rounds as SWACK"));
        _unlocks.Add(new UnlockableHat("brad", Teams.GetTeam("brad"), () => Global.data.disarms > ((!futz) ? 99 : 0), "BRAD DUNGEON", "Disarm 100 ducks."));
        _unlocks.Add(new UnlockableHat("brick", Teams.GetTeam("BRICK"), () => Global.data.laserBulletsFired > ((!futz) ? 149 : 0), "BRICK", "Fire 150 laser bullets."));
        _unlocks.Add(new UnlockableHat("ducks", Teams.GetTeam("DUCKS"), () => Global.data.quacks > 999, "DUCK", "Quack 1000 times."));
        _unlocks.Add(new UnlockableHat("funnyman", Teams.GetTeam("FUNNYMAN"), () => Global.data.hatsStolen > 100, "FUNNYMAN", "Wear 100 different faces."));
        _unlocks.Add(new UnlockableHat("wizards", Teams.GetTeam("Wizards"), () => Global.data.angleShots > 25, "Wizard", "Make 25 angle trick shots."));
        _unlocks.Add(new UnlockableHat("wolves", Teams.GetTeam("wolfy"), () => Global.data.playedDuringFullMoon, "Wolves by DORD", "Play at 12 AM on a Full Moon."));
        _unlocks.Add(new UnlockableHat("masterHat", Teams.GetTeam("masters"), () => Global.data.nettedDuckTossKills > 25, "BackSlash", "Toss kill 25 netted ducks."));
        _unlocks.Add(new UnlockableHat("clamHat", Teams.GetTeam("clams"), () => Global.data.secondsUnderwater > 600, "CLAMS", "Spend 10 minutes under water."));
        _unlocks.Add(new UnlockableHat("wafflesHat", Teams.GetTeam("waffles"), () => Global.data.playedInTheMorning, "WAFFLES", "Play Duck Game at 9:00AM."));
        _unlocks.Add(new UnlockableHats("toeboynearlsHats", new List<Team>
        {
            Teams.GetTeam("toeboys"),
            Teams.GetTeam("bigearls")
        }, () => Global.data.presentsOpened > 100, "TOEBOY N BIGEARL", "Open 100 Presents"));
        _unlocks.Add(new UnlockableHat("katanaHat", Teams.GetTeam("zeros"), () => Global.data.swordKills > 25, "KATANA ZERO", "Get 25 Sword Kills"));
        _unlocks.Add(new UnlockableHat("johnnygreyHat", Teams.GetTeam("johnnygrey"), () => Global.data.typedJohnny, "JOHNNYGREY", "Type 'johnnygrey' into the command console."));
        _unlocks.Add(new UnlockableHat("diplomatsHat", Teams.GetTeam("diplomats"), () => (int)Global.data.onlineMatches >= 50, "DIPLOMATS", "Finish 50 online matches."));
        _unlocks.Add(new UnlockableHat("b52sHat", Teams.GetTeam("B52s"), () => (int)Global.data.winsAsHair >= 10, "B52s", "Win 10 Matches With Great Hair."));
        _unlocks.Add(new UnlockableHats(canHint: false, "swimhats", new List<Team>
        {
            Teams.GetTeam("kerchiefs"),
            Teams.GetTeam("postals"),
            Teams.GetTeam("wahhs")
        }, () => Global.data.bootedSinceSwitchHatPatch > 0 && Global.data.matchesPlayed > 0, "HATS FOR YOU", "Thanks for playing Duck Game!"));
        _unlocks.Add(new UnlockableHat("ufosHat", Teams.GetTeam("uufos"), () => Global.data.timeJetpackedAsRagdoll >= 1200, "UUFOS", "Spend 20 seconds ragdoll jetpacking."));
        string nam = "CYCLOPS";
        if (Teams.GetTeam(nam) != null)
        {
            _unlocks.Add(new UnlockableHat(canHint: false, nam.ToLowerInvariant(), Teams.GetTeam(nam), () => Global.boughtHats.Contains("CYCLOPS"), nam, Teams.GetTeam(nam).description));
        }
        nam = "MOTHERS";
        if (Teams.GetTeam(nam) != null)
        {
            _unlocks.Add(new UnlockableHat(canHint: false, nam.ToLowerInvariant(), Teams.GetTeam(nam), () => Global.boughtHats.Contains("MOTHERS"), nam, Teams.GetTeam(nam).description));
        }
        nam = "BIG ROBO";
        if (Teams.GetTeam(nam) != null)
        {
            _unlocks.Add(new UnlockableHat(canHint: false, nam.ToLowerInvariant(), Teams.GetTeam(nam), () => Global.boughtHats.Contains("BIG ROBO"), nam, Teams.GetTeam(nam).description));
        }
        nam = "TINCAN";
        if (Teams.GetTeam(nam) != null)
        {
            _unlocks.Add(new UnlockableHat(canHint: false, nam.ToLowerInvariant(), Teams.GetTeam(nam), () => Global.boughtHats.Contains("TINCAN"), nam, Teams.GetTeam(nam).description));
        }
        nam = "WELDERS";
        if (Teams.GetTeam(nam) != null)
        {
            _unlocks.Add(new UnlockableHat(canHint: false, nam.ToLowerInvariant(), Teams.GetTeam(nam), () => Global.boughtHats.Contains("WELDERS"), nam, Teams.GetTeam(nam).description));
        }
        nam = "PONYCAP";
        if (Teams.GetTeam(nam) != null)
        {
            _unlocks.Add(new UnlockableHat(canHint: false, nam.ToLowerInvariant(), Teams.GetTeam(nam), () => Global.boughtHats.Contains("PONYCAP"), nam, Teams.GetTeam(nam).description));
        }
        nam = "TRICORNE";
        if (Teams.GetTeam(nam) != null)
        {
            _unlocks.Add(new UnlockableHat(canHint: false, nam.ToLowerInvariant(), Teams.GetTeam(nam), () => Global.boughtHats.Contains("TRICORNE"), nam, Teams.GetTeam(nam).description));
        }
        nam = "TWINTAIL";
        if (Teams.GetTeam(nam) != null)
        {
            _unlocks.Add(new UnlockableHat(canHint: false, nam.ToLowerInvariant(), Teams.GetTeam(nam), () => Global.boughtHats.Contains("TWINTAIL"), nam, Teams.GetTeam(nam).description));
        }
        nam = "MAJESTY";
        if (Teams.GetTeam(nam) != null)
        {
            _unlocks.Add(new UnlockableHat(canHint: false, nam.ToLowerInvariant(), Teams.GetTeam(nam), () => Global.boughtHats.Contains("MAJESTY"), nam, "Max out your level (holy crap!!)"));
        }
        nam = "MOONWALK";
        if (Teams.GetTeam(nam) != null)
        {
            _unlocks.Add(new UnlockableHat(canHint: false, nam.ToLowerInvariant(), Teams.GetTeam(nam), () => Global.boughtHats.Contains("MOONWALK"), nam, "Raise 8 little men."));
        }
        nam = "HIGHFIVES";
        if (Teams.GetTeam(nam) != null)
        {
            _unlocks.Add(new UnlockableHat(canHint: false, nam.ToLowerInvariant(), Teams.GetTeam(nam), () => Global.boughtHats.Contains("HIGHFIVES"), nam, Teams.GetTeam(nam).description));
        }
        _unlocks.Add(new UnlockableHat(canHint: false, "devtimes", Teams.GetTeam("CAPTAIN"), () => (Profile.CalculateLocalFlippers() & 0x10) != 0, "UR THE BEST", "Thank you for playing Duck Game <3"));
        _unlocks.Add(new UnlockableHat("eyebob", Teams.GetTeam("eyebob"), () => Global.data.giantLaserKills > 24, "CHARGE SHOT", "Get 25 Kills With the Giant Death Laser"));
        foreach (Unlockable u in _unlocks)
        {
            u.Initialize();
            if (u.CheckCondition())
            {
                u.DoUnlock();
            }
        }
        if (MonoMain.GetLocalTime().Hour == 9)
        {
            Global.data.playedInTheMorning = true;
        }
        if (MonoMain.FullMoon)
        {
            Global.data.playedDuringFullMoon = true;
        }
    }

    public static bool HasPendingUnlocks()
    {
        if (Profiles.experienceProfile != null)
        {
            if (Profiles.experienceProfile.numLittleMen >= 7 && !Global.boughtHats.Contains("MOONWALK"))
            {
                Global.boughtHats.Add("MOONWALK");
            }
            if (Profiles.experienceProfile.xp >= DuckNetwork.GetLevel(999).xpRequired && !Global.boughtHats.Contains("MAJESTY"))
            {
                Global.boughtHats.Add("MAJESTY");
            }
        }
        _pendingUnlocks.Clear();
        foreach (Unlockable u in _unlocks)
        {
            if (!u.locked)
            {
                continue;
            }
            if (u.CheckCondition())
            {
                if (u.showScreen)
                {
                    _pendingUnlocks.Add(u);
                }
                else
                {
                    u.DoUnlock();
                }
            }
            else
            {
                u.DoLock();
            }
        }
        Steam.StoreStats();
        return _pendingUnlocks.Count > 0;
    }

    public static void CheckAchievements()
    {
        foreach (Unlockable u in _unlocks)
        {
            if (u.locked && !u.showScreen)
            {
                if (u.CheckCondition())
                {
                    u.DoUnlock();
                }
                else
                {
                    u.DoLock();
                }
            }
        }
    }

    public static Unlockable GetUnlock(string identifier)
    {
        return _unlocks.FirstOrDefault((Unlockable x) => x.id == identifier);
    }

    public static HashSet<Unlockable> GetPendingUnlocks()
    {
        return _pendingUnlocks;
    }

    public static void UnlockAll()
    {
        foreach (Unlockable unlock in _unlocks)
        {
            unlock.DoUnlock();
        }
    }
}
