using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DuckGame;

public class MysteryTextbox
{
    private bool allowFocusStealing = true;

    public FancyBitmapFont _font;

    public string text = "";

    public int _cursorPosition;

    private Vec2 _position;

    private Vec2 _size;

    private float _blink;

    public int _maxLines;

    private string _emptyText;

    public Depth depth;

    public bool autoSizeVertically;

    public bool allowRightClick;

    public bool highlightKeywords;

    public int maxLength = 100000;

    public bool filename;

    private char[] invalidPathChars;

    private string _drawText = "";

    private Vec2 _cursorPos;

    private bool _highlightDrag;

    private string _clipboardText = "";

    private int prevMaxIndex;

    private string prevMaxString;

    public bool confirmed;

    public bool enterConfirms;

    private string _prevCalcString;

    public int numPages;

    public int currentPage;

    public Color color = Color.Black;

    public Color cursorColor = Color.Black;

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

    public string emptyText => _emptyText;

    public float textWidth => _font.GetWidth(_drawText);

    public MysteryTextbox(float x, float y, float width, float height, float scale = 1f, int maxLines = int.MaxValue, string emptyText = "", string font = "smallFont")
    {
        _font = new FancyBitmapFont(font);
        _font.scale = new Vec2(scale);
        _font.maxWidth = (int)width;
        _position = new Vec2(x, y);
        _size = new Vec2(width, height);
        _maxLines = maxLines;
        _emptyText = emptyText;
        Keyboard.keyString = "";
        invalidPathChars = Path.GetInvalidPathChars();
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

    public void Update(int page = 0, int rowsPerPage = -1, int firstPageRows = 0)
    {
        bool hovered = false;
        Vec2 mousePos = Mouse.position;
        if (mousePos.x > _position.x && mousePos.y > _position.y && mousePos.x < _position.x + _size.x && mousePos.y < _position.y + _size.y)
        {
            hovered = true;
            Editor.hoverTextBox = true;
            if (Mouse.left == InputState.Pressed)
            {
                Keyboard.keyString = "";
            }
        }
        allowFocusStealing = true;
        Vec2 textDrawPos = _position;
        Keyboard.repeat = true;
        Input._imeAllowed = true;
        int prevLen = text.Length;
        _ = text;
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
        if (_cursorPosition >= text.Length)
        {
            _cursorPosition = text.Length;
        }
        if (filename)
        {
            Keyboard.keyString = DuckFile.FixInvalidPath(Keyboard.keyString, pRemoveDirectoryCharacters: true);
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
            else if (_cursorPosition > -1 && _cursorPosition < text.Length)
            {
                text = text.Remove(_cursorPosition, 1);
            }
        }
        if (Keyboard.Pressed(Keys.Enter) || Input.Pressed("JUMP"))
        {
            if (enterConfirms)
            {
                confirmed = true;
            }
            else
            {
                if (_font._highlightStart != _font._highlightEnd)
                {
                    DeleteHighlight();
                }
                text = text.Insert(_cursorPosition, "\n");
                _cursorPosition++;
            }
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
        if (_cursorPosition > text.Length)
        {
            _cursorPosition = text.Length;
        }
        if (_cursorPosition < 0)
        {
            _cursorPosition = 0;
        }
        _drawText = text;
        if (hovered && Mouse.left == InputState.Pressed)
        {
            int idx = (_cursorPosition = _font.GetCharacterIndex(_drawText, mousePos.x + 4f * _font.scale.x - textDrawPos.x, mousePos.y - textDrawPos.y));
            _font._highlightStart = idx;
            _font._highlightEnd = idx;
            _highlightDrag = true;
            _blink = 0.5f;
        }
        if (_highlightDrag)
        {
            allowFocusStealing = false;
            int idx2 = _font.GetCharacterIndex(_drawText, mousePos.x + 4f * _font.scale.x - textDrawPos.x, mousePos.y - textDrawPos.y);
            _font._highlightEnd = idx2;
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
        _cursorPos = _font.GetCharacterPosition(_drawText, _cursorPosition);
        _drawText = text;
        _blink = (_blink + 0.02f) % 1f;
    }

    public void Draw(int page = 0, int rowsPerPage = -1, int firstPageRows = 0)
    {
        _font.Draw(_drawText, _position.x, _position.y, (text.Length == 0) ? (Colors.BlueGray * 0.8f) : color, depth);
        if (_blink >= 0.5f)
        {
            Vec2 cursPos = _cursorPos;
            cursPos.x += 1f * _font.scale.x;
            Graphics.DrawLine(_position + cursPos, _position + cursPos + new Vec2(0f, 8f * _font.scale.y), cursorColor, 0.5f, depth);
        }
    }
}
