using System;

namespace DuckGame;

public class UIMenuCustomUpdate(Action customUpdate, string title, float xpos, float ypos, float wide = -1, float high = -1, string conString = "", InputProfile conProfile = null, bool tiny = false)
    : UIMenu(title, xpos, ypos, wide, high, conString, conProfile, tiny)
{
    Action _customUpdate = customUpdate;

    public override void Update()
    {
        _customUpdate?.Invoke();
        base.Update();
    }
}
