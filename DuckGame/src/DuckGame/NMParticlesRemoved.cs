using System;
using System.Collections.Generic;

namespace DuckGame;

public class NMParticlesRemoved : NetMessage
{
	public new byte levelIndex;

	public HashSet<ushort> removeParticles = new HashSet<ushort>();

	public NMParticlesRemoved()
	{
		manager = BelongsToManager.GhostManager;
		levelIndex = DuckNetwork.levelIndex;
	}

	public override void CopyTo(NetMessage pMessage)
	{
		(pMessage as NMParticlesRemoved).removeParticles = removeParticles;
		base.CopyTo(pMessage);
	}

	protected override void OnSerialize()
	{
		if (removeParticles.Count > 255)
		{
			throw new Exception("NMParticlesRemoved.removeParticles should not have more than 255 particles.");
		}
		_serializedData.Write(levelIndex);
		_serializedData.Write((byte)removeParticles.Count);
		foreach (ushort u in removeParticles)
		{
			_serializedData.Write(u);
		}
	}

	public override void OnDeserialize(BitBuffer d)
	{
		levelIndex = d.ReadByte();
		byte count = d.ReadByte();
		for (int i = 0; i < count; i++)
		{
			removeParticles.Add(d.ReadUShort());
		}
	}
}
