using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialFrozen : Material
{
    Texture2D _frozenTexture;

    private Thing _thing;

    public float intensity;

    public MaterialFrozen(Thing t) : base(Content.Load<Effect>("Shaders/frozen"))
    {
        _frozenTexture = Content.Load<Texture2D>("frozen");
        _thing = t;
    }

    public override void Apply()
    {
        if (Graphics.device.Textures[0] != null)
        {
            var tex = Graphics.device.Textures[0] as Texture2D;
            var frameSize = SpriteMap.GetFrameSize(tex);
            SetValue("width", frameSize.X / (tex.Width * 0.75f));
            SetValue("height", frameSize.Y / (tex.Height * 0.75f));
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
