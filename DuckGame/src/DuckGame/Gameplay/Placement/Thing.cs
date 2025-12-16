using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace DuckGame;

/// <summary>
/// The base class for everything in Duck Game. Things can be added to the world
/// with Level.Add and they will be drawn and updated automatically.
/// </summary>
public abstract class Thing : Transform
{
    public int maxPlaceable = -1;

    public GhostPriority syncPriority;

    public ushort _ghostType = 1;

    public bool isLocal = true;

    public ushort specialSyncIndex;

    private int _networkSeed;

    private bool _initializedNetworkSeed;

    protected NetIndex8 _authority = 2;

    public NetIndex16 _lastAuthorityChange;

    protected NetworkConnection _connection;

    protected int _framesSinceTransfer = 999;

    private int _networkDrawIndex;

    protected bool _isStateObject;

    protected bool _isStateObjectInitialized;

    public Dictionary<NetworkConnection, uint> currentTick = new Dictionary<NetworkConnection, uint>();

    public bool inPipe;

    private static ushort _staticGlobalIndex = 0;

    private static ushort _staticPhysicsIndex = 0;

    private ushort _globalIndex = GetGlobalIndex();

    protected ushort _physicsIndex;

    private Vec2 _lerpPosition = Vec2.Zero;

    private Vec2 _lerpVector = Vec2.Zero;

    private float _lerpSpeed;

    private Portal _portal;

    protected SequenceItem _sequence;

    protected string _type = "";

    protected Level _level;

    protected float _lastTeleportDirection;

    private bool _removeFromLevel;

    protected bool _placed;

    protected bool _canBeGrouped;

    public float overfollow;

    protected Thing _owner;

    public Thing _prevOwner;

    protected Thing _lastThrownBy;

    protected bool _opaque;

    protected bool _opacityFromGraphic;

    protected Sprite _graphic;

    private DamageMap _damageMap;

    public bool lowLighting;

    private bool _visible = true;

    private Material _material;

    protected bool _enablePhysics = true;

    protected Profile _responsibleProfile;

    private Type _killThingType;

    public float _hSpeed;

    public float _vSpeed;

    protected bool _active = true;

    public bool serverOnly;

    private bool _action;

    private Anchor _anchor;

    public sbyte _offDir = 1;

    protected Layer _layer;

    protected bool _initialized;

    protected int _placementCost = 4;

    protected string _editorName = "";

    public Layer placementLayerOverride;

    public bool _canFlip = true;

    protected bool _canFlipVert;

    protected bool _canHaveChance = true;

    protected float _likelyhoodToExist = 1f;

    protected bool _editorCanModify = true;

    protected bool _processedByEditor;

    protected bool _visibleInGame = true;

    private Vec2 _editorOffset;

    private WallHug _hugWalls;

    protected bool _editorImageCenter;

    public static LevelData loadingLevel = null;

    private bool _isAccessible = true;

    public Type editorCycleType;

    protected bool _flipHorizontal;

    protected bool _flipVertical;

    private int _chanceGroup = -1;

    public string editorTooltip;

    /// <summary>
    /// Adding context menu item names to this filters out 'EditorProperty' values with the same name. 
    /// This is useful for removing undesired inherited EditorProperty members from the right click menu.
    /// </summary>
    protected HashSet<string> _contextMenuFilter = new HashSet<string>();

    public static Effect _alphaTestEffect;

    private bool _skipPositioning;

    private static Dictionary<Type, Sprite> _editorIcons = new Dictionary<Type, Sprite>();

    protected Sprite _editorIcon;

    protected bool _solid = true;

    protected Vec2 _collisionOffset;

    protected Vec2 _collisionSize;

    protected float _topQuick;

    protected float _bottomQuick;

    protected float _leftQuick;

    protected float _rightQuick;

    protected bool _isStatic;

    public static bool skipLayerAdding = false;

    private bool _networkInitialized;

    public int forceEditorGrid;

    public int wasSuperFondled;

    public ProfileNetData _netData;

    public bool isBitBufferCreatedGhostThing;

    private GhostObject _ghostObject;

    public NetIndex16 fixedGhostIndex;

    private bool _ignoreGhosting;

    public Vec2 prevEndVelocity = Vec2.Zero;

    private bool _redoLayer;

    public virtual Vec2 netPosition
    {
        get
        {
            return position;
        }
        set
        {
            position = value;
        }
    }

    public ushort ghostType
    {
        get
        {
            if (!_removeFromLevel)
            {
                return _ghostType;
            }
            return 0;
        }
        set
        {
            if (_ghostType != value)
            {
                _ghostType = value;
            }
        }
    }

    public int networkSeed
    {
        get
        {
            if (Network.isServer && !_initializedNetworkSeed && isStateObject)
            {
                _networkSeed = Rando.Int(2147483646);
                _initializedNetworkSeed = true;
            }
            return _networkSeed;
        }
        set
        {
            _networkSeed = value;
            _initializedNetworkSeed = true;
        }
    }

    public virtual NetIndex8 authority
    {
        get
        {
            return _authority;
        }
        set
        {
            if (_authority != value)
            {
                _lastAuthorityChange = Network.synchronizedTime;
            }
            _authority = value;
        }
    }

    public virtual NetworkConnection connection
    {
        get
        {
            return _connection;
        }
        set
        {
            if (value != _connection && ghostObject != null)
            {
                ghostObject.KillNetworkData();
                if (value == DuckNetwork.localConnection)
                {
                    ghostObject.TakeOwnership();
                }
            }
            _connection = value;
        }
    }

    public int networkDrawIndex
    {
        get
        {
            return _networkDrawIndex;
        }
        set
        {
            _networkDrawIndex = value;
        }
    }

    public bool isStateObject
    {
        get
        {
            if (!_isStateObjectInitialized)
            {
                _isStateObject = Editor.AllStateFields[GetType()].Length != 0;
                _isStateObjectInitialized = true;
            }
            return _isStateObject;
        }
    }

    public bool isServerForObject
    {
        get
        {
            if (Network.isActive && connection != null && connection != DuckNetwork.localConnection)
            {
                return loadingLevel != null;
            }
            return true;
        }
    }

    public bool isClientWhoCreatedObject
    {
        get
        {
            if (_ghostObject == null || loadingLevel != null)
            {
                return true;
            }
            return _ghostObject.ghostObjectIndex._index / GhostManager.kGhostIndexMax == DuckNetwork.localProfile.fixedGhostIndex;
        }
    }

    public virtual Vec2 cameraPosition => position;

    public ushort globalIndex
    {
        get
        {
            return _globalIndex;
        }
        set
        {
            _globalIndex = value;
        }
    }

    public ushort physicsIndex
    {
        get
        {
            return _globalIndex;
        }
        set
        {
            _globalIndex = value;
        }
    }

    public Vec2 lerpPosition
    {
        get
        {
            return _lerpPosition;
        }
        set
        {
            _lerpPosition = value;
        }
    }

    public Vec2 lerpVector
    {
        get
        {
            return _lerpVector;
        }
        set
        {
            _lerpVector = value;
        }
    }

    public float lerpSpeed
    {
        get
        {
            return _lerpSpeed;
        }
        set
        {
            _lerpSpeed = value;
        }
    }

    public Portal portal
    {
        get
        {
            return _portal;
        }
        set
        {
            _portal = value;
        }
    }

    public SequenceItem sequence
    {
        get
        {
            return _sequence;
        }
        set
        {
            _sequence = value;
        }
    }

    public string type => _type;

    public Level level
    {
        get
        {
            return _level;
        }
        set
        {
            _level = value;
        }
    }

    public float lastTeleportDirection
    {
        get
        {
            return _lastTeleportDirection;
        }
        set
        {
            _lastTeleportDirection = value;
        }
    }

    public bool removeFromLevel => _removeFromLevel;

    public virtual int frame
    {
        get
        {
            if (!(graphic is SpriteMap))
            {
                return 0;
            }
            return (graphic as SpriteMap).frame;
        }
        set
        {
            if (graphic is SpriteMap)
            {
                (graphic as SpriteMap).frame = value;
            }
        }
    }

    public bool placed
    {
        get
        {
            return _placed;
        }
        set
        {
            _placed = value;
        }
    }

    public bool canBeGrouped => _canBeGrouped;

    public virtual Thing realObject => this;

    public virtual Thing owner
    {
        get
        {
            return _owner;
        }
        set
        {
            if (_owner != value)
            {
                _prevOwner = _owner;
            }
            _lastThrownBy = _owner;
            _owner = value;
        }
    }

    public Thing prevOwner => _prevOwner;

    public Thing lastThrownBy => _lastThrownBy;

    public bool opaque => false;

    public virtual Sprite graphic
    {
        get
        {
            return _graphic;
        }
        set
        {
            _graphic = value;
        }
    }

    public DamageMap damageMap
    {
        get
        {
            return _damageMap;
        }
        set
        {
            _damageMap = value;
        }
    }

    public virtual bool visible
    {
        get
        {
            return _visible;
        }
        set
        {
            _visible = value;
        }
    }

    public Material material
    {
        get
        {
            return _material;
        }
        set
        {
            _material = value;
        }
    }

    public virtual bool enablePhysics
    {
        get
        {
            return _enablePhysics;
        }
        set
        {
            _enablePhysics = value;
        }
    }

    public Profile responsibleProfile
    {
        get
        {
            if (_responsibleProfile != null)
            {
                return _responsibleProfile;
            }
            Duck d = this as Duck;
            if (d == null)
            {
                d = owner as Duck;
                if (d == null)
                {
                    d = prevOwner as Duck;
                    if (d == null && owner != null)
                    {
                        d = owner.owner as Duck;
                        if (d == null)
                        {
                            d = owner.prevOwner as Duck;
                            if (d == null && prevOwner != null)
                            {
                                d = prevOwner.owner as Duck;
                                if (d == null)
                                {
                                    d = prevOwner.prevOwner as Duck;
                                }
                            }
                        }
                    }
                }
            }
            if (d == null && this is Bullet)
            {
                Bullet b = this as Bullet;
                if (b.firedFrom != null && !(b.firedFrom is Bullet))
                {
                    return b.firedFrom.responsibleProfile;
                }
            }
            return d?.profile;
        }
        set
        {
            _responsibleProfile = value;
        }
    }

    public Type killThingType
    {
        get
        {
            if (_killThingType != null)
            {
                return _killThingType;
            }
            if (this is Bullet)
            {
                Bullet b = this as Bullet;
                if (b.firedFrom != null)
                {
                    return b.firedFrom.GetType();
                }
            }
            if (this is SmallFire)
            {
                SmallFire f = this as SmallFire;
                if (f.firedFrom != null)
                {
                    return f.firedFrom.GetType();
                }
            }
            return GetType();
        }
        set
        {
            _killThingType = value;
        }
    }

    public virtual float hSpeed
    {
        get
        {
            return _hSpeed;
        }
        set
        {
            _hSpeed = value;
        }
    }

    public virtual float vSpeed
    {
        get
        {
            return _vSpeed;
        }
        set
        {
            _vSpeed = value;
        }
    }

    public Vec2 velocity
    {
        get
        {
            return new Vec2(hSpeed, vSpeed);
        }
        set
        {
            _hSpeed = value.x;
            _vSpeed = value.y;
        }
    }

    public virtual bool active
    {
        get
        {
            return _active;
        }
        set
        {
            _active = value;
        }
    }

    public virtual bool action
    {
        get
        {
            return _action;
        }
        set
        {
            _action = value;
        }
    }

    public Anchor anchor
    {
        get
        {
            return _anchor;
        }
        set
        {
            _anchor = value;
        }
    }

    public virtual Vec2 anchorPosition => position;

    public virtual sbyte offDir
    {
        get
        {
            return _offDir;
        }
        set
        {
            _offDir = value;
        }
    }

    public Layer layer
    {
        get
        {
            return _layer;
        }
        set
        {
            if (_layer == value)
            {
                return;
            }
            if (_level != null)
            {
                if (_layer != null)
                {
                    _layer.Remove(this);
                }
                value.Add(this);
            }
            _layer = value;
        }
    }

    public bool isInitialized => _initialized;

    public int placementCost => _placementCost;

    public string editorName
    {
        get
        {
            if (_editorName == "")
            {
                _editorName = GetType().Name;
                if (_editorName.Length > 1)
                {
                    for (int i = 1; i < _editorName.Length; i++)
                    {
                        char c = _editorName[i];
                        if (c >= 'A' && c <= 'Z')
                        {
                            char c2 = _editorName[i - 1];
                            if (c2 >= 'a' && c2 <= 'z')
                            {
                                _editorName = _editorName.Insert(i, " ");
                                i++;
                            }
                        }
                    }
                }
            }
            return _editorName;
        }
    }

    public Layer placementLayer
    {
        get
        {
            if (placementLayerOverride != null)
            {
                return placementLayerOverride;
            }
            return layer;
        }
    }

    public float likelyhoodToExist
    {
        get
        {
            return _likelyhoodToExist;
        }
        set
        {
            _likelyhoodToExist = value;
        }
    }

    public bool editorCanModify => _editorCanModify;

    public bool processedByEditor
    {
        get
        {
            return _processedByEditor;
        }
        set
        {
            _processedByEditor = value;
        }
    }

    public bool visibleInGame => _visibleInGame;

    public Vec2 editorOffset
    {
        get
        {
            return _editorOffset;
        }
        set
        {
            _editorOffset = value;
        }
    }

    public WallHug hugWalls
    {
        get
        {
            return _hugWalls;
        }
        set
        {
            _hugWalls = value;
        }
    }

    public bool isAccessible
    {
        get
        {
            return _isAccessible;
        }
        set
        {
            _isAccessible = value;
        }
    }

    public virtual bool flipHorizontal
    {
        get
        {
            return _flipHorizontal;
        }
        set
        {
            _flipHorizontal = value;
            offDir = (sbyte)((!_flipHorizontal) ? 1 : (-1));
        }
    }

    public virtual bool flipVertical
    {
        get
        {
            return _flipVertical;
        }
        set
        {
            _flipVertical = value;
        }
    }

    public int chanceGroup
    {
        get
        {
            return _chanceGroup;
        }
        set
        {
            _chanceGroup = value;
        }
    }

    public virtual bool solid
    {
        get
        {
            return _solid;
        }
        set
        {
            if (value && !_solid)
            {
                FixClipping();
            }
            _solid = value;
        }
    }

    public bool isOffBottomOfLevel
    {
        get
        {
            if (base.y > Level.activeLevel.lowestPoint + 100f)
            {
                return top > Level.current.camera.bottom + 8f;
            }
            return false;
        }
    }

    public virtual Vec2 collisionOffset
    {
        get
        {
            return _collisionOffset;
        }
        set
        {
            _collisionOffset = value;
        }
    }

    public virtual Vec2 collisionSize
    {
        get
        {
            return _collisionSize;
        }
        set
        {
            _collisionSize = value;
        }
    }

    public float topQuick => _topQuick;

    public float bottomQuick => _bottomQuick;

    public float leftQuick => _leftQuick;

    public float rightQuick => _rightQuick;

    public float topLocal => collisionOffset.y;

    public float bottomLocal => collisionOffset.y + collisionSize.y;

    public float top
    {
        get
        {
            return position.y + _collisionOffset.y;
        }
        set
        {
            position.y = value + (position.y - top);
        }
    }

    public float bottom
    {
        get
        {
            return position.y + _collisionOffset.y + _collisionSize.y;
        }
        set
        {
            position.y = value + (position.y - bottom);
        }
    }

    public float left
    {
        get
        {
            if (offDir <= 0)
            {
                return position.x - _collisionSize.x - _collisionOffset.x;
            }
            return position.x + _collisionOffset.x;
        }
        set
        {
            base.x = value + (base.x - left);
        }
    }

    public float right
    {
        get
        {
            if (offDir <= 0)
            {
                return position.x - _collisionOffset.x;
            }
            return position.x + _collisionOffset.x + _collisionSize.x;
        }
        set
        {
            base.x = value + (base.x - right);
        }
    }

    public Vec2 topLeft => new Vec2(left, top);

    public Vec2 topRight => new Vec2(right, top);

    public Vec2 bottomLeft => new Vec2(left, bottom);

    public Vec2 bottomRight => new Vec2(right, bottom);

    public bool isStatic
    {
        get
        {
            return _isStatic;
        }
        set
        {
            _isStatic = value;
        }
    }

    public float halfWidth => width / 2f;

    public float halfHeight => height / 2f;

    public float width => _collisionSize.x * base.scale.x;

    public float height => _collisionSize.y * base.scale.y;

    public float w => width;

    public float h => height;

    public Rectangle rectangle => new Rectangle((int)left, (int)top, (int)(right - left), (int)(bottom - top));

    public Vec2 collisionCenter
    {
        get
        {
            return new Vec2(left + collisionSize.x / 2f, top + collisionSize.y / 2f);
        }
        set
        {
            left = value.x - collisionSize.x / 2f;
            top = value.y - collisionSize.y / 2f;
        }
    }

    public GhostObject ghostObject
    {
        get
        {
            return _ghostObject;
        }
        set
        {
            _ghostObject = value;
        }
    }

    /// <summary>
    /// If true, this object's Update function is run via Level.UpdateThings. Otherwise, it's run via GhostManager.UpdateGhostLerp
    /// </summary>
    public bool shouldRunUpdateLocally
    {
        get
        {
            if (connection == null || connection.data == null)
            {
                return level != null;
            }
            return false;
        }
    }

    public bool ignoreGhosting
    {
        get
        {
            return _ignoreGhosting;
        }
        set
        {
            _ignoreGhosting = value;
        }
    }

    /// <summary>
    /// Gets the path to an asset that the mod that this Thing is a part of.
    /// </summary>
    /// <param name="asset">The asset name, relative to the mods' Content folder.</param>
    /// <returns>The path.</returns>
    public string GetPath(string asset)
    {
        return ModLoader._modAssemblies[GetType().Assembly].configuration.contentDirectory + asset.Replace('\\', '/');
    }

    /// <summary>
    /// Gets the path to an asset from a mod.
    /// </summary>
    /// <typeparam name="T">The mod type to fetch from</typeparam>
    /// <param name="asset">The asset name, relative to the mods' Content folder.</param>
    /// <returns>The path.</returns>
    public static string GetPath<T>(string asset) where T : Mod
    {
        return Mod.GetPath<T>(asset);
    }

    public void Fondle(Thing t)
    {
        if (t != null && t.CanBeControlled() && t.connection != connection)
        {
            t.OnFondle(connection, 1);
        }
    }

    public static void Fondle(Thing t, NetworkConnection c)
    {
        if (t != null && t != null && t.connection != c)
        {
            t.OnFondle(c, 1);
        }
    }

    public static void ExtraFondle(Thing t, NetworkConnection c)
    {
        t?.OnFondle(c, 8);
    }

    public static void SuperFondle(Thing t, NetworkConnection c)
    {
        t?.OnFondle(c, 25);
    }

    public static void UltraFondle(Thing t, NetworkConnection c)
    {
        t?.OnFondle(c, 30);
    }

    public static void UnstoppableFondle(Thing t, NetworkConnection c)
    {
        t?.OnFondle(c, 35);
    }

    protected virtual void OnFondle(NetworkConnection c, int fondleSize, bool pBreakRules = false)
    {
        if (Network.isActive && c == DuckNetwork.localConnection)
        {
            connection = c;
            authority += fondleSize;
        }
    }

    public static void PowerfulRuleBreakingFondle(Thing t, NetworkConnection c)
    {
        if (t != null && t != null && t.connection != c)
        {
            t.OnFondle(c, 1, pBreakRules: true);
        }
    }

    public static void AuthorityFondle(Thing t, NetworkConnection c, int fondleSize)
    {
        t?.OnFondle(c, fondleSize * c.authorityPower);
    }

    public virtual bool CanBeControlled()
    {
        return true;
    }

    public void IgnoreNetworkSync()
    {
        _isStateObject = false;
        _isStateObjectInitialized = true;
    }

    public virtual bool TransferControl(NetworkConnection to, NetIndex8 auth)
    {
        if (to == connection)
        {
            if (auth > authority)
            {
                authority = auth;
            }
            return true;
        }
        if (connection.profile != null && connection.profile.slotType != SlotType.Spectator)
        {
            if (auth < authority)
            {
                return false;
            }
            if (connection != null && CanBeControlled() && connection.profile != null && connection.profile.slotType != SlotType.Spectator && auth == authority && (connection.profile.networkIndex + DuckNetwork.levelIndex) % GameLevel.NumberOfDucks < (to.profile.networkIndex + DuckNetwork.levelIndex) % GameLevel.NumberOfDucks)
            {
                return false;
            }
        }
        if (NetIndex8.Difference(auth, authority) > 19)
        {
            wasSuperFondled = 120;
        }
        _framesSinceTransfer = 0;
        connection = to;
        authority = auth;
        return true;
    }

    public void PlaySFX(string sound, float vol = 1f, float pitch = 0f, float pan = 0f, bool looped = false)
    {
        if (isServerForObject)
        {
            SFX.PlaySynchronized(sound, vol, pitch, pan, looped);
        }
    }

    public virtual void SpecialNetworkUpdate()
    {
    }

    public virtual void SetTranslation(Vec2 translation)
    {
        position += translation;
    }

    public static ushort GetGlobalIndex()
    {
        _staticGlobalIndex = (ushort)((_staticGlobalIndex + 1) % 65535);
        if (_staticGlobalIndex == 0)
        {
            _staticGlobalIndex++;
        }
        return _staticGlobalIndex;
    }

    public static ushort GetPhysicsIndex()
    {
        _staticPhysicsIndex = (ushort)((_staticPhysicsIndex + 1) % 65535);
        if (_staticPhysicsIndex == 0)
        {
            _staticPhysicsIndex++;
        }
        return _staticPhysicsIndex;
    }

    public float Distance(Thing pOther)
    {
        if (pOther != null)
        {
            return (position - pOther.position).length;
        }
        return float.MaxValue;
    }

    public void Presto()
    {
        if (isServerForObject && !removeFromLevel)
        {
            if (Network.isActive)
            {
                Send.Message(new NMPop(position));
            }
            NMPop.AmazingDisappearingParticles(position);
            if (this is MaterialThing)
            {
                (this as MaterialThing).Destroy(new DTPop());
            }
            base.y += 9999f;
            if (!(this is Duck))
            {
                Level.Remove(this);
            }
        }
    }

    public void ApplyForce(Vec2 force)
    {
        _hSpeed += force.x;
        _vSpeed += force.y;
    }

    public void ApplyForce(Vec2 force, Vec2 limits)
    {
        limits = new Vec2(Math.Abs(limits.x), Math.Abs(limits.y));
        if ((force.x < 0f && _hSpeed > 0f - limits.x) || (force.x > 0f && _hSpeed < limits.x))
        {
            _hSpeed += force.x;
        }
        if ((force.y < 0f && _vSpeed > 0f - limits.y) || (force.y > 0f && _vSpeed < limits.y))
        {
            _vSpeed += force.y;
        }
    }

    public void ApplyForceLimited(Vec2 force)
    {
        _hSpeed += force.x;
        if ((force.x < 0f && _hSpeed < force.x) || (force.x > 0f && _hSpeed > force.x))
        {
            _hSpeed = force.x;
        }
        _vSpeed += force.y;
        if ((force.y < 0f && _vSpeed < force.y) || (force.y > 0f && _vSpeed > force.y))
        {
            _vSpeed = force.y;
        }
    }

    public virtual bool ShouldUpdate()
    {
        return true;
    }

    public List<Type> GetAllTypes()
    {
        return Editor.AllBaseTypes[GetType()];
    }

    public List<Type> GetAllTypesFiltered(Type stopAt)
    {
        return GetAllTypes(GetType(), stopAt);
    }

    public static List<Type> GetAllTypes(Type t, Type stopAt = null)
    {
        List<Type> types = new List<Type>(t.GetInterfaces());
        types.Add(t);
        Type baseType = t.BaseType;
        while (baseType != null && !(baseType == typeof(Thing)) && !(baseType == typeof(object)) && !(baseType == stopAt))
        {
            types.Add(baseType);
            baseType = baseType.BaseType;
        }
        return types;
    }

    public void SetEditorName(string s)
    {
        _editorName = s;
    }

    public virtual void TabRotate()
    {
        if (_canFlip)
        {
            flipHorizontal = !flipHorizontal;
        }
    }

    public static Thing Instantiate(Type t)
    {
        return Editor.CreateThing(t);
    }

    public static bool CheckForBozoData(Thing pThing)
    {
        if (pThing == null || Math.Abs(pThing.y) > 99999f || Math.Abs(pThing.x) > 99999f)
        {
            return true;
        }
        if (pThing is ThingContainer)
        {
            (pThing as ThingContainer).bozocheck = true;
            return false;
        }
        return false;
    }

    public virtual void PrepareForHost()
    {
        GhostManager.context.MakeGhost(this, -1, initLevel: true);
        ghostType = Editor.IDToType[GetType()];
        DuckNetwork.AssignToHost(this);
    }

    public static Thing LoadThing(BinaryClassChunk node, bool chance = true)
    {
        Type t = Editor.GetType(node.GetProperty<string>("type"));
        if (t != null)
        {
            Thing newThing = Editor.CreateThing(t);
            if (!newThing.Deserialize(node))
            {
                return null;
            }
            if (Level.current is Editor || !chance || newThing.likelyhoodToExist == 1f || Level.PassedChanceGroup(newThing.chanceGroup, newThing.likelyhoodToExist))
            {
                if (newThing is IContainPossibleThings)
                {
                    (newThing as IContainPossibleThings).PreparePossibilities();
                }
                return newThing;
            }
            return null;
        }
        return null;
    }

    public virtual BinaryClassChunk Serialize()
    {
        BinaryClassChunk element = new BinaryClassChunk();
        Type t = GetType();
        element.AddProperty("type", ModLoader.SmallTypeName(t));
        element.AddProperty("x", base.x);
        element.AddProperty("y", base.y);
        element.AddProperty("chance", _likelyhoodToExist);
        element.AddProperty("accessible", _isAccessible);
        element.AddProperty("chanceGroup", _chanceGroup);
        element.AddProperty("flipHorizontal", _flipHorizontal);
        if (_canFlipVert)
        {
            element.AddProperty("flipVertical", flipVertical);
        }
        if (sequence != null)
        {
            element.AddProperty("loop", sequence.loop);
            element.AddProperty("popUpOrder", sequence.order);
            element.AddProperty("waitTillOrder", sequence.waitTillOrder);
        }
        foreach (FieldInfo info in Editor.EditorFieldsForType[t])
        {
            object field = info.GetValue(this);
            field.GetType().GetProperty("info").GetValue(field, null);
            object val = field.GetType().GetProperty("value").GetValue(field, null);
            element.AddProperty(info.Name, val);
        }
        return element;
    }

    public virtual bool Deserialize(BinaryClassChunk node)
    {
        base.x = node.GetPrimitive<float>("x");
        base.y = node.GetPrimitive<float>("y");
        _likelyhoodToExist = node.GetPrimitive<float>("chance");
        _isAccessible = node.GetPrimitive<bool>("accessible");
        chanceGroup = node.GetPrimitive<int>("chanceGroup");
        flipHorizontal = node.GetPrimitive<bool>("flipHorizontal");
        if (_canFlipVert)
        {
            flipVertical = node.GetPrimitive<bool>("flipVertical");
        }
        if (sequence != null)
        {
            sequence.loop = node.GetPrimitive<bool>("loop");
            sequence.order = node.GetPrimitive<int>("popUpOrder");
            sequence.waitTillOrder = node.GetPrimitive<bool>("waitTillOrder");
        }
        Type tp = GetType();
        foreach (FieldInfo info in Editor.EditorFieldsForType[tp])
        {
            if (node.HasProperty(info.Name))
            {
                _ = info.FieldType.GetGenericArguments()[0];
                object field = info.GetValue(this);
                field.GetType().GetProperty("value").SetValue(field, node.GetProperty(info.Name), null);
            }
        }
        return true;
    }

    public static Thing LegacyLoadThing(DXMLNode node, bool chance = true)
    {
        Type t = Editor.GetType(node.Element("type").Value);
        if (t != null)
        {
            Thing newThing = Editor.CreateThing(t);
            if (!newThing.LegacyDeserialize(node))
            {
                newThing = null;
            }
            if (Level.current is Editor || !chance || newThing.likelyhoodToExist == 1f || Level.PassedChanceGroup(newThing.chanceGroup, newThing.likelyhoodToExist))
            {
                return newThing;
            }
            return null;
        }
        return null;
    }

    public virtual DXMLNode LegacySerialize()
    {
        DXMLNode element = new DXMLNode("Object");
        Type t = GetType();
        element.Add(new DXMLNode("type", t.AssemblyQualifiedName));
        element.Add(new DXMLNode("x", base.x));
        element.Add(new DXMLNode("y", base.y));
        element.Add(new DXMLNode("chance", _likelyhoodToExist));
        element.Add(new DXMLNode("accessible", _isAccessible));
        element.Add(new DXMLNode("chanceGroup", _chanceGroup));
        element.Add(new DXMLNode("flipHorizontal", _flipHorizontal));
        if (_canFlipVert)
        {
            element.Add(new DXMLNode("flipVertical", _flipVertical));
        }
        if (sequence != null)
        {
            element.Add(new DXMLNode("loop", sequence.loop));
            element.Add(new DXMLNode("popUpOrder", sequence.order));
            element.Add(new DXMLNode("waitTillOrder", sequence.waitTillOrder));
        }
        foreach (Type tp in Editor.AllBaseTypes[t])
        {
            if (tp.IsInterface)
            {
                continue;
            }
            foreach (FieldInfo info in Editor.AllEditorFields[tp])
            {
                object field = info.GetValue(this);
                object val = field.GetType().GetProperty("value").GetValue(field, null);
                element.Add(new DXMLNode(info.Name, val));
            }
        }
        return element;
    }

    public virtual bool LegacyDeserialize(DXMLNode node)
    {
        DXMLNode xpos = node.Element("x");
        if (xpos != null)
        {
            base.x = Change.ToSingle(xpos.Value);
        }
        DXMLNode ypos = node.Element("y");
        if (ypos != null)
        {
            base.y = Change.ToSingle(ypos.Value);
        }
        DXMLNode likely = node.Element("chance");
        if (likely != null)
        {
            _likelyhoodToExist = Change.ToSingle(likely.Value);
        }
        DXMLNode access = node.Element("accessible");
        if (access != null)
        {
            _isAccessible = Change.ToBoolean(access.Value);
        }
        DXMLNode chanceG = node.Element("chanceGroup");
        if (chanceG != null)
        {
            chanceGroup = Convert.ToInt32(chanceG.Value);
        }
        DXMLNode flipper = node.Element("flipHorizontal");
        if (flipper != null)
        {
            flipHorizontal = Convert.ToBoolean(flipper.Value);
        }
        if (_canFlipVert)
        {
            DXMLNode flipper2 = node.Element("flipVertical");
            if (flipper2 != null)
            {
                flipVertical = Convert.ToBoolean(flipper2.Value);
            }
        }
        if (sequence != null)
        {
            DXMLNode val = node.Element("loop");
            if (val != null)
            {
                sequence.loop = Convert.ToBoolean(val.Value);
            }
            val = node.Element("popUpOrder");
            if (val != null)
            {
                sequence.order = Convert.ToInt32(val.Value);
            }
            val = node.Element("waitTillOrder");
            if (val != null)
            {
                sequence.waitTillOrder = Convert.ToBoolean(val.Value);
            }
        }
        foreach (Type tp in Editor.AllBaseTypes[GetType()])
        {
            if (tp.IsInterface)
            {
                continue;
            }
            foreach (FieldInfo info in Editor.AllEditorFields[tp])
            {
                DXMLNode val2 = node.Element(info.Name);
                if (val2 == null)
                {
                    continue;
                }
                Type t = info.FieldType.GetGenericArguments()[0];
                object field = info.GetValue(this);
                PropertyInfo valueField = field.GetType().GetProperty("value");
                if (t == typeof(int))
                {
                    valueField.SetValue(field, Convert.ToInt32(val2.Value), null);
                }
                else if (t == typeof(float))
                {
                    valueField.SetValue(field, Convert.ToSingle(val2.Value), null);
                }
                else if (t == typeof(string))
                {
                    EditorPropertyInfo obj = field.GetType().GetProperty("info").GetValue(field, null) as EditorPropertyInfo;
                    object levVal = val2.Value;
                    if (obj.isLevel)
                    {
                        LevelData dat = DuckFile.LoadLevel(Content.path + "levels/" + levVal?.ToString() + ".lev");
                        if (dat != null)
                        {
                            levVal = dat.metaData.guid;
                        }
                    }
                    valueField.SetValue(field, levVal, null);
                }
                else if (t == typeof(bool))
                {
                    valueField.SetValue(field, Convert.ToBoolean(val2.Value), null);
                }
                else if (t == typeof(byte))
                {
                    valueField.SetValue(field, Convert.ToByte(val2.Value), null);
                }
                else if (t == typeof(short))
                {
                    valueField.SetValue(field, Convert.ToInt16(val2.Value), null);
                }
                else if (t == typeof(long))
                {
                    valueField.SetValue(field, Convert.ToInt64(val2.Value), null);
                }
            }
        }
        return true;
    }

    public virtual void EditorPropertyChanged(object property)
    {
    }

    public virtual void EditorObjectsChanged()
    {
    }

    public virtual void EditorAdded()
    {
    }

    public virtual void EditorRemoved()
    {
    }

    public virtual void EditorFlip(bool pVertical)
    {
    }

    public virtual ContextMenu GetContextMenu()
    {
        EditorGroupMenu menu = new EditorGroupMenu(null, root: true);
        if (_canFlip)
        {
            menu.AddItem(new ContextCheckBox("Flip", null, new FieldBinding(this, "flipHorizontal")));
        }
        if (_canFlipVert)
        {
            menu.AddItem(new ContextCheckBox("Flip V", null, new FieldBinding(this, "flipVertical")));
        }
        if (_canHaveChance)
        {
            EditorGroupMenu contain = new EditorGroupMenu(menu);
            contain.text = "@CHANCEICON@Chance";
            contain.tooltip = "Likelyhood for this object to exist in the level.";
            menu.AddItem(contain);
            contain.AddItem(new ContextSlider("Chance", null, new FieldBinding(this, "likelyhoodToExist"), 0.05f, null, time: false, null, "Chance for object to exist. 1.0 = 100% chance."));
            contain.AddItem(new ContextSlider("Chance Group", null, new FieldBinding(this, "chanceGroup", -1f, 10f), 1f, null, time: false, null, "All objects in a chance group will exist, if their group's chance roll is met. -1 means no grouping."));
            contain.AddItem(new ContextCheckBox("Accessible", null, new FieldBinding(this, "isAccessible"), null, "Flag for level generation, set this to false if the object is behind a locked door and not neccesarily accessible."));
        }
        if (sequence != null && !_contextMenuFilter.Contains("Sequence"))
        {
            EditorGroupMenu contain2 = new EditorGroupMenu(menu);
            contain2.text = "Sequence";
            menu.AddItem(contain2);
            if (!_contextMenuFilter.Contains("Sequence|Loop"))
            {
                contain2.AddItem(new ContextCheckBox("Loop", null, new FieldBinding(sequence, "loop")));
            }
            if (!_contextMenuFilter.Contains("Sequence|Order"))
            {
                contain2.AddItem(new ContextSlider("Order", null, new FieldBinding(sequence, "order", 0f, 100f), 1f, "RAND"));
            }
            if (!_contextMenuFilter.Contains("Sequence|Wait"))
            {
                contain2.AddItem(new ContextCheckBox("Wait", null, new FieldBinding(sequence, "waitTillOrder")));
            }
        }
        List<string> alreadyAdded = new List<string>();
        foreach (Type tp in Editor.AllBaseTypes[GetType()])
        {
            if (tp.IsInterface)
            {
                continue;
            }
            foreach (FieldInfo info in Editor.AllEditorFields[tp])
            {
                if (alreadyAdded.Contains(info.Name) || _contextMenuFilter.Contains(info.Name))
                {
                    continue;
                }
                string processedName = info.Name.Replace("_", " ");
                object fieldObject = info.GetValue(this);
                EditorPropertyInfo field = fieldObject.GetType().GetProperty("info").GetValue(fieldObject, null) as EditorPropertyInfo;
                if (field.name != null)
                {
                    processedName = field.name;
                }
                if (field.value.GetType() == typeof(int) || field.value.GetType() == typeof(float) || field.value.GetType().IsEnum)
                {
                    menu.AddItem(new ContextSlider(processedName, null, new FieldBinding(fieldObject, "value", field.min, field.max), field.increment, field.minSpecial, field.isTime, null, field.tooltip));
                }
                else if (field.value.GetType() == typeof(bool))
                {
                    menu.AddItem(new ContextCheckBox(processedName, null, new FieldBinding(fieldObject, "value"), null, field.tooltip));
                }
                else if (field.value.GetType() == typeof(string))
                {
                    if (field.isLevel)
                    {
                        menu.AddItem(new ContextFile(processedName, null, new FieldBinding(fieldObject, "value"), ContextFileType.Level, field.tooltip));
                    }
                    else
                    {
                        menu.AddItem(new ContextTextbox(processedName, null, new FieldBinding(fieldObject, "value"), field.tooltip));
                    }
                }
                alreadyAdded.Add(info.Name);
            }
        }
        return menu;
    }

    public virtual void DrawHoverInfo()
    {
    }

    public Sprite GetEditorImage(int wide = 16, int high = 16, bool transparentBack = false, Effect effect = null, RenderTarget2D target = null)
    {
        return GetEditorImage(wide, high, transparentBack, effect, target, pUseCollisionSize: false);
    }

    public Sprite GetEditorImage(int wide, int high, bool transparentBack, Effect effect, RenderTarget2D target, bool pUseCollisionSize)
    {
        Sprite tex = null;
        if (_editorIcons.TryGetValue(GetType(), out tex))
        {
            return tex;
        }
        if (Thread.CurrentThread != MonoMain.mainThread)
        {
            return new Sprite("basketBall");
        }
        if (_alphaTestEffect == null)
        {
            _alphaTestEffect = Content.Load<MTEffect>("Shaders/alphatest");
        }
        if (pUseCollisionSize && collisionSize.x > 0f)
        {
            if (wide <= 0)
            {
                wide = (int)collisionSize.x;
            }
            if (high <= 0)
            {
                high = (int)collisionSize.y;
            }
        }
        else if (graphic != null)
        {
            if (wide <= 0)
            {
                wide = graphic.w;
            }
            if (high <= 0)
            {
                high = graphic.h;
            }
        }
        int scalar = ((wide > high) ? wide : high);
        if (target == null)
        {
            target = new RenderTarget2D(wide, high, pdepth: true);
        }
        if (graphic == null)
        {
            return new Sprite(target);
        }
        float s = (float)scalar / ((collisionSize.x > 0f && pUseCollisionSize) ? collisionSize.x : ((float)graphic.width));
        Camera cam = new Camera(0f, 0f, wide, high);
        cam.position = new Vec2(base.x - base.centerx * s, base.y - base.centery * s);
        if (pUseCollisionSize && collisionSize.x > 0f)
        {
            cam.center = new Vec2((int)((left + right) / 2f), (int)((top + bottom) / 2f));
        }
        RenderTarget2D curTarg = Graphics.currentRenderTarget;
        Graphics.SetRenderTarget(target);
        DepthStencilState state = new DepthStencilState
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 1,
            DepthBufferEnable = false
        };
        Graphics.Clear(transparentBack ? new Color(0, 0, 0, 0) : new Color(15, 4, 16));
        Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, state, RasterizerState.CullNone, (effect == null) ? _alphaTestEffect : effect, cam.getMatrix());
        Draw();
        Graphics.screen.End();
        if (curTarg == null || curTarg.IsDisposed)
        {
            Graphics.SetRenderTarget(null);
        }
        else
        {
            Graphics.SetRenderTarget(curTarg);
        }
        Texture2D texture2D = new Texture2D(Graphics.device, target.width, target.height);
        texture2D.SetData(target.GetData());
        Sprite spr = new Sprite(texture2D);
        _editorIcons[GetType()] = spr;
        return spr;
    }

    public virtual Sprite GeneratePreview(int wide = 16, int high = 16, bool transparentBack = false, Effect effect = null, RenderTarget2D target = null)
    {
        bool editorPreview = wide == 16 && high == 16 && transparentBack && effect == null && target == null;
        if (editorPreview && _editorIcon != null)
        {
            return _editorIcon;
        }
        if (Thread.CurrentThread != MonoMain.mainThread)
        {
            return new Sprite("basketBall");
        }
        if (_alphaTestEffect == null)
        {
            _alphaTestEffect = Content.Load<MTEffect>("Shaders/alphatest");
        }
        if (graphic != null)
        {
            if (wide <= 0)
            {
                wide = graphic.w;
            }
            if (high <= 0)
            {
                high = graphic.h;
            }
        }
        if (target == null)
        {
            target = new RenderTarget2D(wide, high, pdepth: true);
        }
        if (graphic == null)
        {
            return new Sprite(target);
        }
        Camera cam = new Camera(0f, 0f, wide, high);
        cam.position = new Vec2(base.x - (float)(wide / 2), base.y - (float)(high / 2));
        Graphics.SetRenderTarget(target);
        DepthStencilState state = new DepthStencilState
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 1,
            DepthBufferEnable = false
        };
        Graphics.Clear(transparentBack ? new Color(0, 0, 0, 0) : new Color(30, 30, 30));
        Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, state, RasterizerState.CullNone, (effect == null) ? _alphaTestEffect : effect, cam.getMatrix());
        Draw();
        Graphics.screen.End();
        Graphics.SetRenderTarget(null);
        Texture2D texture2D = new Texture2D(Graphics.device, target.width, target.height);
        texture2D.SetData(target.GetData());
        Sprite spr = new Sprite(texture2D);
        if (editorPreview)
        {
            _editorIcon = spr;
        }
        return spr;
    }

    public void FixClipping()
    {
        foreach (Block item in Level.CheckRectAll<Block>(topLeft, bottomRight))
        {
            _ = item;
        }
    }

    private string GetPropertyDetails()
    {
        string details = "";
        foreach (FieldInfo info in Editor.AllEditorFields[GetType()])
        {
            object fieldObject = info.GetValue(this);
            EditorPropertyInfo field = fieldObject.GetType().GetProperty("info").GetValue(fieldObject, null) as EditorPropertyInfo;
            if (field.value.GetType() == typeof(int) || field.value.GetType() == typeof(float))
            {
                details = details + info.Name + ": " + Convert.ToString(field.value) + "\n";
            }
        }
        return details;
    }

    public virtual string GetDetailsString()
    {
        if (_likelyhoodToExist == 1f && _chanceGroup == -1)
        {
            return GetPropertyDetails();
        }
        return "Chance: " + Math.Round(likelyhoodToExist / 1f * 100f) + "%\nChance Group: " + ((_chanceGroup == -1) ? "None" : _chanceGroup.ToString(CultureInfo.InvariantCulture)) + "\n" + GetPropertyDetails();
    }

    public virtual void ReturnItemToWorld(Thing t)
    {
        Block rightWall = Level.CheckLine<Block>(position, position + new Vec2(16f, 0f));
        if (rightWall != null && rightWall.solid && t.right > rightWall.left)
        {
            t.right = rightWall.left;
        }
        Block leftWall = Level.CheckLine<Block>(position, position - new Vec2(16f, 0f));
        if (leftWall != null && leftWall.solid && t.left < leftWall.right)
        {
            t.left = leftWall.right;
        }
        Block topWall = Level.CheckLine<Block>(position, position + new Vec2(0f, -16f));
        if (topWall != null && topWall.solid && t.top < topWall.bottom)
        {
            t.top = topWall.bottom;
        }
        Block bottomWall = Level.CheckLine<Block>(position, position + new Vec2(0f, 16f));
        if (bottomWall != null && bottomWall.solid && t.bottom > bottomWall.top)
        {
            t.bottom = bottomWall.top;
        }
    }

    public Vec2 NearestCorner(Vec2 to)
    {
        Vec2 nearest = topLeft;
        float dist = (topLeft - to).length;
        float test = (topRight - to).length;
        if (test < dist)
        {
            nearest = topRight;
            dist = test;
        }
        test = (bottomLeft - to).length;
        if (test < dist)
        {
            nearest = bottomLeft;
            dist = test;
        }
        test = (bottomRight - to).length;
        if (test < dist)
        {
            nearest = bottomRight;
            dist = test;
        }
        return nearest;
    }

    public Vec2 NearestOpenCorner(Vec2 to)
    {
        Vec2 nearest = Vec2.Zero;
        float dist = 9999999f;
        float test = 0f;
        test = (topLeft - to).length;
        if (test < dist && Level.CheckCircle<Block>(topLeft, 2f, this) == null)
        {
            nearest = topLeft;
            dist = test;
        }
        test = (topRight - to).length;
        if (test < dist && Level.CheckCircle<Block>(topRight, 2f, this) == null)
        {
            nearest = topRight;
            dist = test;
        }
        test = (bottomLeft - to).length;
        if (test < dist && Level.CheckCircle<Block>(bottomLeft, 2f, this) == null)
        {
            nearest = bottomLeft;
            dist = test;
        }
        test = (bottomRight - to).length;
        if (test < dist && Level.CheckCircle<Block>(bottomRight, 2f, this) == null)
        {
            nearest = bottomRight;
            dist = test;
        }
        return nearest;
    }

    public Thing(float xval = 0f, float yval = 0f, Sprite sprite = null)
    {
        base.x = xval;
        base.y = yval;
        graphic = sprite;
        if (sprite != null)
        {
            _collisionSize = new Vec2(sprite.w, sprite.h);
        }
        if (Network.isActive)
        {
            connection = DuckNetwork.localConnection;
        }
    }

    public virtual Vec2 OffsetLocal(Vec2 pos)
    {
        Vec2 offset = pos * base.scale;
        if (offDir < 0)
        {
            offset.x *= -1f;
        }
        return offset.Rotate(angle, new Vec2(0f, 0f));
    }

    public virtual Vec2 ReverseOffsetLocal(Vec2 pos)
    {
        return (pos * base.scale).Rotate(0f - angle, new Vec2(0f, 0f));
    }

    public virtual Vec2 Offset(Vec2 pos)
    {
        return position + OffsetLocal(pos);
    }

    public virtual Vec2 ReverseOffset(Vec2 pos)
    {
        pos -= position;
        return ReverseOffsetLocal(pos);
    }

    public virtual float OffsetX(float pos)
    {
        Vec2 offset = new Vec2(pos, 0f);
        if (offDir < 0)
        {
            offset.x *= -1f;
        }
        offset = offset.Rotate(angle, new Vec2(0f, 0f));
        return (position + offset).x;
    }

    public virtual float OffsetY(float pos)
    {
        Vec2 offset = new Vec2(0f, pos);
        if (offDir < 0)
        {
            offset.x *= -1f;
        }
        offset = offset.Rotate(angle, new Vec2(0f, 0f));
        return (position + offset).y;
    }

    public virtual void ResetProperties()
    {
        _level = null;
        _removeFromLevel = false;
        _initialized = false;
    }

    public void AddToLayer()
    {
        if (_layer == null)
        {
            _layer = Layer.Game;
        }
        if (!skipLayerAdding)
        {
            _layer.Add(this);
        }
    }

    public void DoNetworkInitialize()
    {
        if (_networkInitialized)
        {
            return;
        }
        if (isStateObject)
        {
            _ghostType = Editor.IDToType[GetType()];
            if (Network.isServer)
            {
                connection = DuckNetwork.localConnection;
            }
        }
        _networkInitialized = true;
    }

    public virtual void DoInitialize()
    {
        if (_redoLayer)
        {
            AddToLayer();
            _redoLayer = false;
        }
        if (!_initialized)
        {
            if (Network.isActive)
            {
                DoNetworkInitialize();
            }
            _networkDrawIndex = NetworkDebugger.currentIndex;
            Initialize();
            _initialized = true;
        }
    }

    public virtual void Initialize()
    {
    }

    public virtual void DoUpdate()
    {
        if (wasSuperFondled > 0)
        {
            wasSuperFondled--;
        }
        if (_anchor != null)
        {
            position = _anchor.position;
        }
        Update();
        _topQuick = top;
        _bottomQuick = bottom;
        _leftQuick = left;
        _rightQuick = right;
    }

    public virtual void Update()
    {
    }

    public virtual void InactiveUpdate()
    {
    }

    public virtual void DoEditorUpdate()
    {
        _topQuick = top;
        _bottomQuick = bottom;
        _leftQuick = left;
        _rightQuick = right;
        EditorUpdate();
    }

    public virtual void EditorUpdate()
    {
    }

    public virtual void DoEditorRender()
    {
        EditorRender();
    }

    public virtual void EditorRender()
    {
    }

    public virtual void OnEditorLoaded()
    {
    }

    public void Glitch()
    {
        if (material is MaterialGlitch)
        {
            (material as MaterialGlitch).yoffset = Rando.Float(1f);
            (material as MaterialGlitch).amount = Rando.Float(0.9f, 1.2f);
        }
    }

    public ProfileNetData GetOrCreateNetData()
    {
        if (_netData == null)
        {
            _netData = new ProfileNetData();
        }
        return _netData;
    }

    public void NetworkSet<T>(string pVariable, T pValue)
    {
        if (isServerForObject)
        {
            if (_netData == null)
            {
                _netData = new ProfileNetData();
            }
            _netData.Set(pVariable, pValue);
        }
    }

    public T NetworkGet<T>(string pVariable, T pDefault = default(T))
    {
        if (_netData == null)
        {
            return default(T);
        }
        return _netData.Get(pVariable, pDefault);
    }

    public virtual void OnGhostObjectAdded()
    {
    }

    public virtual void DoDraw()
    {
        if (NetworkDebugger.currentIndex < 0 || NetworkDebugger.currentIndex == _networkDrawIndex)
        {
            Graphics.material = _material;
            if (_material != null)
            {
                _material.Update();
            }
            Draw();
            Graphics.material = null;
        }
    }

    public virtual void Draw()
    {
        if (_graphic != null)
        {
            if (!_skipPositioning)
            {
                _graphic.position = position;
                _graphic.alpha = base.alpha;
                _graphic.angle = angle;
                _graphic.depth = base.depth;
                _graphic.scale = base.scale;
                _graphic.center = center;
            }
            _graphic.Draw();
        }
    }

    public void DrawCollision()
    {
        Graphics.DrawRect(topLeft, bottomRight, Color.Orange * 0.8f, 1f, filled: false, 0.5f);
        if (this is PhysicsObject)
        {
            _ = (this as PhysicsObject).sleeping;
        }
    }

    public void Draw(Sprite spr, float xpos, float ypos, int d = 1)
    {
        Draw(spr, new Vec2(xpos, ypos), d);
    }

    public void Draw(Sprite spr, Vec2 pos, int d = 1)
    {
        Vec2 drawOffset = Offset(pos);
        if (graphic != null)
        {
            spr.flipH = graphic.flipH;
        }
        spr.angle = angle;
        spr.alpha = base.alpha;
        spr.depth = base.depth + d;
        spr.scale = base.scale;
        spr.flipH = offDir < 0;
        Graphics.Draw(spr, drawOffset.x, drawOffset.y);
    }

    public void DrawIgnoreAngle(Sprite spr, Vec2 pos, int d = 1)
    {
        Vec2 drawOffset = Offset(pos);
        spr.alpha = base.alpha;
        spr.depth = base.depth + d;
        spr.scale = base.scale;
        Graphics.Draw(spr, drawOffset.x, drawOffset.y);
    }

    public virtual void OnTeleport()
    {
    }

    public virtual void DoTerminate()
    {
        Terminate();
    }

    public virtual void Terminate()
    {
    }

    public virtual void Added(Level parent)
    {
        _removeFromLevel = false;
        _redoLayer = true;
        _level = parent;
        DoInitialize();
    }

    public virtual void Added(Level parent, bool redoLayer, bool reinit)
    {
        if (reinit)
        {
            _initialized = false;
        }
        _removeFromLevel = false;
        _redoLayer = redoLayer;
        _level = parent;
        DoInitialize();
    }

    public virtual void Removed()
    {
        _removeFromLevel = true;
        if (_layer != null)
        {
            _layer.RemoveSoon(this);
        }
    }

    public virtual void NetworkUpdate()
    {
    }

    public virtual void OnSequenceActivate()
    {
    }
}
