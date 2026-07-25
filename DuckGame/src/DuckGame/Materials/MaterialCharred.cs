using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialCharred : Material
{
    public MaterialCharred() : base(Content.Load<Effect>("Shaders/charred")) { }

    public override void Apply()
    {
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
