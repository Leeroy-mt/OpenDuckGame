using System;

namespace DuckGame;

public class UIMenuActionOpenMenuCallFunction(UIComponent menu, UIComponent open, Action f) 
    : UIMenuAction
{
    #region Private Fields

    UIComponent _menu = menu;

    UIComponent _open = open;

    Action _function = f;

    #endregion

    public override void Activate()
    {
        _menu.Close();
        _open.Open();
        if (MonoMain.pauseMenu == _menu || (MonoMain.pauseMenu != null && MonoMain.pauseMenu.GetType() != typeof(UIComponent)) || MonoMain.pauseMenu == null)
            MonoMain.pauseMenu = _open;
        _function();
    }
}
