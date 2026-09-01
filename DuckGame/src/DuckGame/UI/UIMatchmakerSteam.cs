using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;


#if FACEPUNCH
using Steamworks;
using Steamworks.Data;
#else
using Steam;
#endif

namespace DuckGame;

internal class UIMatchmakerSteam(UIServerBrowser.LobbyData joinLobby, UIMenu openOnClose) 
    : UIMatchmakerMark2(joinLobby, openOnClose)
{
    #region Public Fields

    public int _searchAttempts;

    public List<Lobby> lobbies = [];

    #endregion

    protected bool _desparate;

    int _takeIndex;

    #region Public Methods

    public override void Platform_Update()
    {
        if (_state == State.JoinLobby && _timeInState > 480)
            Reset();
        if (Input.Pressed("GRAB"))
        {
            _desparate = false;
            GetDesparate();
        }
        if (Network.connections.Count > 0)
        {
            if (_state != State.JoinLobby)
            {
                messages.Add("|PURPLE|LOBBY |DGGREEN|Connecting...");
                DevConsole.Log("|PURPLE|LOBBY    |DGGREEN|Network appears to be connecting...", Color.White);
            }
            ChangeState(State.JoinLobby);
            _wait = 0;
        }
    }

    public override void Hook_OnLobbyProcessed(object pLobby)
    {
        if (pLobby is Lobby l)
        {
            messages.Clear();
#if FACEPUNCH
            if (l.Owner.Id != 0)
#else
            if (l.Owner != null)
#endif
                messages.Add($"|LIME|Trying to join {l.Owner.Name}'s lobby...");
            else
                messages.Add($"|LIME|Trying to join lobby {_takeIndex}/{lobbies.Count}...");
        }
        base.Hook_OnLobbyProcessed(pLobby);
    }

    public override void Platform_MatchmakerLogic()
    {
        if (_state == State.GetNumberOfLobbies)
        {
            NCSteam.globalSearch = true;
            Network.activeNetwork.core.SearchForLobby();
            Network.activeNetwork.core.RequestGlobalStats();
            pulseLocal = true;
            ChangeState(State.WaitForQuery);
        }
        else if (_state == State.SearchForLobbies)
        {
            _searchAttempts++;
            if (searchMode == 2 && _searchAttempts > 1)
                GetDesparate();
            else if (searchMode != 1 && _searchAttempts > 5)
                GetDesparate();
            NCSteam.globalSearch = _desparate;
            Network.activeNetwork.core.ApplyTS2LobbyFilters();
            Network.activeNetwork.core.AddLobbyStringFilter("started", "false", LobbyFilterComparison.Equal);
            Network.activeNetwork.core.AddLobbyStringFilter("modhash", ModLoader.modHash, LobbyFilterComparison.Equal);
            Network.activeNetwork.core.AddLobbyStringFilter("password", "false", LobbyFilterComparison.Equal);
            Network.activeNetwork.core.SearchForLobby();
            pulseLocal = true;
            ChangeState(State.WaitForQuery);
        }
        else if (_state == State.TryJoiningLobbies)
        {
            if (_directConnectLobby != null)
            {
                _processing = _directConnectLobby.lobby;
#if FACEPUNCH
                if (_processing.Id == 0)
#else
                if (_processing == null)
#endif
                {
                    messages.Clear();
                    messages.Add("|LIME|Trying to join lobby...");
                    DuckNetwork.Join("", _directConnectLobby.lanAddress, _passwordAttempt);
                    ChangeState(State.JoinLobby);
                    return;
                }
            }
            else
                _processing = PeekLobby();
#if FACEPUNCH
            if (_processing.Id == 0)
#else
            if (_processing == null)
#endif
            {
                if (_directConnectLobby != null)
                    ChangeState(State.Failed);
                else if (searchMode == 2 && _searchAttempts < 2)
                    ChangeState(State.SearchForLobbies);
                else if (HostLobby())
                {
                    _wait = 240;
                    ChangeState(State.SearchForLobbies);
                }
                else
                    _wait = 60;
                return;
            }
            attempted.Add(_processing.Id);
#if FACEPUNCH
            var mismatch = DuckNetwork.CheckVersion(_processing.GetData("version"));
#else
            NMVersionMismatch.Type mismatch = DuckNetwork.CheckVersion(_processing.GetLobbyData("version"));
#endif
            if (mismatch != NMVersionMismatch.Type.Match)
            {
                switch (mismatch)
                {
                    case NMVersionMismatch.Type.Older:
                        messages.Add("|PURPLE|LOBBY |DGRED|Skipped Lobby (Their version's too old)...");
                        break;
                    case NMVersionMismatch.Type.Newer:
                        messages.Add("|PURPLE|LOBBY |DGRED|Skipped Lobby (Their version's too new)...");
                        break;
                    default:
                        messages.Add("|PURPLE|LOBBY |DGRED|Skipped Lobby (ERROR)...");
                        break;
                }
                TakeLobby();
                if (_directConnectLobby != null)
                    ChangeState(State.Failed);
            }
#if FACEPUNCH
            else if (_processing.GetData("datahash").Trim() != Network.gameDataHash.ToString())
#else
            else if (_processing.GetLobbyData("datahash").Trim() != Network.gameDataHash.ToString())
#endif
            {
                messages.Add("|PURPLE|LOBBY |DGRED|Skipped Lobby (Incompatible)...");
                TakeLobby();
                if (_directConnectLobby != null)
                    ChangeState(State.Failed);
            }
            else
            {
                if (!Reset())
                    return;

                TakeLobby();
                if (_directConnectLobby != null)
                {
                    messages.Clear();
                    if (_directConnectLobby.name != "" && _directConnectLobby.name != null)
                        messages.Add($"|LIME|Trying to join {_directConnectLobby.name}...");
                    else
                        messages.Add("|LIME|Trying to join lobby...");
                }
                DuckNetwork.Join(_processing.Id.ToString(), "localhost", _passwordAttempt);
                ChangeState(State.JoinLobby);
            }
        }
        else if (_state == State.JoinLobby)
        {
            if (!Network.isActive)
                ChangeState(State.SearchForLobbies);
        }
        else if (_state == State.Aborting)
        {
            if (!Network.isActive)
                FinishAndClose();
        }
        else if (_state == State.WaitForQuery && Network.activeNetwork.core.IsLobbySearchComplete())
        {
            if (_previousState == State.GetNumberOfLobbies)
            {
                pulseNetwork = true;
                _totalLobbies = Network.activeNetwork.core.NumLobbiesFound();
                messages.Add("|DGGREEN|Connected to Moon!");
                messages.Add("");
                messages.Add("|DGYELLOW|Searching for companions...");
                ChangeState(State.SearchForLobbies);
            }
            else if (_previousState == State.SearchForLobbies)
            {
                _joinableLobbies = Network.activeNetwork.core.NumLobbiesFound();
                DevConsole.Log($"|PURPLE|LOBBY    |LIME|found {Math.Max(_joinableLobbies, 0)} lobbies.", Color.White);
                lobbies = GetOrderedLobbyList();
                DevConsole.Log($"|PURPLE|LOBBY    |LIME|found {lobbies.Count} compatible lobbies.", Color.White);
                _takeIndex = 0;
                messages.Add($"Found {lobbies.Count} potential lobbies...");
                ChangeState(State.TryJoiningLobbies);
            }
        }
    }

    public List<Lobby> GetOrderedLobbyList()
    {
        int myRandom = 0;
        try
        {
#if FACEPUNCH
            if (_hostedLobby.Id != 0)
                myRandom = Convert.ToInt32(_hostedLobby.GetData("randomID"));
#else
            if (_hostedLobby != null)
                myRandom = Convert.ToInt32(_hostedLobby.GetLobbyData("randomID"));
#endif
        }
        catch
        {
        }
        List<Lobby> sorted = [];
        int numLobbies = Network.activeNetwork.core.NumLobbiesFound();
#if FACEPUNCH
        for (int i = 0; i < numLobbies; i++)
        {
            Lobby lobby = Network.activeNetwork.core.GetSearchLobbyAtIndex(i);
            foreach (var user in lobby.Members)
                _ = user;
            if (lobby.Owner.Id == SteamClient.SteamId || blacklist.Contains(lobby.Id) || attempted.Contains(lobby.Id) || (UIMatchmakingBox.core != null && UIMatchmakingBox.core.blacklist.Contains(lobby.Id)))
                continue;
            if (myRandom != 0)
            {
                int yourRandom = 0;
                try
                {
                    yourRandom = Convert.ToInt32(lobby.GetData("randomID"));
                }
                catch
                {
                }

                if (myRandom > yourRandom)
                    continue;
            }
            sorted.Add(lobby);
        }
        return [.. sorted.OrderBy(x =>
        {
            int num = 100;
            if (x.GetData("version") != DG.version)
                num += 100;
            if (UIMatchmakingBox.core != null && UIMatchmakingBox.core.nonPreferredServers.Contains(x.Id))
                num += 50;
            return num;
        })];
#else
        for (int i = 0; i < numLobbies; i++)
        {
            Lobby lobby = Network.activeNetwork.core.GetSearchLobbyAtIndex(i);
            foreach (var user in lobby.Users)
                _ = user;
            if (lobby.Owner == DGSteam.User || !lobby.Joinable || blacklist.Contains(lobby.Id) || attempted.Contains(lobby.Id) || (UIMatchmakingBox.core != null && UIMatchmakingBox.core.blacklist.Contains(lobby.Id)))
                continue;
            if (myRandom != 0)
            {
                int yourRandom = 0;
                try
                {
                    yourRandom = Convert.ToInt32(lobby.GetLobbyData("randomID"));
                }
                catch
                {
                }

                if (myRandom > yourRandom)
                    continue;
            }
            sorted.Add(lobby);
        }
        return [.. sorted.OrderBy(x =>
        {
            int num = 100;
            if (x.GetLobbyData("version") != DG.version)
                num += 100;
            if (UIMatchmakingBox.core != null && UIMatchmakingBox.core.nonPreferredServers.Contains(x.Id))
                num += 50;
            return num;
        })];
#endif
    }

    #endregion

    #region Protected Methods

    protected override void Platform_ResetLogic()
    {
#if FACEPUNCH
        if (_hostedLobby.Id != 0)
        {
            _hostedLobby.SetJoinable(false);
            _hostedLobby.Leave();
        }
#else
        if (_hostedLobby != null)
        {
            _hostedLobby.Joinable = false;
            DGSteam.LeaveLobby(_hostedLobby);
        }
#endif
    }

    protected override void Platform_Open()
    {
        _state = State.GetNumberOfLobbies;
        _searchAttempts = 0;
        _resetNetwork = false;
        _desparate = false;
    }

    #endregion

    #region Private Methods

    void GetDesparate()
    {
        if (!_desparate)
        {
            _desparate = true;
            messages.Add("|DGYELLOW|Searching far and wide...");
        }
    }

    bool HasLobby()
    {
        if (lobbies.Count > 0)
            return _takeIndex < lobbies.Count;
        return false;
    }

    Lobby TakeLobby()
    {
        if (HasLobby())
        {
            Lobby result = lobbies[_takeIndex];
            _takeIndex++;
            return result;
        }
        return default;
    }

    Lobby PeekLobby()
    {
        if (HasLobby())
            return lobbies[_takeIndex];
        return default;
    }

    #endregion
}
