using System.Collections.Generic;

namespace DuckGame;

public class CategoryPage : Level, IPageListener
{
    private CategoryState _state;

    private BitmapFont _font;

    private List<Card> _cards = new List<Card>();

    private Card _pageToOpen;

    private Thing _strip;

    private bool _grid;

    public static float camOffset;

    public CategoryPage(List<Card> cards, bool grid)
    {
        _grid = grid;
        _cards = cards;
    }

    public void CardSelected(Card card)
    {
        _state = CategoryState.OpenPage;
        _pageToOpen = card;
    }

    public override void Initialize()
    {
        Layer.HUD.camera.x = camOffset;
        base.backgroundColor = new Color(8, 12, 13);
        _font = new BitmapFont("biosFont", 8);
        HUD.AddCornerControl(HUDCorner.BottomLeft, "@SELECT@SELECT");
        HUD.AddCornerControl(HUDCorner.BottomRight, "@CANCEL@BACK");
        if (_grid)
        {
            _strip = new CategoryGrid(12f, 31f, _cards, this);
            Level.Add(_strip);
        }
        else
        {
            _strip = new CardStrip(12f, 31f, _cards, this, largeCard: false, 4);
            Level.Add(_strip);
        }
        base.Initialize();
    }

    public override void Update()
    {
        Layer.HUD.camera.x = camOffset;
        if (_state == CategoryState.OpenPage)
        {
            _strip.active = false;
            camOffset = Lerp.FloatSmooth(camOffset, 360f, 0.1f);
            if (camOffset > 330f && _pageToOpen.specialText == "VIEW ALL")
            {
                Level.current = new CategoryPage(_cards, grid: true);
            }
        }
        else if (_state == CategoryState.Idle)
        {
            camOffset = Lerp.FloatSmooth(camOffset, -40f, 0.1f);
            if (camOffset < 0f)
            {
                camOffset = 0f;
            }
            _strip.active = camOffset == 0f;
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.HUD)
        {
            BitmapFont font = _font;
            float xscale = (_font.yscale = 1f);
            font.xscale = xscale;
            _font.Draw("CUSTOM LEVELS", 8f, 8f, Color.White, 0.95f);
            BitmapFont font2 = _font;
            xscale = (_font.yscale = 0.75f);
            font2.xscale = xscale;
            _font.Draw("BEST NEW LEVELS", 14f, 22f, Color.White, 0.95f);
        }
    }
}
