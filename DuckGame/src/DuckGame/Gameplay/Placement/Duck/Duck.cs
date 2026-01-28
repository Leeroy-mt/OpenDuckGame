using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class Duck : PhysicsObject, ITakeInput, IAmADuck, IDrawToDifferentLayers
{
    [Flags]
    private enum ConnectionTrouble
    {
        Lag = 1,
        Loss = 2,
        AFK = 4,
        Chatting = 8,
        Disconnection = 0x10,
        Spectator = 0x20,
        Minimized = 0x40,
        Paused = 0x80,
        DevConsole = 0x100
    }

    private class ConnectionIndicators
    {
        private class Indicator
        {
            public ConnectionTrouble problem;

            public SpriteMap sprite;

            public float bloop;

            public ConnectionIndicators owner;

            public float wait;

            public float activeLerp;

            public bool _prevActive;

            private Vec2 drawPos = Vec2.Zero;

            public bool noWait
            {
                get
                {
                    if (problem != ConnectionTrouble.Chatting && problem != ConnectionTrouble.AFK)
                    {
                        return problem == ConnectionTrouble.Minimized;
                    }
                    return true;
                }
            }

            public bool active
            {
                get
                {
                    if (owner.duck.connection == null || owner.duck.profile == null)
                    {
                        return false;
                    }
                    if (problem == ConnectionTrouble.Chatting)
                    {
                        return owner.duck.chatting;
                    }
                    if (problem == ConnectionTrouble.AFK)
                    {
                        return owner.duck.afk;
                    }
                    if (problem == ConnectionTrouble.Disconnection)
                    {
                        if (owner.duck.connection != DuckNetwork.localConnection)
                        {
                            return owner.duck.connection.isExperiencingConnectionTrouble;
                        }
                        return false;
                    }
                    if (problem == ConnectionTrouble.Lag)
                    {
                        if (owner.duck.connection != DuckNetwork.localConnection)
                        {
                            return owner.duck.connection.manager.ping > 0.25f;
                        }
                        return false;
                    }
                    if (problem == ConnectionTrouble.Loss)
                    {
                        if (owner.duck.connection != DuckNetwork.localConnection)
                        {
                            return owner.duck.connection.manager.accumulatedLoss > 10;
                        }
                        return false;
                    }
                    if (problem == ConnectionTrouble.Minimized)
                    {
                        return !owner.duck.profile.netData.Get("gameInFocus", pDefault: true);
                    }
                    if (problem == ConnectionTrouble.Paused)
                    {
                        return owner.duck.profile.netData.Get("gamePaused", pDefault: false);
                    }
                    if (problem == ConnectionTrouble.DevConsole)
                    {
                        return owner.duck.profile.netData.Get("consoleOpen", pDefault: false);
                    }
                    return false;
                }
            }

            public bool visible => activeLerp > 0f;

            public void Update()
            {
                bool a = active;
                if (a != _prevActive)
                {
                    _prevActive = a;
                    if (a)
                    {
                        bloop = 1f;
                    }
                    if (problem == ConnectionTrouble.Chatting || problem == ConnectionTrouble.Minimized || problem == ConnectionTrouble.Paused || problem == ConnectionTrouble.DevConsole || problem == ConnectionTrouble.AFK)
                    {
                        SFX.Play("rainpop", 0.65f, Rando.Float(-0.1f, 0.1f));
                    }
                }
                if (!a)
                {
                    wait = Lerp.Float(wait, 0f, 0.03f);
                    if (noWait)
                    {
                        wait = 0f;
                    }
                    if (wait <= 0f)
                    {
                        if (sprite.currentAnimation != "pop")
                        {
                            sprite.SetAnimation("pop");
                        }
                        else if (sprite.finished)
                        {
                            sprite.SetAnimation("idle");
                            activeLerp = 0f;
                        }
                    }
                }
                else
                {
                    sprite.SetAnimation("idle");
                    wait = 1f;
                    activeLerp = 1f;
                }
                bloop = Lerp.FloatSmooth(bloop, 0f, 0.21f);
                if (bloop < 0.1f)
                {
                    bloop = 0f;
                }
            }

            public void Draw(Vec2 pPos, Vec2 pOffset)
            {
                if ((drawPos - pOffset).Length() > 16f)
                {
                    drawPos = pOffset;
                }
                drawPos = Lerp.Vec2Smooth(drawPos, pOffset, 0.4f);
                sprite.Scale = new Vec2(1f + bloop * 0.6f, 1f + bloop * 0.35f);
                sprite.Depth = 0.9f;
                Graphics.Draw(sprite, pPos.X + drawPos.X, pPos.Y + drawPos.Y);
            }

            public Indicator(SpriteMap pSprite)
            {
                sprite = pSprite;
                sprite.AddAnimation("idle", 1f, true, sprite.frame);
                sprite.AddAnimation("pop", 0.4f, false, 5, 6);
            }
        }

        public Duck duck;

        private List<Indicator> _indicators = new List<Indicator>
        {
            new Indicator(new SpriteMap("lagturtle", 16, 16, 3)
            {
                Center = new Vec2(8f)
            })
            {
                problem = ConnectionTrouble.AFK
            },
            new Indicator(new SpriteMap("lagturtle", 16, 16, 4)
            {
                Center = new Vec2(8f)
            })
            {
                problem = ConnectionTrouble.Chatting
            },
            new Indicator(new SpriteMap("lagturtle", 16, 16, 2)
            {
                Center = new Vec2(8f)
            })
            {
                problem = ConnectionTrouble.Disconnection
            },
            new Indicator(new SpriteMap("lagturtle", 16, 16, 0)
            {
                Center = new Vec2(8f)
            })
            {
                problem = ConnectionTrouble.Lag
            },
            new Indicator(new SpriteMap("lagturtle", 16, 16, 1)
            {
                Center = new Vec2(8f)
            })
            {
                problem = ConnectionTrouble.Loss
            },
            new Indicator(new SpriteMap("lagturtle", 16, 16, 8)
            {
                Center = new Vec2(8f)
            })
            {
                problem = ConnectionTrouble.Minimized
            },
            new Indicator(new SpriteMap("lagturtle", 16, 16, 7)
            {
                Center = new Vec2(8f)
            })
            {
                problem = ConnectionTrouble.Spectator
            },
            new Indicator(new SpriteMap("lagturtle", 16, 16, 9)
            {
                Center = new Vec2(8f)
            })
            {
                problem = ConnectionTrouble.Paused
            },
            new Indicator(new SpriteMap("lagturtle", 16, 16, 10)
            {
                Center = new Vec2(8f)
            })
            {
                problem = ConnectionTrouble.DevConsole
            }
        };

        private int numProblems;

        private static Sprite _rainbowGradient;

        public ConnectionIndicators()
        {
            foreach (Indicator indicator in _indicators)
            {
                indicator.owner = this;
            }
        }

        public void Update()
        {
            numProblems = 0;
            foreach (Indicator indicator in _indicators)
            {
                indicator.Update();
                if (indicator.visible)
                {
                    numProblems++;
                }
            }
        }

        public void Draw()
        {
            if (duck.Position.X < -4000f)
            {
                return;
            }
            if (_rainbowGradient == null)
            {
                _rainbowGradient = new Sprite("rainbowGradient");
            }
            if (numProblems <= 0)
            {
                return;
            }
            float degreesPerElement = 37f;
            float totalDegrees = (float)(numProblems - 1) * degreesPerElement;
            Vec2 position = new Vec2(-1000f, -1000f);
            position = duck.cameraPosition + new Vec2(0f, 6f);
            float rainbowSize = (float)numProblems / 5f;
            int index = 0;
            float floatDistance = -20f;
            bool hasPrevPos = false;
            Vec2 prevPos = Vec2.Zero;
            foreach (Indicator t in _indicators)
            {
                if (t.visible)
                {
                    float deg = (0f - totalDegrees) / 2f + (float)index * degreesPerElement;
                    float xpos = 0f - (float)Math.Sin(Maths.DegToRad(deg)) * floatDistance;
                    float ypos = (float)Math.Cos(Maths.DegToRad(deg)) * floatDistance;
                    Vec2 cur = new Vec2(xpos, ypos);
                    t.Draw(position, cur);
                    if (hasPrevPos)
                    {
                        cur = position + cur;
                        Vec2 moveVector = (cur - prevPos).Normalized;
                        Graphics.DrawTexturedLine(_rainbowGradient.texture, prevPos - moveVector, cur + moveVector, Color.White * rainbowSize, 0.3f + 0.6f * rainbowSize, 0.85f);
                    }
                    hasPrevPos = true;
                    prevPos = new Vec2(position.X + xpos, position.Y + ypos);
                    index++;
                }
            }
        }
    }

    public StateBinding _profileIndexBinding = new StateBinding(GhostPriority.Normal, nameof(netProfileIndex), 4);

    public StateBinding _disarmIndexBinding = new StateBinding(GhostPriority.Normal, nameof(disarmIndex), 4);

    public StateBinding _animationIndexBinding = new StateBinding(GhostPriority.Normal, nameof(netAnimationIndex), 4);

    public StateBinding _holdObjectBinding = new StateBinding(GhostPriority.High, nameof(holdObject));

    public StateBinding _ragdollBinding = new StateBinding(GhostPriority.High, nameof(ragdoll));

    public StateBinding _cookedBinding = new StateBinding(GhostPriority.High, nameof(_cooked));

    public StateBinding _quackBinding = new StateBinding(GhostPriority.High, nameof(quack), 5);

    public StateBinding _quackPitchBinding = new StateBinding(GhostPriority.High, nameof(quackPitch));

    public StateBinding _toungeBinding = new CompressedVec2Binding(GhostPriority.Normal, nameof(tounge), 1);

    public StateBinding _cameraPositionOverrideBinding = new CompressedVec2Binding(GhostPriority.Normal, nameof(cameraPositionOverride));

    public StateBinding _trappedInstanceBinding = new StateBinding(nameof(_trappedInstance));

    public StateBinding _ragdollInstanceBinding = new StateBinding(nameof(_ragdollInstance));

    public StateBinding _cookedInstanceBinding = new StateBinding(nameof(_cookedInstance));

    public StateBinding _duckStateBinding = new DuckFlagBinding(GhostPriority.High);

    public StateBinding _netQuackBinding = new NetSoundBinding(GhostPriority.High, nameof(_netQuack));

    public StateBinding _netSwearBinding = new NetSoundBinding(nameof(_netSwear));

    public StateBinding _netScreamBinding = new NetSoundBinding(nameof(_netScream));

    public StateBinding _netJumpBinding = new NetSoundBinding(GhostPriority.Normal, nameof(_netJump));

    public StateBinding _netDisarmBinding = new NetSoundBinding(nameof(_netDisarm));

    public StateBinding _netTinyMotionBinding = new NetSoundBinding(nameof(_netTinyMotion));

    public StateBinding _conversionResistanceBinding = new StateBinding(nameof(conversionResistance), 8);

    public bool forceDead;

    public bool afk;

    private byte _quackPitch;

    public NetSoundEffect _netQuack = new NetSoundEffect("quack");

    public NetSoundEffect _netJump = new NetSoundEffect("jump")
    {
        volume = 0.5f
    };

    public NetSoundEffect _netDisarm = new NetSoundEffect("disarm")
    {
        volume = 0.3f
    };

    public NetSoundEffect _netTinyMotion = new NetSoundEffect("tinyMotion");

    public NetSoundEffect _netSwear = new NetSoundEffect(new List<string> { "cutOffQuack", "cutOffQuack2" }, new List<string> { "quackBleep" })
    {
        pitchVariationLow = -0.05f,
        pitchVariationHigh = 0.05f
    };

    public NetSoundEffect _netScream = new NetSoundEffect("quackYell01", "quackYell02", "quackYell03");

    public PlusOne currentPlusOne;

    public byte disarmIndex = 9;

    public Vec2 _tounge;

    private int _netProfileIndex = -1;

    private bool _netProfileInit;

    private bool _assignedIndex;

    private Sprite _shield;

    public const int BackpackDepth = -15;

    public const int BackWingDepth = -10;

    public const int ClothingDepth = 4;

    public const int HoldingDepth = 9;

    public const int WingDepth = 11;

    public int ctfTeamIndex;

    public bool forceMindControl;

    public SpriteMap _sprite;

    public SpriteMap _spriteArms;

    public SpriteMap _spriteQuack;

    public SpriteMap _spriteControlled;

    public InputProfile _mindControl;

    public Duck controlledBy;

    private bool _derpMindControl = true;

    protected DuckSkeleton _skeleton = new DuckSkeleton();

    public float slideBuildup;

    public int listenTime;

    public bool listening;

    public bool immobilized;

    public bool beammode;

    public bool jumping;

    public bool doThrow;

    public bool swinging;

    public bool holdObstructed;

    private bool _checkingTeam;

    private bool _checkingPersona;

    private bool _isGrabbedByMagnet;

    public bool isRockThrowDuck;

    public bool _hovering;

    private bool _closingEyes;

    public bool _canFire = true;

    public float crippleTimer;

    public float jumpCharge;

    protected float armOffY;

    protected float armOffX;

    protected float centerOffset;

    protected float holdOffX;

    protected float holdOffY;

    public float holdAngleOff;

    protected float aRadius = 4f;

    protected float dRadius = 4f;

    protected bool reverseThrow;

    protected float kick;

    public float unfocus = 1f;

    protected bool _isGhost;

    private bool _eyesClosed;

    private SinWave _arrowBob = 0.2f;

    private bool _remoteControl;

    private float _ghostTimer = 1f;

    private int _lives;

    private float _runMax = 3.1f;

    private bool _moveLock;

    private InputProfile _inputProfile = InputProfile.Get("SinglePlayer");

    private InputProfile _virtualInput;

    protected Profile _profile;

    protected Sprite _swirl;

    private float _swirlSpin;

    private bool _resetAction;

    protected SpriteMap _bionicArm;

    protected FeatherVolume _featherVolume;

    public List<Equipment> _equipment = new List<Equipment>();

    public int quack;

    public bool quackStart;

    private bool didHat;

    public TrappedDuck _trappedInstance;

    private Ragdoll _ins;

    public CookedDuck _cookedInstance;

    private SpriteMap _lagTurtle;

    private float _duckWidth = 1f;

    private float _duckHeight = 1f;

    private string _collisionMode = "normal";

    private Profile _disarmedBy;

    private DateTime _disarmedAt = DateTime.Now;

    private DateTime _timeSinceFuneralPerformed = DateTime.MinValue;

    private DateTime _timeSinceDuckLayedToRest = DateTime.MinValue;

    private Thing _holdingAtDisarm;

    private int _coolnessThisFrame;

    public Vec2 respawnPos;

    public float respawnTime;

    public Holdable _lastHoldItem;

    public byte _timeSinceThrow;

    public bool killingNet;

    public bool isKillMessage;

    private bool _killed;

    public Profile killedByProfile;

    public int framesSinceKilled;

    public Func<Duck, bool> KillOverride;

    public byte lastAppliedLifeChange;

    public bool invincible;

    public float killMultiplier;

    private List<Thing> added = new List<Thing>();

    public DuckAI ai;

    private bool _crouchLock;

    public byte _flapFrame;

    public int _jumpValid;

    public int _groundValid;

    private TrappedDuck _trappedProp;

    public Profile trappedBy;

    private int breath = -1;

    private bool _throwFondle = true;

    private int tryGrabFrames;

    private bool _heldLeft;

    private bool _heldRight;

    private bool _updatedAnimation;

    private float disarmIndexCooldown;

    public int _disarmWait;

    public int _disarmDisable;

    public int _timeSinceChainKill;

    public int _iceWedging;

    public int framesSinceJump;

    private ATShotgun shotty = new ATShotgun();

    public bool forceFire;

    public float jumpSpeed;

    public float removeBody = 1f;

    public int clientFrame;

    private bool _canWallJump;

    public bool localDuck = true;

    private Ragdoll _currentRagdoll;

    private int _wallJump;

    private bool _rightJump;

    private bool _leftJump;

    private int _atWallFrames;

    private bool leftWall;

    private bool rightWall;

    private bool atWall;

    private int _walkTime;

    private int _walkCount;

    private bool _nextTrigger;

    public bool grappleMul;

    public float grappleMultiplier = 1f;

    public bool onWall;

    private float maxrun;

    public bool pickedHat;

    public Vine _vine;

    public bool _double;

    public float _shieldCharge;

    public int skipPlatFrames;

    private bool didFireSlide;

    public static float JumpSpeed = -4.9f;

    private int _leftPressedFrame;

    private int _rightPressedFrame;

    public bool tvJumped;

    private int bulletIndex;

    public int pipeOut;

    public int pipeBoost;

    public int slamWait;

    public float accelerationMultiplier = 1f;

    public bool strafing;

    private bool crouchCancel;

    private bool vineRelease;

    private Vec2 prevCamPosition;

    private Thing _followPart;

    private int wait;

    private float lastCalc;

    private bool firstCalc = true;

    private CoolnessPlus plus;

    private Ragdoll _prevRagdoll;

    private float _bubbleWait;

    private static int _framesSinceInput = 0;

    private Vec2 _lastGoodPosition;

    private byte _prevDisarmIndex = 6;

    public bool manualQuackPitch;

    private int killedWait;

    private bool unfocused;

    private int _framesUnderwater;

    public int framesSinceRagdoll;

    public float verticalOffset;

    public int swordInvincibility;

    public bool fancyShoes;

    private bool _renderingDuck;

    public float _burnTime = 1f;

    public CookedDuck _cooked;

    private Sound _sizzle;

    private float _handHeat;

    private byte handSmokeWait;

    private Duck _converted;

    public int conversionResistance = 100;

    public bool isConversionMessage;

    private bool _gripped;

    public static bool renderingIcon = false;

    private Camera _iconCamera;

    private Rectangle _iconRect = new Rectangle(0f, 0f, 96f, 96f);

    public Vec2 tongueCheck = Vec2.Zero;

    private Vec2 _stickLerp;

    private Vec2 _stickSlowLerp;

    public bool localSpawnVisible = true;

    public bool enteringWalldoor;

    public bool exitingWalldoor;

    public bool autoExitDoor;

    public int autoExitDoorFrames;

    public DuckAI wallDoorAI;

    public WallDoor transportDoor;

    public float enterDoorSpeed;

    public float tilt;

    private static Material kGhostMaterial;

    public int waitGhost;

    private ConnectionIndicators _indicators;

    public override bool destroyed
    {
        get
        {
            if (!base._destroyed)
            {
                return forceDead;
            }
            return true;
        }
    }

    public byte quackPitch
    {
        get
        {
            return _quackPitch;
        }
        set
        {
            _quackPitch = value;
        }
    }

    public byte spriteFrame
    {
        get
        {
            if (_sprite == null)
            {
                return 0;
            }
            return (byte)_sprite._frame;
        }
        set
        {
            if (_sprite != null)
            {
                _sprite._frame = value;
            }
        }
    }

    public byte spriteImageIndex
    {
        get
        {
            if (_sprite == null)
            {
                return 0;
            }
            return (byte)_sprite._imageIndex;
        }
        set
        {
            if (_sprite != null)
            {
                _sprite._imageIndex = value;
            }
        }
    }

    public float spriteSpeed
    {
        get
        {
            if (_sprite == null)
            {
                return 0f;
            }
            return _sprite._speed;
        }
        set
        {
            if (_sprite != null)
            {
                _sprite._speed = value;
            }
        }
    }

    public float spriteInc
    {
        get
        {
            if (_sprite == null)
            {
                return 0f;
            }
            return _sprite._frameInc;
        }
        set
        {
            if (_sprite != null)
            {
                _sprite._frameInc = value;
            }
        }
    }

    public byte netAnimationIndex
    {
        get
        {
            if (_sprite == null)
            {
                return 0;
            }
            return (byte)_sprite.animationIndex;
        }
        set
        {
            if (_sprite != null && _sprite.animationIndex != value)
            {
                _sprite.animationIndex = value;
            }
        }
    }

    public Vec2 tounge
    {
        get
        {
            if ((!Network.isActive || base.isServerForObject) && inputProfile != null)
            {
                return inputProfile.rightStick;
            }
            return _tounge;
        }
        set
        {
            _tounge = value;
        }
    }

    public byte netProfileIndex
    {
        get
        {
            if (_netProfileIndex < 0 || _netProfileIndex > DG.MaxPlayers - 1)
            {
                return 0;
            }
            return (byte)_netProfileIndex;
        }
        set
        {
            if (_netProfileIndex != value)
            {
                AssignNetProfileIndex(value);
            }
        }
    }

    public Hat hat => GetEquipment(typeof(Hat)) as Hat;

    public InputProfile mindControl
    {
        get
        {
            return _mindControl;
        }
        set
        {
            if (value == null && _mindControl != null && profile != null && (profile.localPlayer || forceMindControl))
            {
                if (holdObject != null)
                {
                    Thing.Fondle(holdObject, DuckNetwork.localConnection);
                }
                foreach (Equipment item in _equipment)
                {
                    Thing.Fondle(item, DuckNetwork.localConnection);
                }
                Thing.Fondle(_ragdollInstance, DuckNetwork.localConnection);
                Thing.Fondle(_cookedInstance, DuckNetwork.localConnection);
                Thing.Fondle(_trappedInstance, DuckNetwork.localConnection);
            }
            _mindControl = value;
        }
    }

    public bool derpMindControl
    {
        get
        {
            return _derpMindControl;
        }
        set
        {
            _derpMindControl = value;
        }
    }

    public DuckSkeleton skeleton
    {
        get
        {
            UpdateSkeleton();
            return _skeleton;
        }
    }

    public bool dead
    {
        get
        {
            return destroyed;
        }
        set
        {
            base._destroyed = value;
        }
    }

    public bool inNet => _trapped != null;

    public Team team
    {
        get
        {
            if (profile == null)
            {
                return null;
            }
            if (_checkingTeam)
            {
                return profile.team;
            }
            if (_converted != null)
            {
                _checkingTeam = true;
                Team result = _converted.team;
                _checkingTeam = false;
                return result;
            }
            return profile.team;
        }
    }

    public DuckPersona persona
    {
        get
        {
            if (profile == null)
            {
                return null;
            }
            if (_checkingPersona)
            {
                return profile.persona;
            }
            if (_converted != null)
            {
                _checkingPersona = true;
                DuckPersona result = _converted.persona;
                _checkingPersona = false;
                return result;
            }
            return profile.persona;
        }
    }

    public bool isGrabbedByMagnet
    {
        get
        {
            return _isGrabbedByMagnet;
        }
        set
        {
            _isGrabbedByMagnet = value;
            if (value || !profile.localPlayer)
            {
                return;
            }
            Angle = 0f;
            immobilized = false;
            gripped = false;
            enablePhysics = true;
            visible = true;
            SetCollisionMode("normal");
            if (holdObject != null)
            {
                Thing.Fondle(holdObject, DuckNetwork.localConnection);
            }
            foreach (Equipment item in _equipment)
            {
                Thing.Fondle(item, DuckNetwork.localConnection);
            }
            Thing.Fondle(_ragdollInstance, DuckNetwork.localConnection);
            Thing.Fondle(_cookedInstance, DuckNetwork.localConnection);
            Thing.Fondle(_trappedInstance, DuckNetwork.localConnection);
        }
    }

    public bool closingEyes
    {
        get
        {
            return _closingEyes;
        }
        set
        {
            _closingEyes = value;
        }
    }

    public bool canFire
    {
        get
        {
            return _canFire;
        }
        set
        {
            _canFire = value;
        }
    }

    public bool isGhost
    {
        get
        {
            return _isGhost;
        }
        set
        {
            _isGhost = value;
        }
    }

    public bool eyesClosed
    {
        get
        {
            return _eyesClosed;
        }
        set
        {
            _eyesClosed = value;
        }
    }

    public bool remoteControl
    {
        get
        {
            return _remoteControl;
        }
        set
        {
            _remoteControl = value;
        }
    }

    public override bool action
    {
        get
        {
            if (!_resetAction)
            {
                if ((CanMove() || (ragdoll != null && !dead && fancyShoes) || _remoteControl || inPipe) && inputProfile.Down("SHOOT"))
                {
                    return _canFire;
                }
                return false;
            }
            return false;
        }
    }

    public Vec2 armPosition => Position + armOffset;

    public Vec2 armOffset
    {
        get
        {
            Vec2 kickVector = Vec2.Zero;
            if (base.gun != null)
            {
                kickVector = -base.gun.barrelVector * kick;
            }
            return new Vec2(armOffX * base.ScaleX + kickVector.X, armOffY * base.ScaleY + kickVector.Y);
        }
    }

    public Vec2 armPositionNoKick => Position + armOffsetNoKick;

    public Vec2 armOffsetNoKick => new Vec2(armOffX * base.ScaleX, armOffY * base.ScaleY);

    public float holdAngle
    {
        get
        {
            if (holdObject != null)
            {
                return holdObject.handAngle + holdAngleOff;
            }
            return holdAngleOff;
        }
    }

    public override Holdable holdObject
    {
        get
        {
            return base.holdObject;
        }
        set
        {
            if (value != holdObject && holdObject != null)
            {
                if (holdObject.isServerForObject && holdObject.owner == this)
                {
                    ThrowItem();
                }
                _lastHoldItem = holdObject;
                _timeSinceThrow = 0;
            }
            base.holdObject = value;
        }
    }

    public int lives
    {
        get
        {
            return _lives;
        }
        set
        {
            _lives = value;
        }
    }

    public float holdingWeight
    {
        get
        {
            if (holdObject == null)
            {
                return 0f;
            }
            return holdObject.weight;
        }
    }

    public override float weight
    {
        get
        {
            return _weight + holdingWeight * 0.4f + ((sliding || crouch) ? 16f : 0f);
        }
        set
        {
            _weight = value;
        }
    }

    public float runMax
    {
        get
        {
            return _runMax;
        }
        set
        {
            _runMax = value;
        }
    }

    public bool moveLock
    {
        get
        {
            return _moveLock;
        }
        set
        {
            _moveLock = value;
        }
    }

    public InputProfile inputProfile
    {
        get
        {
            if (wallDoorAI != null)
            {
                return wallDoorAI;
            }
            if (_mindControl != null)
            {
                return _mindControl;
            }
            if (_virtualInput != null)
            {
                return _virtualInput;
            }
            if (_profile != null)
            {
                return _profile.inputProfile;
            }
            return _inputProfile;
        }
    }

    public Profile profile
    {
        get
        {
            return _profile;
        }
        set
        {
            _profile = value;
            if (Network.isActive && _profile != null)
            {
                if (_profile.localPlayer)
                {
                    Thing.Fondle(this, DuckNetwork.localConnection);
                }
                else if (_profile.connection != null)
                {
                    connection = _profile.connection;
                }
            }
        }
    }

    public override NetworkConnection connection
    {
        get
        {
            return base.connection;
        }
        set
        {
            if (Network.isServer && connection != null && connection.status == ConnectionStatus.Disconnected && Network.InGameLevel())
            {
                Kill(new DTDisconnect(this));
            }
            if (_profile != null)
            {
                if (_profile.localPlayer && !CanBeControlled())
                {
                    if (connection != DuckNetwork.localConnection)
                    {
                        base.connection = DuckNetwork.localConnection;
                        authority += 5;
                    }
                }
                else
                {
                    base.connection = value;
                }
            }
            else
            {
                base.connection = value;
            }
        }
    }

    public bool resetAction
    {
        get
        {
            return _resetAction;
        }
        set
        {
            _resetAction = value;
        }
    }

    public Ragdoll _ragdollInstance
    {
        get
        {
            return _ins;
        }
        set
        {
            _ins = value;
        }
    }

    public float duckWidth
    {
        get
        {
            return _duckWidth;
        }
        set
        {
            _duckWidth = value;
            base.ScaleX = _duckWidth;
        }
    }

    public float duckHeight
    {
        get
        {
            return _duckHeight;
        }
        set
        {
            _duckHeight = value;
            base.ScaleY = _duckHeight;
        }
    }

    public float duckSize
    {
        get
        {
            return _duckHeight;
        }
        set
        {
            float num = (duckHeight = value);
            duckWidth = num;
        }
    }

    public bool crouchLock
    {
        get
        {
            return _crouchLock;
        }
        set
        {
            _crouchLock = value;
        }
    }

    public TrappedDuck _trapped
    {
        get
        {
            return _trappedProp;
        }
        set
        {
            _trappedProp = value;
        }
    }

    public override float holdWeightMultiplier
    {
        get
        {
            float ret = 1f;
            if (holdObject != null)
            {
                ret = holdObject.weightMultiplier;
            }
            if (holstered != null)
            {
                ret = Math.Min(holstered.weightMultiplier, ret);
            }
            return ret;
        }
    }

    public override float holdWeightMultiplierSmall
    {
        get
        {
            if (holdObject != null)
            {
                return holdObject.weightMultiplierSmall;
            }
            return 1f;
        }
    }

    public Ragdoll ragdoll
    {
        get
        {
            return _currentRagdoll;
        }
        set
        {
            _currentRagdoll = value;
            if (_currentRagdoll != null)
            {
                _currentRagdoll._duck = this;
            }
        }
    }

    public override float Angle
    {
        get
        {
            return AngleValue + tilt * 0.2f;
        }
        set
        {
            AngleValue = value;
        }
    }

    public Holdable holstered
    {
        get
        {
            if (GetEquipment(typeof(Holster)) is Holster h)
            {
                return h.containedObject;
            }
            return null;
        }
        set
        {
            if (GetEquipment(typeof(Holster)) is Holster h)
            {
                h.SetContainedObject(value);
            }
        }
    }

    public Holdable skewered
    {
        get
        {
            if (GetEquipment(typeof(SpikeHelm)) is SpikeHelm h)
            {
                return h.poked as Holdable;
            }
            return null;
        }
    }

    public TV tvHeld
    {
        get
        {
            if (holdObject is TV { _ruined: false } t)
            {
                return t;
            }
            return null;
        }
    }

    public TV tvHolster
    {
        get
        {
            TV t = null;
            if (GetEquipment(typeof(Holster)) is Holster h && h.containedObject is TV)
            {
                t = h.containedObject as TV;
            }
            if (t != null && !t._ruined)
            {
                return t;
            }
            return null;
        }
    }

    public TV tvHat
    {
        get
        {
            TV t = null;
            if (GetEquipment(typeof(SpikeHelm)) is SpikeHelm h && h.poked is TV)
            {
                t = h.poked as TV;
            }
            if (t != null && !t._ruined)
            {
                return t;
            }
            return null;
        }
    }

    public override Vec2 cameraPosition
    {
        get
        {
            Vec2 pos = Vec2.Zero;
            pos = ((ragdoll != null) ? ragdoll.cameraPosition : ((_cooked != null) ? _cooked.cameraPosition : ((_trapped == null) ? base.cameraPosition : _trapped.cameraPosition)));
            if ((cameraPositionOverride - Position).Length() < 1000f)
            {
                cameraPositionOverride = Vec2.Zero;
            }
            if (cameraPositionOverride != Vec2.Zero)
            {
                return cameraPositionOverride;
            }
            if (pos.Y < -1000f || pos == Vec2.Zero || pos.X < -5000f)
            {
                pos = prevCamPosition;
            }
            else
            {
                prevCamPosition = pos;
            }
            return pos;
        }
    }

    public override Vec2 anchorPosition => cameraPosition;

    public Thing followPart
    {
        get
        {
            if (_followPart == null)
            {
                return this;
            }
            return _followPart;
        }
    }

    public bool underwater
    {
        get
        {
            if (doFloat && _curPuddle != null)
            {
                return base.top + 2f > _curPuddle.top;
            }
            return false;
        }
    }

    public override bool active
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

    public bool chatting
    {
        get
        {
            if (profile != null)
            {
                return profile.netData.Get<bool>("chatting");
            }
            return false;
        }
    }

    public override Thing realObject
    {
        get
        {
            if (_trapped != null)
            {
                return _trapped;
            }
            return this;
        }
    }

    public bool protectedFromFire
    {
        get
        {
            if ((holdObject == null || !(holdObject.heat < -0.05f)) && (holstered == null || !(holstered.heat < -0.05f)))
            {
                if (skewered != null)
                {
                    return skewered.heat < -0.05f;
                }
                return false;
            }
            return true;
        }
    }

    public Duck converted => _converted;

    public bool gripped
    {
        get
        {
            return _gripped;
        }
        set
        {
            _gripped = value;
        }
    }

    public Color blendColor
    {
        set
        {
            _spriteArms.color = value;
            _spriteControlled.color = value;
            _spriteQuack.color = value;
            _sprite.color = value;
        }
    }

    private void AssignNetProfileIndex(byte pIndex)
    {
        DevConsole.Log(DCSection.General, "Assigning net profile index (" + pIndex + "\\" + Profiles.all.Count() + ")");
        _netProfileIndex = pIndex;
        Profile p = Profiles.all.ElementAt(_netProfileIndex);
        if (Network.isClient && Network.InLobby())
        {
            (Level.current as TeamSelect2).OpenDoor(_netProfileIndex, this);
        }
        profile = p;
        if (p.team == null)
        {
            p.team = Teams.all[_netProfileIndex];
        }
        InitProfile();
        _netProfileInit = true;
        _assignedIndex = true;
    }

    public override bool CanBeControlled()
    {
        if (mindControl != null || isGrabbedByMagnet || listening || dead || wasSuperFondled > 0)
        {
            return true;
        }
        return false;
    }

    public void CancelFlapping()
    {
        _hovering = false;
    }

    public bool IsNetworkDuck()
    {
        if (!isRockThrowDuck)
        {
            return Network.isClient;
        }
        return false;
    }

    public bool CanMove()
    {
        if (holdObject != null && holdObject.immobilizeOwner)
        {
            return false;
        }
        if (!immobilized && !(crippleTimer > 0f) && !inNet && !swinging && !dead && !listening && Level.current.simulatePhysics && !_closingEyes)
        {
            return ragdoll == null;
        }
        return false;
    }

    public static Duck Get(int index)
    {
        foreach (Thing item in Level.current.things[typeof(Duck)])
        {
            Duck d = item as Duck;
            if (Persona.Number(d.profile.persona) == index)
            {
                return d;
            }
        }
        return null;
    }

    public Vec2 HoldOffset(Vec2 pos)
    {
        Vec2 offset = (pos + new Vec2(holdOffX, holdOffY)).Rotate(holdAngle, new Vec2(0f, 0f));
        offset += armOffset;
        return Position + offset;
    }

    public static Duck GetAssociatedDuck(Thing pThing)
    {
        if (pThing == null)
        {
            return null;
        }
        if (pThing is Duck)
        {
            return pThing as Duck;
        }
        if (pThing.owner is Duck)
        {
            return pThing.owner as Duck;
        }
        if (pThing is RagdollPart && (pThing as RagdollPart).doll != null)
        {
            return (pThing as RagdollPart).doll.captureDuck;
        }
        if (pThing is Ragdoll)
        {
            return (pThing as Ragdoll).captureDuck;
        }
        if (pThing is TrappedDuck)
        {
            return (pThing as TrappedDuck).captureDuck;
        }
        return null;
    }

    public Duck GetHeldByDuck()
    {
        if (ragdoll != null)
        {
            if (ragdoll.part1 != null && ragdoll.part1.owner is Duck)
            {
                return ragdoll.part1.owner as Duck;
            }
            if (ragdoll.part2 != null && ragdoll.part2.owner is Duck)
            {
                return ragdoll.part1.owner as Duck;
            }
            if (ragdoll.part3 != null && ragdoll.part3.owner is Duck)
            {
                return ragdoll.part1.owner as Duck;
            }
        }
        else if (_trapped != null && _trapped.owner is Duck)
        {
            return _trapped.owner as Duck;
        }
        return null;
    }

    public bool IsOwnedBy(Thing pThing)
    {
        if (pThing == null)
        {
            return false;
        }
        if (owner == pThing)
        {
            return true;
        }
        if (_trapped != null && _trapped.owner == pThing)
        {
            return true;
        }
        if (ragdoll != null)
        {
            if (ragdoll.part1 != null && ragdoll.part1.owner == pThing)
            {
                return true;
            }
            if (ragdoll.part2 != null && ragdoll.part2.owner == pThing)
            {
                return true;
            }
            if (ragdoll.part3 != null && ragdoll.part3.owner == pThing)
            {
                return true;
            }
        }
        return false;
    }

    public bool HoldingTaped(Holdable pObject)
    {
        if (holdObject is TapedGun && ((holdObject as TapedGun).gun1 == pObject || (holdObject as TapedGun).gun2 == pObject))
        {
            return true;
        }
        return false;
    }

    public bool Held(Holdable pObject, bool ignorePowerHolster = false)
    {
        if (holdObject == pObject)
        {
            return true;
        }
        if (holdObject is TapedGun && ((holdObject as TapedGun).gun1 == pObject || (holdObject as TapedGun).gun2 == pObject))
        {
            return true;
        }
        if (!ignorePowerHolster && GetEquipment(typeof(Holster)) is Holster h && h is PowerHolster && pObject == h.containedObject)
        {
            return true;
        }
        return false;
    }

    public virtual void InitProfile()
    {
        _profile.duck = this;
        _sprite = profile.persona.sprite.CloneMap();
        _spriteArms = profile.persona.armSprite.CloneMap();
        _spriteQuack = profile.persona.quackSprite.CloneMap();
        _spriteControlled = profile.persona.controlledSprite.CloneMap();
        _swirl = new Sprite("swirl");
        _swirl.CenterOrigin();
        _swirl.Scale = new Vec2(0.75f, 0.75f);
        _bionicArm = new SpriteMap("bionicArm", 32, 32);
        _bionicArm.CenterOrigin();
        if (!didHat && (Network.isServer || RockScoreboard.initializingDucks))
        {
            if (profile.team != null && profile.team.hasHat)
            {
                Hat h = new TeamHat(0f, 0f, team, profile);
                if (RockScoreboard.initializingDucks)
                {
                    h.IgnoreNetworkSync();
                }
                Level.Add(h);
                Equip(h, makeSound: false, forceNetwork: true);
            }
            didHat = true;
        }
        graphic = _sprite;
    }

    public Duck(float xval, float yval, Profile pro)
        : base(xval, yval)
    {
        _featherVolume = new FeatherVolume(this);
        _featherVolume.anchor = this;
        duck = true;
        profile = pro;
        if (_profile == null)
        {
            _profile = Profiles.EnvironmentProfile;
        }
        if (profile != null)
        {
            InitProfile();
        }
        base.CenterX = 16f;
        base.CenterY = 16f;
        friction = 0.25f;
        vMax = 8f;
        hMax = 12f;
        _lagTurtle = new SpriteMap("lagturtle", 16, 16);
        _lagTurtle.CenterOrigin();
        physicsMaterial = PhysicsMaterial.Duck;
        base.collideSounds.Add("land", ImpactedFrom.Bottom);
        _impactThreshold = 1.3f;
        _impactVolume = 0.4f;
        SetCollisionMode("normal");
        _shield = new Sprite("sheeld");
        _shield.CenterOrigin();
        flammable = 1f;
        thickness = 0.5f;
    }

    public override void Terminate()
    {
        if (Level.current.camera is FollowCam)
        {
            (Level.current.camera as FollowCam).Remove(this);
        }
        Level.Remove(_featherVolume);
        if (Network.isActive)
        {
            Level.Remove(_ragdollInstance);
            Level.Remove(_trappedInstance);
            Level.Remove(_cookedInstance);
        }
        foreach (Equipment item in _equipment.ToList())
        {
            Level.Remove(item);
        }
    }

    public void SetCollisionMode(string mode)
    {
        _collisionMode = mode;
        if (offDir > 0)
        {
            _featherVolume.anchor.offset = new Vec2(0f, 0f);
        }
        else
        {
            _featherVolume.anchor.offset = new Vec2(1f, 0f);
        }
        switch (mode)
        {
            case "normal":
                collisionSize = new Vec2(8f * duckWidth, 22f * duckHeight);
                collisionOffset = new Vec2(-4f * duckWidth, -7f * duckHeight);
                _featherVolume.collisionSize = new Vec2(12f * duckWidth, 26f * duckHeight);
                _featherVolume.collisionOffset = new Vec2(-6f * duckWidth, -9f * duckHeight);
                break;
            case "slide":
                collisionSize = new Vec2(8f * duckWidth, 11f * duckHeight);
                collisionOffset = new Vec2(-4f * duckWidth, 4f * duckHeight);
                if (offDir > 0)
                {
                    _featherVolume.collisionSize = new Vec2(25f * duckWidth, 13f * duckHeight);
                    _featherVolume.collisionOffset = new Vec2(-13f * duckWidth, 3f * duckHeight);
                }
                else
                {
                    _featherVolume.collisionSize = new Vec2(25f * duckWidth, 13f * duckHeight);
                    _featherVolume.collisionOffset = new Vec2(-12f * duckWidth, 3f * duckHeight);
                }
                break;
            case "crouch":
                collisionSize = new Vec2(8f * duckWidth, 16f * duckHeight);
                collisionOffset = new Vec2(-4f * duckWidth, -1f * duckHeight);
                _featherVolume.collisionSize = new Vec2(12f * duckWidth, 20f * duckHeight);
                _featherVolume.collisionOffset = new Vec2(-6f * duckWidth, -3f * duckHeight);
                break;
            case "netted":
                collisionSize = new Vec2(16f * duckWidth, 17f * duckHeight);
                collisionOffset = new Vec2(-8f * duckWidth, -9f * duckHeight);
                _featherVolume.collisionSize = new Vec2(18f * duckWidth, 19f * duckHeight);
                _featherVolume.collisionOffset = new Vec2(-9f * duckWidth, -10f * duckHeight);
                break;
        }
        if (ragdoll != null)
        {
            _featherVolume.collisionSize = new Vec2(12f * duckWidth, 12f * duckHeight);
            _featherVolume.collisionOffset = new Vec2(-6f * duckWidth, -6f * duckHeight);
        }
    }

    public void KnockOffEquipment(Equipment e, bool ting = true, Bullet b = null)
    {
        if (!_equipment.Contains(e))
        {
            return;
        }
        if (base.isServerForObject)
        {
            RumbleManager.AddRumbleEvent(profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Short, RumbleFalloff.None));
        }
        e.UnEquip();
        if (ting && !Network.isActive)
        {
            SFX.Play("ting2");
        }
        _equipment.Remove(e);
        e.Destroy(new DTImpact(null));
        e.solid = false;
        if (b != null)
        {
            e.hSpeed = b.travelDirNormalized.X;
            e.vSpeed = -2f;
            if (base.isServerForObject)
            {
                hSpeed += b.travelDirNormalized.X * (b.ammo.impactPower + 1f);
                vSpeed += b.travelDirNormalized.Y * (b.ammo.impactPower + 1f);
                vSpeed -= 1f;
            }
        }
        else
        {
            e.hSpeed = (float)(-offDir) * 2f;
            e.vSpeed = -2f;
        }
        ReturnItemToWorld(e);
    }

    public override void ReturnItemToWorld(Thing t)
    {
        Vec2 pos = Position;
        if (sliding)
        {
            pos.Y += 10f;
        }
        else if (crouch)
        {
            pos.Y += 8f;
        }
        Block rightWall = Level.CheckLine<Block>(pos, pos + new Vec2(16f, 0f));
        if (rightWall != null && rightWall.solid && t.right > rightWall.left)
        {
            t.right = rightWall.left;
        }
        Block leftWall = Level.CheckLine<Block>(pos, pos - new Vec2(16f, 0f));
        if (leftWall != null && leftWall.solid && t.left < leftWall.right)
        {
            t.left = leftWall.right;
        }
        Block topWall = Level.CheckLine<Block>(pos, pos + new Vec2(0f, -16f));
        if (topWall != null && topWall.solid && t.top < topWall.bottom)
        {
            t.top = topWall.bottom;
        }
        Block bottomWall = Level.CheckLine<Block>(pos, pos + new Vec2(0f, 16f));
        if (bottomWall != null && bottomWall.solid && t.bottom > bottomWall.top)
        {
            t.bottom = bottomWall.top;
        }
    }

    public void Unequip(Equipment e, bool forceNetwork = false)
    {
        if ((base.isServerForObject || forceNetwork) && e != null && _equipment.Contains(e))
        {
            Fondle(e);
            e.UnEquip();
            _equipment.Remove(e);
            ReturnItemToWorld(e);
        }
    }

    public bool HasJumpModEquipment()
    {
        foreach (Equipment item in _equipment)
        {
            if (item.jumpMod)
            {
                return true;
            }
        }
        return false;
    }

    public Equipment GetEquipment(Type t)
    {
        foreach (Equipment e in _equipment)
        {
            if (e.GetAllTypes().Contains(t))
            {
                return e;
            }
        }
        return null;
    }

    public void Equip(Equipment e, bool makeSound = true, bool forceNetwork = false)
    {
        if (!(base.isServerForObject || forceNetwork))
        {
            return;
        }
        List<Type> types = e.GetAllTypesFiltered(typeof(Equipment));
        if (types.Contains(typeof(ITeleport)))
        {
            types.Remove(typeof(ITeleport));
        }
        foreach (Type t in types)
        {
            if (!t.IsInterface)
            {
                Equipment eq = GetEquipment(t);
                if (eq == null && e.GetType() == typeof(Jetpack))
                {
                    eq = GetEquipment(typeof(Grapple));
                }
                else if (eq == null && e.GetType() == typeof(Grapple))
                {
                    eq = GetEquipment(typeof(Jetpack));
                }
                if (eq != null)
                {
                    _equipment.Remove(eq);
                    Fondle(eq);
                    eq.vSpeed = -2f;
                    eq.hSpeed = (float)offDir * 3f;
                    eq.UnEquip();
                    ReturnItemToWorld(eq);
                }
            }
        }
        if (e is TeamHat)
        {
            TeamHat t2 = e as TeamHat;
            if (profile != null && t2.team != profile.team && !t2.hasBeenStolen)
            {
                Global.data.hatsStolen++;
                t2.hasBeenStolen = true;
            }
        }
        Fondle(e);
        _equipment.Add(e);
        e.Equip(this);
        if (!makeSound)
        {
            e._prevEquipped = true;
        }
        else
        {
            e.equipIndex += 1;
        }
    }

    public List<Equipment> GetArmor()
    {
        List<Equipment> armor = new List<Equipment>();
        foreach (Equipment e in _equipment)
        {
            if (e.isArmor)
            {
                armor.Add(e);
            }
        }
        return armor;
    }

    public bool ExtendsTo(Thing t)
    {
        if (ragdoll != null)
        {
            if (t != ragdoll.part1 && t != ragdoll.part2)
            {
                return t == ragdoll.part3;
            }
            return true;
        }
        return false;
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        if (_trapped != null || (_trappedInstance != null && _trappedInstance.visible))
        {
            return false;
        }
        if (ragdoll != null || (_ragdollInstance != null && _ragdollInstance.visible))
        {
            return false;
        }
        if (bullet.isLocal && !HitArmor(bullet, hitPos))
        {
            Kill(new DTShot(bullet));
            SFX.Play("thwip", 1f, Rando.Float(-0.1f, 0.1f));
        }
        return base.Hit(bullet, hitPos);
    }

    public bool HitArmor(Bullet bullet, Vec2 hitPos)
    {
        if (bullet.isLocal)
        {
            foreach (Equipment e in _equipment)
            {
                if (!bullet._currentlyImpacting.Contains(e) && Collision.Point(hitPos, e))
                {
                    bullet._currentlyImpacting.Add(e);
                    if (e.DoHit(bullet, hitPos))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public override bool Destroy(DestroyType type = null)
    {
        return Kill(type);
    }

    public void AddCoolness(int amount)
    {
        if (Highlights.highlightRatingMultiplier != 0f)
        {
            profile.stats.coolness += amount;
            _coolnessThisFrame += amount;
            if (Recorder.currentRecording != null)
            {
                Recorder.currentRecording.LogCoolness(Math.Abs(amount));
            }
        }
    }

    public bool WillAcceptLifeChange(byte pLifeChange)
    {
        if (lastAppliedLifeChange < pLifeChange || Math.Abs(lastAppliedLifeChange - pLifeChange) > 20)
        {
            lastAppliedLifeChange = pLifeChange;
            return true;
        }
        return false;
    }

    public virtual bool Kill(DestroyType type = null)
    {
        if (_killed || (!isKillMessage && invincible && !(type is DTFall) && !(type is DTPop)))
        {
            return true;
        }
        if (KillOverride != null && KillOverride(this))
        {
            return false;
        }
        forceDead = true;
        _killed = true;
        RumbleManager.AddRumbleEvent(profile, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Short));
        int xpLogged = 10;
        if (type is DTFall)
        {
            Vec2 pos = GetEdgePos();
            Vec2 dir = (pos - GetPos()).Normalized;
            for (int i = 0; i < 8; i++)
            {
                Feather feather = Feather.New(pos.X - dir.X * 16f, pos.Y - dir.Y * 16f, persona);
                feather.hSpeed += dir.X * 1f;
                feather.vSpeed += dir.Y * 1f;
                Level.Add(feather);
            }
        }
        if (!GameMode.firstDead)
        {
            Party.AddDrink(profile, 1);
            if (Rando.Float(1f) > 0.8f)
            {
                Party.AddRandomPerk(profile);
            }
            GameMode.firstDead = true;
        }
        if (Rando.Float(1f) > 0.97f)
        {
            Party.AddRandomPerk(profile);
            Party.AddDrink(profile, 1);
        }
        if (Recorder.currentRecording != null)
        {
            Recorder.currentRecording.LogDeath();
        }
        base._destroyed = true;
        if (_isGhost)
        {
            return false;
        }
        swinging = false;
        Holster h = GetEquipment(typeof(Holster)) as Holster;
        foreach (Equipment e in _equipment)
        {
            if (e != null)
            {
                e.sleeping = false;
                e.owner = null;
                if (!isKillMessage)
                {
                    Thing.ExtraFondle(e, DuckNetwork.localConnection);
                }
                e.hSpeed = hSpeed - (1f + NetRand.Float(2f));
                e.vSpeed = vSpeed - NetRand.Float(1.5f);
                ReturnItemToWorld(e);
                e.UnEquip();
            }
        }
        _equipment.Clear();
        if (TeamSelect2.QUACK3 && h != null)
        {
            Equip(h, makeSound: false);
        }
        Profile killedBy = type.responsibleProfile;
        bool wasTrapped = false;
        if (_trapped != null)
        {
            if (type is DTFall || type is DTImpale)
            {
                killedBy = trappedBy;
                if (_trapped.prevOwner is Duck d)
                {
                    d.AddCoolness(1);
                }
            }
            if ((type is DTFall || type is DTImpale) && trappedBy != null && trappedBy.localPlayer)
            {
                Global.data.nettedDuckTossKills += 1;
            }
            if (!killingNet)
            {
                killingNet = true;
                _trapped.Destroy(type);
            }
            wasTrapped = true;
        }
        if (type is DTIncinerate)
        {
            xpLogged -= 3;
        }
        if (killedBy != null && killedBy.localPlayer)
        {
            killedByProfile = killedBy;
        }
        OnKill(type);
        Holdable prevHold = holdObject;
        if (!isKillMessage)
        {
            ThrowItem(throwWithForce: false);
            if (prevHold != null)
            {
                prevHold.hSpeed *= 0.3f;
                if (type is DTImpale)
                {
                    float num = (prevHold.hSpeed = 0f);
                    prevHold.vSpeed = num;
                }
                if (Network.isActive)
                {
                    Thing.AuthorityFondle(prevHold, DuckNetwork.localConnection, 5);
                }
            }
        }
        else if (profile != null && profile.connection == DuckNetwork.localConnection)
        {
            ThrowItem(throwWithForce: false);
        }
        base.Depth = 0.3f;
        if (killedBy != null)
        {
            if (killedBy == profile)
            {
                killedBy.stats.suicides++;
            }
            else
            {
                killedBy.stats.kills++;
            }
        }
        if (Level.current is TeamSelect2 && isKillMessage)
        {
            ProfileBox2 box = Level.CheckPoint<ProfileBox2>(cameraPosition);
            if (box != null && box.duck == this)
            {
                profile.punished++;
            }
        }
        if (!Network.isActive && !(Level.current is ChallengeLevel))
        {
            int myCoolness = 0;
            int yourCoolness = 0;
            Type weapon = type.killThingType;
            Thing weaponThing = type.thing;
            Bullet bullet = weaponThing as Bullet;
            if (bullet != null)
            {
                if (bullet.travelTime > 0.5f)
                {
                    yourCoolness++;
                }
                if (bullet.bulletDistance > 300f)
                {
                    yourCoolness++;
                }
                if (bullet.didPenetrate)
                {
                    yourCoolness++;
                }
                weaponThing = bullet.firedFrom;
            }
            Event.Log(new KillEvent(killedBy, profile, weapon));
            profile.stats.LogKill(killedBy);
            if (weapon != null)
            {
                if (weapon == typeof(Mine))
                {
                    myCoolness--;
                    xpLogged += 5;
                }
                if (weapon == typeof(HugeLaser))
                {
                    myCoolness--;
                    if (killedBy != null)
                    {
                        yourCoolness++;
                    }
                }
                if (weapon == typeof(SuicidePistol))
                {
                    myCoolness--;
                    if (killedBy != null && !killedBy.duck.dead)
                    {
                        yourCoolness++;
                    }
                }
            }
            if (killedBy != null && killedBy.duck != null)
            {
                if (killedBy == profile)
                {
                    yourCoolness -= 2;
                    if (weapon == typeof(Grenade))
                    {
                        yourCoolness--;
                    }
                    if (prevHold == weaponThing)
                    {
                        yourCoolness--;
                    }
                    if (weapon == typeof(QuadLaser))
                    {
                        yourCoolness--;
                    }
                    Party.AddDrink(profile, 2);
                    if (Rando.Float(1f) > 0.9f)
                    {
                        Party.AddRandomPerk(profile);
                    }
                }
                else
                {
                    yourCoolness++;
                    if (weapon == typeof(QuadLaser) && type is DTIncinerate)
                    {
                        QuadLaserBullet bb = (type as DTIncinerate).thing as QuadLaserBullet;
                        float mult = 1f + Math.Min(bb.timeAlive / 5f, 2f);
                        yourCoolness += (int)(1f * mult);
                    }
                    if ((DateTime.Now - killedBy.stats.lastKillTime).TotalSeconds < 2.0)
                    {
                        yourCoolness++;
                    }
                    if (bullet != null && Math.Abs(bullet.travelDirNormalized.Y) > 0.3f)
                    {
                        yourCoolness++;
                    }
                    killedBy.stats.lastKillTime = DateTime.Now;
                    if (weaponThing is Grenade)
                    {
                        yourCoolness++;
                        Grenade g = weaponThing as Grenade;
                        if (g.cookTimeOnThrow < 0.5f && g.cookThrower != null)
                        {
                            g.cookThrower.AddCoolness(1);
                        }
                    }
                    if (Math.Abs(killedBy.duck.hSpeed) + Math.Abs(killedBy.duck.vSpeed) + Math.Abs(hSpeed) + Math.Abs(vSpeed) > 20f)
                    {
                        yourCoolness++;
                    }
                    if (_holdingAtDisarm != null && _disarmedBy == killedBy && (DateTime.Now - _disarmedAt).TotalSeconds < 3.0)
                    {
                        if (killedBy.duck.holdObject == _holdingAtDisarm)
                        {
                            yourCoolness += 4;
                            myCoolness -= 2;
                        }
                        else
                        {
                            yourCoolness++;
                        }
                    }
                    if (killedBy.duck.dead)
                    {
                        yourCoolness++;
                        killedBy.stats.killsFromTheGrave++;
                    }
                    if (type is DTShot && prevHold == null)
                    {
                        killedBy.stats.unarmedDucksShot++;
                    }
                    else if (prevHold != null)
                    {
                        if (prevHold is PlasmaBlaster)
                        {
                            yourCoolness++;
                        }
                        else if (prevHold is Saxaphone || prevHold is Trombone || prevHold is DrumSet)
                        {
                            yourCoolness--;
                            myCoolness++;
                            Party.AddDrink(killedBy, 1);
                        }
                        else if (prevHold is Flower)
                        {
                            yourCoolness -= 2;
                            myCoolness += 2;
                            Party.AddDrink(killedBy, 1);
                        }
                    }
                    if (weapon != null)
                    {
                        if (weapon == typeof(SledgeHammer) || weapon == typeof(DuelingPistol))
                        {
                            yourCoolness++;
                        }
                        if (weaponThing is Sword && weaponThing.owner != null && (weaponThing as Sword).jabStance)
                        {
                            yourCoolness++;
                        }
                    }
                    if (wasTrapped && type is DTFall)
                    {
                        yourCoolness++;
                    }
                    if (type is DTCrush)
                    {
                        if (weaponThing is PhysicsObject)
                        {
                            _ = (DateTime.Now - (weaponThing as PhysicsObject).lastGrounded).TotalSeconds;
                            yourCoolness += 1 + (int)Math.Floor((DateTime.Now - (weaponThing as PhysicsObject).lastGrounded).TotalSeconds * 6.0);
                            if (Recorder.currentRecording != null)
                            {
                                Recorder.currentRecording.LogAction(14);
                            }
                            Party.AddDrink(profile, 1);
                            xpLogged += 5;
                            if (Rando.Float(1f) > 0.8f)
                            {
                                Party.AddRandomPerk(profile);
                            }
                        }
                        else
                        {
                            yourCoolness++;
                        }
                    }
                }
                if (killedBy.duck.team == team && killedBy != profile)
                {
                    yourCoolness -= 2;
                    Party.AddDrink(killedBy, 1);
                }
                if ((DateTime.Now - _timeSinceDuckLayedToRest).TotalSeconds < 3.0)
                {
                    yourCoolness--;
                }
                if ((DateTime.Now - _timeSinceFuneralPerformed).TotalSeconds < 3.0)
                {
                    yourCoolness -= 2;
                }
            }
            if (controlledBy != null && controlledBy.profile != null)
            {
                controlledBy.profile.stats.coolness += Math.Abs(myCoolness);
                if (myCoolness > 0)
                {
                    myCoolness = 0;
                }
            }
            yourCoolness++;
            myCoolness--;
            if (killedBy != null && killedBy.duck != null)
            {
                yourCoolness *= (int)Math.Ceiling(1f + killedBy.duck.killMultiplier);
                killedBy.duck.AddCoolness(yourCoolness);
            }
            AddCoolness(myCoolness);
            if (killedBy != null && killedBy.duck != null)
            {
                killedBy.duck.killMultiplier += 1f;
                if (TeamSelect2.KillsForPoints)
                {
                    Profile realProfile = killedBy;
                    if (killedBy.duck.converted != null)
                    {
                        realProfile = killedBy.duck.converted.profile;
                    }
                    if (killedBy.team != profile.team)
                    {
                        SFX.Play("scoreDingShort", 0.9f);
                        if (killedBy.duck != null && killedBy.duck.currentPlusOne != null)
                        {
                            killedBy.duck.currentPlusOne.Pulse();
                        }
                        else
                        {
                            PlusOne plus = new PlusOne(0f, 0f, realProfile, temp: false, testMode: true)
                            {
                                _duck = killedBy.duck
                            };
                            plus.anchor = killedBy.duck;
                            plus.anchor.offset = new Vec2(0f, -16f);
                            if (killedBy.duck != null)
                            {
                                killedBy.duck.currentPlusOne = plus;
                            }
                            Level.Add(plus);
                        }
                        realProfile.team.score++;
                        if (Teams.active.Count > 1 && Network.isActive && killedBy.connection == DuckNetwork.localConnection)
                        {
                            DuckNetwork.GiveXP("Ducks Despawned", 1, 1, 4, 25, 40);
                        }
                        if (Network.isActive && Network.isServer)
                        {
                            Send.Message(new NMAssignKill(new List<Profile> { killedBy }, null));
                        }
                    }
                }
            }
        }
        if (Highlights.highlightRatingMultiplier != 0f)
        {
            profile.stats.timesKilled++;
        }
        if (profile.connection == DuckNetwork.localConnection)
        {
            DuckNetwork.deaths++;
        }
        if (!isKillMessage)
        {
            if (profile.connection != DuckNetwork.localConnection)
            {
                DuckNetwork.kills++;
            }
            if (TeamSelect2.Enabled("CORPSEBLOW"))
            {
                Grenade obj = new Grenade(base.X, base.Y)
                {
                    hSpeed = hSpeed + Rando.Float(-2f, 2f),
                    vSpeed = vSpeed - Rando.Float(1f, 2.5f)
                };
                Level.Add(obj);
                obj.PressAction();
            }
            Thing.SuperFondle(this, DuckNetwork.localConnection);
            if (_trappedInstance != null)
            {
                Thing.SuperFondle(_trappedInstance, DuckNetwork.localConnection);
            }
            if (holdObject != null)
            {
                Thing.SuperFondle(holdObject, DuckNetwork.localConnection);
            }
            if (base.Y < -999f)
            {
                Vec2 pos2 = Position;
                Position = _lastGoodPosition;
                GoRagdoll();
                Position = pos2;
            }
            else
            {
                GoRagdoll();
            }
        }
        if (Network.isActive && ragdoll != null && !isKillMessage)
        {
            Thing.SuperFondle(ragdoll, DuckNetwork.localConnection);
        }
        if (Network.isActive && !isKillMessage)
        {
            lastAppliedLifeChange++;
            Send.Message(new NMKillDuck(profile.networkIndex, type is DTCrush, type is DTIncinerate, type is DTFall, lastAppliedLifeChange));
        }
        if (!(this is TargetDuck))
        {
            Global.Kill(this, type);
        }
        return true;
    }

    public override void Zap(Thing zapper)
    {
        GoRagdoll();
        if (ragdoll != null)
        {
            ragdoll.Zap(zapper);
        }
        base.Zap(zapper);
    }

    public override void Removed()
    {
        if (Network.isServer)
        {
            if (_ragdollInstance != null)
            {
                Thing.Fondle(_ragdollInstance, DuckNetwork.localConnection);
                Level.Remove(_ragdollInstance);
            }
            if (_trappedInstance != null)
            {
                Thing.Fondle(_trappedInstance, DuckNetwork.localConnection);
                Level.Remove(_trappedInstance);
            }
            if (_cookedInstance != null)
            {
                Thing.Fondle(_cookedInstance, DuckNetwork.localConnection);
                Level.Remove(_cookedInstance);
            }
        }
        base.Removed();
    }

    public void Disappear()
    {
        if (ragdoll != null)
        {
            Position = ragdoll.Position;
            if (Network.isActive)
            {
                ragdoll.Unragdoll();
            }
            else
            {
                Level.Remove(ragdoll);
            }
            vSpeed = -2f;
        }
        OnTeleport();
        base.Y += 9999f;
    }

    public void Cook()
    {
        if (_cooked != null)
        {
            return;
        }
        if (ragdoll != null)
        {
            Position = ragdoll.Position;
            if (Network.isActive)
            {
                ragdoll.Unragdoll();
            }
            else
            {
                Level.Remove(ragdoll);
            }
            vSpeed = -2f;
        }
        if (Network.isActive)
        {
            _cooked = _cookedInstance;
            if (_cookedInstance != null)
            {
                _cookedInstance.active = true;
                _cookedInstance.visible = true;
                _cookedInstance.solid = true;
                _cookedInstance.enablePhysics = true;
                _cookedInstance._sleeping = false;
                _cookedInstance.X = base.X;
                _cookedInstance.Y = base.Y;
                _cookedInstance.owner = null;
                Thing.ExtraFondle(_cookedInstance, DuckNetwork.localConnection);
                ReturnItemToWorld(_cooked);
                _cooked.vSpeed = vSpeed;
                _cooked.hSpeed = hSpeed;
            }
        }
        else
        {
            _cooked = new CookedDuck(base.X, base.Y);
            ReturnItemToWorld(_cooked);
            _cooked.vSpeed = vSpeed;
            _cooked.hSpeed = hSpeed;
            Level.Add(_cooked);
        }
        OnTeleport();
        SFX.Play("ignite", 1f, -0.3f + Rando.Float(0.3f));
        base.Y -= 25000f;
    }

    public void OnKill(DestroyType type = null)
    {
        if (!(type is DTPop))
        {
            SFX.Play("death");
            SFX.Play("pierce");
        }
        for (int i = 0; i < 8; i++)
        {
            Level.Add(Feather.New(cameraPosition.X, cameraPosition.Y, persona));
        }
        if (!(Level.current is ChallengeLevel))
        {
            Global.data.kills += 1;
        }
        _remoteControl = false;
        if (type is DTShot shot)
        {
            if (shot.bullet != null)
            {
                hSpeed = shot.bullet.travelDirNormalized.X * (shot.bullet.ammo.impactPower + 1f);
                vSpeed = shot.bullet.travelDirNormalized.Y * (shot.bullet.ammo.impactPower + 1f);
            }
            vSpeed -= 3f;
        }
        else if (type is DTIncinerate)
        {
            Cook();
        }
        else if (type is DTPop)
        {
            Disappear();
        }
    }

    public virtual void Netted(Net n)
    {
        if ((Network.isActive && (_trappedInstance == null || _trappedInstance.visible)) || _trapped != null)
        {
            return;
        }
        if (Network.isActive)
        {
            _trapped = _trappedInstance;
            _trappedInstance.active = true;
            _trappedInstance.visible = true;
            _trappedInstance.solid = true;
            _trappedInstance.enablePhysics = true;
            _trappedInstance.X = base.X;
            _trappedInstance.Y = base.Y;
            _trappedInstance.owner = null;
            _trappedInstance.InitializeStuff();
            n.Fondle(_trappedInstance);
            n.Fondle(this);
            if (_trappedInstance._duckOwner != null)
            {
                RumbleManager.AddRumbleEvent(_trappedInstance._duckOwner.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.None));
            }
        }
        else
        {
            RumbleManager.AddRumbleEvent(profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.None));
            _trapped = new TrappedDuck(base.X, base.Y, this);
            Level.Add(_trapped);
        }
        ReturnItemToWorld(_trapped);
        OnTeleport();
        if (holdObject != null)
        {
            n.Fondle(holdObject);
        }
        ThrowItem(throwWithForce: false);
        Level.Remove(n);
        profile.stats.timesNetted++;
        _trapped.clip.Add(this);
        _trapped.clip.Add(n);
        _trapped.hSpeed = hSpeed + n.hSpeed * 0.4f;
        _trapped.vSpeed = vSpeed + n.vSpeed - 1f;
        if (_trapped.hSpeed > 6f)
        {
            _trapped.hSpeed = 6f;
        }
        if (_trapped.hSpeed < -6f)
        {
            _trapped.hSpeed = -6f;
        }
        if (n.onFire)
        {
            Burn(n.Position, n);
        }
        if (n.responsibleProfile != null && n.responsibleProfile.duck != null)
        {
            trappedBy = n.responsibleProfile;
            n.responsibleProfile.duck.AddCoolness(1);
            Event.Log(new NettedEvent(n.responsibleProfile, profile));
        }
    }

    public void Breath()
    {
        Vec2 off = Offset(new Vec2(6f, 0f));
        if (ragdoll != null && ragdoll.part1 != null)
        {
            off = ragdoll.part1.Offset(new Vec2(6f, 0f));
        }
        else if (_trapped != null)
        {
            off = _trapped.Offset(new Vec2(8f, -2f));
        }
        Level.Add(BreathSmoke.New(off.X, off.Y));
        Level.Add(BreathSmoke.New(off.X, off.Y));
    }

    private void UpdateQuack()
    {
        if (dead || inputProfile == null || Level.current == null)
        {
            return;
        }
        if (breath <= 0)
        {
            if (breath == 0 && Level.current.cold && !underwater)
            {
                Breath();
            }
            breath = Rando.Int(70, 220);
        }
        breath--;
        if (inputProfile.Pressed("QUACK"))
        {
            float pitch = inputProfile.leftTrigger;
            if (inputProfile.hasMotionAxis)
            {
                pitch += inputProfile.motionAxis;
            }
            Hat h = GetEquipment(typeof(Hat)) as Hat;
            if (h == null || h.quacks)
            {
                if (Network.isActive)
                {
                    _netQuack.Play(1f, pitch);
                }
                else if (h != null)
                {
                    h.Quack(1f, pitch);
                }
                else
                {
                    _netQuack.Play(1f, pitch);
                }
            }
            if (base.isServerForObject)
            {
                Global.data.quacks.valueInt++;
            }
            profile.stats.quacks++;
            quack = 20;
        }
        if (!inputProfile.Down("QUACK"))
        {
            quack = Maths.CountDown(quack, 1);
        }
        if (inputProfile.Released("QUACK"))
        {
            quack = 0;
        }
    }

    public bool HasEquipment(Equipment t)
    {
        return HasEquipment(t.GetType());
    }

    public bool HasEquipment(Type t)
    {
        foreach (Equipment item in _equipment)
        {
            if (item.GetAllTypesFiltered(typeof(Equipment)).Contains(t))
            {
                return true;
            }
        }
        return false;
    }

    public void ObjectThrown(Holdable h)
    {
        h.enablePhysics = true;
        h.Thrown();
        h.owner = null;
        h.lastGrounded = DateTime.Now;
        h.solid = true;
        h.ReturnToWorld();
        ReturnItemToWorld(h);
    }

    public void ThrowItem(bool throwWithForce = true)
    {
        if (holdObject == null)
        {
            return;
        }
        if (_throwFondle)
        {
            Fondle(holdObject);
        }
        ObjectThrown(holdObject);
        holdObject.hSpeed = 0f;
        holdObject.vSpeed = 0f;
        holdObject.clip.Add(this);
        holdObstructed = false;
        if (holdObject is Mine && !(holdObject as Mine).pin && (!crouch || !base.grounded))
        {
            (holdObject as Mine).Arm();
        }
        if (!crouch)
        {
            float mult = 1f;
            float vMult = 1f;
            if (inputProfile.Down("LEFT") || inputProfile.Down("RIGHT"))
            {
                mult = 2.5f;
            }
            if (mult == 1f && inputProfile.Down("UP"))
            {
                holdObject.vSpeed -= 5f * holdWeightMultiplier;
            }
            else
            {
                mult *= holdWeightMultiplier;
                if (inputProfile.Down("UP"))
                {
                    vMult = 2f;
                }
                vMult *= holdWeightMultiplier;
                if (offDir > 0)
                {
                    holdObject.hSpeed += 3f * mult;
                }
                else
                {
                    holdObject.hSpeed -= 3f * mult;
                }
                if (reverseThrow)
                {
                    holdObject.hSpeed = 0f - holdObject.hSpeed;
                }
                holdObject.vSpeed -= 2f * vMult;
            }
        }
        if (Recorder.currentRecording != null)
        {
            Recorder.currentRecording.LogAction(2);
        }
        holdObject.hSpeed += 0.3f * (float)offDir;
        holdObject.hSpeed *= holdObject.throwSpeedMultiplier;
        if (!throwWithForce)
        {
            Holdable holdable = holdObject;
            float num = (holdObject.vSpeed = 0f);
            holdable.hSpeed = num;
        }
        else if (Network.isActive)
        {
            if (base.isServerForObject)
            {
                _netTinyMotion.Play();
            }
        }
        else
        {
            SFX.Play("tinyMotion");
        }
        _lastHoldItem = holdObject;
        _timeSinceThrow = 0;
        _holdObject = null;
    }

    public void GiveHoldable(Holdable h)
    {
        if (holdObject == h)
        {
            return;
        }
        if (holdObject != null)
        {
            ThrowItem(throwWithForce: false);
        }
        if (h == null)
        {
            return;
        }
        if (profile.localPlayer)
        {
            if (h is RagdollPart)
            {
                RagdollPart p = h as RagdollPart;
                if (p.doll != null)
                {
                    p.doll.connection = connection;
                    p.doll.authority += 8;
                }
            }
            else
            {
                h.connection = connection;
                h.authority += 8;
            }
        }
        holdObject = h;
        holdObject.owner = this;
        holdObject.solid = false;
        h.hSpeed = 0f;
        h.vSpeed = 0f;
        h.enablePhysics = false;
        h._sleeping = false;
    }

    private void TryGrab()
    {
        foreach (Holdable pickup in Level.CheckCircleAll<Holdable>(new Vec2(base.X, base.Y + 4f), 18f).OrderBy((Holdable h) => h, new CompareHoldablePriorities(this)))
        {
            if (pickup.owner != null || !pickup.canPickUp || (pickup == _lastHoldItem && _timeSinceThrow < 30) || !pickup.active || !pickup.visible || Level.CheckLine<Block>(Position, pickup.Position) != null)
            {
                continue;
            }
            GiveHoldable(pickup);
            if (holdObject.weight > 5f)
            {
                if (Rando.Float(1f) < 0.5f)
                {
                    PlaySFX("liftBarrel", 1f, Rando.Float(-0.1f, 0.2f));
                }
                else
                {
                    PlaySFX("liftBarrel2", 1f, Rando.Float(-0.1f, 0.2f));
                }
                quack = 10;
            }
            else if (Network.isActive)
            {
                if (base.isServerForObject)
                {
                    _netTinyMotion.Play();
                }
            }
            else
            {
                SFX.Play("tinyMotion");
            }
            if (holdObject.disarmedFrom != this && (DateTime.Now - holdObject.disarmTime).TotalSeconds < 0.5)
            {
                AddCoolness(2);
            }
            tryGrabFrames = 0;
            break;
        }
    }

    private void UpdateThrow()
    {
        if (!base.isServerForObject)
        {
            return;
        }
        bool pickedUpEquipment = false;
        if (CanMove())
        {
            if (HasEquipment(typeof(Holster)) && inputProfile.Down("UP") && inputProfile.Pressed("GRAB") && (holdObject == null || holdObject.holsterable) && GetEquipment(typeof(Holster)) is Holster h && (!h.chained.value || h.containedObject == null))
            {
                Holdable swap = null;
                bool didAction = false;
                if (h.containedObject != null)
                {
                    swap = h.containedObject;
                    h.SetContainedObject(null);
                    ObjectThrown(swap);
                    didAction = true;
                }
                if (holdObject != null)
                {
                    if (holdObject is RagdollPart)
                    {
                        RagdollPart p = holdObject as RagdollPart;
                        if (p.doll != null && p.doll.part3 != null)
                        {
                            holdObject.owner = null;
                            holdObject = p.doll.part3;
                        }
                    }
                    holdObject.owner = this;
                    h.SetContainedObject(holdObject);
                    if (h.chained.value)
                    {
                        SFX.PlaySynchronized("equip");
                        for (int i = 0; i > 3; i++)
                        {
                            Level.Add(SmallSmoke.New(holdObject.X + Rando.Float(-3f, 3f), holdObject.Y + Rando.Float(-3f, 3f)));
                        }
                    }
                    holdObject = null;
                    didAction = true;
                }
                if (swap != null)
                {
                    GiveHoldable(swap);
                    if (Network.isActive)
                    {
                        if (base.isServerForObject)
                        {
                            _netTinyMotion.Play();
                        }
                    }
                    else
                    {
                        SFX.Play("tinyMotion");
                    }
                }
                if (didAction)
                {
                    return;
                }
            }
            if (holdObject != null && inputProfile.Pressed("GRAB"))
            {
                doThrow = true;
            }
            if (!_isGhost && inputProfile.Pressed("GRAB") && holdObject == null)
            {
                tryGrabFrames = 2;
                TryGrab();
            }
        }
        if (!pickedUpEquipment && doThrow && holdObject != null)
        {
            _ = holdObject;
            doThrow = false;
            ThrowItem();
        }
    }

    private void UpdateAnimation()
    {
        _updatedAnimation = true;
        if (_hovering)
        {
            _flapFrame++;
            if (_flapFrame > 8)
            {
                _flapFrame = 0;
            }
        }
        UpdateCurrentAnimation();
    }

    private void UpdateCurrentAnimation()
    {
        if (dead && _eyesClosed)
        {
            _sprite.currentAnimation = "dead";
        }
        else if (inNet)
        {
            _sprite.currentAnimation = "netted";
        }
        else if (listening)
        {
            _sprite.currentAnimation = "listening";
        }
        else if (crouch)
        {
            _sprite.currentAnimation = "crouch";
            if (sliding)
            {
                _sprite.currentAnimation = "groundSlide";
            }
        }
        else if (base.grounded)
        {
            if (hSpeed > 0f && !_gripped)
            {
                _sprite.currentAnimation = "run";
                if (!strafing && Math.Sign(offDir) != Math.Sign(hSpeed))
                {
                    _sprite.currentAnimation = "slide";
                }
            }
            else if (hSpeed < 0f && !_gripped)
            {
                _sprite.currentAnimation = "run";
                if (!strafing && Math.Sign(offDir) != Math.Sign(hSpeed))
                {
                    _sprite.currentAnimation = "slide";
                }
            }
            else
            {
                _sprite.currentAnimation = "idle";
            }
        }
        else
        {
            _sprite.currentAnimation = "jump";
            _sprite.speed = 0f;
            if (vSpeed < 0f && !_hovering)
            {
                _sprite.frame = 0;
            }
            else
            {
                _sprite.frame = 2;
            }
        }
    }

    private void UpdateBurning()
    {
        burnSpeed = 0.005f;
        if (base.onFire && !dead)
        {
            if (_flameWait > 1f)
            {
                RumbleManager.AddRumbleEvent(profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Short, RumbleFalloff.Short));
            }
            profile.stats.timeOnFire += Maths.IncFrameTimer();
            if (base.wallCollideLeft != null)
            {
                offDir = 1;
            }
            else if (base.wallCollideRight != null)
            {
                offDir = -1;
            }
        }
        else if (!base.onFire && !dead)
        {
            burnt -= 0.005f;
            if (burnt < 0f)
            {
                burnt = 0f;
            }
        }
    }

    public override void Extinquish()
    {
        if (_trapped != null)
        {
            _trapped.Extinquish();
        }
        if (_ragdollInstance != null)
        {
            _ragdollInstance.Extinguish();
        }
        base.Extinquish();
    }

    public void ResetNonServerDeathState()
    {
        _isGhost = false;
        _killed = false;
        forceDead = false;
        unfocus = 1f;
        unfocused = false;
        active = true;
        solid = true;
        beammode = false;
        immobilized = false;
        gravMultiplier = 1f;
        if (Level.current is TeamSelect2 && (Level.current as TeamSelect2)._beam != null && (Level.current as TeamSelect2)._beam2 != null)
        {
            (Level.current as TeamSelect2)._beam.RemoveDuck(this);
            (Level.current as TeamSelect2)._beam2.RemoveDuck(this);
        }
    }

    public void Ressurect()
    {
        dead = false;
        if (ragdoll != null)
        {
            ragdoll.Unragdoll();
        }
        ResetNonServerDeathState();
        Regenerate();
        crouch = false;
        sliding = false;
        burnt = 0f;
        _onFire = false;
        hSpeed = 0f;
        vSpeed = 0f;
        if (Level.current.camera is FollowCam)
        {
            (Level.current.camera as FollowCam).Add(this);
        }
        _cooked = null;
        ResurrectEffect(Position);
        vSpeed = -3f;
        if (Network.isActive && base.isServerForObject)
        {
            Thing.SuperFondle(_cookedInstance, DuckNetwork.localConnection);
            _cookedInstance.visible = false;
            _cookedInstance.active = false;
            lastAppliedLifeChange++;
            Send.Message(new NMRessurect(Position, this, lastAppliedLifeChange));
        }
    }

    public static void ResurrectEffect(Vec2 pPosition)
    {
        for (int i = 0; i < 6; i++)
        {
            Level.Add(new CampingSmoke(pPosition.X - 5f + Rando.Float(10f), pPosition.Y + 6f - 3f + Rando.Float(6f) - (float)i * 1f)
            {
                move =
                {
                    X = -0.3f + Rando.Float(0.6f),
                    Y = -1.8f + Rando.Float(0.8f)
                }
            });
        }
    }

    private void UpdateGhostStatus()
    {
        GhostPack pack = GetEquipment(typeof(GhostPack)) as GhostPack;
        if (pack != null && !_isGhost)
        {
            _equipment.Remove(pack);
            Level.Remove(pack);
        }
        else if (pack == null && _isGhost)
        {
            pack = new GhostPack(0f, 0f);
            _equipment.Add(pack);
            pack.Equip(this);
            Level.Add(pack);
        }
        if (_isGhost)
        {
            _ghostTimer -= 0.023f;
            if (_ghostTimer < 0f)
            {
                _ghostTimer = 1f;
                _isGhost = false;
                Ressurect();
            }
        }
    }

    public void Swear()
    {
        if (Network.isActive)
        {
            if (base.isServerForObject)
            {
                _netSwear.Play();
            }
        }
        else
        {
            float extraChance = 0f;
            if (profile.team != null && profile.team.name == "Sailors")
            {
                extraChance += 0.1f;
            }
            if (Rando.Float(1f) < 0.03f + profile.funslider * 0.045f + extraChance)
            {
                SFX.Play("quackBleep", 0.8f, Rando.Float(-0.05f, 0.05f));
                Event.Log(new SwearingEvent(profile, profile));
            }
            else if (Rando.Float(1f) < 0.5f)
            {
                SFX.Play("cutOffQuack", 1f, Rando.Float(-0.05f, 0.05f));
            }
            else
            {
                SFX.Play("cutOffQuack2", 1f, Rando.Float(-0.05f, 0.05f));
            }
        }
        quack = 10;
    }

    public void Scream()
    {
        if (Network.isActive)
        {
            if (base.isServerForObject)
            {
                _netScream.Play();
            }
        }
        else if (Rando.Float(1f) < 0.03f + profile.funslider * 0.045f)
        {
            SFX.Play("quackBleep", 0.9f);
            Event.Log(new SwearingEvent(profile, profile));
        }
        else if (Rando.Float(1f) < 0.5f)
        {
            SFX.Play("quackYell03");
        }
        else if (Rando.Float(1f) < 0.5f)
        {
            SFX.Play("quackYell02");
        }
        else
        {
            SFX.Play("quackYell01");
        }
        quack = 10;
    }

    public void Disarm(Thing disarmedBy)
    {
        if (!base.isServerForObject)
        {
            return;
        }
        if (holdObject != null && (!Network.isActive || (disarmedBy != null && disarmedBy.isServerForObject)))
        {
            Global.data.disarms.valueInt++;
        }
        Profile responsible = disarmedBy?.responsibleProfile;
        if (responsible != null && holdObject != null)
        {
            disarmIndex = responsible.networkIndex;
            disarmIndexCooldown = 0.5f;
        }
        else
        {
            disarmIndex = 9;
            disarmIndexCooldown = 0.5f;
        }
        _disarmedBy = responsible;
        _disarmedAt = DateTime.Now;
        _holdingAtDisarm = holdObject;
        if (holdObject != null)
        {
            Fondle(holdObject);
            holdObject.disarmedFrom = this;
            holdObject.disarmTime = DateTime.Now;
            if (Network.isActive)
            {
                if (base.isServerForObject)
                {
                    _netDisarm.Play();
                }
            }
            else
            {
                SFX.Play("disarm", 0.3f, Rando.Float(0.2f, 0.4f));
            }
        }
        Event.Log(new DisarmEvent(responsible, profile));
        ThrowItem();
        Swear();
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        Holdable hold = with as Holdable;
        if (_isGhost || with == null || (hold != null && hold.owner == this) || with is FeatherVolume || ((with == _lastHoldItem || (with.owner != null && with.owner == _lastHoldItem)) && _timeSinceThrow < 7) || with == _trapped || with == _trappedInstance || with is Dart || (with.owner != null && with.owner is SpikeHelm))
        {
            return;
        }
        if (with is IceWedge)
        {
            _iceWedging = 5;
        }
        if (with is RagdollPart)
        {
            RagdollPart part = with as RagdollPart;
            if ((part != null && part.doll != null && part.doll.captureDuck != null && part.doll.captureDuck.killedByProfile == profile && part.doll.captureDuck.framesSinceKilled < 50) || (part != null && part.doll != null && (part.doll.PartHeld() || (holdObject is Chainsaw && _timeSinceChainKill < 50))) || (holdObject != null && holdObject is RagdollPart && part != null && part.doll != null && part.doll.holdingOwner == this) || (ragdoll != null && (with == ragdoll.part1 || with == ragdoll.part2 || with == ragdoll.part3)) || part.doll == null || part.doll.captureDuck == this || (_timeSinceThrow < 15 && part.doll != null && (part.doll.part1 == _lastHoldItem || part.doll.part2 == _lastHoldItem || part.doll.part3 == _lastHoldItem)))
            {
                return;
            }
        }
        if (dead || swinging || !(with is PhysicsObject) || !(with.totalImpactPower > with.weightMultiplierInv * 2f))
        {
            return;
        }
        if (with is Duck && with.weight >= 5f)
        {
            Duck d = with as Duck;
            bool bootsmash = d.HasEquipment(typeof(Boots)) && !d.sliding;
            if (from != ImpactedFrom.Top || !(with.bottom - 5f < base.top) || !(with.impactPowerV > 2f) || !bootsmash)
            {
                return;
            }
            vSpeed = with.impactDirectionV * 0.5f;
            with.vSpeed = (0f - with.vSpeed) * 0.7f;
            d._groundValid = 7;
            d.slamWait = 6;
            if (with.isServerForObject)
            {
                RumbleManager.AddRumbleEvent(d.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.None));
                MakeStars(Position + new Vec2(0f, (crouch || ragdoll != null) ? (-2) : (-6)), base.velocity);
                if (Network.isActive)
                {
                    Send.Message(new NMBonk(Position + new Vec2(0f, (crouch || ragdoll != null) ? (-2) : (-6)), base.velocity));
                }
            }
            if (GetEquipment(typeof(Helmet)) is Helmet h)
            {
                SFX.Play("metalRebound");
                RumbleManager.AddRumbleEvent(profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Short, RumbleFalloff.None));
                h.Crush(d);
            }
            else if (with.isServerForObject)
            {
                Global.data.ducksCrushed.valueInt++;
                Kill(new DTCrush(with as PhysicsObject));
            }
        }
        else if (with.dontCrush)
        {
            if (!(with.Alpha > 0.99f) || (from != ImpactedFrom.Left && from != ImpactedFrom.Right) || ((Network.isActive || !(with.impactPowerH > 2.3f)) && !(with.impactPowerH > 3f)))
            {
                return;
            }
            bool processDisarm = with.isServerForObject;
            if (!processDisarm && Level.CheckLine<Block>(Position, with.Position) != null)
            {
                processDisarm = true;
            }
            if (processDisarm)
            {
                hSpeed = with.impactDirectionH * 0.5f;
                if (!(with is EnergyScimitar))
                {
                    with.hSpeed = (0f - with.hSpeed) * with.bouncy;
                }
                if (base.isServerForObject && (!Network.isActive || _disarmWait == 0) && _disarmDisable <= 0)
                {
                    Disarm(with);
                    _disarmWait = 5;
                    RumbleManager.AddRumbleEvent(profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Short, RumbleFalloff.None));
                }
                if (!base.isServerForObject)
                {
                    Send.Message(new NMDisarm(this, with.impactDirectionH * 0.5f), connection);
                }
            }
        }
        else if (from == ImpactedFrom.Top && with.Y < base.Y && with.vSpeed > 0f && with.impactPowerV > 2f && with.weight >= 5f)
        {
            if (!(with is PhysicsObject))
            {
                return;
            }
            PhysicsObject wp = with as PhysicsObject;
            if (!(wp.lastPosition.Y + with.collisionOffset.Y + with.collisionSize.Y < base.top))
            {
                return;
            }
            Helmet h2 = GetEquipment(typeof(Helmet)) as Helmet;
            if (h2 != null && h2 is SpikeHelm && wp == (h2 as SpikeHelm).oldPoke)
            {
                return;
            }
            vSpeed = with.impactDirectionV * 0.5f;
            with.vSpeed = (0f - with.vSpeed) * 0.5f;
            if (with.isServerForObject)
            {
                MakeStars(Position + new Vec2(0f, (crouch || ragdoll != null) ? (-2) : (-6)), base.velocity);
                if (Network.isActive)
                {
                    Send.Message(new NMBonk(Position + new Vec2(0f, (crouch || ragdoll != null) ? (-2) : (-6)), base.velocity));
                }
            }
            if (h2 != null && ragdoll == null)
            {
                SFX.Play("metalRebound");
                RumbleManager.AddRumbleEvent(profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Short, RumbleFalloff.None));
                h2.Crush(wp);
            }
            else if (with.isServerForObject)
            {
                Kill(new DTCrush(with as PhysicsObject));
                Global.data.ducksCrushed.valueInt++;
            }
        }
        else if ((from == ImpactedFrom.Left || from == ImpactedFrom.Right) && ((!Network.isActive && with.impactPowerH > 2f) || with.impactPowerH > 3f))
        {
            if ((holdObject is SledgeHammer && with is RagdollPart) || (holdObject is Sword && (holdObject as Sword).crouchStance && ((offDir < 0 && from == ImpactedFrom.Left) || (offDir > 0 && from == ImpactedFrom.Right))))
            {
                return;
            }
            bool processDisarm2 = with.isServerForObject;
            if (!processDisarm2 && Level.CheckLine<Block>(Position, with.Position) != null)
            {
                processDisarm2 = true;
            }
            if (!processDisarm2)
            {
                return;
            }
            with.hSpeed = (0f - with.hSpeed) * with.bouncy;
            if (!(with is TeamHat))
            {
                hSpeed = with.impactDirectionH * 0.5f;
                if (base.isServerForObject && (!Network.isActive || _disarmWait == 0) && _disarmDisable <= 0)
                {
                    Disarm(with);
                    RumbleManager.AddRumbleEvent(profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Short, RumbleFalloff.None));
                    _disarmWait = 5;
                }
                if (!base.isServerForObject)
                {
                    Send.Message(new NMDisarm(this, with.impactDirectionH * 0.5f), connection);
                }
            }
            else
            {
                Swear();
            }
        }
        else if (!(with is TeamHat) && from == ImpactedFrom.Bottom && with.Y > base.bottom && with.impactPowerV > 2f)
        {
            vSpeed = with.impactDirectionV * 0.5f;
            with.vSpeed = (0f - with.vSpeed) * 0.5f;
        }
    }

    public override void OnTeleport()
    {
        if (holdObject != null)
        {
            holdObject.OnTeleport();
        }
        foreach (Equipment item in _equipment)
        {
            item.OnTeleport();
        }
        if (_vine != null)
        {
            _vine.Degrapple();
            _vine = null;
        }
    }

    public void AdvanceServerTime(int frames)
    {
        while (frames > 0)
        {
            frames--;
            clientFrame++;
            Update();
        }
    }

    public override void Initialize()
    {
        jumpSpeed = JumpSpeed;
        if (Level.current != null)
        {
            if (base.isServerForObject)
            {
                foreach (Equipper e in Level.current.things[typeof(Equipper)])
                {
                    if (e.radius.value != 0 && !((Position - e.Position).Length() <= (float)e.radius.value))
                    {
                        continue;
                    }
                    Thing t = e.GetContainedInstance(Position);
                    if (t == null)
                    {
                        continue;
                    }
                    Level.Add(t);
                    if (t is Holdable)
                    {
                        (t as Holdable).UpdateMaterial();
                    }
                    if ((bool)e.holstered || (bool)e.powerHolstered)
                    {
                        Holster h = null;
                        h = ((!e.powerHolstered) ? new Holster(Position.X, Position.Y) : new PowerHolster(Position.X, Position.Y));
                        Level.Add(h);
                        h.SetContainedObject(t as Holdable);
                        Equip(h);
                        h.chained = e.holsterChained;
                    }
                    else if (t is Equipment && (t as Equipment).wearable)
                    {
                        if (t is Hat && GetEquipment(typeof(Hat)) is Hat existing)
                        {
                            Unequip(existing);
                            existing.Position = Position;
                            existing.vSpeed = 0f;
                            existing.hSpeed = 0f;
                        }
                        Equip(t as Equipment);
                    }
                    else if (t is Holdable)
                    {
                        if (holdObject != null)
                        {
                            Holdable holdable = holdObject;
                            ThrowItem(throwWithForce: false);
                            holdable.Position = Position;
                            holdable.vSpeed = 0f;
                            holdable.hSpeed = 0f;
                        }
                        GiveHoldable(t as Holdable);
                    }
                }
            }
            Level.Add(_featherVolume);
        }
        if (Network.isServer)
        {
            _netProfileIndex = (byte)DuckNetwork.IndexOf(profile);
        }
        if (Network.isActive)
        {
            _netQuack.pitchBinding = new FieldBinding(this, "quackPitch");
        }
        base.Initialize();
    }

    public override void NetworkUpdate()
    {
    }

    public void GoRagdoll()
    {
        if ((Network.isActive && (_ragdollInstance == null || (_ragdollInstance != null && _ragdollInstance.visible) || (_cookedInstance != null && _cookedInstance.visible))) || ragdoll != null || _cooked != null)
        {
            return;
        }
        _hovering = false;
        float ragY = base.Y + 4f;
        float ragDegrees = 0f;
        if (sliding)
        {
            ragY += 6f;
            ragDegrees = ((offDir >= 0) ? 0f : 180f);
        }
        else
        {
            ragDegrees = -90f;
        }
        Vec2 vel = new Vec2(_hSpeed, _vSpeed);
        hSpeed = 0f;
        vSpeed = 0f;
        if (Network.isActive)
        {
            ragdoll = _ragdollInstance;
            _ragdollInstance.active = true;
            _ragdollInstance.visible = true;
            _ragdollInstance.solid = true;
            _ragdollInstance.enablePhysics = true;
            _ragdollInstance.X = base.X;
            _ragdollInstance.Y = base.Y;
            _ragdollInstance.owner = null;
            _ragdollInstance.npi = netProfileIndex;
            _ragdollInstance.SortOutParts(base.X, ragY, this, sliding, ragDegrees, offDir, vel);
            Fondle(_ragdollInstance);
        }
        else
        {
            ragdoll = new Ragdoll(base.X, ragY, this, sliding, ragDegrees, offDir, vel);
            Level.Add(ragdoll);
            ragdoll.RunInit();
        }
        if (ragdoll == null)
        {
            return;
        }
        ragdoll.connection = connection;
        ragdoll.part1.connection = connection;
        ragdoll.part2.connection = connection;
        ragdoll.part3.connection = connection;
        if (!fancyShoes)
        {
            Equipment hat = GetEquipment(typeof(Hat));
            if (hat != null && !(hat as Hat).strappedOn)
            {
                Unequip(hat);
                hat.hSpeed = hSpeed * 1.2f;
                hat.vSpeed = vSpeed - 2f;
            }
            ThrowItem(throwWithForce: false);
        }
        OnTeleport();
        if (base.Y > -4000f)
        {
            base.Y -= 5000f;
        }
        sliding = false;
        crouch = false;
    }

    public virtual void UpdateSkeleton()
    {
        Vec2 pos = Position;
        if (_trapped != null)
        {
            base.X = _trapped.X;
            base.Y = _trapped.Y;
        }
        if (ragdoll != null)
        {
            if (ragdoll.part1 != null && ragdoll.part3 != null)
            {
                _skeleton.upperTorso.position = ragdoll.part1.Offset(new Vec2(0f, 7f));
                _skeleton.upperTorso.orientation = ((ragdoll.part1.offDir > 0) ? (0f - ragdoll.part1.Angle) : ragdoll.part1.Angle);
                _skeleton.lowerTorso.position = ragdoll.part3.Offset(new Vec2(5f, 11f));
                _skeleton.lowerTorso.orientation = ((ragdoll.part3.offDir > 0) ? (0f - ragdoll.part3.Angle) : ragdoll.part3.Angle) + Maths.DegToRad(180f);
                _skeleton.head.position = ragdoll.part1.Offset(new Vec2(-2f, -6f));
                _skeleton.head.orientation = ((ragdoll.part1.offDir > 0) ? (0f - ragdoll.part1.Angle) : ragdoll.part1.Angle);
            }
        }
        else if (_sprite != null)
        {
            _skeleton.head.position = Offset(DuckRig.GetHatPoint(_sprite.imageIndex)) + new Vec2(0f, verticalOffset);
            _skeleton.upperTorso.position = Offset(DuckRig.GetChestPoint(_sprite.imageIndex)) + new Vec2(0f, verticalOffset);
            _skeleton.lowerTorso.position = Position + new Vec2(0f, verticalOffset);
            if (sliding)
            {
                _skeleton.head.orientation = Maths.DegToRad(90f);
                _skeleton.upperTorso.orientation = Maths.DegToRad(90f);
                _skeleton.lowerTorso.orientation = 0f;
            }
            else
            {
                float ang = ((offDir < 0) ? Angle : (0f - Angle));
                _skeleton.head.orientation = ang;
                _skeleton.upperTorso.orientation = ang;
                _skeleton.lowerTorso.orientation = ang;
            }
            if (_trapped != null)
            {
                _skeleton.head.orientation = 0f;
                _skeleton.upperTorso.orientation = 0f;
                _skeleton.lowerTorso.orientation = 0f;
                _skeleton.head.position = Offset(new Vec2(-1f, -10f));
                _skeleton.upperTorso.position = Offset(new Vec2(1f, 2f));
                _skeleton.lowerTorso.position = Offset(new Vec2(0f, -8f));
            }
        }
        Position = pos;
    }

    public bool HasTV()
    {
        if (tvHeld == null && tvHolster == null)
        {
            return tvHat != null;
        }
        return true;
    }

    public int HasTVs(bool pChannel)
    {
        int num = 0;
        if (tvHeld != null && tvHeld.channel == pChannel)
        {
            num++;
        }
        if (tvHolster != null && tvHolster.channel == pChannel)
        {
            num++;
        }
        if (tvHat != null && tvHat.channel == pChannel)
        {
            num++;
        }
        return num;
    }

    public bool CheckTVChannel(bool pChannel)
    {
        if (tvHeld != null && tvHeld.channel == pChannel)
        {
            return true;
        }
        if (tvHolster != null && tvHolster.channel == pChannel)
        {
            return true;
        }
        if (tvHat != null && tvHat.channel == pChannel)
        {
            return true;
        }
        return false;
    }

    public bool CheckTVJump()
    {
        if (pipeOut > 0)
        {
            return false;
        }
        bool tvJump = false;
        if (tvHeld != null && tvHeld.channel && tvHeld.jumpReady)
        {
            tvHeld.jumpReady = false;
            tvJump = true;
        }
        else if (tvHolster != null && tvHolster.channel && tvHolster.jumpReady)
        {
            tvHolster.jumpReady = false;
            tvJump = true;
        }
        else if (tvHat != null && tvHat.channel && tvHat.jumpReady)
        {
            tvHat.jumpReady = false;
            tvJump = true;
        }
        if (tvJump)
        {
            Level.Add(new ColorStar(base.X + hSpeed * 2f, base.Y + 4f, new Vec2(-1.5f, -2.5f) + new Vec2((hSpeed + Rando.Float(-0.5f, 0.5f)) * Rando.Float(0.6f, 0.9f) / 2f, Rando.Float(-0.5f, 0f)), new Color(237, 94, 238)));
            Level.Add(new ColorStar(base.X + hSpeed * 2f, base.Y + 4f, new Vec2(-0.9f, -1.5f) + new Vec2((hSpeed + Rando.Float(-0.5f, 0.5f)) * Rando.Float(0.6f, 0.9f) / 2f, Rando.Float(-0.5f, 0f)), new Color(49, 162, 242)));
            Level.Add(new ColorStar(base.X + hSpeed * 2f, base.Y + 4f, new Vec2(0.9f, -1.5f) + new Vec2((hSpeed + Rando.Float(-0.5f, 0.5f)) * Rando.Float(0.6f, 0.9f) / 2f, Rando.Float(-0.5f, 0f)), new Color(247, 224, 90)));
            Level.Add(new ColorStar(base.X + hSpeed * 2f, base.Y + 4f, new Vec2(1.5f, -2.5f) + new Vec2((hSpeed + Rando.Float(-0.5f, 0.5f)) * Rando.Float(0.6f, 0.9f) / 2f, Rando.Float(-0.5f, 0f)), new Color(192, 32, 45)));
        }
        return tvJump;
    }

    public void UpdateMove()
    {
        if (inputProfile == null)
        {
            return;
        }
        if (pipeOut > 0)
        {
            pipeOut--;
            if (pipeOut == 2 && !inputProfile.Down("JUMP"))
            {
                pipeOut = 0;
            }
            else
            {
                vSpeed -= 0.5f;
            }
        }
        if (pipeBoost > 0)
        {
            pipeBoost--;
        }
        if (slamWait > 0)
        {
            slamWait--;
        }
        tvJumped = false;
        _timeSinceChainKill++;
        weight = 5.3f;
        if (holdObject != null)
        {
            weight += Math.Max(0f, holdObject.weight - 5f);
            if (holdObject.destroyed)
            {
                ThrowItem();
            }
        }
        if (base.isServerForObject)
        {
            UpdateQuack();
        }
        if (hat != null)
        {
            hat.SetQuack((quack > 0 || (_mindControl != null && _derpMindControl)) ? 1 : 0);
        }
        if (_ragdollInstance != null && _ragdollInstance.part2 != null)
        {
            _ragdollInstance.part2.UpdateLastReasonablePosition(Position);
        }
        if (inNet && _trapped != null)
        {
            base.X = _trapped.X;
            base.Y = _trapped.Y;
            if (_ragdollInstance != null && _ragdollInstance.part2 != null)
            {
                _ragdollInstance.part2.UpdateLastReasonablePosition(Position);
            }
            owner = _trapped;
            ThrowItem(throwWithForce: false);
            return;
        }
        owner = null;
        skipPlatFrames = Maths.CountDown(skipPlatFrames, 1);
        crippleTimer = Maths.CountDown(crippleTimer, 0.1f);
        if (inputProfile.Pressed("JUMP"))
        {
            _jumpValid = 4;
            if (!base.grounded && crouch)
            {
                skipPlatFrames = 10;
            }
        }
        else
        {
            _jumpValid = Maths.CountDown(_jumpValid, 1);
        }
        _skipPlatforms = false;
        if (inputProfile.Down("DOWN") && skipPlatFrames > 0)
        {
            _skipPlatforms = true;
        }
        bool hasGround = base.grounded;
        if (!hasGround && HasEquipment(typeof(ChokeCollar)))
        {
            ChokeCollar collar = GetEquipment(typeof(ChokeCollar)) as ChokeCollar;
            if (collar.ball.grounded && collar.ball.bottom < base.top && vSpeed > -1f)
            {
                hasGround = true;
            }
        }
        if (hasGround)
        {
            framesSinceJump = 0;
            _groundValid = 7;
            _hovering = false;
            _double = false;
        }
        else
        {
            _groundValid = Maths.CountDown(_groundValid, 1);
            framesSinceJump++;
        }
        if (mindControl != null)
        {
            mindControl.UpdateExtraInput();
        }
        _heldLeft = false;
        _heldRight = false;
        Block lockBlock = Level.CheckRect<Block>(new Vec2(base.X - 3f, base.Y - 9f), new Vec2(base.X + 3f, base.Y + 4f));
        _crouchLock = (crouch || sliding) && (lockBlock?.solid ?? false);
        float hAcc = 0.55f * holdWeightMultiplier * grappleMultiplier * accelerationMultiplier;
        maxrun = _runMax * holdWeightMultiplier;
        if (_isGhost)
        {
            hAcc *= 1.4f;
            maxrun *= 1.5f;
        }
        int numSpeedTVs = HasTVs(pChannel: false);
        for (int i = 0; i < numSpeedTVs; i++)
        {
            hAcc *= 1.4f;
            maxrun *= 1.5f;
        }
        if (holdObject is EnergyScimitar && (holdObject as EnergyScimitar).dragSpeedBonus)
        {
            if ((holdObject as EnergyScimitar)._spikeDrag)
            {
                hAcc *= 0.5f;
                maxrun *= 0.5f;
            }
            else
            {
                hAcc *= 1.3f;
                maxrun *= 1.35f;
            }
        }
        if (specialFrictionMod > 0f)
        {
            hAcc *= Math.Min(specialFrictionMod * 2f, 1f);
        }
        if (base.isServerForObject && base.isOffBottomOfLevel && !dead)
        {
            if (ragdoll != null && ragdoll.part1 != null && ragdoll.part2 != null && ragdoll.part3 != null)
            {
                ragdoll.part1.Y += 500f;
                ragdoll.part2.Y += 500f;
                ragdoll.part3.Y += 500f;
            }
            base.Y += 500f;
            Kill(new DTFall());
            profile.stats.fallDeaths++;
        }
        if (Network.isActive && ragdoll != null && ragdoll.connection != DuckNetwork.localConnection && ragdoll.TryingToControl() && !ragdoll.PartHeld())
        {
            Fondle(ragdoll);
        }
        if (CanMove())
        {
            if (!_grounded)
            {
                profile.stats.airTime += Maths.IncFrameTimer();
            }
            if (base.isServerForObject && !sliding && inputProfile.Pressed("UP"))
            {
                Desk d = Level.Nearest<Desk>(Position);
                if (d != null && (d.Position - Position).Length() < 22f && Level.CheckLine<Block>(Position, d.Position) == null)
                {
                    Fondle(d);
                    d.Flip(offDir < 0);
                }
            }
            float leftMul = 0f;
            float rightMul = 0f;
            if (inputProfile.Down("LEFT"))
            {
                leftMul = 1f;
                if (_leftPressedFrame == 0)
                {
                    _leftPressedFrame = (int)Graphics.frame;
                }
            }
            else
            {
                leftMul = Maths.NormalizeSection(Math.Abs(Math.Min(inputProfile.leftStick.X, 0f)), 0.2f, 0.9f);
                if (leftMul > 0.01f)
                {
                    if (_leftPressedFrame == 0)
                    {
                        _leftPressedFrame = (int)Graphics.frame;
                    }
                }
                else
                {
                    _leftPressedFrame = 0;
                }
            }
            if (inputProfile.Down("RIGHT"))
            {
                rightMul = 1f;
                if (_rightPressedFrame == 0)
                {
                    _rightPressedFrame = (int)Graphics.frame;
                }
            }
            else
            {
                rightMul = Maths.NormalizeSection(Math.Max(inputProfile.leftStick.X, 0f), 0.2f, 0.9f);
                if (rightMul > 0.01f)
                {
                    if (_rightPressedFrame == 0)
                    {
                        _rightPressedFrame = (int)Graphics.frame;
                    }
                }
                else
                {
                    _rightPressedFrame = 0;
                }
            }
            bool oldAngleCode = Options.Data.oldAngleCode;
            if (!base.isServerForObject && inputProfile != null)
            {
                oldAngleCode = inputProfile.oldAngles;
            }
            if (leftMul < 0.01f && base.onFire && offDir == 1)
            {
                rightMul = 1f;
            }
            if (rightMul < 0.01f && base.onFire && offDir == -1)
            {
                leftMul = 1f;
            }
            if (grappleMul)
            {
                leftMul *= 1.5f;
                rightMul *= 1.5f;
            }
            if (DevConsole.qwopMode && Level.current is GameLevel)
            {
                if (leftMul > 0f)
                {
                    offDir = -1;
                }
                else if (rightMul > 0f)
                {
                    offDir = 1;
                }
                if (_walkTime == 0)
                {
                    rightMul = (leftMul = 0f);
                }
                else
                {
                    if (offDir < 0)
                    {
                        leftMul = 1f;
                    }
                    else
                    {
                        rightMul = 1f;
                    }
                    _walkTime--;
                }
                if (_walkCount > 0)
                {
                    _walkCount--;
                }
                if (inputProfile.Pressed("LTRIGGER"))
                {
                    if (_walkCount > 0 && _nextTrigger)
                    {
                        GoRagdoll();
                        _walkCount = 0;
                    }
                    else
                    {
                        _walkCount += 20;
                        if (DevConsole.rhythmMode && Level.current is GameLevel)
                        {
                            _walkTime += 20;
                        }
                        else
                        {
                            _walkTime += 8;
                        }
                        if (_walkTime > 20)
                        {
                            _walkTime = 20;
                        }
                        if (_walkCount > 40)
                        {
                            _walkCount = 40;
                        }
                        _nextTrigger = true;
                    }
                }
                else if (inputProfile.Pressed("RTRIGGER"))
                {
                    if (_walkCount > 0 && !_nextTrigger)
                    {
                        GoRagdoll();
                        _walkCount = 0;
                    }
                    else
                    {
                        _walkCount += 20;
                        if (DevConsole.rhythmMode && Level.current is GameLevel)
                        {
                            _walkTime += 20;
                        }
                        else
                        {
                            _walkTime += 8;
                        }
                        if (_walkTime > 20)
                        {
                            _walkTime = 20;
                        }
                        if (_walkCount > 40)
                        {
                            _walkCount = 40;
                        }
                        _nextTrigger = false;
                    }
                }
            }
            bool nudging = _crouchLock && base.grounded && inputProfile.Pressed("ANY");
            if (nudging && offDir == -1)
            {
                leftMul = 1f;
                rightMul = 0f;
            }
            if (nudging && offDir == 1)
            {
                rightMul = 1f;
                leftMul = 0f;
            }
            if (_leftJump)
            {
                leftMul = 0f;
            }
            else if (_rightJump)
            {
                rightMul = 0f;
            }
            strafing = false;
            if (!_moveLock)
            {
                strafing = inputProfile.Down("STRAFE");
                if (leftMul > 0.01f && (!crouch || nudging))
                {
                    if (hSpeed > (0f - maxrun) * leftMul)
                    {
                        hSpeed -= hAcc;
                        if (hSpeed < (0f - maxrun) * leftMul)
                        {
                            hSpeed = (0f - maxrun) * leftMul;
                        }
                    }
                    _heldLeft = true;
                    if (!strafing && !nudging && (oldAngleCode || _leftPressedFrame > _rightPressedFrame))
                    {
                        offDir = -1;
                    }
                }
                if (rightMul > 0.01f && (!crouch || nudging))
                {
                    if (hSpeed < maxrun * rightMul)
                    {
                        hSpeed += hAcc;
                        if (hSpeed > maxrun * rightMul)
                        {
                            hSpeed = maxrun * rightMul;
                        }
                    }
                    _heldRight = true;
                    if (!strafing && !nudging && (oldAngleCode || _rightPressedFrame > _leftPressedFrame))
                    {
                        offDir = 1;
                    }
                }
                if (base.isServerForObject && strafing)
                {
                    Global.data.strafeDistance.valueFloat += Math.Abs(hSpeed) * 0.00015f;
                }
                if (_atWallFrames > 0)
                {
                    _atWallFrames--;
                }
                else
                {
                    atWall = false;
                    leftWall = false;
                    rightWall = false;
                }
                _canWallJump = GetEquipment(typeof(WallBoots)) != null;
                int wallJumpGiveFrames = 6;
                if (!base.grounded && _canWallJump)
                {
                    Block blockLeft = Level.CheckLine<Block>(base.topLeft + new Vec2(0f, 4f), base.bottomLeft + new Vec2(-3f, -4f));
                    Block blockRight = Level.CheckLine<Block>(base.topRight + new Vec2(3f, 4f), base.bottomRight + new Vec2(0f, -4f));
                    if (inputProfile.Down("LEFT") && blockLeft != null && !blockLeft.clip.Contains(this))
                    {
                        atWall = true;
                        leftWall = true;
                        _atWallFrames = wallJumpGiveFrames;
                        if (!onWall)
                        {
                            onWall = true;
                            SFX.Play("wallTouch", 1f, Rando.Float(-0.1f, 0.1f));
                            for (int j = 0; j < 2; j++)
                            {
                                float xAdd = 0f;
                                xAdd = ((!leftWall) ? (-4f) : 4f);
                                Feather f = Feather.New(base.X + xAdd + Rando.Float(-1f, 1f), base.Y + Rando.Float(-4f, 4f), persona);
                                f.velocity *= 0.9f;
                                if (leftWall)
                                {
                                    f.hSpeed = Rando.Float(-1f, 2f);
                                }
                                else
                                {
                                    f.hSpeed = Rando.Float(-2f, 1f);
                                }
                                f.vSpeed = Rando.Float(-2f, 1.5f);
                                Level.Add(f);
                            }
                        }
                    }
                    else if (inputProfile.Down("RIGHT") && blockRight != null && !blockRight.clip.Contains(this))
                    {
                        atWall = true;
                        rightWall = true;
                        _atWallFrames = wallJumpGiveFrames;
                        if (!onWall)
                        {
                            onWall = true;
                            SFX.Play("wallTouch", 1f, Rando.Float(-0.1f, 0.1f));
                            for (int k = 0; k < 2; k++)
                            {
                                float xAdd2 = 0f;
                                xAdd2 = ((!leftWall) ? (-4f) : 4f);
                                Feather f2 = Feather.New(base.X + xAdd2 + Rando.Float(-1f, 1f), base.Y + Rando.Float(-4f, 4f), persona);
                                f2.vSpeed = Rando.Float(-2f, 1.5f);
                                f2.velocity *= 0.9f;
                                if (leftWall)
                                {
                                    f2.hSpeed = Rando.Float(-1f, 2f);
                                }
                                else
                                {
                                    f2.hSpeed = Rando.Float(-2f, 1f);
                                }
                                Level.Add(f2);
                            }
                        }
                    }
                }
                if (onWall && _atWallFrames != wallJumpGiveFrames)
                {
                    SFX.Play("wallLeave", 1f, Rando.Float(-0.1f, 0.1f));
                    for (int l = 0; l < 2; l++)
                    {
                        float xAdd3 = 0f;
                        xAdd3 = ((!leftWall) ? (-4f) : 4f);
                        Feather f3 = Feather.New(base.X + xAdd3 + Rando.Float(-1f, 1f), base.Y + Rando.Float(-4f, 4f), persona);
                        f3.vSpeed = Rando.Float(-2f, 1.5f);
                        f3.velocity *= 0.9f;
                        if (leftWall)
                        {
                            f3.hSpeed = Rando.Float(-1f, 2f);
                        }
                        else
                        {
                            f3.hSpeed = Rando.Float(-2f, 1f);
                        }
                        Level.Add(f3);
                    }
                    onWall = false;
                }
                if ((leftWall || rightWall) && vSpeed > 1f && _atWallFrames == wallJumpGiveFrames)
                {
                    vSpeed = 0.5f;
                }
                if (_wallJump > 0)
                {
                    _wallJump--;
                }
                else
                {
                    _rightJump = (_leftJump = false);
                }
                bool canJump = _jumpValid > 0 && ((_groundValid > 0 && !_crouchLock) || (atWall && _wallJump == 0) || doFloat);
                if (_double && !HasJumpModEquipment() && !_hovering && inputProfile.Pressed("JUMP"))
                {
                    PhysicsRopeSection sectionz = null;
                    if (_vine == null)
                    {
                        sectionz = Level.Nearest<PhysicsRopeSection>(base.X, base.Y);
                    }
                    if (sectionz != null && (Position - sectionz.Position).Length() < 18f)
                    {
                        Vine v = sectionz.rope.LatchOn(sectionz, this);
                        _vine = v;
                        _double = false;
                        canJump = false;
                        _groundValid = 0;
                    }
                }
                bool jumpDown = false;
                if (canJump && Math.Abs(hSpeed) < 0.2f && inputProfile.Down("DOWN") && Math.Abs(hSpeed) < 0.2f && inputProfile.Down("DOWN"))
                {
                    foreach (IPlatform plat in Level.CheckLineAll<IPlatform>(base.bottomLeft + new Vec2(0.1f, 1f), base.bottomRight + new Vec2(-0.1f, 1f)))
                    {
                        if (plat is Block)
                        {
                            canJump = true;
                            break;
                        }
                        if (!(plat is MaterialThing t))
                        {
                            continue;
                        }
                        base.clip.Add(t);
                        foreach (IPlatform left in Level.CheckPointAll<IPlatform>(t.topLeft + new Vec2(-2f, 2f)))
                        {
                            if (left != null && left is MaterialThing && !(left is Block))
                            {
                                base.clip.Add(left as MaterialThing);
                            }
                        }
                        foreach (IPlatform right in Level.CheckPointAll<IPlatform>(t.topRight + new Vec2(2f, 2f)))
                        {
                            if (right != null && right is MaterialThing && !(right is Block))
                            {
                                base.clip.Add(right as MaterialThing);
                            }
                        }
                        canJump = false;
                    }
                    if (!canJump)
                    {
                        base.Y += 1f;
                        vSpeed = 1f;
                        _groundValid = 0;
                        _hovering = false;
                        jumping = true;
                        jumpDown = true;
                    }
                }
                PhysicsRopeSection section = null;
                if (_vine == null)
                {
                    section = Level.Nearest<PhysicsRopeSection>(base.X, base.Y);
                    if (section != null && (Position - section.Position).Length() >= 18f)
                    {
                        section = null;
                    }
                }
                bool jet = false;
                if (!jumpDown)
                {
                    if (inputProfile.Pressed("JUMP"))
                    {
                        if (HasEquipment(typeof(Jetpack)) && (_groundValid <= 0 || crouch || sliding))
                        {
                            GetEquipment(typeof(Jetpack)).PressAction();
                            jet = true;
                        }
                        if (!canJump && HasTV() && CheckTVChannel(pChannel: true) && CheckTVJump() && section == null)
                        {
                            _groundValid = 9999;
                            canJump = true;
                            tvJumped = true;
                        }
                    }
                    if (inputProfile.Down("JUMP") && HasEquipment(typeof(Jetpack)) && (_groundValid <= 0 || crouch || sliding))
                    {
                        jet = true;
                    }
                    if (inputProfile.Released("JUMP") && HasEquipment(typeof(Jetpack)))
                    {
                        GetEquipment(typeof(Jetpack)).ReleaseAction();
                    }
                    if (inputProfile.Pressed("JUMP") && HasEquipment(typeof(Grapple)) && !base.grounded && _jumpValid <= 0 && _groundValid <= 0)
                    {
                        jet = true;
                    }
                }
                canJump = canJump && !jet;
                bool cancelJumping = false;
                bool halfSpeed = false;
                bool releasedVine = false;
                if (!canJump && _vine != null && inputProfile.Released("JUMP"))
                {
                    _vine.Degrapple();
                    _vine = null;
                    if (!inputProfile.Down("DOWN"))
                    {
                        canJump = true;
                        cancelJumping = true;
                    }
                    if (!inputProfile.Down("UP"))
                    {
                        halfSpeed = true;
                    }
                    releasedVine = true;
                }
                if (canJump)
                {
                    if (atWall)
                    {
                        _wallJump = 8;
                        if (leftWall)
                        {
                            hSpeed += 4f;
                            _leftJump = true;
                        }
                        else if (rightWall)
                        {
                            hSpeed -= 4f;
                            _rightJump = true;
                        }
                        vSpeed = jumpSpeed;
                    }
                    else
                    {
                        vSpeed = jumpSpeed;
                    }
                    jumping = true;
                    sliding = false;
                    if (Network.isActive)
                    {
                        if (base.isServerForObject)
                        {
                            _netJump.Play();
                        }
                    }
                    else
                    {
                        SFX.Play("jump", 0.5f);
                    }
                    _groundValid = 0;
                    _hovering = false;
                    _jumpValid = 0;
                    profile.stats.timesJumped++;
                    if (Recorder.currentRecording != null)
                    {
                        Recorder.currentRecording.LogAction(6);
                    }
                }
                if (cancelJumping)
                {
                    jumping = false;
                    if (halfSpeed && vSpeed < 0f)
                    {
                        vSpeed *= 0.7f;
                    }
                }
                if (inputProfile.Released("JUMP"))
                {
                    if (jumping)
                    {
                        jumping = false;
                        pipeOut = 0;
                        if (vSpeed < 0f)
                        {
                            vSpeed *= 0.5f;
                        }
                    }
                    _hovering = false;
                }
                if (!canJump && !HasJumpModEquipment() && _groundValid <= 0)
                {
                    bool canHover = !crouch && holdingWeight <= 5f && (pipeOut <= 0 || vSpeed > -0.1f);
                    if (!_hovering && inputProfile.Pressed("JUMP"))
                    {
                        if (section != null)
                        {
                            Vine v2 = section.rope.LatchOn(section, this);
                            _vine = v2;
                            _double = false;
                        }
                        else if (_vine == null && canHover)
                        {
                            _hovering = true;
                            _flapFrame = 0;
                        }
                    }
                    if (canHover && _hovering && vSpeed >= 0f)
                    {
                        if (vSpeed > 1f)
                        {
                            vSpeed = 1f;
                        }
                        vSpeed -= 0.15f;
                    }
                }
                if (doFloat)
                {
                    _hovering = false;
                }
                if (base.isServerForObject)
                {
                    if (inputProfile.Down("DOWN"))
                    {
                        if (!base.grounded && HasTV())
                        {
                            if (slamWait <= 0)
                            {
                                if (vSpeed < vMax)
                                {
                                    vSpeed += 0.6f;
                                }
                                crouch = true;
                            }
                            else
                            {
                                crouch = false;
                            }
                        }
                        else
                        {
                            crouch = true;
                        }
                        if (!disableCrouch && !crouchCancel)
                        {
                            if (base.grounded && Math.Abs(hSpeed) > 1f)
                            {
                                if (!sliding && slideBuildup < -0.3f)
                                {
                                    slideBuildup = 0.4f;
                                    didFireSlide = true;
                                }
                                sliding = true;
                            }
                        }
                        else
                        {
                            crouch = false;
                        }
                    }
                    else
                    {
                        if (!_crouchLock)
                        {
                            crouch = false;
                            sliding = false;
                        }
                        crouchCancel = false;
                    }
                    if (!sliding)
                    {
                        didFireSlide = false;
                    }
                    if (slideBuildup > 0f || !sliding || !didFireSlide)
                    {
                        slideBuildup -= Maths.IncFrameTimer();
                        if (slideBuildup <= -0.6f)
                        {
                            slideBuildup = -0.6f;
                        }
                    }
                }
                if (base.isServerForObject && !(holdObject is DrumSet) && !(holdObject is Trumpet) && inputProfile.Pressed("RAGDOLL") && !(Level.current is TitleScreen) && pipeOut <= 0)
                {
                    framesSinceRagdoll = 0;
                    GoRagdoll();
                }
                if (base.isServerForObject && base.grounded && Math.Abs(vSpeed) + Math.Abs(hSpeed) < 0.5f && !_closingEyes && holdObject == null && inputProfile.Pressed("SHOOT"))
                {
                    Ragdoll d2 = Level.Nearest<Ragdoll>(base.X, base.Y, this);
                    if (d2 != null && d2.active && d2.visible && (d2.Position - Position).Length() < 100f && d2.captureDuck != null && d2.captureDuck.dead && !d2.captureDuck._eyesClosed && (d2.part1.Position - (Position + new Vec2(0f, 8f))).Length() < 4f)
                    {
                        Level.Add(new EyeCloseWing((d2.part1.Angle < 0f) ? (base.X - 4f) : (base.X - 11f), base.Y + 7f, (d2.part1.Angle < 0f) ? 1 : (-1), _spriteArms, this, d2.captureDuck));
                        if (Network.isActive)
                        {
                            Send.Message(new NMEyeCloseWing(Position, this, d2.captureDuck));
                        }
                        _closingEyes = true;
                        profile.stats.respectGivenToDead++;
                        AddCoolness(1);
                        _timeSinceDuckLayedToRest = DateTime.Now;
                        Flower f4 = Level.Nearest<Flower>(base.X, base.Y);
                        if (f4 != null && (f4.Position - Position).Length() < 22f)
                        {
                            Fondle(d2);
                            Fondle(d2.captureDuck);
                            if (Network.isActive)
                            {
                                Send.Message(new NMFuneral(profile, d2.captureDuck));
                            }
                            d2.captureDuck.LayToRest(profile);
                            if (!Music.currentSong.Contains("MarchOfDuck"))
                            {
                                if (Network.isActive)
                                {
                                    Send.Message(new NMPlayMusic("MarchOfDuck"));
                                }
                                Music.Play("MarchOfDuck", looping: false);
                            }
                        }
                    }
                }
                if (inputProfile.Released("JUMP") || vineRelease)
                {
                    vineRelease = false;
                }
                if (releasedVine)
                {
                    vineRelease = true;
                }
            }
        }
        disableCrouch = false;
    }

    public void EmitBubbles(int num, float hVel)
    {
        if (underwater)
        {
            for (int i = 0; i < num; i++)
            {
                Level.Add(new TinyBubble(base.X + (float)(((offDir > 0) ? 6 : (-6)) * ((!sliding) ? 1 : (-1))) + Rando.Float(-1f, 1f), base.top + 7f + Rando.Float(-1f, 1f), Rando.Float(hVel) * (float)offDir, _curPuddle.top + 7f));
            }
        }
    }

    public void MakeStars()
    {
        Level.Add(new DizzyStar(base.X + (float)(offDir * -3), base.Y - 9f, new Vec2(Rando.Float(-0.8f, -1.5f), Rando.Float(0.5f, -1f))));
        Level.Add(new DizzyStar(base.X + (float)(offDir * -3), base.Y - 9f, new Vec2(Rando.Float(-0.8f, -1.5f), Rando.Float(0.5f, -1f))));
        Level.Add(new DizzyStar(base.X + (float)(offDir * -3), base.Y - 9f, new Vec2(Rando.Float(0.8f, 1.5f), Rando.Float(0.5f, -1f))));
        Level.Add(new DizzyStar(base.X + (float)(offDir * -3), base.Y - 9f, new Vec2(Rando.Float(0.8f, 1.5f), Rando.Float(0.5f, -1f))));
        Level.Add(new DizzyStar(base.X + (float)(offDir * -3), base.Y - 9f, new Vec2(Rando.Float(-1.5f, 1.5f), Rando.Float(-0.5f, -1.1f))));
    }

    public static void MakeStars(Vec2 pPosition, Vec2 pVelocity)
    {
        Level.Add(new NewDizzyStar(pPosition.X + pVelocity.X * 2f, pPosition.Y, new Vec2(-1.7f, -1f) + new Vec2((pVelocity.X + Rando.Float(-0.5f, 0.5f)) * Rando.Float(0.6f, 0.9f) / 2f, -1f + Rando.Float(-0.5f, 0f)), new Color(247, 224, 89)));
        Level.Add(new NewDizzyStar(pPosition.X + pVelocity.X * 2f, pPosition.Y, new Vec2(-0.7f, -0.5f) + new Vec2((pVelocity.X + Rando.Float(-0.5f, 0.5f)) * Rando.Float(0.6f, 0.9f) / 2f, -1f + Rando.Float(-0.5f, 0f)), new Color(247, 224, 89)));
        Level.Add(new NewDizzyStar(pPosition.X + pVelocity.X * 2f, pPosition.Y, new Vec2(0.7f, -0.5f) + new Vec2((pVelocity.X + Rando.Float(-0.5f, 0.5f)) * Rando.Float(0.6f, 0.9f) / 2f, -1f + Rando.Float(-0.5f, 0f)), new Color(247, 224, 89)));
        Level.Add(new NewDizzyStar(pPosition.X + pVelocity.X * 2f, pPosition.Y, new Vec2(1.7f, -1f) + new Vec2((pVelocity.X + Rando.Float(-0.5f, 0.5f)) * Rando.Float(0.6f, 0.9f) / 2f, -1f + Rando.Float(-0.5f, 0f)), new Color(247, 224, 89)));
        Level.Add(new NewDizzyStar(pPosition.X + pVelocity.X * 2f, pPosition.Y, new Vec2(0f, -1.4f) + new Vec2((pVelocity.X + Rando.Float(-0.5f, 0.5f)) * Rando.Float(0.6f, 0.9f) / 2f, -1f + Rando.Float(-0.5f, 0f)), new Color(247, 224, 89)));
    }

    public virtual void DuckUpdate()
    {
    }

    public override void SpecialNetworkUpdate()
    {
    }

    public override void OnGhostObjectAdded()
    {
        if (!base.isServerForObject)
        {
            return;
        }
        if (_trappedInstance == null)
        {
            _trappedInstance = new TrappedDuck(base.X, base.Y - 9999f, this);
            _trappedInstance.active = false;
            _trappedInstance.visible = false;
            _trappedInstance.authority = 80;
            if (!GhostManager.inGhostLoop)
            {
                GhostManager.context.MakeGhost(_trappedInstance);
            }
            Level.Add(_trappedInstance);
            Fondle(_trappedInstance);
        }
        if (_cookedInstance == null)
        {
            _cookedInstance = new CookedDuck(base.X, base.Y - 9999f);
            _cookedInstance.active = false;
            _cookedInstance.visible = false;
            _cookedInstance.authority = 80;
            if (!GhostManager.inGhostLoop)
            {
                GhostManager.context.MakeGhost(_cookedInstance);
            }
            Level.Add(_cookedInstance);
            if (_profile.localPlayer)
            {
                Fondle(_cookedInstance);
            }
        }
        if (_ragdollInstance == null)
        {
            _ragdollInstance = new Ragdoll(base.X, base.Y - 9999f, this, slide: false, 0f, 0, Vec2.Zero);
            _ragdollInstance.npi = netProfileIndex;
            _ragdollInstance.RunInit();
            _ragdollInstance.active = false;
            _ragdollInstance.visible = false;
            _ragdollInstance.authority = 80;
            Level.Add(_ragdollInstance);
            Fondle(_ragdollInstance);
        }
    }

    private void RecoverServerControl()
    {
        Thing.Fondle(this, DuckNetwork.localConnection);
        Thing.Fondle(holdObject, DuckNetwork.localConnection);
        Thing.Fondle(_trappedInstance, DuckNetwork.localConnection);
        Thing.Fondle(_ragdollInstance, DuckNetwork.localConnection);
        foreach (Equipment item in _equipment)
        {
            Thing.Fondle(item, DuckNetwork.localConnection);
        }
    }

    public override void Update()
    {
        if (Network.isActive && _trappedInstance != null && _trappedInstance.ghostObject != null && !_trappedInstance.ghostObject.IsInitialized())
        {
            return;
        }
        tilt = Lerp.FloatSmooth(tilt, 0f, 0.25f);
        verticalOffset = Lerp.FloatSmooth(verticalOffset, 0f, 0.25f);
        if (swordInvincibility > 0)
        {
            swordInvincibility--;
        }
        if ((ragdoll == null || ragdoll.tongueStuck == Vec2.Zero) && tongueCheck != Vec2.Zero && base.level.cold)
        {
            Block b = Level.CheckPoint<Block>(tongueCheck);
            if (b != null && b.physicsMaterial == PhysicsMaterial.Metal)
            {
                GoRagdoll();
                if (ragdoll != null)
                {
                    ragdoll.tongueStuck = tongueCheck;
                    ragdoll.tongueStuckThing = b;
                    ragdoll.tongueShakes = 0;
                }
            }
        }
        if (Network.isActive)
        {
            UpdateConnectionIndicators();
            if (_profile == Profiles.EnvironmentProfile && _netProfileIndex >= 0 && _netProfileIndex < DG.MaxPlayers)
            {
                AssignNetProfileIndex((byte)_netProfileIndex);
            }
        }
        framesSinceRagdoll++;
        if (killedByProfile != null)
        {
            framesSinceKilled++;
        }
        _ = crouch;
        if (_sprite == null)
        {
            return;
        }
        fancyShoes = HasEquipment(typeof(FancyShoes));
        if (base.isServerForObject && inputProfile != null)
        {
            if (NetworkDebugger.enabled)
            {
                if (inputProfile.CheckCode(Level.core.konamiCode) || inputProfile.CheckCode(Level.core.konamiCodeAlternate))
                {
                    Position = cameraPosition;
                    Presto();
                }
            }
            else if (inputProfile.CheckCode(Input.konamiCode) || inputProfile.CheckCode(Input.konamiCodeAlternate))
            {
                Position = cameraPosition;
                Presto();
            }
        }
        if (_disarmWait > 0)
        {
            _disarmWait--;
        }
        if (_disarmDisable > 0)
        {
            _disarmDisable--;
        }
        if (killMultiplier > 0f)
        {
            killMultiplier -= 0.016f;
        }
        else
        {
            killMultiplier = 0f;
        }
        if (base.isServerForObject && holdObject != null && holdObject.removeFromLevel)
        {
            holdObject = null;
        }
        if (Network.isActive)
        {
            if (base.isServerForObject)
            {
                if (_assignedIndex)
                {
                    _assignedIndex = false;
                    Thing.Fondle(this, DuckNetwork.localConnection);
                    if (holdObject != null)
                    {
                        Thing.PowerfulRuleBreakingFondle(holdObject, DuckNetwork.localConnection);
                    }
                    foreach (Equipment e in _equipment)
                    {
                        if (e != null)
                        {
                            Thing.PowerfulRuleBreakingFondle(e, DuckNetwork.localConnection);
                        }
                    }
                }
                if (inputProfile != null && !manualQuackPitch)
                {
                    float pitch = inputProfile.leftTrigger;
                    if (inputProfile.hasMotionAxis)
                    {
                        pitch += inputProfile.motionAxis;
                    }
                    quackPitch = (byte)(pitch * 255f);
                }
                _framesSinceInput++;
                if (inputProfile != null && (inputProfile.Pressed("", any: true) || Level.current is RockScoreboard))
                {
                    _framesSinceInput = 0;
                    afk = false;
                }
                if (_framesSinceInput > 1200)
                {
                    afk = true;
                }
            }
            else if (profile != null)
            {
                if (disarmIndex != 9 && disarmIndex != _prevDisarmIndex && (_prevDisarmIndex == profile.networkIndex || _prevDisarmIndex == 9) && disarmIndex >= 0 && disarmIndex < 8 && DuckNetwork.profiles[disarmIndex].connection == DuckNetwork.localConnection)
                {
                    Global.data.disarms.valueInt++;
                }
                _prevDisarmIndex = disarmIndex;
            }
            if (base.isServerForObject)
            {
                disarmIndexCooldown -= Maths.IncFrameTimer();
                if (disarmIndexCooldown <= 0f && profile != null)
                {
                    disarmIndexCooldown = 0f;
                    disarmIndex = profile.networkIndex;
                }
            }
            if (base.Y > -999f)
            {
                _lastGoodPosition = Position;
            }
            if (Network.isActive)
            {
                if (_ragdollInstance != null)
                {
                    _ragdollInstance.captureDuck = this;
                }
                if (ragdoll != null && ragdoll.isServerForObject)
                {
                    if (_trapped != null && _trapped.Y > -5000f)
                    {
                        if (Network.isActive)
                        {
                            ragdoll.active = false;
                            ragdoll.visible = false;
                            ragdoll.owner = null;
                            if (base.Y > -1000f)
                            {
                                ragdoll.Y = -9999f;
                                if (ragdoll.part1 != null)
                                {
                                    ragdoll.part1.Y = -9999f;
                                }
                                if (ragdoll.part2 != null)
                                {
                                    ragdoll.part2.Y = -9999f;
                                }
                                if (ragdoll.part3 != null)
                                {
                                    ragdoll.part3.Y = -9999f;
                                }
                            }
                        }
                        else
                        {
                            Level.Remove(this);
                        }
                        ragdoll = null;
                    }
                    if (ragdoll != null)
                    {
                        if (ragdoll.Y < -5000f)
                        {
                            ragdoll.Position = cameraPosition;
                            if (ragdoll.part1 != null)
                            {
                                ragdoll.part1.Position = cameraPosition;
                            }
                            if (ragdoll.part2 != null)
                            {
                                ragdoll.part2.Position = cameraPosition;
                            }
                            if (ragdoll.part3 != null)
                            {
                                ragdoll.part3.Position = cameraPosition;
                            }
                        }
                        if (ragdoll.part1 != null && ragdoll.part1.owner != null && ragdoll.part1.owner.Y < -5000f)
                        {
                            ragdoll.part1.owner = null;
                        }
                        if (ragdoll.part2 != null && ragdoll.part2.owner != null && ragdoll.part2.owner.Y < -5000f)
                        {
                            ragdoll.part2.owner = null;
                        }
                        if (ragdoll.part3 != null && ragdoll.part3.owner != null && ragdoll.part3.owner.Y < -5000f)
                        {
                            ragdoll.part3.owner = null;
                        }
                    }
                }
                if (_trapped != null && _trapped.Y < -5000f && _trapped.isServerForObject)
                {
                    _trapped.Position = cameraPosition;
                }
                if (_cooked != null && _cooked.Y < -5000f && _cooked.isServerForObject)
                {
                    _cooked.Position = cameraPosition;
                }
            }
            if (_profile.localPlayer && !(this is RockThrowDuck) && base.isServerForObject)
            {
                if (ragdoll == null && _trapped == null && _cooked == null && base.Y < -5000f)
                {
                    Position = cameraPosition;
                }
                if (_ragdollInstance != null)
                {
                    if (ragdoll == null && ((_ragdollInstance.part1 != null && _ragdollInstance.part1.owner != null) || (_ragdollInstance.part2 != null && _ragdollInstance.part2.owner != null) || (_ragdollInstance.part3 != null && _ragdollInstance.part3.owner != null)))
                    {
                        Thing owner1 = _ragdollInstance.part1.owner;
                        Thing owner2 = _ragdollInstance.part2.owner;
                        Thing owner3 = _ragdollInstance.part3.owner;
                        GoRagdoll();
                        if (owner1 != null)
                        {
                            _ragdollInstance.connection = owner1.connection;
                            _ragdollInstance.part1.owner = owner1;
                        }
                        if (owner2 != null)
                        {
                            _ragdollInstance.connection = owner2.connection;
                            _ragdollInstance.part2.owner = owner2;
                        }
                        if (owner3 != null)
                        {
                            _ragdollInstance.connection = owner3.connection;
                            _ragdollInstance.part3.owner = owner3;
                        }
                    }
                    if (_ragdollInstance.visible)
                    {
                        ragdoll = _ragdollInstance;
                    }
                    else
                    {
                        _ragdollInstance.visible = true;
                        _ragdollInstance.visible = false;
                        if (_ragdollInstance.part1 != null)
                        {
                            _ragdollInstance.part1.Y = -9999f;
                        }
                        if (_ragdollInstance.part2 != null)
                        {
                            _ragdollInstance.part2.Y = -9999f;
                        }
                        if (_ragdollInstance.part3 != null)
                        {
                            _ragdollInstance.part3.Y = -9999f;
                        }
                        ragdoll = null;
                    }
                    if (_cookedInstance != null)
                    {
                        if (_cookedInstance.visible)
                        {
                            _cooked = _cookedInstance;
                            if (_ragdollInstance != null)
                            {
                                _ragdollInstance.visible = false;
                                _ragdollInstance.active = false;
                                ragdoll = null;
                            }
                        }
                        else
                        {
                            _cooked = null;
                            _cookedInstance.Y = -9999f;
                        }
                    }
                }
            }
        }
        if (_profile.localPlayer && !(this is RockThrowDuck) && connection != DuckNetwork.localConnection && !CanBeControlled())
        {
            RecoverServerControl();
        }
        if (_trappedInstance != null)
        {
            if (_trappedInstance.visible || _trappedInstance.owner != null)
            {
                _trapped = _trappedInstance;
            }
            else
            {
                _trappedInstance.owner = null;
                _trapped = null;
                _trappedInstance.Y = -9999f;
            }
        }
        if (profile != null && mindControl != null && Level.current is GameLevel)
        {
            profile.stats.timeUnderMindControl += Maths.IncFrameTimer();
        }
        if (underwater)
        {
            _framesUnderwater++;
            if (_framesUnderwater >= 60)
            {
                _framesUnderwater = 0;
                Global.data.secondsUnderwater.valueInt++;
            }
            _bubbleWait += Rando.Float(0.015f, 0.017f);
            if (Rando.Float(1f) > 0.99f)
            {
                _bubbleWait += 0.5f;
            }
            if (_bubbleWait > 1f)
            {
                _bubbleWait = Rando.Float(0.2f);
                EmitBubbles(1, 1f);
            }
        }
        if (!quackStart && quack > 0)
        {
            quackStart = true;
            EmitBubbles(Rando.Int(3, 6), 1.2f);
            if (Level.current.cold && !underwater)
            {
                Breath();
            }
        }
        if (quack <= 0)
        {
            quackStart = false;
        }
        wait++;
        if (TeamSelect2.doCalc && wait > 10 && profile != null)
        {
            wait = 0;
            float newVal = profile.endOfRoundStats.CalculateProfileScore();
            if (firstCalc)
            {
                firstCalc = false;
                lastCalc = newVal;
            }
            if (Math.Abs(lastCalc - newVal) > 0.005f)
            {
                int val = (int)Math.Round((newVal - lastCalc) / 0.005f);
                if (plus == null || plus.removeFromLevel)
                {
                    plus = new CoolnessPlus(base.X, base.Y, this, val);
                    Level.Add(plus);
                }
                else
                {
                    plus.change = val;
                }
            }
            lastCalc = newVal;
        }
        if (grappleMul)
        {
            grappleMultiplier = 1.5f;
        }
        else
        {
            grappleMultiplier = 1f;
        }
        _timeSinceThrow++;
        if (_timeSinceThrow > 30)
        {
            _timeSinceThrow = 30;
        }
        if (_resetAction && !inputProfile.Down("SHOOT"))
        {
            _resetAction = false;
        }
        if (_converted == null)
        {
            _sprite.texture = profile.persona.sprite.texture;
            _spriteArms.texture = profile.persona.armSprite.texture;
            _spriteQuack.texture = profile.persona.quackSprite.texture;
            _spriteControlled.texture = profile.persona.controlledSprite.texture;
        }
        else
        {
            _sprite.texture = _converted.profile.persona.sprite.texture;
            _spriteArms.texture = _converted.profile.persona.armSprite.texture;
            _spriteQuack.texture = _converted.profile.persona.quackSprite.texture;
            _spriteControlled.texture = _converted.profile.persona.controlledSprite.texture;
        }
        listenTime--;
        if (listenTime < 0)
        {
            listenTime = 0;
        }
        if (listening && listenTime <= 0)
        {
            listening = false;
        }
        if (base.isServerForObject && !listening)
        {
            conversionResistance++;
            if (conversionResistance > 100)
            {
                conversionResistance = 100;
            }
        }
        _coolnessThisFrame = 0;
        UpdateBurning();
        UpdateGhostStatus();
        if (dead)
        {
            immobilized = true;
            if (unfocus > 0f)
            {
                unfocus -= 0.015f;
            }
            else if (!unfocused)
            {
                if (!base.grounded && _lives > 0)
                {
                    IEnumerable<Thing> source = Level.current.things[typeof(SpawnPoint)];
                    Thing point = source.ElementAt(Rando.Int(source.Count() - 1));
                    Position = point.Position;
                }
                if (profile != null && profile.localPlayer && Level.current is TeamSelect2)
                {
                    foreach (ProfileBox2 p in (Level.current as TeamSelect2)._profiles)
                    {
                        if (p.duck == this)
                        {
                            Thing.UnstoppableFondle(this, DuckNetwork.localConnection);
                            Vec2 pos = p.Position + new Vec2(82f, 58f);
                            if (!p.rightRoom)
                            {
                                pos = p.Position + new Vec2(58f, 58f);
                            }
                            Position = pos;
                            if (_ragdollInstance != null)
                            {
                                Thing.UnstoppableFondle(_ragdollInstance, DuckNetwork.localConnection);
                                _ragdollInstance.Position = new Vec2(pos.X, pos.Y - 3f);
                                _ragdollInstance.Unragdoll();
                            }
                            RecoverServerControl();
                            if (ragdoll != null)
                            {
                                ragdoll.Unragdoll();
                            }
                            Position = pos;
                            SFX.PlaySynchronized("convert", 0.75f);
                            Ressurect();
                            if (Network.isActive && base.ghostObject != null)
                            {
                                base.ghostObject.SuperDirtyStateMask();
                            }
                        }
                    }
                }
                else
                {
                    Respawner s = Level.Nearest<Respawner>(Position);
                    if (s != null && profile != null && profile.localPlayer)
                    {
                        if (ragdoll != null)
                        {
                            ragdoll.Unragdoll();
                        }
                        Position = s.Position + new Vec2(0f, -16f);
                        SFX.PlaySynchronized("respawn", 0.65f);
                        Ressurect();
                    }
                    else if (_lives > 0)
                    {
                        _lives--;
                        unfocus = 1f;
                        _isGhost = true;
                        Regenerate();
                        immobilized = false;
                        crouch = false;
                        sliding = false;
                    }
                    else
                    {
                        unfocus = -1f;
                        unfocused = true;
                        if (base.isServerForObject)
                        {
                            visible = false;
                        }
                        if (!Network.isActive)
                        {
                            active = false;
                        }
                        if (Level.current.camera is FollowCam && !(Level.current is ChallengeLevel))
                        {
                            (Level.current.camera as FollowCam).Remove(this);
                        }
                        base.Y -= 100000f;
                    }
                }
            }
            sliding = true;
            crouch = true;
        }
        else if (quack > 0)
        {
            profile.stats.timeWithMouthOpen += Maths.IncFrameTimer();
        }
        if (DevConsole.rhythmMode && Level.current is GameLevel && (inputProfile.Pressed("DOWN") || inputProfile.Pressed("JUMP") || inputProfile.Pressed("SHOOT") || inputProfile.Pressed("QUACK") || inputProfile.Pressed("GRAB")) && !RhythmMode.inTime)
        {
            GoRagdoll();
        }
        _iceWedging--;
        if (_iceWedging < 0)
        {
            _iceWedging = 0;
        }
        UpdateMove();
        if (inputProfile == null)
        {
            return;
        }
        if (sliding && _iceWedging <= 0 && base.grounded && Level.CheckLine<Block>(Position + new Vec2(-10f, 0f), Position + new Vec2(10f, 0f)) != null)
        {
            foreach (IPlatform item in Level.CheckPointAll<IPlatform>(new Vec2(Position.X, base.bottom - 4f)))
            {
                if (item is Holdable)
                {
                    sliding = false;
                }
            }
        }
        if (ragdoll != null)
        {
            ragdoll.UpdateUnragdolling();
        }
        centerOffset = 8f;
        if (crouch)
        {
            centerOffset = 24f;
        }
        if (ragdoll == null && base.isServerForObject)
        {
            base.Update();
        }
        if (ragdoll == null && _prevRagdoll != null)
        {
            Level.Add(SmallSmoke.New(base.X - Rando.Float(2f, 5f), base.Y + Rando.Float(-3f, 3f) + 16f));
            Level.Add(SmallSmoke.New(base.X + Rando.Float(2f, 5f), base.Y + Rando.Float(-3f, 3f) + 16f));
            Level.Add(SmallSmoke.New(base.X, base.Y + Rando.Float(-3f, 3f) + 16f));
        }
        _prevRagdoll = ragdoll;
        if (kick > 0f)
        {
            kick -= 0.1f;
        }
        else
        {
            kick = 0f;
        }
        _sprite.speed = 0.1f + Math.Abs(hSpeed) / maxrun * 0.1f;
        _sprite.flipH = offDir < 0;
        if (!swinging)
        {
            UpdateAnimation();
        }
        if (_trapped != null)
        {
            SetCollisionMode("netted");
        }
        else if (_sprite.currentAnimation == "run" || _sprite.currentAnimation == "jump" || _sprite.currentAnimation == "idle")
        {
            SetCollisionMode("normal");
        }
        else if (_sprite.currentAnimation == "slide")
        {
            SetCollisionMode("normal");
        }
        else if (_sprite.currentAnimation == "crouch" || _sprite.currentAnimation == "listening")
        {
            SetCollisionMode("crouch");
        }
        else if (_sprite.currentAnimation == "groundSlide" || _sprite.currentAnimation == "dead")
        {
            SetCollisionMode("slide");
        }
        _ = holdObject;
        if (holdObject != null && base.isServerForObject && (ragdoll == null || !fancyShoes) && !inPipe)
        {
            holdObject.isLocal = isLocal;
            holdObject.UpdateAction();
        }
        if (Network.isActive && holdObject != null && (holdObject.duck != this || !holdObject.active || !holdObject.visible || (!holdObject.isServerForObject && !(holdObject is RagdollPart))) && base.isServerForObject)
        {
            holdObject = null;
        }
        if (tryGrabFrames > 0 && !inputProfile.Pressed("GRAB"))
        {
            tryGrabFrames--;
            TryGrab();
            if (holdObject != null)
            {
                tryGrabFrames = 0;
            }
        }
        else
        {
            tryGrabFrames = 0;
        }
        UpdateThrow();
        doThrow = false;
        reverseThrow = false;
        UpdateHoldPosition();
        if (!base.isServerForObject)
        {
            base.Update();
        }
        forceFire = false;
        foreach (Equipment item2 in _equipment)
        {
            item2.PositionOnOwner();
        }
        _gripped = false;
    }

    public override void HeatUp(Vec2 location)
    {
        if (holdObject != null && holdObject.heat < -0.05f)
        {
            holdObject.DoHeatUp(0.03f, location);
        }
        else if (holstered != null && holstered.heat < -0.05f)
        {
            holstered.DoHeatUp(0.03f, location);
        }
        else if (skewered != null && skewered.heat < -0.05f)
        {
            skewered.DoHeatUp(0.03f, location);
        }
        base.HeatUp(location);
    }

    protected override bool OnBurn(Vec2 firePosition, Thing litBy)
    {
        if (protectedFromFire)
        {
            return false;
        }
        _burnTime -= 0.02f;
        if (!base.onFire)
        {
            if (!dead)
            {
                if (Network.isActive)
                {
                    Scream();
                }
                else
                {
                    SFX.Play("quackYell0" + Change.ToString(Rando.Int(2) + 1), 1f, -0.3f + Rando.Float(0.3f));
                }
                SFX.Play("ignite", 1f, -0.3f + Rando.Float(0.3f));
                if (Rando.Float(1f) < 0.1f)
                {
                    AddCoolness(-1);
                }
                Event.Log(new LitOnFireEvent(litBy?.responsibleProfile, profile));
                profile.stats.timesLitOnFire++;
                if (Recorder.currentRecording != null)
                {
                    Recorder.currentRecording.LogAction(9);
                }
                if (ragdoll == null)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Level.Add(SmallFire.New(-6f + Rando.Float(12f), -8f + Rando.Float(16f), 0f, 0f, shortLife: false, this));
                    }
                }
            }
            base.onFire = true;
        }
        return true;
    }

    public virtual void UpdateHoldPosition(bool updateLerp = true)
    {
        if (_sprite == null || base.Y < -8000f)
        {
            return;
        }
        armOffY = 6f;
        armOffX = -3f * (float)offDir;
        if (holdObject != null)
        {
            armOffY = 6f;
            armOffX = -2f * (float)offDir;
        }
        holdOffX = 6f;
        holdOffY = -3f;
        if (holdObject != null)
        {
            holdObject._sleeping = false;
            if (holdObject.owner != this)
            {
                return;
            }
            if (!base.onFire && holdObject.heat > 0.5f && holdObject.physicsMaterial == PhysicsMaterial.Metal)
            {
                if (_sizzle == null)
                {
                    _sizzle = SFX.Play("sizzle", 0.6f, 0f, 0f, looped: true);
                }
                _handHeat += 0.016f;
                if (_handHeat > 0.4f)
                {
                    if (handSmokeWait <= 0)
                    {
                        Vec2 handpos = new Vec2(armPosition.X + holdObject.handOffset.X * (float)offDir, armPosition.Y + holdObject.handOffset.Y);
                        Level.Add(SmallSmoke.New(handpos.X, handpos.Y, 0.8f, 1f));
                        handSmokeWait = 5;
                    }
                    handSmokeWait--;
                }
                if (_handHeat > 1.1f)
                {
                    _sizzle.Stop();
                    Scream();
                    ThrowItem();
                    _handHeat = 0f;
                }
            }
            else
            {
                if (_sizzle != null)
                {
                    _sizzle.Stop();
                    _sizzle = null;
                }
                _handHeat = 0f;
            }
            if (_sprite.currentAnimation == "run")
            {
                if (_sprite.frame == 1)
                {
                    holdOffY += 1f;
                }
                else if (_sprite.frame == 2)
                {
                    holdOffY += 1f;
                    holdOffX -= 1f;
                }
                else if (_sprite.frame == 3)
                {
                    holdOffY += 1f;
                    holdOffX -= 2f;
                }
                else if (_sprite.frame == 4)
                {
                    holdOffY += 1f;
                    holdOffX -= 1f;
                }
                else if (_sprite.frame == 5)
                {
                    holdOffY += 1f;
                }
            }
            else if (_sprite.currentAnimation == "jump")
            {
                if (_sprite.frame == 0)
                {
                    holdOffY += 1f;
                }
                else if (_sprite.frame == 2)
                {
                    holdOffY -= 1f;
                }
            }
        }
        else
        {
            if (_sizzle != null)
            {
                _sizzle.Stop();
                _sizzle = null;
            }
            _handHeat = 0f;
        }
        holdOffX *= offDir;
        if (holdObject == null || (ragdoll != null && fancyShoes))
        {
            return;
        }
        _spriteArms.Angle = holdAngle;
        _bionicArm.Angle = holdAngle;
        if (base.gun != null)
        {
            kick = base.gun.kick * 5f;
        }
        if (holdObject is DrumSet)
        {
            Position = holdObject.Position + new Vec2(0f, -12f);
        }
        else
        {
            holdObject.Position = armPositionNoKick + holdObject.holdOffset + new Vec2(holdOffX, holdOffY) + new Vec2(2 * offDir, 0f);
        }
        holdObject.CheckIfHoldObstructed();
        if (HasEquipment(typeof(Holster)))
        {
            Holster h = GetEquipment(typeof(Holster)) as Holster;
            if (!h.chained.value || h.containedObject == null)
            {
                if (!base.isServerForObject)
                {
                    holdObstructed = h.netRaise;
                }
                else if (holdObject != null && inputProfile.Down("UP") && holdObject.holsterable)
                {
                    holdObstructed = true;
                }
            }
        }
        if (!(holdObject is RagdollPart))
        {
            holdObject.offDir = offDir;
        }
        if (_sprite.currentAnimation == "slide")
        {
            holdOffY -= 1f;
            holdOffX += 1f;
        }
        else if (_sprite.currentAnimation == "crouch")
        {
            if (holdObject != null)
            {
                armOffY += 4f;
            }
        }
        else if ((_sprite.currentAnimation == "groundSlide" || _sprite.currentAnimation == "dead") && holdObject != null)
        {
            armOffY += 6f;
        }
        UpdateHoldLerp(updateLerp);
        if (!(holdObject is DrumSet))
        {
            holdObject.Position = HoldOffset(holdObject.holdOffset);
            if (!(holdObject is RagdollPart))
            {
                holdObject.Angle = holdObject.handAngle + holdAngleOff;
            }
        }
        _ = holdObject.Y;
        _ = -100f;
    }

    public void UpdateHoldLerp(bool updateLerp = false, bool instant = false)
    {
        if (holdObject.canRaise && ((_hovering && holdObject.hoverRaise) || holdObstructed || holdObject.keepRaised))
        {
            if (updateLerp)
            {
                holdAngleOff = Maths.LerpTowards(holdAngleOff, (0f - (float)Math.PI / 2f * (float)offDir) * holdObject.angleMul, instant ? 1f : (holdObject.raiseSpeed * 2f));
            }
            holdObject.raised = true;
            return;
        }
        if (updateLerp)
        {
            holdAngleOff = Maths.LerpTowards(holdAngleOff, 0f, instant ? 1f : (holdObject.raiseSpeed * 2f * 2f));
        }
        if (holdObject.raised)
        {
            holdObject.raised = false;
        }
    }

    public void ConvertDuck(Duck to)
    {
        if (_converted != to && to != null && to.profile != null)
        {
            to.profile.stats.conversions++;
        }
        RumbleManager.AddRumbleEvent(profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Short, RumbleFalloff.Short));
        _converted = to;
        _spriteArms = to._spriteArms.CloneMap();
        _spriteControlled = to._spriteControlled.CloneMap();
        _spriteQuack = to._spriteQuack.CloneMap();
        _sprite = to._sprite.CloneMap();
        graphic = _sprite;
        if (!isConversionMessage)
        {
            Equipment teamHat = GetEquipment(typeof(TeamHat));
            if (teamHat != null)
            {
                Unequip(teamHat);
            }
            if (to.profile.team.hasHat)
            {
                Hat h = new TeamHat(0f, 0f, to.profile.team, to.profile);
                Level.Add(h);
                Equip(h, makeSound: false);
            }
        }
        for (int i = 0; i < 3; i++)
        {
            Level.Add(new MusketSmoke(base.X - 5f + Rando.Float(10f), base.Y + 6f - 3f + Rando.Float(6f) - (float)i * 1f)
            {
                move =
                {
                    X = -0.2f + Rando.Float(0.4f),
                    Y = -0.2f + Rando.Float(0.4f)
                }
            });
        }
        listenTime = 0;
        listening = false;
        vSpeed -= 5f;
        SFX.Play("convert");
    }

    public void DoFuneralStuff()
    {
        Vec2 pos = Position;
        if (ragdoll != null)
        {
            pos = ragdoll.Position;
        }
        for (int i = 0; i < 3; i++)
        {
            Level.Add(new MusketSmoke(pos.X - 5f + Rando.Float(10f), pos.Y + 6f - 3f + Rando.Float(6f) - (float)i * 1f)
            {
                move =
                {
                    X = -0.2f + Rando.Float(0.4f),
                    Y = -0.2f + Rando.Float(0.4f)
                }
            });
        }
        _timeSinceFuneralPerformed = DateTime.Now;
        SFX.Play("death");
        profile.stats.funeralsRecieved++;
    }

    public void LayToRest(Profile whoDid)
    {
        Vec2 pos = Position;
        if (ragdoll != null)
        {
            pos = ragdoll.Position;
        }
        if (!isConversionMessage)
        {
            Tombstone tombstone = new Tombstone(pos.X, pos.Y);
            Level.Add(tombstone);
            tombstone.vSpeed = -2.5f;
        }
        DoFuneralStuff();
        if (ragdoll != null)
        {
            ragdoll.Y += 10000f;
            ragdoll.part1.Y += 10000f;
            ragdoll.part2.Y += 10000f;
            ragdoll.part3.Y += 10000f;
        }
        base.Y += 10000f;
        if (whoDid != null)
        {
            whoDid.stats.funeralsPerformed++;
            whoDid.duck.AddCoolness(2);
        }
    }

    public void UpdateLerp()
    {
        if (base.lerpSpeed != 0f)
        {
            base.lerpPosition += base.lerpVector * base.lerpSpeed;
        }
    }

    public bool IsQuacking()
    {
        if (quack <= 0 && (_mindControl == null || !_derpMindControl))
        {
            if (ragdoll != null)
            {
                return ragdoll.tongueStuck != Vec2.Zero;
            }
            return false;
        }
        return true;
    }

    public void DrawHat()
    {
        if (hat != null)
        {
            if (_sprite != null)
            {
                hat.Alpha = _sprite.Alpha;
            }
            hat.offDir = offDir;
            hat.Depth = base.Depth + hat.equippedDepth;
            hat.Angle = Angle;
            hat.Draw();
            if (DevConsole.showCollision)
            {
                hat.DrawCollision();
            }
        }
    }

    public Vec2 GetPos()
    {
        Vec2 pos = Position;
        if (ragdoll != null && ragdoll.part1 != null)
        {
            pos = ragdoll.part1.Position;
        }
        else if (_trapped != null)
        {
            pos = _trapped.Position;
        }
        return pos;
    }

    public Vec2 GetEdgePos()
    {
        Vec2 pos = cameraPosition;
        float boarder = 14f;
        if (pos.X < Level.current.camera.left + boarder)
        {
            pos.X = Level.current.camera.left + boarder;
        }
        if (pos.X > Level.current.camera.right - boarder)
        {
            pos.X = Level.current.camera.right - boarder;
        }
        if (pos.Y < Level.current.camera.top + boarder)
        {
            pos.Y = Level.current.camera.top + boarder;
        }
        if (pos.Y > Level.current.camera.bottom - boarder)
        {
            pos.Y = Level.current.camera.bottom - boarder;
        }
        return pos;
    }

    public bool ShouldDrawIcon()
    {
        Vec2 pos = Position;
        if (ragdoll != null)
        {
            if (ragdoll.part1 == null)
            {
                return false;
            }
            pos = ragdoll.part1.Position;
        }
        else if (_trapped != null)
        {
            pos = _trapped.Position;
        }
        if (Network.isActive && _trapped != null && _trappedInstance != null && !_trappedInstance.visible)
        {
            pos = Position;
        }
        if (Network.isActive && ragdoll != null && _ragdollInstance != null && !_ragdollInstance.visible)
        {
            pos = Position;
        }
        if (_cooked != null)
        {
            pos = _cooked.Position;
        }
        if (Network.isActive && _cooked != null && _cookedInstance != null && !_cookedInstance.visible)
        {
            pos = Position;
        }
        if (pos.X < base.level.camera.left - 1000f || pos.Y < -3000f)
        {
            return false;
        }
        float boarder = -6f;
        if (base.level != null && base.level.camera != null && !dead && !VirtualTransition.doingVirtualTransition && (Level.current is GameLevel || Level.current is ChallengeLevel) && Level.current.simulatePhysics)
        {
            if (!(pos.X < base.level.camera.left + boarder) && !(pos.X > base.level.camera.right - boarder) && !(pos.Y < base.level.camera.top + boarder))
            {
                return pos.Y > base.level.camera.bottom - boarder;
            }
            return true;
        }
        return false;
    }

    public void PrepareIconForFrame()
    {
        if (dead)
        {
            return;
        }
        RenderTarget2D iconMap = persona.iconMap;
        Viewport oldV = Graphics.viewport;
        RenderTarget2D oldTarget = Graphics.GetRenderTarget();
        Graphics.SetRenderTarget(iconMap);
        Graphics.viewport = new Viewport(0, 0, 96, 96);
        if (_iconCamera == null)
        {
            _iconCamera = new Camera(0f, 0f, 48f, 48f);
        }
        _iconCamera.center = Position + new Vec2(0f, 2f);
        if (crouch)
        {
            _iconCamera.centerY += 3f;
        }
        if (sliding)
        {
            _iconCamera.centerY += 6f;
            _iconCamera.centerX -= offDir * 7;
        }
        if (ragdoll != null && ragdoll.part2 != null)
        {
            _iconCamera.center = ragdoll.part2.Position - ragdoll.part2.velocity;
        }
        if (_trapped != null)
        {
            _iconCamera.center = _trapped.Position + new Vec2(0f, -5f);
        }
        if (_cooked != null)
        {
            _iconCamera.center = _cooked.Position + new Vec2(0f, -5f);
        }
        renderingIcon = true;
        _renderingDuck = true;
        Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, _iconCamera.getMatrix());
        Graphics.DrawRect(_iconCamera.rectangle, Colors.Transparent, 0.99f);
        Graphics.screen.End();
        Graphics.ResetSpanAdjust();
        Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, _iconCamera.getMatrix());
        if (_cooked != null && (!Network.isActive || (_cookedInstance != null && _cookedInstance.visible)))
        {
            _cooked.Draw();
        }
        else if (ragdoll != null && ragdoll.part1 != null && ragdoll.part3 != null)
        {
            ragdoll.part1.Draw();
            ragdoll.part3.Draw();
            foreach (Equipment item in _equipment)
            {
                item.Draw();
            }
        }
        else if (_trapped != null)
        {
            _trapped.Draw();
            foreach (Equipment item2 in _equipment)
            {
                item2.Draw();
            }
        }
        else
        {
            Draw();
        }
        if (base.onFire)
        {
            foreach (SmallFire f in Level.current.things[typeof(SmallFire)])
            {
                if (f.stick != null && (f.stick == this || f.stick == _trapped || (ragdoll != null && (f.stick == ragdoll.part1 || f.stick == ragdoll.part2))))
                {
                    f.Draw();
                }
            }
        }
        Graphics.screen.End();
        _renderingDuck = false;
        renderingIcon = false;
        Graphics.SetRenderTarget(oldTarget);
        Graphics.viewport = oldV;
    }

    public void DrawIcon()
    {
        if (!dead && _iconCamera != null && !_renderingDuck && ShouldDrawIcon())
        {
            Vec2 pos = Position;
            if (ragdoll != null)
            {
                pos = ragdoll.part1.Position;
            }
            else if (_trapped != null)
            {
                pos = _trapped.Position;
            }
            Vec2 origPos = pos;
            float cameraSize = Level.current.camera.width / 320f * 0.5f;
            cameraSize = 0.75f;
            float boarder = 22f * cameraSize;
            Vec2 dist = new Vec2(0f, 0f);
            if (pos.X < Level.current.camera.left + boarder)
            {
                dist.X = Math.Abs(Level.current.camera.left - pos.X);
                pos.X = Level.current.camera.left + boarder;
            }
            if (pos.X > Level.current.camera.right - boarder)
            {
                dist.X = Math.Abs(Level.current.camera.right - pos.X);
                pos.X = Level.current.camera.right - boarder;
            }
            if (pos.Y < Level.current.camera.top + boarder)
            {
                dist.Y = Math.Abs(Level.current.camera.top - pos.Y);
                pos.Y = Level.current.camera.top + boarder;
            }
            if (pos.Y > Level.current.camera.bottom - boarder)
            {
                dist.Y = Math.Abs(Level.current.camera.bottom - pos.Y);
                pos.Y = Level.current.camera.bottom - boarder;
            }
            cameraSize -= Math.Min((dist * 0.003f).Length(), 1f) * 0.4f;
            Graphics.Draw(persona.iconMap, pos, _iconRect, Color.White, 0f, new Vec2(48f, 48f), new Vec2(0.5f, 0.5f) * cameraSize, SpriteEffects.None, 0.9f + base.Depth.span);
            int f = _sprite.imageIndex;
            _sprite.imageIndex = 21;
            float angle = Maths.DegToRad(Maths.PointDirection(pos, origPos));
            _sprite.Depth = 0.8f;
            _sprite.Angle = 0f - angle;
            _sprite.flipH = false;
            _sprite.UpdateSpriteBox();
            _sprite.Position = new Vec2(pos.X + (float)Math.Cos(angle) * 12f, pos.Y - (float)Math.Sin(angle) * 12f);
            _sprite.DrawWithoutUpdate();
            _sprite.Angle = 0f;
            _sprite.imageIndex = f;
            _sprite.UpdateSpriteBox();
        }
    }

    public virtual void OnDrawLayer(Layer pLayer)
    {
        if (_sprite != null && localSpawnVisible && ShouldDrawIcon())
        {
            if (pLayer == Layer.PreDrawLayer)
            {
                PrepareIconForFrame();
            }
            else if (pLayer == Layer.Foreground)
            {
                DrawIcon();
            }
        }
    }

    public override void Draw()
    {
        if (_sprite == null || !localSpawnVisible)
        {
            return;
        }
        if (inNet)
        {
            DrawHat();
            return;
        }
        if (DevConsole.showCollision)
        {
            Graphics.DrawRect(_featherVolume.rectangle, Color.LightGreen, 0.6f, filled: false, 0.5f);
        }
        _ = _renderingDuck;
        bool skipDraw = false;
        if (Network.isActive)
        {
            if (_trappedInstance != null && _trappedInstance.visible)
            {
                skipDraw = true;
            }
            if (_ragdollInstance != null && _ragdollInstance.visible)
            {
                skipDraw = true;
            }
            if (_cookedInstance != null && _cookedInstance.visible)
            {
                skipDraw = true;
            }
        }
        Depth prevDepth = base.Depth;
        if (!skipDraw)
        {
            if (!_renderingDuck)
            {
                if (!_updatedAnimation)
                {
                    UpdateAnimation();
                }
                _updatedAnimation = false;
                _sprite.UpdateFrame();
            }
            _sprite.flipH = offDir < 0;
            if (enteringWalldoor)
            {
                base.Depth = -0.55f;
            }
            _spriteArms.Depth = base.Depth + 11;
            _bionicArm.Depth = base.Depth + 11;
            DrawAIPath();
            SpriteMap spriteQuack = _spriteQuack;
            SpriteMap spriteControlled = _spriteControlled;
            SpriteMap sprite = _sprite;
            float num = (_spriteArms.Alpha = (_isGhost ? 0.5f : 1f) * base.Alpha);
            float num3 = (sprite.Alpha = num);
            float num5 = (spriteControlled.Alpha = num3);
            spriteQuack.Alpha = num5;
            SpriteMap spriteQuack2 = _spriteQuack;
            bool flipH = (_spriteControlled.flipH = _sprite.flipH);
            spriteQuack2.flipH = flipH;
            _spriteControlled.Depth = base.Depth;
            _sprite.Depth = base.Depth;
            _spriteQuack.Depth = base.Depth;
            SpriteMap sprite2 = _sprite;
            SpriteMap spriteQuack3 = _spriteQuack;
            num3 = (_spriteControlled.Angle = Angle);
            num5 = (spriteQuack3.Angle = num3);
            sprite2.Angle = num5;
            if (ragdoll != null && ragdoll.tongueStuck != Vec2.Zero)
            {
                quack = 10;
            }
            if (IsQuacking())
            {
                Vec2 rs = tounge;
                if (sliding)
                {
                    if (rs.Y < 0f)
                    {
                        rs.Y = 0f;
                    }
                    if (offDir > 0)
                    {
                        if (rs.X < -0.3f)
                        {
                            rs.X = -0.3f;
                        }
                        if (rs.X > 0.4f)
                        {
                            rs.X = 0.4f;
                        }
                    }
                    else
                    {
                        if (rs.X < -0.4f)
                        {
                            rs.X = -0.4f;
                        }
                        if (rs.X > 0.3f)
                        {
                            rs.X = 0.3f;
                        }
                    }
                }
                else
                {
                    if (offDir > 0 && rs.X < 0f)
                    {
                        rs.X = 0f;
                    }
                    if (offDir < 0 && rs.X > 0f)
                    {
                        rs.X = 0f;
                    }
                    if (rs.Y < -0.3f)
                    {
                        rs.Y = -0.3f;
                    }
                    if (rs.Y > 0.4f)
                    {
                        rs.Y = 0.4f;
                    }
                }
                _stickLerp = Lerp.Vec2Smooth(_stickLerp, rs, 0.2f);
                _stickSlowLerp = Lerp.Vec2Smooth(_stickSlowLerp, rs, 0.1f);
                Vec2 stick = _stickLerp;
                stick.Y *= -1f;
                Vec2 stick2 = _stickSlowLerp;
                stick2.Y *= -1f;
                int additionalFrame = 0;
                float length = stick.Length();
                if (length > 0.5f)
                {
                    additionalFrame = 72;
                }
                Graphics.Draw((_mindControl != null && _derpMindControl) ? _spriteControlled : _spriteQuack, _sprite.imageIndex + additionalFrame, base.X, base.Y + verticalOffset, base.ScaleX, base.ScaleY);
                if (length > 0.05f)
                {
                    Vec2 mouthPos = Position + new Vec2(0f, 1f);
                    if (sliding)
                    {
                        mouthPos.Y += 9;
                        mouthPos.X -= 4 * offDir;
                    }
                    else if (crouch)
                    {
                        mouthPos.Y += 4;
                    }
                    else if (!base.grounded)
                    {
                        mouthPos.Y -= 2;
                    }
                    List<Vec2> list = Curve.Bezier(8, mouthPos, mouthPos + stick2 * 6f, mouthPos + stick * 6f);
                    Vec2 prev = Vec2.Zero;
                    float lenMul = 1f;
                    foreach (Vec2 p in list)
                    {
                        if (prev != Vec2.Zero)
                        {
                            Vec2 dir = prev - p;
                            Graphics.DrawTexturedLine(Graphics.tounge.texture, prev + dir.Normalized * 0.4f, p, new Color(223, 30, 30), 0.15f * lenMul, Depth + 1);
                            Graphics.DrawTexturedLine(Graphics.tounge.texture, prev + dir.Normalized * 0.4f, p - dir.Normalized * 0.4f, Color.Black, 0.3f * lenMul, Depth - 1);
                        }
                        lenMul -= 0.1f;
                        prev = p;
                        tongueCheck = p;
                    }
                    if (_graphic != null)
                    {
                        _spriteQuack.Position = Position;
                        _spriteQuack.Alpha = Alpha;
                        _spriteQuack.Angle = Angle;
                        _spriteQuack.Depth = Depth + 2;
                        _spriteQuack.Scale = Scale;
                        _spriteQuack.Center = Center;
                        _spriteQuack.frame += 36;
                        _spriteQuack.Draw();
                        _spriteQuack.frame -= 36;
                    }
                }
                else
                {
                    tongueCheck = Vec2.Zero;
                }
            }
            else
            {
                Graphics.DrawWithoutUpdate(_sprite, X, Y + verticalOffset, ScaleX, ScaleY);
                _stickLerp = Vec2.Zero;
                _stickSlowLerp = Vec2.Zero;
            }
        }
        if (_renderingDuck)
        {
            if (holdObject != null)
            {
                holdObject.Draw();
            }
            foreach (Equipment item in _equipment)
            {
                item.Draw();
            }
        }
        if ((_mindControl != null && _derpMindControl) || listening)
        {
            _swirlSpin += 0.2f;
            _swirl.Angle = _swirlSpin;
            Graphics.Draw(_swirl, X, Y - 12);
        }
        DrawHat();
        if (!skipDraw)
        {
            Grapple g = GetEquipment(typeof(Grapple)) as Grapple;
            bool isGrapple = g != null;
            int hookOffset = 0;
            if (g != null && g.hookInGun)
            {
                hookOffset = 36;
            }
            _spriteArms.imageIndex = _sprite.imageIndex;
            if (!inNet && !_gripped && !listening)
            {
                Vec2 kickVector = Vec2.Zero;
                if (base.gun != null)
                {
                    kickVector = -base.gun.barrelVector * kick;
                }
                float flapper = Math.Abs(((float)(int)_flapFrame - 4f) / 4f) - 0.1f;
                if (!_hovering)
                {
                    flapper = 0f;
                }
                _spriteArms._frameInc = 0f;
                _spriteArms.flipH = _sprite.flipH;
                if (holdObject != null && !holdObject.ignoreHands && !holdObject.hideRightWing)
                {
                    _spriteArms.Angle = holdAngle;
                    _bionicArm.Angle = holdAngle;
                    if (!isGrapple)
                    {
                        bool oldFlip = _spriteArms.flipH;
                        if (holdObject.handFlip)
                        {
                            _spriteArms.flipH = !_spriteArms.flipH;
                        }
                        Graphics.Draw(_spriteArms, _sprite.imageIndex + 18 + Maths.Int(action) * 18 * (holdObject.hasTrigger ? 1 : 0), armPosition.X + holdObject.handOffset.X * offDir, armPosition.Y + holdObject.handOffset.Y, _sprite.ScaleX, _sprite.ScaleY);
                        _spriteArms._frameInc = 0f;
                        _spriteArms.flipH = oldFlip;
                        if (_sprite.currentAnimation == "jump")
                        {
                            _spriteArms.Angle = 0f;
                            _spriteArms.Depth = Depth + -10;
                            Graphics.Draw(_spriteArms, _sprite.imageIndex + 5 + (int)Math.Round(flapper * 2f), X + kickVector.X + (2 * offDir) * ScaleX, Y + kickVector.Y + armOffY * ScaleY, 0f - _sprite.ScaleX, _sprite.ScaleY, maintainFrame: true);
                            _spriteArms.Depth = Depth + 11;
                        }
                    }
                    else
                    {
                        _bionicArm.flipH = _sprite.flipH;
                        if (holdObject.handFlip)
                        {
                            _bionicArm.flipH = !_bionicArm.flipH;
                        }
                        Graphics.Draw(_bionicArm, _sprite.imageIndex + 18 + hookOffset, armPosition.X + holdObject.handOffset.X * offDir, armPosition.Y + holdObject.handOffset.Y, _sprite.ScaleX, _sprite.ScaleY);
                    }
                }
                else if (!_closingEyes)
                {
                    if (!isGrapple)
                    {
                        _spriteArms.Angle = 0f;
                        if (_sprite.currentAnimation == "jump" && _spriteArms.imageIndex == 9)
                        {
                            int offAmount = 2;
                            if (HasEquipment(typeof(ChestPlate)))
                            {
                                offAmount = 3;
                            }
                            if (holdObject == null || !holdObject.hideRightWing)
                            {
                                _spriteArms.Depth = base.Depth + 11;
                                Graphics.Draw(_spriteArms, _spriteArms.imageIndex + 5 + (int)Math.Round(flapper * 2f), base.X + kickVector.X - (float)(offDir * offAmount) * base.ScaleX, base.Y + kickVector.Y + armOffY * base.ScaleY, _sprite.ScaleX, _sprite.ScaleY, maintainFrame: true);
                                _spriteArms.Depth = base.Depth + -10;
                            }
                            if (holdObject == null || !holdObject.hideLeftWing)
                            {
                                _spriteArms.imageIndex = 9;
                                Graphics.Draw(_spriteArms, _spriteArms.imageIndex + 5 + (int)Math.Round(flapper * 2f), base.X + kickVector.X + (float)(2 * offDir) * base.ScaleX, base.Y + kickVector.Y + armOffY * base.ScaleY, 0f - _sprite.ScaleX, _sprite.ScaleY, maintainFrame: true);
                                _spriteArms.Depth = base.Depth + 11;
                            }
                        }
                        else if (holdObject == null || !holdObject.hideRightWing)
                        {
                            Graphics.Draw(_spriteArms, _sprite.imageIndex, armPosition.X, armPosition.Y, _sprite.ScaleX, _sprite.ScaleY);
                        }
                    }
                    else
                    {
                        _bionicArm.Angle = 0f;
                        _bionicArm.flipH = _sprite.flipH;
                        Graphics.Draw(_bionicArm, _sprite.imageIndex + hookOffset, armPosition.X, armPosition.Y, _sprite.ScaleX, _sprite.ScaleY);
                    }
                }
            }
        }
        if (Network.isActive && !_renderingDuck)
        {
            DrawConnectionIndicators();
        }
        Sprite gr = graphic;
        graphic = null;
        base.Draw();
        graphic = gr;
        if (enteringWalldoor)
        {
            base.Depth = prevDepth;
        }
    }

    public void UpdateConnectionIndicators()
    {
        if (_indicators == null)
        {
            _indicators = new ConnectionIndicators
            {
                duck = this
            };
        }
        _indicators.Update();
    }

    public void DrawConnectionIndicators()
    {
        if (_indicators == null)
        {
            _indicators = new ConnectionIndicators
            {
                duck = this
            };
        }
        _indicators.Draw();
    }

    private void DrawAIPath()
    {
        if (ai != null)
        {
            ai.Draw();
        }
    }
}
