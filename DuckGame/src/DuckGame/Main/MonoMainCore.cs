using System.Collections.Generic;

namespace DuckGame;

public class MonoMainCore
{
    #region Public Fields
    public bool saidSpecial;

    public bool closeMenus;

    public bool menuOpenedThisFrame;

    public bool dontResetSelection;

    public int gachas;

    public int rareGachas;

    public float _fade = 1;

    public float _fadeAdd;

    public float _flashAdd;

    public UIComponent _pauseMenu;

    public UIMenu _confirmMenu;

    public Layer ginormoBoardLayer;

    public HashSet<IEngineUpdatable> engineUpdatables = [];

    public List<UIComponent> closeMenuUpdate = [];
    #endregion
}