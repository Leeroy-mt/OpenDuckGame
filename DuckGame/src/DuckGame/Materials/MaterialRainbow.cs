using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialRainbow : Material
{
    public float offset;

    public float offset2;

    public MaterialRainbow() : base(Content.Load<Effect>("Shaders/rainbow")) { }

    public override void Apply()
    {
        Parameters["offset"].SetValue(offset);
        Parameters["offset2"].SetValue(offset2);
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
