using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DuckGame;

public class FancyBitmapFont : Transform
{
    protected RasterFont.Data _rasterData;

    private static Dictionary<string, List<Rectangle>> widthMap = new Dictionary<string, List<Rectangle>>();

    private SpriteMap swearSprites = new SpriteMap("lagturtle", 16, 16);

    protected Sprite _textureInternal;

    private static bool _mapInitialized = false;

    public static char[] _characters = new char[317]
    {
        ' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')',
        '*', '+', ',', '-', '.', '/', '0', '1', '2', '3',
        '4', '5', '6', '7', '8', '9', ':', ';', '>', '=',
        '<', '?', '@', 'A', 'B', 'C', 'D', 'E', 'F', 'G',
        'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q',
        'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '[',
        '\\', ']', '^', '_', '`', 'a', 'b', 'c', 'd', 'e',
        'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',
        'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y',
        'z', '{', '|', '}', '~', '`', 'À', 'Á', 'Â', 'Ã',
        'Ä', 'Å', 'Æ', 'Ç', 'È', 'É', 'Ê', 'Ë', 'Ì', 'Í',
        'Î', 'Ï', 'Ð', 'Ñ', 'Ò', 'Ó', 'Ô', 'Õ', 'Ö', 'Ø',
        'Ù', 'Ú', 'Û', 'Ü', 'Ý', 'Þ', 'ß', 'à', 'á', 'â',
        'ã', 'ä', 'å', 'æ', 'ç', 'è', 'é', 'ê', 'ë', 'ì',
        'í', 'î', 'ï', 'ð', 'ñ', 'ò', 'ó', 'ô', 'õ', 'ö',
        'ø', 'ù', 'ú', 'û', 'ü', 'ý', 'þ', 'ÿ', 'Ā', 'ā',
        'Ă', 'ă', 'Ą', 'ą', 'Ć', 'ć', 'Ċ', 'ċ', 'Č', 'č',
        'Ď', 'ď', 'Ē', 'ē', 'Ę', 'ę', 'Ě', 'ě', 'Ğ', 'ğ',
        'Ġ', 'ġ', 'Ģ', 'ģ', 'Ħ', 'ħ', 'Ī', 'ī', 'Į', 'į',
        'İ', 'ı', 'Ĳ', 'ĳ', 'Ķ', 'ķ', 'Ĺ', 'ĺ', 'Ļ', 'ļ',
        'Ľ', 'ľ', 'Ł', 'ł', 'Ń', 'ń', 'Ņ', 'ņ', 'Ň', 'ň',
        'Ō', 'ō', 'Ő', 'ő', 'Œ', 'œ', 'Ŕ', 'ŕ', 'Ř', 'ř',
        'Ś', 'ś', 'Ş', 'ş', 'Š', 'š', 'Ţ', 'ţ', 'Ť', 'ť',
        'Ū', 'ū', 'Ů', 'ů', 'Ű', 'ű', 'Ų', 'ų', 'Ÿ', 'Ź',
        'ź', 'Ż', 'ż', 'Ž', 'ž', 'ǅ', 'ǆ', 'ǲ', 'ǳ', 'А',
        'Б', 'В', 'Г', 'Д', 'Е', 'Ё', 'Ж', 'З', 'И', 'Й',
        'К', 'Л', 'М', 'Н', 'О', 'П', 'Р', 'С', 'Т', 'У',
        'Ф', 'Х', 'Ц', 'Ч', 'Ш', 'Щ', 'Ъ', 'Ы', 'Ь', 'Э',
        'Ю', 'Я', 'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж',
        'з', 'и', 'й', 'к', 'л', 'м', 'н', 'о', 'п', 'р',
        'с', 'т', 'у', 'ф', 'х', 'ц', 'ч', 'ш', 'щ', 'ъ',
        'ы', 'ь', 'э', 'ю', 'я', '¡', '¿'
    };

    public static SpriteMap _kanjiSprite;

    public static ushort[] _kanjiMap;

    private static int[] _characterMap = new int[65535];

    private const int kTilesPerRow = 16;

    private InputProfile _inputProfile;

    private int _maxWidth;

    protected List<Rectangle> _widths;

    protected List<BitmapFont_CharacterInfo> _characterInfos;

    protected int _charHeight;

    private int _firstYPixel;

    public bool chatFont;

    public NetworkConnection _currentConnection;

    private int _letterIndex;

    private bool _drawingOutline;

    public float symbolYOffset;

    public float lineGap;

    public int _highlightStart = -1;

    public int _highlightEnd = -1;

    private Color _previousColor;

    public bool singleLine;

    public bool enforceWidthByWord = true;

    public virtual Sprite _texture
    {
        get
        {
            return _textureInternal;
        }
        set
        {
            _textureInternal = value;
        }
    }

    public float height => (float)_texture.height * base.Scale.Y;

    public InputProfile inputProfile
    {
        get
        {
            return _inputProfile;
        }
        set
        {
            _inputProfile = value;
        }
    }

    public int maxWidth
    {
        get
        {
            return _maxWidth;
        }
        set
        {
            _maxWidth = value;
        }
    }

    public int maxRows { get; set; }

    public int characterHeight => _charHeight;

    public static void InitializeKanjis()
    {
        if (_kanjiSprite == null)
        {
            _kanjiSprite = new SpriteMap("kanji_font", 8, 8);
            string kanjiMap = DuckFile.ReadAllText(DuckFile.contentDirectory + "kanji_map.txt");
            _kanjiMap = new ushort[65535];
            for (int i = 0; i < kanjiMap.Length; i++)
            {
                _kanjiMap[(uint)kanjiMap[i]] = (ushort)i;
            }
        }
    }

    public FancyBitmapFont()
    {
    }

    public FancyBitmapFont(string image)
    {
        Construct(image);
    }

    protected void Construct(string image)
    {
        InitializeKanjis();
        _texture = new Sprite(image);
        if (!widthMap.TryGetValue(image, out _widths))
        {
            _widths = new List<Rectangle>();
            Color[] data = _texture.texture.GetData();
            bool done = false;
            int leftPixel = -1;
            for (int ypixel = 1; ypixel < _texture.height; ypixel += _charHeight + 1)
            {
                for (int xpixel = 0; xpixel < _texture.width; xpixel++)
                {
                    if (data[xpixel + ypixel * _texture.width].r == 0 && data[xpixel + ypixel * _texture.width].g == 0 && data[xpixel + ypixel * _texture.width].b == 0 && data[xpixel + ypixel * _texture.width].a == 0)
                    {
                        if (leftPixel == -1)
                        {
                            leftPixel = xpixel;
                        }
                    }
                    else
                    {
                        if (leftPixel == -1)
                        {
                            continue;
                        }
                        if (_charHeight == 0)
                        {
                            _firstYPixel = ypixel;
                            xpixel--;
                            for (int yheight = ypixel + 1; yheight < _texture.height; yheight++)
                            {
                                if (data[xpixel + yheight * _texture.width].r != 0 || data[xpixel + yheight * _texture.width].g != 0 || data[xpixel + yheight * _texture.width].b != 0 || data[xpixel + yheight * _texture.width].a != 0)
                                {
                                    _charHeight = yheight - ypixel;
                                    break;
                                }
                            }
                            xpixel++;
                        }
                        _widths.Add(new Rectangle(leftPixel, ypixel, xpixel - leftPixel, _charHeight));
                        leftPixel = -1;
                    }
                }
                if (done)
                {
                    break;
                }
            }
        }
        widthMap[image] = _widths;
        if (_widths.Count > 0)
        {
            _charHeight = (int)_widths[0].height;
        }
        if (_mapInitialized)
        {
            return;
        }
        for (int i = 0; i < 65535; i++)
        {
            char c = (char)i;
            _characterMap[i] = 91;
            for (int iChar = 0; iChar < _characters.Length; iChar++)
            {
                if (_characters[iChar] == c)
                {
                    _characterMap[i] = iChar;
                    break;
                }
            }
        }
        _mapInitialized = true;
    }

    public Sprite ParseSprite(string text, InputProfile input)
    {
        if (text.StartsWith("_!"))
        {
            return null;
        }
        _letterIndex++;
        string trigger = "";
        bool brokeWithAt = false;
        while (_letterIndex != text.Length)
        {
            if (text[_letterIndex] == '@' || (chatFont && text[_letterIndex] == ':'))
            {
                brokeWithAt = true;
                break;
            }
            if (text[_letterIndex] == ' ' || text[_letterIndex] == '\n')
            {
                _letterIndex--;
                break;
            }
            trigger += text[_letterIndex];
            _letterIndex++;
        }
        if (chatFont && !brokeWithAt)
        {
            return null;
        }
        Sprite spr = null;
        if (input != null)
        {
            spr = input.GetTriggerImage(trigger);
        }
        if (spr == null)
        {
            spr = Input.GetTriggerSprite(trigger);
        }
        if (spr == null && Options.Data.mojiFilter != 0)
        {
            spr = DuckFile.GetMoji(trigger, _currentConnection);
            if (spr == null && trigger.Contains("!"))
            {
                return Input.GetTriggerSprite("blankface");
            }
        }
        return spr;
    }

    public Color ParseColor(string text)
    {
        _letterIndex++;
        string read = "";
        while (_letterIndex != text.Length && text[_letterIndex] != ' ' && text[_letterIndex] != '|')
        {
            read += text[_letterIndex];
            _letterIndex++;
        }
        if (read == "PREV")
        {
            return new Color(_previousColor.r, _previousColor.g, _previousColor.b);
        }
        return Colors.ParseColor(read);
    }

    public InputProfile GetInputProfile(InputProfile input)
    {
        if (input == null)
        {
            input = ((_inputProfile != null) ? _inputProfile : InputProfile.FirstProfileWithDevice);
        }
        return input;
    }

    public string FormatWithNewlines(string pText, float maxWidth, bool thinButtons = false)
    {
        float wide = 0f;
        float widest = 0f;
        char[] chars = pText.ToCharArray();
        for (_letterIndex = 0; _letterIndex < chars.Length; _letterIndex++)
        {
            bool processedSpecialCharacter = false;
            if (chars[_letterIndex] == ' ' && wide > maxWidth)
            {
                chars[_letterIndex] = '\n';
            }
            if (chars[_letterIndex] == '@' || (chatFont && chars[_letterIndex] == ':'))
            {
                pText = new string(chars);
                int iPos = _letterIndex;
                Sprite spr = ParseSprite(pText, null);
                if (spr != null)
                {
                    wide += (thinButtons ? 6f : ((float)spr.width * spr.Scale.X + 1f));
                    processedSpecialCharacter = true;
                }
                else
                {
                    _letterIndex = iPos;
                }
            }
            else if (chars[_letterIndex] == '|')
            {
                pText = new string(chars);
                int iPos2 = _letterIndex;
                if (ParseColor(pText) != Colors.Transparent)
                {
                    processedSpecialCharacter = true;
                }
                else
                {
                    _letterIndex = iPos2;
                }
            }
            else if (chars[_letterIndex] == '\n')
            {
                if (wide > widest)
                {
                    widest = wide;
                }
                wide = 0f;
            }
            if (!processedSpecialCharacter)
            {
                char charVal = chars[_letterIndex];
                if (charVal >= 'ぁ')
                {
                    wide += 8f * base.Scale.X;
                }
                else
                {
                    int charIndex = _characterMap[(uint)charVal];
                    if (_characterInfos != null)
                    {
                        if (charIndex < _characterInfos.Count)
                        {
                            wide += (_characterInfos[charIndex].width + _characterInfos[charIndex].trailing + _characterInfos[charIndex].leading) * base.Scale.X;
                        }
                    }
                    else if (charIndex < _widths.Count)
                    {
                        wide += (_widths[charIndex].width - 1f) * base.Scale.X;
                    }
                }
            }
        }
        if (wide > widest)
        {
            widest = wide;
        }
        return new string(chars);
    }

    public float GetWidth(string text, bool thinButtons = false)
    {
        float wide = 0f;
        float widest = 0f;
        for (_letterIndex = 0; _letterIndex < text.Length; _letterIndex++)
        {
            bool processedSpecialCharacter = false;
            if (text[_letterIndex] == '@' || (chatFont && text[_letterIndex] == ':'))
            {
                int iPos = _letterIndex;
                Sprite spr = ParseSprite(text, null);
                if (spr != null)
                {
                    if (chatFont)
                    {
                        Vector2 sprScale = spr.Scale;
                        spr.Scale *= base.Scale.X / 2f;
                        if (this is RasterFont)
                        {
                            float scaleFac = (this as RasterFont).data.fontSize * RasterFont.fontScaleFactor / 10f;
                            spr.Scale *= scaleFac;
                            spr.Scale = new Vector2((float)Math.Round(spr.Scale.X * 2f) / 2f);
                        }
                        wide += (float)spr.width * spr.Scale.X + 1f;
                        spr.Scale = sprScale;
                    }
                    else
                    {
                        wide += (thinButtons ? 6f : ((float)spr.width * spr.Scale.X + 1f));
                    }
                    processedSpecialCharacter = true;
                }
                else
                {
                    _letterIndex = iPos;
                }
            }
            else if (text[_letterIndex] == '|')
            {
                int iPos2 = _letterIndex;
                if (ParseColor(text) != Colors.Transparent)
                {
                    processedSpecialCharacter = true;
                }
                else
                {
                    _letterIndex = iPos2;
                }
            }
            else if (text[_letterIndex] == '\n')
            {
                if (wide > widest)
                {
                    widest = wide;
                }
                wide = 0f;
                processedSpecialCharacter = true;
            }
            if (!processedSpecialCharacter)
            {
                char charVal = text[_letterIndex];
                if (charVal >= 'ぁ')
                {
                    wide += 8f * base.Scale.X;
                }
                else
                {
                    int charIndex = _characterMap[(uint)charVal];
                    if (charIndex >= _widths.Count)
                    {
                        charIndex = _widths.Count - 1;
                    }
                    if (charIndex < 0)
                    {
                        return widest;
                    }
                    if (_characterInfos != null)
                    {
                        if (charIndex < _characterInfos.Count)
                        {
                            wide += (_characterInfos[charIndex].width + _characterInfos[charIndex].trailing + _characterInfos[charIndex].leading) * base.Scale.X;
                        }
                    }
                    else
                    {
                        wide += (_widths[charIndex].width - 1f) * base.Scale.X;
                    }
                }
            }
        }
        if (wide > widest)
        {
            widest = wide;
        }
        return widest;
    }

    public int GetCharacterIndex(string text, float xPosition, float yPosition, int maxRows = int.MaxValue, bool thinButtons = false)
    {
        float wide = 0f;
        float high = 0f;
        int numHigh = 0;
        float widest = 0f;
        for (_letterIndex = 0; _letterIndex < text.Length; _letterIndex++)
        {
            if ((wide >= xPosition && yPosition < high + (float)_charHeight * base.Scale.Y) || numHigh >= maxRows)
            {
                return _letterIndex - 1;
            }
            bool processedSpecialCharacter = false;
            if (text[_letterIndex] == '@' || (chatFont && text[_letterIndex] == ':'))
            {
                int iPos = _letterIndex;
                Sprite spr = ParseSprite(text, null);
                if (spr != null)
                {
                    wide += (thinButtons ? 6f : ((float)spr.width * spr.Scale.X + 1f));
                    processedSpecialCharacter = true;
                }
                else
                {
                    _letterIndex = iPos;
                }
            }
            else if (text[_letterIndex] == '|')
            {
                int iPos2 = _letterIndex;
                if (ParseColor(text) != Colors.Transparent)
                {
                    processedSpecialCharacter = true;
                }
                else
                {
                    _letterIndex = iPos2;
                }
            }
            else if (text[_letterIndex] == '\n')
            {
                if (wide > widest)
                {
                    widest = wide;
                }
                wide = 0f;
                numHigh++;
                high += (float)_charHeight * base.Scale.Y;
                processedSpecialCharacter = true;
                if (numHigh >= maxRows)
                {
                    return _letterIndex;
                }
            }
            if (!processedSpecialCharacter)
            {
                bool skipped = false;
                if (maxWidth > 0)
                {
                    if (text[_letterIndex] == ' ' || text[_letterIndex] == '|' || text[_letterIndex] == '@')
                    {
                        int idxxx = _letterIndex + 1;
                        float additionalWidth = 0f;
                        for (; idxxx < text.Count() && text[idxxx] != ' ' && text[idxxx] != '|' && text[idxxx] != '@'; idxxx++)
                        {
                            char charVal = (char)Maths.Clamp(text[idxxx], 0, _characterMap.Length - 1);
                            int cIndex = _characterMap[(uint)charVal];
                            additionalWidth = ((_characterInfos == null) ? (additionalWidth + (_widths[cIndex].width - 1f) * base.Scale.X) : (additionalWidth + (_characterInfos[cIndex].width + _characterInfos[cIndex].trailing + _characterInfos[cIndex].leading) * base.Scale.X));
                        }
                        if (wide + additionalWidth > (float)maxWidth)
                        {
                            numHigh++;
                            high += (float)_charHeight * base.Scale.Y;
                            wide = 0f;
                            skipped = true;
                            if (numHigh >= maxRows)
                            {
                                return _letterIndex;
                            }
                        }
                    }
                    else
                    {
                        char charVal2 = (char)Maths.Clamp(text[_letterIndex], 0, _characterMap.Length - 1);
                        int cIndex2 = _characterMap[(uint)charVal2];
                        if (_characterInfos != null)
                        {
                            float addWide = (_characterInfos[cIndex2].width + _characterInfos[cIndex2].trailing + _characterInfos[cIndex2].leading) * base.Scale.X;
                            if (wide + addWide * base.Scale.X > (float)maxWidth)
                            {
                                numHigh++;
                                high += (float)_charHeight * base.Scale.Y;
                                wide = 0f;
                                if (numHigh >= maxRows)
                                {
                                    return _letterIndex;
                                }
                            }
                        }
                        else if (wide + _widths[cIndex2].width * base.Scale.X > (float)maxWidth)
                        {
                            numHigh++;
                            high += (float)_charHeight * base.Scale.Y;
                            wide = 0f;
                            if (numHigh >= maxRows)
                            {
                                return _letterIndex;
                            }
                        }
                    }
                }
                if (!skipped)
                {
                    if (text[_letterIndex] >= 'ぁ')
                    {
                        wide += 8f * base.Scale.X;
                    }
                    else
                    {
                        char charVal3 = (char)Maths.Clamp(text[_letterIndex], 0, _characterMap.Length - 1);
                        int charIndex = _characterMap[(uint)charVal3];
                        if (_characterInfos != null)
                        {
                            if (charIndex >= _characterInfos.Count)
                            {
                                continue;
                            }
                            wide += (_characterInfos[charIndex].width + _characterInfos[charIndex].trailing + _characterInfos[charIndex].leading) * base.Scale.X;
                        }
                        else
                        {
                            wide += (_widths[charIndex].width - 1f) * base.Scale.X;
                        }
                    }
                }
            }
            if (high > yPosition)
            {
                return _letterIndex;
            }
        }
        return _letterIndex;
    }

    public Vector2 GetCharacterPosition(string text, int index, bool thinButtons = false)
    {
        float wide = 0f;
        float high = 0f;
        float widest = 0f;
        for (_letterIndex = 0; _letterIndex < text.Length; _letterIndex++)
        {
            if (_letterIndex >= index)
            {
                return new Vector2(wide, high);
            }
            bool processedSpecialCharacter = false;
            if (text[_letterIndex] == '@' || (chatFont && text[_letterIndex] == ':'))
            {
                int iPos = _letterIndex;
                Sprite spr = ParseSprite(text, null);
                if (spr != null)
                {
                    wide += (thinButtons ? 6f : ((float)spr.width * spr.Scale.X + 1f));
                    processedSpecialCharacter = true;
                }
                else
                {
                    _letterIndex = iPos;
                }
            }
            else if (text[_letterIndex] == '|')
            {
                int iPos2 = _letterIndex;
                if (ParseColor(text) != Colors.Transparent)
                {
                    processedSpecialCharacter = true;
                }
                else
                {
                    _letterIndex = iPos2;
                }
            }
            else if (text[_letterIndex] == '\n')
            {
                if (wide > widest)
                {
                    widest = wide;
                }
                wide = 0f;
                high += (float)_charHeight * base.Scale.Y;
                processedSpecialCharacter = true;
            }
            if (!processedSpecialCharacter)
            {
                bool skipped = false;
                if (maxWidth > 0)
                {
                    if (text[_letterIndex] == ' ' || text[_letterIndex] == '|' || text[_letterIndex] == '@')
                    {
                        int idxxx = _letterIndex + 1;
                        float additionalWidth = 0f;
                        for (; idxxx < text.Count() && text[idxxx] != ' ' && text[idxxx] != '|' && text[idxxx] != '@'; idxxx++)
                        {
                            char charVal = (char)Maths.Clamp(text[idxxx], 0, _characterMap.Length - 1);
                            int cIndex = _characterMap[(uint)charVal];
                            additionalWidth = ((_characterInfos == null) ? (additionalWidth + (_widths[cIndex].width - 1f) * base.Scale.X) : (additionalWidth + (_characterInfos[cIndex].width + _characterInfos[cIndex].trailing + _characterInfos[cIndex].leading) * base.Scale.X));
                        }
                        if (wide + additionalWidth > (float)maxWidth)
                        {
                            high += (float)_charHeight * base.Scale.Y;
                            wide = 0f;
                            skipped = true;
                        }
                    }
                    else
                    {
                        char charVal2 = (char)Maths.Clamp(text[_letterIndex], 0, _characterMap.Length - 1);
                        int cIndex2 = _characterMap[(uint)charVal2];
                        if (wide + _widths[cIndex2].width * base.Scale.X > (float)maxWidth)
                        {
                            high += (float)_charHeight * base.Scale.Y;
                            wide = 0f;
                        }
                    }
                }
                if (!skipped)
                {
                    if (text[_letterIndex] >= 'ぁ')
                    {
                        wide += 8f * base.Scale.X;
                    }
                    else
                    {
                        char charVal3 = (char)Maths.Clamp(text[_letterIndex], 0, _characterMap.Length - 1);
                        int charIndex = _characterMap[(uint)charVal3];
                        if (_characterInfos != null)
                        {
                            if (charIndex < _characterInfos.Count)
                            {
                                wide += (_characterInfos[charIndex].width + _characterInfos[charIndex].trailing + _characterInfos[charIndex].leading) * base.Scale.X;
                            }
                        }
                        else
                        {
                            wide += (_widths[charIndex].width - 1f) * base.Scale.X;
                        }
                    }
                }
            }
        }
        return new Vector2(wide, high);
    }

    public void DrawOutline(string text, Vector2 pos, Color c, Color outline, Depth deep = default(Depth), float outlineThickness = 1f)
    {
        _drawingOutline = true;
        Draw(text, pos + new Vector2(0f - outlineThickness, 0f), outline, deep + 2, colorSymbols: true);
        Draw(text, pos + new Vector2(outlineThickness, 0f), outline, deep + 2, colorSymbols: true);
        Draw(text, pos + new Vector2(0f, 0f - outlineThickness), outline, deep + 2, colorSymbols: true);
        Draw(text, pos + new Vector2(0f, outlineThickness), outline, deep + 2, colorSymbols: true);
        Draw(text, pos + new Vector2(0f - outlineThickness, 0f - outlineThickness), outline, deep + 2, colorSymbols: true);
        Draw(text, pos + new Vector2(outlineThickness, 0f - outlineThickness), outline, deep + 2, colorSymbols: true);
        Draw(text, pos + new Vector2(0f - outlineThickness, outlineThickness), outline, deep + 2, colorSymbols: true);
        Draw(text, pos + new Vector2(outlineThickness, outlineThickness), outline, deep + 2, colorSymbols: true);
        _drawingOutline = false;
        Draw(text, pos, c, deep + 5);
    }

    public void Draw(string text, Vector2 pos, Color c, Depth deep = default(Depth), bool colorSymbols = false)
    {
        Draw(text, pos.X, pos.Y, c, deep, colorSymbols);
    }

    public void Draw(string text, float xpos, float ypos, Color c, Depth deep = default(Depth), bool colorSymbols = false)
    {
        _previousColor = c;
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        Color highlight = new Color(255 - c.r, 255 - c.g, 255 - c.b);
        float yOff = 0f;
        float xOff = 0f;
        int curRow = 0;
        for (_letterIndex = 0; _letterIndex < text.Length; _letterIndex++)
        {
            bool processedSpecialCharacter = false;
            if (text[_letterIndex] == '@' || (chatFont && text[_letterIndex] == ':'))
            {
                int iPos = _letterIndex;
                Sprite spr = ParseSprite(text, null);
                if (spr != null)
                {
                    float al = spr.Alpha;
                    spr.Alpha = base.Alpha * c.ToVector4().w;
                    if (spr != null)
                    {
                        float yCenter = characterHeight / 2 - spr.height / 2;
                        yCenter += symbolYOffset;
                        if (colorSymbols)
                        {
                            spr.color = c;
                        }
                        if (chatFont)
                        {
                            Vector2 sprScale = spr.Scale;
                            spr.Scale *= base.Scale.X / 2f;
                            if (this is RasterFont)
                            {
                                float scaleFac = (this as RasterFont).data.fontSize * RasterFont.fontScaleFactor / 10f;
                                spr.Scale *= scaleFac;
                                spr.Scale = new Vector2((float)Math.Round(spr.Scale.X * 2f) / 2f);
                            }
                            yCenter = (float)characterHeight * base.Scale.Y / 2f - (float)spr.height * spr.Scale.Y / 2f;
                            Graphics.Draw(spr, xpos + xOff, ypos + yOff + yCenter, deep + 10 + (int)((ypos + yOff) / 10f));
                            xOff += (float)spr.width * spr.Scale.X + 1f;
                            spr.Scale = sprScale;
                        }
                        else if (_rasterData != null)
                        {
                            Vector2 sprScale2 = spr.Scale;
                            float sizeDif = _rasterData.fontHeight / 24f;
                            spr.Scale *= sizeDif;
                            Graphics.Draw(spr, xpos + xOff, ypos + yOff + 1f * sizeDif, deep);
                            xOff += (float)spr.width * spr.Scale.X + 1f;
                            spr.Scale = sprScale2;
                        }
                        else
                        {
                            Graphics.Draw(spr, xpos + xOff, ypos + yOff + yCenter, deep);
                            xOff += (float)spr.width * spr.Scale.X + 1f;
                        }
                        spr.color = Color.White;
                    }
                    spr.Alpha = al;
                    processedSpecialCharacter = true;
                }
                else
                {
                    _letterIndex = iPos;
                }
            }
            else if (text[_letterIndex] == '|')
            {
                int iPos2 = _letterIndex;
                Color col = ParseColor(text);
                if (col != Colors.Transparent)
                {
                    _previousColor = c;
                    if (!_drawingOutline)
                    {
                        float al2 = c.ToVector4().w;
                        c = col;
                        c *= al2;
                    }
                    processedSpecialCharacter = true;
                }
                else
                {
                    _letterIndex = iPos2;
                }
            }
            if (!processedSpecialCharacter)
            {
                bool skippedLine = false;
                if (maxWidth > 0)
                {
                    if (text[_letterIndex] == ' ' || text[_letterIndex] == '|' || text[_letterIndex] == '@')
                    {
                        int index = _letterIndex + 1;
                        if (enforceWidthByWord)
                        {
                            char sp = ' ';
                            float additionalWidth = _widths[_characterMap[(byte)sp]].width;
                            for (; index < text.Count() && text[index] != ' ' && text[index] != '|' && text[index] != '@'; index++)
                            {
                                byte charVal = (byte)Maths.Clamp(text[index], 0, 254);
                                int cIndex = _characterMap[charVal];
                                additionalWidth += (_widths[cIndex].width - 1f) * base.Scale.X;
                            }
                            if (xOff + additionalWidth > (float)maxWidth)
                            {
                                yOff += (float)_charHeight * base.Scale.Y;
                                xOff = 0f;
                                curRow++;
                                skippedLine = true;
                                if (singleLine)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        byte charVal2 = (byte)Maths.Clamp(text[_letterIndex], 0, 254);
                        int cIndex2 = _characterMap[charVal2];
                        if (xOff + _widths[cIndex2].width * base.Scale.X > (float)maxWidth)
                        {
                            yOff += (float)_charHeight * base.Scale.Y;
                            xOff = 0f;
                            curRow++;
                            if (singleLine)
                            {
                                break;
                            }
                        }
                    }
                }
                if (maxRows != 0 && curRow >= maxRows)
                {
                    break;
                }
                if (!skippedLine)
                {
                    if (text[_letterIndex] == '\n')
                    {
                        yOff += ((float)_charHeight + lineGap) * base.Scale.Y;
                        xOff = 0f;
                        curRow++;
                    }
                    else
                    {
                        char charVal3 = text[_letterIndex];
                        int charIndex = 0;
                        if (charVal3 >= 'ぁ')
                        {
                            charIndex = _kanjiMap[(uint)charVal3];
                            _kanjiSprite.frame = charIndex;
                            _kanjiSprite.Scale = base.Scale;
                            _kanjiSprite.color = c;
                            _kanjiSprite.Alpha = base.Alpha;
                            Graphics.Draw(_kanjiSprite, xpos + xOff + 1f, ypos + yOff + 1f, deep);
                            xOff += 8f * base.Scale.X;
                        }
                        else
                        {
                            charIndex = _characterMap[(uint)charVal3];
                            if (charIndex >= _widths.Count)
                            {
                                charIndex = _widths.Count - 1;
                            }
                            if (charIndex < 0)
                            {
                                break;
                            }
                            Rectangle dat = _widths[charIndex];
                            _texture.Scale = base.Scale;
                            if (_highlightStart != -1 && _highlightStart != _highlightEnd && ((_highlightStart < _highlightEnd && _letterIndex >= _highlightStart && _letterIndex < _highlightEnd) || (_letterIndex < _highlightStart && _letterIndex >= _highlightEnd)))
                            {
                                Graphics.DrawRect(new Vector2(xpos + xOff, ypos + yOff), new Vector2(xpos + xOff, ypos + yOff) + new Vector2(dat.width * base.Scale.X, (float)_charHeight * base.Scale.Y), c, deep - 5);
                                _texture.color = highlight;
                            }
                            else
                            {
                                _texture.color = c;
                            }
                            _texture.Alpha = base.Alpha;
                            if (_characterInfos != null)
                            {
                                if (charIndex < _characterInfos.Count)
                                {
                                    xOff += _characterInfos[charIndex].leading * base.Scale.X;
                                    Graphics.Draw(_texture, xpos + xOff, ypos + yOff, dat, deep);
                                    xOff += _characterInfos[charIndex].trailing * base.Scale.X;
                                    xOff += _characterInfos[charIndex].width * base.Scale.X;
                                }
                            }
                            else
                            {
                                Graphics.Draw(_texture, xpos + xOff, ypos + yOff, dat, deep);
                                xOff += (dat.width - 1f) * base.Scale.X;
                            }
                        }
                    }
                }
            }
        }
    }
    public RichTextBox MakeRTF(string text)
    {
        RichTextBox box = new RichTextBox();
        Color currentColor = Color.Black;
        string curString = "";
        box.SelectionColor = System.Drawing.Color.Black;
        for (_letterIndex = 0; _letterIndex < text.Length; _letterIndex++)
        {
            bool processedSpecialCharacter = false;
            if (text[_letterIndex] == '|')
            {
                int iPos = _letterIndex;
                Color col = ParseColor(text);
                if (col != Colors.Transparent)
                {
                    _previousColor = currentColor;
                    currentColor = col;
                    if (col == Color.White)
                    {
                        col = Color.Black;
                    }
                    box.AppendText(curString);
                    curString = "";
                    box.SelectionColor = System.Drawing.Color.FromArgb(col.r, col.g, col.b);
                    processedSpecialCharacter = true;
                }
                else
                {
                    _letterIndex = iPos;
                }
            }
            if (text[_letterIndex] == '\n')
            {
                box.AppendText(curString);
                curString = "";
                box.SelectionColor = System.Drawing.Color.Black;
            }
            if (!processedSpecialCharacter)
            {
                curString += text[_letterIndex];
            }
        }
        box.AppendText(curString);
        curString = "";
        return box;
    }
}
