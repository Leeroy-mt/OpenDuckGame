using Microsoft.Xna.Framework.Audio;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace DuckGame;

public class VGMSong
{
    private DynamicSoundEffectInstance _instance;

    private byte[] _buffer;

    private int[] _intBuffer;

    private YM2612 _chip = new YM2612();

    private SN76489 _psg = new SN76489();

    private bool _iSaidStop;

    private float _volume = 1f;

    private bool _looped = true;

    private float _playbackSpeed = 1f;

    private const uint FCC_VGM = 544040790u;

    private uint _VGMDataLen;

    private VGM_HEADER _VGMHead;

    private BinaryReader _vgmReader;

    private byte[] _DACData;

    private byte[] _VGMData;

    private int _DACOffset;

    private int _VGMDataOffset;

    private byte _lastCommand;

    private int _wait;

    private float _waitInc;

    public SoundState state
    {
        get
        {
            if (!_iSaidStop)
            {
                return _instance.State;
            }
            return SoundState.Stopped;
        }
    }

    public float volume
    {
        get
        {
            return _volume;
        }
        set
        {
            _volume = MathHelper.Clamp(value, 0f, 1f);
            if (_instance != null && _instance.State == SoundState.Playing)
            {
                _instance.Volume = _volume;
            }
        }
    }

    public bool looped
    {
        get
        {
            return _looped;
        }
        set
        {
            _looped = value;
        }
    }

    public bool gameFroze { get; set; }

    public float playbackSpeed
    {
        get
        {
            return _playbackSpeed;
        }
        set
        {
            _playbackSpeed = value;
        }
    }

    public VGMSong(string file)
    {
        _instance = new DynamicSoundEffectInstance(44100, AudioChannels.Stereo);
        _buffer = new byte[_instance.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(150.0))];
        _intBuffer = new int[_buffer.Length / 2];
        _instance.BufferNeeded += StreamVGM;
        OpenVGMFile(file);
        int Sound_Rate = 44100;
        int Clock_NTSC = (int)_VGMHead.lngHzYM2612;
        _chip.Initialize(Clock_NTSC, Sound_Rate);
        _psg.Initialize(_VGMHead.lngHzPSG);
    }

    public void Terminate()
    {
        _instance.Dispose();
    }

    public void Play()
    {
        _instance.Stop();
        _instance.Play();
        _instance.Volume = _volume;
        _iSaidStop = false;
    }

    public void Pause()
    {
        _instance.Pause();
        _iSaidStop = true;
    }

    public void Resume()
    {
        _instance.Resume();
        _instance.Volume = _volume;
        _iSaidStop = false;
    }

    public void Stop()
    {
        _instance.Stop();
        _instance.Volume = 0f;
        _iSaidStop = true;
        _vgmReader.BaseStream.Seek(0L, SeekOrigin.Begin);
    }

    private static VGM_HEADER ReadVGMHeader(BinaryReader hFile)
    {
        VGM_HEADER CurHead = new VGM_HEADER();
        FieldInfo[] fields = typeof(VGM_HEADER).GetFields();
        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == typeof(uint))
            {
                uint val = hFile.ReadUInt32();
                field.SetValue(CurHead, val);
            }
            else if (field.FieldType == typeof(ushort))
            {
                ushort val2 = hFile.ReadUInt16();
                field.SetValue(CurHead, val2);
            }
            else if (field.FieldType == typeof(char))
            {
                char val3 = hFile.ReadChar();
                field.SetValue(CurHead, val3);
            }
            else if (field.FieldType == typeof(byte))
            {
                byte val4 = hFile.ReadByte();
                field.SetValue(CurHead, val4);
            }
        }
        if (CurHead.lngVersion < 257)
        {
            CurHead.lngRate = 0u;
        }
        if (CurHead.lngVersion < 272)
        {
            CurHead.shtPSG_Feedback = 0;
            CurHead.bytPSG_SRWidth = 0;
            CurHead.lngHzYM2612 = CurHead.lngHzYM2413;
            CurHead.lngHzYM2151 = CurHead.lngHzYM2413;
        }
        if (CurHead.lngHzPSG != 0)
        {
            if (CurHead.shtPSG_Feedback == 0)
            {
                CurHead.shtPSG_Feedback = 9;
            }
            if (CurHead.bytPSG_SRWidth == 0)
            {
                CurHead.bytPSG_SRWidth = 16;
            }
        }
        return CurHead;
    }

    private bool OpenVGMFile(string fileName)
    {
        bool zipped = fileName.Contains(".vgz");
        uint FileSize = 0u;
        FileStream vgmFile = File.Open(fileName, FileMode.Open);
        if (zipped)
        {
            vgmFile.Position = vgmFile.Length - 4;
            byte[] b = new byte[4];
            vgmFile.Read(b, 0, 4);
            FileSize = BitConverter.ToUInt32(b, 0);
            vgmFile.Position = 0L;
            GZipStream stream = new GZipStream(vgmFile, CompressionMode.Decompress);
            _vgmReader = new BinaryReader(stream);
        }
        else
        {
            FileSize = (uint)vgmFile.Length;
            _vgmReader = new BinaryReader(vgmFile);
        }
        if (_vgmReader.ReadUInt32() != 544040790)
        {
            return false;
        }
        _VGMDataLen = FileSize;
        _VGMHead = ReadVGMHeader(_vgmReader);
        if (zipped)
        {
            _vgmReader.Close();
            vgmFile = File.Open(fileName, FileMode.Open);
            GZipStream stream2 = new GZipStream(vgmFile, CompressionMode.Decompress);
            _vgmReader = new BinaryReader(stream2);
        }
        else
        {
            _vgmReader.BaseStream.Seek(0L, SeekOrigin.Begin);
        }
        int offset = (int)_VGMHead.lngDataOffset;
        if (offset == 0 || offset == 12)
        {
            offset = 64;
        }
        _VGMDataOffset = offset;
        _vgmReader.ReadBytes(offset);
        _VGMData = _vgmReader.ReadBytes((int)(FileSize - offset));
        _vgmReader = new BinaryReader(new MemoryStream(_VGMData));
        if ((byte)_vgmReader.PeekChar() == 103)
        {
            _vgmReader.ReadByte();
            if ((byte)_vgmReader.PeekChar() == 102)
            {
                _vgmReader.ReadByte();
                _vgmReader.ReadByte();
                uint size = _vgmReader.ReadUInt32();
                _DACData = _vgmReader.ReadBytes((int)size);
            }
        }
        vgmFile.Close();
        return true;
    }

    private void StreamVGM(object sender, EventArgs e)
    {
        if (_iSaidStop)
        {
            return;
        }
        if (_lastCommand == 102 && !_looped)
        {
            _lastCommand = 0;
            _instance.Volume = 0f;
            _iSaidStop = true;
            Stop();
            return;
        }
        int[] bufferData = new int[2];
        bool writeSample = false;
        int samplesWritten = 0;
        int samplesToWrite = _intBuffer.Length / 2;
        bool songEnded = false;
        while (samplesWritten != samplesToWrite)
        {
            if (_wait == 0 && !gameFroze)
            {
                writeSample = false;
                byte command = (_lastCommand = _vgmReader.ReadByte());
                switch (command)
                {
                    case 79:
                        _vgmReader.ReadByte();
                        break;
                    case 80:
                        {
                            byte aa3 = _vgmReader.ReadByte();
                            _psg.Write(aa3);
                            break;
                        }
                    case 82:
                        {
                            byte aa2 = _vgmReader.ReadByte();
                            byte dd2 = _vgmReader.ReadByte();
                            _chip.WritePort0(aa2, dd2);
                            break;
                        }
                    case 83:
                        {
                            byte aa = _vgmReader.ReadByte();
                            byte dd = _vgmReader.ReadByte();
                            _chip.WritePort1(aa, dd);
                            break;
                        }
                    case 97:
                        {
                            ushort time = _vgmReader.ReadUInt16();
                            _wait = time;
                            if (_wait != 0)
                            {
                                writeSample = true;
                            }
                            break;
                        }
                    case 98:
                        _wait = 735;
                        writeSample = true;
                        break;
                    case 99:
                        _wait = 882;
                        writeSample = true;
                        break;
                    case 224:
                        {
                            uint offset = _vgmReader.ReadUInt32();
                            _DACOffset = (int)offset;
                            break;
                        }
                    case 103:
                        {
                            _vgmReader.ReadByte();
                            _vgmReader.ReadByte();
                            uint size = _vgmReader.ReadUInt32();
                            _vgmReader.BaseStream.Position += size;
                            break;
                        }
                    case 102:
                        if (!_looped)
                        {
                            _vgmReader.BaseStream.Seek(0L, SeekOrigin.Begin);
                            songEnded = true;
                        }
                        else if (_VGMHead.lngLoopOffset != 0)
                        {
                            _vgmReader.BaseStream.Seek(_VGMHead.lngLoopOffset - _VGMDataOffset, SeekOrigin.Begin);
                        }
                        else
                        {
                            _vgmReader.BaseStream.Seek(0L, SeekOrigin.Begin);
                        }
                        break;
                }
                if (command >= 112 && command <= 127)
                {
                    _wait = (command & 0xF) + 1;
                    if (_wait != 0)
                    {
                        writeSample = true;
                    }
                }
                else if (command >= 128 && command <= 143)
                {
                    _wait = command & 0xF;
                    _chip.WritePort0(42, _DACData[_DACOffset]);
                    _DACOffset++;
                    if (_wait != 0)
                    {
                        writeSample = true;
                    }
                }
                if (_wait != 0)
                {
                    _wait--;
                }
            }
            else
            {
                writeSample = true;
                if (_wait > 0)
                {
                    _waitInc += _playbackSpeed;
                    while (_wait > 0 && _waitInc >= 1f)
                    {
                        _waitInc -= 1f;
                        _wait--;
                    }
                }
            }
            if (songEnded)
            {
                break;
            }
            if (writeSample)
            {
                _chip.Update(bufferData, 1);
                short aLeft = (short)bufferData[0];
                short aRight = (short)bufferData[1];
                _psg.Update(bufferData, 1);
                short bLeft = (short)bufferData[0];
                short bRight = (short)bufferData[1];
                _intBuffer[samplesWritten * 2] = Maths.Clamp((aLeft + bLeft) * 2, -32768, 32767);
                _intBuffer[samplesWritten * 2 + 1] = Maths.Clamp((aRight + bRight) * 2, -32768, 32767);
                samplesWritten++;
                if (samplesWritten == samplesToWrite)
                {
                    break;
                }
            }
        }
        for (int i = 0; i < _intBuffer.Length; i++)
        {
            short sValue = (short)_intBuffer[i];
            _buffer[i * 2] = (byte)(sValue & 0xFF);
            _buffer[i * 2 + 1] = (byte)((sValue >> 8) & 0xFF);
        }
        samplesWritten *= 2;
        if ((float)samplesWritten / 4f - (float)(int)((float)samplesWritten / 4f) > 0f)
        {
            samplesWritten -= 2;
        }
        _instance.SubmitBuffer(_buffer, 0, samplesWritten);
        _instance.SubmitBuffer(_buffer, samplesWritten, samplesWritten);
    }
}
