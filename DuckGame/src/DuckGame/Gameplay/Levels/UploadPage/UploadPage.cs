using System.Collections.Generic;

namespace DuckGame;

public class UploadPage : Page, IPageListener
{
    private BitmapFont _font;

    private List<Card> _cards = new List<Card>();

    private Card _pageToOpen;

    private Thing _strip;

    private bool _grid;

    public UploadPage(List<Card> cards, bool grid)
    {
        _grid = grid;
        _cards = cards;
    }

    public override void DeactivateAll()
    {
        _strip.active = false;
    }

    public override void ActivateAll()
    {
        _strip.active = true;
    }

    public override void TransitionOutComplete()
    {
        if (_pageToOpen.specialText == "Upload Thing")
        {
            Level.current = new MainPage(_cards, grid: true);
        }
    }

    public void CardSelected(Card card)
    {
        _state = CategoryState.OpenPage;
        _pageToOpen = card;
    }

    public override void Initialize()
    {
        Layer.HUD.camera.x = Page.camOffset;
        base.backgroundColor = new Color(8, 12, 13);
        _font = new BitmapFont("biosFont", 8);
        HUD.AddCornerControl(HUDCorner.BottomLeft, "@SELECT@SELECT");
        HUD.AddCornerControl(HUDCorner.BottomRight, "@CANCEL@BACK");
        CategoryGrid g = new CategoryGrid(12f, 20f, null, this);
        Level.Add(g);
        if (_cards.Count > 4)
        {
            _cards.Insert(4, new LevelInfo(large: false, "Upload Thing"));
        }
        StripInfo myLevels = new StripInfo(l: false);
        myLevels.cards.AddRange(_cards);
        myLevels.header = "Your Things";
        myLevels.cardsVisible = 4;
        g.AddStrip(myLevels);
        StripInfo browse = new StripInfo(l: false);
        browse.cards.Add(new LevelInfo(large: false, "Not a thing."));
        browse.header = "Browse Things";
        browse.cardsVisible = 4;
        g.AddStrip(browse);
        _strip = g;
        base.Initialize();
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.HUD)
        {
            BitmapFont font = _font;
            float xscale = (_font.ScaleY = 1f);
            font.ScaleX = xscale;
            _font.Draw("Upload", 8f, 8f, Color.White, 0.95f);
        }
    }
}
