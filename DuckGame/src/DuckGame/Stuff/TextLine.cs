using System.Collections.Generic;

namespace DuckGame;

public class TextLine
{
    public List<TextSegment> segments = new List<TextSegment>();

    public Color lineColor = Color.White;

    public string text
    {
        get
        {
            string full = "";
            foreach (TextSegment segment in segments)
            {
                full += segment.text;
            }
            return full;
        }
    }

    public void Add(char letter)
    {
        if (segments.Count == 0)
        {
            segments.Add(new TextSegment
            {
                color = lineColor
            });
        }
        segments[0].text += letter;
    }

    public void Add(string val)
    {
        if (segments.Count == 0)
        {
            segments.Add(new TextSegment
            {
                color = lineColor
            });
        }
        segments[0].text += val;
    }

    public void SwitchColor(Color c)
    {
        lineColor = c;
        if (segments.Count > 0 && segments[segments.Count - 1].text.Length == 0)
        {
            segments[segments.Count - 1].color = c;
            return;
        }
        segments.Insert(0, new TextSegment
        {
            color = c
        });
    }

    public int Length()
    {
        int length = 0;
        foreach (TextSegment segment in segments)
        {
            int len = 0;
            for (int i = 0; i < segment.text.Length; i++)
            {
                if (segment.text[i] == '@')
                {
                    for (i++; segment.text[i] != '@'; i++)
                    {
                    }
                }
                else
                {
                    len++;
                }
            }
            length += len;
        }
        return length;
    }
}
