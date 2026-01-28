namespace DuckGame;

public class MessageDialogue : ContextMenu
{
    private new string _text = "";

    private string[] _description;

    public bool result;

    private BitmapFont _font;

    private ContextMenu _ownerMenu;

    public ContextMenu confirmItem;

    public string contextString;

    private bool _hoverOk;

    private bool _hoverCancel;

    public float windowYOffsetAdd;

    private float bottomRightPos;

    public bool okayOnly;

    public MessageDialogue(ContextMenu pOwnerMenu)
        : base(null)
    {
        _ownerMenu = pOwnerMenu;
    }

    public MessageDialogue()
        : base(null)
    {
    }

    public override void Initialize()
    {
        base.layer = Layer.HUD;
        base.Depth = 0.95f;
        float windowWidth = 300f;
        float windowHeight = 40f;
        Vec2 topLeft = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
        new Vec2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
        Position = topLeft + new Vec2(4f, 20f);
        itemSize = new Vec2(490f, 16f);
        _root = true;
        _font = new BitmapFont("biosFont", 8);
        _font.allowBigSprites = true;
    }

    public void Open(string text, string startingText = "", string pDescription = null)
    {
        base.opened = true;
        _text = text;
        if (pDescription == null)
        {
            _description = null;
        }
        else
        {
            _description = pDescription.Split('\n');
        }
        SFX.Play("openClick", 0.4f);
    }

    public void Close()
    {
        base.opened = false;
    }

    public override void Selected(ContextMenu item)
    {
    }

    public override void Update()
    {
        if (!base.opened)
        {
            return;
        }
        if (_opening)
        {
            _opening = false;
            _selectedIndex = 1;
        }
        if (Keyboard.Pressed(Keys.Enter))
        {
            result = true;
            CompleteDialogue();
        }
        if (Keyboard.Pressed(Keys.Escape) || Mouse.right == InputState.Pressed || Input.Pressed("CANCEL"))
        {
            result = false;
            CompleteDialogue();
        }
        float windowWidth = 300f;
        float windowHeight = 80f;
        Vec2 topLeft = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f + windowYOffsetAdd);
        Vec2 okPos = new Vec2(topLeft.X + 18f, bottomRightPos - 50f);
        Vec2 okSize = new Vec2(120f, 40f);
        float middle = (base.layer.width / 2f + windowWidth / 2f - topLeft.X) / 2f;
        if (okayOnly)
        {
            okPos = new Vec2(topLeft.X + middle - okSize.X / 2f, bottomRightPos - 50f);
        }
        Vec2 cancelPos = new Vec2(topLeft.X + 160f, bottomRightPos - 50f);
        Vec2 cancelSize = new Vec2(120f, 40f);
        Rectangle rOKButton = new Rectangle(okPos.X, okPos.Y, okSize.X, okSize.Y);
        Rectangle rCancelButton = new Rectangle(cancelPos.X, cancelPos.Y, cancelSize.X, cancelSize.Y);
        bool bTouchSelectedOK = false;
        bool bTouchSelectedCancel = false;
        if (Editor.inputMode == EditorInput.Mouse)
        {
            if (Mouse.x > okPos.X && Mouse.x < okPos.X + okSize.X && Mouse.y > okPos.Y && Mouse.y < okPos.Y + okSize.Y)
            {
                _hoverOk = true;
            }
            else
            {
                _hoverOk = false;
            }
            if (!okayOnly)
            {
                if (Mouse.x > cancelPos.X && Mouse.x < cancelPos.X + cancelSize.X && Mouse.y > cancelPos.Y && Mouse.y < cancelPos.Y + cancelSize.Y)
                {
                    _hoverCancel = true;
                }
                else
                {
                    _hoverCancel = false;
                }
            }
        }
        else if (Editor.inputMode == EditorInput.Touch)
        {
            if (!okayOnly)
            {
                if (TouchScreen.GetTap().Check(rOKButton, Layer.HUD.camera))
                {
                    _selectedIndex = 0;
                    _hoverOk = true;
                    bTouchSelectedOK = true;
                }
                else
                {
                    _hoverOk = false;
                }
                if (TouchScreen.GetTap().Check(rCancelButton, Layer.HUD.camera))
                {
                    _selectedIndex = 1;
                    _hoverCancel = true;
                    bTouchSelectedCancel = true;
                }
                else
                {
                    _hoverCancel = false;
                }
            }
        }
        else
        {
            _hoverOk = (_hoverCancel = false);
        }
        if (!_hoverOk && !_hoverCancel)
        {
            if (!Editor.tookInput && Input.Pressed("MENULEFT"))
            {
                _selectedIndex--;
            }
            else if (!Editor.tookInput && Input.Pressed("MENURIGHT"))
            {
                _selectedIndex++;
            }
            if (_selectedIndex < 0)
            {
                _selectedIndex = 0;
            }
            if (_selectedIndex > 1)
            {
                _selectedIndex = 1;
            }
            if (Editor.inputMode == EditorInput.Gamepad)
            {
                if (okayOnly)
                {
                    _hoverOk = true;
                }
                else
                {
                    _hoverOk = (_hoverCancel = false);
                    if (_selectedIndex == 0)
                    {
                        _hoverOk = true;
                    }
                    else
                    {
                        _hoverCancel = true;
                    }
                }
            }
        }
        if (!Editor.tookInput && _hoverOk && ((Editor.inputMode == EditorInput.Mouse && Mouse.left == InputState.Pressed) || Input.Pressed("SELECT") || bTouchSelectedOK))
        {
            result = true;
            CompleteDialogue();
        }
        if (!Editor.tookInput && _hoverCancel && ((Editor.inputMode == EditorInput.Mouse && Mouse.left == InputState.Pressed) || Input.Pressed("SELECT") || bTouchSelectedCancel))
        {
            result = false;
            CompleteDialogue();
        }
    }

    private void CompleteDialogue()
    {
        base.opened = false;
        Editor.tookInput = true;
        Editor.lockInput = null;
        if (_ownerMenu == null)
        {
            if (Level.current is Editor)
            {
                (Level.current as Editor).CompleteDialogue(confirmItem);
            }
        }
        else if (confirmItem != null && _ownerMenu != null && result)
        {
            _ownerMenu.Selected(confirmItem);
        }
    }

    public override void Draw()
    {
        if (!base.opened)
        {
            return;
        }
        base.Draw();
        Graphics.DrawRect(new Vec2(0f, 0f), new Vec2(Layer.HUD.width, Layer.HUD.height), Color.Black * 0.5f, base.Depth - 2);
        float windowWidth = 300f;
        float windowHeight = 80f;
        Vec2 topLeft = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f + windowYOffsetAdd);
        Vec2 bottomRight = new Vec2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f + windowYOffsetAdd);
        float middle = (bottomRight.X - topLeft.X) / 2f;
        if (_description != null)
        {
            int topOffset = 18;
            string[] description = _description;
            foreach (string obj in description)
            {
                float wide = Graphics.GetStringWidth(obj);
                Graphics.DrawString(obj, topLeft + new Vec2(middle - wide / 2f, 5 + topOffset), Color.White, base.Depth + 2);
                topOffset += 8;
                bottomRight.Y += 8f;
            }
        }
        Graphics.DrawRect(topLeft, bottomRight, new Color(70, 70, 70), base.Depth, filled: false);
        Graphics.DrawRect(topLeft, bottomRight, new Color(30, 30, 30), base.Depth - 1);
        Graphics.DrawRect(topLeft + new Vec2(4f, 20f), bottomRight + new Vec2(-4f, -4f), new Color(10, 10, 10), base.Depth + 1);
        Graphics.DrawRect(topLeft + new Vec2(2f, 2f), new Vec2(bottomRight.X - 2f, topLeft.Y + 16f), new Color(70, 70, 70), base.Depth + 1);
        float titleWidth = Graphics.GetStringWidth(_text);
        Graphics.DrawString(_text, topLeft + new Vec2(middle - titleWidth / 2f, 5f), Color.White, base.Depth + 2);
        _font.Scale = new Vec2(2f, 2f);
        if (okayOnly)
        {
            Vec2 okSize = new Vec2(120f, 40f);
            Vec2 okPos = new Vec2(base.X + middle - okSize.X / 2f - 2f, bottomRight.Y - 50f);
            Graphics.DrawRect(okPos, okPos + okSize, _hoverOk ? new Color(80, 80, 80) : new Color(30, 30, 30), base.Depth + 2);
            _font.Draw("OK", okPos.X + okSize.X / 2f - _font.GetWidth("OK") / 2f, okPos.Y + 12f, Color.White, base.Depth + 3);
        }
        else
        {
            Vec2 okPos2 = new Vec2(topLeft.X + 18f, bottomRight.Y - 50f);
            Vec2 okSize2 = new Vec2(120f, 40f);
            Graphics.DrawRect(okPos2, okPos2 + okSize2, _hoverOk ? new Color(80, 80, 80) : new Color(30, 30, 30), base.Depth + 2);
            _font.Draw("OK", okPos2.X + okSize2.X / 2f - _font.GetWidth("OK") / 2f, okPos2.Y + 12f, Color.White, base.Depth + 3);
            Vec2 cancelPos = new Vec2(topLeft.X + 160f, bottomRight.Y - 50f);
            Vec2 cancelSize = new Vec2(120f, 40f);
            Graphics.DrawRect(cancelPos, cancelPos + cancelSize, _hoverCancel ? new Color(80, 80, 80) : new Color(30, 30, 30), base.Depth + 2);
            _font.Draw("CANCEL", cancelPos.X + cancelSize.X / 2f - _font.GetWidth("CANCEL") / 2f, cancelPos.Y + 12f, Color.White, base.Depth + 3);
        }
        bottomRightPos = bottomRight.Y;
    }
}
