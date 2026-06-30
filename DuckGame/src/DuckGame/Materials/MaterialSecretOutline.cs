using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialSecretOutline : Material
{
    public MaterialSecretOutline()
    {
        effect = Content.Load<MTEffect>("Shaders/secret_outline");
    }

    public override void Apply()
    {
        foreach (EffectPass pass in effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
