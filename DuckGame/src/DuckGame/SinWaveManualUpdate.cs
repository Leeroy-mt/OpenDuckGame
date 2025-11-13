using System;

namespace DuckGame;

public class SinWaveManualUpdate
{
	private float _increment;

	private float _wave;

	private float _value;

	public float value
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

	public float normalized => (_value + 1f) / 2f;

	public SinWaveManualUpdate(float inc, float start = 0f)
	{
		_increment = inc;
		_wave = start;
	}

	public SinWaveManualUpdate()
	{
		_increment = 0.1f;
		_wave = 0f;
	}

	public void Update()
	{
		_wave += _increment;
		_value = (float)Math.Sin(_wave);
	}

	public static implicit operator float(SinWaveManualUpdate val)
	{
		return val.value;
	}

	public static implicit operator SinWaveManualUpdate(float val)
	{
		return new SinWaveManualUpdate(val);
	}
}
