using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

#if FACEPUNCH
using Steamworks;
#else
using Steam;
#endif

namespace DuckGame;

public class Network
{
    #region Public Fields

    public static bool lanMode;

    public static uint messageTypeHash;

    public static List<string> synchronizedTriggers =
    [
        "LEFT", "RIGHT", "UP", "DOWN", "SHOOT", "JUMP", "GRAB", "QUACK", "START", "RAGDOLL", "STRAFE"
    ];

    public bool _networkActive;

    public NetIndex16 _lastReceivedTime = new(1, zeroSpecial: true);
    public NetIndex16 _synchronizedTime = new(1, zeroSpecial: true);

    #endregion

    #region Private Fields

    static int _simTick;
    static int _inputDelayFrames;

    static Network _activeNetwork;

    static Map<ushort, ConstructorInfo> _constructorToMessageID = [];
    static Map<ushort, Type> _typeToMessageID = new Map<ushort, Type>();

    static Dictionary<Type, ushort> _allMessageTypesToID = new Dictionary<Type, ushort>();

    static IEnumerable<NetMessagePriority> _netMessagePriorities;

    int _networkIndex;

    uint _currentTick;

    NetIndex16 _tickSync = new(1, zeroSpecial: true);

    NCNetworkImplementation _core;
    NCNetworkImplementation _lanCore;

    #endregion

    #region Public Properties

    public static bool SimulateBadnet => false;
    public static bool available
    {
        get
        {
#if FACEPUNCH
            if (SteamClient.IsValid)
                return SteamClient.SteamId != 0;
#else
            if (DGSteam.IsInitialized())
                return DGSteam.User != null;
#endif
            return false;
        }
    }
    public static bool hasHostConnection
    {
        get
        {
            if (DuckNetwork.localConnection.isHost)
                return true;

            foreach (NetworkConnection connection in connections)
            {
                if (connection.isHost)
                    return true;
            }

            return false;
        }
    }
    public static bool canSetObservers
    {
        get
        {
            if (DuckNetwork.isDedicatedServer)
                return false;

            bool can = isServer && lanMode;

#if FACEPUNCH
            if (isServer && FacepunchSteam.Lobby != null && FacepunchSteam.Lobby.Visibility != LobbyVisibility.Public)
#else
            if (isServer && DGSteam.Lobby != null && DGSteam.Lobby.Type != SteamLobbyType.Public)
#endif
                can = true;

            if (!InLobby())
                can = false;

            return can;
        }
    }
    public static bool isServer
    {
        get => activeNetwork.core.isServer;
        set => activeNetwork.core.isServer = value;
    }
    public static bool isClient => !isServer;
    public static bool isActive => activeNetwork._networkActive;
    public static bool connected => connections.Count > 0;

    public static int simTick
    {
        get => _simTick;
        set => _simTick = value;
    }
    public static int frame
    {
        get => activeNetwork.core.frame;
        set => activeNetwork.core.frame = value;
    }
    public static int inputDelayFrames
    {
        get => _inputDelayFrames;
        set => _inputDelayFrames = value;
    }

    public static uint Tick => activeNetwork._currentTick;

    public static float ping => activeNetwork.core.averagePing;
    public static float highestPing
    {
        get
        {
            var p = 0F;
            foreach (NetworkConnection c in connections)
            {
                if (c.status == ConnectionStatus.Connected && c.manager.ping > p)
                    p = c.manager.ping;
            }
            return p;
        }
    }

    public static long gameDataHash => messageTypeHash + Editor.thingTypesHash;

    public static double Time => 0.0;

    public static NetIndex16 synchronizedTime => activeNetwork.synchTime;
    public static NetIndex16 TickSync => activeNetwork._tickSync;

    public static Network activeNetwork
    {
        get => _activeNetwork ??= new Network();
        set => _activeNetwork = value;
    }
    public static NetGraph netGraph => _activeNetwork.core.netGraph;
    public static NetworkConnection host
    {
        get
        {
            if (DuckNetwork.hostProfile != null && DuckNetwork.hostProfile.connection != null)
                return DuckNetwork.hostProfile.connection;

            if (DuckNetwork.localConnection.isHost)
                return DuckNetwork.localConnection;

            foreach (NetworkConnection c in connections)
            {
                if (c.isHost)
                    return c;
            }

            return null;
        }
    }

    public static List<NetworkConnection> connections => activeNetwork.core.connections;

    public static Map<ushort, ConstructorInfo> constructorToMessageID => _constructorToMessageID;
    public static Map<ushort, Type> typeToMessageID => _typeToMessageID;

    public static Dictionary<Type, ushort> allMessageTypesToID => _allMessageTypesToID;

    public static IEnumerable<NetMessagePriority> netMessagePriorities => _netMessagePriorities;

    public int networkIndex => _networkIndex;

    public NetIndex16 synchTime =>
        core.isServer ? _tickSync : _synchronizedTime;

    public NCNetworkImplementation core
    {
        get => lanMode ? _lanCore : _core;
        set
        {
            if (lanMode)
                _lanCore = value;
            else
                _core = value;
        }
    }

    #endregion

    public Network(int networkIndex = 0)
    {
        _networkIndex = networkIndex;
    }

    #region Public Methods

    public static void ContextSwitch(byte pLevelIndex)
    {
        GhostManager.context.Clear();
        DevConsole.Log(DCSection.GhostMan, $"|DGYELLOW|ContextSwitch ({DuckNetwork.levelIndex}->{pLevelIndex})");
        DuckNetwork.levelIndex = pLevelIndex;

        if (pLevelIndex == 0)
            GhostManager.context.ResetGhostIndex(pLevelIndex);

        foreach (Profile p in Profiles.active)
            p.connection?.manager.Reset();
    }

    public static void ReceiveHostTime(NetIndex16 pTime)
    {
        if (activeNetwork._lastReceivedTime < pTime)
        {
            NetIndex16 adjustedTime = pTime + (ushort)(host.manager.ping / 2 / Maths.IncFrameTimer());
            activeNetwork._synchronizedTime = adjustedTime;
            activeNetwork._lastReceivedTime = pTime;
        }
    }

    public static void MakeActive()
    {
        activeNetwork._networkActive = true;
    }

    public static void MakeInactive()
    {
        activeNetwork._networkActive = false;
    }

    public static void JoinServer(string nameVal, int portVal = 1337, string ip = "localhost")
    {
        activeNetwork.DoJoinServer(nameVal, portVal, ip);
    }

    public static void HostServer(NetworkLobbyType lobbyType, int maxConnectionsVal = 32, string nameVal = "duckGameServer", int portVal = 1337)
    {
        activeNetwork.DoHostServer(lobbyType, maxConnectionsVal, nameVal, portVal);
    }

    public static void OnMessageStatic(NetMessage m)
    {
        _activeNetwork.OnMessage(m);
    }

    public static bool InLobby()
    {
        return Level.current is TeamSelect2;
    }

    public static bool InGameLevel()
    {
        return Level.current is GameLevel;
    }

    public static bool InMatch()
    {
        if (Level.current is not GameLevel and RockScoreboard) // && Level.current is not RockScoreboard (idk if its the same thing or not)
            return Level.current is RockIntro;

        return true;
    }

    public static void EndNetworkingSession(DuckNetErrorInfo error)
    {
        activeNetwork.core.DisconnectClient(DuckNetwork.localConnection, error);
    }

    public static void DisconnectClient(NetworkConnection c, DuckNetErrorInfo error)
    {
        activeNetwork.core.DisconnectClient(c, error);
    }

    public static void Initialize()
    {
        _netMessagePriorities = Enum.GetValues<NetMessagePriority>();
        activeNetwork.DoInitialize();
    }

    public static void InitializeMessageTypes()
    {
        var messageTypes = Editor.GetSubclasses(typeof(NetMessage));

        _typeToMessageID.Clear();
        _constructorToMessageID.Clear();

        ushort index = 1;
        foreach (Type t in messageTypes)
        {
            if (t.GetCustomAttributes(typeof(FixedNetworkID), inherit: false).Length == 0)
                continue;

            var fixedIDAttribute = (FixedNetworkID)t.GetCustomAttributes(typeof(FixedNetworkID), inherit: false)[0];
            if (fixedIDAttribute != null)
            {
                var m = ModLoader.GetModFromTypeIgnoreCore(t);
                if (m != null && m is DisabledMod)
                {
                    m.typeToMessageID.Add(t, fixedIDAttribute.FixedID);
                    m.constructorToMessageID.Add(t.GetConstructor(Type.EmptyTypes), fixedIDAttribute.FixedID);
                }
                else
                {
                    _typeToMessageID.Add(t, fixedIDAttribute.FixedID);
                    _constructorToMessageID.Add(t.GetConstructor(Type.EmptyTypes), fixedIDAttribute.FixedID);
                }

                _allMessageTypesToID.Add(t, fixedIDAttribute.FixedID);
            }
        }

        var fullMessageTypeString = "";
        foreach (Type t2 in messageTypes)
        {
            if (_allMessageTypesToID.ContainsKey(t2))
                continue;

            var empty = t2.GetConstructor(Type.EmptyTypes);
            if (empty == null)
            {
                var error = $"NetMessage ({t2.Name}) has no empty constructor! All NetMessages must allow 'new {t2.Name}()'";
                if (MonoMain.modDebugging)
                {
                    Debugger.Break();
                    Program.crashAssembly = t2.Assembly;
                    throw new Exception(error);
                }
                DevConsole.Log(DCSection.General, $"|DGRED|{error}");
            }

            var m2 = ModLoader.GetModFromTypeIgnoreCore(t2);
            if (m2 != null && m2 is DisabledMod)
            {
                while (m2.typeToMessageID.ContainsKey(m2.currentMessageIDIndex))
                    m2.currentMessageIDIndex++;

                m2.typeToMessageID.Add(t2, m2.currentMessageIDIndex);
                m2.constructorToMessageID.Add(t2.GetConstructor(Type.EmptyTypes), m2.currentMessageIDIndex);
                _allMessageTypesToID.Add(t2, m2.currentMessageIDIndex);
                m2.currentMessageIDIndex++;
                continue;
            }

            while (_typeToMessageID.ContainsKey(index))
                index++;

            if (m2 == null)
                fullMessageTypeString += t2.Name;

            _typeToMessageID.Add(t2, index);
            _constructorToMessageID.Add(empty, index);
            _allMessageTypesToID.Add(t2, index);
            index++;
        }

        messageTypeHash = CRC32.Generate(fullMessageTypeString);
        PhysicsParticle.RegisterNetParticleType(typeof(SmallFire));
        PhysicsParticle.RegisterNetParticleType(typeof(ExtinguisherSmoke));
        PhysicsParticle.RegisterNetParticleType(typeof(Firecracker));
    }

    public static void Terminate()
    {
        activeNetwork.core.Terminate();
    }

    public static void PreUpdate()
    {
        activeNetwork._networkActive = activeNetwork.core.isActive;
        activeNetwork.DoPreUpdate();
    }

    public static void PostUpdate()
    {
        activeNetwork.DoPostUpdate();
    }

    public static void PostDraw()
    {
        activeNetwork.DoPostDraw();
    }

    public Type GetClassType(string name)
    {
        string fullyQualified = typeof(Duck).Assembly.FullName;
        fullyQualified = $"DuckGame.{name}, {fullyQualified}";
        return Editor.GetType(fullyQualified);
    }

    public void OnConnection(NetworkConnection connection)
    {
        UIMatchmakingBox.core.pulseNetwork = true;
    }

    public void ImmediateUnreliableBroadcast(NetMessage pMessage)
    {
        if (!isActive)
            return;

        pMessage.Serialize();

        var usedMessage = false;
        foreach (var connection in connections)
        {
            var m = pMessage;

            if (usedMessage)
            {
                m = Activator.CreateInstance(pMessage.GetType(), null) as NetMessage;
                m.priority = pMessage.priority;
                m.SetSerializedData(pMessage.serializedData);
            }

            connection.manager.SendImmediatelyUnreliable(m);
            usedMessage = true;
        }
    }

    public void ImmediateUnreliableMessage(NetMessage pMessage, NetworkConnection pConnection)
    {
        if (isActive)
        {
            pMessage.Serialize();
            pConnection.manager.SendImmediatelyUnreliable(pMessage);
        }
    }

    public void QueueMessage(NetMessage msg, NetworkConnection who = null)
    {
        if (isActive)
        {
            if (who == null)
            {
                QueueMessage(msg, connections);
                return;
            }

            msg.Serialize();
            who.manager.QueueMessage(msg);
        }
    }

    public void QueueMessage(NetMessage msg, List<NetworkConnection> pConnections)
    {
        if (!isActive)
            return;

        if (msg is SynchronizedNetMessage sync)
            GhostManager.context._synchronizedEvents.Add(sync);

        msg.Serialize();

        bool usedMessage = false;
        foreach (NetworkConnection c in pConnections)
        {
            if (c.profile != null)
            {
                var m = msg;
                if (usedMessage)
                {
                    m = Activator.CreateInstance(msg.GetType(), null) as NetMessage;
                    m.priority = msg.priority;
                    m.SetSerializedData(msg.serializedData);
                    msg.CopyTo(m);
                }
                c.manager.QueueMessage(m);
                usedMessage = true;
            }
        }
    }

    public void QueueMessageForAllBut(NetMessage msg, NetworkConnection who)
    {
        if (!isActive)
            return;

        msg.Serialize();

        bool usedMessage = false;
        foreach (NetworkConnection c in connections)
        {
            if (c.profile != null && who != c)
            {
                var m = msg;
                if (usedMessage)
                {
                    m = Activator.CreateInstance(msg.GetType(), null) as NetMessage;
                    m.priority = msg.priority;
                    m.SetSerializedData(msg.serializedData);
                }

                c.manager.QueueMessage(m);
                usedMessage = true;
            }
        }
    }

    public void QueueMessage(NetMessage msg, NetMessagePriority priority, NetworkConnection who = null)
    {
        msg.priority = priority;
        QueueMessage(msg, who);
    }

    public void QueueMessage(NetMessage msg, NetMessagePriority priority, List<NetworkConnection> pConnections)
    {
        msg.priority = priority;
        QueueMessage(msg, pConnections);
    }

    public void QueueMessageForAllBut(NetMessage msg, NetMessagePriority priority, NetworkConnection who)
    {
        msg.priority = priority;
        QueueMessageForAllBut(msg, who);
    }

    public void DoInitialize()
    {
        _core = new NCSteam(activeNetwork, _networkIndex);

        if (NetworkDebugger.enabled)
            _lanCore = new NCNetDebug(activeNetwork, _networkIndex);
        else
            _lanCore = new NCBasic(activeNetwork, _networkIndex);
    }

    public void Reset()
    {
        _currentTick = 0;
        _synchronizedTime = new NetIndex16(1, zeroSpecial: true);
        _tickSync = new NetIndex16(1, zeroSpecial: true);
    }

    public void DoPreUpdate()
    {
        _currentTick++;
        _synchronizedTime += 1;
        _tickSync += 1;
        core.Update();
        DuckNetwork.Update();
    }

    public void DoPostUpdate()
    {
        core.PostUpdate();
    }

    public void DoPostDraw()
    {
        core.PostDraw();
    }

    #endregion

    #region Private Methods

    void DoJoinServer(string nameVal, int portVal = 1337, string ip = "localhost")
    {
        core.JoinServer(nameVal, portVal, ip);
    }

    void DoHostServer(NetworkLobbyType lobbyType, int maxConnectionsVal = 32, string nameVal = "duckGameServer", int portVal = 1337)
    {
        core.HostServer(nameVal, portVal, lobbyType, maxConnectionsVal);
    }

    void OnMessage(NetMessage m)
    {
        if (m is NMConsoleMessage consoleMessage)
            DevConsole.Log(consoleMessage.message, Color.Lime);
        else if (isServer)
            OnMessageServer(m);
        else
            OnMessageClient(m);
    }

    void OnMessageServer(NetMessage m)
    {
        Level.current.OnMessage(m);
    }

    void OnMessageClient(NetMessage m)
    {
        Level.current.OnMessage(m);
    }

    #endregion
}