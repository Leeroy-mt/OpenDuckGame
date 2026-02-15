using System;
using System.Collections.Generic;
using System.Text;

namespace DuckGame;

internal class Utils
{
    public static List<T> Shuffle<T>(List<T> list)
    {
        List<T> shuffled = new List<T>(list);
        for (int i = 0; i < shuffled.Count - 1; i++)
        {
            int victim_idx = i + Rando.Int(shuffled.Count - 1 - i);
            T temp = shuffled[victim_idx];
            shuffled[victim_idx] = shuffled[i];
            shuffled[i] = temp;
        }
        return shuffled;
    }

    public static string Base64Decode(string base64EncodedData)
    {
        byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public static string Base64Encode(string plainText)
    {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
}
