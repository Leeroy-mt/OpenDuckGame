using System;

namespace DuckGame;

public static class ChallengeRando
{
	private static Random _randomGenerator;

	public static Random generator
	{
		get
		{
			return _randomGenerator;
		}
		set
		{
			_randomGenerator = value;
		}
	}

	public static void DoInitialize()
	{
		_randomGenerator = new Random();
	}

	public static long Long(long min = long.MinValue, long max = long.MaxValue)
	{
		if (_randomGenerator == null)
		{
			DoInitialize();
		}
		byte[] buf = new byte[8];
		_randomGenerator.NextBytes(buf);
		return Math.Abs(BitConverter.ToInt64(buf, 0) % (max - min)) + min;
	}

	public static double Double()
	{
		return _randomGenerator.NextDouble();
	}

	public static float Float(float max)
	{
		return (float)_randomGenerator.NextDouble() * max;
	}

	public static float Float(float min, float max)
	{
		return min + (float)_randomGenerator.NextDouble() * (max - min);
	}

	public static int Int(int _max)
	{
		return _randomGenerator.Next(0, _max + 1);
	}

	public static int Int(int min, int max)
	{
		return _randomGenerator.Next(min, max + 1);
	}

	public static int ChooseInt(params int[] _ints)
	{
		return _ints[Rando.Int(_ints.Length - 1)];
	}

	public static float ChooseFloat(params float[] _ints)
	{
		return _ints[Rando.Int(_ints.Length)];
	}
}
