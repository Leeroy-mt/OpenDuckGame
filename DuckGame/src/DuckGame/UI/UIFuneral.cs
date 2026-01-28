namespace DuckGame;

public class UIFuneral : UIMenu
{
    #region Public Fields

    public static string oldSong;

    public bool finished;

    public bool down = true;

    #endregion

    #region Private Fields

    bool _shown;

    bool _doneDown;

    float _downWait = 1;

    float yOffset = 150;

    Sprite _frame;

    Sprite _portraitFrame;

    SpriteMap _portraitSprite;

    BitmapFont _font;

    FancyBitmapFont _fancyFont;

    UIMenu _link;

    #endregion

    #region Public Constructors

    public UIFuneral(float xpos, float ypos, float wide = -1, float high = -1, UIMenu link = null)
        : base("", xpos, ypos, wide, high)
    {
        Graphics.fade = 1;
        _frame = new Sprite("deathFrame");
        _frame.CenterOrigin();
        _link = link;
        _font = new BitmapFont("biosFontUI", 8, 7);
        _fancyFont = new FancyBitmapFont("smallFont");
        _portraitFrame = new Sprite("funeralPic");
        _portraitSprite = new SpriteMap("littleMan", 16, 16)
        {
            frame = UILevelBox.LittleManFrame(Profiles.experienceProfile.numLittleMen - 9, -1, 0)
        };
    }

    #endregion

    #region Public Methods

    public override void OnClose()
    {
        base.OnClose();
        Profiles.Save(Profiles.experienceProfile);
        if (_link != null)
            MonoMain.pauseMenu = _link;
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
                _downWait = 1;
                down = false;
                SFX.Play("pause", 0.6f);
            }
        }
        else
        {
            if (!_shown)
            {
                oldSong = Music.currentSong;
                Music.Play("littlesad", looping: false);
                HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@CONTINUE");
                _shown = true;
            }
            if (Input.Pressed("SELECT"))
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
        _frame.Depth = Depth;
        Graphics.Draw(_frame, X, Y);
        string text = "FAREWELL";
        Vec2 fontPos = new(0 - _font.GetWidth(text) / 2, -34);
        _font.DrawOutline(text, Position + fontPos, Color.White, Color.Black, Depth + 2);
        string descriptionText = "Little man who's come to pass,\nRest in peace beneath the grass.\nHeavy souls weigh on this day,\nwe send a little man on his way.\n\n\nMay Angles Lead You In.";
        Vec2 descFontPos = new(-33, -15);
        _fancyFont.Scale = new Vec2(0.5f, 0.5f);
        _fancyFont.Draw(descriptionText, Position + descFontPos, new Color(27, 38, 50), Depth + 2);
        Vec2 portraitPos = new(-53, -4);
        _portraitSprite.Depth = Depth + 2;
        _portraitFrame.Depth = Depth + 4;
        Graphics.Draw(_portraitSprite, Position.X + portraitPos.X + 1, Position.Y + portraitPos.Y + 1, new Rectangle(2, 0, 12, 10));
        Graphics.Draw(_portraitFrame, Position.X + portraitPos.X - 2, Position.Y + portraitPos.Y - 2);
        Graphics.DrawRect(Position + portraitPos, Position + portraitPos + new Vec2(13), Colors.DGBlue, Depth + 1);
        Y -= yOffset;
    }

    #endregion
}
