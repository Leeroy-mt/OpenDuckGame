namespace DuckGame;

public class NotifyDialogue : ContextMenu
{
    private new string _text = "";

    public bool result;

    private BitmapFont _font;

    private bool _hoverOk;

    public NotifyDialogue()
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
    }

    public void Open(string text, string startingText = "")
    {
        base.opened = true;
        _text = text;
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
            base.opened = false;
        }
        if (Keyboard.Pressed(Keys.Escape) || Mouse.right == InputState.Pressed)
        {
            result = false;
            base.opened = false;
        }
        float windowWidth = 300f;
        float windowHeight = 80f;
        Vec2 vec = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
        new Vec2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
        Vec2 okPos = vec + new Vec2(18f, 28f);
        Vec2 okSize = new Vec2(windowWidth - 40f, 40f);
        _ = vec + new Vec2(160f, 28f);
        new Vec2(120f, 40f);
        if (Mouse.x > okPos.X && Mouse.x < okPos.X + okSize.X && Mouse.y > okPos.Y && Mouse.y < okPos.Y + okSize.Y)
        {
            _hoverOk = true;
        }
        else
        {
            _hoverOk = false;
        }
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
            _hoverOk = false;
            if (_selectedIndex == 0)
            {
                _hoverOk = true;
            }
        }
        if (!Editor.tookInput && _hoverOk && (Mouse.left == InputState.Pressed || Input.Pressed("SELECT")))
        {
            result = true;
            base.opened = false;
            Editor.tookInput = true;
        }
    }

    public override void Draw()
    {
        if (base.opened)
        {
            base.Draw();
            float windowWidth = 300f;
            float windowHeight = 80f;
            Vec2 topLeft = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
            Vec2 bottomRight = new Vec2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
            Graphics.DrawRect(topLeft, bottomRight, new Color(70, 70, 70), base.Depth, filled: false);
            Graphics.DrawRect(topLeft, bottomRight, new Color(30, 30, 30), base.Depth - 1);
            Graphics.DrawRect(topLeft + new Vec2(4f, 20f), bottomRight + new Vec2(-4f, -4f), new Color(10, 10, 10), base.Depth + 1);
            Graphics.DrawRect(topLeft + new Vec2(2f, 2f), new Vec2(bottomRight.X - 2f, topLeft.Y + 16f), new Color(70, 70, 70), base.Depth + 1);
            Graphics.DrawString(_text, topLeft + new Vec2(5f, 5f), Color.White, base.Depth + 2);
            _font.Scale = new Vec2(2f, 2f);
            Vec2 okPos = topLeft + new Vec2(18f, 28f);
            Vec2 okSize = new Vec2(windowWidth - 36f, 40f);
            Graphics.DrawRect(okPos, okPos + okSize, _hoverOk ? new Color(80, 80, 80) : new Color(30, 30, 30), base.Depth + 2);
            _font.Draw("OK", okPos.X + okSize.X / 2f - _font.GetWidth("OK") / 2f, okPos.Y + 12f, Color.White, base.Depth + 3);
            Graphics.DrawString(_text, topLeft + new Vec2(5f, 5f), Color.White, base.Depth + 2);
        }
    }
}
