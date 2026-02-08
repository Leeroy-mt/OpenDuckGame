using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class ProfileBox2 : Thing
{
    private BitmapFont _font;

    private BitmapFont _fontSmall;

    private SinWave _pulse = new SinWave(0.05f);

    private bool _playerActive;

    private int _teamSelection;

    private Sprite _plaque;

    private Sprite _onlineIcon;

    private Sprite _wirelessIcon;

    private bool _ready;

    private InputProfile _inputProfile;

    private Profile _playerProfile;

    private Sprite _doorLeft;

    private Sprite _doorRight;

    private Sprite _doorLeftBlank;

    private Sprite _doorRightBlank;

    private SpriteMap _doorSpinner;

    private SpriteMap _doorIcon;

    private Sprite _roomLeftBackground;

    private Sprite _roomLeftForeground;

    private SpriteMap _tutorialMessages;

    private Sprite _tutorialTV;

    private SpriteMap _selectConsole;

    private Sprite _consoleHighlight;

    private Sprite _avatar;

    private object avatarUser;

    private Sprite _aButton;

    private Sprite _readySign;

    public float _doorX;

    private int _currentMessage;

    private float _screenFade;

    private float _consoleFade;

    private Vector2 _consolePos = Vector2.Zero;

    private TeamProjector _projector;

    private TeamSelect2 _teamSelect;

    private Profile _defaultProfile;

    private Sprite _hostCrown;

    private Sprite _consoleFlash;

    private SpriteMap _lightBar;

    private SpriteMap _roomSwitch;

    private int _controllerIndex;

    private Duck _duck;

    private VirtualShotgun _gun;

    private RoomDefenceTurret _turret;

    private bool _doorClosing;

    private Window _window;

    private Vector2 _gunSpawnPoint = Vector2.Zero;

    private bool findDuck;

    private int hostFrames;

    public HatSelector _hatSelector;

    private DuckNetStatus _prevStatus = DuckNetStatus.EstablishingCommunicationWithServer;

    private const int kDoorIconStartButtonFrame = 10;

    public float _tooManyPulse;

    public float _noMorePulse;

    private float _prevDoorX;

    public bool playerActive => _playerActive;

    public bool ready => _ready;

    public bool doorIsOpen => _doorX > 82f;

    public Profile profile => _playerProfile;

    public int controllerIndex => _controllerIndex;

    public bool rightRoom
    {
        get
        {
            if (ControllerNumber() != 1 && ControllerNumber() != 3 && ControllerNumber() != 4 && ControllerNumber() != 5)
            {
                return ControllerNumber() == 7;
            }
            return true;
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

    public VirtualShotgun gun
    {
        get
        {
            return _gun;
        }
        set
        {
            _gun = value;
        }
    }

    public RoomDefenceTurret turret
    {
        get
        {
            return _turret;
        }
        set
        {
            _turret = value;
        }
    }

    public void SetProfile(Profile p)
    {
        if (p == null)
        {
            SetProfile(Profiles.all.ElementAt(controllerIndex));
            return;
        }
        _playerProfile = p;
        if (_duck != null)
        {
            _duck.profile = p;
        }
        if (_projector != null)
        {
            _projector.SetProfile(p);
        }
        if (_hatSelector != null)
        {
            _hatSelector.SetProfile(p);
        }
    }

    public void SetHatSelector(HatSelector s)
    {
        _hatSelector = s;
    }

    public ProfileBox2(float xpos, float ypos, InputProfile pProfile, Profile pDefaultProfile, TeamSelect2 pTeamSelect, int pIndex)
        : base(xpos, ypos)
    {
        _hostCrown = new Sprite("hostCrown");
        _hostCrown.CenterOrigin();
        _lightBar = new SpriteMap("lightBar", 2, 1);
        _lightBar.frame = 0;
        _roomSwitch = new SpriteMap("roomSwitch", 7, 5);
        _roomSwitch.frame = 0;
        _controllerIndex = pIndex;
        _font = new BitmapFont("biosFont", 8);
        _fontSmall = new BitmapFont("smallBiosFont", 7, 6);
        base.layer = Layer.Game;
        _collisionSize = new Vector2(150f, 87f);
        _plaque = new Sprite("plaque");
        _plaque.Center = new Vector2(16f, 16f);
        _inputProfile = pProfile;
        _playerProfile = pDefaultProfile;
        _teamSelection = ControllerNumber();
        _doorLeft = new Sprite("selectDoorLeftPC");
        _doorLeft.Depth = 0.905f;
        _doorRight = new Sprite("selectDoorRight");
        _doorRight.Depth = 0.9f;
        _doorLeftBlank = new Sprite("selectDoorLeftBlank");
        _doorLeftBlank.Depth = 0.905f;
        _doorRightBlank = new Sprite("selectDoorRightBlank");
        _doorRightBlank.Depth = 0.9f;
        _doorSpinner = new SpriteMap("doorSpinner", 25, 25);
        _doorSpinner.AddAnimation("spin", 0.2f, true, 0, 1, 2, 3, 4, 5, 6, 7);
        _doorSpinner.SetAnimation("spin");
        _doorIcon = new SpriteMap("doorSpinner", 25, 25);
        _onlineIcon = new Sprite("gameServerOnline");
        _wirelessIcon = new Sprite("gameServerWireless");
        _teamSelect = pTeamSelect;
        _defaultProfile = pDefaultProfile;
        if (rightRoom)
        {
            _roomSwitch = new SpriteMap("roomSwitchRight", 7, 5);
            _roomSwitch.frame = 0;
            _roomLeftBackground = new Sprite("rightRoomBackground");
            _roomLeftForeground = new Sprite("rightRoomForeground");
            Level.Add(new InvisibleBlock(base.X - 2f + 142f - 138f, base.Y + 69f, 138f, 16f, PhysicsMaterial.Metal));
            Level.Add(new InvisibleBlock(base.X - 2f + 142f - 138f, base.Y - 11f, 138f, 12f, PhysicsMaterial.Metal));
            Level.Add(new InvisibleBlock(base.X + 142f - 98f - 46f, base.Y + 56f, 50f, 16f, PhysicsMaterial.Metal));
            Level.Add(new InvisibleBlock(base.X + 142f + 2f - 8f, base.Y, 8f, 100f, PhysicsMaterial.Metal));
            Level.Add(new InvisibleBlock(base.X + 142f - 136f - 9f, base.Y, 8f, 25f, PhysicsMaterial.Metal));
            ScaffoldingTileset obj = new ScaffoldingTileset(base.X + 126f, base.Y + 63f)
            {
                neverCheap = true
            };
            Level.Add(obj);
            obj.Depth = -0.5f;
            obj.PlaceBlock();
            obj.UpdateNubbers();
            Level.Add(new Platform(base.X + 49f, base.Y + 56f, 3f, 5f));
            _readySign = new Sprite("readyLeft");
        }
        else
        {
            _roomLeftBackground = new Sprite("leftRoomBackground");
            _roomLeftForeground = new Sprite("leftRoomForeground");
            Level.Add(new InvisibleBlock(base.X + 2f, base.Y + 69f, 138f, 16f, PhysicsMaterial.Metal));
            Level.Add(new InvisibleBlock(base.X + 2f, base.Y - 11f, 138f, 12f, PhysicsMaterial.Metal));
            Level.Add(new InvisibleBlock(base.X + 92f, base.Y + 56f, 50f, 16f, PhysicsMaterial.Metal));
            Level.Add(new InvisibleBlock(base.X - 4f, base.Y, 8f, 100f, PhysicsMaterial.Metal));
            Level.Add(new InvisibleBlock(base.X + 135f, base.Y, 8f, 25f, PhysicsMaterial.Metal));
            ScaffoldingTileset obj2 = new ScaffoldingTileset(base.X + 14f, base.Y + 63f)
            {
                neverCheap = true
            };
            Level.Add(obj2);
            obj2.Depth = -0.5f;
            obj2.PlaceBlock();
            obj2.UpdateNubbers();
            Level.Add(new Platform(base.X + 89f, base.Y + 56f, 3f, 5f));
            _readySign = new Sprite("readyRight");
        }
        if (rightRoom)
        {
            _gunSpawnPoint = new Vector2(base.X + 142f - 118f, base.Y + 50f);
        }
        else
        {
            _gunSpawnPoint = new Vector2(base.X + 113f, base.Y + 50f);
        }
        _readySign.Depth = 0.2f;
        _roomLeftBackground.Depth = -0.85f;
        _roomLeftForeground.Depth = 0.1f;
        _tutorialMessages = new SpriteMap("tutorialScreensPC", 53, 30);
        _aButton = new Sprite("aButton");
        _tutorialTV = new Sprite("tutorialTV");
        _consoleHighlight = new Sprite("consoleHighlight");
        _consoleFlash = new Sprite("consoleFlash");
        _consoleFlash.CenterOrigin();
        _selectConsole = new SpriteMap("selectConsole", 20, 19);
        _selectConsole.AddAnimation("idle", 1f, true, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        _selectConsole.SetAnimation("idle");
        if (Network.isServer)
        {
            _hatSelector = new HatSelector(base.X, base.Y, _playerProfile, this);
            _hatSelector.profileBoxNumber = (sbyte)pIndex;
            Level.Add(_hatSelector);
        }
        if (rightRoom)
        {
            _projector = new TeamProjector(base.X + 80f, base.Y + 68f, _playerProfile);
            Level.Add(new ItemSpawner(base.X + 26f, base.Y + 54f));
        }
        else
        {
            _projector = new TeamProjector(base.X + 59f, base.Y + 68f, _playerProfile);
            Level.Add(new ItemSpawner(base.X + 112f, base.Y + 54f));
        }
        Level.Add(_projector);
    }

    public override void Initialize()
    {
        if (Network.isServer && _playerProfile != null && _playerProfile.connection != null && Network.isActive)
        {
            Spawn();
        }
        base.Initialize();
    }

    public void ReturnControl()
    {
        if (_duck != null)
        {
            _duck.immobilized = false;
        }
    }

    private int ControllerNumber()
    {
        return _controllerIndex;
    }

    private void SelectTeam()
    {
        Teams.all[_teamSelection].Join(_playerProfile);
        if (_playerProfile.inputProfile == null)
        {
            _playerProfile.inputProfile = _inputProfile;
        }
    }

    public void ChangeProfile(Profile p)
    {
        if (p == null)
        {
            p = _defaultProfile;
        }
        if (p != _playerProfile)
        {
            if (!Network.isActive && !p.isNetworkProfile)
            {
                for (int i = 0; i < Profile.defaultProfileMappings.Count; i++)
                {
                    if (Profile.defaultProfileMappings[i] == p)
                    {
                        Profile.defaultProfileMappings[i] = Profiles.universalProfileList.ElementAt(i);
                    }
                }
                Profile.defaultProfileMappings[_controllerIndex] = p;
                if (_teamSelect != null)
                {
                    foreach (ProfileBox2 pb in _teamSelect._profiles)
                    {
                        if (pb != this && pb.profile == p)
                        {
                            pb.SetProfile(Profile.defaultProfileMappings[pb._controllerIndex]);
                        }
                    }
                }
            }
            Team t = _playerProfile.team;
            if (t != null)
            {
                _playerProfile.team.Leave(_playerProfile);
                t.Join(p);
            }
            if (!Network.isActive)
            {
                p.inputProfile = _playerProfile.inputProfile;
            }
            p.UpdatePersona();
            if (!Network.isActive)
            {
                _playerProfile.inputProfile = null;
            }
            _playerProfile = p;
            if (_duck != null)
            {
                if (_duck.profile.team != null)
                {
                    Team team = _duck.profile.team;
                    team.Leave(_duck.profile);
                    team.Join(_playerProfile);
                }
                _duck.profile = _playerProfile;
                if (Network.isActive && DuckNetwork.IndexOf(_playerProfile) >= 0)
                {
                    _duck.netProfileIndex = (byte)DuckNetwork.IndexOf(_playerProfile);
                }
            }
            _projector.SetProfile(p);
            _hatSelector.SetProfile(p);
        }
        OpenCorners();
    }

    public void OpenCorners()
    {
        if (!(Level.current is ArcadeLevel))
        {
            return;
        }
        HUD.CloseAllCorners();
        HUD.AddCornerCounter(HUDCorner.BottomRight, "@TICKET@ ", new FieldBinding(_playerProfile, "ticketCount"), 0, animateCount: true);
        List<ChallengeSaveData> allSaveData = Challenges.GetAllSaveData(_playerProfile);
        Dictionary<TrophyType, int> dic = new Dictionary<TrophyType, int>
        {
            {
                TrophyType.Bronze,
                0
            },
            {
                TrophyType.Silver,
                0
            },
            {
                TrophyType.Gold,
                0
            },
            {
                TrophyType.Platinum,
                0
            },
            {
                TrophyType.Developer,
                0
            }
        };
        foreach (ChallengeSaveData d in allSaveData)
        {
            if (d.trophy != TrophyType.Baseline)
            {
                dic[d.trophy]++;
            }
        }
        string text = "";
        if (dic[TrophyType.Bronze] > 0 || (dic[TrophyType.Silver] == 0 && dic[TrophyType.Gold] == 0 && dic[TrophyType.Platinum] == 0 && dic[TrophyType.Developer] == 0))
        {
            text = text + "@BRONZE@" + dic[TrophyType.Bronze];
        }
        if (dic[TrophyType.Silver] > 0)
        {
            text = text + " @SILVER@" + dic[TrophyType.Silver];
        }
        if (dic[TrophyType.Gold] > 0)
        {
            text = text + " @GOLD@" + dic[TrophyType.Gold];
        }
        if (dic[TrophyType.Platinum] > 0)
        {
            text = text + " @PLATINUM@" + dic[TrophyType.Platinum];
        }
        if (dic[TrophyType.Developer] > 0)
        {
            text = text + " @DEVELOPER@" + dic[TrophyType.Developer];
        }
        HUD.AddCornerControl(HUDCorner.TopRight, text);
    }

    public void CloseDoor()
    {
        if (_duck != null)
        {
            _duck.immobilized = true;
        }
        _playerActive = false;
        if (_doorX == 0f)
        {
            OnDoorClosed();
        }
    }

    public void OnDoorClosed()
    {
        _doorClosing = true;
        if (Network.isServer)
        {
            if (_playerProfile.team != null)
            {
                _playerProfile.team.Leave(_playerProfile);
            }
            _playerProfile = Profiles.defaultProfiles[ControllerNumber()];
            _teamSelection = ControllerNumber();
            SelectTeam();
            _playerProfile.team.Leave(_playerProfile);
            if (_duck != null)
            {
                _duck.profile = _playerProfile;
                if (_duck.GetEquipment(typeof(Hat)) is Hat existing)
                {
                    _duck.Unequip(existing);
                    Level.Remove(existing);
                }
            }
            foreach (RoomDefenceTurret item in Level.CheckRectAll<RoomDefenceTurret>(base.topLeft, base.bottomRight))
            {
                Level.Remove(item);
            }
            _turret = null;
        }
        Despawn();
        _doorClosing = false;
    }

    public void Spawn()
    {
        profile.UpdatePersona();
        if (_duck != null)
        {
            _teamSelection = ControllerNumber();
            SelectTeam();
            ReturnControl();
            return;
        }
        _gun = new VirtualShotgun(_gunSpawnPoint.X, _gunSpawnPoint.Y);
        _gun.roomIndex = (byte)_controllerIndex;
        Level.Add(_gun);
        if (rightRoom)
        {
            _duck = new Duck(base.X + 142f - 48f, base.Y + 40f, _playerProfile);
            _window = new Window(base.X + 142f - 141f, base.Y + 49f);
            _window.noframe = true;
            Level.Add(_window);
        }
        else
        {
            _duck = new Duck(base.X + 48f, base.Y + 40f, _playerProfile);
            _window = new Window(base.X + 139f, base.Y + 49f);
            _window.noframe = true;
            Level.Add(_window);
        }
        foreach (RoomDefenceTurret item in Level.CheckRectAll<RoomDefenceTurret>(base.topLeft, base.bottomRight))
        {
            Level.Remove(item);
        }
        _turret = null;
        Level.Add(_duck);
        if (_duck != null && _duck.HasEquipment(typeof(TeamHat)))
        {
            _hatSelector.hat = _duck.GetEquipment(typeof(TeamHat)) as TeamHat;
        }
    }

    public void Despawn()
    {
        if (!Network.isServer)
        {
            return;
        }
        if (_duck != null)
        {
            Thing.Fondle(_duck, DuckNetwork.localConnection);
            Level.Remove(_duck);
            if (!Network.isActive && _duck.ragdoll != null)
            {
                Level.Remove(_duck.ragdoll);
            }
        }
        if (_gun != null)
        {
            Thing.Fondle(_gun, DuckNetwork.localConnection);
            Level.Remove(_gun);
        }
        foreach (Window item in Level.CheckRectAll<Window>(base.topLeft, base.bottomRight))
        {
            item.lobbyRemoving = true;
            Thing.Fondle(item, DuckNetwork.localConnection);
            Level.Remove(item);
        }
        _window = null;
        _duck = null;
        _gun = null;
    }

    public void OpenDoor()
    {
        _playerActive = true;
        SelectTeam();
        if (_duck != null)
        {
            _duck.immobilized = false;
        }
    }

    public void PrepareDoor()
    {
        DevConsole.Log(DCSection.DuckNet, "|DGGREEN|Preparing Door..");
        if (Network.isServer)
        {
            if (_duck == null)
            {
                DevConsole.Log(DCSection.DuckNet, "|DGGREEN|Duckspawn!");
                Spawn();
            }
            else
            {
                DevConsole.Log(DCSection.DuckNet, "|DGRED|Duck was not NULL!");
            }
        }
    }

    public void OpenDoor(Duck d)
    {
        _duck = d;
    }

    public override void Update()
    {
        if (Network.isActive && Network.isServer && _duck != null && profile.connection == null)
        {
            Despawn();
        }
        if (Network.isServer && Network.isActive && _hatSelector != null && _hatSelector.isServerForObject)
        {
            _hatSelector.profileBoxNumber = (sbyte)_controllerIndex;
        }
        if (hostFrames > 0)
        {
            hostFrames--;
            if (hostFrames == 0)
            {
                TeamSelect2.FillMatchmakingProfiles();
                if (NetworkDebugger.CurrentServerIndex() < 0)
                {
                    Network.lanMode = true;
                    DuckNetwork.Host(4, NetworkLobbyType.LAN);
                    (base.level as TeamSelect2).NetworkDebuggerPrepare();
                }
                else
                {
                    Network.lanMode = true;
                    DuckNetwork.Join("test", "netdebug");
                    Level.current = new ConnectingScreen();
                }
            }
        }
        if (Network.isActive)
        {
            if (!Network.isServer && profile.networkStatus != DuckNetStatus.Disconnected)
            {
                _duck = null;
                foreach (Duck d in Level.current.things[typeof(Duck)])
                {
                    if (d.netProfileIndex == _controllerIndex)
                    {
                        _duck = d;
                    }
                }
            }
            _playerActive = profile.networkStatus == DuckNetStatus.Connected;
        }
        if (_duck != null && _duck.inputProfile != null)
        {
            _inputProfile = _duck.inputProfile;
        }
        if (_hatSelector == null)
        {
            return;
        }
        if (_hatSelector.open && profile.team == null)
        {
            _hatSelector.Reset();
        }
        foreach (VirtualShotgun s in Level.current.things[typeof(VirtualShotgun)])
        {
            if (s.roomIndex == _controllerIndex && s.isServerForObject && s.Alpha <= 0f)
            {
                s.Position = _gunSpawnPoint;
                s.Alpha = 1f;
                s.vSpeed = -1f;
            }
        }
        bool joinedGame = false;
        if (_teamSelect != null && (!Network.isActive || _hatSelector.connection == DuckNetwork.localConnection) && !Network.isActive && _inputProfile.JoinGamePressed() && !_hatSelector.open && (!NetworkDebugger.enabled || (NetworkDebugger._instances[NetworkDebugger.currentIndex].hover && (Input.Down("SHOOT") || Keyboard.Down(Keys.LeftShift) || Keyboard.Down(Keys.RightShift))) || NetworkDebugger.letJoin))
        {
            if (!_playerActive)
            {
                OpenDoor();
                joinedGame = true;
            }
            if (NetworkDebugger.letJoin && !Input.Down("SHOOT") && !Keyboard.Down(Keys.LeftShift) && !Keyboard.Down(Keys.RightShift))
            {
                NetworkDebugger.letJoin = false;
                hostFrames = 2;
            }
        }
        if (_teamSelect != null && !ready && !Network.isActive && _inputProfile.Pressed("START") && !joinedGame)
        {
            _teamSelect.OpenPauseMenu(this);
        }
        if (!Network.isActive && _duck != null && !_duck.immobilized)
        {
            _playerActive = true;
        }
        if (Network.isServer && _duck == null && _playerProfile.team != null && (!Network.isActive || _playerProfile.connection != null))
        {
            int index = 0;
            using (List<Team>.Enumerator enumerator2 = Teams.all.GetEnumerator())
            {
                while (enumerator2.MoveNext() && !(enumerator2.Current.name == _playerProfile.team.name))
                {
                    index++;
                }
            }
            _teamSelection = Teams.all.IndexOf(_playerProfile.team);
            _playerActive = true;
            SelectTeam();
            Spawn();
        }
        _ready = doorIsOpen && _duck != null && (_duck.dead || _duck.beammode || _duck.cameraPosition.Y < -100f || _duck.cameraPosition.Y > 400f);
        if (_duck != null)
        {
            _currentMessage = 0;
            bool closeToConsole = (_duck.Position - _consolePos).Length() < 20f;
            _consoleFade = Lerp.Float(_consoleFade, closeToConsole ? 1f : 0f, 0.1f);
            if (_teamSelect != null && closeToConsole)
            {
                _currentMessage = 4;
                _duck.canFire = false;
                if (_duck.isServerForObject && doorIsOpen && _inputProfile.Pressed("SHOOT") && !_hatSelector.open && _hatSelector.fade < 0.01f)
                {
                    _duck.immobilized = true;
                    _hatSelector.Open(_playerProfile);
                    _duck.Fondle(_hatSelector);
                    SFX.Play("consoleOpen", 0.5f);
                }
            }
            else
            {
                _duck.canFire = true;
            }
            if (_hatSelector.hat != null && _hatSelector.hat.Alpha < 0.01f && !_duck.HasEquipment(_hatSelector.hat))
            {
                _hatSelector.hat.Alpha = 1f;
                _duck.Equip(_hatSelector.hat, makeSound: false);
            }
            if (ready)
            {
                _currentMessage = 3;
                _readySign.color = Lerp.Color(_readySign.color, Color.LimeGreen, 0.1f);
                if (_hatSelector.hat != null && !_duck.HasEquipment(_hatSelector.hat))
                {
                    _hatSelector.hat.Alpha = 1f;
                    _duck.Equip(_hatSelector.hat, makeSound: false);
                }
            }
            else
            {
                _readySign.color = Lerp.Color(_readySign.color, Color.Red, 0.1f);
                if (_gun != null && (_gun.Position - _duck.Position).Length() < 30f)
                {
                    if (_duck.holdObject != null)
                    {
                        _currentMessage = 2;
                        if (closeToConsole)
                        {
                            _currentMessage = 5;
                        }
                    }
                    else
                    {
                        _currentMessage = 1;
                    }
                }
            }
        }
        _prevDoorX = _doorX;
        bool doorCheck = _playerActive && (_playerProfile.team != null || (Network.isActive && Network.connections.Count == 0));
        if (_playerProfile.connection != null && _playerProfile.connection.levelIndex != DuckNetwork.levelIndex)
        {
            doorCheck = false;
        }
        if (doorCheck && _hatSelector != null && _hatSelector.isServerForObject)
        {
            if (profile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("PERIMETER DEFENCE").index) > 0)
            {
                if (_turret == null)
                {
                    foreach (FurniturePosition p in profile.furniturePositions)
                    {
                        if (RoomEditor.GetFurniture(p.id).name == "PERIMETER DEFENCE")
                        {
                            Vector2 pos = new Vector2(p.x - 2, p.y + 2);
                            if (rightRoom)
                            {
                                pos.X = (float)RoomEditor.roomSize - pos.X;
                                pos.X += 2f;
                            }
                            pos += Position;
                            if (pos.X > base.X && pos.Y > base.Y && pos.X < base.X + (float)RoomEditor.roomSize && pos.Y < base.Y + (float)RoomEditor.roomSize)
                            {
                                _turret = new RoomDefenceTurret(pos, duck);
                                _turret.offDir = (sbyte)((!rightRoom) ? 1 : (-1));
                                Level.Add(_turret);
                            }
                            break;
                        }
                    }
                }
            }
            else if (_turret != null)
            {
                Level.Remove(_turret);
                _turret = null;
            }
        }
        if (_turret != null)
        {
            _turret._friendly = duck;
        }
        _doorX = Maths.LerpTowards(_doorX, doorCheck ? 83f : 0f, 4f);
        if ((Network.isActive && ((profile.networkStatus == DuckNetStatus.Disconnected && _prevStatus != DuckNetStatus.Disconnected) || profile.slotType == SlotType.Spectator)) || (_doorX == 0f && _prevDoorX != 0f))
        {
            OnDoorClosed();
        }
        if (_playerActive && controllerIndex > 3 && !(Level.current.camera is FollowCam))
        {
            TeamSelect2.growCamera = true;
        }
        if (_currentMessage != _tutorialMessages.frame)
        {
            _screenFade = Maths.LerpTowards(_screenFade, 0f, 0.15f);
            if (_screenFade < 0.01f)
            {
                _tutorialMessages.frame = _currentMessage;
            }
        }
        else
        {
            _screenFade = Maths.LerpTowards(_screenFade, 1f, 0.15f);
        }
        _prevStatus = profile.networkStatus;
    }

    public override void Draw()
    {
        if (_hatSelector != null && _hatSelector.fadeVal > 0.9f && _hatSelector._roomEditor._mode != REMode.Place)
        {
            _projector.visible = false;
            if (_duck != null)
            {
                _duck.mindControl = new InputProfile();
            }
            return;
        }
        if (_duck != null)
        {
            _duck.mindControl = null;
        }
        _projector.visible = true;
        if (_tooManyPulse > 0.01f)
        {
            Graphics.DrawStringOutline("ROOM FULL", Position + new Vector2(0f, 36f), Color.Red * _tooManyPulse, Color.Black * _tooManyPulse, 0.95f, 2f);
        }
        if (_noMorePulse > 0.01f)
        {
            Graphics.DrawStringOutline(" NO MORE ", Position + new Vector2(0f, 36f), Color.Red * _noMorePulse, Color.Black * _noMorePulse, 0.95f, 2f);
        }
        _tooManyPulse = Lerp.Float(_tooManyPulse, 0f, 0.05f);
        _noMorePulse = Lerp.Float(_noMorePulse, 0f, 0.05f);
        bool connecting = profile.networkStatus != DuckNetStatus.Disconnected;
        if (_doorX < 82f)
        {
            Sprite leftDoor = _doorLeft;
            Sprite rightDoor = _doorRight;
            bool full = profile.slotType == SlotType.Closed;
            bool friendo = profile.slotType == SlotType.Friend;
            bool invite = profile.slotType == SlotType.Invite;
            bool reserved = profile.slotType == SlotType.Reserved;
            bool local = profile.slotType == SlotType.Local;
            if (Network.isActive)
            {
                leftDoor = _doorLeftBlank;
                rightDoor = _doorRightBlank;
            }
            else
            {
                full = false;
                friendo = false;
                invite = false;
                reserved = false;
                connecting = false;
            }
            leftDoor = _doorLeftBlank;
            rightDoor = _doorRightBlank;
            if (rightRoom)
            {
                Graphics.Draw(sourceRectangle: new Rectangle((int)_doorX, 0f, leftDoor.width, _doorLeft.height), g: leftDoor, x: base.X - 1f, y: base.Y);
                Graphics.Draw(sourceRectangle: new Rectangle((int)(0f - _doorX), 0f, _doorRight.width, _doorRight.height), g: rightDoor, x: base.X - 1f + 68f, y: base.Y);
                if (_doorX == 0f)
                {
                    _fontSmall.Depth = leftDoor.Depth + 10;
                    if (!Network.isActive || (local && Network.isServer))
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 10;
                        Graphics.Draw(_doorIcon, (int)base.X + 57, base.Y + 31f);
                        _fontSmall.DrawOutline("PRESS", new Vector2(base.X + 19f, base.Y + 40f), Color.White, Colors.BlueGray, leftDoor.Depth + 10);
                        _fontSmall.DrawOutline("START", new Vector2(base.X + 85f, base.Y + 40f), Color.White, Colors.BlueGray, rightDoor.Depth + 10);
                    }
                    else if (full)
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 8;
                        Graphics.Draw(_doorIcon, (int)base.X + 57, base.Y + 31f);
                    }
                    else if (connecting)
                    {
                        _doorSpinner.Depth = leftDoor.Depth + 10;
                        Graphics.Draw(_doorSpinner, (int)base.X + 57, base.Y + 31f);
                    }
                    else if (friendo)
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 11;
                        Graphics.Draw(_doorIcon, (int)base.X + 57, base.Y + 31f);
                        _fontSmall.DrawOutline("PALS", new Vector2(base.X + 22f, base.Y + 40f), Color.White, Colors.BlueGray, leftDoor.Depth + 10);
                        _fontSmall.DrawOutline("ONLY", new Vector2(base.X + 90f, base.Y + 40f), Color.White, Colors.BlueGray, rightDoor.Depth + 10);
                    }
                    else if (invite)
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 12;
                        Graphics.Draw(_doorIcon, (int)base.X + 57, base.Y + 31f);
                        _fontSmall.DrawOutline("VIPS", new Vector2(base.X + 22f, base.Y + 40f), Color.White, Colors.BlueGray, leftDoor.Depth + 10);
                        _fontSmall.DrawOutline("ONLY", new Vector2(base.X + 90f, base.Y + 40f), Color.White, Colors.BlueGray, rightDoor.Depth + 10);
                    }
                    else if (reserved && profile.reservedUser != null)
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 12;
                        Graphics.Draw(_doorIcon, (int)base.X + 58, base.Y + 31f);
                        float wide = 120f;
                        float xpos = base.X + 10f;
                        Graphics.DrawRect(new Vector2(xpos, base.Y + 35f), new Vector2(xpos + wide, base.Y + 52f), Color.Black, leftDoor.Depth + 20);
                        string t = "WAITING FOR";
                        _fontSmall.Draw(t, new Vector2(xpos + wide / 2f - _fontSmall.GetWidth(t) / 2f, base.Y + 36f), Color.White, leftDoor.Depth + 30);
                        t = profile.nameUI;
                        if (t.Length > 16)
                        {
                            t = t.Substring(0, 16);
                        }
                        _fontSmall.Draw(t, new Vector2(xpos + wide / 2f - _fontSmall.GetWidth(t) / 2f, base.Y + 44f), Color.White, leftDoor.Depth + 30);
                    }
                    else if (local)
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 13;
                        Graphics.Draw(_doorIcon, (int)base.X + 57, base.Y + 31f);
                        _fontSmall.DrawOutline("HOST", new Vector2(base.X + 22f, base.Y + 40f), Color.White, Colors.BlueGray, leftDoor.Depth + 10);
                        _fontSmall.DrawOutline("SLOT", new Vector2(base.X + 90f, base.Y + 40f), Color.White, Colors.BlueGray, rightDoor.Depth + 10);
                    }
                    else
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 9;
                        Graphics.Draw(_doorIcon, (int)base.X + 57, base.Y + 31f);
                        _fontSmall.DrawOutline("OPEN", new Vector2(base.X + 22f, base.Y + 40f), Color.White, Colors.BlueGray, leftDoor.Depth + 10);
                        _fontSmall.DrawOutline("SLOT", new Vector2(base.X + 90f, base.Y + 40f), Color.White, Colors.BlueGray, rightDoor.Depth + 10);
                    }
                }
            }
            else
            {
                Graphics.Draw(sourceRectangle: new Rectangle((int)_doorX, 0f, _doorLeft.width, _doorLeft.height), g: leftDoor, x: base.X, y: base.Y);
                Graphics.Draw(sourceRectangle: new Rectangle((int)(0f - _doorX), 0f, _doorRight.width, _doorRight.height), g: rightDoor, x: base.X + 68f, y: base.Y);
                if (_doorX == 0f)
                {
                    _fontSmall.Depth = leftDoor.Depth + 10;
                    if (!Network.isActive || (local && Network.isServer))
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 10;
                        Graphics.Draw(_doorIcon, (int)base.X + 58, base.Y + 31f);
                        _fontSmall.DrawOutline("PRESS", new Vector2(base.X + 20f, base.Y + 40f), Color.White, Colors.BlueGray, leftDoor.Depth + 10);
                        _fontSmall.DrawOutline("START", new Vector2(base.X + 86f, base.Y + 40f), Color.White, Colors.BlueGray, rightDoor.Depth + 10);
                    }
                    else if (full)
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 8;
                        Graphics.Draw(_doorIcon, (int)base.X + 58, base.Y + 31f);
                    }
                    else if (connecting)
                    {
                        _doorSpinner.Depth = leftDoor.Depth + 10;
                        Graphics.Draw(_doorSpinner, (int)base.X + 58, base.Y + 31f);
                    }
                    else if (friendo)
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 11;
                        Graphics.Draw(_doorIcon, (int)base.X + 58, base.Y + 31f);
                        _fontSmall.DrawOutline("PALS", new Vector2(base.X + 22f, base.Y + 40f), Color.White, Colors.BlueGray, leftDoor.Depth + 10);
                        _fontSmall.DrawOutline("ONLY", new Vector2(base.X + 90f, base.Y + 40f), Color.White, Colors.BlueGray, rightDoor.Depth + 10);
                    }
                    else if (invite)
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 12;
                        Graphics.Draw(_doorIcon, (int)base.X + 58, base.Y + 31f);
                        _fontSmall.DrawOutline("VIPS", new Vector2(base.X + 22f, base.Y + 40f), Color.White, Colors.BlueGray, leftDoor.Depth + 10);
                        _fontSmall.DrawOutline("ONLY", new Vector2(base.X + 90f, base.Y + 40f), Color.White, Colors.BlueGray, rightDoor.Depth + 10);
                    }
                    else if (reserved && profile.reservedUser != null)
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 12;
                        Graphics.Draw(_doorIcon, (int)base.X + 58, base.Y + 31f);
                        float wide2 = 120f;
                        float xpos2 = base.X + 10f;
                        Graphics.DrawRect(new Vector2(xpos2, base.Y + 35f), new Vector2(xpos2 + wide2, base.Y + 52f), Color.Black, leftDoor.Depth + 20);
                        string t2 = "WAITING FOR";
                        _fontSmall.Draw(t2, new Vector2(xpos2 + wide2 / 2f - _fontSmall.GetWidth(t2) / 2f, base.Y + 36f), Color.White, leftDoor.Depth + 30);
                        t2 = profile.nameUI;
                        if (t2.Length > 16)
                        {
                            t2 = t2.Substring(0, 16);
                        }
                        _fontSmall.Draw(t2, new Vector2(xpos2 + wide2 / 2f - _fontSmall.GetWidth(t2) / 2f, base.Y + 44f), Color.White, leftDoor.Depth + 30);
                    }
                    else if (local)
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 13;
                        Graphics.Draw(_doorIcon, (int)base.X + 58, base.Y + 31f);
                        _fontSmall.DrawOutline("HOST", new Vector2(base.X + 22f, base.Y + 40f), Color.White, Colors.BlueGray, leftDoor.Depth + 10);
                        _fontSmall.DrawOutline("SLOT", new Vector2(base.X + 90f, base.Y + 40f), Color.White, Colors.BlueGray, rightDoor.Depth + 10);
                    }
                    else
                    {
                        _doorIcon.Depth = leftDoor.Depth + 10;
                        _doorIcon.frame = 9;
                        Graphics.Draw(_doorIcon, (int)base.X + 58, base.Y + 31f);
                        _fontSmall.DrawOutline("OPEN", new Vector2(base.X + 22f, base.Y + 40f), Color.White, Colors.BlueGray, leftDoor.Depth + 10);
                        _fontSmall.DrawOutline("SLOT", new Vector2(base.X + 90f, base.Y + 40f), Color.White, Colors.BlueGray, rightDoor.Depth + 10);
                    }
                }
            }
        }
        if (_playerProfile.team == null || !(_doorX > 0f))
        {
            return;
        }
        Furniture theme = null;
        if (Profiles.experienceProfile != null)
        {
            bool shouldDisplayFurniture = true;
            if (Network.isActive && profile.connection != DuckNetwork.localConnection && (profile.ParentalControlsActive || ParentalControls.AreParentalControlsActive() || profile.muteRoom))
            {
                shouldDisplayFurniture = false;
            }
            if (shouldDisplayFurniture)
            {
                List<FurniturePosition> letterMapLeft = new List<FurniturePosition>();
                List<FurniturePosition> letterMapRight = new List<FurniturePosition>();
                foreach (FurniturePosition p in profile.furniturePositions)
                {
                    Furniture f = RoomEditor.GetFurniture(p.id);
                    if (f == null)
                    {
                        continue;
                    }
                    if (f.group == Furniture.Characters)
                    {
                        if (!p.flip && rightRoom)
                        {
                            p.furniMapping = f;
                            letterMapRight.Add(p);
                            continue;
                        }
                        if (p.flip && !rightRoom)
                        {
                            p.furniMapping = f;
                            letterMapLeft.Add(p);
                            continue;
                        }
                    }
                    if (f.type == FurnitureType.Theme)
                    {
                        theme = f;
                    }
                    else if (f.type != FurnitureType.Font)
                    {
                        f.sprite.Depth = -0.56f + (float)f.deep * 0.001f;
                        f.sprite.frame = p.variation;
                        Vector2 pos = new Vector2((int)p.x, (int)p.y);
                        f.sprite.flipH = p.flip;
                        if (rightRoom)
                        {
                            pos.X = (float)RoomEditor.roomSize - pos.X;
                            f.sprite.flipH = !f.sprite.flipH;
                            pos.X -= 1f;
                        }
                        pos += Position;
                        if (f.visible)
                        {
                            f.Draw(pos, f.sprite.Depth, p.variation, profile);
                        }
                        f.sprite.frame = 0;
                        f.sprite.flipH = false;
                    }
                }
                if (letterMapRight.Count > 0)
                {
                    IOrderedEnumerable<FurniturePosition> letters = letterMapRight.OrderBy((FurniturePosition furni) => furni.x + furni.y * 100);
                    IEnumerable<FurniturePosition> lettersReverse = letterMapRight.OrderBy((FurniturePosition furni) => -furni.x + furni.y * 100);
                    int idx = 0;
                    for (int i = 0; i < letters.Count(); i++)
                    {
                        FurniturePosition p2 = letters.ElementAt(i);
                        Furniture f2 = p2.furniMapping;
                        FurniturePosition opposite = lettersReverse.ElementAt(idx);
                        Furniture fOpposite = opposite.furniMapping;
                        f2.sprite.Depth = -0.56f + (float)fOpposite.deep * 0.001f;
                        f2.sprite.frame = p2.variation;
                        Vector2 pos2 = new Vector2((int)opposite.x, (int)opposite.y);
                        f2.sprite.flipH = p2.flip;
                        if (rightRoom)
                        {
                            pos2.X = (float)RoomEditor.roomSize - pos2.X;
                            f2.sprite.flipH = !f2.sprite.flipH;
                            pos2.X -= 1f;
                        }
                        pos2 += Position;
                        if (f2.visible)
                        {
                            f2.Draw(pos2, f2.sprite.Depth, f2.sprite.frame, profile);
                        }
                        f2.sprite.frame = 0;
                        f2.sprite.flipH = false;
                        idx++;
                    }
                }
                if (letterMapLeft.Count > 0)
                {
                    IOrderedEnumerable<FurniturePosition> letters2 = letterMapRight.OrderBy((FurniturePosition furni) => -furni.x + furni.y * 100);
                    IEnumerable<FurniturePosition> lettersReverse2 = letterMapRight.OrderBy((FurniturePosition furni) => furni.x + furni.y * 100);
                    int idx2 = 0;
                    for (int i2 = 0; i2 < letters2.Count(); i2++)
                    {
                        FurniturePosition p3 = letters2.ElementAt(i2);
                        Furniture f3 = p3.furniMapping;
                        FurniturePosition opposite2 = lettersReverse2.ElementAt(idx2);
                        Furniture fOpposite2 = opposite2.furniMapping;
                        f3.sprite.Depth = -0.56f + (float)fOpposite2.deep * 0.001f;
                        f3.sprite.frame = opposite2.variation;
                        Vector2 pos3 = new Vector2((int)p3.x, (int)p3.y);
                        f3.sprite.flipH = p3.flip;
                        if (rightRoom)
                        {
                            pos3.X = (float)RoomEditor.roomSize - pos3.X;
                            f3.sprite.flipH = !f3.sprite.flipH;
                            pos3.X -= 1f;
                        }
                        pos3 += Position;
                        if (f3.visible)
                        {
                            f3.Draw(pos3, f3.sprite.Depth, f3.sprite.frame, profile);
                        }
                        f3.sprite.frame = 0;
                        f3.sprite.flipH = false;
                        idx2++;
                    }
                }
            }
            if (_hatSelector._roomEditor._mode == REMode.Place && _hatSelector._roomEditor.CurFurni().type == FurnitureType.Theme)
            {
                theme = _hatSelector._roomEditor.CurFurni();
            }
        }
        if (rightRoom)
        {
            if (theme == null)
            {
                for (int i3 = 0; i3 < 4; i3++)
                {
                    if (profile.GetLightStatus(i3))
                    {
                        _lightBar.Depth = _tutorialTV.Depth;
                        _lightBar.frame = i3;
                        Graphics.Draw(_lightBar, base.X + 38f + (float)(i3 * 3), base.Y + 49f);
                    }
                }
                _roomSwitch.Depth = _tutorialTV.Depth;
                _roomSwitch.frame = (profile.switchStatus ? 1 : 0);
                Graphics.Draw(_roomSwitch, base.X + 52f, base.Y + 47f);
            }
            if (theme != null)
            {
                Furniture furniture = theme;
                furniture.sprite.flipH = true;
                furniture.sprite.Depth = _roomLeftForeground.Depth;
                furniture.background.Depth = _roomLeftBackground.Depth;
                furniture.sprite.Scale = new Vector2(1f);
                furniture.background.Scale = new Vector2(1f);
                Graphics.Draw(furniture.sprite, base.X + 70f, base.Y + 44f, new Rectangle(0f, 0f, 4f, 87f));
                Graphics.Draw(furniture.sprite, base.X + 70f, base.Y + 44f + 68f, new Rectangle(0f, 68f, 141f, 19f));
                Graphics.Draw(furniture.sprite, base.X + 70f, base.Y + 44f, new Rectangle(0f, 0f, 141f, 16f));
                Graphics.Draw(furniture.sprite, base.X + 21f, base.Y + 44f, new Rectangle(49f, 0f, 92f, 68f));
                furniture.sprite.Depth = _selectConsole.Depth - 20;
                Graphics.Draw(furniture.sprite, base.X + 70f - 4f, base.Y + 44f, new Rectangle(4f, 0f, 44f, 54f));
                furniture.sprite.Depth = 0.31f;
                Graphics.Draw(furniture.sprite, base.X + 70f - 4f, base.Y + 44f + 54f, new Rectangle(4f, 54f, 44f, 14f));
                furniture.sprite.flipH = false;
                furniture.background.flipH = true;
                Graphics.Draw(furniture.background, base.X + 70f, base.Y + 45f);
                furniture.background.flipH = false;
            }
            else
            {
                Graphics.Draw(_roomLeftBackground, X - 1, Y + 1);
                Graphics.Draw(_roomLeftForeground, X - 1, Y + 1, new Rectangle(0, 0, 49, 16));
                Graphics.Draw(_roomLeftForeground, X - 1, Y + 17, new Rectangle(0, 16, 6, 8));
                Graphics.Draw(_roomLeftForeground, X - 1, Y + 56, new Rectangle(0, 55, 53, 13));
                Graphics.Draw(_roomLeftForeground, X - 1, Y + 69, new Rectangle(0, 68, 141, 19));
                Graphics.Draw(_roomLeftForeground, X + 136, Y + 1, new Rectangle(137, 0, 4, 87));
            }
            if (Network.isActive && ((Network.isServer && profile.connection == DuckNetwork.localConnection) || profile.connection == Network.host))
            {
                _hostCrown.Depth = -.5f;
                Graphics.Draw(_hostCrown, base.X + 126f, base.Y + 23f);
            }
        }
        else
        {
            if (theme == null)
            {
                for (int i4 = 0; i4 < 4; i4++)
                {
                    if (profile.GetLightStatus(i4))
                    {
                        _lightBar.Depth = _tutorialTV.Depth;
                        _lightBar.frame = i4;
                        Graphics.Draw(_lightBar, X + 91 + (i4 * 3), Y + 49);
                    }
                }
                _roomSwitch.Depth = _tutorialTV.Depth;
                _roomSwitch.frame = profile.switchStatus ? 1 : 0;
                Graphics.Draw(_roomSwitch, X + 81, Y + 47);
            }
            if (theme != null)
            {
                Furniture furniture2 = theme;
                furniture2.sprite.Depth = _roomLeftForeground.Depth;
                furniture2.background.Depth = _roomLeftBackground.Depth;
                furniture2.sprite.Scale = new Vector2(1f);
                furniture2.background.Scale = new Vector2(1f);
                Graphics.Draw(furniture2.sprite, base.X + 70f, base.Y + 44f, new Rectangle(0f, 0f, 4f, 87f));
                Graphics.Draw(furniture2.sprite, base.X + 70f, base.Y + 44f + 68f, new Rectangle(0f, 68f, 141f, 19f));
                Graphics.Draw(furniture2.sprite, base.X + 70f, base.Y + 44f, new Rectangle(0f, 0f, 141f, 16f));
                Graphics.Draw(furniture2.sprite, base.X + 70f + 49f, base.Y + 44f, new Rectangle(49f, 0f, 92f, 68f));
                furniture2.sprite.Depth = _selectConsole.Depth - 20;
                Graphics.Draw(furniture2.sprite, base.X + 70f + 4f, base.Y + 44f, new Rectangle(4f, 0f, 44f, 54f));
                furniture2.sprite.Depth = 0.31f;
                Graphics.Draw(furniture2.sprite, base.X + 70f + 4f, base.Y + 44f + 54f, new Rectangle(4f, 54f, 44f, 14f));
                Graphics.Draw(furniture2.background, base.X + 70f, base.Y + 45f);
            }
            else
            {
                Graphics.Draw(_roomLeftBackground, base.X + 4f, base.Y + 1f);
                Graphics.Draw(_roomLeftForeground, base.X, base.Y + 1f, new Rectangle(0f, 0f, 4f, 87f));
                Graphics.Draw(_roomLeftForeground, base.X + 4f, base.Y + 1f + 68f, new Rectangle(4f, 68f, 137f, 19f));
                Graphics.Draw(_roomLeftForeground, base.X + 92f, base.Y + 1f, new Rectangle(92f, 0f, 49f, 16f));
                Graphics.Draw(_roomLeftForeground, base.X + 135f, base.Y + 1f + 16f, new Rectangle(135f, 16f, 6f, 8f));
                Graphics.Draw(_roomLeftForeground, base.X + 89f, base.Y + 1f + 55f, new Rectangle(89f, 55f, 52f, 13f));
            }
            if (Network.isActive && ((Network.isServer && profile.connection == DuckNetwork.localConnection) || profile.connection == Network.host))
            {
                _hostCrown.Depth = -0.5f;
                Graphics.Draw(_hostCrown, base.X + 14f, base.Y + 23f);
            }
        }
        _tutorialTV.Depth = -0.58f;
        _tutorialMessages.Depth = -0.5f;
        _tutorialMessages.Alpha = _screenFade;
        _font.Alpha = 1f;
        _font.Depth = 0.6f;
        if (theme != null)
        {
            _tutorialTV.Depth = -0.8f;
            _tutorialMessages.Depth = -0.8f;
        }
        string teamName = _playerProfile.team.currentDisplayName;
        _selectConsole.Depth = -0.5f;
        _consoleHighlight.Depth = -0.49f;
        float tvOffset = 8f;
        if (rightRoom)
        {
            _consolePos = new Vector2(base.X + 116f, base.Y + 30f);
            _consoleFlash.Scale = new Vector2(0.75f, 0.75f);
            if (_selectConsole.imageIndex == 0)
            {
                _consoleFlash.Alpha = 0.3f;
            }
            else if (_selectConsole.imageIndex == 1)
            {
                _consoleFlash.Alpha = 0.1f;
            }
            else if (_selectConsole.imageIndex == 2)
            {
                _consoleFlash.Alpha = 0f;
            }
            Graphics.Draw(_consoleFlash, _consolePos.X + 9f, _consolePos.Y + 7f);
            Graphics.Draw(_selectConsole, _consolePos.X, _consolePos.Y);
            if (_consoleFade > 0.01f)
            {
                _consoleHighlight.Alpha = _consoleFade;
                Graphics.Draw(_consoleHighlight, _consolePos.X, _consolePos.Y);
            }
            Graphics.Draw(_readySign, base.X + 1f, base.Y + 3f);
            float dep = -0.57f;
            if (theme != null)
            {
                dep = -0.8f;
            }
            bool drawTutorial = true;
            if (theme == null)
            {
                Graphics.Draw(_tutorialTV, base.X + 57f - tvOffset, base.Y + 8f);
                tvOffset = 27f;
                if (drawTutorial)
                {
                    if (_tutorialMessages.frame == 0)
                    {
                        _font.Draw("@DPAD@MOVE", new Vector2(base.X + 28f + tvOffset, base.Y + 16f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                        _font.Draw("@JUMP@JUMP", new Vector2(base.X + 28f + tvOffset, base.Y + 30f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                    }
                    else if (_tutorialMessages.frame == 1)
                    {
                        _font.Draw("@GRAB@", new Vector2(base.X + 45f + tvOffset, base.Y + 17f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                        _font.Draw("PICKUP", new Vector2(base.X + 29f + tvOffset, base.Y + 30f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                    }
                    else if (_tutorialMessages.frame == 2)
                    {
                        _font.Draw("@GRAB@TOSS", new Vector2(base.X + 28f + tvOffset, base.Y + 16f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                        _font.Draw("@SHOOT@FIRE", new Vector2(base.X + 28f + tvOffset, base.Y + 30f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                    }
                    else if (_tutorialMessages.frame == 3)
                    {
                        _font.Draw("@CANCEL@", new Vector2(base.X + 45f + tvOffset, base.Y + 17f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                        _font.Draw("CANCEL", new Vector2(base.X + 29f + tvOffset, base.Y + 30f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                    }
                    else if (_tutorialMessages.frame == 4)
                    {
                        _font.Draw("@DPAD@MOVE", new Vector2(base.X + 28f + tvOffset, base.Y + 16f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                        _font.Draw("@SHOOT@TEAM", new Vector2(base.X + 28f + tvOffset, base.Y + 30f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                    }
                    else if (_tutorialMessages.frame == 5)
                    {
                        _font.Draw("@GRAB@TOSS", new Vector2(base.X + 28f + tvOffset, base.Y + 16f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                        _font.Draw("@SHOOT@TEAM", new Vector2(base.X + 28f + tvOffset, base.Y + 30f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                    }
                }
                else
                {
                    Graphics.Draw(_onlineIcon, (int)base.X + 72, base.Y + 19f, dep);
                }
            }
            _font.Depth = 0.6f;
            float vOffset = 0f;
            float hOffset = 0f;
            Vector2 fontscale = new Vector2(1f, 1f);
            if (teamName.Length > 9)
            {
                fontscale = new Vector2(0.75f, 0.75f);
                vOffset = 1f;
                hOffset = 1f;
            }
            if (teamName.Length > 12)
            {
                fontscale = new Vector2(0.5f, 0.5f);
                vOffset = 2f;
                hOffset = 1f;
            }
            _font.Scale = fontscale;
            if (_hatSelector._roomEditor._mode == REMode.Place)
            {
                hOffset = 0f;
                vOffset = 0f;
                string tn = "PLAYER 1";
                float extraXOff = 47f;
                base.X += extraXOff;
                Furniture f4 = _hatSelector._roomEditor.CurFurni();
                if (f4.type == FurnitureType.Font)
                {
                    f4.font.Scale = new Vector2(0.5f, 0.5f);
                    f4.font.spriteScale = new Vector2(0.5f, 0.5f);
                    f4.font.Draw("@SELECT@ACCEPT @CANCEL@CANCEL", base.X + 24f - f4.font.GetWidth(tn) / 2f - hOffset, base.Y + 75f + vOffset, Color.White, 0.7f, profile.inputProfile);
                    f4.font.Scale = new Vector2(1f, 1f);
                }
                else if (f4.type == FurnitureType.Theme)
                {
                    profile.font.Scale = new Vector2(0.5f, 0.5f);
                    profile.font.spriteScale = new Vector2(0.5f, 0.5f);
                    profile.font.Draw("@SELECT@ACCEPT @CANCEL@CANCEL", base.X + 24f - profile.font.GetWidth(tn) / 2f - hOffset, base.Y + 75f + vOffset, Color.White, 0.7f, profile.inputProfile);
                }
                else if (f4.name == "CLEAR ROOM")
                {
                    profile.font.Scale = new Vector2(0.5f, 0.5f);
                    profile.font.spriteScale = new Vector2(0.5f, 0.5f);
                    profile.font.Draw("@MENU2@CLEAR @CANCEL@BACK", base.X + 24f - profile.font.GetWidth(tn) / 2f - hOffset, base.Y + 75f + vOffset, Color.White, 0.7f, profile.inputProfile);
                }
                else
                {
                    profile.font.Scale = new Vector2(0.5f, 0.5f);
                    profile.font.spriteScale = new Vector2(0.5f, 0.5f);
                    if (_hatSelector._roomEditor._hover != null)
                    {
                        profile.font.Draw("@SELECT@DEL @MENU2@GRAB @CANCEL@DONE", base.X + 24f - profile.font.GetWidth(tn) / 2f - hOffset, base.Y + 75f + vOffset, Color.White, 0.7f, profile.inputProfile);
                    }
                    else
                    {
                        profile.font.Draw("@SELECT@ADD @MENU2@MOD @CANCEL@DONE", base.X + 24f - profile.font.GetWidth(tn) / 2f - hOffset, base.Y + 75f + vOffset, Color.White, 0.7f, profile.inputProfile);
                    }
                    profile.font.Scale = new Vector2(0.25f, 0.25f);
                    int numAvail = Profiles.experienceProfile.GetNumFurnitures(f4.index) - profile.GetNumFurnituresPlaced(f4.index);
                    profile.font.Draw(f4.name + ((numAvail > 0) ? " |DGGREEN|" : " |DGRED|") + "x" + numAvail, base.X + 17f - profile.font.GetWidth(tn) / 2f - hOffset, base.Y + 75f + 6.5f + vOffset, Color.White, 0.7f);
                    int totalFurnis = profile.GetTotalFurnituresPlaced();
                    float fill = (float)totalFurnis / (float)RoomEditor.maxFurnitures;
                    profile.font.Draw(totalFurnis + "/" + RoomEditor.maxFurnitures, base.X + 68f - profile.font.GetWidth(tn) / 2f - hOffset, base.Y + 75f + 6.5f + vOffset, Color.Black, 0.7f);
                    Vector2 vec = new Vector2(base.X + 56f - profile.font.GetWidth(tn) / 2f - hOffset, base.Y + 75f + 6f + vOffset);
                    Graphics.DrawRect(vec, vec + new Vector2(37f, 3f), Colors.BlueGray, 0.66f, filled: true, 0.5f);
                    Graphics.DrawRect(vec, vec + new Vector2(37f * fill, 3f), (fill < 0.4f) ? Colors.DGGreen : ((fill < 0.8f) ? Colors.DGYellow : Colors.DGRed), 0.68f, filled: true, 0.5f);
                }
                profile.font.spriteScale = new Vector2(1f, 1f);
                profile.font.Scale = new Vector2(1f, 1f);
                base.X -= extraXOff;
            }
            else
            {
                _playerProfile.font.Scale = fontscale;
                _playerProfile.font.Draw(teamName, base.X + 94f - _playerProfile.font.GetWidth(teamName) / 2f - hOffset, base.Y + 75f + vOffset, Color.White, 0.7f);
                _font.Scale = new Vector2(1f, 1f);
            }
            return;
        }
        _consolePos = new Vector2(base.X + 4f, base.Y + 30f);
        _consoleFlash.Scale = new Vector2(0.75f, 0.75f);
        if (_selectConsole.imageIndex == 0)
        {
            _consoleFlash.Alpha = 0.3f;
        }
        else if (_selectConsole.imageIndex == 1)
        {
            _consoleFlash.Alpha = 0.1f;
        }
        else if (_selectConsole.imageIndex == 2)
        {
            _consoleFlash.Alpha = 0f;
        }
        Graphics.Draw(_consoleFlash, _consolePos.X + 9f, _consolePos.Y + 7f);
        Graphics.Draw(_selectConsole, _consolePos.X, _consolePos.Y);
        if (_consoleFade > 0.01f)
        {
            _consoleHighlight.Alpha = _consoleFade;
            Graphics.Draw(_consoleHighlight, _consolePos.X, _consolePos.Y);
        }
        Graphics.Draw(_readySign, base.X + 96f, base.Y + 3f);
        float dep2 = -0.57f;
        if (theme != null)
        {
            dep2 = -0.8f;
        }
        bool drawTutorial2 = true;
        if (theme == null)
        {
            Graphics.Draw(_tutorialTV, base.X + 22f + tvOffset, base.Y + 8f);
            if (drawTutorial2)
            {
                if (_tutorialMessages.frame == 0)
                {
                    _font.Draw("@WASD@MOVE", new Vector2(base.X + 28f + tvOffset, base.Y + 16f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                    _font.Draw("@JUMP@JUMP", new Vector2(base.X + 28f + tvOffset, base.Y + 30f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                }
                else if (_tutorialMessages.frame == 1)
                {
                    _font.Draw("@GRAB@", new Vector2(base.X + 45f + tvOffset, base.Y + 17f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                    _font.Draw("PICKUP", new Vector2(base.X + 29f + tvOffset, base.Y + 30f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                }
                else if (_tutorialMessages.frame == 2)
                {
                    _font.Draw("@GRAB@TOSS", new Vector2(base.X + 28f + tvOffset, base.Y + 16f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                    _font.Draw("@SHOOT@FIRE", new Vector2(base.X + 28f + tvOffset, base.Y + 30f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                }
                else if (_tutorialMessages.frame == 3)
                {
                    _font.Draw("@CANCEL@", new Vector2(base.X + 45f + tvOffset, base.Y + 17f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                    _font.Draw("CANCEL", new Vector2(base.X + 29f + tvOffset, base.Y + 30f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                }
                else if (_tutorialMessages.frame == 4)
                {
                    _font.Draw("@WASD@MOVE", new Vector2(base.X + 28f + tvOffset, base.Y + 16f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                    _font.Draw("@SHOOT@TEAM", new Vector2(base.X + 28f + tvOffset, base.Y + 30f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                }
                else if (_tutorialMessages.frame == 5)
                {
                    _font.Draw("@GRAB@TOSS", new Vector2(base.X + 28f + tvOffset, base.Y + 16f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                    _font.Draw("@SHOOT@TEAM", new Vector2(base.X + 28f + tvOffset, base.Y + 30f), Color.White * _screenFade, _tutorialTV.Depth + 20, _inputProfile);
                }
            }
            else
            {
                Graphics.Draw(_onlineIcon, (int)base.X + 53, base.Y + 19f, dep2);
            }
        }
        _font.Depth = 0.6f;
        _aButton.Position = new Vector2(base.X + 39f, base.Y + 71f);
        float vOffset2 = 0f;
        float hOffset2 = 0f;
        Vector2 fontscale2 = new Vector2(1f, 1f);
        if (teamName.Length > 9)
        {
            fontscale2 = new Vector2(0.75f, 0.75f);
            vOffset2 = 1f;
            hOffset2 = 1f;
        }
        if (teamName.Length > 12)
        {
            fontscale2 = new Vector2(0.5f, 0.5f);
            vOffset2 = 2f;
            hOffset2 = 1f;
        }
        if (_hatSelector._roomEditor._mode == REMode.Place && Profiles.experienceProfile != null)
        {
            string tn2 = "PLAYER 1";
            hOffset2 = 0f;
            vOffset2 = 0f;
            Furniture f5 = _hatSelector._roomEditor.CurFurni();
            if (f5.type == FurnitureType.Font)
            {
                f5.font.Scale = new Vector2(0.5f, 0.5f);
                f5.font.spriteScale = new Vector2(0.5f, 0.5f);
                f5.font.Draw("@SELECT@ACCEPT @CANCEL@CANCEL", base.X + 24f - f5.font.GetWidth(tn2) / 2f - hOffset2, base.Y + 75f + vOffset2, Color.White, 0.7f, profile.inputProfile);
                f5.font.Scale = new Vector2(1f, 1f);
            }
            else if (f5.type == FurnitureType.Theme)
            {
                profile.font.Scale = new Vector2(0.5f, 0.5f);
                profile.font.spriteScale = new Vector2(0.5f, 0.5f);
                profile.font.Draw("@SELECT@ACCEPT @CANCEL@CANCEL", base.X + 24f - profile.font.GetWidth(tn2) / 2f - hOffset2, base.Y + 75f + vOffset2, Color.White, 0.7f, profile.inputProfile);
            }
            else if (f5.name == "CLEAR ROOM")
            {
                profile.font.Scale = new Vector2(0.5f, 0.5f);
                profile.font.spriteScale = new Vector2(0.5f, 0.5f);
                profile.font.Draw("@MENU2@CLEAR @CANCEL@BACK", base.X + 24f - profile.font.GetWidth(tn2) / 2f - hOffset2, base.Y + 75f + vOffset2, Color.White, 0.7f, profile.inputProfile);
            }
            else
            {
                profile.font.Scale = new Vector2(0.5f, 0.5f);
                profile.font.spriteScale = new Vector2(0.5f, 0.5f);
                if (_hatSelector._roomEditor._hover != null)
                {
                    profile.font.Draw("@SELECT@DEL @MENU2@GRAB @CANCEL@DONE", base.X + 24f - profile.font.GetWidth(tn2) / 2f - hOffset2, base.Y + 75f + vOffset2, Color.White, 0.7f, profile.inputProfile);
                }
                else
                {
                    profile.font.Draw("@SELECT@ADD @MENU2@MOD @CANCEL@DONE", base.X + 24f - profile.font.GetWidth(tn2) / 2f - hOffset2, base.Y + 75f + vOffset2, Color.White, 0.7f, profile.inputProfile);
                }
                profile.font.Scale = new Vector2(0.25f, 0.25f);
                int numAvail2 = Profiles.experienceProfile.GetNumFurnitures(f5.index) - profile.GetNumFurnituresPlaced(f5.index);
                profile.font.Draw(f5.name + ((numAvail2 > 0) ? " |DGGREEN|" : " |DGRED|") + "x" + numAvail2, base.X + 17f - profile.font.GetWidth(tn2) / 2f - hOffset2, base.Y + 75f + 6.5f + vOffset2, Color.White, 0.7f);
                int totalFurnis2 = profile.GetTotalFurnituresPlaced();
                float fill2 = (float)totalFurnis2 / (float)RoomEditor.maxFurnitures;
                profile.font.Draw(totalFurnis2 + "/" + RoomEditor.maxFurnitures, base.X + 68f - profile.font.GetWidth(tn2) / 2f - hOffset2, base.Y + 75f + 6.5f + vOffset2, Color.Black, 0.7f);
                Vector2 vec2 = new Vector2(base.X + 56f - profile.font.GetWidth(tn2) / 2f - hOffset2, base.Y + 75f + 6f + vOffset2);
                Graphics.DrawRect(vec2, vec2 + new Vector2(37f, 3f), Colors.BlueGray, 0.66f, filled: true, 0.5f);
                Graphics.DrawRect(vec2, vec2 + new Vector2(37f * fill2, 3f), (fill2 < 0.4f) ? Colors.DGGreen : ((fill2 < 0.8f) ? Colors.DGYellow : Colors.DGRed), 0.68f, filled: true, 0.5f);
            }
            profile.font.spriteScale = new Vector2(1f, 1f);
            profile.font.Scale = new Vector2(1f, 1f);
        }
        else
        {
            _playerProfile.font.Scale = fontscale2;
            _playerProfile.font.Draw(teamName, base.X + 48f - _playerProfile.font.GetWidth(teamName) / 2f - hOffset2, base.Y + 75f + vOffset2, Color.White, 0.7f);
            _font.Scale = new Vector2(1f, 1f);
        }
    }
}
