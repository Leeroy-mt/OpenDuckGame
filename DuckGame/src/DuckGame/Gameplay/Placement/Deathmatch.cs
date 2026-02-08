using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class Deathmatch : Thing
{
    private bool _matchOver;

    private float _deadTimer = 1f;

    public static bool showdown = false;

    private static int numMatches = 0;

    private static Queue<string> _recentLevels = new Queue<string>();

    private static List<string> _demoLevels = new List<string> { "deathmatch/forest02", "deathmatch/office02", "deathmatch/forest04", "deathmatch/office07", "deathmatch/office10", "deathmatch/office05" };

    private static int _winsPerSet = 5;

    private static int _roundsBetweenIntermission = 5;

    private static int _userMapsPercent = 0;

    private static bool _enableRandom = true;

    private static bool _randomMapsOnly = false;

    public static List<Profile> lastWinners = new List<Profile>();

    private static float _wait = 0f;

    private static bool _endedHighlights = false;

    private static string _currentSong = "";

    private Sprite _bottomWedge;

    private bool _addedPoints;

    private UIComponent _pauseGroup;

    private UIMenu _pauseMenu;

    private UIMenu _confirmMenu;

    private new Level _level;

    private MenuBoolean _quit = new MenuBoolean();

    private static List<string> _networkLevels = null;

    public static int levelsSinceRandom = 0;

    public static int levelsSinceWorkshop = 0;

    public static int levelsSinceCustom = 0;

    public static int clientLevelRoundRobin;

    private static bool _prevNetworkSetting;

    private static bool _prevEightPlayerSetting;

    private static List<string> _fourPlayerLevels;

    private static List<string> _eightPlayerNonRestrictedLevels;

    private static List<string> _eightPlayerAllLevels;

    private static List<string> _rareLevels;

    private static bool _lastLevelWasPyramid = false;

    private bool _paused;

    private bool switched;

    public static int winsPerSet
    {
        get
        {
            return _winsPerSet;
        }
        set
        {
            _winsPerSet = value;
        }
    }

    public static int roundsBetweenIntermission
    {
        get
        {
            return _roundsBetweenIntermission;
        }
        set
        {
            _roundsBetweenIntermission = value;
        }
    }

    public static int userMapsPercent
    {
        get
        {
            return _userMapsPercent;
        }
        set
        {
            _userMapsPercent = value;
        }
    }

    public static bool enableRandom
    {
        get
        {
            return _enableRandom;
        }
        set
        {
            _enableRandom = value;
        }
    }

    public static bool randomMapsOnly
    {
        get
        {
            return _randomMapsOnly;
        }
        set
        {
            _randomMapsOnly = value;
        }
    }

    public Deathmatch(Level l)
    {
        _level = l;
        base.layer = Layer.HUD;
        _bottomWedge = new Sprite("bottomWedge");
    }

    public override void Initialize()
    {
        _pauseGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        _pauseMenu = new UIMenu("@LWING@PAUSE@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@CLOSE @SELECT@SELECT");
        _confirmMenu = new UIMenu("REALLY QUIT?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        UIDivider pauseBox = new UIDivider(vert: true, 0.8f);
        pauseBox.leftSection.Add(new UIMenuItem("RESUME", new UIMenuActionCloseMenu(_pauseGroup), UIAlign.Left));
        pauseBox.leftSection.Add(new UIMenuItem("OPTIONS", new UIMenuActionOpenMenu(_pauseMenu, Options.optionsMenu), UIAlign.Left));
        pauseBox.leftSection.Add(new UIText(" ", Color.White, UIAlign.Left));
        pauseBox.leftSection.Add(new UIMenuItem("|DGRED|QUIT", new UIMenuActionOpenMenu(_pauseMenu, _confirmMenu), UIAlign.Left));
        pauseBox.rightSection.Add(new UIImage("pauseIcons", UIAlign.Right));
        _pauseMenu.Add(pauseBox);
        _pauseMenu.Close();
        _pauseGroup.Add(_pauseMenu, doAnchor: false);
        Options.AddMenus(_pauseGroup);
        Options.openOnClose = _pauseMenu;
        _confirmMenu.Add(new UIMenuItem("NO!", new UIMenuActionOpenMenu(_confirmMenu, _pauseMenu), UIAlign.Left, default(Color), backButton: true));
        _confirmMenu.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _quit)));
        _confirmMenu.Close();
        _pauseGroup.Add(_confirmMenu, doAnchor: false);
        _pauseGroup.Close();
        _pauseGroup.Update();
        _pauseGroup.Update();
        Level.Add(_pauseGroup);
        Highlights.StartRound();
    }

    public override void Terminate()
    {
        Options.openOnClose = null;
    }

    public static string RandomLevelString(string ignore = "", string folder = "deathmatch")
    {
        return RandomLevelString(ignore, folder, forceCustom: false);
    }

    public static string RandomLevelString(string ignore, string folder, bool forceCustom)
    {
        List<string> levels = new List<string>();
        if (_fourPlayerLevels == null || Network.isActive != _prevNetworkSetting || TeamSelect2.eightPlayersActive != _prevEightPlayerSetting)
        {
            _prevNetworkSetting = Network.isActive;
            _prevEightPlayerSetting = TeamSelect2.eightPlayersActive;
            _fourPlayerLevels = Content.GetLevels(folder, LevelLocation.Content, pRecursive: false, Network.isActive, pEightPlayer: false);
            _eightPlayerNonRestrictedLevels = Content.GetLevels(folder, LevelLocation.Content, pRecursive: false, Network.isActive, pEightPlayer: true, pAllowNonRestrictedEightPlayer: true);
            _eightPlayerAllLevels = Content.GetLevels(folder, LevelLocation.Content, pRecursive: false, Network.isActive, pEightPlayer: true);
            _rareLevels = Content.GetLevels(folder + "/rare", LevelLocation.Content, pRecursive: false, Network.isActive, TeamSelect2.eightPlayersActive);
            if (Network.isActive)
            {
                _fourPlayerLevels.AddRange(Content.GetLevels(folder + "/online_only", LevelLocation.Content, pRecursive: false, Network.isActive, pEightPlayer: false));
                _eightPlayerNonRestrictedLevels.AddRange(Content.GetLevels(folder + "/online_only", LevelLocation.Content, pRecursive: false, Network.isActive, pEightPlayer: false, pAllowNonRestrictedEightPlayer: true));
                _eightPlayerAllLevels.AddRange(Content.GetLevels(folder + "/online_only", LevelLocation.Content, pRecursive: false, Network.isActive, pEightPlayer: true));
            }
        }
        if (TeamSelect2.eightPlayersActive)
        {
            levels.AddRange(_eightPlayerAllLevels);
        }
        else
        {
            levels.AddRange(_fourPlayerLevels);
            levels.AddRange(_eightPlayerNonRestrictedLevels);
        }
        DateTime now = MonoMain.GetLocalTime();
        if (DateTime.Now.Month == 12)
        {
            if (now.Day <= 25)
            {
                levels.Add("23ec9c56-dbcc-4384-9507-5b0f80cb0111");
            }
            else if (now.Day == 24 || now.Day == 25)
            {
                levels.Add("23ec9c56-dbcc-4384-9507-5b0f80cb0111");
                levels.Add("23ec9c56-dbcc-4384-9507-5b0f80cb0111");
                levels.Add("23ec9c56-dbcc-4384-9507-5b0f80cb0111");
            }
        }
        if (ignore != "")
        {
            levels.AddRange(_rareLevels);
        }
        if (TeamSelect2.normalMapPercent != 100 || forceCustom)
        {
            MapRollGroup highest = null;
            MapRollGroup winner = null;
            if (forceCustom)
            {
                winner = new MapRollGroup
                {
                    type = MapRollType.Custom
                };
            }
            else
            {
                int randomPercent = TeamSelect2.randomMapPercent;
                if (_lastLevelWasPyramid)
                {
                    randomPercent = (int)((float)randomPercent * 0.5f);
                }
                foreach (MapRollGroup r in new List<MapRollGroup>
                {
                    new MapRollGroup
                    {
                        type = MapRollType.Normal,
                        chance = TeamSelect2.normalMapPercent
                    },
                    new MapRollGroup
                    {
                        type = MapRollType.Random,
                        chance = randomPercent
                    },
                    new MapRollGroup
                    {
                        type = MapRollType.Custom,
                        chance = userMapsPercent
                    },
                    new MapRollGroup
                    {
                        type = MapRollType.Internet,
                        chance = TeamSelect2.workshopMapPercent
                    }
                }.OrderBy((MapRollGroup x) => Rando.Int(2147483646)))
                {
                    if ((r.type != MapRollType.Custom || Editor.customLevelCount != 0) && (r.type != MapRollType.Internet || RandomLevelDownloader.PeekNextLevel() != null))
                    {
                        if (highest == null || r.chance > highest.chance || (highest.chance == 0 && r.chance == 0 && r.type == MapRollType.Normal))
                        {
                            highest = r;
                        }
                        if (Rando.Int(100) < r.chance && (winner == null || r.chance < winner.chance))
                        {
                            winner = r;
                        }
                    }
                }
                if (winner == null)
                {
                    winner = highest;
                }
            }
            if (winner.type == MapRollType.Custom && Editor.customLevelCount == 0)
            {
                winner.type = MapRollType.Normal;
            }
            if (winner.type == MapRollType.Random)
            {
                return "RANDOM";
            }
            if (winner.type == MapRollType.Internet)
            {
                return "WORKSHOP";
            }
            if (winner.type == MapRollType.Custom)
            {
                levels.Clear();
                if (Network.isActive && Network.isServer && (bool)TeamSelect2.GetMatchSetting("clientlevelsenabled").value)
                {
                    Profile found = null;
                    int tries = 0;
                    do
                    {
                        clientLevelRoundRobin++;
                        foreach (Profile p in DuckNetwork.profiles)
                        {
                            if (p.connection != null && p.slotType != SlotType.Local && p.networkIndex == clientLevelRoundRobin % GameLevel.NumberOfDucks && (p.connection != DuckNetwork.localConnection || Editor.activatedLevels.Count != 0) && (p.connection == DuckNetwork.localConnection || p.numClientCustomLevels != 0))
                            {
                                found = p;
                                break;
                            }
                        }
                        tries++;
                        if (tries > 10)
                        {
                            return RandomLevelString(ignore, folder);
                        }
                    }
                    while (found == null);
                    if (found.connection != DuckNetwork.localConnection)
                    {
                        return found.networkIndex + ".client";
                    }
                }
                foreach (string s in Editor.activatedLevels)
                {
                    levels.Add(s + ".custom");
                }
                if (levels.Count == 0 && forceCustom)
                {
                    return "";
                }
            }
        }
        if ((float)_recentLevels.Count > (float)levels.Count * 0.8f)
        {
            _recentLevels.Clear();
        }
        List<string> validLevels = new List<string>();
        validLevels.AddRange(levels);
        _lastLevelWasPyramid = false;
        string curLev = "";
        while (curLev == "")
        {
            if (validLevels.Count == 0)
            {
                curLev = "RANDOM";
                break;
            }
            if (levels.Count == 0 && _recentLevels.Count > 0)
            {
                curLev = _recentLevels.Dequeue();
                if (!validLevels.Contains(curLev))
                {
                    curLev = "";
                }
                continue;
            }
            if (levels.Count == 0)
            {
                curLev = validLevels[0];
                continue;
            }
            curLev = levels[Rando.Int(levels.Count() - 1)];
            if (curLev == ignore && levels.Count > 1)
            {
                levels.Remove(curLev);
                curLev = "";
            }
            else if (!TeamSelect2.eightPlayersActive && _eightPlayerNonRestrictedLevels.Contains(curLev) && Rando.Float(1f) > 0.2f)
            {
                levels.Remove(curLev);
                curLev = "";
            }
            else if (!_rareLevels.Contains(curLev) || Rando.Float(1f) > 0.75f)
            {
                if (_recentLevels.Contains(curLev))
                {
                    if (_recentLevels.LastOrDefault() == curLev)
                    {
                        levels.Remove(curLev);
                        curLev = "";
                    }
                    else if (Rando.Float(1f) < 0.95f)
                    {
                        levels.Remove(curLev);
                        curLev = "";
                    }
                }
            }
            else
            {
                levels.Remove(curLev);
                curLev = "";
            }
        }
        if (curLev != "RANDOM")
        {
            _recentLevels.Enqueue(curLev);
        }
        else
        {
            _lastLevelWasPyramid = true;
        }
        if (curLev.EndsWith(".custom"))
        {
            LevelData dat = DuckFile.LoadLevel(curLev.Substring(0, curLev.Length - 7));
            if (dat != null)
            {
                curLev = dat.metaData.guid + ".custom";
                if (Content.GetLevel(dat.metaData.guid, LevelLocation.Custom) == null)
                {
                    Content.MapLevel(dat.metaData.guid, dat, LevelLocation.Custom);
                }
            }
        }
        if (string.IsNullOrWhiteSpace(curLev))
        {
            return "RANDOM";
        }
        return curLev;
    }

    public override void Update()
    {
        if (Graphics.fade > 0.9f && Input.Pressed("START") && !NetworkDebugger.enabled)
        {
            _pauseGroup.Open();
            _pauseMenu.Open();
            MonoMain.pauseMenu = _pauseGroup;
            if (!_paused)
            {
                Music.Pause();
                SFX.Play("pause", 0.6f);
                _paused = true;
            }
            return;
        }
        if (_paused && MonoMain.pauseMenu == null)
        {
            _paused = false;
            SFX.Play("resume", 0.6f);
            Music.Resume();
        }
        if (_quit.value)
        {
            Graphics.fade -= 0.04f;
            if (Graphics.fade < 0.01f)
            {
                Level.current = new TitleScreen();
            }
            return;
        }
        if (Music.finished)
        {
            _wait -= 0.0006f;
        }
        if (!_matchOver)
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
                    break;
                }
            }
            if (liveTeams.Count <= 1)
            {
                _matchOver = true;
                numMatches++;
                if (numMatches >= roundsBetweenIntermission || showdown)
                {
                    numMatches = 0;
                }
            }
        }
        if (_matchOver)
        {
            _deadTimer -= 0.005f;
        }
        if (_deadTimer < 0.5f && !_addedPoints)
        {
            List<Team> convertedTeams = new List<Team>();
            List<Team> liveTeams2 = new List<Team>();
            foreach (Team t2 in Teams.all)
            {
                foreach (Profile p2 in t2.activeProfiles)
                {
                    if (p2.duck == null || p2.duck.dead)
                    {
                        continue;
                    }
                    if (p2.duck.converted != null && p2.duck.converted.profile.team != p2.team)
                    {
                        if (!liveTeams2.Contains(p2.duck.converted.profile.team))
                        {
                            liveTeams2.Add(p2.duck.converted.profile.team);
                        }
                        if (!convertedTeams.Contains(p2.duck.profile.team))
                        {
                            convertedTeams.Add(p2.duck.profile.team);
                        }
                    }
                    else if (!liveTeams2.Contains(t2))
                    {
                        liveTeams2.Add(t2);
                    }
                    break;
                }
            }
            if (liveTeams2.Count <= 1)
            {
                Highlights.highlightRatingMultiplier = 0f;
                lastWinners.Clear();
                if (liveTeams2.Count > 0)
                {
                    Event.Log(new RoundEndEvent());
                    SFX.Play("scoreDing", 0.8f);
                    if (!TeamSelect2.KillsForPoints)
                    {
                        liveTeams2.AddRange(convertedTeams);
                        foreach (Team item in liveTeams2)
                        {
                            foreach (Profile p3 in item.activeProfiles)
                            {
                                if (p3.duck != null && !p3.duck.dead)
                                {
                                    lastWinners.Add(p3);
                                    p3.stats.lastWon = DateTime.Now;
                                    p3.stats.matchesWon++;
                                    Profile realProfile = p3;
                                    if (p3.duck.converted != null)
                                    {
                                        realProfile = p3.duck.converted.profile;
                                    }
                                    PlusOne plusOne = new PlusOne(0f, 0f, realProfile);
                                    plusOne._duck = p3.duck;
                                    plusOne.anchor = p3.duck;
                                    plusOne.anchor.offset = new Vector2(0f, -16f);
                                    Level.Add(plusOne);
                                }
                            }
                        }
                        if (Network.isActive && Network.isServer)
                        {
                            Send.Message(new NMAssignWin(lastWinners, null));
                        }
                        liveTeams2.First().score++;
                    }
                }
            }
            _addedPoints = true;
        }
        if (_deadTimer < 0.1f && !_endedHighlights)
        {
            _endedHighlights = true;
            Highlights.FinishRound();
        }
        if (!(_deadTimer < 0f) || switched || Network.isActive)
        {
            return;
        }
        foreach (Team item2 in Teams.all)
        {
            foreach (Profile activeProfile in item2.activeProfiles)
            {
                Profiles.Save(activeProfile);
            }
        }
        int highestScore = 0;
        List<Team> winers = Teams.winning;
        if (winers.Count > 0)
        {
            highestScore = winers[0].score;
        }
        if (highestScore <= 4)
        {
            return;
        }
        foreach (Team t3 in Teams.active)
        {
            if (t3.score == highestScore)
            {
                continue;
            }
            if (t3.score < 1)
            {
                foreach (Profile activeProfile2 in t3.activeProfiles)
                {
                    Party.AddRandomPerk(activeProfile2);
                }
            }
            else if (t3.score < 2 && Rando.Float(1f) > 0.3f)
            {
                foreach (Profile activeProfile3 in t3.activeProfiles)
                {
                    Party.AddRandomPerk(activeProfile3);
                }
            }
            else if (t3.score < 5 && Rando.Float(1f) > 0.6f)
            {
                foreach (Profile activeProfile4 in t3.activeProfiles)
                {
                    Party.AddRandomPerk(activeProfile4);
                }
            }
            else if (t3.score < 7 && Rando.Float(1f) > 0.85f)
            {
                foreach (Profile activeProfile5 in t3.activeProfiles)
                {
                    Party.AddRandomPerk(activeProfile5);
                }
            }
            else
            {
                if (t3.score >= highestScore || !(Rando.Float(1f) > 0.9f))
                {
                    continue;
                }
                foreach (Profile activeProfile6 in t3.activeProfiles)
                {
                    Party.AddRandomPerk(activeProfile6);
                }
            }
        }
    }

    public void PlayMusic()
    {
        string text = Music.RandomTrack("InGame", _currentSong);
        Music.Play(text, looping: false);
        _currentSong = text;
        _wait = 1f;
    }

    private SpawnPoint AttemptTeamSpawn(Team team, List<SpawnPoint> usedSpawns, List<Duck> spawned)
    {
        _ = _level;
        List<TeamSpawn> validTeamSpawns = new List<TeamSpawn>();
        foreach (TeamSpawn s in _level.things[typeof(TeamSpawn)])
        {
            if (!usedSpawns.Contains(s))
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

    private SpawnPoint AttemptFreeSpawn(Profile profile, List<SpawnPoint> usedSpawns, List<Duck> spawned)
    {
        _ = _level;
        List<SpawnPoint> validFreeSpawns = new List<SpawnPoint>();
        foreach (FreeSpawn s in _level.things[typeof(FreeSpawn)])
        {
            if (!usedSpawns.Contains(s))
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

    private SpawnPoint AttemptAnySpawn(Profile profile, List<SpawnPoint> usedSpawns, List<Duck> spawned)
    {
        _ = _level;
        List<SpawnPoint> validFreeSpawns = new List<SpawnPoint>();
        foreach (SpawnPoint s in _level.things[typeof(SpawnPoint)])
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

    public List<Duck> SpawnPlayers(bool recordStats)
    {
        return Spawn.SpawnPlayers(recordStats);
    }

    public override void Draw()
    {
    }
}
