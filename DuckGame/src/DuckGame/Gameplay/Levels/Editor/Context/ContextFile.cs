namespace DuckGame;

public class ContextFile : ContextMenu
{
    public FieldBinding _field;

    public string path = "";

    private bool selecting;

    private new ContextFileType _type;

    public ContextFile(string text, IContextListener owner, FieldBinding field, ContextFileType type, string valTooltip)
        : base(owner)
    {
        itemSize.x = 150f;
        itemSize.y = 16f;
        _text = text;
        _field = field;
        base.depth = 0.8f;
        _type = type;
        fancy = true;
        if (field == null)
        {
            _field = new FieldBinding(this, "isChecked");
        }
        tooltip = valTooltip;
    }

    public ContextFile(string text, IContextListener owner, FieldBinding field = null, ContextFileType type = ContextFileType.Level)
        : base(owner)
    {
        itemSize.x = 150f;
        itemSize.y = 16f;
        _text = text;
        _field = field;
        base.depth = 0.8f;
        _type = type;
        fancy = true;
        if (field == null)
        {
            _field = new FieldBinding(this, "isChecked");
        }
    }

    public override void Initialize()
    {
    }

    public override void Terminate()
    {
    }

    public override void Selected()
    {
        if (_field != null && _field.value is string)
        {
            _ = _field.value;
        }
        SFX.Play("highClick", 0.3f, 0.2f);
        if (Level.current is Editor editor)
        {
            if (_type == ContextFileType.Block)
            {
                editor.fileDialog.Open(DuckFile.customBlockDirectory, DuckFile.customBlockDirectory, save: false, selectLevels: false, loadLevel: false, _type);
            }
            else if (_type == ContextFileType.Background)
            {
                editor.fileDialog.Open(DuckFile.customBackgroundDirectory, DuckFile.customBackgroundDirectory, save: false, selectLevels: false, loadLevel: false, _type);
            }
            else if (_type == ContextFileType.Platform)
            {
                editor.fileDialog.Open(DuckFile.customPlatformDirectory, DuckFile.customPlatformDirectory, save: false, selectLevels: false, loadLevel: false, _type);
            }
            else if (_type == ContextFileType.Parallax)
            {
                editor.fileDialog.Open(DuckFile.customParallaxDirectory, DuckFile.customParallaxDirectory, save: false, selectLevels: false, loadLevel: false, _type);
            }
            else if (_type == ContextFileType.ArcadeStyle || _type == ContextFileType.ArcadeAnimation)
            {
                editor.fileDialog.Open(DuckFile.customArcadeDirectory, DuckFile.customArcadeDirectory, save: false, selectLevels: false, loadLevel: false, _type);
            }
            else
            {
                editor.fileDialog.Open(Editor.initialDirectory, Editor.initialDirectory, save: false, selectLevels: false, loadLevel: false);
            }
            selecting = true;
        }
        else if (_owner != null)
        {
            _owner.Selected(this);
        }
    }

    public override void Update()
    {
        if (!(Level.current is Editor editor) || editor.fileDialog.opened)
        {
            return;
        }
        if (selecting && editor.fileDialog.result != null)
        {
            selecting = false;
            string fullPath = editor.fileDialog.rootFolder + editor.fileDialog.result;
            if (_type == ContextFileType.Level)
            {
                LevelData lev = DuckFile.LoadLevel(fullPath);
                if (lev != null)
                {
                    _field.value = lev.metaData.guid;
                }
                else
                {
                    _field.value = editor.fileDialog.result.Substring(1, editor.fileDialog.result.Length - 5);
                }
            }
            else if (editor.fileDialog.result.StartsWith("/"))
            {
                _field.value = editor.fileDialog.result.Substring(1, editor.fileDialog.result.Length - 5);
            }
            else
            {
                _field.value = editor.fileDialog.result.Substring(0, editor.fileDialog.result.Length - 4);
            }
            Editor.hasUnsavedChanges = true;
            editor.fileDialog.result = null;
        }
        if (selecting && !editor.fileDialog.opened)
        {
            selecting = false;
        }
        base.Update();
    }

    public override void Draw()
    {
        string val = "";
        if (_field != null && _field.value is string)
        {
            val = _field.value as string;
        }
        LevelData lev = Content.GetLevel(val);
        if (lev != null)
        {
            val = lev.GetPath();
        }
        if (_hover)
        {
            Graphics.DrawRect(position, position + itemSize, new Color(70, 70, 70), 0.83f);
            if (val.Length > 0)
            {
                Vec2 pos = new Vec2(base.x, base.y);
                pos.x += itemSize.x + 4f;
                pos.y -= 2f;
                int last = val.LastIndexOf("/") + 1;
                string t = val.Substring(last, val.Length - last);
                if (t.Length > 20)
                {
                    t = t.Substring(0, 20);
                }
                Graphics.DrawString(t + "...", position + new Vec2(2f, 5f), Color.White, 0.85f);
            }
            else
            {
                Graphics.DrawString("NO FILE", position + new Vec2(2f, 5f), Color.White, 0.85f);
            }
        }
        else if (val.Length > 0)
        {
            int last2 = val.LastIndexOf("/") + 1;
            string t2 = val.Substring(last2, val.Length - last2);
            if (t2.Length > 20)
            {
                t2 = t2.Substring(0, 20);
            }
            Graphics.DrawString(t2, position + new Vec2(2f, 5f), Color.LimeGreen, 0.85f);
        }
        else
        {
            Graphics.DrawString(_text, position + new Vec2(2f, 5f), Color.Red, 0.85f);
        }
    }
}
