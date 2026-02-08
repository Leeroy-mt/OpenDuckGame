using Microsoft.Xna.Framework;

namespace DuckGame;

public class BetaScreen : Level
{
    private FancyBitmapFont _font;

    private BitmapFont _bigFont;

    private float _wait = 1f;

    private bool _fading;

    private bool _drmSuccess;

    public BetaScreen()
    {
        _centeredView = true;
    }

    public override void Initialize()
    {
        _drmSuccess = DG.InitializeDRM();
        _font = new FancyBitmapFont("smallFont");
        _bigFont = new BitmapFont("biosFont", 8);
        Graphics.fade = 0f;
    }

    public override void Update()
    {
        if (!_fading)
        {
            if (Graphics.fade < 1f)
            {
                Graphics.fade += 0.03f;
            }
            else
            {
                Graphics.fade = 1f;
            }
        }
        else if (Graphics.fade > 0f)
        {
            Graphics.fade -= 0.03f;
        }
        else
        {
            Graphics.fade = 0f;
            Level.current = new TitleScreen();
        }
        _wait -= 0.02f;
        if (!DG.buildExpired && _drmSuccess && _wait < 0f && Input.Pressed("START"))
        {
            _fading = true;
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.Game)
        {
            string text = "|DGYELLOW|HEY!";
            float yPos = 55f;
            _font.Draw(text, layer.width / 2f - _font.GetWidth(text) / 2f, yPos, Color.White);
            if (!_drmSuccess)
            {
                yPos += 10f;
                text = "|WHITE|Woah! DRM is enabled since this is a pre release build.\nMake sure you're connected to steam, and that you're\nSupposed to have this build!";
                _font.Draw(text, layer.width / 2f - _font.GetWidth(text) / 2f, yPos, Color.White);
            }
            else if (DG.buildExpired)
            {
                yPos += 10f;
                text = "|WHITE|Sorry, this build was a limited beta build.\nIt appears to have expired X(.\nShould be easy to get around, or the game\nshould be out on steam now, go get it!";
                _font.Draw(text, layer.width / 2f - _font.GetWidth(text) / 2f, yPos, Color.White);
            }
            else if (DG.betaBuild)
            {
                yPos += 15f;
                text = "|WHITE|This is a near final release of |RED|DUCK GAME|WHITE|!\n|WHITE|Some stuff is still getting finished up, so\nplease bear with me |PINK|{|WHITE|.";
                _font.Draw(text, layer.width / 2f - _font.GetWidth(text) / 2f, yPos, Color.White);
                text = "|WHITE|Press @START@ to continue...";
                _bigFont.Draw(text, new Vector2(layer.width / 2f - _bigFont.GetWidth(text) / 2f, yPos + 55f), Color.White);
            }
        }
    }
}
