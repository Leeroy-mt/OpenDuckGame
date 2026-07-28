using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialSpawn : Material
{
    public MaterialSpawn() : base(Content.Load<Effect>("Shaders/wireframeTex")) { }

    public override void Apply()
    {
        Parameters["screenCross"].SetValue(0.5f);
        Parameters["scanMul"].SetValue(1f);
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
