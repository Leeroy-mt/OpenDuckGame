namespace DuckGame;

public class RagdollFlagBinding : StateFlagBase
{
    public override ushort ushortValue
    {
        get
        {
            _value = 0;
            Ragdoll obj = _thing as Ragdoll;
            if (obj.inSleepingBag)
            {
                _value |= 16;
            }
            if (obj.solid)
            {
                _value |= 8;
            }
            if (obj.enablePhysics)
            {
                _value |= 4;
            }
            if (obj.active)
            {
                _value |= 2;
            }
            if (obj.visible)
            {
                _value |= 1;
            }
            return _value;
        }
        set
        {
            _value = value;
            Ragdoll obj = _thing as Ragdoll;
            obj.inSleepingBag = (_value & 0x10) != 0;
            obj.solid = (_value & 8) != 0;
            obj.enablePhysics = (_value & 4) != 0;
            obj.active = (_value & 2) != 0;
            obj.visible = (_value & 1) != 0;
        }
    }

    public RagdollFlagBinding(GhostPriority p = GhostPriority.Normal)
        : base(p, 5)
    {
    }
}
