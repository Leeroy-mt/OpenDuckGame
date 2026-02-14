using Microsoft.Xna.Framework;

namespace DuckGame;

public class UICustomLevelMenu : UIMenuItemNumber
{
    #region Public Constructors

    public UICustomLevelMenu(UIMenuAction action = null, UIAlign al = UIAlign.Center, Color c = default)
        : base("CUSTOM LEVELS", action, null, 0, c)
    {
        _useBaseActivationLogic = true;
        controlString = null;
    }

    #endregion

    #region Public Methods

    public override void Update()
    {
        int modifiers = 0;
        foreach (string activatedLevel in Editor.activatedLevels)
        {
            _ = activatedLevel;
            modifiers++;
        }
        if (_textItem != null)
        {
            if (modifiers == 0)
                _textItem.text = "NONE";
            else
                _textItem.text = modifiers.ToString();
        }
        base.Update();
    }

    #endregion
}
