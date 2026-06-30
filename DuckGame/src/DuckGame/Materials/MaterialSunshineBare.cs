using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialSunshineBare : Material
{
    public MaterialSunshineBare()
    {
        effect = Content.Load<MTEffect>("Shaders/baresunshine");
    }

    public override void Apply()
    {
        Graphics.device.SamplerStates[0] = SamplerState.LinearClamp;
        foreach (EffectPass pass in effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
