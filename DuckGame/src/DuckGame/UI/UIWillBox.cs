namespace DuckGame;

public class UIWillBox : UIMenu
{
    #region Public Fields

    public bool down = true;

    public bool finished;

    #endregion

    #region Private Fields

    bool _wrapped = true;

    bool _flash;

    bool _doneDown;

    float yOffset = 150;

    float _downWait = 1;

    float _openWait = 1;

    Sprite _frame;

    Sprite _wrappedFrame;

    BitmapFont _font;

    FancyBitmapFont _fancyFont;

    Furniture _furni;

    UIMenu _link;

    #endregion

    #region Public Constructors

    public UIWillBox(Furniture f, float xpos, float ypos, float wide = -1, float high = -1, UIMenu link = null)
        : base("", xpos, ypos, wide, high)
    {
        Graphics.fade = 1;
        _frame = new Sprite("unlockFrame");
        _frame.CenterOrigin();
        _wrappedFrame = new Sprite("unlockFrameWrapped");
        _wrappedFrame.CenterOrigin();
        _font = new BitmapFont("biosFontUI", 8, 7);
        _fancyFont = new FancyBitmapFont("smallFont");
        _furni = f;
        _link = link;
    }

    #endregion

    #region Public Methods

    public override void OnClose()
    {
        base.OnClose();
        Profiles.Save(Profiles.experienceProfile);
        if (_link != null)
        {
            if (UIFuneral.oldSong != null)
                Music.Play(UIFuneral.oldSong);
            MonoMain.pauseMenu = _link;
        }
    }

    public override void Update()
    {
        if (finished)
        {
            _animating = false;
            return;
        }
        yOffset = Lerp.FloatSmooth(yOffset, down ? 150 : 0, 0.3f, 1.1f);
        if (down)
        {
            _downWait -= 0.06f;
            if (_downWait <= 0)
            {
                if (_doneDown)
                {
                    finished = true;
                    Close();
                    return;
                }
                _openWait = 1;
                _wrapped = true;
                _downWait = 1;
                down = false;
                SFX.Play("pause", 0.6f);
            }
        }
        else
        {
            _openWait -= 0.06f;
            if (_openWait <= 0 && _wrapped && !_flash)
                _flash = true;
            if (_flash)
            {
                Graphics.flashAdd = Lerp.Float(Graphics.flashAdd, 1, 0.2f);
                if (Graphics.flashAdd > 0.99f)
                {
                    _wrapped = !_wrapped;
                    if (!_wrapped)
                    {
                        Profiles.experienceProfile.SetNumFurnitures(_furni.index, Profiles.experienceProfile.GetNumFurnitures(_furni.index) + 1);
                        SFX.Play("harp");
                        HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@CONTINUE");
                    }
                    _flash = false;
                }
            }
            else
                Graphics.flashAdd = Lerp.Float(Graphics.flashAdd, 0, 0.2f);
            if (!_wrapped && Input.Pressed("SELECT"))
            {
                HUD.CloseAllCorners();
                SFX.Play("resume", 0.6f);
                down = true;
                _doneDown = true;
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        Y += yOffset;
        if (_wrapped)
        {
            _wrappedFrame.Depth = Depth;
            Graphics.Draw(_wrappedFrame, X, Y);
        }
        else
        {
            _frame.Depth = Depth;
            Graphics.Draw(_frame, X, Y);
            string text = "LAST WISH";
            Vec2 fontPos = new(0 - _font.GetWidth(text) / 2, -42);
            _font.DrawOutline(text, Position + fontPos, Color.White, Color.Black, Depth + 2);
            string unlockText = $"}} {_furni.name} }}";
            _fancyFont.Scale = Vec2.One;
            Vec2 unlockFontPos = new(0 - _fancyFont.GetWidth(unlockText) / 2, -25);
            _fancyFont.DrawOutline(unlockText, Position + unlockFontPos, Colors.DGYellow, Color.Black, Depth + 2);
            _fancyFont.Scale = new Vec2(0.5f);
            string descriptionText = "Your little man wanted you to have this.";
            Vec2 descFontPos = new(0 - _fancyFont.GetWidth(descriptionText) / 2, 38);
            _fancyFont.DrawOutline(descriptionText, Position + descFontPos, Colors.DGGreen, Color.Black, Depth + 2, 0.5f);
            _furni.Draw(Position + new Vec2(0, 10), Depth + 4, (_furni.name == "PHOTO") ? 1 : ((_furni.name == "EASEL") ? 6 : 0));
        }
        Y -= yOffset;
    }

    #endregion
}
