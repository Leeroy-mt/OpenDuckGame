namespace DuckGame;

public class BananaFlagBinding : StateFlagBase
{
	public override ushort ushortValue
	{
		get
		{
			_value = 0;
			Banana obj = _thing as Banana;
			if (obj._pin)
			{
				_value |= 2;
			}
			if (obj._thrown)
			{
				_value |= 1;
			}
			return _value;
		}
		set
		{
			_value = value;
			Banana obj = _thing as Banana;
			obj._pin = (_value & 2) != 0;
			obj._thrown = (_value & 1) != 0;
		}
	}

	public BananaFlagBinding(GhostPriority p = GhostPriority.Normal)
		: base(p, 2)
	{
	}
}
