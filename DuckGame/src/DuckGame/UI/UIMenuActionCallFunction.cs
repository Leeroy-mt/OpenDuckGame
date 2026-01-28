using System;

namespace DuckGame;

public class UIMenuActionCallFunction(Action f) 
    : UIMenuAction
{
    Action _function = f;

    public override void Activate()
    {
        _function();
    }
}
