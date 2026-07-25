using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialSecretOutline : AutoEffect
{
    public MaterialSecretOutline() : base(Content.Load<MTEffect>("Shaders/secret_outline")) { }

    public override void Apply()
    {
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
