using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DuckGame;

public abstract class NCNetworkImplementation
{
    public Network _core;

    private DataLayer _dataLayer;

    public HashSet<object> connectionWalls = new HashSet<object>();

    public bool _connectionsDirty = true;

    private List<NetworkConnection> _connectionsInternal;

    private List<NetworkConnection> _connectionsInternalAll;

    public bool firstPrediction = true;

    private List<NetworkConnection> _connectionHistory = new List<NetworkConnection>();

    private Queue<NetworkPacket> _pendingPackets = new Queue<NetworkPacket>();

    private Thread _networkThread;

    private Thread _timeThread;

    private bool _isServer = true;

    private bool _isServerP2P = true;

    protected Queue<NCError> _pendingMessages = new Queue<NCError>();

    protected Lobby _lobby;

    private GhostManager _ghostManager;

    private NetGraph _netGraph = new NetGraph();

    protected int _networkIndex;

    private volatile bool _killThread;

    private volatile Queue<NCError> _threadPendingMessages = new Queue<NCError>();

    private int _hardDisconnectTimeout = -1;

    public static DuckNetErrorInfo currentError;

    public static DuckNetErrorInfo currentMainDisconnectError;

    private bool _discardMessagePlayed;

    private const int kCompressionTag = 696143206;

    private int _packetHeat;

    private int _shownHeatMessage;

    public int frame;

    public DataLayer dataLayer => _dataLayer;

    public List<NetworkConnection> connections
    {
        get
        {
            if (_connectionsDirty)
            {
                RefreshConnections();
            }
            return _connectionsInternal;
        }
    }

    public List<NetworkConnection> allConnections
    {
        get
        {
            if (_connectionsDirty)
            {
                RefreshConnections();
            }
            return _connectionsInternalAll;
        }
    }

    public List<NetworkConnection> sessionConnections
    {
        get
        {
            List<NetworkConnection> c = new List<NetworkConnection>(connections);
            if (DuckNetwork.localConnection.status != ConnectionStatus.Disconnected)
            {
                c.Add(DuckNetwork.localConnection);
            }
            return c;
        }
    }

    public bool isServer
    {
        get
        {
            return _isServer;
        }
        set
        {
            _isServer = value;
        }
    }

    public bool isServerP2P
    {
        get
        {
            return _isServerP2P;
        }
        set
        {
            _isServerP2P = value;
        }
    }

    public bool isActive => _networkThread != null;

    public Lobby lobby => _lobby;

    public GhostManager ghostManager => _ghostManager;

    public NetGraph netGraph => _netGraph;

    public float averagePing
    {
        get
        {
            if (connections.Count == 0)
            {
                return 0f;
            }
            float val = 0f;
            List<NetworkConnection> cons = connections;
            foreach (NetworkConnection c in cons)
            {
                val += c.manager.ping;
            }
            return val / (float)cons.Count;
        }
    }

    public float averagePingPeak
    {
        get
        {
            if (connections.Count == 0)
            {
                return 0f;
            }
            float val = 0f;
            List<NetworkConnection> cons = connections;
            foreach (NetworkConnection c in cons)
            {
                val += c.manager.pingPeak;
            }
            return val / (float)cons.Count;
        }
    }

    public float averageJitter
    {
        get
        {
            float val = 0f;
            List<NetworkConnection> cons = connections;
            foreach (NetworkConnection c in cons)
            {
                val += c.manager.jitter;
            }
            return val / (float)cons.Count;
        }
    }

    public float averageJitterPeak
    {
        get
        {
            float val = 0f;
            List<NetworkConnection> cons = connections;
            foreach (NetworkConnection c in cons)
            {
                val += c.manager.jitterPeak;
            }
            return val / (float)cons.Count;
        }
    }

    public int averagePacketLoss
    {
        get
        {
            if (connections.Count == 0)
            {
                return 0;
            }
            int losses = 0;
            List<NetworkConnection> cons = connections;
            foreach (NetworkConnection c in cons)
            {
                losses += c.manager.losses;
            }
            return losses / cons.Count;
        }
    }

    public int averagePacketLossPercent
    {
        get
        {
            if (connections.Count == 0)
            {
                return 0;
            }
            int losses = 0;
            int sent = 0;
            List<NetworkConnection> cons = connections;
            foreach (NetworkConnection c in cons)
            {
                losses += c.manager.losses;
                sent += c.manager.sent;
            }
            if (losses == 0)
            {
                return 0;
            }
            losses /= cons.Count;
            sent /= cons.Count;
            return (int)Math.Round((float)losses / (float)sent * 100f);
        }
    }

    private void RefreshConnections()
    {
        lock (_connectionHistory)
        {
            _connectionsInternal = new List<NetworkConnection>();
            _connectionsInternalAll = new List<NetworkConnection>();
            foreach (NetworkConnection c in _connectionHistory)
            {
                if (c.status != ConnectionStatus.Disconnected)
                {
                    _connectionsInternal.Add(c);
                }
                _connectionsInternalAll.Add(c);
            }
        }
        _connectionsDirty = false;
    }

    public NCNetworkImplementation(Network core, int networkIndex)
    {
        _core = core;
        _networkIndex = networkIndex;
        _ghostManager = new GhostManager();
        if (NetworkDebugger.enabled)
        {
            _dataLayer = new DataLayerDebug(this);
        }
        else
        {
            _dataLayer = new DataLayer(this);
        }
    }

    public void HostServer(string identifier, int port, NetworkLobbyType lobbyType, int maxConnections)
    {
        _pendingPackets.Clear();
        _dataLayer.Reset();
        NCError e = OnHostServer(identifier, port, lobbyType, maxConnections);
        if (e != null)
        {
            DevConsole.Log(e.text, e.color, 2f, _networkIndex);
        }
    }

    public abstract NCError OnHostServer(string identifier, int port, NetworkLobbyType lobbyType, int maxConnections);

    public void JoinServer(string identifier, int port, string ip)
    {
        _pendingPackets.Clear();
        _dataLayer.Reset();
        NCError e = OnJoinServer(identifier, port, ip);
        if (e != null)
        {
            DevConsole.Log(e.text, e.color, 2f, _networkIndex);
        }
    }

    public abstract NCError OnJoinServer(string identifier, int port, string ip);

    protected void StartServerThread()
    {
        _hardDisconnectTimeout = -1;
        _isServer = true;
        _isServerP2P = true;
        _killThread = false;
        _networkThread = new Thread(SpinThread);
        _networkThread.CurrentCulture = CultureInfo.InvariantCulture;
        _networkThread.Priority = ThreadPriority.Normal;
        _networkThread.IsBackground = true;
        _networkThread.Start();
        Network.MakeActive();
    }

    protected void StartClientThread()
    {
        _hardDisconnectTimeout = -1;
        _isServer = false;
        _isServerP2P = false;
        _killThread = false;
        _networkThread = new Thread(SpinThread);
        _networkThread.CurrentCulture = CultureInfo.InvariantCulture;
        _networkThread.Priority = ThreadPriority.Normal;
        _networkThread.IsBackground = true;
        _networkThread.Start();
        Network.MakeActive();
    }

    private static uint SwapEndianness(ulong x)
    {
        return (uint)(((x & 0xFF) << 24) + ((x & 0xFF00) << 8) + ((x & 0xFF0000) >> 8) + ((x & 0xFF000000u) >> 24));
    }

    public void Thread_Loop()
    {
        NCError error = null;
        error = ((!_isServer) ? OnSpinClientThread() : OnSpinServerThread());
        lock (_threadPendingMessages)
        {
            foreach (NCError e in _pendingMessages)
            {
                _threadPendingMessages.Enqueue(e);
            }
            _pendingMessages.Clear();
        }
        if (error != null)
        {
            DevConsole.Log(error.text, error.color, 2f, _networkIndex);
            _ = error.type;
            _ = 4;
        }
    }

    protected void SpinThread()
    {
        while (!_killThread)
        {
            Thread.Sleep(8);
            Thread_Loop();
        }
        _pendingMessages.Enqueue(new NCError("|DGBLUE|NETCORE |DGRED|Network thread ended", NCErrorType.Debug));
        _killThread = false;
    }

    protected abstract NCError OnSpinServerThread();

    protected abstract NCError OnSpinClientThread();

    public NetworkConnection AttemptConnection(object context, bool host = false)
    {
        if (context is string)
        {
            string connectionID = (string)context;
            context = GetConnectionObject((string)context);
            if (context == null)
            {
                DevConsole.Log(DCSection.NetCore, "@errorConnection attempt with" + connectionID + "failed (INVALID)@error", _networkIndex);
                return null;
            }
        }
        if (context == null)
        {
            DevConsole.Log(DCSection.NetCore, "@error Null connection attempt, this shouldn't happen!@error", _networkIndex);
            return null;
        }
        if (DuckNetwork.localConnection.status != ConnectionStatus.Connected)
        {
            DevConsole.Log(DCSection.NetCore, "@error Connection ignored due to full disconnect in progress.@error");
            return null;
        }
        NetworkConnection connection = GetOrAddConnection(context);
        if (connection.status != ConnectionStatus.Connected && !connection.banned && Network.isServer && connection.data is User && Options.Data.blockedPlayers.Contains((connection.data as User).id))
        {
            DevConsole.Log(DCSection.NetCore, "@error Ignoring connection from " + connection.ToString() + "(blocked)@error");
            connection.banned = true;
        }
        if (connection.banned)
        {
            DevConsole.Log(DCSection.NetCore, "@error Connection ignored due to ban.@error");
            return null;
        }
        connection.recentlyReceivedPackets.Clear();
        connection.recentlyReceivedPacketsArray = new ushort[NetworkConnection.kMaxRecentlyReceivedPackets];
        connection.recentlyReceivedPacketsArrayIndex = 0;
        if (connection.status != ConnectionStatus.Disconnected && connection.status != ConnectionStatus.Disconnecting)
        {
            DevConsole.Log(DCSection.NetCore, "@error Connection attempt skipped (Already Connected)@error", connection, _networkIndex);
            return connection;
        }
        connection.Reset("Connection Attempt");
        connection.isHost = host;
        connection.BeginConnection();
        connection.StartNewSession();
        if (host)
        {
            DevConsole.Log(DCSection.NetCore, "Attempting connection to |DGGREEN|Host(" + connection.ToString() + ")", connection, _networkIndex);
        }
        else
        {
            DevConsole.Log(DCSection.NetCore, "Attempting connection to |DGYELLOW|Client(" + connection.ToString() + ")", connection, _networkIndex);
        }
        return connection;
    }

    protected virtual object GetConnectionObject(string identifier)
    {
        return null;
    }

    protected void OnAttemptConnection(object context)
    {
    }

    public void SendPeerInfo(object context)
    {
        OnSendPeerInfo(context);
    }

    public virtual NCError OnSendPeerInfo(object context)
    {
        return null;
    }

    public void PushNewConnection(NetworkConnection c)
    {
        lock (_connectionHistory)
        {
            _connectionHistory.Add(c);
            _connectionsDirty = true;
        }
    }

    protected NetworkConnection GetOrAddConnection(object context)
    {
        string id = GetConnectionIdentifier(context);
        NetworkConnection connection = allConnections.FirstOrDefault((NetworkConnection x) => x.identifier == id);
        if (connection == null)
        {
            connection = new NetworkConnection(context);
            PushNewConnection(connection);
        }
        return connection;
    }

    protected NetworkConnection GetConnection(object context)
    {
        string id = GetConnectionIdentifier(context);
        return allConnections.FirstOrDefault((NetworkConnection x) => x.identifier == id);
    }

    public void DisconnectClient(NetworkConnection connection, DuckNetErrorInfo error, bool kicked = false)
    {
        if (connection == null)
        {
            return;
        }
        string reason = "|LIME|You have disconnected.";
        if (error != null)
        {
            string[] parts = error.message.Split('\n');
            if (parts.Length != 0)
            {
                reason = parts[0];
            }
        }
        if (connection.status != ConnectionStatus.Disconnected)
        {
            if (connection.status == ConnectionStatus.Disconnecting)
            {
                return;
            }
            currentError = error;
            connection.BeginDisconnecting(error);
            if (Level.current != null)
            {
                Level.current.OnDisconnect(connection);
            }
            DuckNetwork.OnDisconnect(connection, reason, kicked || (error != null && (error.error == DuckNetError.Kicked || error.error == DuckNetError.Banned)));
            currentError = null;
            if (connection.data == null)
            {
                DevConsole.Log(DCSection.NetCore, "@disconnect You (LOCAL) are disconnecting...|DGRED|(" + reason + ")", _networkIndex);
            }
            else
            {
                DevConsole.Log(DCSection.NetCore, "@disconnect Player Disconnecting...|DGRED|(" + reason + ")", connection, _networkIndex);
            }
            if (connection == DuckNetwork.localConnection)
            {
                currentMainDisconnectError = error;
                foreach (NetworkConnection c in sessionConnections)
                {
                    if (c != connection)
                    {
                        DisconnectClient(c, error);
                    }
                }
                _hardDisconnectTimeout = 240;
            }
            if (connection.profile != null)
            {
                connection.profile.networkStatus = DuckNetStatus.Disconnecting;
            }
            return;
        }
        if (connection.data == null)
        {
            DevConsole.Log(DCSection.NetCore, "@disconnect You (LOCAL) have disconnected.|DGRED|(" + reason + ")(" + connection.sessionID + ")", _networkIndex);
        }
        else
        {
            DevConsole.Log(DCSection.NetCore, "@disconnect Player Disconnected!|DGRED|(" + reason + ")(" + connection.sessionID + ")", connection, _networkIndex);
        }
        if (connection.profile != null)
        {
            connection.profile.networkStatus = DuckNetStatus.Disconnected;
        }
        if (GhostManager.context != null)
        {
            GhostManager.context.OnDisconnect(connection);
        }
        Disconnect(connection);
        connection.Reset("Client Disconnected.");
        if (currentMainDisconnectError != null && currentMainDisconnectError.error == DuckNetError.EveryoneDisconnected)
        {
            currentMainDisconnectError = error;
        }
        if (sessionConnections.Count == 0 || (sessionConnections.Count == 1 && sessionConnections[0] == DuckNetwork.localConnection && !Network.InLobby()))
        {
            if (!Network.isServer || sessionConnections.Count == 0)
            {
                OnSessionEnded((currentMainDisconnectError != null) ? currentMainDisconnectError : error);
            }
            else
            {
                DuckNetwork.TryPeacefulResolution();
            }
        }
    }

    protected virtual void Disconnect(NetworkConnection c)
    {
    }

    public void OnSessionEnded(DuckNetErrorInfo error)
    {
        _dataLayer.EndSession();
        if (_networkThread != null && _networkThread.IsAlive)
        {
            _killThread = true;
            int tries = 0;
            while (_killThread && tries < 20)
            {
                Thread.Sleep(100);
                tries++;
            }
        }
        if (_killThread)
        {
            _networkThread.Abort();
            if (_timeThread != null)
            {
                _timeThread.Abort();
            }
        }
        _networkThread = null;
        KillConnection();
        _pendingPackets.Clear();
        _pendingMessages.Clear();
        _ghostManager = new GhostManager();
        Network.activeNetwork.Reset();
        Network.isServer = true;
        Network.MakeInactive();
        DevConsole.Log(DCSection.NetCore, "@disconnect Session has ended (OnSessionEnded called, " + Network.activeNetwork.core.connections.Count + " connections)", _networkIndex);
        DuckNetwork.OnSessionEnded();
        if (Level.current != null)
        {
            Level.current.OnSessionEnded(error);
        }
        else if (error != null)
        {
            Level.current = new ConnectionError(error.message);
        }
        else
        {
            Level.current = new ConnectionError("|RED|Disconnected from game.");
        }
        if (UIMatchmakerMark2.instance != null)
        {
            UIMatchmakerMark2.instance.Hook_OnSessionEnded(error);
        }
        currentMainDisconnectError = null;
        _hardDisconnectTimeout = -1;
        lock (_connectionHistory)
        {
            _connectionHistory.Clear();
            _connectionsDirty = true;
        }
    }

    /// <summary>
    /// Begin searching for lobby games. (Abstracted during Switch port)
    /// </summary>
    public abstract void SearchForLobby();

    public abstract void AddLobbyStringFilter(string key, string value, LobbyFilterComparison op);

    public abstract void AddLobbyNumericalFilter(string key, int value, LobbyFilterComparison op);

    public abstract void ApplyTS2LobbyFilters();

    public abstract void RequestGlobalStats();

    public abstract int NumLobbiesFound();

    public abstract bool TryRequestDailyKills(out long numKills);

    public abstract Lobby GetSearchLobbyAtIndex(int i);

    public virtual void ApplyLobbyData()
    {
    }

    public void UpdateRandomID(Lobby l)
    {
        l.randomID = Rando.Int(2147483646);
        l.SetLobbyData("randomID", l.randomID.ToString());
    }

    /// <summary>
    /// Update the search for lobby games. (Abstracted during Switch port)
    /// </summary>
    /// <returns>True when lobby search has completed</returns>
    public abstract bool IsLobbySearchComplete();

    public void OnConnection(NetworkConnection connection)
    {
        DevConsole.Log(DCSection.NetCore, "|LIME|Connection established! (" + connection.sessionID + ")", connection, _networkIndex);
        DuckNetwork.OnConnection(connection);
    }

    protected void OnPacket(byte[] data, object context)
    {
        NetworkConnection connection = GetConnection(context);
        if (connection == null)
        {
            if (context != null && context is User)
            {
                _pendingMessages.Enqueue(new NCError("|DGBLUE|NETCORE |DGRED|Packet received from unknown connection(" + (context as User).id + "," + (context as User).name + ").", NCErrorType.Debug));
            }
            else
            {
                _pendingMessages.Enqueue(new NCError("|DGBLUE|NETCORE |DGRED|Packet received from unknown connection.", NCErrorType.Debug));
            }
            return;
        }
        if (connection.banned)
        {
            if (connection.failureNotificationCooldown <= 0)
            {
                if (Network.activeNetwork.core.lobby != null && connection.data is User)
                {
                    Steam_LobbyMessage.Send("COM_FAIL", connection.data as User);
                }
                _pendingMessages.Enqueue(new NCError("|DGBLUE|NETCORE |DGRED|Ignoring packet (banned)(" + connection.ToString() + ").", NCErrorType.Debug));
                connection.failureNotificationCooldown = 60;
            }
            return;
        }
        NetworkPacket p = new NetworkPacket(new BitBuffer(data), connection, 0);
        if (NetworkDebugger.enabled)
        {
            NetworkDebugger.LogReceive(NetworkDebugger.GetID(_networkIndex), p.connection.identifier);
        }
        try
        {
            p.sessionID = p.data.ReadUInt();
            ushort ack = p.data.ReadUShort();
            ushort ackField = p.data.ReadUShort();
            lock (p.connection.acksReceived)
            {
                p.connection.acksReceived.Add(ack);
                for (ushort i = 0; i < 16; i++)
                {
                    if ((ackField & p.connection.kAckOffsets[i]) != 0)
                    {
                        p.connection.acksReceived.Add((ushort)(ack - (i + 1)));
                    }
                }
            }
            bool connectionThinksItIsServer = p.data.ReadBool();
            p.serverPacket = connectionThinksItIsServer;
            if (!p.data.ReadBool())
            {
                return;
            }
            p.order = p.data.ReadUShort();
            p.valid = true;
            if (_pendingPackets.Count > 200)
            {
                _pendingMessages.Enqueue(new NCError("|DGRED|Discarding packets due to overflow..", NCErrorType.Debug));
                _discardMessagePlayed = true;
                return;
            }
            _discardMessagePlayed = false;
            lock (_pendingPackets)
            {
                _pendingPackets.Enqueue(p);
            }
        }
        catch (Exception ex)
        {
            _pendingMessages.Enqueue(new NCError("@error |DGRED|OnPacket exception:", NCErrorType.Debug));
            _pendingMessages.Enqueue(new NCError(ex.Message, NCErrorType.Debug));
        }
    }

    public void SendPacket(NetworkPacket packet, NetworkConnection connection)
    {
        if (connection == null)
        {
            DevConsole.Log(DCSection.NetCore, "|DGRED|Trying to send packet with no connection.", _core.networkIndex);
            return;
        }
        if (NetworkDebugger.enabled)
        {
            NetworkDebugger.LogSend(NetworkDebugger.GetID(_networkIndex), connection.identifier);
        }
        UIMatchmakingBox.core.pulseLocal = true;
        BitBuffer sendData = new BitBuffer();
        sendData.Write(connection.sessionID);
        sendData.Write(connection.GetAck());
        sendData.Write(connection.GetAckBitfield());
        if (Network.isServer)
        {
            sendData.Write(val: true);
        }
        else
        {
            sendData.Write(val: false);
        }
        if (packet != null)
        {
            sendData.Write(val: true);
            sendData.Write(packet.order);
            sendData.Write(packet.data, writeLength: false);
            sendData.Write(val: true);
            sendData.Write(Network.synchronizedTime);
        }
        else
        {
            sendData.Write(val: false);
        }
        connection.sentThisFrame = true;
        NCError message = _dataLayer.SendPacket(sendData, connection);
        connection.PacketSent();
        if (message != null)
        {
            DevConsole.Log(message.text, message.color, 2f, _core.networkIndex);
        }
    }

    public abstract NCError OnSendPacket(byte[] data, int length, object connection);

    public abstract string GetConnectionIdentifier(object connection);

    public abstract string GetConnectionName(object connection);

    public string GetLocalName()
    {
        string loc = OnGetLocalName();
        if (loc.Length > 18)
        {
            loc = loc.Substring(0, 18);
        }
        return loc;
    }

    protected virtual string OnGetLocalName()
    {
        return "";
    }

    public virtual void Update()
    {
        if (_hardDisconnectTimeout > 0)
        {
            _hardDisconnectTimeout--;
            if (_hardDisconnectTimeout == 0)
            {
                _hardDisconnectTimeout = -1;
                Network.Terminate();
                return;
            }
        }
        _dataLayer.Update();
        lock (_threadPendingMessages)
        {
            foreach (NCError e in _threadPendingMessages)
            {
                DevConsole.Log(e.text, e.color, 2f, _networkIndex);
            }
            _threadPendingMessages.Clear();
        }
        List<NetworkPacket> threadPackets = null;
        lock (_pendingPackets)
        {
            threadPackets = new List<NetworkPacket>(_pendingPackets);
            _pendingPackets.Clear();
        }
        if (threadPackets.Count > 1)
        {
            for (int i = 1; i < threadPackets.Count; i++)
            {
                NetworkPacket p1 = threadPackets[i - 1];
                NetworkPacket p2 = threadPackets[i];
                if (p2.order < p1.order)
                {
                    threadPackets[i] = p1;
                    threadPackets[i - 1] = p2;
                    i = 0;
                }
            }
        }
        foreach (NetworkPacket packet in threadPackets)
        {
            if (packet.valid)
            {
                NetworkConnection.context = packet.connection;
                if (packet.connection.banned)
                {
                    DevConsole.Log(DCSection.NetCore, "|DGRED|Ignoring message received from BANNED client(" + packet.connection.ToString() + ").");
                    continue;
                }
                if (packet.serverPacket && !packet.connection.isHost)
                {
                    DevConsole.Log(DCSection.NetCore, "|DGRED|Ignoring message from invalid host (host migration)(" + packet.connection.ToString() + ").");
                    continue;
                }
                try
                {
                    packet.Unpack();
                }
                catch (Exception)
                {
                    DevConsole.Log(DCSection.NetCore, "|DGRED|Message unpack failure, possible corruption");
                    Program.LogLine("Message unpack failure, possible corruption.");
                    continue;
                }
                List<NetMessage> allMessages = packet.GetAllMessages();
                _packetHeat += allMessages.Count();
                foreach (NetMessage m in allMessages)
                {
                    packet.connection.OnAnyMessage(m);
                }
                if (packet.connection.status == ConnectionStatus.Disconnecting || packet.connection.status == ConnectionStatus.Disconnected)
                {
                    DevConsole.Log(DCSection.NetCore, "Dropping packet from client (" + packet.connection.status.ToString() + ")");
                    packet.dropPacket = true;
                }
                if (packet.IsValidSession())
                {
                    if (!packet.dropPacket)
                    {
                        packet.connection.PacketReceived(packet);
                        packet.connection.manager.MessagesReceived(packet.GetAllMessages());
                    }
                }
                else if (packet.connection.status != ConnectionStatus.Connecting)
                {
                    DevConsole.Log(DCSection.NetCore, "Dropping packet, invalid session (" + packet.sessionID + " vs " + packet.connection.sessionID + ")");
                }
                NetworkConnection.context = null;
            }
            else
            {
                DevConsole.Log(DCSection.DuckNet, "|DGRED|Ignoring data from " + packet.connection.name + " due to packet.valid flag");
            }
        }
        _ghostManager.PreUpdate();
        using (List<NetworkConnection>.Enumerator enumerator4 = sessionConnections.GetEnumerator())
        {
            while (enumerator4.MoveNext())
            {
                (NetworkConnection.context = enumerator4.Current).Update();
                NetworkConnection.context = null;
            }
        }
        _ghostManager.UpdateSynchronizedEvents();
    }

    public void PostUpdate()
    {
        if (!Network.isActive)
        {
            return;
        }
        List<NetworkConnection> connectionList = allConnections;
        if (DuckNetwork.localProfile != null && Level.current != null && Level.current.levelIsUpdating && Level.current.transferCompleteCalled)
        {
            _ghostManager.UpdateInit();
            _ghostManager.UpdateGhostLerp();
            _ghostManager.RefreshGhosts();
        }
        frame++;
        int idx = 0;
        for (int p = 0; p < connectionList.Count; p++)
        {
            NetworkConnection.connectionLoopIndex = idx;
            NetworkConnection c = (NetworkConnection.context = connectionList[p]);
            if (c.logTransferSize != 0)
            {
                ConnectionStatusUI.core.tempShow = 2;
            }
            if (c.profile == null || !c.banned)
            {
                c.PostUpdate(frame);
            }
            NetworkConnection.context = null;
            idx++;
        }
        if (DuckNetwork.localProfile != null)
        {
            _ghostManager.PostUpdate();
        }
    }

    public void PostDraw()
    {
        if (Network.isActive)
        {
            _ghostManager.PostDraw();
        }
    }

    protected virtual void KillConnection()
    {
    }

    public void ForcefulTermination()
    {
        OnSessionEnded(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Forceful network termination."));
        lock (_connectionHistory)
        {
            _connectionHistory.Clear();
            _connectionsDirty = true;
        }
    }

    public virtual void Terminate()
    {
        _hardDisconnectTimeout = -1;
        DevConsole.Log(DCSection.NetCore, "|DGRED|@error -----------Beginning Hard Network Termimation-----------@error");
        Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Network was forcefully terminated."));
        foreach (NetworkConnection allConnection in Network.activeNetwork.core.allConnections)
        {
            allConnection.HardTerminate();
        }
        for (int waiter = 0; waiter < 60; waiter++)
        {
            if (_networkThread == null)
            {
                break;
            }
            if (!_networkThread.IsAlive)
            {
                break;
            }
            Network.PreUpdate();
            Network.PostUpdate();
            Thread.Sleep(16);
        }
        if (_networkThread != null && _networkThread.IsAlive)
        {
            _networkThread.Abort();
            _networkThread = null;
        }
        DevConsole.Log(DCSection.NetCore, "|DGRED|@error -----------Network termination Complete-----------@error");
    }
}
