using System;
using System.Collections.Generic;

namespace DuckGame;

public class TokenDeserializer : IDisposable
{
	public static TokenDeserializer instance;

	private TokenDeserializer _prevInstance;

	public List<string> _tokens = new List<string>();

	public TokenDeserializer()
	{
		if (instance != null)
		{
			_prevInstance = instance;
		}
		instance = this;
	}

	public void Dispose()
	{
		if (instance == this)
		{
			instance = _prevInstance;
		}
	}

	public int Token(string pString)
	{
		int val = _tokens.IndexOf(pString);
		if (val == -1)
		{
			_tokens.Add(pString);
			val = _tokens.Count - 1;
		}
		return val;
	}

	public BitBuffer Start(BitBuffer pBuffer)
	{
		if (pBuffer.ReadLong() != 13826924961947138L)
		{
			instance = _prevInstance;
			pBuffer.position = 0;
			return pBuffer;
		}
		instance = null;
		int count = pBuffer.ReadInt();
		for (int i = 0; i < count; i++)
		{
			_tokens.Add(pBuffer.ReadString());
		}
		BitBuffer result = pBuffer.ReadBitBuffer(allowPacking: false);
		instance = this;
		return result;
	}
}
