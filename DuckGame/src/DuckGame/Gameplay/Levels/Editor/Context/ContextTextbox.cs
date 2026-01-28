namespace DuckGame;

public class ContextTextbox : ContextMenu
{
    private FieldBinding _field;

    public string path = "";

    private float _blink;

    private TextEntryDialog _dialog;

    public FancyBitmapFont _fancyFont;

    public ContextTextbox(string text, IContextListener owner, FieldBinding field, string valTooltip)
        : base(owner)
    {
        itemSize.X = 150f;
        itemSize.Y = 16f;
        _text = text;
        _field = field;
        if (field == null)
        {
            _field = new FieldBinding(this, "isChecked");
        }
        _fancyFont = new FancyBitmapFont("smallFont");
        tooltip = valTooltip;
    }

    public ContextTextbox(string text, IContextListener owner, FieldBinding field = null)
        : base(owner)
    {
        itemSize.X = 150f;
        itemSize.Y = 16f;
        _text = text;
        _field = field;
        if (field == null)
        {
            _field = new FieldBinding(this, "isChecked");
        }
        _fancyFont = new FancyBitmapFont("smallFont");
    }

    public override void Initialize()
    {
        _dialog = new TextEntryDialog();
        Level.Add(_dialog);
    }

    public override void Terminate()
    {
        Level.Remove(_dialog);
    }

    public override void Selected()
    {
        string val = "";
        if (_field != null && _field.value is string)
        {
            val = _field.value as string;
        }
        SFX.Play("highClick", 0.3f, 0.2f);
        if (Level.current is Editor)
        {
            _dialog.Open(_text, val, 999999);
        }
        else if (_owner != null)
        {
            _owner.Selected(this);
        }
    }

    public override void Update()
    {
        if (!_dialog.opened)
        {
            _blink += 0.04f;
            if (_blink >= 1f)
            {
                _blink = 0f;
            }
            if (_dialog.result != null)
            {
                _field.value = _dialog.result;
                _dialog.result = null;
                Editor.hasUnsavedChanges = true;
            }
            base.Update();
        }
    }

    public override void Draw()
    {
        string val = "";
        if (_field != null && _field.value is string)
        {
            val = _field.value as string;
        }
        if (_hover)
        {
            Graphics.DrawRect(Position, Position + itemSize, new Color(70, 70, 70), 0.82f);
            if (val.Length > 12)
            {
                Vec2 pos = new Vec2(base.X, base.Y);
                pos.X += itemSize.X + 4f;
                pos.Y -= 2f;
                float menuWidth = 200f;
                float menuHeight = 100f;
                Graphics.DrawString(_text, Position + new Vec2(2f, 5f), Color.White, 0.88f);
                Graphics.DrawRect(pos, pos + new Vec2(menuWidth, menuHeight), new Color(70, 70, 70), 0.83f);
                Graphics.DrawRect(pos + new Vec2(1f, 1f), pos + new Vec2(menuWidth - 1f, menuHeight - 1f), new Color(30, 30, 30), 0.84f);
                _fancyFont.Depth = 0.8f;
                _fancyFont.maxWidth = 200;
                _fancyFont.Draw(val, pos + new Vec2(4f, 4f), Color.White, 0.86f);
            }
            else
            {
                if (_blink >= 0.5f)
                {
                    val += "_";
                }
                _fancyFont.maxWidth = 200;
                _fancyFont.Draw(val, Position + new Vec2(2f, 5f), Color.White, 0.86f);
            }
        }
        else
        {
            Graphics.DrawString(_text, Position + new Vec2(2f, 5f), Color.White, 0.84f);
            if (val.Length > 12)
            {
                val = val.Substring(0, 12) + "..";
            }
            _fancyFont.Depth = 0.81f;
            _fancyFont.Draw(val, Position + new Vec2(itemSize.X - 4f - _fancyFont.GetWidth(val), 5f), Color.White, 0.84f);
        }
    }
}
