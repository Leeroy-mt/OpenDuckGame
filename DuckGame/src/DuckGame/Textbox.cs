using System.Threading;
using System.Windows.Forms;

namespace DuckGame;

public class Textbox
{
	private FancyBitmapFont _font;

	public string text = "";

	public int _cursorPosition;

	private Vec2 _position;

	private Vec2 _size;

	protected bool _inFocus;

	private float _blink;

	public int _maxLines;

	private string _emptyText;

	public Depth depth;

	public int maxLength = 1000;

	private string _drawText = "";

	private Vec2 _cursorPos;

	private bool _highlightDrag;

	private string _clipboardText = "";

	public Vec2 position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
		}
	}

	public Vec2 size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
			_font.maxWidth = (int)value.x;
		}
	}

	public Textbox(float x, float y, float width, float height, float scale = 1f, int maxLines = int.MaxValue, string emptyText = "")
	{
		_font = new FancyBitmapFont("smallFont");
		_font.scale = new Vec2(scale);
		_font.maxWidth = (int)width;
		_position = new Vec2(x, y);
		_size = new Vec2(width, height);
		_maxLines = maxLines;
		_emptyText = emptyText;
	}

	private void ConstrainSelection()
	{
		if (_font._highlightEnd < 0)
		{
			_font._highlightEnd = 0;
		}
		if (_font._highlightStart < 0)
		{
			_font._highlightStart = 0;
		}
		if (_font._highlightEnd > text.Length)
		{
			_font._highlightEnd = text.Length;
		}
		if (_font._highlightStart > text.Length)
		{
			_font._highlightStart = text.Length;
		}
	}

	private void DeleteHighlight()
	{
		ConstrainSelection();
		if (_font._highlightStart < _font._highlightEnd)
		{
			text = text.Remove(_font._highlightStart, _font._highlightEnd - _font._highlightStart);
			_cursorPosition = _font._highlightStart;
			_font._highlightEnd = _cursorPosition;
		}
		else
		{
			text = text.Remove(_font._highlightEnd, _font._highlightStart - _font._highlightEnd);
			_cursorPosition = _font._highlightEnd;
			_font._highlightStart = _cursorPosition;
		}
	}

	public void ReadClipboardText()
	{
		_clipboardText = "";
		if (Clipboard.ContainsText())
		{
			_clipboardText = Clipboard.GetText();
		}
	}

	public void LoseFocus()
	{
		_inFocus = false;
		Editor.PopFocusNow();
	}

	public void GainFocus()
	{
		_inFocus = true;
		Keyboard.keyString = "";
		Editor.PushFocus(this);
	}

	public void Update()
	{
		bool hovered = false;
		if (Mouse.x > _position.x && Mouse.y > _position.y && Mouse.x < _position.x + _size.x && Mouse.y < _position.y + _size.y)
		{
			hovered = true;
			Editor.hoverTextBox = true;
			if (Mouse.left == InputState.Pressed)
			{
				if (Editor.PeekFocus() is Textbox)
				{
					(Editor.PeekFocus() as Textbox)._inFocus = false;
					Editor.PopFocusNow();
				}
				_inFocus = true;
				Keyboard.keyString = "";
				Editor.PushFocus(this);
			}
		}
		Vec2 textDrawPos = _position;
		if (_inFocus)
		{
			Input._imeAllowed = true;
			Keyboard.repeat = true;
			int prevLen = text.Length;
			if (Keyboard.Down(Keys.LeftControl) || Keyboard.Down(Keys.RightControl))
			{
				if (Keyboard.Pressed(Keys.V))
				{
					Thread thread = new Thread((ThreadStart)delegate
					{
						ReadClipboardText();
					});
					thread.SetApartmentState(ApartmentState.STA);
					thread.Start();
					thread.Join();
					if (_clipboardText != "")
					{
						if (_font._highlightStart != _font._highlightEnd)
						{
							DeleteHighlight();
						}
						text = text.Insert(_cursorPosition, _clipboardText);
						_cursorPosition += _clipboardText.Length;
					}
				}
				else if ((Keyboard.Pressed(Keys.C) || Keyboard.Pressed(Keys.X)) && _font._highlightStart != _font._highlightEnd)
				{
					string copyText = "";
					if (_font._highlightStart < _font._highlightEnd)
					{
						copyText = text.Substring(_font._highlightStart, _font._highlightEnd - _font._highlightStart);
					}
					else
					{
						copyText = text.Substring(_font._highlightEnd, _font._highlightStart - _font._highlightEnd);
					}
					if (copyText != "")
					{
						Thread thread2 = new Thread((ThreadStart)delegate
						{
							Clipboard.SetText(copyText);
						});
						thread2.SetApartmentState(ApartmentState.STA);
						thread2.Start();
						thread2.Join();
					}
					if (Keyboard.Pressed(Keys.X))
					{
						DeleteHighlight();
					}
				}
				Keyboard.keyString = "";
			}
			if (Keyboard.keyString.Length > 0 && _font._highlightStart != _font._highlightEnd)
			{
				DeleteHighlight();
			}
			text = text.Insert(_cursorPosition, Keyboard.keyString);
			if (Keyboard.Pressed(Keys.Back) && text.Length > 0)
			{
				if (_font._highlightStart != _font._highlightEnd)
				{
					DeleteHighlight();
				}
				else if (_cursorPosition > 0)
				{
					text = text.Remove(_cursorPosition - 1, 1);
					_cursorPosition--;
				}
			}
			if (Keyboard.Pressed(Keys.Delete) && text.Length > 0)
			{
				if (_font._highlightStart != _font._highlightEnd)
				{
					DeleteHighlight();
				}
				else if (_cursorPosition > 0 && _cursorPosition < text.Length)
				{
					text = text.Remove(_cursorPosition, 1);
				}
			}
			if (Keyboard.Pressed(Keys.Enter))
			{
				if (_font._highlightStart != _font._highlightEnd)
				{
					DeleteHighlight();
				}
				text = text.Insert(_cursorPosition, "\n");
				_cursorPosition++;
			}
			_ = text.Length;
			_cursorPosition += Keyboard.keyString.Length;
			Keyboard.keyString = "";
			if (Keyboard.Pressed(Keys.Left))
			{
				_cursorPosition--;
				_font._highlightStart = _cursorPosition;
				_font._highlightEnd = _cursorPosition;
				_blink = 0.5f;
			}
			if (Keyboard.Pressed(Keys.Right))
			{
				_cursorPosition++;
				_font._highlightStart = _cursorPosition;
				_font._highlightEnd = _cursorPosition;
				_blink = 0.5f;
			}
			if (Keyboard.Pressed(Keys.Up))
			{
				_cursorPosition = _font.GetCharacterIndex(_drawText, _cursorPos.x + 4f * _font.scale.x, _cursorPos.y - (float)_font.characterHeight * _font.scale.y);
				_font._highlightStart = _cursorPosition;
				_font._highlightEnd = _cursorPosition;
				_blink = 0.5f;
			}
			if (Keyboard.Pressed(Keys.Down))
			{
				_cursorPosition = _font.GetCharacterIndex(_drawText, _cursorPos.x + 4f * _font.scale.x, _cursorPos.y + (float)_font.characterHeight * _font.scale.y);
				_font._highlightStart = _cursorPosition;
				_font._highlightEnd = _cursorPosition;
				_blink = 0.5f;
			}
			ConstrainSelection();
			int idx = _font.GetCharacterIndex(text, 99999f, 99999f, _maxLines);
			text = text.Substring(0, idx);
		}
		else
		{
			_font._highlightStart = (_font._highlightEnd = 0);
		}
		_drawText = text;
		if (hovered && Mouse.left == InputState.Pressed)
		{
			int idx2 = (_cursorPosition = _font.GetCharacterIndex(_drawText, Mouse.x + 4f * _font.scale.x - textDrawPos.x, Mouse.y - textDrawPos.y));
			_font._highlightStart = idx2;
			_font._highlightEnd = idx2;
			_highlightDrag = true;
			_blink = 0.5f;
		}
		if (_highlightDrag)
		{
			int idx3 = _font.GetCharacterIndex(_drawText, Mouse.x + 4f * _font.scale.x - textDrawPos.x, Mouse.y - textDrawPos.y);
			_font._highlightEnd = idx3;
			_blink = 0.5f;
		}
		if (text.Length > maxLength)
		{
			text = text.Substring(0, maxLength);
		}
		ConstrainSelection();
		if (Mouse.left != InputState.Pressed && Mouse.left != InputState.Down)
		{
			_highlightDrag = false;
		}
		if (_cursorPosition > text.Length)
		{
			_cursorPosition = text.Length;
		}
		if (_cursorPosition < 0)
		{
			_cursorPosition = 0;
		}
		_cursorPos = _font.GetCharacterPosition(_drawText, _cursorPosition);
		_drawText = text;
		if (text.Length == 0 && !_inFocus)
		{
			_drawText = _emptyText;
		}
		_blink = (_blink + 0.02f) % 1f;
	}

	public void Draw()
	{
		_font.Draw(_drawText, _position, (text.Length == 0) ? (Colors.Silver * 0.8f) : Color.White, depth);
		if (_inFocus && _blink >= 0.5f)
		{
			Vec2 cursPos = _cursorPos;
			cursPos.x += 1f * _font.scale.x;
			Graphics.DrawLine(_position + cursPos, _position + cursPos + new Vec2(0f, 8f * _font.scale.y), Color.White, 0.5f, depth);
		}
	}
}
