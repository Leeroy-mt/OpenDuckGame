using Microsoft.Xna.Framework.Input;
using SDL3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DuckGame;

public class Keyboard : InputDevice
{
    public class RepeatKey
    {
        public Keys key;

        public float repeatTime;
    }

    public enum MapType : uint
    {
        MAPVK_VK_TO_VSC,
        MAPVK_VSC_TO_VK,
        MAPVK_VK_TO_CHAR,
        MAPVK_VSC_TO_VK_EX
    }

    #region Public Fields

    public static bool isComposing;

    public static string keyString = "";

    #endregion

    #region Private Fields

    static bool _keyboardPress;
    static bool _repeat;
    static bool ignoreCore;
    static bool _registerLock;

    static int _lastKeyCount;
    static int _flipper;
    static int ignoreEnter;
    static int _usingVoiceRegister;

    static KeyboardState _keyState;
    static KeyboardState _keyStatePrev;

    static Thing _registerSetThing;

    static List<Keys> _repeatList = [];
    static Dictionary<int, Sprite> _triggerImages;
    static Dictionary<Keys, SDL.SDL_Scancode> KeyScanCodeMap = new()
        {
            { Keys.A,         SDL.SDL_Scancode.SDL_SCANCODE_A },
            { Keys.B,         SDL.SDL_Scancode.SDL_SCANCODE_B },
            { Keys.C,         SDL.SDL_Scancode.SDL_SCANCODE_C },
            { Keys.D,         SDL.SDL_Scancode.SDL_SCANCODE_D },
            { Keys.E,         SDL.SDL_Scancode.SDL_SCANCODE_E },
            { Keys.F,         SDL.SDL_Scancode.SDL_SCANCODE_F },
            { Keys.G,         SDL.SDL_Scancode.SDL_SCANCODE_G },
            { Keys.H,         SDL.SDL_Scancode.SDL_SCANCODE_H },
            { Keys.I,         SDL.SDL_Scancode.SDL_SCANCODE_I },
            { Keys.J,         SDL.SDL_Scancode.SDL_SCANCODE_J },
            { Keys.K,         SDL.SDL_Scancode.SDL_SCANCODE_K },
            { Keys.L,         SDL.SDL_Scancode.SDL_SCANCODE_L },
            { Keys.M,         SDL.SDL_Scancode.SDL_SCANCODE_M },
            { Keys.N,         SDL.SDL_Scancode.SDL_SCANCODE_N },
            { Keys.O,         SDL.SDL_Scancode.SDL_SCANCODE_O },
            { Keys.P,         SDL.SDL_Scancode.SDL_SCANCODE_P },
            { Keys.Q,         SDL.SDL_Scancode.SDL_SCANCODE_Q },
            { Keys.R,         SDL.SDL_Scancode.SDL_SCANCODE_R },
            { Keys.S,         SDL.SDL_Scancode.SDL_SCANCODE_S },
            { Keys.T,         SDL.SDL_Scancode.SDL_SCANCODE_T },
            { Keys.U,         SDL.SDL_Scancode.SDL_SCANCODE_U },
            { Keys.V,         SDL.SDL_Scancode.SDL_SCANCODE_V },
            { Keys.W,         SDL.SDL_Scancode.SDL_SCANCODE_W },
            { Keys.X,         SDL.SDL_Scancode.SDL_SCANCODE_X },
            { Keys.Y,         SDL.SDL_Scancode.SDL_SCANCODE_Y },
            { Keys.Z,         SDL.SDL_Scancode.SDL_SCANCODE_Z },
            { Keys.D0,        SDL.SDL_Scancode.SDL_SCANCODE_0 },
            { Keys.D1,        SDL.SDL_Scancode.SDL_SCANCODE_1 },
            { Keys.D2,        SDL.SDL_Scancode.SDL_SCANCODE_2 },
            { Keys.D3,        SDL.SDL_Scancode.SDL_SCANCODE_3 },
            { Keys.D4,        SDL.SDL_Scancode.SDL_SCANCODE_4 },
            { Keys.D5,        SDL.SDL_Scancode.SDL_SCANCODE_5 },
            { Keys.D6,        SDL.SDL_Scancode.SDL_SCANCODE_6 },
            { Keys.D7,        SDL.SDL_Scancode.SDL_SCANCODE_7 },
            { Keys.D8,        SDL.SDL_Scancode.SDL_SCANCODE_8 },
            { Keys.D9,        SDL.SDL_Scancode.SDL_SCANCODE_9 },
            { Keys.NumPad0,       SDL.SDL_Scancode.SDL_SCANCODE_KP_0 },
            { Keys.NumPad1,       SDL.SDL_Scancode.SDL_SCANCODE_KP_1 },
            { Keys.NumPad2,       SDL.SDL_Scancode.SDL_SCANCODE_KP_2 },
            { Keys.NumPad3,       SDL.SDL_Scancode.SDL_SCANCODE_KP_3 },
            { Keys.NumPad4,       SDL.SDL_Scancode.SDL_SCANCODE_KP_4 },
            { Keys.NumPad5,       SDL.SDL_Scancode.SDL_SCANCODE_KP_5 },
            { Keys.NumPad6,       SDL.SDL_Scancode.SDL_SCANCODE_KP_6 },
            { Keys.NumPad7,       SDL.SDL_Scancode.SDL_SCANCODE_KP_7 },
            { Keys.NumPad8,       SDL.SDL_Scancode.SDL_SCANCODE_KP_8 },
            { Keys.NumPad9,       SDL.SDL_Scancode.SDL_SCANCODE_KP_9 },
            { Keys.OemClear,      SDL.SDL_Scancode.SDL_SCANCODE_KP_CLEAR },
            { Keys.Decimal,       SDL.SDL_Scancode.SDL_SCANCODE_KP_DECIMAL },
            { Keys.Divide,        SDL.SDL_Scancode.SDL_SCANCODE_KP_DIVIDE },
            { Keys.Multiply,      SDL.SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY },
            { Keys.Subtract,      SDL.SDL_Scancode.SDL_SCANCODE_KP_MINUS },
            { Keys.Add,       SDL.SDL_Scancode.SDL_SCANCODE_KP_PLUS },
            { Keys.F1,        SDL.SDL_Scancode.SDL_SCANCODE_F1 },
            { Keys.F2,        SDL.SDL_Scancode.SDL_SCANCODE_F2 },
            { Keys.F3,        SDL.SDL_Scancode.SDL_SCANCODE_F3 },
            { Keys.F4,        SDL.SDL_Scancode.SDL_SCANCODE_F4 },
            { Keys.F5,        SDL.SDL_Scancode.SDL_SCANCODE_F5 },
            { Keys.F6,        SDL.SDL_Scancode.SDL_SCANCODE_F6 },
            { Keys.F7,        SDL.SDL_Scancode.SDL_SCANCODE_F7 },
            { Keys.F8,        SDL.SDL_Scancode.SDL_SCANCODE_F8 },
            { Keys.F9,        SDL.SDL_Scancode.SDL_SCANCODE_F9 },
            { Keys.F10,       SDL.SDL_Scancode.SDL_SCANCODE_F10 },
            { Keys.F11,       SDL.SDL_Scancode.SDL_SCANCODE_F11 },
            { Keys.F12,       SDL.SDL_Scancode.SDL_SCANCODE_F12 },
            { Keys.F13,       SDL.SDL_Scancode.SDL_SCANCODE_F13 },
            { Keys.F14,       SDL.SDL_Scancode.SDL_SCANCODE_F14 },
            { Keys.F15,       SDL.SDL_Scancode.SDL_SCANCODE_F15 },
            { Keys.F16,       SDL.SDL_Scancode.SDL_SCANCODE_F16 },
            { Keys.F17,       SDL.SDL_Scancode.SDL_SCANCODE_F17 },
            { Keys.F18,       SDL.SDL_Scancode.SDL_SCANCODE_F18 },
            { Keys.F19,       SDL.SDL_Scancode.SDL_SCANCODE_F19 },
            { Keys.F20,       SDL.SDL_Scancode.SDL_SCANCODE_F20 },
            { Keys.F21,       SDL.SDL_Scancode.SDL_SCANCODE_F21 },
            { Keys.F22,       SDL.SDL_Scancode.SDL_SCANCODE_F22 },
            { Keys.F23,       SDL.SDL_Scancode.SDL_SCANCODE_F23 },
            { Keys.F24,       SDL.SDL_Scancode.SDL_SCANCODE_F24 },
            { Keys.Space,     SDL.SDL_Scancode.SDL_SCANCODE_SPACE },
            { Keys.Up,        SDL.SDL_Scancode.SDL_SCANCODE_UP },
            { Keys.Down,      SDL.SDL_Scancode.SDL_SCANCODE_DOWN },
            { Keys.Left,      SDL.SDL_Scancode.SDL_SCANCODE_LEFT },
            { Keys.Right,     SDL.SDL_Scancode.SDL_SCANCODE_RIGHT },
            { Keys.LeftAlt,       SDL.SDL_Scancode.SDL_SCANCODE_LALT },
            { Keys.RightAlt,      SDL.SDL_Scancode.SDL_SCANCODE_RALT },
            { Keys.LeftControl,   SDL.SDL_Scancode.SDL_SCANCODE_LCTRL },
            { Keys.RightControl,  SDL.SDL_Scancode.SDL_SCANCODE_RCTRL },
            { Keys.LeftWindows,   SDL.SDL_Scancode.SDL_SCANCODE_LGUI },
            { Keys.RightWindows,  SDL.SDL_Scancode.SDL_SCANCODE_RGUI },
            { Keys.LeftShift,     SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT },
            { Keys.RightShift,    SDL.SDL_Scancode.SDL_SCANCODE_RSHIFT },
            { Keys.Apps,      SDL.SDL_Scancode.SDL_SCANCODE_APPLICATION },
            { Keys.OemQuestion,   SDL.SDL_Scancode.SDL_SCANCODE_SLASH },
            { Keys.OemPipe,       SDL.SDL_Scancode.SDL_SCANCODE_BACKSLASH },
            { Keys.OemOpenBrackets,   SDL.SDL_Scancode.SDL_SCANCODE_LEFTBRACKET },
            { Keys.OemCloseBrackets,  SDL.SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET },
            { Keys.CapsLock,      SDL.SDL_Scancode.SDL_SCANCODE_CAPSLOCK },
            { Keys.OemComma,      SDL.SDL_Scancode.SDL_SCANCODE_COMMA },
            { Keys.Delete,        SDL.SDL_Scancode.SDL_SCANCODE_DELETE },
            { Keys.End,       SDL.SDL_Scancode.SDL_SCANCODE_END },
            { Keys.Back,      SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE },
            { Keys.Enter,     SDL.SDL_Scancode.SDL_SCANCODE_RETURN },
            { Keys.Escape,        SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE },
            { Keys.Home,      SDL.SDL_Scancode.SDL_SCANCODE_HOME },
            { Keys.Insert,        SDL.SDL_Scancode.SDL_SCANCODE_INSERT },
            { Keys.OemMinus,      SDL.SDL_Scancode.SDL_SCANCODE_MINUS },
            { Keys.NumLock,       SDL.SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR },
            { Keys.PageUp,        SDL.SDL_Scancode.SDL_SCANCODE_PAGEUP },
            { Keys.PageDown,      SDL.SDL_Scancode.SDL_SCANCODE_PAGEDOWN },
            { Keys.Pause,     SDL.SDL_Scancode.SDL_SCANCODE_PAUSE },
            { Keys.OemPeriod,     SDL.SDL_Scancode.SDL_SCANCODE_PERIOD },
            { Keys.OemPlus,       SDL.SDL_Scancode.SDL_SCANCODE_EQUALS },
            { Keys.PrintScreen,   SDL.SDL_Scancode.SDL_SCANCODE_PRINTSCREEN },
            { Keys.OemQuotes,     SDL.SDL_Scancode.SDL_SCANCODE_APOSTROPHE },
            { Keys.Scroll,        SDL.SDL_Scancode.SDL_SCANCODE_SCROLLLOCK },
            { Keys.OemSemicolon,  SDL.SDL_Scancode.SDL_SCANCODE_SEMICOLON },
            { Keys.Sleep,     SDL.SDL_Scancode.SDL_SCANCODE_SLEEP },
            { Keys.Tab,       SDL.SDL_Scancode.SDL_SCANCODE_TAB },
            { Keys.OemTilde,      SDL.SDL_Scancode.SDL_SCANCODE_GRAVE },
            { Keys.VolumeUp,      SDL.SDL_Scancode.SDL_SCANCODE_VOLUMEUP },
            { Keys.VolumeDown,    SDL.SDL_Scancode.SDL_SCANCODE_VOLUMEDOWN },
            { Keys.None,      SDL.SDL_Scancode.SDL_SCANCODE_UNKNOWN }
        };

    bool _fakeDisconnect;

    List<RepeatKey> _repeatingKeys = [];

    Dictionary<int, string> _triggerNames;

    #endregion

    #region Public Properties

    public override bool isConnected => !_fakeDisconnect;

    public static bool repeat
    {
        get => _repeat;
        set => _repeat = value;
    }

    public object KeyInterop { get; private set; }

    public static bool control => Down(Keys.LeftControl) || Down(Keys.RightControl);

    public static bool alt => Down(Keys.LeftAlt) || Down(Keys.RightAlt);

    public static bool shift => Down(Keys.LeftShift) || Down(Keys.RightShift);

    #endregion

    #region Constructors

    public Keyboard(string name, int index)
        : base(index)
    {
        _name = "keyboard";
        _productName = name;
        _productGUID = "";
    }

    #endregion

    #region Public Methods

    public static void InitTriggerImages()
    {
        if (_triggerImages != null)
            return;

        _triggerImages = new Dictionary<int, Sprite>
        {
            [9999] = new Sprite("buttons/keyboard/arrows"),
            [9998] = new Sprite("buttons/keyboard/wasd"),
            [int.MaxValue] = new Sprite("buttons/keyboard/key")
        };

        foreach (Keys key in Enum.GetValues<Keys>())
        {
            char c = KeyToChar(key);
            if (c == ' ')
                switch (key)
                {
                    case Keys.Left:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/leftKey");
                        break;
                    case Keys.Right:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/rightKey");
                        break;
                    case Keys.Up:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/upKey");
                        break;
                    case Keys.Down:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/downKey");
                        break;
                    case Keys.Back:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/back");
                        break;
                    case Keys.LeftControl:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/control");
                        break;
                    case Keys.RightControl:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/control");
                        break;
                    case Keys.LeftShift:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/shift");
                        break;
                    case Keys.RightShift:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/shift");
                        break;
                    case Keys.LeftAlt:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/alt");
                        break;
                    case Keys.RightAlt:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/alt");
                        break;
                    case Keys.Tab:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/tab");
                        break;
                    case Keys.Enter:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/enter");
                        break;
                    case Keys.Space:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/space");
                        break;
                    case Keys.Insert:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/insert");
                        break;
                    case Keys.Home:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/home");
                        break;
                    case Keys.PageUp:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/pgup");
                        break;
                    case Keys.PageDown:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/pgdown");
                        break;
                    case Keys.Escape:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/escape");
                        break;
                    case Keys.End:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/end");
                        break;
                    case Keys.F1:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/f1");
                        break;
                    case Keys.F2:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/f2");
                        break;
                    case Keys.F3:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/f3");
                        break;
                    case Keys.F4:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/f4");
                        break;
                    case Keys.F5:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/f5");
                        break;
                    case Keys.F6:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/f6");
                        break;
                    case Keys.F7:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/f7");
                        break;
                    case Keys.F8:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/f8");
                        break;
                    case Keys.F9:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/f9");
                        break;
                    case Keys.F10:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/f10");
                        break;
                    case Keys.F11:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/f11");
                        break;
                    case Keys.F12:
                        _triggerImages[(int)key] = new Sprite("buttons/keyboard/f12");
                        break;
                    case Keys.MouseLeft:
                        {
                            SpriteMap m3 = new("buttons/mouse", 12, 15)
                            {
                                frame = 0
                            };
                            _triggerImages[(int)key] = m3;
                            break;
                        }
                    case Keys.MouseMiddle:
                        {
                            SpriteMap m2 = new("buttons/mouse", 12, 15)
                            {
                                frame = 1
                            };
                            _triggerImages[(int)key] = m2;
                            break;
                        }
                    case Keys.MouseRight:
                        {
                            SpriteMap m = new("buttons/mouse", 12, 15)
                            {
                                frame = 2
                            };
                            _triggerImages[(int)key] = m;
                            break;
                        }
                }
            else
                _triggerImages[(int)key] = new KeyImage(c);
        }
    }

    public static void IMECharEnteredHandler(object sender, CharacterEventArgs e)
    {
        if (e.Character == '\u3000')
            keyString += " ";
        else
            keyString += e.Character;
        ignoreEnter = 4;
    }

    public static void ALTCharEnteredHandler(object sender, CharacterEventArgs e)
    {
        if (e.ExtendedKey)
        {
            if (e.Character == '\u3000')
                keyString += " ";
            else
                keyString += e.Character;
        }
    }

    public static bool NothingPressed()
    {
        return _keyState.GetPressedKeys().Length == 0 && _keyStatePrev.GetPressedKeys().Length == 0;
    }

    public static bool Pressed(Keys key, bool any = false)
    {
        if (_usingVoiceRegister > 0 && IsKeyNote(key))
            return false;

        if (Input.ignoreInput)
            return false;

        if (any && _keyboardPress)
            return true;

        if (key == Keys.Enter && ignoreEnter > 0)
            return false;

        if (key >= Keys.MouseKeys)
        {
            return key switch
            {
                Keys.MouseLeft => Mouse.left == InputState.Pressed,
                Keys.MouseMiddle => Mouse.middle == InputState.Pressed,
                Keys.MouseRight => Mouse.right == InputState.Pressed,
                _ => false,
            };
        }

        return (_keyState.IsKeyDown((Microsoft.Xna.Framework.Input.Keys)key) && !_keyStatePrev.IsKeyDown((Microsoft.Xna.Framework.Input.Keys)key)) || _repeatList.Contains(key);
    }

    public static bool Down(Keys key)
    {
        if (_usingVoiceRegister > 0 && IsKeyNote(key))
            return false;

        if (Input.ignoreInput)
            return false;

        if (key == Keys.Enter && ignoreEnter > 0)
            return false;

        if (key >= Keys.MouseKeys)
        {
            return key switch
            {
                Keys.MouseLeft => Mouse.left is InputState.Down or InputState.Pressed,
                Keys.MouseMiddle => Mouse.middle is InputState.Down or InputState.Pressed,
                Keys.MouseRight => Mouse.right is InputState.Down or InputState.Pressed,
                _ => false
            };
        }

        return _keyState.IsKeyDown((Microsoft.Xna.Framework.Input.Keys)key);
    }

    public static bool Released(Keys key)
    {
        if (_usingVoiceRegister > 0 && IsKeyNote(key))
            return false;

        if (Input.ignoreInput)
            return false;

        if (key == Keys.Enter && ignoreEnter > 0)
            return false;

        if (key >= Keys.MouseKeys)
        {
            return key switch
            {
                Keys.MouseLeft => Mouse.left == InputState.Released,
                Keys.MouseMiddle => Mouse.middle == InputState.Released,
                Keys.MouseRight => Mouse.right == InputState.Released,
                _ => false,
            };
        }

        return !_keyState.IsKeyDown((Microsoft.Xna.Framework.Input.Keys)key) && _keyStatePrev.IsKeyDown((Microsoft.Xna.Framework.Input.Keys)key);
    }

    public static char? CharFromKey(Keys key)
    {
        if (KeyScanCodeMap.TryGetValue(key, out var scanCode))
        {
            var n = SDL.SDL_GetKeyFromScancode(
                        scanCode,
                        SDL.SDL_GetModState(),
                        false
                        );

            var valid = n >= 0x000000 && n <= 0x10ffff
                     && (n < 0x00d800 || n > 0x00dfff);

            if (!valid)
                return null;

            return (char)n;
        }

        return null;
    }

    public static char GetCharFromKey(Keys key)
    {
        char ch = ' ';
        byte[] keyboardState = new byte[256];
        GetKeyboardState(keyboardState);
        uint scanCode = MapVirtualKey((uint)key, MapType.MAPVK_VK_TO_VSC);
        StringBuilder stringBuilder = new(2);
        int result = ToUnicode((uint)key, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);

        if (stringBuilder.Length == 0)
            return ' ';

        switch (result)
        {
            case 1:
                ch = stringBuilder[0];
                switch (ch)
                {
                    case 'º':
                        ch = '`';
                        break;
                    case 'ª':
                        ch = '~';
                        break;
                }
                break;
            case -1 or 0:
                break;
            default:
                ch = stringBuilder[0];
                break;
        }

        return ch;
    }

    public static char KeyToChar(Keys key, bool caps = true, bool shift = false)
    {
        if (caps)
            switch (key)
            {
                case Keys.A:
                    return 'A';
                case Keys.B:
                    return 'B';
                case Keys.C:
                    return 'C';
                case Keys.D:
                    return 'D';
                case Keys.E:
                    return 'E';
                case Keys.F:
                    return 'F';
                case Keys.G:
                    return 'G';
                case Keys.H:
                    return 'H';
                case Keys.I:
                    return 'I';
                case Keys.J:
                    return 'J';
                case Keys.K:
                    return 'K';
                case Keys.L:
                    return 'L';
                case Keys.M:
                    return 'M';
                case Keys.N:
                    return 'N';
                case Keys.O:
                    return 'O';
                case Keys.P:
                    return 'P';
                case Keys.Q:
                    return 'Q';
                case Keys.R:
                    return 'R';
                case Keys.S:
                    return 'S';
                case Keys.T:
                    return 'T';
                case Keys.U:
                    return 'U';
                case Keys.V:
                    return 'V';
                case Keys.W:
                    return 'W';
                case Keys.X:
                    return 'X';
                case Keys.Y:
                    return 'Y';
                case Keys.Z:
                    return 'Z';
                case Keys.D0:
                    return '0';
                case Keys.D1:
                    return '1';
                case Keys.D2:
                    return '2';
                case Keys.D3:
                    return '3';
                case Keys.D4:
                    return '4';
                case Keys.D5:
                    return '5';
                case Keys.D6:
                    return '6';
                case Keys.D7:
                    return '7';
                case Keys.D8:
                    return '8';
                case Keys.D9:
                    return '9';
                case Keys.OemBackslash:
                    return '\\';
                case Keys.OemCloseBrackets:
                    return ']';
                case Keys.OemComma:
                    return ',';
                case Keys.OemMinus:
                    return '-';
                case Keys.OemOpenBrackets:
                    return '[';
                case Keys.OemPeriod:
                    return '.';
                case Keys.OemPipe:
                    return '\\';
                case Keys.OemPlus:
                    return '=';
                case Keys.OemQuestion:
                    return '/';
                case Keys.OemQuotes:
                    return '\'';
                case Keys.OemSemicolon:
                    return ';';
                case Keys.OemTilde:
                    return '~';
                case Keys.NumPad0:
                    return '0';
                case Keys.NumPad1:
                    return '1';
                case Keys.NumPad2:
                    return '2';
                case Keys.NumPad3:
                    return '3';
                case Keys.NumPad4:
                    return '4';
                case Keys.NumPad5:
                    return '5';
                case Keys.NumPad6:
                    return '6';
                case Keys.NumPad7:
                    return '7';
                case Keys.NumPad8:
                    return '8';
                case Keys.NumPad9:
                    return '9';
            }
        else if (shift)
            switch (key)
            {
                case Keys.A:
                    return 'A';
                case Keys.B:
                    return 'B';
                case Keys.C:
                    return 'C';
                case Keys.D:
                    return 'D';
                case Keys.E:
                    return 'E';
                case Keys.F:
                    return 'F';
                case Keys.G:
                    return 'G';
                case Keys.H:
                    return 'H';
                case Keys.I:
                    return 'I';
                case Keys.J:
                    return 'J';
                case Keys.K:
                    return 'K';
                case Keys.L:
                    return 'L';
                case Keys.M:
                    return 'M';
                case Keys.N:
                    return 'N';
                case Keys.O:
                    return 'O';
                case Keys.P:
                    return 'P';
                case Keys.Q:
                    return 'Q';
                case Keys.R:
                    return 'R';
                case Keys.S:
                    return 'S';
                case Keys.T:
                    return 'T';
                case Keys.U:
                    return 'U';
                case Keys.V:
                    return 'V';
                case Keys.W:
                    return 'W';
                case Keys.X:
                    return 'X';
                case Keys.Y:
                    return 'Y';
                case Keys.Z:
                    return 'Z';
                case Keys.D0:
                    return ')';
                case Keys.D1:
                    return '!';
                case Keys.D2:
                    return '@';
                case Keys.D3:
                    return '#';
                case Keys.D4:
                    return '$';
                case Keys.D5:
                    return '%';
                case Keys.D6:
                    return '^';
                case Keys.D7:
                    return '&';
                case Keys.D8:
                    return '*';
                case Keys.D9:
                    return '(';
                case Keys.OemBackslash:
                    return '|';
                case Keys.OemCloseBrackets:
                    return '}';
                case Keys.OemComma:
                    return '<';
                case Keys.OemMinus:
                    return '_';
                case Keys.OemOpenBrackets:
                    return '{';
                case Keys.OemPeriod:
                    return '>';
                case Keys.OemPipe:
                    return '|';
                case Keys.OemPlus:
                    return '+';
                case Keys.OemQuestion:
                    return '?';
                case Keys.OemQuotes:
                    return '"';
                case Keys.OemSemicolon:
                    return ':';
                case Keys.OemTilde:
                    return '~';
                case Keys.NumPad0:
                    return '0';
                case Keys.NumPad1:
                    return '1';
                case Keys.NumPad2:
                    return '2';
                case Keys.NumPad3:
                    return '3';
                case Keys.NumPad4:
                    return '4';
                case Keys.NumPad5:
                    return '5';
                case Keys.NumPad6:
                    return '6';
                case Keys.NumPad7:
                    return '7';
                case Keys.NumPad8:
                    return '8';
                case Keys.NumPad9:
                    return '9';
            }
        else
            switch (key)
            {
                case Keys.A:
                    return 'a';
                case Keys.B:
                    return 'b';
                case Keys.C:
                    return 'c';
                case Keys.D:
                    return 'd';
                case Keys.E:
                    return 'e';
                case Keys.F:
                    return 'f';
                case Keys.G:
                    return 'g';
                case Keys.H:
                    return 'h';
                case Keys.I:
                    return 'i';
                case Keys.J:
                    return 'j';
                case Keys.K:
                    return 'k';
                case Keys.L:
                    return 'l';
                case Keys.M:
                    return 'm';
                case Keys.N:
                    return 'n';
                case Keys.O:
                    return 'o';
                case Keys.P:
                    return 'p';
                case Keys.Q:
                    return 'q';
                case Keys.R:
                    return 'r';
                case Keys.S:
                    return 's';
                case Keys.T:
                    return 't';
                case Keys.U:
                    return 'u';
                case Keys.V:
                    return 'v';
                case Keys.W:
                    return 'w';
                case Keys.X:
                    return 'x';
                case Keys.Y:
                    return 'y';
                case Keys.Z:
                    return 'z';
                case Keys.D0:
                    return '0';
                case Keys.D1:
                    return '1';
                case Keys.D2:
                    return '2';
                case Keys.D3:
                    return '3';
                case Keys.D4:
                    return '4';
                case Keys.D5:
                    return '5';
                case Keys.D6:
                    return '6';
                case Keys.D7:
                    return '7';
                case Keys.D8:
                    return '8';
                case Keys.D9:
                    return '9';
                case Keys.OemBackslash:
                    return '\\';
                case Keys.OemCloseBrackets:
                    return ']';
                case Keys.OemComma:
                    return ',';
                case Keys.OemMinus:
                    return '-';
                case Keys.OemOpenBrackets:
                    return '[';
                case Keys.OemPeriod:
                    return '.';
                case Keys.OemPipe:
                    return '\\';
                case Keys.OemPlus:
                    return '=';
                case Keys.OemQuestion:
                    return '/';
                case Keys.OemQuotes:
                    return '\'';
                case Keys.OemSemicolon:
                    return ';';
                case Keys.OemTilde:
                    return '~';
                case Keys.NumPad0:
                    return '0';
                case Keys.NumPad1:
                    return '1';
                case Keys.NumPad2:
                    return '2';
                case Keys.NumPad3:
                    return '3';
                case Keys.NumPad4:
                    return '4';
                case Keys.NumPad5:
                    return '5';
                case Keys.NumPad6:
                    return '6';
                case Keys.NumPad7:
                    return '7';
                case Keys.NumPad8:
                    return '8';
                case Keys.NumPad9:
                    return '9';
            }
        return ' ';
    }

    public static int CurrentNote(InputProfile pProfile, Thing pInstrument)
    {
        _registerSetThing = pInstrument;
        _usingVoiceRegister = 0;

        if (Input.Pressed("VOICEREG"))
            _registerLock = !_registerLock;

        return KeyNote();
    }

    public override void Update()
    {
        if (_usingVoiceRegister > 0)
            _usingVoiceRegister--;

        ignoreEnter--;
        if (ignoreEnter < 0)
            ignoreEnter = 0;

        if (!Graphics.inFocus)
            return;

        if (_usingVoiceRegister == 0)
        {
            if (Pressed(Keys.D8) && index == 0)
                _fakeDisconnect = !_fakeDisconnect;
            if (Pressed(Keys.D9) && index == 1)
                _fakeDisconnect = !_fakeDisconnect;
        }

        if (_flipper == 0)
        {
            _keyStatePrev = _keyState;
            _keyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            _keyboardPress = false;

            int keyCount = _keyState.GetPressedKeys().Length;
            if (keyCount != _lastKeyCount && keyCount != 0)
                _keyboardPress = true;
            _lastKeyCount = keyCount;

            UpdateKeyboardString();

            _flipper = 1;

            if (_registerLock && (_registerSetThing == null || _registerSetThing.removeFromLevel || _registerSetThing.owner == null || DevConsole.open || DuckNetwork.core.enteringText))
                _registerLock = false;
        }
        else
        {
            _flipper--;
        }

        if (index == 0)
            _repeatList.Clear();

        ignoreCore = true;
        if (_repeat)
        {
            foreach (Keys k in Enum.GetValues<Keys>())
            {
                if (MapPressed((int)k) && (k < Keys.F1 || k > Keys.F12) && _repeatingKeys.FirstOrDefault(x => x.key == k) == null)
                {
                    _repeatingKeys.Add(new RepeatKey
                    {
                        key = k,
                        repeatTime = 2
                    });
                }
            }

            List<RepeatKey> removeKeys = [];
            foreach (RepeatKey key in _repeatingKeys)
            {
                key.repeatTime -= 0.1f;
                bool down = MapDown((int)key.key);

                if (down && key.repeatTime < 0)
                    _repeatList.Add(key.key);
                if (key.repeatTime <= 0 && down)
                    key.repeatTime = 0.25f;
                if (!down)
                    removeKeys.Add(key);
            }

            foreach (RepeatKey k2 in removeKeys)
                _repeatingKeys.Remove(k2);
        }
        ignoreCore = false;
    }

    public override bool MapPressed(int mapping, bool any = false)
    {
        if (!ignoreCore && (DevConsole.open || DuckNetwork.enteringText || Editor.enteringText))
            return false;

        if (!Pressed((Keys)mapping, any))
            return _repeatList.Contains((Keys)mapping);

        return true;
    }

    public override bool MapDown(int mapping, bool any = false)
    {
        if (!ignoreCore && (DevConsole.open || DuckNetwork.enteringText || Editor.enteringText))
            return false;

        return Down((Keys)mapping);
    }

    public override bool MapReleased(int mapping)
    {
        if (!ignoreCore && (DevConsole.open || DuckNetwork.enteringText || Editor.enteringText))
            return false;

        return Released((Keys)mapping);
    }

    public override Sprite GetMapImage(int map)
    {
        _triggerImages.TryGetValue(map, out var spr);
        return spr ?? _triggerImages[int.MaxValue];
    }

    public override Dictionary<int, string> GetTriggerNames()
    {
        if (_triggerNames == null)
        {
            _triggerNames = [];
            foreach (Keys key in Enum.GetValues<Keys>())
            {
                char c = KeyToChar(key);
                if (c == ' ')
                    switch (key)
                    {
                        case Keys.Left:
                            _triggerNames[(int)key] = "LEFT";
                            break;
                        case Keys.Right:
                            _triggerNames[(int)key] = "RIGHT";
                            break;
                        case Keys.Up:
                            _triggerNames[(int)key] = "UP";
                            break;
                        case Keys.Down:
                            _triggerNames[(int)key] = "DOWN";
                            break;
                        case Keys.Back:
                            _triggerNames[(int)key] = "BACK";
                            break;
                        case Keys.LeftControl:
                            _triggerNames[(int)key] = "LCTRL";
                            break;
                        case Keys.RightControl:
                            _triggerNames[(int)key] = "RCTRL";
                            break;
                        case Keys.LeftShift:
                            _triggerNames[(int)key] = "LSHFT";
                            break;
                        case Keys.RightShift:
                            _triggerNames[(int)key] = "RSHFT";
                            break;
                        case Keys.LeftAlt:
                            _triggerNames[(int)key] = "LALT";
                            break;
                        case Keys.RightAlt:
                            _triggerNames[(int)key] = "RALT";
                            break;
                        case Keys.Tab:
                            _triggerNames[(int)key] = "TAB";
                            break;
                        case Keys.Enter:
                            _triggerNames[(int)key] = "ENTER";
                            break;
                        case Keys.Space:
                            _triggerNames[(int)key] = "SPACE";
                            break;
                        case Keys.Insert:
                            _triggerNames[(int)key] = "INSRT";
                            break;
                        case Keys.Home:
                            _triggerNames[(int)key] = "HOME";
                            break;
                        case Keys.PageUp:
                            _triggerNames[(int)key] = "PGUP";
                            break;
                        case Keys.PageDown:
                            _triggerNames[(int)key] = "PGDN";
                            break;
                        case Keys.End:
                            _triggerNames[(int)key] = "END";
                            break;
                        case Keys.Escape:
                            _triggerNames[(int)key] = "ESC";
                            break;
                        case Keys.F1:
                            _triggerNames[(int)key] = "F1";
                            break;
                        case Keys.F2:
                            _triggerNames[(int)key] = "F2";
                            break;
                        case Keys.F3:
                            _triggerNames[(int)key] = "F3";
                            break;
                        case Keys.F4:
                            _triggerNames[(int)key] = "F4";
                            break;
                        case Keys.F5:
                            _triggerNames[(int)key] = "F5";
                            break;
                        case Keys.F6:
                            _triggerNames[(int)key] = "F6";
                            break;
                        case Keys.F7:
                            _triggerNames[(int)key] = "F7";
                            break;
                        case Keys.F8:
                            _triggerNames[(int)key] = "F8";
                            break;
                        case Keys.F9:
                            _triggerNames[(int)key] = "F9";
                            break;
                        case Keys.F10:
                            _triggerNames[(int)key] = "F10";
                            break;
                        case Keys.F11:
                            _triggerNames[(int)key] = "F11";
                            break;
                        case Keys.F12:
                            _triggerNames[(int)key] = "F12";
                            break;
                        case Keys.MouseLeft:
                            _triggerNames[(int)key] = "MB L";
                            break;
                        case Keys.MouseMiddle:
                            _triggerNames[(int)key] = "MB M";
                            break;
                        case Keys.MouseRight:
                            _triggerNames[(int)key] = "MB R";
                            break;
                    }
                else
                    _triggerNames[(int)key] = c.ToString();
            }
        }
        return _triggerNames;
    }

    #endregion

    #region Private Methods

    [DllImport("user32.dll")]
    static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags);

    [DllImport("user32.dll")]
    static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    static extern uint MapVirtualKey(uint uCode, MapType uMapType);

    static bool IsKeyNote(Keys pKey)
    {
        return pKey is Keys.D1
                    or Keys.D2
                    or Keys.D3
                    or Keys.D4
                    or Keys.D5
                    or Keys.D6
                    or Keys.D7
                    or Keys.D8
                    or Keys.D9
                    or Keys.D0
                    or Keys.OemMinus
                    or Keys.OemPlus
                    or Keys.Back;
    }

    static int KeyNote()
    {
        _usingVoiceRegister = 0;

        int reg = -1;
        if (_registerLock)
        {
            if (Down(Keys.D1))
                reg = 0;
            if (Down(Keys.D2))
                reg = 1;
            if (Down(Keys.D3))
                reg = 2;
            if (Down(Keys.D4))
                reg = 3;
            if (Down(Keys.D5))
                reg = 4;
            if (Down(Keys.D6))
                reg = 5;
            if (Down(Keys.D7))
                reg = 6;
            if (Down(Keys.D8))
                reg = 7;
            if (Down(Keys.D9))
                reg = 8;
            if (Down(Keys.D0))
                reg = 9;
            if (Down(Keys.OemMinus))
                reg = 10;
            if (Down(Keys.OemPlus))
                reg = 11;
            if (Down(Keys.Back))
                reg = 12;

            _usingVoiceRegister = 3;
        }
        return reg;
    }

    void UpdateKeyboardString()
    {
        ignoreCore = true;
        if (!isComposing)
        {
            foreach (var key in _keyState.GetPressedKeys())
            {
                if (!MapPressed((int)key))
                    continue;
                switch (key)
                {
                    case Microsoft.Xna.Framework.Input.Keys.Back:
                        if (keyString.Length > 0)
                            keyString = keyString[..^1];
                        continue;
                    case Microsoft.Xna.Framework.Input.Keys.Space:
                        keyString = keyString.Insert(keyString.Length, " ");
                        continue;
                    case Microsoft.Xna.Framework.Input.Keys.Enter:
                    case Microsoft.Xna.Framework.Input.Keys.Escape:
                    case Microsoft.Xna.Framework.Input.Keys.Delete:
                        continue;
                }

                var c = CharFromKey((Keys)key);
                if (c != ' ')
                    keyString += c;
            }
        }
        ignoreCore = false;

        isComposing = false;
    }

    #endregion
}