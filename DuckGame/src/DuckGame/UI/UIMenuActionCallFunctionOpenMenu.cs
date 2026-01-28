using System;

namespace DuckGame;

public class UIMenuActionCallFunctionOpenMenu(UIComponent menu, UIComponent open, Action f) : UIMenuAction
{
    #region Private Fields

    UIComponent _menu = menu;

    UIComponent _open = open;

    Action _function = f;

    #endregion

    public override void Activate()
    {
        _function();
        _menu.Close();
        _open.Open();
        if (MonoMain.pauseMenu == _menu || (MonoMain.pauseMenu != null && MonoMain.pauseMenu.GetType() != typeof(UIComponent)) || MonoMain.pauseMenu == null)
            MonoMain.pauseMenu = _open;
    }
}
