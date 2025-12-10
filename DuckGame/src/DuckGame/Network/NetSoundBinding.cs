using System;

namespace DuckGame;

public class NetSoundBinding : StateBinding
{
    public override Type type => typeof(byte);

    public override object classValue
    {
        get
        {
            return byteValue;
        }
        set
        {
            byteValue = (byte)value;
        }
    }

    public override byte byteValue
    {
        get
        {
            return (byte)(_accessor.getAccessor(_thing) as NetSoundEffect).index;
        }
        set
        {
            (_accessor.getAccessor(_thing) as NetSoundEffect).index = value;
        }
    }

    public NetSoundBinding(string field)
        : base(field, 2)
    {
    }

    public NetSoundBinding(GhostPriority p, string field)
        : base(field, 2)
    {
        _priority = p;
    }
}
