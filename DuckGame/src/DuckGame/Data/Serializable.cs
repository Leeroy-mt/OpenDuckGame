using System;
using System.Reflection;

namespace DuckGame;

public class Serializable
{
    public void SerializeField(BinaryClassChunk element, string name)
    {
        ClassMember member = Editor.GetMember(GetType(), name);
        if (member != null)
        {
            element.AddProperty(name, member.GetValue(this));
        }
    }

    public void DeserializeField(BinaryClassChunk node, string name)
    {
        Editor.GetMember(GetType(), name)?.SetValue(this, node.GetProperty(name));
    }

    public void LegacySerializeField(DXMLNode element, string name)
    {
        object val = null;
        FieldInfo f = GetType().GetField(name);
        if (f != null)
        {
            val = f.GetValue(this);
        }
        else
        {
            PropertyInfo p = GetType().GetProperty(name);
            if (!(p != null))
            {
                return;
            }
            val = p.GetValue(this, null);
        }
        if (val.GetType().IsEnum)
        {
            val = Enum.GetName(val.GetType(), val);
        }
        element.Add(new DXMLNode(name, val));
    }

    public void LegacyDeserializeField(DXMLNode node, string name)
    {
        try
        {
            DXMLNode getNode = node.Element(name);
            if (getNode == null)
            {
                return;
            }
            FieldInfo f = GetType().GetField(name);
            if (f != null)
            {
                if (f.FieldType.IsEnum)
                {
                    f.SetValue(this, Enum.Parse(f.FieldType, getNode.Value));
                }
                else
                {
                    f.SetValue(this, Convert.ChangeType(getNode.Value, f.FieldType));
                }
                return;
            }
            PropertyInfo p = GetType().GetProperty(name);
            if (p != null)
            {
                if (p.PropertyType.IsEnum)
                {
                    p.SetValue(this, Enum.Parse(p.PropertyType, getNode.Value), null);
                }
                else
                {
                    p.SetValue(this, Convert.ChangeType(getNode.Value, p.PropertyType), null);
                }
            }
        }
        catch
        {
        }
    }
}
