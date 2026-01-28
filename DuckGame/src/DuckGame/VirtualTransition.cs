namespace DuckGame;

public class VirtualTransition
{
    static VirtualTransitionCore _core = new();

    #region Public Properties

    public static bool doingVirtualTransition => _core.doingVirtualTransition;

    public static bool isVirtual => _core._virtualMode;

    public static bool active => _core.active;

    public static VirtualTransitionCore core
    {
        get => _core;
        set => _core = value;
    }

    #endregion

    #region Public Methods

    public static void Initialize()
    {
        _core.Initialize();
    }

    public static void Update()
    {
        _core.Update();
    }

    public static void Draw()
    {
        _core.Draw();
    }

    public static void GoVirtual()
    {
        _core.GoVirtual();
    }

    public static void GoUnVirtual()
    {
        _core.GoUnVirtual();
    }

    #endregion
}
