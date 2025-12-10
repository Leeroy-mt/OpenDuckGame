namespace DuckGame;

public class TampingFlagBinding : StateFlagBase
{
    public override ushort ushortValue
    {
        get
        {
            _value = 0;
            TampingWeapon obj = _thing as TampingWeapon;
            if (obj._tamped)
            {
                _value |= 4;
            }
            if (obj.tamping)
            {
                _value |= 2;
            }
            if (obj._rotating)
            {
                _value |= 1;
            }
            return _value;
        }
        set
        {
            _value = value;
            TampingWeapon obj = _thing as TampingWeapon;
            obj._tamped = (_value & 4) != 0;
            obj.tamping = (_value & 2) != 0;
            obj._rotating = (_value & 1) != 0;
        }
    }

    public TampingFlagBinding(GhostPriority p = GhostPriority.Normal)
        : base(p, 3)
    {
    }
}
