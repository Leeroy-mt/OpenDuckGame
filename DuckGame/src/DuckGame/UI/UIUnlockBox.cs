using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UIUnlockBox : UIMenu
{
    #region Public Fields

    public bool down = true;

    public bool finished;

    #endregion

    #region Private Fields

    bool _wrapped = true;

    bool _flash;

    float yOffset = 150;

    float _downWait = 1;

    float _openWait = 1;

    string _oldSong;

    Sprite _frame;

    Sprite _wrappedFrame;

    BitmapFont _font;

    FancyBitmapFont _fancyFont;

    Unlockable _unlock;

    List<Unlockable> _unlocks;

    #endregion

    #region Public Constructors

    public UIUnlockBox(List<Unlockable> unlocks, float xpos, float ypos, float wide = -1, float high = -1)
        : base("", xpos, ypos, wide, high)
    {
        Graphics.fade = 1f;
        _frame = new Sprite("unlockFrame");
        _frame.CenterOrigin();
        _wrappedFrame = new Sprite("unlockFrameWrapped");
        _wrappedFrame.CenterOrigin();
        _font = new BitmapFont("biosFontUI", 8, 7);
        _fancyFont = new FancyBitmapFont("smallFont");
        _unlocks = unlocks;
        _unlock = _unlocks.First();
    }

    #endregion

    #region Public Methods

    public override void Update()
    {
        yOffset = Lerp.FloatSmooth(yOffset, down ? 150 : 0, 0.3f, 1.1f);
        if (down)
        {
            if (_unlocks.Count == 0)
            {
                if (!finished)
                {
                    finished = true;
                    Close();
                }
            }
            else
            {
                _downWait -= 0.06f;
                if (_downWait <= 0f)
                {
                    _openWait = 1;
                    _wrapped = true;
                    _downWait = 1;
                    _unlock = _unlocks.First();
                    _unlocks.RemoveAt(0);
                    down = false;
                    SFX.Play("pause", 0.6f);
                }
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
                        if (_unlock != null && _unlock.name == "UR THE BEST")
                        {
                            _oldSong = Music.currentSong;
                            Music.Play("jollyjingle");
                        }
                        SFX.Play("harp");
                        HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@CONTINUE");
                        _unlock.DoUnlock();
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
                if (_oldSong != null && _unlock != null && _unlock.name == "UR THE BEST")
                    Music.Play(_oldSong);
                down = true;
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
            _frame.Depth = -0.9f;
            Graphics.Draw(_frame, X, Y);
            string text = "@LWING@UNLOCK@RWING@";
            if (_unlock.name == "UR THE BEST")
                text = "@LWING@WOAH!@RWING@";
            Vec2 fontPos = new(-_font.GetWidth(text) / 2, -42);
            _font.DrawOutline(text, Position + fontPos, Color.White, Color.Black, Depth + 2);
            string unlockText = $"}} {_unlock.name} }}";
            _fancyFont.Scale = Vec2.One;
            Vec2 unlockFontPos = new(-_fancyFont.GetWidth(unlockText) / 2, -25);
            _fancyFont.DrawOutline(unlockText, Position + unlockFontPos, Colors.DGYellow, Color.Black, Depth + 2);
            _fancyFont.Scale = new Vec2(0.5f);
            string descriptionText = _unlock.description;
            Vec2 descFontPos = new(-_fancyFont.GetWidth(descriptionText) / 2, 38);
            _fancyFont.DrawOutline(descriptionText, Position + descFontPos, Colors.DGGreen, Color.Black, Depth + 2, 0.5f);
            _unlock.Draw(X, Y + 10, Depth + 4);
        }
        Y -= yOffset;
    }

    #endregion
}
