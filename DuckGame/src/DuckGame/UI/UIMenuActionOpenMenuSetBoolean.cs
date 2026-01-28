namespace DuckGame;

public class UIMenuActionOpenMenuSetBoolean(UIComponent menu, UIComponent open, MenuBoolean value) 
    : UIMenuActionOpenMenu(menu, open)
{
    MenuBoolean _value = value;

    public override void Activate()
    {
        base.Activate();
        _value.value = true;
    }
}
