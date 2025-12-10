namespace DuckGame;

public class PhysicsFlagBinding : StateFlagBase
{
    public override ushort ushortValue
    {
        get
        {
            _value = 0;
            PhysicsObject obj = _thing as PhysicsObject;
            if (obj.solid)
            {
                _value |= 128;
            }
            if (obj.enablePhysics)
            {
                _value |= 16;
            }
            if (obj.active)
            {
                _value |= 8;
            }
            if (obj.visible)
            {
                _value |= 4;
            }
            if (obj.grounded)
            {
                _value |= 64;
            }
            if (obj.onFire)
            {
                _value |= 32;
            }
            if (obj._destroyed)
            {
                _value |= 2;
            }
            if (obj.isSpawned)
            {
                _value |= 1;
            }
            return _value;
        }
        set
        {
            _value = value;
            PhysicsObject obj = _thing as PhysicsObject;
            obj.solid = (_value & 0x80) != 0;
            obj.enablePhysics = (_value & 0x10) != 0;
            obj.active = (_value & 8) != 0;
            obj.visible = (_value & 4) != 0;
            obj.grounded = (_value & 0x40) != 0;
            obj.onFire = (_value & 0x20) != 0;
            obj._destroyed = (_value & 2) != 0;
            obj.isSpawned = (_value & 1) != 0;
        }
    }

    public PhysicsFlagBinding(GhostPriority p)
        : base(p, 8)
    {
    }
}
