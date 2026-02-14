using Microsoft.Xna.Framework;
using System.Linq;

namespace DuckGame;

public class BitmapFont : Transform
{
    private SpriteMap _texture;

    public static SpriteMap _japaneseCharacters;

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
        'Б', 'В', 'Г', 'Д', 'Е', 'Ж', 'З', 'И', 'Й', 'К',
        'Л', 'М', 'Н', 'О', 'П', 'Р', 'С', 'Т', 'У', 'Ф',
        'Х', 'Ц', 'Ч', 'Ш', 'Щ', 'Ъ', 'Ы', 'Ь', 'Э', 'Ю',
        'Я', 'а', 'б', 'в', 'г', 'д', 'е', 'ж', 'з', 'и',
        'й', 'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т',
        'у', 'ф', 'х', 'ц', 'ч', 'ш', 'щ', 'ъ', 'ы', 'ь',
        'э', 'ю', 'я', '¡', '¿', 'Ё', 'ё'
    };

    private static int[] _characterMap = new int[65535];

    private const int kTilesPerRow = 16;

    private int _tileSize = 8;

    public int fallbackIndex;

    private BitmapFont _fallbackFont;

    private InputProfile _inputProfile;

    private Sprite _titleWing;

    private int _maxWidth;

    public bool allowBigSprites;

    private int _letterIndex;

    public bool singleLine;

    public bool enforceWidthByWord = true;

    private Color _previousColor;

    public int characterYOffset;

    public Vector2 spriteScale = new Vector2(1f, 1f);

    public Color colorOverride;

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

    public BitmapFont(string image, int size, int ysize = -1)
    {
        FancyBitmapFont.InitializeKanjis();
        if (ysize < 0)
        {
            ysize = size;
        }
        _texture = new SpriteMap(image, size, ysize);
        _tileSize = size;
        if (!_mapInitialized)
        {
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
        _titleWing = new Sprite("arcade/titleWing");
    }

    public Sprite ParseSprite(string text, InputProfile input)
    {
        if (!allowBigSprites && text.StartsWith("_!"))
        {
            return null;
        }
        _letterIndex++;
        string trigger = "";
        while (_letterIndex != text.Length && text[_letterIndex] != ' ' && text[_letterIndex] != '@')
        {
            trigger += text[_letterIndex];
            _letterIndex++;
        }
        Sprite spr = null;
        if (input != null)
        {
            spr = input.GetTriggerImage(trigger);
            if (spr == null && Triggers.IsTrigger(trigger))
            {
                return new Sprite();
            }
        }
        if (spr == null)
        {
            spr = Input.GetTriggerSprite(trigger);
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
            return new Color(_previousColor.R, _previousColor.G, _previousColor.B);
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

    public float GetWidth(string text, bool thinButtons = false, InputProfile input = null)
    {
        bool didThinButton = false;
        if (input == null)
        {
            if (!MonoMain.started)
            {
                input = InputProfile.DefaultPlayer1;
            }
            else
            {
                input = ((_inputProfile != null) ? _inputProfile : Input.lastActiveProfile);
                if (_inputProfile == null && Profiles.active.Count > 0 && !Network.isActive)
                {
                    input = Profiles.GetLastProfileWithInput().inputProfile;
                }
            }
        }
        float wide = 0f;
        float widest = 0f;
        for (_letterIndex = 0; _letterIndex < text.Length; _letterIndex++)
        {
            bool processedSpecialCharacter = false;
            if (text[_letterIndex] == '@')
            {
                int iPos = _letterIndex;
                Sprite spr = ParseSprite(text, input);
                if (spr != null)
                {
                    if (spr.texture != null)
                    {
                        wide += ((thinButtons && !didThinButton) ? 6f : ((float)spr.width * spr.Scale.X + 1f));
                        didThinButton = true;
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
            }
            if (!processedSpecialCharacter)
            {
                wide += (float)_tileSize * base.Scale.X;
            }
        }
        if (wide > widest)
        {
            widest = wide;
        }
        return widest;
    }

    public void DrawOutline(string text, Vector2 pos, Color c, Color outline, Depth deep = default(Depth))
    {
        Draw(text, pos + new Vector2(-1f * base.Scale.X, 0f), outline, deep + 2, null, colorSymbols: true);
        Draw(text, pos + new Vector2(1f * base.Scale.X, 0f), outline, deep + 2, null, colorSymbols: true);
        Draw(text, pos + new Vector2(0f, -1f * base.Scale.Y), outline, deep + 2, null, colorSymbols: true);
        Draw(text, pos + new Vector2(0f, 1f * base.Scale.Y), outline, deep + 2, null, colorSymbols: true);
        Draw(text, pos + new Vector2(-1f * base.Scale.X, -1f * base.Scale.Y), outline, deep + 2, null, colorSymbols: true);
        Draw(text, pos + new Vector2(1f * base.Scale.X, -1f * base.Scale.Y), outline, deep + 2, null, colorSymbols: true);
        Draw(text, pos + new Vector2(-1f * base.Scale.X, 1f * base.Scale.Y), outline, deep + 2, null, colorSymbols: true);
        Draw(text, pos + new Vector2(1f * base.Scale.X, 1f * base.Scale.Y), outline, deep + 2, null, colorSymbols: true);
        Draw(text, pos, c, deep + 5);
    }

    public void Draw(string text, Vector2 pos, Color c, Depth deep = default(Depth), InputProfile input = null, bool colorSymbols = false)
    {
        Draw(text, pos.X, pos.Y, c, deep, input, colorSymbols);
    }

    public void Draw(string text, float xpos, float ypos, Color c, Depth deep = default(Depth), InputProfile input = null, bool colorSymbols = false)
    {
        if (colorOverride != default(Color))
        {
            c = colorOverride;
        }
        _previousColor = c;
        if (input == null)
        {
            if (!MonoMain.started)
            {
                input = InputProfile.DefaultPlayer1;
            }
            else
            {
                input = ((_inputProfile != null) ? _inputProfile : Input.lastActiveProfile);
                if (_inputProfile == null && Profiles.active.Count > 0 && !Network.isActive)
                {
                    input = Profiles.GetLastProfileWithInput().inputProfile;
                }
            }
        }
        float yOff = 0f;
        float xOff = 0f;
        for (_letterIndex = 0; _letterIndex < text.Length; _letterIndex++)
        {
            bool processedSpecialCharacter = false;
            if (text[_letterIndex] == '@')
            {
                int iPos = _letterIndex;
                Sprite spr = ParseSprite(text, input);
                if (spr != null)
                {
                    if (spr.texture != null)
                    {
                        float al = spr.Alpha;
                        spr.Alpha = base.Alpha * c.ToVector4().W;
                        if (spr != null)
                        {
                            Vector2 sc = spr.Scale;
                            spr.Scale *= spriteScale;
                            float yCenter = (int)((float)_texture.height * spriteScale.Y / 2f) - (int)((float)spr.height * spriteScale.Y / 2f);
                            if (spr.moji)
                            {
                                if (spr.height == 28)
                                {
                                    spr.Scale *= 0.25f * base.Scale;
                                    yCenter += 10f * base.Scale.Y;
                                }
                                else
                                {
                                    spr.Scale *= 0.25f * base.Scale;
                                    yCenter += 3f * base.Scale.Y;
                                }
                            }
                            if (colorSymbols)
                            {
                                spr.color = c;
                            }
                            Graphics.Draw(spr, xpos + xOff, ypos + yOff + yCenter, deep);
                            xOff += (float)spr.width * spr.Scale.X + 1f;
                            spr.Scale = sc;
                            spr.color = Color.White;
                        }
                        spr.Alpha = al;
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
                Color col = ParseColor(text);
                if (colorOverride != default(Color))
                {
                    col = colorOverride;
                }
                if (col != Colors.Transparent)
                {
                    _previousColor = c;
                    float al2 = c.ToVector4().W;
                    c = col;
                    c *= al2;
                    processedSpecialCharacter = true;
                }
                else
                {
                    _letterIndex = iPos2;
                }
            }
            if (!processedSpecialCharacter)
            {
                if (maxWidth > 0)
                {
                    string nextWord = "";
                    int index = _letterIndex;
                    while (index < text.Count() && text[index] != ' ' && text[index] != '|' && text[index] != '@')
                    {
                        nextWord += text[index];
                        index++;
                        if (!enforceWidthByWord)
                        {
                            break;
                        }
                    }
                    if (xOff + (float)nextWord.Count() * ((float)_tileSize * base.Scale.X) > (float)maxWidth)
                    {
                        yOff += (float)_texture.height * base.Scale.Y;
                        xOff = 0f;
                        if (singleLine)
                        {
                            break;
                        }
                    }
                }
                if (text[_letterIndex] == '\n')
                {
                    yOff += (float)_texture.height * base.Scale.Y;
                    xOff = 0f;
                }
                else
                {
                    SpriteMap fontTexture = _texture;
                    char character = text[_letterIndex];
                    int charIndex = 0;
                    if (character >= 'ぁ')
                    {
                        fontTexture = FancyBitmapFont._kanjiSprite;
                        charIndex = FancyBitmapFont._kanjiMap[(uint)character];
                    }
                    else
                    {
                        charIndex = _characterMap[(uint)text[_letterIndex]];
                    }
                    if (fallbackIndex != 0 && charIndex >= fallbackIndex)
                    {
                        if (_fallbackFont == null)
                        {
                            _fallbackFont = new BitmapFont("biosFont", 8);
                        }
                        fontTexture = _fallbackFont._texture;
                    }
                    fontTexture.frame = charIndex;
                    fontTexture.Scale = base.Scale;
                    fontTexture.color = c;
                    fontTexture.Alpha = base.Alpha;
                    Graphics.Draw(fontTexture, xpos + xOff, ypos + yOff + (float)characterYOffset, deep);
                    xOff += (float)_tileSize * base.Scale.X;
                }
            }
        }
    }
}
