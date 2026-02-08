using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class ContextRadio : ContextMenu
{
    private SpriteMap _radioButton;

    private FieldBinding _field;

    private object _index = 0;

    private bool _selected;

    public ContextRadio(string text, bool selected, object index, IContextListener owner, FieldBinding field = null)
        : base(owner)
    {
        itemSize.X = 150f;
        itemSize.Y = 16f;
        _text = text;
        if (field != null)
        {
            _selected = field.value == index;
        }
        else
        {
            _selected = selected;
        }
        _field = field;
        _index = index;
        base.Depth = 0.8f;
        _radioButton = new SpriteMap("Editor/radioButton", 16, 16);
        itemSize.X = Graphics.GetFancyStringWidth(_text) + 40f;
        if (index != null && index is Type)
        {
            _image = Editor.GetThing(index as Type).GeneratePreview(16, 16, transparentBack: true);
            itemSize.X += 32f;
        }
    }

    public override void Selected()
    {
        if (greyOut)
        {
            return;
        }
        SFX.Play("highClick", 0.3f, 0.2f);
        if (Level.current is Editor)
        {
            if (_field != null)
            {
                _field.value = _index;
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
        if (_field != null)
        {
            if (_index == null)
            {
                _selected = _field.value == null;
            }
            else if (_field.value != null)
            {
                _selected = _field.value.Equals(_index);
            }
            else
            {
                _selected = false;
            }
        }
    }

    public override void Draw()
    {
        if (_hover && !greyOut)
        {
            Graphics.DrawRect(Position, Position + itemSize, new Color(70, 70, 70), 0.83f);
        }
        Color c = Color.White;
        if (greyOut)
        {
            c = Color.White * 0.3f;
        }
        if (_image != null)
        {
            Graphics.DrawString(_text, Position + new Vector2(20f, 5f), c, 0.85f);
            _image.Depth = base.Depth + 3;
            _image.X = base.X + 1f;
            _image.Y = base.Y;
            _image.color = c;
            _image.Scale = new Vector2(1f);
            _image.Draw();
        }
        else
        {
            Graphics.DrawString(_text, Position + new Vector2(4f, 5f), c, 0.85f);
        }
        _radioButton.Depth = 0.9f;
        _radioButton.X = base.X + itemSize.X - 16f;
        _radioButton.Y = base.Y;
        _radioButton.frame = (_selected ? 1 : 0);
        _radioButton.color = c;
        _radioButton.Draw();
    }
}
