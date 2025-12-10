using System.Collections.Generic;

namespace DuckGame;

public class DuckAI : InputProfile
{
    private Stack<AIState> _state = new Stack<AIState>();

    private Dictionary<string, InputState> _inputState = new Dictionary<string, InputState>();

    private AILocomotion _locomotion = new AILocomotion();

    public bool canRefresh;

    public InputProfile _manualQuack;

    private int quackWait = 10;

    private int jumpWait = 10;

    public bool virtualQuack;

    public AILocomotion locomotion => _locomotion;

    public override float leftTrigger
    {
        get
        {
            if (virtualQuack)
            {
                return base.virtualDevice.leftTrigger;
            }
            if (_manualQuack != null)
            {
                return _manualQuack.leftTrigger;
            }
            return 0f;
        }
    }

    public void Press(string trigger)
    {
        _inputState[trigger] = InputState.Pressed;
    }

    public void HoldDown(string trigger)
    {
        _inputState[trigger] = InputState.Down;
    }

    public void Release(string trigger)
    {
        _inputState[trigger] = InputState.Released;
    }

    public override bool Pressed(string trigger, bool any = false)
    {
        if (_inputState.TryGetValue(trigger, out var outVal))
        {
            return outVal == InputState.Pressed;
        }
        return false;
    }

    public override bool Released(string trigger)
    {
        if (_inputState.TryGetValue(trigger, out var outVal))
        {
            return outVal == InputState.Released;
        }
        return false;
    }

    public override bool Down(string trigger)
    {
        if (_inputState.TryGetValue(trigger, out var outVal))
        {
            if (outVal != InputState.Pressed)
            {
                return outVal == InputState.Down;
            }
            return true;
        }
        return false;
    }

    public bool SetTarget(Vec2 t)
    {
        _locomotion.target = t;
        return _locomotion.target == Vec2.Zero;
    }

    public void TrimLastTarget()
    {
        _locomotion.TrimLastTarget();
    }

    public DuckAI(InputProfile manualQuacker = null)
    {
        _state.Push(new AIStateDeathmatchBot());
        _manualQuack = manualQuacker;
    }

    public virtual void Update(Duck duck)
    {
        Release("GRAB");
        Release("SHOOT");
        _locomotion.Update(this, duck);
        if (jumpWait > 0)
        {
            jumpWait--;
        }
        else
        {
            jumpWait = 10;
            _locomotion.Jump(5);
        }
        if (quackWait > 0)
        {
            quackWait--;
            return;
        }
        quackWait = 4;
        _locomotion.Quack(2);
    }

    public override void UpdateExtraInput()
    {
        if (_inputState.ContainsKey("QUACK") && _inputState["QUACK"] == InputState.Pressed)
        {
            _inputState["QUACK"] = InputState.Down;
        }
        if (_inputState.ContainsKey("STRAFE") && _inputState["STRAFE"] == InputState.Pressed)
        {
            _inputState["STRAFE"] = InputState.Down;
        }
        if (_manualQuack != null)
        {
            if (_manualQuack.Pressed("QUACK"))
            {
                Press("QUACK");
            }
            else if (_manualQuack.Released("QUACK"))
            {
                Release("QUACK");
            }
            if (_manualQuack.Pressed("STRAFE"))
            {
                Press("STRAFE");
            }
            else if (_manualQuack.Released("STRAFE"))
            {
                Release("STRAFE");
            }
        }
    }

    public void Draw()
    {
        if (_locomotion.pathFinder.path == null)
        {
            return;
        }
        Vec2 lastNode = Vec2.Zero;
        foreach (PathNodeLink n in _locomotion.pathFinder.path)
        {
            if (lastNode != Vec2.Zero)
            {
                Graphics.DrawLine(lastNode, n.owner.position, new Color(255, 0, 255), 2f, 0.9f);
            }
            lastNode = n.owner.position;
        }
    }
}
