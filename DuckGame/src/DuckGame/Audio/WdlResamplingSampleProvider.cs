using NAudio.Wave;

namespace DuckGame;

/// <summary>
/// Fully managed resampling sample provider, based on the WDL Resampler
/// </summary>
public class WdlResamplingSampleProvider : ISampleProvider
{
    private readonly WdlResampler resampler;

    private WaveFormat outFormat;

    private readonly ISampleProvider source;

    private readonly int channels;

    public int sampleRate
    {
        get
        {
            return outFormat.SampleRate;
        }
        set
        {
            if (outFormat == null || outFormat.SampleRate != value)
            {
                outFormat = WaveFormat.CreateIeeeFloatWaveFormat(value, channels);
                resampler.SetRates(source.WaveFormat.SampleRate, outFormat.SampleRate);
            }
        }
    }

    /// <summary>
    /// Output WaveFormat
    /// </summary>
    public WaveFormat WaveFormat => outFormat;

    public WdlResamplingSampleProvider(ISampleProvider source)
        : this(source, source.WaveFormat.SampleRate)
    {
    }

    public WdlResamplingSampleProvider(ISampleProvider source, int pSampleRate)
    {
        resampler = new WdlResampler();
        resampler.SetMode(interp: true, 0, sinc: false);
        resampler.SetFilterParms();
        resampler.SetFeedMode(wantInputDriven: false);
        channels = source.WaveFormat.Channels;
        this.source = source;
        sampleRate = pSampleRate;
    }

    /// <summary>
    /// Reads from this sample provider
    /// </summary>
    public int Read(float[] buffer, int offset, int count)
    {
        int framesRequested = count / channels;
        float[] inBuffer;
        int inBufferOffset;
        int inNeeded = resampler.ResamplePrepare(framesRequested, outFormat.Channels, out inBuffer, out inBufferOffset);
        int inAvailable = source.Read(inBuffer, inBufferOffset, inNeeded * channels) / channels;
        return resampler.ResampleOut(buffer, offset, inAvailable, framesRequested, channels) * channels;
    }
}
