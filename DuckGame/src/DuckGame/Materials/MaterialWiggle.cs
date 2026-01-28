using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialWiggle : Material
{
    private Sprite _sprite;

    public MaterialWiggle(Sprite t)
    {
        _effect = Content.Load<MTEffect>("Shaders/wiggle");
        _sprite = t;
    }

    public override void Apply()
    {
        if (Graphics.device.Textures[0] != null)
        {
            _ = (Tex2D)(Graphics.device.Textures[0] as Texture2D);
            SetValue("xpos", _sprite.X);
            SetValue("ypos", _sprite.Y);
        }
        foreach (EffectPass pass in _effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
