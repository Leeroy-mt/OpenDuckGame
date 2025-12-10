namespace DuckGame;

public class UISlotEditor : UIMenu
{
    private UIMenu _closeMenu;

    private Rectangle _captureRectangle;

    private BitmapFont _littleFont;

    private BitmapFont _littleFont2;

    public static int _slot = 0;

    public static int _indexX;

    public static int _indexY;

    private Vec2 _rectPosition;

    public static bool editingSlots = false;

    public bool finished;

    private bool _selectionChanged = true;

    private bool _showWarning;

    private bool _showedWarning;

    public static int[,] kIndexMap = new int[3, 3]
    {
        { 0, 1, 4 },
        { 2, 3, 5 },
        { 6, -1, 7 }
    };

    public static int hoveringSlot = -1;

    public UISlotEditor(UIMenu closeMenu, float xpos, float ypos, float wide = -1f, float high = -1f)
        : base("", xpos, ypos, wide, high)
    {
        float captureSize = 38f;
        _captureRectangle = new Rectangle((int)(Layer.HUD.camera.width / 2f - captureSize / 2f), (int)(Layer.HUD.camera.height / 2f - captureSize / 2f), (int)captureSize, (int)captureSize);
        _closeMenu = closeMenu;
        _littleFont = new BitmapFont("smallBiosFontUI", 7, 5);
        _littleFont2 = new BitmapFont("smallBiosFont", 7, 6);
    }

    public override void Open()
    {
        HUD.CloseAllCorners();
        editingSlots = true;
        _showedWarning = false;
        _showWarning = false;
        HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@EXIT");
        MonoMain.doPauseFade = false;
        base.Open();
    }

    public override void Close()
    {
        HUD.CloseAllCorners();
        editingSlots = false;
        hoveringSlot = -1;
        MonoMain.doPauseFade = true;
        base.Close();
    }

    public override void Update()
    {
        if (base.open)
        {
            if (_showWarning)
            {
                _selectionChanged = true;
                if (Input.Pressed("CANCEL"))
                {
                    SFX.Play("consoleCancel");
                    _showWarning = false;
                }
                else if (Input.Pressed("MENU2"))
                {
                    SFX.Play("death");
                    _showedWarning = true;
                    _showWarning = false;
                    if (Level.core.gameInProgress)
                    {
                        DuckNetwork.ResetScores();
                    }
                }
            }
            else
            {
                int prevSlot = _slot;
                if (Input.Pressed("MENULEFT"))
                {
                    if (_indexX == 2 && _indexY == 2)
                    {
                        _indexX = 0;
                    }
                    else
                    {
                        _indexX--;
                        if (_indexX < 0)
                        {
                            _indexX = 0;
                        }
                    }
                }
                if (Input.Pressed("MENURIGHT"))
                {
                    if (_indexX == 0 && _indexY == 2)
                    {
                        _indexX = 2;
                    }
                    else
                    {
                        _indexX++;
                        if (_indexX > 2)
                        {
                            _indexX = 2;
                        }
                    }
                }
                if (Input.Pressed("MENUUP"))
                {
                    _indexY--;
                    if (_indexY < 0)
                    {
                        _indexY = 0;
                    }
                }
                if (Input.Pressed("MENUDOWN"))
                {
                    if (_indexX == 1 && _indexY == 1)
                    {
                        _indexY = 1;
                    }
                    else
                    {
                        _indexY++;
                        if (_indexY > 2)
                        {
                            _indexY = 2;
                        }
                    }
                }
                _slot = kIndexMap[_indexY, _indexX];
                hoveringSlot = _slot;
                if (_slot != prevSlot)
                {
                    _selectionChanged = true;
                }
                if (_slot >= 0)
                {
                    if (_selectionChanged)
                    {
                        if (DuckNetwork.profiles[_slot].connection != null && DuckNetwork.profiles[_slot] != DuckNetwork.hostProfile)
                        {
                            HUD.CloseCorner(HUDCorner.BottomMiddle);
                            if (DuckNetwork.profiles[_slot].connection == DuckNetwork.localConnection)
                            {
                                HUD.AddCornerControl(HUDCorner.BottomMiddle, "@MENU2@KICK");
                            }
                            else
                            {
                                HUD.AddCornerControl(HUDCorner.BottomMiddle, "@MENU2@KICK @RAGDOLL@BAN");
                            }
                            HUD.CloseCorner(HUDCorner.TopRight);
                            if (Network.canSetObservers)
                            {
                                HUD.AddCornerControl(HUDCorner.TopRight, "@MENU1@MAKE SPECTATOR");
                            }
                        }
                        else
                        {
                            HUD.CloseCorner(HUDCorner.BottomMiddle);
                            HUD.CloseCorner(HUDCorner.TopRight);
                        }
                        if (DuckNetwork.profiles[_slot].connection == null)
                        {
                            HUD.CloseCorner(HUDCorner.BottomRight);
                            HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@TOGGLE");
                        }
                        else
                        {
                            HUD.CloseCorner(HUDCorner.BottomRight);
                        }
                        _selectionChanged = false;
                    }
                    if (DuckNetwork.profiles[_slot].readyForSpectatorChange && Network.canSetObservers && Input.Pressed("MENU1") && DuckNetwork.profiles[_slot].connection != null)
                    {
                        _selectionChanged = true;
                        DuckNetwork.MakeSpectator(DuckNetwork.profiles[_slot]);
                        SFX.Play("menuBlip01");
                    }
                    else if (Input.Pressed("SELECT") && DuckNetwork.profiles[_slot].connection == null)
                    {
                        int slotType = (int)DuckNetwork.profiles[_slot].slotType;
                        slotType++;
                        if (DuckNetwork.profiles[_slot].reservedUser != null && slotType == 5)
                        {
                            slotType++;
                        }
                        if ((DuckNetwork.profiles[_slot].reservedUser == null && slotType >= 5) || (DuckNetwork.profiles[_slot].reservedUser != null && slotType > 6))
                        {
                            slotType = 0;
                        }
                        DuckNetwork.profiles[_slot].slotType = (SlotType)slotType;
                        DuckNetwork.ChangeSlotSettings();
                        SFX.Play("menuBlip01");
                    }
                    else if (Input.Pressed("MENU2"))
                    {
                        DuckNetwork.Kick(DuckNetwork.profiles[_slot]);
                    }
                    else if (Input.Pressed("RAGDOLL") && DuckNetwork.profiles[_slot].connection != DuckNetwork.localConnection)
                    {
                        DuckNetwork.Ban(DuckNetwork.profiles[_slot]);
                    }
                }
                if (Input.Pressed("CANCEL"))
                {
                    SFX.Play("consoleCancel");
                    new UIMenuActionOpenMenu(this, _closeMenu).Activate();
                }
            }
        }
        base.Update();
    }

    public override void Draw()
    {
    }
}
