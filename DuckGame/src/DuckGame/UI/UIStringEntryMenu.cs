using System;
using System.Text.RegularExpressions;

namespace DuckGame;

public class UIStringEntryMenu : UIMenu
{
    #region Public Fields

    public bool _directional;

    public string password = "";

    #endregion

    #region Private Fields

    bool _cancelled = true;

    bool _numeric;

    bool wasOpen;

    int _maxLength = 24;

    int _minNumber;

    int _maxNumber;

    float blink;

    string _originalValue = "";

    FieldBinding _binding;

    #endregion

    #region Public Constructors

    public UIStringEntryMenu(bool directional, string title, FieldBinding pBinding, int pMaxLength = 24, bool pNumeric = false, int pMinNumber = int.MinValue, int pMaxNumber = int.MaxValue)
        : base(title, Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, directional ? 160 : 220, 60, directional ? "@WASD@SET @SELECT@ACCEPT" : "@ENTERKEY@ACCEPT @ESCAPEKEY@")
    {
        Add(new UIBox(0, 0, 100, 16, vert: true, isVisible: false));
        _binding = pBinding;
        _directional = directional;
        _numeric = pNumeric;
        _maxLength = pMaxLength;
        _minNumber = pMinNumber;
        _maxNumber = pMaxNumber;
    }

    #endregion

    #region Public Methods

    public override void Open()
    {
        if (_directional)
            password = "";
        _originalValue = password;
        Keyboard.keyString = password;
        _cancelled = true;
        base.Open();
    }

    public override void OnClose()
    {
        Keyboard.repeat = false;
        if (wasOpen && _cancelled)
            _binding.value = _directional ? "" : _originalValue;
        wasOpen = false;
        base.OnClose();
    }

    public override void Update()
    {
        if (open)
        {
            Input._imeAllowed = true;
            Keyboard.repeat = true;
            wasOpen = true;
            blink += 0.02f;
            if (_directional)
            {
                if (password.Length < 6)
                {
                    if (Input.Pressed("LEFT"))
                        password += "L";
                    else if (Input.Pressed("RIGHT"))
                        password += "R";
                    else if (Input.Pressed("UP"))
                        password += "U";
                    else if (Input.Pressed("DOWN"))
                        password += "D";
                }
                if (Input.Pressed("SELECT"))
                {
                    _binding.value = password;
                    _cancelled = false;
                    _backFunction.Activate();
                }
            }
            else
            {
                globalUILock = true;
                if (Keyboard.keyString.Length > _maxLength)
                    Keyboard.keyString = Keyboard.keyString[.._maxLength];
                if (_numeric)
                    Keyboard.keyString = Regex.Replace(Keyboard.keyString, "[^0-9]", "");
                InputProfile.ignoreKeyboard = true;
                password = Keyboard.keyString;
                if (Keyboard.Pressed(Keys.Enter))
                {
                    bool invalid = false;
                    if (_numeric)
                    {
                        try
                        {
                            int num = Convert.ToInt32(Keyboard.keyString);
                            if (num < _minNumber)
                            {
                                num = _minNumber;
                                invalid = true;
                            }
                            else if (num > _maxNumber)
                            {
                                num = _maxNumber;
                                invalid = true;
                            }
                            Keyboard.keyString = num.ToString();
                        }
                        catch (Exception)
                        {
                            Keyboard.keyString = "";
                            invalid = true;
                        }
                    }
                    if (!invalid)
                    {
                        globalUILock = false;
                        _binding.value = password;
                        _cancelled = false;
                        _backFunction.Activate();
                    }
                }
                else if (Keyboard.Pressed(Keys.Escape) || Input.Pressed("CANCEL"))
                {
                    globalUILock = false;
                    _cancelled = true;
                    _backFunction.Activate();
                }
            }
            InputProfile.ignoreKeyboard = false;
        }
        base.Update();
    }

    public override void Draw()
    {
        if (_directional)
            Graphics.DrawPassword(password, new Vec2(X - (password.Length * 8 / 2), Y - 6), Color.White, Depth + 10);
        else
            Graphics.DrawString(password + ((blink % 1f > 0.5f) ? "_" : ""), new Vec2(X - (password.Length * 8 / 2), Y - 6), Color.White, Depth + 10);
        base.Draw();
    }

    public void SetValue(string pValue)
    {
        password = pValue;
    }

    #endregion
}
