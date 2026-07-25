using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialSunshineBare : AutoEffect
{
    public MaterialSunshineBare() : base(Content.Load<MTEffect>("Shaders/baresunshine")) { }

    public override void Apply()
    {
        Graphics.device.SamplerStates[0] = SamplerState.LinearClamp;
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
