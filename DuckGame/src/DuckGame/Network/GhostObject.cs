using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DuckGame;

[DebuggerDisplay("Thing = {_thing}")]
public class GhostObject
{
    public byte removeLogCooldown;

    public Vec2 prevPosition;

    public float prevRotation;

    private Dictionary<NetworkConnection, GhostConnectionData> _connectionData = new Dictionary<NetworkConnection, GhostConnectionData>();

    public long lastWrittenMask;

    public int oldGhostTicks;

    private BitBuffer _body = new BitBuffer();

    public bool destroyMessageSent;

    public bool permaOldGhost;

    private NetIndex16 _ghostObjectIndex;

    public bool didDestroyRefresh;

    private BufferedGhostState _networkState;

    private int _finalPositionSyncFrames = 3;

    private long tickIncFrame;

    public bool wrote;

    public BitBuffer previouslySerializedData;

    public List<BufferedGhostState> _stateTimeline = new List<BufferedGhostState>();

    public long[] _last3StateMasks = new long[3];

    public int _last3MaskInc;

    private int delay;

    public BufferedGhostProperty netPositionProperty;

    public BufferedGhostProperty netVelocityProperty;

    public BufferedGhostProperty netAngleProperty;

    public bool isOldGhost;

    private bool _shouldRemove;

    private byte _storedInputStates;

    private ushort[] _inputStates = new ushort[NetworkConnection.packetsEvery];

    private int framesSinceRequestInitialize = 999;

    private int _stateTimelineIndex;

    private const int kStateHistoryLength = 360;

    public static GhostObject applyContext;

    private Thing _prevOwner;

    private Thing _thing;

    private bool initializedCached;

    private List<StateBinding> _fields = new List<StateBinding>();

    private GhostManager _manager;

    public ITakeInput _inputObject;

    public bool isDestroyed => _thing.removeFromLevel;

    public NetIndex16 ghostObjectIndex
    {
        get
        {
            return _ghostObjectIndex;
        }
        set
        {
            _ghostObjectIndex = value;
        }
    }

    public bool isLocalController
    {
        get
        {
            if (_inputObject != null && _inputObject.inputProfile != null)
            {
                return _inputObject.inputProfile.virtualDevice == null;
            }
            return false;
        }
    }

    public bool shouldRemove
    {
        get
        {
            if (!_shouldRemove)
            {
                return thing.removeFromLevel;
            }
            return true;
        }
        set
        {
            _shouldRemove = value;
        }
    }

    public int constipation
    {
        get
        {
            if (_stateTimelineIndex < _stateTimeline.Count - 70)
            {
                return 3;
            }
            if (_stateTimelineIndex < _stateTimeline.Count - 5)
            {
                return 2;
            }
            return 1;
        }
    }

    public Thing thing => _thing;

    public GhostManager manager => _manager;

    private static BufferedGhostProperty MakeBufferedProperty(StateBinding state, object value, int index = 0, NetIndex16 tick = default(NetIndex16))
    {
        if (state.type == typeof(float))
        {
            return new BufferedGhostProperty<float>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        if (state.type == typeof(bool))
        {
            return new BufferedGhostProperty<bool>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        if (state.type == typeof(byte))
        {
            return new BufferedGhostProperty<byte>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        if (state.type == typeof(ushort))
        {
            return new BufferedGhostProperty<ushort>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        if (state.type == typeof(short))
        {
            return new BufferedGhostProperty<short>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        if (state is CompressedVec2Binding || state is InterpolatedVec2Binding)
        {
            return new BufferedGhostProperty<Vec2>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        if (state is CompressedFloatBinding)
        {
            return new BufferedGhostProperty<float>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        if (state.type == typeof(int))
        {
            return new BufferedGhostProperty<int>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        if (state.type == typeof(Vec2))
        {
            return new BufferedGhostProperty<Vec2>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        if (state.type == typeof(NetIndex4))
        {
            return new BufferedGhostProperty<NetIndex4>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        if (state.type == typeof(NetIndex8))
        {
            return new BufferedGhostProperty<NetIndex8>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        if (state.type == typeof(NetIndex16))
        {
            return new BufferedGhostProperty<NetIndex16>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        if (state.type == typeof(sbyte))
        {
            return new BufferedGhostProperty<sbyte>
            {
                binding = state,
                value = value,
                index = index,
                tick = tick
            };
        }
        return new BufferedGhostProperty<object>
        {
            binding = state,
            value = value,
            index = index,
            tick = tick
        };
    }

    public GhostConnectionData GetConnectionData(NetworkConnection c)
    {
        if (c == null)
        {
            return null;
        }
        GhostConnectionData data = null;
        if (!_connectionData.TryGetValue(c, out data))
        {
            data = new GhostConnectionData();
            _connectionData[c] = data;
        }
        return data;
    }

    public void ClearConnectionData(NetworkConnection c)
    {
        if (_connectionData.ContainsKey(c))
        {
            _connectionData.Remove(c);
        }
    }

    private void WriteMinimalStateMask(GhostConnectionData dat, BitBuffer b)
    {
        lastWrittenMask = dat.connectionStateMask;
        int fieldCount = _fields.Count;
        if (fieldCount <= 8)
        {
            b.Write((byte)lastWrittenMask);
        }
        else if (fieldCount <= 16)
        {
            b.Write((ushort)lastWrittenMask);
        }
        else if (fieldCount <= 32)
        {
            b.Write((uint)lastWrittenMask);
        }
        else
        {
            b.Write(lastWrittenMask);
        }
    }

    public static long ReadMinimalStateMask(Type t, BitBuffer b)
    {
        return b.ReadBits<long>(Editor.AllStateFields[t].Length);
    }

    private object GetMinimalStateMask(NetworkConnection c)
    {
        long mask = GetConnectionData(c).connectionStateMask;
        int fieldCount = _fields.Count;
        if (fieldCount <= 8)
        {
            return (byte)mask;
        }
        if (fieldCount <= 16)
        {
            return (short)mask;
        }
        if (fieldCount <= 32)
        {
            return (int)mask;
        }
        return mask;
    }

    public static long ReadMask(Type t, BitBuffer b)
    {
        int count = Editor.AllStateFields[t].Length;
        if (count <= 8)
        {
            return b.ReadByte();
        }
        if (count <= 16)
        {
            return b.ReadUShort();
        }
        if (count <= 32)
        {
            return b.ReadUInt();
        }
        return b.ReadLong();
    }

    public static bool MaskIsMaxValue(Type t, long mask)
    {
        int count = Editor.AllStateFields[t].Length;
        if (count <= 8)
        {
            return mask == 255;
        }
        if (count <= 16)
        {
            return mask == 32767;
        }
        if (count <= 32)
        {
            return mask == int.MaxValue;
        }
        return mask == long.MaxValue;
    }

    public void ClearStateMask(NetworkConnection c)
    {
        GetConnectionData(c).connectionStateMask = 0L;
    }

    public void ClearStateMask(NetworkConnection c, NetIndex8 pAuthority)
    {
        GetConnectionData(c).connectionStateMask = 0L;
        GetConnectionData(c).authority = pAuthority;
    }

    public void DirtyStateMask(long mask, NetworkConnection c)
    {
        GetConnectionData(c).connectionStateMask |= mask;
    }

    public void SuperDirtyStateMask()
    {
        List<NetworkConnection> nclist = Network.activeNetwork.core.connections;
        for (int k = 0; k < nclist.Count; k++)
        {
            NetworkConnection n = nclist[k];
            GetConnectionData(n).connectionStateMask = long.MaxValue;
        }
    }

    public bool NeedsSync(NetworkConnection pConnection)
    {
        if (IsDirty(pConnection) || isDestroyed)
        {
            return true;
        }
        return false;
    }

    public bool IsDirty(NetworkConnection c)
    {
        GhostConnectionData dat = GetConnectionData(c);
        if (dat.connectionStateMask == 0L && !(dat.authority != thing.authority))
        {
            return dat.prevInputState != _inputStates[0];
        }
        return true;
    }

    public void RefreshStateMask(List<NetworkConnection> nclist)
    {
        if (DevConsole.core.constantSync)
        {
            for (int k = 0; k < nclist.Count; k++)
            {
                NetworkConnection n = nclist[k];
                GetConnectionData(n).connectionStateMask |= long.MaxValue;
            }
            return;
        }
        long newBitMask = 0L;
        int bitIndex = 0;
        List<BufferedGhostProperty> properties = _networkState.properties;
        for (int ii = 0; ii < properties.Count; ii++)
        {
            BufferedGhostProperty property = properties[ii];
            if ((thing.owner == null || !(property.binding is InterpolatedVec2Binding) || _finalPositionSyncFrames > 0) && property.Refresh())
            {
                newBitMask |= 1L << bitIndex;
            }
            bitIndex++;
        }
        for (int i = 0; i < nclist.Count; i++)
        {
            NetworkConnection n2 = nclist[i];
            GetConnectionData(n2).connectionStateMask |= newBitMask;
        }
        if (thing.owner == null)
        {
            _finalPositionSyncFrames = 3;
        }
        else
        {
            _finalPositionSyncFrames--;
        }
    }

    public BitBuffer GetNetworkStateData(NetworkConnection pConnection, bool pMinimal)
    {
        wrote = true;
        BitBuffer stateData = new BitBuffer();
        if (isDestroyed)
        {
            stateData.Write((ushort)(int)ghostObjectIndex);
            lastWrittenMask = long.MaxValue;
        }
        else
        {
            GhostConnectionData connectionData = GetConnectionData(pConnection);
            ++connectionData.lastTickSent;
            GhostObjectHeader.Serialize(stateData, this, connectionData.lastTickSent, pDelta: true, pMinimal);
            stateData.Write(FillStateData(connectionData, pForceFull: false));
            connectionData.prevInputState = _inputStates[0];
        }
        return stateData;
    }

    public BitBuffer GetNetworkStateData()
    {
        wrote = true;
        BitBuffer stateData = new BitBuffer();
        if (isDestroyed)
        {
            stateData.Write((ushort)(int)ghostObjectIndex);
            lastWrittenMask = long.MaxValue;
        }
        else
        {
            GhostObjectHeader.Serialize(stateData, this, 0, pDelta: false, pMinimal: true);
            stateData.Write(FillStateData(null, pForceFull: true));
        }
        return stateData;
    }

    /// <summary>
    /// Fills a bit buffer with this objects network state data
    /// </summary>
    /// <param name="pConnectionData">The connection data object of the NetworkConnection this data is being sent to</param>
    /// <param name="pForceFull">If true, the buffer will be filled with all data instead of delta data.</param>
    /// <returns></returns>
    private BitBuffer FillStateData(GhostConnectionData pConnectionData, bool pForceFull)
    {
        _body.Clear();
        short bitIndex = 0;
        bool delta = !pForceFull;
        if (delta)
        {
            WriteMinimalStateMask(pConnectionData, _body);
            if (isLocalController)
            {
                _body.Write(val: true);
                for (int i = 0; i < NetworkConnection.packetsEvery; i++)
                {
                    _body.Write(_inputStates[(_storedInputStates + i) % NetworkConnection.packetsEvery]);
                }
            }
            else
            {
                _body.Write(val: false);
            }
        }
        else
        {
            lastWrittenMask = long.MaxValue;
        }
        StateBinding currentBinding = null;
        try
        {
            foreach (StateBinding state in _fields)
            {
                currentBinding = state;
                if (!delta || (pConnectionData.connectionStateMask & (1L << (int)bitIndex)) != 0L)
                {
                    if (state is DataBinding)
                    {
                        _body.Write(state.GetNetValue() as BitBuffer);
                    }
                    else
                    {
                        _body.WriteBits(state.GetNetValue(), state.bits);
                    }
                }
                bitIndex++;
            }
        }
        catch (Exception ex)
        {
            Main.SpecialCode = Main.SpecialCode + "Writing " + currentBinding.ToString();
            throw ex;
        }
        if (pConnectionData != null)
        {
            pConnectionData.connectionStateMask = 0L;
            pConnectionData.authority = thing.authority;
        }
        return _body;
    }

    public void ReadInNetworkData(NMGhostState ghostState, long mask, NetworkConnection c, bool constructed)
    {
        NetworkDebugger.ghostsReceived[NetworkDebugger.currentIndex]++;
        GhostConnectionData g = GetConnectionData(c);
        ghostState.mask = mask;
        ghostState.ghost = this;
        if (g.lastTickReceived == 0)
        {
            g.lastTickReceived = (int)ghostState.tick - 1;
        }
        if (Math.Abs(NetIndex16.Difference(ghostState.tick, g.lastTickReceived)) > 600)
        {
            _stateTimelineIndex = _stateTimeline.Count + 1;
            ReapplyStates();
            KillNetworkData();
            g.lastTickReceived = (int)ghostState.tick - 1;
        }
        bool outOfOrder = false;
        if (g.lastTickReceived < ghostState.tick)
        {
            if ((int)ghostState.tick - (int)g.lastTickReceived > 10)
            {
                delay = 2;
            }
            g.lastTickReceived = ghostState.tick;
        }
        else
        {
            outOfOrder = true;
            for (int i = _stateTimeline.Count - 1; i > 0; i--)
            {
                if (_stateTimeline[i].tick == ghostState.tick)
                {
                    return;
                }
            }
        }
        if (ghostState.authority > thing.authority)
        {
            thing.authority = ghostState.authority;
        }
        BufferedGhostState bufferedState = new BufferedGhostState();
        bufferedState.tick = ghostState.tick;
        bufferedState.mask = mask;
        bufferedState.authority = ghostState.authority;
        if (ghostState.header.delta && ghostState.data.ReadBool())
        {
            bufferedState.inputStates.Clear();
            for (int j = 0; j < NetworkConnection.packetsEvery; j++)
            {
                bufferedState.inputStates.Add(ghostState.data.ReadUShort());
            }
        }
        short bitIndex = 0;
        foreach (StateBinding state in _fields)
        {
            long itemMask = 1L << (int)bitIndex;
            if ((ghostState.mask & itemMask) != 0L)
            {
                bufferedState.properties.Add(MakeBufferedProperty(state, state.ReadNetValue(ghostState.data), bitIndex, bufferedState.tick));
                state.initialized = true;
            }
            else
            {
                bufferedState.properties.Add(_networkState.properties[bitIndex]);
            }
            bitIndex++;
        }
        if (!IsInitialized())
        {
            foreach (BufferedGhostProperty prop in bufferedState.properties)
            {
                if (!prop.isNetworkStateValue && ghostState.tick > _networkState.properties[prop.index].tick)
                {
                    _networkState.properties[prop.index].value = prop.value;
                    _networkState.properties[prop.index].tick = ghostState.tick;
                    _networkState.properties[prop.index].initialized = true;
                }
            }
            if (!IsInitialized())
            {
                return;
            }
            _networkState.ApplyImmediately();
            if (thing is Holdable)
            {
                Holdable h = thing as Holdable;
                if (h.isSpawned && !h.didSpawn)
                {
                    h.spawnAnimation = true;
                    h.didSpawn = true;
                }
            }
            return;
        }
        _last3MaskInc = (_last3MaskInc + 1) % 3;
        _last3StateMasks[_last3MaskInc] = ghostState.mask;
        if (outOfOrder)
        {
            int insertIndex = 0;
            for (int i2 = _stateTimeline.Count - 1; i2 > 0; i2--)
            {
                if (_stateTimeline[i2].tick > bufferedState.tick)
                {
                    insertIndex = i2;
                }
            }
            _stateTimeline.Insert(insertIndex, bufferedState);
            if (insertIndex < _stateTimelineIndex)
            {
                bufferedState.ApplyImmediately(_networkState);
            }
        }
        else
        {
            AddState(bufferedState);
            if (_stateTimeline.Count == 1)
            {
                bufferedState.ApplyImmediately(_networkState);
            }
        }
        if (_thing is PhysicsObject)
        {
            (_thing as PhysicsObject).sleeping = false;
        }
    }

    private void AddState(BufferedGhostState pState)
    {
        _stateTimeline.Add(pState);
        if (_stateTimeline.Count > 1)
        {
            pState.previousState = _stateTimeline[_stateTimeline.Count - 2];
            pState.previousState.nextState = _stateTimeline[_stateTimeline.Count - 1];
        }
    }

    public BufferedGhostState GetCurrentState()
    {
        BufferedGhostState bufferedState = new BufferedGhostState();
        int count = _fields.Count;
        for (int p = 0; p < count; p++)
        {
            StateBinding field = _fields[p];
            BufferedGhostProperty prop = MakeBufferedProperty(field, field.classValue);
            prop.initialized = thing.connection == DuckNetwork.localConnection;
            prop.isNetworkStateValue = true;
            bufferedState.properties.Add(prop);
            if (field.name == "netPosition")
            {
                netPositionProperty = prop;
            }
            else if (field.name == "netVelocity")
            {
                netVelocityProperty = prop;
            }
            else if (field.name == "_angle")
            {
                netAngleProperty = prop;
            }
        }
        return bufferedState;
    }

    private Vec2 Slerp(Vec2 from, Vec2 to, float step)
    {
        if (step == 0f)
        {
            return from;
        }
        if (from == to || step == 1f)
        {
            return to;
        }
        double theta = Math.Acos(Vec2.Dot(from, to));
        if (theta == 0.0)
        {
            return to;
        }
        double sinTheta = Math.Sin(theta);
        return (float)(Math.Sin((double)(1f - step) * theta) / sinTheta) * from + (float)(Math.Sin((double)step * theta) / sinTheta) * to;
    }

    public void KillNetworkData()
    {
        _stateTimeline.Clear();
        _stateTimelineIndex = 0;
        foreach (BufferedGhostProperty property in _networkState.properties)
        {
            property.tick = 0;
        }
    }

    public void TakeOwnership()
    {
        foreach (KeyValuePair<NetworkConnection, GhostConnectionData> d in _connectionData)
        {
            d.Value.connectionStateMask |= _last3StateMasks[0];
            d.Value.connectionStateMask |= _last3StateMasks[1];
            d.Value.connectionStateMask |= _last3StateMasks[2];
        }
    }

    public bool IsInitialized()
    {
        if (!initializedCached)
        {
            initializedCached = true;
            foreach (BufferedGhostProperty property in _networkState.properties)
            {
                if (!property.initialized)
                {
                    initializedCached = false;
                    break;
                }
            }
        }
        return initializedCached;
    }

    public void UpdateTick()
    {
        if (isLocalController)
        {
            if (MonoMain.pauseMenu != null)
            {
                _inputStates[_storedInputStates] = 0;
                _storedInputStates = (byte)((_storedInputStates + 1) % NetworkConnection.packetsEvery);
            }
            else
            {
                _inputStates[_storedInputStates] = _inputObject.inputProfile.state;
                _storedInputStates = (byte)((_storedInputStates + 1) % NetworkConnection.packetsEvery);
            }
        }
    }

    public void Update()
    {
        if (removeLogCooldown > 0)
        {
            removeLogCooldown--;
        }
        if (_thing == null)
        {
            return;
        }
        _thing.isLocal = false;
        if (!IsInitialized() || _thing.level == null)
        {
            if (framesSinceRequestInitialize > 15 && thing != null && thing.connection != null)
            {
                if (_thing.level == null)
                {
                    DevConsole.Log(DCSection.DuckNet, "|DGYELLOW|Skipping ghost update (" + ghostObjectIndex.ToString() + ", " + thing.GetType().ToString() + ")(LEVEL NULL)...");
                }
                else
                {
                    DevConsole.Log(DCSection.DuckNet, "|DGYELLOW|Skipping ghost update (" + ghostObjectIndex.ToString() + ", " + thing.GetType().ToString() + ")(NOT INITIALIZED)...");
                }
                framesSinceRequestInitialize = 0;
            }
            return;
        }
        if (_thing.active)
        {
            _thing.DoUpdate();
        }
        if (_thing.owner == null && !_thing.isServerForObject)
        {
            if (netPositionProperty != null)
            {
                netPositionProperty.Apply(1f);
            }
            if (netVelocityProperty != null)
            {
                netVelocityProperty.Apply(1f);
            }
            if (netAngleProperty != null)
            {
                netAngleProperty.Apply(1f);
            }
        }
    }

    public void UpdateRemoval()
    {
        if (_thing.ghostType == 0 || (_thing.level != null && _thing.level != Level.current && Level.core.nextLevel == null))
        {
            _shouldRemove = true;
        }
    }

    public BufferedGhostState GetStateForTick(NetIndex16 t)
    {
        for (int i = _stateTimeline.Count - 1; i >= 0; i--)
        {
            BufferedGhostState b = _stateTimeline[i];
            if ((int)b.tick <= (int)t)
            {
                return b;
            }
        }
        return null;
    }

    private BufferedGhostState GetStateToProcess()
    {
        if (_stateTimelineIndex < _stateTimeline.Count)
        {
            return _stateTimeline[_stateTimelineIndex];
        }
        return _stateTimeline.LastOrDefault();
    }

    public void ReapplyStates()
    {
        applyContext = this;
        for (int i = 0; i < _stateTimeline.Count; i++)
        {
            if (i < _stateTimelineIndex)
            {
                _stateTimeline[i]._framesApplied = NetworkConnection.packetsEvery - 1;
                _stateTimeline[i].Apply(1f, _networkState);
            }
        }
        applyContext = null;
    }

    private void ApplyState(BufferedGhostState pState, float pLerp, BufferedGhostState pNetworkState)
    {
        applyContext = this;
        pState.Apply(pLerp, pNetworkState);
        ApplyStateInput(pState);
        applyContext = null;
    }

    public void ReleaseReferences(bool pFull = true)
    {
        if (thing != null && thing.ghostObject == this)
        {
            thing.ghostObject = null;
        }
        if (pFull)
        {
            _thing = null;
            _stateTimeline = null;
            _networkState = null;
            _manager = null;
            _fields = null;
            _inputObject = null;
            _prevOwner = null;
            netPositionProperty = null;
            netVelocityProperty = null;
            netAngleProperty = null;
        }
    }

    private void ApplyStateInput(BufferedGhostState pState)
    {
        if (pState.inputStates == null)
        {
            return;
        }
        int stateIndex = Math.Min(pState._framesApplied - 1, pState.inputStates.Count - 1);
        if (_inputObject != null && _inputObject.inputProfile != null && _inputObject.inputProfile.virtualDevice != null)
        {
            if (pState.previousState != null && pState.previousState.inputStates.Count > 0 && pState.nextState != null)
            {
                _inputObject.inputProfile.virtualDevice.SetState(pState.previousState.inputStates[stateIndex]);
            }
            _inputObject.inputProfile.virtualDevice.SetState(pState.inputStates[stateIndex]);
        }
    }

    public void UpdateState()
    {
        if (delay > 0)
        {
            delay--;
            return;
        }
        BufferedGhostState state = null;
        for (int i = 0; i < 2; i++)
        {
            if (_stateTimeline.Count > 360)
            {
                if (_stateTimelineIndex == 0)
                {
                    state = _stateTimeline[0];
                    ApplyState(state, 1f, _networkState);
                    state._framesApplied = NetworkConnection.packetsEvery;
                }
                if (_stateTimeline.Count > 0)
                {
                    BufferedGhostState bufferedGhostState = _stateTimeline.ElementAt(0);
                    bufferedGhostState.previousState = null;
                    bufferedGhostState.nextState = null;
                    _stateTimeline.RemoveAt(0);
                    _stateTimelineIndex = Math.Max(0, _stateTimelineIndex - 1);
                }
            }
        }
        state = GetStateToProcess();
        if (state != null && state._framesApplied < NetworkConnection.packetsEvery)
        {
            if (state._framesApplied >= NetworkConnection.packetsEvery - 1)
            {
                ApplyState(state, 1f, _networkState);
                state._framesApplied = NetworkConnection.packetsEvery;
                _stateTimelineIndex = Math.Min(_stateTimeline.Count, _stateTimelineIndex + 1);
            }
            else
            {
                ApplyState(state, 0.5f, _networkState);
            }
        }
        else if (state != null)
        {
            ApplyState(state, 1f, _networkState);
            state._framesApplied = NetworkConnection.packetsEvery;
            _stateTimelineIndex = Math.Min(_stateTimeline.Count, _stateTimelineIndex + 1);
        }
        if (thing.owner != _prevOwner)
        {
            ReapplyStates();
        }
        _prevOwner = thing.owner;
        if (_thing.ghostType == 0)
        {
            _shouldRemove = true;
        }
    }

    public GhostObject()
    {
    }

    public static Profile IndexToProfile(NetIndex16 pIndex)
    {
        int idx = (int)((float)pIndex._index / (float)GhostManager.kGhostIndexMax);
        if (idx < 0 || idx >= DuckNetwork.profiles.Count)
        {
            return null;
        }
        return DuckNetwork.profiles[idx];
    }

    public GhostObject(Thing thing, GhostManager manager, int ghostIndex = -1, bool levelInit = false)
    {
        try
        {
            _thing = thing;
            _thing.ghostObject = this;
            _inputObject = _thing as ITakeInput;
            if (ghostIndex == -1 && _thing.fixedGhostIndex != 0)
            {
                ghostIndex = _thing.fixedGhostIndex;
            }
            initializedCached = false;
            FieldInfo[] array = Editor.AllStateFields[_thing.GetType()];
            for (int i = 0; i < array.Length; i++)
            {
                StateBinding state = array[i].GetValue(_thing) as StateBinding;
                state.Connect(_thing);
                _fields.Add(state);
            }
            _networkState = GetCurrentState();
            _manager = manager;
            if (ghostIndex != -1)
            {
                _ghostObjectIndex = new NetIndex16(ghostIndex);
                _thing.ghostType = Editor.IDToType[_thing.GetType()];
            }
            else
            {
                _ghostObjectIndex = _manager.GetGhostIndex(levelInit);
                if (!levelInit || Network.isServer)
                {
                    _thing.connection = DuckNetwork.localConnection;
                }
            }
            DevConsole.Log(DCSection.GhostMan, "|DGBLUE|Creating|PREV| ghost (" + ghostObjectIndex.ToString() + "|PREV|)");
            manager._ghostIndexMap[_ghostObjectIndex] = this;
        }
        catch (Exception ex)
        {
            Main.SpecialCode = "GhostObject Constructor(" + thing.GetType().Name + ")";
            throw ex;
        }
    }
}
