using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class UIControlElement : UIMenuItem
{
    #region Public Fields

    public int randomAssIntField;

    public DeviceInputMapping inputMapping;

    #endregion

    #region Private Fields

    bool _editing;

    bool _skipStep;

    bool _selectStyle;

    int _selectionIndex;

    string _realTrigger;

    Sprite _styleBubble;

    Sprite _styleTray;

    UIText _uiText;

    List<string> _captionList = [];

    #endregion

    #region Public Properties

    public override Vector2 collisionSize
    {
        get => new(160, 2);
        set => _collisionSize = value;
    }

    public string _trigger
    {
        get
        {
            if (_realTrigger == "LSTICK" && inputMapping != null && inputMapping.device is Keyboard)
                return "CHAT";
            if (_realTrigger == "RSTICK" && inputMapping != null && inputMapping.device is Keyboard)
                return "VOICEREG";
            if (_realTrigger == "RTRIGGER" && inputMapping != null && inputMapping.device is Keyboard)
                return "PLAYERINDEX";
            return _realTrigger;
        }
        set => _realTrigger = value;
    }

    #endregion

    #region Public Constructors

    public UIControlElement(string text, string trigger, DeviceInputMapping map, UIMenuAction action = null, FieldBinding field = null, Color c = default(Color))
        : base(action)
    {
        _trigger = trigger;
        if (c == default)
            c = Colors.MenuOption;
        BitmapFont littleFont = new("smallBiosFontUI", 7, 5);
        UIDivider splitter = new(vert: true, 0);
        _uiText = new UIText(text, c);
        _uiText.SetFont(littleFont);
        _uiText.align = UIAlign.Left;
        _uiText.specialScale = 0.5f;
        splitter.leftSection.Add(_uiText);
        UIMultiToggle toggle = new(-1, -1, new FieldBinding(this, "randomAssIntField"), _captionList, compressed: true);
        toggle.SetFont(littleFont);
        toggle.align = UIAlign.Right;
        splitter.rightSection.Add(toggle);
        rightSection.Add(splitter);
        toggle.specialScale = 0.5f;
        _arrow = new UIImage("littleContextArrowRight")
        {
            Scale = new Vector2(0.5f, 0.5f),
            align = UIAlign.Right,
            visible = false
        };
        leftSection.Add(_arrow);
        _styleBubble = new Sprite("buttons/styleBubble")
        {
            Center = new Vector2(0f, 11f)
        };
        _styleTray = new Sprite("buttons/styleTray");
        _styleTray.CenterOrigin();
        inputMapping = map;
    }

    #endregion

    #region Public Methods

    public override void Update()
    {
        collisionSize = new Vector2(collisionSize.X, 2.5f);
        _captionList.Clear();
        if (!_editing)
        {
            string str = (inputMapping.device is Keyboard && (_trigger == "LSTICK" || _trigger == "RSTICK" || _trigger == "LTRIGGER" || _trigger == "RTRIGGER")) ? "|GRAY|" : ((!(_trigger == "LSTICK") && !(_trigger == "RSTICK") && !(_trigger == "LTRIGGER") && !(_trigger == "RTRIGGER")) ? "|WHITE|" : "|DGYELLOW|");
            if (_trigger == "LSTICK")
            {
                _uiText.text = "|DGGREEN|MOVE STICK";
                if (inputMapping.device is Keyboard)
                    _uiText.text = "|GRAY|MOVE STICK";
            }
            if (_trigger == "RSTICK")
            {
                _uiText.text = "|DGGREEN|LICK STICK";
                if (inputMapping.device is Keyboard)
                    _uiText.text = "|GRAY|LICK STICK";
            }
            if (_trigger == "LTRIGGER")
            {
                _uiText.text = "|DGGREEN|QUACK PITCH";
                if (inputMapping.device is Keyboard)
                    _uiText.text = "|GRAY|QUACK PITCH";
            }
            if (_trigger == "RTRIGGER")
            {
                _uiText.text = "|DGGREEN|ZOOM   ";
                if (inputMapping.device is Keyboard)
                    _uiText.text = "|GRAY|ZOOM   ";
            }
            string mapping = inputMapping.GetMappingString(_trigger);
            if (_trigger == "CHAT")
                _uiText.text = "|PINK|CHAT      ";
            if (_trigger == "VOICEREG")
                _uiText.text = "|PINK|JAM BUTTON";
            if (_trigger == "PLAYERINDEX")
            {
                _uiText.text = "|LIME|PLAYER#";
                if (inputMapping.device.productName == "KEYBOARD P1")
                    mapping = (Options.Data.keyboard1PlayerIndex + 1).ToString();
                else if (inputMapping.device.productName == "KEYBOARD P2")
                    mapping = (Options.Data.keyboard2PlayerIndex + 1).ToString();
            }
            _captionList.Add(str + mapping + "  ");
        }
        else
        {
            if (_skipStep)
            {
                _skipStep = false;
                return;
            }
            if (!_selectStyle)
            {
                _captionList.Add("_");
                if (Keyboard.Pressed(Keys.OemTilde))
                {
                    _editing = false;
                    UIMenu.globalUILock = false;
                    HUD.CloseAllCorners();
                }
                else if (inputMapping.RunMappingUpdate(_trigger))
                {
                    _editing = false;
                    UIMenu.globalUILock = false;
                    HUD.CloseAllCorners();
                    if (inputMapping.deviceName != "KEYBOARD P1" && inputMapping.deviceName != "KEYBOARD P1")
                        HUD.AddCornerControl(HUDCorner.BottomLeft, "@MENU2@STYLE");
                    return;
                }
            }
            else
            {
                bool finished = false;
                if (Input.Pressed("MENULEFT"))
                {
                    _selectionIndex--;
                    SFX.Play("textLetter", 0.7f);
                }
                if (Input.Pressed("MENURIGHT"))
                {
                    _selectionIndex++;
                    SFX.Play("textLetter", 0.7f);
                }
                if (Input.Pressed("MENUUP"))
                {
                    _selectionIndex -= 4;
                    SFX.Play("textLetter", 0.7f);
                }
                if (Input.Pressed("MENUDOWN"))
                {
                    _selectionIndex += 4;
                    SFX.Play("textLetter", 0.7f);
                }
                if (_selectionIndex < 0)
                    _selectionIndex = 0;
                if (_selectionIndex >= Input.buttonStyles.Count)
                    _selectionIndex = Input.buttonStyles.Count - 1;
                if (Input.Pressed("CANCEL"))
                {
                    finished = true;
                    SFX.Play("consoleError");
                }
                if (Input.Pressed("SELECT"))
                {
                    finished = true;
                    if (inputMapping.map.TryGetValue(_trigger, out var mapping2))
                    {
                        inputMapping.graphicMap[mapping2] = Input.buttonStyles[_selectionIndex].texture.textureName;
                        SFX.Play("consoleSelect");
                    }
                }
                if (finished)
                {
                    _editing = false;
                    _selectStyle = false;
                    UIMenu.globalUILock = false;
                    HUD.CloseAllCorners();
                    if (inputMapping.deviceName != "KEYBOARD P1" && inputMapping.deviceName != "KEYBOARD P1")
                        HUD.AddCornerControl(HUDCorner.BottomLeft, "@MENU2@STYLE");
                    return;
                }
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        if (_arrow.visible)
        {
            _styleBubble.Depth = 0.9f;
            Vector2 bubblePos = new(X + 76, Y);
            if (_selectStyle)
            {
                bubblePos = new Vector2(X + 85, Y);
                _styleBubble.flipH = true;
            }
            else
                _styleBubble.flipH = false;
            Graphics.Draw(_styleBubble, bubblePos.X, bubblePos.Y);
            if (inputMapping.map.TryGetValue(_trigger, out int value))
            {
                Sprite spr = inputMapping.GetSprite(value);
                spr ??= inputMapping.device.DoGetMapImage(inputMapping.map[_trigger], skipStyleCheck: true);
                if (spr != null)
                {
                    spr.Depth = 0.95f;
                    Graphics.Draw(spr, bubblePos.X + (_selectStyle ? (-22) : 9), bubblePos.Y - 7);
                }
            }
            if (_selectStyle)
            {
                _styleTray.Depth = 0.92f;
                Graphics.Draw(_styleTray, X + 118, Layer.HUD.camera.height / 2);
                Vector2 buttonsDraw = new(X + 90, Layer.HUD.camera.height / 2f - 80);
                int index = 0;
                foreach (Sprite buttonStyle in Input.buttonStyles)
                {
                    Vector2 drawPos = buttonsDraw + new Vector2(index % 4 * 14, index / 4 * 14);
                    buttonStyle.Depth = 0.95f;
                    buttonStyle.color = Color.White * ((index == _selectionIndex) ? 1f : 0.4f);
                    Graphics.Draw(buttonStyle, drawPos.X, drawPos.Y);
                    index++;
                }
            }
        }
        base.Draw();
    }

    public override void Activate(string trigger)
    {
        if (trigger == "MENURIGHT")
        {
            if (_trigger == "PLAYERINDEX")
            {
                if (inputMapping.device.productName == "KEYBOARD P1")
                {
                    Options.Data.keyboard1PlayerIndex++;
                    if (Options.Data.keyboard1PlayerIndex > 7)
                        Options.Data.keyboard1PlayerIndex = 0;
                }
                else if (inputMapping.device.productName == "KEYBOARD P2")
                {
                    Options.Data.keyboard2PlayerIndex++;
                    if (Options.Data.keyboard2PlayerIndex > 7)
                        Options.Data.keyboard2PlayerIndex = 0;
                }
                SFX.Play("consoleSelect");
            }
        }
        else if (trigger == "MENULEFT" && _trigger == "PLAYERINDEX")
        {
            if (inputMapping.device.productName == "KEYBOARD P1")
            {
                Options.Data.keyboard1PlayerIndex--;
                if (Options.Data.keyboard1PlayerIndex < 0)
                    Options.Data.keyboard1PlayerIndex = 7;
            }
            else if (inputMapping.device.productName == "KEYBOARD P2")
            {
                Options.Data.keyboard2PlayerIndex--;
                if (Options.Data.keyboard2PlayerIndex < 0)
                    Options.Data.keyboard2PlayerIndex = 7;
            }
            SFX.Play("consoleSelect");
        }
        if (trigger == "SELECT")
        {
            if (inputMapping.device is Keyboard && (_trigger == "LSTICK" || _trigger == "RSTICK" || _trigger == "LTRIGGER" || _trigger == "RTRIGGER"))
                SFX.Play("consoleError");
            else if (_trigger == "PLAYERINDEX")
            {
                if (inputMapping.device.productName == "KEYBOARD P1")
                {
                    Options.Data.keyboard1PlayerIndex++;
                    if (Options.Data.keyboard1PlayerIndex > 7)
                        Options.Data.keyboard1PlayerIndex = 0;
                }
                else if (inputMapping.device.productName == "KEYBOARD P2")
                {
                    Options.Data.keyboard2PlayerIndex++;
                    if (Options.Data.keyboard2PlayerIndex > 7)
                        Options.Data.keyboard2PlayerIndex = 0;
                }
                SFX.Play("consoleSelect");
            }
            else
            {
                UIMenu.globalUILock = true;
                _editing = true;
                _skipStep = true;
                SFX.Play("consoleSelect");
                HUD.CloseAllCorners();
                HUD.AddCornerControl(HUDCorner.TopLeft, "@CONSOLE@CANCEL");
            }
        }
        else
        {
            if (!(trigger == "MENU2") || !(inputMapping.deviceName != "KEYBOARD P1") || !(inputMapping.deviceName != "KEYBOARD P2"))
                return;
            _selectStyle = true;
            UIMenu.globalUILock = true;
            _editing = true;
            _skipStep = true;
            if (inputMapping.map.TryGetValue(_trigger, out var mapping))
            {
                int index = 0;
                Sprite spr = inputMapping.GetSprite(mapping);
                if (spr != null)
                {
                    foreach (Sprite s in Input.buttonStyles)
                    {
                        if (spr.texture != null && spr.texture.textureName == s.texture.textureName)
                        {
                            _selectionIndex = index;
                            break;
                        }
                        index++;
                    }
                }
            }
            HUD.CloseAllCorners();
            HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@CANCEL");
            HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@SELECT");
        }
    }

    #endregion
}
