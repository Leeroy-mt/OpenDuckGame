using System.Collections.Generic;

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
}
