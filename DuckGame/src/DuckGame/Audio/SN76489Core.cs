namespace DuckGame;

public sealed class SN76489Core
{
    private static float[] volumeTable = new float[64]
    {
        0.25f, 0.2442f, 0.194f, 0.1541f, 0.1224f, 0.0972f, 0.0772f, 0.0613f, 0.0487f, 0.0386f,
        0.0307f, 0.0244f, 0.0193f, 0.0154f, 0.0122f, 0f, -0.25f, -0.2442f, -0.194f, -0.1541f,
        -0.1224f, -0.0972f, -0.0772f, -0.0613f, -0.0487f, -0.0386f, -0.0307f, -0.0244f, -0.0193f, -0.0154f,
        -0.0122f, 0f, 0.25f, 0.2442f, 0.194f, 0.1541f, 0.1224f, 0.0972f, 0.0772f, 0.0613f,
        0.0487f, 0.0386f, 0.0307f, 0.0244f, 0.0193f, 0.0154f, 0.0122f, 0f, 0f, 0f,
        0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
        0f, 0f, 0f, 0f
    };

    private uint volA;

    private uint volB;

    private uint volC;

    private uint volD;

    private int divA;

    private int divB;

    private int divC;

    private int divD;

    private int cntA;

    private int cntB;

    private int cntC;

    private int cntD;

    private float outA;

    private float outB;

    private float outC;

    private float outD;

    private uint noiseLFSR;

    private uint noiseTap;

    private uint latchedChan;

    private bool latchedVolume;

    private float ticksPerSample;

    private float ticksCount;

    public SN76489Core()
    {
        clock(3500000f);
        reset();
    }

    public void clock(float f)
    {
        ticksPerSample = f / 16f / 44100f;
    }

    public void reset()
    {
        volA = 15u;
        volB = 15u;
        volC = 15u;
        volD = 15u;
        outA = 0f;
        outB = 0f;
        outC = 0f;
        outD = 0f;
        latchedChan = 0u;
        latchedVolume = false;
        noiseLFSR = 32768u;
        ticksCount = ticksPerSample;
    }

    public uint getDivByNumber(uint chan)
    {
        return chan switch
        {
            0u => (uint)divA,
            1u => (uint)divB,
            2u => (uint)divC,
            3u => (uint)divD,
            _ => 0u,
        };
    }

    public void setDivByNumber(uint chan, uint div)
    {
        switch (chan)
        {
            case 0u:
                divA = (int)div;
                break;
            case 1u:
                divB = (int)div;
                break;
            case 2u:
                divC = (int)div;
                break;
            case 3u:
                divD = (int)div;
                break;
        }
    }

    public uint getVolByNumber(uint chan)
    {
        return chan switch
        {
            0u => volA,
            1u => volB,
            2u => volC,
            3u => volD,
            _ => 0u,
        };
    }

    public void setVolByNumber(uint chan, uint vol)
    {
        switch (chan)
        {
            case 0u:
                volA = vol;
                break;
            case 1u:
                volB = vol;
                break;
            case 2u:
                volC = vol;
                break;
            case 3u:
                volD = vol;
                break;
        }
    }

    public void write(int val)
    {
        int chan;
        int cdiv;
        if ((val & 0x80) != 0)
        {
            chan = (val >> 5) & 3;
            cdiv = (int)(getDivByNumber((uint)chan) & 0xFFF0) | (val & 0xF);
            latchedChan = (uint)chan;
            latchedVolume = (((val & 0x10) != 0) ? true : false);
        }
        else
        {
            chan = (int)latchedChan;
            cdiv = (int)(getDivByNumber((uint)chan) & 0xF) | ((val & 0x3F) << 4);
        }
        if (latchedVolume)
        {
            setVolByNumber((uint)chan, (getVolByNumber((uint)chan) & 0x10) | (uint)(val & 0xF));
            return;
        }
        setDivByNumber((uint)chan, (uint)cdiv);
        if (chan == 3)
        {
            noiseTap = ((((cdiv >> 2) & 1) == 0) ? 1u : 9u);
            noiseLFSR = 32768u;
        }
    }

    public float render()
    {
        uint i = 0u;
        if (i < 1)
        {
            while (ticksCount > 0f)
            {
                cntA--;
                if (cntA < 0)
                {
                    if (divA > 1)
                    {
                        volA ^= 16u;
                        outA = volumeTable[volA];
                    }
                    cntA = divA;
                }
                cntB--;
                if (cntB < 0)
                {
                    if (divB > 1)
                    {
                        volB ^= 16u;
                        outB = volumeTable[volB];
                    }
                    cntB = divB;
                }
                cntC--;
                if (cntC < 0)
                {
                    if (divC > 1)
                    {
                        volC ^= 16u;
                        outC = volumeTable[volC];
                    }
                    cntC = divC;
                }
                cntD--;
                if (cntD < 0)
                {
                    uint cdiv = (uint)(divD & 3);
                    if (cdiv < 3)
                    {
                        cntD = 16 << (int)cdiv;
                    }
                    else
                    {
                        cntD = divC << 1;
                    }
                    uint tap;
                    if (noiseTap == 9)
                    {
                        tap = noiseLFSR & noiseTap;
                        tap ^= tap >> 8;
                        tap ^= tap >> 4;
                        tap ^= tap >> 2;
                        tap ^= tap >> 1;
                        tap &= 1;
                    }
                    else
                    {
                        tap = noiseLFSR & 1;
                    }
                    noiseLFSR = (noiseLFSR >> 1) | (tap << 15);
                    volD = (volD & 0xF) | (((noiseLFSR & 1) ^ 1) << 4);
                    outD = volumeTable[volD];
                }
                ticksCount -= 1f;
            }
            ticksCount += ticksPerSample;
            return outA + outB + outC + outD;
        }
        return 0f;
    }
}
