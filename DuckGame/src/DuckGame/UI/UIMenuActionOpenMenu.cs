namespace DuckGame;

public class UIMenuActionOpenMenu : UIMenuAction
{
    private UIComponent _menu;

    private UIComponent _open;

    public UIMenuActionOpenMenu(UIComponent menu, UIComponent open)
    {
        _menu = menu;
        _open = open;
    }

    public override void Activate()
    {
        UIComponent pauseMenu = MonoMain.pauseMenu;
        _menu.Close();
        _open.Open();
        if (pauseMenu == _menu)
        {
            MonoMain.pauseMenu = _open;
        }
    }
}
