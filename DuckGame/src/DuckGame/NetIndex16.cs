using System;
using System.Diagnostics;

namespace DuckGame;

[DebuggerDisplay("Index = {_index}")]
public struct NetIndex16 : IComparable
{
	public int _index;

	public int max;

	private bool _zeroSpecial;

	public override string ToString()
	{
		return Convert.ToString(_index);
	}

	public static int MaxForBits(int bits)
	{
		int val = 0;
		for (int i = 0; i < bits; i++)
		{
			val |= 1 << i;
		}
		return val;
	}

	public NetIndex16(int index = 1, bool zeroSpecial = true)
	{
		_index = index;
		_zeroSpecial = zeroSpecial;
		max = MaxForBits(16);
		if (!_zeroSpecial)
		{
			max++;
		}
	}

	public void Increment()
	{
		_index = Mod(_index + 1);
	}

	public int Mod(int val)
	{
		if (_zeroSpecial)
		{
			return Math.Max(val % max, 1);
		}
		return val % max;
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (obj is NetIndex16 otherIndex)
		{
			if (this < otherIndex)
			{
				return -1;
			}
			if (this > otherIndex)
			{
				return 1;
			}
			return 0;
		}
		int otherIndex2 = (int)obj;
		if (this < otherIndex2)
		{
			return -1;
		}
		if (this > otherIndex2)
		{
			return 1;
		}
		return 0;
	}

	public static int Difference(NetIndex16 c1, NetIndex16 c2)
	{
		int sub = ((int)c1 - c1.max / 2) % c1.max;
		if (sub < 0)
		{
			sub = c1.max + sub;
		}
		int valAdd = c1.max - sub;
		return (c1._index + valAdd) % c1.max - (int)(c2 + valAdd) % c1.max;
	}

	public static implicit operator NetIndex16(int val)
	{
		return new NetIndex16(val);
	}

	public static implicit operator int(NetIndex16 val)
	{
		return val._index;
	}

	public static NetIndex16 operator +(NetIndex16 c1, int c2)
	{
		c1._index = c1.Mod(c1._index + c2);
		return c1;
	}

	public static NetIndex16 operator ++(NetIndex16 c1)
	{
		c1._index = c1.Mod(c1._index + 1);
		return c1;
	}

	public static bool operator <(NetIndex16 c1, NetIndex16 c2)
	{
		int sub = ((int)c1 - c1.max / 2) % c1.max;
		if (sub < 0)
		{
			sub = c1.max + sub;
		}
		int valAdd = c1.max - sub;
		return (c1._index + valAdd) % c1.max < (c2._index + valAdd) % c1.max;
	}

	public static bool operator >(NetIndex16 c1, NetIndex16 c2)
	{
		if (c1 == 0)
		{
			return false;
		}
		int sub = ((int)c1 - c1.max / 2) % c1.max;
		if (sub < 0)
		{
			sub = c1.max + sub;
		}
		int valAdd = c1.max - sub;
		return (c1._index + valAdd) % c1.max > (c2._index + valAdd) % c1.max;
	}

	public static bool operator <(NetIndex16 c1, int c2)
	{
		int sub = ((int)c1 - c1.max / 2) % c1.max;
		if (sub < 0)
		{
			sub = c1.max + sub;
		}
		int valAdd = c1.max - sub;
		return (c1._index + valAdd) % c1.max < (c2 + valAdd) % c1.max;
	}

	public static bool operator >(NetIndex16 c1, int c2)
	{
		return (int)c1 > c2;
	}

	public static bool operator ==(NetIndex16 c1, NetIndex16 c2)
	{
		return c1._index == c2._index;
	}

	public static bool operator !=(NetIndex16 c1, NetIndex16 c2)
	{
		return c1._index != c2._index;
	}

	public static bool operator ==(NetIndex16 c1, int c2)
	{
		return c1._index == c2;
	}

	public static bool operator !=(NetIndex16 c1, int c2)
	{
		return c1._index != c2;
	}

	public override bool Equals(object obj)
	{
		return CompareTo(obj) == 0;
	}

	public override int GetHashCode()
	{
		return _index.GetHashCode();
	}
}
