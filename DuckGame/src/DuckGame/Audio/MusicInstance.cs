using System;

namespace DuckGame;

public class MusicInstance : SoundEffectInstance
{
    public MusicInstance(SoundEffect pData)
        : base(pData)
    {
        _isMusic = true;
    }

    public override int Read(float[] buffer, int offset, int count)
    {
        if (_data == null || !_inMixer)
        {
            return 0;
        }
        if (_volume <= 0f)
        {
            return count;
        }
        int samplesToCopy = 0;
        lock (this)
        {
            if (_data.data == null)
            {
                samplesToCopy = _data.Decode(buffer, offset, count);
            }
            else
            {
                while (_position + count > _data.decodedSamples && _data.Decoder_DecodeChunk())
                {
                }
                samplesToCopy = Math.Min(count, _data.decodedSamples - _position);
                Array.Copy(SoundEffect._songBuffer, _position, buffer, offset, samplesToCopy);
            }
            _position += samplesToCopy;
            if (samplesToCopy != count)
            {
                if (base.SoundEndEvent != null)
                {
                    base.SoundEndEvent();
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
                        samplesToCopy = Math.Min(SoundEffect._songBuffer.Length - _position, count - samplesToCopy);
                        Array.Copy(SoundEffect._songBuffer, _position, buffer, offset, samplesToCopy);
                    }
                    _position += samplesToCopy;
                    samplesToCopy = count;
                }
            }
        }
        _inMixer = _inMixer && samplesToCopy == count;
        return samplesToCopy;
    }
}
