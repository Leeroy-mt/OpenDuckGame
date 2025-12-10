using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class ArcadeLevel : XMLLevel
{
    protected FollowCam _followCam;

    public Editor editor;

    public string customMachine;

    public LevGenType genType;

    private List<Duck> _pendingSpawns;

    private Duck _duck;

    private ArcadeMode _arcade;

    private UIComponent _pauseGroup;

    private UIMenu _pauseMenu;

    private UIMenu _confirmMenu;

    private UIMenu _advancedMenu;

    private ArcadeState _state;

    private ArcadeState _desiredState;

    private ArcadeHUD _hud;

    private UnlockScreen _unlockScreen;

    private List<ArcadeMachine> _unlockMachines = new List<ArcadeMachine>();

    private bool _unlockingMachine;

    public List<ArcadeMachine> _challenges = new List<ArcadeMachine>();

    private PrizeTable _prizeTable;

    private PlugMachine _plugMachine;

    private object _hoverThing;

    private ArcadeMachine _hoverMachine;

    public static ArcadeLevel currentArcade;

    private bool _launchedChallenge;

    private bool _flipState;

    private float _unlockMachineWait = 1f;

    private bool _paused;

    private bool _quitting;

    private MenuBoolean _quit = new MenuBoolean();

    private bool _afterChallenge;

    private List<ArcadeFrame> _frames = new List<ArcadeFrame>();

    public bool basementWasUnlocked;

    private MetroidDoor _exitDoor;

    private UIDivider _pausebox;

    private bool _entering = true;

    private bool _enteringCameraUpdated;

    private bool spawnKey;

    private float spawnKeyWait = 0.2f;

    private bool _prevGotDev;

    public bool returnToChallengeList;

    public bool launchSpecialChallenge;

    private Sprite _speedClock;

    public FollowCam followCam => _followCam;

    public ArcadeLevel(string name)
        : base(name)
    {
        _followCam = new FollowCam();
        _followCam.lerpMult = 2f;
        _followCam.startCentered = false;
        base.camera = _followCam;
    }

    public void UpdateDefault()
    {
        if (Level.current == null)
        {
            return;
        }
        foreach (Door d in Level.current.things[typeof(Door)])
        {
            if (d._lockDoor)
            {
                d.locked = !Unlocks.IsUnlocked("BASEMENTKEY", Profiles.active[0]);
            }
        }
    }

    public void CheckFrames()
    {
        float skillIndex = Challenges.GetChallengeSkillIndex();
        foreach (ArcadeFrame frame in _frames)
        {
            if (skillIndex >= (float)frame.respect && ChallengeData.CheckRequirement(Profiles.active[0], frame.requirement))
            {
                frame.visible = true;
            }
            else
            {
                frame.visible = false;
            }
        }
    }

    public ArcadeFrame GetFrame()
    {
        float skillIndex = Challenges.GetChallengeSkillIndex();
        foreach (ArcadeFrame frame in _frames.OrderBy((ArcadeFrame x) => (x.saveData == null) ? Rando.Int(100) : (Rando.Int(100) + 200)))
        {
            if (skillIndex >= (float)frame.respect && ChallengeData.CheckRequirement(Profiles.active[0], frame.requirement))
            {
                return frame;
            }
        }
        return null;
    }

    public ArcadeFrame GetFrame(string id)
    {
        return _frames.FirstOrDefault((ArcadeFrame x) => x._identifier == id);
    }

    public void InitializeMachines()
    {
        base.Initialize();
        foreach (ArcadeMachine m in base.things[typeof(ArcadeMachine)])
        {
            _challenges.Add(m);
        }
    }

    public override void Initialize()
    {
        TeamSelect2.DefaultSettings();
        base.Initialize();
        Deathmatch d = new Deathmatch(this);
        _pendingSpawns = d.SpawnPlayers(recordStats: false);
        foreach (Duck duck in _pendingSpawns)
        {
            followCam.Add(duck);
            Level.First<ArcadeHatConsole>()?.MakeHatSelector(duck);
        }
        UpdateDefault();
        followCam.Adjust();
        if (genType == LevGenType.CustomArcadeMachine)
        {
            if (base.things[typeof(ArcadeMachine)].FirstOrDefault() is ArcadeMachine import)
            {
                LevelData mach = DuckFile.LoadLevel(customMachine);
                if (mach != null && mach.objects != null && mach.objects.objects != null)
                {
                    try
                    {
                        if (Thing.LoadThing(mach.objects.objects.FirstOrDefault()) is ImportMachine m)
                        {
                            m.position = import.position;
                            Level.Remove(import);
                            Level.Add(m);
                            base.things.RefreshState();
                            _challenges.Add(m);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        else
        {
            _ = bareInitialize;
            foreach (ArcadeMachine m2 in base.things[typeof(ArcadeMachine)])
            {
                _challenges.Add(m2);
            }
        }
        Profiles.active[0].ticketCount = Challenges.GetTicketCount(Profiles.active[0]);
        foreach (ArcadeFrame frame in base.things[typeof(ArcadeFrame)])
        {
            _frames.Add(frame);
        }
        foreach (ChallengeSaveData dat in Challenges.GetAllSaveData())
        {
            if (dat.frameID != "")
            {
                ArcadeFrame f = GetFrame(dat.frameID);
                if (f != null)
                {
                    f.saveData = dat;
                }
            }
        }
        foreach (ArcadeMachine challenge in _challenges)
        {
            challenge.unlocked = challenge.CheckUnlocked(ignoreAlreadyUnlocked: false);
        }
        _hud = new ArcadeHUD();
        _hud.alpha = 0f;
        Level.Add(_hud);
        _unlockScreen = new UnlockScreen();
        _unlockScreen.alpha = 0f;
        Level.Add(_unlockScreen);
        _pauseGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        _pauseMenu = new UIMenu("@LWING@ARCADE@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@CLOSE  @SELECT@SELECT");
        _confirmMenu = new UIMenu("EXIT ARCADE?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@BACK  @SELECT@SELECT");
        _advancedMenu = new UIMenu("@LWING@ADVANCED@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 260f, -1f, "@CANCEL@BACK  @SELECT@SELECT");
        _pausebox = new UIDivider(vert: true, 0.8f);
        _pausebox.leftSection.Add(new UIMenuItem("RESUME", new UIMenuActionCloseMenu(_pauseGroup), UIAlign.Left));
        _pausebox.leftSection.Add(new UIMenuItem("OPTIONS", new UIMenuActionOpenMenu(_pauseMenu, Options.optionsMenu), UIAlign.Left));
        _pausebox.leftSection.Add(new UIText("", Color.White));
        _pausebox.leftSection.Add(new UIMenuItem("|DGRED|EXIT ARCADE", new UIMenuActionOpenMenu(_pauseMenu, _confirmMenu), UIAlign.Left));
        _pausebox.rightSection.Add(new UIImage("pauseIcons", UIAlign.Right));
        _pauseMenu.Add(_pausebox);
        _pauseMenu.Close();
        _pauseGroup.Add(_pauseMenu, doAnchor: false);
        Options.AddMenus(_pauseGroup);
        Options.openOnClose = _pauseMenu;
        _confirmMenu.Add(new UIMenuItem("NO!", new UIMenuActionOpenMenu(_confirmMenu, _pauseMenu), UIAlign.Left, default(Color), backButton: true));
        _confirmMenu.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _quit)));
        _confirmMenu.Close();
        _pauseGroup.Add(_confirmMenu, doAnchor: false);
        _advancedMenu.Add(new UIText("|DGBLUE|SPEEDRUN SETTINGS", Color.White));
        _advancedMenu.Add(new UIText("", Color.White));
        _advancedMenu.Add(new UIText("If enabled, Speedrun Mode", Colors.DGBlue));
        _advancedMenu.Add(new UIText("will fix the random generator", Colors.DGBlue));
        _advancedMenu.Add(new UIText("to make target spawns", Colors.DGBlue));
        _advancedMenu.Add(new UIText("deterministic.", Colors.DGBlue));
        _advancedMenu.Add(new UIMenuItemToggle("SPEEDRUN MODE", null, new FieldBinding(DuckNetwork.core, "speedrunMode")));
        _advancedMenu.Add(new UIMenuItemToggle("MAX TROPHY", null, new FieldBinding(DuckNetwork.core, "speedrunMaxTrophy", 0f, 5f), default(Color), null, new List<string> { "OFF", "@BRONZE@", "@SILVER@", "@GOLD@", "@PLATINUM@", "@DEVELOPER@" }, compressedMulti: true));
        _advancedMenu.Add(new UIText("", Color.White));
        _advancedMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenu(_advancedMenu, _pauseMenu), UIAlign.Center, default(Color), backButton: true));
        _advancedMenu.Close();
        _pauseGroup.Add(_advancedMenu, doAnchor: false);
        _pauseGroup.isPauseMenu = true;
        _pauseGroup.Close();
        Level.Add(_pauseGroup);
        _prizeTable = base.things[typeof(PrizeTable)].FirstOrDefault() as PrizeTable;
        _plugMachine = base.things[typeof(PlugMachine)].FirstOrDefault() as PlugMachine;
        if (_prizeTable == null)
        {
            _prizeTable = new PrizeTable(730f, 124f);
        }
        Chancy.activeChallenge = null;
        Chancy.atCounter = true;
        Chancy.lookingAtChallenge = false;
        basementWasUnlocked = Unlocks.IsUnlocked("BASEMENTKEY", Profiles.active[0]);
        Level.Add(_prizeTable);
        Music.Play("Arcade");
        _exitDoor = new MetroidDoor(-192f, 320f);
        Level.Add(_exitDoor);
        _followCam.hardLimitLeft = -192f;
    }

    public override void Terminate()
    {
    }

    public override void Update()
    {
        MonoMain.timeInArcade++;
        if (!_prevGotDev && Options.Data.gotDevMedal)
        {
            _prevGotDev = true;
            _pausebox.leftSection.Insert(new UIMenuItem("ADVANCED", new UIMenuActionOpenMenu(_pauseMenu, _advancedMenu), UIAlign.Left), 2);
        }
        if (_entering)
        {
            Graphics.fade = Lerp.Float(Graphics.fade, 1f, 0.05f);
            if (Graphics.fade > 0.99f)
            {
                _entering = false;
                Graphics.fade = 1f;
            }
        }
        Options.openOnClose = _pauseMenu;
        if (_duck != null && _duck.profile != null && !_duck.profile.inputProfile.HasAnyConnectedDevice())
        {
            foreach (InputProfile i in InputProfile.defaultProfiles)
            {
                if (i.HasAnyConnectedDevice())
                {
                    InputProfile.SwapDefaultInputStrings(i.name, _duck.profile.inputProfile.name);
                    InputProfile.ReassignDefaultInputProfiles();
                    _duck.profile.inputProfile = i;
                    break;
                }
            }
        }
        if (spawnKey)
        {
            if (spawnKeyWait > 0f)
            {
                spawnKeyWait -= Maths.IncFrameTimer();
            }
            else
            {
                SFX.Play("ching");
                spawnKey = false;
                Key k = new Key(_prizeTable.x, _prizeTable.y);
                k.vSpeed = -4f;
                k.depth = _duck.depth + 50;
                Level.Add(SmallSmoke.New(k.x + Rando.Float(-4f, 4f), k.y + Rando.Float(-4f, 4f)));
                Level.Add(SmallSmoke.New(k.x + Rando.Float(-4f, 4f), k.y + Rando.Float(-4f, 4f)));
                Level.Add(SmallSmoke.New(k.x + Rando.Float(-4f, 4f), k.y + Rando.Float(-4f, 4f)));
                Level.Add(SmallSmoke.New(k.x + Rando.Float(-4f, 4f), k.y + Rando.Float(-4f, 4f)));
                Level.Add(k);
            }
        }
        Chancy.Update();
        if (_pendingSpawns != null && _pendingSpawns.Count > 0)
        {
            Duck spawn = _pendingSpawns[0];
            AddThing(spawn);
            _pendingSpawns.RemoveAt(0);
            _duck = spawn;
            _arcade = base.things[typeof(ArcadeMode)].First() as ArcadeMode;
            if (!_enteringCameraUpdated)
            {
                _enteringCameraUpdated = true;
                for (int j = 0; j < 200; j++)
                {
                    _followCam.Update();
                }
            }
        }
        Math.Min(1f, Math.Max(0f, (1f - Layer.Game.fade) * 1.5f));
        base.backgroundColor = Color.Black;
        if (UnlockScreen.open || ArcadeHUD.open)
        {
            foreach (ArcadeMachine challenge in _challenges)
            {
                challenge.visible = false;
            }
            _prizeTable.visible = false;
        }
        else
        {
            foreach (ArcadeMachine challenge2 in _challenges)
            {
                challenge2.visible = true;
            }
            _prizeTable.visible = true;
        }
        if (_duck != null)
        {
            _exitDoor._arcadeProfile = _duck.profile;
        }
        if (_state == _desiredState && _state != ArcadeState.UnlockMachine && _state != ArcadeState.LaunchChallenge)
        {
            if (_quitting)
            {
                Graphics.fade = Lerp.Float(Graphics.fade, 0f, 0.05f);
                if (Graphics.fade <= 0.01f)
                {
                    Chancy.StopShowingChallengeList();
                    if (editor != null)
                    {
                        Level.current = editor;
                    }
                    else
                    {
                        Level.current = new TitleScreen();
                    }
                }
                return;
            }
            ArcadeHatConsole hatConsole = Level.First<ArcadeHatConsole>();
            if (Input.Pressed("START") && (hatConsole == null || !hatConsole.IsOpen()))
            {
                _pauseGroup.Open();
                _pauseMenu.Open();
                MonoMain.pauseMenu = _pauseGroup;
                if (!_paused)
                {
                    Music.Pause();
                    SFX.Play("pause", 0.6f);
                    _paused = true;
                    _duck.immobilized = true;
                }
                base.simulatePhysics = false;
                return;
            }
            if (_paused && MonoMain.pauseMenu == null)
            {
                _paused = false;
                SFX.Play("resume", 0.6f);
                if (_quit.value)
                {
                    _quitting = true;
                }
                else
                {
                    Music.Resume();
                    _duck.immobilized = false;
                    base.simulatePhysics = true;
                }
            }
        }
        if (_paused)
        {
            return;
        }
        if (_hud.launchChallenge)
        {
            _desiredState = ArcadeState.LaunchChallenge;
        }
        if (_desiredState != _state)
        {
            _duck.active = false;
            bool done = false;
            if (_desiredState == ArcadeState.ViewChallenge)
            {
                _duck.alpha = Lerp.FloatSmooth(_duck.alpha, 0f, 0.1f);
                _followCam.manualViewSize = Lerp.FloatSmooth(_followCam.manualViewSize, 2f, 0.16f);
                if (_followCam.manualViewSize < 30f)
                {
                    Layer.Game.fade = Lerp.Float(Layer.Game.fade, 0f, 0.08f);
                    Layer.Background.fade = Lerp.Float(Layer.Game.fade, 0f, 0.08f);
                    _hud.alpha = Lerp.Float(_hud.alpha, 1f, 0.08f);
                    if (_followCam.manualViewSize < 3f && _hud.alpha == 1f && Layer.Game.fade == 0f)
                    {
                        done = true;
                    }
                }
            }
            else if (_desiredState == ArcadeState.Normal)
            {
                if (!_flipState)
                {
                    _followCam.Clear();
                    _followCam.Add(_duck);
                    HUD.CloseAllCorners();
                }
                _duck.alpha = Lerp.FloatSmooth(_duck.alpha, 1f, 0.1f, 1.1f);
                if (_state == ArcadeState.ViewChallenge || _state == ArcadeState.UnlockScreen)
                {
                    _followCam.manualViewSize = Lerp.FloatSmooth(_followCam.manualViewSize, _followCam.viewSize, 0.14f, 1.05f);
                }
                Layer.Game.fade = Lerp.Float(Layer.Game.fade, 1f, 0.05f);
                Layer.Background.fade = Lerp.Float(Layer.Game.fade, 1f, 0.05f);
                _hud.alpha = Lerp.Float(_hud.alpha, 0f, 0.08f);
                _unlockScreen.alpha = Lerp.Float(_unlockScreen.alpha, 0f, 0.08f);
                if ((_followCam.manualViewSize < 0f || _followCam.manualViewSize == _followCam.viewSize) && _hud.alpha == 0f && Layer.Game.fade == 1f)
                {
                    done = true;
                    _followCam.manualViewSize = -1f;
                    _duck.alpha = 1f;
                }
                if (Unlockables.HasPendingUnlocks())
                {
                    MonoMain.pauseMenu = new UIUnlockBox(Unlockables.GetPendingUnlocks().ToList(), Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f);
                }
            }
            else if (_desiredState == ArcadeState.ViewSpecialChallenge || _desiredState == ArcadeState.ViewChallengeList || _desiredState == ArcadeState.ViewProfileSelector)
            {
                if (!_flipState)
                {
                    _followCam.Clear();
                    _followCam.Add(_duck);
                    HUD.CloseAllCorners();
                }
                _duck.alpha = Lerp.FloatSmooth(_duck.alpha, 1f, 0.1f, 1.1f);
                if (_state == ArcadeState.ViewChallenge || _state == ArcadeState.UnlockScreen)
                {
                    _followCam.manualViewSize = Lerp.FloatSmooth(_followCam.manualViewSize, _followCam.viewSize, 0.14f, 1.05f);
                }
                Layer.Game.fade = Lerp.Float(Layer.Game.fade, 1f, 0.05f);
                Layer.Background.fade = Lerp.Float(Layer.Game.fade, 1f, 0.05f);
                _hud.alpha = Lerp.Float(_hud.alpha, 0f, 0.08f);
                _unlockScreen.alpha = Lerp.Float(_unlockScreen.alpha, 0f, 0.08f);
                if ((_followCam.manualViewSize < 0f || _followCam.manualViewSize == _followCam.viewSize) && _hud.alpha == 0f && Layer.Game.fade == 1f)
                {
                    done = true;
                    _followCam.manualViewSize = -1f;
                    _duck.alpha = 1f;
                }
            }
            else if (_desiredState == ArcadeState.UnlockMachine)
            {
                if (!_flipState)
                {
                    _followCam.Clear();
                    _followCam.Add(_unlockMachines[0]);
                    HUD.CloseAllCorners();
                }
                if (_state == ArcadeState.ViewChallenge)
                {
                    _followCam.manualViewSize = Lerp.FloatSmooth(_followCam.manualViewSize, _followCam.viewSize, 0.14f, 1.05f);
                }
                _duck.alpha = Lerp.FloatSmooth(_duck.alpha, 1f, 0.1f, 1.1f);
                Layer.Game.fade = Lerp.Float(Layer.Game.fade, 1f, 0.05f);
                Layer.Background.fade = Lerp.Float(Layer.Game.fade, 1f, 0.05f);
                _hud.alpha = Lerp.Float(_hud.alpha, 0f, 0.08f);
                _unlockScreen.alpha = Lerp.Float(_unlockScreen.alpha, 0f, 0.08f);
                _unlockMachineWait = 1f;
                if ((_followCam.manualViewSize < 0f || _followCam.manualViewSize == _followCam.viewSize) && _hud.alpha == 0f && Layer.Game.fade == 1f)
                {
                    done = true;
                    _followCam.manualViewSize = -1f;
                    _duck.alpha = 1f;
                }
            }
            else if (_desiredState == ArcadeState.LaunchChallenge)
            {
                if (!_flipState)
                {
                    HUD.CloseAllCorners();
                }
                Music.volume = Lerp.Float(Music.volume, 0f, 0.01f);
                _hud.alpha = Lerp.Float(_hud.alpha, 0f, 0.02f);
                _unlockScreen.alpha = Lerp.Float(_unlockScreen.alpha, 0f, 0.08f);
                if (_hud.alpha == 0f)
                {
                    done = true;
                }
            }
            if (_desiredState == ArcadeState.UnlockScreen)
            {
                _duck.alpha = Lerp.FloatSmooth(_duck.alpha, 0f, 0.1f);
                _followCam.manualViewSize = Lerp.FloatSmooth(_followCam.manualViewSize, 2f, 0.16f);
                if (_followCam.manualViewSize < 30f)
                {
                    Layer.Game.fade = Lerp.Float(Layer.Game.fade, 0f, 0.08f);
                    Layer.Background.fade = Lerp.Float(Layer.Game.fade, 0f, 0.08f);
                    _unlockScreen.alpha = Lerp.Float(_unlockScreen.alpha, 1f, 0.08f);
                    if (_followCam.manualViewSize < 3f && _unlockScreen.alpha == 1f && Layer.Game.fade == 0f)
                    {
                        done = true;
                    }
                }
            }
            if (_desiredState == ArcadeState.Plug)
            {
                done = true;
            }
            _flipState = true;
            if (_launchedChallenge)
            {
                Layer.Background.fade = 0f;
                Layer.Game.fade = 0f;
            }
            if (done)
            {
                _flipState = false;
                HUD.CloseAllCorners();
                _state = _desiredState;
                if (_state == ArcadeState.ViewChallenge)
                {
                    if (_afterChallenge)
                    {
                        Music.Play("Arcade");
                        _afterChallenge = false;
                    }
                    _hud.MakeActive();
                    Level.Add(_hud);
                    _duck.active = false;
                }
                else
                {
                    if (_state == ArcadeState.LaunchChallenge)
                    {
                        currentArcade = this;
                        foreach (ChallengeConfetti item in base.things[typeof(ChallengeConfetti)])
                        {
                            Level.Remove(item);
                        }
                        Music.Stop();
                        Level.current = new ChallengeLevel(_hud.selected.challenge.fileName);
                        if (!launchSpecialChallenge)
                        {
                            _desiredState = ArcadeState.ViewChallenge;
                            _hud.launchChallenge = false;
                            _launchedChallenge = false;
                            _afterChallenge = true;
                        }
                        else
                        {
                            _desiredState = ArcadeState.ViewSpecialChallenge;
                            _hud.launchChallenge = false;
                            _launchedChallenge = false;
                            _afterChallenge = true;
                            launchSpecialChallenge = false;
                        }
                        return;
                    }
                    if (_state != ArcadeState.UnlockMachine)
                    {
                        if (_state == ArcadeState.Normal)
                        {
                            _unlockMachines.Clear();
                            foreach (ArcadeMachine machine in _challenges)
                            {
                                if (machine.CheckUnlocked())
                                {
                                    _unlockMachines.Add(machine);
                                }
                            }
                            if (_unlockMachines.Count > 0)
                            {
                                _desiredState = ArcadeState.UnlockMachine;
                                return;
                            }
                            if (!basementWasUnlocked && Unlocks.IsUnlocked("BASEMENTKEY", Profiles.active[0]))
                            {
                                spawnKey = true;
                                basementWasUnlocked = true;
                            }
                            _duck.active = true;
                        }
                        else if (_state == ArcadeState.ViewSpecialChallenge)
                        {
                            _duck.active = false;
                            if (_afterChallenge)
                            {
                                Music.Play("Arcade");
                                _afterChallenge = false;
                                HUD.AddCornerCounter(HUDCorner.BottomMiddle, "@TICKET@ ", new FieldBinding(Profiles.active[0], "ticketCount"), 0, animateCount: true);
                                Chancy.afterChallenge = true;
                                Chancy.afterChallengeWait = 1f;
                            }
                            else
                            {
                                HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@ACCEPT");
                                HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@CANCEL");
                                HUD.AddCornerCounter(HUDCorner.BottomMiddle, "@TICKET@ ", new FieldBinding(Profiles.active[0], "ticketCount"), 0, animateCount: true);
                            }
                            _duck.active = false;
                        }
                        else if (_state == ArcadeState.ViewProfileSelector)
                        {
                            _duck.active = false;
                            ArcadeHatConsole hatConsole2 = Level.First<ArcadeHatConsole>();
                            if (hatConsole2 != null)
                            {
                                HUD.CloseAllCorners();
                                hatConsole2.Open();
                            }
                        }
                        else if (_state == ArcadeState.ViewChallengeList)
                        {
                            _duck.active = false;
                            HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@ACCEPT");
                            HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@BACK");
                        }
                        else if (_state == ArcadeState.UnlockScreen)
                        {
                            basementWasUnlocked = Unlocks.IsUnlocked("BASEMENTKEY", Profiles.active[0]);
                            _unlockScreen.MakeActive();
                            _duck.active = false;
                        }
                        else if (_state == ArcadeState.Plug)
                        {
                            Plug.Add("Yo! I'm |DGBLUE|[QC] WUMP|WHITE|, Chancy's Sister.^Machine behind me plays |DGYELLOW|user challenges|WHITE|, as well^as obscenely hard challenges I made myself!");
                            Plug.Add("And the machine behind this one is today's^|DGYELLOW|Imported Machine.");
                            Plug.Open();
                            _duck.active = false;
                        }
                    }
                }
            }
        }
        else if (_state == ArcadeState.Normal || _state == ArcadeState.UnlockMachine)
        {
            Layer.Game.fade = Lerp.Float(Layer.Game.fade, 1f, 0.08f);
            Layer.Background.fade = Lerp.Float(Layer.Game.fade, 1f, 0.08f);
            _hud.alpha = Lerp.Float(_hud.alpha, 0f, 0.08f);
            if (_state == ArcadeState.Normal)
            {
                object hover = null;
                foreach (ArcadeMachine machine2 in _challenges)
                {
                    _ = (_duck.position - machine2.position).length;
                    _ = 20f;
                    if (machine2.hover)
                    {
                        hover = machine2;
                        if (Input.Pressed("SHOOT"))
                        {
                            _hud.activeChallengeGroup = machine2.data;
                            _desiredState = ArcadeState.ViewChallenge;
                            _followCam.manualViewSize = _followCam.viewSize;
                            _followCam.Clear();
                            _followCam.Add(machine2);
                            HUD.CloseAllCorners();
                            _hoverMachine = null;
                            _hoverThing = null;
                            return;
                        }
                    }
                    if (_prizeTable.hover)
                    {
                        hover = _prizeTable;
                        if (Input.Pressed("SHOOT"))
                        {
                            _desiredState = ArcadeState.UnlockScreen;
                            _followCam.manualViewSize = _followCam.viewSize;
                            _followCam.Clear();
                            _followCam.Add(_prizeTable);
                            HUD.CloseAllCorners();
                            _hoverMachine = null;
                            _hoverThing = null;
                            return;
                        }
                    }
                    else if (_plugMachine != null && _plugMachine.hover)
                    {
                        hover = _plugMachine;
                        if (Input.Pressed("SHOOT"))
                        {
                            _desiredState = ArcadeState.Plug;
                            _followCam.manualViewSize = _followCam.viewSize;
                            _followCam.Clear();
                            _followCam.Add(_plugMachine);
                            HUD.CloseAllCorners();
                            _hoverMachine = null;
                            _hoverThing = null;
                            return;
                        }
                    }
                }
                if (Chancy.hover && Input.Pressed("SHOOT"))
                {
                    _desiredState = ArcadeState.ViewSpecialChallenge;
                    HUD.CloseAllCorners();
                    _hoverMachine = null;
                    _hoverThing = null;
                    Chancy.hover = false;
                    Chancy.lookingAtChallenge = true;
                    Chancy.OpenChallengeView();
                    return;
                }
                ArcadeHatConsole hatConsole3 = Level.First<ArcadeHatConsole>();
                if (hatConsole3 != null && Input.Pressed("SHOOT") && hatConsole3.hover)
                {
                    _desiredState = ArcadeState.ViewProfileSelector;
                    HUD.CloseAllCorners();
                    _hoverMachine = null;
                    _hoverThing = null;
                    return;
                }
                Chancy.hover = false;
                if (!Chancy.atCounter)
                {
                    if ((_duck.position - Chancy.standingPosition).length < 22f)
                    {
                        hover = Chancy.context;
                        Chancy.hover = true;
                    }
                    if (Chancy.standingPosition.x < Layer.Game.camera.left - 16f || Chancy.standingPosition.x > Layer.Game.camera.right + 16f || Chancy.standingPosition.y < Layer.Game.camera.top - 16f || Chancy.standingPosition.y > Layer.Game.camera.bottom + 16f)
                    {
                        Chancy.atCounter = true;
                        Chancy.activeChallenge = null;
                    }
                }
                else if (_prizeTable.hoverChancyChallenge)
                {
                    hover = _arcade;
                    if (Input.Pressed("SHOOT"))
                    {
                        _desiredState = ArcadeState.ViewChallengeList;
                        HUD.CloseAllCorners();
                        Chancy.OpenChallengeList();
                        _hoverMachine = null;
                        _hoverThing = null;
                        Chancy.hover = false;
                        Chancy.lookingAtList = true;
                        return;
                    }
                }
                if (_hoverThing != hover)
                {
                    HUD.CloseAllCorners();
                    _hoverThing = hover;
                    if (_hoverThing is ArcadeMachine)
                    {
                        _hoverMachine = hover as ArcadeMachine;
                    }
                    else
                    {
                        _hoverMachine = null;
                    }
                    if (_hoverMachine != null)
                    {
                        HUD.AddCornerControl(HUDCorner.BottomRight, "@SHOOT@PLAY");
                        string message = _hoverMachine.data.GetNameForDisplay() + " ";
                        foreach (string challenge3 in _hoverMachine.data.challenges)
                        {
                            ChallengeData dat = Challenges.GetChallenge(challenge3);
                            if (dat != null)
                            {
                                ChallengeSaveData saveData = _duck.profile.GetSaveData(dat.levelID);
                                if (saveData.trophy == TrophyType.Baseline)
                                {
                                    message += "@BASELINE@";
                                }
                                else if (saveData.trophy == TrophyType.Bronze)
                                {
                                    message += "@BRONZE@";
                                }
                                else if (saveData.trophy == TrophyType.Silver)
                                {
                                    message += "@SILVER@";
                                }
                                else if (saveData.trophy == TrophyType.Gold)
                                {
                                    message += "@GOLD@";
                                }
                                else if (saveData.trophy == TrophyType.Platinum)
                                {
                                    message += "@PLATINUM@";
                                }
                                else if (saveData.trophy == TrophyType.Developer)
                                {
                                    message += "@DEVELOPER@";
                                }
                            }
                        }
                        HUD.AddCornerMessage(HUDCorner.TopRight, message);
                    }
                    else if (_prizeTable.hover)
                    {
                        if (_prizeTable.hoverChancyChallenge)
                        {
                            HUD.AddCornerControl(HUDCorner.BottomRight, "@SHOOT@VIEW CHALLENGES");
                        }
                        else
                        {
                            HUD.AddCornerControl(HUDCorner.BottomRight, "@SHOOT@CHANCY");
                            HUD.AddCornerCounter(HUDCorner.BottomMiddle, "@TICKET@ ", new FieldBinding(Profiles.active[0], "ticketCount"), 0, animateCount: true);
                        }
                    }
                    else if (hover is ArcadeMode)
                    {
                        if (_prizeTable.hoverChancyChallenge)
                        {
                            HUD.AddCornerControl(HUDCorner.BottomRight, "@SHOOT@VIEW CHALLENGES");
                        }
                    }
                    else if (hover is Chancy)
                    {
                        HUD.AddCornerControl(HUDCorner.BottomRight, "@SHOOT@CHANCY");
                    }
                    else if (hover is PlugMachine)
                    {
                        HUD.AddCornerControl(HUDCorner.BottomRight, "@SHOOT@[QC]WUMP");
                    }
                }
            }
            else if (_state == ArcadeState.UnlockMachine)
            {
                _unlockMachineWait -= 0.02f;
                if (_unlockMachineWait < 0f)
                {
                    if (_unlockingMachine)
                    {
                        _unlockingMachine = false;
                        _followCam.Clear();
                        _followCam.Add(_unlockMachines[0]);
                        _unlockMachineWait = 1f;
                    }
                    else if (_unlockMachines.Count > 0)
                    {
                        _unlockMachines[0].unlocked = true;
                        _unlockMachines.RemoveAt(0);
                        _unlockingMachine = _unlockMachines.Count > 0;
                        SFX.Play("lightTurnOn", 1f, Rando.Float(-0.1f, 0.1f));
                        _unlockMachineWait = 1f;
                    }
                    else
                    {
                        _desiredState = ArcadeState.Normal;
                    }
                }
            }
        }
        else if (_state == ArcadeState.ViewChallenge)
        {
            Graphics.fade = Lerp.Float(Graphics.fade, 1f, 0.05f);
            Layer.Game.fade = Lerp.Float(Layer.Game.fade, 0f, 0.05f);
            Layer.Background.fade = Lerp.Float(Layer.Game.fade, 0f, 0.05f);
            _hud.alpha = Lerp.Float(_hud.alpha, 1f, 0.05f);
            if (_hud.quitOut)
            {
                _hud.quitOut = false;
                _desiredState = ArcadeState.Normal;
                if (Chancy.activeChallenge == null)
                {
                    List<ChallengeData> c = Challenges.GetEligibleIncompleteChancyChallenges(Profiles.active[0]);
                    if (c.Count > 0)
                    {
                        Vec2 pos = _duck.position;
                        ArcadeMachine near = Level.Nearest<ArcadeMachine>(_duck.x, _duck.y);
                        if (near != null)
                        {
                            pos = near.position;
                        }
                        c.OrderBy((ChallengeData v) => v.GetRequirementValue());
                        Chancy.AddProposition(c[c.Count - 1], pos);
                    }
                }
            }
        }
        else if (_state == ArcadeState.UnlockScreen)
        {
            if (_unlockScreen.quitOut)
            {
                _unlockScreen.quitOut = false;
                _desiredState = ArcadeState.Normal;
            }
        }
        else if (_state == ArcadeState.ViewSpecialChallenge)
        {
            if (!launchSpecialChallenge)
            {
                Graphics.fade = Lerp.Float(Graphics.fade, 1f, 0.05f);
                if (Input.Pressed("CANCEL"))
                {
                    if (returnToChallengeList)
                    {
                        _desiredState = ArcadeState.ViewChallengeList;
                        Chancy.hover = false;
                        Chancy.lookingAtList = true;
                    }
                    else
                    {
                        _desiredState = ArcadeState.Normal;
                    }
                    Chancy.lookingAtChallenge = false;
                    HUD.CloseAllCorners();
                    SFX.Play("consoleCancel");
                    return;
                }
                if (Input.Pressed("SELECT"))
                {
                    launchSpecialChallenge = true;
                    SFX.Play("consoleSelect");
                    return;
                }
            }
            else
            {
                Graphics.fade = Lerp.Float(Graphics.fade, 0f, 0.05f);
                if (Graphics.fade < 0.01f)
                {
                    _hud.launchChallenge = true;
                    _hud.selected = new ChallengeCard(0f, 0f, Chancy.activeChallenge);
                    HUD.CloseAllCorners();
                }
            }
        }
        else if (_state == ArcadeState.ViewChallengeList)
        {
            Graphics.fade = Lerp.Float(Graphics.fade, 1f, 0.05f);
            if (Input.Pressed("CANCEL"))
            {
                _desiredState = ArcadeState.Normal;
                Chancy.lookingAtChallenge = false;
                Chancy.lookingAtList = false;
                HUD.CloseAllCorners();
                SFX.Play("consoleCancel");
                return;
            }
            if (Input.Pressed("SELECT"))
            {
                Chancy.AddProposition(Chancy.selectedChallenge, Chancy.standingPosition);
                returnToChallengeList = true;
                _desiredState = ArcadeState.ViewSpecialChallenge;
                HUD.CloseAllCorners();
                _hoverMachine = null;
                _hoverThing = null;
                Chancy.hover = false;
                Chancy.lookingAtChallenge = true;
                Chancy.lookingAtList = false;
                Chancy.OpenChallengeView();
            }
        }
        else if (_state == ArcadeState.ViewProfileSelector)
        {
            Graphics.fade = Lerp.Float(Graphics.fade, 1f, 0.05f);
            ArcadeHatConsole hatConsole4 = Level.First<ArcadeHatConsole>();
            if (hatConsole4 != null && !hatConsole4.IsOpen())
            {
                foreach (ArcadeMachine machine3 in _challenges)
                {
                    if (machine3.CheckUnlocked(ignoreAlreadyUnlocked: false))
                    {
                        machine3.unlocked = true;
                    }
                    else
                    {
                        machine3.unlocked = false;
                    }
                }
                _unlockMachines.Clear();
                UpdateDefault();
                _desiredState = ArcadeState.Normal;
            }
        }
        if (Plug.open)
        {
            Plug.Update();
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.HUD)
        {
            Chancy.Draw();
            if (Plug.open)
            {
                Plug.Draw();
            }
            if (_speedClock == null)
            {
                _speedClock = new Sprite("speedrunClock");
            }
            if (DuckNetwork.core.speedrunMode)
            {
                Graphics.Draw(_speedClock, 4f, 4f);
            }
        }
        if (layer == Layer.Game)
        {
            Chancy.DrawGameLayer();
        }
        base.PostDrawLayer(layer);
    }
}
