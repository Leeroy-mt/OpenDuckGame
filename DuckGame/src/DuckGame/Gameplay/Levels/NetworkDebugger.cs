using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DuckGame;

public class NetworkDebugger : Level
{
    public class GhostDebugData
    {
        public bool hover;

        public Dictionary<DuckPersona, long> dataReceivedFrames = new Dictionary<DuckPersona, long>();
    }

    public class Recorder
    {
        public static Recorder active;

        private Dictionary<int, List<ushort>> inputs = new Dictionary<int, List<ushort>>();

        public int seed = 1337;

        public string level = "";

        public int activeIndex;

        public int frame;

        public void Log(ushort pInputState)
        {
            List<ushort> ins = null;
            if (!inputs.TryGetValue(currentIndex, out ins))
            {
                ins = (inputs[currentIndex] = new List<ushort>());
            }
            ins.Add(pInputState);
        }

        public ushort Get()
        {
            List<ushort> ins = null;
            if (inputs.TryGetValue(currentIndex, out ins))
            {
                if (frame < ins.Count)
                {
                    return ins[frame];
                }
                return 0;
            }
            return 0;
        }
    }

    public const int kNumDebuggers = 4;

    public static List<ulong> profileIDs = new List<ulong> { 0uL, 1uL, 2uL, 3uL, 5uL, 6uL, 7uL, 8uL };

    public static bool enableFrameTimeDebugging = false;

    private static int _currentIndex = 0;

    public static List<InputProfile> inputProfiles = new List<InputProfile>();

    private static bool _enabled = false;

    public static List<NetworkInstance> _instances = new List<NetworkInstance>();

    private Level _startLevel;

    private LayerCore _startLayer;

    private bool _ghostDebugger;

    private MultiMap<string, int> _controlsMapGamepad;

    private MultiMap<string, int> _controlsMapKeyboard;

    private MultiMap<string, int> _controlsMapKeyboard2;

    private InputProfile _defaultInput;

    public static NetworkDebugger instance;

    private static int _lastRect = 0;

    public static bool letJoin = false;

    public static bool startJoin = false;

    public static bool hoveringInstance = true;

    public static int fullscreenIndex = 0;

    private bool eightPlayerMode = true;

    private bool logTimes = true;

    private bool logSections = true;

    private int logSwitchIndex;

    public static Dictionary<GhostObject, GhostDebugData> _ghostDebug = new Dictionary<GhostObject, GhostDebugData>();

    public bool lefpres;

    public static Dictionary<string, Dictionary<string, float>> _sentPulse = new Dictionary<string, Dictionary<string, float>>();

    public static Dictionary<string, Dictionary<string, float>> _receivedPulse = new Dictionary<string, Dictionary<string, float>>();

    public static bool showLogs = false;

    public static int[] showLogPage = new int[4] { 0, -1, -1, -1 };

    public static int[] logsScroll = new int[4];

    private Vector2[] mouseClickPos = new Vector2[4];

    private Vector2[] mouseClickTop = new Vector2[4];

    private bool[] scrollerDrag = new bool[4];

    private float wheel;

    private Dictionary<DCSection, bool> logFilters = new Dictionary<DCSection, bool>
    {
        {
            DCSection.General,
            true
        },
        {
            DCSection.NetCore,
            true
        },
        {
            DCSection.DuckNet,
            true
        },
        {
            DCSection.GhostMan,
            true
        },
        {
            DCSection.Steam,
            true
        },
        {
            DCSection.Mod,
            true
        },
        {
            DCSection.Connection,
            true
        },
        {
            DCSection.Ack,
            true
        }
    };

    private bool showFilters;

    private SpriteMap _connectionArrow;

    private Sprite _connectionX;

    private Sprite _connectionWall;

    public static int[] ghostsReceived = new int[8];

    private static Network oldNetwork;

    private static DuckNetworkCore oldDuckNetworkCore;

    private static VirtualTransitionCore oldVirtualCore;

    private static LevelCore oldLevelCore;

    private static ProfilesCore oldProfileCore;

    private static TeamsCore oldTeamCore;

    private static LayerCore oldLayerCore;

    private static InputProfileCore oldInputCore;

    private static DevConsoleCore oDevCore;

    private static CrowdCore oldCrowdCore;

    private static GameModeCore oldGameModeCore;

    private static ConnectionStatusUICore oldConnectionUICore;

    private static MonoMainCore oldMonoCore;

    private static HUDCore oldHUDCore;

    private static MatchmakingBoxCore oldMatchmakingCore;

    private static AutoUpdatables.Core oldAUCore;

    private static Random oldRando;

    private static List<NetworkInstance.Core> _registeredCores = new List<NetworkInstance.Core>();

    public static int currentIndex => _currentIndex;

    public static bool enabled => _enabled;

    public NetworkDebugger(Level level = null, LayerCore startLayer = null, bool pGhostDebugger = false)
    {
        _startLevel = level;
        _startLayer = startLayer;
        if (level == null)
        {
            foreach (Profile item in Profiles.all)
            {
                item.team = null;
            }
        }
        for (int i = 0; i < 8; i++)
        {
            inputProfiles.Add(new InputProfile());
        }
        _ghostDebugger = pGhostDebugger;
    }

    public static int CurrentServerIndex()
    {
        int idx = 0;
        foreach (NetworkInstance i in _instances)
        {
            if (i.network.core.isActive && i.network.core.isServer)
            {
                return idx;
            }
            idx++;
        }
        return -1;
    }

    public void RefreshRectSizes()
    {
        for (int i = 0; i < 4; i++)
        {
            RefreshRectSize(_instances[i], i);
        }
    }

    public void RefreshRectSize(NetworkInstance host, int index)
    {
        switch (index)
        {
            case 0:
                host.rect = new Rectangle(0f, 0f, Resolution.current.x / 2, Resolution.current.y / 2);
                break;
            case 1:
                host.rect = new Rectangle(Resolution.current.x / 2, 0f, Resolution.current.x / 2, Resolution.current.y / 2);
                break;
            case 2:
                host.rect = new Rectangle(0f, Resolution.current.y / 2, Resolution.current.x / 2, Resolution.current.y / 2);
                break;
            case 3:
                host.rect = new Rectangle(Resolution.current.x / 2, Resolution.current.y / 2, Resolution.current.x / 2, Resolution.current.y / 2);
                break;
        }
    }

    public void CreateInstance(int init, bool isHost)
    {
        _currentIndex = init;
        NetworkInstance host = new NetworkInstance
        {
            network = new Network(_currentIndex)
        };
        RefreshRectSize(host, _currentIndex);
        List<Team> extra = new List<Team>();
        foreach (Team t in Teams.core.extraTeams)
        {
            extra.Add(t.Clone());
        }
        if (_startLevel == null)
        {
            host.teamsCore = new TeamsCore();
        }
        else
        {
            host.teamsCore = Teams.core;
            Teams.core = new TeamsCore();
            Teams.core.Initialize();
        }
        if (_startLayer != null)
        {
            host.layerCore = _startLayer;
            _startLayer = null;
        }
        else
        {
            host.layerCore = new LayerCore();
            host.layerCore.InitializeLayers();
        }
        host.profileCore = Profiles.core;
        Profiles.core = new ProfilesCore();
        Profiles.core.Initialize();
        host.virtualCore = new VirtualTransitionCore();
        host.inputProfile = new InputProfileCore();
        host.levelCore = new LevelCore();
        host.crowdCore = new CrowdCore();
        host.duckNetworkCore = new DuckNetworkCore(waitInit: true);
        host.gameModeCore = new GameModeCore();
        host.connectionUICore = new ConnectionStatusUICore();
        host.consoleCore = new DevConsoleCore();
        host.monoCore = new MonoMainCore();
        host.hudCore = new HUDCore();
        host.matchmakingCore = new MatchmakingBoxCore();
        host.auCore = new AutoUpdatables.Core();
        host.rando = new Random();
        LockInstance(host);
        host.duckNetworkCore.RecreateProfiles();
        Teams.core.Initialize();
        if (init == 0 || init == 1)
        {
            Teams.core.extraTeams = extra;
        }
        Profiles.core.Initialize();
        host.virtualCore.Initialize();
        Input.InitDefaultProfiles();
        host.network.DoInitialize();
        DuckNetwork.Initialize();
        foreach (Team item in Teams.all)
        {
            item.ClearProfiles();
        }
        Level.current = new TeamSelect2();
        host.joined = true;
        if (init >= _instances.Count)
        {
            _instances.Add(host);
        }
        else
        {
            _instances[init] = host;
        }
        base.Initialize();
        if (init != 0)
        {
            switch (init)
            {
                case 1:
                    Profiles.experienceProfile.name = "DAN RANDO";
                    break;
                case 2:
                    Profiles.experienceProfile.name = "Zoo Tycoon 2";
                    Profiles.experienceProfile.preferredColor = 7;
                    break;
                case 3:
                    Profiles.experienceProfile.name = "xXspandeXx";
                    break;
                case 4:
                    Profiles.experienceProfile.name = "boloBoy";
                    break;
                case 5:
                    Profiles.experienceProfile.name = "MINTY TASTE";
                    break;
                case 6:
                    Profiles.experienceProfile.name = "r_b_sprinkles";
                    break;
                case 7:
                    Profiles.experienceProfile.name = "darren";
                    break;
            }
            Profiles.experienceProfile.keepSetName = true;
            Profiles.experienceProfile.furniturePositions.Clear();
        }
        host.debugInterface = new NetDebugInterface(_instances[init]);
        UnlockInstance(_instances[init]);
        foreach (NetworkInstance.Core c in _registeredCores)
        {
            _instances[init].extraCores.Add(new NetworkInstance.Core
            {
                member = c.member,
                originalInstance = c.originalInstance,
                instance = Activator.CreateInstance(c.member.FieldType, null),
                firstLockAction = c.firstLockAction
            });
        }
    }

    public override void Initialize()
    {
        _enabled = true;
        _currentIndex = 0;
        _controlsMapGamepad = InputProfile.DefaultPlayer1.GetControllerMap<GenericController>();
        _controlsMapKeyboard = InputProfile.defaultProfiles[Options.Data.keyboard1PlayerIndex].GetControllerMap<Keyboard>();
        _controlsMapKeyboard2 = InputProfile.defaultProfiles[Options.Data.keyboard2PlayerIndex].GetControllerMap<Keyboard>();
        for (int i = 0; i < 4; i++)
        {
            CreateInstance(i, isHost: true);
        }
        Level.activeLevel = this;
        base.Initialize();
    }

    public static void TerminateThreads()
    {
        foreach (NetworkInstance instance in _instances)
        {
            LockInstance(instance);
            instance.network.core.Terminate();
            UnlockInstance(instance);
        }
    }

    public override void DoUpdate()
    {
        NetworkDebugger.instance = this;
        if (Keyboard.Down(Keys.LeftShift) && Keyboard.Pressed(Keys.L))
        {
            showLogs = !showLogs;
        }
        MonoMain.instance.IsMouseVisible = true;
        if (Mouse.left == InputState.Pressed)
        {
            lefpres = true;
        }
        else
        {
            lefpres = false;
        }
        List<DCLine> debuggerLines = null;
        lock (DevConsole.debuggerLines)
        {
            debuggerLines = DevConsole.debuggerLines;
            DevConsole.debuggerLines = new List<DCLine>();
        }
        _defaultInput = InputProfile.DefaultPlayer1;
        for (int i = 0; i < 4; i++)
        {
            foreach (NetworkInstance instance2 in _instances)
            {
                if (instance2.inputProfile.DefaultPlayer1.JoinGamePressed())
                {
                    letJoin = true;
                }
            }
            NetworkInstance instance = _instances[i];
            _currentIndex = i;
            LockInstance(instance);
            bool hov = false;
            if (_lastRect == _currentIndex || instance.rect.Contains(Mouse.mousePos) || (Math.Abs((float)(Graphics.width / 2) - Mouse.mousePos.X) < 32f && Math.Abs((float)(Graphics.height / 2) - Mouse.mousePos.Y) < 32f))
            {
                _lastRect = _currentIndex;
                InputProfile.active = InputProfile.DefaultPlayer1;
                InputProfile.DefaultPlayer1.SetGenericControllerMapIndex<GenericController>(0, _controlsMapGamepad);
                InputProfile.DefaultPlayer2.SetGenericControllerMapIndex<GenericController>(1, _controlsMapGamepad);
                InputProfile.DefaultPlayer3.SetGenericControllerMapIndex<GenericController>(2, _controlsMapGamepad);
                InputProfile.DefaultPlayer4.SetGenericControllerMapIndex<GenericController>(3, _controlsMapGamepad);
                InputProfile.Get(InputProfile.MPPlayer1).SetGenericControllerMapIndex<GenericController>(0, _controlsMapGamepad);
                InputProfile.Get(InputProfile.MPPlayer2).SetGenericControllerMapIndex<GenericController>(1, _controlsMapGamepad);
                InputProfile.Get(InputProfile.MPPlayer3).SetGenericControllerMapIndex<GenericController>(2, _controlsMapGamepad);
                InputProfile.Get(InputProfile.MPPlayer4).SetGenericControllerMapIndex<GenericController>(3, _controlsMapGamepad);
                InputProfile.DefaultPlayer1.SetGenericControllerMapIndex<Keyboard>(0, null);
                InputProfile.DefaultPlayer2.SetGenericControllerMapIndex<Keyboard>(1, null);
                InputProfile.DefaultPlayer3.SetGenericControllerMapIndex<Keyboard>(2, null);
                InputProfile.DefaultPlayer4.SetGenericControllerMapIndex<Keyboard>(3, null);
                InputProfile.Get(InputProfile.MPPlayer1).SetGenericControllerMapIndex<Keyboard>(0, null);
                InputProfile.Get(InputProfile.MPPlayer2).SetGenericControllerMapIndex<Keyboard>(1, null);
                InputProfile.Get(InputProfile.MPPlayer3).SetGenericControllerMapIndex<Keyboard>(2, null);
                InputProfile.Get(InputProfile.MPPlayer4).SetGenericControllerMapIndex<Keyboard>(3, null);
                InputProfile.defaultProfiles[Options.Data.keyboard1PlayerIndex].SetGenericControllerMapIndex<Keyboard>(0, _controlsMapKeyboard);
                InputProfile.Get(InputProfile.MPPlayers[Options.Data.keyboard1PlayerIndex]).SetGenericControllerMapIndex<Keyboard>(0, _controlsMapKeyboard);
                InputProfile.defaultProfiles[Options.Data.keyboard2PlayerIndex].SetGenericControllerMapIndex<Keyboard>(1, _controlsMapKeyboard2);
                InputProfile.Get(InputProfile.MPPlayers[Options.Data.keyboard2PlayerIndex]).SetGenericControllerMapIndex<Keyboard>(1, _controlsMapKeyboard2);
                hoveringInstance = true;
                hov = true;
            }
            else
            {
                InputProfile.active = InputProfile.DefaultPlayer4;
                InputProfile.DefaultPlayer1.SetGenericControllerMapIndex<GenericController>(0, null);
                InputProfile.DefaultPlayer2.SetGenericControllerMapIndex<GenericController>(1, null);
                InputProfile.DefaultPlayer3.SetGenericControllerMapIndex<GenericController>(2, null);
                InputProfile.DefaultPlayer4.SetGenericControllerMapIndex<GenericController>(3, null);
                InputProfile.Get(InputProfile.MPPlayer1).SetGenericControllerMapIndex<GenericController>(0, null);
                InputProfile.Get(InputProfile.MPPlayer2).SetGenericControllerMapIndex<GenericController>(1, null);
                InputProfile.Get(InputProfile.MPPlayer3).SetGenericControllerMapIndex<GenericController>(2, null);
                InputProfile.Get(InputProfile.MPPlayer4).SetGenericControllerMapIndex<GenericController>(3, null);
                InputProfile.DefaultPlayer1.SetGenericControllerMapIndex<Keyboard>(0, null);
                InputProfile.DefaultPlayer2.SetGenericControllerMapIndex<Keyboard>(1, null);
                InputProfile.DefaultPlayer3.SetGenericControllerMapIndex<Keyboard>(2, null);
                InputProfile.DefaultPlayer4.SetGenericControllerMapIndex<Keyboard>(3, null);
                InputProfile.Get(InputProfile.MPPlayer1).SetGenericControllerMapIndex<Keyboard>(0, null);
                InputProfile.Get(InputProfile.MPPlayer2).SetGenericControllerMapIndex<Keyboard>(1, null);
                InputProfile.Get(InputProfile.MPPlayer3).SetGenericControllerMapIndex<Keyboard>(2, null);
                InputProfile.Get(InputProfile.MPPlayer4).SetGenericControllerMapIndex<Keyboard>(3, null);
                hoveringInstance = false;
            }
            InputProfile.Update();
            if (Recorder.active != null)
            {
                if (currentIndex == Recorder.active.activeIndex)
                {
                    if (InputProfile.DefaultPlayer1.virtualDevice != null)
                    {
                        InputProfile.DefaultPlayer1.virtualDevice.SetState(0);
                        InputProfile.DefaultPlayer1.virtualDevice.SetState(0);
                        InputProfile.DefaultPlayer1.virtualDevice = null;
                    }
                    Recorder.active.Log(InputProfile.DefaultPlayer1.state);
                }
                else
                {
                    if (InputProfile.DefaultPlayer1.virtualDevice == null)
                    {
                        InputProfile.DefaultPlayer1.virtualDevice = VirtualInput.debuggerInputs[currentIndex];
                        for (int inp = 0; inp < Network.synchronizedTriggers.Count; inp++)
                        {
                            InputProfile.DefaultPlayer1.Map(VirtualInput.debuggerInputs[currentIndex], Network.synchronizedTriggers[inp], inp);
                        }
                        VirtualInput.debuggerInputs[currentIndex].availableTriggers = Network.synchronizedTriggers;
                    }
                    InputProfile.DefaultPlayer1.virtualDevice.SetState(Recorder.active.Get());
                }
            }
            foreach (DCLine line in debuggerLines)
            {
                if (line.threadIndex == i)
                {
                    DevConsole.core.lines.Enqueue(line);
                }
            }
            bool skipFrame = false;
            if (hov && Keyboard.Pressed(Keys.OemMinus))
            {
                skipFrame = true;
            }
            if (!skipFrame)
            {
                Network.netGraph.PreUpdate();
                DevConsole.Update();
                Network.PreUpdate();
                MonoMain.UpdatePauseMenu();
                if (!MonoMain.shouldPauseGameplay)
                {
                    foreach (IEngineUpdatable engineUpdatable in MonoMain.core.engineUpdatables)
                    {
                        engineUpdatable.PreUpdate();
                    }
                    AutoUpdatables.Update();
                    FireManager.Update();
                    Level.UpdateLevelChange();
                    Level.UpdateCurrentLevel();
                    foreach (IEngineUpdatable engineUpdatable2 in MonoMain.core.engineUpdatables)
                    {
                        engineUpdatable2.Update();
                    }
                }
                if (!showLogs)
                {
                    instance.debugInterface.Update();
                }
                Network.PostUpdate();
                foreach (IEngineUpdatable engineUpdatable3 in MonoMain.core.engineUpdatables)
                {
                    engineUpdatable3.PostUpdate();
                }
                instance.network.core.Thread_Loop();
            }
            UnlockInstance(instance);
        }
        if (Recorder.active != null)
        {
            Recorder.active.frame++;
        }
        if (showLogs)
        {
            showFilters = false;
            if (Keyboard.Down(Keys.LeftControl))
            {
                showFilters = true;
                for (int j = 49; j <= 56; j++)
                {
                    if (!Keyboard.Pressed((Keys)j))
                    {
                        continue;
                    }
                    int idx = j - 49;
                    if (Keyboard.Down(Keys.LeftShift))
                    {
                        for (int k = 0; k < 8; k++)
                        {
                            logFilters[logFilters.ElementAt(k).Key] = k == idx;
                        }
                    }
                    else
                    {
                        logFilters[logFilters.ElementAt(idx).Key] = !logFilters.ElementAt(idx).Value;
                    }
                }
                if (Keyboard.Pressed(Keys.D9))
                {
                    if (logFilters[logFilters.ElementAt(0).Key] && logFilters[logFilters.ElementAt(1).Key] && logFilters[logFilters.ElementAt(2).Key] && logFilters[logFilters.ElementAt(3).Key] && logFilters[logFilters.ElementAt(4).Key] && logFilters[logFilters.ElementAt(5).Key] && logFilters[logFilters.ElementAt(6).Key])
                    {
                        for (int l = 0; l < 8; l++)
                        {
                            logFilters[logFilters.ElementAt(l).Key] = false;
                        }
                    }
                    else
                    {
                        for (int m = 0; m < 8; m++)
                        {
                            logFilters[logFilters.ElementAt(m).Key] = true;
                        }
                    }
                }
            }
            else if (Keyboard.Down(Keys.LeftShift))
            {
                if (Keyboard.Pressed(Keys.D1))
                {
                    logSwitchIndex = 0;
                    if (showLogPage[logSwitchIndex] < 0)
                    {
                        showLogPage[logSwitchIndex] = logSwitchIndex;
                    }
                }
                else if (Keyboard.Pressed(Keys.D2))
                {
                    logSwitchIndex = 1;
                    if (showLogPage[logSwitchIndex] < 0)
                    {
                        showLogPage[logSwitchIndex] = logSwitchIndex;
                    }
                }
                else if (Keyboard.Pressed(Keys.D3))
                {
                    logSwitchIndex = 2;
                    if (showLogPage[logSwitchIndex] < 0)
                    {
                        showLogPage[logSwitchIndex] = logSwitchIndex;
                    }
                }
                else if (Keyboard.Pressed(Keys.D4))
                {
                    logSwitchIndex = 3;
                    if (showLogPage[logSwitchIndex] < 0)
                    {
                        showLogPage[logSwitchIndex] = logSwitchIndex;
                    }
                }
                if (Keyboard.Pressed(Keys.T))
                {
                    logTimes = !logTimes;
                }
                else if (Keyboard.Pressed(Keys.S))
                {
                    logSections = !logSections;
                }
            }
            else
            {
                if (Keyboard.Pressed(Keys.D1))
                {
                    showLogPage[logSwitchIndex] = 0;
                }
                else if (Keyboard.Pressed(Keys.D2))
                {
                    showLogPage[logSwitchIndex] = 1;
                }
                else if (Keyboard.Pressed(Keys.D3))
                {
                    showLogPage[logSwitchIndex] = 2;
                }
                else if (Keyboard.Pressed(Keys.D4))
                {
                    showLogPage[logSwitchIndex] = 3;
                }
                else if (Keyboard.Pressed(Keys.D0))
                {
                    showLogPage[logSwitchIndex] = -1;
                }
                while (showLogPage[logSwitchIndex] < 0 && logSwitchIndex > 0)
                {
                    logSwitchIndex--;
                }
            }
        }
        wheel = Mouse.scroll;
        for (int n = 0; n < 4; n++)
        {
            LockInstance(_instances[n]);
            _currentIndex = n;
            UnlockInstance(_instances[n]);
        }
        if (Keyboard.Pressed(Keys.F11))
        {
            foreach (NetworkInstance instance3 in _instances)
            {
                instance3.network.core.ForcefulTermination();
            }
            Network.activeNetwork.core.ForcefulTermination();
            _instances.Clear();
            inputProfiles.Clear();
            Level.current = new NetworkDebugger();
        }
        if (Keyboard.shift)
        {
            if (Keyboard.Pressed(Keys.D0))
            {
                fullscreenIndex = 0;
            }
            if (Keyboard.Pressed(Keys.D1))
            {
                fullscreenIndex = 1;
            }
            if (Keyboard.Pressed(Keys.D2))
            {
                fullscreenIndex = 2;
            }
            if (Keyboard.Pressed(Keys.D3))
            {
                fullscreenIndex = 3;
            }
            if (Keyboard.Pressed(Keys.D4))
            {
                fullscreenIndex = 4;
            }
            if (Keyboard.Pressed(Keys.D5))
            {
                fullscreenIndex = 5;
            }
            if (Keyboard.Pressed(Keys.D6))
            {
                fullscreenIndex = 6;
            }
            if (Keyboard.Pressed(Keys.D7))
            {
                fullscreenIndex = 7;
            }
            if (Keyboard.Pressed(Keys.D8))
            {
                fullscreenIndex = 8;
            }
        }
        base.things.RefreshState();
    }

    public override void DoDraw()
    {
        if (_instances.Count == 0)
        {
            return;
        }
        int index = -1;
        foreach (NetworkInstance instance in _instances)
        {
            index++;
            if (!instance.active)
            {
                continue;
            }
            _currentIndex = index;
            LockInstance(instance);
            Viewport v = Graphics.viewport;
            if (fullscreenIndex > 0)
            {
                if (index + 1 == fullscreenIndex)
                {
                    Graphics.viewport = new Viewport(0, 0, v.Width, v.Height);
                }
                else
                {
                    Graphics.viewport = new Viewport(0, 0, 1, 1);
                }
            }
            else
            {
                Graphics.viewport = new Viewport((int)instance.rect.x, (int)instance.rect.y, (int)instance.rect.width, (int)instance.rect.height);
            }
            if (index != 0)
            {
                Level.current.clearScreen = false;
            }
            else
            {
                Level.current.clearScreen = true;
            }
            if (MonoMain.pauseMenu != null)
            {
                if (Level.current.clearScreen)
                {
                    Graphics.Clear(Level.current.backgroundColor);
                }
                Layer.HUD.Begin(transparent: true);
                MonoMain.pauseMenu.Draw();
                foreach (UIComponent item in MonoMain.closeMenuUpdate)
                {
                    item.Draw();
                }
                HUD.Draw();
                Layer.HUD.End(transparent: true);
                Layer.Console.Begin(transparent: true);
                DevConsole.Draw();
                Layer.Console.End(transparent: true);
            }
            else
            {
                Level.DrawCurrentLevel();
            }
            Network.netGraph.Draw();
            Graphics.viewport = v;
            UnlockInstance(instance);
        }
        base.clearScreen = false;
        base.DoDraw();
    }

    public static GhostDebugData GetGhost(GhostObject pGhost)
    {
        GhostDebugData g = null;
        if (!_ghostDebug.TryGetValue(pGhost, out g))
        {
            GhostDebugData ghostDebugData = (_ghostDebug[pGhost] = new GhostDebugData());
            g = ghostDebugData;
        }
        return g;
    }

    public static void ClearGhostDebug()
    {
        _ghostDebug.Clear();
    }

    public static void DrawInstanceGameDebug()
    {
        foreach (GhostObject g in _instances[currentIndex].network.core.ghostManager._ghosts)
        {
            GhostDebugData debug = GetGhost(g);
            if (g.thing == null)
            {
                continue;
            }
            if (g.thing.connection == DuckNetwork.localConnection || g.thing.connection == null)
            {
                Graphics.DrawRect(g.thing.topLeft, g.thing.bottomRight, Color.Red * 0.8f, 1f, filled: false);
            }
            if (g.thing.ghostObject != null && !g.thing.ghostObject.IsInitialized())
            {
                Graphics.DrawRect(g.thing.topLeft + new Vector2(-1f, -1f), g.thing.bottomRight + new Vector2(1f, 1f), Color.Orange * 0.8f, 1f, filled: false);
            }
            foreach (KeyValuePair<DuckPersona, long> pair in debug.dataReceivedFrames)
            {
                if (pair.Value == Graphics.frame)
                {
                    if (pair.Key == Persona.Duck1)
                    {
                        Graphics.DrawRect(g.thing.topLeft + new Vector2(-4f, -4f), g.thing.topLeft + new Vector2(-2f, -2f), pair.Key.colorUsable, 1f, filled: false);
                    }
                    else if (pair.Key == Persona.Duck2)
                    {
                        Graphics.DrawRect(g.thing.topRight + new Vector2(4f, -4f), g.thing.topRight + new Vector2(2f, -2f), pair.Key.colorUsable, 1f, filled: false);
                    }
                    else if (pair.Key == Persona.Duck3)
                    {
                        Graphics.DrawRect(g.thing.bottomLeft + new Vector2(-4f, 2f), g.thing.bottomLeft + new Vector2(-2f, 4f), pair.Key.colorUsable, 1f, filled: false);
                    }
                    else if (pair.Key == Persona.Duck4)
                    {
                        Graphics.DrawRect(g.thing.bottomRight + new Vector2(4f, 2f), g.thing.bottomRight + new Vector2(2f, 4f), pair.Key.colorUsable, 1f, filled: false);
                    }
                }
            }
        }
    }

    public static void StartRecording(string pLevel)
    {
        Recorder.active = new Recorder();
        Recorder.active.level = pLevel;
        StartRecording(0);
    }

    public static void StartRecording(int pIndex)
    {
        if (Recorder.active != null)
        {
            Recorder.active.activeIndex = pIndex;
            Recorder.active.frame = 0;
            DevConsole.RunCommand("level " + Recorder.active.level);
        }
    }

    public static void LogSend(string from, string to)
    {
        if (!_sentPulse.ContainsKey(from))
        {
            _sentPulse[from] = new Dictionary<string, float>();
        }
        if (!_sentPulse[from].ContainsKey(to))
        {
            _sentPulse[from][to] = 0f;
        }
        _sentPulse[from][to] += 1f;
    }

    public static float GetSent(string key, string to)
    {
        if (!_sentPulse.ContainsKey(key))
        {
            return 0f;
        }
        if (!_sentPulse[key].ContainsKey(to))
        {
            return 0f;
        }
        if (_sentPulse[key][to] > 1f)
        {
            _sentPulse[key][to] = 1f;
        }
        _sentPulse[key][to] -= 0.1f;
        if (_sentPulse[key][to] < 0f)
        {
            _sentPulse[key][to] = 0f;
        }
        return _sentPulse[key][to];
    }

    public static void LogReceive(string to, string from)
    {
        if (!_receivedPulse.ContainsKey(to))
        {
            _receivedPulse[to] = new Dictionary<string, float>();
        }
        if (!_receivedPulse[to].ContainsKey(from))
        {
            _receivedPulse[to][from] = 0f;
        }
        _receivedPulse[to][from] += 1f;
    }

    public static float GetReceived(string key, string from)
    {
        if (!_receivedPulse.ContainsKey(key))
        {
            return 0f;
        }
        if (!_receivedPulse[key].ContainsKey(from))
        {
            return 0f;
        }
        if (_receivedPulse[key][from] > 1f)
        {
            _receivedPulse[key][from] = 1f;
        }
        _receivedPulse[key][from] -= 0.1f;
        if (_receivedPulse[key][from] < 0f)
        {
            _receivedPulse[key][from] = 0f;
        }
        return _receivedPulse[key][from];
    }

    public static string GetID(int index)
    {
        return index switch
        {
            0 => "127.0.0.1:1337",
            1 => "127.0.0.1:1338",
            2 => "127.0.0.1:1339",
            3 => "127.0.0.1:1340",
            _ => "",
        };
    }

    private void DrawLogWindow(Vector2 pos, Vector2 size, int page, int index)
    {
        if (_instances.Count <= page)
        {
            return;
        }
        int maxLines = 97;
        if (size.Y < 300f)
        {
            maxLines = maxLines / 2 - 2;
        }
        Queue<DCLine> lines = _instances[page].consoleCore.lines;
        Vector2 logsTL = pos;
        Vector2 logsBR = pos + size;
        Graphics.DrawRect(logsTL, logsBR, Color.Black, 0.8f);
        if (logSwitchIndex == index)
        {
            Graphics.DrawRect(logsTL, logsBR, Color.White * 0.5f, 0.88f, filled: false);
        }
        Graphics.DrawRect(logsTL + new Vector2(0f, -14f), logsTL + new Vector2(100f, 0f), Color.Black, 0.8f);
        Color c = Colors.Duck1;
        switch (page)
        {
            case 1:
                c = Colors.Duck2;
                break;
            case 2:
                c = Colors.Duck3;
                break;
            case 3:
                c = Colors.Duck4;
                break;
        }
        Graphics.DrawString("Player " + (page + 1), logsTL + new Vector2(4f, -12f), c * ((logSwitchIndex == index) ? 1f : 0.6f), 0.81f);
        int lineCount = 0;
        foreach (DCLine l in lines)
        {
            if (logFilters[l.section] || l.line.Contains("@error"))
            {
                lineCount++;
            }
        }
        Graphics.DrawRect(new Vector2(logsTL.X + (size.X - 12f), logsTL.Y), logsBR, Color.Gray * 0.5f, 0.81f);
        float scroll = (float)logsScroll[index] / (float)lineCount;
        float maxBarSize = 300f;
        float barSize = Math.Max(maxBarSize - (float)lineCount, 20f) / maxBarSize;
        float scrollBarHeight = size.Y * barSize;
        Vector2 scrollerTL = new Vector2(logsTL.X + (size.X - 12f), logsTL.Y + scroll * (size.Y - scrollBarHeight));
        Vector2 scrollerBR = new Vector2(logsTL.X + size.X, logsTL.Y + scroll * (size.Y - scrollBarHeight) + scrollBarHeight);
        bool scrollerHover = false;
        if (Mouse.xConsole > scrollerTL.X && Mouse.xConsole < scrollerBR.X && Mouse.yConsole > scrollerTL.Y && Mouse.yConsole < scrollerBR.Y)
        {
            if (Mouse.left == InputState.Pressed)
            {
                scrollerDrag[index] = true;
                mouseClickPos[index] = Mouse.positionConsole;
                mouseClickTop[index] = scrollerTL;
            }
            scrollerHover = true;
        }
        if (scrollerDrag[index])
        {
            Vector2 mouseDif = mouseClickPos[index] - Mouse.positionConsole;
            Vector2 scrollTop = mouseClickTop[index] - mouseDif;
            if (scrollTop.Y < logsTL.Y)
            {
                scrollTop.Y = logsTL.Y;
            }
            if (scrollTop.Y > logsBR.Y - scrollBarHeight)
            {
                scrollTop.Y = logsBR.Y - scrollBarHeight;
            }
            logsScroll[index] = (int)Math.Round((scrollTop.Y - logsTL.Y) / (size.Y - scrollBarHeight) * (float)lineCount);
        }
        if (Mouse.left == InputState.Released)
        {
            scrollerDrag[index] = false;
        }
        Graphics.DrawRect(scrollerTL, scrollerBR, Color.White * ((scrollerHover || scrollerDrag[index]) ? 0.8f : 0.5f), 0.82f);
        if (Mouse.xConsole > logsTL.X && Mouse.xConsole < logsBR.X && Mouse.yConsole > logsTL.Y && Mouse.yConsole < logsBR.Y)
        {
            if (Mouse.scroll > 0f)
            {
                logsScroll[index] += 5;
            }
            else if (Mouse.scroll < 0f)
            {
                logsScroll[index] -= 5;
            }
        }
        if (logsScroll[index] < 0)
        {
            logsScroll[index] = 0;
        }
        if (logsScroll[index] > lineCount - 1)
        {
            logsScroll[index] = lineCount - 1;
        }
        if (lineCount < maxLines)
        {
            logsScroll[index] = 0;
        }
        Vector2 linePos = logsTL + new Vector2(8f, 8f);
        int lineDrawIndex = 0;
        for (int i = 0; i < maxLines; i++)
        {
            int listIndex = i + logsScroll[index];
            if (listIndex >= lineCount)
            {
                continue;
            }
            int frames = 0;
            for (; listIndex + lineDrawIndex < lines.Count && !logFilters[lines.ElementAt(listIndex + lineDrawIndex).section] && !lines.ElementAt(listIndex + lineDrawIndex).line.Contains("@error"); lineDrawIndex++)
            {
                frames += lines.ElementAt(listIndex + lineDrawIndex).frames;
            }
            if (listIndex + lineDrawIndex < lines.Count)
            {
                DCLine line = lines.ElementAt(listIndex + lineDrawIndex);
                DevConsole.DrawLine(linePos, line, logTimes, logSections);
                Color col = DCLine.ColorForSection(line.section);
                col.r = (byte)((float)(int)col.r * 0.1f);
                col.g = (byte)((float)(int)col.g * 0.1f);
                col.b = (byte)((float)(int)col.b * 0.1f);
                if (line.line.Contains("@error"))
                {
                    col = Color.Red;
                    col.r = (byte)((float)(int)col.r * 0.3f);
                    col.g = (byte)((float)(int)col.g * 0.3f);
                    col.b = (byte)((float)(int)col.b * 0.3f);
                }
                Graphics.DrawRect(linePos + new Vector2(-4f, -1f), new Vector2(logsBR.X - 14f, linePos.Y + 9f), col, 0.85f);
                if (line.frames + frames > 0)
                {
                    linePos.Y += 1f;
                    Graphics.DrawLine(linePos + new Vector2(-4f, 10f), new Vector2(logsBR.X - 14f, linePos.Y + 10f), Color.White * 0.24f, 1f, 0.9f);
                    linePos.Y += 2f;
                    if (line.frames + frames > 30)
                    {
                        Graphics.DrawString("~" + (line.frames + frames) + " frames~", linePos + new Vector2(80f, 10f), Color.White * 0.2f, 0.9f);
                        linePos.Y += 10f;
                        maxLines--;
                    }
                }
            }
            linePos.Y += 10f;
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (_instances.Count == 0 || fullscreenIndex != 0)
        {
            return;
        }
        if (layer == Layer.Console)
        {
            Graphics.fade = 1f;
            if (showLogs)
            {
                int numShowing = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (showLogPage[i] >= 0)
                    {
                        numShowing++;
                    }
                }
                Vector2[] positions = new Vector2[4];
                Vector2 tl = new Vector2(20f, 80f);
                if (showFilters)
                {
                    Graphics.DrawRect(tl + new Vector2(0f, -42f), tl + new Vector2(890f, -30f), Color.Black * 0.9f, 0.8f);
                    for (int j = 0; j < 9; j++)
                    {
                        if (j == 8)
                        {
                            Graphics.DrawString("ALL (9)", tl + new Vector2(j * 110, -40f), Color.White, 0.82f);
                            continue;
                        }
                        DCSection s = (DCSection)Enum.GetValues(typeof(DCSection)).GetValue(j);
                        string filt = DCLine.StringForSection(s, colored: true, small: false, formatting: false);
                        if (s == DCSection.General)
                        {
                            filt = "GENERAL";
                        }
                        Graphics.DrawString(filt + " (" + (j + 1) + ")", tl + new Vector2(j * 110, -40f), Color.White * (logFilters[s] ? 1f : 0.5f), 0.82f);
                    }
                }
                positions[0] = tl;
                Vector2 logSize = new Vector2(Layer.Console.width - 40f, Layer.Console.height - 100f);
                if (numShowing > 1)
                {
                    logSize = new Vector2(logSize.X / 2f - 4f, Layer.Console.height - 100f);
                    positions[1] = tl + new Vector2(logSize.X + 4f, 0f);
                }
                if (numShowing > 2)
                {
                    logSize = new Vector2(logSize.X, logSize.Y / 2f - 16f);
                    positions[2] = tl + new Vector2(0f, logSize.Y + 16f);
                    positions[3] = tl + new Vector2(logSize.X + 4f, logSize.Y + 16f);
                }
                for (int k = 0; k < 4; k++)
                {
                    if (showLogPage[k] >= 0)
                    {
                        DrawLogWindow(positions[k], logSize, showLogPage[k], k);
                    }
                }
                return;
            }
            foreach (NetworkInstance i2 in _instances.ToList())
            {
                LockInstance(i2);
                i2.debugInterface.Draw();
                if (i2.debugInterface.visible)
                {
                    Network.netGraph.DrawChart(i2.consoleSize.tl + new Vector2(10f, 300f));
                }
                UnlockInstance(i2);
            }
        }
        base.PostDrawLayer(layer);
    }

    public static void LockInstance(NetworkInstance instance)
    {
        oldNetwork = Network.activeNetwork;
        Network.activeNetwork = instance.network;
        oldDuckNetworkCore = DuckNetwork.core;
        DuckNetwork.core = instance.duckNetworkCore;
        oldVirtualCore = VirtualTransition.core;
        VirtualTransition.core = instance.virtualCore;
        oldLevelCore = Level.core;
        Level.core = instance.levelCore;
        oldProfileCore = Profiles.core;
        Profiles.core = instance.profileCore;
        oldTeamCore = Teams.core;
        Teams.core = instance.teamsCore;
        oldLayerCore = Layer.core;
        Layer.core = instance.layerCore;
        oldInputCore = InputProfile.core;
        InputProfile.core = instance.inputProfile;
        oDevCore = DevConsole.core;
        DevConsole.core = instance.consoleCore;
        oldCrowdCore = Crowd.core;
        Crowd.core = instance.crowdCore;
        oldGameModeCore = GameMode.core;
        GameMode.core = instance.gameModeCore;
        oldConnectionUICore = ConnectionStatusUI.core;
        ConnectionStatusUI.core = instance.connectionUICore;
        oldMonoCore = MonoMain.core;
        MonoMain.core = instance.monoCore;
        oldHUDCore = HUD.core;
        HUD.core = instance.hudCore;
        oldMatchmakingCore = UIMatchmakingBox.core;
        UIMatchmakingBox.core = instance.matchmakingCore;
        oldAUCore = AutoUpdatables.core;
        AutoUpdatables.core = instance.auCore;
        oldRando = Rando.generator;
        Rando.generator = instance.rando;
        foreach (NetworkInstance.Core extraCore in instance.extraCores)
        {
            extraCore.Lock();
        }
    }

    public static void UnlockInstance(NetworkInstance instance)
    {
        Network.activeNetwork = oldNetwork;
        DuckNetwork.core = oldDuckNetworkCore;
        Teams.core = oldTeamCore;
        Layer.core = oldLayerCore;
        VirtualTransition.core = oldVirtualCore;
        Level.core = oldLevelCore;
        Profiles.core = oldProfileCore;
        InputProfile.core = oldInputCore;
        DevConsole.core = oDevCore;
        Crowd.core = oldCrowdCore;
        GameMode.core = oldGameModeCore;
        ConnectionStatusUI.core = oldConnectionUICore;
        MonoMain.core = oldMonoCore;
        HUD.core = oldHUDCore;
        UIMatchmakingBox.core = oldMatchmakingCore;
        AutoUpdatables.core = oldAUCore;
        Rando.generator = oldRando;
        foreach (NetworkInstance.Core extraCore in instance.extraCores)
        {
            extraCore.Unlock();
        }
    }

    public static NetworkInstance Reboot(NetworkInstance pInstance)
    {
        UnlockInstance(pInstance);
        int idx = _instances.IndexOf(pInstance);
        instance.CreateInstance(idx, isHost: false);
        NetworkInstance result = _instances[idx];
        LockInstance(pInstance);
        return result;
    }

    public static void RegisterCore<T>(string pCoreMemberName, Action pRunOnFirstLock)
    {
        FieldInfo m = typeof(T).GetField(pCoreMemberName, BindingFlags.Static | BindingFlags.Public);
        if (m != null)
        {
            _registeredCores.Add(new NetworkInstance.Core
            {
                member = m,
                originalInstance = m.GetValue(null),
                firstLockAction = pRunOnFirstLock
            });
        }
    }
}
