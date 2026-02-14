using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class UIMenuItemStringPicker : UIMenuItem
{
    #region Public Fields

    public bool needsApply;

    public string currentValue;

    public Action<string> selectAction;

    #endregion

    #region Protected Fields

    protected bool _useBaseActivationLogic;

    protected int currentIndex;

    protected FieldBinding _field;

    protected UIText _textItem;

    protected List<string> _valueStrings;

    #endregion

    public UIMenuItemStringPicker(string text, List<string> valStrings, FieldBinding field, UIMenuAction action = null, Color c = default)
        : base(action)
    {
        if (c == default)
            c = Colors.MenuOption;
        _valueStrings = valStrings;
        UIDivider splitter = new(vert: true, _valueStrings != null ? 0 : 0.8f);
        UIText t = new(text, c)
        {
            align = UIAlign.Left
        };
        splitter.leftSection.Add(t);
        currentIndex = _valueStrings.IndexOf(field.value as string);
        if (currentIndex < 0)
            currentIndex = 0;
        _textItem = new UIChangingText(-1, -1, field, null);
        string longest = "";
        foreach (string s in valStrings)
            if (s.Length > longest.Length)
                longest = s;
        (_textItem as UIChangingText).defaultSizeString = longest;
        _textItem.minLength = longest.Length;
        _textItem.text = _valueStrings[currentIndex];
        _textItem.align = UIAlign.Right;
        splitter.rightSection.Add(_textItem);
        rightSection.Add(splitter);
        _arrow = new UIImage("contextArrowRight")
        {
            align = UIAlign.Right,
            visible = false
        };
        leftSection.Add(_arrow);
        _field = field;
        currentValue = _field.value as string;
        controlString = "@WASD@ADJUST @SELECT@APPLY";
    }

    #region Public Fields

    public override void Activate(string trigger)
    {
        if (_useBaseActivationLogic)
        {
            base.Activate(trigger);
            return;
        }
        if (trigger == "SELECT")
        {
            selectAction?.Invoke(currentValue);
            _field.value = currentValue;
            return;
        }
        int num = currentIndex;
        if (trigger == "MENULEFT")
            currentIndex--;
        else if (trigger == "MENURIGHT")
            currentIndex++;
        if (currentIndex < 0)
            currentIndex = 0;
        if (currentIndex > _valueStrings.Count - 1)
            currentIndex = _valueStrings.Count - 1;
        currentValue = _valueStrings[currentIndex];
        if (num != currentIndex)
            SFX.Play("textLetter", 0.7f);
        _textItem?.text = _valueStrings[currentIndex];
    }

    public override void Draw()
    {
        if (!selected)
            currentValue = _field.value as string;
        currentIndex = _valueStrings.IndexOf(currentValue);
        if (currentIndex < 0)
            currentIndex = 0;
        if (!_textItem.text.Contains(_valueStrings[currentIndex]))
            _textItem.text = _valueStrings[currentIndex];
        base.Draw();
    }

    #endregion
}
