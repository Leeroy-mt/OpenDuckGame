using System.Collections.Generic;

namespace DuckGame;

[FixedNetworkID(30231)]
public class NMChangeSlots : NMDuckNetworkEvent
{
	public List<byte> slots = new List<byte>();

	public bool originalConfiguration;

	public NMChangeSlots()
	{
	}

	public NMChangeSlots(List<byte> pSlots, bool pOriginalConfiguration)
	{
		slots = pSlots;
		originalConfiguration = pOriginalConfiguration;
	}

	protected override void OnSerialize()
	{
		_serializedData.Write(originalConfiguration);
		_serializedData.Write((byte)slots.Count);
		for (int i = 0; i < slots.Count; i++)
		{
			_serializedData.Write(slots[i]);
		}
		base.OnSerialize();
	}

	public override void OnDeserialize(BitBuffer msg)
	{
		originalConfiguration = msg.ReadBool();
		slots = new List<byte>();
		byte count = msg.ReadByte();
		for (int i = 0; i < count; i++)
		{
			slots.Add(msg.ReadByte());
		}
		base.OnDeserialize(msg);
	}

	public override void Activate()
	{
		if (!Network.isServer)
		{
			int idx = 0;
			foreach (byte slot in slots)
			{
				if (idx < DuckNetwork.profiles.Count)
				{
					DuckNetwork.profiles[idx].slotType = (SlotType)slot;
					if (originalConfiguration && idx < DG.MaxPlayers)
					{
						DuckNetwork.profiles[idx].originalSlotType = (SlotType)slot;
					}
				}
				idx++;
			}
		}
		base.Activate();
	}
}
