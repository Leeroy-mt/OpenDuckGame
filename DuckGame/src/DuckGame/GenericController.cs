using System;
using System.Collections.Generic;

namespace DuckGame;

public class GenericController : InputDevice
{
	private volatile AnalogGamePad _device;

	public override bool allowStartRemap => true;

	public override int numSticks => 2;

	public override int numTriggers => 2;

	public AnalogGamePad device
	{
		get
		{
			return _device;
		}
		set
		{
			if (_device != null)
			{
				_device.genericController = null;
			}
			_device = value;
			if (_device != null)
			{
				_device.genericController = this;
			}
		}
	}

	public override string productName
	{
		get
		{
			if (_device == null)
			{
				return _productName;
			}
			return _device.productName;
		}
		set
		{
			_productName = value;
		}
	}

	public override string productGUID
	{
		get
		{
			if (_device == null)
			{
				return _productGUID;
			}
			return _device.productGUID;
		}
		set
		{
			_productName = value;
		}
	}

	public override bool isConnected
	{
		get
		{
			if (_device == null)
			{
				return false;
			}
			return _device.isConnected;
		}
	}

	public float leftTrigger
	{
		get
		{
			if (_device == null)
			{
				return 0f;
			}
			return _device.leftTrigger;
		}
	}

	public float rightTrigger
	{
		get
		{
			if (_device == null)
			{
				return 0f;
			}
			return _device.rightTrigger;
		}
	}

	public Vec2 leftStick
	{
		get
		{
			if (_device == null)
			{
				return Vec2.Zero;
			}
			return _device.leftStick;
		}
	}

	public Vec2 rightStick
	{
		get
		{
			if (_device == null)
			{
				return Vec2.Zero;
			}
			return _device.rightStick;
		}
	}

	public override Dictionary<int, string> GetTriggerNames()
	{
		if (_device != null)
		{
			return _device.GetTriggerNames();
		}
		return null;
	}

	public override Sprite GetMapImage(int map)
	{
		if (_device != null)
		{
			return _device.GetMapImage(map);
		}
		return null;
	}

	public GenericController(int index)
		: base(index)
	{
	}

	public override bool MapPressed(int mapping, bool any = false)
	{
		if (_device != null)
		{
			return _device.MapPressed(mapping, any);
		}
		return false;
	}

	public override bool MapReleased(int mapping)
	{
		if (_device != null)
		{
			return _device.MapReleased(mapping);
		}
		return false;
	}

	public override bool MapDown(int mapping, bool any = false)
	{
		if (_device != null)
		{
			return _device.MapDown(mapping, any);
		}
		return false;
	}

	public override void Rumble(float leftIntensity = 0f, float rightIntensity = 0f)
	{
		if (device is XInputPad)
		{
			(device as XInputPad).Rumble(Math.Min(leftIntensity * 1.5f, 1f), Math.Min(rightIntensity * 1.5f, 1f));
		}
	}
}
