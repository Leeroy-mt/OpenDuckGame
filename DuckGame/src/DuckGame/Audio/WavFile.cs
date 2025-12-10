using System.IO;

namespace DuckGame;

public class WavFile
{
    private WavHeader _header;

    private short[][] _stereoData = new short[2][];

    private string _fileName = "";

    public short[][] stereoData => _stereoData;

    public int size => (int)(_header.dataSize / _header.blockSize);

    public int sampleRate => (int)_header.sampleRate;

    public WavFile(string file)
    {
        _fileName = file;
        _header = default(WavHeader);
        using FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        using BinaryReader br = new BinaryReader(fs);
        try
        {
            _header.riffID = br.ReadBytes(4);
            _header.size = br.ReadUInt32();
            _header.wavID = br.ReadBytes(4);
            _header.fmtID = br.ReadBytes(4);
            _header.fmtSize = br.ReadUInt32();
            _header.format = br.ReadUInt16();
            _header.channels = br.ReadUInt16();
            _header.sampleRate = br.ReadUInt32();
            _header.bytePerSec = br.ReadUInt32();
            _header.blockSize = br.ReadUInt16();
            _header.bit = br.ReadUInt16();
            while (true)
            {
                _header.dataID = br.ReadBytes(4);
                _header.dataSize = br.ReadUInt32();
                if (_header.dataID[0] == 100)
                {
                    break;
                }
                br.ReadBytes((int)_header.dataSize);
            }
            if (_header.channels == 1)
            {
                uint count = _header.dataSize / _header.blockSize;
                _stereoData[0] = new short[count];
                for (int i = 0; i < count; i++)
                {
                    _stereoData[0][i] = (short)br.ReadUInt16();
                }
            }
            else if (_header.channels == 2)
            {
                uint count2 = _header.dataSize / _header.blockSize;
                _stereoData[0] = new short[count2];
                _stereoData[1] = new short[count2];
                for (int j = 0; j < count2; j++)
                {
                    _stereoData[0][j] = (short)br.ReadUInt16();
                    _stereoData[1][j] = (short)br.ReadUInt16();
                }
            }
        }
        finally
        {
            br?.Close();
            fs?.Close();
        }
    }
}
