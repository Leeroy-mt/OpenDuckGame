namespace DuckGame;

public class DeathmatchTestDialogue : ContextMenu
{
    private new string _text = "";

    public int result = -1;

    private BitmapFont _font;

    private FancyBitmapFont _fancyFont;

    public static bool success;

    public static bool tooSlow;

    private bool _hoverOk;

    private bool _hoverCancel;

    private bool _hoverBack;

    public static Editor currentEditor;

    private string _caption;

    public DeathmatchTestDialogue()
        : base(null)
    {
    }

    public override void Initialize()
    {
        base.layer = Layer.HUD;
        base.depth = 0.97f;
        float windowWidth = 300f;
        float windowHeight = 40f;
        Vec2 topLeft = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
        new Vec2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
        position = topLeft + new Vec2(4f, 20f);
        itemSize = new Vec2(490f, 16f);
        _root = true;
        _font = new BitmapFont("biosFont", 8);
        _fancyFont = new FancyBitmapFont("smallFont");
    }

    public void Open(string text)
    {
        tooSlow = false;
        base.opened = true;
        _text = text;
        _caption = "Deathmatch Validity Test!";
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
            result = 0;
            base.opened = false;
        }
        if (Keyboard.Pressed(Keys.Escape) || Mouse.right == InputState.Pressed)
        {
            result = 2;
            base.opened = false;
        }
        float windowWidth = 316f;
        float windowHeight = 155f;
        Vec2 vec = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
        Vec2 bottomRight = new Vec2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
        Vec2 okPos = new Vec2(vec.x - 5f, bottomRight.y - 100f) + new Vec2(18f, 28f);
        Vec2 okSize = new Vec2(180f, 30f);
        Vec2 cancelPos = new Vec2(vec.x + 185f, bottomRight.y - 100f) + new Vec2(18f, 28f);
        Vec2 cancelSize = new Vec2(100f, 30f);
        Vec2 backPos = new Vec2(vec.x - 5f, bottomRight.y - 66f) + new Vec2(18f, 28f);
        Vec2 backSize = new Vec2(290f, 30f);
        if (Mouse.x > okPos.x && Mouse.x < okPos.x + okSize.x && Mouse.y > okPos.y && Mouse.y < okPos.y + okSize.y)
        {
            _hoverOk = true;
        }
        else
        {
            _hoverOk = false;
        }
        if (Mouse.x > cancelPos.x && Mouse.x < cancelPos.x + cancelSize.x && Mouse.y > cancelPos.y && Mouse.y < cancelPos.y + cancelSize.y)
        {
            _hoverCancel = true;
        }
        else
        {
            _hoverCancel = false;
        }
        if (Mouse.x > backPos.x && Mouse.x < backPos.x + backSize.x && Mouse.y > backPos.y && Mouse.y < backPos.y + backSize.y)
        {
            _hoverBack = true;
        }
        else
        {
            _hoverBack = false;
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
            result = 0;
            base.opened = false;
            Editor.tookInput = true;
        }
        else if (!Editor.tookInput && _hoverCancel && (Mouse.left == InputState.Pressed || Input.Pressed("SELECT")))
        {
            result = 1;
            base.opened = false;
            Editor.tookInput = true;
        }
        else if (!Editor.tookInput && _hoverBack && (Mouse.left == InputState.Pressed || Input.Pressed("SELECT")))
        {
            result = 2;
            base.opened = false;
            Editor.tookInput = true;
        }
    }

    public override void Draw()
    {
        if (base.opened)
        {
            base.Draw();
            float windowWidth = 316f;
            float windowHeight = 155f;
            Vec2 topLeft = new Vec2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
            Vec2 bottomRight = new Vec2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
            Graphics.DrawRect(topLeft, bottomRight, new Color(70, 70, 70), base.depth, filled: false);
            Graphics.DrawRect(topLeft, bottomRight, new Color(30, 30, 30), base.depth - 1);
            Graphics.DrawRect(topLeft + new Vec2(4f, 20f), bottomRight + new Vec2(-4f, -4f), new Color(10, 10, 10), base.depth + 1);
            Graphics.DrawRect(topLeft + new Vec2(2f, 2f), new Vec2(bottomRight.x - 2f, topLeft.y + 16f), new Color(70, 70, 70), base.depth + 1);
            Graphics.DrawString(_caption, topLeft + new Vec2(5f, 5f), Color.White, base.depth + 2);
            _fancyFont.maxWidth = 300;
            _fancyFont.Draw(_text, topLeft + new Vec2(6f, 22f), Color.White, base.depth + 5);
            _font.scale = new Vec2(2f, 2f);
            Vec2 okPos = new Vec2(topLeft.x - 5f, bottomRight.y - 100f) + new Vec2(18f, 28f);
            Vec2 okSize = new Vec2(180f, 30f);
            Vec2 cancelPos = new Vec2(topLeft.x + 185f, bottomRight.y - 100f) + new Vec2(18f, 28f);
            Vec2 cancelSize = new Vec2(100f, 30f);
            Vec2 backPos = new Vec2(topLeft.x - 5f, bottomRight.y - 66f) + new Vec2(18f, 28f);
            Vec2 backSize = new Vec2(290f, 30f);
            Graphics.DrawRect(okPos, okPos + okSize, _hoverOk ? new Color(80, 80, 80) : new Color(30, 30, 30), base.depth + 2);
            string okText = "LETS DO IT!";
            _font.Draw(okText, okPos.x + okSize.x / 2f - _font.GetWidth(okText) / 2f, okPos.y + 8f, Color.White, base.depth + 3);
            Graphics.DrawRect(cancelPos, cancelPos + cancelSize, _hoverCancel ? new Color(80, 80, 80) : new Color(30, 30, 30), base.depth + 2);
            string cancelText = "NOPE!";
            _font.Draw(cancelText, cancelPos.x + cancelSize.x / 2f - _font.GetWidth(cancelText) / 2f, okPos.y + 8f, Color.White, base.depth + 3);
            Graphics.DrawRect(backPos, backPos + backSize, _hoverBack ? new Color(80, 80, 80) : new Color(30, 30, 30), base.depth + 2);
            string backText = "CANCEL";
            _font.Draw(backText, backPos.x + backSize.x / 2f - _font.GetWidth(cancelText) / 2f - 4f, backPos.y + 8f, Color.White, base.depth + 3);
        }
    }
}
