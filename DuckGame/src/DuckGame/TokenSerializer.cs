using System;
using System.Collections.Generic;

namespace DuckGame;

public class TokenSerializer : IDisposable
{
	public const long kTokenizedIdentifier = 13826924961947138L;

	public static TokenSerializer instance;

	private TokenSerializer _prevInstance;

	public List<string> _tokens = new List<string>();

	public TokenSerializer()
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

	public BitBuffer Finish(BitBuffer pBuffer)
	{
		BitBuffer newData = new BitBuffer();
		newData.Write(13826924961947138L);
		newData.Write(_tokens.Count);
		instance = null;
		for (int i = 0; i < _tokens.Count; i++)
		{
			newData.Write(_tokens[i]);
		}
		newData.Write(pBuffer);
		instance = this;
		return newData;
	}
}
