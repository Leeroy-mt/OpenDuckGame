using System;
using System.Collections;

namespace DuckGame;

public class ContextCheckBox : ContextMenu
{
    private SpriteMap _checkBox;

    private FieldBinding _field;

    public bool isChecked;

    public string path = "";

    public Type _myType;

    public ContextCheckBox(string text, IContextListener owner, FieldBinding field, Type myType, string valTooltip)
        : base(owner)
    {
        itemSize.X = 150f;
        itemSize.Y = 16f;
        _text = text;
        _field = field;
        _checkBox = new SpriteMap("Editor/checkBox", 16, 16);
        base.Depth = 0.8f;
        _myType = myType;
        if (field == null)
        {
            _field = new FieldBinding(this, "isChecked");
        }
        tooltip = valTooltip;
    }

    public ContextCheckBox(string text, IContextListener owner, FieldBinding field = null, Type myType = null)
        : base(owner)
    {
        itemSize.X = 150f;
        itemSize.Y = 16f;
        _text = text;
        _field = field;
        _checkBox = new SpriteMap("Editor/checkBox", 16, 16);
        base.Depth = 0.8f;
        _myType = myType;
        if (field == null)
        {
            _field = new FieldBinding(this, "isChecked");
        }
    }

    public override void Selected()
    {
        SFX.Play("highClick", 0.3f, 0.2f);
        if (Level.current is Editor)
        {
            if (_field == null)
            {
                return;
            }
            if (_field.value is IList)
            {
                IList list = _field.value as IList;
                if (list.Contains(_myType))
                {
                    list.Remove(_myType);
                }
                else
                {
                    list.Add(_myType);
                }
            }
            else
            {
                bool check = (bool)_field.value;
                check = !check;
                _field.value = check;
                Editor.hasUnsavedChanges = true;
            }
        }
        else if (_owner != null)
        {
            _owner.Selected(this);
        }
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Draw()
    {
        if (_hover)
        {
            Graphics.DrawRect(Position, Position + itemSize, new Color(70, 70, 70), 0.82f);
        }
        Graphics.DrawString(_text, Position + new Vec2(2f, 5f), Color.White, 0.85f);
        bool check = false;
        check = ((!(_field.value is IList)) ? ((bool)_field.value) : (_field.value as IList).Contains(_myType));
        _checkBox.Depth = 0.9f;
        _checkBox.X = base.X + itemSize.X - 16f;
        _checkBox.Y = base.Y;
        _checkBox.frame = (check ? 1 : 0);
        _checkBox.Draw();
    }
}
