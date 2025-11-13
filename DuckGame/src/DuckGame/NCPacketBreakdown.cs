using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class NCPacketBreakdown
{
	private static IEnumerable<NCPacketDataType> _dataTypes;

	private Dictionary<NCPacketDataType, int> _bitsPerType = new Dictionary<NCPacketDataType, int>();

	public static IEnumerable<NCPacketDataType> dataTypes
	{
		get
		{
			if (_dataTypes == null)
			{
				_dataTypes = Enum.GetValues(typeof(NCPacketDataType)).Cast<NCPacketDataType>();
			}
			return _dataTypes;
		}
	}

	public NCPacketBreakdown()
	{
		if (_dataTypes == null)
		{
			_dataTypes = Enum.GetValues(typeof(NCPacketDataType)).Cast<NCPacketDataType>();
		}
		foreach (NCPacketDataType t in _dataTypes)
		{
			_bitsPerType[t] = 0;
		}
	}

	public void Add(NCPacketDataType type, int bits)
	{
		_bitsPerType[type] += bits;
	}

	public int Get(NCPacketDataType type)
	{
		return _bitsPerType[type];
	}

	public static Color GetTypeColor(NCPacketDataType type)
	{
		return type switch
		{
			NCPacketDataType.Ack => Color.Lime, 
			NCPacketDataType.Event => Color.Blue, 
			NCPacketDataType.Ghost => Color.Red, 
			NCPacketDataType.InputStream => Color.Pink, 
			NCPacketDataType.ExtraData => Color.White, 
			_ => Color.Yellow, 
		};
	}
}
