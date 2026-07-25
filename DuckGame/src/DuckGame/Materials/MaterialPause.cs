using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialPause : AutoEffect
{
#if NO_TEX2D
    Texture2D _watermark;
#else
    private Tex2D _watermark;
#endif

    private float _fade;

    private float _scrollX;

    private float _scrollY;

    private float _rot;

    private float _rot2;

    public float dim = 0.6f;

    public float fade
    {
        get
        {
            return _fade;
        }
        set
        {
            _fade = value;
        }
    }

    public MaterialPause() : base(Content.Load<MTEffect>("Shaders/pause"))
    {
#if NO_TEX2D
        _watermark = Content.Load<Texture2D>("dc5");
#else
        _watermark = Content.Load<Tex2D>("dc5");
#endif
    }

    public override void Apply()
    {
        Graphics.device.Textures[1] = (Texture2D)_watermark;
        Graphics.device.SamplerStates[1] = SamplerState.PointWrap;
        SetValue("fade", _fade);
        SetValue("dim", dim);
        SetValue("scrollX", _scrollX);
        SetValue("scrollY", _scrollY);
        SetValue("aspect", Resolution.current.aspect);
        float scrollSpeed = 0.0003f;
        _rot += scrollSpeed;
        _rot2 += scrollSpeed;
        _scrollX = _rot;
        _scrollY = 0f - _rot2;
        foreach (EffectPass pass in CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
