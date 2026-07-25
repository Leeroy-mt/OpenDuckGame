using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using XnaRenderTarget2D = Microsoft.Xna.Framework.Graphics.RenderTarget2D;

namespace DuckGame;

public class ConsoleScreen : Thing
{
    private AutoEffect _lcdMaterial;

#if NO_TEX2D
    XnaRenderTarget2D _realScreenTarget;
    XnaRenderTarget2D _bloomTarget;
    XnaRenderTarget2D _finalTarget;
#else
    private RenderTarget2D _realScreenTarget;
    private RenderTarget2D _bloomTarget;
    private RenderTarget2D _finalTarget;
#endif
#if !MODERN_BATCH
    private MTSpriteBatch _batch;
#else
    TriangleBatch _batch;
#endif

    private AutoEffect _blurMaterial;

    public float _darken = 1f;

    private HatSelector _selector;

    public bool _flashTransition;

    private Viewport _oldViewport;

#if NO_TEX2D
    XnaRenderTarget2D _screenTarget
#else
    private RenderTarget2D _screenTarget
#endif
    {
        get => _selector._roomEditor.fade > 0f
            ? _finalTarget
            : _realScreenTarget;
        set => _realScreenTarget = value;
    }

#if NO_TEX2D
    public XnaRenderTarget2D target => _finalTarget;
#else
    public RenderTarget2D target => _finalTarget;
#endif

    public float darken => _darken;

    public bool transitioning => _flashTransition;

    public ConsoleScreen(float xpos, float ypos, HatSelector s)
        : base(xpos, ypos)
    {
        _lcdMaterial = new AutoEffect(Content.Load<MTEffect>("Shaders/lcd"));
        _blurMaterial = new AutoEffect(Content.Load<MTEffect>("Shaders/lcdBlur"));
#if NO_TEX2D
        _screenTarget = XnaRenderTarget2D.CreateSetUpTarget(134, 86);
        _bloomTarget = XnaRenderTarget2D.CreateSetUpTarget(134, 86);
        _finalTarget = XnaRenderTarget2D.CreateSetUpTarget(536, 344);
#else
        _screenTarget = new RenderTarget2D(134, 86);
        _bloomTarget = new RenderTarget2D(134, 86);
        _finalTarget = new RenderTarget2D(536, 344);
#endif
        _batch = new(Graphics.device);
        _selector = s;
    }

    public void DoFlashTransition()
    {
        _flashTransition = true;
    }

    public void BeginDraw()
    {
        _oldViewport = Graphics.viewport;
        Graphics.SetRenderTarget(_screenTarget);
#if NO_TEX2D
        Graphics.viewport = new Viewport(0, 0, _screenTarget.Width, _screenTarget.Height);
        Graphics.Clear(Color.Black);
        Graphics.screen = _batch;
        Camera c = new Camera(3f, 4f, _screenTarget.Width, _screenTarget.Height);
        if (_selector._roomEditor.fade > 0f)
            c = new Camera(3f, 4f, _screenTarget.Width / 4, _screenTarget.Height / 4);
#else
        Graphics.viewport = new Viewport(0, 0, _screenTarget.width, _screenTarget.height);
        Graphics.Clear(Color.Black);
        Graphics.screen = _batch;
        Camera c = new Camera(3f, 4f, _screenTarget.width, _screenTarget.height);
        if (_selector._roomEditor.fade > 0f)
            c = new Camera(3f, 4f, _screenTarget.width / 4, _screenTarget.height / 4);
#endif
        _batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, null, c.getMatrix());
    }

    public void EndDraw()
    {
        _batch.End();
        if (!_flashTransition)
        {
#if NO_TEX2D
            Camera c = new Camera(0f, 0f, _screenTarget.Width, _screenTarget.Height);
            if (!(_selector._roomEditor.fade > 0f))
            {
                Graphics.SetRenderTarget(_bloomTarget);
                Graphics.viewport = new Viewport(0, 0, _bloomTarget.Width, _bloomTarget.Height);
                Graphics.screen = _batch;
                _batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, _blurMaterial, c.getMatrix());
                Graphics.Draw(_screenTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 1f);
                _batch.End();
                Graphics.SetRenderTarget(_finalTarget);
                Graphics.viewport = new Viewport(0, 0, _finalTarget.Width, _finalTarget.Height);
                c = new Camera(0f, 0f, _screenTarget.Width, _screenTarget.Height);
#else
            Camera c = new Camera(0f, 0f, _screenTarget.width, _screenTarget.height);
            if (!(_selector._roomEditor.fade > 0f))
            {
                Graphics.SetRenderTarget(_bloomTarget);
                Graphics.viewport = new Viewport(0, 0, _bloomTarget.width, _bloomTarget.height);
                Graphics.screen = _batch;
                _batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, _blurMaterial, c.getMatrix());
                Graphics.Draw(_screenTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 1f);
                _batch.End();
                Graphics.SetRenderTarget(_finalTarget);
                Graphics.viewport = new Viewport(0, 0, _finalTarget.width, _finalTarget.height);
                c = new Camera(0f, 0f, _screenTarget.width, _screenTarget.height);
#endif
                _batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, _lcdMaterial, c.getMatrix());
                Graphics.device.Textures[1] = (Texture2D)_bloomTarget;
                Graphics.device.SamplerStates[1] = SamplerState.LinearClamp;
                Graphics.Draw(_screenTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(1f, 1f), SpriteEffects.None, 0.82f);
                Graphics.material = null;
                _batch.End();
            }
        }
        Graphics.SetRenderTarget(null);
        Graphics.viewport = _oldViewport;
        Graphics.screen = null;
        Graphics.currentLayer = null;
    }

    public override void Update()
    {
        if (_flashTransition)
        {
            _darken -= 0.2f;
            if (_darken < 0.2f)
            {
                _flashTransition = false;
            }
        }
        if (!_flashTransition)
        {
            if (_darken < 1f)
            {
                _darken += 0.2f;
            }
            else
            {
                _darken = 1f;
            }
        }
    }
}
