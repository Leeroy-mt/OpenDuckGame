using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialFlatColor : Material
{
    public MaterialFlatColor()
    {
        effect = Content.Load<MTEffect>("Shaders/flatColor");
    }

    public override void Apply()
    {
        foreach (EffectPass pass in effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
