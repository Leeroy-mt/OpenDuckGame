using System;
using System.IO;

namespace DuckGame;

public class BinaryFile
{
	private Stream _stream;

	private byte[] _readShort = new byte[2];

	private byte[] _readInt = new byte[4];

	public BinaryFile(string name, BinaryFileMode m)
	{
		_stream = new FileStream(name, (FileMode)m);
	}

	public BinaryFile(byte[] data)
	{
		_stream = new MemoryStream(data);
	}

	public void Close()
	{
		_stream.Close();
	}

	public void SkipBytes(int bytes)
	{
		_stream.Seek(bytes, SeekOrigin.Current);
	}

	public void ResetPosition()
	{
		_stream.Seek(0L, SeekOrigin.Begin);
	}

	public byte ReadByte()
	{
		return (byte)_stream.ReadByte();
	}

	public byte[] ReadBytes(int num)
	{
		byte[] buff = new byte[num];
		_stream.Read(buff, 0, num);
		return buff;
	}

	public short ReadShort()
	{
		_stream.Read(_readShort, 0, 2);
		return BitConverter.ToInt16(_readShort, 0);
	}

	public int ReadInt()
	{
		_stream.Read(_readInt, 0, 4);
		return BitConverter.ToInt32(_readInt, 0);
	}

	public void WriteByte(byte b)
	{
		_stream.WriteByte(b);
	}

	public void WriteBytes(byte[] bytes, int length)
	{
		_stream.Write(bytes, 0, length);
	}

	public void WriteUShort(ushort b)
	{
		_readShort = BitConverter.GetBytes(b);
		byte[] readShort = _readShort;
		foreach (byte bt in readShort)
		{
			_stream.WriteByte(bt);
		}
	}

	public void WriteInt(int b)
	{
		_readInt = BitConverter.GetBytes(b);
		byte[] readInt = _readInt;
		foreach (byte bt in readInt)
		{
			_stream.WriteByte(bt);
		}
	}
}
