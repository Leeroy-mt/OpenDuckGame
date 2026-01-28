using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class UICaptureBox : UIMenu
{
    #region Public Fields

    public bool finished;

    #endregion

    #region Private Fields

    bool _resizable;

    Vec2 _capturePosition;

    Vec2 _captureSize;

    RenderTarget2D _captureTarget;

    UIMenu _closeMenu;

    #endregion

    #region Public Constructors

    public UICaptureBox(UIMenu closeMenu, float xpos, float ypos, float wide = -1f, float high = -1f, bool resizable = false)
        : base("", xpos, ypos, wide, high)
    {
        float captureSize = 38;
        _capturePosition = new Vec2(Layer.HUD.camera.width / 2 - captureSize / 2, Layer.HUD.camera.height / 2 - captureSize / 2);
        _captureSize = new Vec2(captureSize, captureSize);
        if (resizable)
            _captureSize = new Vec2(320, 180);
        _closeMenu = closeMenu;
        _resizable = resizable;
    }

    #endregion

    #region Public Methods

    public override void Update()
    {
        if (open)
        {
            if (_captureTarget == null)
            {
                if (_resizable)
                    _captureTarget = new RenderTarget2D(1280, 720, pdepth: true);
                else
                    _captureTarget = new RenderTarget2D(152, 152, pdepth: true);
            }

            MonoMain.autoPauseFade = false;
            if (Input.Down("MENULEFT"))
                _capturePosition.X -= 1;
            if (Input.Down("MENURIGHT"))
                _capturePosition.X += 1;
            if (Input.Down("MENUUP"))
                _capturePosition.Y -= 1;
            if (Input.Down("MENUDOWN"))
                _capturePosition.Y += 1;
            float mult = Graphics.width / 320;
            if (_resizable)
            {
                var resize = 0 - (InputProfile.DefaultPlayer1.leftTrigger - InputProfile.DefaultPlayer1.rightTrigger);
                resize *= 0.1f;
                _captureSize += _captureSize * resize;
                if (_captureSize.X > 1280)
                    _captureSize.X = 1280;
                if (_captureSize.Y > 720)
                    _captureSize.Y = 720;

                Vec2 bigPos = _capturePosition * mult;
                if (bigPos.X < 0f)
                    bigPos.X = 0f;
                if (bigPos.Y < 0f)
                    bigPos.Y = 0f;

                _capturePosition = bigPos / mult;
            }
            Graphics.SetRenderTarget(_captureTarget);
            Camera c = new(_capturePosition.X * mult, _capturePosition.Y * mult, (float)(int)_captureSize.X * mult, (float)(int)_captureSize.Y * mult);
            Graphics.Clear(Color.Black);
            Viewport viewport = Graphics.viewport;
            Graphics.viewport = new Viewport(0, 0, (int)(_captureSize.X * mult), (int)(_captureSize.Y * mult));
            Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, null, c.getMatrix());
            Graphics.Draw(MonoMain.screenCapture, 0, 0);
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
        if (open)
        {
            Graphics.DrawRect(new Vec2(_capturePosition.X - 1, _capturePosition.Y - 1), new Vec2(_capturePosition.X + (float)(int)_captureSize.X + 1, _capturePosition.Y + (float)(int)_captureSize.Y + 1), Color.White, 1, filled: false);
            if (_captureTarget != null)
                Graphics.Draw(_captureTarget, _capturePosition, new Rectangle(0, 0, (int)_captureSize.X * 4, (int)_captureSize.Y * 4), Color.White, 0, Vec2.Zero, new Vec2(0.25f, 0.25f), SpriteEffects.None, 1);
        }
    }

    #endregion
}
