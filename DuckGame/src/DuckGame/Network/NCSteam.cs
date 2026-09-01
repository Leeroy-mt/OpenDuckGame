using System;
using System.Threading.Tasks;
using System.Linq;

#if FACEPUNCH
using Steamworks;
using Steamworks.Ugc;
using Steamworks.Data;
#else
using Steam;
#endif

namespace DuckGame;

public class NCSteam : NCNetworkImplementation
{
#if FACEPUNCH
    Action<Friend, MemberStateChange, Friend?> _userChange;
    Action<Friend, byte[]> _chatDelegate;
    Action<SteamId> _connectionRequest;
    Action<SteamId, P2PSessionError> _connectionFailed;
    Action<Lobby, SteamId> _inviteReceived;
    Action<Lobby> _lobbySearchComplete;
    Action<SteamId, Result> _requestStatsComplete;
#else
    Lobby.UserStatusChangeDelegate _userChange;
    Lobby.ChatMessageDelegate _chatDelegate;
    DGSteam.ConnectionRequestedDelegate _connectionRequest;
    DGSteam.ConnectionFailedDelegate _connectionFailed;
    DGSteam.InviteReceivedDelegate _inviteReceived;
    DGSteam.LobbySearchCompleteDelegate _lobbySearchComplete;
    DGSteam.RequestCurrentStatsDelegate _requestStatsComplete;
#endif

    string _serverIdentifier = "";

    int _port;

    ulong _connectionPacketIdentifier = 6094567099491692639uL;

    bool _initializedSettings;

    bool _lobbyCreationComplete;

    public static ulong inviteLobbyID;

    bool gotPingString;

    int pingWaitTimeout;

    public static bool globalSearch;

    public NCSteam(Network c, int networkIndex)
        : base(c, networkIndex)
    {
        HookUpDelegates();
    }

    public override NCError OnSendPacket(byte[] data, int length, object connection)
    {
#if FACEPUNCH
        if (connection is Friend { Id: var id } && id != 0)
        {
            if (length < 1200)
                SteamNetworking.SendP2PPacket(id, data, length, 0, P2PSend.Unreliable);
            else
                SteamNetworking.SendP2PPacket(id, data, length, 0, P2PSend.Reliable);
        }
#else
        if (length < 1200)
            DGSteam.SendPacket(connection as User, data, (uint)length, P2PDataSendType.Unreliable);
        else
            DGSteam.SendPacket(connection as User, data, (uint)length, P2PDataSendType.Reliable);
#endif
        return null;
    }

    public void HookUpDelegates()
    {
        if (_connectionRequest == null)
        {
            _connectionRequest = OnConnectionRequest;
            _connectionFailed = OnConnectionFailed;
            _inviteReceived = OnInviteReceived;
            _lobbySearchComplete = OnLobbySearchComplete;
            _requestStatsComplete = OnRequestStatsComplete;
#if FACEPUNCH
            SteamNetworking.OnP2PSessionRequest += _connectionRequest;
            SteamNetworking.OnP2PConnectionFailed += _connectionFailed;
            SteamFriends.OnGameLobbyJoinRequested += _inviteReceived;
            /* DGSteam.LobbySearchComplete += _lobbySearchComplete // SearchForLobby */
            SteamUserStats.OnUserStatsReceived += _requestStatsComplete;
#else
            DGSteam.ConnectionRequested += _connectionRequest;
            DGSteam.ConnectionFailed += _connectionFailed;
            DGSteam.InviteReceived += _inviteReceived;
            DGSteam.LobbySearchComplete += _lobbySearchComplete;
            DGSteam.RequestCurrentStatsComplete += _requestStatsComplete;
#endif
        }
    }

    public void UnhookDelegates()
    {
        if (_connectionRequest != null)
        {
#if FACEPUNCH
            SteamNetworking.OnP2PSessionRequest -= _connectionRequest;
            SteamNetworking.OnP2PConnectionFailed -= _connectionFailed;
            SteamFriends.OnGameLobbyJoinRequested -= _inviteReceived;
            /* 💀 // SearchForLobby */
#else
            DGSteam.ConnectionRequested -= _connectionRequest;
            DGSteam.ConnectionFailed -= _connectionFailed;
            DGSteam.InviteReceived -= _inviteReceived;
            DGSteam.LobbySearchComplete -= _lobbySearchComplete;
#endif
            _connectionRequest = null;
        }
    }

    public override NCError OnHostServer(string identifier, int port, NetworkLobbyType lobbyType, int maxConnections)
    {
        gotPingString = false;
        pingWaitTimeout = 0;
#if FACEPUNCH
        if (_lobby != null)
        {
            _lobby.Leave();
            UnhookLobbyUserStatusChange(_lobby, OnUserStatusChange);
            UnhookLobbyChatMessage(_lobby, OnChatMessage);
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|Leaving lobby to host new lobby.");
        }
        _lobby = null;
        HookUpDelegates();
        _initializedSettings = false;
        _lobby = FacepunchSteam.CreateLobby(maxConnections)
            .GetAwaiter()
            .GetResult();
        switch (lobbyType)
        {
            case NetworkLobbyType.Private:
                _lobby.SetPrivate();
                break;
            case NetworkLobbyType.FriendsOnly:
                _lobby.SetFriendsOnly();
                break;
            case NetworkLobbyType.Public:
                _lobby.SetPublic();
                break;
            case NetworkLobbyType.Invisible:
                _lobby.SetInvisible();
                break;
            case NetworkLobbyType.LAN:
                break;
        }
        _lobby.SetData("name", identifier);
        if (_lobby.Id == 0)
            return new NCError("|DGORANGE|STEAM |DGRED|Steam is not running.", NCErrorType.Error);
#else
        if (_lobby != null)
        {
            DGSteam.LeaveLobby(_lobby);
            UnhookLobbyUserStatusChange(_lobby, OnUserStatusChange);
            UnhookLobbyChatMessage(_lobby, OnChatMessage);
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|Leaving lobby to host new lobby.");
        }
        _lobby = null;
        HookUpDelegates();
        _initializedSettings = false;
        _lobby = DGSteam.CreateLobby((SteamLobbyType)lobbyType, maxConnections);
        _lobby.Name = identifier;
        if (_lobby == null)
            return new NCError("|DGORANGE|STEAM |DGRED|Steam is not running.", NCErrorType.Error);
#endif
        _userChange = OnUserStatusChange;
        HookUpLobbyUserStatusChange(_lobby, _userChange);
        _chatDelegate = OnChatMessage;
        HookUpLobbyChatMessage(_lobby, _chatDelegate);
        _serverIdentifier = identifier;
        _port = port;
        StartServerThread();
        return new NCError("|DGORANGE|STEAM |DGYELLOW|Attempting to create server lobby...", NCErrorType.Message);
    }

#if FACEPUNCH
    void HookUpLobbyUserStatusChange(SteamLobby l, Action<Friend, MemberStateChange, Friend?> del)
    {
        l.UserStatusChange += del;
    }
#else
    void HookUpLobbyUserStatusChange(Lobby l, Lobby.UserStatusChangeDelegate del)
    {
        l.UserStatusChange += del;
    }
#endif

#if FACEPUNCH
    void HookUpLobbyChatMessage(SteamLobby l, Action<Friend, byte[]> del)
    {
        l.ChatMessage += del;
    }
#else
    void HookUpLobbyChatMessage(Lobby l, Lobby.ChatMessageDelegate del)
    {
        l.ChatMessage += del;
    }
#endif

#if FACEPUNCH
    void UnhookLobbyUserStatusChange(SteamLobby l, Action<Friend, MemberStateChange, Friend?> del)
    {
        try
        {
            l.UserStatusChange -= del;
        }
        catch (Exception)
        {
        }
    }
#else
    void UnhookLobbyUserStatusChange(Lobby l, Lobby.UserStatusChangeDelegate del)
    {
        try
        {
            l.UserStatusChange -= del;
        }
        catch (Exception)
        {
        }
    }
#endif

#if FACEPUNCH
    void UnhookLobbyChatMessage(SteamLobby l, Action<Friend, byte[]> del)
    {
        try
        {
            l.ChatMessage -= del;
        }
        catch (Exception)
        {
        }
    }
#else
    void UnhookLobbyChatMessage(Lobby l, Lobby.ChatMessageDelegate del)
    {
        try
        {
            l.ChatMessage -= del;
        }
        catch (Exception)
        {
        }
    }
#endif

#if FACEPUNCH
    public override NCError OnJoinServer(string identifier, int port, string ip)
    {
        gotPingString = false;
        pingWaitTimeout = 0;
        if (_lobby != null)
        {
            _lobby.Leave();
            UnhookLobbyUserStatusChange(_lobby, OnUserStatusChange);
            UnhookLobbyChatMessage(_lobby, OnChatMessage);
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|Leaving lobby to join new lobby.");
        }
        _lobby = default;
        HookUpDelegates();
        _serverIdentifier = identifier;
        if (identifier == "joinTest")
        {
            FacepunchSteam.JoinLobby(1);
            _lobby = FacepunchSteam.Lobby;
            _serverIdentifier = _lobby.Id.ToString();
        }
        else
        {
            FacepunchSteam.JoinLobby(Convert.ToUInt64(identifier));
            _lobby = FacepunchSteam.Lobby;
        }

        if (_lobby.Id == 0)
            return new NCError("Steam is not running.", NCErrorType.Error);
        _userChange = OnUserStatusChange;
        HookUpLobbyUserStatusChange(_lobby, _userChange);
        _chatDelegate = OnChatMessage;
        HookUpLobbyChatMessage(_lobby, _chatDelegate);
        _port = port;
        StartClientThread();
        return new NCError($"|DGORANGE|STEAM |DGGREEN|Connecting to lobbyID {identifier}.", NCErrorType.Message);
    }

    public void OnUserStatusChange(Friend who, MemberStateChange flags, Friend? responsible)
    {
        DevConsole.Log(DCSection.Connection, $"NCSteam.LobbyStatusChange({GetDrawString(who)}, {flags})");
        if (flags.HasFlag(MemberStateChange.Entered))
        {
            DevConsole.Log(DCSection.Steam, $"|DGGREEN|{who.Name} ({who.Id}) has joined the Steam lobby.");
            if (Network.isServer && DuckNetwork.localConnection.status == ConnectionStatus.Connected)
                AttemptConnection(who);
        }
        else if (flags.HasFlag(MemberStateChange.Left))
            DevConsole.Log(DCSection.Steam, $"|DGRED|{GetDrawString(who)} has left the Steam lobby.");
        else if (flags.HasFlag(MemberStateChange.Disconnected))
            DevConsole.Log(DCSection.Steam, $"|DGRED|{GetDrawString(who)} has disconnected from the Steam lobby.");

        if (flags.HasFlag(MemberStateChange.Kicked))
            DevConsole.Log(DCSection.Steam, $"|DGYELLOW|{GetDrawString(responsible.Value)} kicked {GetDrawString(who)}.");
    }
    public void OnChatMessage(Friend who, byte[] data)
    {
        Steam_LobbyMessage m = Steam_LobbyMessage.Receive(who, data);
        if (m != null)
        {
            if (m.message == "COM_FAIL" && m.context.Id == SteamClient.SteamId)
            {
                DevConsole.Log(DCSection.Connection, $"Communication failure with {who.Name}... Disconnecting!");
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.EveryoneDisconnected, "Could not connect to server."));
            }
            else if (m.message == "IM_OUTTAHERE")
            {
                DevConsole.Log(DCSection.Connection, $"Received lobby exit message from {who.Name}...");
                Network.DisconnectClient(GetConnection(who), new DuckNetErrorInfo(DuckNetError.ClientDisconnected, who.Name + " left the lobby."));
            }
        }
    }
    string GetDrawString(Friend pUser)
    {
        return $"{pUser.Name} ({pUser.Id})";
    }
    public void OnConnectionRequest(SteamId id)
    {
        Friend who = new(id);

        DevConsole.Log(DCSection.Connection, $"NCSteam.OnConnectionRequest({GetDrawString(who)})");
        if ((GetConnection(who) != null || (lobby.Id != 0 && lobby.Members.Contains(who))) && Network.isActive)
        {
            DevConsole.Log(DCSection.Steam, $"|DGYELLOW|{GetDrawString(who)} has requested a connection.");
            SteamNetworking.AcceptP2PSessionWithUser(who.Id);
        }
        else if (!Network.isActive)
            DevConsole.Log(DCSection.Steam, $"|DGRED| Connection request ignored({GetDrawString(who)})(Network.isActive == false)");
        else
            DevConsole.Log(DCSection.Steam, $"|DGRED| Connection request ignored({GetDrawString(who)})(User not found)");
    }
    public void OnConnectionFailed(SteamId id, P2PSessionError pError)
    {
        Friend who = new(id);
        DevConsole.Log(DCSection.Steam, $"|DGRED|Connection with {GetDrawString(who)} has failed ({pError})!");
    }
#else
    public override NCError OnJoinServer(string identifier, int port, string ip)
    {
        gotPingString = false;
        pingWaitTimeout = 0;
        if (_lobby != null)
        {
            DGSteam.LeaveLobby(_lobby);
            UnhookLobbyUserStatusChange(_lobby, OnUserStatusChange);
            UnhookLobbyChatMessage(_lobby, OnChatMessage);
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|Leaving lobby to join new lobby.");
        }
        _lobby = null;
        HookUpDelegates();
        _serverIdentifier = identifier;
        if (identifier == "joinTest")
        {
            _lobby = DGSteam.JoinLobby(1uL);
            _serverIdentifier = _lobby.Id.ToString();
        }
        else
            _lobby = DGSteam.JoinLobby(Convert.ToUInt64(identifier));
        if (_lobby == null)
            return new NCError("Steam is not running.", NCErrorType.Error);
        _userChange = OnUserStatusChange;
        HookUpLobbyUserStatusChange(_lobby, _userChange);
        _chatDelegate = OnChatMessage;
        HookUpLobbyChatMessage(_lobby, _chatDelegate);
        _port = port;
        StartClientThread();
        return new NCError("|DGORANGE|STEAM |DGGREEN|Connecting to lobbyID " + identifier + ".", NCErrorType.Message);
    }
    public void OnUserStatusChange(User who, SteamLobbyUserStatusFlags flags, User responsible)
    {
        DevConsole.Log(DCSection.Connection, "NCSteam.LobbyStatusChange(" + GetDrawString(who) + ", " + flags.ToString() + ")");
        if ((flags & SteamLobbyUserStatusFlags.Entered) != 0)
        {
            DevConsole.Log(DCSection.Steam, "|DGGREEN|" + who.Name + " (" + who.Id + ") has joined the Steam lobby.");
            if (Network.isServer && DuckNetwork.localConnection.status == ConnectionStatus.Connected)
                AttemptConnection(who);//
        }
        else if ((flags & SteamLobbyUserStatusFlags.Left) != 0)
            DevConsole.Log(DCSection.Steam, "|DGRED|" + GetDrawString(who) + " has left the Steam lobby.");
        else if ((flags & SteamLobbyUserStatusFlags.Disconnected) != 0)
            DevConsole.Log(DCSection.Steam, "|DGRED|" + GetDrawString(who) + " has disconnected from the Steam lobby.");
        if ((flags & SteamLobbyUserStatusFlags.Kicked) != 0)
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|" + GetDrawString(responsible) + " kicked " + GetDrawString(who) + ".");
    }
    public void OnChatMessage(User who, byte[] data)
    {
        Steam_LobbyMessage m = Steam_LobbyMessage.Receive(who, data);
        if (m != null)
        {
            if (m.message == "COM_FAIL" && m.context == DGSteam.User)
            {
                DevConsole.Log(DCSection.Connection, "Communication failure with " + who.Name + "... Disconnecting!");
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.EveryoneDisconnected, "Could not connect to server."));
            }
            else if (m.message == "IM_OUTTAHERE")
            {
                DevConsole.Log(DCSection.Connection, "Received lobby exit message from " + who.Name + "...");
                Network.DisconnectClient(GetConnection(who), new DuckNetErrorInfo(DuckNetError.ClientDisconnected, who.Name + " left the lobby."));
            }
        }
    }
    string GetDrawString(User pUser)
    {
        return pUser.Name + " (" + pUser.Id + ")";
    }
    public void OnConnectionRequest(User who)
    {
        DevConsole.Log(DCSection.Connection, "NCSteam.OnConnectionRequest(" + GetDrawString(who) + ")");
        if ((GetConnection(who) != null || (base.lobby != null && base.lobby.Users.Contains(who))) && Network.isActive)
        {
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|" + GetDrawString(who) + " has requested a connection.");
            DGSteam.AcceptConnection(who);
        }
        else if (!Network.isActive)
            DevConsole.Log(DCSection.Steam, "|DGRED| Connection request ignored(" + GetDrawString(who) + ")(Network.isActive == false)");
        else
            DevConsole.Log(DCSection.Steam, "|DGRED| Connection request ignored(" + GetDrawString(who) + ")(User not found)");
    }
    public void OnConnectionFailed(User who, byte pError)
    {
        DevConsole.Log(DCSection.Steam, "|DGRED|Connection with " + GetDrawString(who) + " has failed (" + pError + ")!");
    }
#endif

    public static void PrepareProfilesForJoin()
    {
        foreach (Team item in Teams.all)
            item.ClearProfiles();
        Profile.defaultProfileMappings[0] = Profiles.experienceProfile;
        Teams.Player1.Join(Profiles.experienceProfile);
        TeamSelect2.ControllerLayoutsChanged();
    }

#if FACEPUNCH
    public void OnInviteReceived(Lobby lobby, SteamId id)
    {
        inviteLobbyID = lobby.Id;
        if (Level.current is TitleScreen || Level.current is Editor || Level.current is DuckGameTestArea || (Level.current is GameLevel && (Level.current as GameLevel)._editorTestMode))
            PrepareProfilesForJoin();
        Level.current = new JoinServer(lobby.Id);
    }
#else
    public void OnInviteReceived(User who, Lobby lobby)
    {
        inviteLobbyID = lobby.Id;
        if (Level.current is TitleScreen || Level.current is Editor || Level.current is DuckGameTestArea || (Level.current is GameLevel && (Level.current as GameLevel)._editorTestMode))
            PrepareProfilesForJoin();
        Level.current = new JoinServer(lobby.Id);
    }
#endif

    public void OnLobbySearchComplete(Lobby lobby)
    {
    }
#if FACEPUNCH
    public void OnRequestStatsComplete(SteamId id, Result result)
#else
    public void OnRequestStatsComplete()
#endif
    {
    }

    protected override object GetConnectionObject(string identifier)
    {
#if FACEPUNCH
        return new Friend(Convert.ToUInt64(identifier));
#else
        return User.GetUser(Convert.ToUInt64(identifier));
#endif
    }

    public override string GetConnectionIdentifier(object connection)
    {
#if FACEPUNCH
        if (connection is Friend { Id: var id })
#else
        if (connection is User { Id: var id })
#endif
            return id.ToString();
        return "no info";
    }

    public override string GetConnectionName(object connection)
    {
#if FACEPUNCH
        if (connection is Friend user)
#else
        if (connection is User user)
#endif
            return user.Name;
        return "no info";
    }

    protected override string OnGetLocalName()
    {
#if FACEPUNCH
        if (SteamClient.SteamId != 0)
            return FacepunchSteam.Me.Name;
#else
        if (DGSteam.User != null)
            return DGSteam.User.Name;
#endif
        return "no info";
    }

    protected override NCError OnSpinServerThread()
    {
        if (_lobby == null)
        {
            if (NetworkDebugger.enabled)
                return null;

            return new NCError("|DGORANGE|STEAM |DGRED|Lobby was closed.", NCErrorType.CriticalError);
        }

#if FACEPUNCH
        if (_lobby.IsProcessing)
#else
        if (_lobby.Processing)
#endif
            return null;

        if (_lobby.Id == 0L)
            return new NCError("|DGORANGE|STEAM |DGRED|Failed to create lobby.", NCErrorType.CriticalError);

        return RunSharedLogic();
    }

    protected override NCError OnSpinClientThread()
    {
#if !FACEPUNCH
        if (_lobby == null)
            return new NCError("|DGORANGE|STEAM |DGYELLOW|Lobby was closed.", NCErrorType.CriticalError);

        if (_lobby.Processing)
            return null;
#endif

        if (_lobby.Id == 0L)
            return new NCError("|DGORANGE|STEAM |DGRED|Failed to join lobby.", NCErrorType.CriticalError);

        return RunSharedLogic();
    }
#if FACEPUNCH
    byte[] packetData;
#endif
    protected NCError RunSharedLogic()
    {
        while (true)
        {
#if FACEPUNCH
            if (SteamNetworking.IsP2PPacketAvailable(out var size))
            {
                packetData ??= new byte[2048];
                var data = size > 2048
                    ? new byte[size]
                    : packetData;

                SteamId user = 0;
                if (SteamNetworking.ReadP2PPacket(data, ref size, ref user))
                {
                    if (data == packetData)
                    {
                        data = new byte[size];
                        Array.Copy(packetData, data, size);
                    }
                    OnPacket(data, new Friend(user));
                }
                else break;
            }
            else break;
#else
            SteamPacket packet = null;
            packet = DGSteam.ReadPacket();
            if (packet == null)
                break;
            OnPacket(packet.data, packet.connection);
#endif
        }
        return null;
    }

    protected override void Disconnect(NetworkConnection c)
    {
#if FACEPUNCH
        if (c != null && c.data is Friend user)
        {
            DevConsole.Log(DCSection.Steam, $"|DGRED|Closing connection with {GetDrawString(user)}.");
            SteamNetworking.CloseP2PSessionWithUser(user.Id);
        }
#else
        if (c != null && c.data is User user)
        {
            DevConsole.Log(DCSection.Steam, "|DGRED|Closing connection with " + GetDrawString(user) + ".");
            DGSteam.CloseConnection(user);
        }
#endif
        base.Disconnect(c);
    }

    protected override void KillConnection()
    {
#if FACEPUNCH
        if (_lobby != null)
        {
            if (_lobby.Owner.Id == SteamClient.SteamId && DuckNetwork.potentialHostObject is Friend newOwner && _lobby.Members.Contains(newOwner))
                _lobby.Owner = newOwner;

            Steam_LobbyMessage.Send("IM_OUTTAHERE", default);
            _lobby.Leave();
            UnhookLobbyUserStatusChange(_lobby, OnUserStatusChange);
            UnhookLobbyChatMessage(_lobby, OnChatMessage);
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|Leaving lobby to host new lobby.");
        }
        _lobby = default;
        _lobbyCreationComplete = false;
        _initializedSettings = false;
        base.KillConnection();
#else
        if (_lobby != null)
        {
            if (_lobby.Owner == DGSteam.User && DuckNetwork.potentialHostObject is User newOwner && _lobby.Users.Contains(newOwner))
                _lobby.Owner = newOwner;

            Steam_LobbyMessage.Send("IM_OUTTAHERE", null);
            DGSteam.LeaveLobby(_lobby);
            UnhookLobbyUserStatusChange(_lobby, OnUserStatusChange);
            UnhookLobbyChatMessage(_lobby, OnChatMessage);
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|Leaving lobby to host new lobby.");
        }
        _lobby = default;
        _lobbyCreationComplete = false;
        _initializedSettings = false;
        base.KillConnection();
#endif
    }

    public override void ApplyLobbyData()
    {
#if FACEPUNCH
        foreach (MatchSetting s in TeamSelect2.matchSettings)
        {
            if (s.value is int i)
                _lobby.SetData(s.id, i.ToString());
            else if (s.value is bool b)
                _lobby.SetData(s.id, (b ? 1 : 0).ToString());
        }

        foreach (MatchSetting s2 in TeamSelect2.onlineSettings)
        {
            if (s2.id == "password")
                _lobby.SetData("password", ((string)s2.value != "") ? "true" : "false");

            if (s2.id == "modifiers")
            {
                if (s2.filtered)
                    _lobby.SetData(s2.id, ((bool)s2.value) ? "true" : "false");
            }
            else if (s2.id == "dedicated")
                _lobby.SetData(s2.id, ((bool)s2.value) ? "true" : "false");
            else if (s2.value is int i)
                _lobby.SetData(s2.id, i.ToString());
            else if (s2.value is bool b)
                _lobby.SetData(s2.id, (b ? 1 : 0).ToString());
        }

        foreach (UnlockData dat in Unlocks.allUnlocks)
            _lobby.SetData(dat.id, (dat.enabled ? 1 : 0).ToString());

        _lobby.SetData("customLevels", Editor.customLevelCount.ToString());
#else
        foreach (MatchSetting s in TeamSelect2.matchSettings)
        {
            if (s.value is int i)
                _lobby.SetLobbyData(s.id, i.ToString());
            else if (s.value is bool b)
                _lobby.SetLobbyData(s.id, (b ? 1 : 0).ToString());
        }

        foreach (MatchSetting s2 in TeamSelect2.onlineSettings)
        {
            if (s2.id == "password")
                _lobby.SetLobbyData("password", ((string)s2.value != "") ? "true" : "false");

            if (s2.id == "modifiers")
            {
                if (s2.filtered)
                    _lobby.SetLobbyData(s2.id, ((bool)s2.value) ? "true" : "false");
            }
            else if (s2.id == "dedicated")
                _lobby.SetLobbyData(s2.id, ((bool)s2.value) ? "true" : "false");
            else if (s2.value is int i)
                _lobby.SetLobbyData(s2.id, i.ToString());
            else if (s2.value is bool b)
                _lobby.SetLobbyData(s2.id, (b ? 1 : 0).ToString());
        }

        foreach (UnlockData dat in Unlocks.allUnlocks)
            _lobby.SetLobbyData(dat.id, (dat.enabled ? 1 : 0).ToString());

        _lobby.SetLobbyData("customLevels", Editor.customLevelCount.ToString());
#endif
    }

    private void TryGettingPingString()
    {
#if FACEPUNCH
        if (_lobby != null && !_lobby.IsProcessing && _lobby.Id != 0 && pingWaitTimeout <= 0 && !gotPingString)
        {
            string pingString = SteamNetworkingUtils.LocalPingLocation?.ToString() ?? "";
            _lobby.SetData("pingstring", pingString);
            if (pingString != null && pingString != "")
                gotPingString = true;
            pingWaitTimeout = 60;
        }
#else
        if (_lobby != null && !_lobby.Processing && _lobby.Id != 0L && pingWaitTimeout <= 0 && !gotPingString)
        {
            string pingString = DGSteam.GetLocalPingString();
            _lobby.SetLobbyData("pingstring", pingString);
            if (pingString != null && pingString != "")
                gotPingString = true;
            pingWaitTimeout = 60;
        }
#endif
        pingWaitTimeout--;
    }

    public override void Update()
    {
#if FACEPUNCH
        if (_lobby != null && !_lobby.IsProcessing && _lobby.Id != 0)
        {
            if (!_lobbyCreationComplete)
            {
                _lobbyCreationComplete = true;
                if (Network.isServer)
                {
                    if (_lobby.EnterResult != RoomEnter.Success)
                    {
                        DevConsole.Log(DCSection.Steam, "|DGGREEN|Lobby creation failed!");
                        Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Failed to create steam lobby."));
                        return;
                    }
                    DevConsole.Log(DCSection.Steam, "|DGGREEN|Lobby created.");
                }
                else
                {
                    if (_lobby.Owner.Id != 0 && Options.Data.blockedPlayers.Contains(_lobby.Owner.Id))
                    {
                        DuckNetwork.FailWithBlockedUser();
                        DevConsole.Log(DCSection.Steam, $"|DGRED|You have blocked the host! ({_lobby.Owner.Name})");
                        return;
                    }

                    UIMatchmakerMark2.instance?.Hook_OnLobbyProcessed(_lobby);

                    if (_lobby.EnterResult != RoomEnter.Success)
                    {
                        DevConsole.Log(DCSection.Steam, $"|DGGREEN|Failed to join lobby ({_lobby.EnterResult})");
                        string reason = "";
                        reason = _lobby.EnterResult == RoomEnter.DoesntExist
                            ? "Steam Lobby No Longer Exists."
                            : (_lobby.EnterResult != RoomEnter.NotAllowed
                                ? $"Failed to Join Lobby ({_lobby.EnterResult})"
                                : "Failed to Join Lobby (Access Denied)");
                        Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, reason));
                        return;
                    }

                    string version = _lobby.GetData("version");
                    NMVersionMismatch.Type mismatch = DuckNetwork.CheckVersion(version);
                    if (mismatch != NMVersionMismatch.Type.Match)
                    {
                        DuckNetwork.FailWithVersionMismatch(version, mismatch);
                        DevConsole.Log(DCSection.Steam, $"|DGRED|Lobby version mismatch! ({mismatch})");
                        return;
                    }

                    if (_lobby.GetData("modhash").Trim() != ModLoader.modHash)
                    {
                        ConnectionError.joinLobby = FacepunchSteam.Lobby.Base;
                        DuckNetwork.FailWithDifferentModsError();
                        return;
                    }

                    string dataHash = _lobby.GetData("datahash").Trim();
                    if (dataHash != Network.gameDataHash.ToString())
                    {
                        DuckNetwork.FailWithDatahashMismatch();
                        DevConsole.Log(DCSection.Steam, $"|DGRED|Lobby datahash mismatch! ({Network.gameDataHash} vs. {dataHash})");
                        return;
                    }

                    string loadedMods = _lobby.GetData("mods");
                    if (loadedMods != null && loadedMods != "")
                    {
                        string[] array = loadedMods.Split('|');
                        foreach (string s in array)
                        {
                            try
                            {
                                if (s == "" || s == "LOCAL")
                                    continue;

                                string[] s2 = s.Split(',');
                                uint datahash = 0u;

                                if (s2.Length != 2)
                                    continue;

                                ulong pID = Convert.ToUInt64(s2[0].Trim());
                                datahash = Convert.ToUInt32(s2[1].Trim());
                                Mod m = ModLoader.GetModFromWorkshopID(pID);

                                if (m != null)
                                {
                                    if (m.dataHash != datahash)
                                        DuckNetwork.FailWithModDatahashMismatch(m);
                                }
                                else
                                    DevConsole.Log("|DGRED|Non-existing Mod found in Lobby mod list, this should never happen!");
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    DevConsole.Log(DCSection.Steam, "|DGGREEN|----------------------------------------");
                    DevConsole.Log(DCSection.Steam, $"|DGGREEN|Lobby Joined ({_lobby.Owner.Name})");
                    AttemptConnection(_lobby.Owner, host: true);
                }
            }

            if (Network.isServer)
            {
                if (!_initializedSettings && _lobby.Id != 0L)
                {
                    UpdateRandomID(_lobby);
                    _lobby.SetData("started", "false");
                    _lobby.SetData("version", DG.version);
                    _lobby.SetData("beta", "2.0");
                    _lobby.SetData("dev", DG.devBuild ? "true" : "false");
                    _lobby.SetData("modifiers", "false");
                    _lobby.SetData("modhash", ModLoader.modHash);
                    _lobby.SetData("datahash", Network.gameDataHash.ToString());
                    _lobby.SetData("name", $"{FacepunchSteam.Me.Name}'s Lobby");
                    _lobby.SetData("numSlots", DuckNetwork.numSlots.ToString());
                    _lobby.SetData("name", _serverIdentifier);

                    if (_lobby.GetData("name") != TeamSelect2.DefaultGameName())
                        _lobby.SetData("customName", "true");

                    string modList = "";
                    bool first = true;
                    foreach (Mod m2 in ModLoader.accessibleMods)
                    {
                        if (m2 is not CoreMod && m2 is not DisabledMod && m2.configuration != null && !m2.configuration.disabled)
                        {
                            if (!first)
                                modList += "|";
                            modList = m2.configuration.isWorkshop
                                ? $"{modList}{m2.configuration.workshopID},{m2.dataHash}"
                                : $"{modList}LOCAL";
                            first = false;
                        }
                    }
                    _lobby.SetData("mods", modList);
                    ApplyLobbyData();
                    _initializedSettings = true;
                }

                if (!gotPingString)
                    TryGettingPingString();
            }

            if (_lobby.Owner.Id == SteamClient.SteamId && !Network.isServer)
            {
                foreach (var c in connections)
                    if (c.data is Friend friend && c.isHost && _lobby.Members.Contains(friend))
                        _lobby.Owner = friend;
            }
        }
#else
        if (_lobby != null && !_lobby.Processing && _lobby.Id != 0L)
        {
            if (!_lobbyCreationComplete)
            {
                _lobbyCreationComplete = true;
                if (Network.isServer)
                {
                    if (_lobby.JoinResult != SteamLobbyJoinResult.Success)
                    {
                        DevConsole.Log(DCSection.Steam, "|DGGREEN|Lobby creation failed!");
                        Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Failed to create steam lobby."));
                        return;
                    }
                    DevConsole.Log(DCSection.Steam, "|DGGREEN|Lobby created.");
                }
                else
                {
                    if (_lobby.Owner != null && Options.Data.blockedPlayers.Contains(_lobby.Owner.Id))
                    {
                        DuckNetwork.FailWithBlockedUser();
                        DevConsole.Log(DCSection.Steam, $"|DGRED|You have blocked the host! ({_lobby.Owner.Name})");
                        return;
                    }

                    UIMatchmakerMark2.instance?.Hook_OnLobbyProcessed(_lobby);

                    if (_lobby.JoinResult != SteamLobbyJoinResult.Success)
                    {
                        DevConsole.Log(DCSection.Steam, "|DGGREEN|Failed to join lobby (" + _lobby.JoinResult.ToString() + ")");
                        string reason = "";
                        reason = ((_lobby.JoinResult == SteamLobbyJoinResult.DoesntExist) ? "Steam Lobby No Longer Exists." : ((_lobby.JoinResult != SteamLobbyJoinResult.NotAllowed) ? ("Failed to Join Lobby (" + _lobby.JoinResult.ToString() + ")") : "Failed to Join Lobby (Access Denied)"));
                        Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, reason));
                        return;
                    }

                    string version = _lobby.GetLobbyData("version");
                    NMVersionMismatch.Type mismatch = DuckNetwork.CheckVersion(version);
                    if (mismatch != NMVersionMismatch.Type.Match)
                    {
                        DuckNetwork.FailWithVersionMismatch(version, mismatch);
                        DevConsole.Log(DCSection.Steam, "|DGRED|Lobby version mismatch! (" + mismatch.ToString() + ")");
                        return;
                    }

                    if (_lobby.GetLobbyData("modhash").Trim() != ModLoader.modHash)
                    {
                        ConnectionError.joinLobby = DGSteam.Lobby;
                        DuckNetwork.FailWithDifferentModsError();
                        return;
                    }

                    string dataHash = _lobby.GetLobbyData("datahash").Trim();
                    if (dataHash != Network.gameDataHash.ToString())
                    {
                        DuckNetwork.FailWithDatahashMismatch();
                        DevConsole.Log(DCSection.Steam, "|DGRED|Lobby datahash mismatch! (" + Network.gameDataHash + " vs. " + dataHash + ")");
                        return;
                    }

                    string loadedMods = _lobby.GetLobbyData("mods");
                    if (loadedMods != null && loadedMods != "")
                    {
                        string[] array = loadedMods.Split('|');
                        foreach (string s in array)
                        {
                            try
                            {
                                if (s == "" || s == "LOCAL")
                                    continue;

                                string[] s2 = s.Split(',');
                                uint datahash = 0u;

                                if (s2.Length != 2)
                                    continue;

                                ulong pID = Convert.ToUInt64(s2[0].Trim());
                                datahash = Convert.ToUInt32(s2[1].Trim());
                                Mod m = ModLoader.GetModFromWorkshopID(pID);

                                if (m != null)
                                {
                                    if (m.dataHash != datahash)
                                        DuckNetwork.FailWithModDatahashMismatch(m);
                                }
                                else
                                    DevConsole.Log("|DGRED|Non-existing Mod found in Lobby mod list, this should never happen!");
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    DevConsole.Log(DCSection.Steam, "|DGGREEN|----------------------------------------");
                    DevConsole.Log(DCSection.Steam, "|DGGREEN|Lobby Joined (" + _lobby.Owner.Name + ")");
                    AttemptConnection(_lobby.Owner, host: true);
                }
            }

            if (Network.isServer)
            {
                if (!_initializedSettings && _lobby.Id != 0L)
                {
                    UpdateRandomID(_lobby);
                    _lobby.SetLobbyData("started", "false");
                    _lobby.SetLobbyData("version", DG.version);
                    _lobby.SetLobbyData("beta", "2.0");
                    _lobby.SetLobbyData("dev", DG.devBuild ? "true" : "false");
                    _lobby.SetLobbyData("modifiers", "false");
                    _lobby.SetLobbyData("modhash", ModLoader.modHash);
                    _lobby.SetLobbyData("datahash", Network.gameDataHash.ToString());
                    _lobby.SetLobbyData("name", DGSteam.User.Name + "'s Lobby");
                    _lobby.SetLobbyData("numSlots", DuckNetwork.numSlots.ToString());
                    _lobby.Name = _serverIdentifier;

                    if (_lobby.Name != TeamSelect2.DefaultGameName())
                        _lobby.SetLobbyData("customName", "true");

                    string modList = "";
                    bool first = true;
                    foreach (Mod m2 in ModLoader.accessibleMods)
                    {
                        if (m2 is not CoreMod && m2 is not DisabledMod && m2.configuration != null && !m2.configuration.disabled)
                        {
                            if (!first)
                                modList += "|";
                            modList = (m2.configuration.isWorkshop ? (modList + m2.configuration.workshopID + "," + m2.dataHash) : (modList + "LOCAL"));
                            first = false;
                        }
                    }
                    _lobby.SetLobbyModsData(modList);
                    ApplyLobbyData();
                    _initializedSettings = true;
                }

                if (!gotPingString)
                    TryGettingPingString();
            }

            if (_lobby.Owner == DGSteam.User && !Network.isServer)
            {
                foreach (NetworkConnection c in base.connections)
                    if (c.data is User && c.isHost && _lobby.Users.Contains(c.data as User))
                    {
                        User newLobbyOwner = c.data as User;
                        _lobby.Owner = newLobbyOwner;
                    }
            }
        }
#endif
        base.Update();
    }

    public override void Terminate()
    {
        _initializedSettings = false;
        UnhookDelegates();
        base.Terminate();
    }

    public override void AddLobbyStringFilter(string key, string value, LobbyFilterComparison op)
    {
#if FACEPUNCH
        currentQuery = currentQuery.WithKeyValue(key, value);
#else
        DGSteam.AddLobbyStringFilter(key, value, (SteamLobbyComparison)op);
#endif
    }

    public override void AddLobbyNumericalFilter(string key, int value, LobbyFilterComparison op)
    {
#if FACEPUNCH
        currentQuery = ApplyFilter(currentQuery, (FilterMode)op, key, value);
#else
        DGSteam.AddLobbyNumericalFilter(key, value, (SteamLobbyComparison)op);
#endif
    }

    public override void ApplyTS2LobbyFilters()
    {
#if FACEPUNCH
        foreach (MatchSetting s in TeamSelect2.matchSettings)
        {
            if (s.value is int i)
            {
                if (s.filtered)
                    currentQuery = ApplyFilter(currentQuery, s.filterMode, s.id, i);
                else
                    currentQuery = currentQuery.OrderByNear(s.id, (int)s.defaultValue);
            }

            if (s.value is bool b)
            {
                if (s.filtered)
                    currentQuery = ApplyFilter(currentQuery, s.filterMode, s.id, b ? 1 : 0);
                else
                    currentQuery = currentQuery.OrderByNear(s.id, ((bool)s.defaultValue) ? 1 : 0);
            }
        }

        foreach (MatchSetting s2 in TeamSelect2.onlineSettings)
        {
            if (s2.value is int i)
            {
                if (s2.filtered)
                    currentQuery = ApplyFilter(currentQuery, s2.filterMode, s2.id, i);
                else
                    currentQuery = currentQuery.OrderByNear(s2.id, (int)s2.defaultValue);
            }

            if (s2.value is not bool)
                continue;

            if (s2.id == "modifiers")
            {
                if (s2.filtered)
                    currentQuery = currentQuery.WithKeyValue(s2.id, (bool)s2.value ? "true" : "false");
            }
            else if (s2.id == "customlevelsenabled")
            {
                if (s2.filtered)
                {
                    if ((bool)s2.value)
                        currentQuery = ApplyFilter(currentQuery, FilterMode.GreaterThan, s2.id, 0);
                    else
                        currentQuery = ApplyFilter(currentQuery, FilterMode.Equal, s2.id, 0);
                }
            }
            else if (s2.filtered)
                currentQuery = ApplyFilter(currentQuery, s2.filterMode, s2.id, (bool)s2.value ? 1 : 0);
            else
                currentQuery = currentQuery.OrderByNear(s2.id, (bool)s2.defaultValue ? 1 : 0);
        }
#else
        foreach (MatchSetting s in TeamSelect2.matchSettings)
        {
            if (s.value is int i)
            {
                if (s.filtered)
                    DGSteam.AddLobbyNumericalFilter(s.id, i, (SteamLobbyComparison)s.filterMode);
                else if (!s.filtered)
                    DGSteam.AddLobbyNearFilter(s.id, (int)s.defaultValue);
            }
            if (s.value is bool b)
            {
                if (s.filtered)
                    DGSteam.AddLobbyNumericalFilter(s.id, b ? 1 : 0, (SteamLobbyComparison)s.filterMode);
                else if (!s.filtered)
                    DGSteam.AddLobbyNearFilter(s.id, ((bool)s.defaultValue) ? 1 : 0);
            }
        }

        foreach (MatchSetting s2 in TeamSelect2.onlineSettings)
        {
            if (s2.value is int i)
            {
                if (s2.filtered)
                    DGSteam.AddLobbyNumericalFilter(s2.id, i, (SteamLobbyComparison)s2.filterMode);
                else if (!s2.filtered)
                    DGSteam.AddLobbyNearFilter(s2.id, (int)s2.defaultValue);
            }

            if (s2.value is not bool)
                continue;

            if (s2.id == "modifiers")
            {
                if (s2.filtered)
                    DGSteam.AddLobbyStringFilter(s2.id, ((bool)s2.value) ? "true" : "false", SteamLobbyComparison.Equal);
            }
            else if (s2.id == "customlevelsenabled")
            {
                if (s2.filtered)
                {
                    if ((bool)s2.value)
                        DGSteam.AddLobbyNumericalFilter(s2.id, 0, SteamLobbyComparison.GreaterThan);
                    else
                        DGSteam.AddLobbyNumericalFilter(s2.id, 0, SteamLobbyComparison.Equal);
                }
            }
            else if (s2.filtered)
                DGSteam.AddLobbyNumericalFilter(s2.id, ((bool)s2.value) ? 1 : 0, (SteamLobbyComparison)s2.filterMode);
            else if (!s2.filtered)
                DGSteam.AddLobbyNearFilter(s2.id, ((bool)s2.defaultValue) ? 1 : 0);
        }
#endif
    }

    public override void SearchForLobby()
    {
#if FACEPUNCH
        foundLobbies = (globalSearch
            ? currentQuery.FilterDistanceWorldwide()
            : currentQuery)
            .RequestAsync()
            .GetAwaiter()
            .GetResult();

        if (foundLobbies?.Length > 0)
        {
            foreach (var lobby in foundLobbies)
                _lobbySearchComplete(lobby);
        }
#else
        if (globalSearch)
            DGSteam.SearchForLobbyWorldwide();
        else
            DGSteam.SearchForLobby(null);
#endif
        globalSearch = false;
    }

    public override void RequestGlobalStats()
    {
#if FACEPUNCH
#else
        DGSteam.RequestGlobalStats();
#endif
    }

    public override bool IsLobbySearchComplete()
    {
#if FACEPUNCH
        return foundLobbies != null;
#else
        return DGSteam.IsLobbySearchComplete;
#endif
    }

    public override int NumLobbiesFound()
    {
#if FACEPUNCH
        return foundLobbies?.Length ?? -1;
#else
        return DGSteam.LobbiesFound;
#endif
    }

    public override long TryRequestDailyKills()
    {
#if FACEPUNCH
        Stat stat = new("kills");
        var KillsByDays = stat.GetGlobalIntDaysAsync(1)
            .GetAwaiter()
            .GetResult();
        if (KillsByDays?.Length > 0)
            return KillsByDays[0];
        return 0;
#else
        if (!DGSteam.WaitingForGlobalStats)
            return (long)DGSteam.GetDailyGlobalStat("kills");
        DGSteam.RequestGlobalStats();
        return 0;
#endif
    }

    public override Lobby GetSearchLobbyAtIndex(int i)
    {
#if FACEPUNCH
        if (foundLobbies != null && foundLobbies.Length > i)
            return foundLobbies[i];
        return default;
#else
        return DGSteam.GetSearchLobbyAtIndex(i);
#endif
    }
}
