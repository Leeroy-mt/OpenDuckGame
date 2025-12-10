namespace DuckGame;

public class MineFlagBinding : StateFlagBase
{
    public override ushort ushortValue
    {
        get
        {
            _value = 0;
            Mine obj = _thing as Mine;
            if (obj._pin)
            {
                _value |= 8;
            }
            if (obj._armed)
            {
                _value |= 4;
            }
            if (obj._clicked)
            {
                _value |= 2;
            }
            if (obj._thrown)
            {
                _value |= 1;
            }
            return _value;
        }
        set
        {
            _value = value;
            Mine obj = _thing as Mine;
            obj._pin = (_value & 8) != 0;
            obj._armed = (_value & 4) != 0;
            obj._clicked = (_value & 2) != 0;
            obj._thrown = (_value & 1) != 0;
        }
    }

    public MineFlagBinding(GhostPriority p = GhostPriority.Normal)
        : base(p, 4)
    {
    }
}
