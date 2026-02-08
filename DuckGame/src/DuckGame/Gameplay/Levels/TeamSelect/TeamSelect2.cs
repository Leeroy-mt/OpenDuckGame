using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class TeamSelect2 : Level, IHaveAVirtualTransition
{
    public static bool KillsForPoints = false;

    public static bool QUACK3;

    private float dim;

    public static bool fakeOnlineImmediately = false;

    public static int customLevels = 0;

    public static int prevCustomLevels = 0;

    public static int prevNumModifiers = 0;

    private BitmapFont _font;

    private SpriteMap _countdown;

    private float _countTime = 1.5f;

    public List<ProfileBox2> _profiles = new List<ProfileBox2>();

    private SpriteMap _buttons;

    private bool _matchSetup;

    private float _setupFade;

    private bool _starting;

    public static UIMenu openImmediately;

    private bool _returnedFromGame;

    public static bool userMapsOnly = false;

    public static bool enableRandom = false;

    public static bool randomMapsOnly = false;

    public static int randomMapPercent = 10;

    public static int normalMapPercent = 90;

    public static int workshopMapPercent = 0;

    public static bool partyMode = false;

    public static bool ctfMode = false;

    private static Dictionary<string, bool> _modifierStatus = new Dictionary<string, bool>();

    public static bool doCalc = false;

    public int setsPerGame = 3;

    private UIMenu _multiplayerMenu;

    private UIMenu _modifierMenu;

    public TeamBeam _beam;

    public TeamBeam _beam2;

    private Sprite _countdownScreen;

    private UIComponent _pauseGroup;

    private UIMenu _pauseMenu;

    private UIComponent _localPauseGroup;

    private UIMenu _localPauseMenu;

    private UIComponent _playOnlineGroup;

    private UIMenu _playOnlineMenu;

    private UIMenu _joinGameMenu;

    private UIMenu _playOnlineBumper;

    private UIMenu _filtersMenu;

    private UIMenu _filterModifierMenu;

    private UIMenu _hostGameMenu;

    private UIMenu _hostMatchSettingsMenu;

    private UIMenu _hostModifiersMenu;

    private UIMenu _hostLevelSelectMenu;

    private UIMenu _hostSettingsMenu;

    private UIMenu _hostSettingsWirelessGameMenu;

    private UIServerBrowser _browseGamesMenu;

    private UIMatchmakerMark2 _matchmaker;

    private MenuBoolean _returnToMenu = new MenuBoolean();

    private MenuBoolean _inviteFriends = new MenuBoolean();

    private MenuBoolean _findGame = new MenuBoolean();

    private MenuBoolean _backOut = new MenuBoolean();

    private MenuBoolean _localBackOut = new MenuBoolean();

    private MenuBoolean _createGame = new MenuBoolean();

    private MenuBoolean _hostGame = new MenuBoolean();

    public bool openLevelSelect;

    private LevelSelect _levelSelector;

    private UIMenu _inviteMenu;

    private static bool _hostGameEditedMatchSettings = false;

    private bool miniHostMenu;

    public static bool growCamera;

    private static bool eight = true;

    private UIComponent _configGroup;

    private UIMenu _levelSelectMenu;

    private BitmapFont _littleFont;

    private ProfileBox2 _pauseMenuProfile;

    private bool _singlePlayer;

    private int activePlayers;

    private static bool _attemptingToInvite = false;

    private static List<User> _invitedUsers = new List<User>();

    private static bool _didHost = false;

    private static bool _copyInviteLink = false;

    public static bool showEightPlayerSelected = false;

    private int fakeOnlineWait = 40;

    private bool _sentDedicatedCountdown;

    private bool explicitlyCreated;

    private Vector2 oldCameraPos = Vector2.Zero;

    private Vector2 oldCameraSize = Vector2.Zero;

    public static bool eightPlayersActive;

    public static bool zoomedOut;

    private float _waitToShow = 1f;

    private static bool _showedPS4Warning = false;

    private float _afkTimeout;

    private static bool _showedOnlineBumper = false;

    private float _timeoutFade;

    private float _topScroll;

    private float _afkMaxTimeout = 300f;

    private float _afkShowTimeout = 241f;

    private int _timeoutBeep;

    private bool _spectatorCountdownStop;

    public override string networkIdentifier => "@TEAMSELECT";

    public static List<MatchSetting> matchSettings => DuckNetwork.core.matchSettings;

    public static List<MatchSetting> onlineSettings => DuckNetwork.core.onlineSettings;

    public bool menuOpen
    {
        get
        {
            if (!_multiplayerMenu.open && !_modifierMenu.open)
            {
                return MonoMain.pauseMenu != null;
            }
            return true;
        }
    }

    public bool isInPlayOnlineMenu
    {
        get
        {
            if (_playOnlineGroup == null || _playOnlineGroup.open || (MonoMain.pauseMenu != null && MonoMain.pauseMenu.open && MonoMain.pauseMenu is UIGameConnectionBox))
            {
                return true;
            }
            return false;
        }
    }

    public List<Profile> defaultProfiles
    {
        get
        {
            List<Profile> defs = new List<Profile>();
            if (Network.isActive)
            {
                for (int i = 0; i < DG.MaxPlayers; i++)
                {
                    defs.Add(DuckNetwork.profiles[i]);
                }
            }
            else
            {
                for (int j = 0; j < DG.MaxPlayers; j++)
                {
                    defs.Add(Profile.defaultProfileMappings[j]);
                }
            }
            return defs;
        }
    }

    public ProfileBox2 GetBox(byte box)
    {
        return _profiles[box];
    }

    public static string DefaultGameName()
    {
        if (GetSettingInt("type") >= 3 && Profiles.active.Count > 0)
        {
            return Profiles.active[0].name + "'s LAN Game";
        }
        return Profiles.experienceProfile.name + "'s Game";
    }

    public static void DefaultSettings(bool resetMatchSettings = true)
    {
        if (resetMatchSettings)
        {
            foreach (MatchSetting matchSetting3 in matchSettings)
            {
                matchSetting3.value = matchSetting3.defaultValue;
            }
        }
        foreach (MatchSetting onlineSetting in onlineSettings)
        {
            onlineSetting.value = onlineSetting.defaultValue;
        }
        if (resetMatchSettings)
        {
            foreach (UnlockData unlock in Unlocks.GetUnlocks(UnlockType.Modifier))
            {
                unlock.enabled = false;
            }
            Editor.activatedLevels.Clear();
        }
        UpdateModifierStatus();
    }

    public static void DefaultSettingsHostWindow()
    {
        if (_hostGameEditedMatchSettings)
        {
            DefaultSettings();
        }
        _hostGameEditedMatchSettings = false;
    }

    public static MatchSetting GetMatchSetting(string id)
    {
        return matchSettings.FirstOrDefault((MatchSetting x) => x.id == id);
    }

    public static MatchSetting GetOnlineSetting(string id)
    {
        return onlineSettings.FirstOrDefault((MatchSetting x) => x.id == id);
    }

    public static int GetSettingInt(string id)
    {
        foreach (MatchSetting s in onlineSettings)
        {
            if (s.id == id && s.value is int)
            {
                return (int)s.value;
            }
        }
        foreach (MatchSetting s2 in matchSettings)
        {
            if (s2.id == id && s2.value is int)
            {
                return (int)s2.value;
            }
        }
        return -1;
    }

    public void ClearTeam(int index)
    {
        if (index >= 0 && index < DG.MaxPlayers && _profiles != null && _profiles[index]._hatSelector != null)
        {
            _profiles[index]._hatSelector._desiredTeamSelection = (sbyte)index;
            if (_profiles[index].duck != null)
            {
                _profiles[index].duck.profile.team = Teams.all[index];
            }
            _profiles[index]._hatSelector.ConfirmTeamSelection();
            HatSelector hatSelector = _profiles[index]._hatSelector;
            HatSelector hatSelector2 = _profiles[index]._hatSelector;
            sbyte num = (sbyte)index;
            short teamSelection = num;
            hatSelector2._desiredTeamSelection = num;
            hatSelector._teamSelection = teamSelection;
        }
    }

    public static bool GetSettingBool(string id)
    {
        foreach (MatchSetting s in onlineSettings)
        {
            if (s.id == id && s.value is bool)
            {
                return (bool)s.value;
            }
        }
        foreach (MatchSetting s2 in matchSettings)
        {
            if (s2.id == id && s2.value is bool)
            {
                return (bool)s2.value;
            }
        }
        return false;
    }

    public static int GetLANPort()
    {
        int port = 1337;
        try
        {
            return Convert.ToInt32((string)GetOnlineSetting("port").value);
        }
        catch (Exception)
        {
            return 1337;
        }
    }

    public TeamSelect2()
    {
        _centeredView = true;
        DuckNetwork.core.startCountdown = false;
    }

    public TeamSelect2(bool pReturningFromGame)
        : this()
    {
        _returnedFromGame = pReturningFromGame;
    }

    public void CloseAllDialogs()
    {
        if (_playOnlineGroup != null)
        {
            _playOnlineGroup.Close();
        }
        if (_playOnlineMenu != null)
        {
            _playOnlineMenu.Close();
        }
        if (_joinGameMenu != null)
        {
            _joinGameMenu.Close();
        }
        if (_filtersMenu != null)
        {
            _filtersMenu.Close();
        }
        if (_filterModifierMenu != null)
        {
            _filterModifierMenu.Close();
        }
        if (_hostGameMenu != null)
        {
            _hostGameMenu.Close();
        }
        if (_hostMatchSettingsMenu != null)
        {
            _hostMatchSettingsMenu.Close();
        }
        if (_hostModifiersMenu != null)
        {
            _hostModifiersMenu.Close();
        }
        if (_hostLevelSelectMenu != null)
        {
            _hostLevelSelectMenu.Close();
        }
        if (_matchmaker != null)
        {
            _matchmaker.Close();
        }
    }

    public static bool Enabled(string id, bool ignoreTeamSelect = false)
    {
        if (!ignoreTeamSelect && !Network.InGameLevel())
        {
            return false;
        }
        UnlockData dat = Unlocks.GetUnlock(id);
        if (dat != null && (!Network.isActive || dat.onlineEnabled))
        {
            bool unlocked = false;
            _modifierStatus.TryGetValue(id, out unlocked);
            return unlocked;
        }
        return false;
    }

    public static bool UpdateModifierStatus()
    {
        bool has = false;
        foreach (UnlockData dat in Unlocks.GetUnlocks(UnlockType.Modifier))
        {
            _modifierStatus[dat.id] = false;
            if (dat.enabled)
            {
                has = true;
                _modifierStatus[dat.id] = true;
            }
        }
        if (Network.isActive && Network.isServer && Network.activeNetwork.core.lobby != null)
        {
            Network.activeNetwork.core.lobby.SetLobbyData("modifiers", has ? "true" : "false");
            Network.activeNetwork.core.lobby.SetLobbyData("customLevels", Editor.customLevelCount.ToString());
        }
        return has;
    }

    public bool MatchmakerOpen()
    {
        if (UIMatchmakerMark2.instance != null)
        {
            return true;
        }
        if (MonoMain.pauseMenu != null && MonoMain.pauseMenu.open)
        {
            foreach (UIComponent c in MonoMain.pauseMenu.components)
            {
                if (c is UIMatchmakingBox && c.open)
                {
                    return true;
                }
                if (c is UIMatchmakerMark2 && c.open)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasBoxOpen(Profile pProfile)
    {
        foreach (ProfileBox2 box in _profiles)
        {
            if (box.profile == pProfile && box._hatSelector != null && box._hatSelector.open)
            {
                return true;
            }
        }
        return false;
    }

    public void OpenDoor(int index, Duck d)
    {
        _profiles[index].OpenDoor(d);
    }

    public void PrepareForOnline()
    {
        _hostGameEditedMatchSettings = false;
        if (!Network.isServer)
        {
            return;
        }
        GhostManager.context.SetGhostIndex(32);
        int index = 0;
        foreach (ProfileBox2 profile in _profiles)
        {
            profile.ChangeProfile(DuckNetwork.profiles[index]);
            index++;
        }
        foreach (Duck d in Level.current.things[typeof(Duck)])
        {
            if (d.ragdoll != null)
            {
                d.ragdoll.Unragdoll();
            }
        }
        base.things.RefreshState();
        foreach (Thing t in base.things)
        {
            t.DoNetworkInitialize();
            if (Network.isServer && t.isStateObject)
            {
                GhostManager.context.MakeGhost(t);
            }
        }
    }

    private void ShowEightPlayer()
    {
        showEightPlayerSelected = !showEightPlayerSelected;
    }

    public void BuildPauseMenu()
    {
        if (_pauseGroup != null)
        {
            Level.Remove(_pauseGroup);
        }
        _pauseGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        _pauseGroup.isPauseMenu = true;
        _pauseMenu = new UIMenu("@LWING@MULTIPLAYER@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 200f, -1f, "@CANCEL@CLOSE @SELECT@SELECT");
        _inviteMenu = new UIInviteMenu("INVITE FRIENDS", null, Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f);
        ((UIInviteMenu)_inviteMenu).SetAction(new UIMenuActionOpenMenu(_inviteMenu, _pauseMenu));
        UIDivider pauseBox = new UIDivider(vert: true, 0.8f);
        pauseBox.rightSection.Add(new UIImage("pauseIcons", UIAlign.Right));
        _pauseMenu.Add(pauseBox);
        pauseBox.leftSection.Add(new UIMenuItem("RESUME", new UIMenuActionCloseMenu(_pauseGroup)));
        pauseBox.leftSection.Add(new UIMenuItem("OPTIONS", new UIMenuActionOpenMenu(_pauseMenu, Options.optionsMenu)));
        pauseBox.leftSection.Add(new UIText("", Color.White));
        Options.openOnClose = _pauseMenu;
        Options.AddMenus(_pauseGroup);
        if (Network.isActive)
        {
            if (Network.isServer)
            {
                pauseBox.leftSection.Add(new UIMenuItem("END SESSION", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _backOut)));
            }
            else
            {
                pauseBox.leftSection.Add(new UIMenuItem("DISCONNECT", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _backOut)));
            }
        }
        else
        {
            if (_pauseMenuProfile.playerActive)
            {
                pauseBox.leftSection.Add(new UIMenuItem("BACK OUT", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _backOut)));
            }
            pauseBox.leftSection.Add(new UIMenuItem("|DGRED|MAIN MENU", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _returnToMenu)));
        }
        bool addedSpace = false;
        if (!eightPlayersActive)
        {
            addedSpace = true;
            pauseBox.leftSection.Add(new UIText("", Color.White));
            if (showEightPlayerSelected)
            {
                pauseBox.leftSection.Add(new UIMenuItem("|DGGREEN|HIDE 8 PLAYER", new UIMenuActionCloseMenuCallFunction(_pauseGroup, ShowEightPlayer)));
            }
            else
            {
                pauseBox.leftSection.Add(new UIMenuItem("|DGGREEN|SHOW 8 PLAYER", new UIMenuActionCloseMenuCallFunction(_pauseGroup, ShowEightPlayer)));
            }
        }
        if (Network.available && _pauseMenuProfile != null && _pauseMenuProfile.profile.steamID != 0L && _pauseMenuProfile.profile == Profiles.experienceProfile)
        {
            if (!addedSpace)
            {
                pauseBox.leftSection.Add(new UIText("", Color.White));
            }
            pauseBox.leftSection.Add(new UIMenuItem("|DGGREEN|INVITE FRIENDS", new UIMenuActionOpenMenu(_pauseMenu, _inviteMenu), UIAlign.Right));
            pauseBox.leftSection.Add(new UIMenuItem("|DGGREEN|COPY INVITE LINK", new UIMenuActionCloseMenuCallFunction(_pauseGroup, HostGameInviteLink), UIAlign.Left));
        }
        _pauseMenu.Close();
        _pauseGroup.Add(_pauseMenu, doAnchor: false);
        _inviteMenu.Close();
        _pauseGroup.Add(_inviteMenu, doAnchor: false);
        _inviteMenu.DoUpdate();
        _pauseGroup.Close();
        Level.Add(_pauseGroup);
        _pauseGroup.Update();
        _pauseGroup.Update();
        _pauseGroup.Update();
        if (_localPauseGroup != null)
        {
            Level.Remove(_localPauseGroup);
        }
        _localPauseGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        _localPauseMenu = new UIMenu("MULTIPLAYER", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f);
        pauseBox = new UIDivider(vert: true, 0.8f);
        pauseBox.rightSection.Add(new UIImage("pauseIcons", UIAlign.Right));
        _localPauseMenu.Add(pauseBox);
        pauseBox.leftSection.Add(new UIMenuItem("RESUME", new UIMenuActionCloseMenu(_localPauseGroup)));
        pauseBox.leftSection.Add(new UIMenuItem("BACK OUT", new UIMenuActionCloseMenuSetBoolean(_localPauseGroup, _localBackOut)));
        _localPauseMenu.Close();
        _localPauseGroup.Add(_localPauseMenu, doAnchor: false);
        _localPauseGroup.Close();
        Level.Add(_localPauseGroup);
        _localPauseGroup.Update();
        _localPauseGroup.Update();
        _localPauseGroup.Update();
    }

    public void ClearFilters()
    {
        foreach (MatchSetting matchSetting in matchSettings)
        {
            matchSetting.filtered = false;
        }
        foreach (UnlockData unlock in Unlocks.GetUnlocks(UnlockType.Modifier))
        {
            unlock.filtered = false;
            unlock.enabled = false;
        }
    }

    public void ClosedOnline()
    {
        foreach (Profile item in Profiles.all)
        {
            item.team = null;
        }
        int idx = 0;
        foreach (MatchmakingPlayer p in UIMatchmakingBox.core.matchmakingProfiles)
        {
            foreach (ProfileBox2 p2 in _profiles)
            {
                if (p.originallySelectedProfile == p2.profile)
                {
                    p2.profile.team = p.team;
                    p2.profile.inputProfile = p.inputProfile;
                }
            }
            idx++;
        }
        DefaultSettingsHostWindow();
    }

    public static List<byte> GetNetworkModifierList()
    {
        List<byte> values = new List<byte>();
        foreach (UnlockData dat in Unlocks.GetUnlocks(UnlockType.Modifier))
        {
            if (dat.unlocked && dat.enabled && Unlocks.modifierToByte.ContainsKey(dat.id))
            {
                values.Add(Unlocks.modifierToByte[dat.id]);
            }
        }
        return values;
    }

    public static string GetMatchSettingString()
    {
        string s = "";
        s = s + GetSettingInt("requiredwins") + GetSettingInt("restsevery") + GetSettingInt("randommaps") + GetSettingInt("workshopmaps") + GetSettingInt("normalmaps") + (bool)GetOnlineSetting("teams").value + GetSettingInt("custommaps") + Editor.activatedLevels.Count + GetSettingBool("wallmode") + GetSettingBool("clientlevelsenabled");
        foreach (byte networkModifier in GetNetworkModifierList())
        {
            s += networkModifier;
        }
        return s;
    }

    public static void SendMatchSettings(NetworkConnection c = null, bool initial = false)
    {
        UpdateModifierStatus();
        if (Network.isActive)
        {
            Send.Message(new NMMatchSettings(initial, (byte)GetSettingInt("requiredwins"), (byte)GetSettingInt("restsevery"), (byte)GetSettingInt("randommaps"), (byte)GetSettingInt("workshopmaps"), (byte)GetSettingInt("normalmaps"), (bool)GetOnlineSetting("teams").value, (byte)GetSettingInt("custommaps"), Editor.activatedLevels.Count, GetSettingBool("wallmode"), GetNetworkModifierList(), GetSettingBool("clientlevelsenabled")), c);
        }
    }

    public void OpenFindGameMenu()
    {
        OpenFindGameMenu(pThroughModWindow: true);
    }

    public void OpenFindGameMenu(bool pThroughModWindow)
    {
        _playOnlineGroup.Open();
        _playOnlineMenu.Open();
        MonoMain.pauseMenu = _playOnlineGroup;
        if (ModLoader.modHash != "nomods")
        {
            HUD.AddCornerMessage(HUDCorner.TopLeft, "@PLUG@|LIME|Mods enabled.");
        }
        new UIMenuActionOpenMenu(_playOnlineMenu, _joinGameMenu).Activate();
    }

    public void OpenCreateGameMenu()
    {
        OpenCreateGameMenu(pThroughModWindow: true);
    }

    public void OpenCreateGameMenu(bool pThroughModWindow)
    {
        _playOnlineGroup.Open();
        _playOnlineMenu.Open();
        MonoMain.pauseMenu = _playOnlineGroup;
        if (ModLoader.modHash != "nomods")
        {
            HUD.AddCornerMessage(HUDCorner.TopLeft, "@PLUG@|LIME|Mods enabled.");
        }
        new UIMenuActionOpenMenu(_playOnlineMenu, _hostGameMenu).Activate();
    }

    public void OpenNoModsFindGame()
    {
        DefaultSettings(resetMatchSettings: false);
        if (!Options.Data.showNetworkModWarning)
        {
            OpenFindGameMenu(pThroughModWindow: false);
        }
        else
        {
            DuckNetwork.OpenNoModsWindow(OpenFindGameMenu);
        }
    }

    public void OpenNoModsCreateGame()
    {
        if (!Options.Data.showNetworkModWarning)
        {
            OpenCreateGameMenu(pThroughModWindow: false);
        }
        else
        {
            DuckNetwork.OpenNoModsWindow(OpenCreateGameMenu);
        }
    }

    private void SetMatchSettingsOpenedFromHostGame()
    {
        _hostGameEditedMatchSettings = true;
        _hostMatchSettingsMenu.SetBackFunction(new UIMenuActionOpenMenu(_hostMatchSettingsMenu, _hostSettingsMenu));
    }

    private void BuildHostMatchSettingsMenu()
    {
        float wide = 320f;
        float high = 180f;
        _hostMatchSettingsMenu = new UIMenu("@LWING@MATCH SETTINGS@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        _hostLevelSelectMenu = new LevelSelectCompanionMenu(wide / 2f, high / 2f, _hostMatchSettingsMenu);
        _playOnlineGroup.Add(_hostLevelSelectMenu, doAnchor: false);
        _hostModifiersMenu = new UIMenu("MODIFIERS", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 240f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        foreach (UnlockData dat in Unlocks.GetUnlocks(UnlockType.Modifier))
        {
            if (dat.onlineEnabled)
            {
                if (dat.unlocked)
                {
                    _hostModifiersMenu.Add(new UIMenuItemToggle(dat.GetShortNameForDisplay(), null, new FieldBinding(dat, "enabled")));
                }
                else
                {
                    _hostModifiersMenu.Add(new UIMenuItem("@TINYLOCK@LOCKED", null, UIAlign.Center, Color.Red));
                }
            }
        }
        _hostModifiersMenu.SetBackFunction(new UIMenuActionOpenMenu(_hostModifiersMenu, _hostMatchSettingsMenu));
        _hostModifiersMenu.Close();
        _playOnlineGroup.Add(_hostModifiersMenu, doAnchor: false);
        _hostMatchSettingsMenu.AddMatchSetting(GetOnlineSetting("teams"), filterMenu: false);
        foreach (MatchSetting m in matchSettings)
        {
            if ((!(m.id == "workshopmaps") || Network.available) && (!(m.id == "custommaps") || !ParentalControls.AreParentalControlsActive()))
            {
                if (m.id != "partymode")
                {
                    _hostMatchSettingsMenu.AddMatchSetting(m, filterMenu: false);
                }
                if (m.id == "wallmode")
                {
                    _hostMatchSettingsMenu.Add(new UIText(" ", Color.White));
                }
            }
        }
        _hostMatchSettingsMenu.Add(new UIText(" ", Color.White));
        if (!ParentalControls.AreParentalControlsActive())
        {
            _hostMatchSettingsMenu.Add(new UICustomLevelMenu(new UIMenuActionOpenMenu(_hostMatchSettingsMenu, _hostLevelSelectMenu)));
        }
        _hostMatchSettingsMenu.Add(new UIModifierMenuItem(new UIMenuActionOpenMenu(_hostMatchSettingsMenu, _hostModifiersMenu)));
        _hostMatchSettingsMenu.SetBackFunction(new UIMenuActionOpenMenu(_hostMatchSettingsMenu, _hostSettingsMenu));
        _hostMatchSettingsMenu.Close();
        _playOnlineGroup.Add(_hostMatchSettingsMenu, doAnchor: false);
    }

    public void OpenHostGameMenuNonMini()
    {
        miniHostMenu = false;
        _hostGameMenu.SetBackFunction(new UIMenuActionOpenMenu(_hostGameMenu, _playOnlineMenu));
    }

    public static void ControllerLayoutsChanged()
    {
    }

    private void CreateGame()
    {
        if (!miniHostMenu)
        {
            _createGame.value = true;
        }
        else
        {
            _hostGame.value = true;
        }
    }

    public override void Initialize()
    {
        Program.gameLoadedSuccessfully = true;
        Vote.ClearVotes();
        ControllerLayoutsChanged();
        Global.data.bootedSinceUpdate++;
        Global.data.bootedSinceSwitchHatPatch++;
        Global.Save();
        if (!Network.isActive)
        {
            Profiles.SaveActiveProfiles();
        }
        if (!Network.isActive)
        {
            Level.core.gameInProgress = false;
        }
        DuckNetwork.inGame = false;
        if (!Level.core.gameInProgress)
        {
            Main.ResetMatchStuff();
            Main.ResetGameStuff();
            DuckNetwork.ClosePauseMenu();
        }
        if (_returnedFromGame)
        {
            ConnectionStatusUI.Hide();
            if (Network.isServer)
            {
                if (Network.isActive && Network.activeNetwork.core.lobby != null)
                {
                    Network.activeNetwork.core.lobby.SetLobbyData("started", "false");
                    Network.activeNetwork.core.lobby.joinable = true;
                }
                foreach (Profile p in DuckNetwork.profiles)
                {
                    if (p.connection == null && p.slotType != SlotType.Reserved && p.slotType != SlotType.Spectator && p.slotType != SlotType.Invite)
                    {
                        p.slotType = SlotType.Closed;
                    }
                }
            }
        }
        _littleFont = new BitmapFont("smallBiosFontUI", 7, 5);
        _countdownScreen = new Sprite("title/wideScreen");
        base.backgroundColor = Color.Black;
        if (Network.isActive && Network.isServer)
        {
            Network.ContextSwitch(0);
            DuckNetwork.ChangeSlotSettings();
            networkIndex = 0;
        }
        _countdown = new SpriteMap("countdown", 32, 32);
        _countdown.Center = new Vector2(16f, 16f);
        showEightPlayerSelected = false;
        List<Profile> defaults = defaultProfiles;
        ProfileBox2 b = new ProfileBox2(1f, 1f, InputProfile.Get(InputProfile.MPPlayer1), defaults[0], this, 0);
        _profiles.Add(b);
        Level.Add(b);
        b = new ProfileBox2(1f + 178f, 1f, InputProfile.Get(InputProfile.MPPlayer2), defaults[1], this, 1);
        _profiles.Add(b);
        Level.Add(b);
        b = new ProfileBox2(1f, 90f, InputProfile.Get(InputProfile.MPPlayer3), defaults[2], this, 2);
        _profiles.Add(b);
        Level.Add(b);
        b = new ProfileBox2(1f + 178f, 90f, InputProfile.Get(InputProfile.MPPlayer4), defaults[3], this, 3);
        _profiles.Add(b);
        Level.Add(b);
        growCamera = false;
        float yOff = 0f;
        b = new ProfileBox2(357f, yOff + 1f, InputProfile.Get(InputProfile.MPPlayer5), defaults[4], this, 4);
        _profiles.Add(b);
        Level.Add(b);
        b = new ProfileBox2(357f, yOff + 90f, InputProfile.Get(InputProfile.MPPlayer6), defaults[5], this, 5);
        _profiles.Add(b);
        Level.Add(b);
        yOff = 179f;
        b = new ProfileBox2(2f, yOff, InputProfile.Get(InputProfile.MPPlayer7), defaults[6], this, 6);
        _profiles.Add(b);
        Level.Add(b);
        b = new ProfileBox2(357f - 1f, yOff, InputProfile.Get(InputProfile.MPPlayer8), defaults[7], this, 7);
        _profiles.Add(b);
        Level.Add(b);
        Level.Add(new BlankDoor(178f, 179f));
        Level.Add(new HostTable(160f, 170f));
        if (Network.isActive)
        {
            PrepareForOnline();
        }
        _font = new BitmapFont("biosFont", 8);
        _font.Scale = new Vector2(1f, 1f);
        _buttons = new SpriteMap("buttons", 14, 14);
        _buttons.CenterOrigin();
        _buttons.Depth = 0.9f;
        Music.Play("CharacterSelect");
        _beam = new TeamBeam(160f, 0f);
        _beam2 = new TeamBeam(338f, 0f);
        Level.Add(_beam);
        Level.Add(_beam2);
        UpdateModifierStatus();
        _configGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        _multiplayerMenu = new UIMenu("@LWING@MATCH SETTINGS@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        _modifierMenu = new UIMenu("MODIFIERS", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 240f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        _modifierMenu.SetBackFunction(new UIMenuActionOpenMenu(_modifierMenu, _multiplayerMenu));
        _levelSelectMenu = new LevelSelectCompanionMenu(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, _multiplayerMenu);
        foreach (UnlockData dat in Unlocks.GetUnlocks(UnlockType.Modifier))
        {
            if (dat.unlocked)
            {
                _modifierMenu.Add(new UIMenuItemToggle(dat.GetShortNameForDisplay(), null, new FieldBinding(dat, "enabled")));
            }
            else
            {
                _modifierMenu.Add(new UIMenuItem("@TINYLOCK@LOCKED", null, UIAlign.Center, Color.Red));
            }
        }
        _modifierMenu.Close();
        foreach (MatchSetting m in matchSettings)
        {
            if (!(m.id == "clientlevelsenabled") && (!(m.id == "workshopmaps") || Network.available))
            {
                _multiplayerMenu.AddMatchSetting(m, filterMenu: false);
                if (m.id == "wallmode")
                {
                    _multiplayerMenu.Add(new UIText(" ", Color.White));
                }
            }
        }
        _multiplayerMenu.Add(new UIText(" ", Color.White));
        _multiplayerMenu.Add(new UICustomLevelMenu(new UIMenuActionOpenMenu(_multiplayerMenu, _levelSelectMenu)));
        _multiplayerMenu.Add(new UIModifierMenuItem(new UIMenuActionOpenMenu(_multiplayerMenu, _modifierMenu)));
        _multiplayerMenu.Close();
        _configGroup.Add(_multiplayerMenu, doAnchor: false);
        _configGroup.Add(_modifierMenu, doAnchor: false);
        _configGroup.Add(_levelSelectMenu, doAnchor: false);
        _configGroup.Close();
        Level.Add(_configGroup);
        _playOnlineGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        string playOnlineMenuTitle = "@PLANET@PLAY ONLINE@PLANET@";
        _playOnlineMenu = new UIMenu(playOnlineMenuTitle, Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        _hostGameMenu = new UIMenu("@LWING@CREATE GAME@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        _hostSettingsMenu = new UIMenu("@LWING@HOST SETTINGS@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        int padLength = 50;
        float heightAdd = 3f;
        _playOnlineBumper = new UIMenu("PLAYING ONLINE", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 220f, -1f, "@SELECT@OK!");
        _playOnlineBumper.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("There are many tools of expression", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("in Duck Game. Please use them for", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("|PINK|love|WHITE| and not for |DGRED|hate...|WHITE|", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("Things every Duck aught to remember:", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("-Trolling and hate appear exactly the same online.".Padded(padLength), Colors.DGBlue, UIAlign.Center, 0f - heightAdd)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("-Please! be kind to one another.".Padded(padLength), Colors.DGBlue, UIAlign.Center, 0f - heightAdd)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("-Please! don't use hate speech or strong words.".Padded(padLength), Colors.DGBlue, UIAlign.Center, 0f - heightAdd)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("-Please! don't use hacks in public lobbies.".Padded(padLength), Colors.DGBlue, UIAlign.Center, 0f - heightAdd)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("-Please! keep custom content tasteful.".Padded(padLength), Colors.DGBlue, UIAlign.Center, 0f - heightAdd)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("-Angle shots are neat (and are not hacks).".Padded(padLength), Colors.DGBlue, UIAlign.Center, 0f - heightAdd)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("If anyone is hacking or being unkind, please", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("hover their name in the pause menu", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("and go 'Mute -> Block'.", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            Scale = new Vector2(0.5f)
        });
        _playOnlineBumper.SetAcceptFunction(new UIMenuActionOpenMenu(_playOnlineBumper, _playOnlineMenu));
        _playOnlineBumper.SetBackFunction(new UIMenuActionOpenMenu(_playOnlineBumper, _playOnlineMenu));
        _browseGamesMenu = new UIServerBrowser(_playOnlineMenu, "SERVER BROWSER", Layer.HUD.camera.width, Layer.HUD.camera.height, 550f);
        if (Network.available)
        {
            _joinGameMenu = new UIMenu("@LWING@FIND GAME@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, "@CANCEL@BACK @SELECT@SELECT");
            _filtersMenu = new UIMenu("@LWING@FILTERS@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, "@SELECT@SELECT  @MENU2@TYPE");
            _filterModifierMenu = new UIMenu("@LWING@FILTER MODIFIERS@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 240f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        }
        if (Network.available)
        {
            _matchmaker = UIMatchmakerMark2.Platform_GetMatchkmaker(null, _joinGameMenu);
        }
        if (ModLoader.modHash != "nomods")
        {
            if (Network.available)
            {
                _playOnlineMenu.Add(new UIMenuItem("FIND GAME", new UIMenuActionCloseMenuCallFunction(_playOnlineMenu, OpenNoModsFindGame)));
            }
            _playOnlineMenu.Add(new UIMenuItem("CREATE GAME", new UIMenuActionCloseMenuCallFunction(_playOnlineMenu, OpenNoModsCreateGame)));
        }
        else
        {
            if (Network.available)
            {
                _playOnlineMenu.Add(new UIMenuItem("FIND GAME", new UIMenuActionOpenMenu(_playOnlineMenu, _joinGameMenu)));
            }
            _playOnlineMenu.Add(new UIMenuItem("CREATE GAME", new UIMenuActionOpenMenuCallFunction(_playOnlineMenu, _hostGameMenu, OpenHostGameMenuNonMini)));
        }
        _playOnlineMenu.Add(new UIMenuItem("BROWSE GAMES", new UIMenuActionOpenMenu(_playOnlineMenu, _browseGamesMenu)));
        _playOnlineMenu.SetBackFunction(new UIMenuActionCloseMenuCallFunction(_playOnlineGroup, ClosedOnline));
        _playOnlineMenu.Close();
        _playOnlineGroup.Add(_playOnlineMenu, doAnchor: false);
        _playOnlineBumper.Close();
        _playOnlineGroup.Add(_playOnlineBumper, doAnchor: false);
        string lastID = "";
        bool addToSettingsMenu = false;
        foreach (MatchSetting m2 in onlineSettings)
        {
            if (m2.filterOnly)
            {
                continue;
            }
            if (m2.id == "customlevelsenabled" && ParentalControls.AreParentalControlsActive())
            {
                lastID = m2.id;
                continue;
            }
            if (m2.id == "type" && !Network.available)
            {
                lastID = m2.id;
                continue;
            }
            if (lastID == "type")
            {
                addToSettingsMenu = true;
            }
            UIComponent c = null;
            if (addToSettingsMenu)
            {
                c = _hostSettingsMenu.AddMatchSetting(m2, filterMenu: false);
                if (c != null && c is UIMenuItemString && c is UIMenuItemString p2)
                {
                    p2.InitializeEntryMenu(_playOnlineGroup, _hostSettingsMenu);
                }
            }
            else
            {
                c = _hostGameMenu.AddMatchSetting(m2, filterMenu: false);
                if (c != null && c is UIMenuItemString && c is UIMenuItemString p3)
                {
                    p3.InitializeEntryMenu(_playOnlineGroup, _hostGameMenu);
                }
            }
            lastID = m2.id;
        }
        _hostGameMenu.Add(new UIText(" ", Color.White));
        BuildHostMatchSettingsMenu();
        _hostGameMenu.Add(new UIMenuItem("|DGBLUE|SETTINGS", new UIMenuActionOpenMenu(_hostGameMenu, _hostSettingsMenu)));
        _hostSettingsMenu.Add(new UIMenuItem("|DGBLUE|MATCH SETTINGS", new UIMenuActionOpenMenuCallFunction(_hostSettingsMenu, _hostMatchSettingsMenu, SetMatchSettingsOpenedFromHostGame)));
        _hostSettingsMenu.SetBackFunction(new UIMenuActionOpenMenu(_hostSettingsMenu, _hostGameMenu));
        _hostGameMenu.Add(new UIMenuItem("|DGGREEN|CREATE GAME", new UIMenuActionCloseMenuCallFunction(_playOnlineGroup, CreateGame)));
        _hostGameMenu.SetBackFunction(new UIMenuActionOpenMenu(_hostGameMenu, _playOnlineMenu));
        _hostGameMenu.Close();
        _browseGamesMenu.Close();
        _playOnlineGroup.Add(_browseGamesMenu, doAnchor: false);
        _playOnlineGroup.Add(_browseGamesMenu._passwordEntryMenu, doAnchor: false);
        _playOnlineGroup.Add(_browseGamesMenu._portEntryMenu, doAnchor: false);
        _playOnlineGroup.Add(_hostGameMenu, doAnchor: false);
        _hostSettingsMenu.Close();
        _playOnlineGroup.Add(_hostSettingsMenu, doAnchor: false);
        if (Network.available)
        {
            foreach (MatchSetting m3 in onlineSettings)
            {
                if (!m3.createOnly && (!(m3.id == "customlevelsenabled") || !ParentalControls.AreParentalControlsActive()))
                {
                    _joinGameMenu.AddMatchSetting(m3, filterMenu: true);
                }
            }
            _joinGameMenu.Add(new UIText(" ", Color.White));
            _joinGameMenu.Add(new UIMenuItemNumber("Ping", null, new FieldBinding(typeof(UIMatchmakerMark2), "searchMode", 0f, 2f), 1, default(Color), null, null, "", null, new List<string> { "|DGYELLO|PREFER GOOD", "|DGGREEN|GOOD PING", "|DGREDDD|ANY PING" }));
            _joinGameMenu.Add(new UIText(" ", Color.White));
            _joinGameMenu.Add(new UIMenuItem("|DGGREEN|FIND GAME", new UIMenuActionOpenMenu(_joinGameMenu, _matchmaker)));
            _joinGameMenu.AssignDefaultSelection();
            _joinGameMenu.SetBackFunction(new UIMenuActionOpenMenu(_joinGameMenu, _playOnlineMenu));
            _joinGameMenu.Close();
            _playOnlineGroup.Add(_joinGameMenu, doAnchor: false);
            foreach (MatchSetting m4 in matchSettings)
            {
                if (!(m4.id == "workshopmaps") || Network.available)
                {
                    _filtersMenu.AddMatchSetting(m4, filterMenu: true);
                }
            }
            _filtersMenu.Add(new UIText(" ", Color.White));
            _filtersMenu.Add(new UIModifierMenuItem(new UIMenuActionOpenMenu(_filtersMenu, _filterModifierMenu)));
            _filtersMenu.Add(new UIText(" ", Color.White));
            _filtersMenu.Add(new UIMenuItem("|DGBLUE|CLEAR FILTERS", new UIMenuActionCallFunction(ClearFilters)));
            _filtersMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenu(_filtersMenu, _joinGameMenu), UIAlign.Center, default(Color), backButton: true));
            _filtersMenu.Close();
            _playOnlineGroup.Add(_filtersMenu, doAnchor: false);
            foreach (UnlockData dat2 in Unlocks.GetUnlocks(UnlockType.Modifier))
            {
                _filterModifierMenu.Add(new UIMenuItemToggle(dat2.GetShortNameForDisplay(), null, new FieldBinding(dat2, "enabled"), default(Color), new FieldBinding(dat2, "filtered")));
            }
            _filterModifierMenu.Add(new UIText(" ", Color.White));
            _filterModifierMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenu(_filterModifierMenu, _filtersMenu), UIAlign.Center, default(Color), backButton: true));
            _filterModifierMenu.Close();
            _playOnlineGroup.Add(_filterModifierMenu, doAnchor: false);
            _matchmaker.Close();
            _playOnlineGroup.Add(_matchmaker, doAnchor: false);
        }
        _playOnlineGroup.Close();
        Level.Add(_playOnlineGroup);
        Graphics.fade = 0f;
        Layer l = new Layer("HUD2", -85, new Camera());
        l.camera.width /= 2f;
        l.camera.height /= 2f;
        Layer.Add(l);
        Layer hUD = Layer.HUD;
        Layer.HUD = l;
        Layer.HUD = hUD;
        if (!DuckNetwork.isDedicatedServer && !DuckNetwork.ShowUserXPGain() && Unlockables.HasPendingUnlocks())
        {
            MonoMain.pauseMenu = new UIUnlockBox(Unlockables.GetPendingUnlocks().ToList(), Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f);
        }
        Level.core.endedGameInProgress = false;
    }

    public void OpenPauseMenu(ProfileBox2 pProfile)
    {
        _pauseMenuProfile = pProfile;
        BuildPauseMenu();
        if (Network.isActive && pProfile.profile != DuckNetwork.localProfile)
        {
            _localPauseGroup.DoUpdate();
            _localPauseMenu.DoUpdate();
            _localPauseGroup.Open();
            _localPauseMenu.Open();
            MonoMain.pauseMenu = _localPauseGroup;
        }
        else
        {
            _pauseGroup.DoUpdate();
            _pauseMenu.DoUpdate();
            _pauseGroup.Open();
            _pauseMenu.Open();
            SFX.Play("pause", 0.6f);
            MonoMain.pauseMenu = _pauseGroup;
        }
    }

    public override void NetworkDebuggerPrepare()
    {
        PrepareForOnline();
    }

    public static void HostGameInviteLink()
    {
        if (!Network.isActive)
        {
            FillMatchmakingProfiles();
            DuckNetwork.Host(8, NetworkLobbyType.Private);
            (Level.current as TeamSelect2).PrepareForOnline();
            _didHost = true;
            DevConsole.Log(DCSection.Connection, "Hosting Server via Invite Link!");
        }
        else
        {
            DevConsole.Log(DCSection.Connection, "Copied Invite Link!");
        }
        Main.SpecialCode = "Copied Invite Link.";
        _copyInviteLink = true;
    }

    public static void DoInvite()
    {
        if (!Network.isActive)
        {
            FillMatchmakingProfiles();
            DuckNetwork.Host(8, NetworkLobbyType.Private);
            (Level.current as TeamSelect2).PrepareForOnline();
            _didHost = true;
        }
        _attemptingToInvite = true;
        _copyInviteLink = false;
    }

    public static void InvitedFriend(User u)
    {
        if (Network.InLobby() && u != null)
        {
            _invitedUsers.Add(u);
            DuckNetwork.core._invitedFriends.Add(u.id);
            DoInvite();
            Main.SpecialCode = "Invited Friend (" + u.id + ")";
            DevConsole.Log(DCSection.Connection, Main.SpecialCode);
        }
    }

    public override void Update()
    {
        if (MonoMain.pauseMenu == null && Options.Data.showControllerWarning && Input.mightHavePlaystationController && !_showedPS4Warning)
        {
            _showedPS4Warning = true;
            MonoMain.pauseMenu = Options.controllerWarning;
            Options.controllerWarning.Open();
        }
        if (Level.core.endedGameInProgress && !DuckNetwork.isDedicatedServer)
        {
            _waitToShow -= Maths.IncFrameTimer();
            if (_waitToShow <= 0f && MonoMain.pauseMenu == null)
            {
                if (!DuckNetwork.ShowUserXPGain() && Unlockables.HasPendingUnlocks())
                {
                    MonoMain.pauseMenu = new UIUnlockBox(Unlockables.GetPendingUnlocks().ToList(), Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f);
                }
                Level.core.endedGameInProgress = false;
            }
        }
        base.backgroundColor = Color.Black;
        if (_copyInviteLink && Steam.user != null && Steam.lobby != null && Steam.lobby.id != 0L)
        {
            DuckNetwork.CopyInviteLink();
            _copyInviteLink = false;
        }
        bool eightPlayer = false;
        if (Network.isActive)
        {
            int idx = 0;
            foreach (Profile p in DuckNetwork.profiles)
            {
                if (p.slotType != SlotType.Closed && p.slotType != SlotType.Spectator && (p.slotType != SlotType.Invite || p.connection != null || explicitlyCreated) && idx > 3)
                {
                    eightPlayer = true;
                }
                idx++;
            }
        }
        eightPlayersActive = Profiles.activeNonSpectators.Count > 4;
        zoomedOut = false;
        if (growCamera || UISlotEditor.editingSlots || eightPlayer || eightPlayersActive || showEightPlayerSelected)
        {
            if (oldCameraSize == Vector2.Zero)
            {
                oldCameraSize = Level.current.camera.size;
                oldCameraPos = Level.current.camera.position;
            }
            float camSize = 500f;
            Vector2 s = Level.current.camera.size;
            s = Lerp.Vec2Smooth(s, new Vector2(camSize, camSize / 1.77777f), 0.1f, 0.08f);
            Level.current.camera.size = s;
            Level.current.camera.position = Lerp.Vec2Smooth(Level.current.camera.position, new Vector2(-1f, -7f), 0.1f, 0.08f);
            eightPlayersActive = true;
            zoomedOut = true;
        }
        else if (oldCameraSize != Vector2.Zero)
        {
            Level.current.camera.size = Lerp.Vec2Smooth(Level.current.camera.size, oldCameraSize, 0.1f, 0.08f);
            Level.current.camera.position = Lerp.Vec2Smooth(Level.current.camera.position, oldCameraPos, 0.1f, 0.08f);
        }
        growCamera = false;
        if (_findGame.value)
        {
            _findGame.value = false;
            _ = Network.isActive;
        }
        if (_createGame.value || _hostGame.value)
        {
            explicitlyCreated = _createGame.value;
            if (!Network.available)
            {
                GetOnlineSetting("type").value = 3;
            }
            if ((string)GetOnlineSetting("name").value == "")
            {
                GetOnlineSetting("name").value = DefaultGameName();
            }
            DuckNetwork.ChangeSlotSettings();
            if (_hostGame.value)
            {
                FillMatchmakingProfiles();
            }
            bool dedicated = (bool)GetOnlineSetting("dedicated").value;
            foreach (MatchmakingPlayer matchmakingProfile in UIMatchmakingBox.core.matchmakingProfiles)
            {
                matchmakingProfile.spectator = dedicated;
            }
            DuckNetwork.Host(GetSettingInt("maxplayers"), (NetworkLobbyType)GetSettingInt("type"), useCurrentSettings: true);
            PrepareForOnline();
            if (_hostGame.value)
            {
                _beam.ClearBeam();
            }
            DevConsole.Log(DCSection.Connection, "Hosting Game(" + UIMatchmakingBox.core.matchmakingProfiles.Count + ", " + ((NetworkLobbyType)GetSettingInt("type")/*cast due to .constrained prefix*/).ToString() + ")");
            _createGame.value = false;
            _hostGame.value = false;
        }
        if (_inviteFriends.value || _invitedUsers.Count > 0)
        {
            _inviteFriends.value = false;
            if (!Network.isActive)
            {
                FillMatchmakingProfiles();
                DuckNetwork.Host(4, NetworkLobbyType.Private);
                PrepareForOnline();
            }
            _attemptingToInvite = true;
        }
        if (_attemptingToInvite && Network.isActive && (!_didHost || (Steam.lobby != null && !Steam.lobby.processing)))
        {
            foreach (User invitedUser in _invitedUsers)
            {
                Steam.InviteUser(invitedUser, Steam.lobby);
            }
            _invitedUsers.Clear();
            _attemptingToInvite = false;
        }
        if (Network.isActive)
        {
            foreach (InputProfile p2 in InputProfile.defaultProfiles)
            {
                if (p2.JoinGamePressed())
                {
                    DuckNetwork.JoinLocalDuck(p2);
                }
            }
        }
        if (Network.isActive && NetworkDebugger.enabled && NetworkDebugger._instances[NetworkDebugger.currentIndex].hover)
        {
            foreach (InputProfile input in InputProfile.defaultProfiles)
            {
                bool valid = true;
                foreach (ProfileBox2 p3 in _profiles)
                {
                    if (p3.profile != null && (p3.playerActive || p3.profile.connection != null) && p3.profile.inputProfile.genericController == input.genericController)
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid || !input.Pressed("START"))
                {
                    continue;
                }
                foreach (ProfileBox2 box in _profiles)
                {
                    if (box.profile.connection == null)
                    {
                        Profile newProfile = DuckNetwork.JoinLocalDuck(input);
                        if (newProfile != null)
                        {
                            newProfile.inputProfile = input;
                            box.OpenDoor();
                            box.ChangeProfile(newProfile);
                            break;
                        }
                    }
                }
            }
        }
        if (_levelSelector != null)
        {
            if (!_levelSelector.isClosed)
            {
                _levelSelector.Update();
                _beam.active = false;
                _beam.visible = false;
                Editor.selectingLevel = true;
                return;
            }
            _levelSelector.Terminate();
            _levelSelector = null;
            Layer.skipDrawing = false;
            _beam.active = true;
            _beam.visible = true;
            Editor.selectingLevel = false;
        }
        if (openLevelSelect)
        {
            Graphics.fade = Lerp.Float(Graphics.fade, 0f, 0.04f);
            if (Graphics.fade < 0.01f)
            {
                _levelSelector = new LevelSelect("", this);
                _levelSelector.Initialize();
                openLevelSelect = false;
                Layer.skipDrawing = true;
            }
            return;
        }
        int ready = 0;
        activePlayers = 0;
        foreach (ProfileBox2 b in _profiles)
        {
            if (b.ready && b.profile != null)
            {
                ready++;
            }
            if ((b.playerActive || (b.duck != null && !b.duck.dead && !b.duck.removeFromLevel)) && b.profile != null)
            {
                activePlayers++;
            }
        }
        _beam.active = !menuOpen;
        if (!menuOpen)
        {
            HUD.CloseAllCorners();
        }
        if (_backOut.value)
        {
            if (Network.isActive)
            {
                Level.current = new DisconnectFromGame();
            }
            else
            {
                _backOut.value = false;
                if (_pauseMenuProfile != null)
                {
                    if (_beam != null)
                    {
                        _beam.RemoveDuck(_pauseMenuProfile.duck);
                    }
                    _pauseMenuProfile.CloseDoor();
                    _pauseMenuProfile = null;
                }
            }
        }
        if (Graphics.fade <= 0f && _returnToMenu.value)
        {
            Level.current = new TitleScreen();
        }
        if (!Network.isActive)
        {
            DuckNetwork.core.startCountdown = _starting;
        }
        int numLocalPlayers = 1;
        if (Network.isActive)
        {
            numLocalPlayers = 0;
            foreach (Profile profile in DuckNetwork.profiles)
            {
                if (profile.connection == DuckNetwork.localConnection)
                {
                    numLocalPlayers++;
                }
            }
        }
        if (Keyboard.Down(Keys.F1) && !DuckNetwork.TryPeacefulResolution(pDoLevelSwitch: false))
        {
            ready = 2;
            activePlayers = 2;
            numLocalPlayers = 0;
        }
        if (activePlayers == ready && !_returnToMenu.value && ((!Network.isActive && ready > 0) || (Network.isActive && ready > numLocalPlayers)))
        {
            _singlePlayer = ready == 1;
            if (DuckNetwork.core.startCountdown)
            {
                DuckNetwork.inGame = true;
                dim = Maths.LerpTowards(dim, 0.8f, 0.02f);
                _countTime -= 1f / 150f;
                if (_countTime <= 0f && Network.isServer && (Graphics.fade <= 0f || NetworkDebugger.enabled))
                {
                    UpdateModifierStatus();
                    DevConsole.qwopMode = Enabled("QWOPPY", ignoreTeamSelect: true);
                    DevConsole.splitScreen = Enabled("SPLATSCR", ignoreTeamSelect: true);
                    DevConsole.rhythmMode = Enabled("RHYM", ignoreTeamSelect: true);
                    DuckNetwork.SetMatchSettings(initialSettings: true, GetSettingInt("requiredwins"), GetSettingInt("restsevery"), (bool)GetOnlineSetting("teams").value, GetSettingBool("wallmode"), GetSettingInt("normalmaps"), GetSettingInt("randommaps"), GetSettingInt("workshopmaps"), GetSettingInt("custommaps"), Editor.activatedLevels.Count, GetNetworkModifierList(), GetSettingBool("clientlevelsenabled"));
                    partyMode = GetSettingBool("partymode");
                    if (Network.isActive && Network.isServer)
                    {
                        foreach (Profile p4 in DuckNetwork.profiles)
                        {
                            if (p4.connection == null && p4.slotType != SlotType.Reserved && p4.slotType != SlotType.Spectator)
                            {
                                p4.slotType = SlotType.Closed;
                            }
                        }
                    }
                    if (Network.isActive)
                    {
                        SendMatchSettings();
                    }
                    if (!Level.core.gameInProgress)
                    {
                        Main.ResetMatchStuff();
                    }
                    Music.Stop();
                    MonoMain.FinishLazyLoad();
                    if (!_singlePlayer)
                    {
                        if (!Network.isServer)
                        {
                            return;
                        }
                        foreach (Profile p5 in DuckNetwork.profiles)
                        {
                            p5.reservedUser = null;
                            if ((p5.connection == null || p5.connection.status != ConnectionStatus.Connected) && p5.slotType == SlotType.Reserved)
                            {
                                p5.slotType = SlotType.Closed;
                            }
                        }
                        Level newLevel = null;
                        newLevel = ((!ctfMode) ? new GameLevel(Deathmatch.RandomLevelString()) : new CTFLevel(Deathmatch.RandomLevelString("", "ctf")));
                        _spectatorCountdownStop = false;
                        Main.lastLevel = newLevel.level;
                        if (Network.isActive && Network.isServer)
                        {
                            if (Network.activeNetwork.core.lobby != null)
                            {
                                Network.activeNetwork.core.lobby.SetLobbyData("started", "true");
                                Network.activeNetwork.core.lobby.joinable = false;
                            }
                            DuckNetwork.inGame = true;
                        }
                        Level.sendCustomLevels = true;
                        Level.current = newLevel;
                        return;
                    }
                    Level.current.Clear();
                    Level.current = new ArcadeLevel(Content.GetLevelID("arcade"));
                }
            }
            else
            {
                DuckNetwork.inGame = false;
                dim = Maths.LerpTowards(dim, 0f, 0.1f);
                if (dim < 0.05f)
                {
                    _countTime = 1.5f;
                }
            }
            _matchSetup = true;
            if (Network.isServer)
            {
                if (!Network.isActive)
                {
                    if (!_singlePlayer && !_starting && !menuOpen && Input.Pressed("MENU1"))
                    {
                        _configGroup.Open();
                        _multiplayerMenu.Open();
                        MonoMain.pauseMenu = _configGroup;
                    }
                    if (!_starting && !menuOpen && Input.Pressed("MENU2"))
                    {
                        PlayOnlineSinglePlayer();
                    }
                }
                if ((!menuOpen && Input.Pressed("SELECT") && (!_singlePlayer || (Profiles.active.Count > 0 && !Profiles.IsDefault(Profiles.active[0])))) || (DuckNetwork.isDedicatedServer && !_sentDedicatedCountdown && !_spectatorCountdownStop))
                {
                    if (Network.isActive)
                    {
                        Send.Message(new NMBeginCountdown());
                        _sentDedicatedCountdown = true;
                        _spectatorCountdownStop = false;
                    }
                    else
                    {
                        _starting = true;
                    }
                }
                if (Network.isActive && DuckNetwork.isDedicatedServer && !_spectatorCountdownStop && Input.Pressed("CANCEL"))
                {
                    _spectatorCountdownStop = true;
                    _sentDedicatedCountdown = false;
                    _starting = false;
                    DuckNetwork.core.startCountdown = false;
                    Send.Message(new NMCancelCountdown());
                }
            }
        }
        else
        {
            dim = Maths.LerpTowards(dim, 0f, 0.1f);
            if (dim < 0.05f)
            {
                _countTime = 1.5f;
            }
            _matchSetup = false;
            _starting = false;
            DuckNetwork.core.startCountdown = false;
            DuckNetwork.inGame = false;
            _sentDedicatedCountdown = false;
            _spectatorCountdownStop = false;
        }
        base.Update();
        if (Network.isActive)
        {
            _afkTimeout += Maths.IncFrameTimer();
            foreach (Profile p6 in DuckNetwork.profiles)
            {
                if (p6.localPlayer && p6.inputProfile != null && p6.inputProfile.Pressed("ANY", any: true))
                {
                    _afkTimeout = 0f;
                }
            }
            if (DuckNetwork.lobbyType == DuckNetwork.LobbyType.FriendsOnly || DuckNetwork.lobbyType == DuckNetwork.LobbyType.Private)
            {
                _afkTimeout = 0f;
            }
            if (_afkTimeout > _afkShowTimeout && (int)_afkTimeout != _timeoutBeep)
            {
                _timeoutBeep = (int)_afkTimeout;
                SFX.Play("cameraBeep");
            }
            if (_afkTimeout > _afkMaxTimeout)
            {
                Level.current = new DisconnectFromGame();
            }
        }
        else
        {
            _afkTimeout = 0f;
        }
        Graphics.fade = Lerp.Float(Graphics.fade, (_returnToMenu.value || _countTime <= 0f) ? 0f : 1f, 0.02f);
        _setupFade = Lerp.Float(_setupFade, (_matchSetup && !menuOpen && !DuckNetwork.core.startCountdown) ? 1f : 0f, 0.05f);
        Layer.Game.fade = Lerp.Float(Layer.Game.fade, _matchSetup ? 0.5f : 1f, 0.05f);
    }

    private void PlayOnlineSinglePlayer()
    {
        DefaultSettings(resetMatchSettings: false);
        PlayOnlineSinglePlayerAfterOnline();
    }

    private void PlayOnlineSinglePlayerAfterOnline()
    {
        FillMatchmakingProfiles();
        _playOnlineGroup.Open();
        if (!_showedOnlineBumper)
        {
            _showedOnlineBumper = true;
            _playOnlineBumper.Open();
        }
        else
        {
            _playOnlineMenu.Open();
        }
        MonoMain.pauseMenu = _playOnlineGroup;
    }

    private void HostOnlineMultipleLocalPlayers()
    {
        HostOnlineMultipleLocalPlayersAfterOnline();
    }

    private void HostOnlineMultipleLocalPlayersAfterOnline()
    {
        _playOnlineGroup.Open();
        miniHostMenu = true;
        _hostGameMenu.SetBackFunction(new UIMenuActionCloseMenuCallFunction(_playOnlineGroup, DefaultSettingsHostWindow));
        _hostGameMenu.Open();
        MonoMain.pauseMenu = _playOnlineGroup;
    }

    public static void FillMatchmakingProfiles()
    {
        if (Profiles.active.Count == 0)
        {
            NCSteam.PrepareProfilesForJoin();
        }
        for (int i = 0; i < DG.MaxPlayers; i++)
        {
            if (Level.current is TeamSelect2)
            {
                (Level.current as TeamSelect2).ClearTeam(i);
            }
        }
        Profile masterProfile = Profiles.active.FirstOrDefault((Profile x) => x == Profiles.experienceProfile);
        Profile replace = null;
        if (masterProfile == null)
        {
            replace = Profiles.active[0];
            masterProfile = Profiles.experienceProfile;
        }
        UIMatchmakingBox.core.matchmakingProfiles.Clear();
        foreach (Profile p in Profiles.active.ToList())
        {
            p.UpdatePersona();
            if (p.persona == null)
            {
                throw new Exception("FillMatchmakingProfiles() p.persona was null!");
            }
            if (p.team == null)
            {
                throw new Exception("FillMatchmakingProfiles() p.team was null!");
            }
            MatchmakingPlayer play = new MatchmakingPlayer
            {
                inputProfile = p.inputProfile,
                team = p.team,
                persona = p.persona,
                originallySelectedProfile = p,
                customData = null
            };
            if (p == replace)
            {
                play.isMaster = true;
            }
            else if (masterProfile != null && masterProfile != p)
            {
                play.masterProfile = masterProfile;
            }
            UIMatchmakingBox.core.matchmakingProfiles.Add(play);
        }
    }

    public override void Draw()
    {
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (_levelSelector != null)
        {
            if (_levelSelector.isInitialized)
            {
                _levelSelector.PostDrawLayer(layer);
            }
            return;
        }
        if (layer == Layer.Game && UISlotEditor.editingSlots)
        {
            foreach (ProfileBox2 b in Level.current.things[typeof(ProfileBox2)])
            {
                if (UISlotEditor._slot == b.controllerIndex)
                {
                    Graphics.DrawRect(b.Position, b.Position + new Vector2(141f, 89f), Color.White, 0.95f, filled: false);
                }
                else
                {
                    Graphics.DrawRect(b.Position, b.Position + new Vector2(141f, 89f), Color.Black * 0.5f, 0.95f);
                }
            }
            foreach (BlankDoor b2 in Level.current.things[typeof(BlankDoor)])
            {
                Graphics.DrawRect(b2.Position, b2.Position + new Vector2(141f, 89f), Color.Black * 0.5f, 0.95f);
            }
        }
        _ = Layer.Background;
        if (layer == Layer.HUD)
        {
            if (_afkTimeout >= _afkShowTimeout)
            {
                _timeoutFade = Lerp.Float(_timeoutFade, 1f, 0.05f);
                Graphics.DrawRect(new Vector2(-1000f, -1000f), new Vector2(10000f, 10000f), Color.Black * 0.7f * _timeoutFade, 0.95f);
                string timeoutString = "AFK TIMEOUT IN";
                string timeoutTime = ((int)(_afkMaxTimeout - _afkTimeout)).ToString();
                Graphics.DrawString(timeoutString, new Vector2(layer.width / 2f - Graphics.GetStringWidth(timeoutString) / 2f, layer.height / 2f - 8f), Color.White * _timeoutFade, 0.96f);
                Graphics.DrawString(timeoutTime, new Vector2(layer.width / 2f - Graphics.GetStringWidth(timeoutTime), layer.height / 2f + 4f), Color.White * _timeoutFade, 0.96f, null, 2f);
            }
            else
            {
                _timeoutFade = Lerp.Float(_timeoutFade, 0f, 0.05f);
            }
            foreach (Profile p in DuckNetwork.profiles)
            {
                if (p.reservedUser != null && p.slotType == SlotType.Reserved)
                {
                    break;
                }
            }
            if (Level.core.gameInProgress)
            {
                Vector2 topPos = new Vector2(0f, Layer.HUD.barSize);
                Graphics.DrawRect(new Vector2(0f, topPos.Y), new Vector2(320f, topPos.Y + 10f), Color.Black, 0.9f);
                _littleFont.Depth = 0.95f;
                string realText = "GAME STILL IN PROGRESS, HOST RETURNED TO LOBBY.";
                string scrollText = "";
                if (realText.Length > 0)
                {
                    int curChar = 0;
                    int len = realText.Length * 2;
                    if (len < 90)
                    {
                        len = 90;
                    }
                    while (scrollText.Length < len)
                    {
                        scrollText += realText[curChar];
                        curChar++;
                        if (curChar >= realText.Length)
                        {
                            curChar = 0;
                            scrollText += " ";
                        }
                    }
                }
                float inc = 0.01f;
                if (realText.Length > 20)
                {
                    inc = 0.005f;
                }
                if (realText.Length > 30)
                {
                    inc = 0.002f;
                }
                _topScroll += inc;
                if (_topScroll > 1f)
                {
                    _topScroll -= 1f;
                }
                if (_topScroll < 0f)
                {
                    _topScroll += 1f;
                }
                _littleFont.Draw(scrollText, new Vector2(1f - _topScroll * (_littleFont.GetWidth(realText) + 7f), topPos.Y + 3f), Color.White, 0.95f);
            }
            if (_setupFade > 0.01f)
            {
                float yPos = Layer.HUD.camera.height / 2f - 28f;
                string onlineString = "@MENU2@PLAY ONLINE";
                if (!Network.available)
                {
                    onlineString = "@MENU2@PLAY LAN (NO STEAM)";
                    if (Steam.user != null && Steam.user.state == SteamUserState.Offline)
                    {
                        onlineString = "@MENU2@PLAY LAN (STEAM OFFLINE MODE)";
                    }
                }
                else if (Profiles.active.Count > 3)
                {
                    onlineString = "|GRAY|ONLINE UNAVAILABLE (FULL GAME)";
                }
                if (_singlePlayer)
                {
                    string challengeText = "@SELECT@CHALLENGE ARCADE";
                    if (Profiles.active.Count == 0 || Profiles.IsDefault(Profiles.active[0]))
                    {
                        challengeText = "|GRAY|NO ARCADE (SELECT A PROFILE)";
                    }
                    if (Network.available)
                    {
                        string text = challengeText;
                        _font.Alpha = _setupFade;
                        _font.Draw(text, Layer.HUD.width / 2f - _font.GetWidth(text) / 2f, yPos + 15f, Color.White, 0.81f);
                        text = onlineString;
                        _font.Alpha = _setupFade;
                        _font.Draw(text, Layer.HUD.width / 2f - _font.GetWidth(text) / 2f, yPos + 12f + 17f, Color.White, 0.81f);
                    }
                    else
                    {
                        string text2 = challengeText;
                        _font.Alpha = _setupFade;
                        _font.Draw(text2, Layer.HUD.width / 2f - _font.GetWidth(text2) / 2f, yPos + 15f, Color.White, 0.81f);
                        text2 = onlineString;
                        _font.Alpha = _setupFade;
                        _font.Draw(text2, Layer.HUD.width / 2f - _font.GetWidth(text2) / 2f, yPos + 12f + 17f, Color.White, 0.81f);
                    }
                }
                else
                {
                    _font.Alpha = _setupFade;
                    if (Network.isClient)
                    {
                        string text3 = "WAITING FOR HOST TO START";
                        if (Level.core.gameInProgress)
                        {
                            text3 = "WAITING FOR HOST TO RESUME";
                        }
                        _font.Draw(text3, Layer.HUD.width / 2f - _font.GetWidth(text3) / 2f, yPos + 22f, Color.White, 0.81f);
                    }
                    else if (!Network.isActive)
                    {
                        string text4 = "@SELECT@START MATCH";
                        _font.Draw(text4, Layer.HUD.width / 2f - _font.GetWidth(text4) / 2f, yPos + 9f, Color.White, 0.81f);
                        text4 = "@MENU1@MATCH SETTINGS";
                        _font.Draw(text4, Layer.HUD.width / 2f - _font.GetWidth(text4) / 2f, yPos + 22f, Color.White, 0.81f);
                        text4 = onlineString;
                        _font.Draw(text4, Layer.HUD.width / 2f - _font.GetWidth(text4) / 2f, yPos + 35f, Color.White, 0.81f);
                    }
                    else
                    {
                        string text5 = "@SELECT@START MATCH";
                        if (Level.core.gameInProgress)
                        {
                            text5 = "@SELECT@RESUME MATCH";
                        }
                        _font.Draw(text5, Layer.HUD.width / 2f - _font.GetWidth(text5) / 2f, yPos + 22f, Color.White, 0.81f);
                    }
                }
                _countdownScreen.Alpha = _setupFade;
                _countdownScreen.Depth = 0.8f;
                _countdownScreen.CenterY = _countdownScreen.height / 2;
                Graphics.Draw(_countdownScreen, Layer.HUD.camera.x, Layer.HUD.camera.height / 2f);
            }
            if (dim > 0.01f)
            {
                _countdownScreen.Alpha = 1f;
                _countdownScreen.Depth = 0.8f;
                _countdownScreen.CenterY = _countdownScreen.height / 2;
                Graphics.Draw(_countdownScreen, Layer.HUD.camera.x, Layer.HUD.camera.height / 2f);
                _countdown.Alpha = dim * 1.2f;
                _countdown.Depth = 0.81f;
                _countdown.frame = (int)(float)Math.Ceiling((1f - _countTime) * 2f);
                _countdown.CenterY = _countdown.height / 2;
                if (DuckNetwork.isDedicatedServer)
                {
                    Graphics.Draw(_countdown, 160f, Layer.HUD.camera.height / 2f - 8f);
                    string text6 = "@CANCEL@STOP COUNTDOWN";
                    _font.Alpha = dim * 1.2f;
                    _font.Draw(text6, Layer.HUD.width / 2f - _font.GetWidth(text6) / 2f, Layer.HUD.camera.height / 2f + 8f, Color.White, 0.81f);
                }
                else
                {
                    Graphics.Draw(_countdown, 160f, Layer.HUD.camera.height / 2f - 3f);
                }
            }
        }
        base.PostDrawLayer(layer);
    }

    public HatSelector GetHatSelector(int index)
    {
        return _profiles[index]._hatSelector;
    }

    public override void OnMessage(NetMessage m)
    {
    }

    public override void OnNetworkConnected(Profile p)
    {
    }

    public override void OnNetworkConnecting(Profile p)
    {
        if (p.networkIndex < _profiles.Count)
        {
            _profiles[p.networkIndex].Despawn();
            _profiles[p.networkIndex].PrepareDoor();
            return;
        }
        DevConsole.Log(DCSection.Connection, "@error@|DGRED|TeamSelect2.OnNetworkConnecting out of range(" + p.networkIndex + "," + p.slotType.ToString() + ")");
    }

    public override void OnNetworkDisconnected(Profile p)
    {
        if (UIMatchmakerMark2.instance == null && p.networkIndex < _profiles.Count)
        {
            _profiles[p.networkIndex].Despawn();
        }
    }

    public override void OnSessionEnded(DuckNetErrorInfo error)
    {
        if (UIMatchmakerMark2.instance == null)
        {
            base.OnSessionEnded(error);
        }
    }

    public override void OnDisconnect(NetworkConnection n)
    {
        if (UIMatchmakerMark2.instance == null && n != null)
        {
            base.OnDisconnect(n);
        }
    }
}
