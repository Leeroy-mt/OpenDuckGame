using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialFlatColor : Material
{
    public MaterialFlatColor() : base(Content.Load<Effect>("Shaders/flatColor")) { }

    public override void Apply()
    {
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
