using System.Collections.Generic;

namespace DuckGame;

public class DataLayerDebug : DataLayer
{
	public class BadConnection
	{
		public class DelayedPacket
		{
			public BitBuffer data;

			public float time;
		}

		public int lagSpike;

		private float _latency;

		private float _jitter;

		private float _loss;

		private float _duplicate;

		public NetworkConnection connection;

		public List<DelayedPacket> packets = new List<DelayedPacket>();

		private int i;

		public float latency
		{
			get
			{
				if (_latency == 0f)
				{
					return DuckNetwork.localConnection.debuggerContext._latency;
				}
				return _latency;
			}
			set
			{
				_latency = value;
			}
		}

		public float jitter
		{
			get
			{
				if (_jitter == 0f)
				{
					return DuckNetwork.localConnection.debuggerContext._jitter;
				}
				return _jitter;
			}
			set
			{
				_jitter = value;
			}
		}

		public float loss
		{
			get
			{
				if (_loss == 0f)
				{
					return DuckNetwork.localConnection.debuggerContext._loss;
				}
				return _loss;
			}
			set
			{
				_loss = value;
			}
		}

		public float duplicate
		{
			get
			{
				if (_duplicate == 0f)
				{
					return DuckNetwork.localConnection.debuggerContext._duplicate;
				}
				return _duplicate;
			}
			set
			{
				_duplicate = value;
			}
		}

		public BadConnection(NetworkConnection pContext)
		{
			connection = pContext;
		}

		public float CalculateLatency()
		{
			float additionalLatency = 0f;
			if (CalculateLoss())
			{
				if (Rando.Int(3) != 0)
				{
					return float.MaxValue;
				}
				additionalLatency += Rando.Float(2f, 4f);
			}
			float randomExtra = 0f;
			return latency + randomExtra - 0.016f + Rando.Float(0f - jitter, jitter) + additionalLatency;
		}

		public bool CalculateLoss()
		{
			if (loss != 0f)
			{
				return Rando.Float(1f) < loss;
			}
			return false;
		}

		public bool Update(NCNetworkImplementation pNetwork)
		{
			List<DelayedPacket> removePackets = new List<DelayedPacket>();
			foreach (DelayedPacket packet in packets)
			{
				packet.time -= Maths.IncFrameTimer();
				if (packet.time <= 0f && connection.debuggerContext.lagSpike <= 0)
				{
					pNetwork.OnSendPacket(packet.data.buffer, packet.data.lengthInBytes, connection.data);
					removePackets.Add(packet);
				}
			}
			foreach (DelayedPacket packet2 in removePackets)
			{
				if (Rando.Int(15) == 0)
				{
					packet2.time = Rando.Float(2f, 5f);
				}
				else
				{
					packets.Remove(packet2);
				}
			}
			if (lagSpike > 0)
			{
				lagSpike -= 9;
			}
			return packets.Count == 0;
		}

		public void Reset()
		{
			packets.Clear();
		}
	}

	private bool sendingDuplicate;

	public DataLayerDebug(NCNetworkImplementation pImpl)
		: base(pImpl)
	{
	}

	public override NCError SendPacket(BitBuffer sendData, NetworkConnection connection)
	{
		if (!sendingDuplicate && Rando.Float(1f) < connection.debuggerContext.duplicate)
		{
			sendingDuplicate = true;
			SendPacket(sendData, connection);
			if (connection.debuggerContext.duplicate > 0.4f && Rando.Float(1f) < connection.debuggerContext.duplicate)
			{
				SendPacket(sendData, connection);
			}
			if (connection.debuggerContext.duplicate > 0.8f && Rando.Float(1f) < connection.debuggerContext.duplicate)
			{
				SendPacket(sendData, connection);
			}
			sendingDuplicate = false;
		}
		float latency = connection.debuggerContext.CalculateLatency();
		if (connection.debuggerContext.lagSpike > 0)
		{
			latency = 0.001f;
		}
		if (latency == float.MaxValue)
		{
			return null;
		}
		if (latency > 0f)
		{
			connection.debuggerContext.packets.Add(new BadConnection.DelayedPacket
			{
				data = sendData,
				time = latency
			});
			return null;
		}
		return _impl.OnSendPacket(sendData.buffer, sendData.lengthInBytes, connection.data);
	}

	public override void Update()
	{
	}
}
