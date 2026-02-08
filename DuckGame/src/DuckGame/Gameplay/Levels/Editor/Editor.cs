using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

namespace DuckGame;

public class Editor : Level
{
    #region Private Nesteds
    enum EditorTouchState
    {
        Normal,
        OpenMenu,
        Eyedropper,
        EditObject,
        OpenLevel,
        PickTile
    }

    class EditorTouchButton
    {
        public string caption;

        public string explanation;

        public Vector2 position;

        public Vector2 size;

        public bool threeFingerGesture;

        public EditorTouchState state;
    }

    delegate Thing ThingConstructor();
    #endregion

    #region Public Fields
    public static bool editingContent;
    public static bool active;
    public static bool selectingLevel;
    public static bool hasUnsavedChanges;
    public static bool enteringText;
    public static bool clickedMenu;
    public static bool clickedContextBackground;
    public static bool hoverUI;
    public static bool saving;
    public static bool editorDraw;
    public static bool hoverTextBox;
    public static bool ignorePinning;
    public static bool reopenContextMenu;
    public static bool copying;
    public static bool skipFrame;
    public static bool fakeTouch;
    public static bool _clickedTouchButton;
    public static bool tookInput;
    public static bool didUIScroll;
    public static bool waitForNoTouchInput;
    public static bool bigInterfaceMode;
    public static bool hoverMiniButton;
    public static bool isTesting;

    public static int _procXPos = 1;
    public static int _procYPos = 1;
    public static int _procTilesWide = 3;
    public static int _procTilesHigh = 3;
    public static int placementLimit;

    public static uint thingTypesHash;

    public static long kMassiveBitmapStringHeader = 4967129034509872376L;

    public static EditorInput inputMode = EditorInput.Gamepad;

    public static string placementItemDetails = "";
    public static string tooltip;

    public static Vector2 openPosition = Vector2.Zero;

    public static Texture2D previewCapture;
    public static LevelData _currentLevelData = new();
    public static Thing openContextThing;
    public static Thing pretendPinned;

    public static List<Type> ThingTypes;
    public static List<Type> GroupThingTypes;
    public static Dictionary<Type, List<Type>> AllBaseTypes;
    public static Dictionary<Type, IEnumerable<FieldInfo>> AllEditorFields;
    public static Dictionary<Type, List<FieldInfo>> EditorFieldsForType;
    public static Dictionary<Type, FieldInfo[]> AllStateFields;
    public static Map<ushort, Type> IDToType = [];
    public static Dictionary<Type, Thing> _typeInstances = [];
    public static Dictionary<Type, Dictionary<string, AccessorInfo>> _accessorCache = [];

    public bool clicked;
    public bool minimalConversionLoad;
    public bool _pathNorth;
    public bool _pathSouth;
    public bool _pathEast;
    public bool _pathWest;
    public bool _miniMode;
    public bool editingOpenAirVariation;
    public bool hadGUID;
    public bool searching;
    public bool tabletMode;
    public bool _doGen;

    public int generatorComplexity;
    public int placementTotalCost;

    public Vector2 _genSize = new(3);
    public Vector2 _genTilePos = new(1);
    public Vector2 _editTilePos = new(1);
    public Vector2 _prevEditTilePos = new(1);

    public MaterialSelection _selectionMaterial;
    public MaterialSelection _selectionMaterialPaste;

    public HashSet<Thing> _currentDragSelectionHover = [];
    public HashSet<Thing> _currentDragSelectionHoverAdd = [];
    #endregion

    #region Protected Fields
    protected int _procSeed;

    protected RandomLevelData _centerTile;

    protected List<Thing> _levelThingsNormal = [];
    protected List<Thing> _levelThingsAlternate = [];
    #endregion

    #region Private Fields
    static bool _listLoaded;
    static bool _clearOnce;

    static int numPops;

    static string _initialDirectory;

    static EditorGroup _placeables;
    static ContextMenu _lockInput;
    static ContextMenu _lockInputChange;
    static InputProfile _input;

    static Stack<object> focusStack = new();
    static Dictionary<Type, List<MethodInfo>> _networkActionIndexes = [];
    static Dictionary<Type, Thing> _thingMap = [];
    static Dictionary<Type, List<ClassMember>> _classMembers = [];
    static Dictionary<Type, List<ClassMember>> _staticClassMembers = [];
    static Dictionary<Type, Dictionary<string, ClassMember>> _classMemberNames = [];
    static Dictionary<Type, object[]> _constructorParameters = [];
    static Dictionary<Type, ThingConstructor> _defaultConstructors = [];
    static Dictionary<Type, Func<object>> _constructorParameterExpressions = [];

    bool _updateEvenWhenInactive;
    bool _editorLoadFinished;
    bool _placementMode = true;
    bool _editMode;
    bool _copyMode;
    bool _loadingLevel;
    bool processingMirror;
    bool _isPaste;
    bool _looseClear;
    bool _closeMenu;
    bool _placingTiles;
    bool _dragMode;
    bool _deleteMode;
    bool _didPan;
    bool _menuOpen;
    bool _quitting;
    bool _editingOpenAirVariationPrev;
    bool _doingResave;
    bool bMouseInput;
    bool bGamepadInput;
    bool bTouchInput;
    bool _showPlacementMenu;
    bool _runLevelAnyway;
    bool firstClick;
    bool _twoFingerGesture;
    bool _twoFingerGestureStarting;
    bool _twoFingerZooming;
    bool _threeFingerGesture;
    bool _threeFingerGestureRelease;
    bool _prevTouch;
    bool _openTileSelector;
    bool clearedKeyboardStringForSearch;
    bool _openedEditMenu;
    bool _performCopypaste;
    bool _dragSelectShiftModifier;
    bool _leftSelectionDraw = true;
    bool rotateValid;
    bool _onlineSettingChanged;

    int _lastCommand = -1;
    int _gridW = 40;
    int _gridH = 24;
    int _hoverMode;
    int _prevProcX;
    int _prevProcY;
    int _loadPosX;
    int _loadPosY;
    int focusWait = 5;
    int _searchHoverIndex = -1;

    float _cellSize = 16;
    float _twoFingerSpacing;

    CursorMode _cursorMode;
    InputType dragModeInputType;
    InputType dragStartInputType;
    EditorTouchState _touchState;

    string _saveName = "";
    string _additionalSaveDirectory;
    string _prevSearchString = "";

    Vector2 _sizeRestriction = new(800, 640);
    Vector2 _topLeftMost = new(99999);
    Vector2 _bottomRightMost = new(-99999);
    Vector2 _camSize;
    Vector2 _panAnchor;
    Vector2 _tilePosition;
    Vector2 _tileDragContext = Vector2.MinValue;
    Vector2 _tilePositionPrev;
    Vector2 _tileDragDif;
    Vector2 _lastTilePosDraw;
    Vector2 middleClickPos;
    Vector2 lastMousePos = Vector2.Zero;
    Vector2 _selectionDragStart = Vector2.Zero;
    Vector2 _selectionDragEnd = Vector2.Zero;
    Vector2 _moveDragStart = Vector2.Zero;
    Vector2 _copyCenter;
    Vector2 pasteOffset;
    Rectangle _ultimateBounds;
    Vector2 _procDrawOffset = Vector2.Zero;

    EditorCam _editorCam;
    SpriteMap _cursor;
    SpriteMap _tileset;
    BitmapFont _font;
    ContextMenu _placementMenu;
    ContextMenu _objectMenu;
    BinaryClassChunk _eyeDropperSerialized;
    SaveFileDialog _saveForm = new();
    OpenFileDialog _loadForm = new();
    NotifyDialogue _notify;
    SpriteMap _editorButtons;
    Thing _placementType;
    MonoFileDialog _fileDialog;
    SteamUploadDialog _uploadDialog;
    Layer _gridLayer;
    Layer _procLayer;
    Layer _objectMenuLayer;
    Sprite _cantPlace;
    Sprite _sideArrow;
    Sprite _sideArrowHover;
    Sprite _die;
    Sprite _dieHover;
    Sprite _editorCurrency;
    HashSet<Thing> _selection = [];
    Sprite _singleBlock;
    Sprite _multiBlock;
    ContextMenu _lastHoverMenuOpen;
    Thing _hover;
    Thing _secondaryHover;
    Thing _move;
    MessageDialogue _noSpawnsDialogue;
    ContextMenu _hoverMenu;
    GameContext _procContext;
    TileButton _hoverButton;
    RandomLevelNode _currentMapNode;
    EditorTouchButton _activeTouchButton;
    EditorTouchButton _cancelButton;
    EditorTouchButton _editTilesButton;
    Thing _oldHover;
    Thing oldHover;
    Thing oldSecondaryHover;
    RenderTarget2D _procTarget;

    List<string> existingGUID = [];
    List<Command> _commands = [];
    List<Thing> _placeObjects = [];
    List<EditorTouchButton> _touchButtons = [];
    List<EditorTouchButton> _fileDialogButtons = [];
    List<ContextMenu.SearchPair> searchItems = [];
    List<BinaryClassChunk> _selectionCopy = [];
    List<Thing> _pasteBatch = [];
    #endregion

    #region Public Properties
    public static bool miniMode
    {
        get => current is Editor editor && editor._miniMode;
        set
        {
            if (current is Editor)
                (current as Editor)._miniMode = value;
        }
    }

    public static bool arcadeMachineMode
    {
        get
        {
            if (current is Editor e && e._levelThings.Count == 1 && e._levelThings[0] is ImportMachine)
                return true;
            return false;
        }
    }

    public static int clientLevelCount
    {
        get
        {
            if (!(bool)TeamSelect2.GetMatchSetting("clientlevelsenabled").value)
                return 0;
            int profileLevels = 0;
            if (DuckNetwork.profiles != null)
                foreach (Profile p in DuckNetwork.profiles)
                    if (p != null && p.connection != null && p.connection.status != ConnectionStatus.Disconnected)
                        profileLevels += p.numClientCustomLevels;
            return profileLevels;
        }
    }

    public static int customLevelCount => activatedLevels.Count + clientLevelCount;

    public static float interfaceSizeMultiplier =>
        inputMode != EditorInput.Touch ? 1 : 2;

    public static string initialDirectory => _initialDirectory;

    public static EditorGroup Placeables
    {
        get
        {
            while (!_listLoaded)
                Thread.Sleep(16);
            return _placeables;
        }
    }

    public static ContextMenu lockInput
    {
        get => _lockInput;
        set => _lockInputChange = value;
    }

    public static InputProfile input => _input;

    public static Layer objectMenuLayer => Main.editor._objectMenuLayer;

    public static List<string> activatedLevels => DuckNetwork.core._activatedLevels;

    public static Dictionary<Type, Thing> thingMap => _thingMap;

    public bool placementLimitReached =>
        (placementLimit > 0) && (placementTotalCost >= placementLimit);

    public bool _enableSingle
    {
        get => _currentLevelData.proceduralData.enableSingle;
        set => _currentLevelData.proceduralData.enableSingle = value;
    }

    public bool _enableMulti
    {
        get => _currentLevelData.proceduralData.enableMulti;
        set => _currentLevelData.proceduralData.enableMulti = value;
    }

    public bool _canMirror
    {
        get => _currentLevelData.proceduralData.canMirror;
        set => _currentLevelData.proceduralData.canMirror = value;
    }

    public bool _isMirrored
    {
        get => _currentLevelData.proceduralData.isMirrored;
        set => _currentLevelData.proceduralData.isMirrored = value;
    }

    public bool placementOutOfSizeRange => false; //TODO: Remove useless property

    public int _maxPerLevel
    {
        get => _currentLevelData.proceduralData.maxPerLevel;
        set => _currentLevelData.proceduralData.maxPerLevel = value;
    }

    public float cellSize
    {
        get => _cellSize;
        set => _cellSize = value;
    }

    public float _chance
    {
        get => _currentLevelData.proceduralData.chance;
        set => _currentLevelData.proceduralData.chance = value;
    }

    public string saveName
    {
        get => _saveName;
        set => _saveName = value;
    }

    public string additionalSaveDirectory => _additionalSaveDirectory;

    public Thing placementType
    {
        get => _placementType;
        set => (_placementType, _eyeDropperSerialized) = (value, null);
    }

    public MonoFileDialog fileDialog => _fileDialog;

    public List<Thing> levelThings => _levelThings;
    #endregion

    #region Protected Properties
    protected List<Thing> _levelThings
    {
        get
        {
            if (!editingOpenAirVariation)
                return _levelThingsNormal;
            return _levelThingsAlternate;
        }
    }
    #endregion

    #region Private Properties
    float width => (float)_gridW * _cellSize;

    float height => (float)_gridH * _cellSize;
    #endregion

    #region Public Methods
    public static void PopFocus()
    {
        numPops++;
    }

    public static void PopFocusNow()
    {
        if (focusStack.Count > 0)
            focusStack.Pop();
    }

    public static void PushFocus(object o)
    {
        focusStack.Push(o);
    }

    public static void InitializeConstructorLists()
    {
        if (MonoMain.moddingEnabled)
            ThingTypes = [.. ManagedContent.Things.SortedTypes];
        else
            ThingTypes = [.. GetSubclasses(typeof(Thing))];
        GroupThingTypes = [.. ThingTypes];
        AllBaseTypes = [];
        AllEditorFields = [];
        AllStateFields = [];
        EditorFieldsForType = [];
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
            AllEditorFields[t] = [.. fields.Where(val => val.FieldType.IsGenericType && val.FieldType.GetGenericTypeDefinition() == editorFieldType)];
            AllStateFields[t] = [.. fields.Where(val => val.FieldType == stateFieldType)];
            if (AllStateFields[t].Length > 0)
            {
                IDToType[typeIndex] = t;
                if (t.Assembly == main)
                    allTypesString += t.Name;
                typeIndex++;
            }
        }
        thingTypesHash = CRC32.Generate(allTypesString);
        foreach (Type t2 in ThingTypes)
        {
            if (t2.IsAbstract)
                continue;
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
                    _constructorParameters[t2] = [];
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
        _placeables = new EditorGroup(null, null);
        AutoUpdatables.ignoreAdditions = false;
        if (!_clearOnce)
        {
            AutoUpdatables.Clear();
            _clearOnce = true;
        }
        _listLoaded = true;
    }

    public static void Delete(string file)
    {
        file = file.Replace('\\', '/');
        while (file.StartsWith('/'))
            file = file[1..];
        string previewFile = "";
        LevelMetaData data = ReadLevelMetadata(file);
        if (data != null)
        {
            activatedLevels.RemoveAll(x => x == data.guid);
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

    public static void MapThing(Thing t)
    {
        _thingMap[t.GetType()] = t;
    }

    public static void CopyClass(object source, object destination)
    {
        FieldInfo[] fields = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (FieldInfo fi in fields)
            fi.SetValue(destination, fi.GetValue(source));
    }

    public static bool HasFocus()
    {
        return focusStack.Count != 0;
    }

    public static bool HasConstructorParameter(Type t)
    {
        return _constructorParameters.ContainsKey(t);
    }

    public static byte NetworkActionIndex(Type pType, MethodInfo pMethod)
    {
        int idx = GetNetworkActionMethods(pType).IndexOf(pMethod);
        if (idx >= 0)
            return (byte)idx;
        return byte.MaxValue;
    }

    public static int CalculatePlacementCost(Thing pObject)
    {
        return pObject.placementCost;
    }

    public static uint Checksum(byte[] data)
    {
        return CRC32.Generate(data);
    }

    public static uint Checksum(byte[] data, int start, int length)
    {
        return CRC32.Generate(data, start, length);
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

    public static string TextureToString(Texture2D tex)
    {
        try
        {
            MemoryStream s = new();
            tex.SaveAsPng(s, tex.Width, tex.Height);
            s.Flush();
            return Convert.ToBase64String(s.ToArray());
        }
        catch (Exception)
        {
            return "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAPUExURQAAAGwXbeBu4P///8AgLYwkid8AAAC9SURBVDhPY2RgYPgPxGQDsAE54rkQHhCUhBdDWRDQs7IXyoIAZHmFSQoMTFA2BpCfKA/Gk19MAmNcAKsBII0HFfVQMC5DwF54kPcAwgMCmGZswP7+JYZciTwoj4FhysvJuL0AAiANIIwPYBgAsgGmEdk2XACrC0AaidEMAnijETk8YC4iKRrRNWMDeAORGIDTgIf5D4kKTIx0AEu6oISD7AWQgSCAnLQJpgNiAE4DQM6GeQFmOzZAYXZmYAAAEzJYPzQv17kAAAAASUVORK5CYII=";
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
            BitBuffer stream = new(allowPacking: false);
            stream.Write(kMassiveBitmapStringHeader);
            stream.Write(width);
            stream.Write(height);
            bool hasColor = false;
            Color currentColor = default;
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

    public static object PeekFocus()
    {
        if (focusStack.Count > 0)
            return focusStack.Peek();
        return null;
    }

    public static object CreateObject(Type t)
    {
        if (_constructorParameterExpressions.TryGetValue(t, out Func<object> constructor))
            return constructor();
        return null;
    }

    public static Texture2D LoadPreview(string s)
    {
        try
        {
            if (s != null)
            {
                MemoryStream stream = new(Convert.FromBase64String(s));
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

    public static Texture2D StringToTexture(string tex)
    {
        if (string.IsNullOrWhiteSpace(tex))
            return null;
        try
        {
            MemoryStream stream = new(Convert.FromBase64String(tex));
            return Texture2D.FromStream(Graphics.device, stream);
        }
        catch
        {
            return null;
        }
    }

    public static Texture2D MassiveBitmapStringToTexture(string pTexture)
    {
        try
        {
            BitBuffer data = new(Convert.FromBase64String(pTexture));
            if (data.lengthInBytes < 8)
                throw new("(Editor.MassiveBitmapStringToTexture) Preview texture is empty...");
            if (data.ReadLong() != kMassiveBitmapStringHeader)
                throw new("(Editor.MassiveBitmapStringToTexture) Header was invalid.");
            int wide = data.ReadInt();
            int high = data.ReadInt();
            Texture2D tex = new(Graphics.device, wide, high);
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

    public static LevelMetaData ReadLevelMetadata(byte[] pData, bool pNewMetadataOnly = false)
    {
        try
        {
            LevelData lev = DuckFile.LoadLevel(pData, pHeaderOnly: true);
            if (lev.GetExtraHeaderInfo() != null && lev.GetExtraHeaderInfo() is LevelMetaData)
                return lev.GetExtraHeaderInfo() as LevelMetaData;
            if (pNewMetadataOnly)
                return null;
            lev = DuckFile.LoadLevel(pData, pHeaderOnly: false);
            if (lev != null)
                return lev.metaData;
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
                return lev.GetExtraHeaderInfo() as LevelMetaData;
            if (pNewMetadataOnly)
                return null;
            lev = DuckFile.LoadLevel(pFile, pHeaderOnly: false);
            if (lev != null)
                return lev.metaData;
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
            return null;
        try
        {
            if (pData.GetExtraHeaderInfo() != null && pData.GetExtraHeaderInfo() is LevelMetaData)
                return pData.GetExtraHeaderInfo() as LevelMetaData;
            return pData.metaData;
        }
        catch (Exception)
        {
        }
        DevConsole.Log(DCSection.General, "Editor failed loading metadata from level data.");
        return null;
    }

    public static MemoryStream GetCompressedActiveLevelData()
    {
        MemoryStream stream = new();
        BinaryWriter writer = new(new GZipStream(stream, CompressionMode.Compress));
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
        MemoryStream memoryStream = new();
        BinaryWriter binaryWriter = new(new GZipStream(memoryStream, CompressionMode.Compress));
        binaryWriter.Write(level);
        byte[] data = File.ReadAllBytes(DuckFile.levelDirectory + level + ".lev");
        binaryWriter.Write(data.Length);
        binaryWriter.Write(data);
        return memoryStream;
    }

    public static ReceivedLevelInfo ReadCompressedLevelData(MemoryStream stream)
    {
        stream.Position = 0L;
        BinaryReader binaryReader = new(new GZipStream(stream, CompressionMode.Decompress));
        string levelName = binaryReader.ReadString();
        int length = binaryReader.ReadInt32();
        LevelData doc = DuckFile.LoadLevel(binaryReader.ReadBytes(length));
        return new ReceivedLevelInfo
        {
            data = doc,
            name = levelName
        };
    }

    public static Thing GetThing(Type t)
    {
        _thingMap.TryGetValue(t, out Thing thing);
        return thing;
    }

    public static ClassMember GetMember<T>(string name)
    {
        return GetMember(typeof(T), name);
    }

    public static ClassMember GetMember(Type t, string name)
    {
        if (!_classMemberNames.TryGetValue(t, out Dictionary<string, ClassMember> ret))
        {
            GetMembers(t);
            if (!_classMemberNames.TryGetValue(t, out ret))
                return null;
        }
        ret.TryGetValue(name, out ClassMember mem);
        return mem;
    }

    public static AccessorInfo GetAccessorInfo(Type t, string name, FieldInfo field = null, PropertyInfo property = null)
    {
        AccessorInfo accessor;
        if (_accessorCache.TryGetValue(t, out Dictionary<string, AccessorInfo> functions))
        {
            if (functions.TryGetValue(name, out accessor))
                return accessor;
        }
        else
            _accessorCache[t] = [];
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
                property = t.GetProperty(name, flags);
        }
        AccessorInfo accessor = null;
        if (field != null)
        {
            accessor = new AccessorInfo
            {
                type = field.FieldType,
                setAccessor = BuildSetAccessorField(t, field),
                getAccessor = BuildGetAccessorField(t, field)
            };
        }
        else if (property != null)
        {
            accessor = new AccessorInfo
            {
                type = property.PropertyType
            };
            MethodInfo dasSetter = property.GetSetMethod(nonPublic: true);
            if (dasSetter != null)
                accessor.setAccessor = BuildSetAccessorProperty(t, dasSetter);
            accessor.getAccessor = BuildGetAccessorProperty(t, property);
        }
        return accessor;
    }

    public static MethodInfo MethodFromNetworkActionIndex(Type pType, byte pIndex)
    {
        List<MethodInfo> methods = GetNetworkActionMethods(pType);
        if (pIndex < methods.Count)
            return methods[pIndex];
        return null;
    }

    public static Thing CreateThing(Type t)
    {
        if (_defaultConstructors.TryGetValue(t, out ThingConstructor c))
            return c();
        return Activator.CreateInstance(t, GetConstructorParameters(t)) as Thing;
    }

    public static Thing CreateThing(Type t, object[] p)
    {
        return Activator.CreateInstance(t, p) as Thing;
    }

    public static Thing GetOrCreateTypeInstance(Type t)
    {
        if (!_thingMap.TryGetValue(t, out Thing ins) && CreateObject(t) is Thing thing)
        {
            _thingMap[t] = thing;
            ins = thing;
        }
        return ins;
    }

    public static Action<object, object> BuildSetAccessorProperty(Type t, MethodInfo method)
    {
        ParameterExpression obj = Expression.Parameter(typeof(object), "o");
        ParameterExpression value = Expression.Parameter(typeof(object));
        return Expression.Lambda<Action<object, object>>(Expression.Call(method.IsStatic ? null : Expression.Convert(obj, method.DeclaringType), method, Expression.Convert(value, method.GetParameters()[0].ParameterType)), [obj, value]).Compile();
    }

    public static Action<object, object> BuildSetAccessorField(Type t, FieldInfo field)
    {
        ParameterExpression targetExp = Expression.Parameter(typeof(object), "target");
        ParameterExpression valueExp = Expression.Parameter(typeof(object), "value");
        return Expression.Lambda<Action<object, object>>(Expression.Assign(Expression.Field(field.IsStatic ? null : Expression.Convert(targetExp, t), field), Expression.Convert(valueExp, field.FieldType)), [targetExp, valueExp]).Compile();
    }

    public static Func<object, object> BuildGetAccessorProperty(Type t, PropertyInfo property)
    {
        if (property.GetGetMethod(nonPublic: true) == null)
            return null;
        ParameterExpression obj = Expression.Parameter(typeof(object), "o");
        return Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Property(property.GetGetMethod(nonPublic: true).IsStatic ? null : Expression.Convert(obj, t), property), typeof(object)), [obj]).Compile();
    }

    public static Func<object, object> BuildGetAccessorField(Type t, FieldInfo field)
    {
        ParameterExpression obj = Expression.Parameter(typeof(object), "o");
        return Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Field(field.IsStatic ? null : Expression.Convert(obj, t), field), typeof(object)), [obj]).Compile();
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
                    ThingTypes = [.. ManagedContent.Things.SortedTypes];
                else
                    ThingTypes = [.. GetSubclasses(typeof(Thing))];
                localTypesCount = ThingTypes.Count;
            }
            catch (Exception)
            {
            }
            throw new Exception("Error loading constructor parameters for type " + t.ToString() + "(" + _constructorParameters.Count + " parms vs " + Program.thingTypes + ", " + Program.constructorsLoaded + ", " + localTypesCount + " things vs " + Program.thingTypes + ")");
        }
        return ret;
    }

    public static IEnumerable<Type> GetSubclasses(Type parentType)
    {
        return [.. (from t in DG.assemblies.SelectMany((Assembly assembly) => assembly.GetTypes())
                where t.IsSubclassOf(parentType)
                orderby t.FullName
                select t)];
    }

    public static IEnumerable<Type> GetSubclassesAndInterfaces(Type parentType)
    {
        return [.. (from t in DG.assemblies.SelectMany((Assembly assembly) => assembly.GetTypes())
                where parentType.IsAssignableFrom(t)
                orderby t.FullName
                select t)];
    }

    public static List<ClassMember> GetMembers<T>()
    {
        return GetMembers(typeof(T));
    }

    public static List<ClassMember> GetMembers(Type t)
    {
        if (_classMembers.TryGetValue(t, out List<ClassMember> ret))
        {
            return ret;
        }
        _classMemberNames[t] = [];
        ret = [];
        FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        PropertyInfo[] properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo[] array = fields;
        foreach (FieldInfo field in array)
        {
            ClassMember mem = new(field.Name, t, field);
            _classMemberNames[t][field.Name] = mem;
            ret.Add(mem);
        }
        PropertyInfo[] array2 = properties;
        foreach (PropertyInfo property in array2)
        {
            ClassMember mem2 = new(property.Name, t, property);
            _classMemberNames[t][property.Name] = mem2;
            ret.Add(mem2);
        }
        _classMembers[t] = ret;
        return ret;
    }

    public static List<ClassMember> GetStaticMembers(Type t)
    {
        if (_staticClassMembers.TryGetValue(t, out List<ClassMember> ret))
            return ret;
        _classMemberNames[t] = [];
        ret = [];
        FieldInfo[] fields = t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        PropertyInfo[] properties = t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo[] array = fields;
        foreach (FieldInfo field in array)
        {
            ClassMember mem = new(field.Name, t, field);
            _classMemberNames[t][field.Name] = mem;
            ret.Add(mem);
        }
        PropertyInfo[] array2 = properties;
        foreach (PropertyInfo property in array2)
        {
            ClassMember mem2 = new(property.Name, t, property);
            _classMemberNames[t][property.Name] = mem2;
            ret.Add(mem2);
        }
        _staticClassMembers[t] = ret;
        return ret;
    }

    public virtual void RunTestLevel(string name)
    {
        isTesting = true;
        current = new TestArea(this, name, _procSeed, _centerTile);
        current.AddThing(new EditorTestLevel(this));
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

    public override void Initialize()
    {
        while (!_listLoaded)
            Thread.Sleep(16);
        _editorCam = new EditorCam();
        camera = _editorCam;
        camera.InitializeToScreenAspect();
        _selectionMaterial = new MaterialSelection();
        _selectionMaterialPaste = new MaterialSelection
        {
            fade = 0.5f
        };
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
        _camSize = new Vector2(camera.width, camera.height);
        _font = new BitmapFont("biosFont", 8);
        _input = InputProfile.Get(InputProfile.MPPlayer1);
        _tilePosition = new Vector2(0);
        _tilePositionPrev = _tilePosition;
        _objectMenu = new PlacementMenu(0, 0);
        Add(_objectMenu);
        ContextMenu objectMenu = _objectMenu;
        bool visible = (_objectMenu.active = false);
        objectMenu.visible = visible;
        Add(new TileButton(0, 0, new FieldBinding(this, "_chance", 0f, 1f, 0.05f), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/dieBlock", 16, 16), "CHANCE - HOLD @SELECT@ AND MOVE @DPAD@", TileButtonAlign.TileGridBottomLeft));
        Add(new TileButton(0, 16, new FieldBinding(this, "_maxPerLevel", -1f, 8f, 1f), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/numBlock", 16, 16), "MAX IN LEVEL - HOLD @SELECT@ AND MOVE @DPAD@", TileButtonAlign.TileGridBottomLeft));
        Add(new TileButton(-16, 0, new FieldBinding(this, "_enableSingle"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/singleplayerBlock", 16, 16), "AVAILABLE IN SINGLE PLAYER - @SELECT@TOGGLE", TileButtonAlign.TileGridBottomRight));
        Add(new TileButton(0, 0, new FieldBinding(this, "_enableMulti"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/multiplayerBlock", 16, 16), "AVAILABLE IN MULTI PLAYER - @SELECT@TOGGLE", TileButtonAlign.TileGridBottomRight));
        Add(new TileButton(-16, 16, new FieldBinding(this, "_canMirror"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/canMirror", 16, 16), "TILE CAN BE MIRRORED - @SELECT@TOGGLE", TileButtonAlign.TileGridBottomRight));
        Add(new TileButton(0, 16, new FieldBinding(this, "_isMirrored"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/isMirrored", 16, 16), "PRE MIRRORED TILE - @SELECT@TOGGLE", TileButtonAlign.TileGridBottomRight));
        Add(new TileButton(0, 32, new FieldBinding(this, "editingOpenAirVariation"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/openAir", 16, 16), "OPEN AIR VARIATION - @SELECT@TOGGLE", TileButtonAlign.TileGridBottomRight));
        Add(new TileButton(0, 0, new FieldBinding(this, "_pathEast"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/sideArrow", 32, 16), "CONNECTS EAST - @SELECT@TOGGLE", TileButtonAlign.TileGridRight, 90f));
        Add(new TileButton(0, 0, new FieldBinding(this, "_pathWest"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/sideArrow", 32, 16), "CONNECTS WEST - @SELECT@TOGGLE", TileButtonAlign.TileGridLeft, -90f));
        Add(new TileButton(0, 0, new FieldBinding(this, "_pathNorth"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/sideArrow", 32, 16), "CONNECTS NORTH - @SELECT@TOGGLE", TileButtonAlign.TileGridTop));
        Add(new TileButton(0, 0, new FieldBinding(this, "_pathSouth"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/sideArrow", 32, 16), "CONNECTS SOUTH - @SELECT@TOGGLE", TileButtonAlign.TileGridBottom, 180f));
        Add(new TileButton(16, 0, new FieldBinding(this, "_genTilePos", 0f, 6f, 1f), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/moveBlock", 16, 16), "MOVE GEN - HOLD @SELECT@ AND MOVE @DPAD@", TileButtonAlign.TileGridTopLeft));
        Add(new TileButton(32, 0, new FieldBinding(this, "_editTilePos", 0f, 6f, 1f), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/editBlock", 16, 16), "MOVE GEN - HOLD @SELECT@ AND MOVE @DPAD@", TileButtonAlign.TileGridTopLeft));
        Add(new TileButton(48, 0, new FieldBinding(this, "generatorComplexity", 0f, 9f, 1f), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/dieBlockRed", 16, 16), "NUM TILES - HOLD @SELECT@ AND MOVE @DPAD@", TileButtonAlign.TileGridTopLeft));
        Add(new TileButton(0, 0, new FieldBinding(this, "_doGen"), new FieldBinding(this, "_miniMode"), new SpriteMap("Editor/regenBlock", 16, 16), "REGENERATE - HOLD @SELECT@ AND MOVE @DPAD@", TileButtonAlign.TileGridTopRight));
        _notify = new NotifyDialogue();
        Add(_notify);
        Vector2 buttonSizeAdd = new(12);
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
        _editTilesButton.size = new Vector2(Graphics.GetStringWidth(_editTilesButton.caption) + 6, 15) + buttonSizeAdd;
        _editTilesButton.position = Layer.HUD.camera.OffsetTL(10, 10);
        Vector2 buttonPlacement = Layer.HUD.camera.OffsetBR(-14, -14);
        for (int i = _touchButtons.Count - 1; i >= 0; i--)
        {
            EditorTouchButton button = _touchButtons[i];
            if (i == _touchButtons.Count - 1)
            {
                _cancelButton.size = new Vector2(Graphics.GetStringWidth(_cancelButton.caption) + 6, 15) + buttonSizeAdd;
                _cancelButton.position = buttonPlacement - _cancelButton.size;
            }
            button.size = new Vector2(Graphics.GetStringWidth(button.caption) + 6, 15) + buttonSizeAdd;
            button.position = buttonPlacement - button.size;
            buttonPlacement.X -= button.size.X + 4;
        }
        _initialDirectory = DuckFile.levelDirectory;
        _initialDirectory = Path.GetFullPath(_initialDirectory);
        _fileDialog = new MonoFileDialog();
        Add(_fileDialog);
        _uploadDialog = new SteamUploadDialog();
        Add(_uploadDialog);
        _editorButtons = new SpriteMap("editorButtons", 32, 32);
        _doingResave = true;
        _doingResave = false;
        ClearEverything();
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
        foreach (Thing thing in things)
            thing.DoEditorUpdate();
        if (lastMousePos == Vector2.Zero)
            lastMousePos = Mouse.position;
        if (clickedContextBackground)
        {
            clickedContextBackground = false;
            clickedMenu = true;
        }
        EditorInput num = inputMode;
        if (Mouse.left == InputState.Pressed || Mouse.right == InputState.Pressed || Mouse.middle == InputState.Pressed || (!fakeTouch && (lastMousePos - Mouse.position).Length() > 3f))
            inputMode = ((!fakeTouch) ? EditorInput.Mouse : EditorInput.Touch);
        else if (inputMode != EditorInput.Gamepad && InputProfile.active.Pressed("ANY", any: true))
        {
            if ((_selection.Count == 0 || !Keyboard.Pressed(Keys.F, any: true)) && !InputProfile.active.Pressed("RSTICK") && !InputProfile.active.Pressed("CANCEL") && !InputProfile.active.Pressed("MENU1") && !Keyboard.Down(Keys.LeftShift) && !Keyboard.Down(Keys.RightShift) && !Keyboard.Down(Keys.LeftControl) && !Keyboard.Down(Keys.RightControl))
            {
                if (inputMode == EditorInput.Mouse)
                {
                    _tilePosition = Maths.Snap(Mouse.positionScreen + new Vector2(8f, 8f), 16f, 16f);
                    _tilePositionPrev = _tilePosition;
                }
                inputMode = EditorInput.Gamepad;
            }
        }
        else if (TouchScreen.IsScreenTouched())
            inputMode = EditorInput.Touch;
        if (inputMode == EditorInput.Mouse)
            _input.lastActiveDevice = Input.GetDevice<Keyboard>();
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
                    current.RemoveThing(t);
                foreach (Thing t2 in _levelThingsAlternate)
                    current.AddThing(t2);
            }
            else
            {
                foreach (Thing t3 in _levelThingsAlternate)
                    current.RemoveThing(t3);
                foreach (Thing t4 in _levelThingsNormal)
                    current.AddThing(t4);
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
                EndCurrentTouchMode();
            Touch touch = TouchScreen.GetTap();
            if (_touchState == EditorTouchState.Normal)
            {
                _activeTouchButton = null;
                foreach (EditorTouchButton t5 in _touchButtons)
                {
                    if ((touch != Touch.None && touch.positionHUD.X > t5.position.X && touch.positionHUD.X < t5.position.X + t5.size.X && touch.positionHUD.Y > t5.position.Y && touch.positionHUD.Y < t5.position.Y + t5.size.Y) || (t5.threeFingerGesture && _threeFingerGesture))
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
            else if ((touch.positionHUD.X > _cancelButton.position.X && touch.positionHUD.X < _cancelButton.position.X + _cancelButton.size.X && touch.positionHUD.Y > _cancelButton.position.Y && touch.positionHUD.Y < _cancelButton.position.Y + _cancelButton.size.Y) || (_activeTouchButton != null && _activeTouchButton.threeFingerGesture && _threeFingerGesture) || (_activeTouchButton != null && !_activeTouchButton.threeFingerGesture && _threeFingerGesture) || (_activeTouchButton != null && _activeTouchButton.threeFingerGesture && _twoFingerGesture))
            {
                EndCurrentTouchMode();
                if (_fileDialog.opened)
                    _fileDialog.Close();
                SFX.Play("highClick", 0.3f, 0.2f);
                return;
            }
            if (_placingTiles && _placementMenu == null && touch.positionHUD.X > _editTilesButton.position.X && touch.positionHUD.X < _editTilesButton.position.X + _editTilesButton.size.X && touch.positionHUD.Y > _editTilesButton.position.Y && touch.positionHUD.Y < _editTilesButton.position.Y + _editTilesButton.size.Y)
            {
                _openTileSelector = true;
                clickedMenu = true;
            }
            if (_touchState == EditorTouchState.OpenMenu)
            {
                if (_placementMenu == null)
                    _showPlacementMenu = true;
                EndCurrentTouchMode();
            }
            else if (_touchState == EditorTouchState.EditObject)
                _editMode = true;
            else if (_touchState == EditorTouchState.Eyedropper)
                _copyMode = true;
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
            _tileDragDif = Vector2.MaxValue;
        else if (clickedMenu)
            clickedMenu = false;
        else
        {
            if (_notify.opened)
                return;
            if (reopenContextMenu)
            {
                bool num2 = ignorePinning;
                reopenContextMenu = false;
                _placementMenu?.opened = false;
                ignorePinning = num2;
                _placementMenu ??= _objectMenu;
                OpenMenu(_placementMenu);
                if (openContextThing != null)
                    _placementMenu.OpenInto(openContextThing);
                openContextThing = null;
                SFX.Play("openClick", 0.4f);
            }
            hoverTextBox = false;
            if (numPops > 0)
            {
                for (int i = 0; i < numPops; i++)
                {
                    if (focusStack.Count == 0)
                        break;
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
            _placementMenu?.visible = _lockInput == null;
            if (lockInput != null)
            {
                if (_lockInputChange != lockInput)
                    _lockInput = _lockInputChange;
                return;
            }
            if (_lockInputChange != lockInput)
                _lockInput = _lockInputChange;
            if (Keyboard.Pressed(Keys.OemComma))
                searching = true;
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
                            _searchHoverIndex = Math.Min(searchItems.Count - 1, 9);
                        else
                            _searchHoverIndex--;
                        if (_searchHoverIndex < 0)
                            _searchHoverIndex = 0;
                    }
                    else if (Keyboard.Pressed(Keys.Up))
                    {
                        if (_searchHoverIndex < 0)
                            _searchHoverIndex = 0;
                        else
                            _searchHoverIndex++;
                        if (_searchHoverIndex > Math.Min(searchItems.Count - 1, 9))
                            _searchHoverIndex = 0;
                    }
                    _searchHoverIndex = Math.Min(searchItems.Count - 1, _searchHoverIndex);
                }
                else
                    _searchHoverIndex = -1;
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
                    return;
                CloseMenu();
            }
            if (Keyboard.control && Keyboard.Pressed(Keys.S))
            {
                if (Keyboard.shift)
                    SaveAs();
                else
                    Save();
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
                current = new TitleScreen();
            }
            if (Graphics.fade < 0.95f)
                return;
            Layer placementLayer = GetLayerOrOverride(_placementType);
            if (inputMode == EditorInput.Mouse)
            {
                clicked = Mouse.left == InputState.Pressed;
                if (Mouse.middle == InputState.Pressed)
                    middleClickPos = Mouse.position;
            }
            else if (inputMode == EditorInput.Touch)
                clicked = TouchScreen.GetTap() != Touch.None;
            if (_cursorMode == CursorMode.Normal && (Keyboard.Down(Keys.RightShift) || Keyboard.Down(Keys.LeftShift)))
            {
                Vector2 offset = new Vector2(0f, 0f);
                if (Keyboard.Pressed(Keys.Up))
                    offset.Y -= 16f;
                if (Keyboard.Pressed(Keys.Down))
                    offset.Y += 16f;
                if (Keyboard.Pressed(Keys.Left))
                    offset.X -= 16f;
                if (Keyboard.Pressed(Keys.Right))
                    offset.X += 16f;
                if (offset != Vector2.Zero)
                {
                    foreach (Thing t6 in Level.current.things)
                    {
                        t6.Position += offset;
                        if (t6 is IDontMove)
                        {
                            current.things.quadTree.Remove(t6);
                            current.things.quadTree.Add(t6);
                        }
                    }
                }
            }
            _menuOpen = false;
            if (!_editMode)
            {
                foreach (ContextMenu m in things[typeof(ContextMenu)])
                {
                    if (m.visible && m.opened)
                    {
                        clicked = false;
                        _menuOpen = true;
                    }
                }
            }
            if (inputMode == EditorInput.Gamepad)
                _input = InputProfile.active;
            if (_prevEditTilePos != _editTilePos)
            {
                if (_editTilePos.X < 0f)
                    _editTilePos.X = 0f;
                if (_editTilePos.X >= (float)_procTilesWide)
                    _editTilePos.X = _procTilesWide - 1;
                if (_editTilePos.Y < 0f)
                    _editTilePos.Y = 0f;
                if (_editTilePos.Y >= (float)_procTilesHigh)
                    _editTilePos.Y = _procTilesHigh - 1;
                if (_currentMapNode != null)
                {
                    RandomLevelData dat = _currentMapNode.map[(int)_editTilePos.X, (int)_editTilePos.Y].data;
                    if (_levelThings.Count > 0)
                        Save();
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
                    _procXPos = (int)_editTilePos.X;
                    _procYPos = (int)_editTilePos.Y;
                    _genTilePos = new Vector2(_procXPos, _procYPos);
                    _prevEditTilePos = _editTilePos;
                    int reqH = 144;
                    int reqW = 192;
                    _procDrawOffset += new Vector2((_procXPos - _prevProcX) * reqW, (_procYPos - _prevProcY) * reqH);
                    _prevProcX = _procXPos;
                    _prevProcY = _procYPos;
                }
            }
            if (_procXPos != _prevProcX)
                _doGen = true;
            else if (_procYPos != _prevProcY)
                _doGen = true;
            _prevEditTilePos = _editTilePos;
            _prevProcX = _procXPos;
            _prevProcY = _procYPos;
            if (_miniMode && (Keyboard.Pressed(Keys.F1) || _doGen) && !_doingResave)
            {
                if (_saveName == "")
                    _saveName = _initialDirectory + "/pyramid/" + Guid.NewGuid().ToString() + ".lev";
                LevelGenerator.ReInitialize();
                LevelGenerator.complexity = generatorComplexity;
                if (!Keyboard.Down(Keys.RightShift) && !Keyboard.Down(Keys.LeftShift))
                    _procSeed = Rando.Int(2147483646);
                string newName = _saveName[(_saveName.LastIndexOf("assets/levels/") + "assets/levels/".Length)..];
                newName = newName[..^4];
                RandomLevelData centerTile = LevelGenerator.LoadInTile(SaveTempVersion(), newName);
                _loadPosX = _procXPos;
                _loadPosY = _procYPos;
                LevGenType genType = LevGenType.Any;
                if (_currentLevelData.proceduralData.enableSingle && !_currentLevelData.proceduralData.enableMulti)
                    genType = LevGenType.SinglePlayer;
                else if (!_currentLevelData.proceduralData.enableSingle && _currentLevelData.proceduralData.enableMulti)
                    genType = LevGenType.Deathmatch;
                _editTilePos = (_prevEditTilePos = _genTilePos);
                int tries = 0;
                Level lev;
                while (true)
                {
                    _currentMapNode = LevelGenerator.MakeLevel(centerTile, (_pathEast && _pathWest), _procSeed, genType, _procTilesWide, _procTilesHigh, _loadPosX, _loadPosY);
                    _procDrawOffset = new Vector2(0);
                    _procContext = new GameContext();
                    _procContext.ApplyStates();
                    lev = new Level
                    {
                        backgroundColor = new Color(0, 0, 0, 0)
                    };
                    core.currentLevel = lev;
                    RandomLevelNode.editorLoad = true;
                    bool num3 = _currentMapNode.LoadParts(0, 0, lev, _procSeed);
                    RandomLevelNode.editorLoad = false;
                    if (num3 || tries > 100)
                        break;
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
                    _twoFingerSpacing = (TouchScreen.GetTouches()[0].positionHUD - TouchScreen.GetTouches()[1].positionHUD).Length();
                }
                else if (TouchScreen.GetTouches().Count != 2)
                {
                    _twoFingerGesture = false;
                    _twoFingerGestureStarting = false;
                }
                if (_twoFingerGestureStarting && TouchScreen.GetTouches().Count == 2 && !_twoFingerGesture)
                {
                    if ((_panAnchor - TouchScreen.GetAverageOfTouches().positionHUD).Length() > 6f)
                    {
                        _twoFingerZooming = false;
                        _twoFingerGesture = true;
                    }
                    else if (Math.Abs(_twoFingerSpacing - (TouchScreen.GetTouches()[0].positionHUD - TouchScreen.GetTouches()[1].positionHUD).Length()) > 4f)
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
                _panAnchor = Mouse.position;
            _procContext?.Update();
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
                        _showPlacementMenu = true;
                    else
                        CloseMenu();
                    clicked = false;
                    return;
                }
            }
            if (_editorLoadFinished)
            {
                foreach (Thing levelThing in _levelThings)
                    levelThing.OnEditorLoaded();
                foreach (PathNode item in things[typeof(PathNode)])
                {
                    item.UninitializeLinks();
                    item.Update();
                }
                _editorLoadFinished = false;
            }
            things.RefreshState();
            if (_placeObjects.Count > 0)
            {
                foreach (Thing t7 in _placeObjects)
                    foreach (Thing item2 in CheckRectAll<IDontMove>(t7.topLeft + new Vector2(-16), t7.bottomRight + new Vector2(16)))
                        item2.EditorObjectsChanged();
                things.CleanAddList();
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
                    mod = true;
                if (Keyboard.Pressed(Keys.Z))
                {
                    if (mod)
                        History.Redo();
                    else
                        History.Undo();
                    _selection.Clear();
                    _currentDragSelectionHover.Clear();
                    foreach (Thing levelThing2 in _levelThings)
                        levelThing2.EditorObjectsChanged();
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
                        levelThing3.EditorObjectsChanged();
                }
                if (_input.Pressed("RAGDOLL"))
                {
                    History.Redo();
                    _selection.Clear();
                    _currentDragSelectionHover.Clear();
                    foreach (Thing levelThing4 in _levelThings)
                        levelThing4.EditorObjectsChanged();
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
                    CloseMenu();
            }
            if (_clickedTouchButton)
            {
                _clickedTouchButton = false;
                return;
            }
            if (_placementType is AutoBlock || _placementType is PipeTileset)
                cellSize = 16f;
            if (_cursorMode != CursorMode.Selection && _placementMenu == null)
            {
                if (inputMode == EditorInput.Gamepad)
                {
                    if (_input.Pressed("CANCEL"))
                        _selectionDragStart = _tilePosition;
                    if (_selectionDragStart != Vector2.Zero && (_selectionDragStart - _tilePosition).Length() > 4f)
                    {
                        _dragSelectShiftModifier = _selection.Count != 0;
                        _cursorMode = CursorMode.Selection;
                        _selectionDragEnd = _tilePosition;
                        return;
                    }
                    if (_input.Released("CANCEL"))
                        _selectionDragStart = Vector2.Zero;
                }
                else if (inputMode == EditorInput.Mouse)
                {
                    bool specialDrag = Mouse.left == InputState.Pressed && _dragSelectShiftModifier;
                    if (_placementMenu == null && (Mouse.right == InputState.Pressed || specialDrag))
                        _selectionDragStart = Mouse.positionScreen;
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
                        _selectionDragStart = Vector2.Zero;
                        if (_selection.Count > 0)
                        {
                            _cursorMode = CursorMode.HasSelection;
                            return;
                        }
                    }
                    if (_selectionDragStart != Vector2.Zero && (_selectionDragStart - Mouse.positionScreen).Length() > 8f)
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
                        _selectionDragStart = Vector2.Zero;
                }
            }
            if ((_placementMenu == null || _editMode) && _hoverMode == 0)
            {
                UpdateHover(placementLayer, _tilePosition);
                bool middleClick = false;
                if (inputMode == EditorInput.Mouse && Mouse.middle == InputState.Released && (middleClickPos - Mouse.position).Length() < 2f)
                    middleClick = true;
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
                        contextBrowseObject = _secondaryHover;
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
                        contextBrowseObject = _hover;
                }
                else if (_placementType != null && Input.Pressed("START"))
                    contextBrowseObject = _placementType;
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
                        tile = null;
                    else
                    {
                        tile.hover = true;
                        if (inputMode == EditorInput.Mouse)
                            hoverMiniButton = true;
                        if ((inputMode == EditorInput.Gamepad && _input.Down("SELECT")) || (inputMode == EditorInput.Mouse && (Mouse.left == InputState.Down || Mouse.left == InputState.Pressed)) || (inputMode == EditorInput.Touch && TouchScreen.IsScreenTouched()))
                            tile.focus = _input;
                        else
                            tile.focus = null;
                    }
                }
                if (tile != _hoverButton && _hoverButton != null)
                    _hoverButton.focus = null;
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
                                AddThing(_placementMenu);
                        }
                        else if (_secondaryHover != null)
                        {
                            _placementMenu = _secondaryHover.GetContextMenu();
                            if (_placementMenu != null)
                                AddThing(_placementMenu);
                        }
                        if (_placementMenu != null)
                        {
                            OpenMenu(_placementMenu);
                            SFX.Play("openClick", 0.4f);
                        }
                    }
                    else if (inputMode == EditorInput.Mouse && Mouse.right == InputState.Pressed)
                        CloseMenu();
                }
                if (hoverMiniButton)
                {
                    _tilePosition.X = (float)Math.Round(Mouse.positionScreen.X / _cellSize) * _cellSize;
                    _tilePosition.Y = (float)Math.Round(Mouse.positionScreen.Y / _cellSize) * _cellSize;
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
                            _placementMenu.X = 16f;
                            _placementMenu.Y = 16f;
                            _placementMenu.selectedIndex = frame;
                            Add(_placementMenu);
                        }
                    }
                    else if (_placementMenu == null)
                    {
                        _placementMenu = _objectMenu;
                        OpenMenu(_placementMenu);
                        SFX.Play("openClick", 0.4f);
                    }
                    else
                        CloseMenu();
                }
            }
            if (_cursorMode == CursorMode.Normal)
            {
                if (inputMode == EditorInput.Gamepad && _input.Pressed("CANCEL") && _placementMenu != null)
                    CloseMenu();
                if (_placementType != null && _objectMenu != null)
                {
                    if (_placementType._canFlip || _placementType.editorCycleType != null)
                        rotateValid = true;
                    else
                        rotateValid = false;
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
                scroll = Mouse.scroll;
            else if (inputMode == EditorInput.Touch && _twoFingerGesture && _twoFingerZooming)
            {
                float newSpacing = (TouchScreen.GetTouches()[0].positionHUD - TouchScreen.GetTouches()[1].positionHUD).Length();
                if (Math.Abs(newSpacing - _twoFingerSpacing) > 2f)
                    scroll = (0f - (newSpacing - _twoFingerSpacing)) * 1f;
                _twoFingerSpacing = newSpacing;
            }
            if (inputMode == EditorInput.Gamepad)
            {
                scroll = _input.leftTrigger - _input.rightTrigger;
                float speed = base.camera.width / (float)MonoMain.screenWidth * 5f;
                if (_input.Down("LSTICK"))
                    speed *= 2f;
                if (_input.Pressed("LOPTION"))
                {
                    if (cellSize < 10f)
                        cellSize = 16f;
                    else
                        cellSize = 8f;
                }
                if (speed < 5f)
                    speed = 5f;
                camera.x += _input.rightStick.X * speed;
                camera.y -= _input.rightStick.Y * speed;
            }
            if (scroll != 0f && !didUIScroll && !hoverUI)
            {
                int num4 = Math.Sign(scroll);
                _ = camera.height / camera.width;
                float inc = num4 * 64f;
                if (inputMode == EditorInput.Gamepad)
                    inc = scroll * 32f;
                else if (inputMode == EditorInput.Touch)
                    inc = scroll;
                Vector2 prevSize = new(camera.width, camera.height);
                Vector2 mouse = camera.transformScreenVector(Mouse.mousePos);
                if (inputMode == EditorInput.Touch && _twoFingerGesture)
                    mouse = TouchScreen.GetAverageOfTouches().positionCamera;
                if (inputMode == EditorInput.Gamepad)
                    mouse = _tilePosition;
                camera.width += inc;
                if (camera.width < 64f)
                    camera.width = 64f;
                camera.height = camera.width / Resolution.current.aspect;
                Vector2 camPos = camera.position;
                (Matrix.CreateTranslation(new Vec3(camPos.X, camPos.Y, 0)) * Matrix.CreateTranslation(new Vec3(0 - mouse.X, 0 - mouse.Y, 0)) * Matrix.CreateScale(camera.width / prevSize.X, camera.height / prevSize.Y, 1) * Matrix.CreateTranslation(new Vec3(mouse.X, mouse.Y, 0))).Decompose(out var _, out var _, out var translation);
                camera.position = new Vector2(translation.x, translation.y);
            }
            didUIScroll = false;
            if (inputMode == EditorInput.Mouse)
            {
                if (Mouse.middle == InputState.Pressed)
                    _panAnchor = Mouse.position;
                if (Mouse.middle == InputState.Down)
                {
                    Vector2 dif = Mouse.position - _panAnchor;
                    _panAnchor = Mouse.position;
                    float mult = camera.width / Layer.HUD.width;
                    if ((double)dif.Length() > 0.01)
                        _didPan = true;
                    camera.x -= dif.X * mult;
                    camera.y -= dif.Y * mult;
                }
                if (Mouse.middle == InputState.Released)
                {
                    _ = _didPan;
                    _didPan = false;
                }
            }
            else if (inputMode == EditorInput.Touch && _twoFingerGesture && !_twoFingerZooming)
            {
                Vector2 dif2 = TouchScreen.GetAverageOfTouches().positionHUD - _panAnchor;
                _panAnchor = TouchScreen.GetAverageOfTouches().positionHUD;
                float mult2 = base.camera.width / Layer.HUD.width;
                if ((double)dif2.Length() > 0.1)
                {
                    _didPan = true;
                    camera.x -= dif2.X * mult2;
                    camera.y -= dif2.Y * mult2;
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
                    mul = 4;
                _tilePosition = _tilePositionPrev;
                if (_tilePosition.X < base.camera.left)
                    _tilePosition.X = base.camera.left + 32f;
                if (_tilePosition.X > base.camera.right)
                    _tilePosition.X = base.camera.right - 32f;
                if (_tilePosition.Y < base.camera.top)
                    _tilePosition.Y = base.camera.top + 32f;
                if (_tilePosition.Y > base.camera.bottom)
                    _tilePosition.Y = base.camera.bottom - 32f;
                int vertical = 0;
                int horizontal = 0;
                if (_hoverMode == 0 && (_hoverButton == null || _hoverButton.focus == null))
                {
                    if (_input.Pressed("MENULEFT"))
                        horizontal = -1;
                    if (_input.Pressed("MENURIGHT"))
                        horizontal = 1;
                    if (_input.Pressed("MENUUP"))
                        vertical = -1;
                    if (_input.Pressed("MENUDOWN"))
                        vertical = 1;
                }
                float horizontalMove = _cellSize * mul * horizontal;
                float verticalMove = _cellSize * mul * vertical;
                _tilePosition.X += horizontalMove;
                _tilePosition.Y += verticalMove;
                if (_tilePosition.X < camera.left || _tilePosition.X > camera.right)
                    camera.x += horizontalMove;
                if (_tilePosition.Y < camera.top || _tilePosition.Y > camera.bottom)
                    camera.y += verticalMove;
                if (TouchScreen.GetTouch() != Touch.None)
                {
                    _tilePosition.X = (float)Math.Round(TouchScreen.GetTouch().positionCamera.X / _cellSize) * _cellSize;
                    _tilePosition.Y = (float)Math.Round(TouchScreen.GetTouch().positionCamera.Y / _cellSize) * _cellSize;
                    _tilePositionPrev = _tilePosition;
                }
                else
                {
                    _tilePosition.X = (float)Math.Round(_tilePosition.X / _cellSize) * _cellSize;
                    _tilePosition.Y = (float)Math.Round(_tilePosition.Y / _cellSize) * _cellSize;
                    _tilePositionPrev = _tilePosition;
                }
            }
            else if (inputMode == EditorInput.Mouse)
            {
                if (alt)
                {
                    _tilePosition.X = (float)Math.Round(Mouse.positionScreen.X / 1f) * 1f;
                    _tilePosition.Y = (float)Math.Round(Mouse.positionScreen.Y / 1f) * 1f;
                }
                else
                {
                    _tilePosition.X = (float)Math.Round(Mouse.positionScreen.X / _cellSize) * _cellSize;
                    _tilePosition.Y = (float)Math.Round(Mouse.positionScreen.Y / _cellSize) * _cellSize;
                }
            }
            if (_placementType != null && _placementMenu == null)
            {
                _tilePosition += _placementType.editorOffset;
                if (!alt)
                    HugObjectPlacement();
            }
            if (_move != null)
            {
                _move.Position = _tilePosition;
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
                            _move = col;
                        else if (!Keyboard.control)
                            _deleteMode = true;
                    }
                }
                if (_dragMode)
                {
                    if (_tileDragDif == Vector2.MaxValue || inputMode == EditorInput.Gamepad)
                        _tileDragDif = _tilePosition;
                    Vector2 snappedTilePos = Maths.Snap(_tilePosition, _cellSize, _cellSize);
                    Vector2 lerp = _tilePosition;
                    Vector2 prevPlace = Vector2.MaxValue;
                    do
                    {
                        Vector2 snap = Maths.Snap(lerp, _cellSize, _cellSize);
                        if ((Keyboard.control || (Input.Down("SELECT") && Input.Down("MENU1"))) && _tileDragContext == Vector2.MinValue)
                            _tileDragContext = snap;
                        if (snap == Maths.Snap(_tileDragDif, _cellSize, _cellSize) && snap != Maths.Snap(_tilePosition, _cellSize, _cellSize))
                            break;
                        if (snappedTilePos != _tilePosition)
                        {
                            snap = _tilePosition;
                            _tileDragDif = _tilePosition;
                        }
                        lerp = Lerp.Vector2(lerp, _tileDragDif, _cellSize);
                        if (_tileDragDif != _tilePosition)
                            UpdateHover(placementLayer, snap, isDrag: true);
                        if (!_deleteMode && _placementType != null)
                        {
                            Thing col2 = _hover;
                            if (col2 == null && _placementType is not BackgroundTile)
                                col2 = CollisionPointFilter<Thing>(snap, x => x.placementLayer == placementLayer && (_placementType is not PipeTileset || x.GetType() == _placementType.GetType()));
                            if (col2 is TileButton)
                                col2 = null;
                            else if (col2 != null && !_levelThings.Contains(col2))
                                col2 = null;
                            else if (ctrl && alt)
                                col2 = null;
                            else if (_placementType is BackgroundTile && col2 is not BackgroundTile)
                                col2 = null;
                            else if (firstClick && _hover == null)
                                col2 = null;
                            firstClick = false;
                            if ((col2 == null || (_placementType is WireTileset && col2 is IWirePeripheral) || (_placementType is IWirePeripheral && col2 is WireTileset)) && !placementLimitReached && !placementOutOfSizeRange && prevPlace != snap)
                            {
                                prevPlace = snap;
                                Type t8 = _placementType.GetType();
                                Thing newThing2 = null;
                                if (_eyeDropperSerialized == null)
                                    newThing2 = CreateThing(t8);
                                else
                                    newThing2 = Thing.LoadThing(_eyeDropperSerialized);
                                newThing2.X = snap.X;
                                newThing2.Y = snap.Y;
                                if (_placementType is SubBackgroundTile)
                                    (newThing2.graphic as SpriteMap).frame = ((_placementType as SubBackgroundTile).graphic as SpriteMap).frame;
                                if (_placementType is BackgroundTile)
                                {
                                    int frameOffsetX = (int)((snap.X - _tileDragContext.X) / 16f);
                                    int frameOffsetY = (int)((snap.Y - _tileDragContext.Y) / 16f);
                                    (newThing2 as BackgroundTile).frame = (_placementType as BackgroundTile).frame + frameOffsetX + (int)((float)frameOffsetY * ((float)newThing2.graphic.texture.width / 16f));
                                }
                                else if (_placementType is ForegroundTile)
                                    (newThing2.graphic as SpriteMap).frame = ((_placementType as ForegroundTile).graphic as SpriteMap).frame;
                                if (_hover is BackgroundTile)
                                    newThing2.Depth = _hover.Depth + 1;
                                History.Add(() =>
                                {
                                    AddObject(newThing2);
                                }, () =>
                                {
                                    RemoveObject(newThing2);
                                });
                                if (newThing2 is PathNode)
                                    _editorLoadFinished = true;
                                if (preciseMode)
                                    disableDragMode();
                            }
                        }
                        else
                        {
                            Thing col3 = _hover;
                            if (col3 != null)
                            {
                                History.Add(() =>
                                {
                                    RemoveObject(col3);
                                }, () =>
                                {
                                    AddObject(col3);
                                });
                                if (col3 is PathNode)
                                    _editorLoadFinished = true;
                                _hover = null;
                            }
                        }
                        things.RefreshState();
                    }
                    while ((lerp - _tileDragDif).Length() > 2f);
                }
                if ((Mouse.left == InputState.Released && dragModeInputType == InputType.eMouse) || (_input.Released("SELECT") && dragModeInputType == InputType.eGamepad) || (TouchScreen.GetRelease() != Touch.None && dragModeInputType == InputType.eTouch))
                    disableDragMode();
            }
            if (!Keyboard.control && !Input.Down("MENU1"))
                _tileDragContext = Vector2.MinValue;
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
                    positionCursor = true,
                    opened = true
                };
                SFX.Play("openClick", 0.4f);
                _placementMenu.X = 16f;
                _placementMenu.Y = 16f;
                _placementMenu.selectedIndex = frame2;
                Add(_placementMenu);
                _openTileSelector = false;
            }
            if (_editMode && _cursorMode == CursorMode.Normal)
            {
                if (_twoFingerGesture || _threeFingerGesture)
                    DoMenuClose();
                if (clicked && _hover != null)
                {
                    DoMenuClose();
                    _placementMenu = _hover.GetContextMenu();
                    if (_placementMenu != null)
                    {
                        _placementMenu.X = 96f;
                        _placementMenu.Y = 32f;
                        if (inputMode == EditorInput.Gamepad || inputMode == EditorInput.Touch)
                        {
                            _placementMenu.X = 16f;
                            _placementMenu.Y = 16f;
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
                DoMenuClose();
            base.Update();
        }
    }

    public override void PostUpdate()
    {
        if (_placementMenu != null)
        {
            if (_editMode && clickedMenu)
                _hover = _oldHover;
            if (inputMode == EditorInput.Touch && TouchScreen.GetTap() != Touch.None && !clickedMenu && !clickedContextBackground && !_openedEditMenu)
            {
                if (_touchState == EditorTouchState.OpenMenu)
                    EndCurrentTouchMode();
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

    public override void PostDrawLayer(Layer layer)
    {
        base.PostDrawLayer(layer);
        if (layer == Layer.Foreground)
            foreach (Thing thing2 in things)
                thing2.DoEditorRender();
        if (layer == _procLayer && _procTarget != null && _procContext != null)
            Graphics.Draw(_procTarget, new Vector2(0), null, Color.White * 0.5f, 0, Vector2.Zero, new Vector2(1), SpriteEffects.None);
        if (layer == _gridLayer)
        {
            backgroundColor = new Color(20, 20, 20);
            Color gridColor = new(38, 38, 38);
            if (arcadeMachineMode)
                Graphics.DrawRect(_levelThings[0].Position + new Vector2(-17, -21), _levelThings[0].Position + new Vector2(18, 21), gridColor, -0.9f, filled: false);
            else
            {
                float x = (0f - _cellSize) / 2f;
                float y = (0f - _cellSize) / 2f;
                if (_sizeRestriction.X > 0f)
                {
                    Vector2 center = -new Vector2(_gridW * _cellSize / 2, (_gridH - 1) * _cellSize / 2) + new Vector2(8, 0);
                    x += (int)(center.X / _cellSize) * _cellSize;
                    y += (int)(center.Y / _cellSize) * _cellSize;
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
                    x = (int)(_ultimateBounds.x / _cellSize * _cellSize) + _cellSize / 2;
                    wid -= dif;
                }
                if (y < _ultimateBounds.y)
                {
                    int dif2 = (int)((_ultimateBounds.y - y) / _cellSize) + 1;
                    y = (int)(_ultimateBounds.y / _cellSize * _cellSize) + _cellSize / 2;
                    hig -= dif2;
                }
                float limit = x + wid * _cellSize;
                if (limit > _ultimateBounds.Right)
                {
                    int dif3 = (int)((limit - _ultimateBounds.Right) / _cellSize) + 1;
                    wid -= dif3;
                    x = (int)((_ultimateBounds.Right - wid * _cellSize) / _cellSize * _cellSize) - _cellSize / 2;
                }
                limit = y + hig * _cellSize;
                if (y + hig * _cellSize > _ultimateBounds.Bottom)
                {
                    int dif4 = (int)((limit - _ultimateBounds.Bottom) / _cellSize) + 1;
                    hig -= dif4;
                    y = (int)((_ultimateBounds.Bottom - hig * _cellSize) / _cellSize * _cellSize) - _cellSize / 2;
                }
                int reqW = wid * (int)_cellSize;
                int reqH = hig * (int)_cellSize;
                int numHor = (int)(reqW / _cellSize);
                int numVert = (int)(reqH / _cellSize);
                for (int xpos = 0; xpos < numHor + 1; xpos++)
                    Graphics.DrawLine(new Vector2(x + xpos * _cellSize, y), new Vector2(x + xpos * _cellSize, y + numVert * _cellSize), gridColor, 2, -0.9f);
                for (int ypos = 0; ypos < numVert + 1; ypos++)
                    Graphics.DrawLine(new Vector2(x, y + ypos * _cellSize), new Vector2(x + numHor * _cellSize, y + ypos * _cellSize), gridColor, 2, -0.9f);
                Graphics.DrawLine(new Vector2(_ultimateBounds.Left, _ultimateBounds.Top), new Vector2(_ultimateBounds.Right, _ultimateBounds.Top), gridColor, 2, -0.9f);
                Graphics.DrawLine(new Vector2(_ultimateBounds.Right, _ultimateBounds.Top), new Vector2(_ultimateBounds.Right, _ultimateBounds.Bottom), gridColor, 2, -0.9f);
                Graphics.DrawLine(new Vector2(_ultimateBounds.Right, _ultimateBounds.Bottom), new Vector2(_ultimateBounds.Left, _ultimateBounds.Bottom), gridColor, 2, -0.9f);
                Graphics.DrawLine(new Vector2(_ultimateBounds.Left, _ultimateBounds.Bottom), new Vector2(_ultimateBounds.Left, _ultimateBounds.Top), gridColor, 2, -0.9f);
                if (_miniMode)
                {
                    int sides = 0;
                    if (!_pathNorth)
                        _sideArrow.color = new Color(80, 80, 80);
                    else
                    {
                        _sideArrow.color = new Color(100, 200, 100);
                        Graphics.DrawLine(new Vector2(x + (reqW / 2), y - 10), new Vector2(x + (reqW / 2), y + (reqH / 2) - 8), Color.Lime * 0.06f, 16);
                        sides++;
                    }
                    if (!_pathWest)
                        _sideArrow.color = new Color(80, 80, 80);
                    else
                    {
                        _sideArrow.color = new Color(100, 200, 100);
                        Graphics.DrawLine(new Vector2(x - 10, y + (reqH / 2)), new Vector2(x + (reqW / 2) - 8, y + (reqH / 2)), Color.Lime * 0.06f, 16);
                        sides++;
                    }
                    if (!_pathEast)
                        _sideArrow.color = new Color(80, 80, 80);
                    else
                    {
                        _sideArrow.color = new Color(100, 200, 100);
                        Graphics.DrawLine(new Vector2(x + (reqW / 2) + 8, y + (reqH / 2)), new Vector2(x + reqW + 10, y + (reqH / 2)), Color.Lime * 0.06f, 16);
                        sides++;
                    }
                    if (!_pathSouth)
                        _sideArrow.color = new Color(80, 80, 80);
                    else
                    {
                        _sideArrow.color = new Color(100, 200, 100);
                        Graphics.DrawLine(new Vector2(x + (reqW / 2), y + (reqH / 2) + 8), new Vector2(x + (reqW / 2), y + reqH + 10), Color.Lime * 0.06f, 16);
                        sides++;
                    }
                    if (sides > 0)
                        Graphics.DrawLine(new Vector2(x + (reqW / 2) - 8, y + (reqH / 2)), new Vector2(x + (reqW / 2) + 8, y + (reqH / 2)), Color.Lime * 0.06f, 16);
                }
            }
        }
        if (layer == Layer.Foreground)
        {
            float x2 = (0 - _cellSize) / 2,
                  y2 = (0 - _cellSize) / 2;
            int wid2 = _gridW,
                hig2 = _gridH;
            if (_miniMode)
                (wid2, hig2) = (12, 9);
            int reqW2 = wid2 * 16,
                reqH2 = hig2 * 16;
            if (_miniMode)
            {
                _procTilesWide = (int)_genSize.X;
                _procTilesHigh = (int)_genSize.Y;
                _procXPos = (int)_genTilePos.X;
                _procYPos = (int)_genTilePos.Y;
                if (_procXPos > _procTilesWide)
                    _procXPos = _procTilesWide;
                if (_procYPos > _procTilesHigh)
                    _procYPos = _procTilesHigh;
                for (int i = 0; i < _procTilesWide; i++)
                {
                    for (int j = 0; j < _procTilesHigh; j++)
                    {
                        int xDraw = i - _procXPos;
                        int yDraw = j - _procYPos;
                        if (i != _procXPos || j != _procYPos)
                            Graphics.DrawRect(new Vector2(x2 + (reqW2 * xDraw), y2 + (reqH2 * yDraw)), new Vector2(x2 + (reqW2 * (xDraw + 1)), y2 + (reqH2 * (yDraw + 1))), Color.White * 0.2f, 1, filled: false);
                    }
                }
            }
            if (_hoverButton == null)
            {
                if (_cursorMode != CursorMode.Pasting)
                {
                    if (_secondaryHover != null && _placementMode)
                    {
                        Vector2 p = _secondaryHover.topLeft;
                        Vector2 p2 = _secondaryHover.bottomRight;
                        Graphics.DrawRect(p, p2, Color.White * 0.5f, 1, filled: false);
                    }
                    else if (_hover != null && _placementMode && (inputMode != EditorInput.Touch || _editMode))
                    {
                        Vector2 p3 = _hover.topLeft;
                        Vector2 p4 = _hover.bottomRight;
                        Graphics.DrawRect(p3, p4, Color.White * 0.5f, 1, filled: false);
                        _hover.DrawHoverInfo();
                    }
                }
                if (DevConsole.wagnusDebug)
                {
                    Graphics.DrawLine(_tilePosition, _tilePosition + new Vector2(128, 0), Color.White * 0.5f);
                    Graphics.DrawLine(_tilePosition, _tilePosition + new Vector2(-128, 0), Color.White * 0.5f);
                    Graphics.DrawLine(_tilePosition, _tilePosition + new Vector2(0, 128), Color.White * 0.5f);
                    Graphics.DrawLine(_tilePosition, _tilePosition + new Vector2(0, -128), Color.White * 0.5f);
                }
                if ((_hover == null || _cursorMode == CursorMode.DragHover || _cursorMode == CursorMode.Drag) && inputMode == EditorInput.Gamepad)
                {
                    if (_cursorMode == CursorMode.DragHover || _cursorMode == CursorMode.Drag)
                    {
                        _cursor.Depth = 1;
                        _cursor.Scale = new Vector2(1);
                        _cursor.Position = _tilePosition;
                        if (_cursorMode == CursorMode.DragHover)
                            _cursor.frame = 1;
                        else if (_cursorMode == CursorMode.Drag)
                            _cursor.frame = 5;
                        _cursor.Draw();
                    }
                    else if (_placementMenu == null)
                        Graphics.DrawRect(_tilePosition - new Vector2(_cellSize / 2), _tilePosition + new Vector2(_cellSize / 2), Color.White * 0.5f, 1, filled: false);
                }
                if (_cursorMode == CursorMode.Normal && _hover == null && _placementMode && inputMode != EditorInput.Touch && _placementMenu == null && _placementType != null)
                {
                    _placementType.Depth = 0.9f;
                    _placementType.X = _tilePosition.X;
                    _placementType.Y = _tilePosition.Y;
                    _placementType.Draw();
                    if (placementLimitReached || placementOutOfSizeRange)
                        Graphics.Draw(_cantPlace, _placementType.X, _placementType.Y, 0.95f);
                }
            }
            if (_cursorMode == CursorMode.Selection || _cursorMode == CursorMode.HasSelection || _cursorMode == CursorMode.Drag || _cursorMode == CursorMode.DragHover)
            {
                _leftSelectionDraw = false;
                if (_cursorMode == CursorMode.Selection)
                    Graphics.DrawDottedRect(_selectionDragStart, _selectionDragEnd, Color.White * 0.5f, 1f, 2, 4);
            }
            if (_cursorMode == CursorMode.Pasting)
            {
                Graphics.material = _selectionMaterialPaste;
                foreach (Thing item in _pasteBatch)
                {
                    Vector2 pos = item.Position;
                    item.Position -= pasteOffset;
                    item.Draw();
                    item.Position = pos;
                }
                Graphics.material = null;
            }
        }
        string selectText,
               copyText,
               menuText,
               searchText,
               undoText,
               redoText,
               buttonText;
        int num,
            num2;
        if (layer == Layer.HUD)
        {
            if (inputMode == EditorInput.Touch)
            {
                float touchTooltipYOffset = -24;
                if (_activeTouchButton != null || _fileDialog.opened)
                {
                    if (_activeTouchButton != null)
                        Graphics.DrawString(_activeTouchButton.explanation, Layer.HUD.camera.OffsetBR(-20f, touchTooltipYOffset) - new Vector2(Graphics.GetStringWidth(_activeTouchButton.explanation) + (_cancelButton.size.X + 4f), 0f), Color.Gray, 0.99f);
                    else if (_fileDialog.opened)
                    {
                        string explanation = "Double tap level to open!";
                        Graphics.DrawString(explanation, Layer.HUD.camera.OffsetBR(-20, touchTooltipYOffset) - new Vector2(Graphics.GetStringWidth(explanation) + (_cancelButton.size.X + 4), 0), Color.Gray, 0.99f);
                    }
                    Graphics.DrawRect(_cancelButton.position, _cancelButton.position + _cancelButton.size, new Color(70, 70, 70), 0.99f, filled: false);
                    Graphics.DrawRect(_cancelButton.position, _cancelButton.position + _cancelButton.size, new Color(30, 30, 30), 0.98f);
                    Graphics.DrawString(_cancelButton.caption, _cancelButton.position + _cancelButton.size / 2 + new Vector2((0 - Graphics.GetStringWidth(_cancelButton.caption)) / 2, -4), Color.White, 0.99f);
                }
                else if (!_fileDialog.opened)
                {
                    float totalSize = 0f;
                    foreach (EditorTouchButton button in _touchButtons)
                    {
                        Graphics.DrawRect(button.position, button.position + button.size, new Color(70, 70, 70), 0.99f, filled: false);
                        Graphics.DrawRect(button.position, button.position + button.size, new Color(30, 30, 30), 0.98f);
                        Graphics.DrawString(button.caption, button.position + button.size / 2f + new Vector2((0f - Graphics.GetStringWidth(button.caption)) / 2f, -4f), Color.White, 0.99f);
                        totalSize += button.size.X;
                    }
                    if (_placementMenu != null && _placementMenu is EditorGroupMenu)
                    {
                        string explanation2 = "Double tap to select!";
                        Graphics.DrawString(explanation2, Layer.HUD.camera.OffsetBR(-20, touchTooltipYOffset) - new Vector2(Graphics.GetStringWidth(explanation2) + (totalSize + 8), 0), Color.Gray, 0.99f);
                    }
                }
                if (_placingTiles && _placementMenu == null)
                {
                    Graphics.DrawRect(_editTilesButton.position, _editTilesButton.position + _editTilesButton.size, new Color(70, 70, 70), 0.99f, filled: false);
                    Graphics.DrawRect(_editTilesButton.position, _editTilesButton.position + _editTilesButton.size, new Color(30, 30, 30), 0.98f);
                    Graphics.DrawString(_editTilesButton.caption, _editTilesButton.position + _editTilesButton.size / 2 + new Vector2((0f - Graphics.GetStringWidth(_editTilesButton.caption)) / 2, -4), Color.White, 0.99f);
                }
            }
            if (hasUnsavedChanges)
                Graphics.DrawFancyString("*", new Vector2(4), Color.White * 0.6f, 0.99f);
            if (tooltip != null)
            {
                Graphics.DrawRect(new Vector2(16, Layer.HUD.height - 14), new Vector2(16 + Graphics.GetFancyStringWidth(tooltip) + 2, Layer.HUD.height - 2), new Color(0, 0, 0) * 0.75f, 0.99f);
                Graphics.DrawFancyString(tooltip, new Vector2(18, Layer.HUD.height - 12), Color.White, 0.99f);
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
                            buttonText += "@SELECT@DRAG  ";
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
                            buttonText += "@LEFTMOUSE@DRAG  ";
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
                    buttonText = "@WASD@MOVE  " + selectText + "SELECT  @MENU2@DELETE  " + quackText + "CANCEL  @STRAFE@+@RAGDOLL@BROWSE..";
                else if (_menuOpen && inputMode == EditorInput.Gamepad)
                    buttonText = "@WASD@MOVE  " + selectText + "SELECT  @RIGHT@EXPAND  " + quackText + "CLOSE";
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
                        num = 1;
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
                    Vector2 topLeft = new(layer.width - 28 - wide, layer.height - 28);
                    _font.Depth = 0.8f;
                    _font.Draw(buttonText2, topLeft.X, topLeft.Y, Color.White, 0.8f);
                    Graphics.DrawRect(topLeft + new Vector2(-2), topLeft + new Vector2(wide + 2, 9), Color.Black * 0.5f, 0.6f);
                }
            }
            goto IL_2555;
        }
        if (layer != _objectMenuLayer)
            return;
        if (inputMode == EditorInput.Mouse)
        {
            _cursor.Depth = 1;
            _cursor.Scale = new Vector2(1);
            _cursor.Position = Mouse.position;
            if (_cursorMode == CursorMode.Normal)
                _cursor.frame = 0;
            else if (_cursorMode == CursorMode.DragHover)
                _cursor.frame = 1;
            else if (_cursorMode == CursorMode.Drag)
                _cursor.frame = 5;
            else if (_cursorMode == CursorMode.Selection)
                _cursor.frame = (_dragSelectShiftModifier ? 6 : 2);
            else if (_cursorMode == CursorMode.HasSelection)
                _cursor.frame = (_dragSelectShiftModifier ? 6 : 0);
            if (hoverTextBox)
            {
                _cursor.frame = 7;
                _cursor.Y -= 4f;
                _cursor.Scale = new Vector2(0.5f, 1);
            }
            _cursor.Draw();
        }
        if (inputMode != EditorInput.Touch)
            return;
        if (TouchScreen.GetTouches().Count == 0)
        {
            Vector2 pos2 = _objectMenuLayer.camera.transformScreenVector(Mouse.positionConsole + new Vector2(TouchScreen._spoofFingerDistance, 0f));
            Vector2 pos3 = _objectMenuLayer.camera.transformScreenVector(Mouse.positionConsole - new Vector2(TouchScreen._spoofFingerDistance, 0f));
            Graphics.DrawCircle(pos2, 4, Color.White * 0.2f, 2, 1);
            Graphics.DrawCircle(pos3, 4, Color.White * 0.2f, 2, 1);
            Graphics.DrawRect(pos2 + new Vector2(-0.5f), pos2 + new Vector2(0.5f), Color.White, 1);
            Graphics.DrawRect(pos3 + new Vector2(-0.5f), pos3 + new Vector2(0.5f), Color.White, 1);
            return;
        }
        foreach (Touch touch in TouchScreen.GetTouches())
            Graphics.DrawCircle(touch.Transform(_objectMenuLayer.camera), 4, Color.White, 2, 1);
        return;
    IL_2555:
        _font.Scale = new Vector2(1);
        return;
    IL_18bd:
        if (inputMode == EditorInput.Touch)
            buttonText = "";
        if (buttonText != "")
        {
            float wide2 = _font.GetWidth(buttonText);
            Vector2 topLeft2 = new(layer.width - 22 - wide2, layer.height - 28);
            _font.Depth = 0.8f;
            _font.Draw(buttonText, topLeft2.X, topLeft2.Y, Color.White, 0.7f, _input);
        }
        _font.Scale = new Vector2(0.5f);
        float contextObjectOffsetY = 0;
        if (placementLimit > 0)
        {
            contextObjectOffsetY -= 16;
            Vector2 size = new(128, 12);
            Vector2 topLeft3 = new(31, layer.height - 19 - size.Y);
            Graphics.DrawRect(topLeft3, topLeft3 + size, Color.Black * 0.5f, 0.6f);
            Graphics.Draw(_editorCurrency, topLeft3.X - 10, topLeft3.Y + 2, 0.95f);
            float wide3 = (size.X - 4) * Math.Min(placementTotalCost / placementLimit, 1);
            string placementCostString = placementTotalCost + "/" + placementLimit;
            if (placementLimitReached)
                placementCostString += " FULL!";
            float placementCostStringWidth = _font.GetWidth(placementCostString);
            _font.Draw(placementCostString, topLeft3.X + size.X / 2 - placementCostStringWidth / 2, topLeft3.Y + 4, Color.White, 0.7f);
            topLeft3 += new Vector2(2);
            Graphics.DrawRect(topLeft3, topLeft3 + new Vector2(wide3, size.Y - 4), (placementLimitReached ? Colors.DGRed : Colors.DGGreen) * 0.5f, 0.6f);
        }
        if (searching)
        {
            Graphics.DrawRect(Vector2.Zero, new Vector2(layer.width, layer.height), Color.Black * 0.5f, 0.9f);
            Vector2 searchPos = new Vector2(8, layer.height - 26);
            Graphics.DrawString("@searchiconwhitebig@", searchPos, Color.White, 0.95f);
            if (Keyboard.keyString == "")
                Graphics.DrawString("|GRAY|Type to search...", searchPos + new Vector2(26, 7), Color.White, 0.95f);
            else
                Graphics.DrawString(Keyboard.keyString + "_", searchPos + new Vector2(26, 7), Color.White, 0.95f);
            if (inputMode == EditorInput.Mouse)
                _searchHoverIndex = -1;
            float wide4 = 200;
            if (searchItems != null && searchItems.Count > 0)
            {
                searchPos.Y -= 22;
                for (int k = 0; k < 10 && k < searchItems.Count; k++)
                {
                    Graphics.DrawString(searchItems[k].thing.thing.editorName, new Vector2(searchPos.X + 24, searchPos.Y + 6), Color.White, 0.95f);
                    searchItems[k].thing.image.Depth = 0.95f;
                    searchItems[k].thing.image.X = searchPos.X + 4;
                    searchItems[k].thing.image.Y = searchPos.Y;
                    searchItems[k].thing.image.color = Color.White;
                    searchItems[k].thing.image.Scale = new Vector2(1);
                    searchItems[k].thing.image.Draw();
                    if ((inputMode == EditorInput.Mouse && Mouse.x > searchPos.X && Mouse.x < searchPos.X + 200 && Mouse.y > searchPos.Y - 2 && Mouse.y < searchPos.Y + 19) || k == _searchHoverIndex)
                    {
                        _searchHoverIndex = k;
                        Graphics.DrawRect(searchPos + new Vector2(2, -2), searchPos + new Vector2(wide4 - 2, 18), new Color(70, 70, 70), 0.93f);
                    }
                    searchPos.Y -= 20;
                }
                Graphics.DrawRect(searchPos + new Vector2(0, 16), new Vector2(searchPos.X + wide4, layer.height - 28), new Color(30, 30, 30), 0.91f);
            }
            Graphics.DrawRect(new Vector2(8, layer.height - 26), new Vector2(300, layer.height - 6), new Color(30, 30, 30), 0.91f);
        }
        float placementHeight = 0;
        if (_placementType != null && _cursorMode == CursorMode.Normal && _placementMenu == null)
        {
            Vector2 size2 = new(_placementType.width, _placementType.height);
            size2.X += 4;
            size2.Y += 4;
            if (size2.X < 32)
                size2.X = 32;
            if (size2.Y < 32)
                size2.Y = 32;
            Vector2 topLeft4 = new(19, layer.height - 19 - size2.Y + contextObjectOffsetY);
            string deets = _placementType.GetDetailsString();
            while (deets.Count(c => c == '\n') > 5)
                deets = deets[..deets.LastIndexOf('\n')];
            float wide5 = _font.GetWidth(deets) + 8;
            if (deets != "")
                _font.Draw(deets, topLeft4.X + size2.X + 4, topLeft4.Y + 4, Color.White, 0.7f);
            else
                wide5 = 0;
            Graphics.DrawRect(topLeft4, topLeft4 + size2 + new Vector2(wide5, 0), Color.Black * 0.5f, 0.6f);
            editorDraw = true;
            _placementType.left = topLeft4.X + (size2.X / 2 - _placementType.w / 2);
            _placementType.top = topLeft4.Y + (size2.Y / 2 - _placementType.h / 2);
            _placementType.Depth = 0.7f;
            _placementType.Draw();
            editorDraw = false;
            _font.Draw("Placing (" + _placementType.editorName + ")", topLeft4.X, topLeft4.Y - 6, Color.White, 0.7f);
            placementHeight = size2.Y;
        }
        Thing hoverDraw = _hover;
        if (_secondaryHover != null)
            hoverDraw = _secondaryHover;
        if (hoverDraw != null && _cursorMode == CursorMode.Normal && _hoverMode == 0)
        {
            Vector2 size3 = new Vector2(hoverDraw.width, hoverDraw.height);
            size3.X += 4;
            size3.Y += 4;
            if (size3.X < 32)
                size3.X = 32;
            if (size3.Y < 32)
                size3.Y = 32;
            Vector2 topLeft5 = new Vector2(19, layer.height - 19 - size3.Y - (placementHeight + 10) + contextObjectOffsetY);
            string deets2 = hoverDraw.GetDetailsString();
            while (deets2.Count(c => c == '\n') > 5)
                deets2 = deets2[..deets2.LastIndexOf('\n')];
            float wide6 = _font.GetWidth(deets2) + 8;
            if (deets2 != "")
                _font.Draw(deets2, topLeft5.X + size3.X + 4, topLeft5.Y + 4, Color.White, 0.7f);
            else
                wide6 = 0f;
            Graphics.DrawRect(topLeft5, topLeft5 + size3 + new Vector2(wide6, 0), Color.Black * 0.5f, 0.6f);
            Vector2 pos4 = hoverDraw.Position;
            Depth d = hoverDraw.Depth;
            editorDraw = true;
            hoverDraw.left = topLeft5.X + (size3.X / 2 - hoverDraw.w / 2);
            hoverDraw.top = topLeft5.Y + (size3.Y / 2 - hoverDraw.h / 2);
            hoverDraw.Depth = 0.7f;
            hoverDraw.Draw();
            editorDraw = false;
            hoverDraw.Position = pos4;
            hoverDraw.Depth = d;
            _font.Draw("Hovering (" + hoverDraw.editorName + ")", topLeft5.X, topLeft5.Y - 6, Color.White);
        }
        goto IL_2555;
    IL_1762:
        bool shouldDisplayBrowsePrompt = (byte)num2 != 0;
        if (_placementType != null && _hover != null && GetLayerOrOverride(_placementType) == GetLayerOrOverride(_hover))
            buttonText = buttonText + selectText + "ERASE  ";
        else if (_placementType != null)
        {
            buttonText = buttonText + selectText + "PLACE  ";
            if (rotateValid)
                buttonText += "@RSTICK@ROTATE  ";
        }
        if (num != 0)
            buttonText = buttonText + copyText + "COPY  ";
        if (_hover != null && !_placingTiles && _hoverMenu != null)
            buttonText += "@MENU1@EDIT  ";
        if (inputMode == EditorInput.Gamepad)
        {
            if (History.hasUndo)
                buttonText = buttonText + undoText + "UNDO  ";
            if (History.hasRedo)
                buttonText = buttonText + redoText + "REDO  ";
            buttonText += "@CANCEL@DRAG SELECT  ";
        }
        if (_placingTiles)
            buttonText += "@MENU1@TILES  ";
        if (shouldDisplayBrowsePrompt)
            buttonText = buttonText + searchText + "BROWSE  ";
        buttonText = buttonText + menuText + "MENU";
        if (_font.GetWidth(buttonText) < 397)
            buttonText = "@WASD@MOVE  " + buttonText;
        if (inputMode == EditorInput.Mouse)
            buttonText += "  @RIGHTMOUSE@DRAG SELECT";
        goto IL_18bd;
    }

    public override void StartDrawing()
    {
        _procTarget ??= new RenderTarget2D(Graphics.width, Graphics.height);
        _procContext?.Draw(_procTarget, current.camera, _procDrawOffset);
    }

    public void AddObject(Thing obj)
    {
        hasUnsavedChanges = true;
        if (obj == null)
            return;
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
                    if (!Thing.CheckForBozoData(thing))
                        AddObject(thing);
                return;
            }
            {
                foreach (Thing thing2 in container.things)
                    AddObject(thing2);
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
                    History.Add(() =>
                    {
                        RemoveObject(t);
                    }, () =>
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
            _placeObjects.Add(obj);
        placementTotalCost += CalculatePlacementCost(obj);
        if (_sizeRestriction.X > 0f)
            AdjustSizeLimits(obj);
        if (_loadingLevel)
            return;
        if (!_isPaste)
            obj.EditorAdded();
        if (obj is MirrorMode || processingMirror || obj is BackgroundUpdater)
            return;
        processingMirror = true;
        foreach (MirrorMode m in base.things[typeof(MirrorMode)])
        {
            if (((MirrorMode.Setting)m.mode == MirrorMode.Setting.Both || (MirrorMode.Setting)m.mode == MirrorMode.Setting.Vertical) && Math.Abs(m.Position.X - obj.Position.X) > 2f)
            {
                Vector2 newPos = obj.Position - new Vector2((obj.Position.X - m.Position.X) * 2f, 0f);
                Thing mirror = Thing.LoadThing(obj.Serialize());
                mirror.Position = newPos;
                mirror.flipHorizontal = !obj.flipHorizontal;
                AddObject(mirror);
                mirror.EditorFlip(pVertical: false);
            }
            if (((MirrorMode.Setting)m.mode == MirrorMode.Setting.Both || (MirrorMode.Setting)m.mode == MirrorMode.Setting.Horizontal) && Math.Abs(m.Position.Y - obj.Position.Y) > 2f)
            {
                Vector2 newPos2 = obj.Position - new Vector2(0f, (obj.Position.Y - m.Position.Y) * 2f);
                Thing mirror2 = Thing.LoadThing(obj.Serialize());
                mirror2.Position = newPos2;
                AddObject(mirror2);
                mirror2.EditorFlip(pVertical: true);
            }
            if ((MirrorMode.Setting)m.mode == MirrorMode.Setting.Both && Math.Abs(m.Position.X - obj.Position.X) > 2f && Math.Abs(m.Position.Y - obj.Position.Y) > 2f)
            {
                Vector2 newPos3 = obj.Position - new Vector2((obj.Position.X - m.Position.X) * 2f, (obj.Position.Y - m.Position.Y) * 2f);
                Thing mirror3 = Thing.LoadThing(obj.Serialize());
                mirror3.Position = newPos3;
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
        current.RemoveThing(obj);
        _levelThings.Remove(obj);
        if (obj is IDontMove)
            _placeObjects.Add(obj);
        placementTotalCost -= CalculatePlacementCost(obj);
        if (_sizeRestriction.X > 0f && (obj.X <= _topLeftMost.X || obj.X >= _bottomRightMost.X || obj.Y <= _topLeftMost.Y || obj.Y >= _bottomRightMost.Y))
            RecalculateSizeLimits();
        obj.EditorRemoved();
        if (_loadingLevel || obj is MirrorMode || processingMirror || obj is BackgroundUpdater)
            return;
        processingMirror = true;
        foreach (MirrorMode m in base.things[typeof(MirrorMode)])
        {
            if ((MirrorMode.Setting)m.mode == MirrorMode.Setting.Both || (MirrorMode.Setting)m.mode == MirrorMode.Setting.Vertical)
            {
                Vector2 pairPos = obj.Position + new Vector2((0f - (obj.Position.X - m.Position.X)) * 2f, 0f);
                Thing t = Level.current.CollisionPoint(pairPos, obj.GetType());
                if (t != null)
                    RemoveObject(t);
            }
            if ((MirrorMode.Setting)m.mode == MirrorMode.Setting.Both || (MirrorMode.Setting)m.mode == MirrorMode.Setting.Horizontal)
            {
                Vector2 pairPos2 = obj.Position + new Vector2(0f, (0f - (obj.Position.Y - m.Position.Y)) * 2f);
                Thing t2 = Level.current.CollisionPoint(pairPos2, obj.GetType());
                if (t2 != null)
                    RemoveObject(t2);
            }
            if ((MirrorMode.Setting)m.mode == MirrorMode.Setting.Both)
            {
                Vector2 pairPos3 = obj.Position + new Vector2((0f - (obj.Position.X - m.Position.X)) * 2f, (0f - (obj.Position.Y - m.Position.Y)) * 2f);
                Thing t3 = Level.current.CollisionPoint(pairPos3, obj.GetType());
                if (t3 != null)
                    RemoveObject(t3);
            }
        }
        processingMirror = false;
    }

    public void AdjustSizeLimits(Thing pObject)
    {
        if (pObject.X < _topLeftMost.X)
            _topLeftMost.X = pObject.X;
        if (pObject.X > _bottomRightMost.X)
            _bottomRightMost.X = pObject.X;
        if (pObject.Y < _topLeftMost.Y)
            _topLeftMost.Y = pObject.Y;
        if (pObject.Y > _bottomRightMost.Y)
            _bottomRightMost.Y = pObject.Y;
    }

    public void RecalculateSizeLimits()
    {
        _topLeftMost = new Vector2(99999f, 99999f);
        _bottomRightMost = new Vector2(-99999f, -99999f);
        foreach (Thing t in _levelThings)
            AdjustSizeLimits(t);
    }

    public void ClearEverything()
    {
        foreach (Thing t in _levelThingsNormal)
            current.RemoveThing(t);
        _levelThingsNormal.Clear();
        foreach (Thing t2 in _levelThingsAlternate)
            current.RemoveThing(t2);
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
        things.quadTree.Clear();
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
        Content.customPreviewCenter = Vector2.Zero;
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
            _tilePosition = new Vector2(0f, 0f);
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

    public void UpdateObjectMenu()
    {
        if (_objectMenu != null)
        {
            Remove(_objectMenu);
        }
        _objectMenu = new PlacementMenu(0f, 0f);
        Add(_objectMenu);
        ContextMenu objectMenu = _objectMenu;
        bool visible = (_objectMenu.active = false);
        objectMenu.visible = visible;
    }

    public void OpenMenu(ContextMenu menu)
    {
        bool flag = (menu.visible = true);
        menu.active = flag;
        if (inputMode == EditorInput.Mouse)
        {
            menu.X = Mouse.x;
            menu.Y = Mouse.y;
        }
        if (openPosition != Vector2.Zero)
        {
            menu.Position = openPosition + new Vector2(-2, -3);
            openPosition = Vector2.Zero;
        }
        if (_showPlacementMenu)
        {
            menu.X = 96;
            menu.Y = 32;
            _showPlacementMenu = false;
        }
        if (inputMode == EditorInput.Gamepad || inputMode == EditorInput.Touch)
        {
            menu.X = 16;
            menu.Y = 16;
        }
        menu.opened = true;
        _placementMenu = menu;
        disableDragMode();
    }

    public void ShowNoSpawnsDialogue()
    {
        if (_noSpawnsDialogue == null)
        {
            _noSpawnsDialogue = new MessageDialogue(null);
            Add(_noSpawnsDialogue);
        }
        _noSpawnsDialogue.Open("NO SPAWNS", "", "Your level has no spawns.\n\n\n@_!DUCKSPAWN@\n\n\nPlease place a |DGBLUE|Spawns/Spawn Point|PREV|\n in your level.");
        lockInput = _noSpawnsDialogue;
        _noSpawnsDialogue.okayOnly = true;
        _noSpawnsDialogue.windowYOffsetAdd = -30;
    }

    public void DoMenuClose()
    {
        if (_placementMenu != null)
        {
            if (_placementMenu != _objectMenu)
                RemoveThing(_placementMenu);
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

    public void CompleteDialogue(ContextMenu pItem)
    {
    }

    public void CloseMenu()
    {
        _closeMenu = true;
    }

    public void DoSave(string saveName)
    {
        _saveName = saveName;
        if (!_saveName.EndsWith(".lev"))
            _saveName += ".lev";
        Save();
    }

    public void LoadLevel(string load)
    {
        load = load.Replace('\\', '/');
        while (load.StartsWith('/'))
            load = load[1..];
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
            _currentLevelData.metaData.guid = Guid.NewGuid().ToString();
        _onlineSettingChanged = true;
        if (_currentLevelData.customData != null)
        {
            if (_currentLevelData.customData.customTileset01Data != null)
                Custom.ApplyCustomData(_currentLevelData.customData.customTileset01Data.GetTileData(), 0, CustomType.Block);
            if (_currentLevelData.customData.customTileset02Data != null)
                Custom.ApplyCustomData(_currentLevelData.customData.customTileset02Data.GetTileData(), 1, CustomType.Block);
            if (_currentLevelData.customData.customTileset03Data != null)
                Custom.ApplyCustomData(_currentLevelData.customData.customTileset03Data.GetTileData(), 2, CustomType.Block);
            if (_currentLevelData.customData.customBackground01Data != null)
                Custom.ApplyCustomData(_currentLevelData.customData.customBackground01Data.GetTileData(), 0, CustomType.Background);
            if (_currentLevelData.customData.customBackground02Data != null)
                Custom.ApplyCustomData(_currentLevelData.customData.customBackground02Data.GetTileData(), 1, CustomType.Background);
            if (_currentLevelData.customData.customBackground03Data != null)
                Custom.ApplyCustomData(_currentLevelData.customData.customBackground03Data.GetTileData(), 2, CustomType.Background);
            if (_currentLevelData.customData.customPlatform01Data != null)
                Custom.ApplyCustomData(_currentLevelData.customData.customPlatform01Data.GetTileData(), 0, CustomType.Platform);
            if (_currentLevelData.customData.customPlatform02Data != null)
                Custom.ApplyCustomData(_currentLevelData.customData.customPlatform02Data.GetTileData(), 1, CustomType.Platform);
            if (_currentLevelData.customData.customPlatform03Data != null)
                Custom.ApplyCustomData(_currentLevelData.customData.customPlatform03Data.GetTileData(), 2, CustomType.Platform);
            if (_currentLevelData.customData.customParallaxData != null)
                Custom.ApplyCustomData(_currentLevelData.customData.customParallaxData.GetTileData(), 0, CustomType.Parallax);
        }
        previewCapture = LoadPreview(_currentLevelData.previewData.preview);
        _pathNorth = false;
        _pathSouth = false;
        _pathEast = false;
        _pathWest = false;
        _miniMode = false;
        int sideMask = _currentLevelData.proceduralData.sideMask;
        if ((sideMask & 1) != 0)
            _pathNorth = true;
        if ((sideMask & 2) != 0)
            _pathEast = true;
        if ((sideMask & 4) != 0)
            _pathSouth = true;
        if ((sideMask & 8) != 0)
            _pathWest = true;
        if (sideMask != 0)
            _miniMode = true;
        _loadingLevel = true;
        LoadObjects(pAlternate: false);
        LoadObjects(pAlternate: true);
        _loadingLevel = false;
        _editorLoadFinished = true;
        if (!_looseClear)
            CenterView();
        hasUnsavedChanges = false;
        Thing.loadingLevel = null;
    }

    public void LoadObjects(bool pAlternate)
    {
        foreach (BinaryClassChunk item in pAlternate ? _currentLevelData.proceduralData.openAirAlternateObjects.objects : _currentLevelData.objects.objects)
        {
            Thing t = Thing.LoadThing(item);
            if (Thing.CheckForBozoData(t) || t == null || !t.editorCanModify)
                continue;
            if (pAlternate)
            {
                t.active = false;
                if (t is ThingContainer)
                {
                    ThingContainer container = t as ThingContainer;
                    if (container.bozocheck)
                    {
                        foreach (Thing thing in container.things)
                            if (!Thing.CheckForBozoData(thing))
                                _levelThingsAlternate.Add(thing);
                        continue;
                    }
                    foreach (Thing thing2 in container.things)
                        _levelThingsAlternate.Add(thing2);
                }
                else
                    _levelThingsAlternate.Add(t);
            }
            else
                AddObject(t);
        }
    }

    public void LegacyLoadLevel(string load)
    {
        load = load.Replace('\\', '/');
        while (load.StartsWith('/'))
            load = load[1..];
        DuckXML doc = ((_additionalSaveDirectory != null) ? DuckXML.Load(load) : DuckFile.LoadDuckXML(load));
        _saveName = load;
        LegacyLoadLevelParts(null);
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
            _currentLevelData.metaData.onlineMode = Convert.ToBoolean(onlineElement.Value);
        else
            _currentLevelData.metaData.onlineMode = false;
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
                _pathNorth = true;
            if ((num & 2) != 0)
                _pathEast = true;
            if ((num & 4) != 0)
                _pathSouth = true;
            if ((num & 8) != 0)
                _pathWest = true;
            if (num != 0)
                _miniMode = true;
        }
        DXMLNode workshopElement = lev.Element("workshopID");
        if (workshopElement != null)
            _currentLevelData.metaData.workshopID = Convert.ToUInt64(workshopElement.Value);
        workshopElement = lev.Element("workshopName");
        if (workshopElement != null)
            _currentLevelData.workshopData.name = workshopElement.Value;
        workshopElement = lev.Element("workshopDescription");
        if (workshopElement != null)
            _currentLevelData.workshopData.description = workshopElement.Value;
        workshopElement = lev.Element("workshopVisibility");
        if (workshopElement != null)
            _currentLevelData.workshopData.visibility = (RemoteStoragePublishedFileVisibility)Convert.ToInt32(workshopElement.Value);
        workshopElement = lev.Element("workshopTags");
        if (workshopElement != null)
        {
            string[] tagz = workshopElement.Value.Split('|');
            _currentLevelData.workshopData.tags = [];
            if (tagz.Length != 0 && tagz[0] != "")
                _currentLevelData.workshopData.tags = [.. tagz];
        }
        DXMLNode chanceElement = lev.Element("Chance");
        if (chanceElement != null)
            _currentLevelData.proceduralData.chance = Convert.ToSingle(chanceElement.Value);
        DXMLNode maDXMLNode = lev.Element("MaxPerLev");
        if (maDXMLNode != null)
            _currentLevelData.proceduralData.maxPerLevel = Convert.ToInt32(maDXMLNode.Value);
        DXMLNode singleElement = lev.Element("Single");
        if (singleElement != null)
            _currentLevelData.proceduralData.enableSingle = Convert.ToBoolean(singleElement.Value);
        DXMLNode multiElement = lev.Element("Multi");
        if (multiElement != null)
            _currentLevelData.proceduralData.enableMulti = Convert.ToBoolean(multiElement.Value);
        DXMLNode canMirrorElement = lev.Element("CanMirror");
        if (canMirrorElement != null)
            _currentLevelData.proceduralData.canMirror = Convert.ToBoolean(canMirrorElement.Value);
        DXMLNode isMirroredElement = lev.Element("IsMirrored");
        if (isMirroredElement != null)
            _currentLevelData.proceduralData.isMirrored = Convert.ToBoolean(isMirroredElement.Value);
        _loadingLevel = true;
        IEnumerable<DXMLNode> objectsNode = lev.Elements("Objects");
        if (objectsNode != null)
            foreach (DXMLNode elly in objectsNode.Elements("Object"))
                AddObject(Thing.LegacyLoadThing(elly));
        _loadingLevel = false;
        _editorLoadFinished = true;
        if (!_looseClear)
            CenterView();
    }

    public void SerializeObjects(bool pAlternate)
    {
        List<BinaryClassChunk> objects = (pAlternate ? _currentLevelData.proceduralData.openAirAlternateObjects.objects : _currentLevelData.objects.objects);
        List<Thing> things = (pAlternate ? _levelThingsAlternate : _levelThingsNormal);
        objects.Clear();
        if (things.Count <= 0)
            return;
        foreach (Thing item in things)
            item.processedByEditor = false;
        MultiMap<Type, Thing> groups = [];
        foreach (Thing t in things)
        {
            if (t.editorCanModify && !t.processedByEditor)
            {
                t.processedByEditor = true;
                if (t.canBeGrouped)
                    groups.Add(t.GetType(), t);
                else
                    objects.Add(t.Serialize());
            }
        }
        foreach (KeyValuePair<Type, List<Thing>> pair in groups)
        {
            ThingContainer container = new(pair.Value, pair.Key)
            {
                quickSerialize = minimalConversionLoad
            };
            objects.Add(container.Serialize());
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

    public void Play()
    {
        if (!_runLevelAnyway && !arcadeMachineMode && _levelThings.FirstOrDefault(x => x is FreeSpawn || x is TeamSpawn || x is CustomCamera) == null)
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
            name = SaveTempVersion();
        CloseMenu();
        RunTestLevel(name);
    }

    public bool Save(bool isTempSaveForPlayTestMode = false)
    {
        if (_saveName == "")
            SaveAs();
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
                _currentLevelData.SetPath(_saveName);
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
                LevelGenerator.ReInitialize();
            foreach (Thing levelThing in _levelThings)
                levelThing.processedByEditor = false;
            saving = false;
            if (!isTempSaveForPlayTestMode)
                hasUnsavedChanges = false;
        }
        return true;
    }

    public string SaveTempVersion()
    {
        string name = _saveName;
        string tempName = (_saveName = Directory.GetCurrentDirectory() + "\\Content\\_tempPlayLevel.lev");
        Save(isTempSaveForPlayTestMode: true);
        _saveName = name;
        return tempName;
    }

    public Vector2 GetAlignOffset(TileButtonAlign align)
    {
        switch (align)
        {
            case TileButtonAlign.ProcGridTopLeft:
                {
                    int reqW3 = 192;
                    int reqH6 = 144;
                    return new Vector2
                    {
                        X = -(_procTilesWide - (_procTilesWide - _procXPos)) * reqW3,
                        Y = -(_procTilesHigh - (_procTilesHigh - _procYPos)) * reqH6 - 16
                    };
                }
            case TileButtonAlign.TileGridTopLeft:
                return new Vector2
                {
                    X = 0,
                    Y = -16
                };
            case TileButtonAlign.TileGridTopRight:
                {
                    int reqW2 = 192;
                    return new Vector2
                    {
                        X = reqW2 - 16,
                        Y = -16
                    };
                }
            case TileButtonAlign.TileGridBottomLeft:
                {
                    int reqH5 = 144;
                    return new Vector2
                    {
                        X = 0,
                        Y = reqH5
                    };
                }
            case TileButtonAlign.TileGridBottomRight:
                {
                    int reqH4 = 144;
                    int reqW = 192;
                    return new Vector2
                    {
                        X = reqW - 16,
                        Y = reqH4
                    };
                }
            case TileButtonAlign.TileGridRight:
                {
                    int reqH3 = 144;
                    return new Vector2(192, reqH3 / 2 - 8);
                }
            case TileButtonAlign.TileGridLeft:
                {
                    int reqH2 = 144;
                    return new Vector2(-16, reqH2 / 2 - 8);
                }
            case TileButtonAlign.TileGridTop:
                return new Vector2(88, -16);
            case TileButtonAlign.TileGridBottom:
                {
                    int reqH = 144;
                    return new Vector2(88, reqH);
                }
            default:
                return Vector2.Zero;
        }
    }

    public LevelData CreateSaveData(bool isTempSaveForPlayTestMode = false)
    {
        Level curLevel = core.currentLevel;
        core.currentLevel = this;
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
            _currentLevelData.customData.customTileset01Data.ignore = true;
        dat = Custom.GetData(1, CustomType.Block);
        if (dat != null && dat.path != null && dat.texture != null)
        {
            dat.ApplyToChunk(_currentLevelData.customData.customTileset02Data);
            _currentLevelData.metaData.hasCustomArt = true;
            _currentLevelData.customData.customTileset02Data.ignore = false;
        }
        else
            _currentLevelData.customData.customTileset02Data.ignore = true;
        dat = Custom.GetData(2, CustomType.Block);
        if (dat != null && dat.path != null && dat.texture != null)
        {
            dat.ApplyToChunk(_currentLevelData.customData.customTileset03Data);
            _currentLevelData.metaData.hasCustomArt = true;
            _currentLevelData.customData.customTileset03Data.ignore = false;
        }
        else
            _currentLevelData.customData.customTileset03Data.ignore = true;
        dat = Custom.GetData(0, CustomType.Background);
        if (dat != null && dat.path != null && dat.texture != null)
        {
            dat.ApplyToChunk(_currentLevelData.customData.customBackground01Data);
            _currentLevelData.metaData.hasCustomArt = true;
            _currentLevelData.customData.customBackground01Data.ignore = false;
        }
        else
            _currentLevelData.customData.customBackground01Data.ignore = true;
        dat = Custom.GetData(1, CustomType.Background);
        if (dat != null && dat.path != null && dat.texture != null)
        {
            dat.ApplyToChunk(_currentLevelData.customData.customBackground02Data);
            _currentLevelData.metaData.hasCustomArt = true;
            _currentLevelData.customData.customBackground02Data.ignore = false;
        }
        else
            _currentLevelData.customData.customBackground02Data.ignore = true;
        dat = Custom.GetData(2, CustomType.Background);
        if (dat != null && dat.path != null && dat.texture != null)
        {
            dat.ApplyToChunk(_currentLevelData.customData.customBackground03Data);
            _currentLevelData.metaData.hasCustomArt = true;
            _currentLevelData.customData.customBackground03Data.ignore = false;
        }
        else
            _currentLevelData.customData.customBackground03Data.ignore = true;
        dat = Custom.GetData(0, CustomType.Platform);
        if (dat != null && dat.path != null && dat.texture != null)
        {
            dat.ApplyToChunk(_currentLevelData.customData.customPlatform01Data);
            _currentLevelData.metaData.hasCustomArt = true;
            _currentLevelData.customData.customPlatform01Data.ignore = false;
        }
        else
            _currentLevelData.customData.customPlatform01Data.ignore = true;
        dat = Custom.GetData(1, CustomType.Platform);
        if (dat != null && dat.path != null && dat.texture != null)
        {
            dat.ApplyToChunk(_currentLevelData.customData.customPlatform02Data);
            _currentLevelData.metaData.hasCustomArt = true;
            _currentLevelData.customData.customPlatform02Data.ignore = false;
        }
        else
            _currentLevelData.customData.customPlatform02Data.ignore = true;
        dat = Custom.GetData(2, CustomType.Platform);
        if (dat != null && dat.path != null && dat.texture != null)
        {
            dat.ApplyToChunk(_currentLevelData.customData.customPlatform03Data);
            _currentLevelData.metaData.hasCustomArt = true;
            _currentLevelData.customData.customPlatform03Data.ignore = false;
        }
        else
            _currentLevelData.customData.customPlatform03Data.ignore = true;
        dat = Custom.GetData(0, CustomType.Parallax);
        if (dat != null && dat.path != null && dat.texture != null)
        {
            dat.ApplyToChunk(_currentLevelData.customData.customParallaxData);
            _currentLevelData.metaData.hasCustomArt = true;
            _currentLevelData.customData.customParallaxData.ignore = false;
        }
        else
            _currentLevelData.customData.customParallaxData.ignore = true;
        _currentLevelData.modData.workshopIDs.Clear();
        if (_things.Count > 0)
        {
            HashSet<Mod> modsUsed = new HashSet<Mod>();
            foreach (Thing thing in levelThings)
            {
                modsUsed.Add(ModLoader.GetModFromType(thing.GetType()));
                if (thing is IContainAThing { contains: not null } contained)
                    modsUsed.Add(ModLoader.GetModFromType(contained.contains));
            }
            modsUsed.RemoveWhere(a => a == null || a is CoreMod || a is DisabledMod);
            if (modsUsed.Count != 0)
            {
                foreach (Mod mod in modsUsed)
                {
                    if (mod.configuration.workshopID != 0L || mod.workshopIDFacade != 0L)
                        _currentLevelData.modData.workshopIDs.Add((mod.workshopIDFacade != 0L) ? mod.workshopIDFacade : mod.configuration.workshopID);
                    else
                        _currentLevelData.modData.hasLocalMods = true;
                }
            }
        }
        string weaponConfigString = "",
               spawnerConfigString = "";
        int numArmor = 0,
            numEquipment = 0,
            numSpawns = 0,
            numTeamSpawns = 0,
            numLockedDoors = 0,
            numKeys = 0;
        _currentLevelData.metaData.eightPlayer = false;
        _currentLevelData.metaData.eightPlayerRestricted = false;
        _currentLevelData.objects.objects.Clear();
        if (_levelThings.Count > 0)
        {
            _ = new MultiMap<Type, Thing>();
            foreach (Thing t in _levelThings)
            {
                if (t is EightPlayer)
                {
                    _currentLevelData.metaData.eightPlayer = true;
                    _currentLevelData.metaData.eightPlayerRestricted = (t as EightPlayer).eightPlayerOnly.value;
                }
                if (!t.editorCanModify || t.processedByEditor)
                    continue;
                if (_miniMode)
                {
                    if (t is Key)
                        numKeys++;
                    else if (t is Door && (t as Door).locked)
                        numLockedDoors++;
                    else if (t is Equipment)
                    {
                        if (t is ChestPlate || t is Helmet || t is KnightHelmet)
                            numArmor++;
                        else
                            numEquipment++;
                    }
                    else if (t is Gun)
                    {
                        if (weaponConfigString != "")
                            weaponConfigString += "|";
                        weaponConfigString += ModLoader.SmallTypeName(t.GetType());
                    }
                    else if (t is ItemSpawner)
                    {
                        ItemSpawner spawner = t as ItemSpawner;
                        if (typeof(Gun).IsAssignableFrom(spawner.contains) && spawner.likelyhoodToExist == 1 && !spawner.randomSpawn)
                        {
                            if (spawner.spawnNum < 1 && spawner.spawnTime < 8f && spawner.isAccessible)
                            {
                                if (spawnerConfigString != "")
                                    spawnerConfigString += "|";
                                spawnerConfigString += ModLoader.SmallTypeName(spawner.contains);
                            }
                            if (weaponConfigString != "")
                                weaponConfigString += "|";
                            weaponConfigString += ModLoader.SmallTypeName(spawner.contains);
                        }
                    }
                    else if (t.GetType() == typeof(ItemBox))
                    {
                        ItemBox spawner2 = t as ItemBox;
                        if (typeof(Gun).IsAssignableFrom(spawner2.contains) && spawner2.likelyhoodToExist == 1 && spawner2.isAccessible)
                        {
                            if (spawnerConfigString != "")
                                spawnerConfigString += "|";
                            spawnerConfigString += ModLoader.SmallTypeName(spawner2.contains);
                            if (weaponConfigString != "")
                                weaponConfigString += "|";
                            weaponConfigString += ModLoader.SmallTypeName(spawner2.contains);
                        }
                    }
                    else if (t is SpawnPoint)
                        numSpawns++;
                    else if (t is TeamSpawn)
                        numTeamSpawns++;
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
                sideMask |= 1;
            if (_pathEast)
                sideMask |= 2;
            if (_pathSouth)
                sideMask |= 4;
            if (_pathWest)
                sideMask |= 8;
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
            _currentLevelData.previewData.preview = TextureToString(previewCapture);
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
            data.metaData.guid = "tempPlayLevel";
        core.currentLevel = curLevel;
        return data;
    }
    #endregion

    #region Internal Methods
    internal static string SerializeTypeName(Type t)
    {
        if (t == null)
            return "";
        return ModLoader.SmallTypeName(t);
    }

    internal static Type GetType(string name)
    {
        return ModLoader.GetType(name);
    }

    internal static Type DeSerializeTypeName(string serializedTypeName)
    {
        if (serializedTypeName == "")
            return null;
        return GetType(serializedTypeName);
    }
    #endregion

    #region Private Methods
    static void RegisterEditorFields(Type pType)
    {
        if (!EditorFieldsForType.TryGetValue(pType, out List<FieldInfo> fields))
        {
            List<FieldInfo> list = (EditorFieldsForType[pType] = []);
            fields = list;
        }
        foreach (Type baseType in AllBaseTypes[pType])
            if (AllEditorFields.TryGetValue(baseType, out IEnumerable<FieldInfo> value))
                fields.AddRange(value);
    }

    static object GetDefaultValue(Type t)
    {
        if (t.IsValueType)
            return Activator.CreateInstance(t);
        return null;
    }

    static List<MethodInfo> GetNetworkActionMethods(Type pType)
    {
        if (!_networkActionIndexes.ContainsKey(pType))
        {
            List<MethodInfo> infos = new List<MethodInfo>();
            MethodInfo[] methods = pType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo i2 in methods)
                if (i2.GetCustomAttributes(typeof(NetworkAction), inherit: false).Length != 0)
                    infos.Add(i2);
            if (pType.BaseType != null)
            {
                List<MethodInfo> baseMethods = GetNetworkActionMethods(pType.BaseType);
                infos.AddRange(baseMethods);
            }
            _networkActionIndexes[pType] = infos;
        }
        return _networkActionIndexes[pType];
    }

    void RunCommand(Command command)
    {
        hasUnsavedChanges = true;
        if (_lastCommand < _commands.Count - 1)
            _commands.RemoveRange(_lastCommand + 1, _commands.Count - (_lastCommand + 1));
        _commands.Add(command);
        _lastCommand++;
        command.Do();
    }

    void UndoCommand()
    {
        hasUnsavedChanges = true;
        if (_lastCommand >= 0)
            _commands[_lastCommand--].Undo();
    }

    void RedoCommand()
    {
        hasUnsavedChanges = true;
        if (_lastCommand < _commands.Count - 1)
            _commands[++_lastCommand].Do();
    }

    void Resave(string root)
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
                    _currentLevelData.metaData.guid = Guid.NewGuid().ToString();
                existingGUID.Add(_currentLevelData.metaData.guid);
                Save();
                Thread.Sleep(10);
            }
            catch (Exception)
            {
            }
        }
        foreach (string dir in DuckFile.GetDirectoriesNoCloud(root))
            Resave(dir);
    }

    void EndCurrentTouchMode()
    {
        if (!_showPlacementMenu)
            _closeMenu = true;
        _touchState = EditorTouchState.Normal;
        _activeTouchButton = null;
        clickedMenu = true;
        _editMode = false;
        _copyMode = false;
        _hover = null;
    }

    void disableDragMode()
    {
        _dragMode = false;
        _deleteMode = false;
        if (_move != null)
            _move = null;
        dragModeInputType = InputType.eNone;
        History.EndUndoSection();
    }

    void HugObjectPlacement()
    {
        if (_placementType is ItemSpawner)
            (_placementType as ItemSpawner)._seated = false;
        if ((_placementType.hugWalls & WallHug.Right) != WallHug.None && CollisionLine<IPlatform>(_tilePosition, _tilePosition + new Vector2(16f, 0f), _placementType) is Thing b && b.GetType() != _placementType.GetType())
            _tilePosition.X = b.left - _placementType.collisionSize.X - _placementType.collisionOffset.X;
        if ((_placementType.hugWalls & WallHug.Left) != WallHug.None && CollisionLine<IPlatform>(_tilePosition, _tilePosition + new Vector2(-16f, 0f), _placementType) is Thing b2 && b2.GetType() != _placementType.GetType())
            _tilePosition.X = b2.right - _placementType.collisionOffset.X;
        if ((_placementType.hugWalls & WallHug.Ceiling) != WallHug.None && CollisionLine<IPlatform>(_tilePosition, _tilePosition + new Vector2(0f, -16f), _placementType) is Thing b3 && b3.GetType() != _placementType.GetType())
            _tilePosition.Y = b3.bottom - _placementType.collisionOffset.Y;
        if ((_placementType.hugWalls & WallHug.Floor) != WallHug.None && CollisionLine<IPlatform>(_tilePosition, _tilePosition + new Vector2(0f, 16f), _placementType) is Thing b4 && b4.GetType() != _placementType.GetType())
        {
            _tilePosition.Y = b4.top - _placementType.collisionSize.Y - _placementType.collisionOffset.Y;
            if (_placementType is ItemSpawner)
                (_placementType as ItemSpawner)._seated = true;
        }
    }

    void UpdateSelection(bool pObjectsChanged = true)
    {
        foreach (Thing t in _levelThings)
        {
            if (pObjectsChanged)
                t.EditorObjectsChanged();
            t.material = null;
        }
        foreach (Thing thing in current.things)
            thing.material = null;
        foreach (Thing t2 in _selection)
        {
            if (t2 is AutoBlock)
            {
                AutoBlock ab = t2 as AutoBlock;
                if (ab._bLeftNub != null)
                    _currentDragSelectionHover.Add(ab._bLeftNub);
                if (ab._bRightNub != null)
                    _currentDragSelectionHover.Add(ab._bRightNub);
            }
            else if (t2 is AutoPlatform)
            {
                AutoPlatform ab2 = t2 as AutoPlatform;
                if (ab2._leftNub != null)
                    _currentDragSelectionHover.Add(ab2._leftNub);
                if (ab2._rightNub != null)
                    _currentDragSelectionHover.Add(ab2._rightNub);
            }
            else if (t2 is Door)
            {
                Door ab3 = t2 as Door;
                if (ab3._frame != null)
                    _currentDragSelectionHover.Add(ab3._frame);
            }
            else if (t2 is ItemSpawner)
            {
                ItemSpawner ab4 = t2 as ItemSpawner;
                if (ab4._ball1 != null)
                    _currentDragSelectionHover.Add(ab4._ball1);
                if (ab4._ball2 != null)
                    _currentDragSelectionHover.Add(ab4._ball2);
            }
        }
        foreach (Thing item in _currentDragSelectionHover)
            item.material = _selectionMaterial;
        foreach (Thing t3 in _currentDragSelectionHoverAdd)
        {
            if (_currentDragSelectionHover.Contains(t3))
                t3.material = null;
            else
                t3.material = _selectionMaterial;
        }
    }

    void RebuildPasteBatch()
    {
        _pasteBatch.Clear();
        foreach (BinaryClassChunk item in _selectionCopy)
        {
            Thing t = Thing.LoadThing(item);
            _pasteBatch.Add(t);
        }
    }

    void UpdateDragSelection()
    {
        _dragSelectShiftModifier = Keyboard.Down(Keys.LeftShift) || Keyboard.Down(Keys.RightShift) || (inputMode == EditorInput.Gamepad && _selection.Count > 0);
        if (_cursorMode == CursorMode.Selection)
        {
            _selectionDragEnd = ((inputMode == EditorInput.Mouse) ? Mouse.positionScreen : _tilePosition);
            Vector2 tl = _selectionDragStart;
            Vector2 br = _selectionDragEnd;
            if (br.X < tl.X)
                (br.X, tl.X) = (tl.X, br.X);
            if (br.Y < tl.Y)
                (br.Y, tl.Y) = (tl.Y, br.Y);
            if (_dragSelectShiftModifier)
            {
                _currentDragSelectionHoverAdd.Clear();
                foreach (Thing t in CheckRectAll<Thing>(tl, br))
                    _currentDragSelectionHoverAdd.Add(t);
            }
            else
            {
                _currentDragSelectionHover.Clear();
                foreach (Thing t2 in CheckRectAll<Thing>(tl, br))
                    _currentDragSelectionHover.Add(t2);
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
                            _currentDragSelectionHover.Add(t3);
                    }
                }
                foreach (Thing t4 in _currentDragSelectionHover)
                    if (t4 is not ContextMenu && _levelThings.Contains(t4) && !_selection.Contains(t4))
                        _selection.Add(t4);
                _currentDragSelectionHoverAdd.Clear();
                dragStartInputType = InputType.eNone;
                _cursorMode = ((_selection.Count > 0) ? CursorMode.HasSelection : CursorMode.Normal);
                clickedMenu = true;
                _selectionDragStart = Vector2.Zero;
            }
            UpdateSelection(pObjectsChanged: false);
            return;
        }
        if (_cursorMode == CursorMode.Drag)
        {
            Vector2 dragTo = Maths.Snap(Mouse.positionScreen + new Vector2(_cellSize / 2f), _cellSize, _cellSize);
            if (inputMode == EditorInput.Gamepad)
                dragTo = Maths.Snap(_tilePosition + new Vector2(_cellSize / 2f), _cellSize, _cellSize);
            if (dragTo != _moveDragStart)
            {
                Vector2 dif = dragTo - _moveDragStart;
                _moveDragStart = dragTo;
                foreach (Thing t5 in _currentDragSelectionHover)
                {
                    History.Add(delegate
                    {
                        t5.Position += dif;
                        if (t5 is IDontMove)
                        {
                            current.things.quadTree.Remove(t5);
                            current.things.quadTree.Add(t5);
                        }
                    }, delegate
                    {
                        t5.Position -= dif;
                        if (t5 is IDontMove)
                        {
                            current.things.quadTree.Remove(t5);
                            current.things.quadTree.Add(t5);
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
                _copyCenter = Vector2.Zero;
                History.BeginUndoSection();
                foreach (Thing t6 in _selection)
                {
                    copying = true;
                    _selectionCopy.Add(t6.Serialize());
                    _copyCenter += t6.Position;
                    copying = false;
                    if (cut)
                    {
                        History.Add(() =>
                        {
                            RemoveObject(t6);
                        }, () =>
                        {
                            AddObject(t6);
                        });
                    }
                }
                _copyCenter /= _selection.Count;
                if (cut)
                {
                    _selection.Clear();
                    _currentDragSelectionHover.Clear();
                    UpdateSelection();
                }
                History.EndUndoSection();
                RebuildPasteBatch();
                HUD.AddPlayerChangeDisplay("@CLIPCOPY@Selection copied!", 1);
            }
            if ((Keyboard.Pressed(Keys.V) && _pasteBatch.Count > 0) || _performCopypaste)
            {
                _selection.Clear();
                _currentDragSelectionHover.Clear();
                _cursorMode = CursorMode.Pasting;
                UpdateSelection(pObjectsChanged: false);
            }
            pasteOffset = Maths.Snap(_copyCenter - Mouse.positionScreen, 16, 16);
            if (inputMode == EditorInput.Gamepad)
                pasteOffset = Maths.Snap(_copyCenter - _tilePosition, 16, 16);
            _performCopypaste = false;
            if (_cursorMode == CursorMode.Pasting)
            {
                if (Mouse.right == InputState.Released || (_input.Released("CANCEL") && inputMode == EditorInput.Gamepad))
                    _cursorMode = CursorMode.Normal;
                if (Mouse.left == InputState.Pressed || (_input.Pressed("SELECT") && inputMode == EditorInput.Gamepad))
                {
                    History.BeginUndoSection();
                    _selection.Clear();
                    _currentDragSelectionHover.Clear();
                    _isPaste = true;
                    foreach (Thing t7 in _pasteBatch)
                    {
                        _selection.Add(t7);
                        t7.Position -= pasteOffset;
                        foreach (Thing col in CollisionRectAll<Thing>(t7.Position + new Vector2(-6f, -6f), t7.Position + new Vector2(6f, 6f), null))
                        {
                            if (col.placementLayer == t7.placementLayer && _levelThings.Contains(col))
                            {
                                History.Add(() =>
                                {
                                    RemoveObject(col);
                                }, () =>
                                {
                                    AddObject(col);
                                });
                            }
                        }
                    }
                    foreach (Thing t8 in _selection)
                    {
                        History.Add(() =>
                        {
                            AddObject(t8);
                        }, () =>
                        {
                            RemoveObject(t8);
                        });
                    }
                    _selection.Clear();
                    _currentDragSelectionHover.Clear();
                    _isPaste = false;
                    RebuildPasteBatch();
                    _placeObjects.Clear();
                    things.RefreshState();
                    UpdateSelection();
                    disableDragMode();
                }
            }
        }
        else if (_cursorMode == CursorMode.Pasting)
            _cursorMode = CursorMode.Normal;
        if (_selection.Count > 0 && _cursorMode != CursorMode.Pasting && (Keyboard.Pressed(Keys.F) || (_input.Pressed("MENU1") && inputMode == EditorInput.Gamepad)))
        {
            Vector2 centerPoint = Vector2.Zero;
            if (_cursorMode == CursorMode.Pasting)
            {
                foreach (Thing t9 in _pasteBatch)
                    centerPoint += t9.Position;
                centerPoint /= _pasteBatch.Count;
            }
            else
            {
                foreach (Thing t10 in _selection)
                    centerPoint += t10.Position;
                centerPoint /= _selection.Count;
            }
            centerPoint = Maths.SnapRound(centerPoint, _cellSize / 2, _cellSize / 2);
            if (_cursorMode == CursorMode.Pasting)
            {
                foreach (Thing item in _pasteBatch)
                {
                    float dif2 = item.Position.X - centerPoint.X;
                    item.SetTranslation(new Vector2((0 - dif2) * 2, 0));
                    item.EditorFlip(pVertical: false);
                    item.flipHorizontal = !item.flipHorizontal;
                }
            }
            else
            {
                History.BeginUndoSection();
                foreach (Thing t11 in _selection)
                {
                    float dif3 = t11.Position.X - centerPoint.X;
                    History.Add(() =>
                    {
                        t11.SetTranslation(new Vector2((0 - dif3) * 2, 0));
                        t11.EditorFlip(pVertical: false);
                        t11.flipHorizontal = !t11.flipHorizontal;
                        if (t11 is IDontMove)
                        {
                            current.things.quadTree.Remove(t11);
                            current.things.quadTree.Add(t11);
                        }
                    }, () =>
                    {
                        t11.SetTranslation(new Vector2(dif3 * 2, 0));
                        t11.EditorFlip(pVertical: false);
                        t11.flipHorizontal = !t11.flipHorizontal;
                        if (t11 is IDontMove)
                        {
                            current.things.quadTree.Remove(t11);
                            current.things.quadTree.Add(t11);
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
                    History.Add(() =>
                    {
                        RemoveObject(t14);
                    }, () =>
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
                        _moveDragStart = Maths.Snap(_tilePosition + new Vector2(_cellSize / 2), _cellSize, _cellSize);
                    else
                        _moveDragStart = Maths.Snap(Mouse.positionScreen + new Vector2(_cellSize / 2), _cellSize, _cellSize);
                }
                else
                    endSelection = true;
            }
            if (_input.Released("CANCEL"))
            {
                if (_cursorMode == CursorMode.DragHover)
                    _performCopypaste = true;
                else
                    endSelection = true;
            }
            if (_cursorMode != CursorMode.Pasting && (Mouse.right == InputState.Released || endSelection) && (!_dragSelectShiftModifier || inputMode == EditorInput.Gamepad))
            {
                _cursorMode = CursorMode.Normal;
                _selection.Clear();
                _currentDragSelectionHover.Clear();
                UpdateSelection(pObjectsChanged: false);
            }
            Vector2 offset = new(0);
            if (Keyboard.Pressed(Keys.Up))
                offset.Y -= cellSize;
            if (Keyboard.Pressed(Keys.Down))
                offset.Y += cellSize;
            if (Keyboard.Pressed(Keys.Left))
                offset.X -= cellSize;
            if (Keyboard.Pressed(Keys.Right))
                offset.X += cellSize;
            if (!(offset != Vector2.Zero))
                return;
            hasUnsavedChanges = true;
            History.BeginUndoSection();
            foreach (Thing t15 in _selection)
            {
                History.Add(() =>
                {
                    t15.SetTranslation(offset);
                    if (t15 is IDontMove)
                    {
                        current.things.quadTree.Remove(t15);
                        current.things.quadTree.Add(t15);
                    }
                }, () =>
                {
                    t15.SetTranslation(-offset);
                    if (t15 is IDontMove)
                    {
                        current.things.quadTree.Remove(t15);
                        current.things.quadTree.Add(t15);
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

    void UpdateHover(Layer placementLayer, Vector2 tilePosition, bool isDrag = false)
    {
        IEnumerable<Thing> hoverList = [];
        if (inputMode == EditorInput.Gamepad || isDrag)
            hoverList = CollisionPointAll<Thing>(tilePosition);
        else if (inputMode == EditorInput.Touch && TouchScreen.IsScreenTouched())
        {
            if (_editMode || _copyMode)
            {
                if (TouchScreen.GetTap() != Touch.None)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        hoverList = CollisionCircleAll<Thing>(TouchScreen.GetTap().positionCamera, (float)i * 2f);
                        if (hoverList.Any())
                            break;
                    }
                    _hover = null;
                }
            }
            else if (TouchScreen.GetTouch() != Touch.None)
                hoverList = CollisionPointAll<Thing>(tilePosition);
        }
        else if (inputMode == EditorInput.Mouse && !isDrag)
            hoverList = CollisionPointAll<Thing>(Mouse.positionScreen);
        oldHover = _hover;
        if (!_editMode)
            _hover = null;
        _secondaryHover = null;
        List<Thing> secondaryHoverList = [];
        foreach (Thing t in hoverList)
        {
            if (t is TileButton || !_placeables.Contains(t.GetType()) || !t.editorCanModify || !_things.Contains(t) || (_placementType is WireTileset && t is IWirePeripheral) || (_placementType is IWirePeripheral && t is WireTileset))
                continue;
            if (_placementType is PipeTileset && t is PipeTileset && _placementType.GetType() != t.GetType())
                secondaryHoverList.Add(t);
            else if (t.placementLayer != placementLayer && !_copyMode && !_editMode)
                secondaryHoverList.Add(t);
            else if (_hover == null)
            {
                if (_placementType != null && _placementType is BackgroundTile)
                {
                    if (_things.Contains(t))
                    {
                        if (t.GetType() == _placementType.GetType())
                            _hover = t;
                        else
                            secondaryHoverList.Add(t);
                    }
                }
                else if (t.editorCanModify)
                    _hover = t;
            }
            else if (t != _hover)
                secondaryHoverList.Add(t);
        }
        if (inputMode == EditorInput.Mouse && !isDrag && _hover == null && !(_placementType is BackgroundTile) && !(_placementType is PipeTileset))
        {
            List<KeyValuePair<float, Thing>> nearest = current.nearest(tilePosition, _levelThings.AsEnumerable(), null, placementLayer, placementLayer: true);
            if (nearest.Count > 0 && (_placementType is not WireTileset || nearest[0].Value is not IWirePeripheral) && (_placementType is not IWirePeripheral || nearest[0].Value is not WireTileset) && (nearest[0].Value.Position - tilePosition).Length() < 8)
                _hover = nearest[0].Value;
        }
        if (_hover == null || oldHover == null || _hover.GetType() != oldHover.GetType())
        {
            if (_hover == null)
                _hoverMenu = null;
            else
                _hoverMenu = _hover.GetContextMenu();
        }
        if (secondaryHoverList.Count > 0)
        {
            IOrderedEnumerable<Thing> ordered = secondaryHoverList.OrderBy(x => (x.placementLayer == null) ? (-99999) : x.placementLayer.depth);
            if (Keyboard.control)
            {
                if (_hover == null)
                {
                    IOrderedEnumerable<Thing> reverseOrdered = secondaryHoverList.OrderBy(x => (x.placementLayer == null) ? 99999 : (-x.placementLayer.depth));
                    _hover = reverseOrdered.First();
                }
                else
                    _hover = ordered.First();
            }
            else if (_hover == null || Keyboard.control || (_placementType != null && ordered.First().placementLayer == _placementType.placementLayer))
            {
                _secondaryHover = ordered.First();
                _hoverMenu ??= _secondaryHover.GetContextMenu();
            }
        }
        if (_secondaryHover == null && _hover is Block && secondaryHoverList.Count > 0)
        {
            _secondaryHover = secondaryHoverList.FirstOrDefault(x => x is PipeTileset);
            if (_secondaryHover != null && !(_secondaryHover as PipeTileset)._foregroundDraw)
                _secondaryHover = null;
        }
    }

    void CalculateGridRestriction()
    {
        Vector2 size = _bottomRightMost - _topLeftMost;
        Vector2 fullRestriction = _sizeRestriction * 2f - size - new Vector2(16);
        if (fullRestriction.X > _sizeRestriction.X * 2f)
        {
            fullRestriction.X = _sizeRestriction.X * 2f;
        }
        if (fullRestriction.Y > _sizeRestriction.Y * 2f)
        {
            fullRestriction.Y = _sizeRestriction.Y * 2f;
        }
        _gridW = (int)(fullRestriction.X / _cellSize);
        _gridH = (int)(fullRestriction.Y / _cellSize);
    }

    void onLoad(object sender, CancelEventArgs e)
    {
        if (e.Cancel)
            return;
        IEnumerable<DXMLNode> objectsNode = DuckXML.Load(_saveName = _loadForm.FileName).Element("Level").Elements("Objects");
        if (objectsNode == null)
            return;
        ClearEverything();
        foreach (DXMLNode elly in objectsNode.Elements("Object"))
            AddObject(Thing.LegacyLoadThing(elly));
    }

    void CenterView()
    {
        camera.width = _gridW * 16;
        camera.height = camera.width / Resolution.current.aspect;
        camera.centerX = camera.width / 2f - 8;
        camera.centerY = camera.height / 2f - 8;
        float wid = camera.width;
        float hig = camera.height;
        camera.width *= 0.3f;
        camera.height *= 0.3f;
        camera.centerX -= (camera.width - wid) / 2;
        camera.centerY -= (camera.height - hig) / 2;
        if (_sizeRestriction.X > 0f)
            camera.center = (_topLeftMost + _bottomRightMost) / 2;
    }

    bool LevelIsOnlineCapable()
    {
        foreach (Thing levelThing in _levelThings)
            if (!ContentProperties.GetBag(levelThing.GetType()).GetOrDefault("isOnlineCapable", defaultValue: true))
                return false;
        return true;
    }

    LevelType GetLevelType()
    {
        if (arcadeMachineMode)
            return LevelType.Arcade_Machine;
        LevelType type = LevelType.Deathmatch;
        if (_levelThings.FirstOrDefault((Thing x) => x is ChallengeMode) != null)
            type = LevelType.Challenge;
        else if (_levelThings.FirstOrDefault((Thing x) => x is ArcadeMode) != null)
            type = LevelType.Arcade;
        return type;
    }

    LevelSize GetLevelSize()
    {
        _topLeft = new Vector2(99999f, 99999f);
        _bottomRight = new Vector2(-99999f, -99999f);
        CalculateBounds();
        float length = (base.topLeft - base.bottomRight).Length();
        LevelSize levelSizeVal = LevelSize.Ginormous;
        if (length < 900f)
            levelSizeVal = LevelSize.Large;
        if (length < 650f)
            levelSizeVal = LevelSize.Medium;
        if (length < 400f)
            levelSizeVal = LevelSize.Small;
        if (length < 200f)
            levelSizeVal = LevelSize.Tiny;
        return levelSizeVal;
    }

    Layer GetLayerOrOverride(Thing thingToCheck)
    {
        Layer layerResult = ((thingToCheck != null) ? thingToCheck.placementLayer : Layer.Game);
        if (thingToCheck != null && thingToCheck.placementLayerOverride != null)
            layerResult = thingToCheck.placementLayerOverride;
        else if (thingToCheck is AutoBlock)
            layerResult = Layer.Blocks;
        layerResult ??= Layer.Game;
        return layerResult;
    }
    #endregion
}