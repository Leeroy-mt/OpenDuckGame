using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UIMatchmakerMark2 : UIMenu
{
    public class Core
    {
        public UIMatchmakerMark2 instance;
    }

    public enum State
    {
        InitializeMatchmaking,
        InitializeMatchmakingFinish,
        GetNumberOfLobbies,
        WaitForQuery,
        SearchForLobbies,
        TryJoiningLobbies,
        JoinLobby,
        PrepareLobby,
        Aborting,
        Idle,
        Failed,
        PlatformCancelled
    }

    private static Core _core = new Core();

    public static bool pulseLocal;

    public static bool pulseNetwork;

    protected bool _continueSearchOnFail = true;

    private UIMenu _openOnClose;

    private Sprite _window;

    private BitmapFont _font;

    private FancyBitmapFont _fancyFont;

    private SpriteMap _signalCrossLocal;

    private SpriteMap _signalCrossNetwork;

    private SpriteMap _matchmakingSignal;

    private List<SpriteMap> _matchmakingStars = new List<SpriteMap>();

    private bool playMusic = true;

    protected int _totalLobbies = -1;

    protected int _joinableLobbies = -1;

    protected State _state;

    protected State _previousState;

    protected int _timeInState;

    protected UIServerBrowser.LobbyData _directConnectLobby;

    private int _timeOpen;

    private static Level _currentLevel;

    public Lobby _hostedLobby;

    public Lobby _processing;

    public HashSet<ulong> blacklist = new HashSet<ulong>();

    public HashSet<ulong> attempted = new HashSet<ulong>();

    protected int _wait;

    public static int searchMode = 1;

    protected bool _resetNetwork;

    private int _framesSinceReset;

    private bool _resetting;

    protected string _passwordAttempt = "";

    protected List<string> messages = new List<string>();

    private float _scroll;

    private float _dots;

    protected string _caption = "MATCHMAKING";

    public static Core core
    {
        get
        {
            return _core;
        }
        set
        {
            _core = value;
        }
    }

    public static UIMatchmakerMark2 instance
    {
        get
        {
            if (_currentLevel != Level.current)
            {
                _core.instance = null;
            }
            return _core.instance;
        }
        set
        {
            _core.instance = value;
        }
    }

    protected void ChangeState(State pState)
    {
        if (_directConnectLobby != null && (pState == State.TryJoiningLobbies || pState == State.SearchForLobbies))
        {
            pState = State.Failed;
        }
        if (pState == State.Failed && _previousState != State.Failed)
        {
            HUD.CloseAllCorners();
            HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@RETURN");
            messages.Add("|DGRED|Unable to connect to server.");
        }
        _previousState = _state;
        _state = pState;
        _timeInState = 0;
        Wait();
    }

    public static UIMatchmakerMark2 Platform_GetMatchkmaker(UIServerBrowser.LobbyData joinLobby, UIMenu openOnClose)
    {
        return new UIMatchmakerSteam(joinLobby, openOnClose);
    }

    protected UIMatchmakerMark2(UIServerBrowser.LobbyData joinLobby, UIMenu openOnClose)
        : this(openOnClose)
    {
        _directConnectLobby = joinLobby;
    }

    protected UIMatchmakerMark2(UIMenu openOnClose)
        : base("", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f)
    {
        _openOnClose = openOnClose;
        Graphics.fade = 1f;
        _window = new Sprite("online/matchmaking_mk2");
        _window.CenterOrigin();
        _font = new BitmapFont("biosFontUI", 8, 7);
        _fancyFont = new FancyBitmapFont("smallFont");
        _matchmakingSignal = new SpriteMap("online/matchmakingSignal", 4, 9);
        _matchmakingSignal.CenterOrigin();
        SpriteMap star = new SpriteMap("online/matchmakingStar", 7, 7);
        star.AddAnimation("flicker", 0.08f, true, 0, 1, 2, 1);
        star.SetAnimation("flicker");
        star.CenterOrigin();
        _signalCrossLocal = new SpriteMap("online/signalCross", 5, 5);
        _signalCrossLocal.AddAnimation("idle", 0.12f, true, default(int));
        _signalCrossLocal.AddAnimation("flicker", 0.12f, false, 1, 2, 3);
        _signalCrossLocal.SetAnimation("idle");
        _signalCrossLocal.CenterOrigin();
        _signalCrossNetwork = new SpriteMap("online/signalCross", 5, 5);
        _signalCrossNetwork.AddAnimation("idle", 0.12f, true, default(int));
        _signalCrossNetwork.AddAnimation("flicker", 0.12f, false, 1, 2, 3);
        _signalCrossNetwork.SetAnimation("idle");
        _signalCrossNetwork.CenterOrigin();
        _matchmakingStars.Add(star);
        star = new SpriteMap("online/matchmakingStar", 7, 7);
        star.AddAnimation("flicker", 0.11f, true, 0, 1, 2, 1);
        star.SetAnimation("flicker");
        star.CenterOrigin();
        _matchmakingStars.Add(star);
        star = new SpriteMap("online/matchmakingStar", 7, 7);
        star.AddAnimation("flicker", 0.03f, true, 0, 1, 2, 1);
        star.SetAnimation("flicker");
        star.CenterOrigin();
        _matchmakingStars.Add(star);
        star = new SpriteMap("online/matchmakingStar", 7, 7);
        star.AddAnimation("flicker", 0.03f, true, 0, 1, 2, 1);
        star.SetAnimation("flicker");
        star.CenterOrigin();
        _matchmakingStars.Add(star);
    }

    protected virtual void Platform_Open()
    {
    }

    public override void Open()
    {
        _state = State.InitializeMatchmaking;
        Platform_Open();
        _timeOpen = 0;
        _currentLevel = Level.current;
        instance = this;
        _processing = null;
        messages.Clear();
        if (_directConnectLobby != null)
        {
            _state = State.TryJoiningLobbies;
        }
        else
        {
            messages.Add("|DGYELLOW|Connecting to servers on the Moon...");
        }
        _totalLobbies = -1;
        _joinableLobbies = -1;
        attempted.Clear();
        blacklist.Clear();
        HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@ABORT");
        if (playMusic && _directConnectLobby == null)
        {
            Music.Play("jazzroom");
        }
        base.Open();
    }

    public override void Close()
    {
        if (instance == this)
        {
            instance = null;
        }
        _state = State.Idle;
        base.Close();
    }

    public void FinishAndClose()
    {
        if (!Network.isActive)
        {
            Level.current = new TeamSelect2();
            Level.UpdateLevelChange();
        }
        HUD.CloseAllCorners();
        Close();
        if (!Network.isActive)
        {
            if (_openOnClose is UIServerBrowser)
            {
                _openOnClose.Open();
                MonoMain.pauseMenu = _openOnClose;
            }
            else
            {
                MonoMain.pauseMenu = null;
            }
            if (!Network.isActive && Level.current is TeamSelect2 && (Level.current as TeamSelect2)._beam != null)
            {
                (Level.current as TeamSelect2)._beam.ClearBeam();
            }
        }
    }

    public void Hook_OnSessionEnded(DuckNetErrorInfo error)
    {
        _resetting = false;
        _framesSinceReset = 0;
        if (_state != State.Aborting)
        {
            if (_hostedLobby == null)
            {
                if (error == null || error.error != DuckNetError.HostIsABlockedUser)
                {
                    messages.Add("|PURPLE|LOBBY    |DGRED|Connection to lobby failed:");
                    OnConnectionError(error);
                    if (_directConnectLobby == null)
                    {
                        messages.Add("|PURPLE|LOBBY    |WHITE|Looking for more lobbies to try...");
                    }
                }
                _wait += 60;
            }
            ChangeState(State.TryJoiningLobbies);
        }
        _processing = null;
        _hostedLobby = null;
    }

    public virtual void Hook_OnLobbyProcessed(object pLobby)
    {
    }

    public void OnConnectionError(DuckNetErrorInfo error)
    {
        if (error != null)
        {
            if (error.error == DuckNetError.YourVersionTooNew || error.error == DuckNetError.YourVersionTooOld)
            {
                if (error.error == DuckNetError.YourVersionTooNew)
                {
                    messages.Add("|DGRED|Their version was older.");
                }
                else
                {
                    messages.Add("|DGRED|Their version was newer.");
                }
                if (_processing != null)
                {
                    blacklist.Add(_processing.id);
                }
            }
            else if (error.error == DuckNetError.FullServer)
            {
                messages.Add("|DGRED|Failed (FULL SERVER)");
            }
            else if (error.error == DuckNetError.ConnectionTimeout)
            {
                messages.Add("|DGRED|Failed (TIMEOUT)");
            }
            else if (error.error == DuckNetError.GameInProgress)
            {
                messages.Add("|DGRED|Failed (IN PROGRESS)");
            }
            else if (error.error == DuckNetError.GameNotFoundOrClosed)
            {
                messages.Add("|DGRED|Failed (NO LONGER AVAILABLE)");
            }
            else if (error.error == DuckNetError.ClientDisconnected)
            {
                messages.Add("|DGYELLOW|Disconnected");
            }
            else if (error.error == DuckNetError.InvalidPassword)
            {
                messages.Add("|DGRED|Password was incorrect!");
            }
            else if (error.error == DuckNetError.ModsIncompatible)
            {
                messages.Add("|DGRED|Host had different mods enabled!");
            }
            else if (error.error != DuckNetError.HostIsABlockedUser)
            {
                messages.Add("|DGRED|Unknown connection error.");
                if (_processing != null)
                {
                    blacklist.Add(_processing.id);
                }
            }
        }
        else
        {
            messages.Add("|DGRED|Connection timeout.");
            if (_processing != null)
            {
                blacklist.Add(_processing.id);
            }
        }
    }

    public virtual void Hook_OnDucknetJoined()
    {
        if (Level.current is TeamSelect2)
        {
            (Level.current as TeamSelect2).CloseAllDialogs();
        }
        if (Network.isServer)
        {
            Level.current = new TeamSelect2();
            Level.current.suppressLevelMessage = true;
        }
        Close();
        DevConsole.Log("|PURPLE|LOBBY    |DGGREEN|Finished! (HOST).", Color.White);
    }

    private void Wait()
    {
        _wait += 60;
    }

    protected bool HostLobby()
    {
        if (_hostedLobby == null && Reset())
        {
            messages.Add("|DGYELLOW|Having trouble finding an open lobby...");
            messages.Add("|DGGREEN|Creating a lobby of our very own...");
            DuckNetwork.Host(TeamSelect2.GetSettingInt("maxplayers"), NetworkLobbyType.Public);
            _hostedLobby = Network.activeNetwork.core.lobby;
            DevConsole.Log("|PURPLE|LOBBY    |DGYELLOW|Opened lobby while searching.", Color.White);
            _wait = 280 + Rando.Int(120);
        }
        return _hostedLobby != null;
    }

    protected virtual void Platform_ResetLogic()
    {
    }

    protected bool Reset()
    {
        if (Network.isActive)
        {
            Platform_ResetLogic();
            _resetNetwork = true;
            Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Matchmaking disconnect."));
            _resetting = true;
            _framesSinceReset = 0;
        }
        return !Network.isActive;
    }

    public virtual void Platform_Update()
    {
    }

    public virtual void Platform_MatchmakerLogic()
    {
    }

    public override void Update()
    {
        if (!base.open)
        {
            return;
        }
        _timeInState++;
        if (instance == null)
        {
            FinishAndClose();
            return;
        }
        if (_resetNetwork && Network.isActive)
        {
            Reset();
            return;
        }
        _resetNetwork = false;
        if (Input.Pressed("CANCEL"))
        {
            Reset();
            messages.Add("|DGRED|Aborting...");
            ChangeState(State.Aborting);
        }
        if (_resetting)
        {
            _framesSinceReset++;
            if (_framesSinceReset > 120)
            {
                Network.Terminate();
                if (_state != State.Aborting)
                {
                    ChangeState(State.TryJoiningLobbies);
                }
                _processing = null;
                _hostedLobby = null;
            }
        }
        else
        {
            _timeOpen++;
            if (_timeOpen > 7200)
            {
                attempted.Clear();
            }
            Platform_Update();
            if (_wait > 0 && _state != State.Aborting && _state != State.JoinLobby)
            {
                _wait--;
            }
            else
            {
                Platform_MatchmakerLogic();
            }
        }
    }

    public void SetPasswordAttempt(string pPassword)
    {
        _passwordAttempt = pPassword;
    }

    public override void Draw()
    {
        if (!base.open)
        {
            return;
        }
        _window.depth = base.depth;
        Graphics.Draw(_window, base.x, base.y);
        _scroll += 0.06f;
        if (_scroll > 9f)
        {
            _scroll = 0f;
        }
        _dots += 0.01f;
        if (_dots > 1f)
        {
            _dots = 0f;
        }
        if (_state == State.Idle || _state == State.Failed)
        {
            _signalCrossLocal.SetAnimation("idle");
            pulseLocal = false;
        }
        else if (_signalCrossLocal.currentAnimation == "idle")
        {
            if (pulseLocal)
            {
                _signalCrossLocal.SetAnimation("flicker");
                pulseLocal = false;
            }
        }
        else if (_signalCrossLocal.finished)
        {
            _signalCrossLocal.SetAnimation("idle");
        }
        if (_signalCrossNetwork.currentAnimation == "idle")
        {
            if (pulseNetwork)
            {
                _signalCrossNetwork.SetAnimation("flicker");
                pulseNetwork = false;
            }
        }
        else if (_signalCrossNetwork.finished)
        {
            _signalCrossNetwork.SetAnimation("idle");
        }
        float starsOffsetY = base.y - 10f;
        if (_state != State.Failed)
        {
            for (int i = 0; i < 7; i++)
            {
                float leftPos = base.x - 28f;
                float signalPos = leftPos + (float)(i * 9) + (float)Math.Round(_scroll);
                float maxPos = leftPos + 63f;
                float num = (signalPos - leftPos) / (maxPos - leftPos);
                _matchmakingSignal.depth = base.depth + 4;
                if (num > -0.1f)
                {
                    _matchmakingSignal.frame = 0;
                }
                if (num > 0.05f)
                {
                    _matchmakingSignal.frame = 1;
                }
                if (num > 0.1f)
                {
                    _matchmakingSignal.frame = 2;
                }
                if (num > 0.9f)
                {
                    _matchmakingSignal.frame = 1;
                }
                if (num > 0.95f)
                {
                    _matchmakingSignal.frame = 0;
                }
                Graphics.Draw(_matchmakingSignal, signalPos, starsOffsetY - 21f);
            }
        }
        _matchmakingStars[0].depth = base.depth + 2;
        Graphics.Draw(_matchmakingStars[0], base.x - 9f, starsOffsetY - 18f);
        _matchmakingStars[1].depth = base.depth + 2;
        Graphics.Draw(_matchmakingStars[1], base.x + 31f, starsOffsetY - 22f);
        _matchmakingStars[2].depth = base.depth + 2;
        Graphics.Draw(_matchmakingStars[2], base.x + 12f, starsOffsetY - 20f);
        _matchmakingStars[3].depth = base.depth + 2;
        Graphics.Draw(_matchmakingStars[3], base.x - 23f, starsOffsetY - 21f);
        _signalCrossLocal.depth = base.depth + 2;
        Graphics.Draw(_signalCrossLocal, base.x - 45f, starsOffsetY - 19f);
        _signalCrossNetwork.depth = base.depth + 2;
        Graphics.Draw(_signalCrossNetwork, base.x + 55f, starsOffsetY - 23f);
        Vec2 fontPos = new Vec2(0f - _font.GetWidth(_caption) / 2f, -52f);
        _font.DrawOutline(_caption, position + fontPos, Color.White, Color.Black, base.depth + 2);
        _fancyFont.scale = new Vec2(0.5f);
        int yOff = 0;
        while (messages.Count > 10)
        {
            messages.RemoveAt(0);
        }
        int listIDX = 0;
        foreach (string s in messages)
        {
            string t = s;
            if (listIDX == messages.Count - 1)
            {
                string elipsis = "";
                if (s.EndsWith("..."))
                {
                    t = s.Substring(0, s.Length - 3);
                    string elipseChar = ".";
                    if ((t.Count() > 0 && t.Last() == '!') || t.Last() == '.' || t.Last() == '?')
                    {
                        elipseChar = t.Last().ToString() ?? "";
                        t = t.Substring(0, t.Length - 1);
                    }
                    for (int j = 0; j < 3; j++)
                    {
                        if (_dots * 4f > (float)(j + 1))
                        {
                            elipsis += elipseChar;
                        }
                    }
                    t += elipsis;
                }
            }
            _fancyFont.Draw(t, new Vec2(base.x - 64f, base.y - 18f + (float)(yOff * 6)), Color.White, base.depth + 2);
            yOff++;
            listIDX++;
        }
        if (_directConnectLobby != null)
        {
            return;
        }
        if (_totalLobbies >= 0)
        {
            if (_totalLobbies > 1)
            {
                _fancyFont.Draw("Found " + _totalLobbies + " games already in progress.", position + new Vec2(-65f, 49f), Color.Black, base.depth + 2);
            }
            else
            {
                _fancyFont.Draw("Found " + _totalLobbies + " game already in progress.", position + new Vec2(-65f, 49f), Color.Black, base.depth + 2);
            }
        }
        else
        {
            _fancyFont.Draw("Querying moon...", position + new Vec2(-65f, 49f), Color.Black, base.depth + 2);
        }
    }
}
