using System.Net;

namespace DuckGame;

public class NMPeerInformation : NMEvent
{
	public int port;

	public IPAddress address;

	public NMPeerInformation()
	{
	}

	public NMPeerInformation(IPAddress vaddress, int vport)
	{
		address = vaddress;
		port = vport;
	}

	protected override void OnSerialize()
	{
		byte[] addressBytes = address.GetAddressBytes();
		BitBuffer b = new BitBuffer();
		b.Write(addressBytes);
		_serializedData.Write(b);
		_serializedData.Write(port);
	}

	public override void OnDeserialize(BitBuffer d)
	{
		byte[] addressBytes = d.ReadBitBuffer().buffer;
		address = new IPAddress(addressBytes);
		port = d.ReadInt();
	}
}
