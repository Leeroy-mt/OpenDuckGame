using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialDustSparkle : Material
{
    Texture2D _cone;

    public Vector2 position;

    public Vector2 size;

    public float fade;

    public MaterialDustSparkle(Vector2 pos, Vector2 s, bool wide, bool lit) : base(Content.Load<Effect>("Shaders/dustsparkle"))
    {
        if (!lit)
        {
            _cone = Content.Load<Texture2D>("arcade/lightSphere");
            pos.Y += 10f;
        }
        else if (wide)
            _cone = Content.Load<Texture2D>("arcade/bigLightCone");
        else
            _cone = Content.Load<Texture2D>("arcade/lightCone");
        position = pos;
        size = s;
    }

    public override void Apply()
    {
        Graphics.device.Textures[1] = (Texture2D)_cone;
        Graphics.device.SamplerStates[1] = SamplerState.PointClamp;
        SetValue("topLeft", position);
        SetValue("size", size);
        SetValue("fade", Layer.Game.fade * fade);
#if !MODERN_BATCH
        SetValue("viewMatrix", Graphics.screen.viewMatrix);
        SetValue("projMatrix", Graphics.screen.projMatrix);
#else
        SetValue("viewMatrix", Graphics.screen.View);
        SetValue("projMatrix", Graphics.screen.Projection);
#endif
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
