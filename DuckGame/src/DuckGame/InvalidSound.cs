using System;
using Microsoft.Xna.Framework.Audio;

namespace DuckGame;

public class InvalidSound : Sound
{
	private float _pitch;

	private float _pan;

	private bool _isLooped;

	public override float Pitch
	{
		get
		{
			return _pitch;
		}
		set
		{
			_pitch = value;
		}
	}

	public override float Pan
	{
		get
		{
			return _pan;
		}
		set
		{
			_pan = value;
		}
	}

	public override bool IsLooped
	{
		get
		{
			return _isLooped;
		}
		set
		{
			_isLooped = value;
		}
	}

	public override SoundState State => SoundState.Stopped;

	public override float Volume
	{
		get
		{
			return Math.Min(1f, Math.Max(0f, _volume));
		}
		set
		{
			_volume = Math.Min(1f, Math.Max(0f, value));
		}
	}

	public override bool IsDisposed => false;

	public override void Play()
	{
	}

	public override void Stop()
	{
	}

	public override void Unpooled()
	{
	}

	public override void Pause()
	{
	}

	public InvalidSound(string sound, float vol, float pitch, float pan, bool looped)
	{
		_name = sound;
		_volume = vol;
		_pitch = pitch;
		_pan = pan;
		_isLooped = looped;
	}
}
