using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuckGame;

public class BitBuffer
{
    private static int[] _maxMasks;

    private byte[] _buffer = new byte[64];

    private bool _dirty;

    private int _offsetPosition;

    private int _endPosition;

    private int _bitEndOffset;

    private int _bitOffsetPosition;

    private byte[] _trimmedBuffer;

    private static int[] _readMasks;

    private bool _allowPacking = true;

    private int offset;

    private int currentBit;

    public static List<Type> kTypeIndexList = new List<Type>
    {
        typeof(string),
        typeof(byte[]),
        typeof(BitBuffer),
        typeof(float),
        typeof(double),
        typeof(byte),
        typeof(sbyte),
        typeof(bool),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(char),
        typeof(Vector2),
        typeof(Color),
        typeof(NetIndex16),
        typeof(NetIndex2),
        typeof(NetIndex4),
        typeof(NetIndex8),
        typeof(Thing)
    };

    /// <summary>
    /// This BitBuffers internal buffer. This may have zeroes at the end, as the buffer size is doubled whenever it's filled.
    /// </summary>
    public byte[] buffer => _buffer;

    /// <summary>
    /// A byte[] representation of all data in the buffer.
    /// </summary>
    public byte[] data
    {
        get
        {
            try
            {
                byte[] ret = new byte[lengthInBytes];
                Array.Copy(_buffer, ret, lengthInBytes);
                return ret;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public int position
    {
        get
        {
            return _offsetPosition;
        }
        set
        {
            if (_offsetPosition != value)
            {
                _dirty = true;
            }
            _offsetPosition = value;
            if (_offsetPosition > _endPosition)
            {
                _endPosition = _offsetPosition;
                _bitEndOffset = 0;
            }
        }
    }

    public uint positionInBits
    {
        get
        {
            return (uint)(position * 8 + bitOffset);
        }
        set
        {
            position = (int)(value / 8);
            bitOffset = (int)(value % 8);
        }
    }

    public int bitOffset
    {
        get
        {
            return _bitOffsetPosition;
        }
        set
        {
            if (_bitOffsetPosition != value)
            {
                _dirty = true;
            }
            _bitOffsetPosition = value;
            if (_endPosition == _offsetPosition && _bitOffsetPosition > _bitEndOffset)
            {
                _bitEndOffset = value;
            }
        }
    }

    public bool isPacked => _bitEndOffset != 0;

    public int lengthInBits => _endPosition * 8 + _bitEndOffset;

    public int lengthInBytes
    {
        get
        {
            return _endPosition + ((_bitEndOffset > 0) ? 1 : 0);
        }
        set
        {
            _endPosition = value;
        }
    }

    public bool allowPacking => _allowPacking;

    public string ReadTokenizedString()
    {
        int token = ReadInt();
        if (token >= TokenDeserializer.instance._tokens.Count)
        {
            throw new Exception("BitBuffer.ReadTokenizedString() encountered an invalid token.");
        }
        return TokenDeserializer.instance._tokens[token];
    }

    private void WriteTokenizedString(string val)
    {
        Write(TokenSerializer.instance.Token(val));
    }

    public override string ToString()
    {
        string s = "";
        for (int i = 0; i < lengthInBytes; i++)
        {
            s += _buffer[i];
            s += "|";
        }
        return s;
    }

    public static BitBuffer FromString(string s)
    {
        BitBuffer b = new BitBuffer();
        try
        {
            string[] array = s.Split('|');
            foreach (string part in array)
            {
                if (!(part == ""))
                {
                    b.Write(Convert.ToByte(part));
                }
            }
            b.position = 0;
            return b;
        }
        catch (Exception)
        {
            DevConsole.Log(DCSection.General, "BitBuffer conversion from string failed.");
            return new BitBuffer();
        }
    }

    public static long GetMaxValue(int bits)
    {
        if (_maxMasks == null)
        {
            _maxMasks = new int[64];
            int val = 0;
            for (int i = 0; i < 64; i++)
            {
                val |= 1;
                _maxMasks[i] = val;
                val <<= 1;
            }
        }
        return _maxMasks[bits];
    }

    public byte[] GetBytes()
    {
        if (_trimmedBuffer != null && !_dirty)
        {
            return _trimmedBuffer;
        }
        _dirty = false;
        _trimmedBuffer = new byte[lengthInBytes];
        for (int i = 0; i < lengthInBytes; i++)
        {
            _trimmedBuffer[i] = _buffer[i];
        }
        return _trimmedBuffer;
    }

    private void calculateReadMasks()
    {
        if (_readMasks == null)
        {
            _readMasks = new int[64];
            int val = 0;
            for (int i = 0; i < 64; i++)
            {
                val |= 1;
                _readMasks[i] = val;
                val <<= 1;
            }
        }
    }

    public BitBuffer(bool allowPacking = true)
    {
        calculateReadMasks();
        _allowPacking = allowPacking;
    }

    public BitBuffer(byte[] data, int bits = 0, bool allowPacking = true)
    {
        _allowPacking = allowPacking;
        calculateReadMasks();
        Write(data);
        SeekToStart();
        if (bits > 0 && _endPosition * 8 > bits)
        {
            _endPosition--;
            _bitEndOffset = bits - _endPosition * 8;
        }
    }

    public BitBuffer(byte[] data, bool copyData)
    {
        _allowPacking = false;
        calculateReadMasks();
        if (copyData)
        {
            Write(data);
            SeekToStart();
        }
        else
        {
            _buffer = data;
        }
    }

    public void SeekToStart()
    {
        position = 0;
        _bitOffsetPosition = 0;
    }

    public void Fill(byte[] bytes, int offset = 0, int vbitOffset = 0)
    {
        _buffer = bytes;
        position = offset;
        _bitOffsetPosition = vbitOffset;
    }

    public BitBuffer Instance()
    {
        return new BitBuffer
        {
            _buffer = buffer,
            _offsetPosition = _offsetPosition,
            _endPosition = _endPosition,
            _bitEndOffset = _bitEndOffset,
            _bitOffsetPosition = _bitOffsetPosition
        };
    }

    public int ReadPackedBits(int bits)
    {
        if (bits == 0)
        {
            return 0;
        }
        int number = 0;
        if (bits <= 8 - bitOffset)
        {
            number = (_buffer[position] >> bitOffset) & _readMasks[bits - 1];
            bitOffset += bits;
        }
        else
        {
            int read = 0;
            int soFar = 0;
            while (true)
            {
                if (bitOffset > 7)
                {
                    bitOffset = 0;
                    position++;
                }
                if (bits <= 0)
                {
                    break;
                }
                int numRead = 8 - bitOffset;
                if (numRead > bits)
                {
                    numRead = bits;
                }
                read = (_buffer[position] >> bitOffset) & _readMasks[numRead - 1];
                bits -= numRead;
                read <<= soFar;
                number |= read;
                bitOffset += numRead;
                soFar += numRead;
            }
        }
        if (bitOffset > 7)
        {
            bitOffset = 0;
            position++;
        }
        return number;
    }

    public byte[] ReadPacked(int bytes)
    {
        byte[] data = new byte[bytes];
        for (int i = 0; i < bytes; i++)
        {
            data[i] = (byte)ReadPackedBits(8);
        }
        return data;
    }

    public void WritePacked(int number, int bits)
    {
        try
        {
            if (lengthInBits + bits > _buffer.Length * 8)
            {
                resize(_buffer.Length * 2);
            }
            currentBit = 0;
            while (bits > 0)
            {
                _buffer[position] |= (byte)((number & 1) << bitOffset);
                number >>= 1;
                bitOffset++;
                bits--;
                if (bitOffset == 8)
                {
                    position++;
                    bitOffset = 0;
                }
            }
        }
        catch (Exception ex)
        {
            Main.SpecialCode = Main.SpecialCode + bits + ", " + lengthInBits + ", " + _buffer.Length + ", " + position + ", " + number;
            throw ex;
        }
    }

    public void WritePacked(byte[] data)
    {
        foreach (byte b in data)
        {
            WritePacked(b, 8);
        }
    }

    public void WritePacked(byte[] data, int bits)
    {
        if (position + (int)Math.Ceiling((float)bits / 8f) > _buffer.Length)
        {
            resize((position + (int)Math.Ceiling((float)bits / 8f)) * 2);
        }
        int cur = 0;
        if (!isPacked)
        {
            while (bits >= 8)
            {
                _buffer[position] = data[cur];
                position++;
                cur++;
                bits -= 8;
            }
        }
        else
        {
            while (bits >= 8)
            {
                WritePacked(data[cur], 8);
                cur++;
                bits -= 8;
            }
        }
        if (bits > 0)
        {
            WritePacked(data[cur], bits);
        }
    }

    public BitBuffer ReadBitBuffer(bool allowPacking = true)
    {
        int bits = ReadUShort();
        if (bits == 65535)
        {
            bits = ReadInt();
        }
        byte[] data = null;
        if (allowPacking)
        {
            int bytes = (int)Math.Ceiling((float)bits / 8f);
            data = new byte[bytes];
            int writeBits = bits;
            for (int i = 0; i < bytes; i++)
            {
                data[i] = (byte)ReadPackedBits((writeBits >= 8) ? 8 : writeBits);
                if (writeBits >= 8)
                {
                    writeBits -= 8;
                }
            }
        }
        else
        {
            data = new byte[bits];
            Array.Copy(buffer, position, data, 0, bits);
            position += bits;
            bits = 0;
        }
        return new BitBuffer(data, bits, allowPacking);
    }

    public string ReadString()
    {
        if (TokenDeserializer.instance != null)
        {
            return ReadTokenizedString();
        }
        int length = ReadUShort();
        if (length == 65535)
        {
            int tBitOffset = bitOffset;
            int tPosition = position;
            if (ReadUShort() == 42252)
            {
                length = ReadInt();
            }
            else
            {
                position = tPosition;
                bitOffset = tBitOffset;
            }
        }
        if (bitOffset != 0)
        {
            byte[] data = ReadPacked(length);
            return Encoding.UTF8.GetString(data);
        }
        string result = Encoding.UTF8.GetString(_buffer, position, length);
        position += length;
        return result;
    }

    public long ReadLong()
    {
        if (bitOffset != 0)
        {
            return BitConverter.ToInt64(ReadPacked(8), 0);
        }
        long result = BitConverter.ToInt64(_buffer, position);
        position += 8;
        return result;
    }

    public ulong ReadULong()
    {
        if (bitOffset != 0)
        {
            return BitConverter.ToUInt64(ReadPacked(8), 0);
        }
        ulong result = BitConverter.ToUInt64(_buffer, position);
        position += 8;
        return result;
    }

    public int ReadInt()
    {
        if (bitOffset != 0)
        {
            return BitConverter.ToInt32(ReadPacked(4), 0);
        }
        int result = BitConverter.ToInt32(_buffer, position);
        position += 4;
        return result;
    }

    public uint ReadUInt()
    {
        if (bitOffset != 0)
        {
            return BitConverter.ToUInt32(ReadPacked(4), 0);
        }
        uint result = BitConverter.ToUInt32(_buffer, position);
        position += 4;
        return result;
    }

    public short ReadShort()
    {
        if (bitOffset != 0)
        {
            return BitConverter.ToInt16(ReadPacked(2), 0);
        }
        short result = BitConverter.ToInt16(_buffer, position);
        position += 2;
        return result;
    }

    public ushort ReadUShort()
    {
        if (bitOffset != 0)
        {
            return BitConverter.ToUInt16(ReadPacked(2), 0);
        }
        ushort result = BitConverter.ToUInt16(_buffer, position);
        position += 2;
        return result;
    }

    public float ReadFloat()
    {
        if (bitOffset != 0)
        {
            return BitConverter.ToSingle(ReadPacked(4), 0);
        }
        float result = BitConverter.ToSingle(_buffer, position);
        position += 4;
        return result;
    }

    public Vector2 ReadVec2()
    {
        return new Vector2
        {
            X = ReadFloat(),
            Y = ReadFloat()
        };
    }

    public Color ReadColor()
    {
        return new Color
        {
            R = ReadByte(),
            G = ReadByte(),
            B = ReadByte(),
            A = ReadByte()
        };
    }

    public Color ReadRGBColor()
    {
        return new Color
        {
            R = ReadByte(),
            G = ReadByte(),
            B = ReadByte(),
            A = byte.MaxValue
        };
    }

    public double ReadDouble()
    {
        if (bitOffset != 0)
        {
            return BitConverter.ToDouble(ReadPacked(8), 0);
        }
        double result = BitConverter.ToDouble(_buffer, position);
        position += 8;
        return result;
    }

    public char ReadChar()
    {
        if (bitOffset != 0)
        {
            return BitConverter.ToChar(ReadPacked(2), 0);
        }
        char result = BitConverter.ToChar(_buffer, position);
        position += 2;
        return result;
    }

    public byte ReadByte()
    {
        if (bitOffset != 0)
        {
            return ReadPacked(1)[0];
        }
        byte result = _buffer[position];
        position++;
        return result;
    }

    public byte[] ReadBytes()
    {
        int size = ReadInt();
        byte[] data = new byte[size];
        Array.Copy(buffer, position, data, 0, size);
        position += size;
        return data;
    }

    public sbyte ReadSByte()
    {
        if (bitOffset != 0)
        {
            return (sbyte)ReadPacked(1)[0];
        }
        sbyte result = (sbyte)_buffer[position];
        position++;
        return result;
    }

    public bool ReadBool()
    {
        if (_allowPacking)
        {
            return ReadPackedBits(1) > 0;
        }
        if (ReadByte() <= 0)
        {
            return false;
        }
        return true;
    }

    public NetIndex4 ReadNetIndex4()
    {
        return new NetIndex4(ReadPackedBits(4));
    }

    public NetIndex8 ReadNetIndex8()
    {
        return new NetIndex8(ReadPackedBits(8));
    }

    public NetIndex16 ReadNetIndex16()
    {
        return new NetIndex16(ReadPackedBits(16));
    }

    public byte[] ReadData(int length)
    {
        byte[] data = new byte[length];
        Buffer.BlockCopy(buffer, position, data, 0, length);
        position += length;
        return data;
    }

    public object Read(Type type, bool allowPacking = true)
    {
        if (type == typeof(string))
        {
            return ReadString();
        }
        if (type == typeof(float))
        {
            return ReadFloat();
        }
        if (type == typeof(double))
        {
            return ReadDouble();
        }
        if (type == typeof(byte))
        {
            return ReadByte();
        }
        if (type == typeof(sbyte))
        {
            return ReadSByte();
        }
        if (type == typeof(bool))
        {
            return ReadBool();
        }
        if (type == typeof(short))
        {
            return ReadShort();
        }
        if (type == typeof(ushort))
        {
            return ReadUShort();
        }
        if (type == typeof(int))
        {
            return ReadInt();
        }
        if (type == typeof(uint))
        {
            return ReadUInt();
        }
        if (type == typeof(long))
        {
            return ReadLong();
        }
        if (type == typeof(ulong))
        {
            return ReadULong();
        }
        if (type == typeof(char))
        {
            return ReadChar();
        }
        if (type == typeof(Vector2))
        {
            return ReadVec2();
        }
        if (type == typeof(BitBuffer))
        {
            return ReadBitBuffer(allowPacking);
        }
        if (type == typeof(NetIndex16))
        {
            return new NetIndex16(ReadUShort());
        }
        if (type == typeof(NetIndex2))
        {
            return new NetIndex2((int)ReadBits(typeof(int), 2));
        }
        if (type == typeof(NetIndex4))
        {
            return new NetIndex4((int)ReadBits(typeof(int), 4));
        }
        if (type == typeof(NetIndex8))
        {
            return new NetIndex8((int)ReadBits(typeof(int), 8));
        }
        if (typeof(Thing).IsAssignableFrom(type))
        {
            return ReadThing(type);
        }
        throw new Exception("Trying to read unsupported type " + type?.ToString() + " from BitBuffer!");
    }

    public Thing ReadThing(Type pThingType)
    {
        byte levelIndex = ReadByte();
        ushort typeIndex = (ushort)ReadBits(typeof(ushort), 10);
        ushort readIndex = ReadUShort();
        if (levelIndex != DuckNetwork.levelIndex || readIndex == 0)
        {
            return null;
        }
        if (typeIndex == 0)
        {
            return GhostManager.context.GetSpecialSync(readIndex);
        }
        NetIndex16 index = readIndex;
        Profile p = GhostObject.IndexToProfile(index);
        if (p != null && p.removedGhosts.ContainsKey(index))
        {
            return p.removedGhosts[index].thing;
        }
        Type realType = Editor.IDToType[typeIndex];
        if (!pThingType.IsAssignableFrom(realType))
        {
            DevConsole.Log(DCSection.GhostMan, "@error Type mismatch, ignoring ghost (" + index.ToString() + "(" + realType.GetType().Name + " vs. " + pThingType.Name + "))@error");
            return null;
        }
        GhostObject ghost = GhostManager.context.GetGhost(index);
        if (ghost != null && ghost.thing.GetType() != realType)
        {
            DevConsole.Log(DCSection.GhostMan, "@error Type mismatch, removing ghost (" + index.ToString() + " " + ghost.thing.GetType().ToString() + "(my type) vs. " + realType.ToString() + "(your type))@error");
            GhostManager.changingGhostType = true;
            GhostManager.context.RemoveGhost(ghost, 0);
            GhostManager.changingGhostType = false;
            ghost = null;
        }
        if (ghost == null)
        {
            Thing t = Editor.CreateThing(realType);
            t.connection = NetworkConnection.context;
            t.authority = 1;
            if (p != null && index > p.latestGhostIndex)
            {
                p.latestGhostIndex = index;
            }
            if (levelIndex != Level.core.currentLevel.networkIndex)
            {
                ghost = new GhostObject(t, GhostManager.context, index);
                t.Position = new Vector2(-2000f, -2000f);
                GhostManager.context.pendingBitBufferGhosts.Add(ghost);
            }
            else
            {
                ghost = GhostManager.context.MakeGhost(t, index);
                t.Position = new Vector2(-2000f, -2000f);
                ghost.ClearStateMask(NetworkConnection.context);
                t.level = Level.current;
                t.isBitBufferCreatedGhostThing = true;
            }
            t.connection = NetworkConnection.context;
        }
        return ghost.thing;
    }

    public object ReadBits(Type t, int bits)
    {
        if (bits == -1)
        {
            return Read(t);
        }
        int val = ReadPackedBits(bits);
        return ConvertType(val, t);
    }

    public T ReadBits<T>(int bits)
    {
        if (bits < 1)
        {
            return default(T);
        }
        int val = ReadPackedBits(bits);
        return (T)ConvertType(val, typeof(T));
    }

    protected object ConvertType(int obj, Type type)
    {
        if (type == typeof(float))
        {
            return (float)obj;
        }
        if (type == typeof(double))
        {
            return (double)obj;
        }
        if (type == typeof(byte))
        {
            return (byte)obj;
        }
        if (type == typeof(sbyte))
        {
            return (sbyte)obj;
        }
        if (type == typeof(short))
        {
            return (short)obj;
        }
        if (type == typeof(ushort))
        {
            return (ushort)obj;
        }
        if (type == typeof(int))
        {
            return obj;
        }
        if (type == typeof(uint))
        {
            return (uint)obj;
        }
        if (type == typeof(long))
        {
            return (long)obj;
        }
        if (type == typeof(ulong))
        {
            return (ulong)obj;
        }
        if (type == typeof(char))
        {
            return (char)obj;
        }
        throw new Exception("unrecognized conversion type " + type);
    }

    public T Read<T>()
    {
        return (T)Read(typeof(T));
    }

    public void AlignToByte()
    {
        if (bitOffset > 0)
        {
            position++;
            bitOffset = 0;
        }
    }

    public void WriteBufferData(BitBuffer val)
    {
        if (!val.isPacked && !isPacked)
        {
            if (position + val.lengthInBytes > _buffer.Length)
            {
                resize(position + val.lengthInBytes);
            }
            for (int i = 0; i < val.lengthInBytes; i++)
            {
                _buffer[position] = val.buffer[i];
                position++;
            }
        }
        else
        {
            WritePacked(val.buffer, val.lengthInBits);
        }
    }

    public void Write(BitBuffer val, bool writeLength = true)
    {
        if (writeLength)
        {
            int size = (val.allowPacking ? val.lengthInBits : val.lengthInBytes);
            if (size > 65534)
            {
                Write(ushort.MaxValue);
                Write(size);
            }
            else
            {
                Write((ushort)size);
            }
        }
        WriteBufferData(val);
    }

    public void Write(byte[] val, bool writeLength)
    {
        if (writeLength)
        {
            Write(val.Length);
        }
        Write(val, 0, val.Length);
    }

    public void Write(byte[] data, int offset = 0, int length = -1)
    {
        if (!isPacked || bitOffset == 0)
        {
            if (length < 0)
            {
                length = data.Length;
            }
            if (position + length > _buffer.Length)
            {
                resize(position + length);
            }
            Array.Copy(data, offset, buffer, position, length);
            position += length;
        }
        else
        {
            WritePacked(data);
        }
    }

    public void Write(string val)
    {
        if (TokenSerializer.instance != null)
        {
            WriteTokenizedString(val);
            return;
        }
        byte[] stringData = Encoding.UTF8.GetBytes(val);
        if (bitOffset != 0)
        {
            Write((ushort)stringData.Count());
            WritePacked(stringData);
            return;
        }
        int len = stringData.Count();
        if (len > 65535)
        {
            Write(ushort.MaxValue);
            Write((ushort)42252);
            Write(len);
        }
        else
        {
            Write((ushort)stringData.Count());
        }
        int size = stringData.Count();
        if (position + size > _buffer.Count())
        {
            resize(position + size);
        }
        stringData.CopyTo(_buffer, position);
        position += size;
    }

    public void Write(long val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (bitOffset != 0)
        {
            WritePacked(bytes);
            return;
        }
        byte size = (byte)bytes.Count();
        if (position + size > _buffer.Count())
        {
            resize(position + size);
        }
        bytes.CopyTo(_buffer, position);
        position += bytes.Count();
    }

    public void Write(ulong val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (bitOffset != 0)
        {
            WritePacked(bytes);
            return;
        }
        byte size = (byte)bytes.Count();
        if (position + size > _buffer.Count())
        {
            resize(position + size);
        }
        bytes.CopyTo(_buffer, position);
        position += bytes.Count();
    }

    public void Write(int val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (bitOffset != 0)
        {
            WritePacked(bytes);
            return;
        }
        byte size = (byte)bytes.Count();
        if (position + size > _buffer.Count())
        {
            resize(position + size);
        }
        bytes.CopyTo(_buffer, position);
        position += bytes.Count();
    }

    public void Write(uint val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (bitOffset != 0)
        {
            WritePacked(bytes);
            return;
        }
        byte size = (byte)bytes.Count();
        if (position + size > _buffer.Count())
        {
            resize(position + size);
        }
        bytes.CopyTo(_buffer, position);
        position += bytes.Count();
    }

    public void Write(short val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (bitOffset != 0)
        {
            WritePacked(bytes);
            return;
        }
        byte size = (byte)bytes.Count();
        if (position + size > _buffer.Count())
        {
            resize(position + size);
        }
        bytes.CopyTo(_buffer, position);
        position += bytes.Count();
    }

    public void Write(ushort val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (bitOffset != 0)
        {
            WritePacked(bytes);
            return;
        }
        byte size = (byte)bytes.Count();
        if (position + size > _buffer.Count())
        {
            resize(position + size);
        }
        bytes.CopyTo(_buffer, position);
        position += bytes.Count();
    }

    public void Write(float val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (bitOffset != 0)
        {
            WritePacked(bytes);
            return;
        }
        byte size = (byte)bytes.Count();
        if (position + size > _buffer.Count())
        {
            resize(position + size);
        }
        bytes.CopyTo(_buffer, position);
        position += bytes.Count();
    }

    public void Write(Vector2 val)
    {
        Write(val.X);
        Write(val.Y);
    }

    public void Write(Color val)
    {
        Write(val.R);
        Write(val.G);
        Write(val.B);
        Write(val.A);
    }

    public void WriteRGBColor(Color val)
    {
        Write(val.R);
        Write(val.G);
        Write(val.B);
    }

    public void Write(double val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (bitOffset != 0)
        {
            WritePacked(bytes);
            return;
        }
        byte size = (byte)bytes.Count();
        if (position + size > _buffer.Count())
        {
            resize(position + size);
        }
        bytes.CopyTo(_buffer, position);
        position += bytes.Count();
    }

    public void Write(char val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (bitOffset != 0)
        {
            WritePacked(bytes);
            return;
        }
        byte size = (byte)bytes.Count();
        if (position + size > _buffer.Count())
        {
            resize(position + size);
        }
        bytes.CopyTo(_buffer, position);
        position += bytes.Count();
    }

    public void Write(byte val)
    {
        if (bitOffset != 0)
        {
            WritePacked(val, 8);
            return;
        }
        if (position + 1 > _buffer.Count())
        {
            resize(position + 1);
        }
        _buffer[position] = val;
        position++;
    }

    public void Write(sbyte val)
    {
        if (bitOffset != 0)
        {
            WritePacked(val, 8);
            return;
        }
        if (position + 1 > _buffer.Count())
        {
            resize(position + 1);
        }
        _buffer[position] = (byte)val;
        position++;
    }

    public void Write(bool val)
    {
        if (_allowPacking)
        {
            WritePacked(val ? 1 : 0, 1);
        }
        else
        {
            Write((byte)(val ? 1u : 0u));
        }
    }

    public void WriteProfile(Profile pValue)
    {
        if (pValue == null)
        {
            Write((sbyte)(-1));
        }
        else
        {
            Write((sbyte)pValue.networkIndex);
        }
    }

    public Profile ReadProfile()
    {
        sbyte idx = ReadSByte();
        Profile p = null;
        if (idx >= 0 && idx < DuckNetwork.profiles.Count)
        {
            p = DuckNetwork.profiles[idx];
        }
        return p;
    }

    public void WriteTeam(Team pValue)
    {
        int teamSel = -1;
        if (pValue != null)
        {
            teamSel = Teams.IndexOf(pValue);
        }
        Write((ushort)teamSel);
    }

    public Team ReadTeam()
    {
        return Teams.ParseFromIndex(ReadUShort());
    }

    public void WriteObject(object obj)
    {
        int idx = 255;
        if (obj != null)
        {
            idx = ((!(obj is Thing)) ? kTypeIndexList.IndexOf(obj.GetType()) : kTypeIndexList.IndexOf(typeof(Thing)));
        }
        if (idx < 0)
        {
            throw new Exception("Trying to write unsupported type to BitBuffer through WriteObject!");
        }
        Write((byte)idx);
        Write(obj);
    }

    public object ReadObject(out Type pTypeRead)
    {
        byte typeByte = ReadByte();
        if (typeByte == byte.MaxValue || typeByte >= kTypeIndexList.Count)
        {
            pTypeRead = typeof(Thing);
            return null;
        }
        pTypeRead = kTypeIndexList[typeByte];
        return Read(pTypeRead);
    }

    public void Write(object obj)
    {
        if (obj is string)
        {
            Write((string)obj);
        }
        else if (obj is byte[])
        {
            Write((byte[])obj);
        }
        else if (obj is BitBuffer)
        {
            Write(obj as BitBuffer);
        }
        else if (obj is float)
        {
            Write((float)obj);
        }
        else if (obj is double)
        {
            Write((double)obj);
        }
        else if (obj is byte)
        {
            Write((byte)obj);
        }
        else if (obj is sbyte)
        {
            Write((sbyte)obj);
        }
        else if (obj is bool)
        {
            Write((bool)obj);
        }
        else if (obj is short)
        {
            Write((short)obj);
        }
        else if (obj is ushort)
        {
            Write((ushort)obj);
        }
        else if (obj is int)
        {
            Write((int)obj);
        }
        else if (obj is uint)
        {
            Write((uint)obj);
        }
        else if (obj is long)
        {
            Write((long)obj);
        }
        else if (obj is ulong)
        {
            Write((ulong)obj);
        }
        else if (obj is char)
        {
            Write((char)obj);
        }
        else if (obj is Vector2)
        {
            Write((Vector2)obj);
        }
        else if (obj is Color)
        {
            Write((Color)obj);
        }
        else if (obj is NetIndex16)
        {
            Write((ushort)(int)(NetIndex16)obj);
        }
        else if (obj is NetIndex2)
        {
            WritePacked((NetIndex2)obj, 2);
        }
        else if (obj is NetIndex4)
        {
            WritePacked((NetIndex4)obj, 4);
        }
        else if (obj is NetIndex8)
        {
            WritePacked((NetIndex8)obj, 8);
        }
        else if (obj is Thing)
        {
            if ((!(obj as Thing).isStateObject && (obj as Thing).specialSyncIndex == 0) || (obj as Thing).level == null)
            {
                if ((obj as Thing).level != null && MonoMain.modDebugging)
                {
                    DevConsole.Log(DCSection.NetCore, "@error |DGRED|!!BitBuffer.Write() - " + obj.GetType().Name + " is not a State Object (isStateObject == false), it has no StateBindings and cannot be written to a Bitbuffer.");
                    DevConsole.Log(DCSection.NetCore, "@error |DGRED|!!Are you sending a NetMessage with a non GhostObject member variable?");
                }
                Write((object)null);
                return;
            }
            Write((obj as Thing).level.networkIndex);
            if ((obj as Thing).isStateObject)
            {
                WritePacked(Editor.IDToType[(obj as Thing).GetType()], 10);
                GhostObject g = GhostManager.context.MakeGhostLater(obj as Thing);
                Write((ushort)(int)g.ghostObjectIndex);
                if (g.thing.connection == null)
                {
                    g.thing.connection = DuckNetwork.localConnection;
                }
            }
            else
            {
                WritePacked(0, 10);
                Write((obj as Thing).specialSyncIndex);
                GhostManager.context.MapSpecialSync(obj as Thing, (obj as Thing).specialSyncIndex);
            }
        }
        else
        {
            if (obj != null)
            {
                throw new Exception("Trying to write unsupported type " + obj.GetType()?.ToString() + " to BitBuffer!");
            }
            Write(DuckNetwork.levelIndex);
            WritePacked(0, 10);
            Write((ushort)0);
        }
    }

    public void WriteBits(object obj, int bits)
    {
        if (bits == -1)
        {
            Write(obj);
        }
        else
        {
            WritePacked(Convert.ToInt32(obj), bits);
        }
    }

    private void resize(int bytes)
    {
        int reqBytes;
        for (reqBytes = _buffer.Count() * 2; reqBytes < bytes; reqBytes *= 2)
        {
        }
        byte[] newBytes = new byte[reqBytes];
        _buffer.CopyTo(newBytes, 0);
        _buffer = newBytes;
    }

    public void Clear()
    {
        position = 0;
        _endPosition = 0;
        _bitOffsetPosition = 0;
        _bitEndOffset = 0;
        Array.Clear(_buffer, 0, _buffer.Length);
    }

    public void QuickClear()
    {
        position = 0;
        _endPosition = 0;
        _bitOffsetPosition = 0;
        _bitEndOffset = 0;
    }
}
