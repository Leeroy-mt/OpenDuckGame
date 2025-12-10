using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class DXMLNode
{
    public static int toStringDeep;

    private string _name = "";

    private string _value = "";

    private List<DXMLNode> _elements = new List<DXMLNode>();

    private List<DXMLAttribute> _attributes = new List<DXMLAttribute>();

    public string Name => _name;

    public string Value => _value;

    public int NumberOfElements => _elements.Count;

    private string StoreValue(string value)
    {
        return value.Replace("<", "@04202@").Replace(">", "@03019@").Replace("/", "@02032@");
    }

    private string ReadValue(string value)
    {
        return value.Replace("@04202@", "<").Replace("@03019@", ">").Replace("@02032@", "/");
    }

    public override string ToString()
    {
        string n = "";
        string indent = "";
        for (int i = 0; i < toStringDeep; i++)
        {
            indent += "  ";
        }
        if (Name != "")
        {
            n = n + indent + "<" + Name;
            foreach (DXMLAttribute a in Attributes())
            {
                n = n + " " + a.Name + "=" + a.Value;
            }
        }
        if (NumberOfElements > 0 || Value != "")
        {
            if (Value != "")
            {
                n = n + ">" + StoreValue(Value) + "</" + Name + ">\r\n";
            }
            else
            {
                if (Name != "")
                {
                    n += ">\r\n";
                    toStringDeep++;
                }
                foreach (DXMLNode deepnode in Elements())
                {
                    n += deepnode.ToString();
                }
                if (Name != "")
                {
                    toStringDeep--;
                    n = n + indent + "</" + Name + ">\r\n";
                }
            }
        }
        else if (Name != "")
        {
            n += "/>\r\n";
        }
        return n;
    }

    public void SetValue(string varValue)
    {
        _value = ReadValue(varValue);
    }

    public DXMLNode(string varName)
    {
        _name = varName;
    }

    public DXMLNode(string varName, object varValue)
    {
        _name = varName;
        if (varValue != null)
        {
            _value = varValue.ToString();
        }
    }

    protected static DXMLNode ReadNode(string text, ref int index)
    {
        if (text == null || text.Length <= index)
        {
            return null;
        }
        if (text[index] == '<' || TextParser.ReadNext('<', text, ref index) != null)
        {
            string nodeText = TextParser.ReadTo('>', text, ref index);
            bool endNode = false;
            int nodeRead = 0;
            string nodeName = TextParser.ReadNextWord(nodeText, ref nodeRead, '/');
            DXMLNode node;
            string attributeName;
            string attributeValue;
            DXMLAttribute a;
            for (node = new DXMLNode(nodeName); nodeRead != nodeText.Length; attributeValue = TextParser.ReadNextWordBetween('"', nodeText, ref nodeRead), a = new DXMLAttribute(attributeName, attributeValue), node.Add(a))
            {
                attributeName = TextParser.ReadTo('=', nodeText, ref nodeRead).Trim();
                switch (attributeName)
                {
                    case "/":
                        endNode = true;
                        break;
                    case "?":
                        endNode = true;
                        break;
                    default:
                        continue;
                    case "":
                        break;
                }
                break;
            }
            if (endNode)
            {
                return node;
            }
            index++;
            string value = TextParser.ReadTo('<', text, ref index).Trim();
            int tempIndex = index;
            if (TextParser.ReadNextCharacter(text, ref tempIndex) == '!' && TextParser.ReadNextWord(text, ref tempIndex) == "![CDATA[")
            {
                int tagEnd = text.IndexOf("]]>", tempIndex, text.Length - tempIndex);
                if (tagEnd != -1)
                {
                    value = text.Substring(tempIndex, tagEnd - tempIndex);
                    index = tagEnd + 2;
                }
            }
            node.SetValue(value);
            while (TextParser.ReadNext('<', text, ref index) != null)
            {
                index--;
                tempIndex = index;
                if (TextParser.ReadNextWord(text, ref tempIndex) == "/" + nodeName)
                {
                    index = tempIndex;
                    return node;
                }
                DXMLNode n = ReadNode(text, ref index);
                node.Add(n);
            }
        }
        return null;
    }

    public void Add(DXMLNode node)
    {
        _elements.Add(node);
    }

    public void Add(DXMLAttribute attribute)
    {
        _attributes.Add(attribute);
    }

    public IEnumerable<DXMLNode> Elements()
    {
        return _elements.AsEnumerable();
    }

    public IEnumerable<DXMLNode> Elements(string varName)
    {
        List<DXMLNode> els = new List<DXMLNode>();
        foreach (DXMLNode n in _elements)
        {
            if (n.Name == varName)
            {
                els.Add(n);
            }
        }
        return els;
    }

    public DXMLNode Element(string varName)
    {
        new List<DXMLNode>();
        foreach (DXMLNode n in _elements)
        {
            if (n.Name == varName)
            {
                return n;
            }
        }
        return null;
    }

    public IEnumerable<DXMLAttribute> Attributes(string varName)
    {
        List<DXMLAttribute> els = new List<DXMLAttribute>();
        foreach (DXMLAttribute n in _attributes)
        {
            if (n.Name == varName)
            {
                els.Add(n);
            }
        }
        return els;
    }

    public IEnumerable<DXMLAttribute> Attributes()
    {
        return _attributes.AsEnumerable();
    }
}
