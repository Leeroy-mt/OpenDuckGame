namespace DuckGame.Compatibility;

internal static class NetFramework
{
    // algorithm from https://github.com/microsoft/referencesource/blob/user/rbhanda/NET4.8/mscorlib/system/string.cs

    public static int GetHashCode(string str)
    {
        unsafe
        {
            fixed (char* src = str)
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                int* pint = (int*)src;
                int len = str.Length;
                while (len > 2)
                {
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                    pint += 2;
                    len -= 4;
                }

                if (len > 0)
                {
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                }
                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
