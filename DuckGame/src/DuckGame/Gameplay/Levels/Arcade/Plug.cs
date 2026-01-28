using System.Collections.Generic;

namespace DuckGame;

public class Plug
{
    public static Plug context = new Plug();

    public static List<VincentProduct> products = new List<VincentProduct>();

    public static float alpha = 0f;

    private static FancyBitmapFont _font;

    private static SpriteMap _dealer;

    private static List<string> _lines = new List<string>();

    private static string _currentLine = "";

    private static List<TextLine> _lineProgress = new List<TextLine>();

    private static float _waitLetter = 1f;

    private static float _waitAfterLine = 1f;

    private static float _talkMove = 0f;

    private static float _showLerp = 0f;

    private static bool _allowMovement = false;

    public static bool open = false;

    private static int frame = 0;

    private static bool killSkip = false;

    private static float _extraWait = 0f;

    private static int colorLetters = 0;

    private static float _chancyLerp = 0f;

    private static string lastWord = "";

    private static int wait = 0;

    public static void Clear()
    {
        _lines.Clear();
        _waitLetter = 0f;
        _waitAfterLine = 0f;
        _currentLine = "";
    }

    public static void Add(string line)
    {
        _lines.Add(line);
    }

    public static void Open()
    {
        _lineProgress.Clear();
        _showLerp = 0f;
        _allowMovement = false;
        _waitAfterLine = 1f;
        _waitLetter = 1f;
        open = true;
    }

    public static void Initialize()
    {
        if (_dealer == null)
        {
            _dealer = new SpriteMap("arcade/plug", 133, 160);
            _font = new FancyBitmapFont("smallFont");
        }
    }

    public static void Update()
    {
        Initialize();
        if (FurniShopScreen.open)
        {
            alpha = Lerp.Float(alpha, 1f, 0.05f);
        }
        else
        {
            alpha = Lerp.Float(alpha, 0f, 0.05f);
        }
        bool lerpChancy = true;
        _chancyLerp = Lerp.FloatSmooth(_chancyLerp, lerpChancy ? 1f : 0f, 0.2f, 1.05f);
        bool turbo = !_allowMovement && Input.Down("SELECT");
        if (_lines.Count > 0 && _currentLine == "")
        {
            bool num = _waitAfterLine <= 0f;
            _waitAfterLine -= 0.045f;
            if (turbo)
            {
                _waitAfterLine -= 0.045f;
            }
            if (killSkip)
            {
                _waitAfterLine -= 0.1f;
            }
            _talkMove += 0.75f;
            if (_talkMove > 1f)
            {
                frame = 0;
                _talkMove = 0f;
            }
            if (!num && _waitAfterLine <= 0f)
            {
                HUD.AddCornerMessage(HUDCorner.BottomRight, "@SELECT@CONTINUE");
            }
            if (_lineProgress.Count == 0 || Input.Pressed("SELECT"))
            {
                _lineProgress.Clear();
                _currentLine = _lines[0];
                colorLetters = 0;
                _lines.RemoveAt(0);
                _waitAfterLine = 1.3f;
                killSkip = false;
            }
        }
        if (_currentLine != "")
        {
            _waitLetter -= 0.8f;
            if (turbo)
            {
                _waitLetter -= 0.8f;
            }
            if (!(_waitLetter < 0f))
            {
                return;
            }
            _talkMove += 0.75f;
            if (_talkMove > 1f)
            {
                if (_currentLine[0] != ' ' && frame == 1 && _extraWait <= 0f)
                {
                    frame = 2;
                }
                else
                {
                    frame = 1;
                }
                _talkMove = 0f;
            }
            _waitLetter = 1f;
            while (_currentLine[0] == '@')
            {
                string val = _currentLine[0].ToString() ?? "";
                _currentLine = _currentLine.Remove(0, 1);
                while (_currentLine[0] != '@' && _currentLine.Length > 0)
                {
                    val += _currentLine[0];
                    _currentLine = _currentLine.Remove(0, 1);
                }
                _currentLine = _currentLine.Remove(0, 1);
                val += "@";
                _lineProgress[0].Add(val);
                _waitLetter = 3f;
                if (_currentLine.Length == 0)
                {
                    _currentLine = "";
                    return;
                }
            }
            float addWaitLetter = 0f;
            while (_currentLine[0] == '|')
            {
                _currentLine = _currentLine.Remove(0, 1);
                string read = "";
                int len = 0;
                while (_currentLine[0] != '|' && _currentLine.Length > 0)
                {
                    read += _currentLine[0];
                    _currentLine = _currentLine.Remove(0, 1);
                    len++;
                }
                bool end = false;
                if (_currentLine.Length <= 1)
                {
                    _currentLine = "";
                    end = true;
                }
                else
                {
                    _currentLine = _currentLine.Remove(0, 1);
                }
                _ = Color.White;
                bool foundColor = true;
                switch (read)
                {
                    case "0":
                        killSkip = true;
                        foundColor = false;
                        break;
                    case "1":
                        addWaitLetter = 5f;
                        foundColor = false;
                        break;
                    case "2":
                        addWaitLetter = 10f;
                        foundColor = false;
                        break;
                    case "3":
                        addWaitLetter = 15f;
                        foundColor = false;
                        break;
                }
                if (foundColor)
                {
                    colorLetters += len + 2;
                    _lineProgress[0].Add("|" + read + "|");
                }
                else if (end)
                {
                    return;
                }
            }
            string nextWord = "";
            int index = 1;
            if (_currentLine[0] == ' ')
            {
                while (index < _currentLine.Length && _currentLine[index] != ' ' && _currentLine[index] != '^')
                {
                    if (_currentLine[index] == '|')
                    {
                        colorLetters++;
                        index++;
                        while (index < _currentLine.Length && _currentLine[index] != '|')
                        {
                            index++;
                            colorLetters++;
                        }
                        index++;
                        colorLetters++;
                    }
                    else if (_currentLine[index] == '@')
                    {
                        for (index++; index < _currentLine.Length && _currentLine[index] != '@'; index++)
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
            if (_lineProgress.Count == 0 || _currentLine[0] == '^' || (_currentLine[0] == ' ' && _lineProgress[0].Length() + (nextWord.Length - colorLetters) > 44))
            {
                Color c = Color.White;
                if (_lineProgress.Count > 0)
                {
                    c = _lineProgress[0].lineColor;
                }
                _lineProgress.Insert(0, new TextLine
                {
                    lineColor = c
                });
                colorLetters = 0;
                if (_currentLine[0] == ' ' || _currentLine[0] == '^')
                {
                    _currentLine = _currentLine.Remove(0, 1);
                }
            }
            else
            {
                if (_currentLine[0] == '!' || _currentLine[0] == '?' || _currentLine[0] == '.')
                {
                    _waitLetter = 8f;
                }
                else if (_currentLine[0] == ',')
                {
                    _waitLetter = 14f;
                }
                _lineProgress[0].Add(_currentLine[0]);
                char c2 = _currentLine[0].ToString().ToLowerInvariant()[0];
                if (wait > 0)
                {
                    wait--;
                }
                if ((c2 < 'a' || c2 > 'z') && (c2 < '0' || c2 > '9') && c2 != '\'' && lastWord != "")
                {
                    CRC32.Generate(lastWord.Trim());
                    lastWord = "";
                }
                else
                {
                    lastWord += c2;
                }
                if (wait > 0)
                {
                    wait--;
                }
                else
                {
                    wait = 2;
                    SFX.Play("tinyTick", 0.4f, 0.2f);
                }
                _currentLine = _currentLine.Remove(0, 1);
            }
            _waitLetter += addWaitLetter;
        }
        else
        {
            _talkMove += 0.75f;
            if (_talkMove > 1f)
            {
                frame = 0;
                _talkMove = 0f;
            }
        }
    }

    public static void Draw()
    {
        Initialize();
        Vec2 dealerOffset = new Vec2(100f * (1f - _chancyLerp), 100f * (1f - _chancyLerp) - 4f);
        Vec2 descSize = new Vec2(280f, 30f);
        Vec2 descPos = new Vec2(20f, 132f) + dealerOffset;
        int index = 0;
        for (int i = _lineProgress.Count - 1; i >= 0; i--)
        {
            float wide = _font.GetWidth(_lineProgress[i].text);
            float ypos = descPos.Y + 2f + (float)(index * 9);
            float xpos = descPos.X + descSize.X / 2f - wide / 2f;
            for (int j = _lineProgress[i].segments.Count - 1; j >= 0; j--)
            {
                _font.Draw(_lineProgress[i].segments[j].text, new Vec2(xpos, ypos), _lineProgress[i].segments[j].color, 0.98f);
                xpos += (float)(_lineProgress[i].segments[j].text.Length * 8);
            }
            index++;
        }
        _dealer.Depth = 0.96f;
        _dealer.Alpha = 1f;
        Graphics.Draw(_dealer, 214f + dealerOffset.X, 6f + dealerOffset.Y);
        Graphics.DrawRect(descPos + new Vec2(-2f, 0f), descPos + descSize + new Vec2(2f, 0f), Color.Black, 0.97f);
    }
}
