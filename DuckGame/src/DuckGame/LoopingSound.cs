using Microsoft.Xna.Framework.Audio;

namespace DuckGame;

public class LoopingSound
{
	private Sound _effect;

	private float _lerpVolume;

	private float _lerpSpeed = 0.1f;

	private bool _killSound;

	private Level _startLevel;

	public float volume
	{
		get
		{
			if (_effect == null)
			{
				return 1f;
			}
			return _effect.Volume;
		}
		set
		{
			if (_effect != null)
			{
				_effect.Volume = value;
			}
			_lerpVolume = value;
		}
	}

	public float lerpVolume
	{
		get
		{
			return _lerpVolume;
		}
		set
		{
			_lerpVolume = value;
		}
	}

	public float lerpSpeed
	{
		get
		{
			return _lerpSpeed;
		}
		set
		{
			_lerpSpeed = value;
		}
	}

	public float pitch
	{
		get
		{
			if (_effect == null)
			{
				return 0f;
			}
			return _effect.Pitch;
		}
		set
		{
			if (_effect != null)
			{
				_effect.Pitch = value;
			}
		}
	}

	public LoopingSound(string sound, float startVolume = 0f, float startPitch = 0f, string multiSound = null)
	{
		if (startVolume > 0f)
		{
			_effect = SFX.Play(sound, startVolume * SFX.volume, startPitch, 0f, looped: true);
		}
		else if (multiSound != null)
		{
			_effect = SFX.GetMultiSound(sound, multiSound);
		}
		else
		{
			_effect = SFX.Get(sound, startVolume * SFX.volume, startPitch, 0f, looped: true);
		}
		if (_effect == null)
		{
			_effect = new InvalidSound(sound, startVolume, startPitch, 0f, looped: true);
		}
	}

	~LoopingSound()
	{
		_lerpSpeed = 0f;
		_lerpVolume = 0f;
		if (_effect != null)
		{
			_effect.Kill();
			_effect = null;
		}
	}

	public void Kill()
	{
		if (_effect != null)
		{
			_effect.Kill();
		}
	}

	public void Mute()
	{
		if (_effect != null && !_effect.IsDisposed)
		{
			_effect.Volume = 0f;
		}
	}

	public void Update()
	{
		if (_effect != null && _effect.IsDisposed)
		{
			_effect = null;
			return;
		}
		if (_effect == null || (_startLevel != null && Level.current != _startLevel))
		{
			if (_effect != null)
			{
				_effect.Kill();
			}
			return;
		}
		if (_killSound)
		{
			_effect.Stop();
			return;
		}
		if (_effect.Volume > 0.01f && _effect.State != SoundState.Playing)
		{
			_effect.Play();
			_startLevel = Level.current;
		}
		else if (_effect.Volume < 0.01f && _effect.State == SoundState.Playing)
		{
			_effect.Stop();
		}
		_effect.Volume = Maths.LerpTowards(_effect.Volume, _lerpVolume, _lerpSpeed);
	}
}
