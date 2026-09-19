using System;
using System.Collections.Generic;
using System.Text;

namespace DuckGame;

internal class Utils
{
    public static List<T> Shuffle<T>(List<T> list)
    {
        List<T> shuffled = [.. list];
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

    public static string RemoveColorTags(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '|')
            {
                int startIndex = i;
                for (i++; i < s.Length && s[i] != '|'; i++) ;
                if (i < s.Length && s[i] == '|')
                {
                    s = s.Remove(startIndex, i - startIndex + 1);
                    i = -1;
                }
            }
        }
        return s;
    }

    public static int GetHexVal(char hex) =>
        hex - ((hex < ':') ? 48 : 55);
}