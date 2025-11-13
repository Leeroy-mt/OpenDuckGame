using System;
using System.Collections.Generic;

namespace DuckGame;

public class InputCode
{
	public class InputCodeProfileStatus
	{
		public long lastUpdateFrame;

		public bool lastResult;

		public int currentIndex;

		public bool release;

		public float breakTimer = 1f;

		public void Break()
		{
			if (currentIndex > 0)
			{
				currentIndex = 0;
				release = false;
				breakTimer = 1f;
			}
		}

		public void Progress()
		{
			currentIndex++;
			release = false;
			breakTimer = 1f;
		}
	}

	public string name = "";

	public string description = "";

	public string chancyComment = "";

	public Action action;

	private Dictionary<string, InputCodeProfileStatus> status = new Dictionary<string, InputCodeProfileStatus>();

	public List<string> triggers = new List<string>();

	public float breakSpeed = 0.04f;

	private bool hasDoubleInputs;

	private bool _initializedDoubleInputs;

	private static Dictionary<string, InputCode> _codes = new Dictionary<string, InputCode>();

	public InputCodeProfileStatus GetStatus(InputProfile p)
	{
		InputCodeProfileStatus c = null;
		if (!status.TryGetValue(p.name, out c))
		{
			c = new InputCodeProfileStatus();
			status[p.name] = c;
		}
		return c;
	}

	public bool Update(InputProfile p)
	{
		if (p == null)
		{
			return false;
		}
		if (!_initializedDoubleInputs)
		{
			_initializedDoubleInputs = true;
			foreach (string trigger in triggers)
			{
				if (trigger.Contains("|"))
				{
					hasDoubleInputs = true;
					break;
				}
			}
		}
		InputCodeProfileStatus c = GetStatus(p);
		if (c.lastUpdateFrame == Graphics.frame)
		{
			return c.lastResult;
		}
		c.lastUpdateFrame = Graphics.frame;
		c.breakTimer -= breakSpeed;
		if (c.breakTimer <= 0f)
		{
			c.Break();
		}
		string nextInput = triggers[c.currentIndex];
		int reqMask = 0;
		if (hasDoubleInputs && nextInput.Contains("|"))
		{
			string[] array = nextInput.Split('|');
			foreach (string s in array)
			{
				reqMask |= 1 << Network.synchronizedTriggers.Count - Network.synchronizedTriggers.IndexOf(s);
			}
		}
		else
		{
			reqMask = 1 << Network.synchronizedTriggers.Count - Network.synchronizedTriggers.IndexOf(nextInput);
		}
		if (p.state == reqMask)
		{
			if (!c.release)
			{
				if (c.currentIndex == triggers.Count - 1)
				{
					c.Break();
					c.lastResult = true;
					return true;
				}
				c.release = true;
				if (c.currentIndex == 0)
				{
					c.breakTimer = 1f;
				}
			}
		}
		else if (p.state == 0)
		{
			if (c.release)
			{
				c.Progress();
			}
		}
		else
		{
			c.Break();
		}
		c.lastResult = false;
		return false;
	}

	public static implicit operator InputCode(string s)
	{
		InputCode c = null;
		if (!_codes.TryGetValue(s, out c))
		{
			c = new InputCode();
			c.triggers = new List<string>(s.Split('|'));
			_codes[s] = c;
		}
		return c;
	}
}
