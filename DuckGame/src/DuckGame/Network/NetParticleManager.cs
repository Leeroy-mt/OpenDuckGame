using System;
using System.Collections.Generic;

namespace DuckGame;

public class NetParticleManager
{
    private Dictionary<ushort, PhysicsParticle> _particles = new Dictionary<ushort, PhysicsParticle>();

    private ushort _nextParticleIndex = 1;

    private Dictionary<NetworkConnection, ushort> _lastPacketNumbers = new Dictionary<NetworkConnection, ushort>();

    public HashSet<ushort> removedParticleIndexes = new HashSet<ushort>();

    private Queue<List<PhysicsParticle>> _pendingParticles = new Queue<List<PhysicsParticle>>();

    private Dictionary<Type, List<PhysicsParticle>> _inProgressParticleLists = new Dictionary<Type, List<PhysicsParticle>>();

    public static int _particleSyncSpread = 2;

    public static int _syncWait = 4;

    private byte updateOrder;

    private Queue<NMParticlesRemoved> _particleRemoveMessages = new Queue<NMParticlesRemoved>();

    private NMParticles currentParticleList = new NMParticles();

    public ushort GetParticleIndex()
    {
        _nextParticleIndex++;
        if (_nextParticleIndex > 4000)
        {
            ResetParticleIndex();
        }
        return _nextParticleIndex;
    }

    public void ResetParticleIndex()
    {
        _nextParticleIndex = 1;
    }

    private void ClearParticle(PhysicsParticle pParticle)
    {
        _particles.Remove(pParticle.netIndex);
        pParticle.netRemove = true;
        Level.Remove(pParticle);
        removedParticleIndexes.Add(pParticle.netIndex);
    }

    public void OnMessage(NetMessage m)
    {
        if (m is NMParticlesRemoved)
        {
            foreach (ushort u in (m as NMParticlesRemoved).removeParticles)
            {
                PhysicsParticle particle = null;
                if (_particles.TryGetValue(u, out particle))
                {
                    ClearParticle(particle);
                }
            }
            return;
        }
        if (!(m is NMParticles))
        {
            return;
        }
        ushort lastPacket = 0;
        _lastPacketNumbers.TryGetValue(m.packet.connection, out lastPacket);
        _lastPacketNumbers[m.packet.connection] = m.packet.order;
        bool oldPacket = Math.Abs(lastPacket - m.packet.order) < 1000 && m.packet.order < lastPacket;
        NMParticles part = m as NMParticles;
        if (part.levelIndex != DuckNetwork.levelIndex)
        {
            return;
        }
        while (true)
        {
            byte netType = part.data.ReadByte();
            if (netType == byte.MaxValue)
            {
                break;
            }
            Type t = PhysicsParticle.NetTypeToTypeIndex(netType);
            byte ct = part.data.ReadByte();
            for (int i = 0; i < ct; i++)
            {
                ushort netIndex = part.data.ReadUShort();
                PhysicsParticle particle2 = null;
                if ((!_particles.TryGetValue(netIndex, out particle2) || oldPacket) && t != null)
                {
                    if (t == typeof(SmallFire))
                    {
                        particle2 = SmallFire.New(Vec2.NetMin.X, Vec2.NetMin.Y, 0f, 0f, shortLife: false, null, canMultiply: false, null, network: true);
                    }
                    else if (t == typeof(ExtinguisherSmoke))
                    {
                        particle2 = new ExtinguisherSmoke(Vec2.NetMin.X, Vec2.NetMin.Y, network: true);
                    }
                    else if (t == typeof(Firecracker))
                    {
                        particle2 = new Firecracker(Vec2.NetMin.X, Vec2.NetMin.Y, network: true);
                    }
                    if (!oldPacket)
                    {
                        particle2.netIndex = netIndex;
                        particle2.isLocal = false;
                        if (removedParticleIndexes.Count > 3000)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                if (removedParticleIndexes.Contains((ushort)(netIndex - j)))
                                {
                                    removedParticleIndexes.Remove((ushort)(netIndex - j));
                                }
                            }
                        }
                        if (!removedParticleIndexes.Contains(netIndex))
                        {
                            if (_particles.Count > 200)
                            {
                                PhysicsParticle rem = null;
                                if (_particles.TryGetValue((ushort)(netIndex - 100), out rem))
                                {
                                    ClearParticle(rem);
                                }
                            }
                            _particles[netIndex] = particle2;
                            Level.Add(particle2);
                        }
                    }
                }
                particle2.NetDeserialize(part.data);
            }
        }
    }

    public void AddLocalParticle(PhysicsParticle p)
    {
        if (DuckNetwork.localProfile == null)
        {
            return;
        }
        p.connection = DuckNetwork.localConnection;
        p.netIndex = (ushort)(GetParticleIndex() + DuckNetwork.localProfile.networkIndex * 4000);
        _particles[p.netIndex] = p;
        if (_particles.Count > 200)
        {
            PhysicsParticle rem = null;
            if (_particles.TryGetValue((ushort)(p.netIndex - 100), out rem))
            {
                RemoveParticle(rem);
                Level.Remove(rem);
            }
        }
    }

    public void Clear()
    {
        _particles.Clear();
        _inProgressParticleLists.Clear();
        _pendingParticles.Clear();
        removedParticleIndexes.Clear();
        ResetParticleIndex();
        updateOrder = 0;
    }

    public List<PhysicsParticle> GetParticleList(Type t)
    {
        List<PhysicsParticle> list = null;
        if (!_inProgressParticleLists.TryGetValue(t, out list) || list.Count >= 20)
        {
            list = new List<PhysicsParticle>();
            _inProgressParticleLists[t] = list;
            _pendingParticles.Enqueue(list);
        }
        return list;
    }

    public void RemoveParticle(PhysicsParticle p)
    {
        p.netRemove = true;
    }

    public void Notify(NetMessage m, bool dropped)
    {
        if (dropped && m is NMParticlesRemoved && (m as NMParticlesRemoved).levelIndex == DuckNetwork.levelIndex)
        {
            Send.Message(new NMParticlesRemoved
            {
                removeParticles = (m as NMParticlesRemoved).removeParticles
            }, NetMessagePriority.Volatile, m.connection);
        }
    }

    public void Update()
    {
        List<PhysicsParticle> remove = null;
        int idx = 0;
        while (true)
        {
            int startIdx = idx;
            int numSyncParticles = 0;
            foreach (KeyValuePair<ushort, PhysicsParticle> particle in _particles)
            {
                PhysicsParticle f = particle.Value;
                if (f.isLocal)
                {
                    if (f.netRemove)
                    {
                        if (remove == null)
                        {
                            remove = new List<PhysicsParticle>();
                        }
                        removedParticleIndexes.Add(f.netIndex);
                        remove.Add(f);
                    }
                    else
                    {
                        numSyncParticles++;
                        if (f.updateOrder != updateOrder)
                        {
                            f.updateOrder = updateOrder;
                            currentParticleList.Add(f);
                            idx++;
                        }
                    }
                }
                else if (f.netRemove)
                {
                    if (remove == null)
                    {
                        remove = new List<PhysicsParticle>();
                    }
                    removedParticleIndexes.Add(f.netIndex);
                    remove.Add(f);
                    Level.Remove(f);
                }
                if (idx > 30)
                {
                    break;
                }
            }
            if (idx != startIdx || numSyncParticles <= 0)
            {
                break;
            }
            updateOrder++;
        }
        if (currentParticleList.particles.Count > 0)
        {
            Send.Message(currentParticleList, NetMessagePriority.Volatile);
            currentParticleList = new NMParticles();
        }
        if (remove != null)
        {
            NMParticlesRemoved m = new NMParticlesRemoved();
            _particleRemoveMessages.Enqueue(m);
            foreach (PhysicsParticle p in remove)
            {
                if (m.removeParticles.Count >= 32)
                {
                    m = new NMParticlesRemoved();
                    _particleRemoveMessages.Enqueue(m);
                }
                m.removeParticles.Add(p.netIndex);
                _particles.Remove(p.netIndex);
            }
            remove.Clear();
            remove = null;
        }
        if (_particleRemoveMessages.Count > 0)
        {
            Send.Message(_particleRemoveMessages.Dequeue(), NetMessagePriority.Volatile);
        }
    }
}
