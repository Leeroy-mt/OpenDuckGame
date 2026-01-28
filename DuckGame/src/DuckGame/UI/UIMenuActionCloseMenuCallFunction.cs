using System;

namespace DuckGame;

public class UIMenuActionCloseMenuCallFunction(UIComponent menu, Action f) 
    : UIMenuAction
{
    #region Private Fields

    UIComponent _menu = menu;

    Action _function = f;

    #endregion

    public override void Activate()
    {
        _menu.Close();
        _function();
    }
}
