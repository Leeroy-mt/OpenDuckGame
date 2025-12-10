using System.Collections.Generic;

namespace DuckGame;

public class TextParser
{
    private static HashSet<char> _wordSeparators = new HashSet<char>
    {
        ' ', '\n', '\r', '\t', ';', '{', '}', '.', '(', ')',
        ',', '<', '>'
    };

    public static string ReadNextWord(string s, ref int index, char? stop = null)
    {
        if (index < s.Length && stop.HasValue && s[index] == stop.Value)
        {
            return null;
        }
        while (index < s.Length && _wordSeparators.Contains(s[index]))
        {
            if (stop.HasValue && s[index] == stop.Value)
            {
                return null;
            }
            index++;
        }
        string currentWord = "";
        while (index < s.Length && !_wordSeparators.Contains(s[index]) && (!stop.HasValue || s[index] != stop.Value))
        {
            currentWord += s[index];
            index++;
        }
        return currentWord;
    }

    public static string ReadNextWordBetween(char between, string s, ref int index)
    {
        while (index < s.Length && s[index] != between)
        {
            index++;
        }
        index++;
        string currentWord = "";
        while (index < s.Length && s[index] != between)
        {
            currentWord += s[index];
            index++;
        }
        index++;
        return currentWord;
    }

    public static void SkipWord(string s, ref int index)
    {
        ReadNextWord(s, ref index);
    }

    public static string ReverseReadWord(string s, ref int index)
    {
        index--;
        while (index >= 0 && _wordSeparators.Contains(s[index]))
        {
            index--;
        }
        string currentWord = "";
        while (index >= 0 && !_wordSeparators.Contains(s[index]))
        {
            currentWord = currentWord.Insert(0, s[index].ToString() ?? "");
            index--;
        }
        index++;
        return currentWord;
    }

    public static void StepBackWord(string s, ref int index)
    {
        ReverseReadWord(s, ref index);
    }

    public static char ReadNextCharacter(string s, ref int index)
    {
        while (index < s.Length && _wordSeparators.Contains(s[index]))
        {
            index++;
        }
        if (index < s.Length)
        {
            return s[index];
        }
        return ' ';
    }

    public static string ReadNextBrace(string s, ref int index, bool ignoreSemi = false)
    {
        index++;
        while (index < s.Length && (s[index] == ' ' || s[index] == '\n' || s[index] == '\r' || s[index] == '\t'))
        {
            index++;
        }
        while (index < s.Length && s[index] != '}' && s[index] != '{' && (ignoreSemi || s[index] != ';'))
        {
            index++;
        }
        if (index < s.Length)
        {
            return s[index].ToString() ?? "";
        }
        return "}";
    }

    public static string ReadNext(char c, string s, ref int index)
    {
        while (index < s.Length && (s[index] == ' ' || s[index] == '\n' || s[index] == '\r' || s[index] == '\t'))
        {
            index++;
        }
        while (index < s.Length && s[index] != c)
        {
            index++;
        }
        if (index >= s.Length)
        {
            return null;
        }
        index++;
        return s[index - 1].ToString() ?? "";
    }

    public static string ReadTo(char c, string s, ref int index)
    {
        int idx = s.IndexOf(c, index);
        if (idx >= 0)
        {
            string result = s.Substring(index, idx - index);
            index = idx;
            return result;
        }
        return s.Substring(index, s.Length - index);
    }

    public static string ReverseReadNext(char c, string s, ref int index)
    {
        index--;
        while (index >= 0 && (s[index] == ' ' || s[index] == '\n' || s[index] == '\r' || s[index] == '\t'))
        {
            index--;
        }
        while (index >= 0 && s[index] != c)
        {
            index--;
        }
        return s[index].ToString() ?? "";
    }
}
