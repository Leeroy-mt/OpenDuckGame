using Microsoft.Xna.Framework;

namespace DuckGame;

public class UINumber(float wide, float high, FieldBinding field, string append = "", FieldBinding filterField = null, MatchSetting setting = null)
    : UIText("0", Color.White)
{
    #region Private Fields

    string _append = append;

    FieldBinding _field = field;

    FieldBinding _filterField = filterField;

    MatchSetting _setting = setting;

    #endregion

    #region Public Methods

    public override void Draw()
    {
        if (_field != null)
        {
            string prepend = "";
            if (_setting != null && _filterField != null)
            {
                if (_setting.filterMode == FilterMode.GreaterThan)
                    prepend = ">=";
                else if (_setting.filterMode == FilterMode.LessThan)
                    prepend = "<=";
            }
            if (_setting != null && _field.value is int i && i == _setting.min && _setting.minString != null)
                _text = _setting.minString;
            else
            {
                _text = prepend + Change.ToString((int)_field.value) + _append;
                if (_filterField != null && !(bool)_filterField.value)
                    _text = "ANY";
            }
        }
        base.Draw();
    }

    #endregion
}
