using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

internal class UIMatchmakerSteam : UIMatchmakerMark2
{
    public int _searchAttempts;

    public List<Lobby> lobbies = new List<Lobby>();

    private int _takeIndex;

    protected bool _desparate;

    public UIMatchmakerSteam(UIServerBrowser.LobbyData joinLobby, UIMenu openOnClose)
        : base(joinLobby, openOnClose)
    {
    }

    protected override void Platform_Open()
    {
        _state = State.GetNumberOfLobbies;
        _searchAttempts = 0;
        _resetNetwork = false;
        _desparate = false;
    }

    public List<Lobby> GetOrderedLobbyList()
    {
        int myRandom = 0;
        try
        {
            if (_hostedLobby != null)
            {
                myRandom = Convert.ToInt32(_hostedLobby.GetLobbyData("randomID"));
            }
        }
        catch
        {
        }
        List<Lobby> sorted = new List<Lobby>();
        int numLobbies = Network.activeNetwork.core.NumLobbiesFound();
        for (int i = 0; i < numLobbies; i++)
        {
            Lobby lobby = Network.activeNetwork.core.GetSearchLobbyAtIndex(i);
            foreach (User user in lobby.users)
            {
                _ = user;
            }
            if (lobby.owner == Steam.user || !lobby.joinable || blacklist.Contains(lobby.id) || attempted.Contains(lobby.id) || (UIMatchmakingBox.core != null && UIMatchmakingBox.core.blacklist.Contains(lobby.id)))
            {
                continue;
            }
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
                {
                    continue;
                }
            }
            sorted.Add(lobby);
        }
        return sorted.OrderBy(delegate (Lobby x)
        {
            int num = 100;
            if (x.GetLobbyData("version") != DG.version)
            {
                num += 100;
            }
            if (UIMatchmakingBox.core != null && UIMatchmakingBox.core.nonPreferredServers.Contains(x.id))
            {
                num += 50;
            }
            return num;
        }).ToList();
    }

    private Lobby TakeLobby()
    {
        if (HasLobby())
        {
            Lobby result = lobbies[_takeIndex];
            _takeIndex++;
            return result;
        }
        return null;
    }

    private Lobby PeekLobby()
    {
        if (HasLobby())
        {
            return lobbies[_takeIndex];
        }
        return null;
    }

    private bool HasLobby()
    {
        if (lobbies.Count() > 0)
        {
            return _takeIndex < lobbies.Count;
        }
        return false;
    }

    private void GetDesparate()
    {
        if (!_desparate)
        {
            _desparate = true;
            messages.Add("|DGYELLOW|Searching far and wide...");
        }
    }

    protected override void Platform_ResetLogic()
    {
        if (_hostedLobby != null)
        {
            _hostedLobby.joinable = false;
            Steam.LeaveLobby(_hostedLobby);
        }
    }

    public override void Platform_Update()
    {
        if (_state == State.JoinLobby && _timeInState > 480)
        {
            Reset();
        }
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
            if (l.owner != null)
            {
                messages.Add("|LIME|Trying to join " + l.owner.name + "'s lobby...");
            }
            else
            {
                messages.Add("|LIME|Trying to join lobby " + _takeIndex + "/" + lobbies.Count + "...");
            }
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
            UIMatchmakerMark2.pulseLocal = true;
            ChangeState(State.WaitForQuery);
        }
        else if (_state == State.SearchForLobbies)
        {
            _searchAttempts++;
            if (UIMatchmakerMark2.searchMode == 2 && _searchAttempts > 1)
            {
                GetDesparate();
            }
            else if (UIMatchmakerMark2.searchMode != 1 && _searchAttempts > 5)
            {
                GetDesparate();
            }
            NCSteam.globalSearch = _desparate;
            Network.activeNetwork.core.ApplyTS2LobbyFilters();
            Network.activeNetwork.core.AddLobbyStringFilter("started", "false", LobbyFilterComparison.Equal);
            Network.activeNetwork.core.AddLobbyStringFilter("modhash", ModLoader.modHash, LobbyFilterComparison.Equal);
            Network.activeNetwork.core.AddLobbyStringFilter("password", "false", LobbyFilterComparison.Equal);
            Network.activeNetwork.core.SearchForLobby();
            UIMatchmakerMark2.pulseLocal = true;
            ChangeState(State.WaitForQuery);
        }
        else if (_state == State.TryJoiningLobbies)
        {
            if (_directConnectLobby != null)
            {
                _processing = _directConnectLobby.lobby;
                if (_processing == null)
                {
                    messages.Clear();
                    messages.Add("|LIME|Trying to join lobby...");
                    DuckNetwork.Join("", _directConnectLobby.lanAddress, _passwordAttempt);
                    ChangeState(State.JoinLobby);
                    return;
                }
            }
            else
            {
                _processing = PeekLobby();
            }
            if (_processing == null)
            {
                if (_directConnectLobby != null)
                {
                    ChangeState(State.Failed);
                }
                else if (UIMatchmakerMark2.searchMode == 2 && _searchAttempts < 2)
                {
                    ChangeState(State.SearchForLobbies);
                }
                else if (HostLobby())
                {
                    _wait = 240;
                    ChangeState(State.SearchForLobbies);
                }
                else
                {
                    _wait = 60;
                }
                return;
            }
            attempted.Add(_processing.id);
            NMVersionMismatch.Type mismatch = DuckNetwork.CheckVersion(_processing.GetLobbyData("version"));
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
                {
                    ChangeState(State.Failed);
                }
            }
            else if (_processing.GetLobbyData("datahash").Trim() != Network.gameDataHash.ToString())
            {
                messages.Add("|PURPLE|LOBBY |DGRED|Skipped Lobby (Incompatible)...");
                TakeLobby();
                if (_directConnectLobby != null)
                {
                    ChangeState(State.Failed);
                }
            }
            else
            {
                if (!Reset())
                {
                    return;
                }
                TakeLobby();
                if (_directConnectLobby != null)
                {
                    messages.Clear();
                    if (_directConnectLobby.name != "" && _directConnectLobby.name != null)
                    {
                        messages.Add("|LIME|Trying to join " + _directConnectLobby.name + "...");
                    }
                    else
                    {
                        messages.Add("|LIME|Trying to join lobby...");
                    }
                }
                DuckNetwork.Join(_processing.id.ToString(), "localhost", _passwordAttempt);
                ChangeState(State.JoinLobby);
            }
        }
        else if (_state == State.JoinLobby)
        {
            if (!Network.isActive)
            {
                ChangeState(State.SearchForLobbies);
            }
        }
        else if (_state == State.Aborting)
        {
            if (!Network.isActive)
            {
                FinishAndClose();
            }
        }
        else if (_state == State.WaitForQuery && Network.activeNetwork.core.IsLobbySearchComplete())
        {
            if (_previousState == State.GetNumberOfLobbies)
            {
                UIMatchmakerMark2.pulseNetwork = true;
                _totalLobbies = Network.activeNetwork.core.NumLobbiesFound();
                messages.Add("|DGGREEN|Connected to Moon!");
                messages.Add("");
                messages.Add("|DGYELLOW|Searching for companions...");
                ChangeState(State.SearchForLobbies);
            }
            else if (_previousState == State.SearchForLobbies)
            {
                _joinableLobbies = Network.activeNetwork.core.NumLobbiesFound();
                new List<Lobby>();
                DevConsole.Log("|PURPLE|LOBBY    |LIME|found " + Math.Max(_joinableLobbies, 0) + " lobbies.", Color.White);
                lobbies = GetOrderedLobbyList();
                DevConsole.Log("|PURPLE|LOBBY    |LIME|found " + lobbies.Count + " compatible lobbies.", Color.White);
                _takeIndex = 0;
                messages.Add("Found " + lobbies.Count + " potential lobbies...");
                ChangeState(State.TryJoiningLobbies);
            }
        }
    }
}
