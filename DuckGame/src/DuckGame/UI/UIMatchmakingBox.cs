using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UIMatchmakingBox : UIMenu
{
    #region Public Fields

    public static bool errorConnectingToGame;

    public List<BlacklistServer> _permenantBlacklist = [];

    #endregion

    #region Protected Fields

    protected bool playMusic = true;

    protected bool _continueSearchOnFail = true;

    protected bool _searchingIsOver;

    protected string _caption = "LOOKING";

    protected Lobby _tryConnectLobby;

    protected List<string> _newStatusList = [];

    #endregion

    #region Private Fields

    static MatchmakingBoxCore _core = new();

    bool triedHostingAlready;

    bool _quit;

    int _totalLobbiesFound = -1;

    int _tries;

    int searchTryIndex;

    int _totalInGameLobbies;

    int _triesSinceSearch;

    int _connectTimeout;

    float _newStatusWait = 1;

    float _dots;

    float _scroll;

    float _stateWait;

    MatchmakingState _pendingState;

    Sprite _frame;

    SpriteMap _matchmakingSignal;

    BitmapFont _font;

    FancyBitmapFont _fancyFont;

    Lobby _tryHostingLobby;

    SpriteMap _signalCrossLocal;

    SpriteMap _signalCrossNetwork;

    UIMenu _openOnClose;

    Level _currentLevel;

    List<string> _statusList = [];

    List<SpriteMap> _matchmakingStars = [];

    List<BlacklistServer> _failedAttempts = [];

    Dictionary<Profile, Team> _teamProfileLinks = [];

    #endregion

    #region Public Properties

    public static MatchmakingBoxCore core
    {
        get => _core; 
        set => _core = value;
    }

    public List<MatchmakingPlayer> matchmakingProfiles
    {
        get => core.matchmakingProfiles;
        set => core.matchmakingProfiles = value;
    }

    #endregion

    public UIMatchmakingBox(UIMenu openOnClose, float xpos, float ypos, float wide = -1, float high = -1)
        : base("", xpos, ypos, wide, high)
    {
        _openOnClose = openOnClose;
        Graphics.fade = 1;
        _frame = new Sprite("online/matchmaking");
        _frame.CenterOrigin();
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

    #region Public Methods

    public override void Open()
    {
        searchTryIndex = 0;
        _permenantBlacklist.Clear();
        _newStatusList.Add("|DGYELLOW|Connecting to servers on the Moon.");
        HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@ABORT");
        if (playMusic)
            Music.Play("jazzroom");
        _triesSinceSearch = 0;
        triedHostingAlready = false;
        _tryConnectLobby = null;
        _tryHostingLobby = null;
        ChangeState(MatchmakingState.ConnectToMoon);
        _quit = false;
        _tries = 0;
        _totalLobbiesFound = -1;
        _failedAttempts.Clear();
        _currentLevel = Level.current;
        _searchingIsOver = false;
        _teamProfileLinks.Clear();
        foreach (Profile p in Profiles.active)
            _teamProfileLinks[p] = p.team;
        base.Open();
    }

    public override void Close()
    {
        ChangeState(MatchmakingState.None);
        if (_quit)
            foreach (KeyValuePair<Profile, Team> pair in _teamProfileLinks)
                pair.Key.team = pair.Value;
        _quit = false;
        _newStatusList.Clear();
        _statusList.Clear();
        base.Close();
    }

    public override void Update()
    {
        if (!_searchingIsOver)
        {
            _scroll += 0.1f;
            if (_scroll > 9)
                _scroll = 0;
            _dots += 0.01f;
            if (_dots > 1)
                _dots = 0;
        }
        if (open)
        {
            foreach (BlacklistServer failedAttempt in _failedAttempts)
                failedAttempt.cooldown = Lerp.Float(failedAttempt.cooldown, 0, Maths.IncFrameTimer());
            if (_searchingIsOver)
            {
                _signalCrossLocal.SetAnimation("idle");
                _core.pulseLocal = false;
            }
            else if (_signalCrossLocal.currentAnimation == "idle")
            {
                if (_core.pulseLocal)
                {
                    _signalCrossLocal.SetAnimation("flicker");
                    _core.pulseLocal = false;
                }
            }
            else if (_signalCrossLocal.finished)
                _signalCrossLocal.SetAnimation("idle");
            if (_signalCrossNetwork.currentAnimation == "idle")
            {
                if (_core.pulseNetwork)
                {
                    _signalCrossNetwork.SetAnimation("flicker");
                    _core.pulseNetwork = false;
                }
            }
            else if (_signalCrossNetwork.finished)
                _signalCrossNetwork.SetAnimation("idle");
            if (Network.connections.Count > 0 && _core._state != MatchmakingState.Connecting)
            {
                ChangeState(MatchmakingState.Connecting);
                DevConsole.Log("|PURPLE|LOBBY    |DGGREEN|Network appears to be connecting...", Color.White);
            }
            if (DuckNetwork.status == DuckNetStatus.Connected)
            {
                if (_tryHostingLobby != null)
                {
                    (Level.current as TeamSelect2).CloseAllDialogs();
                    Level.current = new TeamSelect2();
                    DevConsole.Log("|PURPLE|LOBBY    |DGGREEN|Finished! (HOST).", Color.White);
                }
                else if (Level.current == _currentLevel)
                {
                    (Level.current as TeamSelect2).CloseAllDialogs();
                    Level.current = new ConnectingScreen();
                    DevConsole.Log("|PURPLE|LOBBY    |DGGREEN|Finished! (CLIENT).", Color.White);
                }
                return;
            }
            if (_core._state == MatchmakingState.Waiting)
            {
                _stateWait -= Maths.IncFrameTimer();
                if (_stateWait <= 0)
                {
                    _stateWait = 0;
                    OnStateChange(_pendingState);
                }
            }
            else if (_core._state == MatchmakingState.ConnectToMoon)
            {
                Network.activeNetwork.core.AddLobbyStringFilter("started", "true", LobbyFilterComparison.Equal);
                Network.activeNetwork.core.SearchForLobby();
                Network.activeNetwork.core.RequestGlobalStats();
                _core.pulseLocal = true;
                ChangeState(MatchmakingState.ConnectingToMoon);
            }
            else if (_core._state == MatchmakingState.ConnectingToMoon)
            {
                if (Network.activeNetwork.core.IsLobbySearchComplete())
                {
                    if (searchTryIndex == 0)
                    {
                        _totalInGameLobbies = Network.activeNetwork.core.NumLobbiesFound();
                        if (_totalInGameLobbies < 0)
                            _totalInGameLobbies = 0;
                        searchTryIndex++;
                        Network.activeNetwork.core.AddLobbyStringFilter("started", "false", LobbyFilterComparison.Equal);
                        Network.activeNetwork.core.SearchForLobby();
                    }
                    else
                    {
                        _core.pulseNetwork = true;
                        _totalLobbiesFound = Network.activeNetwork.core.NumLobbiesFound();
                        _newStatusList.Add("|DGGREEN|Connected to Moon!");
                        _newStatusList.Add("");
                        _newStatusList.Add("|DGYELLOW|Searching for companions.");
                        ChangeState(MatchmakingState.SearchForLobbies);
                    }
                }
            }
            else if (_core._state == MatchmakingState.CheckingTotalGames)
            {
                if (Network.activeNetwork.core.IsLobbySearchComplete())
                {
                    _totalInGameLobbies = Network.activeNetwork.core.NumLobbiesFound();
                    if (_totalInGameLobbies < 0)
                        _totalInGameLobbies = 0;
                    ChangeState(MatchmakingState.SearchForLobbies);
                    _triesSinceSearch = 0;
                }
            }
            else if (_core._state == MatchmakingState.SearchForLobbies)
            {
                if (_triesSinceSearch == 3)
                {
                    Network.activeNetwork.core.AddLobbyStringFilter("started", "true", LobbyFilterComparison.Equal);
                    Network.activeNetwork.core.SearchForLobby();
                    ChangeState(MatchmakingState.CheckingTotalGames);
                    return;
                }
                if (_tries > 0 && _tryHostingLobby == null)
                {
                    DuckNetwork.Host(TeamSelect2.GetSettingInt("maxplayers"), NetworkLobbyType.Public);
                    _tryHostingLobby = Network.activeNetwork.core.lobby;
                    if (!triedHostingAlready)
                        _newStatusList.Add("|DGYELLOW|Searching even harder.");
                    else
                        _newStatusList.Add("|DGYELLOW|Searching.");
                    triedHostingAlready = true;
                    DevConsole.Log("|PURPLE|LOBBY    |DGYELLOW|Opened lobby while searching.", Color.White);
                }
                Network.activeNetwork.core.ApplyTS2LobbyFilters();
                Network.activeNetwork.core.AddLobbyStringFilter("started", "false", LobbyFilterComparison.Equal);
                Network.activeNetwork.core.AddLobbyStringFilter("beta", "2.0", LobbyFilterComparison.Equal);
                Network.activeNetwork.core.AddLobbyStringFilter("dev", DG.devBuild ? "true" : "false", LobbyFilterComparison.Equal);
                Network.activeNetwork.core.AddLobbyStringFilter("modhash", ModLoader.modHash, LobbyFilterComparison.Equal);
                Network.activeNetwork.core.AddLobbyStringFilter("password", "false", LobbyFilterComparison.Equal);
                Network.activeNetwork.core.AddLobbyStringFilter("dedicated", "false", LobbyFilterComparison.Equal);
                _core.pulseLocal = true;
                ChangeState(MatchmakingState.Searching);
                _triesSinceSearch++;
                _tries++;
            }
            else if (_core._state == MatchmakingState.Searching)
            {
                if (Network.activeNetwork.core.IsLobbySearchComplete())
                {
                    _totalLobbiesFound = Network.activeNetwork.core.NumLobbiesFound();
                    List<Lobby> tryLater = [];
                    DevConsole.Log($"|PURPLE|LOBBY    |LIME|found {Math.Max(_totalLobbiesFound, 0)} lobbies.", Color.White);
                    for (int doLater = 0; doLater < 2; doLater++)
                    {
                        int lobbies = 0;
                        lobbies = ((doLater != 0) ? tryLater.Count : Network.activeNetwork.core.NumLobbiesFound());
                        for (int i = 0; i < lobbies; i++)
                        {
                            Lobby lobby = null;
                            lobby = doLater != 0 ? tryLater[i] : Network.activeNetwork.core.GetSearchLobbyAtIndex(i);
                            if (_tryHostingLobby != null && lobby.id == _tryHostingLobby.id)
                                continue;

                            if (i == Network.activeNetwork.core.NumLobbiesFound() - 1)
                                _failedAttempts.RemoveAll(x => x.cooldown <= 0);
                            if (IsBlacklisted(lobby.id))
                            {
                                DevConsole.Log($"|PURPLE|LOBBY    |DGRED|Skipping {lobby.id} (BLACKLISTED)", Color.White);
                                continue;
                            }
                            if (_core.nonPreferredServers.Contains(lobby.id) && doLater == 0)
                            {
                                tryLater.Add(lobby);
                                DevConsole.Log($"|PURPLE|LOBBY    |DGRED|Skipping {lobby.id} (NOT PREFERRED)", Color.White);
                                continue;
                            }
                            switch (DuckNetwork.CheckVersion(lobby.GetLobbyData("version")))
                            {
                                case NMVersionMismatch.Type.Older:
                                    _newStatusList.Add("|PURPLE|LOBBY |DGRED|Skipped(TOO OLD)");
                                    continue;
                                case NMVersionMismatch.Type.Newer:
                                    _newStatusList.Add("|PURPLE|LOBBY |DGRED|Skipped(TOO NEW)");
                                    continue;
                                default:
                                    _newStatusList.Add("|PURPLE|LOBBY |DGRED|Skipped(ERROR)");
                                    continue;
                                case NMVersionMismatch.Type.Match:
                                    break;
                            }
                            if (_tryHostingLobby != null)
                            {
                                int lobbyRandom = -1;
                                try
                                {
                                    string dat = lobby.GetLobbyData("randomID");
                                    if (dat != "")
                                        lobbyRandom = Convert.ToInt32(dat);
                                }
                                catch
                                {
                                }
                                if (lobbyRandom == -1)
                                {
                                    DevConsole.Log("|PURPLE|LOBBY    |DGYELLOW|Bad lobby seed.", Color.White);
                                    lobbyRandom = Rando.Int(2147483646);
                                }
                                if (lobbyRandom < _tryHostingLobby.randomID)
                                {
                                    DevConsole.Log("|PURPLE|LOBBY    |DGYELLOW|Skipping lobby (Chose to keep hosting).", Color.White);
                                    Network.activeNetwork.core.UpdateRandomID(_tryHostingLobby);
                                    continue;
                                }
                                DevConsole.Log("|PURPLE|LOBBY    |DGYELLOW|Lobby beats own lobby, Attempting join.", Color.White);
                            }
                            _tryConnectLobby = lobby;
                            if (lobby.owner != null)
                                _newStatusList.Add($"|LIME|Trying to join {lobby.owner.name}.");
                            else
                                _newStatusList.Add("|LIME|Trying to join server.");
                            ChangeState(MatchmakingState.Disconnect);
                            break;
                        }
                    }
                    if (_tryConnectLobby == null)
                    {
                        DevConsole.Log("|PURPLE|LOBBY    |DGYELLOW|Found no valid lobbies.", Color.White);
                        ChangeState(MatchmakingState.SearchForLobbies, 3);
                    }
                }
            }
            else if (_core._state == MatchmakingState.Connecting)
            {
                _connectTimeout++;
                if (!Network.connected && _connectTimeout > 120)
                {
                    _tryConnectLobby = null;
                    DevConsole.Log("|PURPLE|LOBBY    |DGRED|Failed to connect!", Color.White);
                    if (this is UIGameConnectionBox)
                    {
                        ChangeState(MatchmakingState.None);
                        _searchingIsOver = true;
                        _newStatusList.Add("|DGRED|Unable to connect to server.");
                        HUD.CloseAllCorners();
                        HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@RETURN");
                    }
                    else
                    {
                        _newStatusList.Add("|DGRED|Failed to connect to server!");
                        _newStatusList.Add("|DGORANGE|Back to searching");
                        ChangeState(MatchmakingState.SearchForLobbies, 3f);
                    }
                }
            }
            UpdateAdditionalMatchmakingLogic();
            if (Input.Pressed("CANCEL"))
            {
                _quit = true;
                ChangeState(MatchmakingState.Disconnect);
            }
        }
        if (_newStatusList.Count > 0)
        {
            _newStatusWait -= 0.1f;
            if (_newStatusWait <= 0)
            {
                _newStatusWait = 1;
                while (_fancyFont.GetWidth(_newStatusList[0]) > 100)
                    _newStatusList[0] = _newStatusList[0][..^1];
                _statusList.Add(_newStatusList[0]);
                if (_statusList.Count > 7)
                    _statusList.RemoveAt(0);
                _newStatusList.RemoveAt(0);
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        _frame.Depth = Depth;
        Graphics.Draw(_frame, X, Y);
        if (!_searchingIsOver)
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
                Graphics.Draw(_matchmakingSignal, signalPos, Y - 21);
            }
        }
        _matchmakingStars[0].Depth = Depth + 2;
        Graphics.Draw(_matchmakingStars[0], X - 9, Y - 18);
        _matchmakingStars[1].Depth = Depth + 2;
        Graphics.Draw(_matchmakingStars[1], X + 31, Y - 22);
        _matchmakingStars[2].Depth = Depth + 2;
        Graphics.Draw(_matchmakingStars[2], X + 12, Y - 20);
        _matchmakingStars[3].Depth = Depth + 2;
        Graphics.Draw(_matchmakingStars[3], X - 23, Y - 21);
        _signalCrossLocal.Depth = Depth + 2;
        Graphics.Draw(_signalCrossLocal, X - 35, Y - 19);
        _signalCrossNetwork.Depth = Depth + 2;
        Graphics.Draw(_signalCrossNetwork, X + 45, Y - 23);
        Vector2 fontPos = new(-_font.GetWidth(_caption) / 2, -42);
        _font.DrawOutline(_caption, Position + fontPos, Color.White, Color.Black, Depth + 2);
        _fancyFont.Scale = new Vector2(0.5f);
        int yOff = 0;
        int listIDX = 0;
        foreach (string status in _statusList)
        {
            string t = status;
            if (listIDX == _statusList.Count - 1 && _newStatusList.Count == 0)
            {
                string elipsis = "";
                if (!_searchingIsOver)
                {
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
            _fancyFont.Draw(t, new Vector2(X - 52, Y - 8 + (yOff * 6)), Color.White, Depth + 2);
            yOff++;
            listIDX++;
        }
        if (_totalLobbiesFound != -1)
        {
            string games = "games";
            if (_totalLobbiesFound == 1)
                games = "game";
            if (_totalInGameLobbies > 0)
                _fancyFont.Draw($"{_totalLobbiesFound} open {games} |DGYELLOW|({_totalInGameLobbies} in progress)", Position + new Vector2(-55, 38), Color.Black, Depth + 2);
            else
                _fancyFont.Draw($"{_totalLobbiesFound} open {games}", Position + new Vector2(-55, 38), Color.Black, Depth + 2);
        }
        else if (_searchingIsOver)
            _fancyFont.Draw("Could not connect.", Position + new Vector2(-55, 38), Color.Black, Depth + 2);
        else
            _fancyFont.Draw("Querying moon...", Position + new Vector2(-55, 38), Color.Black, Depth + 2);
    }

    public void ChangeState(MatchmakingState s, float wait = 0)
    {
        _connectTimeout = 0;
        DevConsole.Log($"|PURPLE|LOBBY    |DGYELLOW|CHANGE STATE {s}", Color.White);
        if (s != MatchmakingState.Waiting)
        {
            if (wait == 0)
            {
                OnStateChange(s);
                return;
            }
            _core._state = MatchmakingState.Waiting;
            _pendingState = s;
            _stateWait = wait;
        }
    }

    public void OnDisconnect(NetworkConnection n)
    {
        if (open && _core._state == MatchmakingState.Connecting && _tryHostingLobby != null && Network.connections.Count == 0)
        {
            ChangeState(MatchmakingState.SearchForLobbies);
            DevConsole.Log("|PURPLE|LOBBY    |DGGREEN|Client disconnect, continuing search.", Color.White);
        }
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
        _openOnClose.Open();
        if (_openOnClose is UIServerBrowser)
            _searchingIsOver = true;
        if (_searchingIsOver)
            MonoMain.pauseMenu = _openOnClose;
        if (!Network.isActive && Level.current is TeamSelect2 && (Level.current as TeamSelect2)._beam != null)
            (Level.current as TeamSelect2)._beam.ClearBeam();
    }

    public void OnConnectionError(DuckNetErrorInfo error)
    {
        if (error != null)
        {
            if (error.error == DuckNetError.YourVersionTooNew || error.error == DuckNetError.YourVersionTooOld)
            {
                if (error.error == DuckNetError.YourVersionTooNew)
                    _newStatusList.Add("|DGRED|Their version was older.");
                else
                    _newStatusList.Add("|DGRED|Their version was newer.");
                if (_tryConnectLobby != null)
                {
                    _permenantBlacklist.Add(new BlacklistServer
                    {
                        lobby = _tryConnectLobby.id,
                        cooldown = 15
                    });
                }
            }
            else if (error.error == DuckNetError.FullServer)
                _newStatusList.Add("|DGRED|Failed (FULL SERVER)");
            else if (error.error == DuckNetError.ConnectionTimeout)
                _newStatusList.Add("|DGRED|Failed (TIMEOUT)");
            else if (error.error == DuckNetError.GameInProgress)
                _newStatusList.Add("|DGRED|Failed (IN PROGRESS)");
            else if (error.error == DuckNetError.GameNotFoundOrClosed)
                _newStatusList.Add("|DGRED|Failed (NO LONGER AVAILABLE)");
            else if (error.error == DuckNetError.ClientDisconnected)
                _newStatusList.Add("|DGYELLOW|Disconnected");
            else if (error.error == DuckNetError.InvalidPassword)
                _newStatusList.Add("|DGRED|Password was incorrect!");
            else
            {
                _newStatusList.Add("|DGRED|Unknown connection error.");
                if (_tryConnectLobby != null)
                {
                    _permenantBlacklist.Add(new BlacklistServer
                    {
                        lobby = _tryConnectLobby.id,
                        cooldown = 15
                    });
                }
            }
        }
        else
        {
            _newStatusList.Add("|DGRED|Connection timeout.");
            if (_tryConnectLobby != null)
            {
                _permenantBlacklist.Add(new BlacklistServer
                {
                    lobby = _tryConnectLobby.id,
                    cooldown = 15
                });
            }
        }
        if (_tryConnectLobby != null)
        {
            _failedAttempts.Add(new BlacklistServer
            {
                lobby = _tryConnectLobby.id,
                cooldown = 15
            });
        }
        DevConsole.Log("|PURPLE|LOBBY    |DGGREEN|Connection failure, continuing search.", Color.White);
        _tryConnectLobby = null;
        if (_continueSearchOnFail)
        {
            ChangeState(MatchmakingState.SearchForLobbies);
            return;
        }
        _searchingIsOver = true;
        _newStatusList.Add("|DGRED|Unable to connect to server.");
        HUD.CloseAllCorners();
        HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@RETURN");
    }

    public void OnSessionEnded(DuckNetErrorInfo error)
    {
        if (!open)
            return;

        if (_core._state == MatchmakingState.Disconnect)
        {
            if (_tryHostingLobby != null)
                _tries = 0;
            _tryHostingLobby = null;
            if (_quit)
                FinishAndClose();
            else if (_tryConnectLobby != null)
            {
                DuckNetwork.Join(_tryConnectLobby.id.ToString());
                ChangeState(MatchmakingState.Connecting);
            }
            else
                ChangeState(MatchmakingState.SearchForLobbies);
        }
        else
            OnConnectionError(error);
    }

    public bool IsBlacklisted(ulong lobby)
    {
        if (_permenantBlacklist.FirstOrDefault(x => x.lobby == lobby) != null || _failedAttempts.FirstOrDefault(x => x.lobby == lobby) != null)
            return true;
        if (core.blacklist.Contains(lobby))
            return true;
        return false;
    }

    #endregion

    protected virtual void UpdateAdditionalMatchmakingLogic() { }

    void OnStateChange(MatchmakingState s)
    {
        _core._state = s;
        _stateWait = 0;
        if (_core._state == MatchmakingState.Disconnect)
        {
            if (!Network.isActive)
                OnSessionEnded(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Matchmaking disconnect."));
            else
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Matchmaking disconnect."));
        }
        else if (_core._state == MatchmakingState.Searching)
        {
            Network.activeNetwork.core.SearchForLobby();
            DevConsole.Log("|PURPLE|LOBBY    |DGYELLOW|Searching for lobbies.", Color.White);
        }
        else if (_core._state == MatchmakingState.Connecting)
            DevConsole.Log("|PURPLE|LOBBY    |DGYELLOW|Attempting connection to server.", Color.White);
    }
}
