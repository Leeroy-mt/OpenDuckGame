using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuckGame;

public class GameLevel : XMLLevel, IHaveAVirtualTransition
{
    protected FollowCam _followCam;

    protected GameMode _mode;

    private RandomLevelNode _randomLevel;

    private bool _validityTest;

    private float _infoSlide;

    private float _infoWait;

    private bool _showInfo = true;

    public bool _editorTestMode;

    public string levelInputString;

    private static bool first;

    private bool _startedMatch;

    private static int _numberOfDucksSpawned;

    private int wait;

    public override string networkIdentifier => base.level;

    public FollowCam followCam => _followCam;

    public bool isRandom => _randomLevel != null;

    public bool matchOver
    {
        get
        {
            if (_mode != null)
            {
                return _mode.matchOver;
            }
            return true;
        }
    }

    public static int NumberOfDucks
    {
        get
        {
            if (_numberOfDucksSpawned < 2)
            {
                return 2;
            }
            return _numberOfDucksSpawned;
        }
        set
        {
            _numberOfDucksSpawned = value;
        }
    }

    public string displayName
    {
        get
        {
            string levelName = null;
            if (base.data.workshopData != null && base.data.workshopData.name != null && base.data.workshopData.name != "")
            {
                levelName = base.data.workshopData.name;
            }
            else if (base.data.GetPath() != "" && base.data.GetPath() != null)
            {
                levelName = Path.GetFileNameWithoutExtension(base.data.GetPath());
            }
            return levelName;
        }
    }

    public void SkipMatch()
    {
        if (Network.isActive && Network.isServer)
        {
            Send.Message(new NMSkipLevel());
        }
        if (_mode == null)
        {
            _mode = new DM();
        }
        _mode.SkipMatch();
    }

    public GameLevel(string lev, int seedVal = 0, bool validityTest = false, bool editorTestMode = false)
        : base(lev)
    {
        levelInputString = lev;
        _followCam = new FollowCam();
        _followCam.lerpMult = 1.2f;
        base.camera = _followCam;
        _validityTest = validityTest;
        if (Network.isActive)
        {
            _readyForTransition = false;
        }
        first = !first;
        if (seedVal != 0)
        {
            seed = seedVal;
        }
        _editorTestMode = editorTestMode;
    }

    public override string LevelNameData()
    {
        string nameData = base.LevelNameData();
        if (this != null)
        {
            nameData = nameData + "," + (isCustomLevel ? "1" : "0");
        }
        return nameData;
    }

    public override void Initialize()
    {
        TeamSelect2.QUACK3 = TeamSelect2.Enabled("QUACK3");
        Vote.ClearVotes();
        if (base.level == "RANDOM")
        {
            _randomLevel = LevelGenerator.MakeLevel(null, allowSymmetry: true, seed);
            seed = _randomLevel.seed;
        }
        base.Initialize();
        if (Network.isActive)
        {
            Level.core.gameInProgress = true;
        }
        if (_randomLevel != null)
        {
            GhostManager.context.ResetGhostIndex(networkIndex);
            _randomLevel.LoadParts(0f, 0f, this, seed);
            List<SpawnPoint> spawns = new List<SpawnPoint>();
            foreach (SpawnPoint spawn in base.things[typeof(SpawnPoint)])
            {
                spawns.Add(spawn);
            }
            List<SpawnPoint> chosenSpawns = new List<SpawnPoint>();
            for (int i = 0; i < 4; i++)
            {
                if (chosenSpawns.Count == 0)
                {
                    chosenSpawns.Add(spawns.ElementAt(Rando.Int(spawns.Count - 1)));
                    continue;
                }
                IOrderedEnumerable<SpawnPoint> ordered = spawns.OrderByDescending(delegate (SpawnPoint x)
                {
                    int num = 9999999;
                    foreach (SpawnPoint item in chosenSpawns)
                    {
                        num = (int)Math.Min((item.Position - x.Position).Length(), num);
                    }
                    return num;
                });
                chosenSpawns.Add(ordered.First());
            }
            foreach (SpawnPoint s in spawns)
            {
                if (!chosenSpawns.Contains(s))
                {
                    Level.Remove(s);
                }
            }
            foreach (Thing t in base.things)
            {
                if (Network.isActive && t.isStateObject)
                {
                    GhostManager.context.MakeGhost(t, -1, initLevel: true);
                    t.ghostType = Editor.IDToType[t.GetType()];
                }
            }
            Level.Add(new PyramidBackground(0f, 0f)
            {
                visible = false
            });
            base.Initialize();
        }
        base.things.RefreshState();
        if (_mode == null)
        {
            _mode = new DM(_validityTest, _editorTestMode);
        }
        _mode.DoInitialize();
        if (!Network.isServer)
        {
            return;
        }
        foreach (Duck item2 in _mode.PrepareSpawns())
        {
            item2.localSpawnVisible = false;
            item2.immobilized = true;
            Level.Add(item2);
        }
    }

    public virtual void MatchStart()
    {
        _startedMatch = true;
    }

    public override void Start()
    {
        _things.RefreshState();
        Vec2 p1 = new Vec2(9999f, -9999f);
        Vec2 p2 = Vec2.Zero;
        int numDucks = 0;
        foreach (Duck duck in base.things[typeof(Duck)])
        {
            followCam.Add(duck);
            if (duck.X < p1.X)
            {
                p1 = duck.Position;
            }
            p2 += duck.Position;
            numDucks++;
        }
        p2 /= (float)numDucks;
        followCam.Adjust();
    }

    protected override void OnTransferComplete(NetworkConnection c)
    {
        Level.current.things.RefreshState();
        Vec2 p1 = new Vec2(9999f, -9999f);
        Vec2 p2 = Vec2.Zero;
        int numDucks = 0;
        List<Duck> spawns = new List<Duck>();
        foreach (Duck duck in base.things[typeof(Duck)])
        {
            duck.localSpawnVisible = false;
            followCam.Add(duck);
            if (duck.X < p1.X)
            {
                p1 = duck.Position;
            }
            p2 += duck.Position;
            numDucks++;
            spawns.Add(duck);
        }
        p2 /= (float)numDucks;
        _numberOfDucksSpawned = numDucks;
        if (_numberOfDucksSpawned > 4)
        {
            TeamSelect2.eightPlayersActive = true;
        }
        followCam.Adjust();
        _mode.pendingSpawns = spawns;
        base.OnTransferComplete(c);
    }

    protected override void OnAllClientsReady()
    {
        if (Network.isServer)
        {
            Send.Message(new NMBeginLevel());
        }
        base.OnAllClientsReady();
    }

    public override void Update()
    {
        MonoMain.timeInMatches++;
        if (_mode != null)
        {
            _mode.DoUpdate();
        }
        if (_level == "RANDOM")
        {
            if (wait < 4)
            {
                wait++;
            }
            if (wait == 4)
            {
                wait++;
                foreach (AutoBlock item in base.things[typeof(AutoBlock)])
                {
                    item.PlaceBlock();
                }
                foreach (AutoPlatform item2 in base.things[typeof(AutoPlatform)])
                {
                    item2.PlaceBlock();
                    item2.UpdateNubbers();
                }
                foreach (BlockGroup item3 in base.things[typeof(BlockGroup)])
                {
                    foreach (Block bl in item3.blocks)
                    {
                        if (bl is AutoBlock)
                        {
                            (bl as AutoBlock).PlaceBlock();
                        }
                    }
                }
            }
        }
        base.Update();
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (_mode != null)
        {
            _mode.PostDrawLayer(layer);
        }
        if (layer == Layer.HUD && base.data != null && base.customLevel && !_waitingOnTransition)
        {
            drawsOverPauseMenu = true;
            if ((_showInfo && !GameMode.started) || MonoMain.pauseMenu != null)
            {
                _infoSlide = Lerp.Float(_infoSlide, 1f, 0.06f);
                if (_infoSlide > 0.95f)
                {
                    _infoWait += Maths.IncFrameTimer();
                    if (_infoWait > 2.5f)
                    {
                        _showInfo = false;
                    }
                }
            }
            else
            {
                _infoSlide = Lerp.Float(_infoSlide, 0f, 0.1f);
            }
            if (_infoSlide > 0f)
            {
                float distFromEdge = 10f;
                string levelName = displayName;
                if (synchronizedLevelName != null)
                {
                    levelName = synchronizedLevelName;
                }
                else if (levelName == null)
                {
                    levelName = "CUSTOM LEVEL";
                }
                float nameWidth = Graphics.GetStringWidth(levelName);
                float lerpDist = (nameWidth + distFromEdge + 12f) * (1f - _infoSlide);
                Vec2 backTL = new Vec2(0f - lerpDist, distFromEdge - 1f);
                Vec2 backBR = new Vec2(distFromEdge + nameWidth + 4f, distFromEdge + 10f);
                Graphics.DrawRect(backTL, backBR + new Vec2(0f - lerpDist, 0f), new Color(13, 130, 211), 0.95f);
                Graphics.DrawRect(backTL + new Vec2(-2f, 2f), backBR + new Vec2(0f - lerpDist + 2f, 2f), Colors.BlueGray, 0.9f);
                Graphics.DrawStringOutline(levelName, backTL + new Vec2(distFromEdge, 2f), Color.White, Color.Black, 1f);
                if (base.data.workshopData != null && base.data.workshopData.author != null && base.data.workshopData.author != "")
                {
                    string authorName = base.data.workshopData.author;
                    string text = "BY " + authorName;
                    float stringWidth = Graphics.GetStringWidth(text);
                    float lerpDist2 = (stringWidth + distFromEdge + 12f) * (1f - _infoSlide);
                    backTL = new Vec2(Layer.HUD.width - stringWidth - distFromEdge - 5f + lerpDist2, Layer.HUD.height - distFromEdge - 10f);
                    backBR = new Vec2(Layer.HUD.width + lerpDist2, Layer.HUD.height - distFromEdge + 1f);
                    Graphics.DrawRect(backTL, backBR, new Color(138, 38, 190), 0.95f);
                    Graphics.DrawRect(backTL + new Vec2(-2f, -2f), backBR + new Vec2(2f, -2f), Colors.BlueGray, 0.9f);
                    Graphics.DrawStringOutline(text, new Vec2(Layer.HUD.width - stringWidth - distFromEdge + lerpDist2, Layer.HUD.height - distFromEdge - 8f), Color.White, Color.Black, 1f);
                }
            }
        }
        base.PostDrawLayer(layer);
    }
}
