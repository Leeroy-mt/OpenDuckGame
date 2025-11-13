using System;
using System.Collections.Generic;

namespace DuckGame;

public class Teleprompter : Thing
{
	private BitmapFont _font;

	private List<DuckStory> _lines = new List<DuckStory>();

	private string _currentLine = "";

	private List<TextLine> _lineProgress = new List<TextLine>();

	private float _waitLetter = 1f;

	private float _waitAfterLine = 1f;

	private SpriteMap _newsCaster;

	private CasterMood _mood;

	private float _talkMove;

	private bool _paused;

	private SinWave _pitchSin = 0.15f;

	private bool _demoWait;

	private bool _tried;

	private Sound s;

	public bool finished
	{
		get
		{
			if (_lines.Count == 0)
			{
				return _currentLine == "";
			}
			return false;
		}
	}

	public Teleprompter(float xpos, float ypos, SpriteMap newsCaster)
		: base(xpos, ypos)
	{
		_newsCaster = newsCaster;
	}

	public void Pause()
	{
		_paused = true;
	}

	public void Resume()
	{
		_paused = false;
	}

	public void ReadLine(DuckStory line)
	{
		_lines.Add(line);
	}

	public void InsertLine(string line, int index)
	{
		_lines.Insert(index, new DuckStory
		{
			text = line
		});
	}

	public void ReadStory(DuckStory line)
	{
		_lines.Add(line);
	}

	public void SkipToClose()
	{
		_lines.RemoveAll((DuckStory x) => x.section < NewsSection.BeforeClosing);
		_lines.Insert(0, new DuckStory
		{
			section = NewsSection.DemoClosing,
			text = "|EXCITED|OK! We'll cut it short, But I'd like to talk |RED|Demo|WHITE|!*"
		});
	}

	public void ClearLines()
	{
		_currentLine = "";
		_lines.Clear();
	}

	public override void Initialize()
	{
		_font = new BitmapFont("biosFont", 8);
		base.layer = Layer.HUD;
		base.Initialize();
	}

	public void SetCasterFrame(int frame)
	{
		if (_mood == CasterMood.Excited)
		{
			_newsCaster.frame = frame + 3;
		}
		else if (_mood == CasterMood.Suave)
		{
			_newsCaster.frame = frame + 6;
		}
		else
		{
			_newsCaster.frame = frame;
		}
	}

	public int GetCasterFrame()
	{
		if (_mood == CasterMood.Excited)
		{
			return _newsCaster.frame - 3;
		}
		if (_mood == CasterMood.Suave)
		{
			return _newsCaster.frame - 6;
		}
		return _newsCaster.frame;
	}

	public override void Update()
	{
		if (_demoWait)
		{
			bool isDemo = true;
			bool cancel = false;
			if (!_tried)
			{
				if (Input.Pressed("MENU2"))
				{
					HUD.CloseAllCorners();
					SFX.Play("rockHitGround", 0.9f);
					isDemo = false;
					_tried = true;
				}
				if (Input.Pressed("SELECT"))
				{
					HUD.CloseAllCorners();
					cancel = true;
					_tried = true;
				}
			}
			if (!isDemo)
			{
				Main.isDemo = false;
				_lines.Clear();
				_lines.Add(new DuckStory
				{
					section = NewsSection.Closing,
					text = "|EXCITED|THANK YOU! You now have |GREEN|FULL Duck Game|WHITE|! You're the best!!!*"
				});
				_currentLine = "";
				_demoWait = false;
				HighlightLevel._cancelSkip = true;
			}
			else if (cancel)
			{
				_lines.Insert(0, new DuckStory
				{
					section = NewsSection.Closing,
					text = "Ah, alright then!"
				});
				_demoWait = false;
			}
		}
		else
		{
			if (_paused)
			{
				return;
			}
			if (_lines.Count > 0 && _currentLine == "")
			{
				_waitAfterLine -= 0.03f;
				_talkMove += 0.75f;
				if (_talkMove > 1f)
				{
					SetCasterFrame(0);
					_talkMove = 0f;
				}
				if (_waitAfterLine <= 0f)
				{
					_lineProgress.Clear();
					if (!_lines[0].text.StartsWith("CUE%"))
					{
						_currentLine = _lines[0].text;
						_waitAfterLine = 1.2f;
					}
					_lines[0].DoCallback();
					_lines.RemoveAt(0);
					_mood = CasterMood.Normal;
				}
			}
			if (_currentLine != "")
			{
				_waitLetter -= 0.5f;
				if (!(_waitLetter < 0f))
				{
					return;
				}
				_talkMove += 0.75f;
				if (_talkMove > 1f)
				{
					if (_currentLine[0] != ' ' && GetCasterFrame() == 0)
					{
						SetCasterFrame(Rando.Int(1) + 1);
					}
					else
					{
						SetCasterFrame(0);
					}
					_talkMove = 0f;
				}
				_waitLetter = 1f;
				while (_currentLine[0] == '|')
				{
					_currentLine = _currentLine.Remove(0, 1);
					string read = "";
					while (_currentLine[0] != '|' && _currentLine.Length > 0)
					{
						read += _currentLine[0];
						_currentLine = _currentLine.Remove(0, 1);
					}
					if (_currentLine.Length <= 1)
					{
						_currentLine = "";
						return;
					}
					_currentLine = _currentLine.Remove(0, 1);
					Color c = Color.White;
					bool foundColor = false;
					switch (read)
					{
					case "RED":
						foundColor = true;
						c = Color.Red;
						break;
					case "WHITE":
						foundColor = true;
						c = Color.White;
						break;
					case "BLUE":
						foundColor = true;
						c = Color.Blue;
						break;
					case "GREEN":
						foundColor = true;
						c = Color.LimeGreen;
						break;
					case "EXCITED":
						_mood = CasterMood.Excited;
						break;
					case "SUAVE":
						_mood = CasterMood.Suave;
						break;
					case "CALM":
						_mood = CasterMood.Normal;
						break;
					case "DEMOWAIT":
						HUD.CloseAllCorners();
						HUD.AddCornerControl(HUDCorner.BottomLeft, "PAY THE MAN@MENU2@");
						HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@NO!");
						_demoWait = true;
						return;
					}
					if (foundColor)
					{
						if (_lineProgress.Count == 0)
						{
							_lineProgress.Insert(0, new TextLine
							{
								lineColor = c
							});
						}
						else
						{
							_lineProgress[0].SwitchColor(c);
						}
					}
				}
				string nextWord = "";
				int index = 1;
				if (_currentLine[0] == ' ')
				{
					while (index < _currentLine.Length && _currentLine[index] != ' ')
					{
						if (_currentLine[index] == '|')
						{
							for (index++; index < _currentLine.Length && _currentLine[index] != '|'; index++)
							{
							}
							index++;
						}
						else
						{
							nextWord += _currentLine[index];
							index++;
						}
					}
				}
				if (_lineProgress.Count == 0 || (_currentLine[0] == ' ' && _lineProgress[0].Length() + nextWord.Length > 21))
				{
					Color c2 = Color.White;
					if (_lineProgress.Count > 0)
					{
						c2 = _lineProgress[0].lineColor;
					}
					_lineProgress.Insert(0, new TextLine
					{
						lineColor = c2
					});
					if (_currentLine[0] == ' ')
					{
						_currentLine = _currentLine.Remove(0, 1);
					}
					return;
				}
				if (_currentLine[0] == '!' || _currentLine[0] == '?' || _currentLine[0] == '.')
				{
					_waitLetter = 5f;
				}
				else if (_currentLine[0] == ',')
				{
					_waitLetter = 3f;
				}
				if (_currentLine[0] == '*')
				{
					_waitLetter = 5f;
				}
				else
				{
					_lineProgress[0].Add(_currentLine[0]);
					char c3 = _currentLine[0].ToString().ToLowerInvariant()[0];
					if ((c3 >= 'a' && c3 <= 'z') || (c3 >= '0' && c3 <= '9'))
					{
						s = SFX.Play("tinyNoise" + Convert.ToString(Rando.Int(1, 5)), 1f, Rando.Float(-0.8f, -0.4f));
					}
				}
				_currentLine = _currentLine.Remove(0, 1);
			}
			else
			{
				_talkMove += 0.75f;
				if (_talkMove > 1f)
				{
					SetCasterFrame(0);
					_talkMove = 0f;
				}
			}
		}
	}

	public override void Draw()
	{
		int index = 0;
		for (int i = _lineProgress.Count - 1; i >= 0; i--)
		{
			float wide = _font.GetWidth(_lineProgress[i].text);
			float ypos = 140 - (_lineProgress.Count - 1) * 9 + index * 9;
			Graphics.DrawRect(new Vec2(132f - wide / 2f - 1f, ypos - 1f), new Vec2(132f + wide / 2f, ypos + 9f), Color.Black, 0.84f);
			float xpos = 132f - wide / 2f;
			for (int j = _lineProgress[i].segments.Count - 1; j >= 0; j--)
			{
				_font.Draw(_lineProgress[i].segments[j].text, xpos, ypos, _lineProgress[i].segments[j].color, 0.85f);
				xpos += (float)(_lineProgress[i].segments[j].text.Length * 8);
			}
			index++;
		}
	}
}
