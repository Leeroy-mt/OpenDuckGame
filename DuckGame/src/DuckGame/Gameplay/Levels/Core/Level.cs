using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DuckGame;

public class Level
{
    public List<NMLevel> levelMessages = new List<NMLevel>();

    public bool isCustomLevel;

    public static bool flipH = false;

    public static bool symmetry = false;

    public static bool leftSymmetry = true;

    public static bool loadingOppositeSymmetry = false;

    public string _level = "";

    private static LevelCore _core = new LevelCore();

    public static bool skipInitialize = false;

    public bool isPreview;

    private static Queue<List<object>> _collisionLists = new Queue<List<object>>();

    private bool _simulatePhysics = true;

    private Color _backgroundColor = Color.Black;

    protected QuadTreeObjectList _things = new QuadTreeObjectList();

    protected string _id = "";

    protected Camera _camera = new Camera();

    protected NetLevelStatus _networkStatus;

    public float transitionSpeedMultiplier = 1f;

    public float lowestPoint = 1000f;

    private bool _lowestPointInitialized;

    public float highestPoint = -1000f;

    protected bool _initialized;

    private bool _levelStart;

    public bool _startCalled;

    protected bool _centeredView;

    private bool _waitingOnNewData;

    public byte networkIndex;

    public int seed;

    private bool _notifiedReady;

    private bool _initializeLater;

    public bool bareInitialize;

    protected Vec2 _topLeft = new Vec2(99999f, 99999f);

    protected Vec2 _bottomRight = new Vec2(-99999f, -99999f);

    protected bool _readyForTransition = true;

    public bool _waitingOnTransition;

    public bool cold;

    public bool suppressLevelMessage;

    private static int collectionCount;

    protected int _updateWaitFrames;

    private bool _sentLevelChange;

    private bool _calledAllClientsReady;

    public bool transferCompleteCalled = true;

    private bool _aiInitialized;

    private bool _refreshState;

    private bool initPaths;

    private Dictionary<NetworkConnection, bool> checksumReplies = new Dictionary<NetworkConnection, bool>();

    public static bool doingOnLoadedMessage = false;

    public float flashDissipationSpeed = 0.15f;

    public bool skipCurrentLevelReset;

    private int wait = 60;

    private bool _clearScreen = true;

    public bool drawsOverPauseMenu;

    private Sprite _burnGlow;

    private Sprite _burnGlowWide;

    private Sprite _burnGlowWideLeft;

    private Sprite _burnGlowWideRight;

    public string level => _level;

    public static LevelCore core
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

    public bool simulatePhysics
    {
        get
        {
            return _simulatePhysics;
        }
        set
        {
            _simulatePhysics = value;
        }
    }

    public static bool sendCustomLevels
    {
        get
        {
            return _core.sendCustomLevels;
        }
        set
        {
            _core.sendCustomLevels = value;
        }
    }

    public static Level current
    {
        get
        {
            if (_core.nextLevel == null)
            {
                return _core.currentLevel;
            }
            return _core.nextLevel;
        }
        set
        {
            _core.nextLevel = value;
        }
    }

    public static Level activeLevel
    {
        get
        {
            return _core.currentLevel;
        }
        set
        {
            _core.currentLevel = value;
        }
    }

    public Color backgroundColor
    {
        get
        {
            return _backgroundColor;
        }
        set
        {
            _backgroundColor = value;
        }
    }

    public QuadTreeObjectList things => _things;

    public string id => _id;

    public Camera camera
    {
        get
        {
            return _camera;
        }
        set
        {
            _camera = value;
        }
    }

    public NetLevelStatus networkStatus => _networkStatus;

    public bool initialized => _initialized;

    public bool initializeFunctionHasBeenRun
    {
        get
        {
            if (_initialized)
            {
                return !_initializeLater;
            }
            return false;
        }
    }

    public bool waitingOnNewData
    {
        get
        {
            return _waitingOnNewData;
        }
        set
        {
            _waitingOnNewData = value;
        }
    }

    public Vec2 topLeft => _topLeft;

    public Vec2 bottomRight => _bottomRight;

    public virtual string networkIdentifier => "";

    public bool calledAllClientsReady => _calledAllClientsReady;

    public bool levelIsUpdating
    {
        get
        {
            if (_core.nextLevel == null && (!Network.isActive || _startCalled) && !_waitingOnTransition)
            {
                return transferCompleteCalled;
            }
            return false;
        }
    }

    public bool clearScreen
    {
        get
        {
            return _clearScreen;
        }
        set
        {
            _clearScreen = value;
        }
    }

    public static void InitializeCollisionLists()
    {
        //MonoMain.loadMessage = "Loading Collision Lists";
        for (int i = 0; i < 10; i++)
        {
            _collisionLists.Enqueue(new List<object>());
        }
    }

    public static List<object> GetNextCollisionList()
    {
        return new List<object>();
    }

    public static bool PassedChanceGroup(int group, float val)
    {
        if (group == -1)
        {
            return Rando.Float(1f) < val;
        }
        return _core._chanceGroups[group] < val;
    }

    public static bool PassedChanceGroup2(int group, float val)
    {
        if (group == -1)
        {
            return Rando.Float(1f) < val;
        }
        return _core._chanceGroups2[group] < val;
    }

    public static float GetChanceGroup2(int group)
    {
        if (group == -1)
        {
            return Rando.Float(1f);
        }
        return _core._chanceGroups2[group];
    }

    public static void Add(Thing thing)
    {
        if (_core.currentLevel != null)
        {
            _core.currentLevel.AddThing(thing);
        }
    }

    public static void Remove(Thing thing)
    {
        if (_core.currentLevel != null)
        {
            _core.currentLevel.RemoveThing(thing);
        }
    }

    public static void ClearThings()
    {
        if (_core.currentLevel != null)
        {
            _core.currentLevel.Clear();
        }
    }

    public static void UpdateCurrentLevel()
    {
        if (_core.currentLevel != null)
        {
            _core.currentLevel.DoUpdate();
        }
    }

    public static void DrawCurrentLevel()
    {
        if (_core.currentLevel != null)
        {
            _core.currentLevel.DoDraw();
        }
    }

    public static T First<T>()
    {
        IEnumerable<Thing> things = current.things[typeof(T)];
        if (things.Count() > 0)
        {
            return (T)(object)things.First();
        }
        return default(T);
    }

    public T FirstOfType<T>()
    {
        IEnumerable<Thing> t = things[typeof(T)];
        if (t.Count() > 0)
        {
            return (T)(object)t.First();
        }
        return default(T);
    }

    public virtual void DoInitialize()
    {
        if (waitingOnNewData)
        {
            _initializeLater = true;
            _initialized = true;
            return;
        }
        if (!_initialized)
        {
            GhostManager.context.TransferPendingGhosts();
            Random generator = Rando.generator;
            Rando.generator = new Random(seed + 2500);
            InitChanceGroups();
            Rando.generator = generator;
            Initialize();
            if (!Network.isActive || Network.InLobby())
            {
                DoStart();
            }
            _things.RefreshState();
            CalculateBounds();
            _initialized = true;
            if (_centeredView)
            {
                float ideal = 0.5625f * camera.width;
                float real = Graphics.aspect * camera.width;
                camera.centerY -= (real - ideal) / 2f;
            }
            if (!VirtualTransition.active)
            {
                StaticRenderer.Update();
            }
            if (Network.isActive)
            {
                ClientReady(DuckNetwork.localConnection);
            }
            return;
        }
        foreach (Thing thing in _things)
        {
            thing.AddToLayer();
        }
    }

    public virtual void LevelLoaded()
    {
    }

    public virtual void Initialize()
    {
        _levelStart = true;
        Vote.ClearVotes();
    }

    private void DoStart()
    {
        if (!_startCalled)
        {
            Start();
            _startCalled = true;
        }
    }

    public void SkipStart()
    {
        _startCalled = true;
    }

    public virtual void Start()
    {
    }

    public virtual void Terminate()
    {
        Clear();
    }

    public virtual void AddThing(Thing t)
    {
        if (Thread.CurrentThread == Content.previewThread && this != Content.previewLevel)
        {
            Content.previewLevel.AddThing(t);
            return;
        }
        if (t is ThingContainer)
        {
            ThingContainer container = t as ThingContainer;
            if (container.bozocheck)
            {
                foreach (Thing thing in container.things)
                {
                    if (!Thing.CheckForBozoData(thing))
                    {
                        AddThing(thing);
                    }
                }
                return;
            }
            {
                foreach (Thing thing2 in container.things)
                {
                    AddThing(thing2);
                }
                return;
            }
        }
        if (t.level != this)
        {
            _things.Add(t);
            if (!skipInitialize)
            {
                t.Added(this, !bareInitialize, reinit: false);
            }
        }
        if (Network.isActive && t.connection == null)
        {
            t.connection = DuckNetwork.localConnection;
        }
    }

    public virtual void RemoveThing(Thing t)
    {
        if (t != null)
        {
            _ = t is HatSelector;
            t.DoTerminate();
            t.Removed();
            _things.Remove(t);
            if (t.ghostObject != null && t.isServerForObject)
            {
                GhostManager.context.RemoveLater(t.ghostObject);
            }
        }
    }

    public void Clear()
    {
        foreach (Thing thing in _things)
        {
            thing.Removed();
        }
        Layer.ClearLayers();
        _things.Clear();
    }

    public static void InitChanceGroups()
    {
        for (int i = 0; i < _core._chanceGroups.Count; i++)
        {
            _core._chanceGroups[i] = Rando.Float(1f);
        }
        for (int j = 0; j < _core._chanceGroups2.Count; j++)
        {
            _core._chanceGroups2[j] = Rando.Float(1f);
        }
    }

    public virtual string LevelNameData()
    {
        return GetType().Name;
    }

    public static void UpdateLevelChange()
    {
        if (_core.nextLevel != null)
        {
            RumbleManager.ClearRumbles(null);
            if (_core.currentLevel is IHaveAVirtualTransition && _core.nextLevel is IHaveAVirtualTransition && !(_core.nextLevel is TeamSelect2))
            {
                VirtualTransition.GoVirtual();
            }
            if (Network.isActive && activeLevel != null && !_core.nextLevel._sentLevelChange)
            {
                DevConsole.Log(DCSection.GhostMan, "|DGYELLOW|Performing level swap (" + DuckNetwork.levelIndex + ")");
                if (_core.currentLevel is TeamSelect2 && !(_core.nextLevel is TeamSelect2))
                {
                    DuckNetwork.ClosePauseMenu();
                }
                if (!(_core.currentLevel is TeamSelect2) && _core.nextLevel is TeamSelect2)
                {
                    DuckNetwork.ClosePauseMenu();
                }
                if (Network.isServer && !(_core.nextLevel is IConnectionScreen))
                {
                    if (DuckNetwork.levelIndex > 250)
                    {
                        DuckNetwork.levelIndex = 1;
                    }
                    if (_core.nextLevel is TeamSelect2)
                    {
                        Network.ContextSwitch(0);
                    }
                    else
                    {
                        Network.ContextSwitch((byte)(DuckNetwork.levelIndex + 1));
                    }
                    DuckNetwork.compressedLevelData = null;
                    DevConsole.Log(DCSection.GhostMan, "|DGYELLOW|Incrementing level index (" + (DuckNetwork.levelIndex - 1) + "->" + DuckNetwork.levelIndex + ")");
                    if (!_core.nextLevel.suppressLevelMessage)
                    {
                        Send.Message(new NMLevel(_core.nextLevel));
                    }
                    _core.nextLevel.networkIndex = DuckNetwork.levelIndex;
                }
                else if (_core.nextLevel is IConnectionScreen)
                {
                    Network.ContextSwitch(byte.MaxValue);
                }
                _core.nextLevel._sentLevelChange = true;
            }
            if (!VirtualTransition.active)
            {
                if (NetworkDebugger.enabled && NetworkDebugger.Recorder.active != null)
                {
                    Rando.generator = new Random(NetworkDebugger.Recorder.active.seed);
                }
                DamageManager.ClearHits();
                Layer.ResetLayers();
                HUD.ClearCorners();
                if (_core.currentLevel != null)
                {
                    _core.currentLevel.Terminate();
                }
                string curLevel = ((_core.currentLevel != null) ? _core.currentLevel.LevelNameData() : "null");
                string nexLevel = ((_core.nextLevel != null) ? _core.nextLevel.LevelNameData() : "null");
                if (_core.nextLevel is XMLLevel && (_core.nextLevel as XMLLevel).level == "RANDOM")
                {
                    DevConsole.Log(DCSection.General, "Level Switch (" + curLevel + " -> Random Level(" + (_core.nextLevel as XMLLevel).seed + "))");
                }
                else
                {
                    DevConsole.Log(DCSection.General, "Level Switch (" + curLevel + " -> " + nexLevel + ")");
                }
                _core.currentLevel = _core.nextLevel;
                _core.nextLevel = null;
                Layer.lighting = false;
                VirtualTransition.core._transitionLevel = null;
                AutoUpdatables.ClearSounds();
                SequenceItem.sequenceItems.Clear();
                Graphics.GarbageDisposal(pLevelTransition: true);
                GC.Collect(1, GCCollectionMode.Optimized);
                collectionCount++;
                if (!(_core.currentLevel is GameLevel))
                {
                    if (MonoMain.timeInMatches > 0)
                    {
                        Global.data.timeInMatches.valueInt += MonoMain.timeInMatches / 60;
                        MonoMain.timeInMatches = 0;
                    }
                    if (MonoMain.timeInArcade > 0)
                    {
                        Global.data.timeInArcade.valueInt += MonoMain.timeInArcade / 60;
                        MonoMain.timeInArcade = 0;
                    }
                    if (MonoMain.timeInEditor > 0)
                    {
                        Global.data.timeInEditor.valueInt += MonoMain.timeInEditor / 60;
                        MonoMain.timeInEditor = 0;
                    }
                    if (!(_core.currentLevel is HighlightLevel))
                    {
                        Graphics.fadeAdd = 0f;
                    }
                    Steam.StoreStats();
                }
                foreach (Profile item in Profiles.active)
                {
                    item.duck = null;
                }
                SFX.StopAllSounds();
                _core.currentLevel.DoInitialize();
                if (_core.currentLevel is XMLLevel && (_core.currentLevel as XMLLevel).data != null)
                {
                    string path = (_core.currentLevel as XMLLevel).data.GetPath();
                    if (path != null)
                    {
                        DevConsole.Log(DCSection.General, "Level Initialized(" + path + ")");
                    }
                }
                if (MonoMain.pauseMenu != null && MonoMain.pauseMenu.inWorld)
                {
                    _core.currentLevel.AddThing(MonoMain.pauseMenu);
                }
                if (Network.isActive && DuckNetwork.duckNetUIGroup != null && DuckNetwork.duckNetUIGroup.open)
                {
                    _core.currentLevel.AddThing(DuckNetwork.duckNetUIGroup);
                }
                current._networkStatus = NetLevelStatus.WaitingForDataTransfer;
                if (!(_core.currentLevel is IOnlyTransitionIn) && _core.currentLevel is IHaveAVirtualTransition && !(_core.currentLevel is TeamSelect2) && VirtualTransition.isVirtual)
                {
                    if (current._readyForTransition)
                    {
                        VirtualTransition.GoUnVirtual();
                        Graphics.fade = 1f;
                    }
                    else
                    {
                        current._waitingOnTransition = true;
                        if (Network.isActive)
                        {
                            ConnectionStatusUI.Show();
                        }
                    }
                }
            }
        }
        if (current._waitingOnTransition && current._readyForTransition)
        {
            current._waitingOnTransition = false;
            VirtualTransition.GoUnVirtual();
            if (Network.isActive)
            {
                ConnectionStatusUI.Hide();
            }
        }
    }

    public virtual void OnMessage(NetMessage message)
    {
    }

    public virtual void OnNetworkConnecting(Profile p)
    {
    }

    public virtual void OnNetworkConnected(Profile p)
    {
    }

    public virtual void OnNetworkDisconnected(Profile p)
    {
    }

    public virtual void OnSessionEnded(DuckNetErrorInfo error)
    {
        if (error != null)
        {
            current = new ConnectionError(error.message);
        }
        else
        {
            current = new ConnectionError("|RED|Disconnected from game.");
        }
        DuckNetwork.core.stopEnteringText = true;
    }

    public virtual void OnDisconnect(NetworkConnection n)
    {
    }

    public virtual void ClientReady(NetworkConnection c)
    {
        if (!initializeFunctionHasBeenRun)
        {
            return;
        }
        bool ready = true;
        foreach (Profile pro in DuckNetwork.profiles)
        {
            if (pro.connection != null && pro.connection.levelIndex != DuckNetwork.levelIndex)
            {
                ready = false;
                break;
            }
        }
        if (ready)
        {
            DevConsole.Log(DCSection.DuckNet, "|DGGREEN|All Clients ready! The level can begin...");
            Send.Message(new NMAllClientsReady());
        }
    }

    public virtual void DoAllClientsReady()
    {
        if (!_calledAllClientsReady)
        {
            _calledAllClientsReady = true;
            OnAllClientsReady();
        }
    }

    protected virtual void OnAllClientsReady()
    {
        _networkStatus = NetLevelStatus.Ready;
        current._readyForTransition = true;
        DoStart();
    }

    public void TransferComplete(NetworkConnection c)
    {
        transferCompleteCalled = true;
        _networkStatus = NetLevelStatus.WaitingForTransition;
        OnTransferComplete(c);
    }

    protected virtual void OnTransferComplete(NetworkConnection c)
    {
    }

    public virtual void SendLevelData(NetworkConnection c)
    {
    }

    public void IgnoreLowestPoint()
    {
        _lowestPointInitialized = true;
        lowestPoint = 999999f;
        _topLeft = new Vec2(-99999f, -99999f);
        _bottomRight = new Vec2(99999f, 99999f);
    }

    public void CalculateBounds()
    {
        _lowestPointInitialized = true;
        CameraBounds bounds = FirstOfType<CameraBounds>();
        if (bounds != null)
        {
            _topLeft = new Vec2(bounds.x - (float)((int)bounds.wide / 2), bounds.y - (float)((int)bounds.high / 2));
            _bottomRight = new Vec2(bounds.x + (float)((int)bounds.wide / 2), bounds.y + (float)((int)bounds.high / 2));
            lowestPoint = _bottomRight.y;
            highestPoint = _topLeft.y;
            return;
        }
        _topLeft = new Vec2(99999f, 99999f);
        _bottomRight = new Vec2(-99999f, -99999f);
        foreach (Block b in _things[typeof(Block)])
        {
            if (!(b is RockWall) && !(b.y > 7500f))
            {
                if (b.right > _bottomRight.x)
                {
                    _bottomRight.x = b.right;
                }
                if (b.left < _topLeft.x)
                {
                    _topLeft.x = b.left;
                }
                if (b.bottom > _bottomRight.y)
                {
                    _bottomRight.y = b.bottom;
                }
                if (b.top < _topLeft.y)
                {
                    _topLeft.y = b.top;
                }
            }
        }
        foreach (AutoPlatform b2 in _things[typeof(AutoPlatform)])
        {
            if (!(b2.y > 7500f))
            {
                if (b2.right > _bottomRight.x)
                {
                    _bottomRight.x = b2.right;
                }
                if (b2.left < _topLeft.x)
                {
                    _topLeft.x = b2.left;
                }
                if (b2.bottom > _bottomRight.y)
                {
                    _bottomRight.y = b2.bottom;
                }
                if (b2.top < _topLeft.y)
                {
                    _topLeft.y = b2.top;
                }
            }
        }
        lowestPoint = _bottomRight.y;
        highestPoint = topLeft.y;
    }

    public bool HasChecksumReply(NetworkConnection pConnection)
    {
        bool got = false;
        checksumReplies.TryGetValue(pConnection, out got);
        return got;
    }

    public void ChecksumReplied(NetworkConnection pConnection)
    {
        checksumReplies[pConnection] = true;
    }

    public virtual void DoUpdate()
    {
        if (_updateWaitFrames > 0)
        {
            if (!_refreshState)
            {
                _things.RefreshState();
                VirtualTransition.Update();
                _refreshState = true;
            }
            _updateWaitFrames--;
            if (!_lowestPointInitialized)
            {
                CalculateBounds();
            }
            return;
        }
        Level cur = _core.currentLevel;
        _core.currentLevel = this;
        if (Graphics.flashAdd > 0f)
        {
            Graphics.flashAdd -= flashDissipationSpeed;
        }
        else
        {
            Graphics.flashAdd = 0f;
        }
        if (_levelStart)
        {
            Graphics.fade = Lerp.Float(Graphics.fade, 1f, 0.05f);
            if (Graphics.fade == 1f)
            {
                _levelStart = false;
            }
        }
        if (_core.nextLevel == null && initializeFunctionHasBeenRun && levelMessages.Count > 0)
        {
            for (int i = 0; i < levelMessages.Count; i++)
            {
                doingOnLoadedMessage = true;
                if (levelMessages[i].OnLevelLoaded())
                {
                    levelMessages.RemoveAt(i);
                    i--;
                }
                doingOnLoadedMessage = false;
            }
        }
        if (levelIsUpdating)
        {
            if (_camera != null)
            {
                _camera.DoUpdate();
            }
            Update();
            Layer.UpdateLayers();
            UpdateThings();
            PostUpdate();
            _things.RefreshState();
            Vote.Update();
            HUD.Update();
        }
        else
        {
            _things.RefreshState();
        }
        if (!_notifiedReady && _initialized && !waitingOnNewData)
        {
            DevConsole.Log(DCSection.GhostMan, "Initializing level (" + DuckNetwork.levelIndex + ")");
            if (_initializeLater)
            {
                _initialized = false;
                _initializeLater = false;
                DoInitialize();
            }
            _notifiedReady = true;
        }
        VirtualTransition.Update();
        ConnectionStatusUI.Update();
        if (!_aiInitialized)
        {
            AI.InitializeLevelPaths();
            _aiInitialized = true;
        }
        if (!skipCurrentLevelReset)
        {
            _core.currentLevel = cur;
        }
    }

    public virtual void PostUpdate()
    {
    }

    public virtual void NetworkDebuggerPrepare()
    {
    }

    public virtual void UpdateThings()
    {
        Network.PostDraw();
        IEnumerable<Thing> complexUpdatables = things[typeof(IComplexUpdate)];
        if (Network.isActive)
        {
            foreach (Thing t in complexUpdatables)
            {
                if (t.shouldRunUpdateLocally)
                {
                    (t as IComplexUpdate).OnPreUpdate();
                }
            }
            foreach (Thing t2 in _things.updateList)
            {
                if (t2.active)
                {
                    if (t2.shouldRunUpdateLocally)
                    {
                        t2.DoUpdate();
                    }
                }
                else
                {
                    t2.InactiveUpdate();
                }
                if (_core.nextLevel != null)
                {
                    break;
                }
            }
            {
                foreach (Thing t3 in complexUpdatables)
                {
                    if (t3.shouldRunUpdateLocally)
                    {
                        (t3 as IComplexUpdate).OnPostUpdate();
                    }
                }
                return;
            }
        }
        foreach (Thing item in complexUpdatables)
        {
            (item as IComplexUpdate).OnPreUpdate();
        }
        foreach (Thing t4 in _things.updateList)
        {
            if (t4.active && t4.level != null)
            {
                t4.DoUpdate();
            }
            if (_core.nextLevel != null)
            {
                break;
            }
        }
        foreach (Thing item2 in complexUpdatables)
        {
            (item2 as IComplexUpdate).OnPostUpdate();
        }
    }

    public virtual void Update()
    {
    }

    public virtual void StartDrawing()
    {
    }

    public virtual void DoDraw()
    {
        StartDrawing();
        foreach (IDrawToDifferentLayers item in things[typeof(IDrawToDifferentLayers)])
        {
            item.OnDrawLayer(Layer.PreDrawLayer);
        }
        Layer.DrawTargetLayers();
        Vec3 col = backgroundColor.ToVector3();
        col *= Graphics.fade;
        col.x += Graphics.flashAddRenderValue;
        col.y += Graphics.flashAddRenderValue;
        col.z += Graphics.flashAddRenderValue;
        col = new Vec3(col.x + Graphics.fadeAddRenderValue, col.y + Graphics.fadeAddRenderValue, col.z + Graphics.fadeAddRenderValue);
        Color c = new Color(col);
        c.a = backgroundColor.a;
        if (clearScreen)
        {
            if (!Options.Data.fillBackground)
            {
                Graphics.Clear(c);
            }
            else
            {
                Graphics.Clear(Color.Black);
                Graphics.SetFullViewport();
                Material material = Graphics.material;
                Graphics.material = null;
                Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
                Graphics.DrawRect(new Vec2(0f, 0f), new Vec2(Resolution.current.x, Resolution.current.y), c, -1f);
                Graphics.screen.End();
                Graphics.material = material;
                Graphics.RestoreOldViewport();
            }
        }
        if (Recorder.currentRecording != null)
        {
            Recorder.currentRecording.LogBackgroundColor(backgroundColor);
        }
        BeforeDraw();
        Graphics.screen.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera.getMatrix());
        Draw();
        things.Draw();
        Graphics.screen.End();
        if (DevConsole.splitScreen && this is GameLevel)
        {
            SplitScreen.Draw();
        }
        else
        {
            Layer.DrawLayers();
        }
        if (DevConsole.rhythmMode && this is GameLevel)
        {
            Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Layer.HUD.camera.getMatrix());
            RhythmMode.Draw();
            Graphics.screen.End();
        }
        AfterDrawLayers();
    }

    public virtual void InitializeDraw(Layer l)
    {
        if (l == Layer.HUD && _centeredView)
        {
            float aspect = 0.5625f;
            float dif = Resolution.size.x * Graphics.aspect - Resolution.size.x * aspect;
            if (dif > 0f)
            {
                Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
                Graphics.DrawRect(Vec2.Zero, new Vec2(Resolution.size.x, dif / 2f), Color.Black, 0.9f);
                Graphics.DrawRect(new Vec2(0f, Resolution.size.y - dif / 2f), new Vec2(Resolution.size.x, Resolution.size.y), Color.Black, 0.9f);
                Graphics.screen.End();
            }
        }
    }

    public virtual void BeforeDraw()
    {
    }

    public virtual void AfterDrawLayers()
    {
    }

    public virtual void Draw()
    {
    }

    public virtual void PreDrawLayer(Layer layer)
    {
    }

    public virtual void PostDrawLayer(Layer layer)
    {
        foreach (IEngineUpdatable engineUpdatable in MonoMain.core.engineUpdatables)
        {
            engineUpdatable.OnDrawLayer(layer);
        }
        foreach (IDrawToDifferentLayers item in things[typeof(IDrawToDifferentLayers)])
        {
            item.OnDrawLayer(layer);
        }
        if (layer == Layer.Console)
        {
            DevConsole.Draw();
            if (Network.isActive)
            {
                DuckNetwork.Draw();
            }
        }
        else if (layer == Layer.Foreground)
        {
            if (layer.fade > 0f)
            {
                HUD.DrawForeground();
            }
        }
        else if (layer == Layer.HUD)
        {
            if (layer.fade > 0f)
            {
                Vote.Draw();
                HUD.Draw();
                ConnectionStatusUI.Draw();
            }
        }
        else
        {
            if (layer == Layer.Lighting)
            {
                return;
            }
            if (layer == Layer.Glow && Options.Data.fireGlow)
            {
                foreach (MaterialThing t in things[typeof(MaterialThing)])
                {
                    if (t is Holdable && t.heat > 0.3f && t.physicsMaterial == PhysicsMaterial.Metal)
                    {
                        if (_burnGlow == null)
                        {
                            _burnGlow = new Sprite("redHotGlow");
                            _burnGlow.CenterOrigin();
                        }
                        _burnGlow.alpha = Math.Min(t.heat, 1f) / 1f - 0.2f;
                        Vec2 scale = new Vec2((t.width + 22f) / (float)_burnGlow.width, (t.height + 22f) / (float)_burnGlow.height);
                        _burnGlow.scale = scale;
                        Vec2 center = t.rectangle.Center;
                        Graphics.Draw(_burnGlow, center.x, center.y);
                        Graphics.Draw(_burnGlow, center.x, center.y);
                    }
                    else if (t is FluidPuddle)
                    {
                        FluidPuddle fp = t as FluidPuddle;
                        if ((fp.onFire || fp.data.heat > 0.5f) && fp.alpha > 0.5f)
                        {
                            float num = fp.right - fp.left;
                            float gap = 16f;
                            Math.Sin(fp.fluidWave);
                            if (_burnGlowWide == null)
                            {
                                _burnGlowWide = new Sprite("redGlowWideSharp");
                                _burnGlowWide.CenterOrigin();
                                _burnGlowWide.alpha = 0.75f;
                                _burnGlowWideLeft = new Sprite("redGlowWideLeft");
                                _burnGlowWideLeft.center = new Vec2(_burnGlowWideLeft.width, _burnGlowWideLeft.height / 2);
                                _burnGlowWideLeft.alpha = 0.75f;
                                _burnGlowWideRight = new Sprite("redGlowWideRight");
                                _burnGlowWideRight.center = new Vec2(0f, _burnGlowWideRight.height / 2);
                                _burnGlowWideRight.alpha = 0.75f;
                            }
                            int num2 = (int)Math.Floor(num / gap);
                            if (fp.collisionSize.y > 8f)
                            {
                                _burnGlowWide.xscale = 16f;
                                for (int i = 0; i < num2; i++)
                                {
                                    float xpos = fp.bottomLeft.x + (float)i * gap + 11f - 8f;
                                    float ypos = fp.top - 1f + (float)Math.Sin(fp.fluidWave + (float)i * 0.7f);
                                    Graphics.Draw(_burnGlowWide, xpos, ypos);
                                    if (i == 0)
                                    {
                                        Graphics.Draw(_burnGlowWideLeft, xpos, ypos);
                                    }
                                    else if (i == num2 - 1)
                                    {
                                        Graphics.Draw(_burnGlowWideRight, xpos + 16f, ypos);
                                    }
                                }
                            }
                            else
                            {
                                Graphics.doSnap = false;
                                _burnGlowWide.xscale = fp.collisionSize.x;
                                Graphics.Draw(_burnGlowWide, fp.left, fp.bottom - 2f);
                                Graphics.Draw(_burnGlowWideLeft, fp.left, fp.bottom - 2f);
                                Graphics.Draw(_burnGlowWideRight, fp.right, fp.bottom - 2f);
                                Graphics.doSnap = true;
                            }
                        }
                    }
                    t.DrawGlow();
                }
                {
                    foreach (SmallFire t2 in things[typeof(SmallFire)])
                    {
                        if (_burnGlow == null)
                        {
                            _burnGlow = new Sprite("redGlow");
                            _burnGlow.CenterOrigin();
                        }
                        _burnGlow.alpha = 0.65f * t2.alpha;
                        Graphics.Draw(_burnGlow, t2.x, t2.y - 4f);
                    }
                    return;
                }
            }
            if (layer == Layer.Virtual)
            {
                VirtualTransition.Draw();
            }
            else if (layer == Layer.Game && NetworkDebugger.enabled && !VirtualTransition.active && !(this is NetworkDebugger))
            {
                NetworkDebugger.DrawInstanceGameDebug();
            }
        }
    }

    public static T Nearest<T>(float x, float y, Thing ignore, Layer layer)
    {
        return current.NearestThing<T>(new Vec2(x, y), ignore, layer);
    }

    public static T Nearest<T>(float x, float y, Thing ignore)
    {
        return current.NearestThing<T>(new Vec2(x, y), ignore);
    }

    public static T Nearest<T>(float x, float y)
    {
        return current.NearestThing<T>(new Vec2(x, y));
    }

    public static T Nearest<T>(Vec2 p)
    {
        return current.NearestThing<T>(p);
    }

    public static T Nearest<T>(Vec2 point, Thing ignore, int nearIndex, Layer layer)
    {
        return current.NearestThing<T>(point, ignore, nearIndex, layer);
    }

    public static T Nearest<T>(Vec2 point, Thing ignore, int nearIndex)
    {
        return current.NearestThing<T>(point, ignore, nearIndex);
    }

    public static T CheckCircle<T>(float p1x, float p1y, float radius, Thing ignore)
    {
        return current.CollisionCircle<T>(new Vec2(p1x, p1y), radius, ignore);
    }

    public static T CheckCircle<T>(float p1x, float p1y, float radius)
    {
        return current.CollisionCircle<T>(new Vec2(p1x, p1y), radius);
    }

    public static T CheckCircle<T>(Vec2 p1, float radius, Thing ignore)
    {
        return current.CollisionCircle<T>(p1, radius, ignore);
    }

    public static T CheckCircle<T>(Vec2 p1, float radius)
    {
        return current.CollisionCircle<T>(p1, radius);
    }

    public static IEnumerable<T> CheckCircleAll<T>(Vec2 p1, float radius)
    {
        return current.CollisionCircleAll<T>(p1, radius);
    }

    public T CollisionCircle<T>(float p1x, float p1y, float radius, Thing ignore)
    {
        return CollisionCircle<T>(new Vec2(p1x, p1y), radius, ignore);
    }

    public T CollisionCircle<T>(float p1x, float p1y, float radius)
    {
        return CollisionCircle<T>(new Vec2(p1x, p1y), radius);
    }

    public static T CheckRect<T>(float p1x, float p1y, float p2x, float p2y, Thing ignore)
    {
        return current.CollisionRect<T>(new Vec2(p1x, p1y), new Vec2(p2x, p2y), ignore);
    }

    public static T CheckRect<T>(float p1x, float p1y, float p2x, float p2y)
    {
        return current.CollisionRect<T>(new Vec2(p1x, p1y), new Vec2(p2x, p2y));
    }

    public static T CheckRectFilter<T>(Vec2 p1, Vec2 p2, Predicate<T> filter)
    {
        return current.CollisionRectFilter(p1, p2, filter);
    }

    public static T CheckRect<T>(Vec2 p1, Vec2 p2, Thing ignore)
    {
        return current.CollisionRect<T>(p1, p2, ignore);
    }

    public static T CheckRect<T>(Vec2 p1, Vec2 p2)
    {
        return current.CollisionRect<T>(p1, p2);
    }

    public static List<T> CheckRectAll<T>(Vec2 p1, Vec2 p2, List<T> outList)
    {
        return current.CollisionRectAll(p1, p2, outList);
    }

    public static IEnumerable<T> CheckRectAll<T>(Vec2 p1, Vec2 p2)
    {
        return current.CollisionRectAll<T>(p1, p2, null);
    }

    public T CollisionRect<T>(float p1x, float p1y, float p2x, float p2y, Thing ignore)
    {
        return CollisionRect<T>(new Vec2(p1x, p1y), new Vec2(p2x, p2y), ignore);
    }

    public T CollisionRect<T>(float p1x, float p1y, float p2x, float p2y)
    {
        return CollisionRect<T>(new Vec2(p1x, p1y), new Vec2(p2x, p2y));
    }

    public static T CheckLine<T>(float p1x, float p1y, float p2x, float p2y, Thing ignore)
    {
        return current.CollisionLine<T>(new Vec2(p1x, p1y), new Vec2(p2x, p2y), ignore);
    }

    public static T CheckLine<T>(float p1x, float p1y, float p2x, float p2y)
    {
        return current.CollisionLine<T>(new Vec2(p1x, p1y), new Vec2(p2x, p2y));
    }

    public static T CheckLine<T>(float p1x, float p1y, float p2x, float p2y, out Vec2 position, Thing ignore)
    {
        return current.CollisionLine<T>(new Vec2(p1x, p1y), new Vec2(p2x, p2y), out position, ignore);
    }

    public static T CheckLine<T>(float p1x, float p1y, float p2x, float p2y, out Vec2 position)
    {
        return current.CollisionLine<T>(new Vec2(p1x, p1y), new Vec2(p2x, p2y), out position);
    }

    public static T CheckLine<T>(Vec2 p1, Vec2 p2, Thing ignore)
    {
        return current.CollisionLine<T>(p1, p2, ignore);
    }

    public static T CheckLine<T>(Vec2 p1, Vec2 p2)
    {
        return current.CollisionLine<T>(p1, p2);
    }

    public static T CheckLine<T>(Vec2 p1, Vec2 p2, out Vec2 position, Thing ignore)
    {
        return current.CollisionLine<T>(p1, p2, out position, ignore);
    }

    public static T CheckLine<T>(Vec2 p1, Vec2 p2, out Vec2 position)
    {
        return current.CollisionLine<T>(p1, p2, out position);
    }

    public T CollisionLine<T>(float p1x, float p1y, float p2x, float p2y, Thing ignore)
    {
        return CollisionLine<T>(new Vec2(p1x, p1y), new Vec2(p2x, p2y), ignore);
    }

    public T CollisionLine<T>(float p1x, float p1y, float p2x, float p2y)
    {
        return CollisionLine<T>(new Vec2(p1x, p1y), new Vec2(p2x, p2y));
    }

    public static IEnumerable<T> CheckLineAll<T>(Vec2 p1, Vec2 p2)
    {
        return current.CollisionLineAll<T>(p1, p2);
    }

    public IEnumerable<T> CheckLineAll<T>(float p1x, float p1y, float p2x, float p2y)
    {
        return CollisionLineAll<T>(new Vec2(p1x, p1y), new Vec2(p2x, p2y));
    }

    public static T CheckPoint<T>(float x, float y, Thing ignore, Layer layer)
    {
        return current.CollisionPoint<T>(new Vec2(x, y), ignore, layer);
    }

    public static T CheckPoint<T>(float x, float y, Thing ignore)
    {
        return current.CollisionPoint<T>(new Vec2(x, y), ignore);
    }

    public static Thing CheckPoint(Type pType, float x, float y, Thing ignore)
    {
        return current.CollisionPoint(pType, new Vec2(x, y), ignore);
    }

    public static T CheckPoint<T>(float x, float y)
    {
        return current.CollisionPoint<T>(new Vec2(x, y));
    }

    public static T CheckPointPlacementLayer<T>(float x, float y, Thing ignore = null, Layer layer = null)
    {
        return current.CollisionPointPlacementLayer<T>(new Vec2(x, y), ignore, layer);
    }

    public static T CheckPoint<T>(Vec2 point, Thing ignore, Layer layer)
    {
        return current.CollisionPoint<T>(point, ignore, layer);
    }

    public static T CheckPoint<T>(Vec2 point, Thing ignore)
    {
        return current.CollisionPoint<T>(point, ignore);
    }

    public static T CheckPoint<T>(Vec2 point)
    {
        return current.CollisionPoint<T>(point);
    }

    public static T CheckPointPlacementLayer<T>(Vec2 point, Thing ignore = null, Layer layer = null)
    {
        return current.CollisionPointPlacementLayer<T>(point, ignore, layer);
    }

    public static IEnumerable<T> CheckPointAll<T>(float x, float y, Layer layer)
    {
        return current.CollisionPointAll<T>(new Vec2(x, y), layer);
    }

    public static IEnumerable<T> CheckPointAll<T>(float x, float y)
    {
        return current.CollisionPointAll<T>(new Vec2(x, y));
    }

    public static IEnumerable<T> CheckPointAll<T>(Vec2 point, Layer layer)
    {
        return current.CollisionPointAll<T>(point, layer);
    }

    public static IEnumerable<T> CheckPointAll<T>(Vec2 point)
    {
        return current.CollisionPointAll<T>(point);
    }

    public T CollisionPoint<T>(float x, float y, Thing ignore, Layer layer)
    {
        return CollisionPoint<T>(new Vec2(x, y), ignore, layer);
    }

    public T CollisionPoint<T>(float x, float y, Thing ignore)
    {
        return CollisionPoint<T>(new Vec2(x, y), ignore);
    }

    public T CollisionPoint<T>(float x, float y)
    {
        return CollisionPoint<T>(new Vec2(x, y));
    }

    public Thing nearest_single(Vec2 point, HashSet<Thing> things, Thing ignore, Layer layer, bool placementLayer = false)
    {
        Thing ret = null;
        float dist = float.MaxValue;
        foreach (Thing t in things)
        {
            if (!t.removeFromLevel && t != ignore && (layer == null || ((placementLayer || t.layer == layer) && t.placementLayer == layer)))
            {
                float curDist = (point - t.position).lengthSq;
                if (curDist < dist)
                {
                    dist = curDist;
                    ret = t;
                }
            }
        }
        return ret;
    }

    public Thing nearest_single(Vec2 point, HashSet<Thing> things, Thing ignore)
    {
        Thing ret = null;
        float dist = float.MaxValue;
        foreach (Thing t in things)
        {
            if (!t.removeFromLevel && t != ignore)
            {
                float curDist = (point - t.position).lengthSq;
                if (curDist < dist)
                {
                    dist = curDist;
                    ret = t;
                }
            }
        }
        return ret;
    }

    public Thing nearest_single(Vec2 point, HashSet<Thing> things)
    {
        Thing ret = null;
        float dist = float.MaxValue;
        foreach (Thing t in things)
        {
            if (!t.removeFromLevel)
            {
                float curDist = (point - t.position).lengthSq;
                if (curDist < dist)
                {
                    dist = curDist;
                    ret = t;
                }
            }
        }
        return ret;
    }

    public Thing nearest_single(Vec2 point, IEnumerable<Thing> things, Thing ignore, Layer layer, bool placementLayer = false)
    {
        Thing ret = null;
        float dist = float.MaxValue;
        foreach (Thing t in things)
        {
            if (!t.removeFromLevel && t != ignore && (layer == null || ((placementLayer || t.layer == layer) && t.placementLayer == layer)))
            {
                float curDist = (point - t.position).lengthSq;
                if (curDist < dist)
                {
                    dist = curDist;
                    ret = t;
                }
            }
        }
        return ret;
    }

    public Thing nearest_single(Vec2 point, IEnumerable<Thing> things, Thing ignore)
    {
        Thing ret = null;
        float dist = float.MaxValue;
        foreach (Thing t in things)
        {
            if (!t.removeFromLevel && t != ignore)
            {
                float curDist = (point - t.position).lengthSq;
                if (curDist < dist)
                {
                    dist = curDist;
                    ret = t;
                }
            }
        }
        return ret;
    }

    public Thing nearest_single(Vec2 point, IEnumerable<Thing> things)
    {
        Thing ret = null;
        float dist = float.MaxValue;
        foreach (Thing t in things)
        {
            if (!t.removeFromLevel)
            {
                float curDist = (point - t.position).lengthSq;
                if (curDist < dist)
                {
                    dist = curDist;
                    ret = t;
                }
            }
        }
        return ret;
    }

    public List<KeyValuePair<float, Thing>> nearest(Vec2 point, IEnumerable<Thing> things, Thing ignore, Layer layer, bool placementLayer = false)
    {
        List<KeyValuePair<float, Thing>> dists = new List<KeyValuePair<float, Thing>>();
        foreach (Thing t in things)
        {
            if (!t.removeFromLevel && t != ignore && (layer == null || ((placementLayer || t.layer == layer) && t.placementLayer == layer)))
            {
                dists.Add(new KeyValuePair<float, Thing>((point - t.position).lengthSq, t));
            }
        }
        dists.Sort((KeyValuePair<float, Thing> x, KeyValuePair<float, Thing> y) => (!(x.Key < y.Key)) ? 1 : (-1));
        return dists;
    }

    public List<KeyValuePair<float, Thing>> nearest(Vec2 point, IEnumerable<Thing> things, Thing ignore)
    {
        List<KeyValuePair<float, Thing>> dists = new List<KeyValuePair<float, Thing>>();
        foreach (Thing t in things)
        {
            if (!t.removeFromLevel && t != ignore)
            {
                dists.Add(new KeyValuePair<float, Thing>((point - t.position).lengthSq, t));
            }
        }
        dists.Sort((KeyValuePair<float, Thing> x, KeyValuePair<float, Thing> y) => (!(x.Key < y.Key)) ? 1 : (-1));
        return dists;
    }

    public List<KeyValuePair<float, Thing>> nearest(Vec2 point, IEnumerable<Thing> things)
    {
        List<KeyValuePair<float, Thing>> dists = new List<KeyValuePair<float, Thing>>();
        foreach (Thing t in things)
        {
            if (!t.removeFromLevel)
            {
                dists.Add(new KeyValuePair<float, Thing>((point - t.position).lengthSq, t));
            }
        }
        dists.Sort((KeyValuePair<float, Thing> x, KeyValuePair<float, Thing> y) => (!(x.Key < y.Key)) ? 1 : (-1));
        return dists;
    }

    public T NearestThing<T>(Vec2 point, Thing ignore, Layer layer)
    {
        Thing ret = null;
        Type t = typeof(T);
        ret = ((!(t == typeof(Thing))) ? nearest_single(point, _things[t], ignore, layer) : nearest_single(point, _things[typeof(Thing)], ignore, layer));
        if (ret == null)
        {
            return default(T);
        }
        return (T)(object)ret;
    }

    public T NearestThing<T>(Vec2 point, Thing ignore)
    {
        Thing ret = null;
        Type t = typeof(T);
        ret = ((!(t == typeof(Thing))) ? nearest_single(point, _things[t], ignore) : nearest_single(point, _things[typeof(Thing)], ignore));
        if (ret == null)
        {
            return default(T);
        }
        return (T)(object)ret;
    }

    public T NearestThing<T>(Vec2 point)
    {
        Thing ret = null;
        Type t = typeof(T);
        ret = ((!(t == typeof(Thing))) ? nearest_single(point, _things[t]) : nearest_single(point, _things[typeof(Thing)]));
        if (ret == null)
        {
            return default(T);
        }
        return (T)(object)ret;
    }

    public T NearestThing<T>(Vec2 point, Thing ignore, int nearIndex, Layer layer)
    {
        Type t = typeof(T);
        if (t == typeof(Thing))
        {
            List<KeyValuePair<float, Thing>> dists = nearest(point, _things[typeof(Thing)], ignore, layer);
            if (dists.Count > nearIndex)
            {
                return (T)(object)dists[nearIndex].Value;
            }
        }
        List<KeyValuePair<float, Thing>> distars = nearest(point, _things[t], ignore, layer);
        if (distars.Count > nearIndex)
        {
            return (T)(object)distars[nearIndex].Value;
        }
        return default(T);
    }

    public T NearestThing<T>(Vec2 point, Thing ignore, int nearIndex)
    {
        Type t = typeof(T);
        if (t == typeof(Thing))
        {
            List<KeyValuePair<float, Thing>> dists = nearest(point, _things[typeof(Thing)], ignore);
            if (dists.Count > nearIndex)
            {
                return (T)(object)dists[nearIndex].Value;
            }
        }
        List<KeyValuePair<float, Thing>> distars = nearest(point, _things[t], ignore);
        if (distars.Count > nearIndex)
        {
            return (T)(object)distars[nearIndex].Value;
        }
        return default(T);
    }

    public T NearestThingFilter<T>(Vec2 point, Predicate<Thing> filter)
    {
        Thing ret = null;
        float dist = float.MaxValue;
        foreach (Thing t in things[typeof(T)])
        {
            if (!t.removeFromLevel)
            {
                float curDist = (point - t.position).lengthSq;
                if (curDist < dist && filter(t))
                {
                    dist = curDist;
                    ret = t;
                }
            }
        }
        if (ret == null)
        {
            return default(T);
        }
        return (T)(object)ret;
    }

    public T NearestThingFilter<T>(Vec2 point, Predicate<Thing> filter, float maxDistance)
    {
        maxDistance *= maxDistance;
        Thing ret = null;
        float dist = float.MaxValue;
        foreach (Thing t in things[typeof(T)])
        {
            if (!t.removeFromLevel)
            {
                float curDist = (point - t.position).lengthSq;
                if (curDist < dist && curDist < maxDistance && filter(t))
                {
                    dist = curDist;
                    ret = t;
                }
            }
        }
        if (ret == null)
        {
            return default(T);
        }
        return (T)(object)ret;
    }

    public T CollisionCircle<T>(Vec2 p1, float radius, Thing ignore)
    {
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && thing != ignore && Collision.Circle(p1, radius, thing))
            {
                return (T)(object)thing;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckCircle<T>(p1, radius, ignore);
        }
        return default(T);
    }

    public T CollisionCircle<T>(Vec2 p1, float radius)
    {
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && Collision.Circle(p1, radius, thing))
            {
                return (T)(object)thing;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckCircle<T>(p1, radius);
        }
        return default(T);
    }

    public IEnumerable<T> CollisionCircleAll<T>(Vec2 p1, float radius)
    {
        List<object> list = GetNextCollisionList();
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && Collision.Circle(p1, radius, thing))
            {
                list.Add(thing);
            }
        }
        if (_things.HasStaticObjects(t))
        {
            _things.quadTree.CheckCircleAll<T>(p1, radius, list);
        }
        return list.AsEnumerable().Cast<T>();
    }

    public T CollisionRectFilter<T>(Vec2 p1, Vec2 p2, Predicate<T> filter)
    {
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && Collision.Rect(p1, p2, thing) && filter((T)(object)thing))
            {
                return (T)(object)thing;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckRectangleFilter(p1, p2, filter);
        }
        return default(T);
    }

    public T CollisionRect<T>(Vec2 p1, Vec2 p2, Thing ignore)
    {
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && thing != ignore && Collision.Rect(p1, p2, thing))
            {
                return (T)(object)thing;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckRectangle<T>(p1, p2, ignore);
        }
        return default(T);
    }

    public T CollisionRect<T>(Vec2 p1, Vec2 p2)
    {
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && Collision.Rect(p1, p2, thing))
            {
                return (T)(object)thing;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckRectangle<T>(p1, p2);
        }
        return default(T);
    }

    public List<T> CollisionRectAll<T>(Vec2 p1, Vec2 p2, List<T> outList)
    {
        List<T> list = ((outList == null) ? new List<T>() : outList);
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && Collision.Rect(p1, p2, thing))
            {
                list.Add((T)(object)thing);
            }
        }
        if (_things.HasStaticObjects(t))
        {
            _things.quadTree.CheckRectangleAll(p1, p2, list);
        }
        return list;
    }

    public T CollisionLine<T>(Vec2 p1, Vec2 p2, Thing ignore)
    {
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && thing != ignore && Collision.Line(p1, p2, thing))
            {
                return (T)(object)thing;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckLine<T>(p1, p2, ignore);
        }
        return default(T);
    }

    public T CollisionLine<T>(Vec2 p1, Vec2 p2)
    {
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && Collision.Line(p1, p2, thing))
            {
                return (T)(object)thing;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckLine<T>(p1, p2);
        }
        return default(T);
    }

    public T CollisionLine<T>(Vec2 p1, Vec2 p2, out Vec2 position, Thing ignore)
    {
        position = new Vec2(0f, 0f);
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && thing != ignore)
            {
                Vec2 pos = Collision.LinePoint(p1, p2, thing);
                if (pos != Vec2.Zero)
                {
                    position = pos;
                    return (T)(object)thing;
                }
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckLinePoint<T>(p1, p2, out position, ignore);
        }
        return default(T);
    }

    public T CollisionLine<T>(Vec2 p1, Vec2 p2, out Vec2 position)
    {
        position = new Vec2(0f, 0f);
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel)
            {
                Vec2 pos = Collision.LinePoint(p1, p2, thing);
                if (pos != Vec2.Zero)
                {
                    position = pos;
                    return (T)(object)thing;
                }
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckLinePoint<T>(p1, p2, out position);
        }
        return default(T);
    }

    public IEnumerable<T> CollisionLineAll<T>(Vec2 p1, Vec2 p2)
    {
        List<object> list = GetNextCollisionList();
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && Collision.Line(p1, p2, thing))
            {
                list.Add(thing);
            }
        }
        if (_things.HasStaticObjects(t))
        {
            List<T> things = _things.quadTree.CheckLineAll<T>(p1, p2);
            list.AddRange(things.Cast<object>());
        }
        return list.AsEnumerable().Cast<T>();
    }

    public T CollisionPoint<T>(Vec2 point, Thing ignore, Layer layer)
    {
        Type t = typeof(T);
        if (t == typeof(Thing))
        {
            foreach (Thing thing in _things)
            {
                if (!thing.removeFromLevel && thing != ignore && Collision.Point(point, thing) && (layer == null || layer == thing.layer))
                {
                    return (T)(object)thing;
                }
            }
        }
        foreach (Thing thing2 in _things.GetDynamicObjects(t))
        {
            if (!thing2.removeFromLevel && thing2 != ignore && Collision.Point(point, thing2) && (layer == null || layer == thing2.layer))
            {
                return (T)(object)thing2;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckPoint<T>(point, ignore, layer);
        }
        return default(T);
    }

    public T CollisionPoint<T>(Vec2 point, Thing ignore)
    {
        Type t = typeof(T);
        if (t == typeof(Thing))
        {
            foreach (Thing thing in _things)
            {
                if (!thing.removeFromLevel && thing != ignore && Collision.Point(point, thing))
                {
                    return (T)(object)thing;
                }
            }
        }
        foreach (Thing thing2 in _things.GetDynamicObjects(t))
        {
            if (!thing2.removeFromLevel && thing2 != ignore && Collision.Point(point, thing2))
            {
                return (T)(object)thing2;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckPoint<T>(point, ignore);
        }
        return default(T);
    }

    public Thing CollisionPoint(Type pType, Vec2 point, Thing ignore)
    {
        if (pType == typeof(Thing))
        {
            foreach (Thing thing in _things)
            {
                if (!thing.removeFromLevel && thing != ignore && Collision.Point(point, thing))
                {
                    return thing;
                }
            }
        }
        foreach (Thing thing2 in _things.GetDynamicObjects(pType))
        {
            if (!thing2.removeFromLevel && thing2 != ignore && Collision.Point(point, thing2))
            {
                return thing2;
            }
        }
        if (_things.HasStaticObjects(pType))
        {
            return _things.quadTree.CheckPoint(pType, point, ignore);
        }
        return null;
    }

    public T CollisionPoint<T>(Vec2 point)
    {
        Type t = typeof(T);
        if (t == typeof(Thing))
        {
            foreach (Thing thing in _things)
            {
                if (!thing.removeFromLevel && Collision.Point(point, thing))
                {
                    return (T)(object)thing;
                }
            }
        }
        foreach (Thing thing2 in _things.GetDynamicObjects(t))
        {
            if (!thing2.removeFromLevel && Collision.Point(point, thing2))
            {
                return (T)(object)thing2;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckPoint<T>(point);
        }
        return default(T);
    }

    public T QuadTreePointFilter<T>(Vec2 point, Func<Thing, bool> pFilter)
    {
        Type t = typeof(T);
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckPointFilter<T>(point, pFilter);
        }
        return default(T);
    }

    public Thing CollisionPoint(Vec2 point, Type t, Thing ignore, Layer layer)
    {
        if (t == typeof(Thing))
        {
            foreach (Thing thing in _things)
            {
                if (!thing.removeFromLevel && thing != ignore && Collision.Point(point, thing) && (layer == null || layer == thing.layer))
                {
                    return thing;
                }
            }
        }
        foreach (Thing thing2 in _things.GetDynamicObjects(t))
        {
            if (!thing2.removeFromLevel && thing2 != ignore && Collision.Point(point, thing2) && (layer == null || layer == thing2.layer))
            {
                return thing2;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckPoint(point, t, ignore, layer);
        }
        return null;
    }

    public Thing CollisionPoint(Vec2 point, Type t, Thing ignore)
    {
        if (t == typeof(Thing))
        {
            foreach (Thing thing in _things)
            {
                if (!thing.removeFromLevel && thing != ignore && Collision.Point(point, thing))
                {
                    return thing;
                }
            }
        }
        foreach (Thing thing2 in _things.GetDynamicObjects(t))
        {
            if (!thing2.removeFromLevel && thing2 != ignore && Collision.Point(point, thing2))
            {
                return thing2;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckPoint(point, t, ignore);
        }
        return null;
    }

    public Thing CollisionPoint(Vec2 point, Type t)
    {
        if (t == typeof(Thing))
        {
            foreach (Thing thing in _things)
            {
                if (!thing.removeFromLevel && Collision.Point(point, thing))
                {
                    return thing;
                }
            }
        }
        foreach (Thing thing2 in _things.GetDynamicObjects(t))
        {
            if (!thing2.removeFromLevel && Collision.Point(point, thing2))
            {
                return thing2;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckPoint(point, t);
        }
        return null;
    }

    public T CollisionPointPlacementLayer<T>(Vec2 point, Thing ignore = null, Layer layer = null)
    {
        Type t = typeof(T);
        if (t == typeof(Thing))
        {
            foreach (Thing thing in _things)
            {
                if (!thing.removeFromLevel && thing != ignore && Collision.Point(point, thing) && (layer == null || layer == thing.placementLayer))
                {
                    return (T)(object)thing;
                }
            }
        }
        foreach (Thing thing2 in _things.GetDynamicObjects(t))
        {
            if (!thing2.removeFromLevel && thing2 != ignore && Collision.Point(point, thing2) && (layer == null || layer == thing2.placementLayer))
            {
                return (T)(object)thing2;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckPointPlacementLayer<T>(point, ignore, layer);
        }
        return default(T);
    }

    public T CollisionPointFilter<T>(Vec2 point, Predicate<Thing> filter)
    {
        Type t = typeof(T);
        if (t == typeof(Thing))
        {
            foreach (Thing thing in _things)
            {
                if (!thing.removeFromLevel && filter(thing) && Collision.Point(point, thing))
                {
                    return (T)(object)thing;
                }
            }
        }
        foreach (Thing thing2 in _things.GetDynamicObjects(t))
        {
            if (!thing2.removeFromLevel && filter(thing2) && Collision.Point(point, thing2))
            {
                return (T)(object)thing2;
            }
        }
        if (_things.HasStaticObjects(t))
        {
            return _things.quadTree.CheckPointFilter<T>(point, filter);
        }
        return default(T);
    }

    public IEnumerable<T> CollisionPointAll<T>(Vec2 point, Layer layer)
    {
        List<object> list = GetNextCollisionList();
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && Collision.Point(point, thing) && (layer == null || layer == thing.layer))
            {
                list.Add(thing);
            }
        }
        if (_things.HasStaticObjects(t))
        {
            T thing2 = (T)(object)_things.quadTree.CheckPoint<T>(point, null, layer);
            if (thing2 != null)
            {
                list.Add(thing2);
            }
        }
        return list.AsEnumerable().Cast<T>();
    }

    public IEnumerable<T> CollisionPointAll<T>(Vec2 point)
    {
        List<object> list = GetNextCollisionList();
        Type t = typeof(T);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && Collision.Point(point, thing))
            {
                list.Add(thing);
            }
        }
        if (_things.HasStaticObjects(t))
        {
            T thing2 = (T)(object)_things.quadTree.CheckPoint<T>(point);
            if (thing2 != null)
            {
                list.Add(thing2);
            }
        }
        return list.AsEnumerable().Cast<T>();
    }

    public void CollisionBullet(Vec2 point, List<MaterialThing> output)
    {
        Type t = typeof(MaterialThing);
        foreach (Thing thing in _things.GetDynamicObjects(t))
        {
            if (!thing.removeFromLevel && Collision.Point(point, thing))
            {
                output.Add(thing as MaterialThing);
            }
        }
        if (_things.HasStaticObjects(t))
        {
            MaterialThing thing2 = _things.quadTree.CheckPoint<MaterialThing>(point);
            if (thing2 != null)
            {
                output.Add(thing2);
            }
        }
    }

    public static T CheckRay<T>(Vec2 start, Vec2 end)
    {
        return current.CollisionRay<T>(start, end);
    }

    public T CollisionRay<T>(Vec2 start, Vec2 end)
    {
        Vec2 hitPos;
        return CheckRay<T>(start, end, out hitPos);
    }

    public static T CheckRay<T>(Vec2 start, Vec2 end, out Vec2 hitPos)
    {
        return current.CollisionRay<T>(start, end, out hitPos);
    }

    public static T CheckRay<T>(Vec2 start, Vec2 end, Thing ignore, out Vec2 hitPos)
    {
        return current.CollisionRay<T>(start, end, ignore, out hitPos);
    }

    public T CollisionRay<T>(Vec2 start, Vec2 end, out Vec2 hitPos)
    {
        Vec2 travelDirNormalized = end - start;
        float length = travelDirNormalized.length;
        travelDirNormalized.Normalize();
        Math.Ceiling(length);
        Stack<TravelInfo> tests = new Stack<TravelInfo>();
        tests.Push(new TravelInfo(start, end, length));
        float lengthDiv2 = 0f;
        while (tests.Count > 0)
        {
            TravelInfo current = tests.Pop();
            if (Level.current.CollisionLine<T>(current.p1, current.p2) == null)
            {
                continue;
            }
            if (current.length < 8f)
            {
                T hit = Raycast<T>(current.p1, travelDirNormalized, current.length, out hitPos);
                if (hit != null)
                {
                    return hit;
                }
            }
            else
            {
                lengthDiv2 = current.length * 0.5f;
                Vec2 halfPoint = current.p1 + travelDirNormalized * lengthDiv2;
                tests.Push(new TravelInfo(halfPoint, current.p2, lengthDiv2));
                tests.Push(new TravelInfo(current.p1, halfPoint, lengthDiv2));
            }
        }
        hitPos = end;
        return default(T);
    }

    public T CollisionRay<T>(Vec2 start, Vec2 end, Thing ignore, out Vec2 hitPos)
    {
        Vec2 travelDirNormalized = end - start;
        float length = travelDirNormalized.length;
        travelDirNormalized.Normalize();
        Math.Ceiling(length);
        Stack<TravelInfo> tests = new Stack<TravelInfo>();
        tests.Push(new TravelInfo(start, end, length));
        float lengthDiv2 = 0f;
        while (tests.Count > 0)
        {
            TravelInfo current = tests.Pop();
            if (Level.current.CollisionLine<T>(current.p1, current.p2, ignore) == null)
            {
                continue;
            }
            if (current.length < 8f)
            {
                T hit = Raycast<T>(current.p1, travelDirNormalized, ignore, current.length, out hitPos);
                if (hit != null)
                {
                    return hit;
                }
            }
            else
            {
                lengthDiv2 = current.length * 0.5f;
                Vec2 halfPoint = current.p1 + travelDirNormalized * lengthDiv2;
                tests.Push(new TravelInfo(halfPoint, current.p2, lengthDiv2));
                tests.Push(new TravelInfo(current.p1, halfPoint, lengthDiv2));
            }
        }
        hitPos = end;
        return default(T);
    }

    private T Raycast<T>(Vec2 p1, Vec2 dir, float length, out Vec2 hit)
    {
        int steps = (int)Math.Ceiling(length);
        Vec2 s = p1;
        do
        {
            steps--;
            T col = current.CollisionPoint<T>(s);
            if (col != null)
            {
                hit = s;
                return col;
            }
            s += dir;
        }
        while (steps > 0);
        hit = s;
        return default(T);
    }

    private T Raycast<T>(Vec2 p1, Vec2 dir, Thing ignore, float length, out Vec2 hit)
    {
        int steps = (int)Math.Ceiling(length);
        Vec2 s = p1;
        do
        {
            steps--;
            T col = current.CollisionPoint<T>(s, ignore);
            if (col != null)
            {
                hit = s;
                return col;
            }
            s += dir;
        }
        while (steps > 0);
        hit = s;
        return default(T);
    }

    private T Rectcast<T>(Vec2 p1, Vec2 p2, Rectangle rect, out Vec2 hit)
    {
        Vec2 dir = p2 - p1;
        int steps = (int)Math.Ceiling(dir.length);
        dir.Normalize();
        Vec2 s = p1;
        do
        {
            steps--;
            T col = current.CollisionRect<T>(s + new Vec2(rect.Top, rect.Left), s + new Vec2(rect.Bottom, rect.Right));
            if (col != null)
            {
                hit = s;
                return col;
            }
            s += dir;
        }
        while (steps > 0);
        hit = s;
        return default(T);
    }
}
