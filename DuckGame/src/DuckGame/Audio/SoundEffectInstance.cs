using Microsoft.Xna.Framework.Audio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace DuckGame;

public class SoundEffectInstance : ISampleProvider
{
    public class PitchShiftProvider : ISampleProvider
    {
        private ISampleProvider _chain;

        public float pitch;

        public SoundEffectInstance instance;

        private WdlResamplingSampleProvider _resampler;

        public WaveFormat WaveFormat => _chain.WaveFormat;

        public PitchShiftProvider(ISampleProvider pChain)
        {
            _chain = pChain;
            _resampler = new WdlResamplingSampleProvider(pChain);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            _ = pitch;
            if (pitch < 0f)
            {
                _ = pitch;
            }
            _resampler.sampleRate = (int)(44100.0 * Math.Pow(2.0, 0f - pitch));
            return _resampler.Read(buffer, offset, count);
        }
    }

    protected bool _isMusic;

    public ISampleProvider _chainEnd;

    protected VolumeSampleProvider _volumeChain;

    protected PitchShiftProvider _pitchChain;

    protected PanningSampleProvider _panChain;

    protected SoundEffect _data;

    protected bool _inMixer;

    protected bool _loop;

    protected float _pitch;

    protected float _volume = 1f;

    protected float _pan;

    public int _position;

    public Action SoundEndEvent { get; set; }

    public WaveFormat WaveFormat => _data.format;

    public bool IsDisposed { get; }

    public virtual bool IsLooped
    {
        get
        {
            return _loop;
        }
        set
        {
            _loop = value;
        }
    }

    public float Pitch
    {
        get
        {
            return _pitch;
        }
        set
        {
            _pitch = value;
            if (_pitch != 0f && _pitchChain == null)
            {
                RebuildChain();
            }
            if (_pitchChain != null)
            {
                _pitchChain.pitch = _pitch;
            }
        }
    }

    public float Volume
    {
        get
        {
            return _volume;
        }
        set
        {
            _volume = value;
            if (_volume != 1f && _volumeChain == null)
            {
                RebuildChain();
            }
            if (_volumeChain != null && _data != null)
            {
                _volumeChain.Volume = _volume * _data.replaygainModifier;
            }
        }
    }

    public float Pan
    {
        get
        {
            return _pan;
        }
        set
        {
            _pan = value;
            if (_pan != 0f && _panChain == null)
            {
                RebuildChain();
            }
            if (_panChain != null)
            {
                _panChain.Pan = _pan;
            }
        }
    }

    public SoundState State
    {
        get
        {
            if (!_inMixer)
            {
                return SoundState.Stopped;
            }
            return SoundState.Playing;
        }
    }

    public void SetData(SoundEffect pData)
    {
        _position = 0;
        lock (this)
        {
            _data = pData;
            RebuildChain();
        }
    }

    public SoundEffectInstance(SoundEffect pData)
    {
        SetData(pData);
    }

    private void RebuildChain()
    {
        if (_chainEnd != null)
        {
            Windows_Audio.RemoveSound(_chainEnd);
        }
        _chainEnd = this;
        if (_data != null)
        {
            if (_data.format.Channels == 1 || _pan != 0f)
            {
                _panChain = new PanningSampleProvider(_chainEnd);
                _chainEnd = _panChain;
                _panChain.Pan = _pan;
            }
            if (_volume != 1f)
            {
                _volumeChain = new VolumeSampleProvider(_chainEnd);
                _chainEnd = _volumeChain;
                _volumeChain.Volume = _volume * _data.replaygainModifier;
            }
            if (_pitch != 0f)
            {
                _pitchChain = new PitchShiftProvider(_chainEnd);
                _chainEnd = _pitchChain;
                _pitchChain.pitch = _pitch;
            }
            if (_inMixer)
            {
                Windows_Audio.AddSound(_chainEnd, _isMusic);
            }
        }
    }

    public void Apply3D(AudioListener listener, AudioEmitter emitter)
    {
    }

    public void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
    {
    }

    public void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
    }

    public void Play()
    {
        if (_data != null)
        {
            if (_inMixer)
            {
                Stop();
            }
            _inMixer = true;
            Windows_Audio.AddSound(_chainEnd, _isMusic);
        }
    }

    public void Resume()
    {
        if (_data != null && !_inMixer)
        {
            _inMixer = true;
            Windows_Audio.AddSound(_chainEnd, _isMusic);
        }
    }

    public void Stop()
    {
        if (_data != null)
        {
            Pause();
            _position = 0;
        }
    }

    public void Stop(bool immediate)
    {
        Stop();
    }

    public void Pause()
    {
        if (_data != null)
        {
            _inMixer = false;
        }
    }

    public virtual int Read(float[] buffer, int offset, int count)
    {
        if (_data == null || !_inMixer)
        {
            return 0;
        }
        int samplesToCopy = 0;
        lock (this)
        {
            int availableSamples = _data.dataSize - _position;
            if (_data.data == null)
            {
                samplesToCopy = _data.Decode(buffer, offset, count);
            }
            else
            {
                samplesToCopy = Math.Min(availableSamples, count);
                Array.Copy(_data.data, _position, buffer, offset, samplesToCopy);
            }
            _position += samplesToCopy;
            if (samplesToCopy != count)
            {
                if (SoundEndEvent != null)
                {
                    SoundEndEvent();
                }
                if (_loop)
                {
                    _position = 0;
                    offset += samplesToCopy;
                    if (_data.data == null)
                    {
                        _data.Rewind();
                        samplesToCopy = _data.Decode(buffer, offset, count);
                    }
                    else
                    {
                        availableSamples = _data.dataSize - _position;
                        samplesToCopy = Math.Min(availableSamples, count - samplesToCopy);
                        Array.Copy(_data.data, _position, buffer, offset, samplesToCopy);
                    }
                    _position += samplesToCopy;
                    samplesToCopy = count;
                }
            }
        }
        _inMixer = _inMixer && samplesToCopy == count;
        return samplesToCopy;
    }

    protected void HandleLoop()
    {
    }

    public void Platform_SetProgress(float pProgress)
    {
        if (_data != null)
        {
            pProgress = Maths.Clamp(pProgress, 0f, 1f);
            _position = (int)(pProgress * (float)_data.data.Length);
        }
    }

    public float Platform_GetProgress()
    {
        if (_data == null)
        {
            return 1f;
        }
        return (float)_position / (float)_data.data.Length;
    }

    public int Platform_GetLengthInMilliseconds()
    {
        if (_data == null)
        {
            return 0;
        }
        return (int)((float)(_data.data.Length * 4) / (float)WaveFormat.AverageBytesPerSecond) * 500;
    }
}
