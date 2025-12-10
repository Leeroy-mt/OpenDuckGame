using System.Collections.Generic;

namespace DuckGame;

public class CategoryGrid : Thing
{
    private List<Card> _cards = new List<Card>();

    private List<CardStrip> _strips = new List<CardStrip>();

    private IPageListener _listener;

    private int _selectedStripIndex;

    public CategoryGrid(float xpos, float ypos, List<Card> cards, IPageListener listener)
        : base(xpos, ypos)
    {
        _cards = cards;
        _listener = listener;
    }

    public void AddStrip(List<Card> infos)
    {
        List<Card> info = new List<Card>();
        info.AddRange(infos);
        CardStrip strip = new CardStrip(base.x, base.y, info, _listener, largeCard: true);
        _strips.Add(strip);
        Level.Add(strip);
    }

    public void AddStrip(StripInfo infos)
    {
        List<Card> info = new List<Card>();
        info.AddRange(infos.cards);
        CardStrip strip = new CardStrip(base.x, base.y, info, _listener, infos.large, infos.cardsVisible, infos.header);
        _strips.Add(strip);
        Level.Add(strip);
    }

    public override void Initialize()
    {
        if (_cards != null)
        {
            List<Card> infoStrip = new List<Card>();
            foreach (Card info in _cards)
            {
                infoStrip.Add(info);
                if (infoStrip.Count == 3)
                {
                    AddStrip(infoStrip);
                    infoStrip.Clear();
                }
            }
            if (infoStrip.Count > 0)
            {
                AddStrip(infoStrip);
            }
        }
        base.Initialize();
    }

    public override void Update()
    {
        if (InputProfile.active.Pressed("MENUUP"))
        {
            _selectedStripIndex--;
        }
        else if (InputProfile.active.Pressed("MENUDOWN"))
        {
            _selectedStripIndex++;
        }
        if (_selectedStripIndex < 0)
        {
            _selectedStripIndex = 0;
        }
        else if (_selectedStripIndex >= _strips.Count)
        {
            _selectedStripIndex = _strips.Count - 1;
        }
    }

    public override void Draw()
    {
        float yOff = base.y;
        int index = 0;
        foreach (CardStrip strip in _strips)
        {
            strip.y = yOff;
            yOff += strip.height + 4f;
            strip.selected = index == _selectedStripIndex;
            index++;
        }
    }
}
