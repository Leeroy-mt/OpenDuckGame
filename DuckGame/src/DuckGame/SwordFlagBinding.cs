namespace DuckGame;

public class SwordFlagBinding : StateFlagBase
{
	public override ushort ushortValue
	{
		get
		{
			_value = 0;
			Sword obj = _thing as Sword;
			if (obj._jabStance)
			{
				_value |= 16;
			}
			if (obj._crouchStance)
			{
				_value |= 8;
			}
			if (obj._slamStance)
			{
				_value |= 4;
			}
			if (obj._swinging)
			{
				_value |= 2;
			}
			if (obj._volatile)
			{
				_value |= 1;
			}
			return _value;
		}
		set
		{
			_value = value;
			Sword obj = _thing as Sword;
			obj._jabStance = (_value & 0x10) != 0;
			obj._crouchStance = (_value & 8) != 0;
			obj._slamStance = (_value & 4) != 0;
			obj._swinging = (_value & 2) != 0;
			obj._volatile = (_value & 1) != 0;
		}
	}

	public SwordFlagBinding(GhostPriority p = GhostPriority.Normal)
		: base(p, 5)
	{
	}
}
