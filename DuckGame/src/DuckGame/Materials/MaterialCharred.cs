using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialCharred : Material
{
    public MaterialCharred()
    {
        _effect = Content.Load<MTEffect>("Shaders/charred");
    }

    public override void Apply()
    {
        foreach (EffectPass pass in _effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
