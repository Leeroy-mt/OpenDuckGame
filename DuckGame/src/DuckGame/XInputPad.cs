using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DuckGame;

public class XInputPad : AnalogGamePad
{
	public Dictionary<int, string> _triggerNames = new Dictionary<int, string>
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
		{ 9998, "WASD" }
	};

	public Dictionary<int, Sprite> _triggerImages = new Dictionary<int, Sprite>
	{
		{
			4096,
			new Sprite("buttons/xbox/oButton")
		},
		{
			8192,
			new Sprite("buttons/xbox/aButton")
		},
		{
			16384,
			new Sprite("buttons/xbox/uButton")
		},
		{
			32768,
			new Sprite("buttons/xbox/yButton")
		},
		{
			16,
			new Sprite("buttons/xbox/startButton")
		},
		{
			32,
			new Sprite("buttons/xbox/selectButton")
		},
		{
			4,
			new Sprite("buttons/xbox/dPadLeft")
		},
		{
			8,
			new Sprite("buttons/xbox/dPadRight")
		},
		{
			1,
			new Sprite("buttons/xbox/dPadUp")
		},
		{
			2,
			new Sprite("buttons/xbox/dPadDown")
		},
		{
			256,
			new Sprite("buttons/xbox/leftBumper")
		},
		{
			512,
			new Sprite("buttons/xbox/rightBumper")
		},
		{
			8388608,
			new Sprite("buttons/xbox/leftTrigger")
		},
		{
			4194304,
			new Sprite("buttons/xbox/rightTrigger")
		},
		{
			64,
			new Sprite("buttons/xbox/leftStick")
		},
		{
			128,
			new Sprite("buttons/xbox/rightStick")
		},
		{
			9999,
			new Sprite("buttons/xbox/dPad")
		},
		{
			9998,
			new Sprite("buttons/xbox/dPad")
		}
	};

	private bool _connectedState;

	public override bool isConnected => _connectedState;

	public override bool allowStartRemap => true;

	public override int numSticks => 2;

	public override int numTriggers => 2;

	public XInputPad(int idx)
		: base(idx)
	{
		_name = "xbox" + idx;
		_productName = "XBOX GAMEPAD";
		_productGUID = "";
	}

	public override Dictionary<int, string> GetTriggerNames()
	{
		return _triggerNames;
	}

	public override Sprite GetMapImage(int map)
	{
		Sprite spr = null;
		_triggerImages.TryGetValue(map, out spr);
		return spr;
	}

	public void InitializeState()
	{
		GetState(base.index);
	}

	protected override PadState GetState(int index)
	{
		GamePadState state = GamePad.GetState((PlayerIndex)index, GamePadDeadZone.Circular);
		PadState newState = default(PadState);
		foreach (object v in Enum.GetValues(typeof(PadButton)))
		{
			if (state.IsButtonDown((Buttons)v))
			{
				newState.buttons |= (PadButton)v;
			}
		}
		newState.sticks.left = state.ThumbSticks.Left;
		newState.sticks.right = state.ThumbSticks.Right;
		newState.triggers.left = state.Triggers.Left;
		newState.triggers.right = state.Triggers.Right;
		_connectedState = state.IsConnected;
		return newState;
	}
}
