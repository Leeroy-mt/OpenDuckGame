namespace DuckGame;

public class UIMenuActionCloseMenu(UIComponent menu) 
    : UIMenuAction
{
    UIComponent _menu = menu;

    public override void Activate()
    {
        _menu.Close();
    }
}
