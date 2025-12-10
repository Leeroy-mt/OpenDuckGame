namespace DuckGame;

public class UIFuneral : UIMenu
{
    private Sprite _frame;

    private BitmapFont _font;

    private FancyBitmapFont _fancyFont;

    private float yOffset = 150f;

    public bool down = true;

    private float _downWait = 1f;

    private SpriteMap _portraitSprite;

    private Sprite _portraitFrame;

    private bool _shown;

    private UIMenu _link;

    public static string oldSong;

    private bool _doneDown;

    private float _openWait = 1f;

    public bool finished;

    public UIFuneral(float xpos, float ypos, float wide = -1f, float high = -1f, UIMenu link = null)
        : base("", xpos, ypos, wide, high)
    {
        Graphics.fade = 1f;
        _frame = new Sprite("deathFrame");
        _frame.CenterOrigin();
        _link = link;
        _font = new BitmapFont("biosFontUI", 8, 7);
        _fancyFont = new FancyBitmapFont("smallFont");
        _portraitFrame = new Sprite("funeralPic");
        _portraitSprite = new SpriteMap("littleMan", 16, 16);
        _portraitSprite.frame = UILevelBox.LittleManFrame(Profiles.experienceProfile.numLittleMen - 9, -1, 0uL);
    }

    public override void OnClose()
    {
        base.OnClose();
        Profiles.Save(Profiles.experienceProfile);
        if (_link != null)
        {
            MonoMain.pauseMenu = _link;
        }
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Update()
    {
        if (finished)
        {
            _animating = false;
            return;
        }
        yOffset = Lerp.FloatSmooth(yOffset, down ? 150f : 0f, 0.3f, 1.1f);
        if (down)
        {
            _downWait -= 0.06f;
            if (_downWait <= 0f)
            {
                if (_doneDown)
                {
                    finished = true;
                    Close();
                    return;
                }
                _openWait = 1f;
                _downWait = 1f;
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
        base.y += yOffset;
        _frame.depth = base.depth;
        Graphics.Draw(_frame, base.x, base.y);
        string text = "FAREWELL";
        Vec2 fontPos = new Vec2(0f - _font.GetWidth(text) / 2f, -34f);
        _font.DrawOutline(text, position + fontPos, Color.White, Color.Black, base.depth + 2);
        string descriptionText = "Little man who's come to pass,\nRest in peace beneath the grass.\nHeavy souls weigh on this day,\nwe send a little man on his way.\n\n\nMay Angles Lead You In.";
        Vec2 descFontPos = new Vec2(-33f, -15f);
        _fancyFont.scale = new Vec2(0.5f, 0.5f);
        _fancyFont.Draw(descriptionText, position + descFontPos, new Color(27, 38, 50), base.depth + 2);
        Vec2 portraitPos = new Vec2(-53f, -4f);
        _portraitSprite.depth = base.depth + 2;
        _portraitFrame.depth = base.depth + 4;
        Graphics.Draw(_portraitSprite, position.x + portraitPos.x + 1f, position.y + portraitPos.y + 1f, new Rectangle(2f, 0f, 12f, 10f));
        Graphics.Draw(_portraitFrame, position.x + portraitPos.x - 2f, position.y + portraitPos.y - 2f);
        Graphics.DrawRect(position + portraitPos, position + portraitPos + new Vec2(13f, 13f), Colors.DGBlue, base.depth + 1);
        base.y -= yOffset;
    }
}
