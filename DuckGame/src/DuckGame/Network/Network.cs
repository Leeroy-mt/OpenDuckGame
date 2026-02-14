using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DuckGame;

public class Network
{
    private int _networkIndex;

    private static int _simTick;

    private NCNetworkImplementation _core;

    private NCNetworkImplementation _lanCore;

    public static bool lanMode = false;

    private static Network _activeNetwork;

    private NetIndex16 _tickSync = new NetIndex16(1, zeroSpecial: true);

    public NetIndex16 _lastReceivedTime = new NetIndex16(1, zeroSpecial: true);

    public NetIndex16 _synchronizedTime = new NetIndex16(1, zeroSpecial: true);

    private uint _currentTick;

    private static Map<ushort, ConstructorInfo> _constructorToMessageID = new Map<ushort, ConstructorInfo>();

    private static Map<ushort, Type> _typeToMessageID = new Map<ushort, Type>();

    private static Dictionary<Type, ushort> _allMessageTypesToID = new Dictionary<Type, ushort>();

    private static IEnumerable<NetMessagePriority> _netMessagePriorities;

    public static List<string> synchronizedTriggers = new List<string>
    {
        "LEFT", "RIGHT", "UP", "DOWN", "SHOOT", "JUMP", "GRAB", "QUACK", "START", "RAGDOLL",
        "STRAFE"
    };

    private static int _inputDelayFrames = 0;

    public bool _networkActive;

    public static uint messageTypeHash;

    public static bool SimulateBadnet => false;

    public int networkIndex => _networkIndex;

    public static int simTick
    {
        get
        {
            return _simTick;
        }
        set
        {
            _simTick = value;
        }
    }

    public static bool available
    {
        get
        {
            if (Steam.IsInitialized())
            {
                return Steam.user != null;
            }
            return false;
        }
    }

    public NCNetworkImplementation core
    {
        get
        {
            if (!lanMode)
            {
                return _core;
            }
            return _lanCore;
        }
        set
        {
            if (lanMode)
            {
                _lanCore = value;
            }
            else
            {
                _core = value;
            }
        }
    }

    public static Network activeNetwork
    {
        get
        {
            if (_activeNetwork == null)
            {
                _activeNetwork = new Network();
            }
            return _activeNetwork;
        }
        set
        {
            _activeNetwork = value;
        }
    }

    public static int frame
    {
        get
        {
            return activeNetwork.core.frame;
        }
        set
        {
            activeNetwork.core.frame = value;
        }
    }

    public static NetGraph netGraph => _activeNetwork.core.netGraph;

    public NetIndex16 synchTime
    {
        get
        {
            if (!core.isServer)
            {
                return _synchronizedTime;
            }
            return _tickSync;
        }
    }

    public static NetIndex16 synchronizedTime => activeNetwork.synchTime;

    public static double Time => 0.0;

    public static uint Tick => activeNetwork._currentTick;

    public static NetIndex16 TickSync => activeNetwork._tickSync;

    public static float ping => activeNetwork.core.averagePing;

    public static float highestPing
    {
        get
        {
            float p = 0f;
            foreach (NetworkConnection c in connections)
            {
                if (c.status == ConnectionStatus.Connected && c.manager.ping > p)
                {
                    p = c.manager.ping;
                }
            }
            return p;
        }
    }

    public static NetworkConnection host
    {
        get
        {
            if (DuckNetwork.hostProfile != null && DuckNetwork.hostProfile.connection != null)
            {
                return DuckNetwork.hostProfile.connection;
            }
            if (DuckNetwork.localConnection.isHost)
            {
                return DuckNetwork.localConnection;
            }
            foreach (NetworkConnection c in connections)
            {
                if (c.isHost)
                {
                    return c;
                }
            }
            return null;
        }
    }

    public static Map<ushort, ConstructorInfo> constructorToMessageID => _constructorToMessageID;

    public static Map<ushort, Type> typeToMessageID => _typeToMessageID;

    public static Dictionary<Type, ushort> allMessageTypesToID => _allMessageTypesToID;

    public static IEnumerable<NetMessagePriority> netMessagePriorities => _netMessagePriorities;

    public static int inputDelayFrames
    {
        get
        {
            return _inputDelayFrames;
        }
        set
        {
            _inputDelayFrames = value;
        }
    }

    public static bool hasHostConnection
    {
        get
        {
            if (DuckNetwork.localConnection.isHost)
            {
                return true;
            }
            foreach (NetworkConnection connection in connections)
            {
                if (connection.isHost)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static bool canSetObservers
    {
        get
        {
            if (DuckNetwork.isDedicatedServer)
            {
                return false;
            }
            bool can = isServer && lanMode;
            if (isServer && Steam.lobby != null && Steam.lobby.type != SteamLobbyType.Public)
            {
                can = true;
            }
            if (!InLobby())
            {
                can = false;
            }
            return can;
        }
    }

    public static bool isServer
    {
        get
        {
            return activeNetwork.core.isServer;
        }
        set
        {
            activeNetwork.core.isServer = value;
        }
    }

    public static bool isClient => !isServer;

    public static bool isActive => activeNetwork._networkActive;

    public static bool connected => connections.Count > 0;

    public static List<NetworkConnection> connections => activeNetwork.core.connections;

    public static long gameDataHash => messageTypeHash + Editor.thingTypesHash;

    public Network(int networkIndex = 0)
    {
        _networkIndex = networkIndex;
    }

    public static void ContextSwitch(byte pLevelIndex)
    {
        GhostManager.context.Clear();
        DevConsole.Log(DCSection.GhostMan, "|DGYELLOW|ContextSwitch (" + DuckNetwork.levelIndex + "->" + pLevelIndex + ")");
        DuckNetwork.levelIndex = pLevelIndex;
        if (pLevelIndex == 0)
        {
            GhostManager.context.ResetGhostIndex(pLevelIndex);
        }
        foreach (Profile p in Profiles.active)
        {
            if (p.connection != null)
            {
                p.connection.manager.Reset();
            }
        }
    }

    public static void ReceiveHostTime(NetIndex16 pTime)
    {
        if (activeNetwork._lastReceivedTime < pTime)
        {
            NetIndex16 adjustedTime = pTime + (ushort)(host.manager.ping / 2f / Maths.IncFrameTimer());
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

    public Type GetClassType(string name)
    {
        string fullyQualified = typeof(Duck).Assembly.FullName;
        fullyQualified = "DuckGame." + name + ", " + fullyQualified;
        return Editor.GetType(fullyQualified);
    }

    public static void JoinServer(string nameVal, int portVal = 1337, string ip = "localhost")
    {
        activeNetwork.DoJoinServer(nameVal, portVal, ip);
    }

    private void DoJoinServer(string nameVal, int portVal = 1337, string ip = "localhost")
    {
        core.JoinServer(nameVal, portVal, ip);
    }

    public static void HostServer(NetworkLobbyType lobbyType, int maxConnectionsVal = 32, string nameVal = "duckGameServer", int portVal = 1337)
    {
        activeNetwork.DoHostServer(lobbyType, maxConnectionsVal, nameVal, portVal);
    }

    private void DoHostServer(NetworkLobbyType lobbyType, int maxConnectionsVal = 32, string nameVal = "duckGameServer", int portVal = 1337)
    {
        core.HostServer(nameVal, portVal, lobbyType, maxConnectionsVal);
    }

    public static void OnMessageStatic(NetMessage m)
    {
        _activeNetwork.OnMessage(m);
    }

    private void OnMessage(NetMessage m)
    {
        if (m is NMConsoleMessage)
        {
            DevConsole.Log((m as NMConsoleMessage).message, Color.Lime);
        }
        else if (isServer)
        {
            OnMessageServer(m);
        }
        else
        {
            OnMessageClient(m);
        }
    }

    private void OnMessageServer(NetMessage m)
    {
        Level.current.OnMessage(m);
    }

    private void OnMessageClient(NetMessage m)
    {
        Level.current.OnMessage(m);
    }

    public void OnConnection(NetworkConnection connection)
    {
        UIMatchmakingBox.core.pulseNetwork = true;
    }

    public void ImmediateUnreliableBroadcast(NetMessage pMessage)
    {
        if (!isActive)
        {
            return;
        }
        pMessage.Serialize();
        bool usedMessage = false;
        foreach (NetworkConnection connection in connections)
        {
            NetMessage m = pMessage;
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
        {
            return;
        }
        if (msg is SynchronizedNetMessage sync)
        {
            GhostManager.context._synchronizedEvents.Add(sync);
        }
        msg.Serialize();
        bool usedMessage = false;
        foreach (NetworkConnection c in pConnections)
        {
            if (c.profile != null)
            {
                NetMessage m = msg;
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
        {
            return;
        }
        msg.Serialize();
        bool usedMessage = false;
        foreach (NetworkConnection c in connections)
        {
            if (c.profile != null && who != c)
            {
                NetMessage m = msg;
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
        if (!(Level.current is GameLevel) && !(Level.current is RockScoreboard))
        {
            return Level.current is RockIntro;
        }
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
        _netMessagePriorities = Enum.GetValues(typeof(NetMessagePriority)).Cast<NetMessagePriority>();
        activeNetwork.DoInitialize();
    }

    public static void InitializeMessageTypes()
    {
        IEnumerable<Type> messageTypes = Editor.GetSubclasses(typeof(NetMessage));
        _typeToMessageID.Clear();
        _constructorToMessageID.Clear();
        ushort index = 1;
        foreach (Type t in messageTypes)
        {
            if (t.GetCustomAttributes(typeof(FixedNetworkID), inherit: false).Length == 0)
            {
                continue;
            }
            FixedNetworkID fixedIDAttribute = (FixedNetworkID)t.GetCustomAttributes(typeof(FixedNetworkID), inherit: false)[0];
            if (fixedIDAttribute != null)
            {
                Mod m = ModLoader.GetModFromTypeIgnoreCore(t);
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
        string fullMessageTypeString = "";
        foreach (Type t2 in messageTypes)
        {
            if (_allMessageTypesToID.ContainsKey(t2))
            {
                continue;
            }
            ConstructorInfo empty = t2.GetConstructor(Type.EmptyTypes);
            if (empty == null)
            {
                string error = "NetMessage (" + t2.Name + ") has no empty constructor! All NetMessages must allow 'new " + t2.Name + "()'";
                if (MonoMain.modDebugging)
                {
                    Debugger.Break();
                    Program.crashAssembly = t2.Assembly;
                    throw new Exception(error);
                }
                DevConsole.Log(DCSection.General, "|DGRED|" + error);
            }
            Mod m2 = ModLoader.GetModFromTypeIgnoreCore(t2);
            if (m2 != null && m2 is DisabledMod)
            {
                while (m2.typeToMessageID.ContainsKey(m2.currentMessageIDIndex))
                {
                    m2.currentMessageIDIndex++;
                }
                m2.typeToMessageID.Add(t2, m2.currentMessageIDIndex);
                m2.constructorToMessageID.Add(t2.GetConstructor(Type.EmptyTypes), m2.currentMessageIDIndex);
                _allMessageTypesToID.Add(t2, m2.currentMessageIDIndex);
                m2.currentMessageIDIndex++;
                continue;
            }
            while (_typeToMessageID.ContainsKey(index))
            {
                index++;
            }
            if (m2 == null)
            {
                fullMessageTypeString += t2.Name;
            }
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

    public void DoInitialize()
    {
        _core = new NCSteam(activeNetwork, _networkIndex);
        if (NetworkDebugger.enabled)
        {
            _lanCore = new NCNetDebug(activeNetwork, _networkIndex);
        }
        else
        {
            _lanCore = new NCBasic(activeNetwork, _networkIndex);
        }
    }

    public static void Terminate()
    {
        activeNetwork.core.Terminate();
    }

    public void Reset()
    {
        _currentTick = 0u;
        _synchronizedTime = new NetIndex16(1, zeroSpecial: true);
        _tickSync = new NetIndex16(1, zeroSpecial: true);
    }

    public static void PreUpdate()
    {
        if (activeNetwork.core.isActive)
        {
            activeNetwork._networkActive = true;
        }
        else
        {
            activeNetwork._networkActive = false;
        }
        activeNetwork.DoPreUpdate();
    }

    public void DoPreUpdate()
    {
        _currentTick++;
        _synchronizedTime += 1;
        _tickSync += 1;
        core.Update();
        DuckNetwork.Update();
    }

    public static void PostUpdate()
    {
        activeNetwork.DoPostUpdate();
    }

    public void DoPostUpdate()
    {
        core.PostUpdate();
    }

    public static void PostDraw()
    {
        activeNetwork.DoPostDraw();
    }

    public void DoPostDraw()
    {
        core.PostDraw();
    }
}
