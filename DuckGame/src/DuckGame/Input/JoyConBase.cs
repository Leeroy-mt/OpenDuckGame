using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DuckGame;

public abstract class JoyConBase : AnalogGamePad
{
    private Dictionary<int, string> _triggerNames = new Dictionary<int, string>
    {
        { 4096, "A" },
        { 8192, "B" },
        { 16384, "X" },
        { 32768, "Y" },
        { 16, "START" },
        { 32, "BACK" },
        { 4, "LEFT" },
        { 8, "RIGHT" },
        { 1, "UP" },
        { 2, "DOWN" },
        { 2097152, "L{" },
        { 1073741824, "L/" },
        { 268435456, "L}" },
        { 536870912, "L~" },
        { 134217728, "R{" },
        { 67108864, "R/" },
        { 16777216, "R}" },
        { 33554432, "R~" },
        { 256, "LB" },
        { 512, "RB" },
        { 8388608, "LT" },
        { 4194304, "RT" },
        { 64, "LS" },
        { 128, "RS" },
        { 9999, "DPAD" },
        { 9998, "DPAD" }
    };

    protected readonly Dictionary<int, Sprite> _imageMap;

    public override bool hasMotionAxis => true;

    public override float motionAxis => (float)Math.Abs(Math.Sin((double)SwitchSixAxis.GetAxis(_index) * Math.PI * 2.0));

    public override bool allowStartRemap => false;

    public override Vector2 leftStick => new Vector2(_state.sticks.left.X, _state.sticks.left.Y);

    public override Vector2 rightStick => Vector2.Zero;

    public abstract override bool isConnected { get; }

    protected JoyConBase(int idx, string name, string productname, Dictionary<int, Sprite> image_map)
        : base(idx)
    {
        _name = name;
        _productName = productname;
        _imageMap = image_map;
        _productGUID = "";
    }

    protected Vector2 ReadRightStick()
    {
        return new Vector2(_state.sticks.right.X, _state.sticks.right.Y);
    }

    public override Dictionary<int, string> GetTriggerNames()
    {
        return _triggerNames;
    }

    public override Sprite GetMapImage(int map)
    {
        Sprite spr = null;
        _imageMap.TryGetValue(map, out spr);
        return spr;
    }

    protected override PadState GetState(int index)
    {
        GamePadState state = GamePad.GetState((PlayerIndex)index, GamePadDeadZone.Circular);
        PadState newState = default(PadState);
        if (state.IsButtonDown(Buttons.DPadUp))
        {
            newState.buttons |= PadButton.DPadUp;
        }
        if (state.IsButtonDown(Buttons.DPadDown))
        {
            newState.buttons |= PadButton.DPadDown;
        }
        if (state.IsButtonDown(Buttons.DPadLeft))
        {
            newState.buttons |= PadButton.DPadLeft;
        }
        if (state.IsButtonDown(Buttons.DPadRight))
        {
            newState.buttons |= PadButton.DPadRight;
        }
        if (state.IsButtonDown(Buttons.Start))
        {
            newState.buttons |= PadButton.Start;
        }
        if (state.IsButtonDown(Buttons.Back))
        {
            newState.buttons |= PadButton.Back;
        }
        if (state.IsButtonDown(Buttons.LeftStick))
        {
            newState.buttons |= PadButton.LeftStick;
        }
        if (state.IsButtonDown(Buttons.RightStick))
        {
            newState.buttons |= PadButton.RightStick;
        }
        if (state.IsButtonDown(Buttons.LeftShoulder))
        {
            newState.buttons |= PadButton.LeftShoulder;
        }
        if (state.IsButtonDown(Buttons.RightShoulder))
        {
            newState.buttons |= PadButton.RightShoulder;
        }
        if (state.IsButtonDown(Buttons.BigButton))
        {
            newState.buttons |= PadButton.BigButton;
        }
        if (state.IsButtonDown(Buttons.A))
        {
            newState.buttons |= PadButton.A;
        }
        if (state.IsButtonDown(Buttons.B))
        {
            newState.buttons |= PadButton.B;
        }
        if (state.IsButtonDown(Buttons.X))
        {
            newState.buttons |= PadButton.X;
        }
        if (state.IsButtonDown(Buttons.Y))
        {
            newState.buttons |= PadButton.Y;
        }
        if (state.IsButtonDown(Buttons.LeftThumbstickLeft))
        {
            newState.buttons |= PadButton.LeftThumbstickLeft;
        }
        if (state.IsButtonDown(Buttons.RightTrigger))
        {
            newState.buttons |= PadButton.RightTrigger;
        }
        if (state.IsButtonDown(Buttons.LeftTrigger))
        {
            newState.buttons |= PadButton.LeftTrigger;
        }
        if (state.IsButtonDown(Buttons.RightThumbstickUp))
        {
            newState.buttons |= PadButton.RightThumbstickUp;
        }
        if (state.IsButtonDown(Buttons.RightThumbstickDown))
        {
            newState.buttons |= PadButton.RightThumbstickDown;
        }
        if (state.IsButtonDown(Buttons.RightThumbstickRight))
        {
            newState.buttons |= PadButton.RightThumbstickRight;
        }
        if (state.IsButtonDown(Buttons.RightThumbstickLeft))
        {
            newState.buttons |= PadButton.RightThumbstickLeft;
        }
        if (state.IsButtonDown(Buttons.LeftThumbstickUp))
        {
            newState.buttons |= PadButton.LeftThumbstickUp;
        }
        if (state.IsButtonDown(Buttons.LeftThumbstickDown))
        {
            newState.buttons |= PadButton.LeftThumbstickDown;
        }
        if (state.IsButtonDown(Buttons.LeftThumbstickRight))
        {
            newState.buttons |= PadButton.LeftThumbstickRight;
        }
        newState.sticks.left = state.ThumbSticks.Left;
        newState.sticks.right = state.ThumbSticks.Right;
        newState.triggers.left = state.Triggers.Left;
        newState.triggers.right = state.Triggers.Right;
        return newState;
    }
}
