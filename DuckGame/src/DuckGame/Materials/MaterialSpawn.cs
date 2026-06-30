using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialSpawn : Material
{
    public MaterialSpawn()
    {
        effect = Content.Load<MTEffect>("Shaders/wireframeTex");
    }

    public override void Apply()
    {
        if (Graphics.device.Textures[0] != null)
        {
            _ = (Tex2D)(Graphics.device.Textures[0] as Texture2D);
        }
        effect.effect.Parameters["screenCross"].SetValue(0.5f);
        effect.effect.Parameters["scanMul"].SetValue(1f);
        foreach (EffectPass pass in effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
