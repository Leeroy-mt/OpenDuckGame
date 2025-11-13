namespace DuckGame;

public class NMNetworkTrafficGarbage : NMEvent
{
	private int _numBytes;

	public NMNetworkTrafficGarbage(int numBytes)
	{
		_numBytes = numBytes;
	}

	public NMNetworkTrafficGarbage()
	{
	}

	protected override void OnSerialize()
	{
		if (_numBytes < 4)
		{
			_numBytes = 4;
		}
		_serializedData.Write(_numBytes);
		for (int i = 0; i < _numBytes - 4; i++)
		{
			_serializedData.Write((byte)Rando.Int(255));
		}
		base.OnSerialize();
	}

	public override void OnDeserialize(BitBuffer msg)
	{
		_numBytes = msg.ReadInt();
		for (int i = 0; i < _numBytes - 4; i++)
		{
			msg.ReadByte();
		}
		base.OnDeserialize(msg);
	}
}
