using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialPlasma : AutoEffect
{
    public float offset;

    public float offset2;

    public float scroll;

    public float scroll2;

    public float gradientOffset;

    public float gradientOffset2;

    public Color color1;

    public Color color2;

    private Texture2D _gradient;

    private Texture2D _plasma2;

    public MaterialPlasma() : base(Content.Load<MTEffect>("Shaders/plasma"))
    {
        _gradient = Content.Load<Texture2D>("arcade/gradient");
        _plasma2 = Content.Load<Texture2D>("arcade/plasma2");
    }

    public override void Update()
    {
    }

    public override void Apply()
    {
        Parameters["offset"].SetValue(offset);
        Parameters["offset2"].SetValue(offset2);
        Parameters["scroll"].SetValue(scroll);
        Parameters["scroll2"].SetValue(scroll2);
        Parameters["gradientOffset"].SetValue(gradientOffset);
        Parameters["gradientOffset2"].SetValue(gradientOffset2);
        Parameters["color1"].SetValue(color1.ToVector4());
        Parameters["color2"].SetValue(color2.ToVector4());
        Graphics.device.Textures[1] = _gradient;
        Graphics.device.Textures[2] = _plasma2;
        Graphics.device.SamplerStates[1] = SamplerState.PointWrap;
        Graphics.device.SamplerStates[0] = SamplerState.PointWrap;
        Graphics.device.SamplerStates[2] = SamplerState.PointWrap;
    }
}
