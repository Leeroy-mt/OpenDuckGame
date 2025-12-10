using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UIGachaBoxNew : UIMenu
{
    private Sprite _frame;

    private BitmapFont _font;

    private FancyBitmapFont _fancyFont;

    private SpriteMap _gachaEgg;

    private Sprite _furni;

    private Sprite _star;

    private Furniture _contains;

    private Sprite _whiteCircle;

    private bool _flash;

    private float yOffset = -250f;

    private UIMenu _openOnClose;

    private SpriteMap _duckCoin;

    private SpriteMap _capsule;

    private SpriteMap _capsuleBorder;

    private bool _rare;

    private bool _rareCapsule;

    private Material _flatColor = new MaterialFlatColor();

    private MaterialRainbow _rainbowMaterial = new MaterialRainbow();

    private Sprite _rainbow;

    private Sprite _gachaMachine;

    private Sprite _gachaGlass;

    private Sprite _gachaDoor;

    private Sprite _gachaTwister;

    private Sprite _gachaTwisterShadow;

    private SpriteMap _gachaBall;

    private SpriteMap _coin;

    private Sprite _coinSlot;

    private List<Furniture> prizes = new List<Furniture>();

    private bool didSkipPrompt;

    public static bool skipping;

    public int numGenerate = 3;

    public int numGenerateRare = 3;

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

    public bool down = true;

    private float _downWait = 1f;

    private float gachaY;

    private float gachaSpeed;

    private bool doubleUpdating;

    private Vec2 _eggOffset = Vec2.Zero;

    private Vec2 _toyPosition = Vec2.Zero;

    private Vec2 _toyVelocity = Vec2.Zero;

    private Vec2 _lastStick = Vec2.Zero;

    private float _toyAngle;

    private float _toyAngleLerp;

    private bool _coined;

    private float _initialWait;

    private bool didOpenToyCorner;

    private int _prizesGiven;

    private List<string> numberNames = new List<string> { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };

    private float rott;

    private int seed = 359392;

    public UIGachaBoxNew(float xpos, float ypos, float wide = -1f, float high = -1f, bool rare = false, UIMenu openOnClose = null)
        : base("", xpos, ypos, wide, high)
    {
        _openOnClose = openOnClose;
        _rare = rare;
        _duckCoin = new SpriteMap("duckCoin", 18, 18);
        _duckCoin.CenterOrigin();
        _gachaMachine = new Sprite("arcade/gotcha/machine");
        _gachaMachine.CenterOrigin();
        _gachaGlass = new Sprite("arcade/gotcha/glass");
        _gachaGlass.CenterOrigin();
        _gachaDoor = new Sprite("arcade/gotcha/door");
        _gachaDoor.CenterOrigin();
        _gachaTwister = new Sprite("arcade/gotcha/twister");
        _gachaTwister.CenterOrigin();
        _gachaBall = new SpriteMap("arcade/gotcha/balls", 40, 42);
        _gachaBall.CenterOrigin();
        _gachaTwisterShadow = new Sprite("arcade/gotcha/twisterShadow");
        _gachaTwisterShadow.CenterOrigin();
        _whiteCircle = new Sprite("furni/whiteCircle");
        _whiteCircle.CenterOrigin();
        _coin = new SpriteMap("arcade/gotcha/coin", 22, 22);
        _coin.CenterOrigin();
        _coinSlot = new Sprite("arcade/gotcha/coinSlot");
        _coinSlot.CenterOrigin();
        _rainbow = new Sprite("arcade/rainbow");
        Graphics.fade = 1f;
        _frame = new Sprite("unlockFrame");
        _frame.CenterOrigin();
        _furni = new Sprite("furni/stone");
        _furni.center = new Vec2(_furni.width / 2, _furni.height);
        _star = new Sprite("prettyStar");
        _star.CenterOrigin();
        _font = new BitmapFont("biosFontUI", 8, 7);
        _fancyFont = new FancyBitmapFont("smallFontGacha");
        _gachaEgg = new SpriteMap("gachaEgg", 44, 36);
        _capsule = new SpriteMap("arcade/egg", 40, 23);
        _capsule.CenterOrigin();
        _capsuleBorder = new SpriteMap("arcade/eggBorder", 66, 65);
        _capsuleBorder.CenterOrigin();
        _rare = false;
        numGenerate = MonoMain.core.gachas;
        numGenerateRare = MonoMain.core.rareGachas;
        for (int i = 0; i < numGenerate; i++)
        {
            UIGachaBox.useNumGachas = true;
            Furniture f = UIGachaBox.GetRandomFurniture(Rarity.Common, 1, 1f, gacha: true)[0];
            UIGachaBox.useNumGachas = false;
            Global.data.numGachas++;
            f.ballRot = Rando.Float(360f);
            f.rareGen = false;
            prizes.Add(f);
        }
        for (int j = 0; j < numGenerateRare; j++)
        {
            UIGachaBox.useNumGachas = true;
            Furniture f2 = (from x in UIGachaBox.GetRandomFurniture(Rarity.VeryVeryRare, 1, 0.4f, gacha: true)
                            orderby -x.rarity
                            select x).ElementAt(0);
            UIGachaBox.useNumGachas = false;
            Global.data.numGachas++;
            f2.ballRot = Rando.Float(360f);
            f2.rareGen = true;
            prizes.Add(f2);
        }
        for (int i2 = 0; i2 < 3; i2++)
        {
            UIGachaBox.useNumGachas = true;
            Furniture f3 = UIGachaBox.GetRandomFurniture(Rarity.Common, 1, 1f, gacha: true)[0];
            UIGachaBox.useNumGachas = false;
            Global.data.numGachas++;
            f3.ballRot = Rando.Float(360f);
            f3.rareGen = false;
            prizes.Add(f3);
        }
        if (skipping)
        {
            while (prizes.Count > 3)
            {
                LoadNextPrize();
                Profiles.experienceProfile.SetNumFurnitures(_contains.index, Profiles.experienceProfile.GetNumFurnitures(_contains.index) + 1);
                prizes.RemoveAt(0);
            }
            SFX.Play("harp");
            skipping = false;
        }
        LoadNextPrize();
        _gachaEgg.CenterOrigin();
    }

    public int FigureFrame(Furniture f)
    {
        if (f.rarity >= Rarity.SuperRare)
        {
            return 2;
        }
        if (f.rarity >= Rarity.VeryVeryRare)
        {
            return 0;
        }
        return 1;
    }

    public void LoadNextPrize()
    {
        _contains = prizes.ElementAt(0);
        _capsule.frame = FigureFrame(_contains);
    }

    public override void OnClose()
    {
        MonoMain.core.gachas = 0;
        MonoMain.core.rareGachas = 0;
        numGenerate = 0;
        numGenerateRare = 0;
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
        if (Profiles.experienceProfile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0 && Input.Pressed("START"))
        {
            skipping = true;
            SFX.Play("dacBang");
        }
        if (skipping)
        {
            while (prizes.Count > 3)
            {
                LoadNextPrize();
                Profiles.experienceProfile.SetNumFurnitures(_contains.index, Profiles.experienceProfile.GetNumFurnitures(_contains.index) + 1);
                prizes.RemoveAt(0);
            }
            skipping = false;
            finished = true;
            Close();
            HUD.CloseAllCorners();
            return;
        }
        if (!didSkipPrompt)
        {
            if (Profiles.experienceProfile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0)
            {
                HUD.AddCornerControl(HUDCorner.BottomLeft, "@START@SKIP");
            }
            didSkipPrompt = true;
        }
        if (!doubleUpdating && Input.Down("SELECT"))
        {
            doubleUpdating = true;
            UpdateParts();
            doubleUpdating = false;
        }
        if (!down && yOffset > -1f)
        {
            if (_initialWait < 1f)
            {
                _initialWait += 0.1f;
            }
            else if (_insertCoin < 1f)
            {
                if (!_chinged)
                {
                    SFX.Play("ching", 1f, Rando.Float(0.4f, 0.6f));
                    _chinged = true;
                }
                _insertCoinInc += 0.008f;
                _insertCoin += _insertCoinInc;
            }
            else
            {
                _insertCoin = 1f;
                if (_afterInsertWait < 1f)
                {
                    _afterInsertWait += 0.2f;
                }
                else
                {
                    if (_gachaWait >= 0.2f && !played)
                    {
                        played = true;
                        SFX.Play("gachaSound", 1f, Rando.Float(-0.1f, 0.1f));
                    }
                    if (!_coined && _gachaWait > 0.2f)
                    {
                        SFX.Play("gachaCoin", 1f, Rando.Float(0.4f, 0.6f));
                        _coined = true;
                    }
                    _gachaWait += 0.06f;
                    if (_gachaWait >= 1f)
                    {
                        _gachaWait = 1f;
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
                        float capsuleRadius = 8f;
                        _toyVelocity.y += 0.2f;
                        Vec2 dif = _toyPosition;
                        if (dif.length > capsuleRadius)
                        {
                            Vec2 prevToy = _toyPosition;
                            _toyPosition = dif.normalized * capsuleRadius;
                            Vec2 velChange = _toyPosition - prevToy;
                            _toyVelocity += velChange;
                            if ((double)velChange.length > 1.0)
                            {
                                SFX.Play("gachaBounce", 1f, 0.7f + Rando.Float(0.2f));
                            }
                            _toyAngleLerp = Maths.PointDirection(Vec2.Zero, _toyPosition);
                        }
                        Vec2 leftStick = InputProfile.active.leftStick;
                        if (InputProfile.active.lastActiveDevice is Keyboard)
                        {
                            if (InputProfile.active.Down("LEFT"))
                            {
                                leftStick.x = -1f;
                            }
                            if (InputProfile.active.Down("RIGHT"))
                            {
                                leftStick.x = 1f;
                            }
                            if (InputProfile.active.Down("UP"))
                            {
                                leftStick.y = 1f;
                            }
                            if (InputProfile.active.Down("DOWN"))
                            {
                                leftStick.y = -1f;
                            }
                        }
                        _toyVelocity += (_lastStick - leftStick) * new Vec2(2f, -2f);
                        _lastStick = leftStick;
                        _toyVelocity.x = Math.Max(Math.Min(_toyVelocity.x, 3f), -3f);
                        _toyVelocity.y = Math.Max(Math.Min(_toyVelocity.y, 3f), -3f);
                        _toyPosition += _toyVelocity;
                        if (!opened)
                        {
                            _toyAngle = Lerp.FloatSmooth(_toyAngle, _toyAngleLerp, 0.1f);
                            _eggOffset = Lerp.Vec2Smooth(_eggOffset, leftStick * 8f, 0.3f);
                        }
                        else
                        {
                            _toyAngle = Lerp.FloatSmooth(_toyAngle, -90f, 0.1f);
                            _eggOffset = Lerp.Vec2Smooth(_eggOffset, Vec2.Zero, 0.3f);
                            _toyPosition = Lerp.Vec2Smooth(_toyPosition, Vec2.Zero, 0.3f);
                        }
                        _openWait += 0.029f;
                        if (_openWait >= 1f)
                        {
                            if (!didOpenToyCorner)
                            {
                                HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@OPEN TOY");
                                didOpenToyCorner = true;
                            }
                            if (Input.Pressed("SELECT") && !opened)
                            {
                                opened = true;
                                SFX.Play("gachaOpen", 1f, Rando.Float(0.1f, 0.3f));
                                _gachaEgg.frame += 2;
                            }
                            if (opened)
                            {
                                _swapWait += 0.06f;
                                if (_swapWait >= 1f)
                                {
                                    if (!_swapped)
                                    {
                                        SFX.Play("harp");
                                        HUD.CloseAllCorners();
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
        }
        yOffset = Lerp.FloatSmooth(yOffset, down ? (-250f) : 0f, 0.4f);
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
        if (!down && _swapped && Input.Pressed("SELECT"))
        {
            played = false;
            _gachaWait = 0f;
            _openWait = 0f;
            finished = false;
            opened = false;
            _swapWait = 0f;
            _swapped = false;
            _starGrow = 0f;
            _insertCoin = 0f;
            _insertCoinInc = 0f;
            _afterInsertWait = 0f;
            _chinged = false;
            gachaY = 0f;
            gachaSpeed = 0f;
            doubleUpdating = false;
            _prizesGiven++;
            _eggOffset = Vec2.Zero;
            _toyPosition = Vec2.Zero;
            _toyVelocity = Vec2.Zero;
            _lastStick = Vec2.Zero;
            _toyAngle = 0f;
            _toyAngleLerp = 0f;
            _coined = false;
            _initialWait = 0f;
            didOpenToyCorner = false;
            HUD.CloseAllCorners();
            SFX.Play("resume", 0.6f);
            if (prizes.Count > 4)
            {
                prizes.RemoveAt(0);
                LoadNextPrize();
            }
            else
            {
                down = true;
                _swapped = true;
            }
        }
        base.UpdateParts();
    }

    public override void Draw()
    {
        if (base.animating)
        {
            return;
        }
        base.y += yOffset;
        Random r = Rando.generator;
        Rando.generator = new Random(seed);
        _gachaMachine.depth = -0.8f;
        Graphics.Draw(_gachaMachine, base.x - 14f, base.y);
        _coinSlot.depth = -0.795f;
        Graphics.Draw(_coinSlot, base.x - 13f, base.y - 13f);
        _coin.depth = 0.9f;
        for (int i = 0; i < numGenerate + numGenerateRare - (_prizesGiven + 1); i++)
        {
            if (numGenerate - (_prizesGiven + 1) > 0 && i < numGenerate - (_prizesGiven + 1))
            {
                _coin.frame = 0;
            }
            else
            {
                _coin.frame = 1;
            }
            _coin.depth = 0.9f - (float)i * 0.01f;
            Graphics.Draw(_coin, 16 + i * 4, 16f);
        }
        if (_contains.rareGen)
        {
            _coin.frame = 1;
        }
        else
        {
            _coin.frame = 0;
        }
        float coinRot = Math.Min(_gachaWait * 2f, 1f);
        _coin.depth = -0.798f;
        Graphics.Draw(_coin, base.x - 15f + coinRot * 21f, base.y - 25f - 40f * (1f - _insertCoin) + coinRot * 4f);
        _gachaGlass.depth = -0.9f;
        Graphics.Draw(_gachaGlass, base.x - 14f, base.y - 10f);
        _gachaDoor.depth = -0.84f;
        Graphics.Draw(_gachaDoor, base.x - 14f, base.y);
        Vec2 ballOffset = Vec2.Zero;
        _gachaBall.depth = -0.85f;
        _gachaBall.alpha = 0.3f;
        _gachaBall.angleDegrees = Rando.Float(360f);
        _gachaBall.frame = Rando.Int(2);
        float mul = Rando.Float(4f, 8f);
        ballOffset = new Vec2((float)Math.Sin(_gachaWait * mul + Rando.Float(4f)) * Rando.Float(1f, 2f), (float)Math.Sin(_gachaWait * mul + Rando.Float(4f)) * Rando.Float(1f, 2f));
        Graphics.Draw(_gachaBall, base.x - 56f + ballOffset.x, base.y - 54f + ballOffset.y);
        _gachaBall.angleDegrees = Rando.Float(360f);
        _gachaBall.frame = Rando.Int(2);
        mul = Rando.Float(4f, 8f);
        ballOffset = new Vec2((float)Math.Sin(_gachaWait * mul + Rando.Float(4f)) * Rando.Float(1f, 2f), (float)Math.Sin(_gachaWait * mul + Rando.Float(4f)) * Rando.Float(1f, 2f));
        Graphics.Draw(_gachaBall, base.x - 26f + ballOffset.x, base.y - 74f + ballOffset.y);
        _gachaBall.angleDegrees = Rando.Float(360f);
        _gachaBall.frame = Rando.Int(2);
        mul = Rando.Float(4f, 8f);
        ballOffset = new Vec2((float)Math.Sin(_gachaWait * mul + Rando.Float(4f)) * Rando.Float(1f, 2f), (float)Math.Sin(_gachaWait * mul + Rando.Float(4f)) * Rando.Float(1f, 2f));
        Graphics.Draw(_gachaBall, base.x - 62f + ballOffset.x, base.y - 94f + ballOffset.y);
        _gachaBall.angleDegrees = Rando.Float(360f);
        _gachaBall.frame = Rando.Int(2);
        mul = Rando.Float(4f, 8f);
        ballOffset = new Vec2((float)Math.Sin(_gachaWait * mul + Rando.Float(4f)) * Rando.Float(1f, 2f), (float)Math.Sin(_gachaWait * mul + Rando.Float(4f)) * Rando.Float(1f, 2f));
        Graphics.Draw(_gachaBall, base.x + 6f + ballOffset.x, base.y - 44f + ballOffset.y);
        _gachaBall.angleDegrees = Rando.Float(360f);
        _gachaBall.frame = Rando.Int(2);
        mul = Rando.Float(4f, 8f);
        ballOffset = new Vec2((float)Math.Sin(_gachaWait * mul + Rando.Float(4f)) * Rando.Float(1f, 2f), (float)Math.Sin(_gachaWait * mul + Rando.Float(4f)) * Rando.Float(1f, 2f));
        Graphics.Draw(_gachaBall, base.x + 31f + ballOffset.x, base.y - 64f + ballOffset.y);
        _gachaBall.angleDegrees = Rando.Float(360f);
        _gachaBall.frame = Rando.Int(2);
        mul = Rando.Float(4f, 8f);
        ballOffset = new Vec2((float)Math.Sin(_gachaWait * mul + Rando.Float(4f)) * Rando.Float(1f, 2f), (float)Math.Sin(_gachaWait * mul + Rando.Float(4f)) * Rando.Float(1f, 2f));
        Graphics.Draw(_gachaBall, base.x + 8f + ballOffset.x, base.y - 92f + ballOffset.y);
        _gachaBall.angleDegrees = prizes[2].ballRot;
        _gachaBall.frame = FigureFrame(prizes[2]);
        Graphics.Draw(_gachaBall, base.x + 31f - 42f + _gachaWait * 42f, base.y - 16f - 12f + _gachaWait * 12f);
        _gachaBall.angleDegrees = prizes[1].ballRot;
        _gachaBall.frame = FigureFrame(prizes[1]);
        Graphics.Draw(_gachaBall, base.x + 31f, base.y - 16f + _gachaWait * 42f);
        _gachaBall.angleDegrees = prizes[0].ballRot;
        _gachaBall.frame = FigureFrame(prizes[0]);
        Graphics.Draw(_gachaBall, base.x + 31f - _gachaWait * 42f, base.y - 16f + 42f);
        _gachaBall.alpha = 1f;
        _gachaBall.angleDegrees = 0f;
        _gachaBall.frame = Rando.Int(2);
        Sprite gachaTwister = _gachaTwister;
        float num = (_gachaTwisterShadow.angleDegrees = _gachaWait * 360f);
        gachaTwister.angleDegrees = num;
        Vec2 twisterPos = new Vec2(0f, -4f);
        _gachaTwister.depth = -0.1f;
        Graphics.Draw(_gachaTwister, base.x - 14f + twisterPos.x, base.y + twisterPos.y);
        Vec2 shadowOffset = new Vec2(2f, 2f);
        _gachaTwisterShadow.depth = -0.11f;
        _gachaTwisterShadow.alpha = 0.5f;
        Graphics.Draw(_gachaTwisterShadow, base.x - 14f + twisterPos.x + shadowOffset.x, base.y + twisterPos.y + shadowOffset.y);
        Material obj = Graphics.material;
        Graphics.material = _rainbowMaterial;
        _rainbowMaterial.offset += 0.05f;
        _rainbowMaterial.offset2 += 0.02f;
        _rainbow.alpha = 0.25f;
        _rainbow.depth = -0.95f;
        Graphics.Draw(_rainbow, 0f, 0f);
        Graphics.material = obj;
        Rando.generator = r;
        _frame.depth = -0.9f;
        if (_swapped)
        {
            _contains.Draw(position + new Vec2(0f, 10f), base.depth - 20);
            _whiteCircle.color = _contains.group.color;
            _whiteCircle.depth = base.depth - 30;
            Graphics.Draw(_whiteCircle, position.x, position.y + 10f);
            if (_starGrow <= 1f)
            {
                _star.depth = 0.9f;
                _star.scale = new Vec2(2.5f + _starGrow * 3f);
                _star.alpha = 1f - _starGrow;
                Graphics.Draw(_star, base.x, base.y + 10f);
            }
        }
        else if (gachaY > 10f)
        {
            Vec2 additionalEgg = new Vec2(-25f, 40f);
            float sep = 0f;
            if (opened)
            {
                sep = 3f;
            }
            _capsule.depth = -0.84f;
            Graphics.Draw(_capsule, base.x + _eggOffset.x + additionalEgg.x, base.y - 38f + gachaY - _eggOffset.y - (10f + sep) + additionalEgg.y);
            Material obj2 = Graphics.material;
            Graphics.material = _flatColor;
            _contains.Draw(new Vec2(base.x + _eggOffset.x + _toyPosition.x + additionalEgg.x, base.y - 38f + gachaY - _eggOffset.y - 10f + _toyPosition.y + 8f + additionalEgg.y), -0.835f, 0, null, affectScale: true, !_swapped, Maths.DegToRad(_toyAngle + 90f));
            Graphics.material = obj2;
            _capsule.depth = -0.83f;
            _capsule.frame += 3;
            Graphics.Draw(_capsule, base.x + _eggOffset.x + additionalEgg.x, base.y - 38f + gachaY - _eggOffset.y + (11f + sep) + additionalEgg.y, new Rectangle(0f, 2f, _capsule.width, _capsule.height - 2));
            _capsule.frame -= 3;
            if (gachaY > 30f && !opened)
            {
                _capsuleBorder.depth = -0.81f;
                _capsuleBorder.frame = 0;
                Graphics.Draw(_capsuleBorder, base.x + _eggOffset.x + additionalEgg.x, base.y - 38f + gachaY - _eggOffset.y - 2f + additionalEgg.y);
            }
        }
        if (_swapped)
        {
            string text = "@LWING@NEW TOY@RWING@";
            if (_rare)
            {
                text = "@LWING@RARE TOY@RWING@";
            }
            new Vec2(0f - _font.GetWidth(text) / 2f, -42f);
            string unlockText = "  ???  ";
            if (_swapped)
            {
                unlockText = "} " + _contains.name + " }";
            }
            _fancyFont.scale = new Vec2(1f, 1f);
            Vec2 unlockFontPos = new Vec2(0f - _fancyFont.GetWidth(unlockText) / 2f, -25f);
            _fancyFont.DrawOutline(unlockText, position + unlockFontPos, (_rare || (_swapped && _rareCapsule)) ? Colors.DGYellow : Color.White, Color.Black, base.depth + 2);
            Graphics.DrawRect(position + new Vec2(0f - (_fancyFont.GetWidth(unlockText) / 2f + 4f), -26f), position + new Vec2(_fancyFont.GetWidth(unlockText) / 2f + 4f, -14f), Color.Black, base.depth - 4);
            _fancyFont.scale = new Vec2(0.5f, 0.5f);
            if (_insertCoin > 0.01f)
            {
                _duckCoin.frame = (_rare ? 1 : 0);
                _duckCoin.depth = -0.8f;
                Graphics.Draw(_duckCoin, base.x + 40f, base.y - 100f + _insertCoin * 65f);
            }
            string descriptionText = _contains.description;
            int num3 = Profiles.experienceProfile.GetNumFurnitures(_contains.index) - 1;
            if (num3 > 0)
            {
                descriptionText = "I've already got " + ((num3 - 1 >= numberNames.Count) ? (num3 - 1).ToString() : numberNames[num3 - 1]) + " of these...";
            }
            Vec2 descFontPos = new Vec2(0f - _fancyFont.GetWidth(descriptionText) / 2f, 38f);
            _fancyFont.DrawOutline(descriptionText, position + descFontPos, (num3 > 0) ? Colors.DGYellow : Colors.DGGreen, Color.Black, base.depth + 2, 0.5f);
            Graphics.DrawRect(position + new Vec2(0f - (_fancyFont.GetWidth(descriptionText) / 2f + 4f), 37f), position + new Vec2(_fancyFont.GetWidth(descriptionText) / 2f + 4f, 44f), Color.Black, base.depth - 4);
            Graphics.DrawRect(new Vec2(-100f, -100f), new Vec2(2000f, 2000f), Color.Black * 0.6f, base.depth - 100);
        }
        base.y -= yOffset;
    }
}
