using System;
using System.Globalization;

namespace DuckGame;

public class Change
{
    public static float ToSingle(object value)
    {
        return Convert.ToSingle(value, CultureInfo.InvariantCulture);
    }

    public static ulong ToUInt64(object value)
    {
        return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
    }

    public static int ToInt32(object value)
    {
        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    public static bool ToBoolean(object value)
    {
        return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
    }

    public static string ToString(object value)
    {
        return Convert.ToString(value, CultureInfo.InvariantCulture);
    }
}
