using Microsoft.Xna.Framework;
using System.Globalization;

namespace DuckGame;

public class BuyScreen : Level
{
    private BitmapFont _font;

    private Sprite _payScreen;

    private SpriteMap _moneyType;

    private bool _buy;

    private bool _demo;

    private float _wave;

    private bool _fade;

    private string _currencyType = "USD";

    private string _currencyCharacter = "$";

    private float _price;

    public BuyScreen(string currency, float price)
    {
        _centeredView = true;
        _currencyType = currency;
        _price = price;
    }

    public override void Initialize()
    {
        Graphics.fade = 0f;
        _payScreen = new Sprite("payScreen");
        _payScreen.CenterOrigin();
        _moneyType = new SpriteMap("moneyTypes", 14, 18);
        _font = new BitmapFont("moneyFont", 8);
        if (_currencyType == "USD")
        {
            _currencyCharacter = "$";
            _moneyType.frame = 0;
        }
        else if (_currencyType == "EUR")
        {
            _currencyCharacter = "%";
            _moneyType.frame = 1;
        }
        else if (_currencyType == "GBP")
        {
            _currencyCharacter = "&";
            _moneyType.frame = 2;
        }
        HUD.AddCornerControl(HUDCorner.BottomLeft, "@DPAD@Select");
        HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@Confirm");
        base.Initialize();
    }

    public override void Update()
    {
        if (_fade)
        {
            Graphics.fade = Lerp.Float(Graphics.fade, 0f, 0.02f);
            if (Graphics.fade <= 0f)
            {
                if (_demo)
                {
                    Main.isDemo = true;
                }
                else
                {
                    Main.isDemo = false;
                }
                Level.current = new TitleScreen();
            }
            return;
        }
        Graphics.fade = Lerp.Float(Graphics.fade, 1f, 0.02f);
        _wave += 0.1f;
        if (Input.Pressed("MENUUP"))
        {
            _buy = true;
            SFX.Play("textLetter", 0.9f);
        }
        if (Input.Pressed("MENUDOWN"))
        {
            _buy = false;
            SFX.Play("textLetter", 0.9f);
        }
        if (Input.Pressed("SELECT"))
        {
            if (_buy)
            {
                _fade = true;
                _demo = false;
            }
            else
            {
                _fade = true;
                _demo = true;
            }
            SFX.Play("rockHitGround", 0.9f);
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.Game)
        {
            _payScreen.Depth = 0.5f;
            _moneyType.Depth = 0.6f;
            Graphics.Draw(_payScreen, layer.width / 2f, layer.height / 2f);
            Graphics.Draw(_moneyType, layer.width / 2f - 79f, layer.height / 2f - 23f);
            string text = "Buy Game (" + _currencyCharacter + _price.ToString("0.00", CultureInfo.InvariantCulture) + ")";
            _font.Draw(text, layer.width / 2f - _font.GetWidth(text) / 2f + 15f, layer.height / 2f - 18f, Color.White, 0.8f);
            if (_buy)
            {
                Vector2 vec = new Vector2(layer.width / 2f - (float)(_payScreen.width / 2) + 6f, layer.height / 2f - 25f);
                Vector2 size = new Vector2((float)_payScreen.width - 11.5f, 22f);
                Graphics.DrawRect(vec, vec + size, Color.White, 0.9f, filled: false);
            }
            else
            {
                Vector2 vec2 = new Vector2(layer.width / 2f - (float)(_payScreen.width / 2) + 6f, layer.height / 2f + 3f);
                Vector2 size2 = new Vector2((float)_payScreen.width - 11.5f, 22f);
                Graphics.DrawRect(vec2, vec2 + size2, Color.White, 0.9f, filled: false);
            }
            text = "PLAY DEMO";
            _font.Draw(text, layer.width / 2f - _font.GetWidth(text) / 2f + 12f, layer.height / 2f + 10f, Color.White, 0.8f);
        }
        base.PostDrawLayer(layer);
    }
}
