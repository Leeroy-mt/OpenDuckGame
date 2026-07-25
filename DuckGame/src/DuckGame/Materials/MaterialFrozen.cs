using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialFrozen : AutoEffect
{
#if NO_TEX2D
    Texture2D _frozenTexture;
#else
    private Tex2D _frozenTexture;
#endif

    private Thing _thing;

    public float intensity;

    public MaterialFrozen(Thing t) : base(Content.Load<MTEffect>("Shaders/frozen"))
    {
#if NO_TEX2D
        _frozenTexture = Content.Load<Texture2D>("frozen");
#else
        _frozenTexture = Content.Load<Tex2D>("frozen");
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
            SetValue("width", frameSize.X / (tex.Width * 0.75f));
            SetValue("height", frameSize.Y / (tex.Height * 0.75f));
#else
            Tex2D tex = Graphics.device.Textures[0] as Texture2D;
            SetValue("width", tex.frameWidth / ((float)tex.width * 0.75f));
            SetValue("height", tex.frameHeight / ((float)tex.height * 0.75f));
#endif
            SetValue("xpos", _thing.X);
            SetValue("ypos", _thing.Y);
            SetValue("intensity", intensity);
        }
        Graphics.device.Textures[1] = (Texture2D)_frozenTexture;
        Graphics.device.SamplerStates[1] = SamplerState.PointWrap;
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
