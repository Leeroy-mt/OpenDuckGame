using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UIMenuItemResolution : UIMenuItem
{
    #region Public Fields

    public Resolution currentValue;

    public Action selectAction;

    #endregion

    #region Protected Fields

    protected int currentIndex;

    protected FieldBinding _field;

    protected UIText _textItem;

    protected List<Resolution> _values;

    #endregion

    #region Private Fields

    bool showAll;

    Resolution _current;

    #endregion

    public UIMenuItemResolution(string text, FieldBinding field, UIMenuAction action = null, Color c = default)
        : base(action)
    {
        if (c == default)
            c = Colors.MenuOption;
        if (MonoMain._fullScreen)
            controlString = "@WASD@ADJUST @SELECT@APPLY";
        else
            controlString = "@WASD@ADJUST @SELECT@APPLY @MENU2@ALL";
        UIDivider splitter = new(vert: true, 0);
        UIText t = new(text, c)
        {
            align = UIAlign.Left
        };
        splitter.leftSection.Add(t);
        RefreshValueList();
        currentIndex = _values.IndexOf(field.value as Resolution);
        if (currentIndex < 0)
            currentIndex = 0;
        _textItem = new UIChangingText(-1, -1, field, null);
        string longest = "";
        foreach (Resolution r in _values)
            if (r.ToShortString().Length > longest.Length)
                longest = r.ToShortString();
        (_textItem as UIChangingText).defaultSizeString = $"{longest}   ";
        _textItem.minLength = longest.Length + 3;
        _textItem.text = _values[currentIndex].ToShortString() + "   ";
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
        currentValue = _field.value as Resolution;
    }

    #region Public Methods

    public override void Activate(string trigger)
    {
        if (trigger == "SELECT")
        {
            _field.value = currentValue;
            selectAction?.Invoke();
            return;
        }
        if (trigger == "MENU2")
        {
            showAll = !showAll;
            RefreshValueList();
            SFX.Play("textLetter", 0.7f);
            currentIndex = _values.IndexOf(_field.value as Resolution);
            if (currentIndex < 0)
                currentIndex = 0;
            _textItem.text = _values[currentIndex].ToShortString();
            return;
        }
        if (trigger == "SELECT")
        {
            selectAction?.Invoke();
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
        if (currentIndex > _values.Count - 1)
            currentIndex = _values.Count - 1;
        currentValue = _values[currentIndex];
        if (num != currentIndex)
            SFX.Play("textLetter", 0.7f);
        _textItem?.text = _values[currentIndex].ToShortString();
    }

    public override void Draw()
    {
        if (Resolution.current != _current)
        {
            RefreshValueList();
            _current = Resolution.current;
        }
        if (!selected)
            currentValue = _field.value as Resolution;
        currentIndex = _values.IndexOf(currentValue);
        if (currentIndex < 0)
            currentIndex = _values.Count - 1;
        _textItem.text = _values[currentIndex].ToShortString();
        base.Draw();
    }

    #endregion

    void RefreshValueList()
    {
        if (showAll)
        {
            controlString = "@WASD@ADJUST @SELECT@APPLY @MENU2@BASIC";
            _values = Resolution.supportedDisplaySizes[Resolution.current.mode];
            return;
        }
        controlString = "@WASD@ADJUST @SELECT@APPLY @MENU2@ALL";
        if (Resolution.current.mode == ScreenMode.Windowed)
            _values = [.. Resolution.supportedDisplaySizes[Resolution.current.mode].Where(x => x.recommended || x == Resolution.current)];
        else
            _values = [.. Resolution.supportedDisplaySizes[Resolution.current.mode].Where(x => Math.Abs(x.aspect - Resolution.adapterResolution.aspect) < 0.05f || x == Resolution.current)];
    }
}
