using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialWiggle : Material
{
    private Sprite _sprite;

    public MaterialWiggle(Sprite t) : base(Content.Load<Effect>("Shaders/wiggle"))
    {
        _sprite = t;
    }

    public override void Apply()
    {
        if (Graphics.device.Textures[0] != null)
        {
#if !NO_TEX2D
            _ = (Tex2D)(Graphics.device.Textures[0] as Texture2D);
#endif
            SetValue("xpos", _sprite.X);
            SetValue("ypos", _sprite.Y);
        }
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
