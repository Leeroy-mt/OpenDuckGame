namespace DuckGame;

public class CoilGunFlagBinding : StateFlagBase
{
	public override ushort ushortValue
	{
		get
		{
			_value = 0;
			CoilGun obj = _thing as CoilGun;
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
			CoilGun obj = _thing as CoilGun;
			obj._charging = (_value & 4) != 0;
			obj._fired = (_value & 2) != 0;
			obj.doBlast = (_value & 1) != 0;
		}
	}

	public CoilGunFlagBinding(GhostPriority p = GhostPriority.Normal)
		: base(p, 3)
	{
	}
}
