using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UILevelBox : UIMenu
{
	private List<Sprite> _frames = new List<Sprite>();

	private BitmapFont _font;

	private BitmapFont _thickBiosNum;

	private FancyBitmapFont _fancyFont;

	private FancyBitmapFont _chalkFont;

	private Sprite _xpBar;

	private Sprite _barFront;

	private Sprite _addXPBar;

	private SpriteMap _lev;

	private SpriteMap _littleMan;

	private Sprite _egg;

	private Sprite _levelUpArrow;

	private Sprite _talkBubble;

	private SpriteMap _duckCoin;

	private Sprite _sandwich;

	private Sprite _heart;

	private SpriteMap _sandwichCard;

	private SpriteMap _sandwichStamp;

	private SpriteMap _weekDays;

	private Sprite _taxi;

	private SpriteMap _circle;

	private SpriteMap _cross;

	private SpriteMap _days;

	private Sprite _gachaBar;

	private Sprite _sandwichBar;

	private UILevelBoxState _state;

	private float _xpLost;

	private float _slideXPBar;

	private float _startWait = 1f;

	private float _drain = 1f;

	private float _addLerp;

	private float _coinLerp = 1f;

	private float _coinLerp2 = 1f;

	private bool _firstParticleIn;

	private Sprite _xpPoint;

	private Sprite _xpPointOutline;

	private SpriteMap _milk;

	private List<XPPlus> _particles = new List<XPPlus>();

	private List<LittleHeart> _hearts = new List<LittleHeart>();

	private float _particleWait = 1f;

	private int _xpValue;

	public static int currentLevel;

	private int _currentLevel = 4;

	private int _desiredLevel = 4;

	private int _arrowsLevel = -1;

	private float _newXPValue;

	private float _oldXPValue;

	private int _gachaValue;

	private float _newGachaValue;

	private float _oldGachaValue;

	private int _sandwichValue;

	private float _newSandwichValue;

	private float _oldSandwichValue;

	private int _milkValue;

	private float _newMilkValue;

	private float _oldMilkValue;

	private bool _stampCard;

	private float _stampCardLerp;

	private int _roundsPlayed;

	private BitmapFont _bigFont;

	private int _currentDay;

	private List<Tex2D> _eggs = new List<Tex2D>();

	private static bool _firstOpen;

	private int _originalXP;

	private int _totalXP;

	private int _startRoundsPlayed;

	private int _newGrowthLevel = 1;

	private MenuBoolean _menuBool = new MenuBoolean();

	public KeyValuePair<string, XPPair> _currentStat;

	private float _talk;

	private bool close;

	private string _talkLine = "";

	private string _feedLine = "I AM A HUNGRY\nLITTLE MAN.";

	private string _startFeedLine = "I AM A HUNGRY\nLITTLE MAN.";

	private float _talkWait;

	private bool _talking;

	private float _finishTalkWait;

	private bool _alwaysClose;

	private float _stampWait;

	private float _stampWait2;

	private float _stampWobble;

	private float _stampWobbleSin;

	private float _extraMouthOpen;

	private float _openWait;

	private float _sandwichEat;

	private float _eatWait;

	private float _afterEatWait;

	private bool _finishEat;

	private bool _burp;

	private float _finalWait;

	private float _coin2Wait;

	private float _intermissionSlide;

	private float _newCrossLerp;

	private float _newCircleLerp;

	private float _taxiDrive;

	private float _genericWait;

	private bool queryVisitShop;

	private float _sandwichLerp;

	private bool _sandwichShift;

	private bool _showCard;

	private float _dayTake;

	private bool _updateTime;

	private float _updateTimeWait;

	private bool _finned;

	private float _advanceDayWait;

	private bool _advancedDay;

	private bool _markedNewDay;

	private float _fallVel;

	private float _afterScrollWait;

	private float _intermissionWait;

	private float _slideWait;

	private bool _unSlide;

	private bool _inTaxi;

	private int _giveMoney;

	private float _giveMoneyRise;

	private float _dayStartWait;

	private bool _popDay;

	private bool _attemptingBuy;

	private float _dayFallAway;

	private float _dayScroll;

	private float _ranRot;

	private bool _attemptingVincentClose;

	private bool _gaveToy;

	private bool _didXPDay;

	private float _finishDayWait;

	private bool earlyExit;

	private bool doubleUpdate;

	private bool doubleUpdating;

	private bool _startedLittleManLeave;

	private float _littleManStartWait;

	private bool _finishingNewStamp;

	private List<string> sayQueue = new List<string>();

	public int _eggFeedIndex;

	public List<string> _eggFeedLines = new List<string>
	{
		"..", "...", "EGGS CANNOT\nTALK.", "EGGS CANNOT\nTALK..", "EGGS CANNOT\nTALK!", "I SAID, EGGS\nCANNOT TALK!", "LOOK AT YOURSELF,\nTRYING TO TALK\nTO AN EGG.", "I GUESS I'VE\nBEEN THERE.", "YOU KNOW, PEOPLE\nARE GONNA CALL\nYOU CRAZY..", "CRAZY FOR TALKING\nTO AN EGG.",
		"BUT IF IT WASN'T\nFOR DUCKS LIKE\nYOU, WELL..", "THIS EGG WOULD\nHAVE NOBODY\nTO TALK TO.", "PEOPLE DON'T\nREALLY TAKE EGGS\nSERIOUSLY.", "MAYBE THEY'RE\nTHINKING,", "WHAT WOULD AN\nEGG SAY? IT'S\nJUST AN EGG.", "EGGS DON'T\nKNOW MUCH OF\nANYTHING!", "WELL..\nTHAT MAY BE\nTRUE.", "I KNOW ONLY\nWHAT'S INSIDE\nMY SHELL!", "BUT..", "EGGS HAVE\nA LOT OF TIME\nTO THINK ABOUT\nTHINGS.",
		"I THINK A LOT\nABOUT HOW TO\nBE A GOOD EGG.", "...", "I KNOW THAT\nI SHOULD BE\nKIND.", "KIND TO\nEVERYONE, EGG\nOR OTHERWISE.", "AND THE KEY\nTO KINDNESS IS\nREMEMBERING:", "WE DON'T KNOW\nWHAT'S GOING ON\nINSIDE ANYONE\nELSE'S SHELL!", "...", "PEOPLE WHO THINK\nTHEY KNOW,", "THEY MIGHT THINK\nTHAT EGGS DON'T\nTALK!", "...",
		"WELL, THANKS FOR\nLISTENING TO\nTHIS EGG.", "THANKS FOR\nCARING ABOUT\nWHAT GOES ON\nIN MY SHELL.", "THAT'S ALL I\nHAVE TO SAY.", "I NEED TO\nGET BACK TO\nGROWING UP!", "SO LONG BUDDY!"
	};

	public bool eggTalk;

	public float _xpProgress;

	public float _dayProgress;

	private bool _gotEgg;

	private bool _littleManLeave;

	public string _overrideSlide;

	private bool _didSlide;

	private float _levelSlideWait;

	private bool _driveAway;

	private bool _attemptingGive;

	private ConstantSound _sound = new ConstantSound("chainsawIdle", 0f, 0f, "chainsawIdleMulti");

	private List<Sprite> littleEggs = new List<Sprite>();

	private bool playedSound;

	private bool _prevShopOpen;

	private bool skipping;

	private bool skipUpdate;

	private float time;

	private int gachaNeed = 200;

	private int sandwichNeed = 500;

	private int milkNeed = 1400;

	private Vec2 littleManPos;

	private float _lastFill;

	private float _barHeat;

	private int _lastNum;

	private static bool saidSpecial
	{
		get
		{
			return MonoMain.core.saidSpecial;
		}
		set
		{
			MonoMain.core.saidSpecial = value;
		}
	}

	public static int gachas
	{
		get
		{
			return MonoMain.core.gachas;
		}
		set
		{
			MonoMain.core.gachas = value;
		}
	}

	public static int rareGachas
	{
		get
		{
			return MonoMain.core.rareGachas;
		}
		set
		{
			MonoMain.core.rareGachas = value;
		}
	}

	public static UIMenu _confirmMenu
	{
		get
		{
			return MonoMain.core._confirmMenu;
		}
		set
		{
			MonoMain.core._confirmMenu = value;
		}
	}

	public static bool menuOpen
	{
		get
		{
			if (_confirmMenu != null)
			{
				return _confirmMenu.open;
			}
			return false;
		}
	}

	public UILevelBox(string title, float xpos, float ypos, float wide = -1f, float high = -1f, string conString = "")
		: base(title, xpos, ypos, wide, high, conString)
	{
		Graphics.fade = 1f;
		_firstOpen = true;
		_frames.Add(new Sprite("levWindow_lev0"));
		_frames[_frames.Count - 1].CenterOrigin();
		_frames.Add(new Sprite("levWindow_lev1"));
		_frames[_frames.Count - 1].CenterOrigin();
		_frames.Add(new Sprite("levWindow_lev2"));
		_frames[_frames.Count - 1].CenterOrigin();
		_frames.Add(new Sprite("levWindow_lev4"));
		_frames[_frames.Count - 1].CenterOrigin();
		_frames.Add(new Sprite("levWindow_lev4"));
		_frames[_frames.Count - 1].CenterOrigin();
		_frames.Add(new Sprite("levWindow_lev5"));
		_frames[_frames.Count - 1].CenterOrigin();
		_frames.Add(new Sprite("levWindow_lev6"));
		_frames[_frames.Count - 1].CenterOrigin();
		_frames.Add(new Sprite("levWindow_lev6"));
		_frames[_frames.Count - 1].CenterOrigin();
		_barFront = new Sprite("online/barFront");
		_barFront.center = new Vec2(_barFront.w, 0f);
		_addXPBar = new Sprite("online/xpAddBar");
		_addXPBar.CenterOrigin();
		_bigFont = new BitmapFont("intermissionFont", 24, 23);
		_littleMan = new SpriteMap("littleMan", 16, 16);
		_thickBiosNum = new BitmapFont("thickBiosNum", 16, 16);
		_font = new BitmapFont("biosFontUI", 8, 7);
		_fancyFont = new FancyBitmapFont("smallFont");
		_chalkFont = new FancyBitmapFont("online/chalkFont");
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
		_egg = Profile.GetEggSprite(Profiles.experienceProfile.numLittleMen, 0uL);
		_currentLevel = 0;
		_desiredLevel = 0;
		if (Profiles.experienceProfile != null)
		{
			_xpValue = Profiles.experienceProfile.xp;
			_newXPValue = (_oldXPValue = _xpValue);
			if (_xpValue >= DuckNetwork.GetLevel(9999).xpRequired)
			{
				_desiredLevel = (_currentLevel = DuckNetwork.GetLevel(9999).num);
			}
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
		_currentDay = Profiles.experienceProfile.currentDay;
		SFX.Play("pause");
		_totalXP = DuckNetwork.GetTotalXPEarned();
		_originalXP = _xpValue;
		if (Profiles.experienceProfile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0)
		{
			HUD.CloseAllCorners();
			HUD.AddCornerControl(HUDCorner.BottomLeft, "@START@SKIP XP");
		}
	}

	public override void Close()
	{
		base.Close();
		HUD.ClearCorners();
	}

	public override void OnClose()
	{
		if (!FurniShopScreen.open && _prevShopOpen)
		{
			Music.Play("CharacterSelect");
		}
		DuckNetwork.finishedMatch = false;
		DuckNetwork._xpEarned.Clear();
		Profiles.experienceProfile.xp = _xpValue;
		Profiles.Save(Profiles.experienceProfile);
		UIMenu lastBox = null;
		Graphics.fadeAdd = 0f;
		Graphics.flashAdd = 0f;
		if (Unlockables.HasPendingUnlocks())
		{
			lastBox = new UIUnlockBox(Unlockables.GetPendingUnlocks().ToList(), Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f);
		}
		if (rareGachas > 0 || gachas > 0)
		{
			UIGachaBoxNew.skipping = skipping;
			UIGachaBoxNew box = new UIGachaBoxNew(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, rare: true, lastBox);
			if (!skipping)
			{
				lastBox = box;
			}
		}
		Global.Save();
		Profiles.Save(Profiles.experienceProfile);
		if (_gotEgg && Profiles.experienceProfile.numLittleMen > 8)
		{
			UIWillBox will = new UIWillBox(UIGachaBox.GetRandomFurniture(Rarity.SuperRare, 1, 0.3f)[0], Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, lastBox);
			lastBox = new UIFuneral(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, will);
		}
		if (lastBox != null)
		{
			MonoMain.pauseMenu = lastBox;
		}
	}

	public static int LittleManFrame(int idx, int curLev, ulong seed = 0uL, bool bottomBar = false)
	{
		if (seed == 0L && Profiles.experienceProfile != null)
		{
			seed = Profiles.experienceProfile.steamID;
		}
		if (bottomBar && idx < Profiles.experienceProfile.numLittleMen - 6)
		{
			curLev = 7;
		}
		int rlLev = curLev - 3;
		if (curLev < 0)
		{
			rlLev = 2 + new Random((int)((long)seed + (long)idx)).Next(0, 2);
		}
		Random oldGen = Rando.generator;
		Rando.generator = Profile.GetLongGenerator(seed);
		for (int i = 0; i < idx * 4; i++)
		{
			Rando.Int(100);
		}
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
		{
			Rando.Int(10);
		}
		if (Rando.Int(30) == 0)
		{
			baseNum = (baseNum + 60) % 120;
		}
		baseNum += rlLev;
		baseNum += baseInc * 5;
		if (bottomBar && idx < Profiles.experienceProfile.numLittleMen - 6)
		{
			baseNum += (1 + Rando.Int(2)) * 5;
		}
		Rando.generator = oldGen;
		return baseNum;
	}

	public DayType GetDay(int day)
	{
		if (day % 10 == 7 && _currentLevel >= 3)
		{
			return DayType.PawnDay;
		}
		switch (day)
		{
		case 3:
			return DayType.Sandwich;
		case 6:
			return DayType.ToyDay;
		default:
		{
			if (day % 5 == 4 && day % 20 != 19 && _currentLevel >= 3)
			{
				return DayType.Shop;
			}
			if (day % 5 == 4 && day % 20 == 19 && _currentLevel >= 3)
			{
				return DayType.SaleDay;
			}
			if (day % 20 == 0 && day > 6 && _currentLevel >= 3)
			{
				return DayType.ImportDay;
			}
			if (day % 10 == 8 && _currentLevel >= 5)
			{
				return DayType.PayDay;
			}
			if (day % 40 == 3 && day > 6 && _currentLevel >= 3)
			{
				return DayType.Special;
			}
			if (day % 20 == 13 && _currentLevel >= 3)
			{
				return DayType.FreeXP;
			}
			if (day % 20 == 1 && day > 5 && _currentLevel >= 3)
			{
				return DayType.Sandwich;
			}
			if (day % 20 == 10 && day > 5 && _currentLevel >= 3)
			{
				return DayType.Sandwich;
			}
			if (day % 40 == 6 && _currentLevel >= 3)
			{
				return DayType.Empty;
			}
			if (day % 60 == 57 && day > 5 && _currentLevel >= 3)
			{
				return DayType.Sandwich;
			}
			if (day % 60 == 37 && _currentLevel >= 3)
			{
				return DayType.FreeXP;
			}
			if (day % 20 == 12 && _currentLevel >= 3)
			{
				return DayType.ToyDay;
			}
			if (day % 40 == 7 && _currentLevel >= 3)
			{
				return DayType.ToyDay;
			}
			if (day % 40 == 26 && _currentLevel >= 3)
			{
				return DayType.ToyDay;
			}
			if (day % 80 == 25 && _currentLevel >= 3)
			{
				return DayType.Special;
			}
			if (day % 80 == 65 && _currentLevel >= 3)
			{
				return DayType.FreeXP;
			}
			if (day % 40 == 16 && _currentLevel >= 3)
			{
				return DayType.FreeXP;
			}
			if (day % 40 == 36 && day > 5 && _currentLevel >= 3)
			{
				return DayType.Sandwich;
			}
			if (day % 20 == 2 && _currentLevel >= 3)
			{
				return DayType.Allowance;
			}
			Random generator = Rando.generator;
			Rando.generator = new Random(day);
			int rand = Rando.Int(32);
			float rand2 = Rando.Float(1f);
			Rando.generator = generator;
			switch (rand)
			{
			case 0:
				return DayType.FreeXP;
			case 1:
				if (day > 6)
				{
					return DayType.Sandwich;
				}
				return DayType.FreeXP;
			case 2:
				if (day < 5)
				{
					return DayType.ToyDay;
				}
				return DayType.Special;
			case 3:
				return DayType.ToyDay;
			default:
				if (Unlockables.lockedItems.Count > 18)
				{
					return DayType.HintDay;
				}
				if (Unlockables.lockedItems.Count > 14 && rand2 > 0.2f)
				{
					return DayType.HintDay;
				}
				if (Unlockables.lockedItems.Count > 10 && rand2 > 0.3f)
				{
					return DayType.HintDay;
				}
				if (Unlockables.lockedItems.Count > 5 && rand2 > 0.5f)
				{
					return DayType.HintDay;
				}
				if (Unlockables.lockedItems.Count > 0 && rand2 > 0.75f)
				{
					return DayType.HintDay;
				}
				return DayType.Empty;
			}
		}
		}
	}

	private bool IsVinceDay(DayType d)
	{
		if (d != DayType.Special && d != DayType.PawnDay && d != DayType.ImportDay && d != DayType.SaleDay && d != DayType.Shop)
		{
			return d == DayType.HintDay;
		}
		return true;
	}

	public void AdvanceDay()
	{
		GetDay(Profiles.experienceProfile.currentDay);
	}

	public void Say(string s)
	{
		sayQueue.Add(s);
	}

	public override void UpdateParts()
	{
		if (!FurniShopScreen.open && _prevShopOpen)
		{
			Music.Play("CharacterSelect");
		}
		_prevShopOpen = FurniShopScreen.open;
		InputProfile.repeat = false;
		Keyboard.repeat = false;
		base.UpdateParts();
		_sound.Update();
		currentLevel = _currentLevel;
		if (_confirmMenu != null)
		{
			_confirmMenu.DoUpdate();
		}
		while (littleEggs.Count < Math.Min(Profiles.experienceProfile.numLittleMen, 8))
		{
			littleEggs.Add(Profile.GetEggSprite(Math.Max(0, Profiles.experienceProfile.numLittleMen - 9) + littleEggs.Count, 0uL));
		}
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
			{
				FurniShopScreen.close = true;
			}
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
					_confirmMenu = new UIPresentBox(RoomEditor.GetFurniture("YOYO"), Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f);
					_confirmMenu.depth = 0.98f;
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
					_confirmMenu = new UIPresentBox(RoomEditor.GetFurniture("VOODOO VINCENT"), Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f);
					_confirmMenu.depth = 0.98f;
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
					_confirmMenu = new UIPresentBox(RoomEditor.GetFurniture("PERIMETER DEFENCE"), Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f);
					_confirmMenu.depth = 0.98f;
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
					_confirmMenu = new UIMenu((Vincent.type == DayType.PawnDay) ? "SELL TO VINCENT?" : "BUY FROM VINCENT?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 230f, -1f, (Vincent.type == DayType.PawnDay) ? "@CANCEL@CANCEL @SELECT@SELECT" : "@CANCEL@CANCEL @SELECT@SELECT");
					_confirmMenu.Add(new UIText(FurniShopScreen.attemptBuy.name, Color.Green));
					_confirmMenu.Add(new UIText(" ", Color.White));
					_confirmMenu.Add(new UIText(" ", Color.White));
					_confirmMenu.Add(new UIMenuItem((Vincent.type == DayType.PawnDay) ? ("SELL |WHITE|(|LIME|$" + FurniShopScreen.attemptBuy.cost + "|WHITE|)") : ("BUY |WHITE|(|LIME|$" + FurniShopScreen.attemptBuy.cost + "|WHITE|)"), new UIMenuActionCloseMenuSetBoolean(_confirmMenu, _menuBool)));
					_confirmMenu.Add(new UIMenuItem("CANCEL", new UIMenuActionCloseMenu(_confirmMenu), UIAlign.Center, Colors.MenuOption, backButton: true));
					_confirmMenu.depth = 0.98f;
					_confirmMenu.DoInitialize();
					_confirmMenu.Close();
					for (int i = 0; i < 10; i++)
					{
						_confirmMenu.DoUpdate();
					}
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
							{
								Profiles.experienceProfile.SetNumFurnitures(FurniShopScreen.attemptBuy.furnitureData.index, Profiles.experienceProfile.GetNumFurnitures(FurniShopScreen.attemptBuy.furnitureData.index) + 1);
							}
							else if (FurniShopScreen.attemptBuy.teamData != null)
							{
								Global.boughtHats.Add(FurniShopScreen.attemptBuy.teamData.name);
							}
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
					_confirmMenu = new UIMenu("LEAVE VINCENT?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@SELECT@SELECT");
					_confirmMenu.depth = 0.98f;
					_confirmMenu.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuSetBoolean(_confirmMenu, _menuBool)));
					_confirmMenu.Add(new UIMenuItem("NO!", new UIMenuActionCloseMenu(_confirmMenu), UIAlign.Center, default(Color), backButton: true));
					_confirmMenu.DoInitialize();
					_confirmMenu.Close();
					for (int j = 0; j < 10; j++)
					{
						_confirmMenu.DoUpdate();
					}
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
		{
			Vincent.Update();
		}
		skipUpdate = false;
		if ((FurniShopScreen.open && !Vincent.showingDay) || (_confirmMenu != null && _confirmMenu.open))
		{
			return;
		}
		if (!doubleUpdating && Input.Down("SELECT"))
		{
			doubleUpdating = true;
			UpdateParts();
			UpdateParts();
			doubleUpdating = false;
		}
		if (Input.Pressed("START") && !_littleManLeave && Profiles.experienceProfile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0 && !FurniShopScreen.open)
		{
			skipping = true;
		}
		if (!doubleUpdating && skipping)
		{
			doubleUpdating = true;
			for (int k = 0; k < 200; k++)
			{
				SFX.skip = true;
				UpdateParts();
				SFX.skip = false;
				if (_littleManLeave || !skipping)
				{
					break;
				}
			}
			doubleUpdating = false;
			if (_finned)
			{
				Graphics.fadeAdd = 1f;
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
				if (!(_levelSlideWait >= 1f))
				{
					return;
				}
				int curLev = _currentLevel;
				if (Profiles.experienceProfile.numLittleMen > 0)
				{
					curLev = Profiles.experienceProfile.littleManLevel;
				}
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
				{
					_overrideSlide = "EGG HATCH";
				}
				if (curLev == 6)
				{
					_overrideSlide = "GROWN UP";
				}
				if (curLev == 7)
				{
					_overrideSlide = "MAX OUT!";
				}
				if (_unSlide)
				{
					_intermissionSlide = Lerp.FloatSmooth(_intermissionSlide, 0f, 0.45f);
					if (_intermissionSlide <= 0.01f)
					{
						playedSound = false;
						_unSlide = false;
						_didSlide = true;
						_intermissionSlide = 0f;
						SFX.Play("levelUp");
					}
					return;
				}
				_intermissionSlide = Lerp.FloatSmooth(_intermissionSlide, 1f, 0.21f, 1.05f);
				if (_intermissionSlide >= 1f)
				{
					_slideWait += 0.08f;
					if (_slideWait >= 1f)
					{
						_unSlide = true;
						_slideWait = 0f;
					}
				}
				return;
			}
			_levelSlideWait = 0f;
			Graphics.fadeAdd = Lerp.Float(Graphics.fadeAdd, 1f, 0.1f);
			if (!(Graphics.fadeAdd >= 1f))
			{
				return;
			}
			_didSlide = false;
			_currentLevel = _desiredLevel;
			_talkLine = "";
			_feedLine = "I AM A HUNGRY\nLITTLE MAN.";
			_startFeedLine = _feedLine;
			Profiles.experienceProfile.littleManLevel = _newGrowthLevel;
			int curLev2 = _currentLevel;
			if (Profiles.experienceProfile.numLittleMen > 0)
			{
				curLev2 = Profiles.experienceProfile.littleManLevel;
			}
			if (_currentLevel == 3)
			{
				_oldGachaValue = _gachaValue;
				_newGachaValue = (float)_gachaValue + (_newXPValue - (float)_xpValue);
			}
			if (_currentLevel == 4)
			{
				_oldSandwichValue = _sandwichValue;
				_newSandwichValue = (float)_sandwichValue + (_newXPValue - (float)_xpValue);
			}
			if (_currentLevel >= 7)
			{
				_oldMilkValue = _milkValue;
				_newMilkValue = (float)_milkValue + (_newXPValue - (float)_xpValue);
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
			Graphics.fadeAdd = Lerp.Float(Graphics.fadeAdd, 0f, 0.1f);
			if (Graphics.fadeAdd > 0.01f)
			{
				return;
			}
		}
		if (!_talking)
		{
			_talk = 0f;
		}
		else
		{
			_talkWait += 0.2f;
			if (_talkWait >= 1f)
			{
				_talkWait = 0f;
				if (_feedLine.Length > 0)
				{
					_talkLine += _feedLine[0];
					if (_feedLine[0] == '.' || _feedLine[0] == '!' || _feedLine[0] == '?')
					{
						SFX.Play("tinyTick", 1f, Rando.Float(-0.1f, 0.1f));
						_talkWait = -2f;
					}
					else
					{
						SFX.Play("tinyNoise1", 1f, Rando.Float(-0.1f, 0.1f));
					}
					_feedLine = _feedLine.Remove(0, 1);
				}
			}
			_alwaysClose = false;
			if (_talking && _talkLine == _startFeedLine)
			{
				_alwaysClose = true;
				_finishTalkWait += 0.1f;
				if (_finishTalkWait > 6f && !eggTalk)
				{
					_talking = false;
					_talkLine = "";
					_finishTalkWait = 0f;
				}
			}
			if (_talkLine.Length > 0 && _talkLine[_talkLine.Length - 1] == '.')
			{
				_alwaysClose = true;
			}
			_talk += ((!close) ? 0.2f : (-0.2f));
			if (_talk > 2f)
			{
				_talk = 2f;
				close = true;
			}
			if (_talk < 0f)
			{
				_talk = 0f;
				if (!_alwaysClose)
				{
					close = false;
				}
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
				Graphics.fadeAdd = 1f;
			}
			_littleManStartWait += 0.02f;
			if (!(_littleManStartWait >= 1f))
			{
				return;
			}
			if (_driveAway)
			{
				_sound.lerpVolume = 1f;
				_taxiDrive = Lerp.Float(_taxiDrive, 1f, 0.03f);
				if (_taxiDrive >= 1f)
				{
					SFX.Play("doorOpen");
					_driveAway = false;
					_genericWait = 0.5f;
					ConstantSound sound = _sound;
					float volume = (_sound.lerpVolume = 0f);
					sound.volume = volume;
				}
			}
			else if (_taxiDrive > 0f)
			{
				if (!_inTaxi)
				{
					SFX.Play("doorClose");
					_inTaxi = true;
					_genericWait = 0.5f;
					return;
				}
				_sound.lerpVolume = 1f;
				_taxiDrive = Lerp.Float(_taxiDrive, 2f, 0.03f);
				if (_taxiDrive >= 2f)
				{
					_sound.lerpVolume = 0f;
					Profiles.experienceProfile.numLittleMen++;
					Profiles.experienceProfile.littleManLevel = 1;
					_newGrowthLevel = Profiles.experienceProfile.littleManLevel + 1;
					_egg = Profile.GetEggSprite(Profiles.experienceProfile.numLittleMen, 0uL);
					_littleManStartWait = 0f;
					_littleManLeave = false;
					_driveAway = false;
					_taxiDrive = 0f;
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
				Furniture specialGift = null;
				specialGift = ((Profiles.experienceProfile.numLittleMen == 0) ? RoomEditor.GetFurniture("EGG") : ((Profiles.experienceProfile.numLittleMen == 1) ? RoomEditor.GetFurniture("PHOTO") : ((Profiles.experienceProfile.numLittleMen == 2) ? RoomEditor.GetFurniture("PLATE") : ((Profiles.experienceProfile.numLittleMen == 3) ? RoomEditor.GetFurniture("GIFT BASKET") : ((Profiles.experienceProfile.numLittleMen == 4) ? RoomEditor.GetFurniture("WINE") : ((Profiles.experienceProfile.numLittleMen == 5) ? RoomEditor.GetFurniture("JUNK") : ((Profiles.experienceProfile.numLittleMen == 6) ? RoomEditor.GetFurniture("EASEL") : ((Profiles.experienceProfile.numLittleMen != 7) ? UIGachaBox.GetRandomFurniture(Rarity.VeryRare, 1, 0.75f)[0] : RoomEditor.GetFurniture("JUKEBOX")))))))));
				_confirmMenu = new UIPresentBox(specialGift, Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f);
				_confirmMenu.depth = 0.98f;
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
		{
			_stampCardLerp = Lerp.FloatSmooth(_stampCardLerp, _stampCard ? 1f : 0f, 0.18f, 1.05f);
		}
		foreach (LittleHeart heart in _hearts)
		{
			heart.position += heart.velocity;
			heart.alpha -= 0.02f;
		}
		_hearts.RemoveAll((LittleHeart littleHeart) => littleHeart.alpha <= 0f);
		_coinLerp2 = Lerp.Float(_coinLerp2, 1f, 0.08f);
		_stampWobble = Lerp.Float(_stampWobble, 0f, 0.08f);
		if (_stampCard || _stampCardLerp > 0.01f)
		{
			if (!_showCard)
			{
				if (!_sandwichShift)
				{
					_sandwichLerp = Lerp.Float(_sandwichLerp, 1f, 0.12f);
					if (_sandwichLerp >= 1f)
					{
						_sandwichShift = true;
					}
				}
				else if (_burp)
				{
					_extraMouthOpen = Lerp.FloatSmooth(_extraMouthOpen, 0f, 0.17f, 1.05f);
					_finalWait += 0.1f;
					if (_finalWait >= 1f)
					{
						_finalWait = 0f;
						_finishEat = false;
						_afterEatWait = 0f;
						_burp = false;
						_sandwichLerp = 0f;
						_extraMouthOpen = 0f;
						_sandwichShift = false;
						_eatWait = 0f;
						_openWait = 0f;
						_showCard = true;
						_sandwichEat = 0f;
						_finishingNewStamp = false;
					}
				}
				else if (_finishEat)
				{
					_extraMouthOpen = Lerp.FloatSmooth(_extraMouthOpen, 0f, 0.17f, 1.05f);
					_afterEatWait += 0.08f;
					if (_afterEatWait >= 1f)
					{
						SFX.Play("healthyEat");
						for (int i2 = 0; i2 < 8; i2++)
						{
							_hearts.Add(new LittleHeart
							{
								position = littleManPos + new Vec2(8f + Rando.Float(-4f, 4f), 8f + Rando.Float(-6f, 6f)),
								velocity = new Vec2(0f, Rando.Float(-0.2f, -0.4f))
							});
						}
						_burp = true;
					}
				}
				else
				{
					_sandwichLerp = Lerp.Float(_sandwichLerp, 0f, 0.12f);
					if (_sandwichLerp <= 0f)
					{
						_extraMouthOpen = Lerp.FloatSmooth(_extraMouthOpen, 15f, 0.18f, 1.05f);
						if (_extraMouthOpen >= 1f)
						{
							_openWait += 0.08f;
							if (_openWait >= 1f)
							{
								_sandwichEat = Lerp.Float(_sandwichEat, 1f, 0.08f);
								if (_sandwichEat >= 1f)
								{
									if (_eatWait == 0f)
									{
										SFX.Play("swallow");
									}
									_eatWait += 0.08f;
									if (_eatWait >= 1f)
									{
										_finishEat = true;
									}
								}
							}
						}
					}
				}
			}
			if (!(_stampCardLerp >= 0.99f))
			{
				return;
			}
			_stampWait += 0.2f;
			if (!(_stampWait >= 1f))
			{
				return;
			}
			if (_stampWait2 == 0f)
			{
				Profiles.experienceProfile.numSandwiches++;
				_finishingNewStamp = true;
				_stampWobble = 1f;
				SFX.Play("dacBang");
			}
			_stampWait2 += 0.06f;
			if (_stampWait2 >= 1f)
			{
				if (Profiles.experienceProfile.numSandwiches > 0 && Profiles.experienceProfile.numSandwiches % 6 == 0 && _coin2Wait == 0f)
				{
					_coinLerp2 = 0f;
					_coin2Wait = 1f;
					SFX.Play("ching", 1f, 0.2f);
					rareGachas++;
				}
				_coin2Wait -= 0.08f;
				if (_coin2Wait <= 0f)
				{
					_stampCard = false;
					_stampWait2 = 0f;
					_stampWait = 0f;
					_coin2Wait = 0f;
				}
			}
			return;
		}
		_showCard = false;
		Vec2 target = new Vec2(base.x - 80f, base.y - 10f);
		if (!base.open)
		{
			return;
		}
		if (_currentLevel == _desiredLevel)
		{
			if (_currentLevel > 3 && _finned && Input.Pressed("MENU2"))
			{
				_talking = true;
				_finishTalkWait = 0f;
				_talkLine = "";
				if (Profiles.experienceProfile.littleManLevel <= 2 && _currentLevel > 6)
				{
					_feedLine = "";
					if (!Global.data.hadTalk)
					{
						if (_eggFeedIndex < _eggFeedLines.Count)
						{
							if (_eggFeedIndex > 2)
							{
								eggTalk = true;
							}
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
						{
							_feedLine = "...";
						}
					}
				}
				else
				{
					_feedLine = "I AM A HUNGRY\nLITTLE MAN.";
					if (Rando.Int(1000) == 1)
					{
						_feedLine = "I... AM A HUNGRY\nLITTLE MAN.";
					}
					DateTime now = MonoMain.GetLocalTime();
					if (!saidSpecial)
					{
						if (now.Month == 4 && now.Day == 20)
						{
							_feedLine = "HAPPY BIRTHDAY!";
						}
						else if (now.Month == 3 && now.Day == 9)
						{
							_feedLine = "HAPPY BIRTHDAY!!";
						}
						else if (now.Month == 1 && now.Day == 1)
						{
							_feedLine = "HAPPY NEW YEAR!";
						}
						else if (now.Month == 6 && now.Day == 4)
						{
							_feedLine = "HAPPY BIRTHDAY\nDUCK GAME!";
						}
						else if (Rando.Int(190000) == 1)
						{
							_feedLine = "LET'S DANCE!";
						}
						else if (Rando.Int(80000) == 1)
						{
							_feedLine = "HAPPY BIRTHDAY!";
						}
						saidSpecial = true;
					}
					if (Rando.Int(100000) == 1)
					{
						_feedLine = "I AM A HANGRY\nLITTLE MAN.";
					}
					else if (Rando.Int(150000) == 1)
					{
						_feedLine = "I AM A HAPPY\nLITTLE MAN.";
					}
				}
				_startFeedLine = _feedLine;
			}
			if (_state == UILevelBoxState.LogWinLoss)
			{
				if (Input.Pressed("SELECT"))
				{
					if (Profiles.experienceProfile != null)
					{
						Profiles.experienceProfile.xp = _xpValue;
					}
					SFX.Play("rockHitGround2", 1f, 0.5f);
					Close();
				}
			}
			else if (_state == UILevelBoxState.Wait)
			{
				_startWait -= 0.09f;
				if (_startWait < 0f)
				{
					_startWait = 1f;
					_state = UILevelBoxState.ShowXPBar;
					SFX.Play("rockHitGround2", 1f, 0.5f);
				}
			}
			else if (_state == UILevelBoxState.UpdateTime)
			{
				_advancedDay = false;
				_fallVel = 0f;
				_finned = false;
				_updateTime = false;
				_markedNewDay = false;
				_advanceDayWait = 0f;
				_dayFallAway = 0f;
				_dayScroll = 0f;
				_newCircleLerp = 0f;
				_popDay = false;
				_slideWait = 0f;
				_unSlide = false;
				_intermissionSlide = 0f;
				_intermissionWait = 0f;
				_gaveToy = false;
				if (_roundsPlayed > 0)
				{
					_updateTimeWait += 0.08f;
					if (_updateTimeWait >= 1f)
					{
						_dayTake += 0.8f;
						if (_dayTake >= 1f)
						{
							_dayTake = 0f;
							_roundsPlayed--;
						}
						_dayProgress = 1f - (float)_roundsPlayed / (float)_startRoundsPlayed;
						time += 0.08f;
					}
					if (time >= 1f)
					{
						time -= 1f;
						_state = UILevelBoxState.AdvanceDay;
						_updateTimeWait = 0f;
						_dayTake = 0f;
					}
				}
				else
				{
					_state = UILevelBoxState.Finished;
				}
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
					_giveMoneyRise = Lerp.Float(_giveMoneyRise, 1f, 0.05f);
					_finishDayWait += 0.04f;
					if (_finishDayWait >= 1f)
					{
						_giveMoneyRise = 1f;
						_giveMoney = 0;
						_state = UILevelBoxState.UpdateTime;
					}
					break;
				case DayType.PayDay:
					if (_giveMoney == 0)
					{
						int wage = 75;
						if (_currentLevel > 5)
						{
							wage = 100;
						}
						if (_currentLevel > 6)
						{
							wage = 125;
						}
						_giveMoney = wage + Profiles.experienceProfile.numLittleMen * 25;
						Profiles.experienceProfile.littleManBucks += _giveMoney;
						SFX.Play("ching");
					}
					_giveMoneyRise = Lerp.Float(_giveMoneyRise, 1f, 0.05f);
					_finishDayWait += 0.04f;
					if (_finishDayWait >= 1f)
					{
						_giveMoneyRise = 1f;
						_giveMoney = 0;
						_state = UILevelBoxState.UpdateTime;
					}
					break;
				case DayType.ToyDay:
					if (!_gaveToy)
					{
						_gaveToy = true;
						gachas++;
						_coinLerp = 0f;
						SFX.Play("ching", 1f, 0.2f);
					}
					_finishDayWait += 0.04f;
					if (_finishDayWait >= 1f)
					{
						_state = UILevelBoxState.UpdateTime;
					}
					break;
				case DayType.Sandwich:
					if (!_gaveToy)
					{
						_stampCard = true;
						_state = UILevelBoxState.UpdateTime;
					}
					break;
				case DayType.FreeXP:
					_didXPDay = true;
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
					if (_dayFallAway < 1f)
					{
						_dayFallAway += _fallVel;
						_fallVel += 0.005f;
					}
					else if (_dayScroll < 1f)
					{
						_dayScroll = Lerp.Float(_dayScroll, 1f, 0.1f);
					}
					else
					{
						_advancedDay = true;
						Profiles.experienceProfile.currentDay++;
						_dayFallAway = 0f;
						_dayScroll = 0f;
					}
				}
				else
				{
					_advanceDayWait += 0.1f;
					if (_advanceDayWait >= 1f)
					{
						if (!_markedNewDay)
						{
							SFX.Play("chalk");
							_currentDay = Profiles.experienceProfile.currentDay;
							_markedNewDay = true;
							DayType t2 = GetDay(Profiles.experienceProfile.currentDay);
							if (IsVinceDay(t2))
							{
								skipping = false;
								SFX.skip = false;
								SFX.Play("dacBang");
								Graphics.fadeAdd = 1f;
							}
						}
						if (_markedNewDay)
						{
							_intermissionWait += 0.15f;
							if (_intermissionWait >= 1f)
							{
								if (_unSlide)
								{
									_intermissionSlide = Lerp.FloatSmooth(_intermissionSlide, 0f, 0.42f);
									if (_intermissionSlide <= 0.02f)
									{
										_intermissionSlide = 0f;
										_dayStartWait += 0.11f;
										if (_dayStartWait >= 1f)
										{
											AdvanceDay();
											_state = UILevelBoxState.RunDay;
										}
										if (IsVinceDay(GetDay(Profiles.experienceProfile.currentDay)))
										{
											Vincent.showingDay = false;
										}
									}
								}
								else
								{
									_intermissionSlide = Lerp.FloatSmooth(_intermissionSlide, 1f, 0.2f, 1.05f);
									if (_intermissionSlide >= 1f)
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
										{
											_unSlide = true;
										}
									}
								}
							}
						}
						_newCircleLerp = Lerp.Float(_newCircleLerp, 1f, 0.2f);
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
					{
						HUD.AddCornerControl(HUDCorner.TopRight, "@MENU2@TALK");
					}
					if (Profiles.experienceProfile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0 && (rareGachas > 0 || gachas > 0))
					{
						HUD.AddCornerControl(HUDCorner.BottomLeft, "@START@AUTO TOYS");
					}
					_finned = true;
				}
			}
			else if (_state == UILevelBoxState.ShowXPBar)
			{
				_firstParticleIn = false;
				_drain = 1f;
				if (_currentStat.Key == null)
				{
					_currentStat = DuckNetwork.TakeXPStat();
				}
				if (_currentStat.Key == null)
				{
					if (_roundsPlayed > 0 && _currentLevel >= 4)
					{
						_state = UILevelBoxState.UpdateTime;
					}
					else
					{
						_state = UILevelBoxState.Finished;
					}
				}
				else
				{
					_slideXPBar = Lerp.FloatSmooth(_slideXPBar, 1f, 0.18f, 1.1f);
					if (_slideXPBar >= 1f)
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
				if (_startWait < 0f)
				{
					_startWait = 1f;
					_state = UILevelBoxState.DrainXPBar;
				}
			}
			else if (_state == UILevelBoxState.DrainXPBar)
			{
				_drain -= 0.04f;
				if (_drain > 0f)
				{
					_particleWait -= 0.4f;
					if (_particleWait < 0f)
					{
						float fullYOffset = 30f;
						if (_currentLevel == 3)
						{
							fullYOffset = 25f;
						}
						if (_currentLevel >= 4)
						{
							fullYOffset = 20f;
						}
						if (_currentLevel >= 4)
						{
							fullYOffset = 0f;
						}
						if (_currentLevel >= 7)
						{
							fullYOffset = -12f;
						}
						if (_currentStat.Value.type == 0 || _currentStat.Value.type == 4)
						{
							_particles.Add(new XPPlus
							{
								position = new Vec2(base.x - 72f, base.y - 58f),
								velocity = new Vec2(0f - Rando.Float(3f, 6f), 0f - Rando.Float(1f, 4f)),
								target = target + new Vec2(0f, fullYOffset),
								color = Colors.DGGreen
							});
						}
						if (_currentLevel >= 3 && (_currentStat.Value.type == 1 || _currentStat.Value.type == 4))
						{
							_particles.Add(new XPPlus
							{
								position = new Vec2(base.x - 72f, base.y - 58f),
								velocity = new Vec2(0f - Rando.Float(3f, 6f), 0f - Rando.Float(1f, 4f)),
								target = target + new Vec2(0f, 10f + fullYOffset),
								color = Colors.DGRed
							});
						}
						if (_currentLevel >= 4 && (_currentStat.Value.type == 2 || _currentStat.Value.type == 4))
						{
							_particles.Add(new XPPlus
							{
								position = new Vec2(base.x - 72f, base.y - 58f),
								velocity = new Vec2(0f - Rando.Float(3f, 6f), 0f - Rando.Float(1f, 4f)),
								target = target + new Vec2(0f, 20f + fullYOffset),
								color = Colors.DGBlue
							});
						}
						_xpLost += 1f;
						SFX.Play("tinyTick");
						_particleWait = 1f;
					}
				}
				if (_firstParticleIn)
				{
					_addLerp += 0.04f;
					_xpValue = (int)Lerp.FloatSmooth(_oldXPValue, _newXPValue, _addLerp);
					_gachaValue = (int)Lerp.FloatSmooth(_oldGachaValue, _newGachaValue, _addLerp);
					_sandwichValue = (int)Lerp.FloatSmooth(_oldSandwichValue, _newSandwichValue, _addLerp);
					_milkValue = (int)Lerp.FloatSmooth(_oldMilkValue, _newMilkValue, _addLerp);
					_xpProgress = (float)(_xpValue - _originalXP) / (float)_totalXP;
				}
				if (_drain < 0f)
				{
					_drain = 0f;
				}
				if (_drain <= 0f && _addLerp >= 1f)
				{
					_drain = 0f;
					_addLerp = 0f;
					_state = UILevelBoxState.HideXPBar;
				}
			}
			else if (_state == UILevelBoxState.HideXPBar)
			{
				_slideXPBar = Lerp.FloatSmooth(_slideXPBar, 0f, 0.2f, 1.1f);
				if (_slideXPBar <= 0.02f)
				{
					_currentStat = default(KeyValuePair<string, XPPair>);
					_state = UILevelBoxState.ShowXPBar;
					SFX.Play("rockHitGround2", 1f, 0.5f);
					_slideXPBar = 0f;
				}
			}
		}
		if (_currentLevel == _desiredLevel)
		{
			_coinLerp = Lerp.Float(_coinLerp, 1f, 0.1f);
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
					particle.velocity.y += 0.2f;
					particle.alpha -= 0.05f;
				}
			}
		}
		int c = _particles.Count;
		_particles.RemoveAll((XPPlus part) => (part.position - part.target).lengthSq < 64f);
		if (_particles.Count != c)
		{
			_firstParticleIn = true;
		}
		if (_xpValue >= DuckNetwork.GetLevel(_desiredLevel + 1).xpRequired && _currentLevel != 20)
		{
			_desiredLevel++;
		}
		if (_currentLevel <= 2)
		{
			return;
		}
		if (_gachaValue >= gachaNeed)
		{
			_gachaValue -= gachaNeed;
			_newGachaValue -= gachaNeed;
			_oldGachaValue -= gachaNeed;
			gachas++;
			_coinLerp = 0f;
			SFX.Play("ching", 1f, 0.2f);
		}
		if (_milkValue >= milkNeed)
		{
			_milkValue -= milkNeed;
			_newMilkValue -= milkNeed;
			_oldMilkValue -= milkNeed;
			_newGrowthLevel = Profiles.experienceProfile.littleManLevel + 1;
			if (_newGrowthLevel > 7)
			{
				_newGrowthLevel = 7;
			}
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
		{
			return;
		}
		int offsetLevbar = 33;
		if (_currentLevel >= 3)
		{
			offsetLevbar = 38;
		}
		if (_currentLevel >= 4)
		{
			offsetLevbar = 63;
		}
		if (_currentLevel >= 7)
		{
			offsetLevbar = 75;
		}
		Vec2 addBarPos = new Vec2(base.x, base.y - (float)offsetLevbar * _slideXPBar);
		int curLevMax = _currentLevel;
		if (curLevMax > 8)
		{
			curLevMax = 8;
		}
		Sprite sprite = _frames[curLevMax - 1];
		float fullYOffset = 30f;
		if (_currentLevel == 3)
		{
			fullYOffset = 25f;
		}
		if (_currentLevel >= 4)
		{
			fullYOffset = 20f;
		}
		if (_currentLevel >= 4)
		{
			fullYOffset = 0f;
		}
		if (_currentLevel >= 7)
		{
			fullYOffset = -12f;
		}
		sprite.depth = base.depth;
		Graphics.Draw(sprite, base.x, base.y);
		string text = "@LWING@" + Profiles.experienceProfile.name + "@RWING@";
		float hOffset = 0f;
		float vOffset = 0f;
		Vec2 fontscale = new Vec2(1f, 1f);
		if (Profiles.experienceProfile.name.Length > 9)
		{
			fontscale = new Vec2(0.75f, 0.75f);
			vOffset = 1f;
			hOffset = 1f;
		}
		if (Profiles.experienceProfile.name.Length > 12)
		{
			fontscale = new Vec2(0.5f, 0.5f);
			vOffset = 2f;
			hOffset = 1f;
		}
		_font.scale = fontscale;
		Vec2 fontPos = new Vec2(0f - _font.GetWidth(text) / 2f, -50f + fullYOffset);
		_font.DrawOutline(text, position + fontPos + new Vec2(hOffset, vOffset), Color.White, Color.Black, base.depth + 2);
		_font.scale = new Vec2(1f, 1f);
		_lev.depth = base.depth + 2;
		_lev.frame = _currentLevel - 1;
		Graphics.Draw(_lev, base.x - 90f, base.y - 34f + fullYOffset);
		_font.DrawOutline(_currentLevel.ToString()[0].ToString() ?? "", position + new Vec2(-84f, -30f + fullYOffset), Color.White, Color.Black, base.depth + 2);
		if (_currentLevel > 9)
		{
			_thickBiosNum.scale = new Vec2(0.5f, 0.5f);
			_thickBiosNum.Draw(_currentLevel.ToString()[1].ToString() ?? "", position + new Vec2(-78f, -32f + fullYOffset), Color.White, base.depth + 20);
		}
		float wide = 85f;
		if (_currentLevel == 1)
		{
			wide = 175f;
		}
		if (_currentLevel == 2)
		{
			wide = 154f;
		}
		if (_currentLevel == 3)
		{
			wide = 122f;
		}
		if (_currentLevel >= 4)
		{
			wide = 94f;
		}
		if (_currentLevel >= 7)
		{
			wide = 83f;
		}
		float fill = 0.5f;
		int max = DuckNetwork.GetLevel(_currentLevel + 1).xpRequired;
		int sub = 0;
		if (_currentLevel > 0)
		{
			sub = DuckNetwork.GetLevel(_currentLevel).xpRequired;
		}
		int val = (int)Math.Round((float)max * ((float)_xpValue / (float)max));
		int totalMax = DuckNetwork.GetLevel(9999).xpRequired;
		if (val > totalMax)
		{
			val = totalMax;
		}
		fill = ((max - sub != 0) ? ((float)(_xpValue - sub) / (float)(max - sub)) : 1f);
		string maxString = max.ToString();
		if (maxString.Length > 5)
		{
			maxString = maxString.Substring(0, 3) + "k";
		}
		else if (maxString.Length > 4)
		{
			maxString = maxString.Substring(0, 2) + "k";
		}
		string amountText = "|DGGREEN|" + val + "|WHITE|/|DGBLUE|" + maxString + "|WHITE|";
		float levelTextXOffset = 0f;
		if (_currentLevel == 1)
		{
			levelTextXOffset = 94f;
		}
		if (_currentLevel == 2)
		{
			levelTextXOffset = 70f;
		}
		if (_currentLevel == 3)
		{
			levelTextXOffset = 38f;
		}
		if (_currentLevel >= 4)
		{
			levelTextXOffset = 10f;
		}
		if (_currentLevel >= 7)
		{
			levelTextXOffset = -1f;
		}
		_fancyFont.DrawOutline(amountText, position + new Vec2(-8f + levelTextXOffset, -31f + fullYOffset) - new Vec2(_fancyFont.GetWidth(amountText), 0f), Colors.DGYellow, Color.Black, base.depth + 2);
		if (fill < 0.0235f)
		{
			fill = 0.0235f;
		}
		float barX = wide * fill;
		_xpBar.depth = base.depth + 2;
		_xpBar.xscale = 1f;
		Vec2 barPos = new Vec2(base.x - 87f, base.y - 18f);
		Graphics.Draw(_xpBar, barPos.x, barPos.y + fullYOffset, new Rectangle(0f, 0f, 3f, 6f));
		_xpBar.xscale = barX - 4f;
		Graphics.Draw(_xpBar, barPos.x + 3f, barPos.y + fullYOffset, new Rectangle(2f, 0f, 1f, 6f));
		_xpBar.depth = base.depth + 7;
		_xpBar.xscale = 1f;
		Graphics.Draw(_xpBar, barPos.x + (barX - 2f), barPos.y + fullYOffset, new Rectangle(3f, 0f, 3f, 6f));
		int startCut = 0;
		_barFront.depth = base.depth + 10;
		if (barX < 13f)
		{
			startCut = 13 - (int)barX;
		}
		_barHeat += Math.Abs(_lastFill - fill) * 8f;
		if (_barHeat > 1f)
		{
			_barHeat = 1f;
		}
		_barFront.alpha = _barHeat;
		Graphics.Draw(_barFront, barPos.x + barX + (float)startCut, barPos.y + fullYOffset, new Rectangle(startCut, 0f, _barFront.width - startCut, 6f));
		_barHeat = Maths.CountDown(_barHeat, 0.04f);
		if (_currentLevel >= 3)
		{
			float fill2 = (float)_gachaValue / (float)gachaNeed;
			float gachaWide = 110f;
			if (_currentLevel == 3)
			{
				gachaWide = 149f;
			}
			if (_currentLevel >= 4)
			{
				gachaWide = 122f;
			}
			float barX2 = (float)Math.Floor(gachaWide * fill2);
			if (barX2 < 2f)
			{
				barX2 = 2f;
			}
			_gachaBar.depth = base.depth + 2;
			_gachaBar.xscale = 1f;
			Vec2 barPos2 = new Vec2(base.x - 87f, base.y - 5f);
			Graphics.Draw(_gachaBar, barPos2.x, barPos2.y + fullYOffset, new Rectangle(0f, 0f, 3f, 3f));
			_gachaBar.xscale = barX2 - 5f;
			Graphics.Draw(_gachaBar, barPos2.x + 3f, barPos2.y + fullYOffset, new Rectangle(2f, 0f, 1f, 3f));
			_gachaBar.depth = base.depth + 7;
			_gachaBar.xscale = 1f;
			Graphics.Draw(_gachaBar, barPos2.x + (barX2 - 2f), barPos2.y + fullYOffset, new Rectangle(3f, 0f, 3f, 3f));
			_duckCoin.frame = 0;
			_duckCoin.alpha = 1f - Math.Max(_coinLerp - 0.5f, 0f) * 2f;
			_duckCoin.depth = 0.9f;
			Graphics.Draw(_duckCoin, barPos2.x + (gachaWide - 2f) + 15f, barPos2.y + fullYOffset - 8f - _coinLerp * 18f);
		}
		if (_currentLevel >= 4)
		{
			float fill3 = (float)_sandwichValue / (float)sandwichNeed;
			float sandwichWide = 154f;
			float barX3 = sandwichWide * fill3;
			if (barX3 < 2f)
			{
				barX3 = 2f;
			}
			_sandwichBar.depth = base.depth + 2;
			_sandwichBar.xscale = 1f;
			Vec2 barPos3 = new Vec2(base.x - 87f, base.y + 5f);
			Graphics.Draw(_sandwichBar, barPos3.x, barPos3.y + fullYOffset, new Rectangle(0f, 0f, 3f, 3f));
			_sandwichBar.xscale = barX3 - 5f;
			Graphics.Draw(_sandwichBar, barPos3.x + 3f, barPos3.y + fullYOffset, new Rectangle(2f, 0f, 1f, 3f));
			_sandwichBar.depth = base.depth + 7;
			_sandwichBar.xscale = 1f;
			Graphics.Draw(_sandwichBar, barPos3.x + (barX3 - 2f), barPos3.y + fullYOffset, new Rectangle(3f, 0f, 3f, 3f));
			_sandwich.depth = 0.88f;
			float yOffSandwich = _sandwichLerp * -150f;
			float xOffSandwich = 0f;
			float xCutoff = 0f;
			if (_sandwichShift)
			{
				yOffSandwich -= 20f;
				xOffSandwich = -42f - _sandwichEat * 30f;
				xCutoff = -52f - xOffSandwich;
				if (_currentLevel >= 7)
				{
					xOffSandwich -= 10f;
				}
			}
			xCutoff = Math.Max(xCutoff, 0f);
			if (xCutoff < (float)_sandwich.width)
			{
				Graphics.Draw(_sandwich, barPos3.x + (sandwichWide - 2f) + 12f + xOffSandwich + xCutoff + 1f, barPos3.y + fullYOffset - 16f + yOffSandwich, new Rectangle(xCutoff, 0f, (float)_sandwich.width - xCutoff, _sandwich.height), 0.88f);
			}
		}
		if (_currentStat.Key != null)
		{
			_addXPBar.depth = base.depth - 20;
			_addXPBar.xscale = 1f;
			Graphics.Draw(_addXPBar, addBarPos.x, addBarPos.y);
			string t = "";
			t = ((_currentStat.Value.num == 0) ? _currentStat.Key : (_currentStat.Value.num + " " + _currentStat.Key));
			_fancyFont.DrawOutline(t, addBarPos + new Vec2(-(_addXPBar.width / 2) + 4, -2f), Color.White, Color.Black, base.depth - 10);
			Vec2 vec = addBarPos + new Vec2(-(_addXPBar.width / 2) + 2, -7.5f);
			Graphics.DrawLine(vec, vec + new Vec2((float)(_addXPBar.width - 5) * _drain, 0f), Color.Lime, 1f, _addXPBar.depth + 2);
			string xpVal = (int)((float)_currentStat.Value.xp * _drain) + "|DGBLUE|XP";
			_fancyFont.DrawOutline(xpVal, addBarPos + new Vec2((float)(_addXPBar.width / 2) - _fancyFont.GetWidth(xpVal) - 4f, -2f), Colors.DGGreen, Color.Black, base.depth - 10);
		}
		foreach (XPPlus p in _particles)
		{
			int add = 20;
			if (p.splash)
			{
				add = 40;
			}
			float len = Math.Min((p.position - p.target).length, 30f) / 30f;
			_xpPoint.scale = new Vec2(len);
			_xpPointOutline.scale = new Vec2(len);
			_xpPoint.color = p.color;
			_xpPoint.alpha = p.alpha * len;
			_xpPoint.depth = base.depth + add;
			Graphics.Draw(_xpPoint, p.position.x, p.position.y);
			_xpPointOutline.alpha = p.alpha * len;
			_xpPointOutline.depth = base.depth + (add - 5);
			Graphics.Draw(_xpPointOutline, p.position.x, p.position.y);
		}
		foreach (LittleHeart p2 in _hearts)
		{
			_heart.alpha = p2.alpha;
			_heart.depth = 0.98f;
			_heart.scale = new Vec2(0.5f, 0.5f);
			Graphics.Draw(_heart, p2.position.x, p2.position.y);
		}
		int curlev = _currentLevel;
		if (Profiles.experienceProfile.numLittleMen > 0)
		{
			curlev = Profiles.experienceProfile.littleManLevel;
		}
		if (curlev > 7)
		{
			curlev = 7;
		}
		if (_currentLevel >= 2)
		{
			float mouthOpenAmount = 0f;
			mouthOpenAmount = (float)Math.Round(_talk) + _extraMouthOpen;
			int yOffSize = 0;
			if (curlev <= 4)
			{
				yOffSize = 1;
			}
			if (curlev <= 3)
			{
				yOffSize = 2;
			}
			if (curlev <= 2)
			{
				if (curlev == 2)
				{
					_egg.depth = 0.85f;
					_egg.yscale = 1f;
					int mouthHeight = 8;
					Vec2 littleEggPos = new Vec2(base.x + levelTextXOffset, base.y - 29f + fullYOffset + (float)mouthHeight + (float)yOffSize);
					Graphics.Draw(_egg, littleEggPos.x, littleEggPos.y, new Rectangle(0f, mouthHeight + yOffSize, 16f, 16 - mouthHeight - yOffSize));
					Graphics.Draw(_egg, base.x + levelTextXOffset, base.y - 29f + fullYOffset - mouthOpenAmount, new Rectangle(0f, 0f, 16f, mouthHeight + yOffSize));
					Vec2 center = _egg.center;
					_egg.yscale = mouthOpenAmount;
					_egg.center = center;
				}
			}
			else
			{
				_littleMan.frame = LittleManFrame(Profiles.experienceProfile.numLittleMen, curlev, 0uL);
				_littleMan.depth = 0.85f;
				_littleMan.yscale = 1f;
				littleManPos = new Vec2(base.x + levelTextXOffset, base.y - 29f + fullYOffset + 4f + (float)yOffSize);
				if (!_inTaxi)
				{
					Graphics.Draw(_littleMan, littleManPos.x, littleManPos.y, new Rectangle(0f, 4 + yOffSize, 16f, 12 - yOffSize));
					Graphics.Draw(_littleMan, base.x + levelTextXOffset, base.y - 29f + fullYOffset - mouthOpenAmount, new Rectangle(0f, 0f, 16f, 4 + yOffSize));
					Vec2 center2 = _littleMan.center;
					_littleMan.yscale = mouthOpenAmount;
					Graphics.Draw(_littleMan, base.x + levelTextXOffset, base.y - 29f + (fullYOffset - mouthOpenAmount) + 4f + (float)yOffSize, new Rectangle(0f, 4 + yOffSize, 16f, 1f));
					_littleMan.center = center2;
				}
			}
			_talkBubble.depth = 0.9f;
			string talk = _talkLine;
			if (_talkLine.Length > 0)
			{
				Vec2 talkPos = new Vec2(base.x + levelTextXOffset + 16f, base.y - 28f + fullYOffset);
				_talkBubble.xscale = 1f;
				Graphics.Draw(_talkBubble, talkPos.x, talkPos.y, new Rectangle(0f, 0f, 8f, 8f));
				float talkWidth = Graphics.GetStringWidth(talk) - 5f;
				float talkHeight = Graphics.GetStringHeight(talk) + 2f;
				_talkBubble.xscale = talkWidth;
				Graphics.Draw(_talkBubble, talkPos.x + 8f, talkPos.y, new Rectangle(5f, 0f, 1f, 2f));
				Graphics.Draw(_talkBubble, talkPos.x + 8f, talkPos.y + talkHeight, new Rectangle(5f, 10f, 1f, 2f));
				_talkBubble.xscale = 1f;
				Graphics.Draw(_talkBubble, talkPos.x, talkPos.y + (talkHeight - 2f), new Rectangle(0f, 8f, 8f, 4f));
				Graphics.Draw(_talkBubble, talkPos.x + talkWidth + 8f, talkPos.y + (talkHeight - 2f), new Rectangle(8f, 8f, 4f, 4f));
				Graphics.Draw(_talkBubble, talkPos.x + talkWidth + 8f, talkPos.y, new Rectangle(8f, 0f, 4f, 4f));
				Graphics.DrawRect(talkPos + new Vec2(5f, 2f), talkPos + new Vec2(talkWidth + 11f, talkHeight), Color.White, 0.9f);
				Graphics.DrawLine(talkPos + new Vec2(4.5f, 5f), talkPos + new Vec2(4.5f, talkHeight - 1f), Color.Black, 1f, 0.9f);
				Graphics.DrawLine(talkPos + new Vec2(11.5f + talkWidth, 4f), talkPos + new Vec2(11.5f + talkWidth, talkHeight - 1f), Color.Black, 1f, 0.9f);
				Graphics.DrawString(talk, talkPos + new Vec2(6f, 2f), Color.Black, 0.95f);
			}
		}
		if (_stampCardLerp > 0.01f)
		{
			float yOffStamp = 0f - (1f - _stampCardLerp) * 200f + (float)Math.Sin(_stampWobbleSin) * _stampWobble * 4f;
			Graphics.DrawRect(new Vec2(-1000f, -1000f), new Vec2(1000f, 1000f), Color.Black * 0.5f * _stampCardLerp, 0.96f);
			Graphics.Draw(_sandwichCard, base.x, base.y + yOffStamp, 0.97f);
			Random gen = Rando.generator;
			gen = (Rando.generator = new Random(365023));
			int numSan = Profiles.experienceProfile.numSandwiches % 6;
			if (Profiles.experienceProfile.numSandwiches > 0 && Profiles.experienceProfile.numSandwiches % 6 == 0 && _finishingNewStamp)
			{
				numSan = 6;
			}
			for (int i = 0; i < numSan; i++)
			{
				float xpos = i % 2 * 16;
				float ypos = i / 2 * 16;
				_sandwichStamp.angle = Rando.Float(-0.2f, 0.2f);
				_sandwichStamp.frame = Rando.Int(3);
				Graphics.Draw(_sandwichStamp, base.x + 30f + xpos + Rando.Float(-2f, 2f), base.y - 15f + ypos + Rando.Float(-2f, 2f) + yOffStamp, 0.98f);
				if (i == 5)
				{
					_duckCoin.frame = 1;
					_duckCoin.alpha = 1f - Math.Max(_coinLerp2 - 0.5f, 0f) * 2f;
					_duckCoin.depth = 0.99f;
					Graphics.Draw(_duckCoin, base.x + 30f + xpos, base.y - 15f + ypos + yOffStamp - _coinLerp2 * 18f);
				}
			}
			Rando.generator = gen;
		}
		if (_currentLevel >= 7)
		{
			_milk.depth = 0.7f;
			_milk.frame = (int)((float)_milkValue / (float)milkNeed * 15f);
			Graphics.Draw(_milk, base.x + 26f, base.y - 33f);
			Vec2 littleEggsPos = position + new Vec2(-88f, 44f);
			int eggIdx = 0;
			foreach (Sprite littleEgg in littleEggs)
			{
				littleEgg.depth = 0.85f;
				Graphics.Draw(littleEgg, littleEggsPos.x + (float)(eggIdx * 23) - 3f, littleEggsPos.y - 3f);
				_littleMan.frame = LittleManFrame(Math.Max(Profiles.experienceProfile.numLittleMen - 8, 0) + eggIdx, -1, 0uL, bottomBar: true);
				_littleMan.depth = 0.9f;
				_littleMan.yscale = 1f;
				Graphics.Draw(_littleMan, littleEggsPos.x + (float)(eggIdx * 23) + 3f, littleEggsPos.y + 1f);
				eggIdx++;
			}
		}
		float calYOffset = 0f;
		if (_currentLevel >= 7)
		{
			calYOffset = -12f;
		}
		Vec2 clockPos = position + new Vec2(75.5f, 33f + calYOffset);
		Vec2 clockPos2 = clockPos + new Vec2(0f, -7f);
		if (_currentLevel >= 4)
		{
			int munny = Profiles.experienceProfile.littleManBucks;
			string munnyString = "|DGGREEN|$";
			munnyString = ((munny <= 9999) ? (munnyString + munny) : (munnyString + munny / 1000 + "K"));
			Graphics.DrawRect(clockPos + new Vec2(-16f, 9f), clockPos + new Vec2(15f, 18f), Color.Black, 0.89f);
			_fancyFont.Draw(munnyString, clockPos + new Vec2(-16f, 9f) + new Vec2(30f - _fancyFont.GetWidth(munnyString), 0f), Color.White, 0.9f);
			if (_giveMoney > 0 && _giveMoneyRise < 0.95f)
			{
				string addString = "+" + _giveMoney;
				Color c = Colors.DGGreen;
				Color c2 = Color.Black;
				_fancyFont.DrawOutline(addString, clockPos + new Vec2(-16f, 9f) + new Vec2(30f - _fancyFont.GetWidth(addString), 0f - (10f + _giveMoneyRise * 10f)), c, c2, 0.97f);
			}
			Vec2 minuteHand = new Vec2
			{
				x = (0f - (float)Math.Sin(time * 12f * ((float)Math.PI * 2f) - (float)Math.PI)) * 8f,
				y = (float)Math.Cos(time * 12f * ((float)Math.PI * 2f) - (float)Math.PI) * 8f
			};
			Vec2 hourHand = new Vec2
			{
				x = (0f - (float)Math.Sin(time * ((float)Math.PI * 2f) - (float)Math.PI)) * 5f,
				y = (float)Math.Cos(time * ((float)Math.PI * 2f) - (float)Math.PI) * 5f
			};
			Graphics.DrawLine(clockPos2, clockPos2 + minuteHand, Color.Black, 1f, 0.9f);
			Graphics.DrawLine(clockPos2, clockPos2 + hourHand, Color.Black, 1.5f, 0.9f);
			Random generator = new Random(0);
			Random oldRand = Rando.generator;
			Rando.generator = generator;
			for (int j = 0; j < Profiles.experienceProfile.currentDay; j++)
			{
				Rando.Float(1f);
			}
			Math.Floor((float)Profiles.experienceProfile.currentDay / 5f);
			for (int k = 0; k < 5; k++)
			{
				float deepAdd = 0f;
				if (k == 0)
				{
					deepAdd += 0.1f;
				}
				float rot = Rando.Float(-0.1f, 0.1f);
				int num = (int)((rot + 0.1f) / 0.2f * 10f);
				if (_popDay && k == 0 && _dayFallAway != 0f)
				{
					_weekDays.angle = _ranRot;
				}
				else if (_currentLevel < 6)
				{
					_weekDays.angle = rot;
				}
				else
				{
					_weekDays.angle = 0f;
				}
				if (num == 3 && _currentLevel < 5)
				{
					_weekDays.angle += (float)Math.PI;
				}
				float yOff = 0f;
				float xOff = 0f;
				if (k == 0)
				{
					yOff = _dayFallAway * 100f;
				}
				xOff = (0f - _dayScroll) * 26f;
				if (k == 0)
				{
					_circle.depth = 0.85f + deepAdd;
					_circle.angle = _weekDays.angle;
					if (k == 0 && _advancedDay)
					{
						Graphics.Draw(_circle, position.x - 71f + (float)(k * 28) + xOff, position.y + 33f + calYOffset + yOff, new Rectangle(0f, 0f, (float)_circle.width * _newCircleLerp, _circle.height));
					}
					else
					{
						Graphics.Draw(_circle, position.x - 71f + (float)(k * 28) + xOff, position.y + 33f + calYOffset + yOff);
					}
				}
				_weekDays.depth = 0.83f + deepAdd;
				_weekDays.frame = (Profiles.experienceProfile.currentDay + k) % 5;
				_weekDays.frame += (int)Math.Floor((float)(Profiles.experienceProfile.currentDay + k) / 20f) % 4 * 6;
				Graphics.Draw(_weekDays, position.x - 71f + (float)(k * 28) + xOff, position.y + 33f + yOff + calYOffset);
				DayType t2 = GetDay(Profiles.experienceProfile.currentDay + k);
				if (t2 != DayType.Empty)
				{
					_days.depth = 0.84f + deepAdd;
					_days.frame = (int)t2;
					_days.angle = _weekDays.angle;
					Graphics.Draw(_days, position.x - 71f + (float)(k * 28) + xOff, position.y + calYOffset + 33f + yOff);
				}
			}
			Rando.generator = oldRand;
		}
		if (_confirmMenu != null && _confirmMenu.open)
		{
			Graphics.DrawRect(new Vec2(-1000f, -1000f), new Vec2(1000f, 1000f), Color.Black * 0.5f, 0.974f);
		}
		if (FurniShopScreen.open)
		{
			Graphics.DrawRect(new Vec2(-1000f, -1000f), new Vec2(1000f, 1000f), Color.Black * 0.5f, 0.95f);
			FurniShopScreen.open = true;
			Vincent.Draw();
		}
		if (_taxiDrive > 0f)
		{
			Vec2 taxiPos = new Vec2(position.x - 200f + _taxiDrive * 210f, position.y - 33f);
			_taxi.depth = 0.97f;
			Graphics.Draw(_taxi, taxiPos.x, taxiPos.y);
			if (_inTaxi)
			{
				_littleMan.frame = LittleManFrame(Profiles.experienceProfile.numLittleMen, curlev, 0uL);
				Graphics.Draw(_littleMan, taxiPos.x - 16f, taxiPos.y - 8f, new Rectangle(0f, 0f, 16f, 6f));
			}
		}
		if (_intermissionSlide > 0.01f)
		{
			float xpos2 = -320f + _intermissionSlide * 320f;
			float ypos2 = 60f;
			Graphics.DrawRect(new Vec2(xpos2, ypos2), new Vec2(xpos2 + 320f, ypos2 + 30f), Color.Black, 0.98f);
			xpos2 = 320f - _intermissionSlide * 320f;
			ypos2 = 60f;
			Graphics.DrawRect(new Vec2(xpos2, ypos2 + 30f), new Vec2(xpos2 + 320f, ypos2 + 60f), Color.Black, 0.98f);
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
			{
				slideText = _overrideSlide;
			}
			_bigFont.Draw(slideText, new Vec2(-320f + _intermissionSlide * (320f + Layer.HUD.width / 2f - _bigFont.GetWidth(slideText) / 2f), ypos2 + 18f), Color.White, 0.99f);
		}
		_lastFill = fill;
		if (_confirmMenu != null)
		{
			_confirmMenu.DoDraw();
		}
	}
}
