namespace DuckGame;

public class DuckFlagBinding : StateFlagBase
{
	public override ushort ushortValue
	{
		get
		{
			_value = 0;
			Duck obj = _thing as Duck;
			if (obj.invincible)
			{
				_value |= 1024;
			}
			if (obj.crouch)
			{
				_value |= 512;
			}
			if (obj.sliding)
			{
				_value |= 256;
			}
			if (obj.jumping)
			{
				_value |= 128;
			}
			if (obj._hovering)
			{
				_value |= 64;
			}
			if (obj.immobilized)
			{
				_value |= 32;
			}
			if (obj._canFire)
			{
				_value |= 16;
			}
			if (obj.afk)
			{
				_value |= 8;
			}
			if (obj.listening)
			{
				_value |= 4;
			}
			if (obj.beammode)
			{
				_value |= 2;
			}
			if (obj.eyesClosed)
			{
				_value |= 1;
			}
			return _value;
		}
		set
		{
			_value = value;
			Duck obj = _thing as Duck;
			obj.invincible = (_value & 0x400) != 0;
			obj.crouch = (_value & 0x200) != 0;
			obj.sliding = (_value & 0x100) != 0;
			obj.jumping = (_value & 0x80) != 0;
			obj._hovering = (_value & 0x40) != 0;
			obj.immobilized = (_value & 0x20) != 0;
			obj._canFire = (_value & 0x10) != 0;
			obj.afk = (_value & 8) != 0;
			obj.listening = (_value & 4) != 0;
			obj.beammode = (_value & 2) != 0;
			obj.eyesClosed = (_value & 1) != 0;
		}
	}

	public DuckFlagBinding(GhostPriority p)
		: base(p, 11)
	{
	}
}
