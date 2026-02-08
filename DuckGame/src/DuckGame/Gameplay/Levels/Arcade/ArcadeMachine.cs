using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

[EditorGroup("Special|Arcade", EditorItemType.Arcade)]
[BaggedProperty("isOnlineCapable", false)]
public class ArcadeMachine : Thing
{
    public EditorProperty<string> name = new EditorProperty<string>("NAMELESS");

    public EditorProperty<bool> lit = new EditorProperty<bool>(val: true);

    public EditorProperty<int> style = new EditorProperty<int>(0, null, 0f, 16f, 1f);

    public EditorProperty<int> requirement = new EditorProperty<int>(0, null, 0f, 100f, 1f);

    public EditorProperty<float> respect = new EditorProperty<float>(0f, null, 0f, 1f, 0.05f);

    public EditorProperty<string> challenge01 = new EditorProperty<string>("", null, 0f, 1f, 0f, null, isTime: false, isLevel: true);

    public EditorProperty<string> challenge02 = new EditorProperty<string>("", null, 0f, 1f, 0f, null, isTime: false, isLevel: true);

    public EditorProperty<string> challenge03 = new EditorProperty<string>("", null, 0f, 1f, 0f, null, isTime: false, isLevel: true);

    protected bool _underlayStyle = true;

    protected SpriteMap _sprite;

    private Sprite _customMachineOverlay;

    private Sprite _customMachineOverlayMask;

    private Sprite _customMachineUnderlay;

    private Sprite _outline;

    private BitmapFont _font;

    protected int _styleOffsetX;

    protected int _styleOffsetY;

    protected int _screenOffsetX;

    protected int _screenOffsetY;

    private float _hoverFade;

    private ChallengeGroup _data;

    private SpriteMap _light;

    private Sprite _fixture;

    private DustSparkleEffect _dust;

    private bool _unlocked = true;

    private int _lightColor = 1;

    public bool flip;

    public bool hover;

    private SpriteMap _flash;

    private SpriteMap _flashLarge;

    private SpriteMap _flashWagnus;

    private Sprite _covered;

    private Sprite _boom;

    private Sprite _wagnus;

    private Sprite _wagnusOverlay;

    private Thing _lighting;

    public string machineStyle = "";

    private string _previousMachineStyle = "";

    private Sprite _machineStyleSprite;

    private int _previousStyleOffsetX;

    private int _previousStyleOffsetY;

    public ulong challenge01WorkshopID;

    public ulong challenge02WorkshopID;

    public ulong challenge03WorkshopID;

    public LevelData challenge01Data;

    public LevelData challenge02Data;

    public LevelData challenge03Data;

    public ChallengeGroup data => _data;

    public override bool visible
    {
        get
        {
            return base.visible;
        }
        set
        {
            base.visible = value;
            _dust.visible = base.visible;
        }
    }

    public bool unlocked
    {
        get
        {
            return _unlocked;
        }
        set
        {
            _unlocked = value;
        }
    }

    public int lightColor
    {
        get
        {
            return _lightColor;
        }
        set
        {
            _lightColor = value;
        }
    }

    public ArcadeMachine(float xpos, float ypos, ChallengeGroup c, int index)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("arcade/arcadeMachines", 29, 36);
        _sprite.frame = index;
        graphic = _sprite;
        base.Depth = -0.5f;
        _canHaveChance = false;
        _customMachineOverlay = new Sprite("arcade/customMachine");
        _outline = new Sprite("arcade/arcadeMachineOutline");
        _outline.Depth = base.Depth + 1;
        _outline.CenterOrigin();
        _customMachineOverlayMask = new Sprite("arcade/customOverlay");
        _boom = new Sprite("arcade/boommachine");
        _wagnus = new Sprite("arcade/wagnustrainer");
        _wagnusOverlay = new Sprite("arcade/wagnusOverlay");
        _font = new BitmapFont("biosFont", 8);
        Center = new Vector2(_sprite.width / 2, _sprite.h / 2);
        _data = c;
        _light = new SpriteMap("arcade/lights2", 56, 57);
        _fixture = new Sprite("arcade/fixture");
        _flash = new SpriteMap("arcade/monitorFlash", 11, 9);
        _flash.AddAnimation("idle", 0.1f, true, 0, 1, 2);
        _flash.SetAnimation("idle");
        _flashLarge = new SpriteMap("arcade/monitorFlashLarge", 13, 10);
        _flashLarge.AddAnimation("idle", 0.1f, true, 0, 1, 2);
        _flashLarge.SetAnimation("idle");
        _flashWagnus = new SpriteMap("arcade/monitorFlashWagnus", 15, 11);
        _flashWagnus.AddAnimation("idle", 0.1f, true, 0, 1, 2);
        _flashWagnus.SetAnimation("idle");
        _covered = new Sprite("arcade/coveredMachine");
        _collisionSize = new Vector2(28f, 34f);
        _collisionOffset = new Vector2(-14f, -17f);
        base.hugWalls = WallHug.Floor;
        respect._tooltip = "How much Chancy needs to like you before this machine unlocks.";
        requirement._tooltip = "How many challenges must be completed before this machine unlocks.";
        name._tooltip = "What's this collection of challenges called?";
    }

    public override void Initialize()
    {
        if (Level.current is Editor)
        {
            return;
        }
        _data = new ChallengeGroup();
        _data.name = name.value;
        _data.trophiesRequired = requirement.value;
        _data.challenges.Add(challenge01.value);
        _data.challenges.Add(challenge02.value);
        _data.challenges.Add(challenge03.value);
        if (base.level != null && !base.level.bareInitialize)
        {
            _dust = new DustSparkleEffect(base.X - 28f, base.Y - 40f, wide: false, lit);
            if ((bool)lit)
            {
                _lighting = new ArcadeLight(base.X - 1f, base.Y - 41f);
            }
            else
            {
                _lighting = new ArcadeScreen(base.X, base.Y);
            }
            if (Content.readyToRenderPreview)
            {
                _dust.Y -= 10f;
            }
            else
            {
                Level.Add(_lighting);
            }
            Level.Add(_dust);
            _dust.Depth = base.Depth - 2;
        }
    }

    public bool CheckUnlocked(bool ignoreAlreadyUnlocked = true)
    {
        if (_data == null || (ignoreAlreadyUnlocked && _unlocked))
        {
            return false;
        }
        if (_data.required.Count > 0)
        {
            foreach (string item in _data.required)
            {
                ChallengeData dat = Challenges.GetChallenge(item);
                if (dat != null)
                {
                    ChallengeSaveData save = Profiles.active[0].GetSaveData(dat.levelID, canBeNull: true);
                    if (save == null || save.trophy == TrophyType.Baseline)
                    {
                        return false;
                    }
                }
            }
        }
        if ((float)respect != 0f && Challenges.GetChallengeSkillIndex() < (float)respect)
        {
            return false;
        }
        if ((int)requirement > 0)
        {
            return Challenges.GetNumTrophies(Profiles.active[0]) >= (int)requirement;
        }
        return true;
    }

    public void UpdateStyle()
    {
        if (!(_previousMachineStyle != machineStyle) && _previousStyleOffsetX == _styleOffsetX && _previousStyleOffsetY == _styleOffsetY)
        {
            return;
        }
        _previousStyleOffsetX = _styleOffsetX;
        _previousStyleOffsetY = _styleOffsetY;
        if (machineStyle == null || machineStyle == "")
        {
            _machineStyleSprite = null;
            _customMachineUnderlay = null;
        }
        else
        {
            _machineStyleSprite = new Sprite(Editor.StringToTexture(machineStyle));
            if (Thing._alphaTestEffect == null)
            {
                Thing._alphaTestEffect = Content.Load<MTEffect>("Shaders/alphatest");
            }
            RenderTarget2D target = new RenderTarget2D(48, 48, pdepth: true);
            Camera cam = new Camera(0f, 0f, 48f, 48f);
            Graphics.SetRenderTarget(target);
            DepthStencilState state = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = 1,
                DepthBufferEnable = false
            };
            Graphics.Clear(new Color(0, 0, 0, 0));
            Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, state, RasterizerState.CullNone, Thing._alphaTestEffect, cam.getMatrix());
            Graphics.Draw(_machineStyleSprite, _styleOffsetX, _styleOffsetY, -0.9f);
            Graphics.Draw(_customMachineOverlayMask, 0f, 0f, 0.9f);
            Graphics.screen.End();
            Graphics.SetRenderTarget(null);
            Texture2D newTex = new Texture2D(Graphics.device, target.width, target.height);
            Color[] data = target.GetData();
            for (int i = 0; i < newTex.Width * newTex.Height; i++)
            {
                if (data[i].r == 250 && data[i].g == 0 && data[i].b == byte.MaxValue)
                {
                    data[i] = new Color(0, 0, 0, 0);
                }
            }
            newTex.SetData(data);
            _customMachineUnderlay = new Sprite(newTex);
        }
        _previousMachineStyle = machineStyle;
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk element = base.Serialize();
        element.AddProperty("machineStyle", machineStyle);
        element.AddProperty("arcadeMachineMode", Editor.arcadeMachineMode);
        if (Editor.arcadeMachineMode)
        {
            element.AddProperty("challenge01WorkshopID", challenge01WorkshopID);
            element.AddProperty("challenge02WorkshopID", challenge02WorkshopID);
            element.AddProperty("challenge03WorkshopID", challenge03WorkshopID);
        }
        return element;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        machineStyle = node.GetProperty<string>("machineStyle");
        if (machineStyle == null)
        {
            machineStyle = "";
        }
        if (node.GetProperty<bool>("arcadeMachineMode"))
        {
            challenge01WorkshopID = node.GetProperty<ulong>("challenge01WorkshopID");
            challenge02WorkshopID = node.GetProperty<ulong>("challenge02WorkshopID");
            challenge03WorkshopID = node.GetProperty<ulong>("challenge03WorkshopID");
            UpdateData();
        }
        return true;
    }

    public void UpdateData()
    {
        challenge01Data = Content.GetLevel(challenge01.value);
        challenge02Data = Content.GetLevel(challenge02.value);
        challenge03Data = Content.GetLevel(challenge03.value);
    }

    public override void EditorUpdate()
    {
        UpdateStyle();
        base.EditorUpdate();
    }

    public override void Update()
    {
        UpdateStyle();
        if (_unlocked)
        {
            Duck d = Level.Nearest<Duck>(base.X, base.Y);
            if (d != null)
            {
                if (d.grounded && (d.Position - Position).Length() < 20f)
                {
                    _hoverFade = Lerp.Float(_hoverFade, 1f, 0.1f);
                    hover = true;
                }
                else
                {
                    _hoverFade = Lerp.Float(_hoverFade, 0f, 0.1f);
                    hover = false;
                }
            }
        }
        _dust.fade = 0.7f;
        DustSparkleEffect dust = _dust;
        bool flag = (_lighting.visible = _unlocked && visible);
        dust.visible = flag;
    }

    public override ContextMenu GetContextMenu()
    {
        ContextMenu contextMenu = base.GetContextMenu();
        contextMenu.AddItem(new ContextFile("style", null, new FieldBinding(this, "machineStyle"), ContextFileType.ArcadeStyle, "Custom Arcade Machine Art (48 x 48 PNG located in My Documents/Duck Game/Custom/Arcade)"));
        return contextMenu;
    }

    public override void Draw()
    {
        if (Content.readyToRenderPreview)
        {
            base.Y -= 3f;
            for (int i = 0; i < 3; i++)
            {
                LevelData ld = null;
                ld = i switch
                {
                    0 => challenge01Data,
                    1 => challenge02Data,
                    _ => challenge03Data,
                };
                if (ld != null && ld.previewData.preview != null)
                {
                    Tex2D previewTex = Editor.StringToTexture(ld.previewData.preview);
                    Vector2 pos = new Vector2(base.X - 28f, base.Y + 30f - (float)previewTex.width / 8f - 6f);
                    switch (i)
                    {
                        case 1:
                            pos = new Vector2(base.X + 28f - (float)previewTex.width / 8f, base.Y + 30f - (float)previewTex.width / 8f - 6f);
                            break;
                        case 2:
                            pos = new Vector2(base.X - (float)previewTex.width / 8f / 2f, base.Y + 30f - (float)previewTex.width / 8f);
                            break;
                    }
                    Graphics.DrawRect(new Vector2(pos.X - 0.5f, pos.Y - 0.5f), new Vector2(pos.X + (float)previewTex.width / 8f + 0.5f, pos.Y + (float)previewTex.height / 8f + 0.5f), Color.White, (i == 2) ? 0.9f : 0.8f);
                    Graphics.Draw(previewTex, pos.X, pos.Y, 0.125f, 0.125f, (i == 2) ? 0.99f : 0.85f);
                }
            }
            base.Y -= 6f;
        }
        _sprite.frame = style.value;
        _light.Depth = base.Depth - 6;
        _flash.Depth = base.Depth + 1;
        if (_unlocked)
        {
            _light.frame = _lightColor;
            graphic.color = Color.White;
            if (style.value == 16)
            {
                _flashWagnus.Depth = base.Depth + 4;
                if (flipHorizontal)
                {
                    Graphics.Draw(_flashWagnus, base.X - 3f, base.Y - 8f);
                }
                else
                {
                    Graphics.Draw(_flashWagnus, base.X - 8f, base.Y - 9f);
                }
            }
            else if (style.value == 15)
            {
                if (flipHorizontal)
                {
                    Graphics.Draw(_flashLarge, base.X - 3f, base.Y - 8f);
                }
                else
                {
                    Graphics.Draw(_flashLarge, base.X - 7f, base.Y - 8f);
                }
            }
            else if (flipHorizontal)
            {
                Graphics.Draw(_flash, base.X - 3f + (float)_screenOffsetX, base.Y - 7f + (float)_screenOffsetY);
            }
            else
            {
                Graphics.Draw(_flash, base.X - 7f + (float)_screenOffsetX, base.Y - 7f + (float)_screenOffsetY);
            }
        }
        else
        {
            _light.frame = 0;
            graphic.color = Color.Black;
        }
        if ((bool)lit)
        {
            Graphics.Draw(_light, base.X - 28f, base.Y - 40f);
            _fixture.Depth = base.Depth - 1;
            Graphics.Draw(_fixture, base.X - 10f, base.Y - 65f);
        }
        _sprite.flipH = false;
        if (style.value == 15)
        {
            _boom.flipH = false;
            _boom.Depth = base.Depth;
            Graphics.Draw(_boom, base.X - 17f, base.Y - 36f);
        }
        else if (style.value == 16)
        {
            _wagnus.flipH = false;
            _wagnus.Depth = base.Depth;
            Graphics.Draw(_wagnus, base.X - 17f, base.Y - 20f);
            _wagnusOverlay.flipH = false;
            _wagnusOverlay.Depth = base.Depth + 10;
            Graphics.Draw(_wagnusOverlay, base.X - 17f, base.Y - 6f);
        }
        else if (_machineStyleSprite != null)
        {
            if (_underlayStyle)
            {
                _customMachineUnderlay.Center = new Vector2(23f, 30f);
                _customMachineUnderlay.Depth = base.Depth;
                Graphics.Draw(_customMachineUnderlay, base.X, base.Y);
            }
            else
            {
                _machineStyleSprite.Center = new Vector2(23f, 30f);
                _machineStyleSprite.Depth = base.Depth;
                Graphics.Draw(_machineStyleSprite, base.X, base.Y);
            }
        }
        else
        {
            base.Draw();
        }
        if (!_unlocked)
        {
            _covered.Depth = base.Depth + 2;
            if (flipHorizontal)
            {
                _covered.flipH = true;
                Graphics.Draw(_covered, base.X + 19f, base.Y - 19f);
            }
            else
            {
                Graphics.Draw(_covered, base.X - 18f, base.Y - 19f);
            }
        }
        if (_hoverFade > 0f)
        {
            _outline.Alpha = _hoverFade;
            _outline.flipH = flipHorizontal;
            if (flipHorizontal)
            {
                Graphics.Draw(_outline, base.X, base.Y);
            }
            else
            {
                Graphics.Draw(_outline, base.X + 1f, base.Y);
            }
            _ = _data.name;
            _font.Alpha = _hoverFade;
        }
    }
}
