using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuckGame;

public class RockScoreboard : Level
{
    private static Level _returnLevel;

    private bool _afterHighlights;

    private ScoreBoardMode _mode;

    private ScoreBoardState _state = ScoreBoardState.Intro;

    private float _throwWait = 1f;

    private float _afterThrowWait = 1f;

    private Sprite _bleachers;

    private Sprite _bleacherSeats;

    private Sprite _intermissionText;

    private Sprite _winnerPost;

    private Sprite _winnerBanner;

    private BitmapFont _font;

    private FieldBackground _field;

    private WallLayer _wall;

    private FieldBackground _fieldForeground;

    private FieldBackground _fieldForeground2;

    private ContinueCountdown _netCountdown;

    private GinormoBoard _scoreBoard;

    private bool _shiftCamera;

    private bool _finished;

    private bool _getScreenshot;

    private Sprite _finalSprite;

    private float _intermissionSlide = 1f;

    private float _controlSlide;

    private int _controlMessage = -1;

    private CornerDisplay _continueHUD;

    private float _desiredScroll;

    private float _animWait = 1f;

    private float _backWait = 1f;

    private float _showScoreWait = 1f;

    private float _fieldWidth = 680f;

    private bool _skipFade;

    private float _winnerWait = 1f;

    private bool _matchOver;

    private bool _tie;

    private bool _viewBoard;

    private bool _quit;

    private bool _misfire;

    private float _cameraWait = 1f;

    private bool _takePicture;

    private int _playedBeeps;

    private bool _playedFlash;

    private float _cameraFadeVel;

    private int _flashSkipFrames;

    private float _fieldScroll;

    private SinWave _sin = 0.01f;

    private Crowd _crowd;

    public List<Slot3D> _slots = new List<Slot3D>();

    private Slot3D _highestSlot;

    private Team _winningTeam;

    public static RenderTarget2D finalImage;

    public static RenderTarget2D finalImage2;

    private float _backgroundFade;

    private Layer _sunLayer;

    private RenderTarget2D _sunshineTarget;

    private RenderTarget2D _screenTarget;

    private RenderTarget2D _pixelTarget;

    public static bool _sunEnabled = true;

    private RockWeather _weather;

    private List<InputObject> _inputs = new List<InputObject>();

    public static bool initializingDucks = false;

    public static bool wallMode = false;

    private Thing sunThing;

    private Thing rainbowThing;

    private Thing rainbowThing2;

    private static bool _drawingSunTarget = false;

    private static bool _drawingLighting = false;

    private static bool _drawingNormalTarget = false;

    private bool _hatSelect;

    private bool _focusRock;

    private bool _droppedConfetti;

    private float _confettiDrop;

    private bool _returnToScoreboard;

    private Material _sunshineMaterial;

    private Material _sunshineMaterialBare;

    public override string networkIdentifier
    {
        get
        {
            if (mode == ScoreBoardMode.ShowScores)
            {
                return "@ROCKTHROW|SHOWSCORE";
            }
            if (afterHighlights)
            {
                return "@ROCKTHROW|SHOWEND";
            }
            return "@ROCKTHROW|SHOWWINNER";
        }
    }

    public static Level returnLevel
    {
        get
        {
            if (_returnLevel == null)
            {
                _returnLevel = new GameLevel(Deathmatch.RandomLevelString());
            }
            return _returnLevel;
        }
    }

    public bool afterHighlights
    {
        get
        {
            return _afterHighlights;
        }
        set
        {
            _afterHighlights = value;
        }
    }

    public ScoreBoardMode mode => _mode;

    public Vec3 fieldAddColor
    {
        set
        {
            if (_field != null)
            {
                _field.colorAdd = value;
                _fieldForeground.colorAdd = value;
                _fieldForeground2.colorAdd = value;
                _wall.colorAdd = value;
            }
        }
    }

    public Vec3 fieldMulColor
    {
        set
        {
            if (_field != null)
            {
                _field.colorMul = value;
                _fieldForeground.colorMul = value;
                _fieldForeground2.colorMul = value;
                _wall.colorMul = value;
            }
        }
    }

    public ContinueCountdown netCountdown => _netCountdown;

    public float cameraY
    {
        get
        {
            return base.camera.y;
        }
        set
        {
            base.camera.y = value;
            _field.ypos = base.camera.y * 1.4f;
        }
    }

    public int controlMessage
    {
        get
        {
            return _controlMessage;
        }
        set
        {
            if (_controlMessage != value)
            {
                HUD.CloseAllCorners();
                if (value == 0)
                {
                    HUD.AddCornerControl(HUDCorner.BottomRight, "@START@SKIP");
                }
                else if (value > 0)
                {
                    if (!Network.isServer)
                    {
                        _continueHUD = HUD.AddCornerMessage(HUDCorner.BottomRight, "WAITING");
                    }
                    else
                    {
                        _continueHUD = HUD.AddCornerControl(HUDCorner.BottomRight, "@START@CONTINUE");
                        if (value > 1)
                        {
                            HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@QUIT");
                            HUD.AddCornerControl(HUDCorner.TopRight, "@MENU2@LOBBY");
                        }
                    }
                }
            }
            _controlMessage = value;
        }
    }

    public Vec2 sunPos => sunThing.position;

    public Layer sunLayer => _sunLayer;

    public static bool drawingSunTarget => _drawingSunTarget;

    public static bool drawingLighting => _drawingLighting;

    public static bool drawingNormalTarget => _drawingNormalTarget;

    public RockScoreboard(Level r = null, ScoreBoardMode mode = ScoreBoardMode.ShowScores, bool afterHighlights = false)
    {
        _afterHighlights = afterHighlights;
        if (Network.isServer)
        {
            _returnLevel = r;
        }
        _mode = mode;
        if (mode == ScoreBoardMode.ShowWinner)
        {
            _state = ScoreBoardState.None;
        }
    }

    public void SetWeather(Weather w)
    {
        if (_weather != null)
        {
            _weather.SetWeather(w);
        }
    }

    public override void SendLevelData(NetworkConnection c)
    {
        if (Network.isServer)
        {
            Send.Message(new NMCrowdData(_crowd.NetSerialize()), c);
            Send.Message(new NMWeatherData(_weather.NetSerialize()), c);
        }
    }

    public override void OnMessage(NetMessage message)
    {
        if (message is NMCrowdData && Network.isClient)
        {
            _crowd.NetDeserialize((message as NMCrowdData).data);
        }
        if (message is NMWeatherData && Network.isClient)
        {
            _weather.NetDeserialize((message as NMWeatherData).data);
        }
    }

    public InputProfile GetNetInput(sbyte index)
    {
        if (index >= _inputs.Count || _inputs[index].duckProfile == null || _inputs[index].duckProfile.inputProfile == null)
        {
            return new InputProfile();
        }
        return _inputs[index].duckProfile.inputProfile;
    }

    public override void Initialize()
    {
        if (Network.isActive && Network.isServer && _mode == ScoreBoardMode.ShowScores)
        {
            int idx = 0;
            foreach (Profile p in DuckNetwork.profiles)
            {
                if (p.connection != null && p.slotType != SlotType.Spectator)
                {
                    InputObject o = new InputObject();
                    o.profileNumber = (sbyte)idx;
                    Level.Add(o);
                    _inputs.Add(o);
                    idx++;
                }
            }
        }
        HighlightLevel.didSkip = false;
        if (_afterHighlights)
        {
            _skipFade = true;
        }
        _weather = new RockWeather(this);
        _weather.Start();
        Level.Add(_weather);
        for (int i = 0; i < 350; i++)
        {
            _weather.Update();
        }
        if (_sunEnabled)
        {
            float aspect = 0.5625f;
            _sunshineTarget = new RenderTarget2D(Graphics.width / 12, (int)((float)Graphics.width * aspect) / 12);
            _screenTarget = new RenderTarget2D(Graphics.width, (int)((float)Graphics.width * aspect));
            _pixelTarget = new RenderTarget2D(160, (int)(320f * aspect / 2f));
            _sunLayer = new Layer("SUN LAYER", 99999);
            Layer.Add(_sunLayer);
            Thing tthing = new SpriteThing(150f, 120f, new Sprite("sun"));
            tthing.z = -9999f;
            tthing.depth = -0.99f;
            tthing.layer = _sunLayer;
            tthing.xscale = 1f;
            tthing.yscale = 1f;
            tthing.collisionSize = new Vec2(1f, 1f);
            tthing.collisionOffset = new Vec2(0f, 0f);
            Level.Add(tthing);
            sunThing = tthing;
            SpriteThing rbow = new SpriteThing(150f, 80f, new Sprite("rainbow"));
            rbow.alpha = 0.15f;
            rbow.z = -9999f;
            rbow.depth = -0.99f;
            rbow.layer = _sunLayer;
            rbow.xscale = 1f;
            rbow.yscale = 1f;
            rbow.color = new Color(100, 100, 100);
            rbow.collisionSize = new Vec2(1f, 1f);
            rbow.collisionOffset = new Vec2(0f, 0f);
            Level.Add(rbow);
            rainbowThing = rbow;
            rainbowThing.visible = false;
            SpriteThing rbow2 = new SpriteThing(150f, 80f, new Sprite("rainbow"));
            rbow2.z = -9999f;
            rbow2.depth = -0.99f;
            rbow2.layer = _sunLayer;
            rbow2.xscale = 1f;
            rbow2.yscale = 1f;
            rbow2.color = new Color(255, 255, 255, 90);
            rbow2.collisionSize = new Vec2(1f, 1f);
            rbow2.collisionOffset = new Vec2(0f, 0f);
            Level.Add(rbow2);
            rainbowThing2 = rbow2;
            rainbowThing2.visible = false;
        }
        List<Team> random = Teams.allRandomized;
        if (!Network.isActive && returnLevel == null)
        {
            random[0].Join(Profiles.DefaultPlayer1);
            random[1].Join(Profiles.DefaultPlayer2);
            random[0].score = 10;
            random[1].score = 2;
            Teams.Player3.score = 3;
            Teams.Player4.score = 4;
        }
        _crowd = new Crowd();
        Level.Add(_crowd);
        Crowd.mood = Mood.Calm;
        _field = new FieldBackground("FIELD", 9999);
        Layer.Add(_field);
        _bleacherSeats = new Sprite("bleacherSeats");
        if (RockWeather.weather == Weather.Snowing)
        {
            _bleachers = new Sprite("bleacherBackSnow");
        }
        else
        {
            _bleachers = new Sprite("bleacherBack");
        }
        _bleachers.center = new Vec2(_bleachers.w / 2, _bleachers.height - 3);
        _intermissionText = new Sprite("rockThrow/intermission");
        _winnerPost = new Sprite("rockThrow/winnerPost");
        _winnerBanner = new Sprite("rockThrow/winnerBanner");
        _font = new BitmapFont("biosFont", 8);
        List<Team> teams = new List<Team>();
        foreach (Team t in Teams.all)
        {
            if (t.activeProfiles.Count > 0)
            {
                teams.Add(t);
            }
        }
        int count = 0;
        foreach (Team t2 in Teams.all)
        {
            count += t2.activeProfiles.Count;
        }
        if (_mode == ScoreBoardMode.ShowWinner)
        {
            Vote.ClearVotes();
        }
        foreach (Team tea in teams)
        {
            tea.rockScore = tea.score;
            if (wallMode && _mode == ScoreBoardMode.ShowScores)
            {
                tea.score = Math.Min(tea.score, GameMode.winsPerSet);
            }
            if (_mode == ScoreBoardMode.ShowWinner || _afterHighlights)
            {
                continue;
            }
            foreach (Profile activeProfile in tea.activeProfiles)
            {
                Vote.RegisterVote(activeProfile, VoteType.None);
            }
        }
        if (Network.isActive)
        {
            Level.Add(new HostTable(160f, 170f));
        }
        bool smallMode = teams.Count > 4;
        if (_mode == ScoreBoardMode.ShowScores)
        {
            _intermissionSlide = 1f;
            Graphics.fade = 1f;
            Layer.Game.fade = 0f;
            Layer.Background.fade = 0f;
            Crowd.UpdateFans();
            int index = 0;
            int highestScore = 0;
            foreach (Team t3 in teams)
            {
                float xpos = 223f;
                float ypos = 0f;
                float mul = 26f;
                if (index % 4 == 1)
                {
                    mul = 24f;
                }
                else if (index % 4 == 2)
                {
                    mul = 27f;
                }
                else if (index % 4 == 3)
                {
                    mul = 30f;
                }
                float zpos = 158f - (float)(index % 4) * mul;
                if (index > 3)
                {
                    zpos -= 12f;
                }
                Depth deep = zpos / 200f;
                int dif = t3.prevScoreboardScore;
                int total = GameMode.winsPerSet * 2;
                int rlScore = t3.score;
                if (wallMode && rlScore > GameMode.winsPerSet)
                {
                    rlScore = GameMode.winsPerSet;
                }
                _slots.Add(new Slot3D());
                if (rlScore >= GameMode.winsPerSet && rlScore == highestScore)
                {
                    _tie = true;
                }
                else if (rlScore >= GameMode.winsPerSet && rlScore > highestScore)
                {
                    _tie = false;
                    highestScore = rlScore;
                    _highestSlot = _slots[_slots.Count - 1];
                }
                List<Profile> sortedList = new List<Profile>();
                Profile activeThrower = null;
                bool nextIsThrower = false;
                foreach (Profile p2 in t3.activeProfiles)
                {
                    if (nextIsThrower)
                    {
                        activeThrower = p2;
                        nextIsThrower = false;
                    }
                    if (p2.wasRockThrower)
                    {
                        p2.wasRockThrower = false;
                        nextIsThrower = true;
                    }
                    sortedList.Add(p2);
                }
                if (activeThrower == null)
                {
                    activeThrower = t3.activeProfiles[0];
                }
                sortedList.Remove(activeThrower);
                sortedList.Insert(0, activeThrower);
                activeThrower.wasRockThrower = true;
                byte plane = (byte)(_slots.Count - 1);
                int duckPos = 0;
                foreach (Profile p3 in sortedList)
                {
                    if (p3 == activeThrower)
                    {
                        initializingDucks = true;
                        _slots[plane].duck = new RockThrowDuck(xpos - (float)(duckPos * 10), ypos - 16f, p3);
                        _slots[plane].duck.planeOfExistence = plane;
                        _slots[plane].duck.ignoreGhosting = true;
                        _slots[plane].duck.forceMindControl = true;
                        Level.Add(_slots[plane].duck);
                        _slots[plane].duck.connection = DuckNetwork.localConnection;
                        initializingDucks = false;
                        if (_slots[_slots.Count - 1].duck.GetEquipment(typeof(TeamHat)) is TeamHat h)
                        {
                            h.ignoreGhosting = true;
                        }
                        _slots[_slots.Count - 1].duck.z = zpos;
                        _slots[_slots.Count - 1].duck.depth = deep;
                        _slots[_slots.Count - 1].ai = new DuckAI(p3.inputProfile);
                        if (Network.isActive && p3.connection != DuckNetwork.localConnection)
                        {
                            _slots[_slots.Count - 1].ai._manualQuack = GetNetInput((sbyte)p3.networkIndex);
                        }
                        _slots[_slots.Count - 1].duck.derpMindControl = false;
                        _slots[_slots.Count - 1].duck.mindControl = _slots[_slots.Count - 1].ai;
                        _slots[_slots.Count - 1].rock = new ScoreRock(xpos + 18f + (float)dif / (float)total * _fieldWidth, ypos, p3);
                        _slots[_slots.Count - 1].rock.planeOfExistence = plane;
                        _slots[_slots.Count - 1].rock.ignoreGhosting = true;
                        Level.Add(_slots[_slots.Count - 1].rock);
                        _slots[_slots.Count - 1].rock.z = zpos;
                        _slots[_slots.Count - 1].rock.depth = _slots[_slots.Count - 1].duck.depth + 1;
                        _slots[_slots.Count - 1].rock.grounded = true;
                        _slots[_slots.Count - 1].duck.isRockThrowDuck = true;
                    }
                    else
                    {
                        initializingDucks = true;
                        Duck d = new RockThrowDuck(xpos - (float)(duckPos * 12), ypos - 16f, p3);
                        d.forceMindControl = true;
                        d.planeOfExistence = plane;
                        d.ignoreGhosting = true;
                        Level.Add(d);
                        initializingDucks = false;
                        d.depth = deep;
                        d.z = zpos;
                        d.derpMindControl = false;
                        DuckAI ai = new DuckAI(p3.inputProfile);
                        if (Network.isActive && p3.connection != DuckNetwork.localConnection)
                        {
                            ai._manualQuack = GetNetInput((sbyte)p3.networkIndex);
                        }
                        d.mindControl = ai;
                        d.isRockThrowDuck = true;
                        d.connection = DuckNetwork.localConnection;
                        _slots[_slots.Count - 1].subDucks.Add(d);
                        _slots[_slots.Count - 1].subAIs.Add(ai);
                    }
                    duckPos++;
                }
                _slots[_slots.Count - 1].slotIndex = index;
                _slots[_slots.Count - 1].startX = xpos;
                index++;
            }
            for (int j = 0; j < DG.MaxPlayers; j++)
            {
                Level.Add(new Block(-50f, 0f, 1200f, 32f)
                {
                    planeOfExistence = (byte)j
                });
            }
            if (!_tie && highestScore > 0)
            {
                _matchOver = true;
            }
            if (_tie)
            {
                GameMode.showdown = true;
            }
        }
        else if (_mode == ScoreBoardMode.ShowWinner)
        {
            Level.core.gameFinished = true;
            PurpleBlock.Reset();
            Level.core.gameInProgress = false;
            if (Teams.active.Count > 1 && !_afterHighlights)
            {
                Global.data.matchesPlayed += 1;
                Global.WinMatch(Teams.winning[0]);
                if (Network.isActive)
                {
                    foreach (Profile activeProfile2 in Teams.winning[0].activeProfiles)
                    {
                        if (activeProfile2.connection == DuckNetwork.localConnection)
                        {
                            DuckNetwork.GiveXP("Won Match", 0, 150);
                            break;
                        }
                    }
                    if (DuckNetwork.localProfile != null && DuckNetwork.localProfile.slotType == SlotType.Spectator && DuckNetwork.profiles.Where((Profile x) => x.connection == DuckNetwork.localConnection).Count() == 1 && DuckNetwork._xpEarned.Count == 0)
                    {
                        DuckNetwork.GiveXP("Observer Bonus", 0, 50);
                    }
                }
                DuckNetwork.finishedMatch = true;
                if (GameMode.winsPerSet > (int)Global.data.longestMatchPlayed)
                {
                    Global.data.longestMatchPlayed.valueInt = GameMode.winsPerSet;
                }
            }
            _intermissionSlide = 0f;
            teams.Sort((Team a, Team b) => (a.score != b.score) ? ((a.score < b.score) ? 1 : (-1)) : 0);
            float center = 160f - (float)(teams.Count * 42 / 2) + 21f;
            if (smallMode)
            {
                center = 160f - (float)(teams.Count * 24 / 2) + 12f;
            }
            foreach (Team item in Teams.all)
            {
                item.prevScoreboardScore = 0;
            }
            List<List<Team>> positions = new List<List<Team>>();
            foreach (Team t4 in teams)
            {
                int curVal = t4.score;
                bool inserted = false;
                for (int i2 = 0; i2 < positions.Count; i2++)
                {
                    if (positions[i2][0].score < curVal)
                    {
                        positions.Insert(i2, new List<Team>());
                        positions[i2].Add(t4);
                        inserted = true;
                        break;
                    }
                    if (positions[i2][0].score == curVal)
                    {
                        positions[i2].Add(t4);
                        inserted = true;
                        break;
                    }
                }
                if (!inserted)
                {
                    positions.Add(new List<Team>());
                    positions.Last().Add(t4);
                }
            }
            _winningTeam = teams[0];
            controlMessage = 1;
            _state = ScoreBoardState.None;
            Crowd.mood = Mood.Dead;
            bool localWin = false;
            if (!_afterHighlights)
            {
                if (Network.isServer)
                {
                    Level.Add(new FloorWindow(10f, -5f));
                    Level.Add(new Trombone(10f, -15f));
                    Level.Add(new Saxaphone(14f, -15f));
                    Level.Add(new Trumpet(6f, -15f));
                    Level.Add(new Trumpet(8f, -15f));
                }
                if (Network.isActive)
                {
                    Global.data.onlineMatches.valueInt++;
                }
                int placeIndex = 0;
                int pedestalIndex = 0;
                foreach (List<Team> item2 in positions)
                {
                    foreach (Team t5 in item2)
                    {
                        Level.Add(new Pedestal(center + (float)(pedestalIndex * (smallMode ? 24 : 42)), 150f, t5, placeIndex, smallMode));
                        pedestalIndex++;
                    }
                    placeIndex++;
                }
                if (_winningTeam.activeProfiles.Count > 1)
                {
                    _winningTeam.wins++;
                }
                else
                {
                    _winningTeam.activeProfiles[0].wins++;
                }
                foreach (Profile p4 in _winningTeam.activeProfiles)
                {
                    p4.stats.trophiesWon++;
                    p4.stats.trophiesSinceLastWin = p4.stats.trophiesSinceLastWinCounter;
                    p4.stats.trophiesSinceLastWinCounter = 0;
                    if ((!Network.isActive || p4.connection == DuckNetwork.localConnection) && !localWin)
                    {
                        localWin = true;
                        if (Network.isActive)
                        {
                            Global.data.onlineWins.valueInt++;
                        }
                        if (p4.team.name == "SWACK")
                        {
                            Global.data.winsAsSwack.valueInt++;
                        }
                        if (p4.team.isHair)
                        {
                            Global.data.winsAsHair.valueInt++;
                        }
                    }
                    if (!Network.isActive && p4.team.name == "SWACK")
                    {
                        Global.data.winsAsSwack.valueInt++;
                    }
                }
                foreach (Team item3 in teams)
                {
                    foreach (Profile activeProfile3 in item3.activeProfiles)
                    {
                        activeProfile3.stats.trophiesSinceLastWinCounter++;
                        activeProfile3.stats.gamesPlayed++;
                    }
                }
                Main.lastLevel = "";
            }
        }
        _bottomRight = new Vec2(1000f, 1000f);
        lowestPoint = 1000f;
        _scoreBoard = new GinormoBoard(300f, -320f, (_mode == ScoreBoardMode.ShowScores) ? BoardMode.Points : BoardMode.Wins, teams.Count > 4);
        _scoreBoard.z = -130f;
        Level.Add(_scoreBoard);
        base.backgroundColor = new Color(0, 0, 0);
        Music.volume = 1f;
        if (_mode != ScoreBoardMode.ShowWinner && !_afterHighlights)
        {
            Music.Play("SportsTime");
        }
        cameraY = 0f;
        Sprite field = null;
        field = ((RockWeather.weather == Weather.Snowing) ? new Sprite("fieldNoiseSnow") : ((RockWeather.weather != Weather.Raining) ? new Sprite("fieldNoise") : new Sprite("fieldNoiseRain")));
        field.scale = new Vec2(4f, 4f);
        field.depth = 0.5f;
        field.y -= 16f;
        _field.AddSprite(field);
        Sprite fieldWall = null;
        fieldWall = new Sprite("fieldWall");
        fieldWall.scale = new Vec2(4f, 4f);
        fieldWall.depth = 0.5f;
        fieldWall.y -= 16f;
        _wall = new WallLayer("FIELDWALL", 80);
        if (wallMode)
        {
            _wall.AddWallSprite(fieldWall);
        }
        Layer.Add(_wall);
        _fieldForeground = new FieldBackground("FIELDFOREGROUND", 80);
        _fieldForeground.fieldHeight = -13f;
        Layer.Add(_fieldForeground);
        _fieldForeground2 = new FieldBackground("FIELDFOREGROUND2", 70);
        _fieldForeground2.fieldHeight = -15f;
        Layer.Add(_fieldForeground2);
        if (_mode != ScoreBoardMode.ShowWinner)
        {
            Sprite teevee = new Sprite("rockThrow/chairSeat");
            teevee.CenterOrigin();
            teevee.x = 300f;
            teevee.y = 20f;
            teevee.scale = new Vec2(1.2f, 1.2f);
            _fieldForeground.AddSprite(teevee);
            teevee = new Sprite("rockThrow/tableTop");
            teevee.CenterOrigin();
            teevee.x = 450f;
            teevee.y = 14f;
            teevee.scale = new Vec2(1.2f, 1.4f);
            _fieldForeground2.AddSprite(teevee);
            int ychange = -95;
            Sprite c = new Sprite("rockThrow/chairBottomBack");
            Level.Add(new SpriteThing(300f, -10f, c)
            {
                center = new Vec2(c.w / 2, c.h / 2),
                z = 106 + ychange,
                depth = 0.5f,
                layer = Layer.Background
            });
            c = new Sprite("rockThrow/chairBottom");
            Level.Add(new SpriteThing(300f, -6f, c)
            {
                center = new Vec2(c.w / 2, c.h / 2),
                z = 120 + ychange,
                depth = 0.8f,
                layer = Layer.Background
            });
            c = new Sprite("rockThrow/chairFront");
            Level.Add(new SpriteThing(300f, -9f, c)
            {
                center = new Vec2(c.w / 2, c.h / 2),
                z = 122 + ychange,
                depth = 0.9f,
                layer = Layer.Background
            });
            c = new Sprite("rockThrow/tableBottomBack");
            Level.Add(new SpriteThing(450f, -7f, c)
            {
                center = new Vec2(c.w / 2, c.h / 2),
                z = 106 + ychange,
                depth = 0.5f,
                layer = Layer.Background
            });
            c = new Sprite("rockThrow/tableBottom");
            Level.Add(new SpriteThing(450f, -7f, c)
            {
                center = new Vec2(c.w / 2, c.h / 2),
                z = 120 + ychange,
                depth = 0.8f,
                layer = Layer.Background
            });
            c = new Sprite("rockThrow/keg");
            Level.Add(new SpriteThing(460f, -24f, c)
            {
                center = new Vec2(c.w / 2, c.h / 2),
                z = 120 + ychange - 4,
                depth = -0.4f,
                layer = Layer.Game
            });
            c = new Sprite("rockThrow/cup");
            Level.Add(new SpriteThing(445f, -21f, c)
            {
                center = new Vec2(c.w / 2, c.h / 2),
                z = 120 + ychange - 6,
                depth = -0.5f,
                layer = Layer.Game
            });
            c = new Sprite("rockThrow/cup");
            Level.Add(new SpriteThing(437f, -20f, c)
            {
                center = new Vec2(c.w / 2, c.h / 2),
                z = 120 + ychange,
                depth = -0.3f,
                layer = Layer.Game
            });
            c = new Sprite("rockThrow/cup");
            Level.Add(new SpriteThing(472f, -20f, c)
            {
                center = new Vec2(c.w / 2, c.h / 2),
                z = 120 + ychange - 7,
                depth = -0.5f,
                layer = Layer.Game,
                angleDegrees = 80f
            });
        }
        for (int i3 = 0; i3 < 3; i3++)
        {
            Level.Add(new DistanceMarker(230 + i3 * 175, -25f, (int)Math.Round((float)(i3 * GameMode.winsPerSet) / 2f))
            {
                z = 0f,
                depth = 0.34f,
                layer = Layer.Background
            });
        }
        Sprite cs = null;
        cs = ((RockWeather.weather != Weather.Snowing) ? new Sprite("bleacherBack") : new Sprite("bleacherBackSnow"));
        for (int i4 = 0; i4 < 24; i4++)
        {
            SpriteThing obj = new SpriteThing(100 + i4 * (cs.w + 13), cs.h + 15, cs)
            {
                center = new Vec2(cs.w / 2, cs.h - 1)
            };
            obj.collisionOffset = new Vec2(obj.collisionOffset.x, -cs.h);
            obj.z = 0f;
            obj.depth = 0.33f;
            obj.layer = Layer.Background;
            Level.Add(obj);
        }
        Level.Add(new SpriteThing(600f, 0f, new Sprite("blackSquare"))
        {
            z = -90f,
            centery = 7f,
            depth = 0.1f,
            layer = Layer.Background,
            xscale = 100f,
            yscale = 7f
        });
        _weather.Update();
    }

    public void DoRender()
    {
        Color backColor = base.backgroundColor;
        if (NetworkDebugger.enabled)
        {
            _drawingSunTarget = true;
            Layer.Game.camera.width = 320f;
            Layer.Game.camera.height = 180f;
            _field.fade = Layer.Game.fade;
            _fieldForeground.fade = Layer.Game.fade;
            _wall.fade = Layer.Game.fade;
            _fieldForeground2.fade = Layer.Game.fade;
            base.backgroundColor = backColor;
            MonoMain.RenderGame(_screenTarget);
            _drawingSunTarget = false;
            return;
        }
        base.backgroundColor = Color.Black;
        _drawingSunTarget = true;
        float hudFade = Layer.HUD.fade;
        float consoleFade = Layer.Console.fade;
        float gameFade = Layer.Game.fade;
        float backFade = Layer.Background.fade;
        float fieldFade = _field.fade;
        Layer.Game.fade = 0f;
        Layer.Background.fade = 0f;
        Layer.Foreground.fade = 0f;
        _field.fade = 0f;
        _fieldForeground.fade = 0f;
        _wall.fade = 0f;
        _fieldForeground2.fade = 0f;
        Vec3 gameColorMul = Layer.Game.colorMul;
        Vec3 backColorMul = Layer.Background.colorMul;
        Layer.Game.colorMul = Vec3.One;
        Layer.Background.colorMul = Vec3.One;
        Layer.HUD.fade = 0f;
        Layer.Console.fade = 0f;
        fieldMulColor = Vec3.One;
        Vec3 colorAdd = Layer.Game.colorAdd;
        Layer.Game.colorAdd = Vec3.Zero;
        Layer.Background.colorAdd = Vec3.Zero;
        fieldAddColor = Vec3.Zero;
        Layer.blurry = true;
        sunThing.alpha = RockWeather.sunOpacity;
        ((SpriteThing)rainbowThing2).alpha = 0f;
        _drawingLighting = true;
        MonoMain.RenderGame(_sunshineTarget);
        _drawingLighting = false;
        if (_sunshineMaterialBare == null)
        {
            _sunshineMaterialBare = new MaterialSunshineBare();
        }
        Vec2 pos = sunPos;
        Vec3 newPos = new Vec3(pos.x, -9999f, pos.y);
        Viewport v = new Viewport(0, 0, (int)Layer.HUD.width, (int)Layer.HUD.height);
        newPos = v.Project(newPos, sunLayer.projection, sunLayer.view, Matrix.Identity);
        newPos.y -= 256f;
        newPos.x /= v.Width;
        newPos.y /= v.Height;
        _sunshineMaterialBare.effect.effect.Parameters["lightPos"].SetValue(new Vec2(newPos.x, newPos.y));
        _sunshineMaterialBare.effect.effect.Parameters["weight"].SetValue(1f);
        _sunshineMaterialBare.effect.effect.Parameters["density"].SetValue(0.4f);
        _sunshineMaterialBare.effect.effect.Parameters["decay"].SetValue(0.68f + RockWeather.sunGlow);
        _sunshineMaterialBare.effect.effect.Parameters["exposure"].SetValue(1f);
        Viewport viewport = Graphics.viewport;
        Graphics.SetRenderTarget(_pixelTarget);
        Graphics.viewport = new Viewport(0, 0, _pixelTarget.width, _pixelTarget.height);
        Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, base.camera.getMatrix());
        Graphics.material = _sunshineMaterialBare;
        Graphics.Draw(scale: new Vec2((float)(_pixelTarget.width * 2) / (float)_sunshineTarget.width), texture: _sunshineTarget, position: Vec2.Zero, sourceRectangle: null, color: Color.White, rotation: 0f, origin: Vec2.Zero, effects: SpriteEffects.None);
        Graphics.material = null;
        Graphics.screen.End();
        Graphics.SetRenderTarget(null);
        Graphics.viewport = viewport;
        Layer.blurry = false;
        Layer.HUD.fade = hudFade;
        Layer.Console.fade = consoleFade;
        Layer.Game.fade = gameFade;
        Layer.Foreground.fade = gameFade;
        Layer.Background.fade = backFade;
        _field.fade = fieldFade;
        _fieldForeground.fade = fieldFade;
        _fieldForeground2.fade = fieldFade;
        _wall.fade = fieldFade;
        Layer.Game.colorMul = gameColorMul;
        Layer.Background.colorMul = backColorMul;
        fieldMulColor = backColorMul;
        Layer.Game.colorAdd = colorAdd;
        Layer.Background.colorAdd = colorAdd;
        fieldAddColor = colorAdd;
        _drawingSunTarget = false;
        sunThing.x = 290f + RockWeather.sunPos.x * 8000f;
        sunThing.y = 10000f - RockWeather.sunPos.y * 8000f;
        Thing thing = rainbowThing;
        float y = (rainbowThing2.y = 2000f + _fieldScroll * 12f);
        thing.y = y;
        Thing thing2 = rainbowThing;
        y = (rainbowThing2.x = (0f - _field.scroll) * 15f + 6800f);
        thing2.x = y;
        rainbowThing.alpha = _weather.rainbowLight;
        ((SpriteThing)rainbowThing2).alpha = _weather.rainbowLight2;
        rainbowThing.visible = rainbowThing.alpha > 0.01f;
        rainbowThing2.visible = rainbowThing2.alpha > 0.01f;
        _drawingSunTarget = true;
        Layer.Game.camera.width = 320f;
        Layer.Game.camera.height = 180f;
        _field.fade = Layer.Game.fade;
        _fieldForeground.fade = Layer.Game.fade;
        _fieldForeground2.fade = Layer.Game.fade;
        _wall.fade = Layer.Game.fade;
        base.backgroundColor = backColor;
        _drawingNormalTarget = true;
        MonoMain.RenderGame(_screenTarget);
        _drawingNormalTarget = false;
        _drawingSunTarget = false;
    }

    public override void Update()
    {
        if (Network.isActive)
        {
            if (_netCountdown == null)
            {
                if (Network.isServer)
                {
                    if (DuckNetwork.isDedicatedServer)
                    {
                        _netCountdown = new ContinueCountdown((_mode == ScoreBoardMode.ShowScores) ? 4f : (_afterHighlights ? 5f : 10f));
                    }
                    else
                    {
                        _netCountdown = new ContinueCountdown((_mode == ScoreBoardMode.ShowScores) ? 5f : 15f);
                    }
                    Level.Add(_netCountdown);
                }
                else
                {
                    IEnumerable<Thing> cd = Level.current.things[typeof(ContinueCountdown)];
                    if (cd.Count() > 0)
                    {
                        _netCountdown = cd.ElementAt(0) as ContinueCountdown;
                    }
                }
            }
            else if (_continueHUD != null)
            {
                if (Network.isServer)
                {
                    _continueHUD.text = "@START@CONTINUE(" + (int)Math.Ceiling(_netCountdown.timer) + ")";
                    _netCountdown.UpdateTimer();
                }
                else
                {
                    _continueHUD.text = "WAITING(" + (int)Math.Ceiling(_netCountdown.timer) + ")";
                }
            }
            if (Network.isServer && netCountdown != null && !netCountdown.isServerForObject)
            {
                int oldValue = controlMessage;
                if (oldValue > 0)
                {
                    controlMessage = -1;
                    controlMessage = oldValue;
                }
                Thing.Fondle(netCountdown, DuckNetwork.localConnection);
            }
        }
        bool isServer = Network.isServer;
        Network.isServer = true;
        base.backgroundColor = new Color(139, 204, 248) * _backgroundFade;
        Layer.Game.fade = _backgroundFade;
        Layer.Background.fade = _backgroundFade;
        _backgroundFade = Lerp.Float(_backgroundFade, 1f, 0.02f);
        _field.rise = _fieldScroll;
        _fieldForeground.rise = _fieldScroll;
        _fieldForeground2.rise = _fieldScroll;
        _wall.rise = _fieldScroll;
        _bottomRight = new Vec2(1000f, 1000f);
        lowestPoint = 1000f;
        bool scrollDone = false;
        _field.scroll = Lerp.Float(_field.scroll, _desiredScroll, 6f);
        if (_field.scroll < 297f)
        {
            _field.scroll = 0f;
            scrollDone = true;
        }
        if (_field.scroll < 302f)
        {
            _field.scroll = 302f;
        }
        _fieldForeground.scroll = _field.scroll;
        _fieldForeground2.scroll = _field.scroll;
        _wall.scroll = _field.scroll;
        if (_state != ScoreBoardState.Transition)
        {
            if (_state == ScoreBoardState.Intro)
            {
                if (_animWait > 0f)
                {
                    _animWait -= 0.021f;
                }
                else
                {
                    Crowd.mood = Mood.Silent;
                    _intermissionSlide = Lerp.FloatSmooth(_intermissionSlide, 2.1f, 0.1f, 1.05f);
                    if (_intermissionSlide > 2.09f)
                    {
                        controlMessage = 0;
                        Vote.OpenVoting("", "", openCorners: false);
                        _state = ScoreBoardState.ThrowRocks;
                    }
                }
            }
            else if (_state == ScoreBoardState.MatchOver)
            {
                if (_highestSlot.duck.position.x < _highestSlot.rock.x - 16f)
                {
                    _highestSlot.ai.Release("LEFT");
                    _highestSlot.ai.Press("RIGHT");
                }
                if (_highestSlot.duck.position.x > _highestSlot.rock.x + 16f)
                {
                    _highestSlot.ai.Release("RIGHT");
                    _highestSlot.ai.Press("LEFT");
                }
                if (_highestSlot.duck.position.x > _highestSlot.rock.position.x - 16f || _highestSlot.duck.profile.team == null)
                {
                    _focusRock = true;
                }
                for (int i = 0; i < _highestSlot.subAIs.Count; i++)
                {
                    DuckAI ai = _highestSlot.subAIs[i];
                    Duck duck = _highestSlot.subDucks[i];
                    if (duck.position.x < _highestSlot.rock.x - 16f)
                    {
                        ai.Release("LEFT");
                        ai.Press("RIGHT");
                    }
                    if (duck.position.x > _highestSlot.rock.x + 16f)
                    {
                        ai.Release("RIGHT");
                        ai.Press("LEFT");
                    }
                }
                if (_focusRock)
                {
                    _highestSlot.ai.Release("JUMP");
                    if (Rando.Float(1f) > 0.98f)
                    {
                        _highestSlot.ai.Press("JUMP");
                    }
                    for (int j = 0; j < _highestSlot.subAIs.Count; j++)
                    {
                        DuckAI ai2 = _highestSlot.subAIs[j];
                        _ = _highestSlot.subDucks[j];
                        ai2.Release("JUMP");
                        if (Rando.Float(1f) > 0.98f)
                        {
                            ai2.Press("JUMP");
                        }
                    }
                    if (!_droppedConfetti)
                    {
                        _desiredScroll = _highestSlot.duck.position.x;
                        if (_desiredScroll >= _highestSlot.rock.position.x)
                        {
                            _desiredScroll = _highestSlot.rock.position.x;
                            Crowd.mood = Mood.Extatic;
                            _droppedConfetti = true;
                            for (int k = 0; k < 64; k++)
                            {
                                Level.Add(new Confetti(_confettiDrop + Rando.Float(-32f, 32f), _highestSlot.rock.y - 220f - Rando.Float(50f)));
                            }
                        }
                    }
                    if (Network.isServer && (Input.Pressed("START") || (_netCountdown != null && _netCountdown.timer <= 0f)))
                    {
                        _finished = true;
                    }
                    _winnerWait -= 0.007f;
                    if (_winnerWait < 0f)
                    {
                        _finished = true;
                    }
                }
                else
                {
                    _desiredScroll = _highestSlot.duck.position.x;
                    Crowd.mood = Mood.Excited;
                }
            }
        }
        if (_state == ScoreBoardState.ThrowRocks)
        {
            if (!_shiftCamera)
            {
                _controlSlide = Lerp.FloatSmooth(_controlSlide, 1f, 0.1f, 1.05f);
            }
            bool allowStateUpdate = true;
            foreach (Slot3D slot in _slots)
            {
                slot.follow = false;
                if (allowStateUpdate)
                {
                    if (slot.state != RockThrow.Finished)
                    {
                        allowStateUpdate = false;
                        slot.follow = true;
                    }
                    else if (slot == _slots[_slots.Count - 1])
                    {
                        if (_matchOver)
                        {
                            _skipFade = true;
                        }
                        else
                        {
                            _state = ScoreBoardState.ShowBoard;
                        }
                    }
                    if (slot.state == RockThrow.Idle)
                    {
                        slot.state = RockThrow.PickUpRock;
                    }
                    if (slot.state == RockThrow.PickUpRock)
                    {
                        if (slot.duck.position.x < slot.rock.position.x)
                        {
                            slot.ai.Press("RIGHT");
                        }
                        else
                        {
                            slot.state = RockThrow.ThrowRock;
                            slot.duck.position.x = slot.rock.position.x;
                            slot.duck.hSpeed = 0f;
                            if (TeamSelect2.eightPlayersActive)
                            {
                                _throwWait = 0.5f;
                            }
                            else
                            {
                                _throwWait = 0.9f;
                            }
                        }
                    }
                    if (slot.state == RockThrow.ThrowRock)
                    {
                        if (_throwWait > 0f)
                        {
                            _throwWait -= 0.08f;
                            slot.ai.Release("RIGHT");
                            slot.duck.GiveHoldable(slot.rock);
                            if (TeamSelect2.eightPlayersActive)
                            {
                                _afterThrowWait = 0.5f;
                            }
                            else
                            {
                                _afterThrowWait = 0.7f;
                            }
                        }
                        else
                        {
                            if (slot.duck.holdObject != null)
                            {
                                if (slot.duck.profile.team == null)
                                {
                                    slot.duck.Kill(new DTDisconnect(slot.duck));
                                }
                                else
                                {
                                    _misfire = false;
                                    slot.duck.ThrowItem();
                                    float dif = slot.duck.profile.team.rockScore;
                                    int total = GameMode.winsPerSet * 2;
                                    if (dif > (float)(total - 2))
                                    {
                                        dif = (float)(total - 2) + Math.Min((float)(slot.duck.profile.team.rockScore - GameMode.winsPerSet * 2) / 16f, 1f);
                                    }
                                    float distance = slot.startX + 30f + dif / (float)total * _fieldWidth - slot.rock.x;
                                    slot.rock.vSpeed = -2f - Maths.Clamp(distance / 300f, 0f, 1f) * 4f;
                                    float airTime = Math.Abs(2f * slot.rock.vSpeed) / slot.rock.currentGravity;
                                    _ = slot.rock.currentFriction;
                                    float reqSpeed = distance / airTime;
                                    slot.rock.frictionMult = 0f;
                                    slot.rock.grounded = false;
                                    slot.rock.hMax = 100f;
                                    slot.rock.vMax = 100f;
                                    if (slot.duck.profile.team.rockScore == slot.duck.profile.team.prevScoreboardScore)
                                    {
                                        reqSpeed = 0.3f;
                                        slot.rock.vSpeed = -0.6f;
                                        _misfire = true;
                                    }
                                    slot.rock.hSpeed = reqSpeed * 0.88f;
                                    if (wallMode && slot.duck.profile.team.rockScore > GameMode.winsPerSet)
                                    {
                                        slot.rock.hSpeed += 1f;
                                    }
                                }
                            }
                            if (slot.rock.grounded)
                            {
                                if (slot.duck.profile.team == null)
                                {
                                    slot.duck.Kill(new DTDisconnect(slot.duck));
                                }
                                float sub = 0.015f;
                                if (slot.duck.profile.team != null)
                                {
                                    int change = slot.duck.profile.team.rockScore - slot.duck.profile.team.prevScoreboardScore;
                                    if (change == 0)
                                    {
                                        Crowd.mood = Mood.Dead;
                                    }
                                    else if (change < 2)
                                    {
                                        Crowd.mood = Mood.Calm;
                                    }
                                    else if (change < 5)
                                    {
                                        Crowd.mood = Mood.Excited;
                                        sub = 0.013f;
                                    }
                                    else if (change < 99)
                                    {
                                        Crowd.mood = Mood.Extatic;
                                        sub = 0.01f;
                                    }
                                }
                                _ = GameMode.winsPerSet;
                                if (slot.rock.frictionMult == 0f)
                                {
                                    Sprite s = null;
                                    s = ((RockWeather.weather == Weather.Snowing) ? new Sprite("rockThrow/rockSmudgeSnow") : ((RockWeather.weather != Weather.Raining) ? new Sprite("rockThrow/rockSmudge") : new Sprite("rockThrow/rockSmudgeMud")));
                                    s.position = new Vec2(slot.rock.x - 12f, slot.rock.z - 10f);
                                    s.depth = 0.9f;
                                    s.xscale = 0.8f;
                                    s.yscale = 1.4f;
                                    s.alpha = 0.9f;
                                    _field.AddSprite(s);
                                }
                                slot.slideWait++;
                                if (slot.slideWait > 3 && slot.rock.hSpeed > 0f)
                                {
                                    Sprite s2 = null;
                                    s2 = ((RockWeather.weather == Weather.Snowing) ? new Sprite("rockThrow/rockSmearSnow") : ((RockWeather.weather != Weather.Raining) ? new Sprite("rockThrow/rockSmear") : new Sprite("rockThrow/rockSmearMud")));
                                    s2.position = new Vec2(slot.rock.x - 5f, slot.rock.z - 10f);
                                    s2.depth = 0.9f;
                                    s2.xscale = 0.6f;
                                    s2.yscale = 1.4f;
                                    s2.alpha = 0.9f;
                                    slot.slideWait = 0;
                                    _field.AddSprite(s2);
                                }
                                slot.rock.frictionMult = 4f;
                                _afterThrowWait -= sub;
                                if (_afterThrowWait < 0.4f)
                                {
                                    slot.state = RockThrow.ShowScore;
                                    SFX.Play("scoreDing");
                                    if (slot.duck.profile.team != null && wallMode && slot.duck.profile.team.rockScore > GameMode.winsPerSet)
                                    {
                                        slot.duck.profile.team.rockScore = GameMode.winsPerSet;
                                    }
                                    if (TeamSelect2.eightPlayersActive)
                                    {
                                        _showScoreWait = 0.5f;
                                    }
                                    else
                                    {
                                        _showScoreWait = 0.6f;
                                    }
                                    Crowd.ThrowHats(slot.duck.profile);
                                    if (!slot.showScore)
                                    {
                                        slot.showScore = true;
                                        Level.Add(new PointBoard(slot.rock, slot.duck.profile.team)
                                        {
                                            depth = slot.rock.depth + 1,
                                            z = slot.rock.z
                                        });
                                    }
                                }
                            }
                            else if (slot.duck.profile.team == null)
                            {
                                slot.duck.Kill(new DTDisconnect(slot.duck));
                            }
                            else
                            {
                                int dif2 = slot.duck.profile.team.rockScore;
                                int total2 = GameMode.winsPerSet * 2;
                                if (!_misfire && slot.rock.x > slot.startX + 30f + (float)dif2 / (float)total2 * _fieldWidth)
                                {
                                    slot.rock.x = slot.startX + 30f + (float)dif2 / (float)total2 * _fieldWidth;
                                }
                            }
                        }
                    }
                    if (slot.state == RockThrow.ShowScore)
                    {
                        _showScoreWait -= 0.016f;
                        if (_showScoreWait < 0f)
                        {
                            if (slot.duck.profile.team == null)
                            {
                                slot.state = RockThrow.Finished;
                                if (TeamSelect2.eightPlayersActive)
                                {
                                    _backWait = 0.5f;
                                }
                                else
                                {
                                    _backWait = 0.9f;
                                }
                            }
                            else
                            {
                                slot.state = RockThrow.RunBack;
                            }
                        }
                    }
                    if (slot.state == RockThrow.RunBack)
                    {
                        if (slot == _slots[_slots.Count - 1])
                        {
                            slot.follow = false;
                        }
                        if (slot.duck.position.x > slot.startX)
                        {
                            slot.ai.Press("LEFT");
                        }
                        else
                        {
                            slot.duck.position.x = slot.startX;
                            slot.duck.hSpeed = 0f;
                            slot.duck.offDir = 1;
                            slot.ai.Release("LEFT");
                            _backWait -= 0.05f;
                            Crowd.mood = Mood.Silent;
                            if (_backWait < 0f && (scrollDone || slot == _slots[_slots.Count - 1]))
                            {
                                slot.state = RockThrow.Finished;
                                if (TeamSelect2.eightPlayersActive)
                                {
                                    _backWait = 0.5f;
                                }
                                else
                                {
                                    _backWait = 0.9f;
                                }
                            }
                        }
                    }
                }
                if (slot.follow)
                {
                    if (slot.state == RockThrow.ThrowRock || slot.state == RockThrow.ShowScore)
                    {
                        _desiredScroll = slot.rock.position.x;
                    }
                    else
                    {
                        _desiredScroll = slot.duck.position.x;
                    }
                }
                if (Input.Pressed("START"))
                {
                    foreach (Profile d in Profiles.active)
                    {
                        if (d.inputProfile != null && d.inputProfile.Pressed("START") && (!Network.isActive || d.connection == DuckNetwork.localConnection))
                        {
                            Vote.RegisterVote(d, VoteType.Skip);
                            if (Network.isActive)
                            {
                                Send.Message(new NMVoteToSkip(d));
                            }
                        }
                    }
                }
                if (Vote.Passed(VoteType.Skip))
                {
                    _skipFade = true;
                }
            }
        }
        else
        {
            Vote.CloseVoting();
        }
        if (_state == ScoreBoardState.MatchOver)
        {
            Network.isServer = isServer;
            _controlSlide = Lerp.FloatSmooth(_controlSlide, (controlMessage == 1) ? 1f : 0f, 0.1f, 1.05f);
            if (_controlSlide < 0.01f)
            {
                controlMessage = -1;
            }
        }
        if (_state == ScoreBoardState.ShowBoard)
        {
            Network.isServer = isServer;
            _shiftCamera = true;
            _controlSlide = Lerp.FloatSmooth(_controlSlide, (controlMessage == 1) ? 1f : 0f, 0.1f, 1.05f);
            if (_controlSlide < 0.01f)
            {
                controlMessage = 1;
            }
        }
        if (_shiftCamera)
        {
            if (_state == ScoreBoardState.ThrowRocks)
            {
                _controlSlide = Lerp.FloatSmooth(_controlSlide, 0f, 0.1f, 1.05f);
            }
            _desiredScroll = -79f;
            if (_fieldScroll < 220f)
            {
                _fieldScroll += 4f;
            }
            else
            {
                if (_state == ScoreBoardState.ThrowRocks)
                {
                    _state = ScoreBoardState.ShowBoard;
                }
                if (!_scoreBoard.activated)
                {
                    _scoreBoard.Activate();
                }
                if (!_finished && isServer && (Input.Pressed("START") || (_netCountdown != null && _netCountdown.timer <= 0f)))
                {
                    _finished = true;
                }
                Crowd.mood = Mood.Dead;
            }
        }
        if (_skipFade)
        {
            Network.isServer = isServer;
            controlMessage = -1;
            Graphics.fade = Lerp.Float(Graphics.fade, 0f, 0.02f);
            if (Graphics.fade < 0.01f)
            {
                _skipFade = false;
                if (_mode == ScoreBoardMode.ShowScores)
                {
                    if (!_matchOver)
                    {
                        _state = ScoreBoardState.ShowBoard;
                        _fieldScroll = 220f;
                        _desiredScroll = -79f;
                        _field.scroll = _desiredScroll;
                    }
                    else
                    {
                        _state = ScoreBoardState.MatchOver;
                        _field.scroll = 0f;
                        foreach (Slot3D slot2 in _slots)
                        {
                            if (slot2.duck.profile.team != null)
                            {
                                float dif3 = slot2.duck.profile.team.rockScore;
                                int total3 = GameMode.winsPerSet * 2;
                                if (dif3 > (float)(total3 - 2))
                                {
                                    dif3 = (float)(total3 - 2) + Math.Min((float)(slot2.duck.profile.team.rockScore - GameMode.winsPerSet * 2) / 16f, 1f);
                                }
                                slot2.rock.x = slot2.startX + 30f + dif3 / (float)total3 * _fieldWidth;
                                if (wallMode && slot2.duck.profile.team.rockScore >= GameMode.winsPerSet)
                                {
                                    slot2.rock.x -= 10f;
                                }
                            }
                            slot2.rock.hSpeed = 0f;
                        }
                    }
                }
                else if (_afterHighlights)
                {
                    _fieldScroll = 220f;
                    _desiredScroll = -79f;
                    _field.scroll = _desiredScroll;
                    _scoreBoard.Activate();
                    _viewBoard = true;
                }
                else if (isServer && Network.isActive)
                {
                    Level.current = new RockScoreboard(returnLevel, ScoreBoardMode.ShowWinner, afterHighlights: true);
                }
                else
                {
                    Level.current = new HighlightLevel();
                }
            }
        }
        if (_finished)
        {
            Graphics.fade = Lerp.Float(Graphics.fade, 0f, 0.03f);
            if (Graphics.fade < 0.01f)
            {
                foreach (Team item in Teams.all)
                {
                    item.prevScoreboardScore = item.score;
                }
                if (isServer)
                {
                    if (_mode == ScoreBoardMode.ShowWinner)
                    {
                        if (_returnToScoreboard)
                        {
                            Level.current = new RockScoreboard(returnLevel, ScoreBoardMode.ShowWinner, afterHighlights: true);
                        }
                        else
                        {
                            Main.ResetMatchStuff();
                            if (_hatSelect)
                            {
                                Level.current = new TeamSelect2(pReturningFromGame: true);
                            }
                            else if (!_quit)
                            {
                                Music.Stop();
                                Level.current = returnLevel;
                                Graphics.fade = 1f;
                            }
                            else
                            {
                                if (Network.isActive)
                                {
                                    Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Game Over!"));
                                }
                                Level.current = new TitleScreen();
                            }
                        }
                    }
                    else if (_state != ScoreBoardState.MatchOver)
                    {
                        Music.Stop();
                        Level.current = returnLevel;
                        Graphics.fade = 1f;
                    }
                    else
                    {
                        Level.current = new RockScoreboard(returnLevel, ScoreBoardMode.ShowWinner);
                    }
                }
            }
        }
        else if (!_skipFade && !_finished)
        {
            Graphics.fade = Lerp.Float(Graphics.fade, 1f, 0.03f);
        }
        Network.isServer = isServer;
        if (_mode == ScoreBoardMode.ShowWinner)
        {
            _controlSlide = Lerp.FloatSmooth(_controlSlide, (controlMessage == 1) ? 1f : 0f, 0.1f, 1.05f);
            if (_controlSlide < 0.01f)
            {
                controlMessage = 1;
            }
            if (_viewBoard)
            {
                controlMessage = 2;
                _controlSlide = 1f;
            }
            if (!_scoreBoard.activated)
            {
                if (isServer && (Input.Pressed("START") || (_netCountdown != null && _netCountdown.timer <= 0f)))
                {
                    if (Network.isActive)
                    {
                        _finished = true;
                        _returnToScoreboard = true;
                    }
                    else
                    {
                        _takePicture = true;
                        HUD.CloseAllCorners();
                    }
                }
                if (_takePicture && _flashSkipFrames == 0)
                {
                    _cameraWait -= 0.01f;
                    if (_cameraWait < 0.6f && _playedBeeps == 0)
                    {
                        _playedBeeps = 1;
                        SFX.Play("cameraBeep", 1f, -0.5f);
                    }
                    else if (_cameraWait < 0.3f && _playedBeeps == 1)
                    {
                        _playedBeeps = 2;
                        SFX.Play("cameraBeep", 1f, -0.5f);
                    }
                    if (_cameraWait < 0f && !_playedFlash)
                    {
                        _playedFlash = true;
                        SFX.Play("cameraFlash", 0.8f, 1f);
                    }
                    if (_cameraWait < 0.1f)
                    {
                        _cameraFadeVel += 0.003f;
                        if (_cameraWait < 0.04f)
                        {
                            _cameraFadeVel += 0.01f;
                        }
                    }
                    Graphics.fadeAdd += _cameraFadeVel;
                    if (Graphics.fadeAdd > 1f)
                    {
                        int wide = Graphics.width;
                        int high = Graphics.height;
                        if (!_sunEnabled)
                        {
                            int yCut = Graphics.height / 4 * 3 + 30;
                            Graphics.fadeAdd = 0f;
                            Layer.Background.fade = 0.8f;
                            _flashSkipFrames++;
                            finalImage = new RenderTarget2D(wide, high);
                            Layer.Game.visible = false;
                            Rectangle sciss = _field.scissor;
                            _field.scissor = new Rectangle(0f, 0f, Resolution.size.x, yCut);
                            _field.visible = true;
                            MonoMain.RenderGame(finalImage);
                            Layer.Game.visible = true;
                            Color c = Level.current.backgroundColor;
                            Level.current.backgroundColor = Color.Transparent;
                            finalImage2 = new RenderTarget2D(wide, high);
                            Layer.allVisible = false;
                            Layer.Game.visible = true;
                            yCut -= 5;
                            _field.scissor = new Rectangle(0f, yCut, wide, high - yCut);
                            _field.visible = true;
                            MonoMain.RenderGame(finalImage2);
                            _field.scissor = sciss;
                            Layer.allVisible = true;
                            Level.current.backgroundColor = c;
                            _getScreenshot = true;
                            _finalSprite = new Sprite(finalImage);
                            Stream stream = DuckFile.Create(DuckFile.albumDirectory + "album" + DateTime.Now.ToString("MM-dd-yy H;mm;ss") + ".png");
                            ((Texture2D)_finalSprite.texture.nativeObject).SaveAsPng(stream, wide, high);
                            stream.Dispose();
                        }
                        else
                        {
                            Graphics.fadeAdd = 0f;
                            Layer.Background.fade = 0.8f;
                            _weather.Update();
                            DoRender();
                            finalImage = new RenderTarget2D(wide, high);
                            RenderFinalImage(finalImage, shrink: false);
                            _finalSprite = new Sprite(finalImage);
                            _getScreenshot = true;
                            Graphics.fadeAdd = 1f;
                            wide = 320;
                            high = 180;
                            RenderTarget2D image = new RenderTarget2D(wide, high);
                            RenderFinalImage(image, shrink: true);
                            Graphics.fadeAdd = 1f;
                            Stream stream2 = DuckFile.Create(DuckFile.albumDirectory + DateTime.Now.ToString("MM-dd-yy H;mm") + ".png");
                            (image.nativeObject as Microsoft.Xna.Framework.Graphics.RenderTarget2D).SaveAsPng(stream2, wide, high);
                            stream2.Dispose();
                            DoRender();
                        }
                    }
                }
                if (_getScreenshot && Graphics.screenCapture == null)
                {
                    Level.current.simulatePhysics = false;
                    _flashSkipFrames++;
                    if (_flashSkipFrames > 2)
                    {
                        Graphics.fadeAdd = 1f;
                    }
                    if (_flashSkipFrames > 20)
                    {
                        Level.current = new HighlightLevel();
                    }
                }
            }
            else if (!_finished && isServer)
            {
                if (Input.Pressed("START") || (_netCountdown != null && _netCountdown.timer <= 0f))
                {
                    _finished = true;
                    _hatSelect = DuckNetwork.isDedicatedServer;
                }
                if (Input.Pressed("CANCEL"))
                {
                    _finished = true;
                    _quit = true;
                }
                if (Input.Pressed("MENU2"))
                {
                    _finished = true;
                    _hatSelect = true;
                }
            }
        }
        Network.isServer = isServer;
        base.Update();
    }

    public override void Terminate()
    {
        if (_mode == ScoreBoardMode.ShowWinner)
        {
            foreach (Team item in Teams.all)
            {
                item.prevScoreboardScore = 0;
            }
        }
        else
        {
            foreach (Team item2 in Teams.all)
            {
                item2.prevScoreboardScore = item2.score;
            }
        }
        Vote.CloseVoting();
    }

    public void RenderFinalImage(RenderTarget2D image, bool shrink)
    {
        if (_sunshineMaterial == null)
        {
            _sunshineMaterial = new MaterialSunshine(_screenTarget);
        }
        Graphics.SetRenderTarget(image);
        Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, base.camera.getMatrix());
        Graphics.material = _sunshineMaterial;
        float scale = (float)Graphics.width / ((float)_pixelTarget.width * ((float)Graphics.width / 320f));
        if (shrink)
        {
            scale = 2f;
        }
        Graphics.Draw(_pixelTarget, Vec2.Zero, null, Color.White, 0f, Vec2.Zero, new Vec2(scale), SpriteEffects.None);
        Graphics.material = null;
        Graphics.screen.End();
        Graphics.SetRenderTarget(null);
    }

    public override void DoDraw()
    {
        if (NetworkDebugger.enabled)
        {
            base.DoDraw();
        }
        else if (!_drawingSunTarget && _sunEnabled)
        {
            if (_sunEnabled)
            {
                DoRender();
            }
            Graphics.Clear(Color.Black);
            if (_sunshineMaterial == null)
            {
                _sunshineMaterial = new MaterialSunshine(_screenTarget);
            }
            if (NetworkDebugger.enabled)
            {
                Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, base.camera.getMatrix());
                float scale = (float)Graphics.width / ((float)_screenTarget.width * ((float)Graphics.width / 320f));
                Graphics.Draw(_screenTarget, Vec2.Zero, null, Color.White, 0f, Vec2.Zero, new Vec2(scale), SpriteEffects.None);
                Graphics.material = null;
                Graphics.screen.End();
            }
            else
            {
                Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, base.camera.getMatrix());
                Graphics.material = _sunshineMaterial;
                float scale2 = (float)Graphics.width / ((float)_pixelTarget.width * ((float)Graphics.width / 320f));
                Graphics.Draw(_pixelTarget, Vec2.Zero, null, Color.White, 0f, Vec2.Zero, new Vec2(scale2), SpriteEffects.None);
                Graphics.material = null;
                Graphics.screen.End();
            }
        }
        else
        {
            base.DoDraw();
        }
    }

    public override void Draw()
    {
        Layer.Game.perspective = _mode == ScoreBoardMode.ShowScores;
        Layer.Game.projection = _field.projection;
        Layer.Game.view = _field.view;
        Layer.Background.perspective = true;
        Layer.Background.projection = _field.projection;
        Layer.Background.view = _field.view;
        Layer.Foreground.perspective = true;
        Layer.Foreground.projection = _field.projection;
        Layer.Foreground.view = _field.view;
        if (_sunEnabled)
        {
            _sunLayer.perspective = true;
            _sunLayer.projection = _field.projection;
            _sunLayer.view = _field.view;
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.HUD)
        {
            if (_getScreenshot && Graphics.screenCapture == null)
            {
                _finalSprite.scale = new Vec2(0.25f, 0.25f);
                Graphics.Draw(_finalSprite, 0f, 0f);
            }
            if (_intermissionSlide > 0.01f)
            {
                _intermissionText.depth = 0.91f;
                float xpos = -320f + _intermissionSlide * 320f;
                float ypos = 60f;
                Graphics.DrawRect(new Vec2(xpos, ypos), new Vec2(xpos + 320f, ypos + 30f), Color.Black, 0.9f);
                xpos = 320f - _intermissionSlide * 320f;
                ypos = 60f;
                Graphics.DrawRect(new Vec2(xpos, ypos + 30f), new Vec2(xpos + 320f, ypos + 60f), Color.Black, 0.9f);
                Graphics.Draw(_intermissionText, -320f + _intermissionSlide * 336f, ypos + 18f);
            }
        }
        else if (layer == Layer.Game)
        {
            if (_mode == ScoreBoardMode.ShowWinner && !_afterHighlights)
            {
                _winnerPost.depth = -0.962f;
                _winnerBanner.depth = -0.858f;
                float yOff = -10f;
                Graphics.Draw(_winnerPost, 63f, 40f + yOff);
                Graphics.Draw(_winnerPost, 248f, 40f + yOff);
                Graphics.Draw(_winnerBanner, 70f, 43f + yOff);
                string text = Results.winner.name;
                BitmapFont font = Results.winner.font;
                font.scale = new Vec2(2f, 2f);
                float hOffset = 0f;
                float vOffset = 0f;
                if (text.Length > 12)
                {
                    font.scale = new Vec2(1f);
                    vOffset = 3f;
                }
                else if (text.Length > 9)
                {
                    font.scale = new Vec2(1.5f);
                    hOffset = 2f;
                    vOffset = 1f;
                }
                font.Draw(text, 160f - font.GetWidth(text) / 2f + hOffset, 50f + yOff + vOffset, Color.Black, _winnerBanner.depth + 1);
                font.scale = new Vec2(1f, 1f);
            }
        }
        else if (layer == Layer.Foreground)
        {
            _ = _drawingSunTarget;
        }
        base.PostDrawLayer(layer);
    }
}
