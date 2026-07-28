using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialAlbum : Material
{
    Texture2D _albumTexture;

    public MaterialAlbum() : base(Content.Load<Effect>("Shaders/album"))
    {
        _albumTexture = Content.Load<Texture2D>("playBookPageOffset");
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
