using System;
using System.Collections.Generic;

namespace DuckGame;

public class NMNetSoundEvents : NMEvent
{
	private List<NetSoundEffect> _sounds = new List<NetSoundEffect>();

	public NMNetSoundEvents()
	{
		manager = BelongsToManager.EventManager;
		priority = NetMessagePriority.UnreliableUnordered;
	}

	public NMNetSoundEvents(List<NetSoundEffect> pSounds)
	{
		manager = BelongsToManager.EventManager;
		priority = NetMessagePriority.UnreliableUnordered;
		foreach (NetSoundEffect s in pSounds)
		{
			_sounds.Add(s);
		}
	}

	protected override void OnSerialize()
	{
		byte ct = (byte)Math.Min(_sounds.Count, 16);
		_serializedData.Write(ct);
		for (int i = 0; i < ct; i++)
		{
			_serializedData.Write(_sounds[i].sfxIndex);
		}
	}

	public override void OnDeserialize(BitBuffer d)
	{
		_sounds.Clear();
		byte ct = d.ReadByte();
		for (int i = 0; i < ct; i++)
		{
			NetSoundEffect s = NetSoundEffect.Get(d.ReadUShort());
			if (s != null)
			{
				_sounds.Add(s);
			}
		}
	}

	public override void Activate()
	{
		foreach (NetSoundEffect sound in _sounds)
		{
			sound.Play();
		}
	}
}
