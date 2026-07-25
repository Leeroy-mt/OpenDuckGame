using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialGhost : AutoEffect
{
    public MaterialGhost() : base(Content.Load<MTEffect>("Shaders/ghost")) { }

    public override void Apply()
    {
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
