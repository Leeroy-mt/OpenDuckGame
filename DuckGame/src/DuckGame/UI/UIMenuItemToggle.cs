using System.Collections.Generic;

namespace DuckGame;

public class UIMenuItemToggle : UIMenuItem
{
    #region Private Fields

    FieldBinding _field;

    FieldBinding _filterBinding;

    UIMultiToggle _multiToggleElement;

    List<string> _multiToggle;

    #endregion

    public UIMenuItemToggle(string text, UIMenuAction action = null, FieldBinding field = null, Color c = default, FieldBinding filterBinding = null, List<string> multi = null, bool compressedMulti = false, bool tiny = false)
        : base(action)
    {
        if (c == default)
            c = Colors.MenuOption;
        BitmapFont littleFont = null;
        if (tiny)
            littleFont = new BitmapFont("smallBiosFontUI", 7, 5);
        UIDivider splitter = new(vert: true, 0);
        if (text != "")
        {
            UIText t = new(text, c);
            if (tiny)
                t.SetFont(littleFont);
            t.align = UIAlign.Left;
            splitter.leftSection.Add(t);
        }
        if (multi != null)
        {
            _multiToggleElement = new UIMultiToggle(-1, -1, field, multi, compressedMulti)
            {
                align = compressedMulti ? UIAlign.Right : UIAlign.Right
            };
            if (text != "")
                splitter.rightSection.Add(_multiToggleElement);
            else
            {
                splitter.leftSection.Add(_multiToggleElement);
                _multiToggleElement.align = UIAlign.Left;
            }
            if (tiny)
                _multiToggleElement.SetFont(littleFont);
            _multiToggle = multi;
        }
        else
        {
            UIOnOff toggle = new(-1, -1, field, filterBinding);
            if (tiny)
                toggle.SetFont(littleFont);
            toggle.align = UIAlign.Right;
            splitter.rightSection.Add(toggle);
        }
        rightSection.Add(splitter);
        if (tiny)
            _arrow = new UIImage("littleContextArrowRight");
        else
            _arrow = new UIImage("contextArrowRight");
        _arrow.align = UIAlign.Right;
        _arrow.visible = false;
        leftSection.Add(_arrow);
        _field = field;
        _filterBinding = filterBinding;
        controlString = "@CANCEL@BACK @WASD@ADJUST";
    }

    public override void Activate(string trigger)
    {
        int values = 1;
        int currentValue;
        int minValue = _filterBinding != null ? -1 : 0;
        if (_multiToggle != null)
        {
            values = _multiToggle.Count - 1;
            currentValue = (int)_field.value;
        }
        else
            currentValue = (bool)_field.value ? 1 : 0;
        if (_filterBinding != null && !(bool)_filterBinding.value)
            currentValue = -1;
        bool modifiedSelection = false;
        switch (trigger)
        {
            case "SELECT":
            case "MENURIGHT":
                currentValue++;
                modifiedSelection = true;
                break;
            case "MENULEFT":
                currentValue--;
                modifiedSelection = true;
                break;
        }
        if (currentValue < minValue)
            currentValue = values;
        else if (currentValue > values)
            currentValue = minValue;
        if (currentValue == -1)
            _filterBinding.value = false;
        else
        {
            _filterBinding?.value = true;
            if (_multiToggle != null)
                _field.value = currentValue;
            else
                _field.value = currentValue != 0;
        }
        if (modifiedSelection)
        {
            SFX.Play("textLetter", 0.7f);
            _action?.Activate();
        }
    }

    public void SetFieldBinding(FieldBinding f)
    {
        _field = f;
        _multiToggleElement?.SetFieldBinding(f);
    }
}
