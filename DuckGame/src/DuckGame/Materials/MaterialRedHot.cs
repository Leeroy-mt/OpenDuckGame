using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialRedHot : Material
{
    private Tex2D _goldTexture;

    private Thing _thing;

    public float intensity;

    public MaterialRedHot(Thing t)
    {
        _effect = Content.Load<MTEffect>("Shaders/redhot");
        _goldTexture = Content.Load<Tex2D>("redHot");
        _thing = t;
    }

    public override void Apply()
    {
        if (Graphics.device.Textures[0] != null)
        {
            Tex2D tex = Graphics.device.Textures[0] as Texture2D;
            SetValue("width", tex.frameWidth / (float)tex.width);
            SetValue("height", tex.frameHeight / (float)tex.height);
            SetValue("xpos", _thing.x);
            SetValue("ypos", _thing.y);
            SetValue("intensity", intensity);
        }
        Graphics.device.Textures[1] = (Texture2D)_goldTexture;
        Graphics.device.SamplerStates[1] = SamplerState.PointWrap;
        foreach (EffectPass pass in _effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
