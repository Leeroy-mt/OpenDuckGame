using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class CardStrip : Thing
{
    private int _levelIndex;

    private int _selectedCardIndex;

    private float _indexSlide;

    private List<Card> _cards = new List<Card>();

    private Sprite _arrow;

    private Card _cardSelected;

    private IPageListener _listener;

    private int _numCardsPerScreen = 3;

    private int _maxCardsPerStrip = 5;

    private bool _large;

    private string _heading;

    private bool _selected;

    private static BitmapFont _font = new BitmapFont("biosFont", 8);

    public Card cardSelected => _cardSelected;

    public bool selected
    {
        get
        {
            return _selected;
        }
        set
        {
            if (_selected != value)
            {
                _selected = value;
                _selectedCardIndex = _levelIndex;
            }
        }
    }

    public CardStrip(float xpos, float ypos, List<Card> cards, IPageListener listener, bool largeCard, int cardsPerScreen = 3, string heading = null)
        : base(xpos, ypos)
    {
        if (cards.Count > _maxCardsPerStrip)
        {
            _cards = cards.GetRange(0, 5);
            if (cards[0] is LevelInfo)
            {
                LevelInfo i = _cards[0] as LevelInfo;
                _cards.Add(new LevelInfo
                {
                    specialText = "VIEW ALL",
                    large = i.large
                });
            }
        }
        else
        {
            _cards = cards;
        }
        _listener = listener;
        _large = largeCard;
        _numCardsPerScreen = cardsPerScreen;
        _heading = heading;
        if (cards != null && cards.Count > 0)
        {
            float sizeY = cards[0].height;
            if (heading != null && heading != "")
            {
                sizeY += 10f;
            }
            collisionSize = new Vec2((float)_numCardsPerScreen * (cards[0].width + 4f), sizeY);
        }
    }

    public override void Initialize()
    {
        base.layer = Layer.HUD;
        _arrow = new Sprite("levelBrowserArrow");
        _arrow.CenterOrigin();
    }

    public override void Update()
    {
        if (_selected)
        {
            if (InputProfile.active.Pressed("MENULEFT"))
            {
                _selectedCardIndex--;
            }
            else if (InputProfile.active.Pressed("MENURIGHT"))
            {
                _selectedCardIndex++;
            }
            else if (InputProfile.active.Pressed("SELECT"))
            {
                _listener.CardSelected(_cards[_selectedCardIndex]);
            }
        }
        if (_selectedCardIndex >= _cards.Count())
        {
            _selectedCardIndex = _cards.Count() - 1;
        }
        else if (_selectedCardIndex < 0)
        {
            _selectedCardIndex = 0;
        }
        if (_levelIndex + (_numCardsPerScreen - 1) < _selectedCardIndex)
        {
            if (_indexSlide > -1f)
            {
                _indexSlide = Lerp.FloatSmooth(_indexSlide, -1.2f, 0.2f);
            }
            if (_indexSlide <= -1f)
            {
                _levelIndex++;
                _indexSlide = 0f;
            }
        }
        if (_levelIndex > _selectedCardIndex)
        {
            if (_indexSlide < 1f)
            {
                _indexSlide = Lerp.FloatSmooth(_indexSlide, 1.2f, 0.2f);
            }
            if (_indexSlide >= 1f)
            {
                _levelIndex--;
                _indexSlide = 0f;
            }
        }
    }

    public override void Draw()
    {
        float yDraw = base.Y;
        if (_heading != null && _heading != "")
        {
            _font.Scale = new Vec2(0.75f, 0.75f);
            _font.Draw(_heading, base.X + 4f, base.Y, Color.White, 0.95f);
            yDraw += 10f;
        }
        Vec2 cardPos = Vec2.Zero;
        Vec2 size = Vec2.Zero;
        if (_cards.Count > 0)
        {
            size = new Vec2(_cards[0].width, _cards[0].height);
            cardPos = new Vec2(base.X - (size.X + 4f) + _indexSlide * (size.X + 4f), yDraw);
        }
        int curCard = 0;
        for (int i = _levelIndex - 1; i < _levelIndex + (_numCardsPerScreen + 1); i++)
        {
            if (i >= 0 && i < _cards.Count)
            {
                Card card = _cards[i];
                float a = 1f;
                if (curCard == _numCardsPerScreen + 1)
                {
                    a = Math.Abs(_indexSlide);
                }
                else if (curCard == _numCardsPerScreen && _indexSlide > 0f)
                {
                    a = 1f - Math.Abs(_indexSlide);
                }
                else
                {
                    switch (curCard)
                    {
                        case 0:
                            a = Math.Abs(_indexSlide);
                            break;
                        case 1:
                            if (_indexSlide < 0f)
                            {
                                a = 1f - Math.Abs(_indexSlide);
                            }
                            break;
                    }
                }
                card.Draw(cardPos, _selected && i == _selectedCardIndex, a);
            }
            cardPos.X += size.X + 4f;
            curCard++;
        }
        Sprite arrow = _arrow;
        float num = (_arrow.ScaleY = 0.25f);
        arrow.ScaleX = num;
        _arrow.Depth = 0.98f;
        if (_levelIndex + _numCardsPerScreen < _cards.Count)
        {
            _arrow.flipH = false;
            Graphics.Draw(_arrow, 312f, yDraw + size.Y / 2f);
        }
        if (_levelIndex > 0)
        {
            _arrow.flipH = true;
            Graphics.Draw(_arrow, 8f, yDraw + size.Y / 2f);
        }
    }
}
