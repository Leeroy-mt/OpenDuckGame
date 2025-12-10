using System;
using System.Collections.Generic;

namespace DuckGame;

public class StateFlagBinding : StateBinding
{
    private string[] _fields;

    private List<AccessorInfo> _accessors = new List<AccessorInfo>();

    private ushort _value;

    public override Type type => typeof(ushort);

    public override object classValue
    {
        get
        {
            _value = 0;
            bool first = true;
            foreach (AccessorInfo info in _accessors)
            {
                if (!first)
                {
                    _value <<= 1;
                }
                _value |= (ushort)(((bool)info.getAccessor(_thing)) ? 1 : 0);
                first = false;
            }
            return _value;
        }
        set
        {
            _value = (ushort)value;
            byte currentBit = 1;
            foreach (AccessorInfo accessor in _accessors)
            {
                bool val = (_value & (1L << _bits - currentBit)) != 0;
                accessor.setAccessor(_thing, val);
                currentBit++;
            }
        }
    }

    public override string ToString()
    {
        return GetDebugString(null);
    }

    public override string GetDebugString(object with)
    {
        string text = "";
        int idx = 0;
        byte currentBit = 1;
        string[] fields = _fields;
        foreach (string field in fields)
        {
            text = ((with != null) ? (text + field + ": " + Convert.ToString((((ushort)with & (1L << _bits - currentBit)) != 0L) ? 1 : 0) + " | ") : (text + field + ": " + Convert.ToString(((bool)_accessors[idx].getAccessor(_thing)) ? 1 : 0) + " | "));
            idx++;
            currentBit++;
        }
        return text;
    }

    public bool Contains(string pKey)
    {
        string[] fields = _fields;
        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i] == pKey)
            {
                return true;
            }
        }
        return false;
    }

    public bool Value(string pKey, ushort pValue)
    {
        byte currentBit = 1;
        string[] fields = _fields;
        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i] == pKey)
            {
                return (pValue & (1L << _bits - currentBit)) != 0;
            }
            currentBit++;
        }
        return false;
    }

    public StateFlagBinding(params string[] fields)
        : base("multiple")
    {
        _fields = fields;
        _priority = GhostPriority.Normal;
    }

    public StateFlagBinding(GhostPriority p, params string[] fields)
        : base("multiple")
    {
        _fields = fields;
        _priority = p;
    }

    public override void Connect(Thing t)
    {
        _bits = 0;
        _thing = t;
        Type tp = t.GetType();
        _accessors.Clear();
        string[] fields = _fields;
        foreach (string field in fields)
        {
            AccessorInfo accessor = Editor.GetAccessorInfo(tp, field);
            _accessors.Add(accessor);
            _bits++;
        }
    }
}
