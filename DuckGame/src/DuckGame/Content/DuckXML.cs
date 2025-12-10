using System.IO;
using System.Text;

namespace DuckGame;

public class DuckXML : DXMLNode
{
    public bool invalid;

    public DuckXML()
        : base("")
    {
    }

    public void Save(string file)
    {
    }

    public static DuckXML Load(Stream s)
    {
        s.Position = 0L;
        return FromString(new StreamReader(s).ReadToEnd());
    }

    public static DuckXML Load(byte[] data)
    {
        return FromString(Encoding.UTF8.GetString(data));
    }

    public static DuckXML Load(string file)
    {
        return FromString(File.ReadAllText(file, Encoding.UTF8));
    }

    public static DuckXML FromString(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            DevConsole.Log("|DGRED|DuckXML.FromString called with empty string!");
        }
        DuckXML dxml = new DuckXML();
        int index = 0;
        while (true)
        {
            DXMLNode node = DXMLNode.ReadNode(text, ref index);
            if (node == null)
            {
                break;
            }
            dxml.Add(node);
        }
        if (dxml.NumberOfElements == 0)
        {
            dxml.invalid = true;
        }
        return dxml;
    }
}
