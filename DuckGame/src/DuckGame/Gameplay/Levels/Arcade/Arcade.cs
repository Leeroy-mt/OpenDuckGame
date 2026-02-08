using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class Arcade : Level
{
    protected FollowCam _followCam;

    private ArcadeState _state;

    private ArcadeState _desiredState;

    private ArcadeHUD _hud;

    private UnlockScreen _unlockScreen;

    private List<ArcadeMachine> _unlockMachines = new List<ArcadeMachine>();

    private SpriteThing _background;

    private bool _unlockingMachine;

    private List<ArcadeMachine> _challenges = new List<ArcadeMachine>();

    private PrizeTable _prizeTable;

    private Duck _duck;

    private UIComponent _pauseGroup;

    private UIMenu _pauseMenu;

    private UIMenu _confirmMenu;

    private object _hoverThing;

    private ArcadeMachine _hoverMachine;

    public static Arcade currentArcade;

    private bool _launchedChallenge;

    private bool _flipState;

    private float _unlockMachineWait = 1f;

    private bool _paused;

    private bool _quitting;

    private MenuBoolean _quit = new MenuBoolean();

    private bool _afterChallenge;

    public FollowCam followCam => _followCam;

    public Arcade()
    {
        _followCam = new FollowCam();
        _followCam.lerpMult = 2f;
        base.camera = _followCam;
        DeathmatchLevel.started = true;
    }

    public override void Initialize()
    {
        _background = new SpriteThing(313f, -40f, new Sprite("arcade/arcadeOuya"));
        _background.Center = new Vector2(0f, 0f);
        _background.layer = Layer.Background;
        _duck = new Duck(730f, 100f, Profiles.active[0]);
        Level.Add(_background);
        Level.Add(_duck);
        _followCam.Add(_duck);
        Chancy.Add("SUP MOTHARFUCKAR :P");
        Level.Add(new Block(0f, 187f, 295f, 53f));
        Level.Add(new Block(289f, 195f, 14f, 45f));
        Level.Add(new Block(290f, 203f, 190f, 37f));
        Level.Add(new Block(467f, 195f, 17f, 45f));
        Level.Add(new Block(475f, 187f, 217f, 53f));
        Level.Add(new Block(639f, 179f, 32f, 16f));
        Level.Add(new Block(647f, 171f, 32f, 16f));
        Level.Add(new Block(655f, 163f, 32f, 16f));
        Level.Add(new Block(663f, 155f, 32f, 16f));
        Level.Add(new Block(671f, 147f, 32f, 16f));
        Level.Add(new Block(679f, 139f, 124f, 16f));
        Level.Add(new Block(787f, 0f, 64f, 300f));
        Level.Add(new Block(-16f, 0f, 21f, 300f));
        Level.Add(new Platform(648f, 131f, 12f, 8f));
        Level.Add(new Platform(640f, 123f, 12f, 8f));
        Level.Add(new Platform(632f, 115f, 12f, 8f));
        Level.Add(new Block(624f, 107f, 12f, 8f));
        Level.Add(new Block(616f, 99f, 12f, 8f));
        Level.Add(new Block(-100f, 91f, 720f, 14f));
        Level.Add(new Block(251f, 83f, 268f, 10f));
        Level.Add(new Block(259f, 75f, 252f, 10f));
        Level.Add(new Block(254f, 0f, 64f, 300f));
        List<Vector2> obj = new List<Vector2>
        {
            new Vector2(380f, 186f),
            new Vector2(520f, 170f),
            new Vector2(565f, 74f),
            new Vector2(375f, 58f),
            new Vector2(455f, 58f)
        };
        Vector2 pos = obj[_challenges.Count];
        ChallengeGroup group = null;
        ArcadeMachine machine = null;
        pos = obj[_challenges.Count];
        machine = new ArcadeMachine(c: new ChallengeGroup
        {
            name = "TARGETS",
            challenges = { "challenge/targets01", "challenge/targets03ouya", "challenge/targets02ouya" },
            trophiesRequired = 0
        }, xpos: pos.X, ypos: pos.Y, index: 0)
        {
            lightColor = 2,
            unlocked = true
        };
        Level.Add(machine);
        _challenges.Add(machine);
        pos = obj[_challenges.Count];
        machine = new ArcadeMachine(c: new ChallengeGroup
        {
            name = "VARIETY ZONE",
            challenges = { "challenge/obstacle", "challenge/shootout02", "challenge/jetpack02" },
            trophiesRequired = 0
        }, xpos: pos.X, ypos: pos.Y, index: 6)
        {
            lightColor = 1
        };
        Level.Add(machine);
        _challenges.Add(machine);
        pos = obj[_challenges.Count];
        machine = new ArcadeMachine(c: new ChallengeGroup
        {
            name = "TELEPORTER",
            challenges = { "challenge/tele02", "challenge/tele01", "challenge/tele03" },
            trophiesRequired = 1
        }, xpos: pos.X, ypos: pos.Y, index: 4)
        {
            lightColor = 1
        };
        Level.Add(machine);
        _challenges.Add(machine);
        pos = obj[_challenges.Count];
        machine = new ArcadeMachine(c: new ChallengeGroup
        {
            name = "WEAPON TRAINING",
            challenges = { "challenge/magnumouya", "challenge/chaingunouya", "challenge/sniper" },
            trophiesRequired = 4
        }, xpos: pos.X, ypos: pos.Y, index: 5)
        {
            lightColor = 2
        };
        Level.Add(machine);
        _challenges.Add(machine);
        pos = obj[_challenges.Count];
        group = new ChallengeGroup();
        group.name = "VARIETY ZONE 2";
        group.challenges.Add("challenge/ball01");
        group.challenges.Add("challenge/glass01ouya");
        group.challenges.Add("challenge/grapple04");
        group.trophiesRequired = 9;
        machine = new ArcadeMachine(pos.X, pos.Y, group, 8);
        machine.lightColor = 1;
        Level.Add(machine);
        _challenges.Add(machine);
        _prizeTable = new PrizeTable(730f, 124f);
        Level.Add(_prizeTable);
        _hud = new ArcadeHUD();
        _hud.Alpha = 0f;
        _unlockScreen = new UnlockScreen();
        _unlockScreen.Alpha = 0f;
        Level.Add(_unlockScreen);
        _pauseGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        _pauseMenu = new UIMenu("@LWING@ARCADE@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@CLOSE  @SELECT@SELECT");
        _confirmMenu = new UIMenu("EXIT ARCADE?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@BACK  @SELECT@SELECT");
        UIDivider pauseBox = new UIDivider(vert: true, 0.8f);
        pauseBox.leftSection.Add(new UIMenuItem("RESUME", new UIMenuActionCloseMenu(_pauseGroup), UIAlign.Left));
        pauseBox.leftSection.Add(new UIMenuItem("OPTIONS", new UIMenuActionOpenMenu(_pauseMenu, Options.optionsMenu), UIAlign.Left));
        pauseBox.leftSection.Add(new UIMenuItem("EXIT ARCADE", new UIMenuActionOpenMenu(_pauseMenu, _confirmMenu), UIAlign.Left));
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
        _pauseGroup.isPauseMenu = true;
        Level.Add(_pauseGroup);
        Music.Play("Arcade");
        base.Initialize();
    }

    public override void Terminate()
    {
    }

    public override void Update()
    {
        base.backgroundColor = Color.Black;
        if (UnlockScreen.open || ArcadeHUD.open)
        {
            _background.visible = false;
            foreach (ArcadeMachine challenge in _challenges)
            {
                challenge.visible = false;
            }
            _prizeTable.visible = false;
        }
        else
        {
            _background.visible = true;
            foreach (ArcadeMachine challenge2 in _challenges)
            {
                challenge2.visible = true;
            }
            _prizeTable.visible = true;
        }
        if (_state == _desiredState && _state != ArcadeState.UnlockMachine && _state != ArcadeState.LaunchChallenge)
        {
            if (!_quitting)
            {
                if (Input.Pressed("START"))
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
            else
            {
                Graphics.fade = Lerp.Float(Graphics.fade, 0f, 0.02f);
                if (Graphics.fade <= 0.01f)
                {
                    Level.current = new TitleScreen();
                }
            }
        }
        if (_paused)
        {
            return;
        }
        _hud.Update();
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
                _duck.Alpha = Lerp.FloatSmooth(_duck.Alpha, 0f, 0.1f);
                _followCam.manualViewSize = Lerp.FloatSmooth(_followCam.manualViewSize, 2f, 0.16f);
                if (_followCam.manualViewSize < 30f)
                {
                    Layer.Game.fade = Lerp.Float(Layer.Game.fade, 0f, 0.08f);
                    Layer.Background.fade = Lerp.Float(Layer.Game.fade, 0f, 0.08f);
                    _hud.Alpha = Lerp.Float(_hud.Alpha, 1f, 0.08f);
                    if (_followCam.manualViewSize < 3f && _hud.Alpha == 1f && Layer.Game.fade == 0f)
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
                _duck.Alpha = Lerp.FloatSmooth(_duck.Alpha, 1f, 0.1f, 1.1f);
                if (_state == ArcadeState.ViewChallenge || _state == ArcadeState.UnlockScreen)
                {
                    _followCam.manualViewSize = Lerp.FloatSmooth(_followCam.manualViewSize, _followCam.viewSize, 0.14f, 1.05f);
                }
                Layer.Game.fade = Lerp.Float(Layer.Game.fade, 1f, 0.05f);
                Layer.Background.fade = Lerp.Float(Layer.Game.fade, 1f, 0.05f);
                _hud.Alpha = Lerp.Float(_hud.Alpha, 0f, 0.08f);
                _unlockScreen.Alpha = Lerp.Float(_unlockScreen.Alpha, 0f, 0.08f);
                if ((_followCam.manualViewSize < 0f || _followCam.manualViewSize == _followCam.viewSize) && _hud.Alpha == 0f && Layer.Game.fade == 1f)
                {
                    done = true;
                    _followCam.manualViewSize = -1f;
                    _duck.Alpha = 1f;
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
                _duck.Alpha = Lerp.FloatSmooth(_duck.Alpha, 1f, 0.1f, 1.1f);
                Layer.Game.fade = Lerp.Float(Layer.Game.fade, 1f, 0.05f);
                Layer.Background.fade = Lerp.Float(Layer.Game.fade, 1f, 0.05f);
                _hud.Alpha = Lerp.Float(_hud.Alpha, 0f, 0.08f);
                _unlockScreen.Alpha = Lerp.Float(_unlockScreen.Alpha, 0f, 0.08f);
                _unlockMachineWait = 1f;
                if ((_followCam.manualViewSize < 0f || _followCam.manualViewSize == _followCam.viewSize) && _hud.Alpha == 0f && Layer.Game.fade == 1f)
                {
                    done = true;
                    _followCam.manualViewSize = -1f;
                    _duck.Alpha = 1f;
                }
            }
            else if (_desiredState == ArcadeState.LaunchChallenge)
            {
                if (!_flipState)
                {
                    HUD.CloseAllCorners();
                }
                Music.volume = Lerp.Float(Music.volume, 0f, 0.01f);
                _hud.Alpha = Lerp.Float(_hud.Alpha, 0f, 0.02f);
                _unlockScreen.Alpha = Lerp.Float(_unlockScreen.Alpha, 0f, 0.08f);
                if (_hud.Alpha == 0f)
                {
                    done = true;
                }
            }
            if (_desiredState == ArcadeState.UnlockScreen)
            {
                _duck.Alpha = Lerp.FloatSmooth(_duck.Alpha, 0f, 0.1f);
                _followCam.manualViewSize = Lerp.FloatSmooth(_followCam.manualViewSize, 2f, 0.16f);
                if (_followCam.manualViewSize < 30f)
                {
                    Layer.Game.fade = Lerp.Float(Layer.Game.fade, 0f, 0.08f);
                    Layer.Background.fade = Lerp.Float(Layer.Game.fade, 0f, 0.08f);
                    _unlockScreen.Alpha = Lerp.Float(_unlockScreen.Alpha, 1f, 0.08f);
                    if (_followCam.manualViewSize < 3f && _unlockScreen.Alpha == 1f && Layer.Game.fade == 0f)
                    {
                        done = true;
                    }
                }
            }
            _flipState = true;
            if (_launchedChallenge)
            {
                Layer.Background.fade = 0f;
                Layer.Game.fade = 0f;
            }
            if (!done)
            {
                return;
            }
            _flipState = false;
            HUD.CloseAllCorners();
            _state = _desiredState;
            if (_state == ArcadeState.ViewChallenge)
            {
                if (_afterChallenge)
                {
                    Music.Play("Arcade");
                    _afterChallenge = false;
                    DuckFile.FlagForBackup();
                }
                _hud.MakeActive();
                _duck.active = false;
            }
            else if (_state == ArcadeState.LaunchChallenge)
            {
                currentArcade = this;
                foreach (ChallengeConfetti item in base.things[typeof(ChallengeConfetti)])
                {
                    Level.Remove(item);
                }
                Music.Stop();
                Level.current = new ChallengeLevel(_hud.selected.challenge.fileName);
                _desiredState = ArcadeState.ViewChallenge;
                _hud.launchChallenge = false;
                _launchedChallenge = false;
                _afterChallenge = true;
            }
            else
            {
                if (_state == ArcadeState.UnlockMachine)
                {
                    return;
                }
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
                    }
                    else
                    {
                        _duck.active = true;
                    }
                }
                else if (_state == ArcadeState.UnlockScreen)
                {
                    _unlockScreen.MakeActive();
                    _duck.active = false;
                }
                else if (_state == ArcadeState.ViewSpecialChallenge)
                {
                    if (_afterChallenge)
                    {
                        Music.Play("Arcade");
                        _afterChallenge = false;
                        DuckFile.FlagForBackup();
                    }
                    Chancy.afterChallenge = true;
                    Chancy.afterChallengeWait = 1f;
                    _duck.active = false;
                }
            }
        }
        else if (_state == ArcadeState.Normal || _state == ArcadeState.UnlockMachine)
        {
            Layer.Game.fade = Lerp.Float(Layer.Game.fade, 1f, 0.08f);
            Layer.Background.fade = Lerp.Float(Layer.Game.fade, 1f, 0.08f);
            _hud.Alpha = Lerp.Float(_hud.Alpha, 0f, 0.08f);
            if (_state == ArcadeState.Normal)
            {
                object hover = null;
                foreach (ArcadeMachine machine2 in _challenges)
                {
                    _ = (_duck.Position - machine2.Position).Length();
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
                }
                if (_hoverThing == hover)
                {
                    return;
                }
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
                    HUD.AddCornerControl(HUDCorner.BottomRight, "@SHOOT@PLAY", _duck.inputProfile);
                    string message = _hoverMachine.data.GetNameForDisplay() + " ";
                    foreach (string challenge3 in _hoverMachine.data.challenges)
                    {
                        ChallengeData dat = Challenges.GetChallenge(challenge3);
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
                    HUD.AddCornerMessage(HUDCorner.TopRight, message);
                }
                else if (_prizeTable.hover)
                {
                    HUD.AddCornerControl(HUDCorner.BottomRight, "@SHOOT@CHANCY", _duck.inputProfile);
                    HUD.AddCornerCounter(HUDCorner.BottomMiddle, "@TICKET@ ", new FieldBinding(Profiles.active[0], "ticketCount"), 0, animateCount: true);
                }
            }
            else
            {
                if (_state != ArcadeState.UnlockMachine)
                {
                    return;
                }
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
            Layer.Game.fade = Lerp.Float(Layer.Game.fade, 0f, 0.05f);
            Layer.Background.fade = Lerp.Float(Layer.Game.fade, 0f, 0.05f);
            _hud.Alpha = Lerp.Float(_hud.Alpha, 1f, 0.05f);
            if (_hud.quitOut)
            {
                _hud.quitOut = false;
                _desiredState = ArcadeState.Normal;
            }
        }
        else if (_state == ArcadeState.UnlockScreen && _unlockScreen.quitOut)
        {
            _unlockScreen.quitOut = false;
            _desiredState = ArcadeState.Normal;
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.HUD)
        {
            _hud.Draw();
        }
        base.PostDrawLayer(layer);
    }
}
