using Microsoft.Xna.Framework.Input;
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

    private static KeyboardState _keyState;

    private static KeyboardState _keyStatePrev;

    private static bool _keyboardPress = false;

    private static int _lastKeyCount = 0;

    private static int _flipper = 0;

    public static string keyString = "";

    private bool _fakeDisconnect;

    private Dictionary<int, string> _triggerNames;

    private static Dictionary<int, Sprite> _triggerImages;

    private static bool _repeat = false;

    private static List<Keys> _repeatList = new List<Keys>();

    private List<RepeatKey> _repeatingKeys = new List<RepeatKey>();

    public static bool isComposing = false;

    private static int ignoreEnter;

    private static bool ignoreCore = false;

    private static int _usingVoiceRegister;

    private static Thing _registerSetThing;

    private static bool _registerLock = false;

    private static int _currentNote = 0;

    public override bool isConnected => !_fakeDisconnect;

    public static bool repeat
    {
        get
        {
            return _repeat;
        }
        set
        {
            _repeat = value;
        }
    }

    public object KeyInterop { get; private set; }

    public static bool control
    {
        get
        {
            if (!Down(Keys.LeftControl))
            {
                return Down(Keys.RightControl);
            }
            return true;
        }
    }

    public static bool alt
    {
        get
        {
            if (!Down(Keys.LeftAlt))
            {
                return Down(Keys.RightAlt);
            }
            return true;
        }
    }

    public static bool shift
    {
        get
        {
            if (!Down(Keys.LeftShift))
            {
                return Down(Keys.RightShift);
            }
            return true;
        }
    }

    public static bool NothingPressed()
    {
        if (_keyState.GetPressedKeys().Length == 0)
        {
            return _keyStatePrev.GetPressedKeys().Length == 0;
        }
        return false;
    }

    public Keyboard(string name, int index)
        : base(index)
    {
        _name = "keyboard";
        _productName = name;
        _productGUID = "";
    }

    public override Dictionary<int, string> GetTriggerNames()
    {
        if (_triggerNames == null)
        {
            _triggerNames = new Dictionary<int, string>();
            foreach (Keys key in Enum.GetValues(typeof(Keys)).Cast<Keys>())
            {
                char c = KeyToChar(key);
                if (c == ' ')
                {
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
                }
                else
                {
                    _triggerNames[(int)key] = c.ToString() ?? "";
                }
            }
        }
        return _triggerNames;
    }

    public static void InitTriggerImages()
    {
        if (_triggerImages != null)
        {
            return;
        }
        _triggerImages = new Dictionary<int, Sprite>();
        _triggerImages[9999] = new Sprite("buttons/keyboard/arrows");
        _triggerImages[9998] = new Sprite("buttons/keyboard/wasd");
        _triggerImages[int.MaxValue] = new Sprite("buttons/keyboard/key");
        foreach (Keys key in Enum.GetValues(typeof(Keys)).Cast<Keys>())
        {
            char c = KeyToChar(key);
            if (c == ' ')
            {
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
                            SpriteMap m3 = new SpriteMap("buttons/mouse", 12, 15);
                            m3.frame = 0;
                            _triggerImages[(int)key] = m3;
                            break;
                        }
                    case Keys.MouseMiddle:
                        {
                            SpriteMap m2 = new SpriteMap("buttons/mouse", 12, 15);
                            m2.frame = 1;
                            _triggerImages[(int)key] = m2;
                            break;
                        }
                    case Keys.MouseRight:
                        {
                            SpriteMap m = new SpriteMap("buttons/mouse", 12, 15);
                            m.frame = 2;
                            _triggerImages[(int)key] = m;
                            break;
                        }
                }
            }
            else
            {
                _triggerImages[(int)key] = new KeyImage(c);
            }
        }
    }

    public override Sprite GetMapImage(int map)
    {
        Sprite spr = null;
        _triggerImages.TryGetValue(map, out spr);
        if (spr == null)
        {
            return _triggerImages[int.MaxValue];
        }
        return spr;
    }

    public static char KeyToChar(Keys key, bool caps = true, bool shift = false)
    {
        if (caps)
        {
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
        }
        else if (shift)
        {
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
        }
        else
        {
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
        }
        return ' ';
    }

    public override void Update()
    {
        if (_usingVoiceRegister > 0)
        {
            _usingVoiceRegister--;
        }
        ignoreEnter--;
        if (ignoreEnter < 0)
        {
            ignoreEnter = 0;
        }
        if (!Graphics.inFocus)
        {
            return;
        }
        if (_usingVoiceRegister == 0)
        {
            if (Pressed(Keys.D8) && base.index == 0)
            {
                _fakeDisconnect = !_fakeDisconnect;
            }
            if (Pressed(Keys.D9) && base.index == 1)
            {
                _fakeDisconnect = !_fakeDisconnect;
            }
        }
        if (_flipper == 0)
        {
            _keyStatePrev = _keyState;
            _keyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            _keyboardPress = false;
            int keyCount = _keyState.GetPressedKeys().Count();
            if (keyCount != _lastKeyCount && keyCount != 0)
            {
                _keyboardPress = true;
            }
            _lastKeyCount = keyCount;
            updateKeyboardString();
            _flipper = 1;
            if (_registerLock && (_registerSetThing == null || _registerSetThing.removeFromLevel || _registerSetThing.owner == null || DevConsole.open || DuckNetwork.core.enteringText))
            {
                _registerLock = false;
                _currentNote = 0;
            }
        }
        else
        {
            _flipper--;
        }
        if (base.index == 0)
        {
            _repeatList.Clear();
        }
        ignoreCore = true;
        if (_repeat)
        {
            foreach (Keys k in Enum.GetValues(typeof(Keys)).Cast<Keys>())
            {
                if (MapPressed((int)k) && (k < Keys.F1 || k > Keys.F12) && _repeatingKeys.FirstOrDefault((RepeatKey x) => x.key == k) == null)
                {
                    _repeatingKeys.Add(new RepeatKey
                    {
                        key = k,
                        repeatTime = 2f
                    });
                }
            }
            List<RepeatKey> removeKeys = new List<RepeatKey>();
            foreach (RepeatKey key in _repeatingKeys)
            {
                key.repeatTime -= 0.1f;
                bool down = MapDown((int)key.key);
                if (down && key.repeatTime < 0f)
                {
                    _repeatList.Add(key.key);
                }
                if (key.repeatTime <= 0f && down)
                {
                    key.repeatTime = 0.25f;
                }
                if (!down)
                {
                    removeKeys.Add(key);
                }
            }
            foreach (RepeatKey k2 in removeKeys)
            {
                _repeatingKeys.Remove(k2);
            }
        }
        ignoreCore = false;
    }

    [DllImport("user32.dll")]
    public static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags);

    [DllImport("user32.dll")]
    public static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

    public static char GetCharFromKey(Keys key)
    {
        char ch = ' ';
        byte[] keyboardState = new byte[256];
        GetKeyboardState(keyboardState);
        uint scanCode = MapVirtualKey((uint)key, MapType.MAPVK_VK_TO_VSC);
        StringBuilder stringBuilder = new StringBuilder(2);
        int result = ToUnicode((uint)key, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0u);
        if (stringBuilder.Length < 1)
        {
            return ' ';
        }
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
            default:
                ch = stringBuilder[0];
                break;
            case -1:
            case 0:
                break;
        }
        return ch;
    }

    public static void IMECharEnteredHandler(object sender, CharacterEventArgs e)
    {
        if (e.Character == '\u3000')
        {
            keyString += " ";
        }
        else
        {
            keyString += e.Character;
        }
        ignoreEnter = 4;
    }

    public static void ALTCharEnteredHandler(object sender, CharacterEventArgs e)
    {
        if (e.ExtendedKey)
        {
            if (e.Character == '\u3000')
            {
                keyString += " ";
            }
            else
            {
                keyString += e.Character;
            }
        }
    }

    private void updateKeyboardString()
    {
        ignoreCore = true;
        if (Down(Keys.LeftShift))
        {
            _ = 1;
        }
        else
            Down(Keys.RightShift);
        if (Down(Keys.LeftControl))
        {
            _ = 1;
        }
        else
            Down(Keys.RightControl);
        _ = Console.CapsLock;
        Microsoft.Xna.Framework.Input.Keys[] pressedKeys = _keyState.GetPressedKeys();
        if (!isComposing)
        {
            Microsoft.Xna.Framework.Input.Keys[] array = pressedKeys;
            foreach (Microsoft.Xna.Framework.Input.Keys key in array)
            {
                if (!MapPressed((int)key))
                {
                    continue;
                }
                switch (key)
                {
                    case Microsoft.Xna.Framework.Input.Keys.Back:
                        if (keyString.Length > 0)
                        {
                            keyString = keyString.Remove(keyString.Length - 1, 1);
                        }
                        continue;
                    case Microsoft.Xna.Framework.Input.Keys.Space:
                        keyString = keyString.Insert(keyString.Length, " ");
                        continue;
                    case Microsoft.Xna.Framework.Input.Keys.Enter:
                    case Microsoft.Xna.Framework.Input.Keys.Escape:
                        continue;
                }
                char c = GetCharFromKey((Keys)key);
                if (c != ' ')
                {
                    keyString += c;
                }
            }
        }
        ignoreCore = false;
        isComposing = false;
    }

    private static bool IsKeyNote(Keys pKey)
    {
        if (pKey != Keys.D1 && pKey != Keys.D2 && pKey != Keys.D3 && pKey != Keys.D4 && pKey != Keys.D5 && pKey != Keys.D6 && pKey != Keys.D7 && pKey != Keys.D8 && pKey != Keys.D9 && pKey != Keys.D0 && pKey != Keys.OemPlus && pKey != Keys.OemMinus)
        {
            return pKey == Keys.Back;
        }
        return true;
    }

    private static int KeyNote()
    {
        _usingVoiceRegister = 0;
        int reg = -1;
        if (_registerLock)
        {
            if (Down(Keys.D1))
            {
                reg = 0;
            }
            if (Down(Keys.D2))
            {
                reg = 1;
            }
            if (Down(Keys.D3))
            {
                reg = 2;
            }
            if (Down(Keys.D4))
            {
                reg = 3;
            }
            if (Down(Keys.D5))
            {
                reg = 4;
            }
            if (Down(Keys.D6))
            {
                reg = 5;
            }
            if (Down(Keys.D7))
            {
                reg = 6;
            }
            if (Down(Keys.D8))
            {
                reg = 7;
            }
            if (Down(Keys.D9))
            {
                reg = 8;
            }
            if (Down(Keys.D0))
            {
                reg = 9;
            }
            if (Down(Keys.OemMinus))
            {
                reg = 10;
            }
            if (Down(Keys.OemPlus))
            {
                reg = 11;
            }
            if (Down(Keys.Back))
            {
                reg = 12;
            }
            _usingVoiceRegister = 3;
        }
        return reg;
    }

    public static int CurrentNote(InputProfile pProfile, Thing pInstrument)
    {
        _registerSetThing = pInstrument;
        _usingVoiceRegister = 0;
        if (Input.Pressed("VOICEREG"))
        {
            _registerLock = !_registerLock;
        }
        return KeyNote();
    }

    public override bool MapPressed(int mapping, bool any = false)
    {
        if (!ignoreCore && (DevConsole.open || DuckNetwork.enteringText || Editor.enteringText))
        {
            return false;
        }
        if (!Pressed((Keys)mapping, any))
        {
            return _repeatList.Contains((Keys)mapping);
        }
        return true;
    }

    public static bool Pressed(Keys key, bool any = false)
    {
        if (_usingVoiceRegister > 0 && IsKeyNote(key))
        {
            return false;
        }
        if (Input.ignoreInput)
        {
            return false;
        }
        if (any && _keyboardPress)
        {
            return true;
        }
        if (key == Keys.Enter && ignoreEnter > 0)
        {
            return false;
        }
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
        if ((_keyState.IsKeyDown((Microsoft.Xna.Framework.Input.Keys)key) && !_keyStatePrev.IsKeyDown((Microsoft.Xna.Framework.Input.Keys)key)) || _repeatList.Contains(key))
        {
            return true;
        }
        return false;
    }

    public override bool MapReleased(int mapping)
    {
        if (!ignoreCore && (DevConsole.open || DuckNetwork.enteringText || Editor.enteringText))
        {
            return false;
        }
        return Released((Keys)mapping);
    }

    public static bool Released(Keys key)
    {
        if (_usingVoiceRegister > 0 && IsKeyNote(key))
        {
            return false;
        }
        if (Input.ignoreInput)
        {
            return false;
        }
        if (key == Keys.Enter && ignoreEnter > 0)
        {
            return false;
        }
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
        if (!_keyState.IsKeyDown((Microsoft.Xna.Framework.Input.Keys)key) && _keyStatePrev.IsKeyDown((Microsoft.Xna.Framework.Input.Keys)key))
        {
            return true;
        }
        return false;
    }

    public override bool MapDown(int mapping, bool any = false)
    {
        if (!ignoreCore && (DevConsole.open || DuckNetwork.enteringText || Editor.enteringText))
        {
            return false;
        }
        return Down((Keys)mapping);
    }

    public static bool Down(Keys key)
    {
        if (_usingVoiceRegister > 0 && IsKeyNote(key))
        {
            return false;
        }
        if (Input.ignoreInput)
        {
            return false;
        }
        if (key == Keys.Enter && ignoreEnter > 0)
        {
            return false;
        }
        if (key >= Keys.MouseKeys)
        {
            switch (key)
            {
                case Keys.MouseLeft:
                    if (Mouse.left != InputState.Down)
                    {
                        return Mouse.left == InputState.Pressed;
                    }
                    return true;
                case Keys.MouseMiddle:
                    if (Mouse.middle != InputState.Down)
                    {
                        return Mouse.middle == InputState.Pressed;
                    }
                    return true;
                case Keys.MouseRight:
                    if (Mouse.right != InputState.Down)
                    {
                        return Mouse.right == InputState.Pressed;
                    }
                    return true;
                default:
                    return false;
            }
        }
        if (_keyState.IsKeyDown((Microsoft.Xna.Framework.Input.Keys)key))
        {
            return true;
        }
        return false;
    }
}
