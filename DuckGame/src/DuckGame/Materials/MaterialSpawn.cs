using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialSpawn : Material
{
    public MaterialSpawn()
    {
        _effect = Content.Load<MTEffect>("Shaders/wireframeTex");
    }

    public override void Apply()
    {
        if (Graphics.device.Textures[0] != null)
        {
            _ = (Tex2D)(Graphics.device.Textures[0] as Texture2D);
        }
        base.effect.effect.Parameters["screenCross"].SetValue(0.5f);
        base.effect.effect.Parameters["scanMul"].SetValue(1f);
        foreach (EffectPass pass in _effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
