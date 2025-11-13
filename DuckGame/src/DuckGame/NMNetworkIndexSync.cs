using System.Collections.Generic;

namespace DuckGame;

public class NMNetworkIndexSync : NMEvent
{
	public List<byte> indexes = new List<byte>();

	protected override void OnSerialize()
	{
		for (int i = 0; i < DuckNetwork.profiles.Count; i++)
		{
			_serializedData.Write(DuckNetwork.profiles[i].fixedGhostIndex);
		}
	}

	public override void OnDeserialize(BitBuffer msg)
	{
		for (int i = 0; i < DuckNetwork.profiles.Count; i++)
		{
			indexes.Add(msg.ReadByte());
		}
	}

	public override void Activate()
	{
		for (int i = 0; i < indexes.Count; i++)
		{
			DuckNetwork.profiles[i].SetFixedGhostIndex(indexes[i]);
		}
		DuckNetwork.core.ReorderFixedList();
	}
}
