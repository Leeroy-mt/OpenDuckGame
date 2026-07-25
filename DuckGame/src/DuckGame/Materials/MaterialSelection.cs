using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialSelection : AutoEffect
{
    public float fade = 1f;

    public MaterialSelection() : base(Content.Load<MTEffect>("Shaders/selection")) { }

    public override void Apply()
    {
        SetValue("fade", fade);
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
