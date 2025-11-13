namespace DuckGame;

public class HugeLaserFlagBinding : StateFlagBase
{
	public override ushort ushortValue
	{
		get
		{
			_value = 0;
			HugeLaser obj = _thing as HugeLaser;
			if (obj._charging)
			{
				_value |= 4;
			}
			if (obj._fired)
			{
				_value |= 2;
			}
			if (obj.doBlast)
			{
				_value |= 1;
			}
			return _value;
		}
		set
		{
			_value = value;
			HugeLaser obj = _thing as HugeLaser;
			obj._charging = (_value & 4) != 0;
			obj._fired = (_value & 2) != 0;
			obj.doBlast = (_value & 1) != 0;
		}
	}

	public HugeLaserFlagBinding(GhostPriority p = GhostPriority.Normal)
		: base(p, 3)
	{
	}
}
