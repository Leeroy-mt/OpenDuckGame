using System;

namespace DuckGame;

public class NetSoundsBinding : StateBinding
{
	private ushort _soundIndexBlock;

	public override Type type => typeof(ushort);

	public override object classValue
	{
		get
		{
			return byteValue;
		}
		set
		{
			byteValue = (byte)value;
		}
	}

	public override ushort ushortValue
	{
		get
		{
			return _soundIndexBlock;
		}
		set
		{
			_soundIndexBlock = value;
		}
	}

	public NetSoundsBinding(string field)
		: base(field, 2)
	{
	}

	public NetSoundsBinding(GhostPriority p, string field)
		: base(field, 2)
	{
		_priority = p;
	}
}
