namespace DuckGame;

public class UIMenuActionChangeLevel(UIComponent menu, Level destination) : UIMenuAction
{
    #region Private Fields

    bool _activated;

    Level _destination = destination;

    #endregion

    #region Public Methods

    public override void Update()
    {
        if (_activated)
        {
            Graphics.fade = Lerp.Float(Graphics.fade, 0, 0.09f);
            if (Graphics.fade == 0)
                Level.current = _destination;
        }
    }

    public override void Activate()
    {
        _activated = true;
    }

    #endregion
}
