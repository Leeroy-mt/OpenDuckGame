using Microsoft.Xna.Framework;

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
        base.Depth = 0.97f;
        float windowWidth = 300f;
        float windowHeight = 40f;
        Vector2 topLeft = new Vector2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
        new Vector2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
        Position = topLeft + new Vector2(4f, 20f);
        itemSize = new Vector2(490f, 16f);
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
        Vector2 vec = new Vector2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
        Vector2 bottomRight = new Vector2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
        Vector2 okPos = new Vector2(vec.X - 5f, bottomRight.Y - 100f) + new Vector2(18f, 28f);
        Vector2 okSize = new Vector2(180f, 30f);
        Vector2 cancelPos = new Vector2(vec.X + 185f, bottomRight.Y - 100f) + new Vector2(18f, 28f);
        Vector2 cancelSize = new Vector2(100f, 30f);
        Vector2 backPos = new Vector2(vec.X - 5f, bottomRight.Y - 66f) + new Vector2(18f, 28f);
        Vector2 backSize = new Vector2(290f, 30f);
        if (Mouse.x > okPos.X && Mouse.x < okPos.X + okSize.X && Mouse.y > okPos.Y && Mouse.y < okPos.Y + okSize.Y)
        {
            _hoverOk = true;
        }
        else
        {
            _hoverOk = false;
        }
        if (Mouse.x > cancelPos.X && Mouse.x < cancelPos.X + cancelSize.X && Mouse.y > cancelPos.Y && Mouse.y < cancelPos.Y + cancelSize.Y)
        {
            _hoverCancel = true;
        }
        else
        {
            _hoverCancel = false;
        }
        if (Mouse.x > backPos.X && Mouse.x < backPos.X + backSize.X && Mouse.y > backPos.Y && Mouse.y < backPos.Y + backSize.Y)
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
            Vector2 topLeft = new Vector2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
            Vector2 bottomRight = new Vector2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
            Graphics.DrawRect(topLeft, bottomRight, new Color(70, 70, 70), base.Depth, filled: false);
            Graphics.DrawRect(topLeft, bottomRight, new Color(30, 30, 30), base.Depth - 1);
            Graphics.DrawRect(topLeft + new Vector2(4f, 20f), bottomRight + new Vector2(-4f, -4f), new Color(10, 10, 10), base.Depth + 1);
            Graphics.DrawRect(topLeft + new Vector2(2f, 2f), new Vector2(bottomRight.X - 2f, topLeft.Y + 16f), new Color(70, 70, 70), base.Depth + 1);
            Graphics.DrawString(_caption, topLeft + new Vector2(5f, 5f), Color.White, base.Depth + 2);
            _fancyFont.maxWidth = 300;
            _fancyFont.Draw(_text, topLeft + new Vector2(6f, 22f), Color.White, base.Depth + 5);
            _font.Scale = new Vector2(2f, 2f);
            Vector2 okPos = new Vector2(topLeft.X - 5f, bottomRight.Y - 100f) + new Vector2(18f, 28f);
            Vector2 okSize = new Vector2(180f, 30f);
            Vector2 cancelPos = new Vector2(topLeft.X + 185f, bottomRight.Y - 100f) + new Vector2(18f, 28f);
            Vector2 cancelSize = new Vector2(100f, 30f);
            Vector2 backPos = new Vector2(topLeft.X - 5f, bottomRight.Y - 66f) + new Vector2(18f, 28f);
            Vector2 backSize = new Vector2(290f, 30f);
            Graphics.DrawRect(okPos, okPos + okSize, _hoverOk ? new Color(80, 80, 80) : new Color(30, 30, 30), base.Depth + 2);
            string okText = "LETS DO IT!";
            _font.Draw(okText, okPos.X + okSize.X / 2f - _font.GetWidth(okText) / 2f, okPos.Y + 8f, Color.White, base.Depth + 3);
            Graphics.DrawRect(cancelPos, cancelPos + cancelSize, _hoverCancel ? new Color(80, 80, 80) : new Color(30, 30, 30), base.Depth + 2);
            string cancelText = "NOPE!";
            _font.Draw(cancelText, cancelPos.X + cancelSize.X / 2f - _font.GetWidth(cancelText) / 2f, okPos.Y + 8f, Color.White, base.Depth + 3);
            Graphics.DrawRect(backPos, backPos + backSize, _hoverBack ? new Color(80, 80, 80) : new Color(30, 30, 30), base.Depth + 2);
            string backText = "CANCEL";
            _font.Draw(backText, backPos.X + backSize.X / 2f - _font.GetWidth(cancelText) / 2f - 4f, backPos.Y + 8f, Color.White, base.Depth + 3);
        }
    }
}
