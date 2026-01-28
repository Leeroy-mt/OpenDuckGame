using System;

namespace DuckGame;

/// <summary>
/// A state binding allows a Thing to communicate the state of a field over the network during multiplayer.
/// These are generally private members of your Thing.
/// </summary>
public class StateBinding
{
    public int bitIndex;

    protected GhostPriority _priority;

    public bool valid = true;

    protected bool _lerp;

    private bool _initialized = true;

    private string _fieldName;

    protected object _thing;

    public object _previousValue;

    protected int _bits = -1;

    private bool _trueOnly;

    private bool _isRotation;

    private bool _isVelocity;

    public bool skipLerp;

    protected AccessorInfo _accessor = new AccessorInfo();

    public GhostPriority priority => _priority;

    public bool lerp => _lerp;

    public bool initialized
    {
        get
        {
            return _initialized;
        }
        set
        {
            _initialized = value;
        }
    }

    public string name => _fieldName;

    public object owner => _thing;

    public virtual object classValue
    {
        get
        {
            if (_accessor.getAccessor == null)
            {
                return null;
            }
            return _accessor.getAccessor(_thing);
        }
        set
        {
            if (_accessor.setAccessor != null)
            {
                _accessor.setAccessor(_thing, value);
            }
        }
    }

    public virtual byte byteValue
    {
        get
        {
            if (_accessor.type == typeof(byte))
            {
                return _accessor.Get<byte>(_thing);
            }
            return (byte)classValue;
        }
        set
        {
            if (_accessor.type == typeof(byte))
            {
                _accessor.Set(_thing, value);
            }
            else
            {
                classValue = value;
            }
        }
    }

    public virtual ushort ushortValue
    {
        get
        {
            if (_accessor.type == typeof(ushort))
            {
                return _accessor.Get<ushort>(_thing);
            }
            return (ushort)classValue;
        }
        set
        {
            if (_accessor.type == typeof(ushort))
            {
                _accessor.Set(_thing, value);
            }
            else
            {
                classValue = value;
            }
        }
    }

    public virtual int intValue
    {
        get
        {
            if (_accessor.type == typeof(int))
            {
                return _accessor.Get<int>(_thing);
            }
            return (int)classValue;
        }
        set
        {
            if (_accessor.type == typeof(int))
            {
                _accessor.Set(_thing, value);
            }
            else
            {
                classValue = value;
            }
        }
    }

    public virtual Type type => _accessor.type;

    public virtual int bits => _bits;

    public bool trueOnly => _trueOnly;

    public bool isRotation => _isRotation;

    public bool isVelocity => _isVelocity;

    public bool connected => _accessor != null;

    public override string ToString()
    {
        return GetDebugString(null);
    }

    public virtual string GetDebugString(object with)
    {
        if (with == null)
        {
            if (classValue == null)
            {
                return name + " = null";
            }
            return name + " = " + Convert.ToString(classValue);
        }
        return name + " = " + Convert.ToString(with);
    }

    public virtual string GetDebugStringSpecial(object with)
    {
        if (with == null)
        {
            return name + " = null";
        }
        return name + " = " + Convert.ToString(with);
    }

    public virtual T getTyped<T>()
    {
        if (_accessor.type == typeof(T))
        {
            return _accessor.Get<T>(_thing);
        }
        return (T)classValue;
    }

    public virtual void setTyped<T>(T value)
    {
        if (_accessor.type == typeof(T))
        {
            _accessor.Set(_thing, value);
        }
        else
        {
            classValue = value;
        }
    }

    public static bool CompareBase(object o1, object o2)
    {
        if (o1 is float)
        {
            return Math.Abs((float)o1 - (float)o2) < 0.001f;
        }
        if (o1 is Vec2)
        {
            Vec2 v = (Vec2)o1 - (Vec2)o2;
            if (Math.Abs(v.X) < 0.005f)
            {
                return Math.Abs(v.Y) < 0.005f;
            }
            return false;
        }
        if (o1 is BitBuffer)
        {
            return false;
        }
        return object.Equals(o1, o2);
    }

    public bool Compare<T>(T f, out T newVal)
    {
        newVal = (T)classValue;
        return CompareBase(f, newVal);
    }

    public StateBinding(string field, int bits = -1, bool rot = false, bool vel = false)
    {
        _fieldName = field;
        _previousValue = null;
        _bits = bits;
        _isRotation = rot;
        _isVelocity = vel;
    }

    public StateBinding(bool doLerp, string field, int bits = -1, bool rot = false, bool vel = false)
    {
        _fieldName = field;
        _previousValue = null;
        _bits = bits;
        _isRotation = rot;
        _isVelocity = vel;
        _lerp = doLerp;
        if (_lerp)
        {
            _priority = GhostPriority.Normal;
        }
    }

    public StateBinding(GhostPriority p, string field, int bits = -1, bool rot = false, bool vel = false, bool doLerp = false)
    {
        _fieldName = field;
        _previousValue = null;
        _bits = bits;
        _isRotation = rot;
        _isVelocity = vel;
        _priority = p;
        _lerp = doLerp;
    }

    public StateBinding(string field, int bits, bool rot)
    {
        _fieldName = field;
        _previousValue = null;
        _bits = bits;
        _isRotation = rot;
        _isVelocity = false;
    }

    public virtual object GetNetValue()
    {
        return classValue;
    }

    public virtual object ReadNetValue(object val)
    {
        return val;
    }

    public virtual object ReadNetValue(BitBuffer pData)
    {
        return pData.ReadBits(type, bits);
    }

    public virtual void Connect(Thing t)
    {
        _thing = t;
        _accessor = Editor.GetAccessorInfo(t.GetType(), _fieldName);
        if (_accessor == null)
        {
            throw new Exception("Could not find accessor for binding.");
        }
    }
}
