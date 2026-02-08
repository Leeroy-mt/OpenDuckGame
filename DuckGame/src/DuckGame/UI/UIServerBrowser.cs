using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;

namespace DuckGame;

public class UIServerBrowser : UIMenu
{
    public enum SearchMode
    {
        None,
        Near,
        Global,
        LAN
    }

    public class LobbyData : IComparable<LobbyData>
    {
        #region Public Fields

        public bool isGlobalLobby;
        public bool hasCustomName;
        public bool hasLocalMods;
        public bool hasFirstMod;
        public bool hasRestOfMods;
        public bool downloadedWorkshopItems;
        public bool hasPassword;
        public bool hasFriends;
        public bool dedicated;

        public int estimatedPing = -1;
        public int _userCount;
        public int maxPlayers;
        public int numSlots;
        public int pingRefreshTimeout;

        public long datahash;

        public string pingstring = "";
        public string name;
        public string password;
        public string lanAddress;
        public string restsEvery;
        public string requiredWins;
        public string wallMode;
        public string customLevels;
        public string version;
        public string started;
        public string type;
        public string hasModifiers;
        public string modHash;

        public Lobby lobby;

        public List<WorkshopItem> workshopItems = [];

        #endregion

        #region Public Properties

        public bool canJoin
        {
            get
            {
                if (DG.version == version && (Network.gameDataHash == datahash || ModLoader.modHash != modHash) && started == "false" && (customLevels == "" || customLevels == "0" || !ParentalControls.AreParentalControlsActive()) && (!hasLocalMods || ModLoader.modHash == modHash) && userCount < numSlots)
                {
                    if (type != "2")
                        return lobby == null;
                    return true;
                }
                return false;
            }
        }

        public int userCount
        {
            get
            {
                if (lobby != null)
                    return lobby.users.Count;
                return _userCount;
            }
        }

        #endregion

        #region Public Methods

        public int CompareTo(LobbyData other)
        {
            if (isGlobalLobby && !other.isGlobalLobby)
                return 10000;
            if (!isGlobalLobby && other.isGlobalLobby)
                return -10000;
            if (canJoin && !other.canJoin)
                return -500;
            if (!canJoin && other.canJoin)
                return 500;
            if (hasFriends && !other.hasFriends)
                return -100;
            if (!hasFriends && other.hasFriends)
                return 100;
            if (!hasPassword && other.hasPassword)
                return -10;
            if (hasPassword && !other.hasPassword)
                return 10;
            if (lobby == null && other.lobby != null)
                return -1000;
            if (lobby != null && other.lobby == null)
                return 1000;
            if (dedicated && !other.dedicated)
                return 1;
            if (!dedicated && other.dedicated)
                return -1;
            if (estimatedPing < other.estimatedPing)
                return -2;
            if (estimatedPing > other.estimatedPing)
                return 2;

            return name.CompareTo(other.name);
        }

        #endregion
    }

    #region Public Fields

    public static bool _doLanSearch;

    public static LobbyData _selectedLobby;

    public int lanSearchPort = 1337;

    public string enteredPassword = "";

    public string enteredPort = "";

    public UIStringEntryMenu _passwordEntryMenu;

    public UIStringEntryMenu _portEntryMenu;

    public UIMenu _editModMenu;

    public UIMenu _yesNoMenu;

    public UIMatchmakerMark2 _attemptConnection;

    #endregion

    #region Private Fields

    static LobbyData _joiningLobby;

    static Dictionary<ulong, Tex2D> _previewMap = [];

    static Dictionary<object, ulong> _clientMap = [];

    bool _gamepadMode = true;

    bool fixView = true;

    bool _enteringPort;

    bool _showingMenu;

    bool _searching;

    bool _draggingScrollbar;

    int _hoverIndex;

    int _maxLobbiesToShow;

    int _pressWait;

    int scrollBarTop;

    int scrollBarBottom;

    int scrollBarScrollableHeight;

    int scrollBarOffset;

    int _scrollItemOffset;

    float _refreshingDots;

    long _lobbySearchCooldownNextAvailable;

    Vector2 _oldPos;

    Queue<SearchMode> _modeQueue = new();

    UIMenu _openOnClose;

    Sprite _moreArrow;

    Sprite _noImage;

    Sprite _steamIcon;

    Sprite _lanIcon;

    Sprite _lockedServer;

    Sprite _namedServer;

    Sprite _globeIcon;

    SpriteMap _cursor;

    SpriteMap _localIcon;

    SpriteMap _newIcon;

    UIBox _box;

    FancyBitmapFont _fancyFont;

    UIMenuItem _yesNoYes;

    UIMenuItem _yesNoNo;

    Tex2D defaultImage;

    Tex2D defaultImageLan;

    UIMenu _downloadModsMenu;

    LobbyData _passwordLobby;

    List<LobbyData> _lobbies = [];

    #endregion

    #region Public Properties

    public new SearchMode mode
    {
        get
        {
            if (_modeQueue.Count > 0)
                return _modeQueue.Peek();
            return SearchMode.None;
        }
    }

    #endregion

    #region Public Constructors

    public UIServerBrowser(UIMenu openOnClose, string title, float xpos, float ypos, float wide = -1f, float high = -1f, InputProfile conProfile = null)
        : base(title, xpos, ypos, wide, high, "@WASD@@SELECT@JOIN @MENU1@REFRESH @CANCEL@BACK", conProfile)
    {
        defaultImage = Content.Load<Tex2D>("server_default");
        defaultImageLan = Content.Load<Tex2D>("server_default_lan");
        _splitter.topSection.components[0].align = UIAlign.Left;
        _openOnClose = openOnClose;
        _moreArrow = new Sprite("moreArrow");
        _moreArrow.CenterOrigin();
        _steamIcon = new Sprite("steamIconSmall")
        {
            Scale = new Vector2(.5f)
        };
        _lanIcon = new Sprite("lanIconSmall")
        {
            Scale = new Vector2(.5f)
        };
        _lockedServer = new Sprite("lockedServer");
        _globeIcon = new Sprite("smallEarth");
        _namedServer = new Sprite("namedServer");
        _localIcon = new SpriteMap("iconSheet", 16, 16)
        {
            Scale = new Vector2(.5f)
        };
        _localIcon.SetFrameWithoutReset(1);
        _newIcon = new SpriteMap("presents", 16, 16)
        {
            Scale = new Vector2(2)
        };
        _newIcon.SetFrameWithoutReset(0);
        _noImage = new Sprite("notexture")
        {
            Scale = new Vector2(2)
        };
        _cursor = new SpriteMap("cursors", 16, 16);
        _maxLobbiesToShow = 8;
        _box = new UIBox(0, 0, -1, _maxLobbiesToShow * 36, vert: true, isVisible: false);
        Add(_box);
        _fancyFont = new FancyBitmapFont("smallFont")
        {
            maxWidth = (int)width - 100,
            maxRows = 2
        };
        scrollBarOffset = 0;
        _editModMenu = new UIMenu("<mod name>", Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 160, -1, "@SELECT@SELECT");
        _editModMenu.Add(new UIText(" ", Color.White));
        _editModMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenu(_editModMenu, this)));
        _editModMenu.Close();
        _yesNoMenu = new UIMenu("ARE YOU SURE?", Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 160, -1, "@SELECT@SELECT");
        _yesNoMenu.Add(_yesNoYes = new UIMenuItem("YES"));
        _yesNoMenu.Add(_yesNoNo = new UIMenuItem("NO"));
        _yesNoMenu.Close();
        _downloadModsMenu = new UIMenu("MODS REQUIRED!", Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 290, -1, "@SELECT@SELECT");
        _downloadModsMenu.Add(new UIText("You're missing the mods required", Colors.DGBlue));
        _downloadModsMenu.Add(new UIText("to join this game. Would you", Colors.DGBlue));
        _downloadModsMenu.Add(new UIText("like to automatically subscribe to", Colors.DGBlue));
        _downloadModsMenu.Add(new UIText("all required mods, restart and", Colors.DGBlue));
        _downloadModsMenu.Add(new UIText("join the game?", Colors.DGBlue));
        _downloadModsMenu.Add(new UIText("", Colors.DGBlue));
        _downloadModsMenu.Add(new UIMenuItem("NO!", new UIMenuActionOpenMenu(_downloadModsMenu, this)));
        _downloadModsMenu.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuCallFunction(_downloadModsMenu, SubscribeAndRestart)));
        _downloadModsMenu.Close();
        if (!Network.available)
            _controlString = "@WASD@@SELECT@JOIN @MENU1@REFRESH @CANCEL@BACK";
        else
            _controlString = "@WASD@@SELECT@JOIN @MENU1@REFRESH @CANCEL@BACK @MENU2@REFRESH LAN";
        _passwordEntryMenu = new UIStringEntryMenu(directional: false, "ENTER PASSWORD", new FieldBinding(this, "enteredPassword"));
        _portEntryMenu = new UIStringEntryMenu(directional: false, "ENTER PORT", new FieldBinding(this, "enteredPort"), 6, pNumeric: true, 1337, 55535);
        _portEntryMenu.SetBackFunction(new UIMenuActionOpenMenu(_portEntryMenu, this));
        _portEntryMenu.Close();
        _passwordEntryMenu.SetBackFunction(new UIMenuActionOpenMenu(_passwordEntryMenu, this));
        _passwordEntryMenu.Close();
    }

    #endregion

    #region Public Methods

    public static void SubscribeAndRestart()
    {
        foreach (Mod allMod in ModLoader.allMods)
        {
            allMod.configuration.disabled = true;
        }
        if (ConnectionError.joinLobby != null)
        {
            _joiningLobby = new LobbyData
            {
                lobby = ConnectionError.joinLobby
            };
            string loadedMods = ConnectionError.joinLobby.GetLobbyData("mods");
            if (loadedMods != null && loadedMods != "")
            {
                string[] array = loadedMods.Split('|');
                foreach (string s in array)
                {
                    if (s == null || s.Contains("LOCAL"))
                        continue;
                    string[] s2 = s.Split(',');
                    if (s2.Length == 2)
                    {
                        string workshopID = s2[0].Trim();
                        try
                        {
                            WorkshopItem w = WorkshopItem.GetItem(Convert.ToUInt64(workshopID));
                            _joiningLobby.workshopItems.Add(w);
                        }
                        catch (Exception)
                        {
                            DevConsole.Log(DCSection.General, $"SubscribeAndRestart failed to enable workshop item ({s})");
                        }
                    }
                }
            }
        }
        foreach (WorkshopItem w2 in _joiningLobby.workshopItems)
        {
            Mod m = ModLoader.allMods.FirstOrDefault((Mod x) => x.configuration.workshopID == w2.id);
            m?.configuration.disabled = false;
            Steam.WorkshopSubscribe(w2.id);
        }
        Program.commandLine = $"{Program.commandLine} -downloadmods +connect_lobby {_joiningLobby.lobby.id}";
        if (MonoMain.lobbyPassword != "")
            Program.commandLine = $"{Program.commandLine} +password {MonoMain.lobbyPassword}";
        ModLoader.DisabledModsChanged();
        ModLoader.RestartGame();
    }

    public static string PreviewPathForWorkshopItem(ulong id)
    {
        return $"{DuckFile.workshopDirectory}/modPreview{id}preview.png";
    }

    public override void Close()
    {
        if (!fixView)
        {
            _showingMenu = false;
            _editModMenu.Close();
            Layer.HUD.camera.width /= 2;
            Layer.HUD.camera.height /= 2;
            fixView = true;
            DevConsole.RestoreDevConsole();
        }
        base.Close();
    }

    public override void Open()
    {
        _selectedLobby = null;
        _pressWait = 30;
        base.Open();
        DevConsole.SuppressDevConsole();
        _oldPos = Mouse.positionScreen;
        _hoverIndex = -1;
        if (!_enteringPort)
            RefreshLobbySearch(SearchMode.Near, SearchMode.Global, SearchMode.LAN);
        _enteringPort = false;
    }

    public override void Update()
    {
        if (open)
        {
            if (_passwordLobby != null && _passwordLobby.hasPassword && enteredPassword != "")
            {
                TryJoiningLobby(_passwordLobby);
                _passwordLobby = null;
                return;
            }
            UpdateLobbySearch();
        }
        if (_pressWait > 0)
            _pressWait--;
        if (_downloadModsMenu.open)
        {
            _downloadModsMenu.DoUpdate();
            if (!globalUILock && (Input.Pressed("CANCEL") || Keyboard.Pressed(Keys.Escape)))
            {
                _downloadModsMenu.Close();
                Open();
                return;
            }
        }
        else if (open)
        {
            MonoMain.lobbyPassword = null;
            if (_gamepadMode)
                _hoverIndex = int.Max(_hoverIndex, 0);
            else
            {
                _hoverIndex = -1;
                for (int i = 0; i < _maxLobbiesToShow && _scrollItemOffset + i < _lobbies.Count; i++)
                {
                    float boxLeft = _box.X - _box.halfWidth;
                    float boxTop = _box.Y - _box.halfHeight + (36 * i);
                    if (new Rectangle((int)boxLeft, (int)boxTop, (int)_box.width - 14, 36).Contains(Mouse.position))
                    {
                        _hoverIndex = _scrollItemOffset + i;
                        break;
                    }
                }
            }
            if (_hoverIndex != -1)
            {
                if (Input.Pressed("MENU1"))
                {
                    RefreshLobbySearch(SearchMode.Near, SearchMode.Global, SearchMode.LAN);
                    SFX.Play("rockHitGround", 0.8f);
                }
                else if (Input.Pressed("MENU2") || enteredPort != "")
                {
                    if (enteredPort == "")
                    {
                        _enteringPort = true;
                        _portEntryMenu.SetValue(lanSearchPort.ToString());
                        new UIMenuActionOpenMenu(this, _portEntryMenu).Activate();
                    }
                    else
                    {
                        try
                        {
                            lanSearchPort = Convert.ToInt32(enteredPort);
                        }
                        catch (Exception)
                        {
                        }
                        enteredPort = "";
                        RefreshLobbySearch(SearchMode.LAN);
                        SFX.Play("rockHitGround", 0.8f);
                    }
                }
                if (_lobbies.Count > 0 && _hoverIndex < _lobbies.Count)
                {
                    _selectedLobby = _lobbies[_hoverIndex];
                    if ((Input.Pressed("SELECT") && _pressWait == 0 && _gamepadMode) || (!_gamepadMode && Mouse.left == InputState.Pressed) || enteredPassword != "")
                    {
                        if (!_selectedLobby.canJoin)
                            SFX.Play("consoleError");
                        else
                        {
                            SFX.Play("consoleSelect");
                            if (!_selectedLobby.hasPassword || !(enteredPassword == ""))
                            {
                                TryJoiningLobby(_selectedLobby);
                                return;
                            }
                            _passwordLobby = _selectedLobby;
                            new UIMenuActionOpenMenu(this, _passwordEntryMenu).Activate();
                        }
                    }
                }
            }
            else
                _selectedLobby = null;
            if (_gamepadMode)
            {
                _draggingScrollbar = false;
                if (Input.Pressed("MENUDOWN"))
                    _hoverIndex++;
                else if (Input.Pressed("MENUUP"))
                    _hoverIndex--;
                if (Input.Pressed("STRAFE"))
                    _hoverIndex -= 10;
                else if (Input.Pressed("RAGDOLL"))
                    _hoverIndex += 10;
                if (_hoverIndex < 0)
                    _hoverIndex = 0;
                if ((_oldPos - Mouse.positionScreen).LengthSquared() > 200)
                    _gamepadMode = false;
            }
            else
            {
                if (!_draggingScrollbar)
                {
                    if (Mouse.left == InputState.Pressed && ScrollBarBox().Contains(Mouse.position))
                    {
                        _draggingScrollbar = true;
                        _oldPos = Mouse.position;
                    }
                    if (Mouse.scroll > 0)
                    {
                        _scrollItemOffset += 5;
                        _hoverIndex += 5;
                    }
                    else if (Mouse.scroll < 0)
                    {
                        _scrollItemOffset -= 5;
                        _hoverIndex -= 5;
                        if (_hoverIndex < 0)
                            _hoverIndex = 0;
                    }
                }
                else if (Mouse.left != InputState.Down)
                    _draggingScrollbar = false;
                else
                {
                    Vector2 delta = Mouse.position - _oldPos;
                    _oldPos = Mouse.position;
                    scrollBarOffset += (int)delta.Y;
                    if (scrollBarOffset > scrollBarScrollableHeight)
                        scrollBarOffset = scrollBarScrollableHeight;
                    else if (scrollBarOffset < 0)
                        scrollBarOffset = 0;
                    float heightScrolled = scrollBarOffset / (float)scrollBarScrollableHeight;
                    _scrollItemOffset = (int)((_lobbies.Count - _maxLobbiesToShow) * heightScrolled);
                }
                if (Input.Pressed("ANY"))
                {
                    _gamepadMode = true;
                    _oldPos = Mouse.positionScreen;
                }
            }
            if (_scrollItemOffset < 0)
                _scrollItemOffset = 0;
            else if (_scrollItemOffset > Math.Max(0, _lobbies.Count - _maxLobbiesToShow))
                _scrollItemOffset = Math.Max(0, _lobbies.Count - _maxLobbiesToShow);
            if (_hoverIndex >= _lobbies.Count)
                _hoverIndex = _lobbies.Count - 1;
            else if (_hoverIndex >= _scrollItemOffset + _maxLobbiesToShow)
                _scrollItemOffset += _hoverIndex - (_scrollItemOffset + _maxLobbiesToShow) + 1;
            else if (_hoverIndex >= 0 && _hoverIndex < _scrollItemOffset)
                _scrollItemOffset -= _scrollItemOffset - _hoverIndex;
            if (_scrollItemOffset != 0)
                scrollBarOffset = (int)Lerp.FloatSmooth(0f, scrollBarScrollableHeight, _scrollItemOffset / (float)(_lobbies.Count - _maxLobbiesToShow));
            else
                scrollBarOffset = 0;

            if (!Editor.hoverTextBox && !globalUILock && (Input.Pressed("CANCEL") || Keyboard.Pressed(Keys.Escape)))
            {
                new UIMenuActionOpenMenu(this, _openOnClose).Activate();
                return;
            }
        }
        if (_showingMenu)
        {
            HUD.CloseAllCorners();
            _showingMenu = false;
        }
        base.Update();
    }

    public override void Draw()
    {
        if (_downloadModsMenu.open)
            _downloadModsMenu.DoDraw();
        if (open)
        {
            scrollBarTop = (int)(_box.Y - _box.halfHeight + 1 + 16);
            scrollBarBottom = (int)(_box.Y + _box.halfHeight - 1 - 16);
            scrollBarScrollableHeight = scrollBarBottom - scrollBarTop;
            if (fixView)
            {
                Layer.HUD.camera.width *= 2;
                Layer.HUD.camera.height *= 2;
                fixView = false;
            }
            Graphics.DrawRect(new Vector2(_box.X - _box.halfWidth, _box.Y - _box.halfHeight), new Vector2(_box.X + _box.halfWidth - 12 - 2, _box.Y + _box.halfHeight), Color.Black, 0.4f);
            Graphics.DrawRect(new Vector2(_box.X + _box.halfWidth - 12, _box.Y - _box.halfHeight), new Vector2(_box.X + _box.halfWidth, _box.Y + _box.halfHeight), Color.Black, 0.4f);
            Rectangle sb = ScrollBarBox();
            Graphics.DrawRect(sb, (_draggingScrollbar || sb.Contains(Mouse.position)) ? Color.LightGray : Color.Gray, 0.5f);
            if (_lobbies.Count == 0)
            {
                float boxLeft = _box.X - _box.halfWidth;
                float boxTop = _box.Y - _box.halfHeight;
                if (mode == SearchMode.None)
                    _fancyFont.Draw("No games found!", new Vector2(boxLeft + 10, boxTop + 2), Color.Yellow, 0.5f);
                else
                    _fancyFont.Draw("Waiting for game list.", new Vector2(boxLeft + 10, boxTop + 2), Colors.DGGreen, 0.5f);
            }
            if (mode != SearchMode.None)
            {
                float boxLeft2 = _box.X - _box.halfWidth + 116;
                float boxTop2 = _splitter.topSection.Y - 5;
                _refreshingDots += 0.01f;
                if (_refreshingDots > 1)
                    _refreshingDots = 0;
                string refreshString = "(REFRESHING";
                for (int i = 0; i < 3; i++)
                    if (_refreshingDots * 4 > i + 1)
                        refreshString += ".";
                _fancyFont.Draw($"{refreshString})", new Vector2(boxLeft2, boxTop2), Colors.DGGreen, 0.5f);
            }
            _lobbies.Sort();
            for (int j = 0; j < _maxLobbiesToShow; j++)
            {
                int lobbyIndex = _scrollItemOffset + j;
                if (lobbyIndex >= _lobbies.Count)
                    break;
                float boxLeft3 = _box.X - _box.halfWidth;
                float boxTop3 = _box.Y - _box.halfHeight + (36 * j);
                if (_hoverIndex == lobbyIndex)
                    Graphics.DrawRect(new Vector2(boxLeft3, boxTop3), new Vector2(boxLeft3 + _box.width - 14, boxTop3 + 36), Color.White * 0.6f, 0.4f);
                else if ((lobbyIndex & 1) != 0)
                    Graphics.DrawRect(new Vector2(boxLeft3, boxTop3), new Vector2(boxLeft3 + _box.width - 14, boxTop3 + 36), Color.White * 0.1f, 0.4f);
                LobbyData lobby = _lobbies[lobbyIndex];
                if (lobby == null)
                    continue;
                _noImage.texture = defaultImage;
                if (lobby.lobby == null)
                    _noImage.texture = defaultImageLan;
                _noImage.Scale = Vector2.One;
                List<Tex2D> workshopTextures = [];
                string details = lobby.name;
                if (lobby.lobby == null)
                    details = !lobby.dedicated ? $"{details} (LAN)" : $"{details} |DGGREEN|(DEDICATED LAN SERVER)";
                else if (lobby.dedicated)
                    details += " |DGGREEN|(DEDICATED SERVER)";
                string modDetails = "|WHITE||GRAY|\n";
                if (lobby.workshopItems.Count > 0)
                {
                    WorkshopItem w = lobby.workshopItems[0];
                    if (w.data != null)
                    {
                        lobby.workshopItems = [.. lobby.workshopItems.OrderByDescending(x => x.data != null ? x.data.votesUp : 0)];
                        if (!lobby.downloadedWorkshopItems)
                        {
                            lobby.hasFirstMod = true;
                            lobby.hasRestOfMods = true;
                            bool firstIteration = true;
                            foreach (WorkshopItem item in lobby.workshopItems)
                            {
                                ulong id = item.id;
                                if (ModLoader.accessibleMods.FirstOrDefault(x => x.configuration.workshopID == id) == null)
                                {
                                    if (firstIteration)
                                        lobby.hasFirstMod = false;
                                    else
                                        lobby.hasRestOfMods = false;
                                }
                                firstIteration = false;
                            }
                            lobby.downloadedWorkshopItems = true;
                        }
                        modDetails = !lobby.hasFirstMod ? $"|RED|Requires {w.name}" : $"|DGGREEN|Requires {w.name}";
                        string col = lobby.hasRestOfMods ? "|DGGREEN|" : "|RED|";
                        if (lobby.workshopItems.Count == 2)
                            modDetails = $"{modDetails}{col} +{lobby.workshopItems.Count - 1} other mod.";
                        else if (lobby.workshopItems.Count > 2)
                            modDetails = $"{modDetails}{col} +{lobby.workshopItems.Count - 1} other mods.";
                        modDetails += "\n|GRAY|";
                        if (!_previewMap.TryGetValue(w.id, out Tex2D tex))
                        {
                            if (w.data.previewPath != null && w.data.previewPath != "")
                            {
                                try
                                {
                                    WebClient client = new();
                                    string file = PreviewPathForWorkshopItem(w.id);
                                    DuckFile.CreatePath(file);
                                    if (File.Exists(file))
                                        DuckFile.Delete(file);
                                    client.DownloadFileAsync(new Uri(w.data.previewPath), file);
                                    client.DownloadFileCompleted += Completed;
                                    _clientMap[client] = w.id;
                                }
                                catch (Exception)
                                {
                                }
                            }
                            _previewMap[w.id] = null;
                        }
                        else
                        {
                            if (tex != null)
                                workshopTextures.Add(tex);
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(lobby.requiredWins))
                    modDetails = $"{modDetails}First to {lobby.requiredWins} ";
                if (!string.IsNullOrWhiteSpace(lobby.restsEvery))
                    modDetails = $"{modDetails}rests every {lobby.restsEvery}. ";
                if (!string.IsNullOrWhiteSpace(lobby.wallMode) && lobby.wallMode != "0")
                    modDetails += "Wall Mode: ACTIVE. ";
                if (!string.IsNullOrWhiteSpace(lobby.customLevels) && lobby.customLevels != "0")
                    modDetails = $"{modDetails}Custom Levels: {lobby.customLevels}. ";
                if (!string.IsNullOrWhiteSpace(lobby.hasModifiers) && lobby.hasModifiers != "false")
                    modDetails += "Modifiers: ACTIVE.";
                Graphics.DrawRect(new Vector2(boxLeft3 + 2, boxTop3 + 2), new Vector2(boxLeft3 + 34, boxTop3 + 34), Color.Gray, 0.5f, filled: false, 2);
                if (workshopTextures.Count > 0)
                {
                    Vector2 drawOffset = Vector2.Zero;
                    for (int iTex = 0; iTex < 4; iTex++)
                    {
                        if (iTex >= workshopTextures.Count)
                            continue;
                        _noImage.texture = workshopTextures[iTex];
                        if (workshopTextures.Count > 1)
                            _noImage.Scale = new Vector2(16f / _noImage.texture.width);
                        else
                            _noImage.Scale = new Vector2(32f / _noImage.texture.width);
                        if (_noImage.texture.width != _noImage.texture.height)
                        {
                            if (_noImage.texture.width > _noImage.texture.height)
                            {
                                _noImage.Scale = new Vector2(32f / _noImage.texture.height);
                                Graphics.Draw(_noImage, boxLeft3 + 2 + drawOffset.X, boxTop3 + 2 + drawOffset.Y, new Rectangle(_noImage.texture.width / 2 - _noImage.texture.height / 2, 0, _noImage.texture.height, _noImage.texture.height), 0.5f);
                            }
                            else
                                Graphics.Draw(_noImage, boxLeft3 + 2 + drawOffset.X, boxTop3 + 2 + drawOffset.Y, new Rectangle(0, 0, _noImage.texture.width, _noImage.texture.width), 0.5f);
                        }
                        else
                            Graphics.Draw(_noImage, boxLeft3 + 2 + drawOffset.X, boxTop3 + 2 + drawOffset.Y, 0.5f);
                        drawOffset.X += 16;
                        if (drawOffset.X >= 32)
                        {
                            drawOffset.X = 0;
                            drawOffset.Y += 16;
                        }
                    }
                }
                else
                    Graphics.Draw(_noImage, boxLeft3 + 2, boxTop3 + 2, 0.5f);
                string titleString = details;
                titleString = $"{titleString} ({Math.Min(lobby.userCount - (lobby.dedicated ? 1 : 0), 8)}/{Math.Min(lobby.numSlots, 8)})";
                if (lobby.hasFriends)
                    titleString += " |DGGREEN|FRIEND";
                if (lobby.hasPassword)
                    titleString += " |DGRED|HAS PASSWORD";
                if (!lobby.canJoin)
                {
                    titleString += " |DGRED|(";
                    titleString = lobby.version != DG.version
                        ? DuckNetwork.CheckVersion(lobby.version) switch
                        {
                            NMVersionMismatch.Type.Older => $"{titleString}They have an older version.",
                            NMVersionMismatch.Type.Newer => $"{titleString}They have a newer version.",
                            _ => $"{titleString}They have a different version.",
                        } 
                        : (lobby.datahash != Network.gameDataHash
                           ? $"{titleString}Their version is incompatible."
                           : (lobby.started == "true"
                             ? $"{titleString}This game is in progress."
                             : (lobby.userCount >= lobby.numSlots
                               ? $"{titleString}Lobby is full."
                               : (lobby.lobby != null && lobby.type != "2"
                                 ? $"{titleString}This game is not public."
                                 : (lobby.hasLocalMods
                                   ? $"{titleString}This game is using non-workshop mods."
                                   : ((lobby.customLevels == "" || lobby.customLevels == "0" || !ParentalControls.AreParentalControlsActive())
                                     ? $"{titleString}Cannot join."
                                     : $"{titleString}This game has blocked content."))))));
                    titleString += ")";
                    Graphics.DrawRect(new Vector2(boxLeft3, boxTop3), new Vector2(boxLeft3 + _box.width - 14, boxTop3 + 36), Color.Black * 0.5f, 0.99f);
                }
                _fancyFont.maxWidth = 1000;
                float passOffset = 0;
                if (lobby.hasPassword)
                {
                    Graphics.Draw(_lockedServer, boxLeft3 + 46, boxTop3 + 2.5f, 0.5f);
                    passOffset += 10;
                }
                if (lobby.hasCustomName)
                {
                    Graphics.Draw(_namedServer, boxLeft3 + passOffset + 46, boxTop3 + 2.5f, 0.5f);
                    passOffset += 10;
                }
                if (lobby.isGlobalLobby)
                {
                    Graphics.Draw(_globeIcon, boxLeft3 + passOffset + 46, boxTop3 + 2.5f, 0.5f);
                    passOffset += 10;
                }
                _fancyFont.Draw(titleString, new Vector2(boxLeft3 + passOffset + 46, boxTop3 + 2), Color.Yellow, 0.5f);
                if (lobby.version == DG.version)
                    _fancyFont.Draw(lobby.version, new Vector2(boxLeft3 + 440, boxTop3 + 2), Colors.DGGreen * 0.45f, 0.5f);
                else
                    _fancyFont.Draw(lobby.version, new Vector2(boxLeft3 + 440, boxTop3 + 2), Colors.DGRed * 0.45f, 0.5f);
                _fancyFont.Draw("|WHITE|Ping:", new Vector2(boxLeft3 + 440, boxTop3 + 26), Color.White * 0.45f, 0.5f);
                if (lobby.pingRefreshTimeout <= 0)
                {
                    lobby.pingRefreshTimeout = 60;
                    lobby.estimatedPing = Steam.EstimatePing(lobby.pingstring);
                }
                lobby.pingRefreshTimeout--;
                if (lobby.estimatedPing != -1)
                {
                    Color c = Colors.DGGreen;
                    if (lobby.estimatedPing > 150)
                        c = Colors.DGYellow;
                    if (lobby.estimatedPing > 250)
                        c = Colors.DGRed;
                    _fancyFont.Draw($"{lobby.estimatedPing}ms", new Vector2(boxLeft3 + 470, boxTop3 + 26), c * 0.45f, 0.5f);
                }
                else
                    _fancyFont.Draw("????ms", new Vector2(boxLeft3 + 470, boxTop3 + 26), Colors.DGRed * 0.45f, 0.5f);
                if (lobby.lobby != null)
                    Graphics.Draw(_steamIcon, boxLeft3 + 36, boxTop3 + 2.5f, 0.5f);
                else
                    Graphics.Draw(_lanIcon, boxLeft3 + 36, boxTop3 + 2.5f, 0.5f);
                _fancyFont.Draw(modDetails, new Vector2(boxLeft3 + 36, boxTop3 + _fancyFont.characterHeight + 6), Color.LightGray, 0.5f);
            }
            if (Mouse.available && !_gamepadMode)
            {
                _cursor.Depth = 1;
                _cursor.Scale = Vector2.One;
                _cursor.Position = Mouse.position;
                _cursor.frame = 0;
                if (Editor.hoverTextBox)
                {
                    _cursor.frame = 5;
                    _cursor.Y -= 4;
                    _cursor.Scale = new Vector2(0.5f, 1);
                }
                _cursor.Draw();
            }
        }
        base.Draw();
    }

    public void RefreshLobbySearch(params SearchMode[] pParts)
    {
        _modeQueue.Clear();
        foreach (SearchMode mode in pParts)
            _modeQueue.Enqueue(mode);
        _lobbies.Clear();
        _selectedLobby = null;
    }

    #endregion

    #region Private Methods

    void TryJoiningLobby(LobbyData pLobby)
    {
        _joiningLobby = pLobby;
        if (ModLoader.modHash == _joiningLobby.modHash)
        {
            Close();
            _attemptConnection = UIMatchmakerMark2.Platform_GetMatchkmaker(pLobby, this);
            _attemptConnection.SetPasswordAttempt(enteredPassword);
            enteredPassword = "";
            Level.Add(_attemptConnection);
            _attemptConnection.Open();
            MonoMain.pauseMenu = _attemptConnection;
        }
        else
        {
            MonoMain.lobbyPassword = enteredPassword;
            new UIMenuActionOpenMenu(this, _downloadModsMenu).Activate();
            enteredPassword = "";
        }
    }

    void UpdateLobbySearch()
    {
        if (!_searching && mode != SearchMode.None && Graphics.frame >= _lobbySearchCooldownNextAvailable)
        {
            _lobbySearchCooldownNextAvailable = Graphics.frame;
            Network.lanMode = mode == SearchMode.LAN;
            NCBasic.lobbySearchPort = lanSearchPort;
            if (mode == SearchMode.Global)
                NCSteam.globalSearch = true;
            _selectedLobby = null;
            Network.activeNetwork.core.SearchForLobby();
            _searching = true;
        }
        if (!_searching || mode == SearchMode.None || !Network.activeNetwork.core.IsLobbySearchComplete())
            return;
        _searching = false;
        int lobbies = Network.activeNetwork.core.NumLobbiesFound();
        List<WorkshopItem> queryItems = [];
        if (Network.lanMode)
        {
            foreach (LobbyData d in (Network.activeNetwork.core as NCBasic)._foundLobbies)
                _lobbies.Add(d);
        }
        else
        {
            for (int i = 0; i < lobbies; i++)
            {
                Lobby lobby = Network.activeNetwork.core.GetSearchLobbyAtIndex(i);
                if (_lobbies.FirstOrDefault(x => x.lobby != null && x.lobby.id == lobby.id) != null)
                    continue;
                string lobbyName = lobby.GetLobbyData("name");
                if (string.IsNullOrEmpty(lobbyName))
                    continue;
                LobbyData d2 = new()
                {
                    lobby = lobby,
                    name = DuckNetwork.core.FilterText(lobbyName, null),
                    hasCustomName = lobby.GetLobbyData("customName") == "true",
                    modHash = lobby.GetLobbyData("modhash"),
                    requiredWins = lobby.GetLobbyData("requiredwins"),
                    restsEvery = lobby.GetLobbyData("restsevery"),
                    wallMode = lobby.GetLobbyData("wallmode"),
                    customLevels = lobby.GetLobbyData("customLevels"),
                    version = lobby.GetLobbyData("version"),
                    started = lobby.GetLobbyData("started"),
                    type = lobby.GetLobbyData("type")
                };
                try
                {
                    d2.numSlots = Convert.ToInt32(lobby.GetLobbyData("numSlots"));
                }
                catch (Exception)
                {
                    d2.numSlots = 0;
                }
                d2.hasModifiers = lobby.GetLobbyData("modifiers");
                d2.hasPassword = lobby.GetLobbyData("password") == "true";
                d2.dedicated = lobby.GetLobbyData("dedicated") == "true";
                d2.pingstring = lobby.GetLobbyData("pingstring");
                if (d2.pingstring != "" && d2.pingstring != null)
                    d2.estimatedPing = Steam.EstimatePing(d2.pingstring);
                try
                {
                    d2.datahash = Convert.ToInt64(lobby.GetLobbyData("datahash"));
                }
                catch (Exception)
                {
                }
                d2.isGlobalLobby = mode == SearchMode.Global;
                d2.hasFriends = false;
                foreach (User user in lobby.users)
                    if (Steam.friends.Contains(user))
                    {
                        d2.hasFriends = true;
                        break;
                    }
                string loadedMods = lobby.GetLobbyData("mods");
                if (loadedMods != null && loadedMods != "")
                {
                    string[] array = loadedMods.Split('|');
                    foreach (string s in array)
                    {
                        try
                        {
                            if (s == "")
                                continue;
                            if (s == "LOCAL")
                            {
                                d2.hasLocalMods = true;
                                continue;
                            }
                            string[] s2 = s.Split(',');
                            string workshopID = "";
                            workshopID = (s2.Length != 2) ? s : s2[0].Trim();
                            var w = WorkshopItem.GetItem(Convert.ToUInt64(workshopID));
                            if (w != null)
                            {
                                queryItems.Add(w);
                                d2.workshopItems.Add(w);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                try
                {
                    d2.maxPlayers = Convert.ToInt32(lobby.GetLobbyData("maxplayers"));
                }
                catch (Exception)
                {
                    d2.maxPlayers = 0;
                }
                _lobbies.Add(d2);
            }
        }
        if (queryItems.Count > 0)
            Steam.RequestWorkshopInfo(queryItems);
        _modeQueue.Dequeue();
    }

    void Completed(object sender, AsyncCompletedEventArgs e)
    {
        if (!_clientMap.TryGetValue(sender, out ulong id))
            return;

        _clientMap.Remove(sender);
        if (!_previewMap.ContainsKey(id))
            return;
        Texture2D texture = ContentPack.LoadTexture2D(PreviewPathForWorkshopItem(id), processPink: false);
        if (texture != null)
        {
            Tex2D tex = texture;
            if (tex != null)
                _previewMap[id] = tex;
        }
    }

    Rectangle ScrollBarBox()
    {
        return new(_box.X + _box.halfWidth - 12 + 1, _box.Y - _box.halfHeight + 1 + scrollBarOffset, 10, 32);
    }

    #endregion
}