using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialSecretOutline : Material
{
    public MaterialSecretOutline() : base(Content.Load<Effect>("Shaders/secret_outline")) { }

    public override void Apply()
    {
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
