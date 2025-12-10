using System;

namespace DuckGame;

public static class Rando
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
        ChallengeRando.DoInitialize();
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

    public static Vec2 Vec2(float minX, float maxX, float minY, float maxY)
    {
        return new Vec2(Float(minX, maxX), Float(minY, maxY));
    }

    public static Vec2 Vec2(Vec2 spanX, Vec2 spanY)
    {
        return new Vec2(Float(spanX.x, spanX.y), Float(spanY.x, spanY.y));
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
        return _ints[Int(_ints.Length - 1)];
    }

    public static float ChooseFloat(params float[] _ints)
    {
        return _ints[Int(_ints.Length)];
    }

    public static uint UInt()
    {
        byte[] bytes = new byte[4];
        _randomGenerator.NextBytes(bytes);
        uint ret = BitConverter.ToUInt32(bytes, 0);
        if (ret == 0)
        {
            ret = 1u;
        }
        return ret;
    }
}
