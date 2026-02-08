using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class VirtualInput : InputDevice
{
    public static List<VirtualInput> debuggerInputs = new List<VirtualInput>
    {
        new VirtualInput(0),
        new VirtualInput(0),
        new VirtualInput(0),
        new VirtualInput(0),
        new VirtualInput(0),
        new VirtualInput(0),
        new VirtualInput(0),
        new VirtualInput(0)
    };

    public int pdraw;

    private ushort _state;

    private ushort _prevState;

    private List<string> _availableTriggers = new List<string>();

    public Vector2 leftStick;

    public Vector2 rightStick;

    public float leftTrigger;

    public float rightTrigger;

    public bool setThisFrame;

    public ushort state
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
        }
    }

    public ushort prevState
    {
        get
        {
            return _prevState;
        }
        set
        {
            _prevState = value;
        }
    }

    public List<string> availableTriggers
    {
        get
        {
            return _availableTriggers;
        }
        set
        {
            _availableTriggers = value;
        }
    }

    public VirtualInput(int idx)
        : base(idx)
    {
        _name = "virtual" + idx;
    }

    public override void Update()
    {
    }

    private bool GetState(int mapping, bool prev = false)
    {
        return ((prev ? _prevState : _state) & (1 << _availableTriggers.Count - mapping)) != 0;
    }

    public void SetState(ushort val, bool flagPrev = true)
    {
        if (flagPrev)
        {
            _prevState = _state;
        }
        _state = val;
        setThisFrame = true;
        leftStick = Vector2.Zero;
        rightStick = Vector2.Zero;
    }

    public override bool MapPressed(int mapping, bool any = false)
    {
        if (any)
        {
            for (int i = 0; i < _availableTriggers.Count; i++)
            {
                if (MapPressed(i))
                {
                    return true;
                }
            }
            return false;
        }
        if (GetState(mapping) && !GetState(mapping, prev: true))
        {
            return true;
        }
        return false;
    }

    public override bool MapReleased(int mapping)
    {
        if (!GetState(mapping) && GetState(mapping, prev: true))
        {
            return true;
        }
        return false;
    }

    public override bool MapDown(int mapping, bool any = false)
    {
        if (any)
        {
            return _state != 0;
        }
        if (GetState(mapping))
        {
            return true;
        }
        return false;
    }
}
