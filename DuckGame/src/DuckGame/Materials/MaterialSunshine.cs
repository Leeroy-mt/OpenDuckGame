using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialSunshine : Material
{
    RenderTarget2D _colorMap;

    public MaterialSunshine(RenderTarget2D col) : base(Content.Load<Effect>("Shaders/sunshine"))
    {
        _colorMap = col;
    }

    public override void Apply()
    {
        Graphics.device.Textures[1] = (Texture2D)_colorMap;
        Graphics.device.SamplerStates[1] = SamplerState.PointClamp;
        Graphics.device.SamplerStates[0] = SamplerState.PointClamp;
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
