using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialAlbum : AutoEffect
{
#if NO_TEX2D
    Texture2D _albumTexture;
#else
    private Tex2D _albumTexture;
#endif

    public MaterialAlbum() : base(Content.Load<MTEffect>("Shaders/album"))
    {
#if NO_TEX2D
        _albumTexture = Content.Load<Texture2D>("playBookPageOffset");
#else
        _albumTexture = Content.Load<Tex2D>("playBookPageOffset");
#endif
    }

    public override void Apply()
    {
        Graphics.device.Textures[1] = (Texture2D)_albumTexture;
        Graphics.device.SamplerStates[1] = SamplerState.PointClamp;
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
