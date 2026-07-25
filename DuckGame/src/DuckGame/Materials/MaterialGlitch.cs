using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialGlitch : AutoEffect
{
#if NO_TEX2D
    Texture2D _goldTexture;
#else
    private Tex2D _goldTexture;
#endif

    private Thing _thing;

    public float amount;

    public float yoffset;

    private int lockframes;

    public MaterialGlitch(Thing t) : base(Content.Load<MTEffect>("Shaders/glitch"))
    {
#if NO_TEX2D
        _goldTexture = Content.Load<Texture2D>("glitchMap3");
#else
        _goldTexture = Content.Load<Tex2D>("glitchMap3");
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
            SetValue("frameWidth", frameSize.X);
#else
            Tex2D tex = Graphics.device.Textures[0] as Texture2D;
            SetValue("width", tex.frameWidth / tex.width);
            SetValue("height", tex.frameHeight / tex.height);
            SetValue("frameWidth", tex.frameWidth);
#endif
            SetValue("amount", amount);
            SetValue("yoff", yoffset);
            SetValue("xpos", _thing.X);
            SetValue("ypos", _thing.Y);
        }
        Graphics.device.Textures[1] = (Texture2D)_goldTexture;
        Graphics.device.SamplerStates[1] = SamplerState.PointWrap;
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
        if (lockframes > 0)
        {
            lockframes--;
            return;
        }
        if (Rando.Float(1f) > 0.85f)
        {
            lockframes = Rando.Int(2, 12);
        }
        if (Rando.Float(1f) > 0.2f)
        {
            amount = Lerp.Float(amount, 0f, 0.05f);
        }
        if (Rando.Float(1f) > 0.98f)
        {
            amount += 0.3f;
        }
    }
}
