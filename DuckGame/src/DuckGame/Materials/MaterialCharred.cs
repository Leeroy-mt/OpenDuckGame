using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialCharred : Material
{
    public MaterialCharred()
    {
        effect = Content.Load<MTEffect>("Shaders/charred");
    }

    public override void Apply()
    {
        foreach (EffectPass pass in effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
