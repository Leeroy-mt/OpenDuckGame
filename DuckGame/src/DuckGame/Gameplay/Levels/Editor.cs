using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class Editor : Level
{
	private enum EditorTouchState
	{
		Normal,
		OpenMenu,
		Eyedropper,
		EditObject,
		OpenLevel,
		PickTile
	}

	private class EditorTouchButton
	{
		public string caption;

		public string explanation;

		public Vec2 position;

		public Vec2 size;

		public bool threeFingerGesture;

		public EditorTouchState state;
	}

	private delegate Thing ThingConstructor();

	public static bool editingContent = false;

	private static Stack<object> focusStack = new Stack<object>();

	private static int numPops = 0;

	private EditorCam _editorCam;

	private SpriteMap _cursor;

	private SpriteMap _tileset;

	private BitmapFont _font;

	private ContextMenu _placementMenu;

	private ContextMenu _objectMenu;

	private CursorMode _cursorMode;

	public static bool active = false;

	public static bool selectingLevel = false;

	private InputType dragModeInputType;

	private InputType dragStartInputType;

	private Vec2 _sizeRestriction = new Vec2(800f, 640f);

	public static int placementLimit = 0;

	public int placementTotalCost;

	private Vec2 _topLeftMost = new Vec2(99999f, 99999f);

	private Vec2 _bottomRightMost = new Vec2(-99999f, -99999f);

	public static bool hasUnsavedChanges;

	public static Texture2D previewCapture;

	private BinaryClassChunk _eyeDropperSerialized;

	private static Dictionary<Type, List<MethodInfo>> _networkActionIndexes = new Dictionary<Type, List<MethodInfo>>();

	private static EditorGroup _placeables;

	protected List<Thing> _levelThingsNormal = new List<Thing>();

	protected List<Thing> _levelThingsAlternate = new List<Thing>();

	public static string placementItemDetails = "";

	private string _saveName = "";

	private SaveFileDialog _saveForm = new SaveFileDialog();

	private OpenFileDialog _loadForm = new OpenFileDialog();

	public static bool enteringText = false;

	private static ContextMenu _lockInput;

	private static ContextMenu _lockInputChange;

	private int _lastCommand = -1;

	private List<Command> _commands = new List<Command>();

	public static bool clickedMenu = false;

	public static bool clickedContextBackground = false;

	public bool clicked;

	private bool _updateEvenWhenInactive;

	private bool _editorLoadFinished;

	private NotifyDialogue _notify;

	private bool _placementMode = true;

	private bool _editMode;

	private bool _copyMode;

	public static bool hoverUI = false;

	public static EditorInput inputMode = EditorInput.Gamepad;

	private SpriteMap _editorButtons;

	private bool _loadingLevel;

	private List<Thing> _placeObjects = new List<Thing>();

	public bool minimalConversionLoad;

	private bool processingMirror;

	private bool _isPaste;

	private bool _looseClear;

	public static LevelData _currentLevelData = new LevelData();

	public static bool saving = false;

	private int _gridW = 40;

	private int _gridH = 24;

	private float _cellSize = 16f;

	private Vec2 _camSize;

	private Vec2 _panAnchor;

	private Vec2 _tilePosition;

	private bool _closeMenu;

	private bool _placingTiles;

	private bool _dragMode;

	private bool _deleteMode;

	private bool _didPan;

	private static bool _listLoaded = false;

	private Thing _placementType;

	private MonoFileDialog _fileDialog;

	private SteamUploadDialog _uploadDialog;

	private static string _initialDirectory;

	private bool _menuOpen;

	private Layer _gridLayer;

	private Layer _procLayer;

	private Layer _objectMenuLayer;

	public bool _pathNorth;

	public bool _pathSouth;

	public bool _pathEast;

	public bool _pathWest;

	private bool _quitting;

	private Sprite _cantPlace;

	private Sprite _sideArrow;

	private Sprite _sideArrowHover;

	private Sprite _die;

	private Sprite _dieHover;

	private Sprite _editorCurrency;

	private HashSet<Thing> _selection = new HashSet<Thing>();

	private Sprite _singleBlock;

	private Sprite _multiBlock;

	public bool _miniMode;

	public Vec2 _genSize = new Vec2(3f, 3f);

	public Vec2 _genTilePos = new Vec2(1f, 1f);

	public Vec2 _editTilePos = new Vec2(1f, 1f);

	public Vec2 _prevEditTilePos = new Vec2(1f, 1f);

	public MaterialSelection _selectionMaterial;

	public MaterialSelection _selectionMaterialPaste;

	public int generatorComplexity;

	private bool _editingOpenAirVariationPrev;

	public bool editingOpenAirVariation;

	private string _additionalSaveDirectory;

	private bool _doingResave;

	private List<string> existingGUID = new List<string>();

	private ContextMenu _lastHoverMenuOpen;

	private Thing _hover;

	private Thing _secondaryHover;

	private bool bMouseInput;

	private bool bGamepadInput;

	private bool bTouchInput;

	private Thing _move;

	private bool _showPlacementMenu;

	private static InputProfile _input = null;

	private MessageDialogue _noSpawnsDialogue;

	private bool _runLevelAnyway;

	public static bool copying = false;

	private Vec2 _tileDragContext = Vec2.MinValue;

	private Vec2 _tilePositionPrev = Vec2.Zero;

	private Vec2 _tileDragDif = Vec2.Zero;

	private Vec2 _lastTilePosDraw = Vec2.Zero;

	public static bool tookInput = false;

	public static bool didUIScroll = false;

	private ContextMenu _hoverMenu;

	private int _hoverMode;

	private GameContext _procContext;

	protected int _procSeed;

	private TileButton _hoverButton;

	public bool _doGen;

	private int _prevProcX;

	private int _prevProcY;

	private RandomLevelNode _currentMapNode;

	private int _loadPosX;

	private int _loadPosY;

	public static bool skipFrame = false;

	private bool firstClick;

	public static Thing openContextThing;

	public static Thing pretendPinned = null;

	public static Vec2 openPosition = Vec2.Zero;

	public static bool ignorePinning = false;

	public static bool reopenContextMenu = false;

	private Vec2 middleClickPos;

	private Vec2 lastMousePos = Vec2.Zero;

	public static string tooltip = null;

	private bool _twoFingerGesture;

	private bool _twoFingerGestureStarting;

	private bool _twoFingerZooming;

	private bool _threeFingerGesture;

	private bool _threeFingerGestureRelease;

	private float _twoFingerSpacing;

	private EditorTouchButton _activeTouchButton;

	private EditorTouchState _touchState;

	private bool _prevTouch;

	private List<EditorTouchButton> _touchButtons = new List<EditorTouchButton>();

	private List<EditorTouchButton> _fileDialogButtons = new List<EditorTouchButton>();

	private EditorTouchButton _cancelButton;

	private EditorTouchButton _editTilesButton;

	public static bool fakeTouch = false;

	public static bool _clickedTouchButton = false;

	private Thing _oldHover;

	public static bool waitForNoTouchInput = false;

	public static bool bigInterfaceMode;

	private bool _openTileSelector;

	private List<ContextMenu.SearchPair> searchItems = new List<ContextMenu.SearchPair>();

	public static bool hoverMiniButton;

	private bool clearedKeyboardStringForSearch;

	private string _prevSearchString = "";

	private Vec2 _selectionDragStart = Vec2.Zero;

	private Vec2 _selectionDragEnd = Vec2.Zero;

	private Vec2 _moveDragStart = Vec2.Zero;

	public HashSet<Thing> _currentDragSelectionHover = new HashSet<Thing>();

	public HashSet<Thing> _currentDragSelectionHoverAdd = new HashSet<Thing>();

	private int focusWait = 5;

	private bool _openedEditMenu;

	private List<BinaryClassChunk> _selectionCopy = new List<BinaryClassChunk>();

	private List<Thing> _pasteBatch = new List<Thing>();

	private Vec2 _copyCenter;

	private Vec2 pasteOffset;

	private bool _performCopypaste;

	private bool _dragSelectShiftModifier;

	private Thing oldHover;

	private Thing oldSecondaryHover;

	public static bool editorDraw = false;

	public static int _procXPos = 1;

	public static int _procYPos = 1;

	public static int _procTilesWide = 3;

	public static int _procTilesHigh = 3;

	public static bool hoverTextBox = false;

	private Rectangle _ultimateBounds;

	private bool _leftSelectionDraw = true;

	private int _searchHoverIndex = -1;

	private bool rotateValid;

	private RenderTarget2D _procTarget;

	private Vec2 _procDrawOffset = Vec2.Zero;

	private bool _onlineSettingChanged;

	public bool hadGUID;

	public static long kMassiveBitmapStringHeader = 4967129034509872376L;

	protected RandomLevelData _centerTile;

	public static bool isTesting = false;

	public bool searching;

	private static Dictionary<Type, Thing> _thingMap = new Dictionary<Type, Thing>();

	private static Dictionary<Type, List<ClassMember>> _classMembers = new Dictionary<Type, List<ClassMember>>();

	private static Dictionary<Type, List<ClassMember>> _staticClassMembers = new Dictionary<Type, List<ClassMember>>();

	private static Dictionary<Type, Dictionary<string, ClassMember>> _classMemberNames = new Dictionary<Type, Dictionary<string, ClassMember>>();

	public static Dictionary<Type, Dictionary<string, AccessorInfo>> _accessorCache = new Dictionary<Type, Dictionary<string, AccessorInfo>>();

	private static Dictionary<Type, object[]> _constructorParameters = new Dictionary<Type, object[]>();

	private static Dictionary<Type, ThingConstructor> _defaultConstructors = new Dictionary<Type, ThingConstructor>();

	private static Dictionary<Type, Func<object>> _constructorParameterExpressions = new Dictionary<Type, Func<object>>();

	public static List<Type> ThingTypes;

	public static List<Type> GroupThingTypes;

	public static Dictionary<Type, List<Type>> AllBaseTypes;

	public static Dictionary<Type, IEnumerable<FieldInfo>> AllEditorFields;

	public static Dictionary<Type, List<FieldInfo>> EditorFieldsForType;

	public static Dictionary<Type, FieldInfo[]> AllStateFields;

	public static Map<ushort, Type> IDToType = new Map<ushort, Type>();

	public static Dictionary<Type, Thing> _typeInstances = new Dictionary<Type, Thing>();

	public bool tabletMode;

	public static uint thingTypesHash;

	private static bool _clearOnce = false;

	public bool placementLimitReached
	{
		get
		{
			if (placementLimit > 0)
			{
				return placementTotalCost >= placementLimit;
			}
			return false;
		}
	}

	public bool placementOutOfSizeRange => false;

	public static List<string> activatedLevels => DuckNetwork.core._activatedLevels;

	public static int customLevelCount => activatedLevels.Count + clientLevelCount;

	public static int clientLevelCount
	{
		get
		{
			if (!(bool)TeamSelect2.GetMatchSetting("clientlevelsenabled").value)
			{
				return 0;
			}
			int profileLevels = 0;
			if (DuckNetwork.profiles != null)
			{
				foreach (Profile p in DuckNetwork.profiles)
				{
					if (p != null && p.connection != null && p.connection.status != ConnectionStatus.Disconnected)
					{
						profileLevels += p.numClientCustomLevels;
					}
				}
			}
			return profileLevels;
		}
	}

	public static EditorGroup Placeables
	{
		get
		{
			while (!_listLoaded)
			{
				Thread.Sleep(16);
			}
			return _placeables;
		}
	}

	protected List<Thing> _levelThings
	{
		get
		{
			if (!editingOpenAirVariation)
			{
				return _levelThingsNormal;
			}
			return _levelThingsAlternate;
		}
	}

	public List<Thing> levelThings => _levelThings;

	public string saveName
	{
		get
		{
			return _saveName;
		}
		set
		{
			_saveName = value;
		}
	}

	public static ContextMenu lockInput
	{
		get
		{
			return _lockInput;
		}
		set
		{
			_lockInputChange = value;
		}
	}

	public float cellSize
	{
		get
		{
			return _cellSize;
		}
		set
		{
			_cellSize = value;
		}
	}

	private float width => (float)_gridW * _cellSize;

	private float height => (float)_gridH * _cellSize;

	public Thing placementType
	{
		get
		{
			return _placementType;
		}
		set
		{
			_placementType = value;
			_eyeDropperSerialized = null;
		}
	}

	public MonoFileDialog fileDialog => _fileDialog;

	public static string initialDirectory => _initialDirectory;

	public static Layer objectMenuLayer => Main.editor._objectMenuLayer;

	public static bool miniMode
	{
		get
		{
			if (Level.current is Editor)
			{
				return (Level.current as Editor)._miniMode;
			}
			return false;
		}
		set
		{
			if (Level.current is Editor)
			{
				(Level.current as Editor)._miniMode = value;
			}
		}
	}

	public float _chance
	{
		get
		{
			return _currentLevelData.proceduralData.chance;
		}
		set
		{
			_currentLevelData.proceduralData.chance = value;
		}
	}

	public int _maxPerLevel
	{
		get
		{
			return _currentLevelData.proceduralData.maxPerLevel;
		}
		set
		{
			_currentLevelData.proceduralData.maxPerLevel = value;
		}
	}

	public bool _enableSingle
	{
		get
		{
			return _currentLevelData.proceduralData.enableSingle;
		}
		set
		{
			_currentLevelData.proceduralData.enableSingle = value;
		}
	}

	public bool _enableMulti
	{
		get
		{
			return _currentLevelData.proceduralData.enableMulti;
		}
		set
		{
			_currentLevelData.proceduralData.enableMulti = value;
		}
	}

	public bool _canMirror
	{
		get
		{
			return _currentLevelData.proceduralData.canMirror;
		}
		set
		{
			_currentLevelData.proceduralData.canMirror = value;
		}
	}

	public bool _isMirrored
	{
		get
		{
			return _currentLevelData.proceduralData.isMirrored;
		}
		set
		{
			_currentLevelData.proceduralData.isMirrored = value;
		}
	}

	public string additionalSaveDirectory => _additionalSaveDirectory;

	public static InputProfile input => _input;

	public static float interfaceSizeMultiplier
	{
		get
		{
			if (inputMode != EditorInput.Touch)
			{
				return 1f;
			}
			return 2f;
		}
	}

	public static bool arcadeMachineMode
	{
		get
		{
			if (Level.current is Editor e && e._levelThings.Count == 1 && e._levelThings[0] is ImportMachine)
			{
				return true;
			}
			return false;
		}
	}

	public static Dictionary<Type, Thing> thingMap => _thingMap;

	public static void PopFocus()
	{
		numPops++;
	}

	public static void PopFocusNow()
	{
		if (focusStack.Count > 0)
		{
			focusStack.Pop();
		}
	}

	public static object PeekFocus()
	{
		if (focusStack.Count > 0)
		{
			return focusStack.Peek();
		}
		return null;
	}

	public static void PushFocus(object o)
	{
		focusStack.Push(o);
	}

	public static bool HasFocus()
	{
		return focusStack.Count != 0;
	}

	public static byte NetworkActionIndex(Type pType, MethodInfo pMethod)
	{
		int idx = GetNetworkActionMethods(pType).IndexOf(pMethod);
		if (idx >= 0)
		{
			return (byte)idx;
		}
		return byte.MaxValue;
	}

	public static MethodInfo MethodFromNetworkActionIndex(Type pType, byte pIndex)
	{
		List<MethodInfo> methods = GetNetworkActionMethods(pType);
		if (pIndex < methods.Count)
		{
			return methods[pIndex];
		}
		return null;
	}

	private static List<MethodInfo> GetNetworkActionMethods(Type pType)
	{
		if (!_networkActionIndexes.ContainsKey(pType))
		{
			List<MethodInfo> infos = new List<MethodInfo>();
			MethodInfo[] methods = pType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo i2 in methods)
			{
				if (i2.GetCustomAttributes(typeof(NetworkAction), inherit: false).Any())
				{
					infos.Add(i2);
				}
			}
			if (pType.BaseType != null)
			{
				List<MethodInfo> baseMethods = GetNetworkActionMethods(pType.BaseType);
				infos.AddRange(baseMethods);
			}
			_networkActionIndexes[pType] = infos;
		}
		return _networkActionIndexes[pType];
	}

	private void RunCommand(Command command)
	{
		hasUnsavedChanges = true;
		if (_lastCommand < _commands.Count - 1)
		{
			_commands.RemoveRange(_lastCommand + 1, _commands.Count - (_lastCommand + 1));
		}
		_commands.Add(command);
		_lastCommand++;
		command.Do();
	}

	private void UndoCommand()
	{
		hasUnsavedChanges = true;
		if (_lastCommand >= 0)
		{
			_commands[_lastCommand--].Undo();
		}
	}

	private void RedoCommand()
	{
		hasUnsavedChanges = true;
		if (_lastCommand < _commands.Count - 1)
		{
			_commands[++_lastCommand].Do();
		}
	}

	public void AddObject(Thing obj)
	{
		hasUnsavedChanges = true;
		if (obj == null)
		{
			return;
		}
		if (obj.maxPlaceable >= 0 && base.things[obj.GetType()].Count() >= obj.maxPlaceable)
		{
			HUD.AddPlayerChangeDisplay("@UNPLUG@|RED| Too many placed!", 2f);
			return;
		}
		if (obj is ThingContainer)
		{
			ThingContainer container = obj as ThingContainer;
			if (container.bozocheck)
			{
				foreach (Thing thing in container.things)
				{
					if (!Thing.CheckForBozoData(thing))
					{
						AddObject(thing);
					}
				}
				return;
			}
			{
				foreach (Thing thing2 in container.things)
				{
					AddObject(thing2);
				}
				return;
			}
		}
		if (obj is BackgroundUpdater)
		{
			for (int i = 0; i < _levelThings.Count; i++)
			{
				Thing t = _levelThings[i];
				if (t is BackgroundUpdater)
				{
					History.Add(delegate
					{
						RemoveObject(t);
					}, delegate
					{
						AddObject(t);
					});
					i--;
				}
			}
		}
		obj.active = false;
		AddThing(obj);
		_levelThings.Add(obj);
		if (!_loadingLevel && obj is IDontMove)
		{
			_placeObjects.Add(obj);
		}
		placementTotalCost += CalculatePlacementCost(obj);
		if (_sizeRestriction.x > 0f)
		{
			AdjustSizeLimits(obj);
		}
		if (_loadingLevel)
		{
			return;
		}
		if (!_isPaste)
		{
			obj.EditorAdded();
		}
		if (obj is MirrorMode || processingMirror || obj is BackgroundUpdater)
		{
			return;
		}
		processingMirror = true;
		foreach (MirrorMode m in base.things[typeof(MirrorMode)])
		{
			if (((MirrorMode.Setting)m.mode == MirrorMode.Setting.Both || (MirrorMode.Setting)m.mode == MirrorMode.Setting.Vertical) && Math.Abs(m.position.x - obj.position.x) > 2f)
			{
				Vec2 newPos = obj.position - new Vec2((obj.position.x - m.position.x) * 2f, 0f);
				Thing mirror = Thing.LoadThing(obj.Serialize());
				mirror.position = newPos;
				mirror.flipHorizontal = !obj.flipHorizontal;
				AddObject(mirror);
				mirror.EditorFlip(pVertical: false);
			}
			if (((MirrorMode.Setting)m.mode == MirrorMode.Setting.Both || (MirrorMode.Setting)m.mode == MirrorMode.Setting.Horizontal) && Math.Abs(m.position.y - obj.position.y) > 2f)
			{
				Vec2 newPos2 = obj.position - new Vec2(0f, (obj.position.y - m.position.y) * 2f);
				Thing mirror2 = Thing.LoadThing(obj.Serialize());
				mirror2.position = newPos2;
				AddObject(mirror2);
				mirror2.EditorFlip(pVertical: true);
			}
			if ((MirrorMode.Setting)m.mode == MirrorMode.Setting.Both && Math.Abs(m.position.x - obj.position.x) > 2f && Math.Abs(m.position.y - obj.position.y) > 2f)
			{
				Vec2 newPos3 = obj.position - new Vec2((obj.position.x - m.position.x) * 2f, (obj.position.y - m.position.y) * 2f);
				Thing mirror3 = Thing.LoadThing(obj.Serialize());
				mirror3.position = newPos3;
				mirror3.flipHorizontal = !obj.flipHorizontal;
				AddObject(mirror3);
				mirror3.EditorFlip(pVertical: false);
				mirror3.EditorFlip(pVertical: true);
			}
		}
		processingMirror = false;
	}

	public void RemoveObject(Thing obj)
	{
		hasUnsavedChanges = true;
		Level.current.RemoveThing(obj);
		_levelThings.Remove(obj);
		if (obj is IDontMove)
		{
			_placeObjects.Add(obj);
		}
		placementTotalCost -= CalculatePlacementCost(obj);
		if (_sizeRestriction.x > 0f && (obj.x <= _topLeftMost.x || obj.x >= _bottomRightMost.x || obj.y <= _topLeftMost.y || obj.y >= _bottomRightMost.y))
		{
			RecalculateSizeLimits();
		}
		obj.EditorRemoved();
		if (_loadingLevel || obj is MirrorMode || processingMirror || obj is BackgroundUpdater)
		{
			return;
		}
		processingMirror = true;
		foreach (MirrorMode m in base.things[typeof(MirrorMode)])
		{
			if ((MirrorMode.Setting)m.mode == MirrorMode.Setting.Both || (MirrorMode.Setting)m.mode == MirrorMode.Setting.Vertical)
			{
				Vec2 pairPos = obj.position + new Vec2((0f - (obj.position.x - m.position.x)) * 2f, 0f);
				Thing t = Level.current.CollisionPoint(pairPos, obj.GetType());
				if (t != null)
				{
					RemoveObject(t);
				}
			}
			if ((MirrorMode.Setting)m.mode == MirrorMode.Setting.Both || (MirrorMode.Setting)m.mode == MirrorMode.Setting.Horizontal)
			{
				Vec2 pairPos2 = obj.position + new Vec2(0f, (0f - (obj.position.y - m.position.y)) * 2f);
				Thing t2 = Level.current.CollisionPoint(pairPos2, obj.GetType());
				if (t2 != null)
				{
					RemoveObject(t2);
				}
			}
			if ((MirrorMode.Setting)m.mode == MirrorMode.Setting.Both)
			{
				Vec2 pairPos3 = obj.position + new Vec2((0f - (obj.position.x - m.position.x)) * 2f, (0f - (obj.position.y - m.position.y)) * 2f);
				Thing t3 = Level.current.CollisionPoint(pairPos3, obj.GetType());
				if (t3 != null)
				{
					RemoveObject(t3);
				}
			}
		}
		processingMirror = false;
	}

	public void AdjustSizeLimits(Thing pObject)
	{
		if (pObject.x < _topLeftMost.x)
		{
			_topLeftMost.x = pObject.x;
		}
		if (pObject.x > _bottomRightMost.x)
		{
			_bottomRightMost.x = pObject.x;
		}
		if (pObject.y < _topLeftMost.y)
		{
			_topLeftMost.y = pObject.y;
		}
		if (pObject.y > _bottomRightMost.y)
		{
			_bottomRightMost.y = pObject.y;
		}
	}

	public void RecalculateSizeLimits()
	{
		_topLeftMost = new Vec2(99999f, 99999f);
		_bottomRightMost = new Vec2(-99999f, -99999f);
		foreach (Thing t in _levelThings)
		{
			AdjustSizeLimits(t);
		}
	}

	public static int CalculatePlacementCost(Thing pObject)
	{
		return pObject.placementCost;
	}

	public void ClearEverything()
	{
		foreach (Thing t in _levelThingsNormal)
		{
			Level.current.RemoveThing(t);
		}
		_levelThingsNormal.Clear();
		foreach (Thing t2 in _levelThingsAlternate)
		{
			Level.current.RemoveThing(t2);
		}
		_levelThingsAlternate.Clear();
		editingOpenAirVariation = (_editingOpenAirVariationPrev = false);
		_lastCommand = -1;
		_commands.Clear();
		if (!_looseClear)
		{
			_procContext = null;
			_procTarget = null;
		}
		_pathNorth = false;
		_pathSouth = false;
		_pathWest = false;
		_pathEast = false;
		_miniMode = false;
		base.things.quadTree.Clear();
		generatorComplexity = 0;
		Custom.ClearCustomData();
		_currentLevelData = new LevelData();
		_currentLevelData.metaData.guid = Guid.NewGuid().ToString();
		previewCapture = null;
		hasUnsavedChanges = false;
		placementTotalCost = 0;
		RecalculateSizeLimits();
		History.Clear();
	}

	private LevelType GetLevelType()
	{
		if (arcadeMachineMode)
		{
			return LevelType.Arcade_Machine;
		}
		LevelType type = LevelType.Deathmatch;
		if (_levelThings.FirstOrDefault((Thing x) => x is ChallengeMode) != null)
		{
			type = LevelType.Challenge;
		}
		else if (_levelThings.FirstOrDefault((Thing x) => x is ArcadeMode) != null)
		{
			type = LevelType.Arcade;
		}
		return type;
	}

	private LevelSize GetLevelSize()
	{
		_topLeft = new Vec2(99999f, 99999f);
		_bottomRight = new Vec2(-99999f, -99999f);
		CalculateBounds();
		float length = (base.topLeft - base.bottomRight).length;
		LevelSize levelSizeVal = LevelSize.Ginormous;
		if (length < 900f)
		{
			levelSizeVal = LevelSize.Large;
		}
		if (length < 650f)
		{
			levelSizeVal = LevelSize.Medium;
		}
		if (length < 400f)
		{
			levelSizeVal = LevelSize.Small;
		}
		if (length < 200f)
		{
			levelSizeVal = LevelSize.Tiny;
		}
		return levelSizeVal;
	}

	private bool LevelIsOnlineCapable()
	{
		foreach (Thing levelThing in _levelThings)
		{
			if (!ContentProperties.GetBag(levelThing.GetType()).GetOrDefault("isOnlineCapable", defaultValue: true))
			{
				return false;
			}
		}
		return true;
	}

	public void SteamUpload()
	{
		if (arcadeMachineMode)
		{
			(_levelThings[0] as ArcadeMachine).UpdateData();
			if ((_levelThings[0] as ArcadeMachine).challenge01Data == null || (_levelThings[0] as ArcadeMachine).challenge02Data == null || (_levelThings[0] as ArcadeMachine).challenge02Data == null)
			{
				DoMenuClose();
				_closeMenu = false;
				_notify.Open("You must select 3 valid Challenges!");
				return;
			}
		}
		if (_saveName == "")
		{
			DoMenuClose();
			_closeMenu = false;
			_notify.Open("Please save the level first...");
			return;
		}
		Save();
		_uploadDialog.Open(_currentLevelData);
		DoMenuClose();
		_closeMenu = false;
		Content.customPreviewWidth = 0;
		Content.customPreviewHeight = 0;
		Content.customPreviewCenter = Vec2.Zero;
	}

	public void EnterEditor()
	{
		focusWait = 10;
		Layer.ClearLayers();
		_gridLayer = new Layer("GRID", Layer.Background.depth + 5, Layer.Background.camera);
		_gridLayer.allowTallAspect = true;
		Layer.Add(_gridLayer);
		_procLayer = new Layer("PROC", Layer.Background.depth + 25, new Camera(0f, 0f, Graphics.width, Graphics.height));
		_procLayer.allowTallAspect = true;
		Layer.Add(_procLayer);
		Music.Stop();
		if (!isTesting)
		{
			_placementType = null;
			CenterView();
			_tilePosition = new Vec2(0f, 0f);
		}
		_ultimateBounds = Level.current.things.quadTree.rectangle;
		Layer.HUD.camera.InitializeToScreenAspect();
		Layer.HUD.camera.width *= 2f;
		Layer.HUD.camera.height *= 2f;
		Layer.HUD.allowTallAspect = true;
		if (Resolution.current.aspect > 2f)
		{
			Layer.HUD.camera.width *= 2f;
			Layer.HUD.camera.height *= 2f;
		}
		if (_objectMenuLayer == null)
		{
			_objectMenuLayer = new Layer("OBJECTMENU", Layer.HUD.depth - 25, new Camera(0f, 0f, Layer.HUD.camera.width, Layer.HUD.camera.height));
			_objectMenuLayer.allowTallAspect = true;
		}
		Layer.Add(_objectMenuLayer);
		base.backgroundColor = new Color(20, 20, 20);
		focusStack.Clear();
		active = true;
		isTesting = false;
		inputMode = EditorInput.Gamepad;
	}

	public void Quit()
	{
		_quitting = true;
	}

	public override void DoInitialize()
	{
		SFX.StopAllSounds();
		if (!_initialized)
		{
			Initialize();
			_initialized = true;
		}
		else
		{
			EnterEditor();
			base.DoInitialize();
		}
	}

	public override void Terminate()
	{
	}

	public void UpdateObjectMenu()
	{
		if (_objectMenu != null)
		{
			Level.Remove(_objectMenu);
		}
		_objectMenu = new PlacementMenu(0f, 0f);
		Level.Add(_objectMenu);
		ContextMenu objectMenu = _objectMenu;
		bool visible = (_objectMenu.active = false);
		objectMenu.visible = visible;
	}

	public override void Initialize()
	{
		while (!_listLoaded)
		{
			Thread.Sleep(16);
		}
		_editorCam = new EditorCam();
		base.camera = _editorCam;
		base.camera.InitializeToScreenAspect();
		_selectionMaterial = new MaterialSelection();
		_selectionMaterialPaste = new MaterialSelection();
		_selectionMaterialPaste.fade = 0.5f;
		_cursor = new SpriteMap("cursors", 16, 16);
		_tileset = new SpriteMap("industrialTileset", 16, 16);
		_sideArrow = new Sprite("Editor/sideArrow");
		_sideArrow.CenterOrigin();
		_sideArrowHover = new Sprite("Editor/sideArrowHover");
		_sideArrowHover.CenterOrigin();
		_cantPlace = new Sprite("cantPlace");
		_cantPlace.CenterOrigin();
		_editorCurrency = new Sprite("editorCurrency");
		_die = new Sprite("die");
		_dieHover = new Sprite("dieHover");
		_singleBlock = new Sprite("Editor/singleplayerBlock");
		_multiBlock = new Sprite("Editor/multiplayerBlock");
		Layer.Background.camera.InitializeToScreenAspect();
		Layer.Game.camera.InitializeToScreenAspect();
		Layer.Game.camera.width *= 2f;
		Layer.Game.camera.height *= 2f;
		CalculateGridRestriction();
		EnterEditor();
		_camSize = new Vec2(base.camera.width, base.camera.height);
		_font = new BitmapFont("biosFont", 8);
		_input = InputProfile.Get(InputProfile.MPPlayer1);
		_tilePosition = new Vec2(0f, 0f);
		_tilePositionPrev = _tilePosition;
		_objectMenu = new PlacementMenu(0f, 0f);
		Level.Add(_objectMenu);
		ContextMenu objectMenu = _objectMenu;
		bool visible = (_objectMenu.active = false);
		objectMenu.visible = visible;
		Level.Add(new TileButton(0f, 0f, new FieldBinding(this, "_chance", 0f, 1f, 0.05f), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/dieBlock", 16, 16), "CHANCE - HOLD @SELECT@ AND MOVE @DPAD@", TileButtonAlign.TileGridBottomLeft));
		Level.Add(new TileButton(0f, 16f, new FieldBinding(this, "_maxPerLevel", -1f, 8f, 1f), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/numBlock", 16, 16), "MAX IN LEVEL - HOLD @SELECT@ AND MOVE @DPAD@", TileButtonAlign.TileGridBottomLeft));
		Level.Add(new TileButton(-16f, 0f, new FieldBinding(this, "_enableSingle"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/singleplayerBlock", 16, 16), "AVAILABLE IN SINGLE PLAYER - @SELECT@TOGGLE", TileButtonAlign.TileGridBottomRight));
		Level.Add(new TileButton(0f, 0f, new FieldBinding(this, "_enableMulti"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/multiplayerBlock", 16, 16), "AVAILABLE IN MULTI PLAYER - @SELECT@TOGGLE", TileButtonAlign.TileGridBottomRight));
		Level.Add(new TileButton(-16f, 16f, new FieldBinding(this, "_canMirror"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/canMirror", 16, 16), "TILE CAN BE MIRRORED - @SELECT@TOGGLE", TileButtonAlign.TileGridBottomRight));
		Level.Add(new TileButton(0f, 16f, new FieldBinding(this, "_isMirrored"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/isMirrored", 16, 16), "PRE MIRRORED TILE - @SELECT@TOGGLE", TileButtonAlign.TileGridBottomRight));
		Level.Add(new TileButton(0f, 32f, new FieldBinding(this, "editingOpenAirVariation"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/openAir", 16, 16), "OPEN AIR VARIATION - @SELECT@TOGGLE", TileButtonAlign.TileGridBottomRight));
		Level.Add(new TileButton(0f, 0f, new FieldBinding(this, "_pathEast"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/sideArrow", 32, 16), "CONNECTS EAST - @SELECT@TOGGLE", TileButtonAlign.TileGridRight, 90f));
		Level.Add(new TileButton(0f, 0f, new FieldBinding(this, "_pathWest"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/sideArrow", 32, 16), "CONNECTS WEST - @SELECT@TOGGLE", TileButtonAlign.TileGridLeft, -90f));
		Level.Add(new TileButton(0f, 0f, new FieldBinding(this, "_pathNorth"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/sideArrow", 32, 16), "CONNECTS NORTH - @SELECT@TOGGLE", TileButtonAlign.TileGridTop));
		Level.Add(new TileButton(0f, 0f, new FieldBinding(this, "_pathSouth"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/sideArrow", 32, 16), "CONNECTS SOUTH - @SELECT@TOGGLE", TileButtonAlign.TileGridBottom, 180f));
		Level.Add(new TileButton(16f, 0f, new FieldBinding(this, "_genTilePos", 0f, 6f, 1f), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/moveBlock", 16, 16), "MOVE GEN - HOLD @SELECT@ AND MOVE @DPAD@", TileButtonAlign.TileGridTopLeft));
		Level.Add(new TileButton(32f, 0f, new FieldBinding(this, "_editTilePos", 0f, 6f, 1f), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/editBlock", 16, 16), "MOVE GEN - HOLD @SELECT@ AND MOVE @DPAD@", TileButtonAlign.TileGridTopLeft));
		Level.Add(new TileButton(48f, 0f, new FieldBinding(this, "generatorComplexity", 0f, 9f, 1f), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/dieBlockRed", 16, 16), "NUM TILES - HOLD @SELECT@ AND MOVE @DPAD@", TileButtonAlign.TileGridTopLeft));
		Level.Add(new TileButton(0f, 0f, new FieldBinding(this, "_doGen"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/regenBlock", 16, 16), "REGENERATE - HOLD @SELECT@ AND MOVE @DPAD@", TileButtonAlign.TileGridTopRight));
		_notify = new NotifyDialogue();
		Level.Add(_notify);
		Vec2 buttonSizeAdd = new Vec2(12f, 12f);
		_touchButtons.Add(new EditorTouchButton
		{
			caption = "MENU",
			explanation = "Pick an object for placement...",
			state = EditorTouchState.OpenMenu,
			threeFingerGesture = true
		});
		_touchButtons.Add(new EditorTouchButton
		{
			caption = "COPY",
			explanation = "Pick an object to copy...",
			state = EditorTouchState.Eyedropper
		});
		_touchButtons.Add(new EditorTouchButton
		{
			caption = "EDIT",
			explanation = "Press objects to edit them!",
			state = EditorTouchState.EditObject
		});
		_cancelButton = new EditorTouchButton
		{
			caption = "CANCEL",
			explanation = "",
			state = EditorTouchState.Normal
		};
		_editTilesButton = new EditorTouchButton
		{
			caption = "PICK TILE",
			explanation = "",
			state = EditorTouchState.PickTile
		};
		_editTilesButton.size = new Vec2(Graphics.GetStringWidth(_editTilesButton.caption) + 6f, 15f) + buttonSizeAdd;
		_editTilesButton.position = Layer.HUD.camera.OffsetTL(10f, 10f);
		Vec2 buttonPlacement = Layer.HUD.camera.OffsetBR(-14f, -14f);
		for (int i = _touchButtons.Count - 1; i >= 0; i--)
		{
			EditorTouchButton button = _touchButtons[i];
			if (i == _touchButtons.Count - 1)
			{
				_cancelButton.size = new Vec2(Graphics.GetStringWidth(_cancelButton.caption) + 6f, 15f) + buttonSizeAdd;
				_cancelButton.position = buttonPlacement - _cancelButton.size;
			}
			button.size = new Vec2(Graphics.GetStringWidth(button.caption) + 6f, 15f) + buttonSizeAdd;
			button.position = buttonPlacement - button.size;
			buttonPlacement.x -= button.size.x + 4f;
		}
		_initialDirectory = DuckFile.levelDirectory;
		_initialDirectory = Path.GetFullPath(_initialDirectory);
		_fileDialog = new MonoFileDialog();
		Level.Add(_fileDialog);
		_uploadDialog = new SteamUploadDialog();
		Level.Add(_uploadDialog);
		_editorButtons = new SpriteMap("editorButtons", 32, 32);
		_doingResave = true;
		_doingResave = false;
		ClearEverything();
	}

	public Vec2 GetAlignOffset(TileButtonAlign align)
	{
		switch (align)
		{
		case TileButtonAlign.ProcGridTopLeft:
		{
			int reqW3 = 192;
			int reqH6 = 144;
			return new Vec2
			{
				x = -(_procTilesWide - (_procTilesWide - _procXPos)) * reqW3,
				y = -(_procTilesHigh - (_procTilesHigh - _procYPos)) * reqH6 - 16
			};
		}
		case TileButtonAlign.TileGridTopLeft:
			return new Vec2
			{
				x = 0f,
				y = -16f
			};
		case TileButtonAlign.TileGridTopRight:
		{
			int reqW2 = 192;
			return new Vec2
			{
				x = reqW2 - 16,
				y = -16f
			};
		}
		case TileButtonAlign.TileGridBottomLeft:
		{
			int reqH5 = 144;
			return new Vec2
			{
				x = 0f,
				y = reqH5
			};
		}
		case TileButtonAlign.TileGridBottomRight:
		{
			int reqH4 = 144;
			int reqW = 192;
			return new Vec2
			{
				x = reqW - 16,
				y = reqH4
			};
		}
		case TileButtonAlign.TileGridRight:
		{
			int reqH3 = 144;
			return new Vec2(192f, reqH3 / 2 - 8);
		}
		case TileButtonAlign.TileGridLeft:
		{
			int reqH2 = 144;
			return new Vec2(-16f, reqH2 / 2 - 8);
		}
		case TileButtonAlign.TileGridTop:
			return new Vec2(88f, -16f);
		case TileButtonAlign.TileGridBottom:
		{
			int reqH = 144;
			return new Vec2(88f, reqH);
		}
		default:
			return Vec2.Zero;
		}
	}

	private void Resave(string root)
	{
		foreach (string file in DuckFile.GetFilesNoCloud(root, "*.lev"))
		{
			try
			{
				LoadLevel(file);
				_things.RefreshState();
				_updateEvenWhenInactive = true;
				Update();
				_updateEvenWhenInactive = false;
				if (existingGUID.Contains(_currentLevelData.metaData.guid))
				{
					_currentLevelData.metaData.guid = Guid.NewGuid().ToString();
				}
				existingGUID.Add(_currentLevelData.metaData.guid);
				Save();
				Thread.Sleep(10);
			}
			catch (Exception)
			{
			}
		}
		foreach (string dir in DuckFile.GetDirectoriesNoCloud(root))
		{
			Resave(dir);
		}
	}

	public void ShowNoSpawnsDialogue()
	{
		if (_noSpawnsDialogue == null)
		{
			_noSpawnsDialogue = new MessageDialogue(null);
			Level.Add(_noSpawnsDialogue);
		}
		_noSpawnsDialogue.Open("NO SPAWNS", "", "Your level has no spawns.\n\n\n@_!DUCKSPAWN@\n\n\nPlease place a |DGBLUE|Spawns/Spawn Point|PREV|\n in your level.");
		lockInput = _noSpawnsDialogue;
		_noSpawnsDialogue.okayOnly = true;
		_noSpawnsDialogue.windowYOffsetAdd = -30f;
	}

	public void CompleteDialogue(ContextMenu pItem)
	{
	}

	public void OpenMenu(ContextMenu menu)
	{
		bool flag = (menu.visible = true);
		menu.active = flag;
		if (inputMode == EditorInput.Mouse)
		{
			menu.x = Mouse.x;
			menu.y = Mouse.y;
		}
		if (openPosition != Vec2.Zero)
		{
			menu.position = openPosition + new Vec2(-2f, -3f);
			openPosition = Vec2.Zero;
		}
		if (_showPlacementMenu)
		{
			menu.x = 96f;
			menu.y = 32f;
			_showPlacementMenu = false;
		}
		if (inputMode == EditorInput.Gamepad || inputMode == EditorInput.Touch)
		{
			menu.x = 16f;
			menu.y = 16f;
		}
		menu.opened = true;
		_placementMenu = menu;
		disableDragMode();
	}

	private Layer GetLayerOrOverride(Thing thingToCheck)
	{
		Layer layerResult = ((thingToCheck != null) ? thingToCheck.placementLayer : Layer.Game);
		if (thingToCheck != null && thingToCheck.placementLayerOverride != null)
		{
			layerResult = thingToCheck.placementLayerOverride;
		}
		else if (thingToCheck is AutoBlock)
		{
			layerResult = Layer.Blocks;
		}
		if (layerResult == null)
		{
			layerResult = Layer.Game;
		}
		return layerResult;
	}

	private void EndCurrentTouchMode()
	{
		if (!_showPlacementMenu)
		{
			_closeMenu = true;
		}
		_touchState = EditorTouchState.Normal;
		_activeTouchButton = null;
		clickedMenu = true;
		_editMode = false;
		_copyMode = false;
		_hover = null;
	}

	private void disableDragMode()
	{
		_dragMode = false;
		_deleteMode = false;
		if (_move != null)
		{
			_move = null;
		}
		dragModeInputType = InputType.eNone;
		History.EndUndoSection();
	}

	private void HugObjectPlacement()
	{
		if (_placementType is ItemSpawner)
		{
			(_placementType as ItemSpawner)._seated = false;
		}
		if ((_placementType.hugWalls & WallHug.Right) != WallHug.None && CollisionLine<IPlatform>(_tilePosition, _tilePosition + new Vec2(16f, 0f), _placementType) is Thing b && b.GetType() != _placementType.GetType())
		{
			_tilePosition.x = b.left - _placementType.collisionSize.x - _placementType.collisionOffset.x;
		}
		if ((_placementType.hugWalls & WallHug.Left) != WallHug.None && CollisionLine<IPlatform>(_tilePosition, _tilePosition + new Vec2(-16f, 0f), _placementType) is Thing b2 && b2.GetType() != _placementType.GetType())
		{
			_tilePosition.x = b2.right - _placementType.collisionOffset.x;
		}
		if ((_placementType.hugWalls & WallHug.Ceiling) != WallHug.None && CollisionLine<IPlatform>(_tilePosition, _tilePosition + new Vec2(0f, -16f), _placementType) is Thing b3 && b3.GetType() != _placementType.GetType())
		{
			_tilePosition.y = b3.bottom - _placementType.collisionOffset.y;
		}
		if ((_placementType.hugWalls & WallHug.Floor) != WallHug.None && CollisionLine<IPlatform>(_tilePosition, _tilePosition + new Vec2(0f, 16f), _placementType) is Thing b4 && b4.GetType() != _placementType.GetType())
		{
			_tilePosition.y = b4.top - _placementType.collisionSize.y - _placementType.collisionOffset.y;
			if (_placementType is ItemSpawner)
			{
				(_placementType as ItemSpawner)._seated = true;
			}
		}
	}

	public override void Update()
	{
		if (!Graphics.inFocus)
		{
			focusWait = 5;
			return;
		}
		if (focusWait > 0)
		{
			focusWait--;
			return;
		}
		MonoMain.timeInEditor++;
		tooltip = null;
		foreach (Thing thing in base.things)
		{
			thing.DoEditorUpdate();
		}
		if (lastMousePos == Vec2.Zero)
		{
			lastMousePos = Mouse.position;
		}
		if (clickedContextBackground)
		{
			clickedContextBackground = false;
			clickedMenu = true;
		}
		EditorInput num = inputMode;
		if (Mouse.left == InputState.Pressed || Mouse.right == InputState.Pressed || Mouse.middle == InputState.Pressed || (!fakeTouch && (lastMousePos - Mouse.position).length > 3f))
		{
			inputMode = ((!fakeTouch) ? EditorInput.Mouse : EditorInput.Touch);
		}
		else if (inputMode != EditorInput.Gamepad && InputProfile.active.Pressed("ANY", any: true))
		{
			if ((_selection.Count == 0 || !Keyboard.Pressed(Keys.F, any: true)) && !InputProfile.active.Pressed("RSTICK") && !InputProfile.active.Pressed("CANCEL") && !InputProfile.active.Pressed("MENU1") && !Keyboard.Down(Keys.LeftShift) && !Keyboard.Down(Keys.RightShift) && !Keyboard.Down(Keys.LeftControl) && !Keyboard.Down(Keys.RightControl))
			{
				if (inputMode == EditorInput.Mouse)
				{
					_tilePosition = Maths.Snap(Mouse.positionScreen + new Vec2(8f, 8f), 16f, 16f);
					_tilePositionPrev = _tilePosition;
				}
				inputMode = EditorInput.Gamepad;
			}
		}
		else if (TouchScreen.IsScreenTouched())
		{
			inputMode = EditorInput.Touch;
		}
		if (inputMode == EditorInput.Mouse)
		{
			_input.lastActiveDevice = Input.GetDevice<Keyboard>();
		}
		if (num != inputMode && inputMode == EditorInput.Touch)
		{
			clickedMenu = true;
			waitForNoTouchInput = true;
			return;
		}
		if (waitForNoTouchInput && TouchScreen.GetTouches().Count > 0)
		{
			clickedMenu = true;
			return;
		}
		waitForNoTouchInput = false;
		lastMousePos = Mouse.position;
		if (_editingOpenAirVariationPrev != editingOpenAirVariation)
		{
			if (editingOpenAirVariation)
			{
				foreach (Thing t in _levelThingsNormal)
				{
					Level.current.RemoveThing(t);
				}
				foreach (Thing t2 in _levelThingsAlternate)
				{
					Level.current.AddThing(t2);
				}
			}
			else
			{
				foreach (Thing t3 in _levelThingsAlternate)
				{
					Level.current.RemoveThing(t3);
				}
				foreach (Thing t4 in _levelThingsNormal)
				{
					Level.current.AddThing(t4);
				}
			}
			_editingOpenAirVariationPrev = editingOpenAirVariation;
		}
		if (inputMode == EditorInput.Touch)
		{
			if (_placementMenu == null)
			{
				_objectMenuLayer.camera.width = Layer.HUD.width * 0.75f;
				_objectMenuLayer.camera.height = Layer.HUD.height * 0.75f;
				bigInterfaceMode = true;
			}
			if (_fileDialog.opened && _touchState != EditorTouchState.OpenLevel)
			{
				EndCurrentTouchMode();
				_touchState = EditorTouchState.OpenLevel;
			}
			else if (!_fileDialog.opened && _touchState == EditorTouchState.OpenLevel)
			{
				EndCurrentTouchMode();
			}
			Touch touch = TouchScreen.GetTap();
			if (_touchState == EditorTouchState.Normal)
			{
				_activeTouchButton = null;
				foreach (EditorTouchButton t5 in _touchButtons)
				{
					if ((touch != Touch.None && touch.positionHUD.x > t5.position.x && touch.positionHUD.x < t5.position.x + t5.size.x && touch.positionHUD.y > t5.position.y && touch.positionHUD.y < t5.position.y + t5.size.y) || (t5.threeFingerGesture && _threeFingerGesture))
					{
						_touchState = t5.state;
						_activeTouchButton = t5;
						clickedMenu = true;
						_threeFingerGesture = false;
						_clickedTouchButton = true;
						SFX.Play("highClick", 0.3f, 0.2f);
					}
				}
			}
			else if ((touch.positionHUD.x > _cancelButton.position.x && touch.positionHUD.x < _cancelButton.position.x + _cancelButton.size.x && touch.positionHUD.y > _cancelButton.position.y && touch.positionHUD.y < _cancelButton.position.y + _cancelButton.size.y) || (_activeTouchButton != null && _activeTouchButton.threeFingerGesture && _threeFingerGesture) || (_activeTouchButton != null && !_activeTouchButton.threeFingerGesture && _threeFingerGesture) || (_activeTouchButton != null && _activeTouchButton.threeFingerGesture && _twoFingerGesture))
			{
				EndCurrentTouchMode();
				if (_fileDialog.opened)
				{
					_fileDialog.Close();
				}
				SFX.Play("highClick", 0.3f, 0.2f);
				return;
			}
			if (_placingTiles && _placementMenu == null && touch.positionHUD.x > _editTilesButton.position.x && touch.positionHUD.x < _editTilesButton.position.x + _editTilesButton.size.x && touch.positionHUD.y > _editTilesButton.position.y && touch.positionHUD.y < _editTilesButton.position.y + _editTilesButton.size.y)
			{
				_openTileSelector = true;
				clickedMenu = true;
			}
			if (_touchState == EditorTouchState.OpenMenu)
			{
				if (_placementMenu == null)
				{
					_showPlacementMenu = true;
				}
				EndCurrentTouchMode();
			}
			else if (_touchState == EditorTouchState.EditObject)
			{
				_editMode = true;
			}
			else if (_touchState == EditorTouchState.Eyedropper)
			{
				_copyMode = true;
			}
		}
		else
		{
			_editMode = false;
			_copyMode = false;
			_activeTouchButton = null;
			_touchState = EditorTouchState.Normal;
			if (_placementMenu == null)
			{
				_objectMenuLayer.camera.width = Layer.HUD.width;
				_objectMenuLayer.camera.height = Layer.HUD.height;
				bigInterfaceMode = false;
				pretendPinned = null;
			}
		}
		if (!Graphics.inFocus && !_updateEvenWhenInactive)
		{
			_tileDragDif = Vec2.MaxValue;
		}
		else if (clickedMenu)
		{
			clickedMenu = false;
		}
		else
		{
			if (_notify.opened)
			{
				return;
			}
			if (reopenContextMenu)
			{
				bool num2 = ignorePinning;
				reopenContextMenu = false;
				if (_placementMenu != null)
				{
					_placementMenu.opened = false;
				}
				ignorePinning = num2;
				if (_placementMenu == null)
				{
					_placementMenu = _objectMenu;
				}
				OpenMenu(_placementMenu);
				if (openContextThing != null)
				{
					_placementMenu.OpenInto(openContextThing);
				}
				openContextThing = null;
				SFX.Play("openClick", 0.4f);
			}
			hoverTextBox = false;
			if (numPops > 0)
			{
				for (int i = 0; i < numPops; i++)
				{
					if (focusStack.Count == 0)
					{
						break;
					}
					focusStack.Pop();
				}
				numPops = 0;
			}
			if (tookInput)
			{
				tookInput = false;
				return;
			}
			if (focusStack.Count > 0 || skipFrame)
			{
				skipFrame = false;
				return;
			}
			if (_placementMenu != null)
			{
				_placementMenu.visible = _lockInput == null;
			}
			if (lockInput != null)
			{
				if (_lockInputChange != lockInput)
				{
					_lockInput = _lockInputChange;
				}
				return;
			}
			if (_lockInputChange != lockInput)
			{
				_lockInput = _lockInputChange;
			}
			if (Keyboard.Pressed(Keys.OemComma))
			{
				searching = true;
			}
			if (searching)
			{
				Input._imeAllowed = true;
				if (!clearedKeyboardStringForSearch)
				{
					clearedKeyboardStringForSearch = true;
					Keyboard.keyString = "";
				}
				if (searchItems != null && searchItems.Count > 0)
				{
					if (Keyboard.Pressed(Keys.Down))
					{
						if (_searchHoverIndex == 0)
						{
							_searchHoverIndex = Math.Min(searchItems.Count - 1, 9);
						}
						else
						{
							_searchHoverIndex--;
						}
						if (_searchHoverIndex < 0)
						{
							_searchHoverIndex = 0;
						}
					}
					else if (Keyboard.Pressed(Keys.Up))
					{
						if (_searchHoverIndex < 0)
						{
							_searchHoverIndex = 0;
						}
						else
						{
							_searchHoverIndex++;
						}
						if (_searchHoverIndex > Math.Min(searchItems.Count - 1, 9))
						{
							_searchHoverIndex = 0;
						}
					}
					_searchHoverIndex = Math.Min(searchItems.Count - 1, _searchHoverIndex);
				}
				else
				{
					_searchHoverIndex = -1;
				}
				bool enter = Mouse.left == InputState.Pressed || Keyboard.Pressed(Keys.Enter);
				if (Mouse.right == InputState.Released || Mouse.middle == InputState.Pressed || Keyboard.Pressed(Keys.Escape) || enter)
				{
					if (enter && _searchHoverIndex != -1 && _searchHoverIndex < searchItems.Count)
					{
						_placementType = searchItems[_searchHoverIndex].thing.thing;
						_eyeDropperSerialized = null;
					}
					searching = false;
					clearedKeyboardStringForSearch = false;
					searchItems = null;
					_searchHoverIndex = -1;
				}
				if (_prevSearchString != Keyboard.keyString)
				{
					searchItems = _objectMenu.Search(Keyboard.keyString);
					_prevSearchString = Keyboard.keyString;
				}
				if (_placementMenu == null)
				{
					return;
				}
				CloseMenu();
			}
			if (Keyboard.control && Keyboard.Pressed(Keys.S))
			{
				if (Keyboard.shift)
				{
					SaveAs();
				}
				else
				{
					Save();
				}
			}
			if (_onlineSettingChanged && _placementMenu != null && _placementMenu is EditorGroupMenu)
			{
				(_placementMenu as EditorGroupMenu).UpdateGrayout();
				_onlineSettingChanged = false;
			}
			Graphics.fade = Lerp.Float(Graphics.fade, _quitting ? 0f : 1f, 0.02f);
			if (_quitting && Graphics.fade < 0.01f)
			{
				_quitting = false;
				active = false;
				Level.current = new TitleScreen();
			}
			if (Graphics.fade < 0.95f)
			{
				return;
			}
			Layer placementLayer = GetLayerOrOverride(_placementType);
			if (inputMode == EditorInput.Mouse)
			{
				clicked = Mouse.left == InputState.Pressed;
				if (Mouse.middle == InputState.Pressed)
				{
					middleClickPos = Mouse.position;
				}
			}
			else if (inputMode == EditorInput.Touch)
			{
				clicked = TouchScreen.GetTap() != Touch.None;
			}
			if (_cursorMode == CursorMode.Normal && (Keyboard.Down(Keys.RightShift) || Keyboard.Down(Keys.LeftShift)))
			{
				Vec2 offset = new Vec2(0f, 0f);
				if (Keyboard.Pressed(Keys.Up))
				{
					offset.y -= 16f;
				}
				if (Keyboard.Pressed(Keys.Down))
				{
					offset.y += 16f;
				}
				if (Keyboard.Pressed(Keys.Left))
				{
					offset.x -= 16f;
				}
				if (Keyboard.Pressed(Keys.Right))
				{
					offset.x += 16f;
				}
				if (offset != Vec2.Zero)
				{
					foreach (Thing t6 in Level.current.things)
					{
						t6.position += offset;
						if (t6 is IDontMove)
						{
							Level.current.things.quadTree.Remove(t6);
							Level.current.things.quadTree.Add(t6);
						}
					}
				}
			}
			_menuOpen = false;
			if (!_editMode)
			{
				foreach (ContextMenu m in base.things[typeof(ContextMenu)])
				{
					if (m.visible && m.opened)
					{
						clicked = false;
						_menuOpen = true;
					}
				}
			}
			if (inputMode == EditorInput.Gamepad)
			{
				_input = InputProfile.active;
			}
			if (_prevEditTilePos != _editTilePos)
			{
				if (_editTilePos.x < 0f)
				{
					_editTilePos.x = 0f;
				}
				if (_editTilePos.x >= (float)_procTilesWide)
				{
					_editTilePos.x = _procTilesWide - 1;
				}
				if (_editTilePos.y < 0f)
				{
					_editTilePos.y = 0f;
				}
				if (_editTilePos.y >= (float)_procTilesHigh)
				{
					_editTilePos.y = _procTilesHigh - 1;
				}
				if (_currentMapNode != null)
				{
					RandomLevelData dat = _currentMapNode.map[(int)_editTilePos.x, (int)_editTilePos.y].data;
					if (_levelThings.Count > 0)
					{
						Save();
					}
					_looseClear = true;
					if (dat == null)
					{
						ClearEverything();
						_saveName = "";
					}
					else
					{
						string dir = Directory.GetCurrentDirectory() + "\\..\\..\\..\\assets\\levels\\" + dat.file + ".lev";
						LoadLevel(dir);
					}
					_procXPos = (int)_editTilePos.x;
					_procYPos = (int)_editTilePos.y;
					_genTilePos = new Vec2(_procXPos, _procYPos);
					_prevEditTilePos = _editTilePos;
					int reqH = 144;
					int reqW = 192;
					_procDrawOffset += new Vec2((_procXPos - _prevProcX) * reqW, (_procYPos - _prevProcY) * reqH);
					_prevProcX = _procXPos;
					_prevProcY = _procYPos;
				}
			}
			if (_procXPos != _prevProcX)
			{
				_doGen = true;
			}
			else if (_procYPos != _prevProcY)
			{
				_doGen = true;
			}
			_prevEditTilePos = _editTilePos;
			_prevProcX = _procXPos;
			_prevProcY = _procYPos;
			if (_miniMode && (Keyboard.Pressed(Keys.F1) || _doGen) && !_doingResave)
			{
				if (_saveName == "")
				{
					_saveName = _initialDirectory + "/pyramid/" + Guid.NewGuid().ToString() + ".lev";
				}
				LevelGenerator.ReInitialize();
				LevelGenerator.complexity = generatorComplexity;
				if (!Keyboard.Down(Keys.RightShift) && !Keyboard.Down(Keys.LeftShift))
				{
					_procSeed = Rando.Int(2147483646);
				}
				string newName = _saveName.Substring(_saveName.LastIndexOf("assets/levels/") + "assets/levels/".Length);
				newName = newName.Substring(0, newName.Length - 4);
				RandomLevelData centerTile = LevelGenerator.LoadInTile(SaveTempVersion(), newName);
				_loadPosX = _procXPos;
				_loadPosY = _procYPos;
				LevGenType genType = LevGenType.Any;
				if (_currentLevelData.proceduralData.enableSingle && !_currentLevelData.proceduralData.enableMulti)
				{
					genType = LevGenType.SinglePlayer;
				}
				else if (!_currentLevelData.proceduralData.enableSingle && _currentLevelData.proceduralData.enableMulti)
				{
					genType = LevGenType.Deathmatch;
				}
				_editTilePos = (_prevEditTilePos = _genTilePos);
				int tries = 0;
				Level lev;
				while (true)
				{
					_currentMapNode = LevelGenerator.MakeLevel(centerTile, (_pathEast && _pathWest) ? true : false, _procSeed, genType, _procTilesWide, _procTilesHigh, _loadPosX, _loadPosY);
					_procDrawOffset = new Vec2(0f, 0f);
					_procContext = new GameContext();
					_procContext.ApplyStates();
					lev = new Level();
					lev.backgroundColor = new Color(0, 0, 0, 0);
					Level.core.currentLevel = lev;
					RandomLevelNode.editorLoad = true;
					bool num3 = _currentMapNode.LoadParts(0f, 0f, lev, _procSeed);
					RandomLevelNode.editorLoad = false;
					if (num3 || tries > 100)
					{
						break;
					}
					tries++;
				}
				lev.CalculateBounds();
				_procContext.RevertStates();
				_doGen = false;
			}
			_looseClear = false;
			if (inputMode == EditorInput.Touch)
			{
				if (!_twoFingerGestureStarting && TouchScreen.GetTouches().Count == 2)
				{
					_twoFingerGestureStarting = true;
					_panAnchor = TouchScreen.GetAverageOfTouches().positionHUD;
					_twoFingerSpacing = (TouchScreen.GetTouches()[0].positionHUD - TouchScreen.GetTouches()[1].positionHUD).length;
				}
				else if (TouchScreen.GetTouches().Count != 2)
				{
					_twoFingerGesture = false;
					_twoFingerGestureStarting = false;
				}
				if (_twoFingerGestureStarting && TouchScreen.GetTouches().Count == 2 && !_twoFingerGesture)
				{
					if ((_panAnchor - TouchScreen.GetAverageOfTouches().positionHUD).length > 6f)
					{
						_twoFingerZooming = false;
						_twoFingerGesture = true;
					}
					else if (Math.Abs(_twoFingerSpacing - (TouchScreen.GetTouches()[0].positionHUD - TouchScreen.GetTouches()[1].positionHUD).length) > 4f)
					{
						_twoFingerZooming = true;
						_twoFingerGesture = true;
					}
				}
				if (!_threeFingerGestureRelease && TouchScreen.GetTouches().Count == 3)
				{
					_threeFingerGesture = true;
					_threeFingerGestureRelease = true;
				}
				else if (TouchScreen.GetTouches().Count != 3)
				{
					_threeFingerGesture = false;
					_threeFingerGestureRelease = false;
				}
			}
			if (inputMode == EditorInput.Mouse && Mouse.middle == InputState.Pressed)
			{
				_panAnchor = Mouse.position;
			}
			if (_procContext != null)
			{
				_procContext.Update();
			}
			if (tabletMode && clicked)
			{
				if (Mouse.x < 32f && Mouse.y < 32f)
				{
					_placementMode = true;
					_editMode = false;
					clicked = false;
					return;
				}
				if (Mouse.x < 64f && Mouse.y < 32f)
				{
					_placementMode = false;
					_editMode = true;
					clicked = false;
					return;
				}
				if (Mouse.x < 96f && Mouse.y < 32f)
				{
					if (_placementMenu == null)
					{
						_showPlacementMenu = true;
					}
					else
					{
						CloseMenu();
					}
					clicked = false;
					return;
				}
			}
			if (_editorLoadFinished)
			{
				foreach (Thing levelThing in _levelThings)
				{
					levelThing.OnEditorLoaded();
				}
				foreach (PathNode item in base.things[typeof(PathNode)])
				{
					item.UninitializeLinks();
					item.Update();
				}
				_editorLoadFinished = false;
			}
			base.things.RefreshState();
			if (_placeObjects.Count > 0)
			{
				foreach (Thing t7 in _placeObjects)
				{
					foreach (Thing item2 in Level.CheckRectAll<IDontMove>(t7.topLeft + new Vec2(-16f, -16f), t7.bottomRight + new Vec2(16f, 16f)))
					{
						item2.EditorObjectsChanged();
					}
				}
				base.things.CleanAddList();
				_placeObjects.Clear();
			}
			if (_placementMenu != null && inputMode == EditorInput.Mouse && Mouse.right == InputState.Released)
			{
				_placementMenu.Disappear();
				CloseMenu();
			}
			if (Keyboard.Down(Keys.LeftControl) || Keyboard.Down(Keys.RightControl))
			{
				bool mod = false;
				if (Keyboard.Down(Keys.LeftShift) || Keyboard.Down(Keys.RightShift))
				{
					mod = true;
				}
				if (Keyboard.Pressed(Keys.Z))
				{
					if (mod)
					{
						History.Redo();
					}
					else
					{
						History.Undo();
					}
					_selection.Clear();
					_currentDragSelectionHover.Clear();
					foreach (Thing levelThing2 in _levelThings)
					{
						levelThing2.EditorObjectsChanged();
					}
				}
			}
			if (inputMode == EditorInput.Gamepad && _placementMenu == null)
			{
				if (_input.Pressed("STRAFE"))
				{
					History.Undo();
					_selection.Clear();
					_currentDragSelectionHover.Clear();
					foreach (Thing levelThing3 in _levelThings)
					{
						levelThing3.EditorObjectsChanged();
					}
				}
				if (_input.Pressed("RAGDOLL"))
				{
					History.Redo();
					_selection.Clear();
					_currentDragSelectionHover.Clear();
					foreach (Thing levelThing4 in _levelThings)
					{
						levelThing4.EditorObjectsChanged();
					}
				}
			}
			if ((_input.Pressed("MENU2") || _showPlacementMenu) && _cursorMode == CursorMode.Normal)
			{
				if (_placementMenu == null)
				{
					_placementMenu = _objectMenu;
					OpenMenu(_placementMenu);
					SFX.Play("openClick", 0.4f);
				}
				else
				{
					CloseMenu();
				}
			}
			if (_clickedTouchButton)
			{
				_clickedTouchButton = false;
				return;
			}
			if (_placementType is AutoBlock || _placementType is PipeTileset)
			{
				cellSize = 16f;
			}
			if (_cursorMode != CursorMode.Selection && _placementMenu == null)
			{
				if (inputMode == EditorInput.Gamepad)
				{
					if (_input.Pressed("CANCEL"))
					{
						_selectionDragStart = _tilePosition;
					}
					if (_selectionDragStart != Vec2.Zero && (_selectionDragStart - _tilePosition).length > 4f)
					{
						_dragSelectShiftModifier = _selection.Count != 0;
						_cursorMode = CursorMode.Selection;
						_selectionDragEnd = _tilePosition;
						return;
					}
					if (_input.Released("CANCEL"))
					{
						_selectionDragStart = Vec2.Zero;
					}
				}
				else if (inputMode == EditorInput.Mouse)
				{
					bool specialDrag = Mouse.left == InputState.Pressed && _dragSelectShiftModifier;
					if (_placementMenu == null && (Mouse.right == InputState.Pressed || specialDrag))
					{
						_selectionDragStart = Mouse.positionScreen;
					}
					if (_dragSelectShiftModifier && (Mouse.right == InputState.Released || Mouse.left == InputState.Released))
					{
						if (_hover != null)
						{
							_selection.Add(_hover);
							_currentDragSelectionHover.Add(_hover);
						}
						if (_secondaryHover != null)
						{
							_selection.Add(_secondaryHover);
							_currentDragSelectionHover.Add(_secondaryHover);
						}
						UpdateSelection(pObjectsChanged: false);
						_selectionDragStart = Vec2.Zero;
						if (_selection.Count > 0)
						{
							_cursorMode = CursorMode.HasSelection;
							return;
						}
					}
					if (_selectionDragStart != Vec2.Zero && (_selectionDragStart - Mouse.positionScreen).length > 8f)
					{
						if (!_dragSelectShiftModifier)
						{
							_selection.Clear();
							_currentDragSelectionHover.Clear();
						}
						_cursorMode = CursorMode.Selection;
						_selectionDragEnd = Mouse.positionScreen;
						return;
					}
					if (Mouse.right == InputState.Released || Mouse.left == InputState.Released)
					{
						_selectionDragStart = Vec2.Zero;
					}
				}
			}
			if ((_placementMenu == null || _editMode) && _hoverMode == 0)
			{
				UpdateHover(placementLayer, _tilePosition);
				bool middleClick = false;
				if (inputMode == EditorInput.Mouse && Mouse.middle == InputState.Released && (middleClickPos - Mouse.position).length < 2f)
				{
					middleClick = true;
				}
				Thing contextBrowseObject = null;
				if (_secondaryHover != null)
				{
					if (Input.Released("CANCEL") || middleClick)
					{
						copying = true;
						_eyeDropperSerialized = _secondaryHover.Serialize();
						copying = false;
						_placementType = Thing.LoadThing(_eyeDropperSerialized);
					}
					else if (Input.Pressed("START"))
					{
						contextBrowseObject = _secondaryHover;
					}
				}
				else if (_hover != null)
				{
					if (_copyMode || Input.Released("CANCEL") || middleClick)
					{
						copying = true;
						_eyeDropperSerialized = _hover.Serialize();
						copying = false;
						_placementType = Thing.LoadThing(_eyeDropperSerialized);
						if (inputMode == EditorInput.Touch)
						{
							EndCurrentTouchMode();
							return;
						}
					}
					else if (Input.Pressed("START"))
					{
						contextBrowseObject = _hover;
					}
				}
				else if (_placementType != null && Input.Pressed("START"))
				{
					contextBrowseObject = _placementType;
				}
				if (contextBrowseObject != null)
				{
					ignorePinning = true;
					reopenContextMenu = true;
					openContextThing = contextBrowseObject;
				}
				TileButton tile = CollisionPoint<TileButton>(_tilePosition);
				if (tile != null)
				{
					if (!tile.visible)
					{
						tile = null;
					}
					else
					{
						tile.hover = true;
						if (inputMode == EditorInput.Mouse)
						{
							hoverMiniButton = true;
						}
						if ((inputMode == EditorInput.Gamepad && _input.Down("SELECT")) || (inputMode == EditorInput.Mouse && (Mouse.left == InputState.Down || Mouse.left == InputState.Pressed)) || (inputMode == EditorInput.Touch && TouchScreen.IsScreenTouched()))
						{
							tile.focus = _input;
						}
						else
						{
							tile.focus = null;
						}
					}
				}
				if (tile != _hoverButton && _hoverButton != null)
				{
					_hoverButton.focus = null;
				}
				_hoverButton = tile;
			}
			if (inputMode == EditorInput.Mouse)
			{
				_ = Mouse.right;
				_ = 2;
			}
			if (_cursorMode == CursorMode.Normal)
			{
				if (_hoverMenu != null && !_placingTiles && ((inputMode == EditorInput.Mouse && Mouse.right == InputState.Released) || (_input.Pressed("MENU1") && !_input.Down("SELECT"))))
				{
					if (_placementMenu == null)
					{
						if (_hover != null)
						{
							_placementMenu = _hover.GetContextMenu();
							if (_placementMenu != null)
							{
								AddThing(_placementMenu);
							}
						}
						else if (_secondaryHover != null)
						{
							_placementMenu = _secondaryHover.GetContextMenu();
							if (_placementMenu != null)
							{
								AddThing(_placementMenu);
							}
						}
						if (_placementMenu != null)
						{
							OpenMenu(_placementMenu);
							SFX.Play("openClick", 0.4f);
						}
					}
					else if (inputMode == EditorInput.Mouse && Mouse.right == InputState.Pressed)
					{
						CloseMenu();
					}
				}
				if (hoverMiniButton)
				{
					_tilePosition.x = (float)Math.Round(Mouse.positionScreen.x / _cellSize) * _cellSize;
					_tilePosition.y = (float)Math.Round(Mouse.positionScreen.y / _cellSize) * _cellSize;
					hoverMiniButton = false;
					return;
				}
				if (_hoverMenu == null && inputMode == EditorInput.Mouse && Mouse.right == InputState.Released)
				{
					if (_hover is BackgroundTile)
					{
						if (_placingTiles && _placementMenu == null)
						{
							int frame = _placementType.frame;
							_placementMenu = new ContextBackgroundTile(_placementType, null, placement: false);
							_placementMenu.opened = true;
							SFX.Play("openClick", 0.4f);
							_placementMenu.x = 16f;
							_placementMenu.y = 16f;
							_placementMenu.selectedIndex = frame;
							Level.Add(_placementMenu);
						}
					}
					else if (_placementMenu == null)
					{
						_placementMenu = _objectMenu;
						OpenMenu(_placementMenu);
						SFX.Play("openClick", 0.4f);
					}
					else
					{
						CloseMenu();
					}
				}
			}
			if (_cursorMode == CursorMode.Normal)
			{
				if (inputMode == EditorInput.Gamepad && _input.Pressed("CANCEL") && _placementMenu != null)
				{
					CloseMenu();
				}
				if (_placementType != null && _objectMenu != null)
				{
					if (_placementType._canFlip || _placementType.editorCycleType != null)
					{
						rotateValid = true;
					}
					else
					{
						rotateValid = false;
					}
					if (_input.Pressed("RSTICK") || Keyboard.Pressed(Keys.Tab))
					{
						if (_placementType.editorCycleType != null)
						{
							_placementType = _objectMenu.GetPlacementType(_placementType.editorCycleType);
							_eyeDropperSerialized = null;
						}
						else
						{
							Thing newThing = null;
							newThing = ((_eyeDropperSerialized != null) ? Thing.LoadThing(_eyeDropperSerialized) : CreateThing(_placementType.GetType()));
							newThing.TabRotate();
							_placementType = newThing;
							_eyeDropperSerialized = newThing.Serialize();
						}
					}
				}
			}
			float scroll = 0f;
			if (inputMode == EditorInput.Mouse)
			{
				scroll = Mouse.scroll;
			}
			else if (inputMode == EditorInput.Touch && _twoFingerGesture && _twoFingerZooming)
			{
				float newSpacing = (TouchScreen.GetTouches()[0].positionHUD - TouchScreen.GetTouches()[1].positionHUD).length;
				if (Math.Abs(newSpacing - _twoFingerSpacing) > 2f)
				{
					scroll = (0f - (newSpacing - _twoFingerSpacing)) * 1f;
				}
				_twoFingerSpacing = newSpacing;
			}
			if (inputMode == EditorInput.Gamepad)
			{
				scroll = _input.leftTrigger - _input.rightTrigger;
				float speed = base.camera.width / (float)MonoMain.screenWidth * 5f;
				if (_input.Down("LSTICK"))
				{
					speed *= 2f;
				}
				if (_input.Pressed("LOPTION"))
				{
					if (cellSize < 10f)
					{
						cellSize = 16f;
					}
					else
					{
						cellSize = 8f;
					}
				}
				if (speed < 5f)
				{
					speed = 5f;
				}
				base.camera.x += _input.rightStick.x * speed;
				base.camera.y -= _input.rightStick.y * speed;
			}
			if (scroll != 0f && !didUIScroll && !hoverUI)
			{
				int num4 = Math.Sign(scroll);
				_ = base.camera.height / base.camera.width;
				float inc = (float)num4 * 64f;
				if (inputMode == EditorInput.Gamepad)
				{
					inc = scroll * 32f;
				}
				else if (inputMode == EditorInput.Touch)
				{
					inc = scroll;
				}
				Vec2 prevSize = new Vec2(base.camera.width, base.camera.height);
				Vec2 mouse = base.camera.transformScreenVector(Mouse.mousePos);
				if (inputMode == EditorInput.Touch && _twoFingerGesture)
				{
					mouse = TouchScreen.GetAverageOfTouches().positionCamera;
				}
				if (inputMode == EditorInput.Gamepad)
				{
					mouse = _tilePosition;
				}
				base.camera.width += inc;
				if (base.camera.width < 64f)
				{
					base.camera.width = 64f;
				}
				base.camera.height = base.camera.width / Resolution.current.aspect;
				Vec2 camPos = base.camera.position;
				(Matrix.CreateTranslation(new Vec3(camPos.x, camPos.y, 0f)) * Matrix.CreateTranslation(new Vec3(0f - mouse.x, 0f - mouse.y, 0f)) * Matrix.CreateScale(base.camera.width / prevSize.x, base.camera.height / prevSize.y, 1f) * Matrix.CreateTranslation(new Vec3(mouse.x, mouse.y, 0f))).Decompose(out var _, out var _, out var translation);
				base.camera.position = new Vec2(translation.x, translation.y);
			}
			didUIScroll = false;
			if (inputMode == EditorInput.Mouse)
			{
				if (Mouse.middle == InputState.Pressed)
				{
					_panAnchor = Mouse.position;
				}
				if (Mouse.middle == InputState.Down)
				{
					Vec2 dif = Mouse.position - _panAnchor;
					_panAnchor = Mouse.position;
					float mult = base.camera.width / Layer.HUD.width;
					if ((double)dif.length > 0.01)
					{
						_didPan = true;
					}
					base.camera.x -= dif.x * mult;
					base.camera.y -= dif.y * mult;
				}
				if (Mouse.middle == InputState.Released)
				{
					_ = _didPan;
					_didPan = false;
				}
			}
			else if (inputMode == EditorInput.Touch && _twoFingerGesture && !_twoFingerZooming)
			{
				Vec2 dif2 = TouchScreen.GetAverageOfTouches().positionHUD - _panAnchor;
				_panAnchor = TouchScreen.GetAverageOfTouches().positionHUD;
				float mult2 = base.camera.width / Layer.HUD.width;
				if ((double)dif2.length > 0.1)
				{
					_didPan = true;
					base.camera.x -= dif2.x * mult2;
					base.camera.y -= dif2.y * mult2;
				}
			}
			bool alt = Keyboard.Down(Keys.LeftAlt) || Keyboard.Down(Keys.RightAlt);
			bool ctrl = Keyboard.Down(Keys.LeftControl) || Keyboard.Down(Keys.RightControl);
			bool preciseMode = false;
			if (alt && ctrl)
			{
				_hover = null;
				_secondaryHover = null;
				preciseMode = true;
			}
			if ((inputMode == EditorInput.Gamepad || inputMode == EditorInput.Touch) && _placementMenu == null)
			{
				int mul = 1;
				if (_input.Down("LSTICK"))
				{
					mul = 4;
				}
				_tilePosition = _tilePositionPrev;
				if (_tilePosition.x < base.camera.left)
				{
					_tilePosition.x = base.camera.left + 32f;
				}
				if (_tilePosition.x > base.camera.right)
				{
					_tilePosition.x = base.camera.right - 32f;
				}
				if (_tilePosition.y < base.camera.top)
				{
					_tilePosition.y = base.camera.top + 32f;
				}
				if (_tilePosition.y > base.camera.bottom)
				{
					_tilePosition.y = base.camera.bottom - 32f;
				}
				int vertical = 0;
				int horizontal = 0;
				if (_hoverMode == 0 && (_hoverButton == null || _hoverButton.focus == null))
				{
					if (_input.Pressed("MENULEFT"))
					{
						horizontal = -1;
					}
					if (_input.Pressed("MENURIGHT"))
					{
						horizontal = 1;
					}
					if (_input.Pressed("MENUUP"))
					{
						vertical = -1;
					}
					if (_input.Pressed("MENUDOWN"))
					{
						vertical = 1;
					}
				}
				float horizontalMove = _cellSize * (float)mul * (float)horizontal;
				float verticalMove = _cellSize * (float)mul * (float)vertical;
				_tilePosition.x += horizontalMove;
				_tilePosition.y += verticalMove;
				if (_tilePosition.x < base.camera.left || _tilePosition.x > base.camera.right)
				{
					base.camera.x += horizontalMove;
				}
				if (_tilePosition.y < base.camera.top || _tilePosition.y > base.camera.bottom)
				{
					base.camera.y += verticalMove;
				}
				if (TouchScreen.GetTouch() != Touch.None)
				{
					_tilePosition.x = (float)Math.Round(TouchScreen.GetTouch().positionCamera.x / _cellSize) * _cellSize;
					_tilePosition.y = (float)Math.Round(TouchScreen.GetTouch().positionCamera.y / _cellSize) * _cellSize;
					_tilePositionPrev = _tilePosition;
				}
				else
				{
					_tilePosition.x = (float)Math.Round(_tilePosition.x / _cellSize) * _cellSize;
					_tilePosition.y = (float)Math.Round(_tilePosition.y / _cellSize) * _cellSize;
					_tilePositionPrev = _tilePosition;
				}
			}
			else if (inputMode == EditorInput.Mouse)
			{
				if (alt)
				{
					_tilePosition.x = (float)Math.Round(Mouse.positionScreen.x / 1f) * 1f;
					_tilePosition.y = (float)Math.Round(Mouse.positionScreen.y / 1f) * 1f;
				}
				else
				{
					_tilePosition.x = (float)Math.Round(Mouse.positionScreen.x / _cellSize) * _cellSize;
					_tilePosition.y = (float)Math.Round(Mouse.positionScreen.y / _cellSize) * _cellSize;
				}
			}
			if (_placementType != null && _placementMenu == null)
			{
				_tilePosition += _placementType.editorOffset;
				if (!alt)
				{
					HugObjectPlacement();
				}
			}
			if (_move != null)
			{
				_move.position = new Vec2(_tilePosition);
			}
			UpdateDragSelection();
			if (!_editMode && !_copyMode && _cursorMode == CursorMode.Normal && !_dragSelectShiftModifier && _placementMenu == null)
			{
				bMouseInput = false;
				bGamepadInput = false;
				bTouchInput = false;
				if (inputMode == EditorInput.Mouse && Mouse.left == InputState.Pressed)
				{
					bMouseInput = true;
					dragModeInputType = InputType.eMouse;
				}
				else if (inputMode == EditorInput.Gamepad && _input.Pressed("SELECT"))
				{
					bGamepadInput = true;
					dragModeInputType = InputType.eGamepad;
				}
				else if ((inputMode == EditorInput.Touch && TouchScreen.GetDrag() != Touch.None) || TouchScreen.GetTap() != Touch.None)
				{
					bTouchInput = true;
					dragModeInputType = InputType.eTouch;
				}
				if (!_dragMode && (bMouseInput || bGamepadInput || bTouchInput) && _placementMode && _hoverMode == 0 && (_hoverButton == null || _hoverButton.focus == null))
				{
					firstClick = true;
					_dragMode = true;
					History.BeginUndoSection();
					Thing col = _hover;
					if (col != null && (!(_hover is BackgroundTile) || (_placementType != null && _hover.GetType() == _placementType.GetType())))
					{
						if ((Keyboard.Down(Keys.LeftControl) || Keyboard.Down(Keys.RightControl)) && !(_placementType is BackgroundTile))
						{
							_move = col;
						}
						else if (!Keyboard.control)
						{
							_deleteMode = true;
						}
					}
				}
				if (_dragMode)
				{
					if (_tileDragDif == Vec2.MaxValue || inputMode == EditorInput.Gamepad)
					{
						_tileDragDif = _tilePosition;
					}
					Vec2 snappedTilePos = Maths.Snap(_tilePosition, _cellSize, _cellSize);
					Vec2 lerp = _tilePosition;
					Vec2 prevPlace = Vec2.MaxValue;
					do
					{
						Vec2 snap = Maths.Snap(lerp, _cellSize, _cellSize);
						if ((Keyboard.control || (Input.Down("SELECT") && Input.Down("MENU1"))) && _tileDragContext == Vec2.MinValue)
						{
							_tileDragContext = snap;
						}
						if (snap == Maths.Snap(_tileDragDif, _cellSize, _cellSize) && snap != Maths.Snap(_tilePosition, _cellSize, _cellSize))
						{
							break;
						}
						if (snappedTilePos != _tilePosition)
						{
							snap = _tilePosition;
							_tileDragDif = _tilePosition;
						}
						lerp = Lerp.Vec2(lerp, _tileDragDif, _cellSize);
						if (_tileDragDif != _tilePosition)
						{
							UpdateHover(placementLayer, snap, isDrag: true);
						}
						if (!_deleteMode && _placementType != null)
						{
							Thing col2 = _hover;
							if (col2 == null && !(_placementType is BackgroundTile))
							{
								col2 = CollisionPointFilter<Thing>(snap, (Thing x) => x.placementLayer == placementLayer && (!(_placementType is PipeTileset) || x.GetType() == _placementType.GetType()));
							}
							if (col2 is TileButton)
							{
								col2 = null;
							}
							else if (col2 != null && !_levelThings.Contains(col2))
							{
								col2 = null;
							}
							else if (ctrl && alt)
							{
								col2 = null;
							}
							else if (_placementType is BackgroundTile && !(col2 is BackgroundTile))
							{
								col2 = null;
							}
							else if (firstClick && _hover == null)
							{
								col2 = null;
							}
							firstClick = false;
							if ((col2 == null || (_placementType is WireTileset && col2 is IWirePeripheral) || (_placementType is IWirePeripheral && col2 is WireTileset)) && !placementLimitReached && !placementOutOfSizeRange && prevPlace != snap)
							{
								prevPlace = snap;
								Type t8 = _placementType.GetType();
								Thing newThing2 = null;
								if (_eyeDropperSerialized == null)
								{
									newThing2 = CreateThing(t8);
								}
								else
								{
									newThing2 = Thing.LoadThing(_eyeDropperSerialized);
								}
								newThing2.x = snap.x;
								newThing2.y = snap.y;
								if (_placementType is SubBackgroundTile)
								{
									(newThing2.graphic as SpriteMap).frame = ((_placementType as SubBackgroundTile).graphic as SpriteMap).frame;
								}
								if (_placementType is BackgroundTile)
								{
									int frameOffsetX = (int)((snap.x - _tileDragContext.x) / 16f);
									int frameOffsetY = (int)((snap.y - _tileDragContext.y) / 16f);
									(newThing2 as BackgroundTile).frame = (_placementType as BackgroundTile).frame + frameOffsetX + (int)((float)frameOffsetY * ((float)newThing2.graphic.texture.width / 16f));
								}
								else if (_placementType is ForegroundTile)
								{
									(newThing2.graphic as SpriteMap).frame = ((_placementType as ForegroundTile).graphic as SpriteMap).frame;
								}
								if (_hover is BackgroundTile)
								{
									newThing2.depth = _hover.depth + 1;
								}
								History.Add(delegate
								{
									AddObject(newThing2);
								}, delegate
								{
									RemoveObject(newThing2);
								});
								if (newThing2 is PathNode)
								{
									_editorLoadFinished = true;
								}
								if (preciseMode)
								{
									disableDragMode();
								}
							}
						}
						else
						{
							Thing col3 = _hover;
							if (col3 != null)
							{
								History.Add(delegate
								{
									RemoveObject(col3);
								}, delegate
								{
									AddObject(col3);
								});
								if (col3 is PathNode)
								{
									_editorLoadFinished = true;
								}
								_hover = null;
							}
						}
						base.things.RefreshState();
					}
					while ((lerp - _tileDragDif).length > 2f);
				}
				if ((Mouse.left == InputState.Released && dragModeInputType == InputType.eMouse) || (_input.Released("SELECT") && dragModeInputType == InputType.eGamepad) || (TouchScreen.GetRelease() != Touch.None && dragModeInputType == InputType.eTouch))
				{
					disableDragMode();
				}
			}
			if (!Keyboard.control && !Input.Down("MENU1"))
			{
				_tileDragContext = Vec2.MinValue;
			}
			_tileDragDif = _tilePosition;
			_placingTiles = false;
			if (_placementType is BackgroundTile)
			{
				_placingTiles = true;
			}
			if (_placingTiles && _placementMenu == null && ((_input.Pressed("MENU1") && !_input.Down("SELECT")) || _openTileSelector) && _cursorMode == CursorMode.Normal)
			{
				DoMenuClose();
				int frame2 = _placementType.frame;
				_placementMenu = new ContextBackgroundTile(_placementType, null, placement: false)
				{
					positionCursor = true
				};
				_placementMenu.opened = true;
				SFX.Play("openClick", 0.4f);
				_placementMenu.x = 16f;
				_placementMenu.y = 16f;
				_placementMenu.selectedIndex = frame2;
				Level.Add(_placementMenu);
				_openTileSelector = false;
			}
			if (_editMode && _cursorMode == CursorMode.Normal)
			{
				if (_twoFingerGesture || _threeFingerGesture)
				{
					DoMenuClose();
				}
				if (clicked && _hover != null)
				{
					DoMenuClose();
					_placementMenu = _hover.GetContextMenu();
					if (_placementMenu != null)
					{
						_placementMenu.x = 96f;
						_placementMenu.y = 32f;
						if (inputMode == EditorInput.Gamepad || inputMode == EditorInput.Touch)
						{
							_placementMenu.x = 16f;
							_placementMenu.y = 16f;
						}
						_openedEditMenu = true;
						AddThing(_placementMenu);
						_placementMenu.opened = true;
						SFX.Play("openClick", 0.4f);
						clicked = false;
						_oldHover = _hover;
						_lastHoverMenuOpen = _placementMenu;
					}
				}
			}
			hoverUI = false;
			if (_closeMenu)
			{
				DoMenuClose();
			}
			base.Update();
		}
	}

	public override void PostUpdate()
	{
		if (_placementMenu != null)
		{
			if (_editMode && clickedMenu)
			{
				_hover = _oldHover;
			}
			if (inputMode == EditorInput.Touch && TouchScreen.GetTap() != Touch.None && !clickedMenu && !clickedContextBackground && !_openedEditMenu)
			{
				if (_touchState == EditorTouchState.OpenMenu)
				{
					EndCurrentTouchMode();
				}
				_showPlacementMenu = false;
				CloseMenu();
			}
		}
		if (_touchState == EditorTouchState.OpenMenu && _placementMenu == null)
		{
			_touchState = EditorTouchState.Normal;
			_activeTouchButton = null;
		}
		_openedEditMenu = false;
	}

	public void DoMenuClose()
	{
		if (_placementMenu != null)
		{
			if (_placementMenu != _objectMenu)
			{
				RemoveThing(_placementMenu);
			}
			else
			{
				_placementMenu.visible = false;
				_placementMenu.active = false;
				_placementMenu.opened = false;
			}
		}
		_placementMenu = null;
		_closeMenu = false;
	}

	private void UpdateSelection(bool pObjectsChanged = true)
	{
		foreach (Thing t in _levelThings)
		{
			if (pObjectsChanged)
			{
				t.EditorObjectsChanged();
			}
			t.material = null;
		}
		foreach (Thing thing in Level.current.things)
		{
			thing.material = null;
		}
		foreach (Thing t2 in _selection)
		{
			if (t2 is AutoBlock)
			{
				AutoBlock ab = t2 as AutoBlock;
				if (ab._bLeftNub != null)
				{
					_currentDragSelectionHover.Add(ab._bLeftNub);
				}
				if (ab._bRightNub != null)
				{
					_currentDragSelectionHover.Add(ab._bRightNub);
				}
			}
			else if (t2 is AutoPlatform)
			{
				AutoPlatform ab2 = t2 as AutoPlatform;
				if (ab2._leftNub != null)
				{
					_currentDragSelectionHover.Add(ab2._leftNub);
				}
				if (ab2._rightNub != null)
				{
					_currentDragSelectionHover.Add(ab2._rightNub);
				}
			}
			else if (t2 is Door)
			{
				Door ab3 = t2 as Door;
				if (ab3._frame != null)
				{
					_currentDragSelectionHover.Add(ab3._frame);
				}
			}
			else if (t2 is ItemSpawner)
			{
				ItemSpawner ab4 = t2 as ItemSpawner;
				if (ab4._ball1 != null)
				{
					_currentDragSelectionHover.Add(ab4._ball1);
				}
				if (ab4._ball2 != null)
				{
					_currentDragSelectionHover.Add(ab4._ball2);
				}
			}
		}
		foreach (Thing item in _currentDragSelectionHover)
		{
			item.material = _selectionMaterial;
		}
		foreach (Thing t3 in _currentDragSelectionHoverAdd)
		{
			if (_currentDragSelectionHover.Contains(t3))
			{
				t3.material = null;
			}
			else
			{
				t3.material = _selectionMaterial;
			}
		}
	}

	private void RebuildPasteBatch()
	{
		_pasteBatch.Clear();
		foreach (BinaryClassChunk item in _selectionCopy)
		{
			Thing t = Thing.LoadThing(item);
			_pasteBatch.Add(t);
		}
	}

	private void UpdateDragSelection()
	{
		_dragSelectShiftModifier = Keyboard.Down(Keys.LeftShift) || Keyboard.Down(Keys.RightShift) || (inputMode == EditorInput.Gamepad && _selection.Count > 0);
		if (_cursorMode == CursorMode.Selection)
		{
			_selectionDragEnd = ((inputMode == EditorInput.Mouse) ? Mouse.positionScreen : _tilePosition);
			Vec2 tl = _selectionDragStart;
			Vec2 br = _selectionDragEnd;
			if (br.x < tl.x)
			{
				float swap = tl.x;
				tl.x = br.x;
				br.x = swap;
			}
			if (br.y < tl.y)
			{
				float swap2 = tl.y;
				tl.y = br.y;
				br.y = swap2;
			}
			if (_dragSelectShiftModifier)
			{
				_currentDragSelectionHoverAdd.Clear();
				foreach (Thing t in Level.CheckRectAll<Thing>(tl, br))
				{
					_currentDragSelectionHoverAdd.Add(t);
				}
			}
			else
			{
				_currentDragSelectionHover.Clear();
				foreach (Thing t2 in Level.CheckRectAll<Thing>(tl, br))
				{
					_currentDragSelectionHover.Add(t2);
				}
			}
			if (Mouse.right == InputState.Released || Mouse.left == InputState.Released || (inputMode == EditorInput.Gamepad && _input.Released("CANCEL")))
			{
				if (_dragSelectShiftModifier)
				{
					foreach (Thing t3 in _currentDragSelectionHoverAdd)
					{
						if (_currentDragSelectionHover.Contains(t3))
						{
							_currentDragSelectionHover.Remove(t3);
							_selection.Remove(t3);
						}
						else
						{
							_currentDragSelectionHover.Add(t3);
						}
					}
				}
				foreach (Thing t4 in _currentDragSelectionHover)
				{
					if (!(t4 is ContextMenu) && _levelThings.Contains(t4) && !_selection.Contains(t4))
					{
						_selection.Add(t4);
					}
				}
				_currentDragSelectionHoverAdd.Clear();
				dragStartInputType = InputType.eNone;
				_cursorMode = ((_selection.Count > 0) ? CursorMode.HasSelection : CursorMode.Normal);
				clickedMenu = true;
				_selectionDragStart = Vec2.Zero;
			}
			UpdateSelection(pObjectsChanged: false);
			return;
		}
		if (_cursorMode == CursorMode.Drag)
		{
			Vec2 dragTo = Maths.Snap(Mouse.positionScreen + new Vec2(_cellSize / 2f), _cellSize, _cellSize);
			if (inputMode == EditorInput.Gamepad)
			{
				dragTo = Maths.Snap(_tilePosition + new Vec2(_cellSize / 2f), _cellSize, _cellSize);
			}
			if (dragTo != _moveDragStart)
			{
				Vec2 dif = dragTo - _moveDragStart;
				_moveDragStart = dragTo;
				foreach (Thing t5 in _currentDragSelectionHover)
				{
					History.Add(delegate
					{
						t5.position += dif;
						if (t5 is IDontMove)
						{
							Level.current.things.quadTree.Remove(t5);
							Level.current.things.quadTree.Add(t5);
						}
					}, delegate
					{
						t5.position -= dif;
						if (t5 is IDontMove)
						{
							Level.current.things.quadTree.Remove(t5);
							Level.current.things.quadTree.Add(t5);
						}
					});
				}
			}
			if (Mouse.left == InputState.Released || _input.Released("SELECT"))
			{
				_cursorMode = CursorMode.HasSelection;
				UpdateSelection();
				History.EndUndoSection();
				hasUnsavedChanges = true;
			}
			return;
		}
		if (_performCopypaste || ((Keyboard.Down(Keys.LeftControl) || Keyboard.Down(Keys.RightControl) || _cursorMode == CursorMode.Pasting) && (_cursorMode == CursorMode.Normal || _cursorMode == CursorMode.HasSelection || _cursorMode == CursorMode.Pasting || _cursorMode == CursorMode.DragHover)))
		{
			bool cut = Keyboard.Pressed(Keys.X);
			if (_selection.Count > 0 && (Keyboard.Pressed(Keys.C) || cut || _performCopypaste))
			{
				_selectionCopy.Clear();
				_copyCenter = Vec2.Zero;
				History.BeginUndoSection();
				foreach (Thing t6 in _selection)
				{
					copying = true;
					_selectionCopy.Add(t6.Serialize());
					_copyCenter += t6.position;
					copying = false;
					if (cut)
					{
						History.Add(delegate
						{
							RemoveObject(t6);
						}, delegate
						{
							AddObject(t6);
						});
					}
				}
				_copyCenter /= (float)_selection.Count;
				if (cut)
				{
					_selection.Clear();
					_currentDragSelectionHover.Clear();
					UpdateSelection();
				}
				History.EndUndoSection();
				RebuildPasteBatch();
				HUD.AddPlayerChangeDisplay("@CLIPCOPY@Selection copied!", 1f);
			}
			if ((Keyboard.Pressed(Keys.V) && _pasteBatch.Count > 0) || _performCopypaste)
			{
				_selection.Clear();
				_currentDragSelectionHover.Clear();
				_cursorMode = CursorMode.Pasting;
				UpdateSelection(pObjectsChanged: false);
			}
			pasteOffset = Maths.Snap(_copyCenter - Mouse.positionScreen, 16f, 16f);
			if (inputMode == EditorInput.Gamepad)
			{
				pasteOffset = Maths.Snap(_copyCenter - _tilePosition, 16f, 16f);
			}
			_performCopypaste = false;
			if (_cursorMode == CursorMode.Pasting)
			{
				if (Mouse.right == InputState.Released || (_input.Released("CANCEL") && inputMode == EditorInput.Gamepad))
				{
					_cursorMode = CursorMode.Normal;
				}
				if (Mouse.left == InputState.Pressed || (_input.Pressed("SELECT") && inputMode == EditorInput.Gamepad))
				{
					History.BeginUndoSection();
					_selection.Clear();
					_currentDragSelectionHover.Clear();
					_isPaste = true;
					foreach (Thing t7 in _pasteBatch)
					{
						_selection.Add(t7);
						t7.position -= pasteOffset;
						foreach (Thing col in CollisionRectAll<Thing>(t7.position + new Vec2(-6f, -6f), t7.position + new Vec2(6f, 6f), null))
						{
							if (col.placementLayer == t7.placementLayer && _levelThings.Contains(col))
							{
								History.Add(delegate
								{
									RemoveObject(col);
								}, delegate
								{
									AddObject(col);
								});
							}
						}
					}
					foreach (Thing t8 in _selection)
					{
						History.Add(delegate
						{
							AddObject(t8);
						}, delegate
						{
							RemoveObject(t8);
						});
					}
					_selection.Clear();
					_currentDragSelectionHover.Clear();
					_isPaste = false;
					RebuildPasteBatch();
					_placeObjects.Clear();
					base.things.RefreshState();
					UpdateSelection();
					disableDragMode();
				}
			}
		}
		else if (_cursorMode == CursorMode.Pasting)
		{
			_cursorMode = CursorMode.Normal;
		}
		if (_selection.Count > 0 && _cursorMode != CursorMode.Pasting && (Keyboard.Pressed(Keys.F) || (_input.Pressed("MENU1") && inputMode == EditorInput.Gamepad)))
		{
			Vec2 centerPoint = Vec2.Zero;
			if (_cursorMode == CursorMode.Pasting)
			{
				foreach (Thing t9 in _pasteBatch)
				{
					centerPoint += t9.position;
				}
				centerPoint /= (float)_pasteBatch.Count;
			}
			else
			{
				foreach (Thing t10 in _selection)
				{
					centerPoint += t10.position;
				}
				centerPoint /= (float)_selection.Count;
			}
			centerPoint = Maths.SnapRound(centerPoint, _cellSize / 2f, _cellSize / 2f);
			if (_cursorMode == CursorMode.Pasting)
			{
				foreach (Thing item in _pasteBatch)
				{
					float dif2 = item.position.x - centerPoint.x;
					item.SetTranslation(new Vec2((0f - dif2) * 2f, 0f));
					item.EditorFlip(pVertical: false);
					item.flipHorizontal = !item.flipHorizontal;
				}
			}
			else
			{
				History.BeginUndoSection();
				foreach (Thing t11 in _selection)
				{
					float dif3 = t11.position.x - centerPoint.x;
					History.Add(delegate
					{
						t11.SetTranslation(new Vec2((0f - dif3) * 2f, 0f));
						t11.EditorFlip(pVertical: false);
						t11.flipHorizontal = !t11.flipHorizontal;
						if (t11 is IDontMove)
						{
							Level.current.things.quadTree.Remove(t11);
							Level.current.things.quadTree.Add(t11);
						}
					}, delegate
					{
						t11.SetTranslation(new Vec2(dif3 * 2f, 0f));
						t11.EditorFlip(pVertical: false);
						t11.flipHorizontal = !t11.flipHorizontal;
						if (t11 is IDontMove)
						{
							Level.current.things.quadTree.Remove(t11);
							Level.current.things.quadTree.Add(t11);
						}
					});
				}
				UpdateSelection();
				History.EndUndoSection();
			}
			UpdateSelection();
		}
		if (_selection.Count > 0)
		{
			_cursorMode = CursorMode.HasSelection;
			if (inputMode == EditorInput.Mouse)
			{
				foreach (Thing t12 in _selection)
				{
					if (Collision.Point(Mouse.positionScreen, t12))
					{
						_cursorMode = CursorMode.DragHover;
						break;
					}
				}
			}
			else if (inputMode == EditorInput.Gamepad)
			{
				foreach (Thing t13 in _selection)
				{
					if (Collision.Point(_tilePosition, t13))
					{
						_cursorMode = CursorMode.DragHover;
						break;
					}
				}
			}
			bool endSelection = false;
			if (Keyboard.Pressed(Keys.Delete) || (_input.Pressed("MENU2") && inputMode == EditorInput.Gamepad))
			{
				History.BeginUndoSection();
				foreach (Thing t14 in _selection)
				{
					History.Add(delegate
					{
						RemoveObject(t14);
					}, delegate
					{
						AddObject(t14);
					});
				}
				UpdateSelection();
				History.EndUndoSection();
				endSelection = true;
			}
			if (Mouse.left == InputState.Pressed || (_input.Pressed("SELECT") && inputMode == EditorInput.Gamepad))
			{
				if (_cursorMode == CursorMode.DragHover)
				{
					History.BeginUndoSection();
					_cursorMode = CursorMode.Drag;
					if (inputMode == EditorInput.Gamepad)
					{
						_moveDragStart = Maths.Snap(_tilePosition + new Vec2(_cellSize / 2f), _cellSize, _cellSize);
					}
					else
					{
						_moveDragStart = Maths.Snap(Mouse.positionScreen + new Vec2(_cellSize / 2f), _cellSize, _cellSize);
					}
				}
				else
				{
					endSelection = true;
				}
			}
			if (_input.Released("CANCEL"))
			{
				if (_cursorMode == CursorMode.DragHover)
				{
					_performCopypaste = true;
				}
				else
				{
					endSelection = true;
				}
			}
			if (_cursorMode != CursorMode.Pasting && (Mouse.right == InputState.Released || endSelection) && (!_dragSelectShiftModifier || inputMode == EditorInput.Gamepad))
			{
				_cursorMode = CursorMode.Normal;
				_selection.Clear();
				_currentDragSelectionHover.Clear();
				UpdateSelection(pObjectsChanged: false);
			}
			Vec2 offset = new Vec2(0f, 0f);
			if (Keyboard.Pressed(Keys.Up))
			{
				offset.y -= cellSize;
			}
			if (Keyboard.Pressed(Keys.Down))
			{
				offset.y += cellSize;
			}
			if (Keyboard.Pressed(Keys.Left))
			{
				offset.x -= cellSize;
			}
			if (Keyboard.Pressed(Keys.Right))
			{
				offset.x += cellSize;
			}
			if (!(offset != Vec2.Zero))
			{
				return;
			}
			hasUnsavedChanges = true;
			History.BeginUndoSection();
			foreach (Thing t15 in _selection)
			{
				History.Add(delegate
				{
					t15.SetTranslation(offset);
					if (t15 is IDontMove)
					{
						Level.current.things.quadTree.Remove(t15);
						Level.current.things.quadTree.Add(t15);
					}
				}, delegate
				{
					t15.SetTranslation(-offset);
					if (t15 is IDontMove)
					{
						Level.current.things.quadTree.Remove(t15);
						Level.current.things.quadTree.Add(t15);
					}
				});
			}
			UpdateSelection();
			History.EndUndoSection();
		}
		else if (_cursorMode == CursorMode.HasSelection)
		{
			_cursorMode = CursorMode.Normal;
		}
	}

	private void UpdateHover(Layer placementLayer, Vec2 tilePosition, bool isDrag = false)
	{
		IEnumerable<Thing> hoverList = new List<Thing>();
		if (inputMode == EditorInput.Gamepad || isDrag)
		{
			hoverList = CollisionPointAll<Thing>(tilePosition);
		}
		else if (inputMode == EditorInput.Touch && TouchScreen.IsScreenTouched())
		{
			if (_editMode || _copyMode)
			{
				if (TouchScreen.GetTap() != Touch.None)
				{
					for (int i = 0; i < 4; i++)
					{
						hoverList = CollisionCircleAll<Thing>(TouchScreen.GetTap().positionCamera, (float)i * 2f);
						if (hoverList.Count() > 0)
						{
							break;
						}
					}
					_hover = null;
				}
			}
			else if (TouchScreen.GetTouch() != Touch.None)
			{
				hoverList = CollisionPointAll<Thing>(tilePosition);
			}
		}
		else if (inputMode == EditorInput.Mouse && !isDrag)
		{
			hoverList = CollisionPointAll<Thing>(Mouse.positionScreen);
		}
		oldHover = _hover;
		if (!_editMode)
		{
			_hover = null;
		}
		_secondaryHover = null;
		List<Thing> secondaryHoverList = new List<Thing>();
		foreach (Thing t in hoverList)
		{
			if (t is TileButton || !_placeables.Contains(t.GetType()) || !t.editorCanModify || !_things.Contains(t) || (_placementType is WireTileset && t is IWirePeripheral) || (_placementType is IWirePeripheral && t is WireTileset))
			{
				continue;
			}
			if (_placementType is PipeTileset && t is PipeTileset && _placementType.GetType() != t.GetType())
			{
				secondaryHoverList.Add(t);
			}
			else if (t.placementLayer != placementLayer && !_copyMode && !_editMode)
			{
				secondaryHoverList.Add(t);
			}
			else if (_hover == null)
			{
				if (_placementType != null && _placementType is BackgroundTile)
				{
					if (_things.Contains(t))
					{
						if (t.GetType() == _placementType.GetType())
						{
							_hover = t;
						}
						else
						{
							secondaryHoverList.Add(t);
						}
					}
				}
				else if (t.editorCanModify)
				{
					_hover = t;
				}
			}
			else if (t != _hover)
			{
				secondaryHoverList.Add(t);
			}
		}
		if (inputMode == EditorInput.Mouse && !isDrag && _hover == null && !(_placementType is BackgroundTile) && !(_placementType is PipeTileset))
		{
			List<KeyValuePair<float, Thing>> nearest = Level.current.nearest(tilePosition, _levelThings.AsEnumerable(), null, placementLayer, placementLayer: true);
			if (nearest.Count > 0 && (!(_placementType is WireTileset) || !(nearest[0].Value is IWirePeripheral)) && (!(_placementType is IWirePeripheral) || !(nearest[0].Value is WireTileset)) && (nearest[0].Value.position - tilePosition).length < 8f)
			{
				_hover = nearest[0].Value;
			}
		}
		if (_hover == null || oldHover == null || _hover.GetType() != oldHover.GetType())
		{
			if (_hover == null)
			{
				_hoverMenu = null;
			}
			else
			{
				_hoverMenu = _hover.GetContextMenu();
			}
		}
		if (secondaryHoverList.Count > 0)
		{
			IOrderedEnumerable<Thing> ordered = secondaryHoverList.OrderBy((Thing x) => (x.placementLayer == null) ? (-99999) : x.placementLayer.depth);
			if (Keyboard.control)
			{
				if (_hover == null)
				{
					IOrderedEnumerable<Thing> reverseOrdered = secondaryHoverList.OrderBy((Thing x) => (x.placementLayer == null) ? 99999 : (-x.placementLayer.depth));
					_hover = reverseOrdered.First();
				}
				else
				{
					_hover = ordered.First();
				}
			}
			else if (_hover == null || Keyboard.control || (_placementType != null && ordered.First().placementLayer == _placementType.placementLayer))
			{
				_secondaryHover = ordered.First();
				if (_hoverMenu == null)
				{
					_hoverMenu = _secondaryHover.GetContextMenu();
				}
			}
		}
		if (_secondaryHover == null && _hover is Block && secondaryHoverList.Count > 0)
		{
			_secondaryHover = secondaryHoverList.FirstOrDefault((Thing x) => x is PipeTileset);
			if (_secondaryHover != null && !(_secondaryHover as PipeTileset)._foregroundDraw)
			{
				_secondaryHover = null;
			}
		}
	}

	public override void Draw()
	{
		base.Draw();
	}

	private void CalculateGridRestriction()
	{
		Vec2 size = _bottomRightMost - _topLeftMost;
		Vec2 fullRestriction = _sizeRestriction * 2f - size - new Vec2(16f, 16f);
		if (fullRestriction.x > _sizeRestriction.x * 2f)
		{
			fullRestriction.x = _sizeRestriction.x * 2f;
		}
		if (fullRestriction.y > _sizeRestriction.y * 2f)
		{
			fullRestriction.y = _sizeRestriction.y * 2f;
		}
		_gridW = (int)(fullRestriction.x / _cellSize);
		_gridH = (int)(fullRestriction.y / _cellSize);
	}

	public override void PostDrawLayer(Layer layer)
	{
		base.PostDrawLayer(layer);
		if (layer == Layer.Foreground)
		{
			foreach (Thing thing2 in base.things)
			{
				thing2.DoEditorRender();
			}
		}
		if (layer == _procLayer && _procTarget != null && _procContext != null)
		{
			Graphics.Draw(_procTarget, new Vec2(0f, 0f), null, Color.White * 0.5f, 0f, Vec2.Zero, new Vec2(1f, 1f), SpriteEffects.None);
		}
		if (layer == _gridLayer)
		{
			base.backgroundColor = new Color(20, 20, 20);
			Color gridColor = new Color(38, 38, 38);
			if (arcadeMachineMode)
			{
				Graphics.DrawRect(_levelThings[0].position + new Vec2(-17f, -21f), _levelThings[0].position + new Vec2(18f, 21f), gridColor, -0.9f, filled: false);
			}
			else
			{
				float x = (0f - _cellSize) / 2f;
				float y = (0f - _cellSize) / 2f;
				if (_sizeRestriction.x > 0f)
				{
					Vec2 center = -new Vec2((float)_gridW * _cellSize / 2f, (float)(_gridH - 1) * _cellSize / 2f) + new Vec2(8f, 0f);
					x += (float)(int)(center.x / _cellSize) * _cellSize;
					y += (float)(int)(center.y / _cellSize) * _cellSize;
				}
				int wid = _gridW;
				int hig = _gridH;
				if (_miniMode)
				{
					wid = 12;
					hig = 9;
				}
				if (x < _ultimateBounds.x)
				{
					int dif = (int)((_ultimateBounds.x - x) / _cellSize) + 1;
					x = (float)(int)(_ultimateBounds.x / _cellSize * _cellSize) + _cellSize / 2f;
					wid -= dif;
				}
				if (y < _ultimateBounds.y)
				{
					int dif2 = (int)((_ultimateBounds.y - y) / _cellSize) + 1;
					y = (float)(int)(_ultimateBounds.y / _cellSize * _cellSize) + _cellSize / 2f;
					hig -= dif2;
				}
				float limit = x + (float)wid * _cellSize;
				if (limit > _ultimateBounds.Right)
				{
					int dif3 = (int)((limit - _ultimateBounds.Right) / _cellSize) + 1;
					wid -= dif3;
					x = (float)(int)((_ultimateBounds.Right - (float)wid * _cellSize) / _cellSize * _cellSize) - _cellSize / 2f;
				}
				limit = y + (float)hig * _cellSize;
				if (y + (float)hig * _cellSize > _ultimateBounds.Bottom)
				{
					int dif4 = (int)((limit - _ultimateBounds.Bottom) / _cellSize) + 1;
					hig -= dif4;
					y = (float)(int)((_ultimateBounds.Bottom - (float)hig * _cellSize) / _cellSize * _cellSize) - _cellSize / 2f;
				}
				int reqW = wid * (int)_cellSize;
				int reqH = hig * (int)_cellSize;
				int numHor = (int)((float)reqW / _cellSize);
				int numVert = (int)((float)reqH / _cellSize);
				for (int xpos = 0; xpos < numHor + 1; xpos++)
				{
					Graphics.DrawLine(new Vec2(x + (float)xpos * _cellSize, y), new Vec2(x + (float)xpos * _cellSize, y + (float)numVert * _cellSize), gridColor, 2f, -0.9f);
				}
				for (int ypos = 0; ypos < numVert + 1; ypos++)
				{
					Graphics.DrawLine(new Vec2(x, y + (float)ypos * _cellSize), new Vec2(x + (float)numHor * _cellSize, y + (float)ypos * _cellSize), gridColor, 2f, -0.9f);
				}
				Graphics.DrawLine(new Vec2(_ultimateBounds.Left, _ultimateBounds.Top), new Vec2(_ultimateBounds.Right, _ultimateBounds.Top), gridColor, 2f, -0.9f);
				Graphics.DrawLine(new Vec2(_ultimateBounds.Right, _ultimateBounds.Top), new Vec2(_ultimateBounds.Right, _ultimateBounds.Bottom), gridColor, 2f, -0.9f);
				Graphics.DrawLine(new Vec2(_ultimateBounds.Right, _ultimateBounds.Bottom), new Vec2(_ultimateBounds.Left, _ultimateBounds.Bottom), gridColor, 2f, -0.9f);
				Graphics.DrawLine(new Vec2(_ultimateBounds.Left, _ultimateBounds.Bottom), new Vec2(_ultimateBounds.Left, _ultimateBounds.Top), gridColor, 2f, -0.9f);
				if (_miniMode)
				{
					int sides = 0;
					if (!_pathNorth)
					{
						_sideArrow.color = new Color(80, 80, 80);
					}
					else
					{
						_sideArrow.color = new Color(100, 200, 100);
						Graphics.DrawLine(new Vec2(x + (float)(reqW / 2), y - 10f), new Vec2(x + (float)(reqW / 2), y + (float)(reqH / 2) - 8f), Color.Lime * 0.06f, 16f);
						sides++;
					}
					if (!_pathWest)
					{
						_sideArrow.color = new Color(80, 80, 80);
					}
					else
					{
						_sideArrow.color = new Color(100, 200, 100);
						Graphics.DrawLine(new Vec2(x - 10f, y + (float)(reqH / 2)), new Vec2(x + (float)(reqW / 2) - 8f, y + (float)(reqH / 2)), Color.Lime * 0.06f, 16f);
						sides++;
					}
					if (!_pathEast)
					{
						_sideArrow.color = new Color(80, 80, 80);
					}
					else
					{
						_sideArrow.color = new Color(100, 200, 100);
						Graphics.DrawLine(new Vec2(x + (float)(reqW / 2) + 8f, y + (float)(reqH / 2)), new Vec2(x + (float)reqW + 10f, y + (float)(reqH / 2)), Color.Lime * 0.06f, 16f);
						sides++;
					}
					if (!_pathSouth)
					{
						_sideArrow.color = new Color(80, 80, 80);
					}
					else
					{
						_sideArrow.color = new Color(100, 200, 100);
						Graphics.DrawLine(new Vec2(x + (float)(reqW / 2), y + (float)(reqH / 2) + 8f), new Vec2(x + (float)(reqW / 2), y + (float)reqH + 10f), Color.Lime * 0.06f, 16f);
						sides++;
					}
					if (sides > 0)
					{
						Graphics.DrawLine(new Vec2(x + (float)(reqW / 2) - 8f, y + (float)(reqH / 2)), new Vec2(x + (float)(reqW / 2) + 8f, y + (float)(reqH / 2)), Color.Lime * 0.06f, 16f);
					}
				}
			}
		}
		if (layer == Layer.Foreground)
		{
			float x2 = (0f - _cellSize) / 2f;
			float y2 = (0f - _cellSize) / 2f;
			int wid2 = _gridW;
			int hig2 = _gridH;
			if (_miniMode)
			{
				wid2 = 12;
				hig2 = 9;
			}
			int reqW2 = wid2 * 16;
			int reqH2 = hig2 * 16;
			if (_miniMode)
			{
				_procTilesWide = (int)_genSize.x;
				_procTilesHigh = (int)_genSize.y;
				_procXPos = (int)_genTilePos.x;
				_procYPos = (int)_genTilePos.y;
				if (_procXPos > _procTilesWide)
				{
					_procXPos = _procTilesWide;
				}
				if (_procYPos > _procTilesHigh)
				{
					_procYPos = _procTilesHigh;
				}
				for (int i = 0; i < _procTilesWide; i++)
				{
					for (int j = 0; j < _procTilesHigh; j++)
					{
						int xDraw = i - _procXPos;
						int yDraw = j - _procYPos;
						if (i != _procXPos || j != _procYPos)
						{
							Graphics.DrawRect(new Vec2(x2 + (float)(reqW2 * xDraw), y2 + (float)(reqH2 * yDraw)), new Vec2(x2 + (float)(reqW2 * (xDraw + 1)), y2 + (float)(reqH2 * (yDraw + 1))), Color.White * 0.2f, 1f, filled: false);
						}
					}
				}
			}
			if (_hoverButton == null)
			{
				if (_cursorMode != CursorMode.Pasting)
				{
					if (_secondaryHover != null && _placementMode)
					{
						Vec2 p = _secondaryHover.topLeft;
						Vec2 p2 = _secondaryHover.bottomRight;
						Graphics.DrawRect(p, p2, Color.White * 0.5f, 1f, filled: false);
					}
					else if (_hover != null && _placementMode && (inputMode != EditorInput.Touch || _editMode))
					{
						Vec2 p3 = _hover.topLeft;
						Vec2 p4 = _hover.bottomRight;
						Graphics.DrawRect(p3, p4, Color.White * 0.5f, 1f, filled: false);
						_hover.DrawHoverInfo();
					}
				}
				if (DevConsole.wagnusDebug)
				{
					Graphics.DrawLine(_tilePosition, _tilePosition + new Vec2(128f, 0f), Color.White * 0.5f);
					Graphics.DrawLine(_tilePosition, _tilePosition + new Vec2(-128f, 0f), Color.White * 0.5f);
					Graphics.DrawLine(_tilePosition, _tilePosition + new Vec2(0f, 128f), Color.White * 0.5f);
					Graphics.DrawLine(_tilePosition, _tilePosition + new Vec2(0f, -128f), Color.White * 0.5f);
				}
				if ((_hover == null || _cursorMode == CursorMode.DragHover || _cursorMode == CursorMode.Drag) && inputMode == EditorInput.Gamepad)
				{
					if (_cursorMode == CursorMode.DragHover || _cursorMode == CursorMode.Drag)
					{
						_cursor.depth = 1f;
						_cursor.scale = new Vec2(1f, 1f);
						_cursor.position = _tilePosition;
						if (_cursorMode == CursorMode.DragHover)
						{
							_cursor.frame = 1;
						}
						else if (_cursorMode == CursorMode.Drag)
						{
							_cursor.frame = 5;
						}
						_cursor.Draw();
					}
					else if (_placementMenu == null)
					{
						Graphics.DrawRect(_tilePosition - new Vec2(_cellSize / 2f, _cellSize / 2f), _tilePosition + new Vec2(_cellSize / 2f, _cellSize / 2f), Color.White * 0.5f, 1f, filled: false);
					}
				}
				if (_cursorMode == CursorMode.Normal && _hover == null && _placementMode && inputMode != EditorInput.Touch && _placementMenu == null && _placementType != null)
				{
					_placementType.depth = 0.9f;
					_placementType.x = _tilePosition.x;
					_placementType.y = _tilePosition.y;
					_placementType.Draw();
					if (placementLimitReached || placementOutOfSizeRange)
					{
						Graphics.Draw(_cantPlace, _placementType.x, _placementType.y, 0.95f);
					}
				}
			}
			if (_cursorMode == CursorMode.Selection || _cursorMode == CursorMode.HasSelection || _cursorMode == CursorMode.Drag || _cursorMode == CursorMode.DragHover)
			{
				_leftSelectionDraw = false;
				if (_cursorMode == CursorMode.Selection)
				{
					Graphics.DrawDottedRect(_selectionDragStart, _selectionDragEnd, Color.White * 0.5f, 1f, 2f, 4f);
				}
			}
			if (_cursorMode == CursorMode.Pasting)
			{
				Graphics.material = _selectionMaterialPaste;
				foreach (Thing item in _pasteBatch)
				{
					Vec2 pos = item.position;
					item.position -= pasteOffset;
					item.Draw();
					item.position = pos;
				}
				Graphics.material = null;
			}
		}
		string selectText;
		string copyText;
		string menuText;
		string searchText;
		string undoText;
		string redoText;
		string buttonText;
		int num;
		int num2;
		if (layer == Layer.HUD)
		{
			if (inputMode == EditorInput.Touch)
			{
				float touchTooltipYOffset = -24f;
				if (_activeTouchButton != null || _fileDialog.opened)
				{
					if (_activeTouchButton != null)
					{
						Graphics.DrawString(_activeTouchButton.explanation, Layer.HUD.camera.OffsetBR(-20f, touchTooltipYOffset) - new Vec2(Graphics.GetStringWidth(_activeTouchButton.explanation) + (_cancelButton.size.x + 4f), 0f), Color.Gray, 0.99f);
					}
					else if (_fileDialog.opened)
					{
						string explanation = "Double tap level to open!";
						Graphics.DrawString(explanation, Layer.HUD.camera.OffsetBR(-20f, touchTooltipYOffset) - new Vec2(Graphics.GetStringWidth(explanation) + (_cancelButton.size.x + 4f), 0f), Color.Gray, 0.99f);
					}
					Graphics.DrawRect(_cancelButton.position, _cancelButton.position + _cancelButton.size, new Color(70, 70, 70), 0.99f, filled: false);
					Graphics.DrawRect(_cancelButton.position, _cancelButton.position + _cancelButton.size, new Color(30, 30, 30), 0.98f);
					Graphics.DrawString(_cancelButton.caption, _cancelButton.position + _cancelButton.size / 2f + new Vec2((0f - Graphics.GetStringWidth(_cancelButton.caption)) / 2f, -4f), Color.White, 0.99f);
				}
				else if (!_fileDialog.opened)
				{
					float totalSize = 0f;
					foreach (EditorTouchButton button in _touchButtons)
					{
						Graphics.DrawRect(button.position, button.position + button.size, new Color(70, 70, 70), 0.99f, filled: false);
						Graphics.DrawRect(button.position, button.position + button.size, new Color(30, 30, 30), 0.98f);
						Graphics.DrawString(button.caption, button.position + button.size / 2f + new Vec2((0f - Graphics.GetStringWidth(button.caption)) / 2f, -4f), Color.White, 0.99f);
						totalSize += button.size.x;
					}
					if (_placementMenu != null && _placementMenu is EditorGroupMenu)
					{
						string explanation2 = "Double tap to select!";
						Graphics.DrawString(explanation2, Layer.HUD.camera.OffsetBR(-20f, touchTooltipYOffset) - new Vec2(Graphics.GetStringWidth(explanation2) + (totalSize + 8f), 0f), Color.Gray, 0.99f);
					}
				}
				if (_placingTiles && _placementMenu == null)
				{
					Graphics.DrawRect(_editTilesButton.position, _editTilesButton.position + _editTilesButton.size, new Color(70, 70, 70), 0.99f, filled: false);
					Graphics.DrawRect(_editTilesButton.position, _editTilesButton.position + _editTilesButton.size, new Color(30, 30, 30), 0.98f);
					Graphics.DrawString(_editTilesButton.caption, _editTilesButton.position + _editTilesButton.size / 2f + new Vec2((0f - Graphics.GetStringWidth(_editTilesButton.caption)) / 2f, -4f), Color.White, 0.99f);
				}
			}
			if (hasUnsavedChanges)
			{
				Graphics.DrawFancyString("*", new Vec2(4f, 4f), Color.White * 0.6f, 0.99f);
			}
			if (tooltip != null)
			{
				Graphics.DrawRect(new Vec2(16f, Layer.HUD.height - 14f), new Vec2(16f + Graphics.GetFancyStringWidth(tooltip) + 2f, Layer.HUD.height - 2f), new Color(0, 0, 0) * 0.75f, 0.99f);
				Graphics.DrawFancyString(tooltip, new Vec2(18f, Layer.HUD.height - 12f), Color.White, 0.99f);
			}
			bool showMouseControls = _input.lastActiveDevice is Keyboard;
			if (_hoverMode == 0 && _hoverButton == null)
			{
				buttonText = "";
				string quackText = "@CANCEL@";
				selectText = "@SELECT@";
				copyText = "@CANCEL@";
				menuText = "@MENU2@";
				searchText = "@START@";
				undoText = "@STRAFE@";
				redoText = "@RAGDOLL@";
				if (showMouseControls)
				{
					quackText = "@RIGHTMOUSE@" + quackText;
					selectText = "@LEFTMOUSE@" + selectText;
					copyText = "@MIDDLEMOUSE@" + copyText;
					menuText = "@RIGHTMOUSE@" + menuText;
				}
				if (_cursorMode == CursorMode.HasSelection || _cursorMode == CursorMode.Drag || _cursorMode == CursorMode.DragHover)
				{
					if (inputMode == EditorInput.Gamepad)
					{
						if (_cursorMode == CursorMode.DragHover || _cursorMode == CursorMode.Drag)
						{
							buttonText += "@SELECT@DRAG  ";
						}
						if (_cursorMode == CursorMode.HasSelection || _cursorMode == CursorMode.DragHover)
						{
							buttonText += "@CANCEL@DRAG ADD  ";
							buttonText += "@MENU1@FLIP  ";
							buttonText += "@MENU2@DELETE  ";
						}
						buttonText += ((_cursorMode == CursorMode.DragHover) ? "@CANCEL@COPY  " : "@CANCEL@DESELECT  ");
					}
					else
					{
						buttonText = "@KBDARROWS@NUDGE  ";
						if (_cursorMode == CursorMode.DragHover)
						{
							buttonText += "@LEFTMOUSE@DRAG  ";
						}
						buttonText += "@RIGHTMOUSE@DESELECT  ";
						if (_cursorMode == CursorMode.HasSelection || _cursorMode == CursorMode.DragHover)
						{
							buttonText += "@KBDSHIFT@ADD SELECTION  ";
							buttonText += "@KBDF@FLIP  ";
						}
					}
				}
				else if (_cursorMode == CursorMode.Pasting)
				{
					if (inputMode == EditorInput.Gamepad)
					{
						buttonText += "@SELECT@PASTE  ";
						buttonText += "@CANCEL@CANCEL  ";
					}
					else
					{
						buttonText += "@LEFTMOUSE@PASTE  ";
						buttonText += "@RIGHTMOUSE@CANCEL  ";
					}
				}
				else if (_fileDialog.opened)
				{
					buttonText = "@WASD@MOVE  " + selectText + "SELECT  @MENU2@DELETE  " + quackText + "CANCEL  @STRAFE@+@RAGDOLL@BROWSE..";
				}
				else if (_menuOpen && inputMode == EditorInput.Gamepad)
				{
					buttonText = "@WASD@MOVE  " + selectText + "SELECT  @RIGHT@EXPAND  " + quackText + "CLOSE";
				}
				else if (inputMode == EditorInput.Gamepad || inputMode == EditorInput.Mouse)
				{
					if (_secondaryHover == null)
					{
						num = ((_hover != null) ? 1 : 0);
						if (num == 0 && !_placingTiles)
						{
							num2 = ((_placementType != null) ? 1 : 0);
							goto IL_1762;
						}
					}
					else
					{
						num = 1;
					}
					num2 = 1;
					goto IL_1762;
				}
				goto IL_18bd;
			}
			if (_hoverButton != null)
			{
				string buttonText2 = _hoverButton.hoverText;
				if (buttonText2 != null)
				{
					float wide = _font.GetWidth(buttonText2);
					Vec2 topLeft = new Vec2(layer.width - 28f - wide, layer.height - 28f);
					_font.depth = 0.8f;
					_font.Draw(buttonText2, topLeft.x, topLeft.y, Color.White, 0.8f);
					Graphics.DrawRect(topLeft + new Vec2(-2f, -2f), topLeft + new Vec2(wide + 2f, 9f), Color.Black * 0.5f, 0.6f);
				}
			}
			goto IL_2555;
		}
		if (layer != _objectMenuLayer)
		{
			return;
		}
		if (inputMode == EditorInput.Mouse)
		{
			_cursor.depth = 1f;
			_cursor.scale = new Vec2(1f, 1f);
			_cursor.position = Mouse.position;
			if (_cursorMode == CursorMode.Normal)
			{
				_cursor.frame = 0;
			}
			else if (_cursorMode == CursorMode.DragHover)
			{
				_cursor.frame = 1;
			}
			else if (_cursorMode == CursorMode.Drag)
			{
				_cursor.frame = 5;
			}
			else if (_cursorMode == CursorMode.Selection)
			{
				_cursor.frame = (_dragSelectShiftModifier ? 6 : 2);
			}
			else if (_cursorMode == CursorMode.HasSelection)
			{
				_cursor.frame = (_dragSelectShiftModifier ? 6 : 0);
			}
			if (hoverTextBox)
			{
				_cursor.frame = 7;
				_cursor.position.y -= 4f;
				_cursor.scale = new Vec2(0.5f, 1f);
			}
			_cursor.Draw();
		}
		if (inputMode != EditorInput.Touch)
		{
			return;
		}
		if (TouchScreen.GetTouches().Count == 0)
		{
			Vec2 pos2 = _objectMenuLayer.camera.transformScreenVector(Mouse.positionConsole + new Vec2(TouchScreen._spoofFingerDistance, 0f));
			Vec2 pos3 = _objectMenuLayer.camera.transformScreenVector(Mouse.positionConsole - new Vec2(TouchScreen._spoofFingerDistance, 0f));
			Graphics.DrawCircle(pos2, 4f, Color.White * 0.2f, 2f, 1f);
			Graphics.DrawCircle(pos3, 4f, Color.White * 0.2f, 2f, 1f);
			Graphics.DrawRect(pos2 + new Vec2(-0.5f, -0.5f), pos2 + new Vec2(0.5f, 0.5f), Color.White, 1f);
			Graphics.DrawRect(pos3 + new Vec2(-0.5f, -0.5f), pos3 + new Vec2(0.5f, 0.5f), Color.White, 1f);
			return;
		}
		foreach (Touch touch in TouchScreen.GetTouches())
		{
			Graphics.DrawCircle(touch.Transform(_objectMenuLayer.camera), 4f, Color.White, 2f, 1f);
		}
		return;
		IL_2555:
		_font.scale = new Vec2(1f, 1f);
		return;
		IL_18bd:
		if (inputMode == EditorInput.Touch)
		{
			buttonText = "";
		}
		if (buttonText != "")
		{
			float wide2 = _font.GetWidth(buttonText);
			Vec2 topLeft2 = new Vec2(layer.width - 22f - wide2, layer.height - 28f);
			_font.depth = 0.8f;
			_font.Draw(buttonText, topLeft2.x, topLeft2.y, Color.White, 0.7f, _input);
		}
		_font.scale = new Vec2(0.5f, 0.5f);
		float contextObjectOffsetY = 0f;
		if (placementLimit > 0)
		{
			contextObjectOffsetY -= 16f;
			Vec2 size = new Vec2(128f, 12f);
			Vec2 topLeft3 = new Vec2(31f, layer.height - 19f - size.y);
			Graphics.DrawRect(topLeft3, topLeft3 + size, Color.Black * 0.5f, 0.6f);
			Graphics.Draw(_editorCurrency, topLeft3.x - 10f, topLeft3.y + 2f, 0.95f);
			float wide3 = (size.x - 4f) * Math.Min((float)placementTotalCost / (float)placementLimit, 1f);
			string placementCostString = placementTotalCost + "/" + placementLimit;
			if (placementLimitReached)
			{
				placementCostString += " FULL!";
			}
			float placementCostStringWidth = _font.GetWidth(placementCostString);
			_font.Draw(placementCostString, topLeft3.x + size.x / 2f - placementCostStringWidth / 2f, topLeft3.y + 4f, Color.White, 0.7f);
			topLeft3 += new Vec2(2f, 2f);
			Graphics.DrawRect(topLeft3, topLeft3 + new Vec2(wide3, size.y - 4f), (placementLimitReached ? Colors.DGRed : Colors.DGGreen) * 0.5f, 0.6f);
		}
		if (searching)
		{
			Graphics.DrawRect(Vec2.Zero, new Vec2(layer.width, layer.height), Color.Black * 0.5f, 0.9f);
			Vec2 searchPos = new Vec2(8f, layer.height - 26f);
			Graphics.DrawString("@searchiconwhitebig@", searchPos, Color.White, 0.95f);
			if (Keyboard.keyString == "")
			{
				Graphics.DrawString("|GRAY|Type to search...", searchPos + new Vec2(26f, 7f), Color.White, 0.95f);
			}
			else
			{
				Graphics.DrawString(Keyboard.keyString + "_", searchPos + new Vec2(26f, 7f), Color.White, 0.95f);
			}
			if (inputMode == EditorInput.Mouse)
			{
				_searchHoverIndex = -1;
			}
			float wide4 = 200f;
			if (searchItems != null && searchItems.Count > 0)
			{
				searchPos.y -= 22f;
				for (int k = 0; k < 10 && k < searchItems.Count; k++)
				{
					Graphics.DrawString(searchItems[k].thing.thing.editorName, new Vec2(searchPos.x + 24f, searchPos.y + 6f), Color.White, 0.95f);
					searchItems[k].thing.image.depth = 0.95f;
					searchItems[k].thing.image.x = searchPos.x + 4f;
					searchItems[k].thing.image.y = searchPos.y;
					searchItems[k].thing.image.color = Color.White;
					searchItems[k].thing.image.scale = new Vec2(1f);
					searchItems[k].thing.image.Draw();
					if ((inputMode == EditorInput.Mouse && Mouse.x > searchPos.x && Mouse.x < searchPos.x + 200f && Mouse.y > searchPos.y - 2f && Mouse.y < searchPos.y + 19f) || k == _searchHoverIndex)
					{
						_searchHoverIndex = k;
						Graphics.DrawRect(searchPos + new Vec2(2f, -2f), searchPos + new Vec2(wide4 - 2f, 18f), new Color(70, 70, 70), 0.93f);
					}
					searchPos.y -= 20f;
				}
				Graphics.DrawRect(searchPos + new Vec2(0f, 16f), new Vec2(searchPos.x + wide4, layer.height - 28f), new Color(30, 30, 30), 0.91f);
			}
			Graphics.DrawRect(new Vec2(8f, layer.height - 26f), new Vec2(300f, layer.height - 6f), new Color(30, 30, 30), 0.91f);
		}
		float placementHeight = 0f;
		if (_placementType != null && _cursorMode == CursorMode.Normal && _placementMenu == null)
		{
			Vec2 size2 = new Vec2(_placementType.width, _placementType.height);
			size2.x += 4f;
			size2.y += 4f;
			if (size2.x < 32f)
			{
				size2.x = 32f;
			}
			if (size2.y < 32f)
			{
				size2.y = 32f;
			}
			Vec2 topLeft4 = new Vec2(19f, layer.height - 19f - size2.y + contextObjectOffsetY);
			string deets = _placementType.GetDetailsString();
			while (deets.Count((char c) => c == '\n') > 5)
			{
				deets = deets.Substring(0, deets.LastIndexOf('\n'));
			}
			float wide5 = _font.GetWidth(deets) + 8f;
			if (deets != "")
			{
				_font.Draw(deets, topLeft4.x + size2.x + 4f, topLeft4.y + 4f, Color.White, 0.7f);
			}
			else
			{
				wide5 = 0f;
			}
			Graphics.DrawRect(topLeft4, topLeft4 + size2 + new Vec2(wide5, 0f), Color.Black * 0.5f, 0.6f);
			editorDraw = true;
			_placementType.left = topLeft4.x + (size2.x / 2f - _placementType.w / 2f);
			_placementType.top = topLeft4.y + (size2.y / 2f - _placementType.h / 2f);
			_placementType.depth = 0.7f;
			_placementType.Draw();
			editorDraw = false;
			_font.Draw("Placing (" + _placementType.editorName + ")", topLeft4.x, topLeft4.y - 6f, Color.White, 0.7f);
			placementHeight = size2.y;
		}
		Thing hoverDraw = _hover;
		if (_secondaryHover != null)
		{
			hoverDraw = _secondaryHover;
		}
		if (hoverDraw != null && _cursorMode == CursorMode.Normal && _hoverMode == 0)
		{
			Vec2 size3 = new Vec2(hoverDraw.width, hoverDraw.height);
			size3.x += 4f;
			size3.y += 4f;
			if (size3.x < 32f)
			{
				size3.x = 32f;
			}
			if (size3.y < 32f)
			{
				size3.y = 32f;
			}
			Vec2 topLeft5 = new Vec2(19f, layer.height - 19f - size3.y - (placementHeight + 10f) + contextObjectOffsetY);
			string deets2 = hoverDraw.GetDetailsString();
			while (deets2.Count((char c) => c == '\n') > 5)
			{
				deets2 = deets2.Substring(0, deets2.LastIndexOf('\n'));
			}
			float wide6 = _font.GetWidth(deets2) + 8f;
			if (deets2 != "")
			{
				_font.Draw(deets2, topLeft5.x + size3.x + 4f, topLeft5.y + 4f, Color.White, 0.7f);
			}
			else
			{
				wide6 = 0f;
			}
			Graphics.DrawRect(topLeft5, topLeft5 + size3 + new Vec2(wide6, 0f), Color.Black * 0.5f, 0.6f);
			Vec2 pos4 = hoverDraw.position;
			Depth d = hoverDraw.depth;
			editorDraw = true;
			hoverDraw.left = topLeft5.x + (size3.x / 2f - hoverDraw.w / 2f);
			hoverDraw.top = topLeft5.y + (size3.y / 2f - hoverDraw.h / 2f);
			hoverDraw.depth = 0.7f;
			hoverDraw.Draw();
			editorDraw = false;
			hoverDraw.position = pos4;
			hoverDraw.depth = d;
			_font.Draw("Hovering (" + hoverDraw.editorName + ")", topLeft5.x, topLeft5.y - 6f, Color.White);
		}
		goto IL_2555;
		IL_1762:
		bool shouldDisplayBrowsePrompt = (byte)num2 != 0;
		if (_placementType != null && _hover != null && GetLayerOrOverride(_placementType) == GetLayerOrOverride(_hover))
		{
			buttonText = buttonText + selectText + "ERASE  ";
		}
		else if (_placementType != null)
		{
			buttonText = buttonText + selectText + "PLACE  ";
			if (rotateValid)
			{
				buttonText += "@RSTICK@ROTATE  ";
			}
		}
		if (num != 0)
		{
			buttonText = buttonText + copyText + "COPY  ";
		}
		if (_hover != null && !_placingTiles && _hoverMenu != null)
		{
			buttonText += "@MENU1@EDIT  ";
		}
		if (inputMode == EditorInput.Gamepad)
		{
			if (History.hasUndo)
			{
				buttonText = buttonText + undoText + "UNDO  ";
			}
			if (History.hasRedo)
			{
				buttonText = buttonText + redoText + "REDO  ";
			}
			buttonText += "@CANCEL@DRAG SELECT  ";
		}
		if (_placingTiles)
		{
			buttonText += "@MENU1@TILES  ";
		}
		if (shouldDisplayBrowsePrompt)
		{
			buttonText = buttonText + searchText + "BROWSE  ";
		}
		buttonText = buttonText + menuText + "MENU";
		if (_font.GetWidth(buttonText) < 397f)
		{
			buttonText = "@WASD@MOVE  " + buttonText;
		}
		if (inputMode == EditorInput.Mouse)
		{
			buttonText += "  @RIGHTMOUSE@DRAG SELECT";
		}
		goto IL_18bd;
	}

	public override void StartDrawing()
	{
		if (_procTarget == null)
		{
			_procTarget = new RenderTarget2D(Graphics.width, Graphics.height);
		}
		if (_procContext != null)
		{
			_procContext.Draw(_procTarget, Level.current.camera, _procDrawOffset);
		}
	}

	public void CloseMenu()
	{
		_closeMenu = true;
	}

	public void DoSave(string saveName)
	{
		_saveName = saveName;
		if (!_saveName.EndsWith(".lev"))
		{
			_saveName += ".lev";
		}
		Save();
	}

	private void onLoad(object sender, CancelEventArgs e)
	{
		if (e.Cancel)
		{
			return;
		}
		IEnumerable<DXMLNode> objectsNode = DuckXML.Load(_saveName = _loadForm.FileName).Element("Level").Elements("Objects");
		if (objectsNode == null)
		{
			return;
		}
		ClearEverything();
		foreach (DXMLNode elly in objectsNode.Elements("Object"))
		{
			AddObject(Thing.LegacyLoadThing(elly));
		}
	}

	public void LoadLevel(string load)
	{
		load = load.Replace('\\', '/');
		while (load.StartsWith("/"))
		{
			load = load.Substring(1);
		}
		ClearEverything();
		_saveName = load;
		_currentLevelData = DuckFile.LoadLevel(load);
		Thing.loadingLevel = _currentLevelData;
		if (_currentLevelData == null)
		{
			_currentLevelData = new LevelData();
			Thing.loadingLevel = null;
			return;
		}
		_currentLevelData.SetPath(_saveName);
		if (_currentLevelData.metaData.guid == null || (!editingContent && Content.GetLevel(_currentLevelData.metaData.guid, LevelLocation.Content) != null))
		{
			_currentLevelData.metaData.guid = Guid.NewGuid().ToString();
		}
		_onlineSettingChanged = true;
		if (_currentLevelData.customData != null)
		{
			if (_currentLevelData.customData.customTileset01Data != null)
			{
				Custom.ApplyCustomData(_currentLevelData.customData.customTileset01Data.GetTileData(), 0, CustomType.Block);
			}
			if (_currentLevelData.customData.customTileset02Data != null)
			{
				Custom.ApplyCustomData(_currentLevelData.customData.customTileset02Data.GetTileData(), 1, CustomType.Block);
			}
			if (_currentLevelData.customData.customTileset03Data != null)
			{
				Custom.ApplyCustomData(_currentLevelData.customData.customTileset03Data.GetTileData(), 2, CustomType.Block);
			}
			if (_currentLevelData.customData.customBackground01Data != null)
			{
				Custom.ApplyCustomData(_currentLevelData.customData.customBackground01Data.GetTileData(), 0, CustomType.Background);
			}
			if (_currentLevelData.customData.customBackground02Data != null)
			{
				Custom.ApplyCustomData(_currentLevelData.customData.customBackground02Data.GetTileData(), 1, CustomType.Background);
			}
			if (_currentLevelData.customData.customBackground03Data != null)
			{
				Custom.ApplyCustomData(_currentLevelData.customData.customBackground03Data.GetTileData(), 2, CustomType.Background);
			}
			if (_currentLevelData.customData.customPlatform01Data != null)
			{
				Custom.ApplyCustomData(_currentLevelData.customData.customPlatform01Data.GetTileData(), 0, CustomType.Platform);
			}
			if (_currentLevelData.customData.customPlatform02Data != null)
			{
				Custom.ApplyCustomData(_currentLevelData.customData.customPlatform02Data.GetTileData(), 1, CustomType.Platform);
			}
			if (_currentLevelData.customData.customPlatform03Data != null)
			{
				Custom.ApplyCustomData(_currentLevelData.customData.customPlatform03Data.GetTileData(), 2, CustomType.Platform);
			}
			if (_currentLevelData.customData.customParallaxData != null)
			{
				Custom.ApplyCustomData(_currentLevelData.customData.customParallaxData.GetTileData(), 0, CustomType.Parallax);
			}
		}
		previewCapture = LoadPreview(_currentLevelData.previewData.preview);
		_pathNorth = false;
		_pathSouth = false;
		_pathEast = false;
		_pathWest = false;
		_miniMode = false;
		int sideMask = _currentLevelData.proceduralData.sideMask;
		if ((sideMask & 1) != 0)
		{
			_pathNorth = true;
		}
		if ((sideMask & 2) != 0)
		{
			_pathEast = true;
		}
		if ((sideMask & 4) != 0)
		{
			_pathSouth = true;
		}
		if ((sideMask & 8) != 0)
		{
			_pathWest = true;
		}
		if (sideMask != 0)
		{
			_miniMode = true;
		}
		_loadingLevel = true;
		LoadObjects(pAlternate: false);
		LoadObjects(pAlternate: true);
		_loadingLevel = false;
		_editorLoadFinished = true;
		if (!_looseClear)
		{
			CenterView();
		}
		hasUnsavedChanges = false;
		Thing.loadingLevel = null;
	}

	public void LoadObjects(bool pAlternate)
	{
		foreach (BinaryClassChunk item in pAlternate ? _currentLevelData.proceduralData.openAirAlternateObjects.objects : _currentLevelData.objects.objects)
		{
			Thing t = Thing.LoadThing(item);
			if (Thing.CheckForBozoData(t) || t == null || !t.editorCanModify)
			{
				continue;
			}
			if (pAlternate)
			{
				t.active = false;
				if (t is ThingContainer)
				{
					ThingContainer container = t as ThingContainer;
					if (container.bozocheck)
					{
						foreach (Thing thing in container.things)
						{
							if (!Thing.CheckForBozoData(thing))
							{
								_levelThingsAlternate.Add(thing);
							}
						}
						continue;
					}
					foreach (Thing thing2 in container.things)
					{
						_levelThingsAlternate.Add(thing2);
					}
				}
				else
				{
					_levelThingsAlternate.Add(t);
				}
			}
			else
			{
				AddObject(t);
			}
		}
	}

	public void LegacyLoadLevel(string load)
	{
		load = load.Replace('\\', '/');
		while (load.StartsWith("/"))
		{
			load = load.Substring(1);
		}
		DuckXML doc = null;
		doc = ((_additionalSaveDirectory != null) ? DuckXML.Load(load) : DuckFile.LoadDuckXML(load));
		_saveName = load;
		LegacyLoadLevelParts(doc);
		hasUnsavedChanges = false;
	}

	public void LegacyLoadLevelParts(DuckXML doc)
	{
		hadGUID = false;
		ClearEverything();
		DXMLNode lev = doc.Element("Level");
		DXMLNode id = lev.Element("ID");
		if (id != null)
		{
			_currentLevelData.metaData.guid = id.Value;
			hadGUID = true;
		}
		DXMLNode onlineElement = lev.Element("ONLINE");
		if (onlineElement != null)
		{
			_currentLevelData.metaData.onlineMode = Convert.ToBoolean(onlineElement.Value);
		}
		else
		{
			_currentLevelData.metaData.onlineMode = false;
		}
		previewCapture = LegacyLoadPreview(lev);
		_pathNorth = false;
		_pathSouth = false;
		_pathEast = false;
		_pathWest = false;
		_miniMode = false;
		DXMLNode pathMask = lev.Element("PathMask");
		if (pathMask != null)
		{
			int num = Convert.ToInt32(pathMask.Value);
			if ((num & 1) != 0)
			{
				_pathNorth = true;
			}
			if ((num & 2) != 0)
			{
				_pathEast = true;
			}
			if ((num & 4) != 0)
			{
				_pathSouth = true;
			}
			if ((num & 8) != 0)
			{
				_pathWest = true;
			}
			if (num != 0)
			{
				_miniMode = true;
			}
		}
		DXMLNode workshopElement = lev.Element("workshopID");
		if (workshopElement != null)
		{
			_currentLevelData.metaData.workshopID = Convert.ToUInt64(workshopElement.Value);
		}
		workshopElement = lev.Element("workshopName");
		if (workshopElement != null)
		{
			_currentLevelData.workshopData.name = workshopElement.Value;
		}
		workshopElement = lev.Element("workshopDescription");
		if (workshopElement != null)
		{
			_currentLevelData.workshopData.description = workshopElement.Value;
		}
		workshopElement = lev.Element("workshopVisibility");
		if (workshopElement != null)
		{
			_currentLevelData.workshopData.visibility = (RemoteStoragePublishedFileVisibility)Convert.ToInt32(workshopElement.Value);
		}
		workshopElement = lev.Element("workshopTags");
		if (workshopElement != null)
		{
			string[] tagz = workshopElement.Value.Split('|');
			_currentLevelData.workshopData.tags = new List<string>();
			if (tagz.Count() != 0 && tagz[0] != "")
			{
				_currentLevelData.workshopData.tags = tagz.ToList();
			}
		}
		DXMLNode chanceElement = lev.Element("Chance");
		if (chanceElement != null)
		{
			_currentLevelData.proceduralData.chance = Convert.ToSingle(chanceElement.Value);
		}
		DXMLNode maDXMLNode = lev.Element("MaxPerLev");
		if (maDXMLNode != null)
		{
			_currentLevelData.proceduralData.maxPerLevel = Convert.ToInt32(maDXMLNode.Value);
		}
		DXMLNode singleElement = lev.Element("Single");
		if (singleElement != null)
		{
			_currentLevelData.proceduralData.enableSingle = Convert.ToBoolean(singleElement.Value);
		}
		DXMLNode multiElement = lev.Element("Multi");
		if (multiElement != null)
		{
			_currentLevelData.proceduralData.enableMulti = Convert.ToBoolean(multiElement.Value);
		}
		DXMLNode canMirrorElement = lev.Element("CanMirror");
		if (canMirrorElement != null)
		{
			_currentLevelData.proceduralData.canMirror = Convert.ToBoolean(canMirrorElement.Value);
		}
		DXMLNode isMirroredElement = lev.Element("IsMirrored");
		if (isMirroredElement != null)
		{
			_currentLevelData.proceduralData.isMirrored = Convert.ToBoolean(isMirroredElement.Value);
		}
		_loadingLevel = true;
		IEnumerable<DXMLNode> objectsNode = lev.Elements("Objects");
		if (objectsNode != null)
		{
			foreach (DXMLNode elly in objectsNode.Elements("Object"))
			{
				AddObject(Thing.LegacyLoadThing(elly));
			}
		}
		_loadingLevel = false;
		_editorLoadFinished = true;
		if (!_looseClear)
		{
			CenterView();
		}
	}

	private void CenterView()
	{
		base.camera.width = _gridW * 16;
		base.camera.height = base.camera.width / Resolution.current.aspect;
		base.camera.centerX = base.camera.width / 2f - 8f;
		base.camera.centerY = base.camera.height / 2f - 8f;
		float wid = base.camera.width;
		float hig = base.camera.height;
		base.camera.width *= 0.3f;
		base.camera.height *= 0.3f;
		base.camera.centerX -= (base.camera.width - wid) / 2f;
		base.camera.centerY -= (base.camera.height - hig) / 2f;
		if (_sizeRestriction.x > 0f)
		{
			base.camera.center = (_topLeftMost + _bottomRightMost) / 2f;
		}
	}

	public static Texture2D LoadPreview(string s)
	{
		try
		{
			if (s != null)
			{
				MemoryStream stream = new MemoryStream(Convert.FromBase64String(s));
				return Texture2D.FromStream(Graphics.device, stream);
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	public static Texture2D LegacyLoadPreview(DXMLNode e)
	{
		try
		{
			DXMLNode preview = e.Element("Preview");
			if (preview != null)
			{
				MemoryStream stream = new MemoryStream(Convert.FromBase64String(preview.Value));
				return Texture2D.FromStream(Graphics.device, stream);
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	public static string LegacyLoadPreviewString(DXMLNode e)
	{
		try
		{
			return e.Element("Preview")?.Value;
		}
		catch
		{
			return null;
		}
	}

	public static string ScriptToString(byte[] scriptData)
	{
		try
		{
			return Convert.ToBase64String(scriptData);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static byte[] StringToScript(string script)
	{
		try
		{
			return Convert.FromBase64String(script);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static string BytesToString(byte[] pData)
	{
		try
		{
			return Convert.ToBase64String(pData);
		}
		catch (Exception)
		{
			return "";
		}
	}

	public static byte[] StringToBytes(string pString)
	{
		try
		{
			return Convert.FromBase64String(pString);
		}
		catch
		{
			return null;
		}
	}

	public static string TextureToString(Texture2D tex)
	{
		try
		{
			MemoryStream s = new MemoryStream();
			tex.SaveAsPng(s, tex.Width, tex.Height);
			s.Flush();
			return Convert.ToBase64String(s.ToArray());
		}
		catch (Exception)
		{
			return "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAPUExURQAAAGwXbeBu4P///8AgLYwkid8AAAC9SURBVDhPY2RgYPgPxGQDsAE54rkQHhCUhBdDWRDQs7IXyoIAZHmFSQoMTFA2BpCfKA/Gk19MAmNcAKsBII0HFfVQMC5DwF54kPcAwgMCmGZswP7+JYZciTwoj4FhysvJuL0AAiANIIwPYBgAsgGmEdk2XACrC0AaidEMAnijETk8YC4iKRrRNWMDeAORGIDTgIf5D4kKTIx0AEu6oISD7AWQgSCAnLQJpgNiAE4DQM6GeQFmOzZAYXZmYAAAEzJYPzQv17kAAAAASUVORK5CYII=";
		}
	}

	public static Texture2D StringToTexture(string tex)
	{
		if (string.IsNullOrWhiteSpace(tex))
		{
			return null;
		}
		try
		{
			MemoryStream stream = new MemoryStream(Convert.FromBase64String(tex));
			return Texture2D.FromStream(Graphics.device, stream);
		}
		catch
		{
			return null;
		}
	}

	public static string TextureToMassiveBitmapString(Texture2D tex)
	{
		Color[] colors = new Color[tex.Width * tex.Height];
		tex.GetData(colors);
		return TextureToMassiveBitmapString(colors, tex.Width, tex.Height);
	}

	public static string TextureToMassiveBitmapString(Color[] colors, int width, int height)
	{
		try
		{
			BitBuffer stream = new BitBuffer(allowPacking: false);
			stream.Write(kMassiveBitmapStringHeader);
			stream.Write(width);
			stream.Write(height);
			bool hasColor = false;
			Color currentColor = default(Color);
			int num = 0;
			foreach (Color c in colors)
			{
				if (!hasColor || currentColor != c)
				{
					currentColor = c;
					if (hasColor)
					{
						stream.Write(num);
						num = 0;
					}
					stream.WriteRGBColor(c);
					hasColor = true;
				}
				num++;
			}
			stream.Write(num);
			return Convert.ToBase64String(stream.GetBytes());
		}
		catch (Exception)
		{
			return "";
		}
	}

	public static Texture2D MassiveBitmapStringToTexture(string pTexture)
	{
		try
		{
			BitBuffer data = new BitBuffer(Convert.FromBase64String(pTexture));
			if (data.lengthInBytes < 8)
			{
				throw new Exception("(Editor.MassiveBitmapStringToTexture) Preview texture is empty...");
			}
			if (data.ReadLong() != kMassiveBitmapStringHeader)
			{
				throw new Exception("(Editor.MassiveBitmapStringToTexture) Header was invalid.");
			}
			int wide = data.ReadInt();
			int high = data.ReadInt();
			Texture2D tex = new Texture2D(Graphics.device, wide, high);
			Color[] colors = new Color[tex.Width * tex.Height];
			int idx = 0;
			do
			{
				Color c = data.ReadRGBColor();
				int num = data.ReadInt();
				for (int i = 0; i < num; i++)
				{
					colors[idx] = c;
					idx++;
				}
			}
			while (data.position != data.lengthInBytes);
			tex.SetData(colors);
			return tex;
		}
		catch
		{
			return null;
		}
	}

	public LevelData CreateSaveData(bool isTempSaveForPlayTestMode = false)
	{
		Level curLevel = Level.core.currentLevel;
		Level.core.currentLevel = this;
		_currentLevelData.SetExtraHeaderInfo(new LevelMetaData());
		_currentLevelData.Header<LevelMetaData>().type = GetLevelType();
		_currentLevelData.Header<LevelMetaData>().size = GetLevelSize();
		_currentLevelData.Header<LevelMetaData>().online = LevelIsOnlineCapable();
		_currentLevelData.Header<LevelMetaData>().guid = _currentLevelData.metaData.guid;
		_currentLevelData.Header<LevelMetaData>().workshopID = _currentLevelData.metaData.workshopID;
		_currentLevelData.Header<LevelMetaData>().deathmatchReady = _currentLevelData.metaData.deathmatchReady;
		_currentLevelData.Header<LevelMetaData>().onlineMode = _currentLevelData.metaData.onlineMode;
		_currentLevelData.RerouteMetadata(_currentLevelData.Header<LevelMetaData>());
		_currentLevelData.metaData.hasCustomArt = false;
		CustomTileData dat = Custom.GetData(0, CustomType.Block);
		if (dat != null && dat.path != null && dat.texture != null)
		{
			dat.ApplyToChunk(_currentLevelData.customData.customTileset01Data);
			_currentLevelData.metaData.hasCustomArt = true;
			_currentLevelData.customData.customTileset01Data.ignore = false;
		}
		else
		{
			_currentLevelData.customData.customTileset01Data.ignore = true;
		}
		dat = Custom.GetData(1, CustomType.Block);
		if (dat != null && dat.path != null && dat.texture != null)
		{
			dat.ApplyToChunk(_currentLevelData.customData.customTileset02Data);
			_currentLevelData.metaData.hasCustomArt = true;
			_currentLevelData.customData.customTileset02Data.ignore = false;
		}
		else
		{
			_currentLevelData.customData.customTileset02Data.ignore = true;
		}
		dat = Custom.GetData(2, CustomType.Block);
		if (dat != null && dat.path != null && dat.texture != null)
		{
			dat.ApplyToChunk(_currentLevelData.customData.customTileset03Data);
			_currentLevelData.metaData.hasCustomArt = true;
			_currentLevelData.customData.customTileset03Data.ignore = false;
		}
		else
		{
			_currentLevelData.customData.customTileset03Data.ignore = true;
		}
		dat = Custom.GetData(0, CustomType.Background);
		if (dat != null && dat.path != null && dat.texture != null)
		{
			dat.ApplyToChunk(_currentLevelData.customData.customBackground01Data);
			_currentLevelData.metaData.hasCustomArt = true;
			_currentLevelData.customData.customBackground01Data.ignore = false;
		}
		else
		{
			_currentLevelData.customData.customBackground01Data.ignore = true;
		}
		dat = Custom.GetData(1, CustomType.Background);
		if (dat != null && dat.path != null && dat.texture != null)
		{
			dat.ApplyToChunk(_currentLevelData.customData.customBackground02Data);
			_currentLevelData.metaData.hasCustomArt = true;
			_currentLevelData.customData.customBackground02Data.ignore = false;
		}
		else
		{
			_currentLevelData.customData.customBackground02Data.ignore = true;
		}
		dat = Custom.GetData(2, CustomType.Background);
		if (dat != null && dat.path != null && dat.texture != null)
		{
			dat.ApplyToChunk(_currentLevelData.customData.customBackground03Data);
			_currentLevelData.metaData.hasCustomArt = true;
			_currentLevelData.customData.customBackground03Data.ignore = false;
		}
		else
		{
			_currentLevelData.customData.customBackground03Data.ignore = true;
		}
		dat = Custom.GetData(0, CustomType.Platform);
		if (dat != null && dat.path != null && dat.texture != null)
		{
			dat.ApplyToChunk(_currentLevelData.customData.customPlatform01Data);
			_currentLevelData.metaData.hasCustomArt = true;
			_currentLevelData.customData.customPlatform01Data.ignore = false;
		}
		else
		{
			_currentLevelData.customData.customPlatform01Data.ignore = true;
		}
		dat = Custom.GetData(1, CustomType.Platform);
		if (dat != null && dat.path != null && dat.texture != null)
		{
			dat.ApplyToChunk(_currentLevelData.customData.customPlatform02Data);
			_currentLevelData.metaData.hasCustomArt = true;
			_currentLevelData.customData.customPlatform02Data.ignore = false;
		}
		else
		{
			_currentLevelData.customData.customPlatform02Data.ignore = true;
		}
		dat = Custom.GetData(2, CustomType.Platform);
		if (dat != null && dat.path != null && dat.texture != null)
		{
			dat.ApplyToChunk(_currentLevelData.customData.customPlatform03Data);
			_currentLevelData.metaData.hasCustomArt = true;
			_currentLevelData.customData.customPlatform03Data.ignore = false;
		}
		else
		{
			_currentLevelData.customData.customPlatform03Data.ignore = true;
		}
		dat = Custom.GetData(0, CustomType.Parallax);
		if (dat != null && dat.path != null && dat.texture != null)
		{
			dat.ApplyToChunk(_currentLevelData.customData.customParallaxData);
			_currentLevelData.metaData.hasCustomArt = true;
			_currentLevelData.customData.customParallaxData.ignore = false;
		}
		else
		{
			_currentLevelData.customData.customParallaxData.ignore = true;
		}
		_currentLevelData.modData.workshopIDs.Clear();
		if (_things.Count > 0)
		{
			HashSet<Mod> modsUsed = new HashSet<Mod>();
			foreach (Thing thing in levelThings)
			{
				modsUsed.Add(ModLoader.GetModFromType(thing.GetType()));
				if (thing is IContainThings)
				{
					IContainThings things = (IContainThings)thing;
					if (things.contains == null)
					{
						continue;
					}
					foreach (Type type in things.contains)
					{
						modsUsed.Add(ModLoader.GetModFromType(type));
					}
				}
				else if (thing is IContainAThing)
				{
					IContainAThing contained = (IContainAThing)thing;
					if (contained.contains != null)
					{
						modsUsed.Add(ModLoader.GetModFromType(contained.contains));
					}
				}
			}
			modsUsed.RemoveWhere((Mod a) => a == null || a is CoreMod || a is DisabledMod);
			if (modsUsed.Count != 0)
			{
				foreach (Mod mod in modsUsed)
				{
					if (mod.configuration.workshopID != 0L || mod.workshopIDFacade != 0L)
					{
						_currentLevelData.modData.workshopIDs.Add((mod.workshopIDFacade != 0L) ? mod.workshopIDFacade : mod.configuration.workshopID);
					}
					else
					{
						_currentLevelData.modData.hasLocalMods = true;
					}
				}
			}
		}
		string weaponConfigString = "";
		string spawnerConfigString = "";
		int numArmor = 0;
		int numEquipment = 0;
		int numSpawns = 0;
		int numTeamSpawns = 0;
		int numLockedDoors = 0;
		int numKeys = 0;
		_currentLevelData.metaData.eightPlayer = false;
		_currentLevelData.metaData.eightPlayerRestricted = false;
		_currentLevelData.objects.objects.Clear();
		if (_levelThings.Count > 0)
		{
			new MultiMap<Type, Thing>();
			foreach (Thing t in _levelThings)
			{
				if (t is EightPlayer)
				{
					_currentLevelData.metaData.eightPlayer = true;
					_currentLevelData.metaData.eightPlayerRestricted = (t as EightPlayer).eightPlayerOnly.value;
				}
				if (!t.editorCanModify || t.processedByEditor)
				{
					continue;
				}
				if (_miniMode)
				{
					if (t is Key)
					{
						numKeys++;
					}
					else if (t is Door && (t as Door).locked)
					{
						numLockedDoors++;
					}
					else if (t is Equipment)
					{
						if (t is ChestPlate || t is Helmet || t is KnightHelmet)
						{
							numArmor++;
						}
						else
						{
							numEquipment++;
						}
					}
					else if (t is Gun)
					{
						if (weaponConfigString != "")
						{
							weaponConfigString += "|";
						}
						weaponConfigString += ModLoader.SmallTypeName(t.GetType());
					}
					else if (t is ItemSpawner)
					{
						ItemSpawner spawner = t as ItemSpawner;
						if (typeof(Gun).IsAssignableFrom(spawner.contains) && spawner.likelyhoodToExist == 1f && !spawner.randomSpawn)
						{
							if (spawner.spawnNum < 1 && spawner.spawnTime < 8f && spawner.isAccessible)
							{
								if (spawnerConfigString != "")
								{
									spawnerConfigString += "|";
								}
								spawnerConfigString += ModLoader.SmallTypeName(spawner.contains);
							}
							if (weaponConfigString != "")
							{
								weaponConfigString += "|";
							}
							weaponConfigString += ModLoader.SmallTypeName(spawner.contains);
						}
					}
					else if (t.GetType() == typeof(ItemBox))
					{
						ItemBox spawner2 = t as ItemBox;
						if (typeof(Gun).IsAssignableFrom(spawner2.contains) && spawner2.likelyhoodToExist == 1f && spawner2.isAccessible)
						{
							if (spawnerConfigString != "")
							{
								spawnerConfigString += "|";
							}
							spawnerConfigString += ModLoader.SmallTypeName(spawner2.contains);
							if (weaponConfigString != "")
							{
								weaponConfigString += "|";
							}
							weaponConfigString += ModLoader.SmallTypeName(spawner2.contains);
						}
					}
					else if (t is SpawnPoint)
					{
						numSpawns++;
					}
					else if (t is TeamSpawn)
					{
						numTeamSpawns++;
					}
				}
				t.processedByEditor = true;
			}
		}
		SerializeObjects(pAlternate: false);
		SerializeObjects(pAlternate: true);
		_currentLevelData.proceduralData.sideMask = 0;
		if (_miniMode)
		{
			int sideMask = 0;
			if (_pathNorth)
			{
				sideMask |= 1;
			}
			if (_pathEast)
			{
				sideMask |= 2;
			}
			if (_pathSouth)
			{
				sideMask |= 4;
			}
			if (_pathWest)
			{
				sideMask |= 8;
			}
			_currentLevelData.proceduralData.sideMask = sideMask;
			_currentLevelData.proceduralData.weaponConfig = weaponConfigString;
			_currentLevelData.proceduralData.spawnerConfig = spawnerConfigString;
			_currentLevelData.proceduralData.numArmor = numArmor;
			_currentLevelData.proceduralData.numEquipment = numEquipment;
			_currentLevelData.proceduralData.numSpawns = numSpawns;
			_currentLevelData.proceduralData.numTeamSpawns = numTeamSpawns;
			_currentLevelData.proceduralData.numLockedDoors = numLockedDoors;
			_currentLevelData.proceduralData.numKeys = numKeys;
		}
		if (previewCapture != null)
		{
			_currentLevelData.previewData.preview = TextureToString(previewCapture);
		}
		try
		{
			Content.doingTempSave = isTempSaveForPlayTestMode;
			Content.GeneratePreview(_currentLevelData, !isTempSaveForPlayTestMode);
			Content.doingTempSave = false;
		}
		catch (Exception)
		{
			DevConsole.Log(DCSection.General, "Error creating preview for level " + _currentLevelData.metaData.guid.ToString());
		}
		LevelData data = _currentLevelData.Clone();
		data.RerouteMetadata(data.Header<LevelMetaData>());
		if (isTempSaveForPlayTestMode)
		{
			data.metaData.guid = "tempPlayLevel";
		}
		Level.core.currentLevel = curLevel;
		return data;
	}

	public void SerializeObjects(bool pAlternate)
	{
		List<BinaryClassChunk> objects = (pAlternate ? _currentLevelData.proceduralData.openAirAlternateObjects.objects : _currentLevelData.objects.objects);
		List<Thing> things = (pAlternate ? _levelThingsAlternate : _levelThingsNormal);
		objects.Clear();
		if (things.Count <= 0)
		{
			return;
		}
		foreach (Thing item in things)
		{
			item.processedByEditor = false;
		}
		MultiMap<Type, Thing> groups = new MultiMap<Type, Thing>();
		foreach (Thing t in things)
		{
			if (t.editorCanModify && !t.processedByEditor)
			{
				t.processedByEditor = true;
				if (t.canBeGrouped)
				{
					groups.Add(t.GetType(), t);
				}
				else
				{
					objects.Add(t.Serialize());
				}
			}
		}
		foreach (KeyValuePair<Type, List<Thing>> pair in groups)
		{
			ThingContainer container = new ThingContainer(pair.Value, pair.Key);
			container.quickSerialize = minimalConversionLoad;
			objects.Add(container.Serialize());
		}
	}

	public bool Save(bool isTempSaveForPlayTestMode = false)
	{
		if (_saveName == "")
		{
			SaveAs();
		}
		else
		{
			saving = true;
			LevelData data = CreateSaveData(isTempSaveForPlayTestMode);
			data.customData.customTileset01Data.ignore = _currentLevelData.customData.customTileset01Data.ignore;
			data.customData.customTileset02Data.ignore = _currentLevelData.customData.customTileset02Data.ignore;
			data.customData.customTileset03Data.ignore = _currentLevelData.customData.customTileset03Data.ignore;
			data.customData.customBackground01Data.ignore = _currentLevelData.customData.customBackground01Data.ignore;
			data.customData.customBackground02Data.ignore = _currentLevelData.customData.customBackground02Data.ignore;
			data.customData.customBackground03Data.ignore = _currentLevelData.customData.customBackground03Data.ignore;
			data.customData.customPlatform01Data.ignore = _currentLevelData.customData.customPlatform01Data.ignore;
			data.customData.customPlatform02Data.ignore = _currentLevelData.customData.customPlatform02Data.ignore;
			data.customData.customPlatform03Data.ignore = _currentLevelData.customData.customPlatform03Data.ignore;
			data.customData.customParallaxData.ignore = _currentLevelData.customData.customParallaxData.ignore;
			data.SetPath(_saveName);
			if (!DuckFile.SaveChunk(data, _saveName))
			{
				_notify.Open("Could not save data.");
				return false;
			}
			if (!isTempSaveForPlayTestMode)
			{
				_currentLevelData.SetPath(_saveName);
			}
			Content.MapLevel(data.metaData.guid, data, LevelLocation.Custom);
			if (_additionalSaveDirectory != null && _saveName.LastIndexOf("assets/levels/") != -1)
			{
				string newName = _saveName.Substring(_saveName.LastIndexOf("assets/levels/") + "assets/levels/".Length);
				string newDir = Directory.GetCurrentDirectory() + "/Content/levels/" + newName;
				DuckFile.CreatePath(newDir);
				File.Copy(_saveName, newDir, overwrite: true);
				File.SetAttributes(_saveName, FileAttributes.Normal);
			}
			if (_miniMode && !_doingResave)
			{
				LevelGenerator.ReInitialize();
			}
			foreach (Thing levelThing in _levelThings)
			{
				levelThing.processedByEditor = false;
			}
			saving = false;
			if (!isTempSaveForPlayTestMode)
			{
				hasUnsavedChanges = false;
			}
		}
		return true;
	}

	public static LevelMetaData ReadLevelMetadata(byte[] pData, bool pNewMetadataOnly = false)
	{
		try
		{
			LevelData lev = DuckFile.LoadLevel(pData, pHeaderOnly: true);
			if (lev.GetExtraHeaderInfo() != null && lev.GetExtraHeaderInfo() is LevelMetaData)
			{
				return lev.GetExtraHeaderInfo() as LevelMetaData;
			}
			if (pNewMetadataOnly)
			{
				return null;
			}
			lev = DuckFile.LoadLevel(pData, pHeaderOnly: false);
			if (lev != null)
			{
				return lev.metaData;
			}
		}
		catch (Exception)
		{
		}
		DevConsole.Log(DCSection.General, "Editor failed loading metadata from byte[].");
		return null;
	}

	public static LevelMetaData ReadLevelMetadata(string pFile, bool pNewMetadataOnly = false)
	{
		try
		{
			LevelData lev = DuckFile.LoadLevel(pFile, pHeaderOnly: true);
			if (lev.GetExtraHeaderInfo() != null && lev.GetExtraHeaderInfo() is LevelMetaData)
			{
				return lev.GetExtraHeaderInfo() as LevelMetaData;
			}
			if (pNewMetadataOnly)
			{
				return null;
			}
			lev = DuckFile.LoadLevel(pFile, pHeaderOnly: false);
			if (lev != null)
			{
				return lev.metaData;
			}
		}
		catch (Exception)
		{
		}
		DevConsole.Log(DCSection.General, "Editor failed loading metadata from level (" + pFile + ")");
		return null;
	}

	public static LevelMetaData ReadLevelMetadata(LevelData pData)
	{
		if (pData == null)
		{
			return null;
		}
		try
		{
			if (pData.GetExtraHeaderInfo() != null && pData.GetExtraHeaderInfo() is LevelMetaData)
			{
				return pData.GetExtraHeaderInfo() as LevelMetaData;
			}
			return pData.metaData;
		}
		catch (Exception)
		{
		}
		DevConsole.Log(DCSection.General, "Editor failed loading metadata from level data.");
		return null;
	}

	public static void Delete(string file)
	{
		file = file.Replace('\\', '/');
		while (file.StartsWith("/"))
		{
			file = file.Substring(1);
		}
		string previewFile = "";
		LevelMetaData data = ReadLevelMetadata(file);
		if (data != null)
		{
			activatedLevels.RemoveAll((string x) => x == data.guid);
			previewFile = DuckFile.editorPreviewDirectory + data.guid;
		}
		File.SetAttributes(file, FileAttributes.Normal);
		DuckFile.Delete(file);
		if (File.Exists(previewFile))
		{
			File.SetAttributes(previewFile, FileAttributes.Normal);
			File.Delete(previewFile);
		}
	}

	public void SaveAs()
	{
		_fileDialog.Open(_initialDirectory, _initialDirectory, save: true);
		DoMenuClose();
		_closeMenu = false;
	}

	public void Load()
	{
		_fileDialog.Open(_initialDirectory, _initialDirectory, save: false);
		DoMenuClose();
		_closeMenu = false;
	}

	public string SaveTempVersion()
	{
		string name = _saveName;
		string tempName = (_saveName = Directory.GetCurrentDirectory() + "\\Content\\_tempPlayLevel.lev");
		Save(isTempSaveForPlayTestMode: true);
		_saveName = name;
		return tempName;
	}

	public void Play()
	{
		if (!_runLevelAnyway && !arcadeMachineMode && _levelThings.FirstOrDefault((Thing x) => x is FreeSpawn || x is TeamSpawn || x is CustomCamera) == null)
		{
			CloseMenu();
			ShowNoSpawnsDialogue();
			return;
		}
		isTesting = true;
		string name = "";
		if (_miniMode && _procContext != null)
		{
			LevelGenerator.ReInitialize();
			_centerTile = LevelGenerator.LoadInTile(SaveTempVersion());
			name = "RANDOM";
		}
		else
		{
			name = SaveTempVersion();
		}
		CloseMenu();
		RunTestLevel(name);
	}

	public virtual void RunTestLevel(string name)
	{
		isTesting = true;
		Level.current = new TestArea(this, name, _procSeed, _centerTile);
		Level.current.AddThing(new EditorTestLevel(this));
	}

	public static MemoryStream GetCompressedActiveLevelData()
	{
		MemoryStream stream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(new GZipStream(stream, CompressionMode.Compress));
		foreach (string l in activatedLevels)
		{
			writer.Write(value: true);
			writer.Write(l);
			byte[] data = File.ReadAllBytes(DuckFile.levelDirectory + l + ".lev");
			writer.Write(data.Length);
			writer.Write(data);
		}
		writer.Write(value: false);
		return stream;
	}

	public static MemoryStream GetCompressedLevelData(string level)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(new GZipStream(memoryStream, CompressionMode.Compress));
		binaryWriter.Write(level);
		byte[] data = File.ReadAllBytes(DuckFile.levelDirectory + level + ".lev");
		binaryWriter.Write(data.Length);
		binaryWriter.Write(data);
		return memoryStream;
	}

	public static ReceivedLevelInfo ReadCompressedLevelData(MemoryStream stream)
	{
		stream.Position = 0L;
		BinaryReader binaryReader = new BinaryReader(new GZipStream(stream, CompressionMode.Decompress));
		string levelName = binaryReader.ReadString();
		int length = binaryReader.ReadInt32();
		LevelData doc = DuckFile.LoadLevel(binaryReader.ReadBytes(length));
		return new ReceivedLevelInfo
		{
			data = doc,
			name = levelName
		};
	}

	public static uint Checksum(byte[] data)
	{
		return CRC32.Generate(data);
	}

	public static uint Checksum(byte[] data, int start, int length)
	{
		return CRC32.Generate(data, start, length);
	}

	public static void MapThing(Thing t)
	{
		_thingMap[t.GetType()] = t;
	}

	public static Thing GetThing(Type t)
	{
		Thing thing = null;
		_thingMap.TryGetValue(t, out thing);
		return thing;
	}

	public static List<ClassMember> GetMembers<T>()
	{
		return GetMembers(typeof(T));
	}

	public static List<ClassMember> GetMembers(Type t)
	{
		List<ClassMember> ret = null;
		if (_classMembers.TryGetValue(t, out ret))
		{
			return ret;
		}
		_classMemberNames[t] = new Dictionary<string, ClassMember>();
		ret = new List<ClassMember>();
		FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		PropertyInfo[] properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		FieldInfo[] array = fields;
		foreach (FieldInfo field in array)
		{
			ClassMember mem = new ClassMember(field.Name, t, field);
			_classMemberNames[t][field.Name] = mem;
			ret.Add(mem);
		}
		PropertyInfo[] array2 = properties;
		foreach (PropertyInfo property in array2)
		{
			ClassMember mem2 = new ClassMember(property.Name, t, property);
			_classMemberNames[t][property.Name] = mem2;
			ret.Add(mem2);
		}
		_classMembers[t] = ret;
		return ret;
	}

	public static List<ClassMember> GetStaticMembers(Type t)
	{
		List<ClassMember> ret = null;
		if (_staticClassMembers.TryGetValue(t, out ret))
		{
			return ret;
		}
		_classMemberNames[t] = new Dictionary<string, ClassMember>();
		ret = new List<ClassMember>();
		FieldInfo[] fields = t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		PropertyInfo[] properties = t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		FieldInfo[] array = fields;
		foreach (FieldInfo field in array)
		{
			ClassMember mem = new ClassMember(field.Name, t, field);
			_classMemberNames[t][field.Name] = mem;
			ret.Add(mem);
		}
		PropertyInfo[] array2 = properties;
		foreach (PropertyInfo property in array2)
		{
			ClassMember mem2 = new ClassMember(property.Name, t, property);
			_classMemberNames[t][property.Name] = mem2;
			ret.Add(mem2);
		}
		_staticClassMembers[t] = ret;
		return ret;
	}

	public static ClassMember GetMember<T>(string name)
	{
		return GetMember(typeof(T), name);
	}

	public static ClassMember GetMember(Type t, string name)
	{
		Dictionary<string, ClassMember> ret = null;
		if (!_classMemberNames.TryGetValue(t, out ret))
		{
			GetMembers(t);
			if (!_classMemberNames.TryGetValue(t, out ret))
			{
				return null;
			}
		}
		ClassMember mem = null;
		ret.TryGetValue(name, out mem);
		return mem;
	}

	internal static Type GetType(string name)
	{
		return ModLoader.GetType(name);
	}

	internal static Type DeSerializeTypeName(string serializedTypeName)
	{
		if (serializedTypeName == "")
		{
			return null;
		}
		return GetType(serializedTypeName);
	}

	internal static string SerializeTypeName(Type t)
	{
		if (t == null)
		{
			return "";
		}
		return ModLoader.SmallTypeName(t);
	}

	public static void CopyClass(object source, object destination)
	{
		FieldInfo[] fields = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fi in fields)
		{
			fi.SetValue(destination, fi.GetValue(source));
		}
	}

	public static IEnumerable<Type> GetSubclasses(Type parentType)
	{
		return (from t in DG.assemblies.SelectMany((Assembly assembly) => assembly.GetTypes())
			where t.IsSubclassOf(parentType)
			orderby t.FullName
			select t).ToArray();
	}

	public static IEnumerable<Type> GetSubclassesAndInterfaces(Type parentType)
	{
		return (from t in DG.assemblies.SelectMany((Assembly assembly) => assembly.GetTypes())
			where parentType.IsAssignableFrom(t)
			orderby t.FullName
			select t).ToArray();
	}

	public static AccessorInfo GetAccessorInfo(Type t, string name, FieldInfo field = null, PropertyInfo property = null)
	{
		AccessorInfo accessor = null;
		Dictionary<string, AccessorInfo> functions = null;
		if (_accessorCache.TryGetValue(t, out functions))
		{
			if (functions.TryGetValue(name, out accessor))
			{
				return accessor;
			}
		}
		else
		{
			_accessorCache[t] = new Dictionary<string, AccessorInfo>();
		}
		accessor = CreateAccessor(field, property, t, name);
		_accessorCache[t][name] = accessor;
		return accessor;
	}

	public static AccessorInfo CreateAccessor(FieldInfo field, PropertyInfo property, Type t, string name)
	{
		if (field == null && property == null)
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			field = t.GetField(name, flags);
			if (field == null)
			{
				property = t.GetProperty(name, flags);
			}
		}
		AccessorInfo accessor = null;
		if (field != null)
		{
			accessor = new AccessorInfo();
			accessor.type = field.FieldType;
			accessor.setAccessor = BuildSetAccessorField(t, field);
			accessor.getAccessor = BuildGetAccessorField(t, field);
		}
		else if (property != null)
		{
			accessor = new AccessorInfo();
			accessor.type = property.PropertyType;
			MethodInfo dasSetter = property.GetSetMethod(nonPublic: true);
			if (dasSetter != null)
			{
				accessor.setAccessor = BuildSetAccessorProperty(t, dasSetter);
			}
			accessor.getAccessor = BuildGetAccessorProperty(t, property);
		}
		return accessor;
	}

	public static Action<object, object> BuildSetAccessorProperty(Type t, MethodInfo method)
	{
		ParameterExpression obj = Expression.Parameter(typeof(object), "o");
		ParameterExpression value = Expression.Parameter(typeof(object));
		return Expression.Lambda<Action<object, object>>(Expression.Call(method.IsStatic ? null : Expression.Convert(obj, method.DeclaringType), method, Expression.Convert(value, method.GetParameters()[0].ParameterType)), new ParameterExpression[2] { obj, value }).Compile();
	}

	public static Action<object, object> BuildSetAccessorField(Type t, FieldInfo field)
	{
		ParameterExpression targetExp = Expression.Parameter(typeof(object), "target");
		ParameterExpression valueExp = Expression.Parameter(typeof(object), "value");
		return Expression.Lambda<Action<object, object>>(Expression.Assign(Expression.Field(field.IsStatic ? null : Expression.Convert(targetExp, t), field), Expression.Convert(valueExp, field.FieldType)), new ParameterExpression[2] { targetExp, valueExp }).Compile();
	}

	public static Func<object, object> BuildGetAccessorProperty(Type t, PropertyInfo property)
	{
		if (property.GetGetMethod(nonPublic: true) == null)
		{
			return null;
		}
		ParameterExpression obj = Expression.Parameter(typeof(object), "o");
		return Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Property(property.GetGetMethod(nonPublic: true).IsStatic ? null : Expression.Convert(obj, t), property), typeof(object)), new ParameterExpression[1] { obj }).Compile();
	}

	public static Func<object, object> BuildGetAccessorField(Type t, FieldInfo field)
	{
		ParameterExpression obj = Expression.Parameter(typeof(object), "o");
		return Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Field(field.IsStatic ? null : Expression.Convert(obj, t), field), typeof(object)), new ParameterExpression[1] { obj }).Compile();
	}

	private static object GetDefaultValue(Type t)
	{
		if (t.IsValueType)
		{
			return Activator.CreateInstance(t);
		}
		return null;
	}

	public static Thing CreateThing(Type t)
	{
		ThingConstructor c = null;
		if (_defaultConstructors.TryGetValue(t, out c))
		{
			return c();
		}
		return Activator.CreateInstance(t, GetConstructorParameters(t)) as Thing;
	}

	public static Thing CreateThing(Type t, object[] p)
	{
		return Activator.CreateInstance(t, p) as Thing;
	}

	public static Thing GetOrCreateTypeInstance(Type t)
	{
		Thing ins = null;
		if (!_thingMap.TryGetValue(t, out ins) && CreateObject(t) is Thing thing)
		{
			_thingMap[t] = thing;
			ins = thing;
		}
		return ins;
	}

	public static object CreateObject(Type t)
	{
		Func<object> constructor = null;
		if (_constructorParameterExpressions.TryGetValue(t, out constructor))
		{
			return constructor();
		}
		return null;
	}

	private static void RegisterEditorFields(Type pType)
	{
		List<FieldInfo> fields = null;
		if (!EditorFieldsForType.TryGetValue(pType, out fields))
		{
			List<FieldInfo> list = (EditorFieldsForType[pType] = new List<FieldInfo>());
			fields = list;
		}
		foreach (Type baseType in AllBaseTypes[pType])
		{
			if (AllEditorFields.ContainsKey(baseType))
			{
				fields.AddRange(AllEditorFields[baseType]);
			}
		}
	}

	public static void InitializeConstructorLists()
	{
		//MonoMain.loadMessage = "Loading Constructor Lists";
		if (MonoMain.moddingEnabled)
		{
			//MonoMain.loadMessage = "Loading Constructor Lists";
			ThingTypes = ManagedContent.Things.SortedTypes.ToList();
		}
		else
		{
			ThingTypes = GetSubclasses(typeof(Thing)).ToList();
		}
		GroupThingTypes = new List<Type>();
		GroupThingTypes.AddRange(ThingTypes);
		AllBaseTypes = new Dictionary<Type, List<Type>>();
		AllEditorFields = new Dictionary<Type, IEnumerable<FieldInfo>>();
		AllStateFields = new Dictionary<Type, FieldInfo[]>();
		EditorFieldsForType = new Dictionary<Type, List<FieldInfo>>();
		Type editorFieldType = typeof(EditorProperty<>);
		Type stateFieldType = typeof(StateBinding);
		BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		ushort typeIndex = 2;
		Assembly main = Assembly.GetExecutingAssembly();
		string allTypesString = "";
		foreach (Type t in ThingTypes)
		{
			AllBaseTypes[t] = Thing.GetAllTypes(t);
			FieldInfo[] fields = t.GetFields(flags);
			AllEditorFields[t] = fields.Where((FieldInfo val) => val.FieldType.IsGenericType && val.FieldType.GetGenericTypeDefinition() == editorFieldType).ToArray();
			AllStateFields[t] = fields.Where((FieldInfo val) => val.FieldType == stateFieldType).ToArray();
			if (AllStateFields[t].Count() > 0)
			{
				IDToType[typeIndex] = t;
				if (t.Assembly == main)
				{
					allTypesString += t.Name;
				}
				typeIndex++;
			}
		}
		thingTypesHash = CRC32.Generate(allTypesString);
		foreach (Type t2 in ThingTypes)
		{
			if (t2.IsAbstract)
			{
				continue;
			}
			RegisterEditorFields(t2);
			ConstructorInfo[] constructors = t2.GetConstructors();
			foreach (ConstructorInfo info in constructors)
			{
				ParameterInfo[] parms = info.GetParameters();
				if (parms.Length == 0)
				{
					NewExpression newExp = Expression.New(info, (Expression[])null);
					LambdaExpression lambda = Expression.Lambda(typeof(ThingConstructor), (Expression)newExp, (ParameterExpression[])null);
					_defaultConstructors[t2] = (ThingConstructor)lambda.Compile();
					_constructorParameters[t2] = new object[0];
					continue;
				}
				Expression[] argsExp = new Expression[parms.Length];
				object[] parmList = new object[parms.Length];
				int index = 0;
				ParameterInfo[] array = parms;
				foreach (ParameterInfo paramInfo in array)
				{
					Type ty = paramInfo.ParameterType;
					parmList[index] = ((paramInfo.DefaultValue != null && paramInfo.DefaultValue.GetType() != typeof(DBNull)) ? paramInfo.DefaultValue : GetDefaultValue(ty));
					argsExp[index] = Expression.Constant(parmList[index], ty);
					index++;
				}
				NewExpression newExp2 = Expression.New(info, argsExp);
				LambdaExpression lambda2 = Expression.Lambda(typeof(ThingConstructor), (Expression)newExp2, (ParameterExpression[])null);
				_defaultConstructors[t2] = (ThingConstructor)lambda2.Compile();
				_constructorParameters[t2] = parmList;
			}
			MonoMain.loadyBits++;
		}
		Program.constructorsLoaded = _constructorParameters.Count;
		Program.thingTypes = ThingTypes.Count;
		foreach (Type t3 in ThingTypes)
		{
			ConstructorInfo[] constructors = t3.GetConstructors();
			foreach (ConstructorInfo info2 in constructors)
			{
				ParameterInfo[] parms2 = info2.GetParameters();
				if (parms2.Length == 0)
				{
					_constructorParameterExpressions[t3] = () => info2.Invoke(null);
					continue;
				}
				_ = new Expression[parms2.Length];
				int index2 = 0;
				object[] vals = new object[parms2.Length];
				ParameterInfo[] array = parms2;
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					Type ty2 = array[num2].ParameterType;
					vals[index2] = GetDefaultValue(ty2);
					index2++;
				}
				_constructorParameterExpressions[t3] = () => info2.Invoke(vals);
			}
			MonoMain.loadyBits++;
		}
	}

	public static void InitializePlaceableList()
	{
		if (_placeables == null)
		{
			InitializeConstructorLists();
			InitializePlaceableGroup();
		}
	}

	public static void InitializePlaceableGroup()
	{
		AutoUpdatables.ignoreAdditions = true;
		//MonoMain.loadMessage = "Loading Editor Groups";
		_placeables = new EditorGroup(null, null);
		AutoUpdatables.ignoreAdditions = false;
		if (!_clearOnce)
		{
			AutoUpdatables.Clear();
			_clearOnce = true;
		}
		_listLoaded = true;
	}

	public static bool HasConstructorParameter(Type t)
	{
		return _constructorParameters.ContainsKey(t);
	}

	public static object[] GetConstructorParameters(Type t)
	{
		object[] ret = null;
		_constructorParameters.TryGetValue(t, out ret);
		if (ret == null)
		{
			int localTypesCount = 0;
			try
			{
				if (MonoMain.moddingEnabled)
				{
					ThingTypes = ManagedContent.Things.SortedTypes.ToList();
				}
				else
				{
					ThingTypes = GetSubclasses(typeof(Thing)).ToList();
				}
				localTypesCount = ThingTypes.Count;
			}
			catch (Exception)
			{
			}
			throw new Exception("Error loading constructor parameters for type " + t.ToString() + "(" + _constructorParameters.Count + " parms vs " + Program.thingTypes + ", " + Program.constructorsLoaded + ", " + localTypesCount + " things vs " + Program.thingTypes + ")");
		}
		return ret;
	}
}
