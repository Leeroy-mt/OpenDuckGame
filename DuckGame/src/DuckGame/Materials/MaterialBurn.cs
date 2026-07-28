using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialBurn : Material
{
    Texture2D _burnTexture;

    private float _burnVal;

    public float burnVal
    {
        get
        {
            return _burnVal;
        }
        set
        {
            _burnVal = value;
        }
    }

    public MaterialBurn(float burnVal = 0f) : base(Content.Load<Effect>("Shaders/burn"))
    {
        _burnTexture = Content.Load<Texture2D>("burn");
        _burnVal = burnVal;
    }

    public override void Apply()
    {
        Graphics.device.Textures[1] = (Texture2D)_burnTexture;
        var tex = Graphics.device.Textures[0] as Texture2D;
        var frameSize = SpriteMap.GetFrameSize(tex);
        SetValue("width", frameSize.X / tex.Width);
        SetValue("height", frameSize.Y / tex.Height);
        SetValue("burn", _burnVal);
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
