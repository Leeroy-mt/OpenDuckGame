namespace DuckGame;

public class UIPresentBox : UIMenu
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

    string _oldSong;

    Sprite _frame;

    Sprite _wrappedFrame;

    BitmapFont _font;

    FancyBitmapFont _fancyFont;

    Furniture _furni;

    #endregion

    #region Public Constructors

    public UIPresentBox(Furniture f, float xpos, float ypos, float wide = -1, float high = -1)
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
    }

    #endregion

    #region Public Methods

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
                        _oldSong = Music.currentSong;
                        Music.Play("jollyjingle");
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
                if (_oldSong != null)
                    Music.Play(_oldSong);
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
            string text = "@LWING@MEMENTO@RWING@";
            if (_furni.name == "VOODOO VINCENT")
                text = "@LWING@XP SKIP@RWING@";
            Vec2 fontPos = new(-_font.GetWidth(text) / 2, -42);
            _font.DrawOutline(text, Position + fontPos, Color.White, Color.Black, Depth + 2);
            string unlockText = $"}} {_furni.name} }}";
            if (_furni.name.Length >= 15)
                unlockText = _furni.name;
            _fancyFont.Scale = Vec2.One;
            Vec2 unlockFontPos = new(-_fancyFont.GetWidth(unlockText) / 2, -25);
            _fancyFont.DrawOutline(unlockText, Position + unlockFontPos, Colors.DGYellow, Color.Black, Depth + 2);
            _fancyFont.Scale = new Vec2(0.5f);
            string descriptionText = _furni.description;
            Vec2 descFontPos = new(-_fancyFont.GetWidth(descriptionText) / 2, 38);
            _fancyFont.DrawOutline(descriptionText, Position + descFontPos, Colors.DGGreen, Color.Black, Depth + 2, 0.5f);
            _furni.Draw(Position + new Vec2(0, 10), Depth + 4, _furni.name == "PHOTO" ? 1 : (_furni.name == "EASEL" ? 6 : 0));
        }
        Y -= yOffset;
    }

    #endregion
}
