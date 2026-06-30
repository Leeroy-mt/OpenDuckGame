using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialRainbow : Material
{
    public float offset;

    public float offset2;

    public MaterialRainbow()
    {
        effect = Content.Load<MTEffect>("Shaders/rainbow");
    }

    public override void Apply()
    {
        effect.effect.Parameters["offset"].SetValue(offset);
        effect.effect.Parameters["offset2"].SetValue(offset2);
        foreach (EffectPass pass in effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
