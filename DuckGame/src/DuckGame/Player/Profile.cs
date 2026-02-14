using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class Profile
{
    public int prevXPsave;

    public string prevFurniPositionData;

    public Dictionary<NetworkConnection, int> connectionTrouble = new Dictionary<NetworkConnection, int>();

    public NetIndex16 latestGhostIndex = 25;

    private Dictionary<string, ChallengeSaveData> _challengeData = new Dictionary<string, ChallengeSaveData>();

    public int numClientCustomLevels;

    public int flagIndex = -1;

    public bool _blockStatusDirty = true;

    private string _muteString = "";

    private bool _blocked;

    public SlotType pendingSpectatorMode = SlotType.Max;

    public List<Team> customTeams = new List<Team>();

    public List<bool> networkHatUnlockStatuses;

    public ProfileNetData netData = new ProfileNetData();

    public HashSet<string> sentMojis = new HashSet<string>();

    public Holdable carryOverObject;

    public float storeValue;

    private bool _parentalControlsActive;

    private static BitmapFont _defaultFont;

    private List<FurniturePosition> _internalFurniturePositions = new List<FurniturePosition>();

    private Dictionary<int, int> _internalFurnitures = new Dictionary<int, int>();

    private List<Furniture> _availableList;

    private int _roundsSinceXP;

    private int _littleManBucks;

    private int _numLittleMen;

    private int _littleManLevel = 1;

    private int _milkFill = 1;

    private int _numSandwiches = 1;

    private int _currentDay = 1;

    private int _punished;

    public bool keepSetName;

    private string _name = "";

    public static bool loading;

    private string _id;

    private static MTSpriteBatch _batch;

    private static SpriteMap _egg;

    private static SpriteMap _eggShine;

    private static SpriteMap _eggBorder;

    private static SpriteMap _eggOuter;

    private static SpriteMap _eggSymbols;

    private static List<Color> _allowedColors;

    private static SpriteMap _easel;

    private static SpriteMap _easelSymbols;

    private List<string> _unlocks = new List<string>();

    private int _ticketCount;

    private int _timesMetVincent;

    private int _timesMetVincentSale;

    private int _timesMetVincentSell;

    private int _timesMetVincentImport;

    private int _timesMetVincentHint;

    public float timeOfDay;

    private int _xp;

    public byte flippers;

    private float _funSlider = 0.5f;

    private int _preferredColor = -1;

    private int _requestedColor = -1;

    private ProfileStats _stats = new ProfileStats();

    private ProfileStats _junkStats;

    private ProfileStats _prevStats = new ProfileStats();

    private ProfileStats _endOfRoundStats;

    private CurrentGame _currentGame = new CurrentGame();

    public string lastKnownName;

    private ulong _steamID;

    private ulong _lanProfileID;

    private NetworkConnection _connection;

    private static bool _networkStatusLooping;

    private DuckNetStatus _networkStatus;

    private float _currentStatusTimeout;

    private int _currentStatusTries;

    public bool invited;

    private Profile _linkedProfile;

    private bool _ready = true;

    private byte _networkIndex = byte.MaxValue;

    private byte _fixedGhostIndex = byte.MaxValue;

    public bool isRemoteLocalDuck;

    public byte spectatorChangeIndex;

    private int _spectatorChangeCooldown;

    private byte _remoteSpectatorChangeIndex;

    public Dictionary<NetIndex16, GhostObject> removedGhosts = new Dictionary<NetIndex16, GhostObject>();

    private SlotType _slotType;

    public SlotType originalSlotType;

    private object _reservedUser;

    private Team _reservedTeam;

    private sbyte _reservedSpectatorPersona = -1;

    private Duck _duck;

    private DuckPersona _persona;

    private InputProfile _inputProfile;

    private Team _team;

    private int _wins;

    private bool _wasRockThrower;

    private List<DeviceInputMapping> _inputMappingOverrides = new List<DeviceInputMapping>();

    public Team defaultTeam;

    public DuckPersona defaultPersona;

    public bool isNetworkProfile;

    public string fileName = "";

    private bool isDefaultProfile;

    public Team networkDefaultTeam
    {
        get
        {
            if (_networkIndex < DG.MaxPlayers)
            {
                return Teams.all[_networkIndex];
            }
            return Teams.all[Rando.Int(7)];
        }
    }

    public DuckPersona networkDefaultPersona
    {
        get
        {
            if (_networkIndex < DG.MaxPlayers)
            {
                return Persona.all.ElementAt(_networkIndex);
            }
            return Persona.Duck1;
        }
    }

    public Dictionary<string, ChallengeSaveData> challengeData => _challengeData;

    public bool muteChat
    {
        get
        {
            if (_blockStatusDirty)
            {
                RefreshBlockStatus();
            }
            return _muteString.Contains("C");
        }
        set
        {
            Options.SetMuteSetting(this, "C", value);
            RefreshBlockStatus();
        }
    }

    public bool muteHat
    {
        get
        {
            if (_blockStatusDirty)
            {
                RefreshBlockStatus();
            }
            return _muteString.Contains("H");
        }
        set
        {
            Options.SetMuteSetting(this, "H", value);
            RefreshBlockStatus();
        }
    }

    public bool muteRoom
    {
        get
        {
            if (_blockStatusDirty)
            {
                RefreshBlockStatus();
            }
            return _muteString.Contains("R");
        }
        set
        {
            Options.SetMuteSetting(this, "R", value);
            RefreshBlockStatus();
        }
    }

    public bool muteName
    {
        get
        {
            if (_blockStatusDirty)
            {
                RefreshBlockStatus();
            }
            return _muteString.Contains("N");
        }
        set
        {
            Options.SetMuteSetting(this, "N", value);
            RefreshBlockStatus();
        }
    }

    public bool blocked
    {
        get
        {
            if (_blockStatusDirty)
            {
                RefreshBlockStatus();
            }
            return _blocked;
        }
    }

    public bool spectator => slotType == SlotType.Spectator;

    public ushort customTeamIndexOffset => (ushort)(Teams.kCustomOffset + fixedGhostIndex * Teams.kCustomSpread);

    public bool ParentalControlsActive
    {
        get
        {
            if (connection == null || connection == DuckNetwork.localConnection)
            {
                return ParentalControls.AreParentalControlsActive();
            }
            return _parentalControlsActive;
        }
        set
        {
            _parentalControlsActive = value;
        }
    }

    public BitmapFont font
    {
        get
        {
            if (_defaultFont == null)
            {
                _defaultFont = new BitmapFont("biosFont", 8);
            }
            BitmapFont f = _defaultFont;
            foreach (FurniturePosition p in _furniturePositions)
            {
                if (p != null)
                {
                    Furniture fur = RoomEditor.GetFurniture(p.id);
                    if (fur != null && fur.type == FurnitureType.Font && fur.font != null)
                    {
                        f = fur.font;
                        break;
                    }
                }
            }
            return f;
        }
    }

    private List<FurniturePosition> _furniturePositions
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile._furniturePositions;
            }
            _internalFurniturePositions.RemoveAll((FurniturePosition x) => x == null);
            return _internalFurniturePositions;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile._furniturePositions = value;
            }
            else
            {
                _internalFurniturePositions = value;
            }
        }
    }

    public List<FurniturePosition> furniturePositions => _furniturePositions;

    public BitBuffer furniturePositionData
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.furniturePositionData;
            }
            BitBuffer buf = new BitBuffer();
            foreach (FurniturePosition p in _furniturePositions)
            {
                buf.Write(p.x);
                buf.Write(p.y);
                buf.Write(p.variation);
                buf.WritePacked(p.id, 15);
                buf.WritePacked(p.flip ? 1 : 0, 1);
            }
            return buf;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.furniturePositionData = value;
                return;
            }
            _furniturePositions.Clear();
            try
            {
                int numTurrets = 0;
                while (value.position != value.lengthInBytes)
                {
                    FurniturePosition pos = new FurniturePosition();
                    pos.x = value.ReadByte();
                    pos.y = value.ReadByte();
                    pos.variation = value.ReadByte();
                    pos.id = value.ReadBits<ushort>(15);
                    pos.flip = value.ReadBits<byte>(1) > 0;
                    Furniture f = RoomEditor.GetFurniture(pos.id);
                    if (f == null)
                    {
                        continue;
                    }
                    pos.furniMapping = f;
                    if (f.type == FurnitureType.Font || f.type == FurnitureType.Theme || (pos.y < 80 && pos.x < RoomEditor.roomSize + 20))
                    {
                        _furniturePositions.Add(pos);
                        if (f.name == "PERIMETER DEFENCE")
                        {
                            numTurrets++;
                        }
                    }
                }
                if (numTurrets > 1)
                {
                    _furniturePositions.RemoveAll((FurniturePosition x) => x.furniMapping.name == "PERIMETER DEFENCE");
                }
            }
            catch (Exception)
            {
                DevConsole.Log(DCSection.General, "Failed to load furniture position data.");
                _furniturePositions.Clear();
            }
        }
    }

    public BitBuffer furnitureOwnershipData
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.furnitureOwnershipData;
            }
            BitBuffer buf = new BitBuffer();
            foreach (KeyValuePair<int, int> p in _furnitures)
            {
                buf.Write(p.Key);
                buf.Write(p.Value);
            }
            return buf;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.furnitureOwnershipData = value;
                return;
            }
            _furnitures.Clear();
            _availableList = null;
            try
            {
                while (value.position != value.lengthInBytes)
                {
                    new FurniturePosition();
                    _furnitures[value.ReadInt()] = value.ReadInt();
                }
            }
            catch (Exception)
            {
                DevConsole.Log(DCSection.General, "Failed to load furniture ownership data.");
            }
        }
    }

    public Dictionary<int, int> _furnitures
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile._furnitures;
            }
            return _internalFurnitures;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile._furnitures = value;
            }
            else
            {
                _internalFurnitures = value;
            }
        }
    }

    public int roundsSinceXP
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.roundsSinceXP;
            }
            return _roundsSinceXP;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.roundsSinceXP = value;
            }
            _roundsSinceXP = value;
        }
    }

    public int littleManBucks
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.littleManBucks;
            }
            return _littleManBucks;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.littleManBucks = value;
            }
            _littleManBucks = value;
        }
    }

    public int numLittleMen
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.numLittleMen;
            }
            return _numLittleMen;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.numLittleMen = value;
            }
            _numLittleMen = value;
        }
    }

    public int littleManLevel
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.littleManLevel;
            }
            return _littleManLevel;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.littleManLevel = value;
            }
            _littleManLevel = value;
        }
    }

    public int milkFill
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.milkFill;
            }
            return _milkFill;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.milkFill = value;
            }
            _milkFill = value;
        }
    }

    public int numSandwiches
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.numSandwiches;
            }
            return _numSandwiches;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.numSandwiches = value;
            }
            _numSandwiches = value;
        }
    }

    public int currentDay
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.currentDay;
            }
            return _currentDay;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.currentDay = value;
            }
            _currentDay = value;
        }
    }

    public int punished
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.punished;
            }
            return _punished;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.punished = value;
            }
            _punished = value;
        }
    }

    public string formattedName
    {
        get
        {
            string formattedName = rawName;
            if (steamID != 0L)
            {
                formattedName = steamID.ToString();
            }
            return formattedName;
        }
    }

    public string rawName => _name;

    public string nameUI
    {
        get
        {
            string n = name;
            if (muteName)
            {
                n = "Player " + (networkIndex + 1);
            }
            return n;
        }
    }

    public string name
    {
        get
        {
            if (linkedProfile != null && connection == DuckNetwork.localConnection && !Profiles.IsExperience(this))
            {
                return linkedProfile.name;
            }
            if (keepSetName)
            {
                return _name;
            }
            if (steamID != 0L && slotType != SlotType.Local)
            {
                if (Steam.user != null && steamID == Steam.user.id)
                {
                    return Steam.user.name;
                }
                if (lastKnownName != null)
                {
                    return lastKnownName;
                }
                if (_name == steamID.ToString())
                {
                    if (Steam.IsInitialized())
                    {
                        User u = User.GetUser(steamID);
                        if (u != null && u.id != 0L)
                        {
                            lastKnownName = u.name;
                            return lastKnownName;
                        }
                    }
                    return "STEAM PROFILE";
                }
            }
            return _name;
        }
        set
        {
            _name = value;
        }
    }

    public static bool logStats => true;

    public string id => _id;

    public static Random steamGenerator
    {
        get
        {
            if (Steam.user != null)
            {
                Random gen = new Random(Math.Abs((int)(Steam.user.id % int.MaxValue)));
                for (int i = 0; i < (int)(Steam.user.id % 252); i++)
                {
                    Rando.Int(100);
                }
                return gen;
            }
            return new Random(90210);
        }
    }

    public List<string> unlocks
    {
        get
        {
            return _unlocks;
        }
        set
        {
            _unlocks = value;
        }
    }

    public int ticketCount
    {
        get
        {
            return _ticketCount;
        }
        set
        {
            if (MonoMain.logFileOperations && _ticketCount != value)
            {
                DevConsole.Log(DCSection.General, ("Profile(" + name != null) ? name : (").ticketCount set(" + ticketCount + ")"));
            }
            _ticketCount = value;
        }
    }

    public int timesMetVincent
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.timesMetVincent;
            }
            return _timesMetVincent;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.timesMetVincent = value;
            }
            _timesMetVincent = value;
        }
    }

    public int timesMetVincentSale
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.timesMetVincentSale;
            }
            return _timesMetVincentSale;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.timesMetVincentSale = value;
            }
            _timesMetVincentSale = value;
        }
    }

    public int timesMetVincentSell
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.timesMetVincentSell;
            }
            return _timesMetVincentSell;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.timesMetVincentSell = value;
            }
            _timesMetVincentSell = value;
        }
    }

    public int timesMetVincentImport
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.timesMetVincentImport;
            }
            return _timesMetVincentImport;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.timesMetVincentImport = value;
            }
            _timesMetVincentImport = value;
        }
    }

    public int timesMetVincentHint
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.timesMetVincentHint;
            }
            return _timesMetVincentHint;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.timesMetVincentHint = value;
            }
            _timesMetVincentHint = value;
        }
    }

    public int xp
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.xp;
            }
            if (Steam.user != null && this == Profiles.experienceProfile)
            {
                if ((int)Steam.GetStat("xp") == 0)
                {
                    Steam.SetStat("xp", _xp);
                }
                return _xp;
            }
            return _xp;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.xp = value;
            }
            if (MonoMain.logFileOperations && _xp != value)
            {
                DevConsole.Log(DCSection.General, ("Profile(" + name != null) ? name : (").xp set(" + xp + ")"));
            }
            if (Steam.user != null && this == Profiles.experienceProfile)
            {
                Steam.SetStat("xp", value);
            }
            _xp = value;
        }
    }

    public bool switchStatus => (flippers & 1) != 0;

    public float funslider
    {
        get
        {
            return _funSlider;
        }
        set
        {
            _funSlider = value;
        }
    }

    public int preferredColor
    {
        get
        {
            if (linkedProfile != null)
            {
                return linkedProfile.preferredColor;
            }
            return _preferredColor;
        }
        set
        {
            if (linkedProfile != null)
            {
                linkedProfile.preferredColor = value;
            }
            else
            {
                _preferredColor = value;
            }
        }
    }

    public int requestedColor
    {
        get
        {
            if (linkedProfile != null)
            {
                return linkedProfile.requestedColor;
            }
            return _requestedColor;
        }
        set
        {
            if (linkedProfile != null)
            {
                linkedProfile.requestedColor = value;
            }
            else
            {
                _requestedColor = value;
            }
        }
    }

    public int currentColor => persona.index;

    public ProfileStats stats
    {
        get
        {
            if (!logStats)
            {
                if (_junkStats == null)
                {
                    DXMLNode node = _stats.Serialize();
                    _junkStats = new ProfileStats();
                    _junkStats.Deserialize(node);
                }
                return _junkStats;
            }
            _junkStats = null;
            if (_linkedProfile != null)
            {
                return _linkedProfile.stats;
            }
            return _stats;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.stats = value;
            }
            _stats = value;
        }
    }

    public ProfileStats prevStats
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.prevStats;
            }
            return _prevStats;
        }
        set
        {
            if (_linkedProfile != null)
            {
                _linkedProfile.prevStats = value;
            }
            _prevStats = value;
        }
    }

    public static int totalFansThisGame
    {
        get
        {
            int fans = 0;
            foreach (Profile p in Profiles.active)
            {
                if (p.slotType != SlotType.Spectator)
                {
                    fans += p.stats.GetFans();
                }
            }
            return fans;
        }
    }

    public ProfileStats endOfRoundStats
    {
        get
        {
            _endOfRoundStats = (stats - prevStats) as ProfileStats;
            return _endOfRoundStats;
        }
        set
        {
            _endOfRoundStats = value;
        }
    }

    public CurrentGame currentGame => _currentGame;

    public ulong steamID
    {
        get
        {
            if (connection == DuckNetwork.localConnection && (!Network.isActive || !Network.lanMode))
            {
                return DG.localID;
            }
            if (connection != null && connection.data is User)
            {
                return (connection.data as User).id;
            }
            return _steamID;
        }
        set
        {
            if (_steamID != value)
            {
                _blockStatusDirty = true;
            }
            _steamID = value;
        }
    }

    public NetworkConnection connection
    {
        get
        {
            return _connection;
        }
        set
        {
            _connection = value;
            if (_connection == null)
            {
                _networkStatus = DuckNetStatus.Disconnected;
            }
        }
    }

    public DuckNetStatus networkStatus
    {
        get
        {
            return _networkStatus;
        }
        set
        {
            if (!isRemoteLocalDuck && !_networkStatusLooping && connection != null)
            {
                _networkStatusLooping = true;
                foreach (Profile p in DuckNetwork.profiles)
                {
                    if (p.connection == connection)
                    {
                        p.networkStatus = value;
                    }
                }
                _networkStatusLooping = false;
            }
            else
            {
                if (value != _networkStatus)
                {
                    _currentStatusTimeout = 1f;
                    _currentStatusTries = 0;
                }
                _networkStatus = value;
            }
        }
    }

    public float currentStatusTimeout
    {
        get
        {
            return _currentStatusTimeout;
        }
        set
        {
            _currentStatusTimeout = value;
        }
    }

    public int currentStatusTries
    {
        get
        {
            return _currentStatusTries;
        }
        set
        {
            _currentStatusTries = value;
        }
    }

    public Profile linkedProfile
    {
        get
        {
            return _linkedProfile;
        }
        set
        {
            _linkedProfile = value;
        }
    }

    public bool isHost => connection == Network.host;

    public bool ready
    {
        get
        {
            return _ready;
        }
        set
        {
            _ready = value;
        }
    }

    public byte networkIndex => _networkIndex;

    public byte fixedGhostIndex => _fixedGhostIndex;

    public bool localPlayer
    {
        get
        {
            if (Network.isActive)
            {
                return _connection == DuckNetwork.localConnection;
            }
            return true;
        }
    }

    public byte remoteSpectatorChangeIndex
    {
        get
        {
            return _remoteSpectatorChangeIndex;
        }
        set
        {
            _remoteSpectatorChangeIndex = value;
            _spectatorChangeCooldown = 120;
        }
    }

    public bool readyForSpectatorChange
    {
        get
        {
            if (_spectatorChangeCooldown <= 0)
            {
                return DuckNetwork.allClientsReady;
            }
            return false;
        }
    }

    public SlotType slotType
    {
        get
        {
            return _slotType;
        }
        set
        {
            if (_slotType != value)
            {
                _slotType = value;
                if (!DuckNetwork.preparingProfiles && _slotType != SlotType.Spectator)
                {
                    DuckNetwork.ChangeSlotSettings();
                }
            }
        }
    }

    public object reservedUser
    {
        get
        {
            return _reservedUser;
        }
        set
        {
            _reservedUser = value;
        }
    }

    public Team reservedTeam
    {
        get
        {
            return _reservedTeam;
        }
        set
        {
            _reservedTeam = value;
        }
    }

    public sbyte reservedSpectatorPersona
    {
        get
        {
            return _reservedSpectatorPersona;
        }
        set
        {
            _reservedSpectatorPersona = value;
        }
    }

    public DuckPersona fallbackPersona
    {
        get
        {
            if (Network.isActive)
            {
                return networkDefaultPersona;
            }
            return defaultPersona;
        }
    }

    public DuckPersona desiredPersona
    {
        get
        {
            if (requestedColor >= 0 && requestedColor < DG.MaxPlayers)
            {
                return Persona.all.ElementAt(requestedColor);
            }
            if (preferredColor >= 0 && preferredColor < DG.MaxPlayers)
            {
                return Persona.all.ElementAt(preferredColor);
            }
            return fallbackPersona;
        }
    }

    public Duck duck
    {
        get
        {
            return _duck;
        }
        set
        {
            _duck = value;
        }
    }

    public DuckPersona persona
    {
        get
        {
            if (slotType == SlotType.Spectator)
            {
                sbyte pers = netData.Get("spectatorPersona", (sbyte)(-1));
                if (pers >= 0 && pers < 8)
                {
                    return Persona.all.ElementAt(pers);
                }
            }
            if (_persona == null)
            {
                if (this == Profiles.DefaultPlayer1)
                {
                    _persona = Persona.Duck1;
                }
                else if (this == Profiles.DefaultPlayer2)
                {
                    _persona = Persona.Duck2;
                }
                else if (this == Profiles.DefaultPlayer3)
                {
                    _persona = Persona.Duck3;
                }
                else if (this == Profiles.DefaultPlayer4)
                {
                    _persona = Persona.Duck4;
                }
                else
                {
                    _persona = Persona.Duck1;
                }
            }
            return _persona;
        }
        set
        {
            _persona = value;
        }
    }

    public static List<Profile> defaultProfileMappings => Profiles.core.defaultProfileMappings;

    public InputProfile inputProfile
    {
        get
        {
            if (!Network.isActive && _inputProfile == null)
            {
                if (this == Profiles.DefaultPlayer1)
                {
                    _inputProfile = InputProfile.Get(InputProfile.MPPlayer1);
                }
                else if (this == Profiles.DefaultPlayer2)
                {
                    _inputProfile = InputProfile.Get(InputProfile.MPPlayer2);
                }
                else if (this == Profiles.DefaultPlayer3)
                {
                    _inputProfile = InputProfile.Get(InputProfile.MPPlayer3);
                }
                else if (this == Profiles.DefaultPlayer4)
                {
                    _inputProfile = InputProfile.Get(InputProfile.MPPlayer4);
                }
                else if (this == Profiles.DefaultPlayer5)
                {
                    _inputProfile = InputProfile.Get(InputProfile.MPPlayer5);
                }
                else if (this == Profiles.DefaultPlayer6)
                {
                    _inputProfile = InputProfile.Get(InputProfile.MPPlayer6);
                }
                else if (this == Profiles.DefaultPlayer7)
                {
                    _inputProfile = InputProfile.Get(InputProfile.MPPlayer7);
                }
                else if (this == Profiles.DefaultPlayer8)
                {
                    _inputProfile = InputProfile.Get(InputProfile.MPPlayer8);
                }
                else
                {
                    _inputProfile = InputProfile.Get(InputProfile.MPPlayer1);
                }
            }
            return _inputProfile;
        }
        set
        {
            if (_inputProfile != null && _inputProfile != value)
            {
                _inputProfile.lastActiveDevice.Rumble();
                Input.ApplyDefaultMapping(_inputProfile);
            }
            if (value != null && value != _inputProfile)
            {
                Input.ApplyDefaultMapping(value, this);
            }
            _inputProfile = value;
        }
    }

    public Team team
    {
        get
        {
            return _team;
        }
        set
        {
            if (value != null)
            {
                if (slotType != SlotType.Spectator)
                {
                    value.Join(this, set: false);
                }
                _team = value;
            }
            else if (_team != null)
            {
                _team.Leave(this, set: false);
                _team = null;
                requestedColor = -1;
            }
        }
    }

    public int wins
    {
        get
        {
            return _wins;
        }
        set
        {
            _wins = value;
        }
    }

    public bool wasRockThrower
    {
        get
        {
            return _wasRockThrower;
        }
        set
        {
            _wasRockThrower = value;
        }
    }

    public List<DeviceInputMapping> inputMappingOverrides
    {
        get
        {
            if (_linkedProfile != null)
            {
                return _linkedProfile.inputMappingOverrides;
            }
            return _inputMappingOverrides;
        }
    }

    public void ReportConnectionTrouble(NetworkConnection pFrom)
    {
        connectionTrouble[pFrom] = 200;
    }

    public void TickConnectionTrouble()
    {
        if (connectionTrouble.Count > 0)
        {
            foreach (NetworkConnection c in Network.connections)
            {
                if (connectionTrouble.ContainsKey(c) && connectionTrouble[c] > 0)
                {
                    connectionTrouble[c]--;
                }
            }
        }
        if (_spectatorChangeCooldown > 0)
        {
            _spectatorChangeCooldown--;
        }
    }

    public void HasConnectionFailed()
    {
        foreach (NetworkConnection c in Network.connections)
        {
            if (connectionTrouble.ContainsKey(c) && connectionTrouble[c] > 0)
            {
                connectionTrouble[c]--;
            }
        }
    }

    public ChallengeSaveData GetSaveData(string guid, bool canBeNull = false)
    {
        if (_challengeData.TryGetValue(guid, out var val))
        {
            return val;
        }
        if (canBeNull)
        {
            return null;
        }
        ChallengeSaveData data = new ChallengeSaveData();
        data.profileID = id;
        data.challenge = guid;
        _challengeData.Add(guid, data);
        return data;
    }

    private void RefreshBlockStatus()
    {
        _blockStatusDirty = false;
        _blocked = Options.Data.blockedPlayers != null && Options.Data.blockedPlayers.Contains(steamID);
        _muteString = Options.GetMuteSettings(this);
    }

    public int IndexOfCustomTeam(Team pTeam)
    {
        int idx = customTeams.IndexOf(pTeam);
        if (idx >= 0)
        {
            return customTeamIndexOffset + idx;
        }
        return idx;
    }

    public Team GetCustomTeam(ushort pIndex)
    {
        if (connection == DuckNetwork.localConnection)
        {
            if (Teams.core.extraTeams.Count > pIndex && pIndex >= 0)
            {
                return Teams.core.extraTeams[pIndex];
            }
            return null;
        }
        while (customTeams.Count <= pIndex)
        {
            customTeams.Add(new Team("CUSTOM", "hats/cluehat")
            {
                owner = this
            });
        }
        return customTeams[pIndex];
    }

    public int GetNumFurnituresPlaced(int idx)
    {
        int num = 0;
        foreach (FurniturePosition furniturePosition in _furniturePositions)
        {
            if (furniturePosition.id == idx)
            {
                num++;
            }
        }
        return num;
    }

    public int GetTotalFurnituresPlaced()
    {
        int count = 0;
        foreach (FurniturePosition furniturePosition in _furniturePositions)
        {
            Furniture f = RoomEditor.GetFurniture(furniturePosition.id);
            if (f != null && f.type != FurnitureType.Theme && f.type != FurnitureType.Font)
            {
                count++;
            }
        }
        return count;
    }

    public void ClearFurnitures()
    {
        _furnitures.Clear();
        _furniturePositions.Clear();
    }

    public int GetNumFurnitures(int idx)
    {
        int num = 0;
        Furniture f = RoomEditor.GetFurniture(idx);
        if (f != null && Profiles.experienceProfile != null)
        {
            if (f.alwaysHave)
            {
                return 1;
            }
            if (f.name == "EGG" || f.name == "PHOTO")
            {
                return Profiles.experienceProfile.numLittleMen + 1;
            }
        }
        _furnitures.TryGetValue(idx, out num);
        if (f.name == "PERIMETER DEFENCE" && num > 1)
        {
            return 1;
        }
        return num;
    }

    public void SetNumFurnitures(int idx, int num)
    {
        _furnitures[idx] = num;
        _availableList = null;
    }

    public int GetTotalFurnitures()
    {
        int num = 0;
        foreach (KeyValuePair<int, int> furniture in _furnitures)
        {
            num += furniture.Value;
        }
        return num;
    }

    private static string Stringlonger(int length)
    {
        string s = "";
        for (int i = 0; i < length; i++)
        {
            s += "z";
        }
        return s;
    }

    private static string AvailFurniSortKey(Furniture x)
    {
        return x.group.name + Stringlonger(RoomEditor._furniGroupMap[x.group].IndexOf(x));
    }

    public List<Furniture> GetAvailableFurnis()
    {
        if (_availableList == null)
        {
            _availableList = new List<Furniture>();
            foreach (KeyValuePair<int, int> p in _furnitures)
            {
                if (p.Value > 0)
                {
                    Furniture f = RoomEditor.GetFurniture(p.Key);
                    if (f != null)
                    {
                        _availableList.Add(f);
                    }
                }
            }
            foreach (Furniture f2 in RoomEditor.AllFurnis())
            {
                if (f2.alwaysHave)
                {
                    _availableList.Add(f2);
                }
            }
            _availableList.Sort((Furniture x, Furniture y) => AvailFurniSortKey(x).CompareTo(AvailFurniSortKey(y)));
        }
        return _availableList;
    }

    public void SetID(string varID)
    {
        _id = varID;
    }

    private static Color PickColor()
    {
        int idx = Rando.Int(_allowedColors.Count - 1);
        Color result = _allowedColors[idx];
        _allowedColors.RemoveAt(idx);
        return result;
    }

    public static Random GetLongGenerator(ulong id)
    {
        Random gen = new Random(Math.Abs((int)(id % int.MaxValue)));
        for (int i = 0; i < (int)(id % 252); i++)
        {
            Rando.Int(100);
        }
        return gen;
    }

    public static Sprite GetEggSprite(int index = 0, ulong seed = 0uL)
    {
        if (seed == 0L && Profiles.experienceProfile != null)
        {
            seed = Profiles.experienceProfile.steamID;
        }
        Sprite s = new Sprite();
        Graphics.AddRenderTask(delegate
        {
            s.texture = GetEggTexture(index, seed);
        });
        return s;
    }

    public static Tex2D GetEggTexture(int index, ulong seed)
    {
        RenderTarget2D targ = new RenderTarget2D(16, 16, pdepth: false, RenderTargetUsage.PreserveContents);
        if (_egg == null)
        {
            _batch = new MTSpriteBatch(Graphics.device);
            _egg = new SpriteMap("online/eggWhite", 16, 16);
            _eggShine = new SpriteMap("online/eggShine", 16, 16);
            _eggBorder = new SpriteMap("online/eggBorder", 16, 16);
            _eggOuter = new SpriteMap("online/eggOuter", 16, 16);
            _eggSymbols = new SpriteMap("online/eggSymbols", 16, 16);
        }
        Random realGen = Rando.generator;
        Rando.generator = GetLongGenerator(seed);
        for (int i = 0; i < index; i++)
        {
            Rando.Int(100);
        }
        bool hasBlob = Rando.Float(1f) > 0.02f;
        bool hasBlob2 = Rando.Float(1f) > 0.9f;
        bool hasSymbol = Rando.Float(1f) > 0.4f;
        bool dontSymbolMesh = Rando.Int(8) == 1;
        _allowedColors = new List<Color>
        {
            Colors.DGBlue,
            Colors.DGYellow,
            Colors.DGRed,
            Color.White,
            new Color(48, 224, 242),
            new Color(199, 234, 96)
        };
        _allowedColors.Add(Colors.DGPink);
        _allowedColors.Add(new Color((byte)(54 + Rando.Int(200)), (byte)(54 + Rando.Int(200)), (byte)(54 + Rando.Int(200))));
        if (Rando.Int(6) == 1)
        {
            _allowedColors.Add(Colors.DGPurple);
            _allowedColors.Add(Colors.DGEgg);
        }
        else if (Rando.Int(100) == 1)
        {
            _allowedColors.Add(Colors.SuperDarkBlueGray);
            _allowedColors.Add(Colors.BlueGray);
            _allowedColors.Add(Colors.DGOrange);
            _allowedColors.Add(new Color((byte)(54 + Rando.Int(200)), (byte)(54 + Rando.Int(200)), (byte)(54 + Rando.Int(200))));
        }
        else if (Rando.Int(1200) == 1)
        {
            _allowedColors.Add(Colors.Platinum);
        }
        else if (Rando.Int(100000) == 1)
        {
            _allowedColors.Add(new Color(250, 10, 250));
        }
        else if (Rando.Int(1000000) == 1)
        {
            _allowedColors.Add(new Color(229, 245, 181));
        }
        Graphics.SetRenderTarget(targ);
        Graphics.Clear(Color.Black);
        _batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        int frame = 0;
        int symbol = 8 + Rando.Int(12);
        if (Rando.Int(100) == 1)
        {
            frame = 1;
        }
        frame = 3;
        if (Rando.Int(10) == 1)
        {
            symbol = 1;
        }
        if (Rando.Int(30) == 1)
        {
            symbol = 5;
        }
        if (Rando.Int(100) == 1)
        {
            symbol = 2;
        }
        else if (Rando.Int(200) == 1)
        {
            symbol = 3;
        }
        else if (Rando.Int(1000) == 1)
        {
            symbol = 4;
        }
        else if (Rando.Int(10000) == 1)
        {
            symbol = 7;
        }
        else if (Rando.Int(1000000) == 1)
        {
            symbol = 6;
        }
        bool hasLetter = Rando.Int(300) == 1;
        MTSpriteBatch screen = Graphics.screen;
        Graphics.screen = _batch;
        _batch.Draw(_egg.texture, new Vector2(0f, 0f), new Rectangle(frame * 16, 0f, 16f, 16f), Color.White, 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, 1f);
        if (hasSymbol)
        {
            if (hasLetter)
            {
                char c = BitmapFont._characters[Rando.Int(33, 59)];
                if (Rando.Int(5) == 1)
                {
                    c = BitmapFont._characters[Rando.Int(16, 26)];
                }
                else if (Rando.Int(50) == 1)
                {
                    c = BitmapFont._characters[Rando.Int(BitmapFont._characters.Length - 1)];
                }
                Graphics.DrawString(c.ToString() ?? "", new Vector2(4f, 6f), new Color(60, 60, 60, 200), 0.9f);
            }
            else
            {
                _batch.Draw(_eggSymbols.texture, new Vector2(0f, 0f), new Rectangle(symbol * 16, 0f, 16f, 16f), new Color(60, 60, 60, 200), 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, 0.9f);
            }
        }
        _batch.Draw(_eggOuter.texture, new Vector2(0f, 0f), new Rectangle(frame * 16, 0f, 16f, 16f), Color.White, 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, 1f);
        _batch.End();
        Graphics.screen = screen;
        Graphics.SetRenderTarget(null);
        Color[] cols = targ.GetData();
        float trig = 0.099999994f;
        Color eggColor = PickColor();
        Color blobColor = PickColor();
        PickColor();
        Color symbolColor = PickColor();
        float xOff = Rando.Float(100000f);
        float yOff = Rando.Float(100000f);
        for (int yVal = 0; yVal < targ.height; yVal++)
        {
            for (int xVal = 0; xVal < targ.width; xVal++)
            {
                float realX = (float)(xVal + 32) * 0.75f;
                int realY = yVal + 32;
                float val1 = (Noise.Generate((xOff + realX) * (trig * 1f), (yOff + (float)realY) * (trig * 1f)) + 1f) / 2f * (hasBlob ? 1f : 0f);
                float val2 = (Noise.Generate(xOff + (realX + 100f) * (trig * 2f), (yOff + (float)realY + 100f) * (trig * 2f)) + 1f) / 2f * (hasBlob2 ? 1f : 0f);
                val1 = ((!(val1 < 0.5f)) ? 1f : 0f);
                val2 = ((!(val2 < 0.5f)) ? 1f : 0f);
                Color c2 = cols[xVal + yVal * targ.width];
                float extraMul = 1f;
                if (val2 > 0f)
                {
                    extraMul = 0.9f;
                }
                if (c2.R == 0)
                {
                    cols[xVal + yVal * targ.width] = new Color(0, 0, 0, 0);
                }
                else if (c2.R < 110)
                {
                    if (dontSymbolMesh)
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)symbolColor.R * 0.6f), (byte)((float)(int)symbolColor.G * 0.6f), (byte)((float)(int)symbolColor.B * 0.6f));
                        continue;
                    }
                    float rlExtra = extraMul;
                    rlExtra = ((rlExtra != 1f) ? 1f : 0.9f);
                    if (val1 > 0f)
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)eggColor.R * 0.6f * rlExtra), (byte)((float)(int)eggColor.G * 0.6f * rlExtra), (byte)((float)(int)eggColor.B * 0.6f * rlExtra));
                    }
                    else
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)blobColor.R * 0.6f * rlExtra), (byte)((float)(int)blobColor.G * 0.6f * rlExtra), (byte)((float)(int)blobColor.B * 0.6f * rlExtra));
                    }
                }
                else if (c2.R < 120)
                {
                    if (dontSymbolMesh)
                    {
                        cols[xVal + yVal * targ.width] = new Color(symbolColor.R, symbolColor.G, symbolColor.B);
                        continue;
                    }
                    float rlExtra2 = extraMul;
                    rlExtra2 = ((rlExtra2 != 1f) ? 1f : 0.9f);
                    if (val1 > 0f)
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)eggColor.R * rlExtra2), (byte)((float)(int)eggColor.G * rlExtra2), (byte)((float)(int)eggColor.B * rlExtra2));
                    }
                    else
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)blobColor.R * rlExtra2), (byte)((float)(int)blobColor.G * rlExtra2), (byte)((float)(int)blobColor.B * rlExtra2));
                    }
                }
                else if (c2.R < byte.MaxValue)
                {
                    if (val1 > 0f)
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)blobColor.R * 0.6f * extraMul), (byte)((float)(int)blobColor.G * 0.6f * extraMul), (byte)((float)(int)blobColor.B * 0.6f * extraMul));
                    }
                    else
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)eggColor.R * 0.6f * extraMul), (byte)((float)(int)eggColor.G * 0.6f * extraMul), (byte)((float)(int)eggColor.B * 0.6f * extraMul));
                    }
                }
                else if (val1 > 0f)
                {
                    cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)blobColor.R * extraMul), (byte)((float)(int)blobColor.G * extraMul), (byte)((float)(int)blobColor.B * extraMul));
                }
                else
                {
                    cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)eggColor.R * extraMul), (byte)((float)(int)eggColor.G * extraMul), (byte)((float)(int)eggColor.B * extraMul));
                }
            }
        }
        targ.SetData(cols);
        Graphics.SetRenderTarget(targ);
        _batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        _batch.Draw(_eggShine.texture, new Vector2(0f, 0f), new Rectangle(frame * 16, 0f, 16f, 16f), Color.White, 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, 1f);
        _batch.Draw(_eggBorder.texture, new Vector2(0f, 0f), new Rectangle(frame * 16, 0f, 16f, 16f), Color.White, 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, 1f);
        _batch.End();
        Graphics.SetRenderTarget(null);
        Rando.generator = realGen;
        Tex2D tex2D = new Tex2D(targ.width, targ.height);
        tex2D.SetData(targ.GetData());
        targ.Dispose();
        return tex2D;
    }

    public static Sprite GetPaintingSprite(int index = 0, ulong seed = 0uL)
    {
        if (seed == 0L && Profiles.experienceProfile != null)
        {
            seed = Profiles.experienceProfile.steamID;
        }
        Sprite s = new Sprite();
        Graphics.AddRenderTask(delegate
        {
            GetPainting(index, seed, s);
        });
        return s;
    }

    public static void GetPainting(int index, ulong seed, Sprite spr)
    {
        Tex2D targ = new RenderTarget2D(19, 12, pdepth: false, RenderTargetUsage.PreserveContents);
        if (_easel == null)
        {
            _batch = new MTSpriteBatch(Graphics.device);
            _easel = new SpriteMap("online/easelWhite", 19, 12);
            _eggShine = new SpriteMap("online/eggShine", 16, 16);
            _eggBorder = new SpriteMap("online/eggBorder", 16, 16);
            _eggOuter = new SpriteMap("online/eggOuter", 16, 16);
            _easelSymbols = new SpriteMap("online/easelPic", 19, 12);
        }
        Random realGen = Rando.generator;
        Rando.generator = GetLongGenerator(seed);
        for (int i = 0; i < index; i++)
        {
            Rando.Int(100);
        }
        bool hasBlob = Rando.Float(1f) > 0.03f;
        bool hasBlob2 = Rando.Float(1f) > 0.8f;
        Rando.Float(1f);
        bool dontSymbolMesh = Rando.Int(6) == 1;
        _allowedColors = new List<Color>
        {
            Colors.DGBlue,
            Colors.DGYellow,
            Colors.DGRed,
            Color.White,
            new Color(48, 224, 242),
            new Color(199, 234, 96)
        };
        _allowedColors.Add(Colors.DGPink);
        _allowedColors.Add(new Color((byte)(54 + Rando.Int(200)), (byte)(54 + Rando.Int(200)), (byte)(54 + Rando.Int(200))));
        if (Rando.Int(6) == 1)
        {
            _allowedColors.Add(Colors.DGPurple);
            _allowedColors.Add(Colors.DGEgg);
        }
        else if (Rando.Int(100) == 1)
        {
            _allowedColors.Add(Colors.SuperDarkBlueGray);
            _allowedColors.Add(Colors.BlueGray);
            _allowedColors.Add(Colors.DGOrange);
            _allowedColors.Add(new Color((byte)(54 + Rando.Int(200)), (byte)(54 + Rando.Int(200)), (byte)(54 + Rando.Int(200))));
        }
        else if (Rando.Int(1200) == 1)
        {
            _allowedColors.Add(Colors.Platinum);
        }
        else if (Rando.Int(100000) == 1)
        {
            _allowedColors.Add(new Color(250, 10, 250));
        }
        else if (Rando.Int(1000000) == 1)
        {
            _allowedColors.Add(new Color(229, 245, 181));
        }
        Graphics.SetRenderTarget(targ as RenderTarget2D);
        Graphics.Clear(Color.Black);
        _batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        int symbol = 8 + Rando.Int(12);
        Rando.Int(100);
        _ = 1;
        Rando.Int(15);
        Rando.Int(300);
        MTSpriteBatch screen = Graphics.screen;
        Graphics.screen = _batch;
        _batch.Draw(_easel.texture, new Vector2(0f, 0f), null, Color.White, 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, 1f);
        _batch.Draw(_easelSymbols.texture, new Vector2(0f, 0f), new Rectangle(symbol * 19, 0f, 19f, 12f), new Color(60, 60, 60, 200), 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, 0.9f);
        _batch.End();
        Graphics.screen = screen;
        Graphics.SetRenderTarget(null);
        Color[] cols = targ.GetData();
        float trig = 0.099999994f;
        Color eggColor = PickColor();
        Color blobColor = PickColor();
        PickColor();
        Color symbolColor = PickColor();
        float xOff = Rando.Float(100000f);
        float yOff = Rando.Float(100000f);
        for (int yVal = 0; yVal < targ.height; yVal++)
        {
            for (int xVal = 0; xVal < targ.width; xVal++)
            {
                float realX = (float)(xVal + 32) * 0.75f;
                int realY = yVal + 32;
                float val1 = (Noise.Generate((xOff + realX) * (trig * 1f), (yOff + (float)realY) * (trig * 1f)) + 1f) / 2f * (hasBlob ? 1f : 0f);
                float val2 = (Noise.Generate(xOff + (realX + 100f) * (trig * 2f), (yOff + (float)realY + 100f) * (trig * 2f)) + 1f) / 2f * (hasBlob2 ? 1f : 0f);
                val1 = ((!(val1 < 0.5f)) ? 1f : 0f);
                val2 = ((!(val2 < 0.5f)) ? 1f : 0f);
                Color c = cols[xVal + yVal * targ.width];
                float extraMul = 1f;
                if (val2 > 0f)
                {
                    extraMul = 0.9f;
                }
                if (c.R == 0)
                {
                    cols[xVal + yVal * targ.width] = new Color(0, 0, 0, 0);
                }
                else if (c.R < 110)
                {
                    if (dontSymbolMesh)
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)symbolColor.R * 0.6f), (byte)((float)(int)symbolColor.G * 0.6f), (byte)((float)(int)symbolColor.B * 0.6f));
                        continue;
                    }
                    float rlExtra = extraMul;
                    rlExtra = ((rlExtra != 1f) ? 1f : 0.9f);
                    if (val1 > 0f)
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)eggColor.R * 0.6f * rlExtra), (byte)((float)(int)eggColor.G * 0.6f * rlExtra), (byte)((float)(int)eggColor.B * 0.6f * rlExtra));
                    }
                    else
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)blobColor.R * 0.6f * rlExtra), (byte)((float)(int)blobColor.G * 0.6f * rlExtra), (byte)((float)(int)blobColor.B * 0.6f * rlExtra));
                    }
                }
                else if (c.R < 120)
                {
                    if (dontSymbolMesh)
                    {
                        cols[xVal + yVal * targ.width] = new Color(symbolColor.R, symbolColor.G, symbolColor.B);
                        continue;
                    }
                    float rlExtra2 = extraMul;
                    rlExtra2 = ((rlExtra2 != 1f) ? 1f : 0.9f);
                    if (val1 > 0f)
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)eggColor.R * rlExtra2), (byte)((float)(int)eggColor.G * rlExtra2), (byte)((float)(int)eggColor.B * rlExtra2));
                    }
                    else
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)blobColor.R * rlExtra2), (byte)((float)(int)blobColor.G * rlExtra2), (byte)((float)(int)blobColor.B * rlExtra2));
                    }
                }
                else if (c.R < byte.MaxValue)
                {
                    if (val1 > 0f)
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)blobColor.R * 0.6f * extraMul), (byte)((float)(int)blobColor.G * 0.6f * extraMul), (byte)((float)(int)blobColor.B * 0.6f * extraMul));
                    }
                    else
                    {
                        cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)eggColor.R * 0.6f * extraMul), (byte)((float)(int)eggColor.G * 0.6f * extraMul), (byte)((float)(int)eggColor.B * 0.6f * extraMul));
                    }
                }
                else if (val1 > 0f)
                {
                    cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)blobColor.R * extraMul), (byte)((float)(int)blobColor.G * extraMul), (byte)((float)(int)blobColor.B * extraMul));
                }
                else
                {
                    cols[xVal + yVal * targ.width] = new Color((byte)((float)(int)eggColor.R * extraMul), (byte)((float)(int)eggColor.G * extraMul), (byte)((float)(int)eggColor.B * extraMul));
                }
            }
        }
        targ.SetData(cols);
        Rando.generator = realGen;
        spr.texture = targ;
        Tex2D tex = new Tex2D(targ.width, targ.height);
        tex.SetData(targ.GetData());
        targ.Dispose();
        spr.texture = tex;
    }

    public static byte CalculateLocalFlippers()
    {
        bool allDev = true;
        bool allComplete = true;
        foreach (ChallengeData dat in Challenges.challengesInArcade)
        {
            bool hasDev = false;
            bool hasComplete = false;
            foreach (Profile universalProfile in Profiles.universalProfileList)
            {
                ChallengeSaveData savedat = universalProfile.GetSaveData(dat.levelID, canBeNull: true);
                if (savedat != null && savedat.trophy > TrophyType.Baseline)
                {
                    hasComplete = true;
                    if (savedat.trophy == TrophyType.Developer)
                    {
                        hasDev = true;
                        break;
                    }
                }
            }
            if (!hasDev)
            {
                allDev = false;
            }
            if (!hasComplete)
            {
                allComplete = false;
            }
            if (!allComplete)
            {
                break;
            }
        }
        return (byte)((byte)((byte)((byte)((byte)((byte)((byte)((byte)((byte)((byte)(
                                                       0 |
                                                       (allDev ? 1 : 0)) << 1) |
                                                       (Global.data.onlineWins >= 50 ? 1 : 0)) << 1) |
                                                       (Global.data.matchesPlayed >= 100 ? 1 : 0)) << 1) |
                                                       (allComplete ? 1 : 0)) << 1) |
                                                       (Options.Data.shennanigans ? 1 : 0)) |
                                                       (Options.Data.rumbleIntensity > 0 ? 1u : 0u));
    }

    public bool GetLightStatus(int index)
    {
        return ((flippers >> index + 1) & 1) != 0;
    }

    public void IncrementRequestedColor()
    {
        int request;
        for (request = requestedColor + 1; request != requestedColor; request++)
        {
            if (request >= DG.MaxPlayers)
            {
                request = 0;
            }
            DuckPersona p = Persona.all.ElementAt(request);
            bool found = false;
            foreach (Profile pr in Profiles.active)
            {
                if (pr != this && pr.persona == p)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                break;
            }
        }
        requestedColor = request;
    }

    public void RecordPreviousStats()
    {
        DXMLNode e = stats.Serialize();
        prevStats = new ProfileStats();
        prevStats.Deserialize(e);
        _endOfRoundStats = null;
    }

    public void SetNetworkIndex(byte idx)
    {
        _networkIndex = idx;
    }

    public void SetFixedGhostIndex(byte idx)
    {
        _fixedGhostIndex = idx;
    }

    public void Special_SetSlotType(SlotType pType)
    {
        _slotType = pType;
    }

    public void UpdatePersona()
    {
        DuckNetwork.RequestPersona(this, desiredPersona);
    }

    public void PersonaRequestResult(DuckPersona pPersona)
    {
        persona = pPersona;
    }

    public void SetInputProfileLink(InputProfile pLink)
    {
        _inputProfile = pLink;
    }

    public void ClearCurrentGame()
    {
        _currentGame = new CurrentGame();
    }

    public void ApplyDefaults()
    {
        if (defaultTeam == null)
        {
            team = Teams.Player1;
        }
        else
        {
            team = defaultTeam;
        }
        UpdatePersona();
    }

    public Profile(string varName, InputProfile varProfile, Team varStartTeam, DuckPersona varDefaultPersona, bool network, string varID, bool pDefaultProfile)
    {
        _name = varName;
        _inputProfile = varProfile;
        if (_inputProfile != null)
        {
            _inputProfile.oldAngles = false;
        }
        if (varStartTeam != null)
        {
            varStartTeam.Join(this);
            defaultTeam = varStartTeam;
        }
        _persona = varDefaultPersona;
        defaultPersona = varDefaultPersona;
        if (varID == null)
        {
            _id = Guid.NewGuid().ToString();
        }
        else
        {
            _id = varID;
        }
        isNetworkProfile = network;
        isDefaultProfile = pDefaultProfile;
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "new Profile(" + varName + ")");
        }
    }

    public Profile(string varName, InputProfile varProfile = null, Team varStartTeam = null, DuckPersona varDefaultPersona = null, bool network = false, string varID = null)
        : this(varName, varProfile, varStartTeam, varDefaultPersona, network, varID, pDefaultProfile: false)
    {
    }
}
