using NAudio.Dsp;
using NAudio.Wave;

namespace DuckGame;

internal class MainFilterProvider : ISampleProvider
{
    private ISampleProvider _chain;

    private BiQuadFilter _filter;

    public WaveFormat WaveFormat => _chain.WaveFormat;

    public MainFilterProvider(ISampleProvider pChain)
    {
        _chain = pChain;
        _filter = BiQuadFilter.LowPassFilter(44100f, 12000f, 1f);
    }

    public int Read(float[] buffer, int offset, int count)
    {
        _chain.Read(buffer, offset, count);
        int samples = 0;
        for (samples = 0; samples < count; samples++)
        {
            buffer[offset + samples] = _filter.Transform(buffer[offset + samples]);
        }
        return samples;
    }
}
