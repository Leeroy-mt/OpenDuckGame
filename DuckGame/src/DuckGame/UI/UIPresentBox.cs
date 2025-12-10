namespace DuckGame;

public class UIPresentBox : UIMenu
{
    private Sprite _frame;

    private Sprite _wrappedFrame;

    private BitmapFont _font;

    private FancyBitmapFont _fancyFont;

    private bool _wrapped = true;

    private bool _flash;

    private Furniture _furni;

    private float yOffset = 150f;

    public bool down = true;

    private float _downWait = 1f;

    private Sprite _eggSprite;

    private string _oldSong;

    private bool _doneDown;

    private float _openWait = 1f;

    public bool finished;

    public UIPresentBox(Furniture f, float xpos, float ypos, float wide = -1f, float high = -1f)
        : base("", xpos, ypos, wide, high)
    {
        Graphics.fade = 1f;
        _frame = new Sprite("unlockFrame");
        _frame.CenterOrigin();
        _wrappedFrame = new Sprite("unlockFrameWrapped");
        _wrappedFrame.CenterOrigin();
        _font = new BitmapFont("biosFontUI", 8, 7);
        _fancyFont = new FancyBitmapFont("smallFont");
        _furni = f;
        _eggSprite = Profile.GetEggSprite(0, 0uL);
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
                _wrapped = true;
                _downWait = 1f;
                down = false;
                SFX.Play("pause", 0.6f);
            }
        }
        else
        {
            _openWait -= 0.06f;
            if (_openWait <= 0f && _wrapped && !_flash)
            {
                _flash = true;
            }
            if (_flash)
            {
                Graphics.flashAdd = Lerp.Float(Graphics.flashAdd, 1f, 0.2f);
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
            {
                Graphics.flashAdd = Lerp.Float(Graphics.flashAdd, 0f, 0.2f);
            }
            if (!_wrapped && Input.Pressed("SELECT"))
            {
                HUD.CloseAllCorners();
                SFX.Play("resume", 0.6f);
                if (_oldSong != null)
                {
                    Music.Play(_oldSong);
                }
                down = true;
                _doneDown = true;
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        base.y += yOffset;
        if (_wrapped)
        {
            _wrappedFrame.depth = base.depth;
            Graphics.Draw(_wrappedFrame, base.x, base.y);
        }
        else
        {
            _frame.depth = base.depth;
            Graphics.Draw(_frame, base.x, base.y);
            string text = "@LWING@MEMENTO@RWING@";
            if (_furni.name == "VOODOO VINCENT")
            {
                text = "@LWING@XP SKIP@RWING@";
            }
            Vec2 fontPos = new Vec2(0f - _font.GetWidth(text) / 2f, -42f);
            _font.DrawOutline(text, position + fontPos, Color.White, Color.Black, base.depth + 2);
            string unlockText = "} " + _furni.name + " }";
            if (_furni.name.Length >= 15)
            {
                unlockText = _furni.name;
            }
            _fancyFont.scale = new Vec2(1f, 1f);
            Vec2 unlockFontPos = new Vec2(0f - _fancyFont.GetWidth(unlockText) / 2f, -25f);
            _fancyFont.DrawOutline(unlockText, position + unlockFontPos, Colors.DGYellow, Color.Black, base.depth + 2);
            _fancyFont.scale = new Vec2(0.5f, 0.5f);
            string descriptionText = _furni.description;
            Vec2 descFontPos = new Vec2(0f - _fancyFont.GetWidth(descriptionText) / 2f, 38f);
            _fancyFont.DrawOutline(descriptionText, position + descFontPos, Colors.DGGreen, Color.Black, base.depth + 2, 0.5f);
            _furni.Draw(position + new Vec2(0f, 10f), base.depth + 4, (_furni.name == "PHOTO") ? 1 : ((_furni.name == "EASEL") ? 6 : 0));
        }
        base.y -= yOffset;
    }
}
