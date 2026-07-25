using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialGold : Material
{
#if NO_TEX2D
    Texture2D _goldTexture;
#else
    private Tex2D _goldTexture;
#endif

    private Thing _thing;

    public MaterialGold(Thing t) : base(Content.Load<Effect>("Shaders/gold"))
    {
#if NO_TEX2D
        _goldTexture = Content.Load<Texture2D>("bigGold");
#else
        _goldTexture = Content.Load<Tex2D>("bigGold");
#endif
        _thing = t;
    }

    public override void Apply()
    {
        if (Graphics.device.Textures[0] != null)
        {
#if NO_TEX2D
            var tex = Graphics.device.Textures[0] as Texture2D;
            var frameSize = SpriteMap.GetFrameSize(tex);
            SetValue("width", frameSize.X / tex.Width);
            SetValue("height", frameSize.Y / tex.Height);
#else
            Tex2D tex = Graphics.device.Textures[0] as Texture2D;
            SetValue("width", tex.frameWidth / tex.width);
            SetValue("height", tex.frameHeight / tex.height);
#endif
            SetValue("xpos", _thing.X);
            SetValue("ypos", _thing.Y);
        }
        Graphics.device.Textures[1] = (Texture2D)_goldTexture;
        Graphics.device.SamplerStates[1] = SamplerState.PointWrap;
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
