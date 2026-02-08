using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialDustSparkle : Material
{
    private Tex2D _cone;

    public Vector2 position;

    public Vector2 size;

    public float fade;

    public MaterialDustSparkle(Vector2 pos, Vector2 s, bool wide, bool lit)
    {
        _effect = Content.Load<MTEffect>("Shaders/dustsparkle");
        if (!lit)
        {
            _cone = Content.Load<Tex2D>("arcade/lightSphere");
            pos.Y += 10f;
        }
        else if (wide)
        {
            _cone = Content.Load<Tex2D>("arcade/bigLightCone");
        }
        else
        {
            _cone = Content.Load<Tex2D>("arcade/lightCone");
        }
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
        SetValue("viewMatrix", Graphics.screen.viewMatrix);
        SetValue("projMatrix", Graphics.screen.projMatrix);
        foreach (EffectPass pass in _effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
