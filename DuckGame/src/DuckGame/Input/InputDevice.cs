using System.Collections.Generic;

namespace DuckGame;

public class InputDevice
{
    protected string _name;

    public float rumbleIntensity;

    protected int _index;

    protected string _productName;

    protected string _productGUID;

    private volatile GenericController _genericController;

    public DeviceInputMapping overrideMap;

    private bool _rumbleThisFrame;

    private int _framesRumbled;

    public byte inputDeviceType
    {
        get
        {
            if (this is Keyboard)
            {
                return 0;
            }
            if (this is XInputPad)
            {
                return 1;
            }
            if (this is GenericController && (this as GenericController).device != null)
            {
                return (this as GenericController).device.inputDeviceType;
            }
            return byte.MaxValue;
        }
    }

    public string name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
        }
    }

    public int index => _index;

    public virtual string productName
    {
        get
        {
            return _productName;
        }
        set
        {
            _productName = value;
        }
    }

    public virtual string productGUID
    {
        get
        {
            return _productGUID;
        }
        set
        {
            _productGUID = value;
        }
    }

    public virtual bool hasMotionAxis => false;

    public virtual float motionAxis => 0f;

    public virtual bool allowStartRemap => true;

    public virtual int numSticks => 0;

    public virtual int numTriggers => 0;

    public virtual bool allowDirectionalMapping => true;

    public GenericController genericController
    {
        get
        {
            return _genericController;
        }
        set
        {
            _genericController = value;
        }
    }

    public virtual bool isConnected => true;

    public InputDevice(int idx = 0)
    {
        _index = idx;
    }

    public virtual Dictionary<int, string> GetTriggerNames()
    {
        return null;
    }

    public virtual Sprite DoGetMapImage(int map, bool skipStyleCheck = false)
    {
        if (skipStyleCheck)
        {
            return GetMapImage(map);
        }
        DeviceInputMapping mapping = overrideMap;
        if (overrideMap == null)
        {
            mapping = Input.GetDefaultMapping(productName, productGUID, presets: false, makeClone: false);
        }
        Sprite val = mapping.GetSprite(map);
        if (val != null)
        {
            return val;
        }
        if (map == 9999 || map == 9998)
        {
            map = ((!mapping.map.ContainsKey("LEFT") || mapping.map["LEFT"] != 37) ? 9998 : 9999);
        }
        return GetMapImage(map);
    }

    public float RumbleIntensityModifier()
    {
        if ((double)rumbleIntensity > 0.3)
        {
            _rumbleThisFrame = true;
        }
        if (_framesRumbled > 120)
        {
            return 0f;
        }
        return Options.Data.rumbleIntensity;
    }

    public virtual void Rumble(float leftIntensity = 0f, float rightIntensity = 0f)
    {
    }

    public virtual Sprite GetMapImage(int map)
    {
        return null;
    }

    public virtual void Update()
    {
        if (_rumbleThisFrame)
        {
            _framesRumbled++;
        }
        else
        {
            _framesRumbled = 0;
        }
        _rumbleThisFrame = false;
    }

    public virtual bool MapPressed(int mapping, bool any = false)
    {
        return false;
    }

    public virtual bool MapReleased(int mapping)
    {
        return false;
    }

    public virtual bool MapDown(int mapping, bool any = false)
    {
        return false;
    }
}
