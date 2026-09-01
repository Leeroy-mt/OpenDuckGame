using Steamworks;

namespace Steam;

// Common helper class that turns out to be used ~ 2 times per function...
public static class SteamHelper
{
    public static List<T> GetList<T>(int count, Func<int, T> get)
    {
        if (count <= 0)
            return [];

        List<T> list = new(count);
        for (int i = 0; i < count; i++)
            list.Add(get(i));

        return list;
    }

    public static TOut[] GetArray<TIn, TOut>(IList<TIn> list, Func<TIn, TOut> get)
    {
        if (list.Count <= 0)
            return [];

        var array = new TOut[list.Count];
        for (int i = 0; i < array.Length; i++)
            array[i] = get(list[i]);

        return array;
    }

    public static byte[]? GetImageRGBA(int id)
    {
        if (!SteamUtils.GetImageSize(id, out uint w, out uint h))
            return null;

        var data = new byte[w * h * 4];
        if (!SteamUtils.GetImageRGBA(id, data, data.Length))
            return null;

        return data;
    }
}