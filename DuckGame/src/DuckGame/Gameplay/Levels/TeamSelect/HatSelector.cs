using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class HatSelector : Thing, ITakeInput
{
    public StateBinding _profileBoxNumberBinding = new StateBinding(nameof(profileBoxNumber));

    private sbyte _profileBoxNumber = -1;

    public StateBinding _positionBinding = new StateBinding(nameof(netPosition));

    public StateBinding _openBinding = new StateBinding(nameof(_open));

    public StateBinding _selectorPositionBinding = new StateBinding(nameof(_selectorPosition));

    public StateBinding _desiredTeamSelectionBinding = new StateBinding(nameof(_desiredTeamSelection));

    public StateBinding _mainSelectionBinding = new StateBinding(nameof(_mainSelection));

    public StateBinding _selectionBinding = new StateBinding(nameof(selectionInt));

    public StateBinding _lcdFlashBinding = new StateBinding(nameof(_lcdFlash));

    public StateBinding _lcdFlashIncBinding = new StateBinding(nameof(_lcdFlashInc));

    public StateBinding _editingRoomBinding = new StateBinding(nameof(_editingRoom));

    public StateBinding _gettingXPBinding = new StateBinding(nameof(_gettingXP));

    public StateBinding _gettingXPCompletionBinding = new StateBinding(nameof(_gettingXPCompletion));

    public StateBinding _flashTransitionBinding = new StateBinding(nameof(flashTransition));

    public StateBinding _darkenBinding = new StateBinding(nameof(darken));

    public float _fade;

    private float _blackFade;

    public bool _open;

    private bool _closing;

    public bool _gettingXP;

    public float _gettingXPCompletion;

    public bool _editingRoom;

    public short _selectorPosition;

    public short _teamSelection;

    public short _desiredTeamSelection;

    public short _mainSelection;

    public float _slide;

    public float _slideTo;

    public float _upSlide;

    public float _upSlideTo;

    private bool fakefade;

    private string _firstWord = "";

    private string _secondWord = "";

    private InputProfile _blankProfile = new InputProfile();

    private InputProfile _inputProfile;

    private Profile _profile;

    private BitmapFont _font;

    public float _lcdFlash;

    public float _lcdFlashInc;

    private HSSelection _selection = HSSelection.ChooseProfile;

    private ConsoleScreen _screen;

    private ProfileSelector _profileSelector;

    public RoomEditor _roomEditor;

    private Sprite _oButton;

    private ProfileBox2 _box;

    private SpriteMap _demoBox;

    private Sprite _selectBorder;

    private Sprite _consoleText;

    private Sprite _contextArrow;

    private SpriteMap _clueHat;

    private SpriteMap _boardLoader;

    private SpriteMap _lock;

    private SpriteMap _goldLock;

    private SpriteMap _gettingXPBoard;

    private SpriteMap _editingRoomBoard;

    private Sprite _blind;

    public Hat hat;

    private bool _teamWasCustomHat;

    private string _teamName = "";

    private Team _netHoveringTeam;

    private MaterialSecretOutline _outlineMaterial;

    private int _prevDesiredTeam;

    private Profile _experienceProfileCheck;

    public bool isArcadeHatSelector;

    private bool _editRoomDisabled;

    private Team _startingTeam;

    private bool _inputSkip;

    private float _blindLerp;

    public sbyte profileBoxNumber
    {
        get
        {
            return _profileBoxNumber;
        }
        set
        {
            if (value < 0)
            {
                return;
            }
            if (_box != null)
            {
                _ = _box.controllerIndex;
            }
            bool changed = _profileBoxNumber != value;
            _profileBoxNumber = value;
            if (Network.isClient)
            {
                _profile = DuckNetwork.profiles[_profileBoxNumber];
                _inputProfile = _profile.inputProfile;
                if (Level.current is TeamSelect2 ts)
                {
                    _box = ts.GetBox((byte)_profileBoxNumber);
                    _box.SetHatSelector(this);
                }
                else
                {
                    DevConsole.Log(DCSection.General, "!---CRITICAL! Profile box link failure!(" + _profileBoxNumber + ")---!");
                }
            }
            if (_profile != null && _profile.connection == DuckNetwork.localConnection)
            {
                if (changed)
                {
                    Thing.Fondle(this, DuckNetwork.localConnection);
                }
                connection = DuckNetwork.localConnection;
            }
        }
    }

    public new virtual Vector2 netPosition
    {
        get
        {
            return Position;
        }
        set
        {
            Position = value;
        }
    }

    public bool flashTransition
    {
        get
        {
            return _screen._flashTransition;
        }
        set
        {
            _screen._flashTransition = value;
        }
    }

    public float darken
    {
        get
        {
            return _screen._darken;
        }
        set
        {
            _screen._darken = value;
        }
    }

    public float fade
    {
        get
        {
            return _fade;
        }
        set
        {
            _fade = value;
        }
    }

    public bool open => _open;

    public float fadeVal
    {
        get
        {
            if (fakefade)
            {
                return 1f;
            }
            float val = _fade;
            if (_profileSelector.fade > 0f)
            {
                val = 1f;
            }
            if (_roomEditor.fade > 0f)
            {
                val = 1f;
            }
            return val;
        }
    }

    public string firstWord
    {
        get
        {
            return _firstWord;
        }
        set
        {
            _firstWord = value;
        }
    }

    public string secondWord
    {
        get
        {
            return _secondWord;
        }
        set
        {
            _secondWord = value;
        }
    }

    public InputProfile profileInput
    {
        get
        {
            if (_profile != null)
            {
                return _profile.inputProfile;
            }
            return inputProfile;
        }
    }

    public InputProfile inputProfile
    {
        get
        {
            if (Network.isActive && connection != DuckNetwork.localConnection)
            {
                return _blankProfile;
            }
            if (_profile != null)
            {
                return _profile.inputProfile;
            }
            return _inputProfile;
        }
    }

    public Profile profile => _profile;

    public float lcdFlash => _lcdFlash;

    public byte selectionInt
    {
        get
        {
            return (byte)_selection;
        }
        set
        {
            _selection = (HSSelection)value;
        }
    }

    public ConsoleScreen screen => _screen;

    public ProfileBox2 box => _box;

    public HatSelector(float xpos, float ypos, Profile profile, ProfileBox2 box)
        : base(xpos, ypos)
    {
        _profile = profile;
        _inputProfile = _profile.inputProfile;
        _box = box;
        if (Network.isServer)
        {
            _profileBoxNumber = (sbyte)box.controllerIndex;
        }
        Construct();
    }

    public HatSelector()
    {
        Construct();
    }

    public void Construct()
    {
        _font = new BitmapFont("biosFontUI", 8, 7);
        _font.Scale = new Vector2(0.5f, 0.5f);
        _collisionSize = new Vector2(141f, 89f);
        _oButton = new Sprite("oButton");
        _demoBox = new SpriteMap("demoCrate", 20, 20);
        _demoBox.CenterOrigin();
        _clueHat = new SpriteMap("hats/cluehat", 32, 32);
        _clueHat.CenterOrigin();
        _blind = new Sprite("blind");
        _gettingXPBoard = new SpriteMap("gettingXP", 63, 30);
        _gettingXPBoard.CenterOrigin();
        _editingRoomBoard = new SpriteMap("editingRoom", 63, 30);
        _editingRoomBoard.CenterOrigin();
        _boardLoader = new SpriteMap("boardLoader", 7, 7);
        _boardLoader.AddAnimation("idle", 0.2f, true, 0, 1, 2, 3, 4, 5, 6, 7);
        _boardLoader.CenterOrigin();
        _boardLoader.SetAnimation("idle");
        _selectBorder = new Sprite("selectBorder2");
        _consoleText = new Sprite("corptronConsoleText");
        _contextArrow = new Sprite("contextArrowRight");
        _lock = new SpriteMap("arcade/unlockLock", 15, 18);
        _goldLock = new SpriteMap("arcade/goldUnlockLock", 15, 18);
    }

    public void SetProfile(Profile newProfile)
    {
        _profile = newProfile;
    }

    public void ConfirmProfile()
    {
        _selection = HSSelection.Main;
    }

    public override void Initialize()
    {
        _profileSelector = new ProfileSelector(base.X, base.Y, _box, this);
        Level.Add(_profileSelector);
        _roomEditor = new RoomEditor(base.X, base.Y, _box, this);
        Level.Add(_roomEditor);
        _screen = new ConsoleScreen(base.X, base.Y, this);
    }

    private int ControllerNumber()
    {
        if (Network.isActive)
        {
            return Maths.Clamp(_profileBoxNumber, 0, DG.MaxPlayers - 1);
        }
        if (inputProfile.name == InputProfile.MPPlayer1)
        {
            return 0;
        }
        if (inputProfile.name == InputProfile.MPPlayer2)
        {
            return 1;
        }
        if (inputProfile.name == InputProfile.MPPlayer3)
        {
            return 2;
        }
        if (inputProfile.name == InputProfile.MPPlayer4)
        {
            return 3;
        }
        if (inputProfile.name == InputProfile.MPPlayer5)
        {
            return 4;
        }
        if (inputProfile.name == InputProfile.MPPlayer6)
        {
            return 5;
        }
        if (inputProfile.name == InputProfile.MPPlayer7)
        {
            return 6;
        }
        if (inputProfile.name == InputProfile.MPPlayer8)
        {
            return 7;
        }
        return 0;
    }

    private void SelectTeam()
    {
        if (_desiredTeamSelection < AllTeams().Count)
        {
            FilterTeam().Join(_profile);
        }
    }

    private Team FilterTeam(bool hardFilter = false)
    {
        _teamWasCustomHat = false;
        _teamName = "";
        if (Network.isActive)
        {
            if (_desiredTeamSelection >= AllTeams().Count)
            {
                ControllerNumber();
            }
            Team t = AllTeams()[_desiredTeamSelection];
            _teamName = t.name.ToUpperInvariant();
            if (Teams.core.extraTeams.Contains(t))
            {
                _teamWasCustomHat = true;
            }
            return t;
        }
        List<Team> teams = AllTeams();
        if (_desiredTeamSelection < 0 || _desiredTeamSelection >= teams.Count)
        {
            return teams[0];
        }
        return teams[_desiredTeamSelection];
    }

    public override void Terminate()
    {
        base.Terminate();
    }

    public void ConfirmTeamSelection()
    {
        Team t = FilterTeam(hardFilter: true);
        if (Network.isActive && _box.duck != null)
        {
            if (_teamWasCustomHat)
            {
                foreach (NetworkConnection c in Network.connections)
                {
                    Send.Message(new NMSpecialHat(t, _profile, c.profile != null && c.profile.muteHat), c);
                }
            }
            Send.Message(new NMSetTeam(_box.duck.profile, t, _teamWasCustomHat));
        }
        if (t.hasHat)
        {
            if (_box.duck != null)
            {
                Hat existing = _box.duck.GetEquipment(typeof(Hat)) as Hat;
                Hat h = new TeamHat(0f, 0f, t, _box.duck.profile);
                Level.Add(h);
                _box.duck.Equip(h, makeSound: false);
                _box.duck.Fondle(h);
                if (hat != null)
                {
                    Level.Remove(hat);
                }
                hat = h;
                if (existing != null)
                {
                    Level.Remove(existing);
                    if (Network.isActive)
                    {
                        Send.Message(new NMUnequip(_box.duck, existing), NetMessagePriority.ReliableOrdered);
                    }
                }
                if (Network.isActive)
                {
                    Send.Message(new NMEquip(_box.duck, hat), NetMessagePriority.ReliableOrdered);
                }
            }
            else if (hat != null)
            {
                Level.Remove(hat);
            }
        }
        else
        {
            if (hat != null)
            {
                Level.Remove(hat);
            }
            hat = null;
            if (_box.duck != null && _box.duck.GetEquipment(typeof(Hat)) is Hat existing2)
            {
                _box.duck.Unequip(existing2);
                Level.Remove(existing2);
                if (Network.isActive)
                {
                    Send.Message(new NMUnequip(_box.duck, existing2), NetMessagePriority.ReliableOrdered);
                }
            }
        }
        if (_desiredTeamSelection > DG.MaxPlayers - 1 && _box.duck != null)
        {
            DuckNetwork.OnTeamSwitch(_box.duck.profile);
        }
    }

    private int TeamIndexAdd(int index, int plus, bool alwaysThree = true)
    {
        if (alwaysThree && index < DG.MaxPlayers && index >= 0)
        {
            index = DG.MaxPlayers - 1;
        }
        int val = index + plus;
        if (val >= AllTeams().Count)
        {
            return val - AllTeams().Count + (DG.MaxPlayers - 1);
        }
        if (val < DG.MaxPlayers - 1)
        {
            return AllTeams().Count + (val - (DG.MaxPlayers - 1));
        }
        return val;
    }

    private int TeamIndexAddSpecial(int index, int plus, bool alwaysThree = true)
    {
        if (alwaysThree && index < DG.MaxPlayers && index >= 0)
        {
            index = DG.MaxPlayers - 1;
        }
        int val = index + plus;
        if (val >= AllTeams().Count)
        {
            val = val - AllTeams().Count + (DG.MaxPlayers - 1);
        }
        if (val < DG.MaxPlayers - 1)
        {
            val = AllTeams().Count + (val - (DG.MaxPlayers - 1));
        }
        if (val <= DG.MaxPlayers - 1)
        {
            val = profileBoxNumber;
        }
        return val;
    }

    private int GetTeamIndex(Team tm)
    {
        int t = 0;
        using (List<Team>.Enumerator enumerator = AllTeams().GetEnumerator())
        {
            while (enumerator.MoveNext() && enumerator.Current != tm)
            {
                t++;
            }
        }
        return t;
    }

    public void Reset()
    {
        _netHoveringTeam = null;
        _open = false;
        _closing = true;
        _selection = HSSelection.Main;
        _mainSelection = 0;
        _editingRoom = false;
        _gettingXP = false;
        _profileSelector.Reset();
        _roomEditor.Reset();
    }

    public List<Team> AllTeams()
    {
        if (!Network.isActive)
        {
            return Teams.all;
        }
        if (_profile != null)
        {
            if (_profile.connection != DuckNetwork.localConnection)
            {
                List<Team> teams = new List<Team>(Teams.core.teams);
                {
                    foreach (Team t in _profile.customTeams)
                    {
                        teams.Add(t);
                    }
                    return teams;
                }
            }
            List<Team> teams2 = new List<Team>(Teams.core.teams);
            {
                foreach (Team t2 in Teams.core.extraTeams)
                {
                    teams2.Add(t2);
                }
                return teams2;
            }
        }
        return Teams.core.teams;
    }

    public override void Update()
    {
        bool canEditRoom = true;
        if (_profileBoxNumber < 0 || inputProfile == null || _box == null || _profile == null)
        {
            return;
        }
        if (connection == DuckNetwork.localConnection && inputProfile.Pressed("ANY"))
        {
            authority++;
        }
        if (Network.isActive && connection == DuckNetwork.localConnection && Profiles.experienceProfile != null && profile.linkedProfile == Profiles.experienceProfile)
        {
            if (MonoMain.pauseMenu != null)
            {
                if (MonoMain.pauseMenu is UILevelBox)
                {
                    _gettingXP = true;
                    UILevelBox box = MonoMain.pauseMenu as UILevelBox;
                    _gettingXPCompletion = (box._dayProgress + box._xpProgress) / 2f * 0.7f;
                }
                else
                {
                    _gettingXPCompletion = 0.7f;
                    if (MonoMain.pauseMenu is UIFuneral)
                    {
                        _gettingXPCompletion = 0.8f;
                    }
                    else if (MonoMain.pauseMenu is UIGachaBox)
                    {
                        _gettingXPCompletion = 0.9f;
                    }
                }
            }
            else
            {
                _gettingXP = false;
                _gettingXPCompletion = 0f;
            }
        }
        if (Network.isActive && (connection == null || connection.status == ConnectionStatus.Disconnected || profile == null || profile.connection == null || profile.connection.status == ConnectionStatus.Disconnected))
        {
            _experienceProfileCheck = null;
            _gettingXP = false;
            _open = false;
        }
        _fade = Lerp.Float(_fade, (_open && !_profileSelector.open && !_roomEditor.open) ? 1f : 0f, 0.1f);
        _blackFade = Lerp.Float(_blackFade, _open ? 1f : 0f, 0.1f);
        _screen.Update();
        if (_screen.transitioning)
        {
            _experienceProfileCheck = null;
            return;
        }
        if (_profileSelector.open || _roomEditor.open)
        {
            _experienceProfileCheck = null;
            return;
        }
        if (Profiles.IsDefault(_profile))
        {
            canEditRoom = false;
        }
        if (Profiles.experienceProfile == null)
        {
            canEditRoom = false;
        }
        _editRoomDisabled = false;
        if (NetworkDebugger.enabled)
        {
            canEditRoom = true;
            _editRoomDisabled = false;
        }
        else if (!canEditRoom && Network.isActive)
        {
            canEditRoom = true;
            _editRoomDisabled = true;
        }
        if (isArcadeHatSelector)
        {
            canEditRoom = false;
        }
        if (!_open)
        {
            if (_fade < 0.01f && _closing)
            {
                _closing = false;
                if (_box != null)
                {
                    _box.ReturnControl();
                }
            }
            _experienceProfileCheck = null;
            return;
        }
        if (_profile.team == null || _inputSkip)
        {
            _inputSkip = false;
            return;
        }
        _lcdFlashInc += Rando.Float(0.3f, 0.6f);
        _lcdFlash = 0.9f + ((float)Math.Sin(_lcdFlashInc) + 1f) / 2f * 0.1f;
        _ = _teamSelection;
        if (_prevDesiredTeam != _desiredTeamSelection && !base.isServerForObject)
        {
            if (TeamIndexAddSpecial(_desiredTeamSelection, 5) == _prevDesiredTeam)
            {
                _upSlideTo = -1f;
            }
            else if (TeamIndexAddSpecial(_desiredTeamSelection, -5) == _prevDesiredTeam)
            {
                _upSlideTo = 1f;
            }
            else if (TeamIndexAddSpecial(_desiredTeamSelection, -1) == _prevDesiredTeam)
            {
                _slideTo = 1f;
            }
            else if (TeamIndexAddSpecial(_desiredTeamSelection, 1) == _prevDesiredTeam)
            {
                _slideTo = -1f;
            }
            else if (TeamIndexAddSpecial(_desiredTeamSelection, -6) == _prevDesiredTeam)
            {
                _slideTo = 1f;
                _upSlideTo = 1f;
            }
            else if (TeamIndexAddSpecial(_desiredTeamSelection, -4) == _prevDesiredTeam)
            {
                _slideTo = -1f;
                _upSlideTo = 1f;
            }
            else if (TeamIndexAddSpecial(_desiredTeamSelection, 6) == _prevDesiredTeam)
            {
                _slideTo = -1f;
                _upSlideTo = -1f;
            }
            else if (TeamIndexAddSpecial(_desiredTeamSelection, 4) == _prevDesiredTeam)
            {
                _slideTo = 1f;
                _upSlideTo = -1f;
            }
            else
            {
                _teamSelection = _desiredTeamSelection;
            }
            SFX.Play("consoleTick", 0.6f);
            List<Team> allteams = AllTeams();
            if (_desiredTeamSelection < allteams.Count)
            {
                _teamName = allteams[_desiredTeamSelection].name.ToUpperInvariant();
                _netHoveringTeam = allteams[_desiredTeamSelection];
            }
            _prevDesiredTeam = _desiredTeamSelection;
        }
        if (_slideTo != 0f && _slide != _slideTo)
        {
            _slide = Lerp.Float(_slide, _slideTo, 0.1f);
        }
        else if (_slideTo != 0f && _slide == _slideTo)
        {
            _slide = 0f;
            _slideTo = 0f;
            _teamSelection = _desiredTeamSelection;
            if (base.isServerForObject)
            {
                Team t = AllTeams()[_desiredTeamSelection];
                if (!Main.isDemo || t.inDemo)
                {
                    SelectTeam();
                }
            }
        }
        if (_upSlideTo != 0f && _upSlide != _upSlideTo)
        {
            _upSlide = Lerp.Float(_upSlide, _upSlideTo, 0.1f);
        }
        else if (_upSlideTo != 0f && _upSlide == _upSlideTo)
        {
            _upSlide = 0f;
            _upSlideTo = 0f;
            _teamSelection = _desiredTeamSelection;
            if (base.isServerForObject)
            {
                Team t2 = AllTeams()[_desiredTeamSelection];
                if (!Main.isDemo || t2.inDemo)
                {
                    SelectTeam();
                }
            }
        }
        if (_selection == HSSelection.ChooseTeam)
        {
            if (_desiredTeamSelection == _teamSelection)
            {
                bool moved = false;
                if (inputProfile.Down("MENULEFT"))
                {
                    if (_desiredTeamSelection < DG.MaxPlayers)
                    {
                        _desiredTeamSelection = (short)(AllTeams().Count - 1);
                    }
                    else if (_desiredTeamSelection == DG.MaxPlayers)
                    {
                        _desiredTeamSelection = (short)ControllerNumber();
                    }
                    else
                    {
                        _desiredTeamSelection--;
                    }
                    _slideTo = -1f;
                    moved = true;
                    SFX.Play("consoleTick", 0.7f);
                }
                if (inputProfile.Down("MENURIGHT"))
                {
                    if (_desiredTeamSelection >= AllTeams().Count - 1)
                    {
                        _desiredTeamSelection = (short)ControllerNumber();
                    }
                    else if (_desiredTeamSelection < DG.MaxPlayers)
                    {
                        _desiredTeamSelection = (short)DG.MaxPlayers;
                    }
                    else
                    {
                        _desiredTeamSelection++;
                    }
                    _slideTo = 1f;
                    moved = true;
                    SFX.Play("consoleTick", 0.7f);
                }
                if (inputProfile.Down("MENUUP"))
                {
                    if (_desiredTeamSelection < DG.MaxPlayers)
                    {
                        _desiredTeamSelection = 0;
                    }
                    else
                    {
                        _desiredTeamSelection -= (short)(DG.MaxPlayers - 1);
                    }
                    _desiredTeamSelection -= 5;
                    if (_desiredTeamSelection < 0)
                    {
                        _desiredTeamSelection += (short)(AllTeams().Count - (DG.MaxPlayers - 1));
                    }
                    if (_desiredTeamSelection == 0)
                    {
                        _desiredTeamSelection = (short)ControllerNumber();
                    }
                    else
                    {
                        _desiredTeamSelection += (short)(DG.MaxPlayers - 1);
                    }
                    _upSlideTo = -1f;
                    moved = true;
                    SFX.Play("consoleTick", 0.7f);
                }
                if (inputProfile.Down("MENUDOWN"))
                {
                    if (_desiredTeamSelection < DG.MaxPlayers)
                    {
                        _desiredTeamSelection = 0;
                    }
                    else
                    {
                        _desiredTeamSelection -= (short)(DG.MaxPlayers - 1);
                    }
                    _desiredTeamSelection += 5;
                    if (_desiredTeamSelection >= AllTeams().Count - (DG.MaxPlayers - 1))
                    {
                        _desiredTeamSelection -= (short)(AllTeams().Count - (DG.MaxPlayers - 1));
                    }
                    if (_desiredTeamSelection == 0)
                    {
                        _desiredTeamSelection = (short)ControllerNumber();
                    }
                    else
                    {
                        _desiredTeamSelection += (short)(DG.MaxPlayers - 1);
                    }
                    _upSlideTo = 1f;
                    moved = true;
                    SFX.Play("consoleTick", 0.7f);
                }
                if (inputProfile.Pressed("SELECT") && !moved)
                {
                    if (_profile.team.locked)
                    {
                        SFX.Play("consoleError");
                    }
                    else
                    {
                        SFX.Play("consoleSelect", 0.4f);
                        _selection = HSSelection.Main;
                        _screen.DoFlashTransition();
                        ConfirmTeamSelection();
                    }
                }
                if (inputProfile.Pressed("RAGDOLL"))
                {
                    if (profile.requestedColor == -1)
                    {
                        profile.requestedColor = profile.currentColor;
                    }
                    profile.IncrementRequestedColor();
                    SFX.Play("consoleTick", 0.7f);
                    profile.UpdatePersona();
                }
                if (inputProfile.Pressed("CANCEL"))
                {
                    _desiredTeamSelection = (short)GetTeamIndex(_startingTeam);
                    _teamSelection = _desiredTeamSelection;
                    SelectTeam();
                    ConfirmTeamSelection();
                    SFX.Play("consoleCancel", 0.4f);
                    _selection = HSSelection.Main;
                    _screen.DoFlashTransition();
                }
            }
            Vector2 realPos = Position;
            Position = Vector2.Zero;
            _screen.BeginDraw();
            float selectorYOffset = -18f;
            _profile.persona.sprite.Alpha = _fade;
            _profile.persona.sprite.color = Color.White;
            _profile.persona.sprite.color = new Color(_profile.persona.sprite.color.R, _profile.persona.sprite.color.G, _profile.persona.sprite.color.B);
            _profile.persona.sprite.Depth = 0.9f;
            _profile.persona.sprite.Scale = new Vector2(1f, 1f);
            Graphics.Draw(_profile.persona.sprite, base.X + 70f, base.Y + 60f + selectorYOffset, 0.9f);
            short realTeam = 0;
            bool hasRealTeam = false;
            if (_teamSelection >= AllTeams().Count)
            {
                realTeam = _teamSelection;
                _teamSelection = (short)(AllTeams().Count - 1);
                hasRealTeam = true;
            }
            _ = AllTeams().Count;
            _ = DG.MaxPlayers;
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 7; i++)
                {
                    int add = -3 + i + (j - 2) * 5;
                    float xpos = base.X + 2f + (float)(i * 22) + (0f - _slide) * 20f;
                    float ypos = base.Y + 37f + (0f - _upSlide) * 20f;
                    int idx = TeamIndexAdd(_teamSelection, add);
                    if (idx == 3)
                    {
                        idx = ControllerNumber();
                    }
                    Team t3 = AllTeams()[idx];
                    float left = base.X + 2f;
                    float right = base.X + 2f + 154f;
                    float middle = base.X + (right - left) / 2f - 9f;
                    float distMult = Maths.Clamp((50f - Math.Abs(xpos - middle)) / 50f, 0f, 1f);
                    float colorMult = Maths.NormalizeSection(distMult, 0.9f, 1f) * 0.8f + 0.2f;
                    if (distMult < 0.5f)
                    {
                        colorMult = Maths.NormalizeSection(distMult, 0.1f, 0.2f) * 0.3f;
                    }
                    colorMult = 0.3f;
                    colorMult = Maths.NormalizeSection(distMult, 0f, 0.1f) * 0.3f;
                    switch (j)
                    {
                        case 0:
                            ypos -= distMult * 3f;
                            colorMult = ((!(_upSlide < 0f)) ? 0f : (Math.Abs(_upSlide) * colorMult));
                            break;
                        case 1:
                            ypos -= distMult * 3f;
                            if (_upSlide > 0f)
                            {
                                colorMult = (1f - Math.Abs(_upSlide)) * colorMult;
                            }
                            break;
                        case 2:
                            ypos -= distMult * 4f * (1f - Math.Abs(_upSlide));
                            ypos = ((!(_upSlide > 0f)) ? (ypos + distMult * 4f * Math.Abs(_upSlide)) : (ypos - distMult * 3f * Math.Abs(_upSlide)));
                            colorMult = Maths.NormalizeSection(distMult, 0.9f, 1f) * 0.7f + colorMult;
                            break;
                        case 3:
                            {
                                float slide = Math.Max(0f, _upSlide);
                                ypos += distMult * 4f * (1f - slide) + (0f - distMult) * 4f * slide;
                                if (_upSlide < 0f)
                                {
                                    colorMult = (1f - Math.Abs(_upSlide)) * colorMult;
                                }
                                break;
                            }
                        case 4:
                            ypos += distMult * 4f;
                            colorMult = ((!(_upSlide > 0f)) ? 0f : (Math.Abs(_upSlide) * colorMult));
                            break;
                    }
                    if (colorMult < 0.01f)
                    {
                        continue;
                    }
                    _profile.persona.sprite.Alpha = _fade;
                    _profile.persona.sprite.color = Color.White;
                    _profile.persona.sprite.color = new Color(_profile.persona.sprite.color.R, _profile.persona.sprite.color.G, _profile.persona.sprite.color.B);
                    _profile.persona.sprite.Depth = 0.9f;
                    _profile.persona.sprite.Scale = new Vector2(1f, 1f);
                    DuckRig.GetHatPoint(_profile.persona.sprite.imageIndex);
                    SpriteMap hat = t3.GetHat(_profile.persona);
                    Vector2 hatOffset = t3.hatOffset;
                    bool locked = t3.locked;
                    int tIdx = -1;
                    if (Network.isActive && !base.isServerForObject && _profile.networkHatUnlockStatuses != null)
                    {
                        tIdx = Teams.core.teams.IndexOf(t3);
                        if (tIdx >= 0 && tIdx < _profile.networkHatUnlockStatuses.Count)
                        {
                            locked = _profile.networkHatUnlockStatuses[tIdx];
                        }
                    }
                    if (locked)
                    {
                        hat = _lock;
                        if (t3.name == "Chancy")
                        {
                            hat = _goldLock;
                        }
                        hatOffset = new Vector2(-10f, -10f);
                    }
                    bool demoHat = Main.isDemo && !t3.inDemo;
                    if (demoHat)
                    {
                        hat = _demoBox;
                    }
                    hat.Depth = 0.95f;
                    hat.Alpha = _profile.persona.sprite.Alpha;
                    hat.color = Color.White * colorMult;
                    hat.Scale = new Vector2(1f, 1f);
                    if (!demoHat)
                    {
                        hat.Center = new Vector2(16f, 16f) + hatOffset;
                    }
                    if (idx > DG.MaxPlayers - 1 && _fade > 0.01f)
                    {
                        Vector2 drawPos = Vector2.Zero;
                        drawPos = ((!demoHat) ? new Vector2(xpos, ypos + selectorYOffset + (float)(j * 20) - 20f) : new Vector2(xpos + 2f, ypos + selectorYOffset + (float)(j * 20) - 20f + 1f));
                        drawPos = Maths.RoundToPixel(drawPos);
                        if (tIdx != -1 && !locked && t3.locked)
                        {
                            if (_outlineMaterial == null)
                            {
                                _outlineMaterial = new MaterialSecretOutline();
                            }
                            Graphics.material = _outlineMaterial;
                            Graphics.Draw(hat, drawPos.X, drawPos.Y);
                            Graphics.material = null;
                        }
                        else
                        {
                            if (t3.metadata != null && t3.metadata.UseDuckColor.value)
                            {
                                Graphics.material = _profile.persona.material;
                            }
                            Graphics.Draw(hat, drawPos.X, drawPos.Y);
                            Graphics.material = null;
                        }
                    }
                    _profile.persona.sprite.color = Color.White;
                    hat.color = Color.White;
                    _profile.persona.sprite.Scale = new Vector2(1f, 1f);
                    hat.Scale = new Vector2(1f, 1f);
                }
            }
            _font.Alpha = _fade;
            _font.Depth = 0.96f;
            string pickTeam = "NO PROFILE";
            if (!Profiles.IsDefault(_profile))
            {
                pickTeam = _profile.name;
            }
            if (_selection == HSSelection.ChooseProfile)
            {
                pickTeam = "> " + pickTeam + " <";
            }
            if (_selection == HSSelection.ChooseTeam)
            {
                string arrows = "<              >";
                Vector2 arrowPos = new Vector2(base.X + base.width / 2f - _font.GetWidth(arrows) / 2f, base.Y + 60f + selectorYOffset);
                arrowPos = Maths.RoundToPixel(arrowPos);
                _font.Draw(arrows, arrowPos.X, arrowPos.Y, Color.White, 0.95f);
            }
            string teamName = _profile.team.name;
            teamName = ((!(teamName == Teams.Player1.name) && !(teamName == Teams.Player2.name) && !(teamName == Teams.Player3.name) && !(teamName == Teams.Player4.name) && !(teamName == Teams.Player5.name) && !(teamName == Teams.Player6.name) && !(teamName == Teams.Player7.name) && !(teamName == Teams.Player8.name)) ? _profile.team.GetNameForDisplay() : "NO TEAM");
            if (_teamName != "")
            {
                teamName = _teamName;
            }
            bool locked2 = _profile.team.locked;
            bool netcheck = false;
            if (Network.isActive && !base.isServerForObject && _profile.networkHatUnlockStatuses != null && _desiredTeamSelection < _profile.networkHatUnlockStatuses.Count)
            {
                locked2 = _profile.networkHatUnlockStatuses[_desiredTeamSelection];
                netcheck = true;
            }
            if (locked2)
            {
                teamName = "LOCKED";
            }
            else if (netcheck && _netHoveringTeam != null && _netHoveringTeam.locked)
            {
                teamName = "UNKNOWN";
            }
            _font.Scale = new Vector2(1f, 1f);
            float wide = _font.GetWidth(teamName);
            Vector2 teamNamePos = new Vector2(base.X + base.width / 2f - wide / 2f, base.Y + 25f + selectorYOffset);
            teamNamePos = Maths.RoundToPixel(teamNamePos);
            _font.Draw(teamName, teamNamePos.X, teamNamePos.Y, Color.LimeGreen * ((_selection == HSSelection.ChooseTeam) ? 1f : 0.6f), 0.95f);
            Graphics.DrawLine(teamNamePos + new Vector2(-10f, 4f), teamNamePos + new Vector2(wide + 10f, 4f), Color.White * 0.1f, 2f, 0.93f);
            string buttons = "@SELECT@";
            _font.Draw(buttons, base.X + 4f, base.Y + 79f, new Color(180, 180, 180), 0.95f, profileInput);
            buttons = "@RAGDOLL@";
            _font.Draw(buttons, base.X + 122f, base.Y + 79f, new Color(180, 180, 180), 0.95f, profileInput);
            _screen.EndDraw();
            Position = realPos;
            if (hasRealTeam)
            {
                _teamSelection = realTeam;
            }
        }
        else
        {
            if (_selection != HSSelection.Main)
            {
                return;
            }
            if (Level.current is ArcadeLevel && !Options.Data.defaultAccountMerged)
            {
                if (_experienceProfileCheck != _profile)
                {
                    if (_profile == Profiles.experienceProfile)
                    {
                        HUD.AddCornerControl(HUDCorner.BottomLeft, "@MENU2@MERGE DEFAULT", inputProfile);
                    }
                    else
                    {
                        HUD.CloseCorner(HUDCorner.BottomLeft);
                    }
                    _experienceProfileCheck = _profile;
                }
                if (_profile == Profiles.experienceProfile && inputProfile.Pressed("MENU2"))
                {
                    UIMenu uIMenu = Options.CreateProfileMergeMenu();
                    Level.Add(uIMenu);
                    MonoMain.pauseMenu = uIMenu;
                    uIMenu.Open();
                }
            }
            if (inputProfile.Pressed("MENUUP"))
            {
                if (_mainSelection > 0)
                {
                    _mainSelection--;
                    SFX.Play("consoleTick");
                    if (_editRoomDisabled && _mainSelection == 2)
                    {
                        _mainSelection = 1;
                    }
                }
            }
            else if (inputProfile.Pressed("MENUDOWN"))
            {
                if (_mainSelection < (canEditRoom ? 3 : 2))
                {
                    _mainSelection++;
                    SFX.Play("consoleTick");
                }
                if (_editRoomDisabled && _mainSelection == 2)
                {
                    _mainSelection = 3;
                }
            }
            else if (inputProfile.Pressed("SELECT"))
            {
                if (_mainSelection == 1 && (!Network.isActive || !Profiles.IsExperience(_profile)))
                {
                    _profileSelector.Open(_profile);
                    SFX.Play("consoleSelect", 0.4f);
                    _fade = 0f;
                    _screen.DoFlashTransition();
                }
                else if (_mainSelection == 0)
                {
                    _selection = HSSelection.ChooseTeam;
                    SFX.Play("consoleSelect", 0.4f);
                    _screen.DoFlashTransition();
                }
                else if (_mainSelection == (canEditRoom ? 3 : 2))
                {
                    _open = false;
                    _closing = true;
                    SFX.Play("consoleCancel", 0.4f);
                    _selection = HSSelection.Main;
                }
                else if (canEditRoom && _mainSelection == 2)
                {
                    _editingRoom = true;
                    _roomEditor.Open(_profile);
                    SFX.Play("consoleSelect", 0.4f);
                    _fade = 0f;
                    _screen.DoFlashTransition();
                }
            }
            else if (_mainSelection == 1 && inputProfile.Pressed("MENU1") && !Profiles.IsDefault(_profile))
            {
                _profileSelector.EditProfile(_profile);
                SFX.Play("consoleSelect", 0.4f);
                _fade = 0f;
                _screen.DoFlashTransition();
            }
            else if (inputProfile.Pressed("CANCEL"))
            {
                _open = false;
                _closing = true;
                SFX.Play("consoleCancel", 0.4f);
                _selection = HSSelection.Main;
            }
            _screen.BeginDraw();
            _font.Scale = new Vector2(1f, 1f);
            string text = "@LWING@CUSTOM DUCK@RWING@";
            _font.Draw(text, Maths.RoundToPixel(new Vector2(base.width / 2f - _font.GetWidth(text) / 2f, 10f)), Color.White, 0.95f);
            text = ((!Profiles.IsDefault(_profile)) ? _profile.name : "PICK PROFILE");
            float itemSize = _font.GetWidth(text);
            Vector2 itemPos = Maths.RoundToPixel(new Vector2(base.width / 2f - itemSize / 2f, 39f));
            _font.Draw(text, itemPos, Colors.MenuOption * ((_mainSelection == 1) ? 1f : 0.6f), 0.95f);
            if (_mainSelection == 1)
            {
                Graphics.Draw(_contextArrow, itemPos.X - 8f, itemPos.Y);
            }
            if (canEditRoom)
            {
                text = "@RAINBOWICON@EDIT ROOM";
                itemSize = _font.GetWidth(text);
                itemPos = Maths.RoundToPixel(new Vector2(base.width / 2f - itemSize / 2f, 48f));
                _font.Draw(text, itemPos, _editRoomDisabled ? Colors.SuperDarkBlueGray : (Colors.MenuOption * ((_mainSelection == 2) ? 1f : 0.6f)), 0.95f, null, colorSymbols: true);
                if (_mainSelection == 2)
                {
                    Graphics.Draw(_contextArrow, itemPos.X - 8f, itemPos.Y);
                }
            }
            text = (_profile.team.hasHat ? ("|LIME|" + _profile.team.GetNameForDisplay() + "|MENUORANGE| HAT") : "|MENUORANGE|CHOOSE HAT");
            itemSize = _font.GetWidth(text);
            itemPos = Maths.RoundToPixel(new Vector2(base.width / 2f - itemSize / 2f, 30f));
            _font.Draw(text, itemPos, Color.White * ((_mainSelection == 0) ? 1f : 0.6f), 0.95f);
            if (_mainSelection == 0)
            {
                Graphics.Draw(_contextArrow, itemPos.X - 8f, itemPos.Y);
            }
            text = "EXIT";
            itemSize = _font.GetWidth(text);
            itemPos = Maths.RoundToPixel(new Vector2(base.width / 2f - itemSize / 2f, 50 + (canEditRoom ? 12 : 9)));
            _font.Draw(text, itemPos, Colors.MenuOption * ((_mainSelection == (canEditRoom ? 3 : 2)) ? 1f : 0.6f), 0.95f);
            if (_mainSelection == (canEditRoom ? 3 : 2))
            {
                Graphics.Draw(_contextArrow, itemPos.X - 8f, itemPos.Y);
            }
            string buttons2 = "@SELECT@";
            _font.Draw(buttons2, 4f, 79f, new Color(180, 180, 180), 0.95f, profileInput);
            buttons2 = ((_mainSelection != 1 || Profiles.IsDefault(_profile)) ? "@CANCEL@" : "@MENU1@");
            _font.Draw(buttons2, 122f, 79f, new Color(180, 180, 180), 0.95f, profileInput);
            _consoleText.color = new Color(140, 140, 140);
            Graphics.Draw(_consoleText, 30f, 18f);
            _screen.EndDraw();
        }
    }

    public void Open(Profile p)
    {
        _profile = p;
        _startingTeam = _profile.team;
        _open = true;
        _mainSelection = 0;
        _editingRoom = false;
        _gettingXP = false;
        _selection = HSSelection.Main;
        _teamSelection = (_desiredTeamSelection = (short)GetTeamIndex(_profile.team));
        _inputSkip = true;
    }

    public override void Draw()
    {
        if (_profileBoxNumber < 0 || _box == null)
        {
            return;
        }
        fakefade = false;
        if (Network.isActive && _box.profile != null && _box.profile.connection != DuckNetwork.localConnection)
        {
            _blindLerp = Lerp.Float(_blindLerp, (_editingRoom || _gettingXP) ? 1f : 0f, 0.05f);
            if (_blindLerp > 0.01f)
            {
                for (int i = 0; i < 8; i++)
                {
                    _blind.ScaleY = Math.Max(0f, Math.Min(_blindLerp * 3f - (float)i * 0.05f, 1f));
                    _blind.Depth = 0.91f + (float)i * 0.008f;
                    _blind.flipH = false;
                    Graphics.Draw(_blind, base.X - 3f + (float)i * (9f * _blindLerp), base.Y + 1f);
                    _blind.flipH = true;
                    Graphics.Draw(_blind, base.X + 4f + 140f - (float)i * (9f * _blindLerp), base.Y + 1f);
                }
                float endlerp = Math.Max((_blindLerp - 0.5f) * 2f, 0f);
                if (endlerp > 0.01f)
                {
                    if (_gettingXP)
                    {
                        _gettingXPBoard.Depth = 0.99f;
                        _gettingXPBoard.frame = (int)Math.Round(_gettingXPCompletion * 9f);
                        Graphics.Draw(_gettingXPBoard, base.X + 71f, base.Y + 43f * endlerp);
                        _boardLoader.Depth = 0.995f;
                        Graphics.Draw(_boardLoader, base.X + 94f, base.Y + 52f * endlerp);
                    }
                    else if (_editingRoom)
                    {
                        _editingRoomBoard.Depth = 0.99f;
                        Graphics.Draw(_editingRoomBoard, base.X + 71f, base.Y + 43f * endlerp);
                        _boardLoader.Depth = 0.995f;
                        Graphics.Draw(_boardLoader, base.X + 94f, base.Y + 52f * endlerp);
                    }
                }
            }
            if (_editingRoom)
            {
                fakefade = true;
            }
        }
        if (fadeVal < 0.01f || _roomEditor._mode == REMode.Place)
        {
            return;
        }
        Graphics.Draw(_screen.target, Position + new Vector2(3f, 3f), null, new Color(_screen.darken, _screen.darken, _screen.darken) * fadeVal, 0f, Vector2.Zero, new Vector2(0.25f, 0.25f), SpriteEffects.None, 0.82f);
        _selectBorder.Alpha = fadeVal;
        _selectBorder.Depth = 0.85f;
        Graphics.Draw(_selectBorder, base.X - 1f, base.Y, new Rectangle(0f, 0f, 4f, _selectBorder.height));
        Graphics.Draw(_selectBorder, base.X - 1f + (float)_selectBorder.width - 4f, base.Y, new Rectangle(_selectBorder.width - 4, 0f, 4f, _selectBorder.height));
        Graphics.Draw(_selectBorder, base.X - 1f + 4f, base.Y, new Rectangle(4f, 0f, _selectBorder.width - 8, 4f));
        Graphics.Draw(_selectBorder, base.X - 1f + 4f, base.Y + (float)(_selectBorder.height - 25), new Rectangle(4f, _selectBorder.height - 25, _selectBorder.width - 8, 25f));
        string buttons = _firstWord;
        _font.Scale = new Vector2(1f, 1f);
        _font.Draw(buttons, base.X + 25f, base.Y + 79f, new Color(163, 206, 39) * fadeVal * _lcdFlash, 0.9f);
        buttons = _secondWord;
        _font.Scale = new Vector2(1f, 1f);
        _font.Draw(buttons, base.X + 116f - _font.GetWidth(buttons), base.Y + 79f, new Color(163, 206, 39) * fadeVal * _lcdFlash, 0.9f);
        if (_selection == HSSelection.ChooseTeam)
        {
            _firstWord = "OK";
            _secondWord = "COLOR";
        }
        else if (_selection == HSSelection.Main)
        {
            _firstWord = "PICK";
            if (_mainSelection == 1 && !Profiles.IsDefault(_profile))
            {
                _secondWord = "EDIT";
            }
            else
            {
                _secondWord = "EXIT";
            }
        }
    }
}
