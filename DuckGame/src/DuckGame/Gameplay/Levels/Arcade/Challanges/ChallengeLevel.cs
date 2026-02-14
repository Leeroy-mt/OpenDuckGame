using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class ChallengeLevel : XMLLevel, IHaveAVirtualTransition
{
    protected FollowCam _followCam;

    private BitmapFont _font;

    private LevelData _levelData;

    public bool _validityTest;

    private List<Duck> _pendingSpawns;

    public static int targetsShot;

    public static int goodiesGot;

    public static bool running;

    public static bool allTargetsShot;

    private static ChallengeTimer _timer;

    public static bool random;

    private MenuBoolean _capture = new MenuBoolean();

    private MenuBoolean _quit = new MenuBoolean();

    private MenuBoolean _restart = new MenuBoolean();

    private UIComponent _pauseGroup;

    private UIComponent _trophyGroup;

    private UIMenu _pauseMenu;

    private UIMenu _confirmMenu;

    private UIMenu _trophyMenu;

    private UIMenu _captureMenu;

    private bool _firstStart = true;

    private float _finishWait = 0.75f;

    private bool _finished;

    private bool _playedEndMusic;

    private float _restartMessageWait = 1f;

    private bool _win;

    private bool _developer;

    public ChallengeMode _challenge;

    private bool _doRestart;

    private float _waitForRestart = 1f;

    private float _waitFade;

    protected float _waitSpawn = 2f;

    private float _showResultsWait = 1f;

    private float _waitAfterSpawn = 1f;

    private int _waitAfterSpawnDings;

    private bool _didFade;

    private bool _started;

    private float _fontFade = 1f;

    private bool _paused;

    private bool _restarting;

    private static Duck _duck;

    private bool _showedEndMenu;

    private float _showEndTextWait = 1f;

    private bool _fading;

    private RenderTarget2D _captureTarget;

    public FollowCam followCam => _followCam;

    public static ChallengeTimer timer => _timer;

    public static Duck duck => _duck;

    public ChallengeLevel(string name)
        : base(name)
    {
        _followCam = new FollowCam();
        _followCam.lerpMult = 1f;
        _followCam.startCentered = false;
        base.camera = _followCam;
        base.simulatePhysics = false;
    }

    public ChallengeLevel(LevelData data, bool validityTest)
        : base(data)
    {
        _followCam = new FollowCam();
        _followCam.lerpMult = 1f;
        _followCam.startCentered = false;
        base.camera = _followCam;
        base.simulatePhysics = false;
        _levelData = data;
        _validityTest = validityTest;
    }

    public override void Initialize()
    {
        MonoMain.FinishLazyLoad();
        targetsShot = 0;
        goodiesGot = 0;
        allTargetsShot = true;
        running = false;
        _timer = new ChallengeTimer();
        if (DuckNetwork.core.speedrunMode)
        {
            ChallengeRando.generator = new Random(1337);
        }
        base.Initialize();
        _font = new BitmapFont("biosFont", 8);
        foreach (Team item in Teams.all)
        {
            int prevScoreboardScore = (item.score = 0);
            item.prevScoreboardScore = prevScoreboardScore;
        }
        bool first = true;
        foreach (Profile pro in Profiles.active)
        {
            if (first)
            {
                first = false;
                continue;
            }
            if (pro.team != null)
            {
                pro.team.Leave(pro);
            }
            pro.inputProfile = null;
        }
        Deathmatch d = new Deathmatch(this);
        _pendingSpawns = d.SpawnPlayers(recordStats: false);
        _pendingSpawns = _pendingSpawns.OrderBy((Duck sp) => sp.X).ToList();
        foreach (Duck duck in _pendingSpawns)
        {
            followCam.Add(duck);
        }
        _pauseGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        _pauseMenu = new UIMenu("@LWING@CHALLENGE@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@CLOSE @SELECT@SELECT");
        _confirmMenu = new UIMenu("REALLY QUIT?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        _captureMenu = new UICaptureBox(_pauseMenu, Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f);
        _captureMenu.Close();
        _pauseGroup.Add(_captureMenu, doAnchor: false);
        UIDivider pauseBox = new UIDivider(vert: true, 0.8f);
        pauseBox.leftSection.Add(new UIMenuItem("RESTART!", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _restart), UIAlign.Left));
        pauseBox.leftSection.Add(new UIMenuItem("RESUME", new UIMenuActionCloseMenu(_pauseGroup), UIAlign.Left));
        pauseBox.leftSection.Add(new UIMenuItem("OPTIONS", new UIMenuActionOpenMenu(_pauseMenu, Options.optionsMenu), UIAlign.Left));
        pauseBox.leftSection.Add(new UIText("", Color.White));
        pauseBox.leftSection.Add(new UIMenuItem("|DGRED|QUIT", new UIMenuActionOpenMenu(_pauseMenu, _confirmMenu), UIAlign.Left));
        if (base.things[typeof(EditorTestLevel)].Count() > 0)
        {
            pauseBox.leftSection.Add(new UIText("", Color.White));
            pauseBox.leftSection.Add(new UIMenuItem("CAPTURE ICON", new UIMenuActionOpenMenu(_pauseMenu, _captureMenu), UIAlign.Left));
        }
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
        _pauseGroup.isPauseMenu = true;
        _pauseGroup.Close();
        Level.Add(_pauseGroup);
        Music.volume = 1f;
        followCam.Adjust();
    }

    public void ChallengeEnded(ChallengeMode challenge)
    {
        Music.Stop();
        _developer = false;
        if (challenge.wonTrophies.Count > 0)
        {
            SFX.Play("scoreDing");
            _win = true;
            if (challenge.wonTrophies[0].type == TrophyType.Developer)
            {
                Options.Data.gotDevMedal = true;
                _developer = true;
            }
        }
        else
        {
            SFX.Play("recordStop");
            _win = false;
        }
        _finished = true;
        _challenge = challenge;
    }

    public void RestartChallenge()
    {
        _doRestart = true;
    }

    private string FormatResultString(string captionColor, string caption, string resultColor, string result, string trophyColor, string trophy, int wide = 26)
    {
        if (trophy == null)
        {
            trophy = "";
        }
        if (trophyColor == null)
        {
            trophyColor = "";
        }
        int numReq = wide - (caption + result + trophy).Length;
        bool flop = false;
        while (numReq > 0)
        {
            numReq--;
            if (flop || trophy == "")
            {
                caption += " ";
            }
            else
            {
                result += " ";
            }
            flop = !flop;
        }
        return captionColor + caption + resultColor + result + trophyColor + trophy;
    }

    public override void Update()
    {
        MonoMain.timeInArcade++;
        _timer.Update();
        if (_fading)
        {
            Graphics.fade = Lerp.Float(Graphics.fade, 0f, 0.05f);
            if (!(Graphics.fade < 0.01f))
            {
                return;
            }
            if (_validityTest)
            {
                if (_challenge.wonTrophies.Count > 0 && _challenge.wonTrophies.Count > 0 && (_challenge.wonTrophies[0].type == TrophyType.Platinum || _challenge.wonTrophies[0].type == TrophyType.Developer))
                {
                    ArcadeTestDialogue.success = true;
                }
                else
                {
                    ArcadeTestDialogue.success = false;
                }
                Level.current = ArcadeTestDialogue.currentEditor;
                Graphics.fade = 1f;
                return;
            }
            if (base.things[typeof(EditorTestLevel)].Count() > 0)
            {
                Level.current = (base.things[typeof(EditorTestLevel)].First() as EditorTestLevel).editor;
                Music.Stop();
            }
            else if (Arcade.currentArcade != null)
            {
                Level.current = Arcade.currentArcade;
            }
            else
            {
                Level.current = ArcadeLevel.currentArcade;
            }
            _fading = false;
            return;
        }
        if (_restartMessageWait > 0f)
        {
            _restartMessageWait -= 0.008f;
        }
        else
        {
            HUD.CloseCorner(HUDCorner.TopLeft);
        }
        if (_doRestart)
        {
            running = false;
            _waitForRestart -= 0.04f;
            if (_waitForRestart <= 0f)
            {
                _restarting = true;
            }
        }
        _waitFade -= 0.04f;
        if (!_didFade && _waitFade <= 0f && Graphics.fade < 1f)
        {
            Graphics.fade = Lerp.Float(Graphics.fade, 1f, 0.04f);
        }
        else if (_restarting)
        {
            running = false;
            transitionSpeedMultiplier = 2f;
            EditorTestLevel l = null;
            if (base.things[typeof(EditorTestLevel)].Count() > 0)
            {
                l = base.things[typeof(EditorTestLevel)].First() as EditorTestLevel;
            }
            if (_level != "")
            {
                Level.current = new ChallengeLevel(_level);
            }
            else
            {
                Level.current = new ChallengeLevel(_levelData, _validityTest);
            }
            Level.current.transitionSpeedMultiplier = 2f;
            ((ChallengeLevel)Level.current)._waitSpawn = 0f;
            if (l != null)
            {
                Level.current.AddThing(l);
            }
        }
        else
        {
            if (_waitFade > 0f)
            {
                return;
            }
            _didFade = true;
            if (_finished)
            {
                running = false;
                PauseLogic();
                if (_finishWait > 0f)
                {
                    _finishWait -= 0.01f;
                    return;
                }
                if (!_playedEndMusic)
                {
                    _playedEndMusic = true;
                    Level.current.simulatePhysics = false;
                    ArcadeFrame frame = null;
                    if (_win)
                    {
                        if (ArcadeLevel.currentArcade != null)
                        {
                            frame = ArcadeLevel.currentArcade.GetFrame();
                            if (frame != null)
                            {
                                Vector2 frameSize = frame.GetRenderTargetSize();
                                float frameMult = frame.GetRenderTargetZoom();
                                if (_captureTarget == null)
                                {
                                    _captureTarget = new RenderTarget2D((int)(frameSize.X * 6f), (int)(frameSize.Y * 6f));
                                }
                                _ = Graphics.width / 320;
                                Camera cam = new Camera(0f, 0f, (float)_captureTarget.width * frameMult, (float)_captureTarget.height * frameMult);
                                if (_duck != null)
                                {
                                    Layer.HUD.visible = false;
                                    MonoMain.RenderGame(MonoMain.screenCapture);
                                    Layer.HUD.visible = true;
                                    Matrix.CreateOrthographicOffCenter(0f, MonoMain.screenCapture.width, MonoMain.screenCapture.height, 0f, 0f, -1f, out var projMatrix);
                                    projMatrix.M41 += -0.5f * projMatrix.M11;
                                    projMatrix.M42 += -0.5f * projMatrix.M22;
                                    Matrix mat = Level.current.camera.getMatrix();
                                    Vector3 pos = Graphics.viewport.Project(new Vector3(_duck.cameraPosition.X, _duck.cameraPosition.Y, 0f), projMatrix, mat, Matrix.Identity);
                                    Graphics.SetRenderTarget(_captureTarget);
                                    cam.center = new Vector2(pos.X, pos.Y);
                                    if (cam.bottom > (float)MonoMain.screenCapture.height)
                                    {
                                        cam.centerY = (float)MonoMain.screenCapture.height - cam.height / 2f;
                                    }
                                    if (cam.top < 0f)
                                    {
                                        cam.centerY = cam.height / 2f;
                                    }
                                    if (cam.right > (float)MonoMain.screenCapture.width)
                                    {
                                        cam.centerX = (float)MonoMain.screenCapture.width - cam.width / 2f;
                                    }
                                    if (cam.left < 0f)
                                    {
                                        cam.centerX = cam.width / 2f;
                                    }
                                    Graphics.Clear(Color.Black);
                                    Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, null, cam.getMatrix());
                                    Graphics.Draw(MonoMain.screenCapture, 0f, 0f);
                                    Graphics.screen.End();
                                    Graphics.SetRenderTarget(null);
                                }
                            }
                        }
                        if (_challenge.wonTrophies.Count > 0 && _challenge.wonTrophies[0].type == TrophyType.Developer)
                        {
                            SFX.Play("developerWin");
                        }
                        else
                        {
                            SFX.Play("challengeWin");
                        }
                        _showEndTextWait = 1f;
                    }
                    else
                    {
                        SFX.Play("challengeLose");
                        _showEndTextWait = 1f;
                    }
                    _trophyGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
                    if (_validityTest && _challenge.wonTrophies.Count > 0 && (_challenge.wonTrophies[0].type == TrophyType.Platinum || _challenge.wonTrophies[0].type == TrophyType.Developer))
                    {
                        _trophyMenu = new UIMenu("@LWING@" + _challenge.challenge.GetNameForDisplay() + "@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 230f, -1f, "@SELECT@CONTINUE");
                    }
                    else
                    {
                        _trophyMenu = new UIMenu("@LWING@" + _challenge.challenge.GetNameForDisplay() + "@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 230f, -1f, "@CANCEL@RETRY @SELECT@CONTINUE");
                    }
                    UIDivider menuBox = new UIDivider(vert: false, 0f, 6f);
                    UIDivider trophyBox = new UIDivider(vert: true, 0f);
                    SpriteMap trophy = new SpriteMap("challengeTrophy", 60, 58);
                    menuBox.leftSection.vertical = false;
                    menuBox.leftSection.borderSize.Y = 2f;
                    bool shouldSaveSnapshot = false;
                    int prevTargets = 0;
                    int prevGoodies = 0;
                    int prevTime = 0;
                    bool earnedBetterTrophy = false;
                    ChallengeSaveData save = _duck.profile.GetSaveData(base.id);
                    if (_challenge.wonTrophies.Count > 0 && _challenge.wonTrophies[0].type > save.trophy)
                    {
                        save.trophy = _challenge.wonTrophies[0].type;
                        earnedBetterTrophy = true;
                    }
                    string bestColor = "|DGRED|";
                    if (save.trophy == TrophyType.Bronze)
                    {
                        bestColor = "|CBRONZE|";
                    }
                    else if (save.trophy == TrophyType.Silver)
                    {
                        bestColor = "|CSILVER|";
                    }
                    else if (save.trophy == TrophyType.Gold)
                    {
                        bestColor = "|CGOLD|";
                    }
                    else if (save.trophy == TrophyType.Platinum)
                    {
                        bestColor = "|CPLATINUM|";
                    }
                    else if (save.trophy == TrophyType.Developer)
                    {
                        bestColor = "|CDEV|";
                    }
                    prevTime = save.bestTime;
                    if (save.bestTime == 0 || (int)(timer.elapsed.TotalSeconds * 1000.0) < save.bestTime)
                    {
                        save.bestTime = (int)(timer.elapsed.TotalSeconds * 1000.0);
                    }
                    prevTargets = save.targets;
                    if (targetsShot > save.targets)
                    {
                        save.targets = targetsShot;
                    }
                    prevGoodies = save.targets;
                    if (goodiesGot > save.goodies)
                    {
                        save.goodies = goodiesGot;
                    }
                    bool foundReq = false;
                    if (_challenge.challenge.hasTimeRequirements)
                    {
                        bool newBestTime = prevTime <= 0 || timer.elapsed.TotalSeconds * 1000.0 < (double)prevTime;
                        string result = FormatResultString("|WHITE|", "TIME", newBestTime ? "|TIMELIME|" : "|DGRED|", MonoMain.TimeString(timer.elapsed, 3, small: true), (_challenge.wonTrophies.Count > 0) ? _challenge.wonTrophies[0].colorString : "|DGRED|", (_challenge.wonTrophies.Count > 0) ? _challenge.wonTrophies[0].name : "FAILED!");
                        menuBox.leftSection.Add(new UIText(result, Color.White, UIAlign.Left));
                        if (newBestTime)
                        {
                            shouldSaveSnapshot = true;
                        }
                        string timeString = MonoMain.TimeString(TimeSpan.FromMilliseconds(save.bestTime), 3, small: true);
                        trophyBox.leftSection.Add(new UIText(FormatResultString(bestColor, "BEST", "|WHITE|", timeString, null, null, 19), Color.White, UIAlign.Left));
                        trophyBox.leftSection.Add(new UIText("               ", Color.White, UIAlign.Left));
                        foundReq = true;
                    }
                    string prefix = "";
                    if (!foundReq && _challenge.challenge.countTargets)
                    {
                        prefix = "TARGETS";
                        if (_challenge.challenge.prefix != "" && _challenge.challenge.prefix != null)
                        {
                            prefix = _challenge.challenge.prefix;
                        }
                        string numString = targetsShot.ToString();
                        if (prevTargets < targetsShot)
                        {
                            shouldSaveSnapshot = true;
                        }
                        trophyBox.leftSection.Add(new UIText(FormatResultString(bestColor, "BEST", "|WHITE|", save.targets.ToString(), null, null, 19), Color.White, UIAlign.Left));
                        trophyBox.leftSection.Add(new UIText("               ", Color.White, UIAlign.Left));
                        string result2 = FormatResultString("|WHITE|", prefix, (targetsShot > prevTargets) ? "|TIMELIME|" : "|DGRED|", numString, (_challenge.wonTrophies.Count > 0) ? _challenge.wonTrophies[0].colorString : "|DGRED|", (_challenge.wonTrophies.Count > 0) ? _challenge.wonTrophies[0].name : "FAILED!");
                        menuBox.leftSection.Add(new UIText(result2, Color.White, UIAlign.Left));
                        foundReq = true;
                    }
                    if (!foundReq && _challenge.challenge.countGoodies)
                    {
                        prefix = "NUMBER";
                        if (_challenge.challenge.prefix != "" && _challenge.challenge.prefix != null)
                        {
                            prefix = _challenge.challenge.prefix;
                        }
                        string numString2 = goodiesGot.ToString();
                        if (prevGoodies < goodiesGot)
                        {
                            shouldSaveSnapshot = true;
                        }
                        trophyBox.leftSection.Add(new UIText(FormatResultString(bestColor, "BEST", "|WHITE|", save.goodies.ToString(), null, null, 19), Color.White, UIAlign.Left));
                        trophyBox.leftSection.Add(new UIText("               ", Color.White, UIAlign.Left));
                        string result3 = FormatResultString("|WHITE|", prefix, (goodiesGot > prevGoodies) ? "|TIMELIME|" : "|DGRED|", numString2, (_challenge.wonTrophies.Count > 0) ? _challenge.wonTrophies[0].colorString : "|DGRED|", (_challenge.wonTrophies.Count > 0) ? _challenge.wonTrophies[0].name : "FAILED!");
                        menuBox.leftSection.Add(new UIText(result3, Color.White, UIAlign.Left));
                        foundReq = true;
                    }
                    if (save.trophy < TrophyType.Gold || !earnedBetterTrophy)
                    {
                        shouldSaveSnapshot = false;
                    }
                    int wd = 19;
                    foreach (ChallengeTrophy t in _challenge.challenge.trophies.OrderBy((ChallengeTrophy x) => 0 - x.type))
                    {
                        if (t.type != TrophyType.Gold && t.type != TrophyType.Silver && t.type != TrophyType.Bronze)
                        {
                            continue;
                        }
                        string resultString = "";
                        if (_challenge.challenge.hasTimeRequirements)
                        {
                            if (t.timeRequirement == 0 && _challenge.challenge.trophies[0].timeRequirement != 0)
                            {
                                t.timeRequirement = _challenge.challenge.trophies[0].timeRequirement;
                            }
                            resultString = ((t.timeRequirement == 0) ? "ANY TIME" : MonoMain.TimeString(TimeSpan.FromSeconds(t.timeRequirement), 3, small: true));
                        }
                        else if (_challenge.challenge.countGoodies)
                        {
                            resultString = t.goodies.ToString();
                        }
                        else if (_challenge.challenge.countTargets)
                        {
                            resultString = t.targets.ToString();
                        }
                        trophyBox.leftSection.Add(new UIText(FormatResultString(t.colorString, t.name, "|WHITE|", resultString, null, null, wd), Color.White, UIAlign.Left));
                        trophyBox.leftSection.Add(new UIText("               ", Color.White, UIAlign.Left));
                    }
                    if (_challenge.wonTrophies.Count > 0)
                    {
                        trophy.frame = (int)_challenge.wonTrophies[0].type;
                    }
                    trophyBox.rightSection.Add(new UIImage(trophy, UIAlign.Right));
                    if (_validityTest && _challenge.wonTrophies.Count > 0 && (_challenge.wonTrophies[0].type == TrophyType.Platinum || _challenge.wonTrophies[0].type == TrophyType.Developer))
                    {
                        _trophyMenu.SetBackFunction(new UIMenuActionCloseMenuSetBoolean(_trophyGroup, _quit));
                        _trophyMenu.SetAcceptFunction(new UIMenuActionCloseMenuSetBoolean(_trophyGroup, _quit));
                    }
                    else
                    {
                        _trophyMenu.SetBackFunction(new UIMenuActionCloseMenuSetBoolean(_trophyGroup, _restart));
                        _trophyMenu.SetAcceptFunction(new UIMenuActionCloseMenuSetBoolean(_trophyGroup, _quit));
                    }
                    menuBox.rightSection.Add(trophyBox);
                    _trophyMenu.Add(menuBox);
                    _trophyMenu.Close();
                    _trophyGroup.Add(_trophyMenu, doAnchor: false);
                    _trophyGroup.Close();
                    Level.Add(_trophyGroup);
                    if (frame != null && shouldSaveSnapshot && save != null)
                    {
                        save.frameID = frame._identifier;
                        save.frameImage = Editor.TextureToString(_captureTarget);
                        frame.saveData = save;
                    }
                    Profiles.Save(_duck.profile);
                }
                if (_showEndTextWait > 0f)
                {
                    _showEndTextWait -= 0.01f;
                    return;
                }
                _fontFade = 1f;
                if (_showResultsWait > 0f)
                {
                    _showResultsWait -= 0.01f;
                }
                else if (!_showedEndMenu)
                {
                    _trophyGroup.Open();
                    _trophyMenu.Open();
                    MonoMain.pauseMenu = _trophyGroup;
                    SFX.Play("pause", 0.6f, -0.2f);
                    _showedEndMenu = true;
                }
                if (_restart.value)
                {
                    _restarting = true;
                    SFX.Play("resume", 0.6f);
                }
                else if (_quit.value)
                {
                    _fading = true;
                    SFX.Play("resume", 0.6f);
                }
                return;
            }
            _waitSpawn -= 0.06f;
            if (!(_waitSpawn <= 0f))
            {
                return;
            }
            if (_pendingSpawns != null && _pendingSpawns.Count > 0)
            {
                _waitSpawn = 0.5f;
                Duck spawn = _pendingSpawns[0];
                AddThing(spawn);
                _pendingSpawns.RemoveAt(0);
                Vector3 col = spawn.profile.persona.color;
                Level.Add(new SpawnLine(spawn.X, spawn.Y, 0, 0f, new Color((int)col.X, (int)col.Z, (int)col.Z), 32f));
                Level.Add(new SpawnLine(spawn.X, spawn.Y, 0, -4f, new Color((int)col.X, (int)col.Y, (int)col.Z), 4f));
                Level.Add(new SpawnLine(spawn.X, spawn.Y, 0, 4f, new Color((int)col.X, (int)col.Y, (int)col.Z), 4f));
                SFX.Play("pullPin", 0.7f);
                _duck = spawn;
                _challenge = base.things[typeof(ChallengeMode)].First() as ChallengeMode;
                if (_challenge == null)
                {
                    return;
                }
                _challenge.PrepareCounts();
                random = _challenge.random.value;
                _challenge.duck = spawn;
                _timer.maxTime = TimeSpan.FromSeconds(_challenge.challenge.trophies[0].timeRequirement);
                HUD.AddCornerTimer(HUDCorner.BottomRight, "", _timer);
                if (_challenge.challenge.countTargets)
                {
                    int baseline = _challenge.challenge.trophies[0].targets;
                    if (baseline < 0 && _challenge.goalTypes != null && _challenge.goalTypes.Count > 0)
                    {
                        baseline = 0;
                        foreach (GoalType t2 in _challenge.goalTypes)
                        {
                            baseline += t2.numObjectsRemaining;
                        }
                    }
                    HUD.AddCornerCounter(HUDCorner.BottomLeft, "@RETICULE@", new FieldBinding(this, "targetsShot"), (baseline > 0) ? baseline : 0);
                }
                if (_challenge.challenge.countGoodies)
                {
                    MultiMap<Type, ISequenceItem> types = new MultiMap<Type, ISequenceItem>();
                    foreach (ISequenceItem seq in Level.current.things[typeof(ISequenceItem)])
                    {
                        Type t3 = seq.GetType();
                        SequenceItem s = (seq as Thing).sequence;
                        if (s.isValid && s.type == SequenceItemType.Goody)
                        {
                            types.Add(t3, seq);
                        }
                    }
                    Type most = null;
                    int num = 0;
                    foreach (KeyValuePair<Type, List<ISequenceItem>> t4 in types)
                    {
                        if (t4.Value.Count > num)
                        {
                            most = t4.Key;
                            num = t4.Value.Count;
                        }
                    }
                    if (most != null)
                    {
                        ISequenceItem g = types[most][0];
                        string image = "@STARGOODY@";
                        if (g is LapGoody || g is InvisiGoody)
                        {
                            image = "@LAPGOODY@";
                        }
                        else if (g is SuitcaseGoody)
                        {
                            image = "@SUITCASEGOODY@";
                        }
                        else if (g is Window || g is YellowBarrel || g is Door)
                        {
                            image = "@RETICULE@";
                        }
                        int baseline2 = _challenge.challenge.trophies[0].goodies;
                        HUD.AddCornerCounter(HUDCorner.BottomLeft, image, new FieldBinding(this, "goodiesGot"), (baseline2 > 0) ? baseline2 : 0);
                    }
                }
                if (_firstStart)
                {
                    int highestOrder = -1;
                    foreach (TargetDuck d in base.things[typeof(TargetDuck)])
                    {
                        if (d.sequence.order > highestOrder)
                        {
                            highestOrder = d.sequence.order;
                        }
                    }
                    foreach (TargetDuck d2 in base.things[typeof(TargetDuck)])
                    {
                        if (d2.sequence.order == -1)
                        {
                            if (highestOrder <= 0)
                            {
                                d2.sequence.order = Rando.Int(255);
                            }
                            else
                            {
                                d2.sequence.order = Rando.Int(highestOrder);
                            }
                        }
                    }
                    if (base.things[typeof(RandomControllerNew)].Count() == 0)
                    {
                        if (random)
                        {
                            IEnumerable<Thing> stuff = base.things[typeof(ISequenceItem)];
                            if (stuff.Count() > 0)
                            {
                                stuff.ElementAt(ChallengeRando.Int(stuff.Count() - 1)).sequence.BeginRandomSequence();
                            }
                        }
                        else
                        {
                            foreach (TargetDuck d3 in base.things[typeof(TargetDuck)])
                            {
                                if (d3.sequence.order == 0)
                                {
                                    d3.sequence.Activate();
                                }
                            }
                        }
                    }
                    _firstStart = false;
                }
                if (Music.stopped)
                {
                    if (_challenge.music == "")
                    {
                        Music.Load("Challenging");
                    }
                    else if (_challenge.music == "donutmystery")
                    {
                        Music.Load("spacemystery");
                    }
                    else
                    {
                        Music.Load(Music.FindSong(_challenge.music));
                    }
                }
            }
            else if (!_started)
            {
                _waitAfterSpawn -= 0.06f;
                if (!(_waitAfterSpawn <= 0f))
                {
                    return;
                }
                _waitAfterSpawnDings++;
                if (_waitAfterSpawnDings > 2)
                {
                    _started = true;
                    base.simulatePhysics = true;
                    running = true;
                    SFX.Play("ding");
                    _timer.Start();
                    if (Music.stopped)
                    {
                        Music.PlayLoaded();
                    }
                }
                else
                {
                    SFX.Play("preStartDing");
                }
                _waitSpawn = 1.1f;
            }
            else
            {
                _fontFade -= 0.1f;
                if (_fontFade < 0f)
                {
                    _fontFade = 0f;
                }
                PauseLogic();
            }
        }
    }

    public void PauseLogic()
    {
        if (Input.Pressed("START"))
        {
            _pauseGroup.Open();
            _pauseMenu.Open();
            MonoMain.pauseMenu = _pauseGroup;
            if (!_paused)
            {
                SFX.Play("pause", 0.6f);
                _timer.Stop();
                _paused = true;
            }
            base.simulatePhysics = false;
        }
        else if (_paused && MonoMain.pauseMenu == null)
        {
            _paused = false;
            SFX.Play("resume", 0.6f);
            _waitAfterSpawn = 1f;
            _waitAfterSpawnDings = 0;
            _started = false;
            _fontFade = 1f;
            if (_restart.value)
            {
                _restarting = true;
            }
            else if (_quit.value)
            {
                _fading = true;
            }
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.HUD && (!_started || _finished) && _waitAfterSpawnDings > 0 && _fontFade > 0.01f)
        {
            _font.Scale = new Vector2(2f, 2f);
            _font.Alpha = _fontFade;
            string s = "GET";
            if (_finished)
            {
                s = ((!_win) ? "LOSE!" : ((!_developer) ? "WIN!" : "WOAH!"));
            }
            else if (_waitAfterSpawnDings == 2)
            {
                s = "READY";
            }
            else if (_waitAfterSpawnDings == 3)
            {
                s = "";
            }
            float wide = _font.GetWidth(s);
            float thick = 1f;
            _font.Draw(s, Layer.HUD.camera.width / 2f - wide / 2f - thick, Layer.HUD.camera.height / 2f - _font.height / 2f - thick, Color.Black, 0.9f);
            _font.Draw(s, Layer.HUD.camera.width / 2f - wide / 2f - thick, Layer.HUD.camera.height / 2f - _font.height / 2f + thick, Color.Black, 0.9f);
            _font.Draw(s, Layer.HUD.camera.width / 2f - wide / 2f + thick, Layer.HUD.camera.height / 2f - _font.height / 2f - thick, Color.Black, 0.9f);
            _font.Draw(s, Layer.HUD.camera.width / 2f - wide / 2f + thick, Layer.HUD.camera.height / 2f - _font.height / 2f + thick, Color.Black, 0.9f);
            _font.Draw(s, Layer.HUD.camera.width / 2f - wide / 2f, Layer.HUD.camera.height / 2f - _font.height / 2f, Color.White, 1f);
        }
        base.PostDrawLayer(layer);
    }
}
