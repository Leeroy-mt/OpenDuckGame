using Microsoft.Xna.Framework;
using SDL3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DuckGame;

public class DuckNetwork
{
    public enum LobbyType
    {
        Private,
        FriendsOnly,
        Public,
        Invisible
    }

    private static List<OnlineLevel> _levels = new List<OnlineLevel>
    {
        new OnlineLevel
        {
            num = 1,
            xpRequired = 0
        },
        new OnlineLevel
        {
            num = 2,
            xpRequired = 175
        },
        new OnlineLevel
        {
            num = 3,
            xpRequired = 400
        },
        new OnlineLevel
        {
            num = 4,
            xpRequired = 1200
        },
        new OnlineLevel
        {
            num = 5,
            xpRequired = 3500
        },
        new OnlineLevel
        {
            num = 6,
            xpRequired = 6500
        },
        new OnlineLevel
        {
            num = 7,
            xpRequired = 10000
        },
        new OnlineLevel
        {
            num = 8,
            xpRequired = 13000
        },
        new OnlineLevel
        {
            num = 9,
            xpRequired = 16000
        },
        new OnlineLevel
        {
            num = 10,
            xpRequired = 19000
        },
        new OnlineLevel
        {
            num = 11,
            xpRequired = 23000
        },
        new OnlineLevel
        {
            num = 12,
            xpRequired = 28000
        },
        new OnlineLevel
        {
            num = 13,
            xpRequired = 34000
        },
        new OnlineLevel
        {
            num = 14,
            xpRequired = 40000
        },
        new OnlineLevel
        {
            num = 15,
            xpRequired = 45000
        },
        new OnlineLevel
        {
            num = 16,
            xpRequired = 50000
        },
        new OnlineLevel
        {
            num = 17,
            xpRequired = 56000
        },
        new OnlineLevel
        {
            num = 18,
            xpRequired = 62000
        },
        new OnlineLevel
        {
            num = 19,
            xpRequired = 75000
        },
        new OnlineLevel
        {
            num = 20,
            xpRequired = 100000
        }
    };

    public static int kills;

    public static int deaths;

    public static bool finishedMatch = false;

    private static DuckNetworkCore _core = new DuckNetworkCore();

    public static string compressedLevelName = null;

    public static int numSlots = 4;

    private static Action _modsAcceptFunction;

    private static UIMenu _ducknetMenu;

    private static UIComponent _uhOhGroup;

    private static UIMenu _uhOhMenu;

    public static bool invited;

    public static bool preparingProfiles;

    public static int joinPort = 0;

    private static List<Profile> _spectatorSwaps = new List<Profile>();

    private static List<NetworkConnection> _processedConnections = new List<NetworkConnection>();

    public static object potentialHostObject;

    private static string _currentTransferLevelName = null;

    public const string kServerIdentifier = "SERVER";

    public const string kServerLocalIdentifier = "SERVERLOCAL";

    public static float chatScale => 1f;

    public static Dictionary<string, XPPair> _xpEarned
    {
        get
        {
            return _core._xpEarned;
        }
        set
        {
            _core._xpEarned = value;
        }
    }

    private static UIMenu _xpMenu
    {
        get
        {
            return _core.xpMenu;
        }
        set
        {
            _core.xpMenu = value;
        }
    }

    public static DuckNetworkCore core
    {
        get
        {
            return _core;
        }
        set
        {
            _core = value;
        }
    }

    public static NetworkConnection localConnection
    {
        get
        {
            return _core.localConnection;
        }
        set
        {
            _core.localConnection = value;
        }
    }

    public static bool active => _core.status != DuckNetStatus.Disconnected;

    public static byte levelIndex
    {
        get
        {
            return localConnection.levelIndex;
        }
        set
        {
            localConnection.levelIndex = value;
        }
    }

    public static MemoryStream compressedLevelData
    {
        get
        {
            return _core.compressedLevelData;
        }
        set
        {
            _core.compressedLevelData = value;
        }
    }

    public static List<Profile> profiles => _core.profiles;

    public static List<Profile> profilesFixedOrder => _core.profilesFixedOrder;

    public static Profile localProfile => _core.localProfile;

    public static Profile hostProfile => _core.hostProfile;

    public static int hostDuckIndex
    {
        get
        {
            if (hostProfile == null)
            {
                return 0;
            }
            return profiles.IndexOf(hostProfile);
        }
    }

    public static int localDuckIndex
    {
        get
        {
            if (localProfile == null)
            {
                return 0;
            }
            return profiles.IndexOf(localProfile);
        }
    }

    public static DuckNetStatus status => _core.status;

    public static bool inGame
    {
        get
        {
            return _core.inGame;
        }
        set
        {
            _core.inGame = value;
        }
    }

    public static bool enteringText => _core.enteringText;

    private static UIComponent _ducknetUIGroup
    {
        get
        {
            return _core.ducknetUIGroup;
        }
        set
        {
            _core.ducknetUIGroup = value;
        }
    }

    public static UIComponent duckNetUIGroup => _ducknetUIGroup;

    public static LobbyType lobbyType => _core.lobbyType;

    public static bool allClientsReady
    {
        get
        {
            foreach (Profile p in profiles)
            {
                if (p.connection != null && p.connection.levelIndex != levelIndex)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public static bool isDedicatedServer
    {
        get
        {
            return _core.isDedicatedServer;
        }
        set
        {
            _core.isDedicatedServer = value;
        }
    }

    public static void UpdateFont()
    {
        if (Options.Data.chatFont != "" && RasterFont.GetName(Options.Data.chatFont) != null)
        {
            _core._rasterChatFont = new RasterFont(Options.Data.chatFont, Options.Data.chatFontSize)
            {
                chatFont = true
            };
        }
        else
        {
            _core._rasterChatFont = null;
        }
    }

    public static void Disconnect()
    {
        _core.status = DuckNetStatus.Disconnecting;
    }

    public static OnlineLevel GetLevel(int lev)
    {
        foreach (OnlineLevel l in _levels)
        {
            if (l.num == lev)
            {
                return l;
            }
        }
        return _levels.Last();
    }

    public static void GiveXP(string category, int num, int xp, int type = 4, int firstCap = 9999999, int secondCap = 9999999, int finalCap = 9999999)
    {
        if (Profiles.experienceProfile != null && (!NetworkDebugger.enabled || DG.di == 0))
        {
            if (!_xpEarned.ContainsKey(category))
            {
                _xpEarned[category] = new XPPair();
            }
            _xpEarned[category].num += num;
            if (_xpEarned[category].xp > secondCap)
            {
                _xpEarned[category].xp += xp / 4;
            }
            else if (_xpEarned[category].xp > firstCap)
            {
                _xpEarned[category].xp += xp / 2;
            }
            else
            {
                _xpEarned[category].xp += xp;
            }
            if (_xpEarned[category].xp > finalCap)
            {
                _xpEarned[category].xp = finalCap;
            }
            _xpEarned[category].type = type;
        }
    }

    public static bool ShowUserXPGain()
    {
        if (Level.core.gameFinished && _xpEarned.Count > 0)
        {
            _xpMenu = new UILevelBox("@LWING@PAUSE@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@CLOSE @SELECT@SELECT");
            MonoMain.pauseMenu = _xpMenu;
            Level.core.gameFinished = false;
            return true;
        }
        return false;
    }

    public static KeyValuePair<string, XPPair> TakeXPStat()
    {
        if (_xpEarned.Count == 0)
        {
            return default(KeyValuePair<string, XPPair>);
        }
        KeyValuePair<string, XPPair> val = _xpEarned.ElementAt(0);
        _xpEarned.Remove(val.Key);
        return val;
    }

    public static int GetTotalXPEarned()
    {
        int total = 0;
        foreach (KeyValuePair<string, XPPair> item in _xpEarned)
        {
            total += item.Value.xp;
        }
        return total;
    }

    public static void Initialize()
    {
        _core._builtInChatFont = new FancyBitmapFont("smallFontChat");
        _core._builtInChatFont.chatFont = true;
        _core.initialized = true;
    }

    public static void Kick(Profile p)
    {
        if (p.slotType == SlotType.Local)
        {
            SendToEveryone(new NMClientDisconnect(localConnection.identifier, p));
            ResetProfile(p, reserve: false);
            p.team = null;
            p.slotType = SlotType.Open;
            ChangeSlotSettings();
        }
        else if (Network.isServer && p != null && p.connection != null && p.connection != localConnection)
        {
            SFX.Play("little_punch");
            Send.Message(new NMKick(), p.connection);
            Send.Message(new NMKicked(p));
            DevConsole.Log(DCSection.DuckNet, "|DGRED|Kicking " + p.connection.ToString() + "...");
            p.connection.kicking = true;
            Network.activeNetwork.core.DisconnectClient(p.connection, new DuckNetErrorInfo(DuckNetError.Kicked, ""), kicked: true);
        }
    }

    public static void Ban(Profile p)
    {
        if (Network.isServer && p != null && p.connection != null && p.connection != localConnection)
        {
            SFX.Play("little_punch");
            Send.Message(new NMBan(), p.connection);
            Send.Message(new NMBanned(p));
            DevConsole.Log(DCSection.DuckNet, "|DGRED|Banning " + p.connection.ToString() + "...");
            p.connection.banned = true;
            p.connection.kicking = true;
            Network.activeNetwork.core.DisconnectClient(p.connection, new DuckNetErrorInfo(DuckNetError.Banned, ""), kicked: true);
        }
    }

    private static bool ShouldKickForCustomContent()
    {
        if (Network.isActive && ParentalControls.AreParentalControlsActive() && (int)TeamSelect2.GetMatchSetting("custommaps").value > 0 && TeamSelect2.customLevels > 0)
        {
            return true;
        }
        return false;
    }

    public static void SetMatchSettings(bool initialSettings, int varWinsPerSet, int varRoundsPerIntermission, bool varTeams, bool varWallmode, int varNormalPercent, int varRandomPercent, int varWorkshopPercent, int varCustomPercent, int varCustomLevels, List<byte> enabledModifiers, bool varClientLevels)
    {
        TeamSelect2.GetMatchSetting("requiredwins").value = varWinsPerSet;
        TeamSelect2.GetMatchSetting("restsevery").value = varRoundsPerIntermission;
        TeamSelect2.GetMatchSetting("randommaps").value = varRandomPercent;
        TeamSelect2.GetMatchSetting("workshopmaps").value = varWorkshopPercent;
        TeamSelect2.GetMatchSetting("normalmaps").value = varNormalPercent;
        TeamSelect2.GetMatchSetting("custommaps").value = varCustomPercent;
        TeamSelect2.GetMatchSetting("wallmode").value = varWallmode;
        TeamSelect2.GetMatchSetting("clientlevelsenabled").value = varClientLevels;
        RockScoreboard.wallMode = varWallmode;
        TeamSelect2.GetOnlineSetting("teams").value = varTeams;
        if (initialSettings)
        {
            TeamSelect2.prevCustomLevels = varCustomLevels;
        }
        else
        {
            TeamSelect2.prevCustomLevels = TeamSelect2.customLevels;
        }
        TeamSelect2.customLevels = varCustomLevels;
        if (ShouldKickForCustomContent())
        {
            Network.DisconnectClient(localConnection, new DuckNetErrorInfo(DuckNetError.ParentalControls, "Disconnected - Restricted Content"));
        }
        int numModifiers = 0;
        foreach (UnlockData dat in Unlocks.GetUnlocks(UnlockType.Modifier))
        {
            if (Unlocks.modifierToByte.ContainsKey(dat.id))
            {
                byte val = Unlocks.modifierToByte[dat.id];
                if (enabledModifiers.Contains(val))
                {
                    dat.enabled = true;
                    numModifiers++;
                }
                else
                {
                    dat.enabled = false;
                }
                if (initialSettings)
                {
                    dat.prevEnabled = dat.enabled;
                }
            }
        }
        GameMode.roundsBetweenIntermission = varRoundsPerIntermission;
        GameMode.winsPerSet = varWinsPerSet;
        Deathmatch.userMapsPercent = varCustomPercent;
        TeamSelect2.randomMapPercent = varRandomPercent;
        TeamSelect2.normalMapPercent = varNormalPercent;
        TeamSelect2.workshopMapPercent = varWorkshopPercent;
        TeamSelect2.UpdateModifierStatus();
        if (!initialSettings)
        {
            return;
        }
        TeamSelect2.prevNumModifiers = numModifiers;
        foreach (MatchSetting matchSetting in TeamSelect2.matchSettings)
        {
            matchSetting.prevValue = matchSetting.value;
        }
        foreach (MatchSetting onlineSetting in TeamSelect2.onlineSettings)
        {
            onlineSetting.prevValue = onlineSetting.value;
        }
    }

    public static void ChangeSlotSettings()
    {
        ChangeSlotSettings(pInitializingClient: false);
    }

    public static void ChangeSlotSettings(bool pInitializingClient)
    {
        numSlots = 0;
        LobbyType type = LobbyType.Private;
        foreach (Profile p in profiles)
        {
            if (p.connection != localConnection)
            {
                if (p.slotType == SlotType.Friend && type < LobbyType.FriendsOnly)
                {
                    type = LobbyType.FriendsOnly;
                }
                if (p.slotType == SlotType.Open && type < LobbyType.Public)
                {
                    type = LobbyType.Public;
                }
            }
            if (p.slotType != SlotType.Closed && p.slotType != SlotType.Spectator)
            {
                numSlots++;
            }
        }
        if (!Network.isServer)
        {
            return;
        }
        if (Steam.lobby != null)
        {
            if (Steam.lobby.type == SteamLobbyType.Private || Steam.lobby.type == SteamLobbyType.FriendsOnly)
            {
                invited = true;
            }
            Steam.lobby.type = (SteamLobbyType)type;
            Steam.lobby.maxMembers = 32;
            Steam.lobby.SetLobbyData("numSlots", numSlots.ToString());
            TeamSelect2.GetOnlineSetting("maxplayers").value = numSlots;
            Steam.lobby.SetLobbyData("maxplayers", numSlots.ToString());
        }
        List<byte> slots = new List<byte>();
        for (int i = 0; i < DG.MaxPlayers; i++)
        {
            slots.Add((byte)profiles[i].slotType);
        }
        Send.Message(new NMChangeSlots(slots, pInitializingClient));
    }

    public static void KickedPlayer()
    {
        if (_core.kickContext != null)
        {
            Kick(_core.kickContext);
            _core.kickContext = null;
        }
    }

    public static void BannedPlayer()
    {
        if (_core.kickContext != null)
        {
            Ban(_core.kickContext);
            _core.kickContext = null;
        }
    }

    public static void BlockedPlayer()
    {
        if (_core.kickContext != null)
        {
            if (Options.Data.blockedPlayers != null && !Options.Data.blockedPlayers.Contains(_core.kickContext.steamID))
            {
                Options.Data.muteSettings[_core.kickContext.steamID] = "CHR";
                Options.Data.blockedPlayers.Add(_core.kickContext.steamID);
                Options.Data.unblockedPlayers.Remove(_core.kickContext.steamID);
                Options.Save();
            }
            _core.kickContext._blockStatusDirty = true;
            Ban(_core.kickContext);
            _core.kickContext = null;
        }
    }

    public static void UnblockPlayer(Profile pProfile)
    {
        Options.Data.blockedPlayers.Remove(pProfile.steamID);
        if (!Options.Data.unblockedPlayers.Contains(pProfile.steamID))
        {
            Options.Data.unblockedPlayers.Add(pProfile.steamID);
        }
        Options.Data.muteSettings[pProfile.steamID] = "";
        pProfile._blockStatusDirty = true;
        SFX.Play("textLetter", 0.7f);
    }

    public static void ClosePauseMenu()
    {
        if (Network.isActive && MonoMain.pauseMenu != null)
        {
            MonoMain.pauseMenu.Close();
            MonoMain.pauseMenu = null;
            if (_ducknetUIGroup != null)
            {
                Level.Remove(_ducknetUIGroup);
                _ducknetUIGroup = null;
            }
        }
    }

    public static void OpenMatchSettingsInfo()
    {
        _core._willOpenSettingsInfo = true;
    }

    public static void OpenSpectatorInfo(bool pSpectator)
    {
        _core._willOpenSpectatorInfo = ((!pSpectator) ? 1 : 2);
    }

    public static void OpenNoModsWindow(Action acceptFunction)
    {
        float wide = 320f;
        float high = 180f;
        _core._noModsUIGroup = new UIComponent(wide / 2f, high / 2f, 0f, 0f);
        _core._noModsMenu = CreateNoModsOnlineWindow(acceptFunction);
        _core._noModsUIGroup.Add(_core._noModsMenu, doAnchor: false);
        _core._noModsUIGroup.Close();
        _core._noModsUIGroup.Close();
        Level.Add(_core._noModsUIGroup);
        _core._noModsUIGroup.Update();
        _core._noModsUIGroup.Update();
        _core._noModsUIGroup.Update();
        _core._noModsUIGroup.Open();
        _core._noModsMenu.Open();
        MonoMain.pauseMenu = _core._noModsUIGroup;
        _core._pauseOpen = true;
        SFX.Play("pause", 0.6f);
    }

    private static UIMenu CreateNoModsOnlineWindow(Action acceptFunction)
    {
        _modsAcceptFunction = acceptFunction;
        UIMenu uIMenu = new UIMenu("@LWING@YOU HAVE MODS ENABLED@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 230f, -1f, "@CANCEL@BACK");
        BitmapFont littleFont = new BitmapFont("smallBiosFontUI", 7, 5);
        UIText t = new UIText("YOU WILL |DGRED|NOT|WHITE| BE ABLE TO PLAY", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("ONLINE WITH ANYONE WHO DOES ", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("NOT HAVE THE |DGRED|SAME MODS|WHITE|.     ", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("WOULD YOU LIKE TO |DGGREEN|DISABLE|WHITE|   ", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("MODS AND RESTART THE GAME?  ", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        uIMenu.Add(new UIMenuItem("|DGGREEN|DISABLE MODS AND RESTART", new UIMenuActionCloseMenuCallFunction(_core._noModsUIGroup, ModLoader.DisableModsAndRestart), UIAlign.Center, Color.White));
        uIMenu.Add(new UIMenuItem("|DGYELLOW|I KNOW WHAT I'M DOING", new UIMenuActionCloseMenuCallFunction(_core._noModsUIGroup, acceptFunction), UIAlign.Center, Color.White));
        uIMenu.Add(new UIText(" ", Color.White));
        uIMenu.Add(new UIMenuItem("|DGPURPLE|STOP ASKING, I LOVE MODS!", new UIMenuActionCloseMenuCallFunction(_core._noModsUIGroup, QuitShowingModWindow), UIAlign.Center, Color.White));
        uIMenu.SetBackFunction(new UIMenuActionCloseMenu(_core._noModsUIGroup));
        uIMenu.Close();
        return uIMenu;
    }

    public static void QuitShowingModWindow()
    {
        if (_modsAcceptFunction != null)
        {
            Options.Data.showNetworkModWarning = false;
            _modsAcceptFunction();
            Options.Save();
        }
    }

    public static UIComponent OpenModsRestartWindow(UIMenu openOnClose)
    {
        float wide = 320f;
        float high = 180f;
        _core._restartModsUIGroup = new UIComponent(wide / 2f, high / 2f, 0f, 0f);
        _core._restartModsMenu = CreateModsRestartWindow(openOnClose);
        _core._restartModsUIGroup.Add(_core._restartModsMenu, doAnchor: false);
        _core._restartModsUIGroup.Close();
        _core._restartModsUIGroup.Close();
        Level.Add(_core._restartModsUIGroup);
        _core._restartModsUIGroup.Update();
        _core._restartModsUIGroup.Update();
        _core._restartModsUIGroup.Update();
        _core._restartModsUIGroup.Open();
        _core._restartModsMenu.Open();
        MonoMain.pauseMenu = _core._restartModsUIGroup;
        _core._pauseOpen = true;
        SFX.Play("pause", 0.6f);
        return _core._restartModsUIGroup;
    }

    public static UIComponent OpenResolutionRestartMenu(UIMenu openOnClose)
    {
        float wide = 320f;
        float high = 180f;
        _core._resUIGroup = new UIComponent(wide / 2f, high / 2f, 0f, 0f);
        _core._resMenu = CreateResolutionRestartWindow(openOnClose);
        _core._resUIGroup.Add(_core._resMenu, doAnchor: false);
        _core._resUIGroup.Close();
        _core._resUIGroup.Close();
        Level.Add(_core._resUIGroup);
        _core._resUIGroup.Update();
        _core._resUIGroup.Update();
        _core._resUIGroup.Update();
        _core._resUIGroup.Open();
        _core._resMenu.Open();
        MonoMain.pauseMenu = _core._resUIGroup;
        _core._pauseOpen = true;
        SFX.Play("pause", 0.6f);
        return _core._restartModsUIGroup;
    }

    private static UIMenu CreateResolutionRestartWindow(UIMenu openOnClose)
    {
        UIMenu uIMenu = new UIMenu("@LWING@GRAPHICS CHANGE@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 230f, -1f, "@CANCEL@BACK");
        BitmapFont littleFont = new BitmapFont("smallBiosFontUI", 7, 5);
        UIText t = new UIText("YOU NEED TO RESTART THE GAME", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("FOR CHANGES TO TAKE EFFECT. ", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("(ASPECT RATIO CHANGED)", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("DO YOU WANT TO |DGGREEN|RESTART|WHITE| NOW? ", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        uIMenu.Add(new UIMenuItem("|DGGREEN|RESTART NOW", new UIMenuActionCloseMenuCallFunction(_core._resUIGroup, ModLoader.RestartGame), UIAlign.Center, Color.White));
        uIMenu.Add(new UIMenuItem("|DGYELLOW|RESTART LATER", new UIMenuActionOpenMenu(_core._resUIGroup, openOnClose), UIAlign.Center, Color.White));
        uIMenu.Close();
        return uIMenu;
    }

    private static UIMenu CreateModsRestartWindow(UIMenu openOnClose)
    {
        UIMenu uIMenu = new UIMenu("@LWING@MODS CHANGED@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 230f, -1f, "@CANCEL@BACK");
        BitmapFont littleFont = new BitmapFont("smallBiosFontUI", 7, 5);
        UIText t = new UIText("YOU NEED TO RESTART THE GAME", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("FOR CHANGES TO TAKE EFFECT. ", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        t = new UIText("DO YOU WANT TO |DGGREEN|RESTART|WHITE| NOW? ", Color.White);
        t.SetFont(littleFont);
        uIMenu.Add(t);
        uIMenu.Add(new UIMenuItem("|DGGREEN|RESTART", new UIMenuActionCloseMenuCallFunction(_core._restartModsUIGroup, ModLoader.RestartGame), UIAlign.Center, Color.White));
        uIMenu.Add(new UIMenuItem("|DGYELLOW|CONTINUE", new UIMenuActionOpenMenu(_core._restartModsUIGroup, openOnClose), UIAlign.Center, Color.White));
        uIMenu.Close();
        return uIMenu;
    }

    public static UIMenu CreateSpectatorWindow(bool isSpectator)
    {
        UIMenu spectatorMenu = null;
        spectatorMenu = new UIMenu("@LWING@SPECTATOR MODE@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 230f, -1f, "@CANCEL@BACK");
        BitmapFont littleFont = new BitmapFont("smallBiosFontUI", 7, 5);
        if (isSpectator)
        {
            UIText t = new UIText("THE HOST HAS MADE YOU", Color.White);
            t.SetFont(littleFont);
            spectatorMenu.Add(t);
            t = new UIText("A SPECTATOR. YOU WILL", Color.White);
            t.SetFont(littleFont);
            spectatorMenu.Add(t);
            t = new UIText("BE ABLE TO WATCH AND CHAT", Color.White);
            t.SetFont(littleFont);
            spectatorMenu.Add(t);
            t = new UIText("BUT NOT PLAY.", Color.White);
            t.SetFont(littleFont);
            spectatorMenu.Add(t);
        }
        else
        {
            UIText t2 = new UIText("THE HOST DISABLED SPECTATOR", Color.White);
            t2.SetFont(littleFont);
            spectatorMenu.Add(t2);
            t2 = new UIText("MODE. YOU WILL NOW BE ABLE", Color.White);
            t2.SetFont(littleFont);
            spectatorMenu.Add(t2);
            t2 = new UIText("TO PLAY.", Color.White);
            t2.SetFont(littleFont);
            spectatorMenu.Add(t2);
        }
        UIText t3 = new UIText("", Color.White);
        t3.SetFont(littleFont);
        spectatorMenu.Add(t3);
        spectatorMenu.SetBackFunction(new UIMenuActionCloseMenu(_ducknetUIGroup));
        spectatorMenu.Close();
        return spectatorMenu;
    }

    private static UIMenu CreateMatchSettingsInfoWindow(UIMenu openOnClose = null)
    {
        UIMenu matchSettingsInfo = null;
        matchSettingsInfo = ((openOnClose == null) ? new UIMenu("@LWING@NEW SETTINGS@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, "@CANCEL@BACK") : new UIMenu("@LWING@MATCH SETTINGS@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, "@CANCEL@BACK"));
        BitmapFont littleFont = new BitmapFont("biosFontUI", 8, 7);
        UIText t = new UIText("HOST CHANGED SETTINGS", Color.White);
        t.SetFont(littleFont);
        int part1Length = 16;
        int part2Length = 5;
        MatchSetting m = TeamSelect2.GetOnlineSetting("teams");
        string textPart1 = m.name;
        string textPart2 = (((bool)m.value) ? "ON" : "OFF");
        while (textPart1.Length < part1Length)
        {
            textPart1 += " ";
        }
        while (textPart2.Length < part2Length)
        {
            textPart2 = " " + textPart2;
        }
        string text = textPart1 + " " + textPart2;
        t = (m.value.Equals(m.prevValue) ? new UIText(text, Colors.Silver) : new UIText(text, Colors.DGBlue));
        m.prevValue = m.value;
        t.SetFont(littleFont);
        matchSettingsInfo.Add(t);
        m = TeamSelect2.GetMatchSetting("requiredwins");
        textPart1 = m.name;
        textPart2 = m.value.ToString();
        while (textPart1.Length < part1Length)
        {
            textPart1 += " ";
        }
        while (textPart2.Length < part2Length)
        {
            textPart2 = " " + textPart2;
        }
        text = textPart1 + " " + textPart2;
        t = (m.value.Equals(m.prevValue) ? new UIText(text, Colors.Silver) : new UIText(text, Colors.DGBlue));
        m.prevValue = m.value;
        t.SetFont(littleFont);
        matchSettingsInfo.Add(t);
        m = TeamSelect2.GetMatchSetting("restsevery");
        textPart1 = m.name;
        textPart2 = m.value.ToString();
        while (textPart1.Length < part1Length)
        {
            textPart1 += " ";
        }
        while (textPart2.Length < part2Length)
        {
            textPart2 = " " + textPart2;
        }
        text = textPart1 + " " + textPart2;
        t = (m.value.Equals(m.prevValue) ? new UIText(text, Colors.Silver) : new UIText(text, Colors.DGBlue));
        m.prevValue = m.value;
        t.SetFont(littleFont);
        matchSettingsInfo.Add(t);
        m = TeamSelect2.GetMatchSetting("wallmode");
        textPart1 = m.name;
        textPart2 = (((bool)m.value) ? "ON" : "OFF");
        while (textPart1.Length < part1Length)
        {
            textPart1 += " ";
        }
        while (textPart2.Length < part2Length)
        {
            textPart2 = " " + textPart2;
        }
        text = textPart1 + " " + textPart2;
        t = (m.value.Equals(m.prevValue) ? new UIText(text, Colors.Silver) : new UIText(text, Colors.DGBlue));
        m.prevValue = m.value;
        t.SetFont(littleFont);
        matchSettingsInfo.Add(t);
        t = new UIText(" ", Color.White);
        t.SetFont(littleFont);
        matchSettingsInfo.Add(t);
        m = TeamSelect2.GetMatchSetting("normalmaps");
        textPart1 = m.name;
        textPart2 = m.value.ToString() + "%";
        if (m.minString != null && m.value is int && (int)m.value == m.min)
        {
            textPart2 = m.minString;
        }
        int endIdx = m.name.LastIndexOf('|');
        string realTextPart1 = m.name.Substring(endIdx, m.name.Count() - endIdx);
        while (realTextPart1.Length < part1Length)
        {
            textPart1 += " ";
            realTextPart1 += " ";
        }
        while (textPart2.Length < part2Length)
        {
            textPart2 = " " + textPart2;
        }
        text = textPart1 + " " + textPart2;
        text = text.Replace("|DGBLUE|", "");
        t = (m.value.Equals(m.prevValue) ? new UIText(text, Colors.Silver) : new UIText(text, Colors.DGBlue));
        m.prevValue = m.value;
        t.SetFont(littleFont);
        matchSettingsInfo.Add(t);
        m = TeamSelect2.GetMatchSetting("randommaps");
        textPart1 = m.name;
        textPart2 = m.value.ToString() + "%";
        endIdx = m.name.LastIndexOf('|');
        realTextPart1 = m.name.Substring(endIdx, m.name.Count() - endIdx);
        while (realTextPart1.Length < part1Length)
        {
            textPart1 += " ";
            realTextPart1 += " ";
        }
        while (textPart2.Length < part2Length)
        {
            textPart2 = " " + textPart2;
        }
        text = textPart1 + " " + textPart2;
        text = text.Replace("|DGBLUE|", "");
        t = (m.value.Equals(m.prevValue) ? new UIText(text, Colors.Silver) : new UIText(text, Colors.DGBlue));
        m.prevValue = m.value;
        t.SetFont(littleFont);
        matchSettingsInfo.Add(t);
        if (!ParentalControls.AreParentalControlsActive())
        {
            m = TeamSelect2.GetMatchSetting("custommaps");
            textPart1 = m.name;
            textPart2 = m.value.ToString() + "%";
            if (m.minString != null && m.value is int && (int)m.value == m.min)
            {
                textPart2 = m.minString;
            }
            endIdx = m.name.LastIndexOf('|');
            realTextPart1 = m.name.Substring(endIdx, m.name.Count() - endIdx);
            while (realTextPart1.Length < part1Length)
            {
                textPart1 += " ";
                realTextPart1 += " ";
            }
            while (textPart2.Length < part2Length)
            {
                textPart2 = " " + textPart2;
            }
            text = textPart1 + " " + textPart2;
            text = text.Replace("|DGBLUE|", "");
            t = (m.value.Equals(m.prevValue) ? new UIText(text, Colors.Silver) : new UIText(text, Colors.DGBlue));
            m.prevValue = m.value;
            t.SetFont(littleFont);
            matchSettingsInfo.Add(t);
        }
        m = TeamSelect2.GetMatchSetting("workshopmaps");
        textPart1 = m.name;
        textPart2 = m.value.ToString() + "%";
        endIdx = m.name.LastIndexOf('|');
        realTextPart1 = m.name.Substring(endIdx, m.name.Count() - endIdx);
        while (realTextPart1.Length < part1Length)
        {
            textPart1 += " ";
            realTextPart1 += " ";
        }
        while (textPart2.Length < part2Length)
        {
            textPart2 = " " + textPart2;
        }
        text = textPart1 + " " + textPart2;
        text = text.Replace("|DGBLUE|", "");
        t = (m.value.Equals(m.prevValue) ? new UIText(text, Colors.Silver) : new UIText(text, Colors.DGBlue));
        m.prevValue = m.value;
        t.SetFont(littleFont);
        matchSettingsInfo.Add(t);
        t = new UIText(" ", Color.White);
        t.SetFont(littleFont);
        matchSettingsInfo.Add(t);
        if (!ParentalControls.AreParentalControlsActive())
        {
            textPart1 = "CUSTOM LEVELS ";
            int numCustom = Editor.customLevelCount;
            textPart2 = numCustom.ToString();
            if (textPart2 == "0")
            {
                textPart2 = "NONE";
            }
            while (textPart1.Length < part1Length)
            {
                textPart1 += " ";
            }
            while (textPart2.Length < part2Length)
            {
                textPart2 = " " + textPart2;
            }
            text = textPart1 + " " + textPart2;
            t = ((numCustom == TeamSelect2.prevCustomLevels) ? new UIText(text, Colors.Silver) : new UIText(text, Colors.DGBlue));
            TeamSelect2.prevCustomLevels = numCustom;
            t.SetFont(littleFont);
            matchSettingsInfo.Add(t);
            m = TeamSelect2.GetMatchSetting("clientlevelsenabled");
            textPart1 = m.name;
            textPart2 = (((bool)m.value) ? "ON" : "OFF");
            while (textPart1.Length < part1Length)
            {
                textPart1 += " ";
            }
            while (textPart2.Length < part2Length)
            {
                textPart2 = " " + textPart2;
            }
            text = textPart1 + " " + textPart2;
            t = (m.value.Equals(m.prevValue) ? new UIText(text, Colors.Silver) : new UIText(text, Colors.DGBlue));
            m.prevValue = m.value;
            t.SetFont(littleFont);
            matchSettingsInfo.Add(t);
            t = new UIText(" ", Color.White);
            t.SetFont(littleFont);
            matchSettingsInfo.Add(t);
        }
        int numModifiers = 0;
        foreach (UnlockData dat in Unlocks.GetUnlocks(UnlockType.Modifier))
        {
            if (dat.onlineEnabled && dat.enabled)
            {
                numModifiers++;
            }
        }
        textPart1 = "MODIFIERS ";
        textPart2 = numModifiers.ToString();
        if (numModifiers == 0)
        {
            textPart2 = "NONE";
        }
        while (textPart1.Length < part1Length)
        {
            textPart1 += " ";
        }
        while (textPart2.Length < part2Length)
        {
            textPart2 = " " + textPart2;
        }
        text = textPart1 + " " + textPart2;
        t = ((TeamSelect2.prevNumModifiers == numModifiers) ? new UIText(text, Colors.Silver) : new UIText(text, Colors.DGBlue));
        TeamSelect2.prevNumModifiers = numModifiers;
        t.SetFont(littleFont);
        matchSettingsInfo.Add(t);
        foreach (UnlockData dat2 in Unlocks.GetUnlocks(UnlockType.Modifier))
        {
            if (dat2.onlineEnabled)
            {
                text = dat2.GetShortNameForDisplay();
                while (text.Length < 20)
                {
                    text += " ";
                }
                if (dat2.enabled != dat2.prevEnabled || dat2.enabled)
                {
                    text = ((!dat2.enabled) ? ("@USEROFFLINE@" + text) : ("@USERONLINE@" + text));
                    t = ((dat2.enabled == dat2.prevEnabled) ? new UIText(text, dat2.enabled ? Color.White : Colors.Silver) : new UIText(text, dat2.enabled ? Colors.DGGreen : Colors.DGRed));
                    t.SetFont(littleFont);
                    matchSettingsInfo.Add(t);
                }
                dat2.prevEnabled = dat2.enabled;
            }
        }
        if (openOnClose != null)
        {
            matchSettingsInfo.SetBackFunction(new UIMenuActionOpenMenu(matchSettingsInfo, openOnClose));
        }
        else
        {
            matchSettingsInfo.SetBackFunction(new UIMenuActionCloseMenu(_ducknetUIGroup));
        }
        matchSettingsInfo.Close();
        return matchSettingsInfo;
    }

    private static void DoMatchSettingsInfoOpen()
    {
        float high = 180f;
        _ducknetUIGroup = new UIComponent(320f / 2f, high / 2f, 0f, 0f);
        _core._matchSettingMenu = CreateMatchSettingsInfoWindow();
        _ducknetUIGroup.Add(_core._matchSettingMenu, doAnchor: false);
        _ducknetUIGroup.Close();
        _ducknetUIGroup.Close();
        Level.Add(_ducknetUIGroup);
        _ducknetUIGroup.Update();
        _ducknetUIGroup.Update();
        _ducknetUIGroup.Update();
        _ducknetUIGroup.Open();
        _core._matchSettingMenu.Open();
        _ducknetUIGroup.isPauseMenu = true;
        MonoMain.pauseMenu = _ducknetUIGroup;
        _core._pauseOpen = true;
        SFX.Play("pause", 0.6f);
    }

    private static void DoSpectatorOpen(bool pSpectator)
    {
        float high = 180f;
        _ducknetUIGroup = new UIComponent(320f / 2f, high / 2f, 0f, 0f);
        UIMenu m = CreateSpectatorWindow(pSpectator);
        _ducknetUIGroup.Add(m, doAnchor: false);
        _ducknetUIGroup.Close();
        _ducknetUIGroup.Close();
        Level.Add(_ducknetUIGroup);
        _ducknetUIGroup.Update();
        _ducknetUIGroup.Update();
        _ducknetUIGroup.Update();
        _ducknetUIGroup.Open();
        m.Open();
        _ducknetUIGroup.isPauseMenu = true;
        MonoMain.pauseMenu = _ducknetUIGroup;
        _core._pauseOpen = true;
        SFX.Play("pause", 0.6f);
    }

    public static void ResetScores()
    {
        Main.ResetGameStuff();
        Main.ResetMatchStuff();
        if (Level.core.gameInProgress)
        {
            Level.core.endedGameInProgress = true;
        }
        Level.core.gameInProgress = false;
        Level.core.gameFinished = true;
        if (Network.isServer)
        {
            Send.Message(new NMResetGameSettings());
        }
    }

    public static void CopyInviteLink()
    {
        if (Steam.user != null && Steam.lobby != null)
        {
            SDL.SDL_SetClipboardText("steam://joinlobby/312530/" + Steam.lobby.id + "/" + Steam.user.id);
            HUD.AddPlayerChangeDisplay("@CLIPCOPY@Invite Link Copied!");
        }
    }

    private static void OpenMenu(Profile whoOpen)
    {
        if (_ducknetUIGroup != null)
        {
            Level.Remove(_ducknetUIGroup);
        }
        bool canInvite = Network.InLobby();
        _core._menuOpenProfile = whoOpen;
        float wide = 320f;
        float high = 180f;
        _ducknetUIGroup = new UIComponent(wide / 2f, high / 2f, 0f, 0f);
        _ducknetUIGroup.isPauseMenu = true;
        core._ducknetMenu = new UIMenu("@LWING@MULTIPLAYER@RWING@", wide / 2f, high / 2f, 210f, -1f, "@CANCEL@CLOSE @SELECT@SELECT");
        _ducknetMenu = core._ducknetMenu;
        if (whoOpen.slotType == SlotType.Local)
        {
            core._confirmMenu = new UIMenu("REALLY BACK OUT?", wide / 2f, high / 2f, 160f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        }
        else
        {
            core._confirmMenu = new UIMenu("REALLY QUIT?", wide / 2f, high / 2f, 160f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        }
        core._confirmBlacklistMenu = new UIMenu("AVOID LEVEL?", wide / 2f, high / 2f, 10f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        core._confirmKick = new UIMenu("REALLY KICK?", wide / 2f, high / 2f, 160f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        core._confirmBan = new UIMenu("REALLY BAN?", wide / 2f, high / 2f, 160f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        core._confirmBlock = new UIMenu("BLOCK PLAYER?", wide / 2f, high / 2f, 280f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        core._confirmReturnToLobby = new UIMenu("RETURN TO LOBBY?", wide / 2f, high / 2f, 230f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        core._confirmMatchSettings = new UIMenu("CHANGING SETTINGS", wide / 2f, high / 2f, 230f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        core._confirmEditSlots = new UIMenu("CHANGING SETTINGS", wide / 2f, high / 2f, 230f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        core._optionsMenu = Options.CreateOptionsMenu();
        _core._settingsBeforeOpen = TeamSelect2.GetMatchSettingString();
        Main.SpecialCode = "men0";
        foreach (Profile p in profiles.OrderBy((Profile x) => x.slotType == SlotType.Spectator))
        {
            if (p.connection != null)
            {
                core._ducknetMenu.Add(new UIConnectionInfo(p, core._ducknetMenu, core._confirmKick, core._confirmBan, core._confirmBlock));
            }
        }
        Main.SpecialCode = "men1";
        core._ducknetMenu.Add(new UIText("", Color.White));
        core._ducknetMenu.Add(new UIMenuItem("RESUME", new UIMenuActionCloseMenuSetBoolean(_ducknetUIGroup, core._menuClosed), UIAlign.Left, default(Color), backButton: true));
        core._ducknetMenu.AssignDefaultSelection();
        if (whoOpen.slotType != SlotType.Local)
        {
            core._ducknetMenu.Add(new UIMenuItem("OPTIONS", new UIMenuActionOpenMenu(core._ducknetMenu, core._optionsMenu), UIAlign.Left));
        }
        if (whoOpen.slotType != SlotType.Local && canInvite && Network.isServer)
        {
            _core._slotEditor = new UISlotEditor(core._ducknetMenu, Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f);
            _core._slotEditor.Close();
            _ducknetUIGroup.Add(_core._slotEditor, doAnchor: false);
            _core._matchSettingMenu = new UIMenu("@LWING@MATCH SETTINGS@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, "@CANCEL@BACK @SELECT@SELECT");
            _core._matchModifierMenu = new UIMenu("MODIFIERS", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 240f, -1f, "@CANCEL@BACK @SELECT@SELECT");
            _core._levelSelectMenu = new LevelSelectCompanionMenu(wide / 2f, high / 2f, _core._matchSettingMenu);
            foreach (UnlockData dat in Unlocks.GetUnlocks(UnlockType.Modifier))
            {
                if (dat.onlineEnabled)
                {
                    if (dat.unlocked)
                    {
                        _core._matchModifierMenu.Add(new UIMenuItemToggle(dat.GetShortNameForDisplay(), null, new FieldBinding(dat, "enabled")));
                    }
                    else
                    {
                        _core._matchModifierMenu.Add(new UIMenuItem("@TINYLOCK@LOCKED", null, UIAlign.Center, Color.Red));
                    }
                }
            }
            Main.SpecialCode = "men2";
            _core._matchModifierMenu.SetBackFunction(new UIMenuActionOpenMenu(_core._matchModifierMenu, _core._matchSettingMenu));
            _core._matchModifierMenu.Close();
            _core._matchSettingMenu.AddMatchSetting(TeamSelect2.GetOnlineSetting("teams"), filterMenu: false);
            foreach (MatchSetting m in TeamSelect2.matchSettings)
            {
                if ((!(m.id == "workshopmaps") || Network.available) && ((!(m.id == "custommaps") && !(m.id == "clientlevelsenabled")) || !ParentalControls.AreParentalControlsActive()))
                {
                    if (m.id != "partymode")
                    {
                        _core._matchSettingMenu.AddMatchSetting(m, filterMenu: false);
                    }
                    if (m.id == "wallmode")
                    {
                        _core._matchSettingMenu.Add(new UIText(" ", Color.White));
                    }
                }
            }
            Main.SpecialCode = "men3";
            _core._matchSettingMenu.Add(new UIText(" ", Color.White));
            if (!ParentalControls.AreParentalControlsActive())
            {
                _core._matchSettingMenu.Add(new UICustomLevelMenu(new UIMenuActionOpenMenu(_core._matchSettingMenu, _core._levelSelectMenu)));
            }
            _core._matchSettingMenu.Add(new UIModifierMenuItem(new UIMenuActionOpenMenu(_core._matchSettingMenu, _core._matchModifierMenu)));
            _core._matchSettingMenu.SetBackFunction(new UIMenuActionOpenMenu(_core._matchSettingMenu, core._ducknetMenu));
            _core._matchSettingMenu.Close();
            _ducknetUIGroup.Add(_core._matchSettingMenu, doAnchor: false);
            _ducknetUIGroup.Add(_core._matchModifierMenu, doAnchor: false);
            _ducknetUIGroup.Add(_core._levelSelectMenu, doAnchor: false);
            _ducknetUIGroup.Close();
            Main.SpecialCode = "men4";
            if (Level.core.gameInProgress)
            {
                _core._ducknetMenu.Add(new UIMenuItem("|DGBLUE|MATCH SETTINGS", new UIMenuActionOpenMenu(_core._ducknetMenu, _core._confirmMatchSettings), UIAlign.Left));
                _core._ducknetMenu.Add(new UIMenuItem("|DGBLUE|EDIT SLOTS", new UIMenuActionOpenMenu(_core._ducknetMenu, _core._confirmEditSlots), UIAlign.Left));
            }
            else
            {
                _core._ducknetMenu.Add(new UIMenuItem("|DGBLUE|MATCH SETTINGS", new UIMenuActionOpenMenu(_core._ducknetMenu, _core._matchSettingMenu), UIAlign.Left));
                _core._ducknetMenu.Add(new UIMenuItem("|DGBLUE|EDIT SLOTS", new UIMenuActionOpenMenu(_core._ducknetMenu, _core._slotEditor), UIAlign.Left));
            }
        }
        Main.SpecialCode = "men5";
        if ((Network.isClient && whoOpen.slotType != SlotType.Local) || (Network.isServer && !Network.InLobby()))
        {
            UIMenu settingsInfoMenu = CreateMatchSettingsInfoWindow(_core._ducknetMenu);
            _ducknetUIGroup.Add(settingsInfoMenu, doAnchor: false);
            _core._ducknetMenu.Add(new UIMenuItem("|DGBLUE|VIEW MATCH SETTINGS", new UIMenuActionOpenMenu(_core._ducknetMenu, settingsInfoMenu), UIAlign.Left));
            Main.SpecialCode = "men6";
            if ((bool)TeamSelect2.GetMatchSetting("clientlevelsenabled").value && Network.InLobby() && !ParentalControls.AreParentalControlsActive())
            {
                _core._levelSelectMenu = new LevelSelectCompanionMenu(wide / 2f, high / 2f, _core._ducknetMenu);
                _core._ducknetMenu.Add(new UICustomLevelMenu(new UIMenuActionOpenMenu(_core._ducknetMenu, _core._levelSelectMenu)));
                _ducknetUIGroup.Add(_core._levelSelectMenu, doAnchor: false);
            }
        }
        Main.SpecialCode = "men7";
        _core._ducknetMenu.Add(new UIText("", Color.White));
        if (canInvite && whoOpen.slotType != SlotType.Local && Network.available)
        {
            Main.SpecialCode = "men8";
            _core._inviteMenu = new UIInviteMenu("INVITE FRIENDS", null, Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f);
            ((UIInviteMenu)_core._inviteMenu).SetAction(new UIMenuActionOpenMenu(_core._inviteMenu, _core._ducknetMenu));
            _core._inviteMenu.Close();
            _ducknetUIGroup.Add(_core._inviteMenu, doAnchor: false);
            Main.SpecialCode = "men9";
            _core._ducknetMenu.Add(new UIMenuItem("|DGGREEN|INVITE FRIENDS", new UIMenuActionOpenMenu(_core._ducknetMenu, _core._inviteMenu), UIAlign.Left));
            _core._ducknetMenu.Add(new UIMenuItem("|DGGREEN|COPY INVITE LINK", new UIMenuActionCloseMenuCallFunction(_ducknetUIGroup, CopyInviteLink), UIAlign.Left));
        }
        Main.SpecialCode = "men10";
        if (Level.current is GameLevel && Level.current.isCustomLevel)
        {
            if ((Level.current as GameLevel).data.metaData.workshopID != 0L && Steam.IsInitialized())
            {
                Main.SpecialCode = "men11";
                WorkshopItem item = WorkshopItem.GetItem((Level.current as GameLevel).data.metaData.workshopID);
                if (item != null)
                {
                    _core._ducknetMenu.Add(new UIMenuItem("@STEAMICON@|DGGREEN|VIEW", new UIMenuActionCallFunction(GameMode.View), UIAlign.Left));
                    if ((item.stateFlags & WorkshopItemState.Subscribed) != WorkshopItemState.None)
                    {
                        _core._ducknetMenu.Add(new UIMenuItem("@STEAMICON@|DGRED|UNSUBSCRIBE", new UIMenuActionCloseMenuCallFunction(_ducknetUIGroup, GameMode.Subscribe), UIAlign.Left));
                    }
                    else
                    {
                        _core._ducknetMenu.Add(new UIMenuItem("@STEAMICON@|DGGREEN|SUBSCRIBE", new UIMenuActionCloseMenuCallFunction(_ducknetUIGroup, GameMode.Subscribe), UIAlign.Left));
                        if (Network.isServer)
                        {
                            _core._ducknetMenu.Add(new UIMenuItem("@blacklist@|DGRED|NEVER AGAIN", new UIMenuActionOpenMenu(_core._ducknetMenu, _core._confirmBlacklistMenu), UIAlign.Left));
                        }
                    }
                }
            }
            Main.SpecialCode = "men12";
            if (!(Level.current as GameLevel).matchOver && Network.isServer)
            {
                _core._ducknetMenu.Add(new UIMenuItem("@SKIPSPIN@|DGRED|SKIP", new UIMenuActionCloseMenuCallFunction(_ducknetUIGroup, GameMode.Skip), UIAlign.Left));
            }
            _core._ducknetMenu.Add(new UIText(" ", Color.White));
        }
        Main.SpecialCode = "men13";
        if (whoOpen.slotType != SlotType.Local || Network.InLobby())
        {
            if (whoOpen.slotType == SlotType.Local)
            {
                _core._ducknetMenu.Add(new UIMenuItem("|DGRED|BACK OUT", new UIMenuActionOpenMenu(_core._ducknetMenu, _core._confirmMenu), UIAlign.Left));
            }
            else
            {
                _core._ducknetMenu.Add(new UIMenuItem("|DGRED|DISCONNECT", new UIMenuActionOpenMenu(_core._ducknetMenu, _core._confirmMenu), UIAlign.Left));
            }
        }
        Main.SpecialCode = "men14";
        if (Network.isServer && Level.current is GameLevel)
        {
            _core._ducknetMenu.Add(new UIMenuItem("|DGRED|BACK TO LOBBY", new UIMenuActionOpenMenu(_core._ducknetMenu, _core._confirmReturnToLobby), UIAlign.Left));
        }
        _ducknetUIGroup._closeFunction = new UIMenuActionCloseMenuSetBoolean(_ducknetUIGroup, _core._menuClosed);
        _core._ducknetMenu.Close();
        _ducknetUIGroup.Add(_core._ducknetMenu, doAnchor: false);
        Options.AddMenus(_ducknetUIGroup);
        Options.openOnClose = _core._ducknetMenu;
        Main.SpecialCode = "men15";
        _core._confirmReturnToLobby.Add(new UIText("YOU WILL BE ABLE TO RETURN", Color.White));
        _core._confirmReturnToLobby.Add(new UIText("TO THE CURRENT GAME.", Color.White));
        _core._confirmReturnToLobby.Add(new UIMenuItem("NO!", new UIMenuActionOpenMenu(_core._confirmReturnToLobby, _core._ducknetMenu), UIAlign.Left, default(Color), backButton: true));
        _core._confirmReturnToLobby.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuSetBoolean(_ducknetUIGroup, _core._returnToLobby)));
        _core._confirmReturnToLobby.Close();
        _ducknetUIGroup.Add(_core._confirmReturnToLobby, doAnchor: false);
        _core._confirmEditSlots.Add(new UIText("WOULD YOU LIKE TO", Color.White));
        _core._confirmEditSlots.Add(new UIText("RESET SCORES?", Color.White));
        _core._confirmEditSlots.Add(new UIText("", Color.White));
        _core._confirmEditSlots.Add(new UIMenuItem("KEEP SCORES", new UIMenuActionOpenMenu(_core._confirmEditSlots, _core._slotEditor)));
        _core._confirmEditSlots.Add(new UIMenuItem("|DGRED|RESET SCORES", new UIMenuActionOpenMenuCallFunction(_core._confirmEditSlots, _core._slotEditor, ResetScores)));
        _core._confirmEditSlots.Close();
        _ducknetUIGroup.Add(_core._confirmEditSlots, doAnchor: false);
        _core._confirmMatchSettings.Add(new UIText("WOULD YOU LIKE TO", Color.White));
        _core._confirmMatchSettings.Add(new UIText("RESET SCORES?", Color.White));
        _core._confirmMatchSettings.Add(new UIText("", Color.White));
        _core._confirmMatchSettings.Add(new UIMenuItem("KEEP SCORES", new UIMenuActionOpenMenu(_core._confirmMatchSettings, _core._matchSettingMenu)));
        _core._confirmMatchSettings.Add(new UIMenuItem("|DGRED|RESET SCORES", new UIMenuActionOpenMenuCallFunction(_core._confirmMatchSettings, _core._matchSettingMenu, ResetScores)));
        _core._confirmMatchSettings.Close();
        _ducknetUIGroup.Add(_core._confirmMatchSettings, doAnchor: false);
        Main.SpecialCode = "men16";
        _core._confirmMenu.Add(new UIMenuItem("NO!", new UIMenuActionOpenMenu(_core._confirmMenu, _core._ducknetMenu), UIAlign.Left, default(Color), backButton: true));
        _core._confirmMenu.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuSetBoolean(_ducknetUIGroup, _core._quit)));
        _core._confirmMenu.Close();
        _ducknetUIGroup.Add(_core._confirmMenu, doAnchor: false);
        _core._confirmBlacklistMenu.Add(new UIText("", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        _core._confirmBlacklistMenu.Add(new UIText("Are you sure you want to avoid", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        _core._confirmBlacklistMenu.Add(new UIText("this level in the future?", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        _core._confirmBlacklistMenu.Add(new UIText("", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        _core._confirmBlacklistMenu.Add(new UIMenuItem("|DGRED|@blacklist@YES!", new UIMenuActionCloseMenuCallFunction(_ducknetUIGroup, GameMode.Blacklist)));
        _core._confirmBlacklistMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenu(_core._confirmBlacklistMenu, _core._ducknetMenu), UIAlign.Center, default(Color), backButton: true));
        _core._confirmBlacklistMenu.Close();
        _ducknetUIGroup.Add(_core._confirmBlacklistMenu, doAnchor: false);
        Main.SpecialCode = "men17";
        _core._confirmKick.Add(new UIMenuItem("NO!", new UIMenuActionOpenMenu(_core._confirmKick, _core._ducknetMenu), UIAlign.Left, default(Color), backButton: true));
        _core._confirmKick.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuCallFunction(_ducknetUIGroup, KickedPlayer)));
        _core._confirmKick.Close();
        _ducknetUIGroup.Add(_core._confirmKick, doAnchor: false);
        _core._confirmBan.Add(new UIMenuItem("NO!", new UIMenuActionOpenMenu(_core._confirmBan, _core._ducknetMenu), UIAlign.Left, default(Color), backButton: true));
        _core._confirmBan.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuCallFunction(_ducknetUIGroup, BannedPlayer)));
        _core._confirmBan.Close();
        _ducknetUIGroup.Add(_core._confirmBan, doAnchor: false);
        Main.SpecialCode = "men18";
        _core._confirmBlock.Add(new UIText("I'm sorry it's come to this :(", Colors.DGBlue));
        _core._confirmBlock.Add(new UIText("", Color.White));
        _core._confirmBlock.Add(new UIText("Blocking this player will", Colors.DGBlue));
        _core._confirmBlock.Add(new UIText("stop their interactions with", Colors.DGBlue));
        _core._confirmBlock.Add(new UIText("you and prevent them from", Colors.DGBlue));
        _core._confirmBlock.Add(new UIText("joining your games in the future.", Colors.DGBlue));
        _core._confirmBlock.Add(new UIText("", Color.White));
        _core._confirmBlock.Add(new UIText("Are you sure?", Colors.DGBlue));
        _core._confirmBlock.Add(new UIText("", Color.White));
        _core._confirmBlock.Add(new UIMenuItem("NO!", new UIMenuActionOpenMenu(_core._confirmBlock, _core._ducknetMenu), UIAlign.Center, default(Color), backButton: true));
        _core._confirmBlock.Add(new UIMenuItem("YES, PLEASE BLOCK THEM!", new UIMenuActionCloseMenuCallFunction(_ducknetUIGroup, BlockedPlayer)));
        _core._confirmBlock.Close();
        _ducknetUIGroup.Add(_core._confirmBlock, doAnchor: false);
        Main.SpecialCode = "men19";
        _core._optionsMenu.SetBackFunction(new UIMenuActionOpenMenuCallFunction(_core._optionsMenu, _core._ducknetMenu, Options.OptionsMenuClosed));
        _core._optionsMenu.Close();
        _ducknetUIGroup.Add(_core._optionsMenu, doAnchor: false);
        _ducknetUIGroup.Add(Options._lastCreatedGraphicsMenu, doAnchor: false);
        _ducknetUIGroup.Add(Options._lastCreatedAudioMenu, doAnchor: false);
        if (Options._lastCreatedAccessibilityMenu != null)
        {
            _ducknetUIGroup.Add(Options._lastCreatedAccessibilityMenu, doAnchor: false);
        }
        if (Options._lastCreatedTTSMenu != null)
        {
            _ducknetUIGroup.Add(Options._lastCreatedTTSMenu, doAnchor: false);
        }
        if (Options._lastCreatedBlockMenu != null)
        {
            _ducknetUIGroup.Add(Options._lastCreatedBlockMenu, doAnchor: false);
        }
        if (Options._lastCreatedControlsMenu != null)
        {
            _ducknetUIGroup.Add(Options._lastCreatedControlsMenu, doAnchor: false);
        }
        Main.SpecialCode = "men20";
        _ducknetUIGroup.Close();
        Level.Add(_ducknetUIGroup);
        _ducknetUIGroup.Update();
        _ducknetUIGroup.Update();
        _ducknetUIGroup.Update();
        _ducknetUIGroup.Open();
        _core._ducknetMenu.Open();
        MonoMain.pauseMenu = _ducknetUIGroup;
        HUD.AddCornerControl(HUDCorner.BottomRight, "@CHAT@CHAT");
        _core._pauseOpen = true;
        SFX.Play("pause", 0.6f);
    }

    public static void SendCurrentLevelData(ushort session, NetworkConnection c)
    {
        int maxPacketSizeInBytes = 512;
        MemoryStream data = compressedLevelData;
        long dataSize = data.Length;
        int dataPosition = 0;
        Math.Ceiling((float)dataSize / (float)maxPacketSizeInBytes);
        data.Position = 0L;
        Send.Message(new NMLevelDataHeader(session, (int)dataSize, compressedLevelName), c);
        while (dataPosition != dataSize)
        {
            BitBuffer chunk = new BitBuffer();
            _ = new byte[maxPacketSizeInBytes];
            int writeBytes = (int)Math.Min(dataSize - dataPosition, maxPacketSizeInBytes);
            chunk.Write(data.GetBuffer(), dataPosition, writeBytes);
            dataPosition += writeBytes;
            Send.Message(new NMLevelDataChunk(session, chunk), c);
        }
    }

    private static void OpenTeamSwitchDialogue(Profile p)
    {
        if (_uhOhGroup != null && _uhOhGroup.open)
        {
            return;
        }
        if (_uhOhGroup != null)
        {
            Level.Remove(_uhOhGroup);
        }
        ClearTeam(p);
        _uhOhGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        _uhOhMenu = new UIMenu("UH OH", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 210f, -1f, "@SELECT@OK");
        float width = 190f;
        string text = "The host isn't allowing teams, and someone else is already wearing your hat :(";
        string curText = "";
        string nextWord = "";
        while (true)
        {
            if (text.Length > 0 && text[0] != ' ')
            {
                nextWord += text[0];
            }
            else
            {
                if ((float)((curText.Length + nextWord.Length) * 8) > width)
                {
                    _uhOhMenu.Add(new UIText(curText, Color.White, UIAlign.Left));
                    curText = "";
                }
                if (curText.Length > 0)
                {
                    curText += " ";
                }
                curText += nextWord;
                nextWord = "";
            }
            if (text.Length == 0)
            {
                break;
            }
            text = text.Remove(0, 1);
        }
        if (nextWord.Length > 0)
        {
            if (curText.Length > 0)
            {
                curText += " ";
            }
            curText += nextWord;
        }
        if (curText.Length > 0)
        {
            _uhOhMenu.Add(new UIText(curText, Color.White, UIAlign.Left));
            curText = "";
        }
        _uhOhMenu.Add(new UIText(" ", Color.White));
        _uhOhMenu.Add(new UIMenuItem("OH DEAR", new UIMenuActionCloseMenu(_uhOhGroup), UIAlign.Center, Colors.MenuOption, backButton: true));
        _uhOhMenu.Close();
        _uhOhGroup.Add(_uhOhMenu, doAnchor: false);
        _uhOhGroup.Close();
        Level.Add(_uhOhGroup);
        _uhOhGroup.Open();
        _uhOhMenu.Open();
        MonoMain.pauseMenu = _uhOhGroup;
        SFX.Play("pause", 0.6f);
    }

    public static void ClearTeam(Profile p)
    {
        if (Level.current is TeamSelect2)
        {
            (Level.current as TeamSelect2).ClearTeam(p.networkIndex);
        }
    }

    public static bool OnTeamSwitch(Profile p)
    {
        if (TeamSelect2.GetSettingBool("teams"))
        {
            return true;
        }
        Team team = p.team;
        bool success = true;
        if (team != null)
        {
            foreach (Profile pro in profiles)
            {
                if (p.connection != null && p != pro && pro.team == p.team && pro.slotType != SlotType.Spectator && Network.isServer)
                {
                    if (p.connection == localConnection)
                    {
                        OpenTeamSwitchDialogue(p);
                    }
                    else
                    {
                        Send.Message(new NMTeamSetDenied(p, p.team), p.connection);
                    }
                    success = false;
                    return success;
                }
            }
        }
        return success;
    }

    private static string GetNameForIndex(string pOriginalName, int pIndex)
    {
        if (pIndex == 0)
        {
            return pOriginalName;
        }
        if (pOriginalName.Length > 14)
        {
            pOriginalName = pOriginalName.Substring(0, 14);
        }
        pOriginalName = pOriginalName + "(" + (pIndex + 1) + ")";
        return pOriginalName;
    }

    private static void SendJoinMessage(NetworkConnection c)
    {
        invited = false;
        ulong joinID = 0uL;
        if (Steam.lobby != null && NCSteam.inviteLobbyID != 0L && NCSteam.inviteLobbyID == Steam.lobby.id)
        {
            invited = true;
        }
        NCSteam.inviteLobbyID = 0uL;
        if (!Network.lanMode)
        {
            joinID = DG.localID;
        }
        List<string> names = new List<string>();
        List<byte> personas = new List<byte>();
        int num = 1;
        foreach (MatchmakingPlayer p in UIMatchmakingBox.core.matchmakingProfiles)
        {
            if (p.masterProfile != null)
            {
                names.Add(GetNameForIndex(p.masterProfile.name, num));
                num++;
            }
            else
            {
                names.Add(p.originallySelectedProfile.name);
            }
            personas.Add((byte)p.persona.index);
        }
        Send.Message(new NMRequestJoin(names, personas, invited, core.serverPassword, joinID), NetMessagePriority.ReliableOrdered, c);
    }

    private static void ClearAllNetworkData()
    {
        Network.Terminate();
    }

    private static void EnsureMatchmakingProfileExperienceProfile(bool pLan)
    {
        if (pLan)
        {
            return;
        }
        foreach (MatchmakingPlayer p in UIMatchmakingBox.core.matchmakingProfiles)
        {
            if (p.isMaster && p.originallySelectedProfile != Profiles.experienceProfile)
            {
                Profile replace = p.originallySelectedProfile;
                Profiles.experienceProfile.team = replace.team;
                Profiles.experienceProfile.inputProfile = replace.inputProfile;
                replace.team = null;
                replace.inputProfile = null;
                p.originallySelectedProfile = Profiles.experienceProfile;
            }
        }
    }

    public static void Host(int maxPlayers, NetworkLobbyType lobbyType, bool useCurrentSettings = false)
    {
        isDedicatedServer = false;
        invited = false;
        if (lobbyType == NetworkLobbyType.LAN || lobbyType == NetworkLobbyType.Invisible)
        {
            Network.lanMode = true;
        }
        else
        {
            Network.lanMode = false;
        }
        _core.serverPassword = (string)TeamSelect2.GetOnlineSetting("password").value;
        if (_core.status == DuckNetStatus.Disconnected && Network.connections.Count == 0)
        {
            DevConsole.Log(DCSection.DuckNet, "|LIME|Hosting new server. ");
            EnsureMatchmakingProfileExperienceProfile(Network.lanMode);
            Reset();
            foreach (Profile universalProfile in Profiles.universalProfileList)
            {
                universalProfile.team = null;
            }
            if (!useCurrentSettings)
            {
                TeamSelect2.DefaultSettings();
            }
            int port = TeamSelect2.GetLANPort();
            Network.HostServer(lobbyType, maxPlayers, (string)TeamSelect2.GetOnlineSetting("name").value, port);
            localConnection.BeginConnection();
            preparingProfiles = true;
            int idx = 0;
            foreach (Profile p in profiles)
            {
                switch (lobbyType)
                {
                    case NetworkLobbyType.Private:
                        p.slotType = SlotType.Invite;
                        break;
                    case NetworkLobbyType.FriendsOnly:
                        p.slotType = SlotType.Friend;
                        break;
                    default:
                        p.slotType = SlotType.Open;
                        break;
                }
                if (idx >= maxPlayers)
                {
                    p.slotType = SlotType.Closed;
                }
                if (idx >= DG.MaxPlayers)
                {
                    p.slotType = SlotType.Spectator;
                }
                p.originalSlotType = p.slotType;
                idx++;
            }
            preparingProfiles = false;
            ChangeSlotSettings();
            int numJoin = 1;
            foreach (MatchmakingPlayer localProfile in UIMatchmakingBox.core.matchmakingProfiles)
            {
                if (localProfile.spectator)
                {
                    isDedicatedServer = true;
                }
                string localName = Network.activeNetwork.core.GetLocalName();
                if (Network.activeNetwork.core is NCBasic)
                {
                    localName = (NCBasic._localName = localProfile.originallySelectedProfile.name);
                }
                ServerCreateProfile(_core.localConnection, localProfile.inputProfile, localProfile.originallySelectedProfile, localName, pLocal: true, pWasInvited: false, localProfile.spectator).persona = localProfile.persona;
                numJoin++;
            }
            _core.localConnection.levelIndex = 0;
            _core.localConnection.isHost = true;
            _core.status = DuckNetStatus.ConnectingToServer;
        }
        else
        {
            DevConsole.Log(DCSection.DuckNet, "@error !!DuckNetwork.Host called while still connected!!@error");
        }
    }

    public static void Join(string id, string ip = "localhost")
    {
        Join(id, ip, "");
    }

    public static void Join(string id, string ip, string pPassword)
    {
        isDedicatedServer = false;
        if (pPassword == null)
        {
            pPassword = "";
        }
        core.serverPassword = pPassword;
        if (_core.status == DuckNetStatus.Disconnected && Network.connections.Count == 0)
        {
            if (ip != "localhost")
            {
                Network.lanMode = true;
            }
            else
            {
                Network.lanMode = false;
            }
            DevConsole.Log(DCSection.DuckNet, "|LIME|Attempting to join " + id);
            EnsureMatchmakingProfileExperienceProfile(Network.lanMode);
            Reset();
            foreach (Profile universalProfile in Profiles.universalProfileList)
            {
                universalProfile.team = null;
            }
            for (int i = 0; i < DG.MaxPlayers; i++)
            {
                Teams.all[i].customData = null;
            }
            TeamSelect2.DefaultSettings();
            Network.JoinServer(id, 1337 + joinPort, ip);
            localConnection.BeginConnection();
            _core.status = DuckNetStatus.EstablishingCommunicationWithServer;
        }
        else
        {
            DevConsole.Log(DCSection.DuckNet, "@error !!DuckNetwork.Join called while still connected!!@error");
        }
    }

    public static Profile JoinLocalDuck(InputProfile pInput)
    {
        if (Network.isServer)
        {
            foreach (Profile pro in profiles)
            {
                if (pro.team != null && pro.inputProfile == pInput)
                {
                    return pro;
                }
            }
            int index = profiles.Where((Profile x) => x.connection == localConnection).Count();
            string localName = Network.activeNetwork.core.GetLocalName();
            if (index > 0)
            {
                localName = localName + "(" + (index + 1) + ")";
            }
            Profile localPro = null;
            if (pInput.mpIndex >= 0)
            {
                localPro = Profile.defaultProfileMappings[pInput.mpIndex];
            }
            Profile newProfile = ServerCreateProfile(localConnection, pInput, localPro, localName, pLocal: true, pWasInvited: false, pSpectator: false);
            if (newProfile == null)
            {
                return null;
            }
            newProfile.isRemoteLocalDuck = true;
            Level.current.OnNetworkConnecting(newProfile);
            Server_AcceptJoinRequest(new List<Profile> { newProfile }, pLocal: true);
            return newProfile;
        }
        return null;
    }

    private static IEnumerable<Profile> GetOpenProfiles(NetworkConnection pConnection, bool pInvited, bool pLocal, bool pSpectator)
    {
        bool pFriend = false;
        if (pConnection.data is User && (pConnection.data as User).id != 0L)
        {
            if (_core._invitedFriends.Contains((pConnection.data as User).id))
            {
                pInvited = true;
            }
            if ((pConnection.data as User).relationship == FriendRelationship.Friend)
            {
                pFriend = true;
            }
        }
        IEnumerable<Profile> available = profiles.Where((Profile x) => x.connection == null && x.reservedUser != null && pConnection.data == x.reservedUser);
        if (available.Count() == 0)
        {
            available = ((!pSpectator) ? profiles.Where((Profile x) => x.connection == null && ((x.slotType == SlotType.Invite && (pInvited || pLocal)) || (x.slotType == SlotType.Friend && (pFriend || pLocal)) || (pLocal && x.slotType == SlotType.Local) || x.slotType == SlotType.Open) && x.slotType != SlotType.Spectator && x.networkIndex <= 7) : profiles.Where((Profile x) => x.connection == null && x.slotType == SlotType.Spectator));
        }
        return available;
    }

    private static Profile ServerCreateProfile(NetworkConnection pConnection, InputProfile pInput, Profile pLocalProfile, string pName, bool pLocal, bool pWasInvited, bool pSpectator)
    {
        Profile profile = GetOpenProfiles(pConnection, pWasInvited, pLocal, pSpectator).FirstOrDefault();
        if (profile == null)
        {
            return null;
        }
        PrepareProfile(profile, pConnection, pInput, pLocalProfile, pName);
        profile.invited = pWasInvited;
        return profile;
    }

    private static void PrepareProfile(Profile pProfile, NetworkConnection pConnection, InputProfile pLocalInput, Profile pLocalProfile, string pName)
    {
        DevConsole.Log(DCSection.DuckNet, "PrepareProfile(" + pConnection.ToString() + ", " + pName + "," + pProfile.networkIndex + ")");
        Team reservedTeam = pProfile.reservedTeam;
        sbyte reservedSpectatorPersona = pProfile.reservedSpectatorPersona;
        ResetProfile(pProfile);
        pProfile.linkedProfile = pLocalProfile;
        if (pConnection.profile == null)
        {
            pConnection.profile = pProfile;
            pConnection.name = pName;
        }
        pProfile.connection = pConnection;
        pProfile.name = pName;
        if (pConnection == localConnection)
        {
            if (_core.localProfile == null)
            {
                _core.localProfile = pProfile;
            }
            if (Network.isServer && _core.hostProfile == null)
            {
                _core.hostProfile = pProfile;
            }
            pProfile.networkStatus = DuckNetStatus.Connected;
            if (!Network.lanMode)
            {
                pProfile.steamID = DG.localID;
            }
            if (profiles.Where((Profile x) => x.connection == pConnection).Count() > 1)
            {
                pProfile.slotType = SlotType.Local;
                pProfile.isRemoteLocalDuck = true;
            }
            pProfile.flagIndex = Global.data.flag;
            pProfile.inputProfile = pLocalInput;
        }
        else
        {
            pProfile.inputProfile = InputProfile.GetVirtualInput(pProfile.networkIndex);
        }
        if (reservedTeam != null)
        {
            pProfile.team = reservedTeam;
        }
        else
        {
            pProfile.team = pProfile.networkDefaultTeam;
        }
        pProfile.reservedUser = null;
        pProfile.reservedTeam = null;
        if (pProfile.slotType == SlotType.Reserved)
        {
            pProfile.slotType = SlotType.Invite;
        }
        pProfile.persona = pProfile.networkDefaultPersona;
        if (reservedSpectatorPersona != -1)
        {
            pProfile.netData.Set("spectatorPersona", reservedSpectatorPersona);
            pProfile.reservedSpectatorPersona = reservedSpectatorPersona;
        }
        if (pProfile.steamID != 0L && !Options.Data.recentPlayers.Contains(pProfile.steamID))
        {
            Options.Data.recentPlayers.Insert(0, pProfile.steamID);
            while (Options.Data.recentPlayers.Count > 10)
            {
                Options.Data.recentPlayers.RemoveAt(Options.Data.recentPlayers.Count - 1);
            }
            Options.flagForSave = 60;
        }
    }

    public static void Reset()
    {
        foreach (Profile profile in profiles)
        {
            ResetProfile(profile, reserve: false);
        }
        Level.core.gameInProgress = false;
        _core._invitedFriends.Clear();
        Main.ResetGameStuff();
        Main.ResetMatchStuff();
        _core.RecreateProfiles();
        _core.hostProfile = null;
        _core.localProfile = null;
        DataLayerDebug.BadConnection bad = null;
        if (_core.localConnection != null)
        {
            bad = _core.localConnection.debuggerContext;
        }
        _core.localConnection = new NetworkConnection(null);
        _core.localConnection.SetDebuggerContext(bad);
        _core.inGame = false;
        _core.status = DuckNetStatus.Disconnected;
        _core.levelTransferSession = 0;
        _core.levelTransferProgress = 0;
        _core.levelTransferSize = 0;
        _core.enteringText = false;
        _core.localConnection.levelIndex = byte.MaxValue;
        DevConsole.Log(DCSection.DuckNet, "|LIME|----------------Duck Network has been RESET----------------");
    }

    public static void ResetProfile(Profile p)
    {
        ResetProfile(p, reserve: false);
    }

    public static void ResetProfile(Profile p, bool reserve)
    {
        DevConsole.Log(DCSection.DuckNet, "|DGRED|Resetting profile (" + ((p.connection != null) ? p.connection.ToString() : "null") + ")");
        if (p.connection != null && p.connection.profile == p)
        {
            if (p.connection != localConnection)
            {
                Level.current.OnNetworkDisconnected(p);
            }
            p.connection.profile = null;
        }
        p.reservedUser = ((!reserve) ? null : ((p.connection != null) ? p.connection.data : null));
        if (p.team == null || p.IndexOfCustomTeam(p.team) > 0)
        {
            p.reservedTeam = null;
        }
        else
        {
            p.reservedTeam = (reserve ? p.team : null);
        }
        if (p.persona != null)
        {
            p.reservedSpectatorPersona = (sbyte)(reserve ? ((sbyte)p.persona.index) : (-1));
        }
        p.netData = new ProfileNetData();
        p.removedGhosts.Clear();
        byte spectatorChangeIndex = (p.remoteSpectatorChangeIndex = 0);
        p.spectatorChangeIndex = spectatorChangeIndex;
        p.connection = null;
        p.team = null;
        p.networkStatus = DuckNetStatus.Disconnected;
        p.flippers = 0;
        p._blockStatusDirty = true;
        p.linkedProfile = null;
        p.preferredColor = -1;
        p.furniturePositions.Clear();
        p.ParentalControlsActive = false;
        p.flagIndex = -1;
        p.numClientCustomLevels = 0;
        p.keepSetName = false;
        p.customTeams.Clear();
        p.networkHatUnlockStatuses = null;
        p.isRemoteLocalDuck = false;
        p.steamID = 0uL;
        p.duck = null;
        p.inputProfile = InputProfile.GetVirtualInput(p.networkIndex);
        if (p.inputProfile != null)
        {
            if (p.inputProfile.virtualDevice != null)
            {
                p.inputProfile.virtualDevice.SetState(0);
                p.inputProfile.virtualDevice.SetState(0);
            }
            p.inputProfile.lastActiveOverride = null;
            p.inputMappingOverrides.Clear();
        }
    }

    public static void OnDisconnect(NetworkConnection connection, string reason, bool kicked = false)
    {
        if (_core.localProfile == null || connection == localConnection)
        {
            if (connection.isHost)
            {
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.EveryoneDisconnected, "Could not connect to server."));
            }
            return;
        }
        if (status == DuckNetStatus.ConnectingToClients && NCNetworkImplementation.currentError != null && NCNetworkImplementation.currentError.error == DuckNetError.ConnectionTimeout)
        {
            Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.EveryoneDisconnected, "Could not connect to: {\n\n\n" + connection.name));
            return;
        }
        if (connection.isHost && kicked)
        {
            Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.Kicked, "|RED|Oh no! The host kicked you :("));
            return;
        }
        if (!kicked)
        {
            byte lowestNetIndex = 99;
            NetworkConnection lowestIndexConnection = null;
            byte myNetIndex = 99;
            if (hostProfile == null)
            {
                DevConsole.Log(DCSection.General, "Host changed but I'm still in the process of connecting.  This is bad since I have incomplete data, disconnecting.");
                Level.current = new DisconnectFromGame();
                return;
            }
            if (hostProfile.connection == connection)
            {
                foreach (Profile p in core.profiles)
                {
                    if (!p.isRemoteLocalDuck)
                    {
                        if (p.connection != null && p.connection != connection && p.networkIndex < lowestNetIndex)
                        {
                            lowestNetIndex = p.networkIndex;
                            lowestIndexConnection = p.connection;
                        }
                        if (p.connection == localConnection && p.networkIndex < myNetIndex)
                        {
                            myNetIndex = p.networkIndex;
                        }
                    }
                }
                if (lowestIndexConnection == null)
                {
                    throw new Exception("DuckNetwork.OnDisconnect index failure!");
                }
                OnHostChange(lowestNetIndex, myNetIndex <= lowestNetIndex);
            }
        }
        List<Profile> list = GetProfiles(connection);
        bool hadProfile = false;
        foreach (Profile p2 in list)
        {
            hadProfile = true;
            if (p2.connection != localConnection)
            {
                if (Network.isServer)
                {
                    if (MonoMain.closingGame)
                    {
                        SendToEveryone(new NMClientClosedGame());
                    }
                    else
                    {
                        SendToEveryone(new NMClientDisconnect(p2.connection.identifier, p2));
                    }
                }
                bool reserve = false;
                if (Network.InMatch())
                {
                    if (reason == "CRASH")
                    {
                        HUD.AddPlayerChangeDisplay("@UNPLUG@|RED|" + p2.nameUI + " Crashed!");
                    }
                    else if (reason == "CLOSED")
                    {
                        HUD.AddPlayerChangeDisplay("@UNPLUG@|RED|" + p2.nameUI + " Closed Duck Game!");
                    }
                    else
                    {
                        HUD.AddPlayerChangeDisplay("@UNPLUG@|RED|" + p2.nameUI + " Disconnected!");
                    }
                    if (p2.slotType != SlotType.Open && !kicked)
                    {
                        reserve = true;
                    }
                }
                DevConsole.Log(DCSection.DuckNet, p2.nameUI + " |RED|Has left the DuckNet.");
                _ = p2.networkStatus;
                ResetProfile(p2, reserve);
                if (reserve && p2.slotType != SlotType.Spectator)
                {
                    p2.slotType = SlotType.Reserved;
                    ChangeSlotSettings();
                }
                if (Level.core.gameInProgress && Network.InLobby())
                {
                    if (p2.slotType != SlotType.Spectator)
                    {
                        p2.slotType = SlotType.Closed;
                    }
                    ChangeSlotSettings();
                }
            }
            else
            {
                p2.networkStatus = DuckNetStatus.Disconnected;
            }
        }
        if (status == DuckNetStatus.ConnectingToClients && hostProfile != null && hostProfile.connection != null)
        {
            CheckConnectingStatus();
        }
        if (hadProfile && status != DuckNetStatus.Disconnecting && status != DuckNetStatus.Disconnected && Network.isServer && !(Level.current is TeamSelect2) && (Level.current._waitingOnTransition || !Level.current._startCalled))
        {
            Level.current = new GameLevel(Deathmatch.RandomLevelString(GameMode.previousLevel));
        }
    }

    public static bool IsEstablishingConnection()
    {
        if (status != DuckNetStatus.EstablishingCommunicationWithServer && status != DuckNetStatus.ConnectingToServer)
        {
            return status == DuckNetStatus.ConnectingToClients;
        }
        return true;
    }

    public static void OnHostChange(ulong pStationID, bool pLocal)
    {
        if (IsEstablishingConnection())
        {
            DevConsole.Log(DCSection.General, "|DGRED|Host changed but I'm still in the process of connecting.  This is bad since I have incomplete data, disconnecting.");
            Network.DisconnectClient(localConnection, new DuckNetErrorInfo(DuckNetError.ConnectionTimeout, "Host disconnected."));
            return;
        }
        if (status == DuckNetStatus.Disconnected || status == DuckNetStatus.Disconnecting)
        {
            DevConsole.Log(DCSection.General, "|DGRED|Skipping host migration (DuckNetwork.status = " + status.ToString() + ").");
            return;
        }
        bool isServer = (Network.isServer = pLocal);
        bool assignedNewHost = false;
        foreach (Profile p in core.profiles)
        {
            if (p.connection != null)
            {
                p.connection.isHost = false;
                if (p.networkIndex == pStationID || (pLocal && p.connection == localConnection))
                {
                    core.hostProfile = p;
                    p.connection.isHost = true;
                    DevConsole.Log(DCSection.NetCore, "Host migrating, new host is " + p.connection?.ToString() + (isServer ? "(local)" : "(remote)"));
                    assignedNewHost = true;
                }
            }
        }
        if (!assignedNewHost)
        {
            DevConsole.Log(DCSection.NetCore, "@error@|DGRED|Host migration failed to find new host connection (" + pStationID + ")");
            Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.HostDisconnected, "Game Over: migration failed!"));
        }
        else
        {
            if (!Network.isServer)
            {
                return;
            }
            if ((Level.current._waitingOnTransition || !Level.current._startCalled) && !(Level.current is TeamSelect2))
            {
                Level.current = new GameLevel(Deathmatch.RandomLevelString(GameMode.previousLevel));
            }
            if (Level.current is RockScoreboard { controlMessage: var oldValue } scoreboardLevel)
            {
                if (oldValue > 0)
                {
                    scoreboardLevel.controlMessage = -1;
                    scoreboardLevel.controlMessage = oldValue;
                }
                Thing.Fondle(scoreboardLevel.netCountdown, localConnection);
            }
        }
    }

    public static void OnSessionEnded()
    {
        DevConsole.Log(DCSection.DuckNet, "----------------Duck Network Session ENDED----------------");
        _core.enteringText = false;
        Reset();
    }

    private static void CheckConnectingStatus()
    {
        if (status != DuckNetStatus.ConnectingToClients || hostProfile == null)
        {
            return;
        }
        bool allConnected = true;
        foreach (Profile p in profiles)
        {
            if (p.connection != null && p.connection.status != ConnectionStatus.Connected)
            {
                allConnected = false;
            }
        }
        if (allConnected)
        {
            Send.Message(new NMAllClientsConnected(), hostProfile.connection);
        }
    }

    public static void OnConnection(NetworkConnection c)
    {
        if (status == DuckNetStatus.EstablishingCommunicationWithServer)
        {
            if (c.isHost)
            {
                DevConsole.Log(DCSection.DuckNet, "Host contacted. Sending join request. ");
                _core.status = DuckNetStatus.ConnectingToServer;
                SendJoinMessage(c);
            }
        }
        else if (status == DuckNetStatus.ConnectingToClients)
        {
            CheckConnectingStatus();
        }
        else if (c.isHost)
        {
            DevConsole.Log(DCSection.DuckNet, "@error Host contacted. Join request not sent due to wrong status (" + status.ToString() + ")@error");
        }
    }

    public static void Update()
    {
        if (MonoMain.pauseMenu == null && _core._pauseOpen)
        {
            HUD.CloseAllCorners();
            _core._pauseOpen = false;
            if (Network.isServer)
            {
                TeamSelect2.UpdateModifierStatus();
            }
        }
        if (MonoMain.pauseMenu == null && _core._willOpenSettingsInfo)
        {
            if (!ShouldKickForCustomContent())
            {
                HUD.AddPlayerChangeDisplay("@SETTINGSCHANGED@NEW MATCH SETTINGS");
            }
            _core._willOpenSettingsInfo = false;
        }
        if (MonoMain.pauseMenu == null && _core._willOpenSpectatorInfo > 0)
        {
            DoSpectatorOpen(_core._willOpenSpectatorInfo != 1);
            _core._willOpenSpectatorInfo = 0;
        }
        if (_core.status == DuckNetStatus.Disconnected || _core.status == DuckNetStatus.Disconnecting)
        {
            _core._quit.value = false;
            return;
        }
        if (_core._quit.value)
        {
            if (_core._menuOpenProfile != null && _core._menuOpenProfile.slotType == SlotType.Local)
            {
                Kick(_core._menuOpenProfile);
            }
            else
            {
                if (Steam.lobby != null)
                {
                    UIMatchmakingBox.core.nonPreferredServers.Add(Steam.lobby.id);
                }
                if (!finishedMatch)
                {
                    _xpEarned.Clear();
                }
                Level.current = new DisconnectFromGame();
            }
            _core._quit.value = false;
        }
        if (_core._returnToLobby.value && Network.isServer)
        {
            Level.current = new TeamSelect2(pReturningFromGame: true);
            _core._returnToLobby.value = false;
        }
        if (_core._menuClosed.value)
        {
            if (Level.current is TeamSelect2)
            {
                Send.Message(new NMNumCustomLevels(Editor.activatedLevels.Count));
            }
            if (Level.current is TeamSelect2)
            {
                if (TeamSelect2.GetMatchSettingString() != _core._settingsBeforeOpen)
                {
                    TeamSelect2.SendMatchSettings();
                    Send.Message(new NMMatchSettingsChanged());
                }
                Network.activeNetwork.core.ApplyLobbyData();
            }
            _core._menuClosed.value = false;
        }
        if (Keyboard.Pressed(Keys.F1) && !Keyboard.Down(Keys.LeftShift) && !Keyboard.Down(Keys.RightShift))
        {
            ConnectionStatusUI.Show();
        }
        if (core.logTransferSize > 0)
        {
            ConnectionStatusUI.core.tempShow = 2;
        }
        if (Keyboard.Released(Keys.F1))
        {
            ConnectionStatusUI.Hide();
        }
        bool noPauseMenu = MonoMain.pauseMenu == null || MonoMain.pauseMenu is UILevelBox || MonoMain.pauseMenu is UIGachaBoxNew || MonoMain.pauseMenu is UIFuneral || MonoMain.pauseMenu is UIGachaBox;
        if (!noPauseMenu)
        {
            _core.enteringText = false;
            _core.stopEnteringText = false;
        }
        bool skipPause = false;
        if (Network.isActive && noPauseMenu)
        {
            List<ChatMessage> remove = new List<ChatMessage>();
            foreach (ChatMessage m in _core.chatMessages)
            {
                m.timeout -= 0.016f;
                if (m.timeout < 0f)
                {
                    m.alpha -= 0.01f;
                }
                if (m.alpha < 0f)
                {
                    remove.Add(m);
                }
            }
            foreach (ChatMessage m2 in remove)
            {
                _core.chatMessages.Remove(m2);
            }
            if (_core.stopEnteringText)
            {
                _core.enteringText = false;
                _core.stopEnteringText = false;
            }
            if (!DevConsole.open)
            {
                bool entering = _core.enteringText;
                _core.enteringText = false;
                bool num = Input.Pressed("CHAT") && (!Keyboard.alt || !Keyboard.Pressed(Keys.Enter));
                _core.enteringText = entering;
                if (num)
                {
                    if (!_core.enteringText)
                    {
                        _core.enteringText = true;
                        _core.currentEnterText = "";
                        Keyboard.keyString = "";
                    }
                    else
                    {
                        if (_core.currentEnterText != "")
                        {
                            string sendString = _core.currentEnterText;
                            if (sendString.StartsWith("/steal"))
                            {
                                string[] parts = sendString.Split(':');
                                if (parts.Count() == 3)
                                {
                                    DuckFile.StealMoji(parts[1]);
                                }
                            }
                            else
                            {
                                try
                                {
                                    foreach (KeyValuePair<string, Sprite> pair in DuckFile.mojis)
                                    {
                                        if (pair.Key.EndsWith("!"))
                                        {
                                            continue;
                                        }
                                        string mojiString = ":" + pair.Key + ":";
                                        if (!sendString.Contains(mojiString))
                                        {
                                            mojiString = "@" + pair.Key + "@";
                                        }
                                        if (!sendString.Contains(mojiString))
                                        {
                                            continue;
                                        }
                                        foreach (Profile p in profiles)
                                        {
                                            if (p.connection != null && p.connection != localConnection && (!p.isHost || p == hostProfile) && !p.sentMojis.Contains(mojiString))
                                            {
                                                if (pair.Value.texture.width <= 28 && pair.Value.texture.height <= 28)
                                                {
                                                    Send.Message(new NMMojiData(Editor.TextureToString(pair.Value.texture), pair.Key), p.connection);
                                                }
                                                p.sentMojis.Add(mojiString);
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                }
                                NMChatMessage nMChatMessage = new NMChatMessage(_core.localProfile, sendString, _core.chatIndex);
                                _core.chatIndex++;
                                SendToEveryoneFiltered(nMChatMessage);
                                ChatMessageReceived(nMChatMessage, _core.currentEnterText);
                            }
                            _core.currentEnterText = "";
                        }
                        _core.stopEnteringText = true;
                    }
                }
                else if (_core.enteringText && Keyboard.Pressed(Keys.Escape))
                {
                    _core.stopEnteringText = true;
                    skipPause = true;
                }
            }
            if (_core.enteringText)
            {
                Input._imeAllowed = true;
                if (Keyboard.keyString.Length > 90)
                {
                    Keyboard.keyString = Keyboard.keyString.Substring(0, 90);
                }
                Keyboard.keyString = Keyboard.keyString.Replace("\n", "");
                _core.currentEnterText = Keyboard.keyString;
            }
        }
        bool openedMenu = false;
        int numConnected = 0;
        _processedConnections.Clear();
        Profile potentialNewHostProfile = null;
        foreach (Profile p2 in profiles)
        {
            if (p2.pendingSpectatorMode != SlotType.Max && Network.isServer && (Level.current is TeamSelect2 || VirtualTransition.active))
            {
                _spectatorSwaps.Add(p2);
            }
            if (NetworkDebugger.enabled && p2.inputProfile is BullshitInput)
            {
                p2.inputProfile.UpdateExtraInput();
            }
            p2.TickConnectionTrouble();
            if (p2.connection == localConnection && p2.inputProfile != null)
            {
                if (p2.duck == null || p2.duck.dead)
                {
                    if (p2.inputProfile.Pressed("QUACK"))
                    {
                        p2.netData.Set("quack", pValue: true);
                    }
                    else if (p2.inputProfile.Released("QUACK"))
                    {
                        p2.netData.Set("quack", pValue: false);
                    }
                }
                if (p2.slotType == SlotType.Spectator && (Level.current is TeamSelect2 || Level.current is RockScoreboard) && MonoMain.pauseMenu == null)
                {
                    float pitch = p2.inputProfile.leftTrigger;
                    if (p2.inputProfile.hasMotionAxis)
                    {
                        pitch += p2.inputProfile.motionAxis;
                    }
                    p2.netData.Set("quackPitch", pitch);
                    if (p2.inputProfile.Pressed("RAGDOLL"))
                    {
                        p2.netData.Set("spectatorHat", !p2.netData.Get("spectatorHat", pDefault: true));
                    }
                    if (p2.inputProfile.Pressed("STRAFE") && !p2.inputProfile.Down("RAGDOLL"))
                    {
                        p2.netData.Set("spectatorFlip", !p2.netData.Get("spectatorFlip", pDefault: false));
                    }
                    if (p2.inputProfile.Pressed("GRAB"))
                    {
                        p2.netData.Set("spectatorBeverage", (sbyte)(p2.netData.Get("spectatorBeverage", (sbyte)(-1)) + 1));
                        if (p2.netData.Get("spectatorBeverage", (sbyte)(-1)) > 13)
                        {
                            p2.netData.Set("spectatorBeverage", (sbyte)(-1));
                        }
                    }
                    if (p2.netData.Get("spectatorPersona", (sbyte)(-1)) == -1)
                    {
                        p2.netData.Set("spectatorPersona", (sbyte)p2.persona.index);
                    }
                    if ((p2.inputProfile.Down("RAGDOLL") && p2.inputProfile.Pressed("STRAFE")) || (p2.inputProfile.Down("STRAFE") && p2.inputProfile.Pressed("RAGDOLL")))
                    {
                        p2.netData.Set("spectatorPersona", (sbyte)(p2.netData.Get("spectatorPersona", (sbyte)(-1)) + 1));
                        if (p2.netData.Get("spectatorPersona", (sbyte)0) > 7)
                        {
                            p2.netData.Set("spectatorPersona", (sbyte)0);
                        }
                    }
                    p2.netData.Set("spectatorTongue", p2.inputProfile.rightStick);
                    Vector2 additionalTilt = Vector2.Zero;
                    if (p2.inputProfile.leftStick.Length() < 0.05f)
                    {
                        if (p2.inputProfile.Down("LEFT"))
                        {
                            additionalTilt += new Vector2(-1f, 0f);
                        }
                        if (p2.inputProfile.Down("RIGHT"))
                        {
                            additionalTilt += new Vector2(1f, 0f);
                        }
                        if (p2.inputProfile.Down("DOWN"))
                        {
                            additionalTilt += new Vector2(0f, -1f);
                        }
                        if (p2.inputProfile.Down("UP"))
                        {
                            additionalTilt += new Vector2(0f, 1f);
                        }
                    }
                    else
                    {
                        additionalTilt = p2.inputProfile.leftStick;
                    }
                    if (p2.inputProfile.Down("SHOOT"))
                    {
                        p2.netData.Set("spectatorBob", additionalTilt);
                    }
                    else
                    {
                        p2.netData.Set("spectatorTilt", additionalTilt);
                    }
                }
                p2.netData.Set("gamePaused", MonoMain.pauseMenu != null);
                p2.netData.Set("gameInFocus", Graphics.inFocus);
                p2.netData.SetFiltered("chatting", _core.enteringText);
                p2.netData.Set("consoleOpen", DevConsole.open);
                if (MonoMain.pauseMenu == null && (_ducknetUIGroup == null || !_ducknetUIGroup.open) && p2.inputProfile.Pressed("START") && !skipPause && !openedMenu && (Network.InLobby() || Network.InGameLevel()) && (!(Level.current is TeamSelect2) || !(Level.current as TeamSelect2).HasBoxOpen(p2)))
                {
                    OpenMenu(p2);
                    openedMenu = true;
                }
            }
            if (p2.connection != null && !_processedConnections.Contains(p2.connection))
            {
                _processedConnections.Add(p2.connection);
                if (Network.isServer && p2.connection != localConnection && p2.connection.status == ConnectionStatus.Connected && p2.networkStatus == DuckNetStatus.Connected && (potentialNewHostProfile == null || potentialNewHostProfile.networkIndex > p2.networkIndex))
                {
                    potentialNewHostProfile = p2;
                }
                numConnected++;
                if (p2.connection.status == ConnectionStatus.Connected && p2.networkStatus != DuckNetStatus.Disconnecting && p2.networkStatus != DuckNetStatus.Disconnected && p2.networkStatus != DuckNetStatus.Failure)
                {
                    p2.currentStatusTimeout -= Maths.IncFrameTimer();
                }
            }
        }
        if (potentialNewHostProfile != null)
        {
            potentialHostObject = potentialNewHostProfile.connection.data;
        }
        if (_spectatorSwaps.Count <= 0)
        {
            return;
        }
        if (Network.isServer)
        {
            foreach (Profile p3 in _spectatorSwaps)
            {
                Profile firstFree = null;
                if (p3.pendingSpectatorMode == SlotType.Spectator && p3.slotType != SlotType.Spectator)
                {
                    firstFree = profiles.FirstOrDefault((Profile x) => x.slotType == SlotType.Spectator && x.connection == null);
                    if (firstFree != null)
                    {
                        MakeSpectator_Swap(p3, firstFree);
                    }
                }
                else if (p3.pendingSpectatorMode == SlotType.Open && p3.slotType == SlotType.Spectator)
                {
                    firstFree = profiles.FirstOrDefault((Profile x) => x.slotType != SlotType.Spectator && x.slotType != SlotType.Reserved && x.slotType != SlotType.Closed && x.connection == null);
                    if (firstFree == null)
                    {
                        firstFree = profiles.FirstOrDefault((Profile x) => x.slotType != SlotType.Spectator && x.slotType != SlotType.Reserved && x.connection == null);
                    }
                    if (firstFree != null)
                    {
                        MakePlayer_Swap(p3, firstFree);
                    }
                }
                p3.pendingSpectatorMode = SlotType.Max;
            }
            TryPeacefulResolution();
        }
        _spectatorSwaps.Clear();
    }

    public static bool TryPeacefulResolution(bool pDoLevelSwitch = true)
    {
        if (Network.isActive && (!pDoLevelSwitch || (!(Level.current is TeamSelect2) && !(Level.current is IConnectionScreen) && !(Level.current is TitleScreen))))
        {
            int numTeamsActive = 0;
            foreach (Team item in Teams.all)
            {
                if (item.activeProfiles.Count > 0)
                {
                    numTeamsActive++;
                }
            }
            if (numTeamsActive <= 1)
            {
                if (pDoLevelSwitch && Network.isServer)
                {
                    bool hasReserved = false;
                    foreach (Profile profile in profiles)
                    {
                        if (profile.reservedUser != null)
                        {
                            hasReserved = true;
                        }
                    }
                    if (!hasReserved)
                    {
                        ResetScores();
                        foreach (Profile p in profiles)
                        {
                            if (p.slotType != SlotType.Spectator)
                            {
                                p.slotType = p.originalSlotType;
                            }
                        }
                    }
                    Level.current = new TeamSelect2(hasReserved);
                }
                return true;
            }
        }
        return false;
    }

    public static void ChatMessageReceived(NMChatMessage message)
    {
        ChatMessageReceived(message, null);
    }

    private static int FilterPlayer(Profile pProfile)
    {
        if (pProfile != null && pProfile.connection != localConnection && pProfile.connection != null)
        {
            int chatMode = Options.Data.chatMode;
            if (DG.di == 1)
            {
                chatMode = 0;
            }
            bool filter = false;
            bool muted = false;
            if (NetworkDebugger.enabled)
            {
                if (pProfile.muteChat)
                {
                    filter = true;
                    muted = true;
                }
            }
            else if (pProfile.connection.data is User && pProfile.muteChat)
            {
                filter = true;
                muted = true;
            }
            if (chatMode > 0)
            {
                bool friend = false;
                if (pProfile.connection.data is User { relationship: FriendRelationship.Friend })
                {
                    friend = true;
                }
                if (chatMode == 1 && !friend && !invited)
                {
                    filter = true;
                }
                else if (chatMode == 2 && !friend)
                {
                    filter = true;
                }
            }
            if (filter && muted)
            {
                return 2;
            }
            if (filter)
            {
                return 1;
            }
        }
        return 0;
    }

    public static void ChatMessageReceived(NMChatMessage message, string realText)
    {
        if (message.profile == null)
        {
            return;
        }
        int filter = FilterPlayer(message.profile);
        if (filter > 0)
        {
            if (message.connection.sentFilterMessage)
            {
                return;
            }
            message.connection.sentFilterMessage = true;
            realText = ((message is NMChatDisabledMessage) ? message.text : ((filter != 2) ? "@error@Chat |DGRED|disabled|PREV| in options. Ignoring messages..." : "@error@Player is |DGRED|muted|PREV|. Ignoring messages..."));
        }
        _core.AddChatMessage(new ChatMessage(message.profile, (realText != null) ? realText : message.text, message.index));
        SFX.Play("chatmessage", 0.8f, Rando.Float(-0.15f, 0.15f));
    }

    public static List<Profile> GetProfiles(NetworkConnection connection)
    {
        List<Profile> pz = new List<Profile>();
        foreach (Profile p in profiles)
        {
            if (p.connection == connection)
            {
                pz.Add(p);
            }
        }
        return pz;
    }

    public static List<Profile> GetProfiles(string identifier)
    {
        List<Profile> pz = new List<Profile>();
        foreach (Profile p in profiles)
        {
            if (p.connection != null && p.connection.identifier == identifier)
            {
                pz.Add(p);
            }
        }
        return pz;
    }

    public static int IndexOf(Profile p)
    {
        return profiles.IndexOf(p);
    }

    public static void SendSpecialHat(Team pTeam, NetworkConnection pTo)
    {
    }

    public static void SendAllPlayerMetaData(Profile who, NetworkConnection to)
    {
        if (who.team != null)
        {
            if (who.team.customData != null)
            {
                Send.Message(new NMSpecialHat(who.team, who, to.profile != null && to.profile.muteHat), to);
            }
            Send.Message(new NMSetTeam(who, who.team, who.team.customData != null), to);
        }
        if (who.furniturePositions.Count > 0)
        {
            Send.Message(new NMRoomData(who, who.furniturePositionData), to);
        }
        Send.Message(new NMProfileInfo(who, who.stats.unloyalFans, who.stats.loyalFans, who.ParentalControlsActive, who.flagIndex, (ushort)Teams.core.extraTeams.Count, Teams.core.teams), to);
        Send.Message(new NMNumCustomLevels(Editor.activatedLevels.Count), to);
        Send.Message(new NMOldAngles(who, Options.Data.oldAngleCode), to);
    }

    public static void Server_SendProfile(Profile p, NetworkConnection to)
    {
        Send.Message(new NMNewDuckNetConnection(p, (p.connection != localConnection) ? p.connection.identifier : (p.isRemoteLocalDuck ? "SERVERLOCAL" : "SERVER"), p.name, p.team, p.flippers, p.ParentalControlsActive, p.flagIndex, p.steamID, (byte)p.persona.index), to);
    }

    public static void Server_AcceptJoinRequest(List<Profile> pJoinedProfiles, bool pLocal = false)
    {
        if (UIMatchmakerMark2.instance != null)
        {
            UIMatchmakerMark2.instance.Hook_OnDucknetJoined();
        }
        Send.Message(new NMNetworkIndexSync());
        if (!pLocal)
        {
            Send.Message(new NMJoinDuckNetSuccess(pJoinedProfiles), pJoinedProfiles[0].connection);
            List<byte> slots = new List<byte>();
            for (int i = 0; i < DG.MaxPlayers; i++)
            {
                slots.Add((byte)profiles[i].slotType);
            }
            TeamSelect2.SendMatchSettings(pJoinedProfiles[0].connection, initial: true);
            ChangeSlotSettings(pInitializingClient: true);
        }
        Server_SendAllProfiles(pJoinedProfiles, pLocal);
        foreach (Profile profile in GetProfiles(localConnection))
        {
            SendAllPlayerMetaData(profile, pJoinedProfiles[0].connection);
        }
        Send.Message(new NMEndOfDuckNetworkData(), pJoinedProfiles[0].connection);
    }

    public static void MakeSpectator(Profile pProfile)
    {
        pProfile.pendingSpectatorMode = SlotType.Spectator;
    }

    public static void MakeSpectator_Swap(Profile pProfile, Profile pReplacementSlot)
    {
        if (pProfile.slotType != SlotType.Spectator && pReplacementSlot.slotType == SlotType.Spectator && pProfile.networkIndex < DG.MaxPlayers)
        {
            pProfile.spectatorChangeIndex++;
            if (pProfile.connection == localConnection)
            {
                pProfile.remoteSpectatorChangeIndex = pProfile.spectatorChangeIndex;
            }
            if (Network.isServer)
            {
                Send.Message(new NMMakeSpectator(pProfile, pReplacementSlot, pProfile.spectatorChangeIndex));
            }
            if (Level.current is TeamSelect2)
            {
                ProfileBox2 profileBox = (Level.current as TeamSelect2)._profiles[pProfile.networkIndex];
                profileBox.Despawn();
                profileBox.SetProfile(pReplacementSlot);
            }
            SwapProfileIndices(pProfile, pReplacementSlot);
            pReplacementSlot.slotType = pProfile.slotType;
            pReplacementSlot.team = null;
            pReplacementSlot.duck = null;
            pReplacementSlot.persona = pReplacementSlot.networkDefaultPersona;
            pProfile.slotType = SlotType.Spectator;
            pProfile.team.Leave(pProfile, set: false);
            pProfile.duck = null;
        }
    }

    public static void MakePlayer(Profile pProfile)
    {
        pProfile.pendingSpectatorMode = SlotType.Open;
    }

    public static void MakePlayer_Swap(Profile pProfile, Profile pReplacementSlot)
    {
        if (pProfile.slotType == SlotType.Spectator && pReplacementSlot.slotType != SlotType.Spectator && pReplacementSlot.networkIndex < DG.MaxPlayers)
        {
            pProfile.spectatorChangeIndex++;
            if (Network.isServer)
            {
                Send.Message(new NMMakePlayer(pProfile, pReplacementSlot, pProfile.spectatorChangeIndex));
            }
            if (Level.current is TeamSelect2)
            {
                (Level.current as TeamSelect2)._profiles[pReplacementSlot.networkIndex].SetProfile(pProfile);
            }
            SwapProfileIndices(pProfile, pReplacementSlot);
            pProfile.slotType = pReplacementSlot.slotType;
            if (pProfile.team != null && (pProfile.team.activeProfiles.Count == 0 || TeamSelect2.GetSettingBool("teams")) && !pProfile.team.defaultTeam)
            {
                pProfile.team.Join(pProfile, set: false);
            }
            else
            {
                pProfile.team = pProfile.networkDefaultTeam;
            }
            pProfile.persona = pProfile.networkDefaultPersona;
            pProfile.duck = null;
            pReplacementSlot.slotType = SlotType.Spectator;
            pReplacementSlot.team = null;
            pReplacementSlot.duck = null;
        }
    }

    public static void SwapProfileIndices(Profile pProfile, Profile pReplacementSlot)
    {
        profiles[pProfile.networkIndex] = pReplacementSlot;
        profiles[pReplacementSlot.networkIndex] = pProfile;
        byte oldProfileIndex = pProfile.networkIndex;
        pProfile.SetNetworkIndex(pReplacementSlot.networkIndex);
        pReplacementSlot.SetNetworkIndex(oldProfileIndex);
    }

    public static void Server_SendAllProfiles(List<Profile> pTo, bool local = false)
    {
        foreach (Profile p in profiles)
        {
            if (p.networkStatus == DuckNetStatus.Disconnecting || pTo.Contains(p) || p.connection == null || (local && p.connection == localConnection))
            {
                continue;
            }
            if (!local)
            {
                Server_SendProfile(p, pTo[0].connection);
            }
            if (p.connection == localConnection)
            {
                continue;
            }
            foreach (Profile item in pTo)
            {
                Server_SendProfile(item, p.connection);
            }
        }
    }

    public static NMVersionMismatch.Type CheckVersion(string id)
    {
        string[] split = id.Split('.');
        NMVersionMismatch.Type type = NMVersionMismatch.Type.Match;
        if (split.Length == 4)
        {
            try
            {
                int verLow = Convert.ToInt32(split[3]);
                int verHigh = Convert.ToInt32(split[2]);
                int verMajor = Convert.ToInt32(split[1]);
                if (verHigh < DG.versionHigh || verMajor < DG.versionMajor)
                {
                    type = NMVersionMismatch.Type.Older;
                }
                else if (verHigh > DG.versionHigh || verMajor > DG.versionMajor)
                {
                    type = NMVersionMismatch.Type.Newer;
                }
                else if (verLow < DG.versionLow)
                {
                    type = NMVersionMismatch.Type.Older;
                }
                else if (verLow > DG.versionLow)
                {
                    type = NMVersionMismatch.Type.Newer;
                }
            }
            catch
            {
                type = NMVersionMismatch.Type.Error;
            }
        }
        return type;
    }

    public static NetMessage OnMessageFromNewClient(NetMessage m)
    {
        if (Network.isServer)
        {
            if (m is NMRequestJoin)
            {
                if (inGame || (!(Level.current is TeamSelect2) && !(Level.current is IConnectionScreen)))
                {
                    return new NMGameInProgress();
                }
                NMRequestJoin joinMessage = m as NMRequestJoin;
                if (joinMessage.names == null || joinMessage.names.Count == 0)
                {
                    return new NMErrorEmptyJoinMessage();
                }
                DevConsole.Log(DCSection.DuckNet, "Join attempt from " + joinMessage.names.First());
                if (GetOpenProfiles(m.connection, joinMessage.wasInvited, pLocal: false, pSpectator: false).Count() < joinMessage.names.Count)
                {
                    DevConsole.Log(DCSection.DuckNet, "@error " + joinMessage.names[0] + " could not join, server is full.@error");
                    return new NMServerFull();
                }
                if (joinMessage.password != core.serverPassword && !core._invitedFriends.Contains(joinMessage.localID))
                {
                    DevConsole.Log(DCSection.DuckNet, "@error " + joinMessage.names[0] + " could not join, password was incorrect.@error");
                    return new NMInvalidPassword();
                }
                List<Profile> joined = new List<Profile>();
                int count = 0;
                foreach (string iName in joinMessage.names)
                {
                    Profile joinProfile = ServerCreateProfile(m.connection, null, null, iName, pLocal: false, joinMessage.wasInvited, pSpectator: false);
                    joinProfile.ParentalControlsActive = joinMessage.info.parentalControlsActive;
                    joinProfile.flippers = joinMessage.info.roomFlippers;
                    joinProfile.flagIndex = joinMessage.info.flagIndex;
                    joinProfile.networkStatus = DuckNetStatus.Connected;
                    joinProfile.steamID = joinMessage.localID;
                    if (joinMessage.personas.Count > count)
                    {
                        byte persone = joinMessage.personas[count];
                        if (persone >= 0 && persone < Persona.all.Count())
                        {
                            joinProfile.preferredColor = persone;
                            RequestPersona(joinProfile, Persona.all.ElementAt(persone), pSendMessages: false);
                        }
                    }
                    _core.status = DuckNetStatus.Connected;
                    Level.current.OnNetworkConnecting(joinProfile);
                    joined.Add(joinProfile);
                    count++;
                }
                Server_AcceptJoinRequest(joined);
                return null;
            }
            if (m is NMMessageIgnored)
            {
                return null;
            }
        }
        else
        {
            if (m is NMRequestJoin)
            {
                DevConsole.Log(DCSection.DuckNet, "@error Received NMRequestJoin, but you're not the host!!.@error");
                return new NMGameInProgress();
            }
            if (m is NMMessageIgnored)
            {
                return null;
            }
        }
        return new NMMessageIgnored();
    }

    private static void AttemptReconnect(NetworkConnection fromConnection, string toConnection)
    {
        List<Profile> connectFrom = GetProfiles(fromConnection);
        if (connectFrom.Count > 0)
        {
            List<Profile> connectTo = GetProfiles(toConnection);
            foreach (Profile to in connectTo)
            {
                Server_SendProfile(to, fromConnection);
                if (to.connection != localConnection)
                {
                    foreach (Profile item in connectFrom)
                    {
                        Server_SendProfile(item, to.connection);
                    }
                }
                DevConsole.Log(DCSection.DuckNet, fromConnection.name + " needs a connection to " + to.connection.name + "...");
            }
            if (connectTo.Count == 0)
            {
                Send.Message(new NMNoConnectionExists(toConnection), fromConnection);
                DevConsole.Log(DCSection.DuckNet, "@error Client requested reconnect whith non-existing client!(to " + toConnection + ")@error", fromConnection);
            }
        }
        else
        {
            Send.Message(new NMInvalidUser(), fromConnection);
            DevConsole.Log(DCSection.DuckNet, "@error An outside user not in this game requested a reconnect!?@error", fromConnection);
        }
    }

    public static bool HandleCoreConnectionMessages(NetMessage m)
    {
        if (m is NMAllClientsConnected)
        {
            if (Network.isServer)
            {
                Send.Message(new NMRightOnManNiceJob(), m.connection);
                Send.Message(new NMLevel(Level.current), m.connection);
            }
        }
        else if (m is NMRightOnManNiceJob)
        {
            if (status == DuckNetStatus.ConnectingToClients)
            {
                _core.status = DuckNetStatus.Connected;
                if (UIMatchmakerMark2.instance != null)
                {
                    UIMatchmakerMark2.instance.Hook_OnDucknetJoined();
                }
            }
        }
        else
        {
            if (m is NMClientNeedsLevelData)
            {
                NMClientNeedsLevelData needsData = m as NMClientNeedsLevelData;
                if (needsData.levelIndex == levelIndex)
                {
                    m.connection.dataTransferProgress = 0;
                    m.connection.dataTransferSize = 0;
                    SendCurrentLevelData(needsData.transferSession, m.connection);
                    Level.current.ChecksumReplied(needsData.connection);
                }
                return true;
            }
            if (m is NMLevelFileReady)
            {
                NMLevelFileReady fileReady = m as NMLevelFileReady;
                if (Level.current != null && Level.current.networkIndex == fileReady.levelIndex)
                {
                    Level.current.ChecksumReplied(fileReady.connection);
                }
                return true;
            }
            if (m is NMLevelDataHeader)
            {
                NMLevelDataHeader header = m as NMLevelDataHeader;
                if (_core.levelTransferSession == header.transferSession)
                {
                    _core.levelTransferProgress = 0;
                    _core.levelTransferSize = header.length;
                    _currentTransferLevelName = header.levelName;
                }
                return true;
            }
            if (m is NMLevelDataChunk)
            {
                NMLevelDataChunk chunk = m as NMLevelDataChunk;
                if (_core.levelTransferSession == chunk.transferSession)
                {
                    _core.levelTransferProgress += chunk.GetBuffer().lengthInBytes;
                    if (_core.compressedLevelData == null)
                    {
                        _core.compressedLevelData = new MemoryStream();
                    }
                    _core.compressedLevelData.Write(chunk.GetBuffer().buffer, 0, chunk.GetBuffer().lengthInBytes);
                    if (_core.levelTransferProgress == _core.levelTransferSize)
                    {
                        XMLLevel apply = Level.core.nextLevel as XMLLevel;
                        if (apply == null)
                        {
                            apply = Level.core.currentLevel as XMLLevel;
                        }
                        if (apply == null)
                        {
                            DevConsole.Log("|DGRED|Tried to apply NMLevelDataChunk while connecting/disconnecting. Ignoring...");
                            return true;
                        }
                        apply.synchronizedLevelName = _currentTransferLevelName;
                        if (!apply.ApplyLevelData(Editor.ReadCompressedLevelData(_core.compressedLevelData)))
                        {
                            Network.DisconnectClient(localConnection, new DuckNetErrorInfo(DuckNetError.ParentalControls, "Disconnecting - Error storing custom map."));
                        }
                        apply.ChecksumReplied(chunk.connection);
                        if (!Network.isServer)
                        {
                            Send.Message(new NMLevelFileReady(apply.networkIndex), hostProfile.connection);
                        }
                    }
                }
                return true;
            }
        }
        if (m is NMChatMessage)
        {
            NMChatMessage obj = m as NMChatMessage;
            obj.index = _core.chatIndex;
            _core.chatIndex++;
            ChatMessageReceived(obj);
            return true;
        }
        if (Network.isServer)
        {
            if (m is NMRequiresNewConnection)
            {
                NMRequiresNewConnection newConnection = m as NMRequiresNewConnection;
                AttemptReconnect(newConnection.connection, newConnection.toWhom);
                return true;
            }
        }
        else if (m is NMKicked || m is NMBanned)
        {
            Profile p = profiles.FirstOrDefault((Profile x) => x == (m as NMKicked).profile);
            if (p != null)
            {
                string msg = "";
                if (p.connection != null)
                {
                    msg = "|DGRED|" + p.connection.ToString() + " has been " + ((m is NMKicked) ? "kicked" : "banned") + "...";
                    DevConsole.Log(DCSection.DuckNet, msg);
                    NetworkConnection c = p.connection;
                    if (m is NMBanned)
                    {
                        c.banned = true;
                    }
                    p.connection.kicking = true;
                    Network.DisconnectClient(p.connection, new DuckNetErrorInfo(DuckNetError.Kicked, ""));
                }
            }
        }
        else
        {
            if (m is NMKick)
            {
                _core.status = DuckNetStatus.Failure;
                if (Steam.lobby != null)
                {
                    UIMatchmakingBox.core.blacklist.Add(Steam.lobby.id);
                }
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.Kicked, "|RED|Oh no! The host kicked you :("));
                return true;
            }
            if (m is NMBan)
            {
                _core.status = DuckNetStatus.Failure;
                if (Steam.lobby != null)
                {
                    UIMatchmakingBox.core.nonPreferredServers.Add(Steam.lobby.id);
                    UIMatchmakingBox.core.blacklist.Add(Steam.lobby.id);
                }
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.Kicked, "|RED|Oh no!! The host banned you :("));
                return true;
            }
            if (m is NMClientDisconnect)
            {
                NMClientDisconnect disconnect = m as NMClientDisconnect;
                if (disconnect.profile != null && disconnect.profile.connection != null)
                {
                    bool correctIdentifier = disconnect.profile.connection.identifier == disconnect.whom;
                    if (disconnect.whom == "local")
                    {
                        correctIdentifier = disconnect.profile.connection == disconnect.connection;
                    }
                    if (correctIdentifier)
                    {
                        if (disconnect.profile.connection == Network.host && disconnect.profile != hostProfile)
                        {
                            disconnect.profile.networkStatus = DuckNetStatus.Disconnected;
                            disconnect.profile.connection = null;
                            disconnect.profile.team = null;
                            DevConsole.Log(DCSection.DuckNet, "Host local duck left", m.connection);
                        }
                        else
                        {
                            Network.activeNetwork.core.DisconnectClient(disconnect.profile.connection, new DuckNetErrorInfo(DuckNetError.ClientDisconnected, "Client disconnected."));
                            DevConsole.Log(DCSection.DuckNet, "Client disconnected", m.connection);
                        }
                    }
                }
                return true;
            }
            if (m is NMErrorEmptyJoinMessage)
            {
                _core.status = DuckNetStatus.Failure;
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.GameInProgress, "|RED|No ducks, please retry."));
                return true;
            }
            if (m is NMGameInProgress)
            {
                _core.status = DuckNetStatus.Failure;
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.GameInProgress, "|RED|Game was already in progress."));
                return true;
            }
            if (m is NMServerFull)
            {
                _core.status = DuckNetStatus.Failure;
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.FullServer, "|RED|The game was full!"));
                return true;
            }
            if (m is NMInvalidPassword)
            {
                _core.status = DuckNetStatus.Failure;
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.InvalidPassword, "|RED|Password was incorrect!"));
                return true;
            }
            if (m is NMInvalidLevel)
            {
                _core.status = DuckNetStatus.Failure;
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.InvalidLevel, "|RED|Level request was invalid!"));
                return true;
            }
            if (m is NMInvalidUser)
            {
                _core.status = DuckNetStatus.Failure;
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.InvalidUser, "|RED|The host did not reconize you!"));
                return true;
            }
            if (m is NMInvalidCustomHat)
            {
                _core.status = DuckNetStatus.Failure;
                Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.InvalidCustomHat, "|RED|Your custom hat was invalid!"));
                return true;
            }
            if (m is NMVersionMismatch)
            {
                _core.status = DuckNetStatus.Failure;
                NMVersionMismatch ver = m as NMVersionMismatch;
                FailWithVersionMismatch(ver.serverVersion, ver.GetCode());
                return true;
            }
        }
        return false;
    }

    public static void FailWithVersionMismatch(string theirVersion, NMVersionMismatch.Type type)
    {
        Steam.MarkForUpdateCheck();
        _core.status = DuckNetStatus.Failure;
        Network.EndNetworkingSession(AssembleMismatchError(theirVersion));
    }

    public static void FailWithDatahashMismatch()
    {
        Steam.MarkForUpdateCheck();
        _core.status = DuckNetStatus.Failure;
        Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ModsIncompatible, "|RED|Game data does not match the host's!\n    Please ensure game versions\n           are the same."));
    }

    public static void FailWithModDatahashMismatch(Mod pMod)
    {
        Steam.MarkForUpdateCheck();
        _core.status = DuckNetStatus.Failure;
        Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ModsIncompatible, "|RED|Mod data mismatch:\n   |DGBLUE|" + pMod.configuration.displayName + "|PREV|\n"));
    }

    public static void FailWithDifferentModsError()
    {
        _core.status = DuckNetStatus.Failure;
        Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ModsIncompatible, "Host has different Mods enabled!"));
    }

    public static void FailWithBlockedUser()
    {
        _core.status = DuckNetStatus.Failure;
        Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.HostIsABlockedUser, "|RED|Could not join lobby: \n   you have blocked\n         the host."));
    }

    public static DuckNetErrorInfo AssembleMismatchError(string theirVersion)
    {
        NMVersionMismatch.Type num = CheckVersion(theirVersion);
        return new DuckNetErrorInfo(msg: GetVersionMismatchError(num, theirVersion), e: (num == NMVersionMismatch.Type.Older) ? DuckNetError.YourVersionTooNew : DuckNetError.YourVersionTooOld);
    }

    public static string GetVersionMismatchError(string theirVersion)
    {
        return GetVersionMismatchError(CheckVersion(theirVersion), theirVersion);
    }

    public static string GetVersionMismatchError(NMVersionMismatch.Type type, string theirVersion, bool shortMessage = false)
    {
        string message = "";
        switch (type)
        {
            case NMVersionMismatch.Type.Error:
                message = "|RED|Your game version caused an error.\n\n|WHITE|HOST: |GREEN|" + theirVersion + "\n|WHITE|YOU:  |RED|" + DG.version;
                break;
            case NMVersionMismatch.Type.Newer:
                message = "|RED|Your version is too old.\n\n|WHITE|HOST: |GREEN|" + theirVersion + "\n|WHITE|YOU:  |RED|" + DG.version;
                break;
            case NMVersionMismatch.Type.Older:
                message = "|RED|Your version is too new.\n\n|WHITE|HOST: |GREEN|" + theirVersion + "\n|WHITE|YOU:  |RED|" + DG.version;
                break;
        }
        return message;
    }

    public static DuckPersona NextFree(Profile pProfile, DuckPersona pRequested)
    {
        int request = pRequested.index;
        do
        {
            DuckPersona p = Persona.all.ElementAt(request);
            bool found = false;
            foreach (Profile pr in Profiles.active)
            {
                if (pr != pProfile && pr.persona == p)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                break;
            }
            request++;
            if (request > Persona.all.Count() - 1)
            {
                request = 0;
            }
        }
        while (request != pRequested.index);
        return Persona.all.ElementAt(request);
    }

    public static void RequestPersona(Profile pProfile, DuckPersona pPersona, bool pSendMessages = true)
    {
        if (Network.isServer)
        {
            bool found = false;
            foreach (Profile p in Profiles.active)
            {
                if (p != pProfile && p.persona == pPersona)
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                pProfile.PersonaRequestResult(NextFree(pProfile, pProfile.persona));
            }
            else
            {
                pProfile.PersonaRequestResult(pPersona);
            }
            if (pSendMessages)
            {
                Send.Message(new NMSetPersona(pProfile, pProfile.persona));
            }
        }
        else if (hostProfile != null && hostProfile.connection != null && pSendMessages)
        {
            Send.Message(new NMRequestPersona(pProfile, pPersona), hostProfile.connection);
        }
    }

    public static void OnMessage(NetMessage m)
    {
        if (m != null && m.connection.status != ConnectionStatus.Connected)
        {
            if (m.connection.status == ConnectionStatus.Disconnecting || m.connection.status == ConnectionStatus.Disconnected)
            {
                DevConsole.Log(DCSection.NetCore, "|DGRED|@error DuckNet message |WHITE|" + m.ToString() + " while disconnecting!|PREV|");
            }
            else
            {
                DevConsole.Log(DCSection.NetCore, "|DGRED|@error DuckNet message |WHITE|" + m.ToString() + " while still connecting!|PREV|");
            }
        }
        if (m is NMJoinDuckNetSuccess || m is NMNewDuckNetConnection)
        {
            DevConsole.Log(DCSection.DuckNet, "Received new DuckNet connection message (NMJoinDuckNetSuccess)!");
        }
        if (status == DuckNetStatus.Disconnected)
        {
            return;
        }
        if (m is NMDuckNetworkEvent)
        {
            (m as NMDuckNetworkEvent).Activate();
            return;
        }
        UIMatchmakingBox.core.pulseNetwork = true;
        if (GetProfiles(m.connection).Count == 0 && m.connection != Network.host)
        {
            NetMessage result = OnMessageFromNewClient(m);
            if (result != null)
            {
                Send.Message(result, m.connection);
            }
        }
        else
        {
            if (HandleCoreConnectionMessages(m) || status == DuckNetStatus.Disconnecting)
            {
                return;
            }
            Main.codeNumber = 13373;
            foreach (Profile p in GetProfiles(m.connection))
            {
                if (p.networkStatus == DuckNetStatus.Disconnecting || p.networkStatus == DuckNetStatus.Disconnected || p.networkStatus == DuckNetStatus.Failure)
                {
                    return;
                }
            }
            if (m is NMMojiData)
            {
                NMMojiData t = m as NMMojiData;
                try
                {
                    DuckFile.RegisterMoji(t.name, new Sprite(Editor.StringToTexture(t.data)), t.connection);
                    DevConsole.Log(DCSection.DuckNet, "|DGBLUE|Received Moji (" + t.name + ")");
                    return;
                }
                catch (Exception)
                {
                    return;
                }
            }
            if (m is NMSpecialHat)
            {
                NMSpecialHat specialHat = m as NMSpecialHat;
                {
                    foreach (Profile p2 in profiles)
                    {
                        if (p2.connection != null && p2.connection == specialHat.connection)
                        {
                            while (p2.customTeams.Count < specialHat.customTeamIndex)
                            {
                                p2.customTeams.Add(new Team("CUSTOM", "hats/cluehat")
                                {
                                    owner = p2
                                });
                            }
                            Team.deserializeInto = p2.GetCustomTeam(specialHat.customTeamIndex);
                            Team.networkDeserialize = true;
                            if (specialHat.filtered)
                            {
                                Team.deserializeInto.customData = null;
                                Team.deserializeInto.SetHatSprite(new SpriteMap("hats/default", 32, 32));
                                Team.deserializeInto = null;
                            }
                            else
                            {
                                Team.Deserialize(specialHat.GetData()).customConnection = specialHat.connection;
                            }
                            Team.networkDeserialize = false;
                        }
                    }
                    return;
                }
            }
            if (m is NMSetTeam)
            {
                NMSetTeam t2 = m as NMSetTeam;
                if (t2.profile == null || t2.profile.connection == null || t2.profile.team == null)
                {
                    return;
                }
                if (t2.team == null)
                {
                    DevConsole.Log("|DGRED|NMSetTeam.team is NULL.");
                    return;
                }
                if (t2.profile.slotType == SlotType.Spectator)
                {
                    if (t2.profile.team != null)
                    {
                        t2.profile.team.Leave(t2.profile, set: false);
                    }
                    t2.profile.team = t2.team;
                }
                else
                {
                    t2.profile.team = t2.team;
                }
                if (Network.isServer && OnTeamSwitch(t2.profile))
                {
                    Send.MessageToAllBut(new NMSetTeam(t2.profile, t2.team, t2.custom), NetMessagePriority.ReliableOrdered, m.connection);
                }
            }
            else if (m is NMRoomData)
            {
                NMRoomData t3 = m as NMRoomData;
                if (t3.profile != null && t3.profile.linkedProfile == null && t3.profile.connection != null && t3.profile.connection != localConnection)
                {
                    t3.profile.furniturePositionData = t3.data;
                }
            }
            else if (Network.isServer)
            {
                if (m is NMClientLoadedLevel)
                {
                    if ((m as NMClientLoadedLevel).levelIndex == levelIndex)
                    {
                        m.connection.wantsGhostData = (m as NMClientLoadedLevel).levelIndex;
                        return;
                    }
                    DevConsole.Log(DCSection.DuckNet, "@error The client loaded the wrong level! (" + (m as NMClientLoadedLevel).levelIndex + " VS " + levelIndex + ")@error", m.connection);
                }
            }
            else if (m is NMJoinDuckNetSuccess || m is NMNewDuckNetConnection)
            {
                if (m is NMJoinDuckNetSuccess)
                {
                    DevConsole.Log(DCSection.DuckNet, "DuckNet |LIME|connection with host was established!");
                    NMJoinDuckNetSuccess obj = m as NMJoinDuckNetSuccess;
                    int count = 0;
                    byte flippers = Profile.CalculateLocalFlippers();
                    {
                        foreach (Profile p3 in obj.profiles)
                        {
                            if (p3.connection == localConnection)
                            {
                                p3.networkStatus = DuckNetStatus.Connected;
                            }
                            else
                            {
                                InputProfile inputProfile = InputProfile.defaultProfiles[count];
                                Profile originallySelectedProfile = null;
                                Profile nameProfile = null;
                                if (UIMatchmakingBox.core != null && UIMatchmakingBox.core.matchmakingProfiles != null && UIMatchmakingBox.core.matchmakingProfiles.Count > 0)
                                {
                                    inputProfile = UIMatchmakingBox.core.matchmakingProfiles[count].inputProfile;
                                    originallySelectedProfile = UIMatchmakingBox.core.matchmakingProfiles[count].originallySelectedProfile;
                                    nameProfile = UIMatchmakingBox.core.matchmakingProfiles[count].masterProfile;
                                    if (nameProfile == null)
                                    {
                                        nameProfile = originallySelectedProfile;
                                    }
                                }
                                DuckPersona setPersona = p3.persona;
                                string profileName = ((nameProfile != null) ? GetNameForIndex(nameProfile.name, count) : GetNameForIndex(Network.activeNetwork.core.GetLocalName(), count));
                                PrepareProfile(p3, localConnection, inputProfile, originallySelectedProfile, profileName);
                                p3.keepSetName = count != 0;
                                p3.flippers = flippers;
                                p3.persona = setPersona;
                            }
                            count++;
                        }
                        return;
                    }
                }
                NMNewDuckNetConnection remote = m as NMNewDuckNetConnection;
                NetworkConnection joinConnection = remote.connection;
                bool hostConnection = remote.identifier == "SERVER" || remote.identifier == "SERVERLOCAL";
                if (!hostConnection)
                {
                    joinConnection = Network.activeNetwork.core.AttemptConnection(remote.identifier);
                    if (joinConnection == null)
                    {
                        Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.InvalidConnectionInformation, "Invalid connection information (" + remote.identifier + ")"));
                        return;
                    }
                }
                Profile profile = remote.profile;
                PrepareProfile(profile, joinConnection, null, null, remote.name);
                profile.team = remote.team;
                profile.flippers = remote.flippers;
                profile.flagIndex = remote.flagIndex;
                profile.steamID = remote.profileID;
                profile.ParentalControlsActive = remote.parentalControlsActive;
                profile.networkStatus = DuckNetStatus.Connected;
                profile.isRemoteLocalDuck = remote.identifier == "SERVERLOCAL";
                profile.latestGhostIndex = remote.latestGhostIndex;
                profile.persona = Persona.all.ElementAt(remote.persona);
                DevConsole.Log(DCSection.DuckNet, "Queuing up join message payload for " + joinConnection.ToString());
                if (joinConnection.status != ConnectionStatus.Connected)
                {
                    DevConsole.Log(DCSection.DuckNet, "|DGBLUE|This Payload will be sent when a connection is established.");
                }
                else
                {
                    DevConsole.Log(DCSection.DuckNet, "|DGBLUE|This Payload will be sent at once!");
                }
                Send.Message(new NMLevelReady(levelIndex), joinConnection);
                foreach (Profile profile2 in GetProfiles(localConnection))
                {
                    SendAllPlayerMetaData(profile2, joinConnection);
                }
                if (hostConnection && _core.hostProfile == null)
                {
                    _core.hostProfile = remote.profile;
                }
            }
            else if (m is NMEndOfDuckNetworkData)
            {
                _core.status = DuckNetStatus.ConnectingToClients;
                CheckConnectingStatus();
            }
            else if (m is NMTeamSetDenied)
            {
                NMTeamSetDenied t4 = m as NMTeamSetDenied;
                if (t4.profile != null && t4.profile.connection == localConnection && t4.profile.team != null && t4.profile.team == t4.team)
                {
                    OpenTeamSwitchDialogue(t4.profile);
                }
            }
        }
    }

    public static void AssignToHost(Thing pThing)
    {
        if (hostProfile != null && hostProfile.connection != null)
        {
            pThing.connection = hostProfile.connection;
        }
    }

    public static void SendToEveryone(NetMessage m)
    {
        List<NetworkConnection> allConnections = new List<NetworkConnection>();
        foreach (Profile p in profiles)
        {
            if (p.connection != null && p.connection != localConnection && (!p.isHost || p == hostProfile) && !allConnections.Contains(p.connection))
            {
                allConnections.Add(p.connection);
            }
        }
        Send.Message(m, NetMessagePriority.ReliableOrdered, allConnections);
    }

    public static void SendToEveryoneFiltered(NetMessage m)
    {
        List<NetworkConnection> allConnections = new List<NetworkConnection>();
        foreach (Profile p in profiles)
        {
            if (FilterPlayer(p) == 0 && p.connection != null && p.connection != localConnection && (!p.isHost || p == hostProfile) && !allConnections.Contains(p.connection))
            {
                allConnections.Add(p.connection);
            }
        }
        Send.Message(m, NetMessagePriority.ReliableOrdered, allConnections);
    }

    public static void Draw()
    {
        if (localProfile == null)
        {
            return;
        }
        Vector2 size = new Vector2(Layer.Console.width, Layer.Console.height);
        float yDraw = 0f;
        int numDraw = 8;
        float chatScale = DuckNetwork.chatScale;
        float duckfontScale = ((Resolution.current.x > 1920) ? 2f : 1f);
        _core._chatFont.Scale = new Vector2(Resolution.fontSizeMultiplier * chatScale);
        _core._chatFont.Scale = new Vector2(duckfontScale, duckfontScale);
        if (_core._chatFont is RasterFont)
        {
            _core._chatFont.Scale = new Vector2(0.5f);
        }
        float opacity = (float)Options.Data.chatOpacity / 100f;
        if (_core.enteringText && !_core.stopEnteringText)
        {
            _core.cursorFlash++;
            if (_core.cursorFlash > 30)
            {
                _core.cursorFlash = 0;
            }
            bool num = _core.cursorFlash >= 15;
            Profile p = localProfile;
            string fullString = p.name + ": " + _core.currentEnterText;
            string fullWords = fullString;
            if (num)
            {
                fullString += "_";
            }
            float messageWidth = _core._chatFont.GetWidth(fullWords + "_") + 8f * chatScale;
            float messageHeight = (float)(_core._chatFont.characterHeight + 2) * _core._chatFont.Scale.Y;
            Vector2 messagePos = new Vector2(14f, yDraw + (size.Y - (float)(_core._chatFont.characterHeight + 10) * _core._chatFont.Scale.Y));
            Graphics.DrawRect(messagePos + new Vector2(-1f, -1f), messagePos + new Vector2(messageWidth, messageHeight) + new Vector2(1f, 1f), Color.Black * opacity, 0.7f, filled: false, 1f * chatScale);
            Color boxColor = Color.White;
            Color textColor = Color.Black;
            if (p.persona != null)
            {
                boxColor = p.persona.colorUsable;
            }
            if (p.slotType == SlotType.Spectator)
            {
                boxColor = Colors.DGPurple;
            }
            Graphics.DrawRect(messagePos, messagePos + new Vector2(messageWidth, messageHeight), boxColor * 0.85f * opacity, 0.8f);
            _core._chatFont.symbolYOffset = 4f;
            _core._chatFont.Draw(fullString, messagePos + new Vector2(2f, 2f), textColor * opacity, 1f);
            yDraw -= messageHeight + 4f * _core._chatFont.Scale.Y;
        }
        float hatDepth = 0.1f;
        foreach (ChatMessage message in _core.chatMessages)
        {
            float leftOffset = (float)(10 * (Options.Data.chatHeadScale + 1)) * duckfontScale;
            _core._chatFont._currentConnection = ((message.who.connection == localConnection) ? null : message.who.connection);
            _core._chatFont.Scale = new Vector2(Resolution.fontSizeMultiplier * message.scale * chatScale);
            _core._chatFont.Scale = new Vector2(duckfontScale, duckfontScale) * message.scale;
            if (_core._chatFont is RasterFont)
            {
                _core._chatFont.Scale = new Vector2(0.5f);
            }
            float messageWidth2 = _core._chatFont.GetWidth(message.text) + leftOffset + 8f * chatScale;
            if (message.who.slotType == SlotType.Spectator)
            {
                if (_core._chatFont is RasterFont)
                {
                    float scaleFac = (_core._chatFont as RasterFont).data.fontSize * RasterFont.fontScaleFactor / 10f;
                    messageWidth2 += 6f * scaleFac;
                }
                else
                {
                    messageWidth2 += 8f * _core._chatFont.Scale.X;
                }
            }
            float messageHeight2 = (float)(message.newlines * (_core._chatFont.characterHeight + 2)) * _core._chatFont.Scale.Y;
            Vector2 messagePos2 = new Vector2(14f, yDraw + (size.Y - (messageHeight2 + 10f)));
            Vector2 messageBR = messagePos2 + new Vector2(messageWidth2, messageHeight2);
            Graphics.DrawRect(messagePos2 + new Vector2(-1f, -1f), messageBR + new Vector2(1f, 1f), Color.Black * 0.8f * message.alpha * opacity, hatDepth - 0.0015f, filled: false, 1f * chatScale);
            float extraScale = 0.3f + (float)message.text.Length * 0.007f;
            if (extraScale > 0.5f)
            {
                extraScale = 0.5f;
            }
            if (message.slide > 0.8f)
            {
                message.scale = Lerp.FloatSmooth(message.scale, 1f, 0.1f, 1.1f);
            }
            else if (message.slide > 0.5f)
            {
                message.scale = Lerp.FloatSmooth(message.scale, 1f + extraScale, 0.1f, 1.1f);
            }
            message.slide = Lerp.FloatSmooth(message.slide, 1f, 0.1f, 1.1f);
            Color boxColor2 = Color.White;
            Color textColor2 = Color.Black;
            if (message.who.persona != null)
            {
                boxColor2 = message.who.persona.colorUsable;
                if (message.who.persona == Persona.Duck2)
                {
                    boxColor2.R += 30;
                    boxColor2.G += 30;
                    boxColor2.B += 30;
                }
                if (message.who.slotType == SlotType.Spectator)
                {
                    boxColor2 = Colors.DGPurple;
                }
                SpriteMap hat = null;
                SpriteMap torso = message.who.persona.chatBust;
                Vector2 offset = Vector2.Zero;
                if (message.who.team != null && message.who.team.hasHat && (message.who.connection != localConnection || !message.who.team.locked))
                {
                    offset = message.who.team.hatOffset * duckfontScale * (Options.Data.chatHeadScale + 1);
                    hat = message.who.team.GetHat(message.who.persona);
                }
                bool quack = message.who.netData.Get<bool>("quack");
                if (message.who.duck != null && !message.who.duck.dead && !message.who.duck.removeFromLevel)
                {
                    quack = message.who.duck.quack > 0;
                }
                Vector2 hatDraw = new Vector2(messagePos2.X, messagePos2.Y + (float)(-2 * (Options.Data.chatHeadScale + 1)));
                if (hat != null)
                {
                    hat.CenterOrigin();
                    hat.Depth = hatDepth - 0.001f;
                    hat.Alpha = message.alpha * opacity;
                    if (quack && hat.texture != null && hat.texture.width > 32)
                    {
                        hat.frame = 1;
                    }
                    else
                    {
                        hat.frame = 0;
                    }
                    hat.Scale = new Vector2(duckfontScale, duckfontScale) * (Options.Data.chatHeadScale + 1);
                    Graphics.Draw(hat, hatDraw.X - offset.X, hatDraw.Y - offset.Y);
                    hat.Scale = new Vector2(1f, 1f);
                    hat.Alpha = 1f;
                }
                if (quack)
                {
                    torso.frame = 1;
                }
                else
                {
                    torso.frame = 0;
                }
                torso.Depth = hatDepth - 0.0015f;
                torso.Alpha = message.alpha * opacity;
                torso.Scale = new Vector2(duckfontScale, duckfontScale) * (Options.Data.chatHeadScale + 1);
                Graphics.Draw(torso, hatDraw.X + 2f * torso.Scale.X, hatDraw.Y + 5f * torso.Scale.Y);
                boxColor2 *= 0.85f;
                boxColor2.A = byte.MaxValue;
            }
            Graphics.DrawRect(messagePos2, messageBR, boxColor2 * 0.85f * message.alpha * opacity, hatDepth - 0.002f);
            _core._chatFont.symbolYOffset = 4f;
            _core._chatFont.lineGap = 2f;
            if (message.who.slotType == SlotType.Spectator)
            {
                _core._chatFont.Draw("@SPECTATORBIG@" + message.text, messagePos2 + new Vector2(2f + leftOffset, 1f * _core._chatFont.Scale.Y), textColor2 * message.alpha * opacity, 1f);
            }
            else
            {
                _core._chatFont.Draw(message.text, messagePos2 + new Vector2(2f + leftOffset, 1f * _core._chatFont.Scale.Y), textColor2 * message.alpha * opacity, 1f);
            }
            _core._chatFont._currentConnection = null;
            yDraw -= messageHeight2 + 4f;
            hatDepth -= 0.01f;
            if (numDraw == 0)
            {
                break;
            }
            numDraw--;
        }
    }
}
