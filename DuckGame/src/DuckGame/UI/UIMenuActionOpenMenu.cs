namespace DuckGame;

public class UIMenuActionOpenMenu(UIComponent menu, UIComponent open)
    : UIMenuAction
{
    #region Private Fields

    UIComponent _menu = menu;

    UIComponent _open = open;

    #endregion

    public override void Activate()
    {
        UIComponent pauseMenu = MonoMain.pauseMenu;
        _menu.Close();
        _open.Open();
        if (pauseMenu == _menu)
            MonoMain.pauseMenu = _open;
    }
}
