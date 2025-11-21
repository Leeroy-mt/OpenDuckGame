using System;
using System.Collections.Generic;

namespace DuckGame;

public class NMParticles : NetMessage
{
	public Dictionary<byte, List<PhysicsParticle>> particles = new Dictionary<byte, List<PhysicsParticle>>();

	public new byte levelIndex;

	public byte count;

	public Type type;

	public BitBuffer data;

	public void Add(PhysicsParticle pParticle)
	{
		byte t = PhysicsParticle.TypeToNetTypeIndex(pParticle.GetType());
		List<PhysicsParticle> parts = null;
		if (!particles.TryGetValue(t, out parts))
		{
			List<PhysicsParticle> list = (particles[t] = new List<PhysicsParticle>());
			parts = list;
		}
		parts.Add(pParticle);
	}

	public NMParticles()
	{
		manager = BelongsToManager.GhostManager;
		levelIndex = DuckNetwork.levelIndex;
	}

	public override void CopyTo(NetMessage pMessage)
	{
		(pMessage as NMParticles).particles = particles;
		base.CopyTo(pMessage);
	}

	protected override void OnSerialize()
	{
		BitBuffer d = new BitBuffer();
		d.Write(levelIndex);
		foreach (KeyValuePair<byte, List<PhysicsParticle>> pair in particles)
		{
			d.Write(pair.Key);
			d.Write((byte)pair.Value.Count);
			foreach (PhysicsParticle p in pair.Value)
			{
				d.Write(p.netIndex);
				p.NetSerialize(d);
			}
		}
		d.Write(byte.MaxValue);
		_serializedData.Write(d);
	}

	public override void OnDeserialize(BitBuffer d)
	{
		data = d.ReadBitBuffer();
		levelIndex = data.ReadByte();
	}
}
