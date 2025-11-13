using System;
using System.Collections.Generic;
using System.Net;

namespace DuckGame;

public class NCNetDebug : NCBasic
{
	public static Dictionary<IPEndPoint, List<NCBasicPacket>> _socketData = new Dictionary<IPEndPoint, List<NCBasicPacket>>();

	public NCNetDebug(Network c, int networkIndex)
		: base(c, networkIndex)
	{
	}

	public override NCError OnSendPacket(byte[] data, int length, object connection)
	{
		byte[] newData = new byte[length + 8];
		BitBuffer b = new BitBuffer(newData, copyData: false);
		b.Write(2449832521355936907L);
		if (data != null)
		{
			b.Write(data, 0, length);
		}
		lock (_socketData)
		{
			List<NCBasicPacket> packets = null;
			if (!_socketData.TryGetValue(connection as IPEndPoint, out packets))
			{
				packets = (_socketData[connection as IPEndPoint] = new List<NCBasicPacket>());
			}
			packets.Add(new NCBasicPacket
			{
				data = newData,
				sender = localEndPoint
			});
			bytesThisFrame += length + 8;
			_ = bytesThisFrame;
			_ = 1600;
		}
		return null;
	}

	protected override void ReceivePackets(Queue<NCBasicPacket> packets)
	{
		try
		{
			lock (_socketData)
			{
				List<NCBasicPacket> p = null;
				if (!_socketData.TryGetValue(localEndPoint, out p))
				{
					return;
				}
				foreach (NCBasicPacket pack in p)
				{
					packets.Enqueue(pack);
				}
				p.Clear();
			}
		}
		catch (Exception)
		{
		}
	}
}
