namespace DuckGame;

public class UIMenuActionCloseMenuSetBoolean(UIComponent menu, MenuBoolean value) : UIMenuAction
{
    #region Private Fields

    UIComponent _menu = menu;

    MenuBoolean _value = value;

    #endregion

    public override void Activate()
    {
        _menu.Close();
        _value.value = true;
    }
}
