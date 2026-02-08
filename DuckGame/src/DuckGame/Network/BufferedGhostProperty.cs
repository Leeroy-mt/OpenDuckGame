using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class BufferedGhostProperty<T> : BufferedGhostProperty
{
    private T _value;

    public override object value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = (T)value;
        }
    }

    public override bool Refresh()
    {
        if (!binding.Compare(_value, out var newVal))
        {
            _value = newVal;
            return true;
        }
        return false;
    }

    public override void UpdateFrom(StateBinding bind)
    {
        _value = bind.getTyped<T>();
    }

    public override void Apply(float lerp)
    {
        if (lerp < 1f)
        {
            if (binding is CompressedVec2Binding)
            {
                Vector2 me = binding.getTyped<Vector2>();
                Vector2 targ = (Vector2)value;
                if ((me - targ).LengthSquared() > 1024f)
                {
                    binding.setTyped(targ);
                }
                else
                {
                    binding.setTyped(Lerp.Vec2Smooth(me, targ, lerp));
                }
            }
            else if (binding.isRotation)
            {
                Vector2 vec = Maths.AngleToVec(binding.getTyped<float>());
                Vector2 angle2 = Maths.AngleToVec((float)value);
                Vector2 angle3 = BufferedGhostProperty.Slerp(vec, angle2, lerp);
                binding.setTyped(Maths.DegToRad(Maths.PointDirection(Vector2.Zero, angle3)));
            }
            else
            {
                binding.setTyped(_value);
            }
        }
        else
        {
            if (binding.name == "netPosition")
            {
                _ = ((Vector2)value).X;
                _ = 47f;
            }
            binding.setTyped(_value);
        }
    }
}
public abstract class BufferedGhostProperty
{
    public StateBinding binding;

    public int index;

    public NetIndex16 tick;

    public bool isNetworkStateValue;

    public bool initialized;

    public abstract object value { get; set; }

    public override string ToString()
    {
        if (isNetworkStateValue)
        {
            return binding.GetDebugStringSpecial(value) + "(ns)";
        }
        return binding.GetDebugStringSpecial(value);
    }

    public abstract bool Refresh();

    public abstract void Apply(float lerp);

    public abstract void UpdateFrom(StateBinding bind);

    public void UpdateFrom(BufferedGhostProperty prop)
    {
        tick = prop.tick;
        UpdateFrom(prop.binding);
    }

    protected static Vector2 Slerp(Vector2 from, Vector2 to, float step)
    {
        if (step == 0f)
        {
            return from;
        }
        if (from == to || step == 1f)
        {
            return to;
        }
        double theta = Math.Acos(Vector2.Dot(from, to));
        if (theta == 0.0)
        {
            return to;
        }
        double sinTheta = Math.Sin(theta);
        return (float)(Math.Sin((double)(1f - step) * theta) / sinTheta) * from + (float)(Math.Sin((double)step * theta) / sinTheta) * to;
    }
}
