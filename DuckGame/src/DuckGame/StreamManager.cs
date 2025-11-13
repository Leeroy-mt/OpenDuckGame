using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class StreamManager
{
	private class NetQueue
	{
		private ushort[] _buffer = new ushort[128];

		private int _size;

		private int _first;

		public NetQueue()
		{
			_size = 0;
			_first = 0;
		}

		public bool Contains(ushort val)
		{
			return false;
		}

		public void Add(ushort val)
		{
			if (_size < 128)
			{
				_buffer[_size++] = val;
				return;
			}
			_buffer[_first++] = val;
			_first &= 127;
		}
	}

	private float _ping = 1f;

	private float _pingPeak;

	private int _losses;

	private int _lossAccumulator;

	private int _lossAccumulatorInc;

	public bool lossThisFrame;

	private int _sent;

	private float _jitter;

	private float _jitterPeak;

	private int _jitterPeakReset;

	private float[] _previousPings = new float[32];

	private int _currentPing;

	private float _prevAverage;

	private EventManager _eventManager;

	private List<NetMessage> _unreliableMessages = new List<NetMessage>();

	private List<NetMessage> _unacknowledgedMessages = new List<NetMessage>();

	private List<NetMessage> _freshMessages = new List<NetMessage>();

	private List<NetMessage> _orderedPackets = new List<NetMessage>();

	private NetworkConnection _connection;

	private ushort _expectedReliableOrder;

	private int _lastReceivedAck = -1;

	private HashSet<ushort> _receivedVolatilePackets = new HashSet<ushort>();

	private HashSet<ushort> _receivedUrgentPackets = new HashSet<ushort>();

	private Dictionary<ushort, uint> _previousReliableMessageSizes = new Dictionary<ushort, uint>();

	private List<NMMessageFragment> currentFragmentCollection = new List<NMMessageFragment>();

	private ushort _packetOrder = 1;

	private ushort _reliableOrder;

	private ushort _volatileID = 10000;

	private ushort _urgentID = 10000;

	private NetworkPacket _currentPacketInternal;

	private ConnectionStatus _prevSendStatus = ConnectionStatus.None;

	private int _retransmitCycle;

	public float ping => _ping;

	public float pingPeak => _pingPeak;

	public int losses => _losses;

	public int accumulatedLoss => _lossAccumulator;

	public int sent => _sent;

	public float jitter => _jitter;

	public float jitterPeak => _jitterPeak;

	public static StreamManager context
	{
		get
		{
			if (NetworkConnection.context != null)
			{
				return NetworkConnection.context.manager;
			}
			return null;
		}
	}

	public EventManager eventManager => _eventManager;

	public NetworkConnection connection => _connection;

	public ushort expectedReliableOrder => _expectedReliableOrder;

	public int lastReceivedAck => _lastReceivedAck;

	private NetworkPacket currentPacket
	{
		get
		{
			if (_currentPacketInternal == null)
			{
				BitBuffer buffer = new BitBuffer();
				_currentPacketInternal = new NetworkPacket(buffer, _connection, GetPacketOrder());
			}
			return _currentPacketInternal;
		}
	}

	public void Reset()
	{
		_unacknowledgedMessages.RemoveAll((NetMessage x) => x.priority != NetMessagePriority.ReliableOrdered);
		_unreliableMessages.Clear();
		_previousReliableMessageSizes.Clear();
		_receivedUrgentPackets.Clear();
		_receivedVolatilePackets.Clear();
		_freshMessages.RemoveAll((NetMessage x) => x.priority != NetMessagePriority.ReliableOrdered);
	}

	public void RecordLoss()
	{
		lossThisFrame = true;
		_lossAccumulator++;
		_losses++;
	}

	public void LogPing(float pingVal)
	{
		if (pingVal < 0f)
		{
			pingVal = 0f;
		}
		_previousPings[_currentPing % 32] = pingVal;
		_currentPing++;
		float avg = 0f;
		for (int i = 0; i < 32; i++)
		{
			avg += _previousPings[i];
		}
		_ping = avg / 32f;
		if (_ping > _pingPeak)
		{
			_pingPeak = _ping;
		}
	}

	public long GetPendingStates(GhostObject obj)
	{
		long changing = 0L;
		foreach (NetMessage s in _unreliableMessages)
		{
			if (!(s is NMGhostData))
			{
				continue;
			}
			foreach (NMGhostData.GhostMaskPair gp in (s as NMGhostData).ghostMaskPairs)
			{
				if (gp.ghost == obj)
				{
					changing |= gp.mask;
				}
			}
		}
		return changing;
	}

	private void IncrementExpectedOrder()
	{
		_expectedReliableOrder = (ushort)((_expectedReliableOrder + 1) % 65535);
	}

	public StreamManager(NetworkConnection connection)
	{
		_connection = connection;
		_eventManager = new EventManager(connection, this);
	}

	public void DoAcks(HashSet<ushort> acksReceived)
	{
		if (acksReceived != null && acksReceived.Count > 0)
		{
			List<NetMessage> found = new List<NetMessage>();
			lock (_unacknowledgedMessages)
			{
				foreach (NetMessage nm in _unacknowledgedMessages)
				{
					foreach (ushort ack in acksReceived)
					{
						if (nm.packetsActive.Contains(ack))
						{
							found.Add(nm);
							break;
						}
					}
				}
				foreach (NetMessage nm2 in found)
				{
					_unacknowledgedMessages.Remove(nm2);
				}
			}
			lock (_unreliableMessages)
			{
				foreach (NetMessage nm3 in _unreliableMessages)
				{
					foreach (ushort ack2 in acksReceived)
					{
						if (nm3.packetsActive.Contains(ack2))
						{
							found.Add(nm3);
						}
					}
				}
				foreach (NetMessage nm4 in found)
				{
					_unreliableMessages.Remove(nm4);
				}
			}
			acksReceived.Clear();
			_ = _unacknowledgedMessages.Count;
			_ = 0;
			foreach (NetMessage nm5 in found)
			{
				NotifyAfterMessageAck(nm5, dropped: false);
			}
		}
		lock (_unreliableMessages)
		{
			List<NetMessage> timedOut = new List<NetMessage>();
			foreach (NetMessage nm6 in _unreliableMessages)
			{
				nm6.timeout -= Maths.IncFrameTimer();
				if (nm6.timeout <= 0f)
				{
					timedOut.Add(nm6);
				}
			}
			foreach (NetMessage nm7 in timedOut)
			{
				nm7.queued = false;
				nm7.packet = null;
				nm7.packetsActive.Clear();
				_unreliableMessages.Remove(nm7);
				NotifyAfterMessageAck(nm7, dropped: true);
			}
		}
	}

	public void NotifyAfterMessageAck(NetMessage m, bool dropped)
	{
		if (dropped)
		{
			RecordLoss();
		}
		else
		{
			m.DoMessageWasReceived();
		}
		if (m.manager == BelongsToManager.GhostManager)
		{
			GhostManager.context.Notify(this, m, dropped);
		}
	}

	public uint GetExistingReceivedReliableMessageSize(ushort pMessageOrder)
	{
		uint size = 0u;
		_previousReliableMessageSizes.TryGetValue(pMessageOrder, out size);
		return size;
	}

	public void StoreReceivedReliableMessageSize(ushort pOrder, uint pSize)
	{
		_previousReliableMessageSizes[pOrder] = pSize;
	}

	public void MessagesReceived(List<NetMessage> messages)
	{
		foreach (NetMessage m in messages)
		{
			if (connection.status != ConnectionStatus.Connected && !(m is NMNetworkCoreMessage))
			{
				if (connection.status == ConnectionStatus.Disconnecting || connection.status == ConnectionStatus.Disconnected)
				{
					DevConsole.Log(DCSection.NetCore, "@error Received |WHITE|" + m.ToString() + "|PREV| while disconnecting!!@error");
				}
				else
				{
					DevConsole.Log(DCSection.NetCore, "@error Received |WHITE|" + m.ToString() + "|PREV| while connecting!!@error");
				}
			}
			if (m.priority == NetMessagePriority.ReliableOrdered)
			{
				if (m.order >= _expectedReliableOrder && _orderedPackets.FirstOrDefault((NetMessage x) => x.order == m.order) == null)
				{
					int insertIndex = 0;
					for (insertIndex = 0; insertIndex < _orderedPackets.Count && _orderedPackets[insertIndex].order <= m.order; insertIndex++)
					{
					}
					_orderedPackets.Insert(insertIndex, m);
				}
			}
			else if (m.priority == NetMessagePriority.Volatile)
			{
				if (!_receivedVolatilePackets.Contains(m.order))
				{
					NetMessageReceived(m);
					_receivedVolatilePackets.Add(m.order);
					_receivedVolatilePackets.Remove((ushort)(m.order - 64));
				}
			}
			else if (m.priority == NetMessagePriority.Urgent)
			{
				if (!_receivedUrgentPackets.Contains(m.order))
				{
					NetMessageReceived(m);
					_receivedUrgentPackets.Add(m.order);
					_receivedUrgentPackets.Remove((ushort)(m.order - 64));
				}
			}
			else
			{
				NetMessageReceived(m);
			}
		}
	}

	public void NetMessageReceived(NetMessage m)
	{
		if (m.priority != NetMessagePriority.ReliableOrdered)
		{
			ProcessReceivedMessage(m);
		}
	}

	public void ProcessReceivedMessage(NetMessage m)
	{
		NetworkConnection.context = _connection;
		Main.codeNumber = m.typeIndex;
		if (m.manager == BelongsToManager.GhostManager)
		{
			GhostManager.context.OnMessage(m);
		}
		else if (m.manager == BelongsToManager.EventManager)
		{
			_eventManager.OnMessage(m);
		}
		else if (m.manager == BelongsToManager.DuckNetwork)
		{
			DuckNetwork.OnMessage(m);
		}
		else if (m.manager == BelongsToManager.None)
		{
			Network.OnMessageStatic(m);
		}
		Main.codeNumber = 12345;
		NetworkConnection.context = null;
	}

	public void QueueMessage(NetMessage msg)
	{
		if (msg.queued)
		{
			DevConsole.Log(DCSection.NetCore, "Message has been queued twice! This shouldn't happen!! (" + msg.GetType().Name + ")");
			return;
		}
		if (msg.levelIndex == byte.MaxValue)
		{
			msg.levelIndex = DuckNetwork.levelIndex;
		}
		msg.queued = true;
		msg.connection = connection;
		lock (_freshMessages)
		{
			_freshMessages.Add(msg);
		}
	}

	public void SendImmediatelyUnreliable(NetMessage pMessage)
	{
		if (connection != null && connection.data != null)
		{
			pMessage.connection = connection;
			BitBuffer buffer = new BitBuffer();
			NetworkPacket packet = new NetworkPacket(buffer, _connection, GetPacketOrder());
			buffer.Write(val: true);
			WriteMessageData(pMessage, buffer);
			pMessage.lastTransmitted = Graphics.frame;
			pMessage.packet = packet;
			buffer.Write(val: false);
			Network.activeNetwork.core.SendPacket(packet, _connection);
		}
	}

	public void Update()
	{
		if (_lossAccumulator > 0)
		{
			_lossAccumulatorInc++;
			if (_lossAccumulatorInc > 8)
			{
				_lossAccumulatorInc = 0;
				_lossAccumulator--;
			}
			if (_lossAccumulator > 30)
			{
				_lossAccumulator = 30;
			}
		}
		if (connection.status == ConnectionStatus.Disconnecting || connection.status == ConnectionStatus.Disconnected)
		{
			return;
		}
		_eventManager.Update();
		bool found;
		do
		{
			found = false;
			new Queue<NetMessage>();
			for (int i = 0; i < _orderedPackets.Count; i++)
			{
				NetMessage p = _orderedPackets[i];
				p.timeout += Maths.IncFrameTimer();
				if (p.timeout > 2f && p.timeout < 3f)
				{
					p.timeout = 1000f;
					DevConsole.Log(DCSection.DuckNet, "@disconnect Ordered message |WHITE|" + p.ToString() + "|PREV| (" + p.order + "->" + expectedReliableOrder + ") Has been stuck in queue for 2 seconds...", p.connection);
				}
				if (p.order > _expectedReliableOrder)
				{
					continue;
				}
				if (p.order == _expectedReliableOrder)
				{
					IncrementExpectedOrder();
				}
				if (p.serializedData != null)
				{
					NetworkConnection.context = p.connection;
					p.Deserialize(p.serializedData);
					p.ClearSerializedData();
					NetworkConnection.context = null;
				}
				if (p is NMMessageFragment)
				{
					NMMessageFragment frag = p as NMMessageFragment;
					if (frag.finalFragment)
					{
						NetMessage trueMessage = frag.Finish(currentFragmentCollection);
						if (trueMessage != null)
						{
							_orderedPackets[i] = trueMessage;
							DevConsole.Log(DCSection.DuckNet, "@received |DGGREEN|NMMessageFragment assembled (" + trueMessage.GetType().ToString() + ")");
							i--;
							currentFragmentCollection.Clear();
							continue;
						}
						currentFragmentCollection.Clear();
					}
					else
					{
						currentFragmentCollection.Add(frag);
					}
				}
				if (p is ConditionalMessage && !(p as ConditionalMessage).Update())
				{
					break;
				}
				if (!p.activated)
				{
					DevConsole.Log(DCSection.DuckNet, "@received Activating |WHITE|" + p.ToString() + "|PREV| (" + p.order + "->" + expectedReliableOrder + ")", p.connection);
					ProcessReceivedMessage(p);
					p.activated = true;
				}
				if (!p.MessageIsCompleted())
				{
					break;
				}
				_orderedPackets.RemoveAt(i);
				i--;
				found = true;
			}
		}
		while (found);
	}

	private ushort GetPacketOrder()
	{
		_packetOrder = (ushort)((_packetOrder + 1) % 65535);
		if (_packetOrder == 0)
		{
			DevConsole.Log(DCSection.NetCore, "@error !!Packet order index wrapped!!@error");
		}
		return _packetOrder;
	}

	private ushort GetReliableOrder()
	{
		ushort reliableOrder = _reliableOrder;
		_reliableOrder = (ushort)((_reliableOrder + 1) % 65535);
		if (_reliableOrder == 0)
		{
			DevConsole.Log(DCSection.NetCore, "@error !!Reliable message order wrapped!!@error");
			_previousReliableMessageSizes.Clear();
		}
		return reliableOrder;
	}

	private ushort GetVolatileID()
	{
		ushort volatileID = _volatileID;
		_volatileID = (ushort)((_volatileID + 1) % 65535);
		return volatileID;
	}

	private ushort GetUrgentID()
	{
		ushort urgentID = _urgentID;
		_urgentID = (ushort)((_urgentID + 1) % 65535);
		return urgentID;
	}

	public void WriteMessageData(NetMessage pMessage, BitBuffer pData)
	{
		pData.Write(pMessage.order);
		Mod m = ModLoader.GetModFromTypeIgnoreCore(pMessage.GetType());
		if (m != null && m is DisabledMod)
		{
			pData.Write((byte)4);
			pData.Write(pMessage.serializedData);
			pData.Write((byte)pMessage.priority);
			pData.Write(m.identifierHash);
		}
		else
		{
			pData.Write((byte)pMessage.priority);
			if (pMessage.priority == NetMessagePriority.ReliableOrdered)
			{
				pData.Write(pMessage.serializedData);
			}
			else
			{
				pData.WriteBufferData(pMessage.serializedData);
			}
		}
	}

	public void Flush(bool sendUnacknowledged, bool pSkipUnacknowledged = false)
	{
		if ((_unacknowledgedMessages.Count == 0 && _freshMessages.Count == 0) || (_freshMessages.Count == 0 && !sendUnacknowledged))
		{
			return;
		}
		lock (_unacknowledgedMessages)
		{
			lock (_freshMessages)
			{
				if (!pSkipUnacknowledged)
				{
					_retransmitCycle++;
					foreach (NetMessage m in _unacknowledgedMessages)
					{
						if (currentPacket.data.lengthInBytes > 400)
						{
							DevConsole.Log(DCSection.DuckNet, "@error |DGRED|Large retransmit! (" + currentPacket.data.lengthInBytes + ")", connection);
							break;
						}
						if (m.priority != NetMessagePriority.Urgent || m.timesRetransmitted >= 2)
						{
							int framesToWait = (int)(MathHelper.Clamp(ping, 0.064f, 1f) * 60f) + 1;
							if (m.serializedData.lengthInBytes > 500)
							{
								framesToWait += 30;
							}
							if (m.lastTransmitted + framesToWait > Graphics.frame)
							{
								continue;
							}
						}
						m.packetsActive.Add(currentPacket.order);
						currentPacket.data.Write(val: true);
						WriteMessageData(m, currentPacket.data);
						m.lastTransmitted = Graphics.frame;
						m.timesRetransmitted++;
					}
				}
				for (int i = 0; i < _freshMessages.Count && currentPacket.data.lengthInBytes <= 1000; i++)
				{
					NetMessage m2 = _freshMessages[i];
					if (connection.status != ConnectionStatus.Connected && !(m2 is IConnectionMessage))
					{
						if (_prevSendStatus != connection.status)
						{
							_prevSendStatus = connection.status;
							DevConsole.Log(DCSection.DuckNet, "|DGRED|Holding back queued messages until a connection is established.", connection);
						}
						continue;
					}
					if (connection.levelIndex != byte.MaxValue && connection.levelIndex != m2.levelIndex && !(m2 is IConnectionMessage) && m2.priority != NetMessagePriority.ReliableOrdered)
					{
						if (m2.levelIndex < connection.levelIndex)
						{
							_freshMessages.Remove(m2);
							i--;
						}
						continue;
					}
					switch (m2.priority)
					{
					case NetMessagePriority.Urgent:
						m2.order = GetUrgentID();
						break;
					case NetMessagePriority.ReliableOrdered:
						if (!(m2 is INetworkChunk) && NMMessageFragment.FragmentsRequired(m2) > 1)
						{
							int originalI = i;
							_freshMessages.RemoveAt(i);
							foreach (NMMessageFragment f in NMMessageFragment.BreakApart(m2))
							{
								f.Serialize();
								_freshMessages.Insert(i, f);
								i++;
							}
							i = originalI - 1;
							continue;
						}
						m2.order = GetReliableOrder();
						DevConsole.Log(DCSection.DuckNet, "@sent Sent |WHITE|" + m2.ToString() + "|PREV| (" + m2.order + ")", connection);
						break;
					case NetMessagePriority.Volatile:
						m2.order = GetVolatileID();
						break;
					}
					m2.packetsActive.Add(currentPacket.order);
					currentPacket.data.Write(val: true);
					WriteMessageData(m2, currentPacket.data);
					m2.lastTransmitted = Graphics.frame;
					m2.packet = currentPacket;
					if (m2.priority != NetMessagePriority.UnreliableUnordered && m2.priority != NetMessagePriority.Volatile)
					{
						_unacknowledgedMessages.Add(m2);
					}
					else if (m2.priority == NetMessagePriority.Volatile)
					{
						m2.timeout = Math.Min(Math.Max(ping * 1.3f, 0.1f), 2f);
						_unreliableMessages.Add(m2);
					}
					_freshMessages.Remove(m2);
					i--;
				}
			}
		}
		if (currentPacket.data.lengthInBits > 0)
		{
			currentPacket.data.Write(val: false);
			_sent++;
			Network.activeNetwork.core.SendPacket(currentPacket, _connection);
			_currentPacketInternal = null;
		}
	}
}
