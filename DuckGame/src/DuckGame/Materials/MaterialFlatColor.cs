using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialFlatColor : AutoEffect
{
    public MaterialFlatColor() : base(Content.Load<MTEffect>("Shaders/flatColor")) { }

    public override void Apply()
    {
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
