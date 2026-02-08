using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UILevelBox : UIMenu
{
    #region Public Fields

    public static int currentLevel;

    public bool eggTalk;

    public int _eggFeedIndex;

    public float _xpProgress;

    public float _dayProgress;

    public string _overrideSlide;

    public KeyValuePair<string, XPPair> _currentStat;

    public List<string> _eggFeedLines =
    [
        "..", "...", "EGGS CANNOT\nTALK.", "EGGS CANNOT\nTALK..", "EGGS CANNOT\nTALK!", "I SAID, EGGS\nCANNOT TALK!", "LOOK AT YOURSELF,\nTRYING TO TALK\nTO AN EGG.", "I GUESS I'VE\nBEEN THERE.", "YOU KNOW, PEOPLE\nARE GONNA CALL\nYOU CRAZY..", "CRAZY FOR TALKING\nTO AN EGG.",
        "BUT IF IT WASN'T\nFOR DUCKS LIKE\nYOU, WELL..", "THIS EGG WOULD\nHAVE NOBODY\nTO TALK TO.", "PEOPLE DON'T\nREALLY TAKE EGGS\nSERIOUSLY.", "MAYBE THEY'RE\nTHINKING,", "WHAT WOULD AN\nEGG SAY? IT'S\nJUST AN EGG.", "EGGS DON'T\nKNOW MUCH OF\nANYTHING!", "WELL..\nTHAT MAY BE\nTRUE.", "I KNOW ONLY\nWHAT'S INSIDE\nMY SHELL!", "BUT..", "EGGS HAVE\nA LOT OF TIME\nTO THINK ABOUT\nTHINGS.",
        "I THINK A LOT\nABOUT HOW TO\nBE A GOOD EGG.", "...", "I KNOW THAT\nI SHOULD BE\nKIND.", "KIND TO\nEVERYONE, EGG\nOR OTHERWISE.", "AND THE KEY\nTO KINDNESS IS\nREMEMBERING:", "WE DON'T KNOW\nWHAT'S GOING ON\nINSIDE ANYONE\nELSE'S SHELL!", "...", "PEOPLE WHO THINK\nTHEY KNOW,", "THEY MIGHT THINK\nTHAT EGGS DON'T\nTALK!", "...",
        "WELL, THANKS FOR\nLISTENING TO\nTHIS EGG.", "THANKS FOR\nCARING ABOUT\nWHAT GOES ON\nIN MY SHELL.", "THAT'S ALL I\nHAVE TO SAY.", "I NEED TO\nGET BACK TO\nGROWING UP!", "SO LONG BUDDY!"
    ];

    #endregion

    #region Private Fields

    bool _finishEat;
    bool _burp;
    bool _stampCard;
    bool _talking;
    bool _alwaysClose;
    bool queryVisitShop;
    bool close;
    bool _sandwichShift;
    bool _finned;
    bool _showCard;
    bool _firstParticleIn;
    bool _attemptingVincentClose;
    bool _gaveToy;
    bool doubleUpdating;
    bool _startedLittleManLeave;
    bool _finishingNewStamp;
    bool _popDay;
    bool _attemptingBuy;
    bool _unSlide;
    bool _inTaxi;
    bool _advancedDay;
    bool _markedNewDay;
    bool _gotEgg;
    bool _littleManLeave;
    bool _didSlide;
    bool _driveAway;
    bool _attemptingGive;
    bool playedSound;
    bool _prevShopOpen;
    bool skipping;
    bool skipUpdate;

    int milkNeed = 1400;
    int sandwichNeed = 500;
    int gachaNeed = 200;
    int _currentLevel = 4;
    int _desiredLevel = 4;
    int _newGrowthLevel = 1;
    int _xpValue;
    int _milkValue;
    int _roundsPlayed;
    int _sandwichValue;
    int _gachaValue;
    int _originalXP;
    int _totalXP;
    int _startRoundsPlayed;
    int _giveMoney;

    float _particleWait = 1;
    float _startWait = 1;
    float _coinLerp2 = 1;
    float _coinLerp = 1;
    float _drain = 1;
    float _talk;
    float _talkWait;
    float _finishTalkWait;
    float _sandwichLerp;
    float _dayTake;
    float _updateTimeWait;
    float _advanceDayWait;
    float _stampWait;
    float _stampWait2;
    float _stampWobble;
    float _stampWobbleSin;
    float _extraMouthOpen;
    float _openWait;
    float _sandwichEat;
    float _eatWait;
    float _afterEatWait;
    float _newXPValue;
    float _oldXPValue;
    float _slideXPBar;
    float _addLerp;
    float _newGachaValue;
    float _oldGachaValue;
    float _newSandwichValue;
    float _oldSandwichValue;
    float _newMilkValue;
    float _oldMilkValue;
    float _stampCardLerp;
    float _fallVel;
    float _intermissionWait;
    float _slideWait;
    float _giveMoneyRise;
    float _dayStartWait;
    float _dayFallAway;
    float _dayScroll;
    float _ranRot;
    float _littleManStartWait;
    float _finishDayWait;
    float _finalWait;
    float _coin2Wait;
    float _intermissionSlide;
    float _newCircleLerp;
    float _taxiDrive;
    float _genericWait;
    float _levelSlideWait;
    float _lastFill;
    float time;
    float _barHeat;

    string _talkLine = "";
    string _feedLine = "I AM A HUNGRY\nLITTLE MAN.";
    string _startFeedLine = "I AM A HUNGRY\nLITTLE MAN.";

    UILevelBoxState _state;

    Vector2 littleManPos;

    MenuBoolean _menuBool = new();
    ConstantSound _sound = new("chainsawIdle", 0, 0, "chainsawIdleMulti");
    BitmapFont _font;
    BitmapFont _thickBiosNum;
    FancyBitmapFont _fancyFont;
    Sprite _xpBar;
    Sprite _barFront;
    Sprite _addXPBar;
    SpriteMap _lev;
    SpriteMap _littleMan;
    Sprite _egg;
    Sprite _talkBubble;
    SpriteMap _duckCoin;
    Sprite _sandwich;
    Sprite _heart;
    SpriteMap _sandwichCard;
    SpriteMap _sandwichStamp;
    SpriteMap _weekDays;
    Sprite _taxi;
    SpriteMap _circle;
    SpriteMap _cross;
    SpriteMap _days;
    Sprite _gachaBar;
    Sprite _sandwichBar;
    Sprite _xpPoint;
    Sprite _xpPointOutline;
    SpriteMap _milk;
    BitmapFont _bigFont;

    List<Sprite> littleEggs = [];
    List<string> sayQueue = [];
    List<Sprite> _frames = [];
    List<XPPlus> _particles = [];
    List<LittleHeart> _hearts = [];

    #endregion

    #region Public Properties

    public static bool menuOpen
    {
        get
        {
            if (_confirmMenu != null)
                return _confirmMenu.open;
            return false;
        }
    }

    public static int gachas
    {
        get => MonoMain.core.gachas;
        set => MonoMain.core.gachas = value;
    }

    public static int rareGachas
    {
        get => MonoMain.core.rareGachas;
        set => MonoMain.core.rareGachas = value;
    }

    public static UIMenu _confirmMenu
    {
        get => MonoMain.core._confirmMenu;
        set => MonoMain.core._confirmMenu = value;
    }

    #endregion

    static bool saidSpecial
    {
        get => MonoMain.core.saidSpecial;
        set => MonoMain.core.saidSpecial = value;
    }

    public UILevelBox(string title, float xpos, float ypos, float wide = -1, float high = -1, string conString = "")
        : base(title, xpos, ypos, wide, high, conString)
    {
        Graphics.fade = 1;
        _frames.Add(new Sprite("levWindow_lev0"));
        _frames[^1].CenterOrigin();
        _frames.Add(new Sprite("levWindow_lev1"));
        _frames[^1].CenterOrigin();
        _frames.Add(new Sprite("levWindow_lev2"));
        _frames[^1].CenterOrigin();
        _frames.Add(new Sprite("levWindow_lev4"));
        _frames[^1].CenterOrigin();
        _frames.Add(new Sprite("levWindow_lev4"));
        _frames[^1].CenterOrigin();
        _frames.Add(new Sprite("levWindow_lev5"));
        _frames[^1].CenterOrigin();
        _frames.Add(new Sprite("levWindow_lev6"));
        _frames[^1].CenterOrigin();
        _frames.Add(new Sprite("levWindow_lev6"));
        _frames[^1].CenterOrigin();
        _barFront = new Sprite("online/barFront");
        _barFront.Center = new Vector2(_barFront.w, 0);
        _addXPBar = new Sprite("online/xpAddBar");
        _addXPBar.CenterOrigin();
        _bigFont = new BitmapFont("intermissionFont", 24, 23);
        _littleMan = new SpriteMap("littleMan", 16, 16);
        _thickBiosNum = new BitmapFont("thickBiosNum", 16, 16);
        _font = new BitmapFont("biosFontUI", 8, 7);
        _fancyFont = new FancyBitmapFont("smallFont");
        _xpBar = new Sprite("online/xpBar");
        _gachaBar = new Sprite("online/gachaBar");
        _sandwichBar = new Sprite("online/sandwichBar");
        _duckCoin = new SpriteMap("duckCoin", 18, 18);
        _duckCoin.CenterOrigin();
        _sandwich = new Sprite("sandwich");
        _sandwich.CenterOrigin();
        _heart = new Sprite("heart");
        _heart.CenterOrigin();
        _milk = new SpriteMap("milk", 10, 22);
        _milk.CenterOrigin();
        _taxi = new Sprite("taxi");
        _taxi.CenterOrigin();
        _circle = new SpriteMap("circle", 27, 31);
        _circle.CenterOrigin();
        _cross = new SpriteMap("scribble", 26, 21);
        _cross.CenterOrigin();
        _days = new SpriteMap("calanderDays", 27, 31);
        _days.CenterOrigin();
        _weekDays = new SpriteMap("weekDays", 27, 31);
        _weekDays.CenterOrigin();
        _sandwichCard = new SpriteMap("sandwichCard", 115, 54);
        _sandwichCard.CenterOrigin();
        _sandwichStamp = new SpriteMap("sandwichStamp", 14, 14);
        _sandwichStamp.CenterOrigin();
        _xpPoint = new Sprite("online/xpPlus");
        _xpPoint.CenterOrigin();
        _xpPointOutline = new Sprite("online/xpPlusOutline");
        _xpPointOutline.CenterOrigin();
        _talkBubble = new Sprite("talkBubble");
        _lev = new SpriteMap("levs", 27, 14);
        _egg = Profile.GetEggSprite(Profiles.experienceProfile.numLittleMen, 0);
        _currentLevel = 0;
        _desiredLevel = 0;
        if (Profiles.experienceProfile != null)
        {
            _xpValue = Profiles.experienceProfile.xp;
            _newXPValue = (_oldXPValue = _xpValue);
            if (_xpValue >= DuckNetwork.GetLevel(9999).xpRequired)
                _desiredLevel = (_currentLevel = DuckNetwork.GetLevel(9999).num);
            else
            {
                while (_xpValue >= DuckNetwork.GetLevel(_desiredLevel + 1).xpRequired && _xpValue < DuckNetwork.GetLevel(9999).xpRequired)
                {
                    _desiredLevel++;
                    _currentLevel++;
                }
            }
            if (_desiredLevel >= 3)
            {
                _gachaValue = (_xpValue - DuckNetwork.GetLevel(3).xpRequired) % gachaNeed;
                _newGachaValue = _gachaValue;
                _oldGachaValue = _gachaValue;
            }
            if (_desiredLevel >= 4)
            {
                _sandwichValue = (_xpValue - DuckNetwork.GetLevel(4).xpRequired) % sandwichNeed;
                _newSandwichValue = _sandwichValue;
                _oldSandwichValue = _sandwichValue;
            }
            if (_desiredLevel >= 7)
            {
                _milkValue = (_xpValue - DuckNetwork.GetLevel(7).xpRequired) % milkNeed;
                _newMilkValue = _milkValue;
                _oldMilkValue = _milkValue;
            }
        }
        _newGrowthLevel = Profiles.experienceProfile.littleManLevel;
        gachas = 0;
        rareGachas = 0;
        _roundsPlayed = Profiles.experienceProfile.roundsSinceXP;
        _startRoundsPlayed = _roundsPlayed;
        Profiles.experienceProfile.roundsSinceXP = 0;
        time = Profiles.experienceProfile.timeOfDay;
        SFX.Play("pause");
        _totalXP = DuckNetwork.GetTotalXPEarned();
        _originalXP = _xpValue;
        if (Profiles.experienceProfile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0)
        {
            HUD.CloseAllCorners();
            HUD.AddCornerControl(HUDCorner.BottomLeft, "@START@SKIP XP");
        }
    }

    #region Public Methods

    public static int LittleManFrame(int idx, int curLev, ulong seed = 0uL, bool bottomBar = false)
    {
        if (seed == 0 && Profiles.experienceProfile != null)
            seed = Profiles.experienceProfile.steamID;
        if (bottomBar && idx < Profiles.experienceProfile.numLittleMen - 6)
            curLev = 7;
        int rlLev = curLev - 3;
        if (curLev < 0)
            rlLev = 2 + new Random((int)((long)seed + idx)).Next(0, 2);
        Random oldGen = Rando.generator;
        Rando.generator = Profile.GetLongGenerator(seed);
        for (int i = 0; i < idx * 4; i++)
            Rando.Int(100);
        int baseNum = Rando.Int(5) * 20;
        int baseInc = 0;
        switch (rlLev)
        {
            case 1:
                baseInc = Rando.Int(3);
                break;
            case 2:
                baseInc = Rando.Int(3);
                break;
            case 3:
                baseInc = Rando.Int(2);
                break;
        }
        for (int j = 0; j < curLev; j++)
            Rando.Int(10);
        if (Rando.Int(30) == 0)
            baseNum = (baseNum + 60) % 120;
        baseNum += rlLev;
        baseNum += baseInc * 5;
        if (bottomBar && idx < Profiles.experienceProfile.numLittleMen - 6)
            baseNum += (1 + Rando.Int(2)) * 5;
        Rando.generator = oldGen;
        return baseNum;
    }

    public override void Close()
    {
        base.Close();
        HUD.ClearCorners();
    }

    public override void OnClose()
    {
        if (!FurniShopScreen.open && _prevShopOpen)
            Music.Play("CharacterSelect");
        DuckNetwork.finishedMatch = false;
        DuckNetwork._xpEarned.Clear();
        Profiles.experienceProfile.xp = _xpValue;
        Profiles.Save(Profiles.experienceProfile);
        UIMenu lastBox = null;
        Graphics.fadeAdd = 0;
        Graphics.flashAdd = 0;
        if (Unlockables.HasPendingUnlocks())
            lastBox = new UIUnlockBox([.. Unlockables.GetPendingUnlocks()], Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 190);
        if (rareGachas > 0 || gachas > 0)
        {
            UIGachaBoxNew.skipping = skipping;
            UIGachaBoxNew box = new(Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 190, -1, rare: true, lastBox);
            if (!skipping)
                lastBox = box;
        }
        Global.Save();
        Profiles.Save(Profiles.experienceProfile);
        if (_gotEgg && Profiles.experienceProfile.numLittleMen > 8)
        {
            UIWillBox will = new(UIGachaBox.GetRandomFurniture(Rarity.SuperRare, 1, 0.3f)[0], Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 190, -1, lastBox);
            lastBox = new UIFuneral(Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 190, -1, will);
        }
        if (lastBox != null)
            MonoMain.pauseMenu = lastBox;
    }

    public override void UpdateParts()
    {
        if (!FurniShopScreen.open && _prevShopOpen)
            Music.Play("CharacterSelect");
        _prevShopOpen = FurniShopScreen.open;
        InputProfile.repeat = false;
        Keyboard.repeat = false;
        base.UpdateParts();
        _sound.Update();
        currentLevel = _currentLevel;
        _confirmMenu?.DoUpdate();
        while (littleEggs.Count < Math.Min(Profiles.experienceProfile.numLittleMen, 8))
            littleEggs.Add(Profile.GetEggSprite(Math.Max(0, Profiles.experienceProfile.numLittleMen - 9) + littleEggs.Count, 0));
        if ((Input.Pressed("SELECT") || (!FurniShopScreen.open && Input.Pressed("START") && Profiles.experienceProfile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0)) && _finned)
        {
            if (Input.Pressed("START") && Profiles.experienceProfile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0 && (!skipping || _finned))
            {
                SFX.skip = false;
                SFX.Play("dacBang");
                skipping = true;
            }
            FurniShopScreen.open = false;
            Vincent.Clear();
            Close();
            _finned = false;
            SFX.Play("resume");
        }
        if (queryVisitShop && _menuBool.value)
        {
            Vincent.Open(GetDay(Profiles.experienceProfile.currentDay));
            FurniShopScreen.open = true;
            queryVisitShop = false;
        }
        if (FurniShopScreen.open && !Vincent.showingDay)
        {
            if (skipping)
                FurniShopScreen.close = true;
            if (FurniShopScreen.close)
            {
                FurniShopScreen.close = false;
                FurniShopScreen.open = false;
                Vincent.Clear();
                _menuBool.value = false;
                _state = UILevelBoxState.UpdateTime;
            }
            if (FurniShopScreen.giveYoYo)
            {
                if (!_attemptingGive)
                {
                    HUD.CloseAllCorners();
                    _menuBool.value = false;
                    _confirmMenu = new UIPresentBox(RoomEditor.GetFurniture("YOYO"), Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 190)
                    {
                        Depth = 0.98f
                    };
                    _attemptingGive = true;
                }
                if (_attemptingGive && _confirmMenu != null && !_confirmMenu.open)
                {
                    _confirmMenu = null;
                    _attemptingGive = false;
                    FurniShopScreen.giveYoYo = false;
                }
            }
            else if (FurniShopScreen.giveVooDoo)
            {
                if (!_attemptingGive)
                {
                    HUD.CloseAllCorners();
                    _menuBool.value = false;
                    _confirmMenu = new UIPresentBox(RoomEditor.GetFurniture("VOODOO VINCENT"), Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 190);
                    _confirmMenu.Depth = 0.98f;
                    _attemptingGive = true;
                }
                if (_attemptingGive && _confirmMenu != null && !_confirmMenu.open)
                {
                    _confirmMenu = null;
                    _attemptingGive = false;
                    FurniShopScreen.giveVooDoo = false;
                }
            }
            else if (FurniShopScreen.givePerimeterDefence)
            {
                if (!_attemptingGive)
                {
                    HUD.CloseAllCorners();
                    _menuBool.value = false;
                    _confirmMenu = new UIPresentBox(RoomEditor.GetFurniture("PERIMETER DEFENCE"), Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 190);
                    _confirmMenu.Depth = 0.98f;
                    _attemptingGive = true;
                }
                if (_attemptingGive && _confirmMenu != null && !_confirmMenu.open)
                {
                    _confirmMenu = null;
                    _attemptingGive = false;
                    FurniShopScreen.givePerimeterDefence = false;
                }
            }
            else if (FurniShopScreen.attemptBuy != null)
            {
                if (!_attemptingBuy)
                {
                    _menuBool.value = false;
                    _confirmMenu = new UIMenu((Vincent.type == DayType.PawnDay) ? "SELL TO VINCENT?" : "BUY FROM VINCENT?", Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 230, -1, Vincent.type == DayType.PawnDay ? "@CANCEL@CANCEL @SELECT@SELECT" : "@CANCEL@CANCEL @SELECT@SELECT");
                    _confirmMenu.Add(new UIText(FurniShopScreen.attemptBuy.name, Color.Green));
                    _confirmMenu.Add(new UIText(" ", Color.White));
                    _confirmMenu.Add(new UIText(" ", Color.White));
                    _confirmMenu.Add(new UIMenuItem(Vincent.type == DayType.PawnDay ? $"SELL |WHITE|(|LIME|${FurniShopScreen.attemptBuy.cost}|WHITE|)" : $"BUY |WHITE|(|LIME|${FurniShopScreen.attemptBuy.cost}|WHITE|)", new UIMenuActionCloseMenuSetBoolean(_confirmMenu, _menuBool)));
                    _confirmMenu.Add(new UIMenuItem("CANCEL", new UIMenuActionCloseMenu(_confirmMenu), UIAlign.Center, Colors.MenuOption, backButton: true));
                    _confirmMenu.Depth = 0.98f;
                    _confirmMenu.DoInitialize();
                    _confirmMenu.Close();
                    for (int i = 0; i < 10; i++)
                        _confirmMenu.DoUpdate();
                    _confirmMenu.Open();
                    _attemptingBuy = true;
                }
                if (_attemptingBuy && _confirmMenu != null && !_confirmMenu.open)
                {
                    if (_menuBool.value)
                    {
                        _attemptingBuy = false;
                        _confirmMenu = null;
                        SFX.Play("ching");
                        if (Vincent.type == DayType.PawnDay)
                        {
                            Profiles.experienceProfile.littleManBucks += FurniShopScreen.attemptBuy.cost;
                            Profiles.experienceProfile.SetNumFurnitures(FurniShopScreen.attemptBuy.furnitureData.index, Profiles.experienceProfile.GetNumFurnitures(FurniShopScreen.attemptBuy.furnitureData.index) - 1);
                            foreach (Profile p in Profiles.all)
                            {
                                FurniturePosition remove;
                                do
                                {
                                    remove = null;
                                    foreach (FurniturePosition pos in p.furniturePositions)
                                    {
                                        if (p.GetNumFurnituresPlaced(pos.id) > Profiles.experienceProfile.GetNumFurnitures(pos.id))
                                        {
                                            remove = pos;
                                            break;
                                        }
                                    }
                                    p.furniturePositions.Remove(remove);
                                }
                                while (remove != null);
                            }
                            Vincent.Clear();
                            Vincent.Add("|POINT|THANKS! |CONCERNED|I DON'T REGRET BUYING THIS AT ALL...");
                        }
                        else
                        {
                            Profiles.experienceProfile.littleManBucks -= FurniShopScreen.attemptBuy.cost;
                            if (FurniShopScreen.attemptBuy.furnitureData != null)
                                Profiles.experienceProfile.SetNumFurnitures(FurniShopScreen.attemptBuy.furnitureData.index, Profiles.experienceProfile.GetNumFurnitures(FurniShopScreen.attemptBuy.furnitureData.index) + 1);
                            else if (FurniShopScreen.attemptBuy.teamData != null)
                                Global.boughtHats.Add(FurniShopScreen.attemptBuy.teamData.name);
                        }
                        Vincent.Sold();
                        if (Vincent.products.Count == 1 && Vincent.type != DayType.PawnDay)
                        {
                            FurniShopScreen.open = false;
                            Vincent.Clear();
                            _state = UILevelBoxState.UpdateTime;
                        }
                        FurniShopScreen.attemptBuy = null;
                    }
                    else
                    {
                        _attemptingBuy = false;
                        _confirmMenu = null;
                        if (Vincent.type == DayType.PawnDay)
                        {
                            Vincent.Clear();
                            Vincent.Add("|CONCERNED|HAVING SECOND THOUGHTS ABOUT SELLING THAT, HUH?");
                        }
                        FurniShopScreen.attemptBuy = null;
                        skipUpdate = true;
                    }
                }
            }
            else
            {
                if (!_attemptingVincentClose && Input.Pressed("CANCEL") && !Vincent._willGiveVooDoo && !Vincent._willGiveYoYo && !Vincent._willGivePerimeterDefence)
                {
                    _menuBool.value = false;
                    _confirmMenu = new UIMenu("LEAVE VINCENT?", Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 160, -1, "@SELECT@SELECT")
                    {
                        Depth = 0.98f
                    };
                    _confirmMenu.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuSetBoolean(_confirmMenu, _menuBool)));
                    _confirmMenu.Add(new UIMenuItem("NO!", new UIMenuActionCloseMenu(_confirmMenu), UIAlign.Center, default, backButton: true));
                    _confirmMenu.DoInitialize();
                    _confirmMenu.Close();
                    for (int j = 0; j < 10; j++)
                        _confirmMenu.DoUpdate();
                    _confirmMenu.Open();
                    _attemptingVincentClose = true;
                }
                if (_attemptingVincentClose && _confirmMenu != null && !_confirmMenu.open)
                {
                    if (_menuBool.value)
                    {
                        FurniShopScreen.open = false;
                        Vincent.Clear();
                        _menuBool.value = false;
                        _state = UILevelBoxState.UpdateTime;
                    }
                    else
                    {
                        _attemptingVincentClose = false;
                        _confirmMenu = null;
                    }
                }
            }
        }
        if (FurniShopScreen.open && !skipUpdate)
            Vincent.Update();
        skipUpdate = false;
        if ((FurniShopScreen.open && !Vincent.showingDay) || (_confirmMenu != null && _confirmMenu.open))
            return;
        if (!doubleUpdating && Input.Down("SELECT"))
        {
            doubleUpdating = true;
            UpdateParts();
            UpdateParts();
            doubleUpdating = false;
        }
        if (Input.Pressed("START") && !_littleManLeave && Profiles.experienceProfile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0 && !FurniShopScreen.open)
            skipping = true;
        if (!doubleUpdating && skipping)
        {
            doubleUpdating = true;
            for (int k = 0; k < 200; k++)
            {
                SFX.skip = true;
                UpdateParts();
                SFX.skip = false;
                if (_littleManLeave || !skipping)
                    break;
            }
            doubleUpdating = false;
            if (_finned)
            {
                Graphics.fadeAdd = 1;
                skipping = false;
                SFX.skip = false;
                SFX.Play("dacBang");
            }
        }
        if (_genericWait > 0f)
        {
            _genericWait -= Maths.IncFrameTimer();
            return;
        }
        _overrideSlide = null;
        if (_desiredLevel != _currentLevel || _newGrowthLevel != Profiles.experienceProfile.littleManLevel)
        {
            if (!_didSlide)
            {
                _levelSlideWait += 0.08f;
                if (_levelSlideWait < 1)
                    return;
                int curLev = _currentLevel;
                if (Profiles.experienceProfile.numLittleMen > 0)
                    curLev = Profiles.experienceProfile.littleManLevel;
                if (!_unSlide && !playedSound)
                {
                    SFX.Play("dukget");
                    playedSound = true;
                }
                _overrideSlide = "GROWING UP";
                if (curLev == 1)
                {
                    _overrideSlide = "EGG GET";
                    _gotEgg = true;
                }
                if (curLev == 2)
                    _overrideSlide = "EGG HATCH";
                if (curLev == 6)
                    _overrideSlide = "GROWN UP";
                if (curLev == 7)
                    _overrideSlide = "MAX OUT!";
                if (_unSlide)
                {
                    _intermissionSlide = Lerp.FloatSmooth(_intermissionSlide, 0, 0.45f);
                    if (_intermissionSlide <= 0.01f)
                    {
                        playedSound = false;
                        _unSlide = false;
                        _didSlide = true;
                        _intermissionSlide = 0;
                        SFX.Play("levelUp");
                    }
                    return;
                }
                _intermissionSlide = Lerp.FloatSmooth(_intermissionSlide, 1, 0.21f, 1.05f);
                if (_intermissionSlide >= 1)
                {
                    _slideWait += 0.08f;
                    if (_slideWait >= 1)
                    {
                        _unSlide = true;
                        _slideWait = 0;
                    }
                }
                return;
            }
            _levelSlideWait = 0;
            Graphics.fadeAdd = Lerp.Float(Graphics.fadeAdd, 1, 0.1f);
            if (Graphics.fadeAdd < 1)
                return;
            _didSlide = false;
            _currentLevel = _desiredLevel;
            _talkLine = "";
            _feedLine = "I AM A HUNGRY\nLITTLE MAN.";
            _startFeedLine = _feedLine;
            Profiles.experienceProfile.littleManLevel = _newGrowthLevel;
            int curLev2 = _currentLevel;
            if (Profiles.experienceProfile.numLittleMen > 0)
                curLev2 = Profiles.experienceProfile.littleManLevel;
            if (_currentLevel == 3)
            {
                _oldGachaValue = _gachaValue;
                _newGachaValue = _gachaValue + (_newXPValue - _xpValue);
            }
            if (_currentLevel == 4)
            {
                _oldSandwichValue = _sandwichValue;
                _newSandwichValue = _sandwichValue + (_newXPValue - _xpValue);
            }
            if (_currentLevel >= 7)
            {
                _oldMilkValue = _milkValue;
                _newMilkValue = _milkValue + (_newXPValue - _xpValue);
            }
            _ = _currentLevel;
            _ = 3;
            if (curLev2 >= 7)
            {
                _littleManLeave = true;
                HUD.CloseAllCorners();
            }
        }
        else
        {
            Graphics.fadeAdd = Lerp.Float(Graphics.fadeAdd, 0, 0.1f);
            if (Graphics.fadeAdd > 0.01f)
                return;
        }
        if (!_talking)
            _talk = 0;
        else
        {
            _talkWait += 0.2f;
            if (_talkWait >= 1)
            {
                _talkWait = 0;
                if (_feedLine.Length > 0)
                {
                    _talkLine += _feedLine[0];
                    if (_feedLine[0] == '.' || _feedLine[0] == '!' || _feedLine[0] == '?')
                    {
                        SFX.Play("tinyTick", 1, Rando.Float(-0.1f, 0.1f));
                        _talkWait = -2;
                    }
                    else
                        SFX.Play("tinyNoise1", 1, Rando.Float(-0.1f, 0.1f));
                    _feedLine = _feedLine[1..];
                }
            }
            _alwaysClose = false;
            if (_talking && _talkLine == _startFeedLine)
            {
                _alwaysClose = true;
                _finishTalkWait += 0.1f;
                if (_finishTalkWait > 6 && !eggTalk)
                {
                    _talking = false;
                    _talkLine = "";
                    _finishTalkWait = 0;
                }
            }
            if (_talkLine.Length > 0 && _talkLine[^1] == '.')
                _alwaysClose = true;
            _talk += !close ? 0.2f : -0.2f;
            if (_talk > 2)
            {
                _talk = 2;
                close = true;
            }
            if (_talk < 0)
            {
                _talk = 0;
                if (!_alwaysClose)
                    close = false;
            }
        }
        if (sayQueue.Count > 0 && !_talking)
        {
            _talking = true;
            _talkLine = "";
            _feedLine = sayQueue.First();
            sayQueue.RemoveAt(0);
            _startFeedLine = _feedLine;
        }
        if (_littleManLeave)
        {
            if (skipping)
            {
                skipping = false;
                SFX.skip = false;
                SFX.Play("dacBang");
                Graphics.fadeAdd = 1;
            }
            _littleManStartWait += 0.02f;
            if (!(_littleManStartWait >= 1))
                return;

            if (_driveAway)
            {
                _sound.lerpVolume = 1;
                _taxiDrive = Lerp.Float(_taxiDrive, 1, 0.03f);
                if (_taxiDrive >= 1)
                {
                    SFX.Play("doorOpen");
                    _driveAway = false;
                    _genericWait = 0.5f;
                    ConstantSound sound = _sound;
                    sound.volume = _sound.lerpVolume = 0;
                }
            }
            else if (_taxiDrive > 0)
            {
                if (!_inTaxi)
                {
                    SFX.Play("doorClose");
                    _inTaxi = true;
                    _genericWait = 0.5f;
                    return;
                }
                _sound.lerpVolume = 1;
                _taxiDrive = Lerp.Float(_taxiDrive, 2, 0.03f);
                if (_taxiDrive >= 2)
                {
                    _sound.lerpVolume = 0;
                    Profiles.experienceProfile.numLittleMen++;
                    Profiles.experienceProfile.littleManLevel = 1;
                    _newGrowthLevel = Profiles.experienceProfile.littleManLevel + 1;
                    _egg = Profile.GetEggSprite(Profiles.experienceProfile.numLittleMen, 0);
                    _littleManStartWait = 0;
                    _littleManLeave = false;
                    _driveAway = false;
                    _taxiDrive = 0;
                    _inTaxi = false;
                    _startedLittleManLeave = false;
                    littleEggs.Clear();
                }
            }
            else if (!_startedLittleManLeave)
            {
                if (Profiles.experienceProfile.numLittleMen == 0)
                {
                    Say("I AM A FULL\nLITTLE MAN.");
                    Say("THANK YOU FOR\nRAISING ME.");
                    Say("I MUST LEAVE\nNOW.");
                    Say("I LOVE MY\nPARENT.");
                    Say("...");
                    Say("PLEASE ACCEPT\nTHIS GIFT.");
                }
                else
                {
                    Say("I LOVE MY\nPARENT.");
                    Say("...");
                    Say("PLEASE ACCEPT\nTHIS GIFT.");
                }
                _startedLittleManLeave = true;
            }
            else if (!_talking)
            {
                Furniture specialGift = Profiles.experienceProfile.numLittleMen == 0
                              ? RoomEditor.GetFurniture("EGG")
                              : (Profiles.experienceProfile.numLittleMen == 1
                                ? RoomEditor.GetFurniture("PHOTO")
                                : (Profiles.experienceProfile.numLittleMen == 2
                                  ? RoomEditor.GetFurniture("PLATE")
                                  : (Profiles.experienceProfile.numLittleMen == 3
                                    ? RoomEditor.GetFurniture("GIFT BASKET")
                                    : (Profiles.experienceProfile.numLittleMen == 4
                                      ? RoomEditor.GetFurniture("WINE")
                                      : (Profiles.experienceProfile.numLittleMen == 5
                                        ? RoomEditor.GetFurniture("JUNK")
                                        : (Profiles.experienceProfile.numLittleMen == 6
                                          ? RoomEditor.GetFurniture("EASEL")
                                          : (Profiles.experienceProfile.numLittleMen != 7
                                            ? UIGachaBox.GetRandomFurniture(Rarity.VeryRare, 1, 0.75f)[0]
                                            : RoomEditor.GetFurniture("JUKEBOX"))))))));
                _confirmMenu = new UIPresentBox(specialGift, Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 190)
                {
                    Depth = 0.98f
                };
                _confirmMenu.DoInitialize();
                _confirmMenu.Open();
                _genericWait = 0.5f;
                _driveAway = true;
            }
            return;
        }
        _inTaxi = false;
        _stampWobbleSin += 0.8f;
        if (_showCard)
            _stampCardLerp = Lerp.FloatSmooth(_stampCardLerp, _stampCard ? 1 : 0, 0.18f, 1.05f);
        foreach (LittleHeart heart in _hearts)
        {
            heart.position += heart.velocity;
            heart.alpha -= 0.02f;
        }
        _hearts.RemoveAll(littleHeart => littleHeart.alpha <= 0);
        _coinLerp2 = Lerp.Float(_coinLerp2, 1, 0.08f);
        _stampWobble = Lerp.Float(_stampWobble, 0, 0.08f);
        if (_stampCard || _stampCardLerp > 0.01f)
        {
            if (!_showCard)
            {
                if (!_sandwichShift)
                {
                    _sandwichLerp = Lerp.Float(_sandwichLerp, 1, 0.12f);
                    if (_sandwichLerp >= 1)
                        _sandwichShift = true;
                }
                else if (_burp)
                {
                    _extraMouthOpen = Lerp.FloatSmooth(_extraMouthOpen, 0, 0.17f, 1.05f);
                    _finalWait += 0.1f;
                    if (_finalWait >= 1)
                    {
                        _finalWait = 0;
                        _finishEat = false;
                        _afterEatWait = 0;
                        _burp = false;
                        _sandwichLerp = 0;
                        _extraMouthOpen = 0;
                        _sandwichShift = false;
                        _eatWait = 0;
                        _openWait = 0;
                        _showCard = true;
                        _sandwichEat = 0;
                        _finishingNewStamp = false;
                    }
                }
                else if (_finishEat)
                {
                    _extraMouthOpen = Lerp.FloatSmooth(_extraMouthOpen, 0, 0.17f, 1.05f);
                    _afterEatWait += 0.08f;
                    if (_afterEatWait >= 1)
                    {
                        SFX.Play("healthyEat");
                        for (int i2 = 0; i2 < 8; i2++)
                        {
                            _hearts.Add(new LittleHeart
                            {
                                position = littleManPos + new Vector2(8 + Rando.Float(-4, 4), 8 + Rando.Float(-6, 6)),
                                velocity = new Vector2(0, Rando.Float(-0.2f, -0.4f))
                            });
                        }
                        _burp = true;
                    }
                }
                else
                {
                    _sandwichLerp = Lerp.Float(_sandwichLerp, 0, 0.12f);
                    if (_sandwichLerp <= 0)
                    {
                        _extraMouthOpen = Lerp.FloatSmooth(_extraMouthOpen, 15, 0.18f, 1.05f);
                        if (_extraMouthOpen >= 1)
                        {
                            _openWait += 0.08f;
                            if (_openWait >= 1)
                            {
                                _sandwichEat = Lerp.Float(_sandwichEat, 1, 0.08f);
                                if (_sandwichEat >= 1)
                                {
                                    if (_eatWait == 0)
                                        SFX.Play("swallow");
                                    _eatWait += 0.08f;
                                    if (_eatWait >= 1)
                                        _finishEat = true;
                                }
                            }
                        }
                    }
                }
            }
            if (_stampCardLerp < 0.99f)
                return;

            _stampWait += 0.2f;
            if (_stampWait < 1)
                return;

            if (_stampWait2 == 0)
            {
                Profiles.experienceProfile.numSandwiches++;
                _finishingNewStamp = true;
                _stampWobble = 1;
                SFX.Play("dacBang");
            }
            _stampWait2 += 0.06f;
            if (_stampWait2 >= 1)
            {
                if (Profiles.experienceProfile.numSandwiches > 0 && Profiles.experienceProfile.numSandwiches % 6 == 0 && _coin2Wait == 0)
                {
                    _coinLerp2 = 0;
                    _coin2Wait = 1;
                    SFX.Play("ching", 1, 0.2f);
                    rareGachas++;
                }
                _coin2Wait -= 0.08f;
                if (_coin2Wait <= 0)
                {
                    _stampCard = false;
                    _stampWait2 = 0;
                    _stampWait = 0;
                    _coin2Wait = 0;
                }
            }
            return;
        }
        _showCard = false;
        Vector2 target = new(X - 80, Y - 10);
        if (!open)
            return;

        if (_currentLevel == _desiredLevel)
        {
            if (_currentLevel > 3 && _finned && Input.Pressed("MENU2"))
            {
                _talking = true;
                _finishTalkWait = 0;
                _talkLine = "";
                if (Profiles.experienceProfile.littleManLevel <= 2 && _currentLevel > 6)
                {
                    _feedLine = "";
                    if (!Global.data.hadTalk)
                    {
                        if (_eggFeedIndex < _eggFeedLines.Count)
                        {
                            if (_eggFeedIndex > 2)
                                eggTalk = true;
                            _feedLine = _eggFeedLines[_eggFeedIndex];
                            _eggFeedIndex++;
                            if (_eggFeedIndex >= _eggFeedLines.Count)
                            {
                                Global.data.hadTalk = true;
                                HUD.CloseAllCorners();
                                eggTalk = false;
                                _finned = false;
                            }
                        }
                        else
                            _feedLine = "...";
                    }
                }
                else
                {
                    _feedLine = "I AM A HUNGRY\nLITTLE MAN.";
                    if (Rando.Int(1000) == 1)
                        _feedLine = "I... AM A HUNGRY\nLITTLE MAN.";
                    DateTime now = MonoMain.GetLocalTime();
                    if (!saidSpecial)
                    {
                        if (now.Month == 4 && now.Day == 20)
                            _feedLine = "HAPPY BIRTHDAY!";
                        else if (now.Month == 3 && now.Day == 9)
                            _feedLine = "HAPPY BIRTHDAY!!";
                        else if (now.Month == 1 && now.Day == 1)
                            _feedLine = "HAPPY NEW YEAR!";
                        else if (now.Month == 6 && now.Day == 4)
                            _feedLine = "HAPPY BIRTHDAY\nDUCK GAME!";
                        else if (Rando.Int(190000) == 1)
                            _feedLine = "LET'S DANCE!";
                        else if (Rando.Int(80000) == 1)
                            _feedLine = "HAPPY BIRTHDAY!";
                        saidSpecial = true;
                    }
                    if (Rando.Int(100000) == 1)
                        _feedLine = "I AM A HANGRY\nLITTLE MAN.";
                    else if (Rando.Int(150000) == 1)
                        _feedLine = "I AM A HAPPY\nLITTLE MAN.";
                }
                _startFeedLine = _feedLine;
            }
            if (_state == UILevelBoxState.LogWinLoss)
            {
                if (Input.Pressed("SELECT"))
                {
                    Profiles.experienceProfile?.xp = _xpValue;
                    SFX.Play("rockHitGround2", 1, 0.5f);
                    Close();
                }
            }
            else if (_state == UILevelBoxState.Wait)
            {
                _startWait -= 0.09f;
                if (_startWait < 0)
                {
                    _startWait = 1;
                    _state = UILevelBoxState.ShowXPBar;
                    SFX.Play("rockHitGround2", 1, 0.5f);
                }
            }
            else if (_state == UILevelBoxState.UpdateTime)
            {
                _advancedDay = false;
                _fallVel = 0;
                _finned = false;
                _markedNewDay = false;
                _advanceDayWait = 0;
                _dayFallAway = 0;
                _dayScroll = 0;
                _newCircleLerp = 0;
                _popDay = false;
                _slideWait = 0;
                _unSlide = false;
                _intermissionSlide = 0;
                _intermissionWait = 0;
                _gaveToy = false;
                if (_roundsPlayed > 0)
                {
                    _updateTimeWait += 0.08f;
                    if (_updateTimeWait >= 1)
                    {
                        _dayTake += 0.8f;
                        if (_dayTake >= 1)
                        {
                            _dayTake = 0;
                            _roundsPlayed--;
                        }
                        _dayProgress = 1 - _roundsPlayed / _startRoundsPlayed;
                        time += 0.08f;
                    }
                    if (time >= 1)
                    {
                        time -= 1;
                        _state = UILevelBoxState.AdvanceDay;
                        _updateTimeWait = 0;
                        _dayTake = 0;
                    }
                }
                else
                    _state = UILevelBoxState.Finished;
            }
            else if (_state == UILevelBoxState.RunDay)
            {
                DayType t = GetDay(Profiles.experienceProfile.currentDay);
                switch (t)
                {
                    case DayType.Allowance:
                        if (_giveMoney == 0)
                        {
                            _giveMoney = 200;
                            Profiles.experienceProfile.littleManBucks += _giveMoney;
                            SFX.Play("ching");
                        }
                        _giveMoneyRise = Lerp.Float(_giveMoneyRise, 1, 0.05f);
                        _finishDayWait += 0.04f;
                        if (_finishDayWait >= 1)
                        {
                            _giveMoneyRise = 1;
                            _giveMoney = 0;
                            _state = UILevelBoxState.UpdateTime;
                        }
                        break;
                    case DayType.PayDay:
                        if (_giveMoney == 0)
                        {
                            int wage = 75;
                            if (_currentLevel > 5)
                                wage = 100;
                            if (_currentLevel > 6)
                                wage = 125;
                            _giveMoney = wage + Profiles.experienceProfile.numLittleMen * 25;
                            Profiles.experienceProfile.littleManBucks += _giveMoney;
                            SFX.Play("ching");
                        }
                        _giveMoneyRise = Lerp.Float(_giveMoneyRise, 1, 0.05f);
                        _finishDayWait += 0.04f;
                        if (_finishDayWait >= 1)
                        {
                            _giveMoneyRise = 1;
                            _giveMoney = 0;
                            _state = UILevelBoxState.UpdateTime;
                        }
                        break;
                    case DayType.ToyDay:
                        if (!_gaveToy)
                        {
                            _gaveToy = true;
                            gachas++;
                            _coinLerp = 0;
                            SFX.Play("ching", 1, 0.2f);
                        }
                        _finishDayWait += 0.04f;
                        if (_finishDayWait >= 1)
                            _state = UILevelBoxState.UpdateTime;
                        break;
                    case DayType.Sandwich:
                        if (!_gaveToy)
                        {
                            _stampCard = true;
                            _state = UILevelBoxState.UpdateTime;
                        }
                        break;
                    case DayType.FreeXP:
                        DuckNetwork.GiveXP("FREE XP DAY", 0, 75);
                        _state = UILevelBoxState.Wait;
                        break;
                    case DayType.Empty:
                        _state = UILevelBoxState.UpdateTime;
                        break;
                    default:
                        if (IsVinceDay(t) && !Input.Down("SELECT"))
                        {
                            _state = UILevelBoxState.UpdateTime;
                            Vincent.showingDay = false;
                            FurniShopScreen.close = true;
                        }
                        break;
                }
            }
            else if (_state == UILevelBoxState.AdvanceDay)
            {
                if (!_advancedDay)
                {
                    if (!_popDay)
                    {
                        _popDay = true;
                        _fallVel = -0.025f;
                        _ranRot = Rando.Float(-0.3f, 0.3f);
                    }
                    if (_dayFallAway < 1)
                    {
                        _dayFallAway += _fallVel;
                        _fallVel += 0.005f;
                    }
                    else if (_dayScroll < 1)
                        _dayScroll = Lerp.Float(_dayScroll, 1, 0.1f);
                    else
                    {
                        _advancedDay = true;
                        Profiles.experienceProfile.currentDay++;
                        _dayFallAway = 0;
                        _dayScroll = 0;
                    }
                }
                else
                {
                    _advanceDayWait += 0.1f;
                    if (_advanceDayWait >= 1)
                    {
                        if (!_markedNewDay)
                        {
                            SFX.Play("chalk");
                            _markedNewDay = true;
                            DayType t2 = GetDay(Profiles.experienceProfile.currentDay);
                            if (IsVinceDay(t2))
                            {
                                skipping = false;
                                SFX.skip = false;
                                SFX.Play("dacBang");
                                Graphics.fadeAdd = 1;
                            }
                        }
                        if (_markedNewDay)
                        {
                            _intermissionWait += 0.15f;
                            if (_intermissionWait >= 1)
                            {
                                if (_unSlide)
                                {
                                    _intermissionSlide = Lerp.FloatSmooth(_intermissionSlide, 0, 0.42f);
                                    if (_intermissionSlide <= 0.02f)
                                    {
                                        _intermissionSlide = 0;
                                        _dayStartWait += 0.11f;
                                        if (_dayStartWait >= 1)
                                        {
                                            AdvanceDay();
                                            _state = UILevelBoxState.RunDay;
                                        }
                                        if (IsVinceDay(GetDay(Profiles.experienceProfile.currentDay)))
                                            Vincent.showingDay = false;
                                    }
                                }
                                else
                                {
                                    _intermissionSlide = Lerp.FloatSmooth(_intermissionSlide, 1, 0.2f, 1.05f);
                                    if (_intermissionSlide >= 1)
                                    {
                                        DayType t3 = GetDay(Profiles.experienceProfile.currentDay);
                                        if (IsVinceDay(t3) && !Vincent.showingDay)
                                        {
                                            Vincent.Clear();
                                            Vincent.showingDay = true;
                                            Vincent.Open(t3);
                                            FurniShopScreen.open = true;
                                            _roundsPlayed = 0;
                                        }
                                        _slideWait += 0.11f;
                                        if (_slideWait >= 1.8f)
                                            _unSlide = true;
                                    }
                                }
                            }
                        }
                        _newCircleLerp = Lerp.Float(_newCircleLerp, 1, 0.2f);
                    }
                }
            }
            else if (_state == UILevelBoxState.Finished)
            {
                if (!_finned)
                {
                    HUD.CloseAllCorners();
                    HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@CONTINUE");
                    if (_currentLevel > 3 && (!Global.data.hadTalk || Profiles.experienceProfile.littleManLevel > 2))
                        HUD.AddCornerControl(HUDCorner.TopRight, "@MENU2@TALK");
                    if (Profiles.experienceProfile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0 && (rareGachas > 0 || gachas > 0))
                        HUD.AddCornerControl(HUDCorner.BottomLeft, "@START@AUTO TOYS");
                    _finned = true;
                }
            }
            else if (_state == UILevelBoxState.ShowXPBar)
            {
                _firstParticleIn = false;
                _drain = 1;
                if (_currentStat.Key == null)
                    _currentStat = DuckNetwork.TakeXPStat();
                if (_currentStat.Key == null)
                {
                    if (_roundsPlayed > 0 && _currentLevel >= 4)
                        _state = UILevelBoxState.UpdateTime;
                    else
                        _state = UILevelBoxState.Finished;
                }
                else
                {
                    _slideXPBar = Lerp.FloatSmooth(_slideXPBar, 1, 0.18f, 1.1f);
                    if (_slideXPBar >= 1)
                    {
                        _oldXPValue = _xpValue;
                        _newXPValue = _xpValue + _currentStat.Value.xp;
                        if (_currentLevel > 2)
                        {
                            _oldGachaValue = _gachaValue;
                            _newGachaValue = _gachaValue + _currentStat.Value.xp;
                            if (_currentLevel > 3)
                            {
                                _oldSandwichValue = _sandwichValue;
                                _newSandwichValue = _sandwichValue + _currentStat.Value.xp;
                                if (_currentLevel >= 7)
                                {
                                    _oldMilkValue = _milkValue;
                                    _newMilkValue = _milkValue + _currentStat.Value.xp;
                                }
                            }
                        }
                        _state = UILevelBoxState.WaitXPBar;
                        SFX.Play("scoreDing", 1f, 0.5f);
                    }
                }
            }
            else if (_state == UILevelBoxState.WaitXPBar)
            {
                _startWait -= 0.15f;
                if (_startWait < 0)
                {
                    _startWait = 1;
                    _state = UILevelBoxState.DrainXPBar;
                }
            }
            else if (_state == UILevelBoxState.DrainXPBar)
            {
                _drain -= 0.04f;
                if (_drain > 0)
                {
                    _particleWait -= 0.4f;
                    if (_particleWait < 0)
                    {
                        float fullYOffset = 30;
                        if (_currentLevel == 3)
                            fullYOffset = 25;
                        if (_currentLevel >= 4)
                            fullYOffset = 20;
                        if (_currentLevel >= 4)
                            fullYOffset = 0;
                        if (_currentLevel >= 7)
                            fullYOffset = -12;
                        if (_currentStat.Value.type == 0 || _currentStat.Value.type == 4)
                        {
                            _particles.Add(new XPPlus
                            {
                                position = new Vector2(X - 72, Y - 58),
                                velocity = new Vector2(-Rando.Float(3, 6), -Rando.Float(1, 4)),
                                target = target + new Vector2(0, fullYOffset),
                                color = Colors.DGGreen
                            });
                        }
                        if (_currentLevel >= 3 && (_currentStat.Value.type == 1 || _currentStat.Value.type == 4))
                        {
                            _particles.Add(new XPPlus
                            {
                                position = new Vector2(X - 72, Y - 58),
                                velocity = new Vector2(-Rando.Float(3, 6), -Rando.Float(1, 4)),
                                target = target + new Vector2(0, 10 + fullYOffset),
                                color = Colors.DGRed
                            });
                        }
                        if (_currentLevel >= 4 && (_currentStat.Value.type == 2 || _currentStat.Value.type == 4))
                        {
                            _particles.Add(new XPPlus
                            {
                                position = new Vector2(X - 72, Y - 58),
                                velocity = new Vector2(-Rando.Float(3, 6), -Rando.Float(1, 4)),
                                target = target + new Vector2(0, 20 + fullYOffset),
                                color = Colors.DGBlue
                            });
                        }
                        SFX.Play("tinyTick");
                        _particleWait = 1;
                    }
                }
                if (_firstParticleIn)
                {
                    _addLerp += 0.04f;
                    _xpValue = (int)Lerp.FloatSmooth(_oldXPValue, _newXPValue, _addLerp);
                    _gachaValue = (int)Lerp.FloatSmooth(_oldGachaValue, _newGachaValue, _addLerp);
                    _sandwichValue = (int)Lerp.FloatSmooth(_oldSandwichValue, _newSandwichValue, _addLerp);
                    _milkValue = (int)Lerp.FloatSmooth(_oldMilkValue, _newMilkValue, _addLerp);
                    _xpProgress = (_xpValue - _originalXP) / (float)_totalXP;
                }
                if (_drain < 0)
                    _drain = 0;
                if (_drain <= 0 && _addLerp >= 1)
                {
                    _drain = 0;
                    _addLerp = 0;
                    _state = UILevelBoxState.HideXPBar;
                }
            }
            else if (_state == UILevelBoxState.HideXPBar)
            {
                _slideXPBar = Lerp.FloatSmooth(_slideXPBar, 0, 0.2f, 1.1f);
                if (_slideXPBar <= 0.02f)
                {
                    _currentStat = default;
                    _state = UILevelBoxState.ShowXPBar;
                    SFX.Play("rockHitGround2", 1, 0.5f);
                    _slideXPBar = 0;
                }
            }
        }
        if (_currentLevel == _desiredLevel)
        {
            _coinLerp = Lerp.Float(_coinLerp, 1, 0.1f);
            foreach (XPPlus particle in _particles)
            {
                particle.position += particle.velocity;
                if (!particle.splash)
                {
                    particle.position = Lerp.Vec2Smooth(particle.position, particle.target, particle.time);
                    particle.time += 0.01f;
                }
                else
                {
                    particle.velocity.Y += 0.2f;
                    particle.alpha -= 0.05f;
                }
            }
        }
        int c = _particles.Count;
        _particles.RemoveAll(part => (part.position - part.target).LengthSquared() < 64);
        if (_particles.Count != c)
            _firstParticleIn = true;
        if (_xpValue >= DuckNetwork.GetLevel(_desiredLevel + 1).xpRequired && _currentLevel != 20)
            _desiredLevel++;
        if (_currentLevel <= 2)
            return;
        if (_gachaValue >= gachaNeed)
        {
            _gachaValue -= gachaNeed;
            _newGachaValue -= gachaNeed;
            _oldGachaValue -= gachaNeed;
            gachas++;
            _coinLerp = 0;
            SFX.Play("ching", 1, 0.2f);
        }
        if (_milkValue >= milkNeed)
        {
            _milkValue -= milkNeed;
            _newMilkValue -= milkNeed;
            _oldMilkValue -= milkNeed;
            _newGrowthLevel = Profiles.experienceProfile.littleManLevel + 1;
            if (_newGrowthLevel > 7)
                _newGrowthLevel = 7;
        }
        if (_currentLevel > 3 && _sandwichValue >= sandwichNeed)
        {
            _sandwichValue -= sandwichNeed;
            _newSandwichValue -= sandwichNeed;
            _oldSandwichValue -= sandwichNeed;
            _stampCard = true;
        }
    }

    public override void Draw()
    {
        if (skipping)
            return;
        int offsetLevbar = 33;
        if (_currentLevel >= 3)
            offsetLevbar = 38;
        if (_currentLevel >= 4)
            offsetLevbar = 63;
        if (_currentLevel >= 7)
            offsetLevbar = 75;
        Vector2 addBarPos = new(X, Y - offsetLevbar * _slideXPBar);
        int curLevMax = _currentLevel;
        if (curLevMax > 8)
            curLevMax = 8;
        Sprite sprite = _frames[curLevMax - 1];
        float fullYOffset = 30;
        if (_currentLevel == 3)
            fullYOffset = 25;
        if (_currentLevel >= 4)
            fullYOffset = 20;
        if (_currentLevel >= 4)
            fullYOffset = 0;
        if (_currentLevel >= 7)
            fullYOffset = -12;
        sprite.Depth = Depth;
        Graphics.Draw(sprite, X, Y);
        string text = $"@LWING@{Profiles.experienceProfile.name}@RWING@";
        float hOffset = 0;
        float vOffset = 0;
        Vector2 fontscale = Vector2.One;
        if (Profiles.experienceProfile.name.Length > 9)
        {
            fontscale = new Vector2(0.75f);
            vOffset = 1;
            hOffset = 1;
        }
        if (Profiles.experienceProfile.name.Length > 12)
        {
            fontscale = new Vector2(0.5f);
            vOffset = 2;
            hOffset = 1;
        }
        _font.Scale = fontscale;
        Vector2 fontPos = new(-_font.GetWidth(text) / 2, fullYOffset - 50);
        _font.DrawOutline(text, Position + fontPos + new Vector2(hOffset, vOffset), Color.White, Color.Black, Depth + 2);
        _font.Scale = Vector2.One;
        _lev.Depth = Depth + 2;
        _lev.frame = _currentLevel - 1;
        Graphics.Draw(_lev, X - 90, Y - 34 + fullYOffset);
        _font.DrawOutline(_currentLevel.ToString()[0].ToString() ?? "", Position + new Vector2(-84, fullYOffset - 30), Color.White, Color.Black, Depth + 2);
        if (_currentLevel > 9)
        {
            _thickBiosNum.Scale = new Vector2(0.5f);
            _thickBiosNum.Draw(_currentLevel.ToString()[1].ToString() ?? "", Position + new Vector2(-78, fullYOffset - 32), Color.White, Depth + 20);
        }
        float wide = 85;
        if (_currentLevel == 1)
            wide = 175;
        if (_currentLevel == 2)
            wide = 154;
        if (_currentLevel == 3)
            wide = 122;
        if (_currentLevel >= 4)
            wide = 94;
        if (_currentLevel >= 7)
            wide = 83;
        int max = DuckNetwork.GetLevel(_currentLevel + 1).xpRequired;
        int sub = 0;
        if (_currentLevel > 0)
            sub = DuckNetwork.GetLevel(_currentLevel).xpRequired;
        int val = (int)Math.Round(max * (_xpValue / (float)max));
        int totalMax = DuckNetwork.GetLevel(9999).xpRequired;
        if (val > totalMax)
            val = totalMax;
        float fill = (max - sub != 0) ? (_xpValue - sub) / (float)(max - sub) : 1;
        string maxString = max.ToString();
        if (maxString.Length > 5)
            maxString = $"{maxString[..3]}k";
        else if (maxString.Length > 4)
            maxString = $"{maxString[..2]}k";
        string amountText = $"|DGGREEN|{val}|WHITE|/|DGBLUE|{maxString}|WHITE|";
        float levelTextXOffset = 0;
        if (_currentLevel == 1)
            levelTextXOffset = 94;
        if (_currentLevel == 2)
            levelTextXOffset = 70;
        if (_currentLevel == 3)
            levelTextXOffset = 38;
        if (_currentLevel >= 4)
            levelTextXOffset = 10;
        if (_currentLevel >= 7)
            levelTextXOffset = -1;
        _fancyFont.DrawOutline(amountText, Position + new Vector2(levelTextXOffset - 8, fullYOffset - 31) - new Vector2(_fancyFont.GetWidth(amountText), 0), Colors.DGYellow, Color.Black, Depth + 2);
        if (fill < 0.0235f)
            fill = 0.0235f;
        float barX = wide * fill;
        _xpBar.Depth = Depth + 2;
        _xpBar.ScaleX = 1;
        Vector2 barPos = new(X - 87, Y - 18);
        Graphics.Draw(_xpBar, barPos.X, barPos.Y + fullYOffset, new Rectangle(0, 0, 3, 6));
        _xpBar.ScaleX = barX - 4;
        Graphics.Draw(_xpBar, barPos.X + 3, barPos.Y + fullYOffset, new Rectangle(2, 0, 1, 6));
        _xpBar.Depth = Depth + 7;
        _xpBar.ScaleX = 1;
        Graphics.Draw(_xpBar, barPos.X + (barX - 2), barPos.Y + fullYOffset, new Rectangle(3, 0, 3, 6));
        int startCut = 0;
        _barFront.Depth = Depth + 10;
        if (barX < 13)
            startCut = 13 - (int)barX;
        _barHeat += Math.Abs(_lastFill - fill) * 8;
        if (_barHeat > 1)
            _barHeat = 1;
        _barFront.Alpha = _barHeat;
        Graphics.Draw(_barFront, barPos.X + barX + startCut, barPos.Y + fullYOffset, new Rectangle(startCut, 0, _barFront.width - startCut, 6));
        _barHeat = Maths.CountDown(_barHeat, 0.04f);
        if (_currentLevel >= 3)
        {
            float fill2 = _gachaValue / (float)gachaNeed;
            float gachaWide = 110;
            if (_currentLevel == 3)
                gachaWide = 149;
            if (_currentLevel >= 4)
                gachaWide = 122;
            float barX2 = (float)Math.Floor(gachaWide * fill2);
            if (barX2 < 2)
                barX2 = 2;
            _gachaBar.Depth = Depth + 2;
            _gachaBar.ScaleX = 1;
            Vector2 barPos2 = new(X - 87, Y - 5);
            Graphics.Draw(_gachaBar, barPos2.X, barPos2.Y + fullYOffset, new Rectangle(0, 0, 3, 3));
            _gachaBar.ScaleX = barX2 - 5;
            Graphics.Draw(_gachaBar, barPos2.X + 3, barPos2.Y + fullYOffset, new Rectangle(2, 0, 1, 3));
            _gachaBar.Depth = Depth + 7;
            _gachaBar.ScaleX = 1;
            Graphics.Draw(_gachaBar, barPos2.X + (barX2 - 2), barPos2.Y + fullYOffset, new Rectangle(3, 0, 3, 3));
            _duckCoin.frame = 0;
            _duckCoin.Alpha = 1 - Math.Max(_coinLerp - 0.5f, 0) * 2;
            _duckCoin.Depth = 0.9f;
            Graphics.Draw(_duckCoin, barPos2.X + (gachaWide - 2) + 15, barPos2.Y + fullYOffset - 8 - _coinLerp * 18);
        }
        if (_currentLevel >= 4)
        {
            float fill3 = _sandwichValue / (float)sandwichNeed;
            float sandwichWide = 154;
            float barX3 = sandwichWide * fill3;
            if (barX3 < 2)
                barX3 = 2;
            _sandwichBar.Depth = Depth + 2;
            _sandwichBar.ScaleX = 1;
            Vector2 barPos3 = new(X - 87, Y + 5);
            Graphics.Draw(_sandwichBar, barPos3.X, barPos3.Y + fullYOffset, new Rectangle(0, 0, 3, 3));
            _sandwichBar.ScaleX = barX3 - 5f;
            Graphics.Draw(_sandwichBar, barPos3.X + 3, barPos3.Y + fullYOffset, new Rectangle(2, 0, 1, 3));
            _sandwichBar.Depth = Depth + 7;
            _sandwichBar.ScaleX = 1;
            Graphics.Draw(_sandwichBar, barPos3.X + (barX3 - 2), barPos3.Y + fullYOffset, new Rectangle(3, 0, 3, 3));
            _sandwich.Depth = 0.88f;
            float yOffSandwich = _sandwichLerp * -150;
            float xOffSandwich = 0;
            float xCutoff = 0;
            if (_sandwichShift)
            {
                yOffSandwich -= 20;
                xOffSandwich = -(_sandwichEat + 42) * 30;
                xCutoff = -(xOffSandwich + 52);
                if (_currentLevel >= 7)
                    xOffSandwich -= 10;
            }
            xCutoff = Math.Max(xCutoff, 0);
            if (xCutoff < _sandwich.width)
                Graphics.Draw(_sandwich, barPos3.X + (sandwichWide - 2) + 12 + xOffSandwich + xCutoff + 1, barPos3.Y + fullYOffset - 16 + yOffSandwich, new Rectangle(xCutoff, 0, _sandwich.width - xCutoff, _sandwich.height), 0.88f);
        }
        if (_currentStat.Key != null)
        {
            _addXPBar.Depth = Depth - 20;
            _addXPBar.ScaleX = 1;
            Graphics.Draw(_addXPBar, addBarPos.X, addBarPos.Y);
            string t = (_currentStat.Value.num == 0) ? _currentStat.Key : $"{_currentStat.Value.num} {_currentStat.Key}";
            _fancyFont.DrawOutline(t, addBarPos + new Vector2(-(_addXPBar.width / 2) + 4, -2), Color.White, Color.Black, Depth - 10);
            Vector2 vec = addBarPos + new Vector2(-(_addXPBar.width / 2) + 2, -7.5f);
            Graphics.DrawLine(vec, vec + new Vector2((_addXPBar.width - 5) * _drain, 0), Color.Lime, 1, _addXPBar.Depth + 2);
            string xpVal = $"{(int)(_currentStat.Value.xp * _drain)}|DGBLUE|XP";
            _fancyFont.DrawOutline(xpVal, addBarPos + new Vector2((_addXPBar.width / 2) - _fancyFont.GetWidth(xpVal) - 4, -2), Colors.DGGreen, Color.Black, Depth - 10);
        }
        foreach (XPPlus p in _particles)
        {
            int add = 20;
            if (p.splash)
                add = 40;
            float len = Math.Min((p.position - p.target).Length(), 30) / 30;
            _xpPoint.Scale = new Vector2(len);
            _xpPointOutline.Scale = new Vector2(len);
            _xpPoint.color = p.color;
            _xpPoint.Alpha = p.alpha * len;
            _xpPoint.Depth = Depth + add;
            Graphics.Draw(_xpPoint, p.position.X, p.position.Y);
            _xpPointOutline.Alpha = p.alpha * len;
            _xpPointOutline.Depth = Depth + (add - 5);
            Graphics.Draw(_xpPointOutline, p.position.X, p.position.Y);
        }
        foreach (LittleHeart p2 in _hearts)
        {
            _heart.Alpha = p2.alpha;
            _heart.Depth = 0.98f;
            _heart.Scale = new Vector2(0.5f);
            Graphics.Draw(_heart, p2.position.X, p2.position.Y);
        }
        int curlev = _currentLevel;
        if (Profiles.experienceProfile.numLittleMen > 0)
            curlev = Profiles.experienceProfile.littleManLevel;
        if (curlev > 7)
            curlev = 7;
        if (_currentLevel >= 2)
        {
            float mouthOpenAmount = (float)Math.Round(_talk) + _extraMouthOpen;
            int yOffSize = 0;
            if (curlev <= 4)
                yOffSize = 1;
            if (curlev <= 3)
                yOffSize = 2;
            if (curlev <= 2)
            {
                if (curlev == 2)
                {
                    _egg.Depth = 0.85f;
                    _egg.ScaleY = 1;
                    int mouthHeight = 8;
                    Vector2 littleEggPos = new(X + levelTextXOffset, Y - 29 + fullYOffset + mouthHeight + yOffSize);
                    Graphics.Draw(_egg, littleEggPos.X, littleEggPos.Y, new Rectangle(0, mouthHeight + yOffSize, 16, 16 - mouthHeight - yOffSize));
                    Graphics.Draw(_egg, X + levelTextXOffset, Y - 29 + fullYOffset - mouthOpenAmount, new Rectangle(0, 0, 16, mouthHeight + yOffSize));
                    Vector2 center = _egg.Center;
                    _egg.ScaleY = mouthOpenAmount;
                    _egg.Center = center;
                }
            }
            else
            {
                _littleMan.frame = LittleManFrame(Profiles.experienceProfile.numLittleMen, curlev, 0);
                _littleMan.Depth = 0.85f;
                _littleMan.ScaleY = 1;
                littleManPos = new Vector2(X + levelTextXOffset, Y + fullYOffset + yOffSize - 25);
                if (!_inTaxi)
                {
                    Graphics.Draw(_littleMan, littleManPos.X, littleManPos.Y, new Rectangle(0, 4 + yOffSize, 16, 12 - yOffSize));
                    Graphics.Draw(_littleMan, X + levelTextXOffset, Y + fullYOffset - mouthOpenAmount - 29, new Rectangle(0, 0, 16, 4 + yOffSize));
                    Vector2 center2 = _littleMan.Center;
                    _littleMan.ScaleY = mouthOpenAmount;
                    Graphics.Draw(_littleMan, X + levelTextXOffset, Y + (fullYOffset - mouthOpenAmount) + yOffSize - 25, new Rectangle(0, 4 + yOffSize, 16, 1));
                    _littleMan.Center = center2;
                }
            }
            _talkBubble.Depth = 0.9f;
            string talk = _talkLine;
            if (_talkLine.Length > 0)
            {
                Vector2 talkPos = new(X + levelTextXOffset + 16, Y + fullYOffset - 28);
                _talkBubble.ScaleX = 1;
                Graphics.Draw(_talkBubble, talkPos.X, talkPos.Y, new Rectangle(0, 0, 8, 8));
                float talkWidth = Graphics.GetStringWidth(talk) - 5;
                float talkHeight = Graphics.GetStringHeight(talk) + 2;
                _talkBubble.ScaleX = talkWidth;
                Graphics.Draw(_talkBubble, talkPos.X + 8, talkPos.Y, new Rectangle(5, 0, 1, 2));
                Graphics.Draw(_talkBubble, talkPos.X + 8, talkPos.Y + talkHeight, new Rectangle(5, 10, 1, 2));
                _talkBubble.ScaleX = 1;
                Graphics.Draw(_talkBubble, talkPos.X, talkPos.Y + (talkHeight - 2), new Rectangle(0, 8, 8, 4));
                Graphics.Draw(_talkBubble, talkPos.X + talkWidth + 8, talkPos.Y + (talkHeight - 2), new Rectangle(8, 8, 4, 4));
                Graphics.Draw(_talkBubble, talkPos.X + talkWidth + 8, talkPos.Y, new Rectangle(8, 0, 4, 4));
                Graphics.DrawRect(talkPos + new Vector2(5, 2), talkPos + new Vector2(talkWidth + 11, talkHeight), Color.White, 0.9f);
                Graphics.DrawLine(talkPos + new Vector2(4.5f, 5), talkPos + new Vector2(4.5f, talkHeight - 1), Color.Black, 1, 0.9f);
                Graphics.DrawLine(talkPos + new Vector2(11.5f + talkWidth, 4), talkPos + new Vector2(11.5f + talkWidth, talkHeight - 1), Color.Black, 1, 0.9f);
                Graphics.DrawString(talk, talkPos + new Vector2(6, 2), Color.Black, 0.95f);
            }
        }
        if (_stampCardLerp > 0.01f)
        {
            float yOffStamp = -(1 - _stampCardLerp) * 200 + (float)Math.Sin(_stampWobbleSin) * _stampWobble * 4;
            Graphics.DrawRect(new Vector2(-1000), new Vector2(1000), Color.Black * 0.5f * _stampCardLerp, 0.96f);
            Graphics.Draw(_sandwichCard, X, Y + yOffStamp, 0.97f);
            Random gen = Rando.generator = new Random(365023);
            int numSan = Profiles.experienceProfile.numSandwiches % 6;
            if (Profiles.experienceProfile.numSandwiches > 0 && Profiles.experienceProfile.numSandwiches % 6 == 0 && _finishingNewStamp)
                numSan = 6;
            for (int i = 0; i < numSan; i++)
            {
                float xpos = i % 2 * 16;
                float ypos = i / 2 * 16;
                _sandwichStamp.Angle = Rando.Float(-0.2f, 0.2f);
                _sandwichStamp.frame = Rando.Int(3);
                Graphics.Draw(_sandwichStamp, X + xpos + Rando.Float(-2, 2) + 30, Y + ypos + Rando.Float(-2, 2) + yOffStamp - 15, 0.98f);
                if (i == 5)
                {
                    _duckCoin.frame = 1;
                    _duckCoin.Alpha = 1 - Math.Max(_coinLerp2 - 0.5f, 0) * 2;
                    _duckCoin.Depth = 0.99f;
                    Graphics.Draw(_duckCoin, X + 30 + xpos, Y - 15 + ypos + yOffStamp - _coinLerp2 * 18);
                }
            }
            Rando.generator = gen;
        }
        if (_currentLevel >= 7)
        {
            _milk.Depth = 0.7f;
            _milk.frame = (int)(_milkValue / (float)milkNeed * 15);
            Graphics.Draw(_milk, X + 26, Y - 33);
            Vector2 littleEggsPos = Position + new Vector2(-88, 44);
            int eggIdx = 0;
            foreach (Sprite littleEgg in littleEggs)
            {
                littleEgg.Depth = 0.85f;
                Graphics.Draw(littleEgg, littleEggsPos.X + (eggIdx * 23) - 3, littleEggsPos.Y - 3);
                _littleMan.frame = LittleManFrame(Math.Max(Profiles.experienceProfile.numLittleMen - 8, 0) + eggIdx, -1, 0, bottomBar: true);
                _littleMan.Depth = 0.9f;
                _littleMan.ScaleY = 1;
                Graphics.Draw(_littleMan, littleEggsPos.X + (eggIdx * 23) + 3, littleEggsPos.Y + 1);
                eggIdx++;
            }
        }
        float calYOffset = 0;
        if (_currentLevel >= 7)
            calYOffset = -12;
        Vector2 clockPos = Position + new Vector2(75.5f, 33 + calYOffset);
        Vector2 clockPos2 = clockPos + new Vector2(0, -7);
        if (_currentLevel >= 4)
        {
            int munny = Profiles.experienceProfile.littleManBucks;
            string munnyString = "|DGGREEN|$";
            munnyString = munny <= 9999 ? $"{munnyString}{munny}" : $"{munnyString}{munny / 1000}K";
            Graphics.DrawRect(clockPos + new Vector2(-16, 9), clockPos + new Vector2(15, 18), Color.Black, 0.89f);
            _fancyFont.Draw(munnyString, clockPos + new Vector2(-16, 9) + new Vector2(30 - _fancyFont.GetWidth(munnyString), 0), Color.White, 0.9f);
            if (_giveMoney > 0 && _giveMoneyRise < 0.95f)
            {
                string addString = $"+{_giveMoney}";
                Color c = Colors.DGGreen;
                Color c2 = Color.Black;
                _fancyFont.DrawOutline(addString, clockPos + new Vector2(-16, 9) + new Vector2(30 - _fancyFont.GetWidth(addString), 0 - (10 + _giveMoneyRise * 10)), c, c2, 0.97f);
            }
            Vector2 minuteHand = new()
            {
                X = -float.Sin(time * 12 * (float.Pi * 2) - float.Pi) * 8,
                Y = float.Cos(time * 12 * (float.Pi * 2) - float.Pi) * 8
            };
            Vector2 hourHand = new()
            {
                X = (-float.Sin(time * (float.Pi * 2) - float.Pi)) * 5,
                Y = float.Cos(time * (float.Pi * 2) - float.Pi) * 5
            };
            Graphics.DrawLine(clockPos2, clockPos2 + minuteHand, Color.Black, 1, 0.9f);
            Graphics.DrawLine(clockPos2, clockPos2 + hourHand, Color.Black, 1.5f, 0.9f);
            Random generator = new(0);
            Random oldRand = Rando.generator;
            Rando.generator = generator;
            for (int j = 0; j < Profiles.experienceProfile.currentDay; j++)
                Rando.Float(1);
            Math.Floor(Profiles.experienceProfile.currentDay / 5f);
            for (int k = 0; k < 5; k++)
            {
                float deepAdd = 0;
                if (k == 0)
                    deepAdd += 0.1f;
                float rot = Rando.Float(-0.1f, 0.1f);
                int num = (int)((rot + 0.1f) / 0.2f * 10);
                if (_popDay && k == 0 && _dayFallAway != 0)
                    _weekDays.Angle = _ranRot;
                else if (_currentLevel < 6)
                    _weekDays.Angle = rot;
                else
                    _weekDays.Angle = 0;
                if (num == 3 && _currentLevel < 5)
                    _weekDays.Angle += (float)Math.PI;
                float yOff = 0;
                if (k == 0)
                    yOff = _dayFallAway * 100;
                float xOff = (0 - _dayScroll) * 26;
                if (k == 0)
                {
                    _circle.Depth = 0.85f + deepAdd;
                    _circle.Angle = _weekDays.Angle;
                    if (k == 0 && _advancedDay)
                        Graphics.Draw(_circle, Position.X - 71 + (k * 28) + xOff, Position.Y + 33 + calYOffset + yOff, new Rectangle(0, 0, _circle.width * _newCircleLerp, _circle.height));
                    else
                        Graphics.Draw(_circle, Position.X - 71 + (k * 28) + xOff, Position.Y + 33 + calYOffset + yOff);
                }
                _weekDays.Depth = 0.83f + deepAdd;
                _weekDays.frame = (Profiles.experienceProfile.currentDay + k) % 5;
                _weekDays.frame += (int)Math.Floor((Profiles.experienceProfile.currentDay + k) / 20f) % 4 * 6;
                Graphics.Draw(_weekDays, Position.X + (k * 28) + xOff - 71, Position.Y + yOff + calYOffset + 33);
                DayType t2 = GetDay(Profiles.experienceProfile.currentDay + k);
                if (t2 != DayType.Empty)
                {
                    _days.Depth = 0.84f + deepAdd;
                    _days.frame = (int)t2;
                    _days.Angle = _weekDays.Angle;
                    Graphics.Draw(_days, Position.X + (k * 28) + xOff - 71, Position.Y + calYOffset + yOff + 33);
                }
            }
            Rando.generator = oldRand;
        }
        if (_confirmMenu != null && _confirmMenu.open)
            Graphics.DrawRect(new Vector2(-1000), new Vector2(1000), Color.Black * 0.5f, 0.974f);
        if (FurniShopScreen.open)
        {
            Graphics.DrawRect(new Vector2(-1000), new Vector2(1000), Color.Black * 0.5f, 0.95f);
            FurniShopScreen.open = true;
            Vincent.Draw();
        }
        if (_taxiDrive > 0)
        {
            Vector2 taxiPos = new(Position.X - 200 + _taxiDrive * 210, Position.Y - 33);
            _taxi.Depth = 0.97f;
            Graphics.Draw(_taxi, taxiPos.X, taxiPos.Y);
            if (_inTaxi)
            {
                _littleMan.frame = LittleManFrame(Profiles.experienceProfile.numLittleMen, curlev, 0);
                Graphics.Draw(_littleMan, taxiPos.X - 16, taxiPos.Y - 8, new Rectangle(0, 0, 16, 6));
            }
        }
        if (_intermissionSlide > 0.01f)
        {
            float xpos2 = -320 + _intermissionSlide * 320;
            float ypos2 = 60;
            Graphics.DrawRect(new Vector2(xpos2, ypos2), new Vector2(xpos2 + 320, ypos2 + 30), Color.Black, 0.98f);
            xpos2 = 320 - _intermissionSlide * 320;
            ypos2 = 60;
            Graphics.DrawRect(new Vector2(xpos2, ypos2 + 30), new Vector2(xpos2 + 320, ypos2 + 60), Color.Black, 0.98f);
            string slideText = "ADVANCE DAY";
            switch (GetDay(Profiles.experienceProfile.currentDay))
            {
                case DayType.Allowance:
                    slideText = "ALLOWANCE";
                    break;
                case DayType.FreeXP:
                    slideText = "TRAINING DAY";
                    break;
                case DayType.ImportDay:
                    slideText = "FANCY IMPORTS";
                    break;
                case DayType.HintDay:
                    slideText = "RUMOURS";
                    break;
                case DayType.PawnDay:
                    slideText = "VINCENT";
                    break;
                case DayType.PayDay:
                    slideText = "PAY DAY";
                    break;
                case DayType.SaleDay:
                    slideText = "SUPER SALE";
                    break;
                case DayType.Sandwich:
                    slideText = "SANDWICH DAY";
                    break;
                case DayType.Shop:
                    slideText = "VINCENT";
                    break;
                case DayType.Special:
                    slideText = "VINCENT";
                    break;
                case DayType.ToyDay:
                    slideText = "FREE TOY";
                    break;
            }
            if (_overrideSlide != null)
                slideText = _overrideSlide;
            _bigFont.Draw(slideText, new Vector2(-320 + _intermissionSlide * (320 + Layer.HUD.width / 2 - _bigFont.GetWidth(slideText) / 2), ypos2 + 18), Color.White, 0.99f);
        }
        _lastFill = fill;
        _confirmMenu?.DoDraw();
    }

    public void AdvanceDay()
    {
        GetDay(Profiles.experienceProfile.currentDay);
    }

    public void Say(string s)
    {
        sayQueue.Add(s);
    }

    public DayType GetDay(int day)
    {
        if (day % 10 == 7 && _currentLevel >= 3)
            return DayType.PawnDay;
        switch (day)
        {
            case 3:
                return DayType.Sandwich;
            case 6:
                return DayType.ToyDay;
            default:
                {
                    if (day % 5 == 4 && day % 20 != 19 && _currentLevel >= 3)
                        return DayType.Shop;
                    if (day % 5 == 4 && day % 20 == 19 && _currentLevel >= 3)
                        return DayType.SaleDay;
                    if (day % 20 == 0 && day > 6 && _currentLevel >= 3)
                        return DayType.ImportDay;
                    if (day % 10 == 8 && _currentLevel >= 5)
                        return DayType.PayDay;
                    if (day % 40 == 3 && day > 6 && _currentLevel >= 3)
                        return DayType.Special;
                    if (day % 20 == 13 && _currentLevel >= 3)
                        return DayType.FreeXP;
                    if (day % 20 == 1 && day > 5 && _currentLevel >= 3)
                        return DayType.Sandwich;
                    if (day % 20 == 10 && day > 5 && _currentLevel >= 3)
                        return DayType.Sandwich;
                    if (day % 40 == 6 && _currentLevel >= 3)
                        return DayType.Empty;
                    if (day % 60 == 57 && day > 5 && _currentLevel >= 3)
                        return DayType.Sandwich;
                    if (day % 60 == 37 && _currentLevel >= 3)
                        return DayType.FreeXP;
                    if (day % 20 == 12 && _currentLevel >= 3)
                        return DayType.ToyDay;
                    if (day % 40 == 7 && _currentLevel >= 3)
                        return DayType.ToyDay;
                    if (day % 40 == 26 && _currentLevel >= 3)
                        return DayType.ToyDay;
                    if (day % 80 == 25 && _currentLevel >= 3)
                        return DayType.Special;
                    if (day % 80 == 65 && _currentLevel >= 3)
                        return DayType.FreeXP;
                    if (day % 40 == 16 && _currentLevel >= 3)
                        return DayType.FreeXP;
                    if (day % 40 == 36 && day > 5 && _currentLevel >= 3)
                        return DayType.Sandwich;
                    if (day % 20 == 2 && _currentLevel >= 3)
                        return DayType.Allowance;
                    Random generator = Rando.generator;
                    Rando.generator = new Random(day);
                    int rand = Rando.Int(32);
                    float rand2 = Rando.Float(1);
                    Rando.generator = generator;
                    switch (rand)
                    {
                        case 0:
                            return DayType.FreeXP;
                        case 1:
                            if (day > 6)
                                return DayType.Sandwich;
                            return DayType.FreeXP;
                        case 2:
                            if (day < 5)
                                return DayType.ToyDay;
                            return DayType.Special;
                        case 3:
                            return DayType.ToyDay;
                        default:
                            if (Unlockables.lockedItems.Count > 18)
                                return DayType.HintDay;
                            if (Unlockables.lockedItems.Count > 14 && rand2 > 0.2f)
                                return DayType.HintDay;
                            if (Unlockables.lockedItems.Count > 10 && rand2 > 0.3f)
                                return DayType.HintDay;
                            if (Unlockables.lockedItems.Count > 5 && rand2 > 0.5f)
                                return DayType.HintDay;
                            if (Unlockables.lockedItems.Count > 0 && rand2 > 0.75f)
                                return DayType.HintDay;
                            return DayType.Empty;
                    }
                }
        }
    }

    #endregion

    bool IsVinceDay(DayType d)
    {
        if (d != DayType.Special && d != DayType.PawnDay && d != DayType.ImportDay && d != DayType.SaleDay && d != DayType.Shop)
            return d == DayType.HintDay;
        return true;
    }
}