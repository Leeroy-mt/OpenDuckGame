using System.Collections.Generic;

namespace DuckGame;

public class UIMenuItemNumber : UIMenuItem
{
    public List<FieldBinding> percentageGroup = [];

    #region Protected Fields

    protected bool _useBaseActivationLogic;

    protected int _step;

    protected FieldBinding _field;

    protected FieldBinding _upperBoundField;

    protected FieldBinding _lowerBoundField;

    protected FieldBinding _filterField;

    protected UIText _textItem;

    #endregion

    #region Private Fields

    MatchSetting _setting;

    List<string> _valueStrings;

    #endregion

    public UIMenuItemNumber(string text, UIMenuAction action = null, FieldBinding field = null, int step = 1, Color c = default, FieldBinding upperBoundField = null, FieldBinding lowerBoundField = null, string append = "", FieldBinding filterField = null, List<string> valStrings = null, MatchSetting setting = null)
        : base(action)
    {
        _setting = setting;
        if (c == default)
            c = Colors.MenuOption;
        _valueStrings = valStrings;
        UIDivider splitter = new(vert: true, _valueStrings != null ? 0 : 0.8f);
        UIText t = new(text, c)
        {
            align = UIAlign.Left
        };
        splitter.leftSection.Add(t);
        if (field == null)
        {
            _textItem = new UIChangingText(-1, -1, field, null)
            {
                align = UIAlign.Right
            };
            splitter.rightSection.Add(_textItem);
        }
        else if (_valueStrings != null)
        {
            if (text == "" || text == null)
            {
                splitter.leftSection.align = UIAlign.Left;
                _textItem = t;
                int newVal = (int)field.value;
                if (newVal >= 0 && newVal < _valueStrings.Count)
                    _textItem.text = _valueStrings[newVal];
            }
            else
            {
                _textItem = new UIChangingText(-1, -1, field, null);
                int newVal2 = (int)field.value;
                if (newVal2 >= 0 && newVal2 < _valueStrings.Count)
                    _textItem.text = _valueStrings[newVal2];
                _textItem.align = UIAlign.Right;
                splitter.rightSection.Add(_textItem);
            }
        }
        else
        {
            UINumber number = new(-1, -1, field, append, filterField, _setting)
            {
                align = UIAlign.Right
            };
            splitter.rightSection.Add(number);
        }
        if (_valueStrings != null)
        {
            string longest = "";
            foreach (string r in _valueStrings)
                if (r.Length > longest.Length)
                    longest = r;
            (_textItem as UIChangingText).defaultSizeString = $"{longest}   ";
            _textItem.minLength = longest.Length + 3;
            _textItem.text = _textItem.text;
        }
        rightSection.Add(splitter);
        _arrow = new UIImage("contextArrowRight")
        {
            align = UIAlign.Right,
            visible = false
        };
        leftSection.Add(_arrow);
        _field = field;
        _step = step;
        _upperBoundField = upperBoundField;
        _lowerBoundField = lowerBoundField;
        _filterField = filterField;
        controlString = "@CANCEL@BACK @WASD@ADJUST";
    }

    public override void Activate(string trigger)
    {
        if (_useBaseActivationLogic)
        {
            base.Activate(trigger);
            return;
        }
        if (_filterField != null)
        {
            if (!(bool)_filterField.value && (trigger == "MENURIGHT" || trigger == "SELECT"))
            {
                SFX.Play("textLetter", 0.7f);
                _filterField.value = true;
                _field.value = (int)_field.min;
                return;
            }
            if (!(bool)_filterField.value && trigger == "MENULEFT")
            {
                SFX.Play("textLetter", 0.7f);
                _filterField.value = true;
                _field.value = (int)_field.max;
                return;
            }
            if ((bool)_filterField.value && trigger == "MENULEFT" && (int)_field.value == _field.min)
            {
                SFX.Play("textLetter", 0.7f);
                _filterField.value = false;
                return;
            }
            if ((bool)_filterField.value && (trigger == "MENURIGHT" || trigger == "SELECT") && (int)_field.value == _field.max)
            {
                SFX.Play("textLetter", 0.7f);
                _filterField.value = false;
                return;
            }
            if (_setting != null && trigger == "MENU2")
            {
                SFX.Play("textLetter", 0.7f);
                if (_setting.filterMode == FilterMode.GreaterThan)
                    _setting.filterMode = FilterMode.Equal;
                else if (_setting.filterMode == FilterMode.Equal)
                    _setting.filterMode = FilterMode.LessThan;
                else if (_setting.filterMode == FilterMode.LessThan)
                    _setting.filterMode = FilterMode.GreaterThan;
                return;
            }
        }
        int prev = (int)_field.value;
        switch (trigger)
        {
            case "MENULEFT":
                _field.value = (int)_field.value - GetStep((int)_field.value, up: false);
                break;
            case "MENURIGHT":
            case "SELECT":
                _field.value = (int)_field.value + GetStep((int)_field.value, up: true);
                break;
        }
        int newVal = (int)Maths.Clamp((int)_field.value, _field.min, _field.max);
        if (_upperBoundField != null && newVal > (int)_upperBoundField.value)
            _upperBoundField.value = newVal;
        if (_lowerBoundField != null && newVal < (int)_lowerBoundField.value)
            _lowerBoundField.value = newVal;
        if (prev != newVal && _action != null)
            _action.Activate();
        if (prev != (int)_field.value)
            SFX.Play("textLetter", 0.7f);
        int dif = newVal - prev;
        _field.value = newVal;
        if (dif > 0)
        {
            int totalPercent = dif;
            foreach (FieldBinding p in percentageGroup)
                while ((int)p.value > p.min && totalPercent > 0)
                {
                    int newPVal = (int)p.value;
                    newPVal -= (int)p.inc;
                    p.value = newPVal;
                    totalPercent -= (int)p.inc;
                }
        }
        else if (dif < 0)
        {
            int totalPercent2 = dif;
            foreach (FieldBinding p2 in percentageGroup)
                while ((int)p2.value < p2.max && totalPercent2 < 0)
                {
                    int newPVal2 = (int)p2.value;
                    newPVal2 += (int)p2.inc;
                    p2.value = newPVal2;
                    totalPercent2 += (int)p2.inc;
                }
        }
        if (_textItem != null && newVal >= 0 && newVal < _valueStrings.Count)
            _textItem.text = _valueStrings[newVal];
    }

    int GetStep(int current, bool up)
    {
        if (_setting == null || _setting.stepMap == null)
            return _step;
        int step = 0;
        foreach (KeyValuePair<int, int> pair in _setting.stepMap)
        {
            step = pair.Value;
            if ((up && pair.Key > current) || (!up && pair.Key >= current))
                break;
        }
        return step;
    }
}
