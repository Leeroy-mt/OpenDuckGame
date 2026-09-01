using Microsoft.Xna.Framework;
using System;
using System.IO;

#if FACEPUNCH
using Steamworks;
#else
using Steam;
#endif

namespace DuckGame;

public class TextEntryDialog : ContextMenu
{
    private new string _text = "";

    public string result;

    public FancyBitmapFont _fancyFont;

    public float _cursorFlash;

    public bool filename;

    private MysteryTextbox _textbox;

    private string _startingText = "";

    private bool _usingOnscreenKeyboard;

    private char[] invalidPathChars;

    private int _maxChars = 30;

    private string _default = "";

    public TextEntryDialog()
        : base(null)
    {
    }

    private void DoStuff(IAsyncResult r)
    {
        base.opened = false;
        Editor.PopFocus();
    }

    public override void Initialize()
    {
        if (Level.current is Editor)
        {
            base.layer = Editor.objectMenuLayer;
        }
        else
        {
            base.layer = Layer.HUD;
        }
        base.Depth = 0.95f;
        float windowWidth = 300f;
        float windowHeight = 40f;
        Vector2 topLeft = new Vector2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
        new Vector2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
        Position = topLeft + new Vector2(4f, 20f);
        itemSize = new Vector2(490f, 16f);
        _root = true;
        invalidPathChars = Path.GetInvalidPathChars();
        _fancyFont = new FancyBitmapFont("smallFont");
        _textbox = new MysteryTextbox(topLeft.X + 4f, topLeft.Y + 4f, windowWidth - 20f, windowHeight - 10f);
        _textbox.enterConfirms = true;
        _textbox.filename = true;
    }

#if FACEPUNCH
    void TextEntryComplete(bool submited)
    {
        SteamUtils.OnGamepadTextInputDismissed -= TextEntryComplete;

        if (!submited || SteamUtils.GetEnteredGamepadText() is not string pResult || pResult is "")
            result = _startingText;
        else
            result = pResult;

        opened = false;
        Editor.skipFrame = true;
        Editor.PopFocus();
        Editor.enteringText = false;
    }
#else
    void TextEntryComplete(string pResult)
    {
        DGSteam.TextEntryComplete -= TextEntryComplete;
        result = pResult;
        if (result == null || result == "")
        {
            result = _startingText;
        }
        base.opened = false;
        Editor.skipFrame = true;
        Editor.PopFocus();
        Editor.enteringText = false;
    }
#endif

    public void Open(string text, string startingText = "", int maxChars = 30)
    {
        _usingOnscreenKeyboard = false;
        _startingText = startingText;
        result = null;
        opened = true;
#if FACEPUNCH
        if (FacepunchSteam.ShowOnscreenKeyboard(multiline: false, text, startingText, maxChars))
        {
            SteamUtils.OnGamepadTextInputDismissed += TextEntryComplete;
#else
        if (DGSteam.ShowOnscreenKeyboard(multiline: false, text, startingText, maxChars))
        {
            DGSteam.TextEntryComplete += TextEntryComplete;
#endif
            _usingOnscreenKeyboard = true;
            Editor.enteringText = true;
            Editor.PushFocus(this);
            SFX.Play("openClick", 0.4f);
            return;
        }
        _text = text;
        _default = startingText;
        Keyboard.keyString = "";
        Editor.enteringText = true;
        _maxChars = maxChars;
        Editor.PushFocus(this);
        SFX.Play("openClick", 0.4f);
        _textbox.text = _default;
        _textbox._cursorPosition = _textbox.text.Length;
        _textbox.color = Color.White;
        _textbox.cursorColor = Color.White;
    }

    public void Close()
    {
        Editor.enteringText = false;
        Editor.PopFocus();
        base.opened = false;
    }

    public override void Selected(ContextMenu item)
    {
    }

    public override void Update()
    {
        if (base.opened && !_usingOnscreenKeyboard)
        {
            _textbox.Update();
            if (_textbox.confirmed)
            {
                _textbox.confirmed = false;
                result = _textbox.text;
                base.opened = false;
                Editor.skipFrame = true;
                Editor.PopFocus();
                Editor.enteringText = false;
            }
            if (Keyboard.Pressed(Keys.Escape) || Mouse.right == InputState.Pressed || Input.Pressed("CANCEL"))
            {
                result = _default;
                base.opened = false;
                Editor.PopFocus();
                Editor.skipFrame = true;
                Editor.enteringText = false;
            }
        }
    }

    public override void Draw()
    {
        if (base.opened && !_usingOnscreenKeyboard)
        {
            base.Draw();
            float windowWidth = 300f;
            float windowHeight = 72f;
            Vector2 topLeft = new Vector2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
            Vector2 bottomRight = new Vector2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
            Graphics.DrawRect(topLeft, bottomRight, new Color(70, 70, 70), base.Depth, filled: false, 0.95f);
            Graphics.DrawRect(topLeft, bottomRight, new Color(30, 30, 30), base.Depth - 1);
            Graphics.DrawRect(topLeft + new Vector2(4f, 20f), bottomRight + new Vector2(-4f, -4f), new Color(10, 10, 10), base.Depth + 1);
            Graphics.DrawRect(topLeft + new Vector2(2f, 2f), new Vector2(bottomRight.X - 2f, topLeft.Y + 16f), new Color(70, 70, 70), base.Depth + 1);
            _textbox.depth = base.Depth + 20;
            _textbox.Draw();
            Graphics.DrawString(_text, topLeft + new Vector2(5f, 5f), Color.White, base.Depth + 2);
            Graphics.DrawRect(new Vector2(bottomRight.X - 145f, bottomRight.Y), new Vector2(bottomRight.X, bottomRight.Y + 10f), new Color(70, 70, 70), base.Depth + 8);
            if (!(InputProfile.FirstProfileWithDevice.lastActiveDevice is Keyboard))
            {
                Graphics.DrawString("@SELECT@ACCEPT  @CANCEL@CANCEL", bottomRight + new Vector2(-147f, 1f), Color.White, base.Depth + 10);
            }
            else
            {
                Graphics.DrawString("@ENTERKEY@ACCEPT  @ESCAPE@CANCEL", bottomRight + new Vector2(-147f, 1f), Color.White, base.Depth + 10);
            }
        }
    }
}
