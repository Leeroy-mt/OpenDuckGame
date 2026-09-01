#if FACEPUNCH
using Steamworks;
#else
using Steam;
#endif

namespace DuckGame;

public class StatBinding
{
    private string _name;

    private object _value;

    public object value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
        }
    }

    public int valueInt
    {
        get
        {
            if (_value is float)
            {
                return (int)(float)_value;
            }
            return (int)_value;
        }
        set
        {
            if (_value is not int || value > (int)_value)
                _value = value;

#if FACEPUNCH
            FacepunchSteam.SetStat(_name, valueInt);
#else
            DGSteam.SetStat(_name, valueInt);
#endif
        }
    }

    public float valueFloat
    {
        get
        {
            return _value is int i ? i : (float)_value;
        }
        set
        {
            if (_value is not float || value > (float)_value)
                _value = value;

#if FACEPUNCH
            SteamUserStats.SetStat(_name, valueFloat);
#else
            DGSteam.SetStat(_name, valueFloat);
#endif
        }
    }

    public bool isFloat => _value is float;

    public void BindName(string name)
    {
        _name = name;
        _value = 0;

#if FACEPUNCH
        if (SteamClient.IsValid)
        {
            float f = SteamUserStats.GetStatFloat(_name);
#else
        if (DGSteam.IsInitialized())
        {
            float f = DGSteam.GetStat(_name);
#endif
            if (f > -99999f)
                _value = f;
        }
    }

    public static implicit operator float(StatBinding val)
    {
        return val.valueFloat;
    }

    public static implicit operator int(StatBinding val)
    {
        return val.valueInt;
    }

    public static StatBinding operator +(StatBinding c1, int c2)
    {
        c1.valueInt += c2;
        return c1;
    }

    public static StatBinding operator -(StatBinding c1, int c2)
    {
        c1.valueInt -= c2;
        return c1;
    }

    public static StatBinding operator +(StatBinding c1, float c2)
    {
        c1.valueFloat += c2;
        return c1;
    }

    public static StatBinding operator -(StatBinding c1, float c2)
    {
        c1.valueFloat -= c2;
        return c1;
    }

    public static bool operator <(StatBinding c1, float c2)
    {
        return c1.valueFloat < c2;
    }

    public static bool operator >(StatBinding c1, float c2)
    {
        return c1.valueFloat > c2;
    }

    public static bool operator <(StatBinding c1, int c2)
    {
        return c1.valueInt < c2;
    }

    public static bool operator >(StatBinding c1, int c2)
    {
        return c1.valueInt > c2;
    }
}
