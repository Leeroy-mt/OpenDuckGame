using Microsoft.Xna.Framework.Graphics;
using XnaRenderTarget2D = Microsoft.Xna.Framework.Graphics.RenderTarget2D;

namespace DuckGame;

public class MaterialSunshine : AutoEffect
{
#if NO_TEX2D
    XnaRenderTarget2D _colorMap;

    public MaterialSunshine(XnaRenderTarget2D col) : base(Content.Load<MTEffect>("Shaders/sunshine"))
#else
    private RenderTarget2D _colorMap;

    public MaterialSunshine(RenderTarget2D col) : base(Content.Load<MTEffect>("Shaders/sunshine"))
#endif
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
