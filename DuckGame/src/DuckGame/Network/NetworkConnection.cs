using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace DuckGame;

public class NetworkConnection
{
    public class FailedGhost
    {
        public ushort ghost;

        public long mask;
    }

    public bool banned;

    public bool kicking;

    public int failureNotificationCooldown;

    private static NetworkConnection _context;

    public List<NetIndex16> _destroyedGhostResends = new List<NetIndex16>();

    private DataLayerDebug.BadConnection _debuggerContext;

    private Profile _profile;

    private uint _sessionID;

    public ushort _lastReceivedPacketOrder;

    private object _data;

    private string _realName;

    private string _identifier;

    private StreamManager _manager;

    private byte _loadingStatus = byte.MaxValue;

    protected ConnectionStatus _internalStatus;

    protected ConnectionStatus _previousStatus;

    private bool _isHost;

    public HashSet<ushort> recentlyReceivedPackets = new HashSet<ushort>();

    public byte recentlyReceivedPacketsArrayIndex;

    public ushort[] recentlyReceivedPacketsArray;

    public static readonly int kMaxRecentlyReceivedPackets = 128;

    private bool _sentThisFrame;

    private uint _lastReceivedTime;

    private uint _lastSentTime;

    private byte _connectsReceived;

    private uint _personalTick;

    private int _disconnectsSent;

    private string _theirVersion = "";

    private bool _theirModsIncompatible;

    private bool _connectionTimeout;

    public HashSet<InputDevice> synchronizedInputDevices = new HashSet<InputDevice>();

    public byte lastSynchronizedDeviceType = byte.MaxValue;

    public int triesSinceInputChangeSend = 60;

    public int logTransferSize;

    public int logTransferProgress;

    public int dataTransferSize;

    public int dataTransferProgress;

    public int wantsGhostData = -1;

    public static int connectionLoopIndex;

    private int _currentSessionTicks;

    private int _ticksTillDisconnectAttempt;

    private uint _lastTickReceived;

    private uint _estimatedClientTick;

    private uint _previousReceiveGap;

    public bool sentFilterMessage;

    public HashSet<ushort> acksReceived = new HashSet<ushort>();

    public List<FailedGhost> failedGhosts = new List<FailedGhost>();

    private static int kconnectionIDInc = 0;

    private int _connectionID;

    public ushort[] kAckOffsets;

    private NetworkPacket[] _packetHistory = new NetworkPacket[17];

    private int _packetHistoryIndex;

    private bool _pongedThisFrame;

    public bool restartPingTimer = true;

    public double averageHeartbeatTime;

    private DuckNetErrorInfo _connectionError;

    private byte pingIndex;

    private Dictionary<byte, NMNewPing> _pings = new Dictionary<byte, NMNewPing>();

    private int pingWait;

    private int _pingsSent;

    private const int kConnectionTroubleGap = 400;

    private const int kConnectionFailureGap = 960;

    private int _numErrorLogs;

    public bool sendPacketsNow;

    /// <summary>
    ///             DON'T set this variable in realtime! It's used to determine how many input states to
    ///             synchronize and that data will be read incorrectly if this is changed while the game runs. It can
    ///             be changed only at the start of the game.
    /// </summary>
    public static int packetsEvery = 1;

    public static float ghostLerpDivisor = 0f;

    public int authorityPower
    {
        get
        {
            if (profile != null)
            {
                return GameLevel.NumberOfDucks - (profile.networkIndex + DuckNetwork.levelIndex) % GameLevel.NumberOfDucks + 1;
            }
            return 1;
        }
    }

    public int debuggerIndex
    {
        get
        {
            if (NetworkDebugger.enabled)
            {
                if (_identifier != null)
                {
                    return Convert.ToInt32(_identifier.Split(':')[1]) - 1330;
                }
                return -1;
            }
            return -1;
        }
    }

    public static NetworkConnection context
    {
        get
        {
            return _context;
        }
        set
        {
            _context = value;
        }
    }

    public DataLayerDebug.BadConnection debuggerContext
    {
        get
        {
            if (_debuggerContext == null)
            {
                _debuggerContext = new DataLayerDebug.BadConnection(this);
            }
            return _debuggerContext;
        }
    }

    public Profile profile
    {
        get
        {
            return _profile;
        }
        set
        {
            _profile = value;
        }
    }

    public uint sessionID => _sessionID;

    public object data => _data;

    public string identifier => _identifier;

    public bool hasRealName => _realName != null;

    public string name
    {
        get
        {
            if (_realName != null)
            {
                return _realName;
            }
            string n = "NULL";
            if (_data != null)
            {
                n = Network.activeNetwork.core.GetConnectionName(_data);
            }
            switch (n)
            {
                case null:
                case "":
                case "no info":
                    return _identifier;
                default:
                    return n;
            }
        }
        set
        {
            _realName = value;
        }
    }

    public StreamManager manager => _manager;

    public byte levelIndex
    {
        get
        {
            return _loadingStatus;
        }
        set
        {
            _loadingStatus = value;
        }
    }

    protected ConnectionStatus _status
    {
        get
        {
            return _internalStatus;
        }
        set
        {
            if (_internalStatus != value && Network.activeNetwork != null && Network.activeNetwork.core != null)
            {
                Network.activeNetwork.core._connectionsDirty = true;
            }
            _internalStatus = value;
        }
    }

    public ConnectionStatus status => _status;

    public bool isHost
    {
        get
        {
            return _isHost;
        }
        set
        {
            _isHost = value;
        }
    }

    public bool sentThisFrame
    {
        get
        {
            return _sentThisFrame;
        }
        set
        {
            _sentThisFrame = value;
        }
    }

    public uint lastTickReceived
    {
        get
        {
            return _lastTickReceived;
        }
        set
        {
            _lastTickReceived = value;
            _estimatedClientTick = _lastTickReceived;
        }
    }

    public uint estimatedClientTick
    {
        get
        {
            return (uint)(_estimatedClientTick + (int)(manager.ping / 2f * 60f));
        }
        set
        {
            _estimatedClientTick = value;
        }
    }

    public int connectionID => _connectionID;

    private int timeBetweenPings
    {
        get
        {
            if (_pingsSent < 45)
            {
                return 2;
            }
            if (MonoMain.pauseMenu != null || Keyboard.Down(Keys.F1))
            {
                return 4;
            }
            return 10;
        }
    }

    public uint receiveGap => _personalTick - _lastReceivedTime;

    public bool isExperiencingConnectionTrouble => receiveGap > 100;

    public override string ToString()
    {
        string connectionName = "|WHITE|(";
        if (profile != null && profile.persona != null)
        {
            connectionName = connectionName + profile.persona.colorUsable.ToDGColorString() + "(" + profile.networkIndex + ")";
        }
        if (isHost)
        {
            connectionName += "(H)";
        }
        string realName = null;
        if (!hasRealName || data == null)
        {
            realName = ((data is User) ? (data as User).id.ToString() : ((Steam.user == null) ? "LAN USER" : Steam.user.id.ToString()));
        }
        else if (Network.activeNetwork.core is NCSteam)
        {
            realName = name + "," + (data as User).id;
        }
        else if (Network.activeNetwork.core is NCBasic)
        {
            realName = name + "," + (data as IPEndPoint).ToString();
        }
        realName.Replace("|", "(");
        realName.Replace("@", "$");
        connectionName += realName;
        if (profile != null && profile.networkStatus != DuckNetStatus.Connected)
        {
            connectionName = connectionName + "," + profile.networkStatus;
        }
        connectionName += "|WHITE|)";
        return connectionName + "(" + _connectionID + ")";
    }

    public void SetDebuggerContext(DataLayerDebug.BadConnection pContext)
    {
        _debuggerContext = pContext;
    }

    public void BeginConnection()
    {
        ChangeStatus((_data != null) ? ConnectionStatus.Connecting : ConnectionStatus.Connected);
    }

    public void LeaveLobby()
    {
        ChangeStatus(ConnectionStatus.Disconnected);
    }

    private void ChangeStatus(ConnectionStatus s)
    {
        if (s != ConnectionStatus.Disconnecting && s != ConnectionStatus.Disconnected && banned)
        {
            return;
        }
        bool num = _status != s;
        _status = s;
        if (num)
        {
            DevConsole.Log(DCSection.Connection, "|DGORANGE|Connection Status Changed (" + s.ToString() + ")", this);
            if (_status == ConnectionStatus.Disconnected)
            {
                Network.activeNetwork.core.DisconnectClient(this, _connectionError);
            }
        }
    }

    public void SetData(object d)
    {
        _data = d;
        if (_data != null)
        {
            _identifier = Network.activeNetwork.core.GetConnectionIdentifier(_data);
        }
        Reset("Connection _data changed");
    }

    public NetworkConnection(object dat, string id = null)
    {
        _connectionID = kconnectionIDInc++;
        if (dat != null)
        {
            _identifier = Network.activeNetwork.core.GetConnectionIdentifier(dat);
        }
        else
        {
            _identifier = "local";
        }
        if (id != null)
        {
            _identifier = id;
        }
        Reset("NetworkConnection constructor");
        _data = dat;
        kAckOffsets = new ushort[16];
        for (int i = 0; i < 16; i++)
        {
            kAckOffsets[i] = (ushort)(1 << i);
        }
    }

    public void Reset(string reason)
    {
        _manager = new StreamManager(this);
        acksReceived = new HashSet<ushort>();
        _isHost = false;
        ChangeStatus(ConnectionStatus.Disconnected);
        levelIndex = byte.MaxValue;
        _sentThisFrame = false;
        _lastReceivedTime = 0u;
        _lastSentTime = 0u;
        _personalTick = 0u;
        _connectsReceived = 0;
        sentFilterMessage = false;
        _numErrorLogs = 0;
        _lastTickReceived = 0u;
        _estimatedClientTick = 0u;
        _currentSessionTicks = 0;
        _ticksTillDisconnectAttempt = 0;
        _disconnectsSent = 0;
        wantsGhostData = -1;
        _lastReceivedPacketOrder = 0;
        _packetHistory = new NetworkPacket[33];
        _packetHistoryIndex = 0;
        _theirVersion = "";
        _connectionTimeout = false;
        synchronizedInputDevices.Clear();
        lastSynchronizedDeviceType = byte.MaxValue;
        pingWait = 0;
        _realName = null;
        _pingsSent = 0;
        kicking = false;
        recentlyReceivedPackets.Clear();
        recentlyReceivedPacketsArray = new ushort[kMaxRecentlyReceivedPackets];
        failureNotificationCooldown = 0;
        if (_data != null && GhostManager.context != null)
        {
            GhostManager.context.Clear(this);
        }
        if (NetworkDebugger.enabled)
        {
            debuggerContext.Reset();
        }
        DevConsole.Log(DCSection.Connection, "@disconnect Reset called on " + identifier + "(" + ((Steam.user != null) ? Steam.user.id.ToString() : "local") + ", " + reason + ")");
    }

    public void StartNewSession()
    {
        _sessionID = Rando.UInt();
        _currentSessionTicks = 0;
    }

    public void SynchronizeSession(uint pWith)
    {
        if (pWith == _sessionID + 1)
        {
            _sessionID = pWith;
            BecomeConnected(pWith);
        }
        else
        {
            _sessionID = pWith + 1;
            DevConsole.Log(DCSection.Connection, "|DGGREEN|Synchronizing Session (" + _sessionID + ")", this);
        }
    }

    public ushort GetAck()
    {
        return _lastReceivedPacketOrder;
    }

    public ushort GetAckBitfield()
    {
        ushort bitfield = 0;
        for (int i = 0; i < 17; i++)
        {
            if (_packetHistory[i] != null)
            {
                int dif = _lastReceivedPacketOrder - _packetHistory[i].order;
                if (dif < 0)
                {
                    dif += 65535;
                }
                if (dif > 0 && dif < 17)
                {
                    bitfield |= kAckOffsets[dif - 1];
                }
            }
        }
        return bitfield;
    }

    public void PacketReceived(NetworkPacket packet)
    {
        _lastReceivedTime = _personalTick;
        if (PacketOrderGreater(packet.order, _lastReceivedPacketOrder))
        {
            _lastReceivedPacketOrder = packet.order;
        }
        _packetHistory[_packetHistoryIndex % 17] = packet;
        _packetHistoryIndex++;
    }

    private static bool PacketOrderGreater(ushort s1, ushort s2)
    {
        if (s1 <= s2 || s1 - s2 > 32768)
        {
            if (s1 < s2)
            {
                return s2 - s1 > 32768;
            }
            return false;
        }
        return true;
    }

    public void PacketSent()
    {
        _lastSentTime = _personalTick;
    }

    public void OnNonConnectionMessage(NetMessage message)
    {
    }

    public void HardTerminate()
    {
        Network.activeNetwork.core.DisconnectClient(this, new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Hard Terminate."));
        ChangeStatus(ConnectionStatus.Disconnected);
    }

    public void Disconnect()
    {
        Network.activeNetwork.core.DisconnectClient(this, new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Hard Terminate."));
        ChangeStatus(ConnectionStatus.Disconnected);
    }

    public void BecomeConnected(uint pSession)
    {
        if (status == ConnectionStatus.Connecting)
        {
            DevConsole.Log(DCSection.NetCore, ToString() + " BecomeConnected on session index " + pSession);
            ChangeStatus(ConnectionStatus.Connected);
            Network.activeNetwork.core.OnConnection(this);
        }
    }

    private void Disconnect_IncompatibleMods()
    {
        Network.activeNetwork.core.DisconnectClient(this, new DuckNetErrorInfo(DuckNetError.ModsIncompatible, "Host has different Mods enabled!"));
        ConnectionError.joinLobby = Steam.lobby;
    }

    private void Disconnect_DifferentVersion(string theirVersion)
    {
        DuckNetwork.CheckVersion(theirVersion);
        Network.activeNetwork.core.DisconnectClient(this, DuckNetwork.AssembleMismatchError(theirVersion));
    }

    private void Disconnect_ConnectionTimeout()
    {
        Network.activeNetwork.core.DisconnectClient(this, new DuckNetErrorInfo(DuckNetError.ConnectionTimeout, "Could not connect. (timeout)"));
    }

    public void Disconnect_ConnectionFailure()
    {
        DevConsole.Log(DCSection.Connection, "|DGRED|Disconnect_ConnectionFailure()");
        Network.activeNetwork.core.DisconnectClient(this, new DuckNetErrorInfo(DuckNetError.ConnectionLost, "Connection was lost."));
    }

    public void OnAnyMessage(NetMessage pMessage)
    {
        if (pMessage.session == sessionID)
        {
            BecomeConnected(sessionID);
        }
        _lastReceivedTime = _personalTick;
        if (pMessage is NMNetworkCoreMessage)
        {
            OnMessage(pMessage as NMNetworkCoreMessage);
        }
        else
        {
            OnNonConnectionMessage(pMessage);
        }
    }

    public void OnMessage(NMNetworkCoreMessage message)
    {
        if (_data == null)
        {
            DevConsole.Log(DCSection.Connection, "|RED|Null Connection Data, cannot receive message!");
            return;
        }
        if (message is NMDisconnect)
        {
            DevConsole.Log(DCSection.DuckNet, "@received Received |WHITE|" + message.ToString(), message.connection);
            Network.DisconnectClient(this, (message as NMDisconnect).GetError());
        }
        else if (status == ConnectionStatus.Disconnecting)
        {
            DevConsole.Log(DCSection.Connection, "|RED|Received connection message during disconnect...");
            return;
        }
        if (message is NMConnect)
        {
            DevConsole.Log(DCSection.DuckNet, "@received Received |WHITE|" + message.ToString(), message.connection);
            NMConnect connect = message as NMConnect;
            if (DG.version != connect.version)
            {
                _theirVersion = connect.version;
                Send.Message(new NMWrongVersion(DG.version), NetMessagePriority.UnreliableUnordered, this);
                Disconnect_DifferentVersion(_theirVersion);
                return;
            }
            if (ModLoader.modHash != connect.modHash)
            {
                Send.Message(new NMModIncompatibility(), NetMessagePriority.UnreliableUnordered, this);
                Disconnect_IncompatibleMods();
                return;
            }
            if (connect.session > sessionID)
            {
                SynchronizeSession(connect.session);
            }
        }
        if (message is NMNewPing)
        {
            if (!_pongedThisFrame)
            {
                Send.Message(new NMNewPong((message as NMNewPing).index), NetMessagePriority.UnreliableUnordered, this);
                sendPacketsNow = true;
                _pongedThisFrame = true;
            }
            if (message is NMNewPingHost)
            {
                Network.ReceiveHostTime((message as NMNewPingHost).hostSynchronizedTime);
            }
        }
        else if (message is NMNewPong)
        {
            NMNewPing p = null;
            if (_pings.TryGetValue((message as NMNewPong).index, out p))
            {
                manager.LogPing(p.GetTotalSeconds() - 0.032f);
            }
        }
        else if (message is NMWrongVersion)
        {
            _theirVersion = (message as NMWrongVersion).version;
            Disconnect_DifferentVersion(_theirVersion);
        }
        else if (message is NMModIncompatibility)
        {
            Disconnect_IncompatibleMods();
        }
    }

    public void TerminateConnection()
    {
        ChangeStatus(ConnectionStatus.Disconnected);
    }

    public void BeginDisconnecting(DuckNetErrorInfo error)
    {
        if (_status != ConnectionStatus.Disconnecting)
        {
            ChangeStatus(ConnectionStatus.Disconnecting);
            _disconnectsSent = 0;
            _ticksTillDisconnectAttempt = 0;
            _connectionError = error;
            Send.ImmediateUnreliableMessage(new NMDisconnect((_connectionError != null) ? _connectionError.error : DuckNetError.UnknownError), this);
            Send.ImmediateUnreliableMessage(new NMDisconnect((_connectionError != null) ? _connectionError.error : DuckNetError.UnknownError), this);
            Send.ImmediateUnreliableMessage(new NMDisconnect((_connectionError != null) ? _connectionError.error : DuckNetError.UnknownError), this);
        }
    }

    public void Update()
    {
        if (failureNotificationCooldown > 0)
        {
            failureNotificationCooldown--;
        }
        _pongedThisFrame = false;
        if (_debuggerContext != null)
        {
            _debuggerContext.Update(Network.activeNetwork.core);
        }
        if (_status == ConnectionStatus.Disconnected)
        {
            return;
        }
        if (_status == ConnectionStatus.Disconnecting)
        {
            if (_data == null || _disconnectsSent > 10)
            {
                ChangeStatus(ConnectionStatus.Disconnected);
                _connectionError = null;
                return;
            }
            _ticksTillDisconnectAttempt--;
            if (_ticksTillDisconnectAttempt <= 0)
            {
                if (!kicking)
                {
                    Send.Message(new NMDisconnect((_connectionError != null) ? _connectionError.error : DuckNetError.UnknownError), NetMessagePriority.UnreliableUnordered, this);
                    DevConsole.Log(DCSection.Connection, "Disconnect send    (" + sessionID + ")", this);
                }
                _disconnectsSent++;
                _ticksTillDisconnectAttempt = 4;
            }
            return;
        }
        _currentSessionTicks++;
        if (_status == ConnectionStatus.Connecting)
        {
            if (Maths.TicksToSeconds(_currentSessionTicks) > 0f && _numErrorLogs == 0)
            {
                _numErrorLogs++;
                LogSessionDetails();
            }
            if (Maths.TicksToSeconds(_currentSessionTicks) > 5f && _numErrorLogs == 1)
            {
                _numErrorLogs++;
                LogSessionDetails();
            }
            if (Maths.TicksToSeconds(_currentSessionTicks) > 8f && _numErrorLogs == 2)
            {
                _numErrorLogs++;
                LogSessionDetails();
            }
            if (Maths.TicksToSeconds(_currentSessionTicks) > 10f && _numErrorLogs == 3)
            {
                _numErrorLogs++;
                LogSessionDetails();
            }
            if (Maths.TicksToSeconds(_currentSessionTicks) > 15f && _numErrorLogs == 4)
            {
                _numErrorLogs++;
                LogSessionDetails();
            }
            float waitTime = 18f;
            if (NetworkDebugger.enabled)
            {
                waitTime = 8f;
            }
            if (Maths.TicksToSeconds(_currentSessionTicks) > waitTime && !MonoMain.noConnectionTimeout)
            {
                Disconnect_ConnectionTimeout();
            }
        }
        if (_data == null)
        {
            return;
        }
        lock (acksReceived)
        {
            _manager.DoAcks(acksReceived);
        }
        _manager.Update();
        if (status == ConnectionStatus.Connecting)
        {
            if (pingWait <= 0)
            {
                Send.Message(new NMConnect(_connectsReceived, 0, DG.version, ModLoader.modHash), NetMessagePriority.UnreliableUnordered, this);
                DevConsole.Log(DCSection.Connection, "Connect send    (" + sessionID + ")", this);
                pingWait = 20;
            }
            pingWait--;
        }
        else
        {
            if (pingWait > timeBetweenPings)
            {
                restartPingTimer = true;
                NMNewPing ping = null;
                ping = ((!Network.isServer) ? new NMNewPing(pingIndex) : new NMNewPingHost(pingIndex));
                _pingsSent++;
                _pings[pingIndex] = ping;
                pingIndex = (byte)((pingIndex + 1) % 10);
                Send.Message(ping, NetMessagePriority.UnreliableUnordered, this);
                pingWait = 0;
                sendPacketsNow = true;
            }
            pingWait++;
        }
        _personalTick++;
        _estimatedClientTick++;
        if (_status != ConnectionStatus.Connecting && _status != ConnectionStatus.Disconnecting)
        {
            if (_status == ConnectionStatus.Connected && receiveGap > 960)
            {
                DevConsole.Log(DCSection.Connection, "|DGRED|Connection timeout with " + ToString());
                Network.activeNetwork.core.DisconnectClient(this, new DuckNetErrorInfo(DuckNetError.ConnectionLost, "Connection was lost."));
            }
            _previousReceiveGap = receiveGap;
        }
    }

    private void LogSessionDetails()
    {
        if (data is User)
        {
            SessionState state = Steam.GetSessionState(data as User);
            DevConsole.Log("Information for " + ToString() + ":", Colors.DGBlue);
            FieldInfo[] fields = state.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo f in fields)
            {
                DevConsole.Log(f.Name + ": " + f.GetValue(state).ToString(), Colors.DGBlue);
            }
            DevConsole.Log("", Colors.DGBlue);
        }
    }

    public void PostUpdate(int frameCounter)
    {
        ghostLerpDivisor = 1f / (float)packetsEvery;
        if (_status == ConnectionStatus.Disconnected)
        {
            return;
        }
        bool sendPackets = (frameCounter + connectionLoopIndex) % packetsEvery == 0;
        if (DuckNetwork.levelIndex == levelIndex && levelIndex != byte.MaxValue && status == ConnectionStatus.Connected)
        {
            foreach (Profile pro in DuckNetwork.profiles)
            {
                if (pro.connection == DuckNetwork.localConnection && pro.netData.IsDirty(this))
                {
                    Send.Message(new NMProfileNetData(pro, this), NetMessagePriority.Volatile, this);
                    pro.netData.Clean(this);
                }
            }
            GhostManager.context.Update(this, sendPackets);
        }
        NetSoundEffect.Update();
        _manager.Flush(sendPackets);
        if (sendPackets && !_sentThisFrame)
        {
            Network.activeNetwork.core.SendPacket(null, this);
        }
        _sentThisFrame = false;
    }
}
