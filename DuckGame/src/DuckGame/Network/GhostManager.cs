using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class GhostManager
{
	private class HelperPhysicsIndexSorter : IComparer<Thing>
	{
		int IComparer<Thing>.Compare(Thing a, Thing b)
		{
			return a.physicsIndex - b.physicsIndex;
		}
	}

	private static int kglobalID;

	public int globalID;

	public List<SynchronizedNetMessage> _synchronizedEvents = new List<SynchronizedNetMessage>();

	private NetParticleManager _particleManager = new NetParticleManager();

	private GhostObject _managerState;

	public bool inGhostLerpLoop;

	public HashSet<GhostObject> _tempGhosts = new HashSet<GhostObject>();

	public HashSet<GhostObject> _ghosts = new HashSet<GhostObject>();

	public HashSet<GhostObject> pendingBitBufferGhosts = new HashSet<GhostObject>();

	private NetIndex16 ghostObjectIndex = new NetIndex16(32);

	public static readonly int kGhostIndexMax = 2500;

	private int _framesSinceClear;

	public Dictionary<NetIndex16, GhostObject> _ghostIndexMap = new Dictionary<NetIndex16, GhostObject>();

	public static bool inGhostLoop = false;

	public static bool receivingDestroyMessage = false;

	public static bool changingGhostType = false;

	public static bool updatingBullets = false;

	public List<GhostObject> _destroyedGhosts = new List<GhostObject>();

	public List<NetIndex16> _destroyResends = new List<NetIndex16>();

	private Dictionary<ushort, Thing> _specialSyncMap = new Dictionary<ushort, Thing>();

	private static HelperPhysicsIndexSorter helperPhysicsIndexSorter = new HelperPhysicsIndexSorter();

	private HashSet<GhostObject> _removeList = new HashSet<GhostObject>();

	public NetParticleManager particleManager => _particleManager;

	public NetIndex16 predictionIndex
	{
		get
		{
			return (_managerState.thing as GhostManagerState).predictionIndex;
		}
		set
		{
			(_managerState.thing as GhostManagerState).predictionIndex = value;
		}
	}

	public static GhostManager context => Network.activeNetwork.core.ghostManager;

	public NetIndex16 currentGhostIndex => ghostObjectIndex;

	public void UpdateSynchronizedEvents()
	{
		for (int i = 0; i < _synchronizedEvents.Count; i++)
		{
			if (_synchronizedEvents[i].Update())
			{
				DevConsole.Log(DCSection.DuckNet, "@received Activating |WHITE|" + _synchronizedEvents[i].ToString() + "|PREV|", DuckNetwork.localConnection);
				if (_synchronizedEvents[i] is NMSynchronizedEvent)
				{
					(_synchronizedEvents[i] as NMSynchronizedEvent).Activate();
				}
				_synchronizedEvents.RemoveAt(i);
				i--;
			}
		}
	}

	public void TransferPendingGhosts()
	{
		foreach (GhostObject g in pendingBitBufferGhosts)
		{
			AddGhost(g);
		}
		pendingBitBufferGhosts.Clear();
	}

	public GhostManager()
	{
		globalID = kglobalID++;
	}

	public NetIndex16 GetGhostIndex()
	{
		return GetGhostIndex(levelInit: false);
	}

	public NetIndex16 GetGhostIndex(bool levelInit)
	{
		int idxOffset = DuckNetwork.localProfile.fixedGhostIndex;
		if (levelInit)
		{
			idxOffset = DuckNetwork.hostProfile.fixedGhostIndex;
		}
		NetIndex16 idx = ghostObjectIndex;
		while (_ghostIndexMap.ContainsKey(ghostObjectIndex + idxOffset * kGhostIndexMax))
		{
			++ghostObjectIndex;
			if (ghostObjectIndex > kGhostIndexMax - 10)
			{
				ghostObjectIndex = 32;
			}
			if (ghostObjectIndex == idx || idx < 32)
			{
				break;
			}
		}
		idx = ghostObjectIndex;
		++ghostObjectIndex;
		return idx + idxOffset * kGhostIndexMax;
	}

	public void SetGhostIndex(NetIndex16 idx)
	{
		ghostObjectIndex = idx;
		Clear();
	}

	public void ResetGhostIndex(byte levelIndex)
	{
		if (levelIndex == 0)
		{
			if (DuckNetwork.localProfile != null)
			{
				ghostObjectIndex = (ushort)(int)DuckNetwork.localProfile.latestGhostIndex + 25;
			}
			else
			{
				ghostObjectIndex = Rando.Int(kGhostIndexMax - 500) + 5;
			}
		}
		else if (levelIndex % 2 == 1)
		{
			ghostObjectIndex = kGhostIndexMax / 2 + 100;
		}
		else
		{
			ghostObjectIndex = 300;
		}
		Clear();
	}

	public void Clear()
	{
		foreach (GhostObject ghost in _ghosts)
		{
			ghost.ReleaseReferences();
		}
		_ghosts.Clear();
		_ghostIndexMap.Clear();
		foreach (Profile profile in DuckNetwork.profiles)
		{
			profile.removedGhosts.Clear();
		}
		if (NetworkDebugger.enabled)
		{
			NetworkDebugger.ClearGhostDebug();
		}
		DevConsole.Log(DCSection.GhostMan, "Clearing all ghost data.");
		particleManager.Clear();
		_specialSyncMap.Clear();
		_destroyedGhosts.Clear();
		_framesSinceClear = 0;
	}

	public void Clear(NetworkConnection c)
	{
		bool changedData = false;
		foreach (GhostObject ghost in _ghosts)
		{
			ghost.ClearConnectionData(c);
			changedData = true;
		}
		if (Network.host != null)
		{
			foreach (GhostObject g in _ghosts)
			{
				if (g.thing.connection == c)
				{
					Thing.SuperFondle(g.thing, Network.host);
					changedData = true;
				}
			}
		}
		if (changedData)
		{
			DevConsole.Log(DCSection.GhostMan, "Clearing ghost data for " + c.identifier);
		}
	}

	public GhostObject GetGhost(NetIndex16 id)
	{
		GhostObject g = null;
		_ghostIndexMap.TryGetValue(id, out g);
		if (g == null && pendingBitBufferGhosts.Count > 0)
		{
			return pendingBitBufferGhosts.FirstOrDefault((GhostObject x) => x.ghostObjectIndex == id);
		}
		return g;
	}

	public GhostObject GetGhost(Thing thing)
	{
		return thing.ghostObject;
	}

	public void OnMessage(NetMessage m)
	{
		try
		{
			if (m is NMParticles)
			{
				NMParticles particles = m as NMParticles;
				if (particles.levelIndex == DuckNetwork.levelIndex)
				{
					particleManager.OnMessage(particles);
				}
			}
			else if (m is NMParticlesRemoved)
			{
				NMParticlesRemoved particles2 = m as NMParticlesRemoved;
				if (particles2.levelIndex == DuckNetwork.levelIndex)
				{
					particleManager.OnMessage(particles2);
				}
			}
			else if (m is NMProfileNetData)
			{
				NMProfileNetData net = m as NMProfileNetData;
				if (net._profile != null && net._netData != null)
				{
					net._profile.netData.Deserialize(net._netData, net.connection, pMakingDirty: false);
				}
			}
			else if (m is NMObjectNetData)
			{
				NMObjectNetData net2 = m as NMObjectNetData;
				if (net2.thing != null && net2._netData != null)
				{
					net2.thing.GetOrCreateNetData().Deserialize(net2._netData, net2.connection, !net2.thing.TransferControl(net2.connection, net2.authority));
				}
			}
			else if (m is NMRemoveGhosts)
			{
				NMRemoveGhosts r = m as NMRemoveGhosts;
				if (r.levelIndex != DuckNetwork.levelIndex)
				{
					return;
				}
				receivingDestroyMessage = true;
				foreach (NetIndex16 n in r.remove)
				{
					GhostObject obj = GetGhost(n);
					if (obj != null)
					{
						obj.thing.connection = m.connection;
						RemoveGhost(obj);
					}
				}
				receivingDestroyMessage = false;
			}
			else
			{
				if (m is NMGhostData)
				{
					NMGhostData state = m as NMGhostData;
					if (state.levelIndex != DuckNetwork.levelIndex)
					{
						return;
					}
					{
						foreach (NMGhostState s in state.states)
						{
							ProcessGhostState(s);
						}
						return;
					}
				}
				if (m is NMGhostState)
				{
					NMGhostState state2 = m as NMGhostState;
					if (state2.levelIndex == DuckNetwork.levelIndex)
					{
						ProcessGhostState(state2);
					}
				}
			}
		}
		catch (Exception ex)
		{
			DevConsole.Log(DCSection.GhostMan, "@error !! GHOST MANAGER UPDATE EXCEPTION", m.connection);
			DevConsole.Log(DCSection.GhostMan, ex.ToString(), m.connection);
			receivingDestroyMessage = false;
		}
	}

	private void ProcessGhostState(NMGhostState pState)
	{
		Profile p = GhostObject.IndexToProfile(pState.id);
		if (p != null && p.removedGhosts.ContainsKey(pState.id))
		{
			GhostObject g = p.removedGhosts[pState.id];
			if (g != null)
			{
				if (g.removeLogCooldown == 0)
				{
					DevConsole.Log(DCSection.GhostMan, "Ignoring removed ghost(" + g.ToString() + ")", pState.connection);
					g.removeLogCooldown = 5;
				}
				else
				{
					g.removeLogCooldown--;
				}
			}
			else
			{
				DevConsole.Log(DCSection.GhostMan, "Ignoring removed ghost(" + pState.ToString() + ")", pState.connection);
			}
			return;
		}
		GhostObject obj = null;
		obj = GetGhost(pState.id);
		if (pState.classID == 0)
		{
			RemoveGhost(obj, pState.id);
			return;
		}
		Type t = Editor.IDToType[pState.classID];
		long mask = (pState.header.delta ? GhostObject.ReadMask(t, pState.data) : long.MaxValue);
		if (obj != null && (t != obj.thing.GetType() || (obj.isDestroyed && mask == long.MaxValue)))
		{
			receivingDestroyMessage = true;
			changingGhostType = true;
			RemoveGhost(obj, obj.ghostObjectIndex);
			obj = null;
			receivingDestroyMessage = false;
			changingGhostType = false;
		}
		if (obj == null)
		{
			Thing thing = Editor.CreateThing(t);
			thing.position = new Vec2(-2000f, -2000f);
			Level.Add(thing);
			thing.connection = pState.connection;
			obj = new GhostObject(thing, this, pState.id);
			obj.ClearStateMask(pState.connection);
			pState.ghost = obj;
			AddGhost(obj);
			if (pState.connection.profile != null && pState.id > pState.connection.profile.latestGhostIndex)
			{
				pState.connection.profile.latestGhostIndex = pState.id;
			}
		}
		else
		{
			if (obj.isDestroyed)
			{
				DevConsole.Log(DCSection.GhostMan, "Skipped ghost data (DESTROYED)(" + obj.ghostObjectIndex.ToString() + ")", pState.connection);
				return;
			}
			if (obj.thing.isBitBufferCreatedGhostThing)
			{
				obj.thing.isBitBufferCreatedGhostThing = false;
				obj.thing.level = null;
				Level.Add(obj.thing);
			}
			if (pState.header.connection != null)
			{
				obj.thing.TransferControl(pState.header.connection, pState.authority);
			}
			else
			{
				obj.thing.TransferControl(pState.connection, pState.authority);
			}
		}
		if (NetworkDebugger.enabled && pState.connection.profile != null)
		{
			NetworkDebugger.GetGhost(obj).dataReceivedFrames[pState.connection.profile.persona] = Graphics.frame;
		}
		if (obj.thing.connection == pState.connection || obj.thing.connection == pState.header.connection)
		{
			obj.ReadInNetworkData(pState, mask, pState.connection, constructed: false);
		}
		else
		{
			for (int ii = 0; ii < Network.connections.Count; ii++)
			{
				NetworkConnection c = Network.connections[ii];
				obj.DirtyStateMask(mask, c);
			}
		}
		_ = obj.thing.position.x;
		_ = -1000f;
	}

	public void Notify(StreamManager pManager, NetMessage pMessage, bool pDropped)
	{
		if (pMessage is NMParticles || pMessage is NMParticlesRemoved)
		{
			_particleManager.Notify(pMessage, pDropped);
		}
		if (!pDropped)
		{
			return;
		}
		if (pMessage is NMGhostState)
		{
			NMGhostState m = pMessage as NMGhostState;
			if (m.mask == 0L)
			{
				m.ghost.GetConnectionData(pManager.connection).prevInputState = ushort.MaxValue;
				return;
			}
			long difference = ~pManager.GetPendingStates(m.ghost) & m.mask;
			m.ghost.DirtyStateMask(difference, m.connection);
			return;
		}
		if (pMessage is NMGhostData)
		{
			foreach (NMGhostData.GhostMaskPair pair in (pMessage as NMGhostData).ghostMaskPairs)
			{
				if (pair.mask == 0L)
				{
					pair.ghost.GetConnectionData(pManager.connection).prevInputState = ushort.MaxValue;
				}
				else
				{
					long difference2 = ~pManager.GetPendingStates(pair.ghost) & pair.mask;
					pair.ghost.DirtyStateMask(difference2, pMessage.connection);
				}
			}
			return;
		}
		if (pMessage is NMProfileNetData)
		{
			NMProfileNetData netData = pMessage as NMProfileNetData;
			{
				foreach (int hash in netData._hashes)
				{
					netData._profile.netData.MakeDirty(hash, netData.connection, netData.syncIndex);
				}
				return;
			}
		}
		if (pMessage is NMObjectNetData)
		{
			NMObjectNetData netData2 = pMessage as NMObjectNetData;
			if (netData2.thing == null)
			{
				return;
			}
			{
				foreach (int hash2 in netData2._hashes)
				{
					netData2.thing._netData.MakeDirty(hash2, netData2.connection, netData2.syncIndex);
				}
				return;
			}
		}
		if (pMessage is NMRemoveGhosts && DuckNetwork.levelIndex == (pMessage as NMRemoveGhosts).levelIndex && Level.core.nextLevel == null && Level.current.networkIndex == (pMessage as NMRemoveGhosts).levelIndex)
		{
			Send.Resend(pMessage);
		}
	}

	public void IncrementPrediction()
	{
		++(_managerState.thing as GhostManagerState).predictionIndex;
	}

	public void RemoveLater(GhostObject g)
	{
		if (!receivingDestroyMessage)
		{
			_removeList.Add(g);
		}
	}

	public void PostUpdate()
	{
		particleManager.Update();
	}

	public void UpdateGhostLerp()
	{
		_framesSinceClear++;
		inGhostLoop = true;
		inGhostLerpLoop = true;
		int numComplexGhosts = 0;
		foreach (GhostObject ghost in _ghosts)
		{
			if (ghost.thing is IComplexUpdate)
			{
				if (ghost.IsInitialized())
				{
					(ghost.thing as IComplexUpdate).OnPreUpdate();
				}
				numComplexGhosts++;
			}
		}
		foreach (GhostObject ghost2 in _ghosts)
		{
			if (ghost2.thing.connection != DuckNetwork.localConnection)
			{
				for (int i = 0; i < ghost2.constipation; i++)
				{
					ghost2.UpdateState();
				}
				ghost2.Update();
			}
			else
			{
				ghost2.UpdateTick();
			}
		}
		if (numComplexGhosts > 0)
		{
			foreach (GhostObject ghost3 in _ghosts)
			{
				if (ghost3.thing is IComplexUpdate && ghost3.IsInitialized())
				{
					(ghost3.thing as IComplexUpdate).OnPostUpdate();
				}
			}
		}
		inGhostLerpLoop = false;
		foreach (GhostObject g in _tempGhosts)
		{
			_ghosts.Add(g);
		}
		_tempGhosts.Clear();
		inGhostLoop = false;
	}

	public void PostDraw()
	{
	}

	public void UpdateRemoval()
	{
		if (_destroyedGhosts.Count > 0 || _destroyResends.Count > 0)
		{
			Send.Message(new NMRemoveGhosts(this), NetMessagePriority.Volatile);
		}
	}

	public void RemoveGhost(GhostObject ghost, bool makeOld)
	{
		if (ghost != null)
		{
			RemoveGhost(ghost, ghost.ghostObjectIndex);
		}
	}

	public void RemoveGhost(GhostObject ghost)
	{
		if (ghost != null)
		{
			RemoveGhost(ghost, ghost.ghostObjectIndex);
		}
	}

	public void RemoveGhost(GhostObject ghost, NetIndex16 ghostIndex)
	{
		RemoveGhost(ghost, ghostIndex, viaGhostMessage: false);
	}

	public void RemoveGhost(GhostObject ghost, NetIndex16 ghostIndex, bool viaGhostMessage = false)
	{
		if (ghost == null)
		{
			return;
		}
		_ghosts.Remove(ghost);
		if (ghost.thing != null && !ghost.thing.removeFromLevel)
		{
			Level.Remove(ghost.thing);
		}
		if (!receivingDestroyMessage && ghost.thing != null && ghost.thing.isServerForObject)
		{
			_destroyedGhosts.Add(ghost);
		}
		_ghostIndexMap.Remove(ghost.ghostObjectIndex);
		if (!changingGhostType && _framesSinceClear > 60)
		{
			Profile p = GhostObject.IndexToProfile(ghost.ghostObjectIndex);
			if (p != null && p.connection != null)
			{
				p.removedGhosts[ghost.ghostObjectIndex] = ghost;
			}
		}
		ghost.isOldGhost = true;
		ghost.ReleaseReferences(pFull: false);
	}

	public void MapSpecialSync(Thing t, ushort index)
	{
		_specialSyncMap[index] = t;
	}

	public Thing GetSpecialSync(ushort index)
	{
		Thing t = null;
		if (!_specialSyncMap.TryGetValue(index, out t))
		{
			t = Level.current.things.First((Thing x) => x.specialSyncIndex == index);
			if (t != null)
			{
				_specialSyncMap[index] = t;
			}
		}
		return t;
	}

	public GhostObject MakeGhost(Thing t, int index = -1, bool initLevel = false)
	{
		if (t.ghostObject == null)
		{
			GhostObject ghost = new GhostObject(t, this, index, initLevel);
			AddGhost(ghost);
			return ghost;
		}
		return t.ghostObject;
	}

	public GhostObject MakeGhostLater(Thing t, int index = -1, bool initLevel = false)
	{
		bool inloop = inGhostLerpLoop;
		inGhostLerpLoop = true;
		GhostObject result = MakeGhost(t, index, initLevel);
		inGhostLerpLoop = inloop;
		return result;
	}

	internal void AddGhost(GhostObject pGhost)
	{
		if (inGhostLerpLoop)
		{
			_tempGhosts.Add(pGhost);
		}
		else if (!_ghosts.Contains(pGhost) && pGhost.thing != null)
		{
			_ghosts.Add(pGhost);
			pGhost.thing.OnGhostObjectAdded();
		}
	}

	public void MapGhost(Thing pThing, GhostObject pGhost)
	{
		AddGhost(pGhost);
	}

	public void RefreshGhosts(Level lev = null)
	{
		if (lev == null)
		{
			lev = Level.current;
		}
		if (lev.things.objectsDirty)
		{
			Thing[] orderedStuff = new Thing[lev.things.updateList.Count];
			lev.things.updateList.CopyTo(orderedStuff);
			Array.Sort(orderedStuff, helperPhysicsIndexSorter);
			if (orderedStuff != null)
			{
				int c = orderedStuff.Count();
				for (int ii = 0; ii < c; ii++)
				{
					Thing t = orderedStuff[ii];
					if (t.isStateObject && !t.removeFromLevel && !t.ignoreGhosting && t.ghostObject == null)
					{
						GhostObject ghost = new GhostObject(t, this);
						AddGhost(ghost);
					}
				}
			}
			lev.things.objectsDirty = false;
		}
		int idx = 0;
		List<NetworkConnection> nclist = Network.activeNetwork.core.connections;
		foreach (GhostObject g in _ghosts)
		{
			if (g.thing.isServerForObject)
			{
				g.RefreshStateMask(nclist);
			}
			idx++;
		}
		foreach (GhostObject g2 in _ghosts)
		{
			if (g2.shouldRemove && g2.thing != null)
			{
				if (!g2.thing.removeFromLevel)
				{
					Level.Remove(g2.thing);
				}
				RemoveLater(g2);
			}
		}
		foreach (GhostObject t2 in _removeList)
		{
			if (t2.thing != null)
			{
				t2.thing.ghostType = 0;
			}
			RemoveGhost(t2, t2.ghostObjectIndex);
		}
		_removeList.Clear();
		if (Level.core.nextLevel == null && Level.current.initializeFunctionHasBeenRun)
		{
			UpdateRemoval();
		}
	}

	public void UpdateInit()
	{
		if (_managerState == null)
		{
			_managerState = new GhostObject(new GhostManagerState(), this, 0);
		}
	}

	public void OnDisconnect(NetworkConnection connection)
	{
		foreach (GhostObject g in _ghosts)
		{
			g.DirtyStateMask(long.MaxValue, connection);
			if (g.thing._netData != null)
			{
				g.thing._netData.MakeDirty(int.MaxValue, connection, 0);
			}
		}
		foreach (Profile profile in DuckNetwork.profiles)
		{
			profile.netData.MakeDirty(int.MaxValue, connection, 0);
		}
	}

	public void PreUpdate()
	{
	}

	public void Update(NetworkConnection connection, bool sendPackets)
	{
		if (sendPackets)
		{
			UpdateGhostSync(connection, pDelta: true, pSendMessages: true);
		}
	}

	public void UpdateRemovalMessages()
	{
	}

	public List<NMGhostData> UpdateGhostSync(NetworkConnection pConnection, bool pDelta, bool pSendMessages, NetMessagePriority pPriority = NetMessagePriority.Volatile)
	{
		List<NMGhostData> messages = new List<NMGhostData>();
		List<GhostObject> serializeGhosts = new List<GhostObject>();
		foreach (GhostObject ghost in _ghosts)
		{
			if (ghost.thing.connection == null)
			{
				ghost.thing.connection = Network.host;
			}
			if (pDelta && ghost.thing.connection != DuckNetwork.localConnection && (ghost.thing.connection != null || !Network.isServer))
			{
				continue;
			}
			if (!ghost.thing.isInitialized)
			{
				ghost.thing.DoInitialize();
			}
			if (pDelta && pSendMessages && ghost.thing._netData != null && ghost.thing._netData.IsDirty(pConnection))
			{
				Send.Message(new NMObjectNetData(ghost.thing, pConnection), NetMessagePriority.Volatile, pConnection);
				ghost.thing._netData.Clean(pConnection);
			}
			if (pDelta && !ghost.NeedsSync(pConnection))
			{
				continue;
			}
			if (pDelta)
			{
				ghost.previouslySerializedData = ghost.GetNetworkStateData(pConnection, pMinimal: true);
			}
			else
			{
				ghost.previouslySerializedData = ghost.GetNetworkStateData();
				ghost.ClearStateMask(pConnection, ghost.thing.authority);
			}
			if (pDelta)
			{
				int insertIdx = 0;
				for (int i = 0; i < serializeGhosts.Count; i++)
				{
					if (ghost.thing is Duck)
					{
						insertIdx = 0;
						break;
					}
					if (serializeGhosts[i].thing is Duck)
					{
						insertIdx++;
					}
					if (serializeGhosts[i].thing.GetType() == ghost.thing.GetType())
					{
						insertIdx = i;
						break;
					}
				}
				serializeGhosts.Insert(insertIdx, ghost);
			}
			else
			{
				serializeGhosts.Add(ghost);
			}
		}
		int idx = 0;
		while (idx < serializeGhosts.Count)
		{
			NMGhostData dat = NMGhostData.GetSerializedGhostData(serializeGhosts, idx);
			messages.Add(dat);
			idx += dat.ghostMaskPairs.Count;
			if (pSendMessages)
			{
				Send.Message(dat, pPriority, pConnection);
			}
		}
		return messages;
	}
}
