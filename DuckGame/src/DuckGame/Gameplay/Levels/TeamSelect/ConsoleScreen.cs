using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DuckGame;

public class ConsoleScreen : Thing
{
    private Effect _lcdMaterial;

    RenderTarget2D _realScreenTarget;
    RenderTarget2D _bloomTarget;
    RenderTarget2D _finalTarget;

#if !MODERN_BATCH
    private MTSpriteBatch _batch;
#else
    TriangleBatch _batch;
#endif

    private Effect _blurMaterial;

    public float _darken = 1f;

    private HatSelector _selector;

    public bool _flashTransition;

    private Viewport _oldViewport;

    RenderTarget2D _screenTarget
    {
        get => _selector._roomEditor.fade > 0f
            ? _finalTarget
            : _realScreenTarget;
        set => _realScreenTarget = value;
    }

    public RenderTarget2D target => _finalTarget;

    public float darken => _darken;

    public bool transitioning => _flashTransition;

    public ConsoleScreen(float xpos, float ypos, HatSelector s)
        : base(xpos, ypos)
    {
        _lcdMaterial = Content.Load<Effect>("Shaders/lcd");
        _blurMaterial = Content.Load<Effect>("Shaders/lcdBlur");
        _screenTarget = RenderTarget2D.CreateSetUpTarget(134, 86);
        _bloomTarget = RenderTarget2D.CreateSetUpTarget(134, 86);
        _finalTarget = RenderTarget2D.CreateSetUpTarget(536, 344);
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
        Graphics.viewport = new Viewport(0, 0, _screenTarget.Width, _screenTarget.Height);
        Graphics.Clear(Color.Black);
        Graphics.screen = _batch;
        Camera c = new Camera(3f, 4f, _screenTarget.Width, _screenTarget.Height);
        if (_selector._roomEditor.fade > 0f)
            c = new Camera(3f, 4f, _screenTarget.Width / 4, _screenTarget.Height / 4);
        _batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, null, c.getMatrix());
    }

    public void EndDraw()
    {
        _batch.End();
        if (!_flashTransition)
        {
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
