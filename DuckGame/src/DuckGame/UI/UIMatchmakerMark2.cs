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

    #region Public Fields

    public static bool pulseLocal;

    public static bool pulseNetwork;

    public static int searchMode = 1;

    public Lobby _hostedLobby;

    public Lobby _processing;

    public HashSet<ulong> blacklist = [];

    public HashSet<ulong> attempted = [];

    #endregion

    #region Protected Fields

    protected bool _continueSearchOnFail = true;

    protected bool _resetNetwork;

    protected int _totalLobbies = -1;

    protected int _joinableLobbies = -1;

    protected int _timeInState;

    protected int _wait;

    protected string _caption = "MATCHMAKING";

    protected string _passwordAttempt = "";

    protected State _state;

    protected State _previousState;

    protected UIServerBrowser.LobbyData _directConnectLobby;

    protected List<string> messages = [];

    #endregion

    #region Private Fields

    static Core _core = new();

    static Level _currentLevel;

    bool playMusic = true;

    bool _resetting;

    int _timeOpen;

    int _framesSinceReset;

    float _scroll;

    float _dots;

    UIMenu _openOnClose;

    Sprite _window;

    BitmapFont _font;

    FancyBitmapFont _fancyFont;

    SpriteMap _signalCrossLocal;

    SpriteMap _signalCrossNetwork;

    SpriteMap _matchmakingSignal;

    List<SpriteMap> _matchmakingStars = [];

    #endregion

    #region Public Properties

    public static UIMatchmakerMark2 instance
    {
        get
        {
            if (_currentLevel != Level.current)
                _core.instance = null;
            return _core.instance;
        }
        set => _core.instance = value;
    }

    public static Core core
    {
        get => _core;
        set => _core = value;
    }

    #endregion

    #region Protected Constructors

    protected UIMatchmakerMark2(UIServerBrowser.LobbyData joinLobby, UIMenu openOnClose)
        : this(openOnClose)
    {
        _directConnectLobby = joinLobby;
    }

    protected UIMatchmakerMark2(UIMenu openOnClose)
        : base("", Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 190)
    {
        _openOnClose = openOnClose;
        Graphics.fade = 1;
        _window = new Sprite("online/matchmaking_mk2");
        _window.CenterOrigin();
        _font = new BitmapFont("biosFontUI", 8, 7);
        _fancyFont = new FancyBitmapFont("smallFont");
        _matchmakingSignal = new SpriteMap("online/matchmakingSignal", 4, 9);
        _matchmakingSignal.CenterOrigin();
        SpriteMap star = new("online/matchmakingStar", 7, 7);
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

    #endregion

    #region Public Methods

    public static UIMatchmakerMark2 Platform_GetMatchkmaker(UIServerBrowser.LobbyData joinLobby, UIMenu openOnClose) => new UIMatchmakerSteam(joinLobby, openOnClose);

    public virtual void Hook_OnLobbyProcessed(object pLobby) { }

    public virtual void Platform_Update() { }

    public virtual void Platform_MatchmakerLogic() { }

    public virtual void Hook_OnDucknetJoined()
    {
        if (Level.current is TeamSelect2)
            (Level.current as TeamSelect2).CloseAllDialogs();
        if (Network.isServer)
        {
            Level.current = new TeamSelect2();
            Level.current.suppressLevelMessage = true;
        }
        Close();
        DevConsole.Log("|PURPLE|LOBBY    |DGGREEN|Finished! (HOST).", Color.White);
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
            _state = State.TryJoiningLobbies;
        else
            messages.Add("|DGYELLOW|Connecting to servers on the Moon...");
        _totalLobbies = -1;
        _joinableLobbies = -1;
        attempted.Clear();
        blacklist.Clear();
        HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@ABORT");
        if (playMusic && _directConnectLobby == null)
            Music.Play("jazzroom");
        base.Open();
    }

    public override void Close()
    {
        if (instance == this)
            instance = null;
        _state = State.Idle;
        base.Close();
    }

    public override void Update()
    {
        if (!open)
            return;

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
                    ChangeState(State.TryJoiningLobbies);
                _processing = null;
                _hostedLobby = null;
            }
        }
        else
        {
            _timeOpen++;
            if (_timeOpen > 7200)
                attempted.Clear();
            Platform_Update();
            if (_wait > 0 && _state != State.Aborting && _state != State.JoinLobby)
                _wait--;
            else
                Platform_MatchmakerLogic();
        }
    }

    public override void Draw()
    {
        if (!open)
            return;

        _window.Depth = Depth;
        Graphics.Draw(_window, X, Y);
        _scroll += 0.06f;
        if (_scroll > 9)
            _scroll = 0;
        _dots += 0.01f;
        if (_dots > 1)
            _dots = 0;
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
            _signalCrossLocal.SetAnimation("idle");
        if (_signalCrossNetwork.currentAnimation == "idle")
        {
            if (pulseNetwork)
            {
                _signalCrossNetwork.SetAnimation("flicker");
                pulseNetwork = false;
            }
        }
        else if (_signalCrossNetwork.finished)
            _signalCrossNetwork.SetAnimation("idle");
        float starsOffsetY = Y - 10;
        if (_state != State.Failed)
        {
            for (int i = 0; i < 7; i++)
            {
                float leftPos = X - 28;
                float signalPos = leftPos + (i * 9) + (float)Math.Round(_scroll);
                float maxPos = leftPos + 63;
                float num = (signalPos - leftPos) / (maxPos - leftPos);
                _matchmakingSignal.Depth = Depth + 4;
                if (num > -0.1f)
                    _matchmakingSignal.frame = 0;
                if (num > 0.05f)
                    _matchmakingSignal.frame = 1;
                if (num > 0.1f)
                    _matchmakingSignal.frame = 2;
                if (num > 0.9f)
                    _matchmakingSignal.frame = 1;
                if (num > 0.95f)
                    _matchmakingSignal.frame = 0;
                Graphics.Draw(_matchmakingSignal, signalPos, starsOffsetY - 21);
            }
        }
        _matchmakingStars[0].Depth = Depth + 2;
        Graphics.Draw(_matchmakingStars[0], X - 9, starsOffsetY - 18);
        _matchmakingStars[1].Depth = Depth + 2;
        Graphics.Draw(_matchmakingStars[1], X + 31, starsOffsetY - 22);
        _matchmakingStars[2].Depth = Depth + 2;
        Graphics.Draw(_matchmakingStars[2], X + 12, starsOffsetY - 20);
        _matchmakingStars[3].Depth = Depth + 2;
        Graphics.Draw(_matchmakingStars[3], X - 23, starsOffsetY - 21);
        _signalCrossLocal.Depth = Depth + 2;
        Graphics.Draw(_signalCrossLocal, X - 45, starsOffsetY - 19);
        _signalCrossNetwork.Depth = Depth + 2;
        Graphics.Draw(_signalCrossNetwork, X + 55, starsOffsetY - 23);
        Vec2 fontPos = new(-_font.GetWidth(_caption) / 2, -52);
        _font.DrawOutline(_caption, Position + fontPos, Color.White, Color.Black, Depth + 2);
        _fancyFont.Scale = new Vec2(0.5f);
        int yOff = 0;
        while (messages.Count > 10)
            messages.RemoveAt(0);
        int listIDX = 0;
        foreach (string s in messages)
        {
            string t = s;
            if (listIDX == messages.Count - 1)
            {
                string elipsis = "";
                if (s.EndsWith("..."))
                {
                    t = s[..^3];
                    string elipseChar = ".";
                    if ((t.Length > 0 && t.Last() == '!') || t.Last() == '.' || t.Last() == '?')
                    {
                        elipseChar = t.Last().ToString() ?? "";
                        t = t[..^1];
                    }
                    for (int j = 0; j < 3; j++)
                        if (_dots * 4 > j + 1)
                            elipsis += elipseChar;
                    t += elipsis;
                }
            }
            _fancyFont.Draw(t, new Vec2(X - 64, Y - 18 + yOff * 6), Color.White, Depth + 2);
            yOff++;
            listIDX++;
        }
        if (_directConnectLobby != null)
            return;
        if (_totalLobbies >= 0)
        {
            if (_totalLobbies > 1)
                _fancyFont.Draw($"Found {_totalLobbies} games already in progress.", Position + new Vec2(-65, 49), Color.Black, Depth + 2);
            else
                _fancyFont.Draw($"Found {_totalLobbies} game already in progress.", Position + new Vec2(-65, 49), Color.Black, Depth + 2);
        }
        else
            _fancyFont.Draw("Querying moon...", Position + new Vec2(-65, 49), Color.Black, Depth + 2);
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
                MonoMain.pauseMenu = null;
            if (!Network.isActive && Level.current is TeamSelect2 && (Level.current as TeamSelect2)._beam != null)
                (Level.current as TeamSelect2)._beam.ClearBeam();
        }
    }

    public void SetPasswordAttempt(string pPassword)
    {
        _passwordAttempt = pPassword;
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
                        messages.Add("|PURPLE|LOBBY    |WHITE|Looking for more lobbies to try...");
                }
                _wait += 60;
            }
            ChangeState(State.TryJoiningLobbies);
        }
        _processing = null;
        _hostedLobby = null;
    }

    public void OnConnectionError(DuckNetErrorInfo error)
    {
        if (error != null)
        {
            if (error.error == DuckNetError.YourVersionTooNew || error.error == DuckNetError.YourVersionTooOld)
            {
                if (error.error == DuckNetError.YourVersionTooNew)
                    messages.Add("|DGRED|Their version was older.");
                else
                    messages.Add("|DGRED|Their version was newer.");
                if (_processing != null)
                    blacklist.Add(_processing.id);
            }
            else if (error.error == DuckNetError.FullServer)
                messages.Add("|DGRED|Failed (FULL SERVER)");
            else if (error.error == DuckNetError.ConnectionTimeout)
                messages.Add("|DGRED|Failed (TIMEOUT)");
            else if (error.error == DuckNetError.GameInProgress)
                messages.Add("|DGRED|Failed (IN PROGRESS)");
            else if (error.error == DuckNetError.GameNotFoundOrClosed)
                messages.Add("|DGRED|Failed (NO LONGER AVAILABLE)");
            else if (error.error == DuckNetError.ClientDisconnected)
                messages.Add("|DGYELLOW|Disconnected");
            else if (error.error == DuckNetError.InvalidPassword)
                messages.Add("|DGRED|Password was incorrect!");
            else if (error.error == DuckNetError.ModsIncompatible)
                messages.Add("|DGRED|Host had different mods enabled!");
            else if (error.error != DuckNetError.HostIsABlockedUser)
            {
                messages.Add("|DGRED|Unknown connection error.");
                if (_processing != null)
                    blacklist.Add(_processing.id);
            }
        }
        else
        {
            messages.Add("|DGRED|Connection timeout.");
            if (_processing != null)
                blacklist.Add(_processing.id);
        }
    }

    #endregion

    #region Protected Methods

    protected virtual void Platform_Open() { }

    protected virtual void Platform_ResetLogic() { }

    protected void ChangeState(State pState)
    {
        if (_directConnectLobby != null && (pState == State.TryJoiningLobbies || pState == State.SearchForLobbies))
            pState = State.Failed;
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

    #endregion

    void Wait()
    {
        _wait += 60;
    }
}
