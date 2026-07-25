using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialSunshineBare : Material
{
    public MaterialSunshineBare() : base(Content.Load<Effect>("Shaders/baresunshine")) { }

    public override void Apply()
    {
        Graphics.device.SamplerStates[0] = SamplerState.LinearClamp;
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
