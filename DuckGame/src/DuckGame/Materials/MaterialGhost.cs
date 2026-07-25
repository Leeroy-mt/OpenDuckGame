using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialGhost : Material
{
    public MaterialGhost() : base(Content.Load<Effect>("Shaders/ghost")) { }

    public override void Apply()
    {
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
