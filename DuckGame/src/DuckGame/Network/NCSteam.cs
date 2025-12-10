using System;

namespace DuckGame;

public class NCSteam : NCNetworkImplementation
{
    private Lobby.UserStatusChangeDelegate _userChange;

    private Lobby.ChatMessageDelegate _chatDelegate;

    private Steam.ConnectionRequestedDelegate _connectionRequest;

    private Steam.ConnectionFailedDelegate _connectionFailed;

    private Steam.InviteReceivedDelegate _inviteReceived;

    private Steam.LobbySearchCompleteDelegate _lobbySearchComplete;

    private Steam.RequestCurrentStatsDelegate _requestStatsComplete;

    private string _serverIdentifier = "";

    private int _port;

    private ulong _connectionPacketIdentifier = 6094567099491692639uL;

    private bool _initializedSettings;

    private bool _lobbyCreationComplete;

    public static ulong inviteLobbyID;

    private bool gotPingString;

    private int pingWaitTimeout;

    public static bool globalSearch;

    public NCSteam(Network c, int networkIndex)
        : base(c, networkIndex)
    {
        HookUpDelegates();
    }

    public override NCError OnSendPacket(byte[] data, int length, object connection)
    {
        if (length < 1200)
        {
            Steam.SendPacket(connection as User, data, (uint)length, P2PDataSendType.Unreliable);
        }
        else
        {
            Steam.SendPacket(connection as User, data, (uint)length, P2PDataSendType.Reliable);
        }
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
            Steam.ConnectionRequested += _connectionRequest;
            Steam.ConnectionFailed += _connectionFailed;
            Steam.InviteReceived += _inviteReceived;
            Steam.LobbySearchComplete += _lobbySearchComplete;
            Steam.RequestCurrentStatsComplete += _requestStatsComplete;
        }
    }

    public void UnhookDelegates()
    {
        if (_connectionRequest != null)
        {
            Steam.ConnectionRequested -= _connectionRequest;
            Steam.ConnectionFailed -= _connectionFailed;
            Steam.InviteReceived -= _inviteReceived;
            Steam.LobbySearchComplete -= _lobbySearchComplete;
            _connectionRequest = null;
        }
    }

    public override NCError OnHostServer(string identifier, int port, NetworkLobbyType lobbyType, int maxConnections)
    {
        gotPingString = false;
        pingWaitTimeout = 0;
        if (_lobby != null)
        {
            Steam.LeaveLobby(_lobby);
            UnhookLobbyUserStatusChange(_lobby, OnUserStatusChange);
            UnhookLobbyChatMessage(_lobby, OnChatMessage);
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|Leaving lobby to host new lobby.");
        }
        _lobby = null;
        HookUpDelegates();
        _initializedSettings = false;
        _lobby = Steam.CreateLobby((SteamLobbyType)lobbyType, maxConnections);
        _lobby.name = identifier;
        if (_lobby == null)
        {
            return new NCError("|DGORANGE|STEAM |DGRED|Steam is not running.", NCErrorType.Error);
        }
        _userChange = OnUserStatusChange;
        HookUpLobbyUserStatusChange(_lobby, _userChange);
        _chatDelegate = OnChatMessage;
        HookUpLobbyChatMessage(_lobby, _chatDelegate);
        _serverIdentifier = identifier;
        _port = port;
        StartServerThread();
        return new NCError("|DGORANGE|STEAM |DGYELLOW|Attempting to create server lobby...", NCErrorType.Message);
    }

    private void HookUpLobbyUserStatusChange(Lobby l, Lobby.UserStatusChangeDelegate del)
    {
        l.UserStatusChange += del;
    }

    private void HookUpLobbyChatMessage(Lobby l, Lobby.ChatMessageDelegate del)
    {
        l.ChatMessage += del;
    }

    private void UnhookLobbyUserStatusChange(Lobby l, Lobby.UserStatusChangeDelegate del)
    {
        try
        {
            l.UserStatusChange -= del;
        }
        catch (Exception)
        {
        }
    }

    private void UnhookLobbyChatMessage(Lobby l, Lobby.ChatMessageDelegate del)
    {
        try
        {
            l.ChatMessage -= del;
        }
        catch (Exception)
        {
        }
    }

    public override NCError OnJoinServer(string identifier, int port, string ip)
    {
        gotPingString = false;
        pingWaitTimeout = 0;
        if (_lobby != null)
        {
            Steam.LeaveLobby(_lobby);
            UnhookLobbyUserStatusChange(_lobby, OnUserStatusChange);
            UnhookLobbyChatMessage(_lobby, OnChatMessage);
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|Leaving lobby to join new lobby.");
        }
        _lobby = null;
        HookUpDelegates();
        _serverIdentifier = identifier;
        if (identifier == "joinTest")
        {
            _lobby = Steam.JoinLobby(1uL);
            _serverIdentifier = _lobby.id.ToString();
        }
        else
        {
            _lobby = Steam.JoinLobby(Convert.ToUInt64(identifier));
        }
        if (_lobby == null)
        {
            return new NCError("Steam is not running.", NCErrorType.Error);
        }
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
            DevConsole.Log(DCSection.Steam, "|DGGREEN|" + who.name + " (" + who.id + ") has joined the Steam lobby.");
            if (Network.isServer && DuckNetwork.localConnection.status == ConnectionStatus.Connected)
            {
                AttemptConnection(who);
            }
        }
        else if ((flags & SteamLobbyUserStatusFlags.Left) != 0)
        {
            DevConsole.Log(DCSection.Steam, "|DGRED|" + GetDrawString(who) + " has left the Steam lobby.");
        }
        else if ((flags & SteamLobbyUserStatusFlags.Disconnected) != 0)
        {
            DevConsole.Log(DCSection.Steam, "|DGRED|" + GetDrawString(who) + " has disconnected from the Steam lobby.");
        }
        if ((flags & SteamLobbyUserStatusFlags.Kicked) != 0)
        {
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|" + GetDrawString(responsible) + " kicked " + GetDrawString(who) + ".");
        }
    }

    public void OnChatMessage(User who, byte[] data)
    {
        Steam_LobbyMessage m = Steam_LobbyMessage.Receive(who, data);
        if (m != null)
        {
            if (m.message == "COM_FAIL" && m.context == Steam.user)
            {
                DevConsole.Log(DCSection.Connection, "Communication failure with " + who.name + "... Disconnecting!");
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.EveryoneDisconnected, "Could not connect to server."));
            }
            else if (m.message == "IM_OUTTAHERE")
            {
                DevConsole.Log(DCSection.Connection, "Received lobby exit message from " + who.name + "...");
                Network.DisconnectClient(GetConnection(who), new DuckNetErrorInfo(DuckNetError.ClientDisconnected, who.name + " left the lobby."));
            }
        }
    }

    private string GetDrawString(User pUser)
    {
        return pUser.name + " (" + pUser.id + ")";
    }

    public void OnConnectionRequest(User who)
    {
        DevConsole.Log(DCSection.Connection, "NCSteam.OnConnectionRequest(" + GetDrawString(who) + ")");
        if ((GetConnection(who) != null || (base.lobby != null && base.lobby.users.Contains(who))) && Network.isActive)
        {
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|" + GetDrawString(who) + " has requested a connection.");
            Steam.AcceptConnection(who);
        }
        else if (!Network.isActive)
        {
            DevConsole.Log(DCSection.Steam, "|DGRED| Connection request ignored(" + GetDrawString(who) + ")(Network.isActive == false)");
        }
        else
        {
            DevConsole.Log(DCSection.Steam, "|DGRED| Connection request ignored(" + GetDrawString(who) + ")(User not found)");
        }
    }

    public void OnConnectionFailed(User who, byte pError)
    {
        DevConsole.Log(DCSection.Steam, "|DGRED|Connection with " + GetDrawString(who) + " has failed (" + pError + ")!");
    }

    public static void PrepareProfilesForJoin()
    {
        foreach (Team item in Teams.all)
        {
            item.ClearProfiles();
        }
        Profile.defaultProfileMappings[0] = Profiles.experienceProfile;
        Teams.Player1.Join(Profiles.experienceProfile);
        TeamSelect2.ControllerLayoutsChanged();
    }

    public void OnInviteReceived(User who, Lobby lobby)
    {
        inviteLobbyID = lobby.id;
        if (Level.current is TitleScreen || Level.current is Editor || Level.current is DuckGameTestArea || (Level.current is GameLevel && (Level.current as GameLevel)._editorTestMode))
        {
            PrepareProfilesForJoin();
        }
        Level.current = new JoinServer(lobby.id);
    }

    public void OnLobbySearchComplete(Lobby lobby)
    {
    }

    public void OnRequestStatsComplete()
    {
    }

    protected override object GetConnectionObject(string identifier)
    {
        return User.GetUser(Convert.ToUInt64(identifier));
    }

    public override string GetConnectionIdentifier(object connection)
    {
        if (connection is User { id: var id })
        {
            return id.ToString();
        }
        return "no info";
    }

    public override string GetConnectionName(object connection)
    {
        if (connection is User user)
        {
            return user.name;
        }
        return "no info";
    }

    protected override string OnGetLocalName()
    {
        if (Steam.user != null)
        {
            return Steam.user.name;
        }
        return "no info";
    }

    protected override NCError OnSpinServerThread()
    {
        if (_lobby == null)
        {
            if (NetworkDebugger.enabled)
            {
                return null;
            }
            return new NCError("|DGORANGE|STEAM |DGRED|Lobby was closed.", NCErrorType.CriticalError);
        }
        if (_lobby.processing)
        {
            return null;
        }
        if (_lobby.id == 0L)
        {
            return new NCError("|DGORANGE|STEAM |DGRED|Failed to create lobby.", NCErrorType.CriticalError);
        }
        return RunSharedLogic();
    }

    protected override NCError OnSpinClientThread()
    {
        if (_lobby == null)
        {
            return new NCError("|DGORANGE|STEAM |DGYELLOW|Lobby was closed.", NCErrorType.CriticalError);
        }
        if (_lobby.processing)
        {
            return null;
        }
        if (_lobby.id == 0L)
        {
            return new NCError("|DGORANGE|STEAM |DGRED|Failed to join lobby.", NCErrorType.CriticalError);
        }
        return RunSharedLogic();
    }

    protected NCError RunSharedLogic()
    {
        while (true)
        {
            SteamPacket packet = null;
            packet = Steam.ReadPacket();
            if (packet == null)
            {
                break;
            }
            OnPacket(packet.data, packet.connection);
        }
        return null;
    }

    protected override void Disconnect(NetworkConnection c)
    {
        if (c != null && c.data is User user)
        {
            DevConsole.Log(DCSection.Steam, "|DGRED|Closing connection with " + GetDrawString(user) + ".");
            Steam.CloseConnection(user);
        }
        base.Disconnect(c);
    }

    protected override void KillConnection()
    {
        if (_lobby != null)
        {
            if (_lobby.owner == Steam.user && DuckNetwork.potentialHostObject is User newOwner && _lobby.users.Contains(newOwner))
            {
                _lobby.owner = newOwner;
            }
            Steam_LobbyMessage.Send("IM_OUTTAHERE", null);
            Steam.LeaveLobby(_lobby);
            UnhookLobbyUserStatusChange(_lobby, OnUserStatusChange);
            UnhookLobbyChatMessage(_lobby, OnChatMessage);
            DevConsole.Log(DCSection.Steam, "|DGYELLOW|Leaving lobby to host new lobby.");
        }
        _lobby = null;
        _lobbyCreationComplete = false;
        _initializedSettings = false;
        base.KillConnection();
    }

    public override void ApplyLobbyData()
    {
        foreach (MatchSetting s in TeamSelect2.matchSettings)
        {
            if (s.value is int)
            {
                _lobby.SetLobbyData(s.id, ((int)s.value).ToString());
            }
            else if (s.value is bool)
            {
                _lobby.SetLobbyData(s.id, (((bool)s.value) ? 1 : 0).ToString());
            }
        }
        foreach (MatchSetting s2 in TeamSelect2.onlineSettings)
        {
            if (s2.id == "password")
            {
                _lobby.SetLobbyData("password", ((string)s2.value != "") ? "true" : "false");
            }
            if (s2.id == "modifiers")
            {
                if (s2.filtered)
                {
                    _lobby.SetLobbyData(s2.id, ((bool)s2.value) ? "true" : "false");
                }
            }
            else if (s2.id == "dedicated")
            {
                _lobby.SetLobbyData(s2.id, ((bool)s2.value) ? "true" : "false");
            }
            else if (s2.value is int)
            {
                _lobby.SetLobbyData(s2.id, ((int)s2.value).ToString());
            }
            else if (s2.value is bool)
            {
                _lobby.SetLobbyData(s2.id, (((bool)s2.value) ? 1 : 0).ToString());
            }
        }
        foreach (UnlockData dat in Unlocks.allUnlocks)
        {
            _lobby.SetLobbyData(dat.id, (dat.enabled ? 1 : 0).ToString());
        }
        _lobby.SetLobbyData("customLevels", Editor.customLevelCount.ToString());
    }

    private void TryGettingPingString()
    {
        if (_lobby != null && !_lobby.processing && _lobby.id != 0L && pingWaitTimeout <= 0 && !gotPingString)
        {
            string pingString = Steam.GetLocalPingString();
            _lobby.SetLobbyData("pingstring", pingString);
            if (pingString != null && pingString != "")
            {
                gotPingString = true;
            }
            pingWaitTimeout = 60;
        }
        pingWaitTimeout--;
    }

    public override void Update()
    {
        if (_lobby != null && !_lobby.processing && _lobby.id != 0L)
        {
            if (!_lobbyCreationComplete)
            {
                _lobbyCreationComplete = true;
                if (Network.isServer)
                {
                    if (_lobby.joinResult != SteamLobbyJoinResult.Success)
                    {
                        DevConsole.Log(DCSection.Steam, "|DGGREEN|Lobby creation failed!");
                        Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Failed to create steam lobby."));
                        return;
                    }
                    DevConsole.Log(DCSection.Steam, "|DGGREEN|Lobby created.");
                }
                else
                {
                    if (_lobby.owner != null && Options.Data.blockedPlayers.Contains(_lobby.owner.id))
                    {
                        DuckNetwork.FailWithBlockedUser();
                        DevConsole.Log(DCSection.Steam, "|DGRED|You have blocked the host! (" + _lobby.owner.name + ")");
                        return;
                    }
                    if (UIMatchmakerMark2.instance != null)
                    {
                        UIMatchmakerMark2.instance.Hook_OnLobbyProcessed(_lobby);
                    }
                    if (_lobby.joinResult != SteamLobbyJoinResult.Success)
                    {
                        DevConsole.Log(DCSection.Steam, "|DGGREEN|Failed to join lobby (" + _lobby.joinResult.ToString() + ")");
                        string reason = "";
                        reason = ((_lobby.joinResult == SteamLobbyJoinResult.DoesntExist) ? "Steam Lobby No Longer Exists." : ((_lobby.joinResult != SteamLobbyJoinResult.NotAllowed) ? ("Failed to Join Lobby (" + _lobby.joinResult.ToString() + ")") : "Failed to Join Lobby (Access Denied)"));
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
                        ConnectionError.joinLobby = Steam.lobby;
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
                                {
                                    continue;
                                }
                                string[] s2 = s.Split(',');
                                uint datahash = 0u;
                                if (s2.Length != 2)
                                {
                                    continue;
                                }
                                ulong pID = Convert.ToUInt64(s2[0].Trim());
                                datahash = Convert.ToUInt32(s2[1].Trim());
                                Mod m = ModLoader.GetModFromWorkshopID(pID);
                                if (m != null)
                                {
                                    if (m.dataHash != datahash)
                                    {
                                        DuckNetwork.FailWithModDatahashMismatch(m);
                                    }
                                }
                                else
                                {
                                    DevConsole.Log("|DGRED|Non-existing Mod found in Lobby mod list, this should never happen!");
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    DevConsole.Log(DCSection.Steam, "|DGGREEN|----------------------------------------");
                    DevConsole.Log(DCSection.Steam, "|DGGREEN|Lobby Joined (" + _lobby.owner.name + ")");
                    AttemptConnection(_lobby.owner, host: true);
                }
            }
            if (Network.isServer)
            {
                if (!_initializedSettings && _lobby.id != 0L)
                {
                    UpdateRandomID(_lobby);
                    _lobby.SetLobbyData("started", "false");
                    _lobby.SetLobbyData("version", DG.version);
                    _lobby.SetLobbyData("beta", "2.0");
                    _lobby.SetLobbyData("dev", DG.devBuild ? "true" : "false");
                    _lobby.SetLobbyData("modifiers", "false");
                    _lobby.SetLobbyData("modhash", ModLoader.modHash);
                    _lobby.SetLobbyData("datahash", Network.gameDataHash.ToString());
                    _lobby.SetLobbyData("name", Steam.user.name + "'s Lobby");
                    _lobby.SetLobbyData("numSlots", DuckNetwork.numSlots.ToString());
                    _lobby.name = _serverIdentifier;
                    if (_lobby.name != TeamSelect2.DefaultGameName())
                    {
                        _lobby.SetLobbyData("customName", "true");
                    }
                    string modList = "";
                    bool first = true;
                    foreach (Mod m2 in ModLoader.accessibleMods)
                    {
                        if (!(m2 is CoreMod) && !(m2 is DisabledMod) && m2.configuration != null && !m2.configuration.disabled)
                        {
                            if (!first)
                            {
                                modList += "|";
                            }
                            modList = (m2.configuration.isWorkshop ? (modList + m2.configuration.workshopID + "," + m2.dataHash) : (modList + "LOCAL"));
                            first = false;
                        }
                    }
                    _lobby.SetLobbyModsData(modList);
                    ApplyLobbyData();
                    _initializedSettings = true;
                }
                if (!gotPingString)
                {
                    TryGettingPingString();
                }
            }
            if (_lobby.owner == Steam.user && !Network.isServer)
            {
                foreach (NetworkConnection c in base.connections)
                {
                    if (c.data is User && c.isHost && _lobby.users.Contains(c.data as User))
                    {
                        User newLobbyOwner = c.data as User;
                        _lobby.owner = newLobbyOwner;
                    }
                }
            }
        }
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
        Steam.AddLobbyStringFilter(key, value, (SteamLobbyComparison)op);
    }

    public override void AddLobbyNumericalFilter(string key, int value, LobbyFilterComparison op)
    {
        Steam.AddLobbyNumericalFilter(key, value, (SteamLobbyComparison)op);
    }

    public override void ApplyTS2LobbyFilters()
    {
        foreach (MatchSetting s in TeamSelect2.matchSettings)
        {
            if (s.value is int)
            {
                if (s.filtered)
                {
                    Steam.AddLobbyNumericalFilter(s.id, (int)s.value, (SteamLobbyComparison)s.filterMode);
                }
                else if (!s.filtered)
                {
                    Steam.AddLobbyNearFilter(s.id, (int)s.defaultValue);
                }
            }
            if (s.value is bool)
            {
                if (s.filtered)
                {
                    Steam.AddLobbyNumericalFilter(s.id, ((bool)s.value) ? 1 : 0, (SteamLobbyComparison)s.filterMode);
                }
                else if (!s.filtered)
                {
                    Steam.AddLobbyNearFilter(s.id, ((bool)s.defaultValue) ? 1 : 0);
                }
            }
        }
        foreach (MatchSetting s2 in TeamSelect2.onlineSettings)
        {
            if (s2.value is int)
            {
                if (s2.filtered)
                {
                    Steam.AddLobbyNumericalFilter(s2.id, (int)s2.value, (SteamLobbyComparison)s2.filterMode);
                }
                else if (!s2.filtered)
                {
                    Steam.AddLobbyNearFilter(s2.id, (int)s2.defaultValue);
                }
            }
            if (!(s2.value is bool))
            {
                continue;
            }
            if (s2.id == "modifiers")
            {
                if (s2.filtered)
                {
                    Steam.AddLobbyStringFilter(s2.id, ((bool)s2.value) ? "true" : "false", SteamLobbyComparison.Equal);
                }
            }
            else if (s2.id == "customlevelsenabled")
            {
                if (s2.filtered)
                {
                    if ((bool)s2.value)
                    {
                        Steam.AddLobbyNumericalFilter(s2.id, 0, SteamLobbyComparison.GreaterThan);
                    }
                    else
                    {
                        Steam.AddLobbyNumericalFilter(s2.id, 0, SteamLobbyComparison.Equal);
                    }
                }
            }
            else if (s2.filtered)
            {
                Steam.AddLobbyNumericalFilter(s2.id, ((bool)s2.value) ? 1 : 0, (SteamLobbyComparison)s2.filterMode);
            }
            else if (!s2.filtered)
            {
                Steam.AddLobbyNearFilter(s2.id, ((bool)s2.defaultValue) ? 1 : 0);
            }
        }
    }

    public override void SearchForLobby()
    {
        if (globalSearch)
        {
            Steam.SearchForLobbyWorldwide();
        }
        else
        {
            Steam.SearchForLobby(null);
        }
        globalSearch = false;
    }

    public override void RequestGlobalStats()
    {
        Steam.RequestGlobalStats();
    }

    public override bool IsLobbySearchComplete()
    {
        return Steam.lobbySearchComplete;
    }

    public override int NumLobbiesFound()
    {
        return Steam.lobbiesFound;
    }

    public override bool TryRequestDailyKills(out long kills)
    {
        kills = 0L;
        if (!Steam.waitingForGlobalStats)
        {
            kills = (long)Steam.GetDailyGlobalStat("kills");
        }
        Steam.RequestGlobalStats();
        return true;
    }

    public override Lobby GetSearchLobbyAtIndex(int i)
    {
        return Steam.GetSearchLobbyAtIndex(i);
    }
}
