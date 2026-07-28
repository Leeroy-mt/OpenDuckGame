using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialGold : Material
{
    Texture2D _goldTexture;

    private Thing _thing;

    public MaterialGold(Thing t) : base(Content.Load<Effect>("Shaders/gold"))
    {
        _goldTexture = Content.Load<Texture2D>("bigGold");
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
