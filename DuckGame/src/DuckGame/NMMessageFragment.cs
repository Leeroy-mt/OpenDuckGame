using System;
using System.Collections.Generic;

namespace DuckGame;

public class NMMessageFragment : NMEvent, INetworkChunk
{
	public Mod mod;

	public const ushort kMaxFragmentSize = 700;

	public bool finalFragment;

	public ushort length = 700;

	public BitBuffer data;

	public ushort type;

	public static int FragmentsRequired(NetMessage pMessage)
	{
		return (int)Math.Ceiling((float)pMessage.serializedData.lengthInBytes / 700f);
	}

	public static List<NMMessageFragment> BreakApart(NetMessage pMessage)
	{
		List<NMMessageFragment> parts = new List<NMMessageFragment>();
		int numParts = FragmentsRequired(pMessage);
		for (int i = 0; i < numParts; i++)
		{
			NMMessageFragment fragment = new NMMessageFragment();
			if (i == numParts - 1)
			{
				fragment.finalFragment = true;
				int totalLen = i * 700;
				fragment.length = (ushort)Math.Min(700, pMessage.serializedData.lengthInBytes - totalLen);
				fragment.mod = ModLoader.GetModFromTypeIgnoreCore(pMessage.GetType());
				fragment.type = Network.allMessageTypesToID[pMessage.GetType()];
			}
			byte[] data = new byte[fragment.length];
			Array.Copy(pMessage.serializedData.buffer, i * 700, data, 0, fragment.length);
			fragment.data = new BitBuffer(data);
			parts.Add(fragment);
		}
		return parts;
	}

	public NetMessage Finish(List<NMMessageFragment> pFragments)
	{
		BitBuffer finalData = new BitBuffer();
		foreach (NMMessageFragment frag in pFragments)
		{
			finalData.WriteBufferData(frag.data);
		}
		finalData.WriteBufferData(data);
		finalData.SeekToStart();
		NetMessage message = null;
		if (mod != null)
		{
			if (mod is CoreMod)
			{
				DevConsole.Log(DCSection.DuckNet, "|GRAY|Ignoring fragmented message from unknown client mod.");
				return null;
			}
			message = mod.constructorToMessageID[type].Invoke(null) as NetMessage;
		}
		else
		{
			message = Network.constructorToMessageID[type].Invoke(null) as NetMessage;
		}
		message.order = order;
		message.connection = base.connection;
		message.priority = priority;
		message.session = session;
		message.typeIndex = finalData.ReadUShort();
		message.order = order;
		message.packet = base.packet;
		message.SetSerializedData(finalData);
		return message;
	}

	protected override void OnSerialize()
	{
		if (finalFragment)
		{
			_serializedData.Write(val: true);
			if (mod != null)
			{
				_serializedData.Write(ushort.MaxValue);
				_serializedData.Write(type);
				_serializedData.Write(mod.identifierHash);
			}
			else
			{
				_serializedData.Write(type);
			}
		}
		else
		{
			_serializedData.Write(val: false);
		}
		_serializedData.Write(data);
	}

	public override void OnDeserialize(BitBuffer pData)
	{
		finalFragment = pData.ReadBool();
		if (finalFragment)
		{
			type = pData.ReadUShort();
			if (type == ushort.MaxValue)
			{
				type = pData.ReadUShort();
				uint modID = pData.ReadUInt();
				mod = ModLoader.GetModFromHash(modID);
				if (mod == null)
				{
					mod = CoreMod.coreMod;
				}
			}
		}
		data = pData.ReadBitBuffer();
	}
}
