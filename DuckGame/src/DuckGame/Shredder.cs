using System.Collections.Generic;

namespace DuckGame;

public class Shredder
{
	private static List<Note> _basicScale = new List<Note>
	{
		Note.c,
		Note.d,
		Note.f,
		Note.a
	};

	private static int _currentNote = 0;

	private static float _noteWait = 0f;

	public static int GetNextNote(int note, List<Note> scale)
	{
		for (int i = 0; i < scale.Count; i++)
		{
			if (scale[i] != (Note)(_currentNote % 12))
			{
				continue;
			}
			if (i < scale.Count - 1)
			{
				int num = 0;
				int req = (int)scale[i + 1];
				int cur = (int)scale[i];
				while (cur != req)
				{
					cur = (cur + 1) % 12;
					num++;
				}
				note += num;
			}
			else
			{
				int num2 = 0;
				int req2 = (int)scale[0];
				int cur2 = (int)scale[i];
				while (cur2 != req2)
				{
					cur2 = (cur2 + 1) % 12;
					num2++;
				}
				note += num2;
			}
		}
		return note % 48;
	}

	public static int GetPrevNote(int note, List<Note> scale)
	{
		for (int i = 0; i < scale.Count; i++)
		{
			if (scale[i] != (Note)(_currentNote % 12))
			{
				continue;
			}
			if (i > 0)
			{
				int num = 0;
				int req = (int)scale[i - 1];
				int cur = (int)scale[i];
				while (cur != req)
				{
					cur = (cur + 1) % 12;
					num++;
				}
				note -= 12 - num;
			}
			else
			{
				int num2 = 0;
				int req2 = (int)scale[scale.Count - 1];
				int cur2 = (int)scale[i];
				while (cur2 != req2)
				{
					cur2 = (cur2 + 1) % 12;
					num2++;
				}
				note -= 12 - num2;
			}
		}
		if (note < 0)
		{
			note = 48 + note;
		}
		return note % 48;
	}

	public static void Update()
	{
		XInputPad pad = Input.GetDevice<XInputPad>();
		if (_noteWait <= 0f)
		{
			if (pad.MapDown(16384))
			{
				_currentNote = GetNextNote(_currentNote, _basicScale);
				SFX.Play("guitar/guitar-" + Change.ToString(_currentNote));
				_noteWait = 1f;
			}
			if (pad.MapDown(32768))
			{
				_currentNote = (_currentNote = (int)_basicScale[0]);
				SFX.Play("guitar/guitar-" + Change.ToString(_currentNote));
				_noteWait = 1f;
			}
			if (pad.MapDown(8192))
			{
				_currentNote = GetPrevNote(_currentNote, _basicScale);
				SFX.Play("guitar/guitar-" + Change.ToString(_currentNote));
				_noteWait = 1f;
			}
		}
		_noteWait -= 0.15f;
	}
}
