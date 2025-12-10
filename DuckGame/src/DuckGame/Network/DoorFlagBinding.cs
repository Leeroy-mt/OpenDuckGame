namespace DuckGame;

public class DoorFlagBinding : StateFlagBase
{
    public override ushort ushortValue
    {
        get
        {
            _value = 0;
            Door obj = _thing as Door;
            if (obj._didJiggle)
            {
                _value |= 8;
            }
            if (obj._jammed)
            {
                _value |= 4;
            }
            if (obj._destroyed)
            {
                _value |= 2;
            }
            if (obj.locked)
            {
                _value |= 1;
            }
            return _value;
        }
        set
        {
            _value = value;
            Door obj = _thing as Door;
            obj._didJiggle = (_value & 8) != 0;
            obj._jammed = (_value & 4) != 0;
            obj._destroyed = (_value & 2) != 0;
            obj.locked = (_value & 1) != 0;
        }
    }

    public DoorFlagBinding(GhostPriority p = GhostPriority.Normal)
        : base(p, 4)
    {
    }
}
