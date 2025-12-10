using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class UICaptureBox : UIMenu
{
    private Vec2 _capturePosition;

    private Vec2 _captureSize;

    private RenderTarget2D _captureTarget;

    private UIMenu _closeMenu;

    private bool _resizable;

    public bool finished;

    public UICaptureBox(UIMenu closeMenu, float xpos, float ypos, float wide = -1f, float high = -1f, bool resizable = false)
        : base("", xpos, ypos, wide, high)
    {
        float captureSize = 38f;
        _capturePosition = new Vec2(Layer.HUD.camera.width / 2f - captureSize / 2f, Layer.HUD.camera.height / 2f - captureSize / 2f);
        _captureSize = new Vec2(captureSize, captureSize);
        if (resizable)
        {
            _captureSize = new Vec2(320f, 180f);
        }
        _closeMenu = closeMenu;
        _resizable = resizable;
    }

    public override void Update()
    {
        if (base.open)
        {
            if (_captureTarget == null)
            {
                if (_resizable)
                {
                    _captureTarget = new RenderTarget2D(1280, 720, pdepth: true);
                }
                else
                {
                    _captureTarget = new RenderTarget2D(152, 152, pdepth: true);
                }
            }
            MonoMain.autoPauseFade = false;
            if (Input.Down("MENULEFT"))
            {
                _capturePosition.x -= 1f;
            }
            if (Input.Down("MENURIGHT"))
            {
                _capturePosition.x += 1f;
            }
            if (Input.Down("MENUUP"))
            {
                _capturePosition.y -= 1f;
            }
            if (Input.Down("MENUDOWN"))
            {
                _capturePosition.y += 1f;
            }
            float mult = Graphics.width / 320;
            if (_resizable)
            {
                float resize = 0f - (InputProfile.DefaultPlayer1.leftTrigger - InputProfile.DefaultPlayer1.rightTrigger);
                resize *= 0.1f;
                _captureSize += _captureSize * resize;
                if (_captureSize.x > 1280f)
                {
                    _captureSize.x = 1280f;
                }
                if (_captureSize.y > 720f)
                {
                    _captureSize.y = 720f;
                }
                Vec2 bigPos = _capturePosition * mult;
                if (bigPos.x < 0f)
                {
                    bigPos.x = 0f;
                }
                if (bigPos.y < 0f)
                {
                    bigPos.y = 0f;
                }
                _capturePosition = bigPos / mult;
            }
            Graphics.SetRenderTarget(_captureTarget);
            Camera c = new Camera(_capturePosition.x * mult, _capturePosition.y * mult, (float)(int)_captureSize.x * mult, (float)(int)_captureSize.y * mult);
            Graphics.Clear(Color.Black);
            Viewport viewport = Graphics.viewport;
            Graphics.viewport = new Viewport(0, 0, (int)(_captureSize.x * mult), (int)(_captureSize.y * mult));
            Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, null, c.getMatrix());
            Graphics.Draw(MonoMain.screenCapture, 0f, 0f);
            Graphics.screen.End();
            Graphics.viewport = viewport;
            Graphics.SetRenderTarget(null);
            if (Input.Pressed("SELECT"))
            {
                SFX.Play("cameraFlash");
                Editor.previewCapture = _captureTarget;
                _captureTarget = null;
                new UIMenuActionOpenMenu(this, _closeMenu).Activate();
            }
            else if (Input.Pressed("CANCEL"))
            {
                SFX.Play("consoleCancel");
                new UIMenuActionOpenMenu(this, _closeMenu).Activate();
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        if (base.open)
        {
            Graphics.DrawRect(new Vec2(_capturePosition.x - 1f, _capturePosition.y - 1f), new Vec2(_capturePosition.x + (float)(int)_captureSize.x + 1f, _capturePosition.y + (float)(int)_captureSize.y + 1f), Color.White, 1f, filled: false);
            if (_captureTarget != null)
            {
                Graphics.Draw(_captureTarget, _capturePosition, new Rectangle(0f, 0f, (int)_captureSize.x * 4, (int)_captureSize.y * 4), Color.White, 0f, Vec2.Zero, new Vec2(0.25f, 0.25f), SpriteEffects.None, 1f);
            }
        }
    }
}
