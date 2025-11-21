using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace DuckGame;

public class NMGhostData : NetMessage, INetworkChunk
{
	public struct GhostMaskPair
	{
		public GhostObject ghost;

		public long mask;
	}

	public new byte levelIndex;

	public List<NMGhostState> states = new List<NMGhostState>();

	private List<GhostObject> _ghostSelection;

	private int _startIndex;

	private const int kMaxDataSize = 512;

	public List<GhostMaskPair> ghostMaskPairs = new List<GhostMaskPair>();

	public static NMGhostData GetSerializedGhostData(List<GhostObject> pGhosts, int pStartIndex)
	{
		NMGhostData nMGhostData = new NMGhostData();
		nMGhostData._ghostSelection = pGhosts;
		nMGhostData._startIndex = pStartIndex;
		nMGhostData.Serialize();
		return nMGhostData;
	}

	public NMGhostData()
	{
		manager = BelongsToManager.GhostManager;
	}

	protected override void OnSerialize()
	{
		_serializedData.Write(DuckNetwork.levelIndex);
		int sizeOffset = _serializedData.position;
		_serializedData.Write((byte)0);
		ushort lastIndex = ushort.MaxValue;
		for (int i = _startIndex; i < _ghostSelection.Count; i++)
		{
			GhostObject take = _ghostSelection[i];
			if (_serializedData.lengthInBytes + take.previouslySerializedData.lengthInBytes > 512)
			{
				break;
			}
			if (lastIndex == ushort.MaxValue || lastIndex != take.thing.ghostType)
			{
				lastIndex = take.thing.ghostType;
				_serializedData.Write(val: true);
				_serializedData.Write(lastIndex);
			}
			else
			{
				_serializedData.Write(val: false);
			}
			ghostMaskPairs.Add(new GhostMaskPair
			{
				ghost = take,
				mask = take.lastWrittenMask
			});
			_serializedData.Write(take.previouslySerializedData);
		}
		_serializedData.position = sizeOffset;
		_serializedData.bitOffset = 0;
		_serializedData.Write((byte)ghostMaskPairs.Count);
	}

	public override void OnDeserialize(BitBuffer pData)
	{
		levelIndex = pData.ReadByte();
		ushort num = pData.ReadByte();
		ushort typeIndex = 0;
		for (int i = 0; i < num; i++)
		{
			if (pData.ReadBool())
			{
				typeIndex = pData.ReadUShort();
			}
			BitBuffer data = pData.ReadBitBuffer();
			NMGhostState newState = new NMGhostState
			{
				minimalState = true,
				packet = base.packet
			};
			if (typeIndex == 0)
			{
				newState.header.id = data.ReadUShort();
			}
			else
			{
				newState.Deserialize(data);
			}
			newState.header.levelIndex = levelIndex;
			newState.connection = base.connection;
			newState.header.classID = typeIndex;
			if (!newState.header.delta)
			{
				newState.header.tick = 1;
			}
			states.Add(newState);
		}
	}

	private void Compress(BitBuffer pCompress)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(new GZipStream(memoryStream, CompressionMode.Compress));
		binaryWriter.Write((ushort)pCompress.lengthInBytes);
		binaryWriter.Write(pCompress.buffer, 0, pCompress.lengthInBytes);
		binaryWriter.Close();
		byte[] compressedData = memoryStream.ToArray();
		_serializedData.Write((ushort)compressedData.Length);
		_serializedData.Write(compressedData);
	}

	private BitBuffer Decompress(BitBuffer pData)
	{
		ushort size = pData.ReadUShort();
		BinaryReader binaryReader = new BinaryReader(new GZipStream(new MemoryStream(pData.ReadPacked(size)), CompressionMode.Decompress));
		ushort uncompressedSize = binaryReader.ReadUInt16();
		return new BitBuffer(binaryReader.ReadBytes(uncompressedSize));
	}
}
