using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialEnergyBlade : Material
{
    Texture2D _energyTexture;

    private OldEnergyScimi _thing;

    private EnergyScimitar _thing2;

    private float _time;

    public float glow;

    public MaterialEnergyBlade(OldEnergyScimi t) : base(Content.Load<Effect>("Shaders/energyBlade"))
    {
        _energyTexture = Content.Load<Texture2D>("energyTex");
        _thing = t;
    }

    public MaterialEnergyBlade(EnergyScimitar t) : base(Content.Load<Effect>("Shaders/energyBlade"))
    {
        _energyTexture = Content.Load<Texture2D>("energyTex");
        _thing2 = t;
    }

    public override void Apply()
    {
        _time += 0.016f;
        if (Graphics.device.Textures[0] != null)
        {
            var tex = Graphics.device.Textures[0] as Texture2D;
            var frameSize = SpriteMap.GetFrameSize(tex);
            SetValue("width", frameSize.X / tex.Width);
            SetValue("height", frameSize.Y / tex.Height);
            if (_thing != null)
            {
                SetValue("xpos", _thing.X);
                SetValue("ypos", _thing.Y);
                SetValue("time", _time);
                SetValue("glow", glow);
                SetValue("bladeColor", _thing.swordColor);
            }
            else
            {
                SetValue("xpos", _thing2.X);
                SetValue("ypos", _thing2.Y);
                SetValue("time", _time);
                SetValue("glow", glow);
                SetValue("bladeColor", _thing2.swordColor);
            }
        }
        Graphics.device.Textures[1] = (Texture2D)_energyTexture;
        Graphics.device.SamplerStates[1] = SamplerState.PointWrap;
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
