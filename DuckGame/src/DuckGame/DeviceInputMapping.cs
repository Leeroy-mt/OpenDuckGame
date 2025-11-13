using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class DeviceInputMapping : DataClass
{
	public string deviceName;

	public string deviceGUID;

	public Dictionary<string, int> map = new Dictionary<string, int>();

	public Dictionary<int, string> graphicMap = new Dictionary<int, string>();

	private Dictionary<string, Sprite> _spriteMap = new Dictionary<string, Sprite>();

	public int inputOverrideType;

	public InputDevice deviceOverride;

	public InputDevice device
	{
		get
		{
			if (deviceOverride != null)
			{
				return deviceOverride;
			}
			if (deviceName == "XBOX GAMEPAD")
			{
				return Input.GetDevice<XInputPad>();
			}
			foreach (InputDevice d in Input.GetInputDevices())
			{
				if (d.productName == deviceName && d.productGUID == deviceGUID)
				{
					return d;
				}
			}
			return new InputDevice();
		}
	}

	public List<InputDevice> devices
	{
		get
		{
			if (deviceName == "XBOX GAMEPAD")
			{
				return (from x in Input.GetInputDevices()
					where x is XInputPad
					select x).ToList();
			}
			return (from x in Input.GetInputDevices()
				where x.productName == deviceName && x.productGUID == deviceGUID
				select x).ToList();
		}
	}

	public void MapInput(string pTrigger, int pIndex)
	{
		map[pTrigger] = pIndex;
	}

	public Sprite GetSprite(int mapping)
	{
		string val = null;
		if (graphicMap.TryGetValue(mapping, out val))
		{
			Sprite spr = null;
			if (_spriteMap.TryGetValue(val, out spr))
			{
				return spr;
			}
			spr = new Sprite(val);
			_spriteMap[val] = spr;
			return spr;
		}
		return null;
	}

	public DeviceInputMapping()
	{
		_nodeName = "InputMapping";
	}

	public string GetMappingString(string trigger)
	{
		int mapping = 0;
		if (!map.TryGetValue(trigger, out mapping))
		{
			return "";
		}
		Dictionary<int, string> triggerNames = device.GetTriggerNames();
		string name = "???";
		triggerNames?.TryGetValue(mapping, out name);
		return name;
	}

	public bool IsEqual(DeviceInputMapping compare)
	{
		if (map.Count != compare.map.Count)
		{
			return false;
		}
		foreach (KeyValuePair<string, int> pair in map)
		{
			int val1 = -1;
			int val2 = -1;
			map.TryGetValue(pair.Key, out val1);
			compare.map.TryGetValue(pair.Key, out val2);
			if (val1 != val2)
			{
				return false;
			}
		}
		if (graphicMap.Count != compare.graphicMap.Count)
		{
			return false;
		}
		foreach (KeyValuePair<int, string> pair2 in graphicMap)
		{
			string val3 = "";
			string val4 = "";
			graphicMap.TryGetValue(pair2.Key, out val3);
			compare.graphicMap.TryGetValue(pair2.Key, out val4);
			if (val3 != val4)
			{
				return false;
			}
		}
		return true;
	}

	public DeviceInputMapping Clone()
	{
		DeviceInputMapping newMapping = new DeviceInputMapping();
		newMapping.deviceName = deviceName;
		newMapping.deviceGUID = deviceGUID;
		foreach (KeyValuePair<string, int> pair in map)
		{
			newMapping.MapInput(pair.Key, pair.Value);
		}
		foreach (KeyValuePair<int, string> pair2 in graphicMap)
		{
			newMapping.graphicMap[pair2.Key] = pair2.Value;
		}
		return newMapping;
	}

	private void CleanDupes(string trigger, int padButton, bool allowDupes)
	{
		if (Triggers.IsUITrigger(trigger) || Triggers.IsBasicMovement(trigger))
		{
			allowDupes = false;
		}
		if (allowDupes)
		{
			return;
		}
		int current = map[trigger];
		string dupe = null;
		foreach (KeyValuePair<string, int> pair in map)
		{
			if (pair.Key != trigger && pair.Value == padButton && (Triggers.IsUITrigger(trigger) == Triggers.IsUITrigger(pair.Key) || (Triggers.IsBasicMovement(trigger) && Triggers.IsBasicMovement(pair.Key))))
			{
				dupe = pair.Key;
				break;
			}
		}
		if (dupe != null)
		{
			MapInput(dupe, current);
		}
	}

	public bool RunMappingUpdate(string trigger, bool allowDupes = true)
	{
		bool finished = false;
		if (device is AnalogGamePad || (device is GenericController && (device as GenericController).device != null))
		{
			AnalogGamePad pad = device as AnalogGamePad;
			if (pad == null && device is GenericController)
			{
				pad = (device as GenericController).device;
			}
			switch (trigger)
			{
			case "LSTICK":
			case "RSTICK":
				if (pad.leftStick.length > 0.1f)
				{
					CleanDupes(trigger, 64, allowDupes);
					MapInput(trigger, 64);
					finished = true;
				}
				else if (pad.rightStick.length > 0.1f)
				{
					CleanDupes(trigger, 128, allowDupes);
					MapInput(trigger, 128);
					finished = true;
				}
				break;
			case "LTRIGGER":
			case "RTRIGGER":
				if (pad.leftTrigger > 0.1f)
				{
					CleanDupes(trigger, 8388608, allowDupes);
					MapInput(trigger, 8388608);
					finished = true;
				}
				else if (pad.rightTrigger > 0.1f)
				{
					CleanDupes(trigger, 4194304, allowDupes);
					MapInput(trigger, 4194304);
					finished = true;
				}
				break;
			default:
				foreach (PadButton b in Enum.GetValues(typeof(PadButton)).Cast<PadButton>())
				{
					if (b != PadButton.LeftThumbstickUp && b != PadButton.LeftThumbstickDown && b != PadButton.LeftThumbstickLeft && b != PadButton.LeftThumbstickRight && b != PadButton.RightThumbstickUp && b != PadButton.RightThumbstickDown && b != PadButton.RightThumbstickLeft && b != PadButton.RightThumbstickRight && device.MapPressed((int)b))
					{
						CleanDupes(trigger, (int)b, allowDupes);
						MapInput(trigger, (int)b);
						finished = true;
					}
				}
				break;
			}
		}
		else if (device is Keyboard)
		{
			foreach (Keys b2 in Enum.GetValues(typeof(Keys)).Cast<Keys>())
			{
				if (device.MapPressed((int)b2))
				{
					CleanDupes(trigger, (int)b2, allowDupes);
					MapInput(trigger, (int)b2);
					finished = true;
				}
			}
			if (!finished)
			{
				if (Mouse.left == InputState.Pressed)
				{
					MapInput(trigger, 999990);
					finished = true;
				}
				else if (Mouse.middle == InputState.Pressed)
				{
					MapInput(trigger, 999991);
					finished = true;
				}
				else if (Mouse.right == InputState.Pressed)
				{
					MapInput(trigger, 999992);
					finished = true;
				}
			}
		}
		return finished;
	}
}
