using Microsoft.Xna.Framework;
using XnaRenderTarget2D = Microsoft.Xna.Framework.Graphics.RenderTarget2D;

namespace DuckGame;

public class GameContext
{
    public LayerCore layerCore;

    private LayerCore _oldLayerCore;

    public LevelCore levelCore;

    private LevelCore _oldLevelCore;

    public GameContext()
    {
        layerCore = new LayerCore();
        layerCore.InitializeLayers();
        levelCore = new LevelCore();
    }

    public void ApplyStates()
    {
        _oldLayerCore = Layer.core;
        Layer.core = layerCore;
        _oldLevelCore = Level.core;
        Level.core = levelCore;
    }

    public void RevertStates()
    {
        Layer.core = _oldLayerCore;
        Level.core = _oldLevelCore;
    }

    public void Update()
    {
        ApplyStates();
        Level.UpdateLevelChange();
        Level.UpdateCurrentLevel();
        RevertStates();
    }

#if NO_TEX2D
    public void Draw(XnaRenderTarget2D target = null, Camera c = null, Vector2 offset = default(Vector2))
    {
        ApplyStates();
        c.position += offset;
        if (c != null)
        {
            Level.current.camera = c;
        }
        XnaRenderTarget2D curTarget = null;
        if (target != null)
        {
            curTarget = Graphics.GetRenderTarget();
            Graphics.SetRenderTarget(target);
        }
        Level.DrawCurrentLevel();
        if (target != null)
        {
            Graphics.SetRenderTarget(curTarget);
        }
        RevertStates();
        c.position -= offset;
    }
#else
    public void Draw(RenderTarget2D target = null, Camera c = null, Vector2 offset = default(Vector2))
    {
        ApplyStates();
        c.position += offset;
        if (c != null)
        {
            Level.current.camera = c;
        }
        RenderTarget2D curTarget = null;
        if (target != null)
        {
            curTarget = Graphics.GetRenderTarget();
            Graphics.SetRenderTarget(target);
        }
        Level.DrawCurrentLevel();
        if (target != null)
        {
            Graphics.SetRenderTarget(curTarget);
        }
        RevertStates();
        c.position -= offset;
    }
#endif
}
