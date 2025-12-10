using System;

namespace DuckGame;

/// <summary>
/// A pad button collection.
/// </summary>
[Flags]
public enum PadButton
{
    DPadUp = 1,
    DPadDown = 2,
    DPadLeft = 4,
    DPadRight = 8,
    Start = 0x10,
    Back = 0x20,
    LeftStick = 0x40,
    RightStick = 0x80,
    LeftShoulder = 0x100,
    RightShoulder = 0x200,
    BigButton = 0x800,
    A = 0x1000,
    B = 0x2000,
    X = 0x4000,
    Y = 0x8000,
    LeftThumbstickLeft = 0x200000,
    RightTrigger = 0x400000,
    LeftTrigger = 0x800000,
    RightThumbstickUp = 0x1000000,
    RightThumbstickDown = 0x2000000,
    RightThumbstickRight = 0x4000000,
    RightThumbstickLeft = 0x8000000,
    LeftThumbstickUp = 0x10000000,
    LeftThumbstickDown = 0x20000000,
    LeftThumbstickRight = 0x40000000
}
