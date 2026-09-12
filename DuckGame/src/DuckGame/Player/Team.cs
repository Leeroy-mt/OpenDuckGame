using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

#if FACEPUNCH
using Steamworks;
#else
using Steam;
#endif

namespace DuckGame;

public class Team
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class Metapixel : Attribute
    {
        #region Public Fields

        public readonly int index;

        public readonly string name;

        public readonly string description;

        #endregion

        public Metapixel(int pIndex, string pName, string pDescription)
        {
            index = pIndex;
            name = pName;
            description = pDescription;
        }
    }

    public class CustomMetadata
    {
        #region Public Fields

        public static HatMetadataElement kPreviousParameter;

        public static HatMetadataElement kCurrentParameter;

        public Dictionary<int, HatMetadataElement> _fieldMap = [];

        #endregion

        #region Public Methods

        public int Index(HatMetadataElement pParameter)
        {
            foreach (var p in _fieldMap)
            {
                if (p.Value == pParameter)
                    return p.Key;
            }
            return -1;
        }

        public virtual bool Deserialize(Color pColor)
        {
            if (_fieldMap.TryGetValue(pColor.R, out var el))
            {
                if (el is not CustomHatMetadata.MDRandomizer)
                    kCurrentParameter = el;

                el.Parse(pColor);

                if (el is not CustomHatMetadata.MDRandomizer)
                    kPreviousParameter = el;

                return true;
            }
            return false;
        }

        public static Dictionary<Func<object, object>, Metapixel> PrepareParameterAttributes(Type pType)
        {
            Dictionary<Func<object, object>, Metapixel> parameterAttributes = [];
            foreach (var f in from x in pType.GetFields(BindingFlags.Instance | BindingFlags.Public)
                              where typeof(HatMetadataElement).IsAssignableFrom(x.FieldType)
                              select x)
            {
                parameterAttributes[Editor.BuildGetAccessorField(pType, f)] = f.GetCustomAttribute<Metapixel>();
            }
            return parameterAttributes;
        }

        public static Dictionary<int, HatMetadataElement> PrepareFieldMap(Dictionary<Func<object, object>, Metapixel> pParameterAttributes, object pFor)
        {
            Dictionary<int, HatMetadataElement> fieldMap = [];
            foreach (KeyValuePair<Func<object, object>, Metapixel> pair in pParameterAttributes)
                fieldMap[pair.Value.index] = (HatMetadataElement)pair.Key(pFor);
            return fieldMap;
        }

        #endregion
    }

    public abstract class HatMetadataElement
    {
        #region Public Fields

        public bool set;

        public Vector2 randomizerX;

        public Vector2 randomizerY;

        #endregion

        public abstract void Parse(Color pColor);
    }

    public class CustomHatMetadata : CustomMetadata
    {
        public abstract class V<T> : HatMetadataElement
        {
            #region Public Fields

            public int defaultCopyIndex;

            public Action postParseScript;

            #endregion

            protected T _value;

            #region Public Properties

            protected V<T> _defaultCopy
            {
                get
                {
                    if (defaultCopyIndex != 0 && kCurrentMetadata != null && kCurrentMetadata._fieldMap.TryGetValue(defaultCopyIndex, out var m))
                        return m as V<T>;
                    return null;
                }
            }

            public virtual T value
            {
                get => _value;
                set => _value = value;
            }

            #endregion

            #region Public Methods

            public override void Parse(Color pColor)
            {
                randomizerX = Vector2.Zero;
                randomizerY = Vector2.Zero;
                set = true;
                OnParse(pColor);
                postParseScript?.Invoke();
            }

            public abstract void OnParse(Color pColor);

            #endregion
        }

        public class MDVec2 : V<Vector2>
        {
            #region Public Fields

            public bool allowNegative = true;

            public float range = 16;

            #endregion

            public override Vector2 value
            {
                get
                {
                    Vector2 modified = _value;
                    if (_value == Vector2.MaxValue && _defaultCopy != null)
                        modified = _defaultCopy.value;

                    if (randomizerX != Vector2.Zero)
                        modified.X = Rando.Float(_value.X * randomizerX.X, _value.X * randomizerX.Y);

                    if (randomizerY == Vector2.MaxValue)
                        modified.Y = modified.X;
                    else if (randomizerY != Vector2.Zero)
                        modified.Y = Rando.Float(_value.Y * randomizerY.X, _value.Y * randomizerY.Y);

                    if (allowNegative)
                    {
                        modified.X = Maths.Clamp(modified.X, 0f - range, range);
                        modified.Y = Maths.Clamp(modified.Y, 0f - range, range);
                    }
                    else
                    {
                        modified.X = Maths.Clamp(modified.X, 0f, range);
                        modified.Y = Maths.Clamp(modified.Y, 0f, range);
                    }

                    return modified;
                }
                set => _value = value;
            }

            public override void OnParse(Color pColor)
            {
                float xOffset = Maths.Clamp(pColor.G - 128, 0f - range, range),
                      yOffset = Maths.Clamp(pColor.B - 128, 0f - range, range);
                _value = new Vector2(xOffset, yOffset);
            }
        }

        public class MDVec2Normalized : MDVec2
        {
            public MDVec2Normalized()
            {
                range = 1;
                allowNegative = false;
            }

            public override void OnParse(Color pColor)
            {
                if (allowNegative)
                {
                    float realRange = range;
                    range = 127f;
                    base.OnParse(pColor);
                    _value /= range;
                    range = realRange;
                }
                else
                {
                    _value = new Vector2(pColor.G / 255F, pColor.B / 255F);
                }
                _value *= range;
            }
        }

        public class MDRandomizer : MDVec2Normalized
        {
            #region Public Fields

            public bool randomizeY;

            public bool randomizeBoth;

            #endregion

            public override void OnParse(Color pColor)
            {
                base.OnParse(pColor);
                if (kPreviousParameter != null)
                {
                    if (!randomizeY)
                        kPreviousParameter.randomizerX = value;
                    else
                        kPreviousParameter.randomizerY = value;

                    if (randomizeBoth)
                        kPreviousParameter.randomizerY = Vector2.MaxValue;
                }
            }
        }

        public class MDBool : V<bool>
        {
            public override void OnParse(Color pColor)
            {
                _value = true;
            }
        }

        public class MDFloat : V<float>
        {
            #region Public Fields

            public bool allowNegative;

            public float range = 1;

            #endregion

            public override float value
            {
                get
                {
                    var modified = _value;
                    if (_value == float.MaxValue && _defaultCopy != null)
                        modified = _defaultCopy.value;

                    if (randomizerX != Vector2.Zero)
                        modified = Rando.Float(_value * randomizerX.X, _value * randomizerX.Y);

                    if (allowNegative)
                        return Maths.Clamp(modified, 0 - range, range);

                    return Maths.Clamp(modified, 0, range);
                }
                set => _value = value;
            }

            public override void OnParse(Color pColor)
            {
                if (allowNegative)
                    _value = (pColor.G - 128) / 128F;
                else
                    _value = pColor.G / 255F;

                _value *= range;
            }
        }

        public class MDInt : V<int>
        {
            #region Public Fields

            public bool allowNegative;

            public int range = 255;

            #endregion

            public override int value
            {
                get
                {
                    float modified = _value;
                    if (_value == int.MaxValue && _defaultCopy != null)
                        modified = _defaultCopy.value;

                    if (randomizerX != Vector2.Zero)
                        modified = Rando.Float(_value * randomizerX.X, _value * randomizerX.Y);

                    return (int)Math.Round(modified);
                }
                set => _value = value;
            }

            public override void OnParse(Color pColor)
            {
                if (allowNegative)
                    _value = pColor.G - 128;
                else
                    _value = pColor.G;

                _value = Maths.Clamp(value, -range, range);
            }
        }

        public class MDIntPair : V<Vector2>
        {
            #region Public Fields

            public bool allowNegative;

            public int rangeX = 255;

            public int rangeY = 255;

            #endregion

            public override Vector2 value
            {
                get
                {
                    var modified = _value;
                    if (_value == Vector2.MaxValue && base._defaultCopy != null)
                        modified = _defaultCopy.value;

                    if (randomizerX != Vector2.Zero)
                        modified.X = (float)Math.Round(Rando.Float(_value.X * randomizerX.X, _value.X * randomizerX.Y));

                    if (randomizerY == Vector2.MaxValue)
                        modified.Y = modified.X;
                    else if (randomizerY != Vector2.Zero)
                        modified.Y = (float)Math.Round(Rando.Float(_value.Y * randomizerY.X, _value.Y * randomizerY.Y));

                    return modified;
                }
                set => _value = value;
            }

            public override void OnParse(Color pColor)
            {
                if (allowNegative)
                {
                    _value.X = pColor.G - 128;
                    _value.Y = pColor.B - 128;
                }
                else
                {
                    _value.X = pColor.G;
                    _value.Y = pColor.B;
                }

                _value.X = Maths.Clamp(value.X, -rangeX, rangeX);
                _value.Y = Maths.Clamp(value.Y, -rangeY, rangeY);
            }
        }

        public string hatPath;

        [Metapixel(1, "Hat Offset", "Hat offset position in pixels")]
        public MDVec2 HatOffset = new()
        {
            range = 16
        };

        [Metapixel(2, "Use Duck Color", "If this metapixel exists, White (255, 255, 255) and Grey(157, 157, 157) will be recolored to duck colors.")]
        public MDBool UseDuckColor = new();

        [Metapixel(3, "Hat No-Flip", "If this metapixel exists, the hat will not be flipped with the direction of the duck.")]
        public MDBool HatNoFlip = new();

        [Metapixel(10, "Cape Offset", "Cape offset position in pixels")]
        public MDVec2 CapeOffset = new()
        {
            range = 16
        };

        [Metapixel(11, "Cape Is Foreground", "If this metapixel exists, the cape will be drawn over the duck.")]
        public MDBool CapeForeground = new();

        [Metapixel(12, "Cape Sway Modifier", "Affects cape length, and left to right sway.")]
        public MDVec2Normalized CapeSwayModifier = new()
        {
            value = new Vector2(0.3f, 1),
            allowNegative = true
        };

        [Metapixel(13, "Cape Wiggle Modifier", "Affects how much the cape wiggles in the wind.")]
        public MDVec2Normalized CapeWiggleModifier = new()
        {
            value = new Vector2(1, 1),
            allowNegative = true
        };

        [Metapixel(14, "Cape Taper Start", "Affects how narrow the cape/trail is at the top/beginning.")]
        public MDFloat CapeTaperStart = new()
        {
            value = 0.5f
        };

        [Metapixel(15, "Cape Taper End", "Affects how narrow the cape/trail is at the bottom/end.")]
        public MDFloat CapeTaperEnd = new()
        {
            value = 1
        };

        [Metapixel(16, "Cape Alpha Start", "Affects how transparent the cape/trail is at the top/beginning.")]
        public MDFloat CapeAlphaStart = new()
        {
            value = 1
        };

        [Metapixel(17, "Cape Alpha End", "Affects how transparent the cape/trail is at the bottom/end.")]
        public MDFloat CapeAlphaEnd = new()
        {
            value = 1
        };

        [Metapixel(20, "Cape Is Trail", "If this metapixel exists, the cape will be a trail instead of a cape (think of the rainbow trail left by the TV object).")]
        public MDBool CapeIsTrail = new();

        [Metapixel(30, "Particle Emitter Offset", "The offset in pixels from the center of the hat where particles will be emitted.")]
        public MDVec2 ParticleEmitterOffset = new()
        {
            range = 16
        };

        [Metapixel(31, "Particle Default Behavior", "B defines a particle behavior from a list of presets: 0 = No Behavior, 1 = Spit, 2 = Burst, 3 = Halo, 4 = Exclamation")]
        public MDInt ParticleDefaultBehavior = new()
        {
            range = 4,
            postParseScript = ApplyDefaultParticleBehavior
        };

        [Metapixel(32, "Particle Emit Shape", "G: 0 = Point, 1 = Circle, 2 = Box   B: 0 = Emit Around Shape Border Randomly, 1 = Fill Shape Randomly, 2 = Emit Around Shape Border Uniformly")]
        public MDIntPair ParticleEmitShape = new()
        {
            rangeX = 2,
            rangeY = 2
        };

        [Metapixel(33, "Particle Emit Shape Size", "X and Y size of the particle emitter (in pixels)")]
        public MDVec2 ParticleEmitShapeSize = new()
        {
            range = 32,
            value = new Vector2(24, 24)
        };

        [Metapixel(34, "Particle Count", "The number of particles to emit.")]
        public MDInt ParticleCount = new()
        {
            range = 8,
            value = 4
        };

        [Metapixel(35, "Particle Lifespan", "Life span of the particle, in seconds.")]
        public MDFloat ParticleLifespan = new()
        {
            range = 2,
            value = 1
        };

        [Metapixel(36, "Particle Velocity", "Initial velocity of the particle.")]
        public MDVec2Normalized ParticleVelocity = new()
        {
            range = 2,
            allowNegative = true
        };

        [Metapixel(37, "Particle Gravity", "Gravity applied to the particle.")]
        public MDVec2Normalized ParticleGravity = new()
        {
            range = 2,
            allowNegative = true,
            value = new Vector2(0, PhysicsObject.gravity)
        };

        [Metapixel(38, "Particle Friction", "Friction applied to the particle (The value it's velocity is multiplied by every frame).")]
        public MDVec2Normalized ParticleFriction = new()
        {
            range = 1,
            allowNegative = false,
            value = new Vector2(1, 1)
        };

        [Metapixel(39, "Particle Alpha", "G = Start alpha, B = End alpha")]
        public MDVec2Normalized ParticleAlpha = new()
        {
            range = 1,
            allowNegative = false,
            value = new Vector2(1, 1)
        };

        [Metapixel(40, "Particle Scale", "G = Start scale, B = End scale")]
        public MDVec2Normalized ParticleScale = new()
        {
            range = 2,
            allowNegative = false,
            value = new Vector2(1, 0)
        };

        [Metapixel(41, "Particle Rotation", "G = Start rotation, B = End rotation")]
        public MDVec2Normalized ParticleRotation = new()
        {
            range = 36,
            value = new Vector2(0, 0)
        };

        [Metapixel(42, "Particle Offset", "Additional X Y offset of particle.")]
        public MDVec2 ParticleOffset = new()
        {
            range = 16
        };

        [Metapixel(43, "Particle Background", "If this metapixel exists, particles will be rendered behind the duck.")]
        public MDBool ParticleBackground = new();

        [Metapixel(44, "Particle Anchor", "If this metapixel exists, particles will stay anchored around the hat position when it's moving.")]
        public MDBool ParticleAnchor = new();

        [Metapixel(45, "Particle Animated", "If this metapixel exists, particles will animate through their frames. Otherwise, a frame will be picked randomly.")]
        public MDBool ParticleAnimated = new();

        [Metapixel(46, "Particle Animation Loop", "If this metapixel exists, the particle animation will loop.")]
        public MDBool ParticleAnimationLoop = new();

        [Metapixel(47, "Particle Animation Random Frame", "If this metapixel exists, the particle animation will start on a random frame.")]
        public MDBool ParticleAnimationRandomFrame = new();

        [Metapixel(48, "Particle Animation Speed", "How quickly the particle animates.")]
        public MDFloat ParticleAnimationSpeed = new()
        {
            range = 1,
            value = 0.1f
        };

        [Metapixel(49, "Particle Anchor Orientation", "If this metapixel exists, particles will flip and rotate to orient with the hat.")]
        public MDBool ParticleAnchorOrientation = new();

        [Metapixel(60, "Quack Delay", "Amount of time in between pressing the quack button and the quack frame appearing.")]
        public MDFloat QuackDelay = new()
        {
            range = 2,
            value = 0
        };

        [Metapixel(61, "Quack Hold", "Minimum amount of time to keep the quack frame held, even if the quack button is released.")]
        public MDFloat QuackHold = new MDFloat
        {
            range = 2,
            value = 0
        };

        [Metapixel(62, "Quack Suppress Requack", "If this metapixel exists, a new quack will not be allowed to begin until Quack Delay and Quack Hold are finished.")]
        public MDBool QuackSuppressRequack = new();

        [Metapixel(70, "Wet Lips", "If this metapixel exists, the hat will have 'wet lips'.")]
        public MDBool WetLips = new();

        [Metapixel(71, "Mechanical Lips", "If this metapixel exists, the hat will have 'mechanical lips'.")]
        public MDBool MechanicalLips = new();

        [Metapixel(100, "Randomize Parameter X", "If present, the previously defined metapixel value will have it's X value multiplied by a random normalized number between G and B each time it's used. This will generally only work with particles..")]
        public MDRandomizer RandomizeParameterX = new()
        {
            range = 1,
            allowNegative = true
        };

        [Metapixel(101, "Randomize Parameter Y", "If present, the previously defined metapixel value will have it's Y value multiplied by a random normalized number between G and B each time it's used. This will generally only work with particles..")]
        public MDRandomizer RandomizeParameterY = new()
        {
            range = 1,
            allowNegative = true,
            randomizeY = true
        };

        [Metapixel(102, "Randomize Parameter", "If present, the previously defined metapixel value will have a random number between G and B applied to its X and Y values each time it's used. This will generally only work with particles..")]
        public MDRandomizer RandomizeParameter = new()
        {
            range = 1,
            allowNegative = true,
            randomizeBoth = true
        };

        public Team team;

        static CustomHatMetadata kCurrentMetadata;

        static Dictionary<Func<object, object>, Metapixel> kParameterAttributes;

        static void ApplyDefaultParticleBehavior()
        {
            var value = kCurrentMetadata.ParticleDefaultBehavior.value;

            if (value == 1)
            {
                kCurrentMetadata.ParticleEmitShape.value = new Vector2(0, 0);
                kCurrentMetadata.ParticleOffset.value = new Vector2(2, 2);
                kCurrentMetadata.ParticleOffset.randomizerX = new Vector2(-1, 1);
                kCurrentMetadata.ParticleOffset.randomizerY = new Vector2(-1, 1);
                kCurrentMetadata.ParticleVelocity.value = new Vector2(3, 1.5f);
                kCurrentMetadata.ParticleVelocity.randomizerX = new Vector2(0.3f, 1);
                kCurrentMetadata.ParticleVelocity.randomizerY = new Vector2(-1, 0.3f);
                kCurrentMetadata.ParticleScale.value = new Vector2(1, 1);
                kCurrentMetadata.ParticleScale.randomizerX = new Vector2(0.7f, 1);
                kCurrentMetadata.ParticleScale.randomizerY = Vector2.MaxValue;
                kCurrentMetadata.ParticleCount.value = 5;
                kCurrentMetadata.ParticleCount.randomizerX = new Vector2(0.3f, 1);
                kCurrentMetadata.ParticleBackground.value = false;
            }

            if (value == 2)
            {
                kCurrentMetadata.ParticleEmitShape.value = new Vector2(0, 0);
                kCurrentMetadata.ParticleOffset.value = new Vector2(2, 2);
                kCurrentMetadata.ParticleOffset.randomizerX = new Vector2(-1, 1);
                kCurrentMetadata.ParticleOffset.randomizerY = new Vector2(-1, 1);
                kCurrentMetadata.ParticleVelocity.value = new Vector2(1.5f, 2.5f);
                kCurrentMetadata.ParticleVelocity.randomizerX = new Vector2(-1, 1);
                kCurrentMetadata.ParticleVelocity.randomizerY = new Vector2(-1, 1);
                kCurrentMetadata.ParticleScale.value = new Vector2(1, 0);
                kCurrentMetadata.ParticleScale.randomizerX = new Vector2(0.7f, 1);
                kCurrentMetadata.ParticleCount.value = 8;
                kCurrentMetadata.ParticleCount.randomizerX = new Vector2(0.5f, 1);
                kCurrentMetadata.ParticleBackground.value = false;
            }

            if (value == 3)
            {
                kCurrentMetadata.ParticleEmitShape.value = new Vector2(1, 2);
                kCurrentMetadata.ParticleAlpha.value = new Vector2(1, 0);
                kCurrentMetadata.ParticleCount.value = 8;
                kCurrentMetadata.ParticleBackground.value = true;
                kCurrentMetadata.ParticleGravity.value = new Vector2(0, 0);
                kCurrentMetadata.ParticleAnchor.value = true;
            }

            if (value == 4)
            {
                kCurrentMetadata.ParticleEmitShape.value = new Vector2(0, 0);
                kCurrentMetadata.ParticleScale.value = new Vector2(0.3f, 1.5f);
                kCurrentMetadata.ParticleCount.value = 1;
                kCurrentMetadata.ParticleBackground.value = false;
                kCurrentMetadata.ParticleGravity.value = new Vector2(0, 0);
                kCurrentMetadata.ParticleAnchor.value = true;
                kCurrentMetadata.ParticleVelocity.value = new Vector2(1.4f, -1.2f);
                kCurrentMetadata.ParticleFriction.value = new Vector2(0.92f, 0.9f);
                kCurrentMetadata.ParticleLifespan.value = 0.8f;
            }
        }

        public CustomHatMetadata(Team pTeam)
        {
            team = pTeam;
            kCurrentMetadata = this;
            kParameterAttributes ??= PrepareParameterAttributes(GetType());
            _fieldMap = PrepareFieldMap(kParameterAttributes, this);
            ApplyDefaultParticleBehavior();
        }

        public override bool Deserialize(Color pColor)
        {
            if (!base.Deserialize(pColor))
            {
                DevConsole.Log(DCSection.General, $"Metapixel with invalid ID value ({pColor.R}) found in custom hat.");
                return false;
            }
            return true;
        }
    }

    #region Public Fields

    public static bool networkDeserialize = false;

    public static int currentLoadHat;
    public static int totalLoadHats;

    public static Team deserializeInto;

    public static List<string> hatSearchPaths =
    [
        $"{Directory.GetCurrentDirectory()}/Hats",
        $"{DuckFile.saveDirectory}/Hats",
        $"{DuckFile.saveDirectory}/Custom/Hats"
    ];
    public static List<Team> deserializedTeams = [];

    public bool capeRequestSuccess = true;
    public bool inDemo;
    public bool isFolder;
    public bool defaultTeam;
    public bool isTemporaryTeam;
    public bool isHair;
    public bool noCrouchOffset;

    public string customHatPath;
    public string fullFolderPath;

    public Vector2 prevTreeDraw = Vector2.Zero;

    public NetworkConnection customConnection;
    public Texture2D _capeTexture;
    public Texture2D _rockTexture;
    public CustomHatMetadata _basicMetadata;
    public CustomHatMetadata _metadata;
    public Team folder;
    public Texture2D folderTexture;
    public Profile owner;
    public List<Texture2D> _customParticles = [];

    #endregion

    #region Private Fields

    static readonly long kPngHatKey = 630430737023345;

    bool _locked;

    int _score;
    int _rockScore;
    int _wins;
    int _prevScoreboardScore;

    string _name = "";
    string _description = "";
    string _hatID;

    Vector2 _hatOffset;

    SpriteMap _default;
    SpriteMap _hat;

    byte[] _customData;

    Dictionary<DuckPersona, SpriteMap> _recolors = [];
    List<Profile> _activeProfiles = [];


    #endregion

    #region Public Properties

    public bool filter
    {
        get
        {
            if (customConnection == null)
                return false;

            Profile p = customConnection.profile;
            if (p != null && p.muteHat)
                return true;

            bool filter = Options.Data.hatFilter == 2;
#if FACEPUNCH
            if (Options.Data.hatFilter == 1 && customConnection.data is Friend friend && friend.Relationship != Relationship.Friend)
#else
            if (Options.Data.hatFilter == 1 && customConnection.data is User && (customConnection.data as User).Relationship != FriendRelationship.Friend)
#endif
                filter = true;

            return filter;
        }
    }
    public bool hasHat
    {
        get
        {
            if (hat != null)
                return hat.texture.Name != "hats/noHat";
            return false;
        }
    }
    public bool locked
    {
        get
        {
            if (!NetworkDebugger.enabled || NetworkDebugger.currentIndex != 1)
                return _locked;

            if (Teams.all.IndexOf(this) > 15)
                return Teams.all.IndexOf(this) < 35;

            return false;
        }
        set => _locked = value;
    }

    public int score
    {
        get => _score;
        set => _score = value;
    }
    public int rockScore
    {
        get => _rockScore;
        set => _rockScore = value;
    }
    public int wins
    {
        get => _wins;
        set => _wins = value;
    }
    public int prevScoreboardScore
    {
        get => _prevScoreboardScore;
        set => _prevScoreboardScore = value;
    }
    public int numMembers => _activeProfiles.Count;

    public string name => _name;
    public string description => _description;
    public string currentDisplayName
    {
        get
        {
            var n = "";
            if (activeProfiles != null && activeProfiles.Count > 0)
                n = (activeProfiles.Count > 1) ? GetNameForDisplay() : ((!Profiles.IsDefault(activeProfiles[0]) || Network.isActive) ? activeProfiles[0].nameUI : GetNameForDisplay());
            return n;
        }
    }
    public string hatID
    {
        get => GetFacadeTeam()._hatID;
        set => _hatID = value;
    }

    public Vector2 hatOffset
    {
        get
        {
            if (filter)
                return Vector2.Zero;
            return GetFacadeTeam()._hatOffset;
        }
        set => _hatOffset = value;
    }

    public Texture2D capeTexture
    {
        get
        {
            if (filter)
                return null;
            return GetFacadeTeam()._capeTexture;
        }
    }
    public Texture2D rockTexture
    {
        get
        {
            if (filter)
                return null;
            return GetFacadeTeam()._rockTexture;
        }
    }

    public SpriteMap hat
    {
        get
        {
            if (filter)
                return _default ??= new SpriteMap("hats/default", 32, 32);
            return GetFacadeTeam()._hat;
        }
    }

    public CustomHatMetadata metadata
    {
        get
        {
            if (!filter)
                return GetFacadeTeam()._metadata;

            if (_basicMetadata == null)
            {
                _basicMetadata = new CustomHatMetadata(this);
                _basicMetadata.UseDuckColor.value = true;
            }

            return _basicMetadata;
        }
        set => _metadata = value;
    }

    public Team facade
    {
        get
        {
            Team t = GetFacadeTeam();
            if (t == this)
                return null;
            return t;
        }
    }

    public List<Texture2D> customParticles
    {
        get
        {
            if (filter)
                return [];
            return GetFacadeTeam()._customParticles;
        }
    }
    public List<Profile> activeProfiles => _activeProfiles;

    public byte[] customData
    {
        get => GetFacadeTeam()._customData;
        set => _customData = value;
    }

    #endregion

    #region Constructors

    public Team(string varName, string hatTexture, bool demo = false, bool lockd = false, Vector2 hatOff = default, string desc = "", Texture2D capeTex = null)
    {
        _name = varName;
        _hat = new SpriteMap(hatTexture, 32, 32);
        _hatOffset = hatOff;
        inDemo = demo;
        _locked = lockd;
        _description = desc;
        _capeTexture = capeTex;
    }

    public Team(bool varHair, string varName, string hatTexture, bool demo = false, bool lockd = false, Vector2 hatOff = default, string desc = "", Texture2D capeTex = null)
        : this(varName, hatTexture, demo, lockd, hatOff, desc, capeTex)
    {
        _name = varName;
        _hat = new SpriteMap(hatTexture, 32, 32);
        _hatOffset = hatOff;
        inDemo = demo;
        _locked = lockd;
        _description = desc;
        _capeTexture = capeTex;
        isHair = varHair;
    }

    public Team(string varName, string hatTexture, bool demo, bool lockd, Vector2 hatOff)
    {
        _name = varName;
        _hat = new SpriteMap(hatTexture, 32, 32);
        _hatOffset = hatOff;
        inDemo = demo;
        _locked = lockd;
    }

    public Team(string varName, Texture2D hatTexture, bool demo = false, bool lockd = false, Vector2 hatOff = default, string desc = "")
    {
        Construct(varName, hatTexture, demo, lockd, hatOff, desc);
    }

    public Team(string varName, Texture2D hatTexture, bool demo, bool lockd, Vector2 hatOff)
    {
        _name = varName;
        _hat = new SpriteMap(hatTexture, 32, 32);
        _hatOffset = hatOff;
        inDemo = demo;
        _locked = lockd;
    }

    #endregion

    #region Public Methods

    public SpriteMap GetHat(DuckPersona pPersona)
    {
        if (metadata == null || !metadata.UseDuckColor.value)
            return hat;

        if (!_recolors.TryGetValue(pPersona, out var s))
        {
            s = new SpriteMap(Graphics.RecolorNew(hat.texture, pPersona.color.ToColor(), pPersona.colorDark.ToColor()), 32, 32);
            _recolors[pPersona] = s;
        }

        return s;
    }

    public Team Clone()
    {
        return new Team(name, _hat.texture)
        {
            _metadata = _metadata,
            _rockTexture = _rockTexture,
            _capeTexture = _capeTexture,
            _customData = _customData,
            _customParticles = _customParticles,
            customConnection = customConnection
        };
    }

    public static Team Deserialize(string file)
    {
        if (!DuckFile.FileExists(file))
            return null;

        if (file.EndsWith(".png"))
            return DeserializeFromPNG(file);

        return Deserialize(File.ReadAllBytes(file), file);
    }

    public static Team DeserializeFromPNG(string pFile)
    {
        try
        {
            if (pFile.EndsWith("folder_preview.png"))
                return null;

            var pData = File.ReadAllBytes(pFile);
            var name = Path.GetFileNameWithoutExtension(pFile);

            return DeserializeFromPNG(pData, name, pFile);
        }
        catch
        {
            return null;
        }
    }

    public static Team DeserializeFromPNG(byte[] pData, string pName, string pPath, bool pIgnoreSizeRestriction = false)
    {
        try
        {
            using MemoryStream memory = new(pData);
            var tex = TextureConverter.TextureFromStream(Graphics.device, memory, true);
            _ = tex.Width / 32F % 1f;
            Team newTeam = deserializeInto;

            if (newTeam == null)
                newTeam = new Team(pName, tex);
            else
                newTeam.Construct(pName, tex);

            deserializeInto = null;
            newTeam.hatID = CRC32.Generate(pData).ToString();
            BitBuffer teamData = new BitBuffer();
            teamData.Write(kPngHatKey);
            teamData.Write(pName);
            teamData.Write(new BitBuffer(pData));
            newTeam.customData = teamData.buffer;

            if (tex.Width >= 96)
            {
                Color[] rawData = new Color[1024];
                tex.GetData(0, new Rectangle(64, 0, 32, 32), rawData, 0, 1024);
                newTeam._capeTexture = new Texture2D(Graphics.device, 32, 32);
                newTeam._capeTexture.SetData(rawData);
                newTeam.capeRequestSuccess = true;
            }

            if (tex.Height >= 56)
            {
                var rockData = new Color[576];
                tex.GetData(0, new Rectangle(0, 32, 24, 24), rockData, 0, 576);
                if (CheckForPixelData(rockData, 16))
                {
                    newTeam._rockTexture = new Texture2D(Graphics.device, 24, 24);
                    newTeam._rockTexture.SetData(rockData);
                }

                if (tex.Width > 32)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var particleData = new Color[144];
                        tex.GetData(0, new Rectangle(24 + 12 * (i % 2), 32 + 12 * (i / 2), 12, 12), particleData, 0, 144);
                        if (CheckForPixelData(particleData))
                        {
                            Texture2D particleTex = new(Graphics.device, 12, 12);
                            particleTex.SetData(particleData);
                            newTeam.customParticles.Add(particleTex);
                        }
                    }
                }
            }

            ProcessMetadata(tex, newTeam);
            newTeam.customHatPath = pPath;
            return newTeam;
        }
        catch
        {
            return null;
        }
    }

    public static void LoadCustomHatsFromFolder(string pFolder, string pExtension)
    {
        var files = DuckFile.GetFiles(pFolder, $"*.{pExtension}");
        foreach (string f in files)
        {
            totalLoadHats++;
            currentLoadHat++;
            Team team;

            if (pExtension == "png")
            {
                var pData = File.ReadAllBytes(f);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(f);
                team = DeserializeFromPNG(pData, fileNameWithoutExtension, f);
            }
            else
            {
                team = Deserialize(f);
            }

            if (team != null)
                deserializedTeams.Add(team);
        }
    }

    public static void DeserializeCustomHats(IProgress<float> progress)
    {
        var hatsTotalCount = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.hat").Length;
        foreach (var hatSearchPath in hatSearchPaths)
        {
            if (!Directory.Exists(hatSearchPath))
                continue;

            hatsTotalCount += Directory.GetFiles(hatSearchPath, "*.hat").Length;
            hatsTotalCount += Directory.GetFiles(hatSearchPath, "*.png").Length;
        }

        foreach (var hatSearchPath in hatSearchPaths)
        {
            if (!Directory.Exists(hatSearchPath))
                continue;

            foreach (var f in Directory.EnumerateFiles(hatSearchPath))
            {
                var extension = Path.GetExtension(f);
                if (extension is not "png" and "hat")
                    continue;

                totalLoadHats++;
                currentLoadHat++;

                progress?.Report(totalLoadHats / (float)hatsTotalCount);

                Team team;

                if (extension == "png")
                {
                    var pData = File.ReadAllBytes(f);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(f);
                    team = DeserializeFromPNG(pData, fileNameWithoutExtension, f);
                }
                else
                {
                    team = Deserialize(f);
                }

                if (team != null)
                    deserializedTeams.Add(team);
            }
        }

        //LoadCustomHatsFromFolder(Directory.GetCurrentDirectory(), "hat");
        //foreach (string hatSearchPath in hatSearchPaths)
        //{
        //    LoadCustomHatsFromFolder(hatSearchPath, "hat");
        //    LoadCustomHatsFromFolder(hatSearchPath, "png");
        //}
    }

    public static bool CheckForPixelData(Color[] pColors, int pMinimumNumberOfPixels = 1)
    {
        int numValid = 0;
        for (int i = 0; i < pColors.Length; i++)
        {
            if ((pColors[i].R != byte.MaxValue || pColors[i].G != 0 || pColors[i].B != byte.MaxValue) && pColors[i] != Colors.Transparent)
                numValid++;
        }
        return numValid >= pMinimumNumberOfPixels;
    }

    public static void ProcessMetadata(Texture2D pTex, Team pTeam)
    {
        int metadataWidth = pTex.Width % 32;
        if (pTex.Width > 100 || metadataWidth <= 0)
            return;

        pTeam.metadata = new CustomHatMetadata(pTeam);
        var meta = new Color[metadataWidth * Math.Min(pTex.Height, 56)];
        pTex.GetData(0, new Rectangle(pTex.Width - metadataWidth, 0, metadataWidth, Math.Min(pTex.Height, 56)), meta, 0, meta.Length);
        for (int i = 0; i < meta.Length; i++)
        {
            Color c = meta[i];
            if ((c.R != byte.MaxValue || c.G != 0 || c.B != byte.MaxValue) && c != Colors.Transparent)
                pTeam.metadata.Deserialize(c);
        }
        pTeam.hatOffset = pTeam.metadata.HatOffset.value;
    }

    public static Team Deserialize(byte[] teamData)
    {
        return Deserialize(teamData, null);
    }

    public static Team Deserialize(byte[] teamData, string pPath)
    {
        try
        {
            if (teamData == null)
                return null;

            MemoryStream baseStream = new(teamData);
            if (new BinaryReader(baseStream).ReadInt64() == kPngHatKey)
            {
                BitBuffer bitBuffer = new(teamData);
                bitBuffer.ReadLong();
                return DeserializeFromPNG(pName: bitBuffer.ReadString(), pData: bitBuffer.ReadBitBuffer().buffer, pPath: pPath);
            }

            baseStream.Seek(0L, SeekOrigin.Begin);
            RijndaelManaged RMCrypto = new();
            byte[] Key =
            [
                243, 22, 152, 32, 1, 244, 122, 111, 97, 42,
                13, 2, 19, 15, 45, 230
            ];
            RMCrypto.Key = Key;
            RMCrypto.IV = ReadByteArray(baseStream);
            BinaryReader reader = new(new CryptoStream(baseStream, RMCrypto.CreateDecryptor(RMCrypto.Key, RMCrypto.IV), CryptoStreamMode.Read));
            var key = reader.ReadInt64();
            if (key == 402965919293045L || key == 630430777029345L || key == 630449177029345L || key == 465665919293045L)
            {
                if (key == 630449177029345L || key == 465665919293045L)
                    reader.ReadString();

                var name = reader.ReadString();
                var length = reader.ReadInt32();
                return DeserializeFromPNG(reader.ReadBytes(length), name, pPath, pIgnoreSizeRestriction: true);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public string GetNameForDisplay()
    {
        return name.ToUpperInvariant();
    }

    public void SetHatSprite(SpriteMap pSprite)
    {
        _hat = pSprite;
    }

    public void Join(Profile joinProfile, bool setProfileTeam = true)
    {
        if (_activeProfiles.Contains(joinProfile))
            return;

        joinProfile.team?.Leave(joinProfile, setProfileTeam);

        _activeProfiles.Add(joinProfile);

        if (setProfileTeam)
            joinProfile.team = this;
    }

    public void Leave(Profile prof, bool set = true)
    {
        _activeProfiles.Remove(prof);

        if (set)
            prof.team = null;
    }

    public void ClearProfiles()
    {
        foreach (var p in new List<Profile>(_activeProfiles)) //!! FIXME: useless memory allocation
            Leave(p);

        _activeProfiles.Clear();
    }

    public void ResetTeam()
    {
        _score = 0;
    }

    public void Construct(string varName, Texture2D hatTexture, bool demo = false, bool lockd = false, Vector2 hatOff = default, string desc = "")
    {
        _name = varName;
        _hat = new SpriteMap(hatTexture, 32, 32);
        _hatOffset = hatOff;
        inDemo = demo;
        _locked = lockd;
        _description = desc;
    }

    #endregion

    #region Private Methods

    static byte[] ReadByteArray(Stream s)
    {
        var rawLength = new byte[4];
        if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            throw new SystemException("Stream did not contain properly formatted byte array");

        var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
        if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            throw new SystemException("Did not read byte array properly");

        return buffer;
    }

    Team GetFacadeTeam()
    {
        if (!Network.isActive && !HostTable.loop)
            return this;

        foreach (var p in DuckNetwork.profiles)
        {
            if (p.team != this)
                continue;

            if (p.fixedGhostIndex >= 8)
                continue;

            if (p.connection != null && Teams.core._facadeMap.TryGetValue(p, out var t))
                return t;
        }

        return this;
    }

    #endregion
}