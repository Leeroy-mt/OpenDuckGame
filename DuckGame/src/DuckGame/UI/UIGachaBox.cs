using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UIGachaBox : UIMenu
{
    private Sprite _frame;

    private BitmapFont _font;

    private FancyBitmapFont _fancyFont;

    private SpriteMap _gachaEgg;

    private Sprite _furni;

    private Sprite _star;

    private Furniture _contains;

    private float gachaY;

    private float gachaSpeed;

    public static bool useNumGachas;

    private bool _flash;

    private float yOffset = 150f;

    public bool down = true;

    private float _downWait = 1f;

    private UIMenu _openOnClose;

    private SpriteMap _duckCoin;

    private bool _rare;

    private bool _rareCapsule;

    private string _oldSong;

    private bool played;

    private float _gachaWait;

    private float _openWait;

    public bool finished;

    private bool opened;

    private float _swapWait;

    private bool _swapped;

    private float _starGrow;

    private float _insertCoin;

    private float _insertCoinInc;

    private float _afterInsertWait;

    private bool _chinged;

    private bool doubleUpdating;

    private List<string> numberNames = new List<string> { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };

    public static Furniture GetRandomFurniture(int minRarity)
    {
        return GetRandomFurniture(minRarity, 1)[0];
    }

    public static List<Furniture> GetRandomFurniture(int minRarity, int num, float rarityMult = 1f, bool gacha = false, int numDupes = 0, bool avoidDupes = false, bool rareDupesChance = false)
    {
        List<Furniture> ret = new List<Furniture>();
        IOrderedEnumerable<Furniture> furnis = null;
        furnis = from x in RoomEditor.AllFurnis()
                 where x.rarity >= minRarity
                 orderby Rando.Int(999999)
                 select x;
        for (int i = 0; i < num; i++)
        {
            Furniture winner = null;
            Furniture lowest = null;
            Furniture absoluteLowest = null;
            List<int> dealtRarities = new List<int>();
            foreach (Furniture f in furnis)
            {
                if (gacha && !f.canGetInGacha)
                {
                    continue;
                }
                if (absoluteLowest == null)
                {
                    absoluteLowest = f;
                }
                bool dupe = Profiles.experienceProfile.GetNumFurnitures(f.index) > 0;
                int chanceForDupe = 35;
                if (f.rarity >= Rarity.VeryRare)
                {
                    chanceForDupe = 25;
                }
                if (f.rarity >= Rarity.SuperRare)
                {
                    chanceForDupe = 10;
                }
                if (useNumGachas && Global.data.numGachas % 8 == 0)
                {
                    chanceForDupe *= 4;
                }
                if (avoidDupes)
                {
                    chanceForDupe *= 8;
                }
                if (dupe && (f.type != FurnitureType.Prop || (Rando.Int(chanceForDupe) != 0 && numDupes <= 0)))
                {
                    continue;
                }
                if (lowest == null)
                {
                    lowest = f;
                }
                if (Profiles.experienceProfile.GetNumFurnitures(f.index) <= 0 || Rando.Int(2) == 0)
                {
                    dealtRarities.Add(f.rarity);
                }
                if (lowest == null || f.rarity < lowest.rarity)
                {
                    lowest = f;
                }
                if (absoluteLowest == null || f.rarity < absoluteLowest.rarity)
                {
                    absoluteLowest = f;
                }
                if (winner == null || f.rarity > winner.rarity)
                {
                    int roll = f.rarity;
                    if (rareDupesChance && dupe && f.rarity > minRarity)
                    {
                        roll = (int)((float)roll * 0.5f);
                    }
                    if (f.rarity == Rarity.Common || Rando.Int((int)((float)roll * rarityMult)) == 0)
                    {
                        winner = f;
                    }
                }
            }
            if (winner == null)
            {
                winner = lowest;
            }
            if (winner == null)
            {
                winner = absoluteLowest;
            }
            if (Profiles.experienceProfile.GetNumFurnitures(winner.index) > 0)
            {
                numDupes--;
            }
            ret.Add(winner);
            if (i != num - 1)
            {
                furnis = from x in furnis
                         where x != winner
                         orderby Rando.Int(999999)
                         select x;
            }
        }
        return ret;
    }

    public UIGachaBox(float xpos, float ypos, float wide = -1f, float high = -1f, bool rare = false, UIMenu openOnClose = null)
        : base("", xpos, ypos, wide, high)
    {
        _openOnClose = openOnClose;
        _rare = rare;
        _duckCoin = new SpriteMap("duckCoin", 18, 18);
        _duckCoin.CenterOrigin();
        Graphics.fade = 1f;
        _frame = new Sprite("unlockFrame");
        _frame.CenterOrigin();
        _furni = new Sprite("furni/tub");
        _furni.Center = new Vec2(_furni.width / 2, _furni.height);
        _star = new Sprite("prettyStar");
        _star.CenterOrigin();
        _font = new BitmapFont("biosFontUI", 8, 7);
        _fancyFont = new FancyBitmapFont("smallFontGacha");
        _gachaEgg = new SpriteMap("gachaEgg", 44, 36);
        bool rareMul = false;
        if (Rando.Int(10) == 5)
        {
            rareMul = true;
        }
        _contains = GetRandomFurniture(_rare ? Rarity.VeryVeryRare : Rarity.Common, 1, rareMul ? 0.75f : (_rare ? 0.75f : 1f), gacha: true)[0];
        _rareCapsule = _contains.rarity >= Rarity.VeryVeryRare;
        if (_rareCapsule)
        {
            _gachaEgg.frame = 36;
        }
        else
        {
            _gachaEgg.frame = Rando.Int(2) * 12;
            if (Rando.Int(1000) == 1)
            {
                _gachaEgg.frame += 9;
            }
            else if (Rando.Int(500) == 1)
            {
                _gachaEgg.frame += 6;
            }
            else if (Rando.Int(100) == 1)
            {
                _gachaEgg.frame += 3;
            }
        }
        _gachaEgg.CenterOrigin();
    }

    public override void OnClose()
    {
        Profiles.Save(Profiles.experienceProfile);
        if (_openOnClose != null)
        {
            MonoMain.pauseMenu = _openOnClose;
        }
    }

    public override void Open()
    {
        base.Open();
    }

    public override void UpdateParts()
    {
        if (!doubleUpdating && Input.Down("SELECT"))
        {
            doubleUpdating = true;
            UpdateParts();
            doubleUpdating = false;
        }
        if (yOffset < 1f)
        {
            if (_insertCoin < 1f)
            {
                _insertCoinInc += 0.008f;
                _insertCoin += _insertCoinInc;
            }
            else
            {
                if (!_chinged)
                {
                    SFX.Play("ching", 1f, Rando.Float(0.4f, 0.6f));
                    _chinged = true;
                }
                _insertCoin = 1f;
                if (_afterInsertWait < 1f)
                {
                    _afterInsertWait += 0.32f;
                }
                else
                {
                    if (_gachaWait >= 0.5f && !played)
                    {
                        played = true;
                        SFX.Play("gachaSound", 1f, Rando.Float(-0.1f, 0.1f));
                    }
                    _gachaWait += 0.1f;
                    if (_gachaWait >= 1f)
                    {
                        gachaSpeed += 0.25f;
                        if (gachaSpeed > 6f)
                        {
                            gachaSpeed = 6f;
                        }
                        gachaY += gachaSpeed;
                        if (gachaY > 50f && gachaSpeed > 0f)
                        {
                            if (gachaSpeed > 0.8f)
                            {
                                SFX.Play("gachaBounce", 1f, 0.2f);
                            }
                            gachaY = 50f;
                            gachaSpeed = (0f - gachaSpeed) * 0.4f;
                        }
                        _openWait += 0.019f;
                        if (_openWait >= 1f)
                        {
                            if (!opened)
                            {
                                opened = true;
                                SFX.Play("gachaOpen", 1f, Rando.Float(0.1f, 0.3f));
                                _gachaEgg.frame += 2;
                            }
                            _swapWait += 0.06f;
                            if (_swapWait >= 1f)
                            {
                                if (!_swapped)
                                {
                                    SFX.Play("harp");
                                    HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@CONTINUE");
                                    Profiles.experienceProfile.SetNumFurnitures(_contains.index, Profiles.experienceProfile.GetNumFurnitures(_contains.index) + 1);
                                }
                                _starGrow += 0.05f;
                                _swapped = true;
                            }
                        }
                    }
                }
            }
        }
        yOffset = Lerp.FloatSmooth(yOffset, down ? 150f : 0f, 0.4f, 1.1f);
        if (down)
        {
            if (_swapped)
            {
                finished = true;
                Close();
            }
            else
            {
                _downWait -= 0.06f;
                if (_downWait <= 0f)
                {
                    _downWait = 1f;
                    down = false;
                    SFX.Play("gachaGet", 1f, -0.4f);
                }
            }
        }
        if (_swapped && Input.Pressed("SELECT"))
        {
            HUD.CloseAllCorners();
            SFX.Play("resume", 0.6f);
            down = true;
        }
        base.UpdateParts();
    }

    public override void Draw()
    {
        base.Y += yOffset;
        _frame.Depth = -0.9f;
        Graphics.Draw(_frame, base.X, base.Y);
        _frame.Depth = -0.7f;
        Graphics.Draw(_frame, base.X, base.Y, new Rectangle(0f, 0f, 125f, 36f));
        if (_swapped)
        {
            _contains.Draw(Position + new Vec2(0f, 10f), -0.8f);
            if (_starGrow <= 1f)
            {
                _star.Depth = 0.9f;
                _star.Scale = new Vec2(2.5f + _starGrow * 3f);
                _star.Alpha = 1f - _starGrow;
                Graphics.Draw(_star, base.X, base.Y + 10f);
            }
        }
        else if (gachaY > 10f)
        {
            _gachaEgg.Depth = -0.8f;
            Graphics.Draw(_gachaEgg, base.X, base.Y - 38f + gachaY);
        }
        string text = "@LWING@NEW TOY@RWING@";
        if (_rare)
        {
            text = "@LWING@RARE TOY@RWING@";
        }
        Vec2 fontPos = new Vec2(0f - _font.GetWidth(text) / 2f, -42f);
        _font.DrawOutline(text, Position + fontPos, _rare ? Colors.DGYellow : Color.White, Color.Black, base.Depth + 2);
        string unlockText = "  ???  ";
        if (_swapped)
        {
            unlockText = "} " + _contains.name + " }";
        }
        _fancyFont.Scale = new Vec2(1f, 1f);
        Vec2 unlockFontPos = new Vec2(0f - _fancyFont.GetWidth(unlockText) / 2f, -25f);
        _fancyFont.DrawOutline(unlockText, Position + unlockFontPos, (_rare || (_swapped && _rareCapsule)) ? Colors.DGYellow : Color.White, Color.Black, base.Depth + 2);
        _fancyFont.Scale = new Vec2(0.5f, 0.5f);
        if (_insertCoin > 0.01f)
        {
            _duckCoin.frame = (_rare ? 1 : 0);
            _duckCoin.Depth = -0.8f;
            Graphics.Draw(_duckCoin, base.X + 40f, base.Y - 100f + _insertCoin * 65f);
        }
        if (_swapped)
        {
            string descriptionText = _contains.description;
            int num = Profiles.experienceProfile.GetNumFurnitures(_contains.index) - 1;
            if (num > 0)
            {
                descriptionText = "I've already got " + ((num - 1 >= numberNames.Count) ? num.ToString() : numberNames[num - 1]) + " of these...";
            }
            Vec2 descFontPos = new Vec2(0f - _fancyFont.GetWidth(descriptionText) / 2f, 38f);
            _fancyFont.DrawOutline(descriptionText, Position + descFontPos, (num > 0) ? Colors.DGYellow : Colors.DGGreen, Color.Black, base.Depth + 2, 0.5f);
        }
        base.Y -= yOffset;
    }
}
