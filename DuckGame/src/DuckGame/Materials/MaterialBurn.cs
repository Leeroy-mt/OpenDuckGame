using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialBurn : Material
{
#if NO_TEX2D
    Texture2D _burnTexture;
#else
    private Tex2D _burnTexture;
#endif

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
#if NO_TEX2D
        _burnTexture = Content.Load<Texture2D>("burn");
#else
        _burnTexture = Content.Load<Tex2D>("burn");
#endif
        _burnVal = burnVal;
    }

    public override void Apply()
    {
        Graphics.device.Textures[1] = (Texture2D)_burnTexture;
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
        SetValue("burn", _burnVal);
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
