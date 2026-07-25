using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialCharred : AutoEffect
{
    public MaterialCharred() : base(Content.Load<MTEffect>("Shaders/charred")) { }

    public override void Apply()
    {
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
