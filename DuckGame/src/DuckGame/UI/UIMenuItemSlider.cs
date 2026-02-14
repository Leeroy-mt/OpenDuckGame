using Microsoft.Xna.Framework;

namespace DuckGame;

public class UIMenuItemSlider : UIMenuItem
{
    #region Private Fields

    float _step;

    FieldBinding _field;

    #endregion

    public UIMenuItemSlider(string text, UIMenuAction action = null, FieldBinding field = null, float step = 0.1f, Color c = default)
        : base(action)
    {
        if (c == default)
            c = Colors.MenuOption;
        UIDivider splitter = new(vert: true, 0);
        UIText t = new(text, c)
        {
            align = UIAlign.Left
        };
        splitter.leftSection.Add(t);
        UIProgressBar bar = new(step < 1F / 19 ? 26 : 30, 7, field, step)
        {
            align = UIAlign.Right
        };
        splitter.rightSection.Add(bar);
        rightSection.Add(splitter);
        _arrow = new UIImage("contextArrowRight")
        {
            align = UIAlign.Right,
            visible = false
        };
        leftSection.Add(_arrow);
        _field = field;
        _step = step;
        controlString = "@CANCEL@BACK @WASD@ADJUST";
    }

    public override void Activate(string trigger)
    {
        float newVal;
        if (trigger == "MENULEFT")
            newVal = Maths.Clamp((float)_field.value - _step, _field.min, _field.max);
        else
        {
            if (trigger != "MENURIGHT")
                return;

            newVal = Maths.Clamp((float)_field.value + _step, _field.min, _field.max);
        }
        if (newVal != (float)_field.value)
            SFX.Play("textLetter", 0.7f);
        _field.value = newVal;
    }
}
