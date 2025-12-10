namespace DuckGame;

public class UIModifierMenuItem : UIMenuItemNumber
{
    public UIModifierMenuItem(UIMenuAction action = null, UIAlign al = UIAlign.Center, Color c = default(Color))
        : base("MODIFIERS", action, null, 0, c)
    {
        _useBaseActivationLogic = true;
        controlString = null;
    }

    public override void Update()
    {
        int modifiers = 0;
        foreach (UnlockData dat in Unlocks.GetUnlocks(UnlockType.Modifier))
        {
            if (dat.enabled && dat.unlocked)
            {
                modifiers++;
            }
        }
        if (_textItem != null)
        {
            if (modifiers == 0)
            {
                _textItem.text = "NONE";
            }
            else
            {
                _textItem.text = modifiers.ToString();
            }
        }
        base.Update();
    }
}
