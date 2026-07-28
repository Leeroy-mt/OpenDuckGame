using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialGlitch : Material
{
    Texture2D _goldTexture;

    private Thing _thing;

    public float amount;

    public float yoffset;

    private int lockframes;

    public MaterialGlitch(Thing t) : base(Content.Load<Effect>("Shaders/glitch"))
    {
        _goldTexture = Content.Load<Texture2D>("glitchMap3");
        _thing = t;
    }

    public override void Apply()
    {
        if (Graphics.device.Textures[0] != null)
        {
            var tex = Graphics.device.Textures[0] as Texture2D;
            var frameSize = SpriteMap.GetFrameSize(tex);
            SetValue("width", frameSize.X / tex.Width);
            SetValue("height", frameSize.Y / tex.Height);
            SetValue("frameWidth", frameSize.X);
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
