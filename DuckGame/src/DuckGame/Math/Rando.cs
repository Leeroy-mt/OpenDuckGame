using System;

namespace DuckGame;

public static class Rando
{
    public static Random Generator { get; set; }

    #region Public Methods

    public static void DoInitialize()
    {
        Generator = new Random();
        ChallengeRando.DoInitialize();
    }

    public static int Int(int max)
    {
        return Generator.Next(0, max + 1);
    }

    public static int Int(int min, int max)
    {
        return Generator.Next(min, max + 1);
    }

    public static uint UInt()
    {
        byte[] bytes = new byte[4];
        Generator.NextBytes(bytes);
        uint ret = BitConverter.ToUInt32(bytes, 0);
        if (ret == 0)
        {
            ret = 1u;
        }
        return ret;
    }

    public static float Float()
    {
        return Generator.NextSingle();
    }

    public static float Float(float max)
    {
        return Generator.NextSingle() * max;
    }

    public static float Float(float min, float max)
    {
        return min + Generator.NextSingle() * (max - min);
    }

    public static long Long(long min = long.MinValue, long max = long.MaxValue)
    {
        if (Generator == null)
        {
            DoInitialize();
        }
        byte[] buf = new byte[8];
        Generator.NextBytes(buf);
        return Math.Abs(BitConverter.ToInt64(buf, 0) % (max - min)) + min;
    }

    public static double Double()
    {
        return Generator.NextDouble();
    }

    public static T Choose<T>(params T[] items)
    {
        return items[Int(items.Length - 1)];
    }

    #endregion
}
