using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuckGame;

public class DuckNetworkCore
{
    private static int kWinsStep = 5;

    private static int kWinsMin = 5;

    public List<MatchSetting> matchSettings = new List<MatchSetting>
    {
        new MatchSetting
        {
            id = "requiredwins",
            name = "Required Wins",
            value = 10,
            min = kWinsMin,
            max = 100,
            step = kWinsStep,
            stepMap = new Dictionary<int, int>
            {
                { 50, kWinsStep },
                { 100, 10 },
                { 500, 50 },
                { 1000, 100 }
            }
        },
        new MatchSetting
        {
            id = "restsevery",
            name = "Rests Every",
            value = 10,
            min = kWinsMin,
            max = 100,
            step = kWinsStep,
            stepMap = new Dictionary<int, int>
            {
                { 50, kWinsStep },
                { 100, 10 },
                { 500, 50 },
                { 1000, 100 }
            }
        },
        new MatchSetting
        {
            id = "wallmode",
            name = "Wall Mode",
            value = false
        },
        new MatchSetting
        {
            id = "normalmaps",
            name = "@NORMALICON@|DGBLUE|Normal Levels",
            value = 90,
            suffix = "%",
            min = 0,
            max = 100,
            step = 5,
            percentageLinks = new List<string> { "randommaps", "custommaps", "workshopmaps" }
        },
        new MatchSetting
        {
            id = "randommaps",
            name = "@RANDOMICON@|DGBLUE|Random Levels",
            value = 10,
            suffix = "%",
            min = 0,
            max = 100,
            step = 5,
            percentageLinks = new List<string> { "normalmaps", "workshopmaps", "custommaps" }
        },
        new MatchSetting
        {
            id = "custommaps",
            name = "@CUSTOMICON@|DGBLUE|Custom Levels",
            value = 0,
            suffix = "%",
            min = 0,
            max = 100,
            step = 5,
            percentageLinks = new List<string> { "normalmaps", "randommaps", "workshopmaps" }
        },
        new MatchSetting
        {
            id = "workshopmaps",
            name = "@RAINBOWICON@|DGBLUE|Internet Levels",
            value = 0,
            suffix = "%",
            min = 0,
            max = 100,
            step = 5,
            percentageLinks = new List<string> { "normalmaps", "custommaps", "randommaps" }
        },
        new MatchSetting
        {
            id = "clientlevelsenabled",
            name = "Client Maps",
            value = false
        }
    };

    public List<MatchSetting> onlineSettings = new List<MatchSetting>
    {
        new MatchSetting
        {
            id = "maxplayers",
            name = "Max Players",
            value = 4,
            min = 2,
            max = 8,
            step = 1
        },
        new MatchSetting
        {
            id = "teams",
            name = "Teams",
            value = false,
            filtered = false,
            filterOnly = true
        },
        new MatchSetting
        {
            id = "customlevelsenabled",
            name = "Custom Levels",
            value = false,
            filtered = false,
            filterOnly = true
        },
        new MatchSetting
        {
            id = "modifiers",
            name = "Modifiers",
            value = false,
            filtered = true,
            filterOnly = true
        },
        new MatchSetting
        {
            id = "type",
            name = "Type",
            value = 2,
            min = 0,
            max = 3,
            createOnly = true,
            valueStrings = new List<string> { "PRIVATE", "FRIENDS", "PUBLIC", "LAN" }
        },
        new MatchSetting
        {
            id = "name",
            name = "Name",
            value = "",
            filtered = false,
            filterOnly = false,
            createOnly = true
        },
        new MatchSetting
        {
            id = "password",
            name = "Password",
            value = "",
            filtered = false,
            filterOnly = false,
            createOnly = true
        },
        new MatchSetting
        {
            id = "port",
            name = "Port",
            value = "1337",
            filtered = false,
            filterOnly = false,
            condition = () => (int)TeamSelect2.GetOnlineSetting("type").value == 3
        },
        new MatchSetting
        {
            id = "dedicated",
            name = "Dedicated",
            value = false,
            filtered = false,
            filterOnly = false,
            createOnly = true
        }
    };

    public Dictionary<string, XPPair> _xpEarned = new Dictionary<string, XPPair>();

    public int levelTransferSize;

    public int levelTransferProgress;

    public int logTransferSize;

    public int logTransferProgress;

    public bool isDedicatedServer;

    public string serverPassword = "";

    public UIMenu xpMenu;

    public UIComponent ducknetUIGroup;

    public DuckNetwork.LobbyType lobbyType;

    public bool speedrunMode;

    public int speedrunMaxTrophy;

    public List<Profile> profiles = new List<Profile>();

    public List<Profile> profilesFixedOrder = new List<Profile>();

    public Profile localProfile;

    public Profile hostProfile;

    public List<NetMessage> pending = new List<NetMessage>();

    public MemoryStream compressedLevelData;

    public bool enteringText;

    public string currentEnterText = "";

    public int cursorFlash;

    public ushort chatIndex;

    public ushort levelTransferSession;

    public NetworkConnection localConnection = new NetworkConnection(null);

    public DuckNetStatus status;

    public FancyBitmapFont _builtInChatFont;

    public FancyBitmapFont _rasterChatFont;

    public bool initialized;

    public int randomID;

    public bool inGame;

    public bool stopEnteringText;

    public List<ChatMessage> chatMessages = new List<ChatMessage>();

    private int swearCharOffset;

    private string[] swearChars = new string[7] { "%", "+", "{", "}", "$", "!", "Z" };

    private string[] swearChars2 = new string[7] { "%", "+", "#", "$", "~", "!", "Z" };

    private int rainbowIndex;

    private string[] swearColors = new string[7] { "|RBOW_1|", "|RBOW_2|", "|RBOW_3|", "|RBOW_4|", "|RBOW_5|", "|RBOW_6|", "|RBOW_7|" };

    public static string filteredSpeech;

    public UIMenu _ducknetMenu;

    public UIMenu _optionsMenu;

    public UIMenu _confirmMenu;

    public UIMenu _confirmBlacklistMenu;

    public UIMenu _confirmBlock;

    public UIMenu _confirmReturnToLobby;

    public UIMenu _confirmKick;

    public UIMenu _confirmBan;

    public UIMenu _confirmEditSlots;

    public UIMenu _confirmMatchSettings;

    public MenuBoolean _quit = new MenuBoolean();

    public MenuBoolean _menuClosed = new MenuBoolean();

    public MenuBoolean _returnToLobby = new MenuBoolean();

    public UIMenu _levelSelectMenu;

    public Profile _menuOpenProfile;

    public Profile kickContext;

    public List<ulong> _invitedFriends = new List<ulong>();

    public MenuBoolean _inviteFriends = new MenuBoolean();

    public UIMenu _inviteMenu;

    public UIMenu _slotEditor;

    public UIMenu _matchSettingMenu;

    public UIMenu _matchModifierMenu;

    public UIComponent _noModsUIGroup;

    public UIMenu _noModsMenu;

    public UIComponent _restartModsUIGroup;

    public UIMenu _restartModsMenu;

    public UIComponent _resUIGroup;

    public UIMenu _resMenu;

    public bool _pauseOpen;

    public string _settingsBeforeOpen = "";

    public bool _willOpenSettingsInfo;

    public int _willOpenSpectatorInfo;

    public bool startCountdown;

    public List<string> _activatedLevels = new List<string>();

    public FancyBitmapFont _chatFont
    {
        get
        {
            if (_rasterChatFont == null)
            {
                return _builtInChatFont;
            }
            return _rasterChatFont;
        }
    }

    public void RecreateProfiles()
    {
        profiles.Clear();
        profilesFixedOrder.Clear();
        for (int i = 0; i < DG.MaxPlayers; i++)
        {
            Profile p = new Profile("Netduck" + (i + 1), InputProfile.GetVirtualInput(i), null, Persona.all.ElementAt(i), network: true);
            p.SetNetworkIndex((byte)i);
            p.SetFixedGhostIndex((byte)i);
            if (i > 3)
            {
                p.Special_SetSlotType(SlotType.Closed);
            }
            profiles.Add(p);
            profilesFixedOrder.Add(p);
        }
        for (int j = 0; j < DG.MaxSpectators; j++)
        {
            Profile p2 = new Profile("Observer" + (j + 1), InputProfile.GetVirtualInput(j + DG.MaxPlayers), null, Persona.all.ElementAt(0), network: true);
            p2.SetNetworkIndex((byte)(j + DG.MaxPlayers));
            p2.SetFixedGhostIndex((byte)(j + DG.MaxPlayers));
            p2.slotType = SlotType.Spectator;
            profiles.Add(p2);
            profilesFixedOrder.Add(p2);
        }
    }

    public DuckNetworkCore()
    {
        RecreateProfiles();
        randomID = Rando.Int(2147483646);
    }

    public DuckNetworkCore(bool waitInit)
    {
        if (!waitInit)
        {
            RecreateProfiles();
        }
        randomID = Rando.Int(2147483646);
    }

    public void ReorderFixedList()
    {
        profilesFixedOrder = profiles.OrderBy((Profile x) => x.fixedGhostIndex).ToList();
    }

    public string FilterText(string pText, User pUser)
    {
        if (Options.Data.languageFilter)
        {
            filteredSpeech = "";
            pText = pText.Replace("*", "@_sr_@");
            pText = Steam.FilterText(pText, pUser);
            swearCharOffset = 0;
            bool swearing = false;
            string newMessage = "";
            for (int i = 0; i < pText.Length; i++)
            {
                if (pText[i] == '*')
                {
                    if (!swearing)
                    {
                        swearing = true;
                        filteredSpeech += "quack";
                    }
                    newMessage += swearColors[rainbowIndex];
                    rainbowIndex = (rainbowIndex + 1) % swearColors.Length;
                    if (_rasterChatFont == null)
                    {
                        newMessage = newMessage + swearChars[Rando.Int(swearChars.Length - 1)] + "|PREV|";
                        swearCharOffset = (swearCharOffset + 1) % swearChars2.Length;
                    }
                    else
                    {
                        newMessage = newMessage + swearChars2[Rando.Int(swearChars2.Length - 1)] + "|PREV|";
                        swearCharOffset = (swearCharOffset + 1) % swearChars2.Length;
                    }
                }
                else
                {
                    swearing = false;
                    swearCharOffset = 0;
                    newMessage += pText[i];
                    filteredSpeech += pText[i];
                }
            }
            pText = newMessage;
            pText = pText.Replace("@_sr_@", "*");
            filteredSpeech = filteredSpeech.Replace("@_sr_@", "*");
        }
        else
        {
            filteredSpeech = pText;
        }
        return pText;
    }

    public void AddChatMessage(ChatMessage pMessage)
    {
        if (pMessage.who == null)
        {
            return;
        }
        ChatMessage prev = null;
        if (chatMessages.Count > 0)
        {
            prev = chatMessages[0];
        }
        int newlines = 0;
        pMessage.text = FilterText(pMessage.text, null);
        if (Options.Data.textToSpeech)
        {
            if (Options.Data.textToSpeechReadNames)
            {
                SFX.Say(pMessage.who.nameUI + " says " + filteredSpeech);
            }
            else
            {
                SFX.Say(filteredSpeech);
            }
        }
        float chatScale = DuckNetwork.chatScale;
        _chatFont.Scale = new Vec2(2f * pMessage.scale * chatScale);
        if (_chatFont is RasterFont)
        {
            _chatFont.Scale *= 0.5f;
        }
        try
        {
            pMessage.text = _chatFont.FormatWithNewlines(pMessage.text, 800f);
        }
        catch (Exception)
        {
            pMessage.text = "??????";
        }
        newlines = pMessage.text.Count((char x) => x == '\n');
        if (prev != null && newlines == 0 && prev.newlines < 3 && prev.timeout > 2f && prev.who == pMessage.who)
        {
            pMessage.text = "|GRAY|" + pMessage.who.nameUI + ": |BLACK|" + pMessage.text;
            prev.timeout = 10f;
            prev.text += "\n";
            prev.text += pMessage.text;
            prev.index = pMessage.index;
            prev.slide = 0.5f;
            prev.newlines++;
        }
        else
        {
            pMessage.newlines = newlines + 1;
            pMessage.text = "|WHITE|" + pMessage.who.nameUI + ": |BLACK|" + pMessage.text;
            chatMessages.Add(pMessage);
        }
        chatMessages = chatMessages.OrderBy((ChatMessage x) => -x.index).ToList();
    }
}
