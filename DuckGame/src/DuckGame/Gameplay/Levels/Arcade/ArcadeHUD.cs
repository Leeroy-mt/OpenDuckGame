using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class ArcadeHUD : Thing
{
    private BitmapFont _font;

    private Sprite _titleWing;

    private ChallengeGroup _activeChallengeGroup;

    private int _selected;

    private float _lerpOffset;

    private float _oldLerpOffset;

    private bool _goBack;

    private ChallengeCard _viewing;

    public bool quitOut;

    public bool launchChallenge;

    private bool _afterChallenge;

    private float _afterChallengeWait = 1f;

    public static bool open;

    private static float _curAlpha;

    private int _giveTickets;

    private List<ChallengeCard> _cards = new List<ChallengeCard>();

    private ChallengeCard _lastPlayed;

    public ChallengeGroup activeChallengeGroup
    {
        get
        {
            return _activeChallengeGroup;
        }
        set
        {
            _activeChallengeGroup = value;
            _cards.Clear();
            foreach (string challenge in _activeChallengeGroup.challenges)
            {
                ChallengeCard newCard = new ChallengeCard(0f, 0f, Challenges.GetChallenge(challenge));
                if (Level.current is ArcadeLevel && (Level.current as ArcadeLevel).customMachine != null)
                {
                    newCard.testing = true;
                }
                _cards.Add(newCard);
            }
            UnlockChallenges();
            _cards[0].hover = true;
            _selected = 0;
        }
    }

    public ChallengeCard selected
    {
        get
        {
            return _viewing;
        }
        set
        {
            _viewing = value;
        }
    }

    public static float alphaVal => _curAlpha;

    public override bool visible
    {
        get
        {
            if (!(base.Alpha < 0.01f))
            {
                return base.visible;
            }
            return false;
        }
        set
        {
            base.visible = value;
        }
    }

    public void FinishChallenge()
    {
        _afterChallengeWait = 0f;
        _lastPlayed = null;
        _afterChallenge = false;
    }

    public ArcadeHUD()
    {
        _font = new BitmapFont("biosFont", 8);
        base.layer = Layer.HUD;
        _titleWing = new Sprite("arcade/titleWing");
    }

    public override void Initialize()
    {
    }

    public void MakeActive()
    {
        if (!_afterChallenge)
        {
            HUD.CloseAllCorners();
            HUD.AddCornerCounter(HUDCorner.BottomMiddle, "@TICKET@ ", new FieldBinding(Profiles.active[0], "ticketCount"), 0, animateCount: true);
            HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@BACK");
            HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@SELECT");
        }
        else
        {
            HUD.CloseAllCorners();
            HUD.AddCornerCounter(HUDCorner.BottomMiddle, "@TICKET@ ", new FieldBinding(Profiles.active[0], "ticketCount"), 0, animateCount: true);
        }
    }

    public void MakeConfetti()
    {
        for (int i = 0; i < 40; i++)
        {
            Level.Add(new ChallengeConfetti((float)(i * 8) + Rando.Float(-10f, 10f), -124f + Rando.Float(110f)));
        }
        SFX.Play("dacBang", 1f, -0.7f);
    }

    public void UnlockChallenges(bool animate = false)
    {
        bool first = true;
        bool prevWon = false;
        foreach (ChallengeCard card in _cards)
        {
            if (first)
            {
                card.unlocked = true;
            }
            else if (!card.unlocked && prevWon)
            {
                card.unlocked = prevWon;
                if (animate)
                {
                    card.UnlockAnimation();
                }
            }
            prevWon = Profiles.active[0].GetSaveData(card.challenge.levelID).trophy != TrophyType.Baseline;
            first = false;
        }
    }

    public bool CanUnlockChallenges()
    {
        bool first = true;
        bool prevWon = false;
        foreach (ChallengeCard card in _cards)
        {
            if (first && !card.unlocked)
            {
                return true;
            }
            if (prevWon && !card.unlocked)
            {
                return true;
            }
            prevWon = Profiles.active[0].GetSaveData(card.challenge.levelID).trophy != TrophyType.Baseline;
            first = false;
        }
        return false;
    }

    public override void Update()
    {
        _curAlpha = base.Alpha;
        if (launchChallenge)
        {
            _afterChallenge = true;
            _afterChallengeWait = 1f;
            return;
        }
        if (base.Alpha > 0.95f)
        {
            open = true;
        }
        else
        {
            open = false;
        }
        if (!(base.Alpha > 0.01f))
        {
            return;
        }
        int index = 0;
        if (!_afterChallenge && !_goBack)
        {
            if (_viewing == null)
            {
                if (Input.Pressed("MENUDOWN"))
                {
                    _selected++;
                    if (_selected >= _cards.Count)
                    {
                        _selected = _cards.Count - 1;
                    }
                    else
                    {
                        SFX.Play("menuBlip01");
                    }
                }
                else if (Input.Pressed("MENUUP"))
                {
                    _selected--;
                    if (_selected < 0)
                    {
                        _selected = 0;
                    }
                    else
                    {
                        SFX.Play("menuBlip01");
                    }
                }
                else if (Input.Pressed("SELECT"))
                {
                    index = 0;
                    bool canselect = false;
                    foreach (ChallengeCard card in _cards)
                    {
                        if (index == _selected)
                        {
                            if (card.unlocked)
                            {
                                canselect = true;
                            }
                            break;
                        }
                        index++;
                    }
                    index = 0;
                    if (canselect)
                    {
                        foreach (ChallengeCard card2 in _cards)
                        {
                            if (index == _selected)
                            {
                                card2.expand = true;
                                card2.contract = false;
                                _lerpOffset = card2.Y;
                                _viewing = card2;
                                _oldLerpOffset = card2.Y;
                                SFX.Play("menu_select");
                            }
                            else
                            {
                                card2.contract = true;
                                card2.expand = false;
                            }
                            index++;
                        }
                    }
                    else
                    {
                        SFX.Play("scanFail");
                    }
                }
                else if (Input.Pressed("CANCEL"))
                {
                    SFX.Play("menu_back");
                    quitOut = true;
                }
            }
            else if (Input.Pressed("CANCEL"))
            {
                SFX.Play("menu_back");
                foreach (ChallengeCard card4 in _cards)
                {
                    card4.contract = false;
                    card4.expand = false;
                }
                _goBack = true;
            }
            else if (Input.Pressed("SELECT"))
            {
                if (!selected.unlocked)
                {
                    SFX.Play("scanFail");
                }
                else
                {
                    SFX.Play("selectItem");
                    launchChallenge = true;
                }
            }
        }
        if (_afterChallenge)
        {
            if (_afterChallengeWait > 0f)
            {
                _afterChallengeWait -= 0.03f;
            }
            else if (_lastPlayed == null)
            {
                _lastPlayed = selected;
                foreach (ChallengeCard card5 in _cards)
                {
                    card5.contract = false;
                    card5.expand = false;
                    _goBack = true;
                }
                _afterChallengeWait = 1f;
                SFX.Play("menu_back");
            }
            else if (_lastPlayed.HasNewBest() || _lastPlayed.HasNewTrophy())
            {
                _lastPlayed.GiveTime();
                _giveTickets = _lastPlayed.GiveTrophy();
                _afterChallengeWait = 1f;
                MakeConfetti();
            }
            else if (_giveTickets != 0)
            {
                Profiles.active[0].ticketCount += _giveTickets;
                _afterChallengeWait = 2f;
                _giveTickets = 0;
                SFX.Play("ching");
            }
            else if (CanUnlockChallenges())
            {
                UnlockChallenges(animate: true);
                _afterChallengeWait = 1f;
            }
            else
            {
                _afterChallengeWait = 0f;
                _lastPlayed = null;
                _afterChallenge = false;
                HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@BACK");
                HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@SELECT");
                Profiles.Save(Profiles.active[0]);
            }
        }
        if (_goBack)
        {
            _lerpOffset = Lerp.Float(_lerpOffset, _oldLerpOffset, 8f);
            if (_lerpOffset == _oldLerpOffset)
            {
                _goBack = false;
                _viewing = null;
            }
        }
        else if (_viewing != null)
        {
            _lerpOffset = Lerp.Float(_lerpOffset, 28f, 8f);
        }
        index = 0;
        foreach (ChallengeCard card3 in _cards)
        {
            if (index == _selected)
            {
                card3.hover = true;
            }
            else
            {
                card3.hover = false;
            }
            card3.Update();
            index++;
        }
    }

    public override void Draw()
    {
        if (!(base.Alpha > 0.01f) || _activeChallengeGroup == null)
        {
            return;
        }
        float ypos = 16f;
        string name = _activeChallengeGroup.GetNameForDisplay();
        _font.Alpha = base.Alpha;
        float wide = _font.GetWidth(name);
        _font.Draw(name, 160f - wide / 2f, ypos, Color.White);
        _titleWing.Alpha = base.Alpha;
        _titleWing.flipH = false;
        _titleWing.X = 160f - wide / 2f - (float)(_titleWing.width + 1);
        _titleWing.Y = ypos;
        _titleWing.Draw();
        _titleWing.flipH = true;
        _titleWing.X = 160f + wide / 2f + (float)_titleWing.width;
        _titleWing.Y = ypos;
        _titleWing.Draw();
        int index = 0;
        foreach (ChallengeCard card in _cards)
        {
            card.Alpha = base.Alpha;
            if (index == _selected && card == _viewing)
            {
                card.Position = new Vector2(31f, _lerpOffset);
            }
            else
            {
                card.Position = new Vector2(31f, ypos + 12f + (float)(index * 44));
            }
            card.Draw();
            index++;
        }
    }
}
