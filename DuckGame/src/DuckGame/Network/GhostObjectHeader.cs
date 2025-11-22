namespace DuckGame;

public struct GhostObjectHeader
{
	public NetworkConnection connection;

	public NetIndex16 id;

	public ushort classID;

	public byte levelIndex;

	public NetIndex8 authority;

	public NetIndex16 tick;

	public bool delta;

	public NetMessagePriority priority
	{
		get
		{
			if (!delta)
			{
				return NetMessagePriority.ReliableOrdered;
			}
			return NetMessagePriority.UnreliableUnordered;
		}
	}

	public GhostObjectHeader(bool pClean)
	{
		id = default(NetIndex16);
		classID = 0;
		levelIndex = 0;
		authority = 0;
		tick = 2;
		delta = false;
		connection = null;
	}

	public static void Serialize(BitBuffer pBuffer, GhostObject pGhost, NetIndex16 pTick, bool pDelta, bool pMinimal)
	{
		pBuffer.Write((ushort)(int)pGhost.ghostObjectIndex);
		pBuffer.Write((object)pGhost.thing.authority);
		if (pDelta)
		{
			pBuffer.Write(val: true);
			pBuffer.Write((ushort)(int)pTick);
			return;
		}
		pBuffer.Write(val: false);
		if (pGhost.thing.connection.profile != null)
		{
			pBuffer.Write(pGhost.thing.connection.profile.networkIndex);
		}
		else
		{
			pBuffer.Write(byte.MaxValue);
		}
		if (!pMinimal)
		{
			pBuffer.Write(Editor.IDToType[pGhost.thing.GetType()]);
			pBuffer.Write(DuckNetwork.levelIndex);
		}
	}

	public static GhostObjectHeader Deserialize(BitBuffer pBuffer, bool pMinimal)
	{
		GhostObjectHeader h = new GhostObjectHeader(pClean: true);
		h.id = pBuffer.ReadUShort();
		h.authority = pBuffer.ReadByte();
		if (pBuffer.ReadBool())
		{
			h.tick = pBuffer.ReadUShort();
			h.delta = true;
		}
		else
		{
			byte idx = pBuffer.ReadByte();
			if (idx != byte.MaxValue)
			{
				h.connection = DuckNetwork.profiles[idx].connection;
			}
			if (!pMinimal)
			{
				h.classID = pBuffer.ReadUShort();
				h.levelIndex = pBuffer.ReadByte();
			}
		}
		return h;
	}
}
