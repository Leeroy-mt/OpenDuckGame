using System.Collections.Generic;

namespace DuckGame;

public class UIControlConfig : UIMenu
{
    #region Public Fields

    public int playerSelected;

    public int inputMode;

    public int inputConfigType;

    public UIMenu _confirmMenu;

    public UIMenu _warningMenu;

    #endregion

    #region Private Fields

    static bool showWarning;

    bool _showingMenu;

    UIMenuItemToggle _configuringToggle;

    UIBox _controlBox;

    UIMenu _openOnClose;

    List<string> inputTypes = ["GAMEPAD", "KEYBOARD"];

    List<UIBox> _playerBoxes = [];

    List<DeviceInputMapping> inputMaps = [];

    List<UIControlElement> _controlElements = [];

    #endregion

    #region Public Constructors

    public UIControlConfig(UIMenu openOnClose, string title, float xpos, float ypos, float wide = -1, float high = -1, string conString = "", InputProfile conProfile = null)
        : base(title, xpos, ypos, wide, high, conString, conProfile)
    {
        _openOnClose = openOnClose;
        _ = new List<string> { "P1   ", "P2   ", "P3   ", "P4" };
        _ = new List<string> { "GAMEPAD", "KEYBOARD", "PAD + KEYS" };
        BitmapFont littleFont = new("smallBiosFontUI", 7, 5);
        UIBox box = new(vert: true, isVisible: false);
        _configuringToggle = new UIMenuItemToggle("", new UIMenuActionCallFunction(SwitchConfigType), new FieldBinding(this, "inputConfigType"), default, null, inputTypes, compressedMulti: true, tiny: true);
        box.Add(_configuringToggle);
        UIText t = new(" ", Color.White);
        _controlElements.Add(new UIControlElement("|DGBLUE|{LEFT", "LEFT", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGBLUE|/RIGHT", "RIGHT", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGBLUE|}UP", "UP", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGBLUE|~DOWN", "DOWN", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGBLUE|JUMP", "JUMP", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGBLUE|FIRE", "SHOOT", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGBLUE|GRAB", "GRAB", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGBLUE|QUACK", "QUACK", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGBLUE|STRAFE", "STRAFE", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGBLUE|RAGDOLL", "RAGDOLL", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        box.Add(new UIText(" ", Color.White, UIAlign.Center, -6));
        _controlElements.Add(new UIControlElement("|DGPURPLE|{MENU LEFT", "MENULEFT", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGPURPLE|/MENU RIGHT", "MENURIGHT", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGPURPLE|}MENU UP", "MENUUP", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGPURPLE|~MENU DOWN", "MENUDOWN", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGPURPLE|ACCEPT", "SELECT", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGPURPLE|MENU 1", "MENU1", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGPURPLE|MENU 2", "MENU2", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGPURPLE|CANCEL", "CANCEL", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGPURPLE|START", "START", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        box.Add(new UIText(" ", Color.White, UIAlign.Center, -6));
        _controlElements.Add(new UIControlElement("|DGGREEN|MOVE STICK", "LSTICK", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGGREEN|LICK STICK", "RSTICK", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGGREEN|QUACK PITCH", "LTRIGGER", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        _controlElements.Add(new UIControlElement("|DGGREEN|ZOOM   ", "RTRIGGER", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume")));
        box.Add(_controlElements[^1]);
        UIMenuItem menuItem = new("|RED|REVERT TO DEFAULT", new UIMenuActionCallFunction(ResetToDefault));
        menuItem.SetFont(littleFont);
        box.Add(menuItem);
        t = new UIText(" ", Color.White);
        t.SetFont(littleFont);
        box.Add(t);
        t = new UIText("Personal controls can be", Color.White);
        t.SetFont(littleFont);
        box.Add(t);
        t = new UIText("set in profile screen.", Color.White);
        t.SetFont(littleFont);
        box.Add(t);
        _controlBox = box;
        _playerBoxes.Add(box);
        Add(_playerBoxes[0]);
        _confirmMenu = new UIMenu("SAVE CHANGES?", Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 160, -1, "@SELECT@SELECT @CANCEL@BACK");
        _confirmMenu.Add(new UIMenuItem("YES!", new UIMenuActionCallFunction(CloseMenuSaving)));
        _confirmMenu.Add(new UIMenuItem("NO!", new UIMenuActionCallFunction(CloseMenu)));
        _confirmMenu.SetBackFunction(new UIMenuActionOpenMenu(_confirmMenu, this));
        _confirmMenu.Close();
        _warningMenu = new UIMenu("WARNING!", Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 180, -1, "@SELECT@ I see...");
        _warningMenu.Add(new UIText("", Color.White, UIAlign.Center, -3)
        {
            Scale = new Vec2(0.5f)
        });
        _warningMenu.Add(new UIText("One or more profiles have", Color.White, UIAlign.Center, -4)
        {
            Scale = new Vec2(0.5f)
        });
        _warningMenu.Add(new UIText("|DGBLUE|custom controls|PREV| defined, which will", Color.White, UIAlign.Center, -4)
        {
            Scale = new Vec2(0.5f)
        });
        _warningMenu.Add(new UIText("|DGRED|override|PREV| any controls set here!", Color.White, UIAlign.Center, -4)
        {
            Scale = new Vec2(0.5f)
        });
        _warningMenu.Add(new UIText("", Color.White, UIAlign.Center, -3)
        {
            Scale = new Vec2(0.5f)
        });
        _warningMenu.Add(new UIText("If these controls are not working,", Color.White, UIAlign.Center, -4)
        {
            Scale = new Vec2(0.5f)
        });
        _warningMenu.Add(new UIText("enter the hat console in the lobby", Color.White, UIAlign.Center, -4)
        {
            Scale = new Vec2(0.5f)
        });
        _warningMenu.Add(new UIText("and press |DGORANGE|EDIT|PREV| on your profile name.", Color.White, UIAlign.Center, -4)
        {
            Scale = new Vec2(0.5f)
        });
        _warningMenu.Add(new UIText("Select |DGORANGE|CONTROLS|PREV|, select your desired", Color.White, UIAlign.Center, -4)
        {
            Scale = new Vec2(0.5f)
        });
        _warningMenu.Add(new UIText("input device, go to |DGORANGE|PAGE 2|PREV|", Color.White, UIAlign.Center, -4)
        {
            Scale = new Vec2(0.5f)
        });
        _warningMenu.Add(new UIText("and select |DGORANGE|RESET|PREV|.", Color.White, UIAlign.Center, -4)
        {
            Scale = new Vec2(0.5f)
        });
        _warningMenu.Add(new UIText("", Color.White, UIAlign.Center, -3)
        {
            Scale = new Vec2(0.5f)
        });
        _warningMenu.SetAcceptFunction(new UIMenuActionOpenMenu(_warningMenu, this));
        _warningMenu.SetBackFunction(new UIMenuActionOpenMenu(_warningMenu, this));
        _warningMenu.Close();
    }

    #endregion

    #region Public Methods

    public static void ResetWarning()
    {
        showWarning = false;
        foreach (Profile p in Profiles.active)
        {
            if (p.inputMappingOverrides != null && p.inputMappingOverrides.Count > 0)
            {
                showWarning = true;
                break;
            }
        }
    }

    public override void Open()
    {
        SwitchPlayerProfile();
        base.Open();
    }

    public override void Update()
    {
        if (open)
        {
            if (!globalUILock && showWarning)
            {
                new UIMenuActionOpenMenu(this, _warningMenu).Activate();
                showWarning = false;
                return;
            }
            if (!globalUILock && (Input.Pressed("CANCEL") || Keyboard.Pressed(Keys.OemTilde)))
            {
                new UIMenuActionOpenMenu(this, _confirmMenu).Activate();
                return;
            }
            if (Input.uiDevicesHaveChanged)
            {
                SwitchPlayerProfile();
                Input.uiDevicesHaveChanged = false;
            }
        }
        if (_controlBox.selection > 0 && _controlBox.selection < 17)
        {
            if (!_showingMenu && inputConfigType < inputMaps.Count && inputMaps[inputConfigType].deviceName != "KEYBOARD P1" && inputMaps[inputConfigType].deviceName != "KEYBOARD P2")
            {
                HUD.AddCornerControl(HUDCorner.BottomLeft, "@MENU2@STYLE");
                _showingMenu = true;
            }
        }
        else if (_showingMenu)
        {
            HUD.CloseAllCorners();
            _showingMenu = false;
        }
        base.Update();
    }

    public void SwitchPlayerProfile()
    {
        inputTypes.Clear();
        inputMaps.Clear();
        for (int i = 0; i < 4; i++)
        {
            XInputPad pad = Input.GetDevice<XInputPad>(i);
            if (pad != null && pad.isConnected)
            {
                inputTypes.Add("XBOX GAMEPAD");
                inputMaps.Add(Input.GetDefaultMapping(pad.productName, pad.productGUID).Clone());
                break;
            }
        }
        inputTypes.Add("KEYBOARD P1");
        inputMaps.Add(Input.GetDefaultMapping("KEYBOARD P1", "").Clone());
        inputTypes.Add("KEYBOARD P2");
        inputMaps.Add(Input.GetDefaultMapping("KEYBOARD P2", "").Clone());
        inputConfigType = 0;
        SwitchConfigType();
    }

    public void SwitchConfigType()
    {
        foreach (UIControlElement e in _controlElements)
            if (inputConfigType < inputMaps.Count)
                e.inputMapping = inputMaps[inputConfigType];
    }

    public void ResetToDefault()
    {
        if (inputConfigType < inputMaps.Count)
            inputMaps[inputConfigType] = Input.GetDefaultMapping(inputMaps[inputConfigType].deviceName, inputMaps[inputConfigType].deviceGUID, presets: true).Clone();
        SwitchConfigType();
    }

    public void CloseMenu()
    {
        _showingMenu = false;
        Close();
        _openOnClose.Open();
        _confirmMenu.Close();
        _warningMenu.Close();
        inputMaps.Clear();
        HUD.CloseAllCorners();
    }

    public void CloseMenuSaving()
    {
        _showingMenu = false;
        foreach (DeviceInputMapping inputMap in inputMaps)
            Input.SetDefaultMapping(inputMap);
        Input.ApplyDefaultMappings();
        Input.Save();
        Close();
        _openOnClose.Open();
        _confirmMenu.Close();
        _warningMenu.Close();
        inputMaps.Clear();
        HUD.CloseAllCorners();
    }

    #endregion
}
