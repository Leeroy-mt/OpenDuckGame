using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace DuckGame;

public class Team
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class Metapixel : Attribute
    {
        public readonly int index;

        public readonly string name;

        public readonly string description;

        public Metapixel(int pIndex, string pName, string pDescription)
        {
            index = pIndex;
            name = pName;
            description = pDescription;
        }
    }

    public class CustomMetadata
    {
        public static HatMetadataElement kPreviousParameter;

        public static HatMetadataElement kCurrentParameter;

        public Dictionary<int, HatMetadataElement> _fieldMap = new Dictionary<int, HatMetadataElement>();

        public int Index(HatMetadataElement pParameter)
        {
            foreach (KeyValuePair<int, HatMetadataElement> p in _fieldMap)
            {
                if (p.Value == pParameter)
                {
                    return p.Key;
                }
            }
            return -1;
        }

        public virtual bool Deserialize(Color pColor)
        {
            HatMetadataElement el = null;
            if (_fieldMap.TryGetValue(pColor.r, out el))
            {
                if (!(el is CustomHatMetadata.MDRandomizer))
                {
                    kCurrentParameter = el;
                }
                el.Parse(pColor);
                if (!(el is CustomHatMetadata.MDRandomizer))
                {
                    kPreviousParameter = el;
                }
                return true;
            }
            return false;
        }

        public static Dictionary<Func<object, object>, Metapixel> PrepareParameterAttributes(Type pType)
        {
            Dictionary<Func<object, object>, Metapixel> parameterAttributes = new Dictionary<Func<object, object>, Metapixel>();
            foreach (FieldInfo f in from x in pType.GetFields(BindingFlags.Instance | BindingFlags.Public)
                                    where typeof(HatMetadataElement).IsAssignableFrom(x.FieldType)
                                    select x)
            {
                parameterAttributes[Editor.BuildGetAccessorField(pType, f)] = f.GetCustomAttribute<Metapixel>();
            }
            return parameterAttributes;
        }

        public static Dictionary<int, HatMetadataElement> PrepareFieldMap(Dictionary<Func<object, object>, Metapixel> pParameterAttributes, object pFor)
        {
            Dictionary<int, HatMetadataElement> fieldMap = new Dictionary<int, HatMetadataElement>();
            foreach (KeyValuePair<Func<object, object>, Metapixel> pair in pParameterAttributes)
            {
                fieldMap[pair.Value.index] = (HatMetadataElement)pair.Key(pFor);
            }
            return fieldMap;
        }
    }

    public abstract class HatMetadataElement
    {
        public bool set;

        public Vec2 randomizerX;

        public Vec2 randomizerY;

        public abstract void Parse(Color pColor);
    }

    public class CustomHatMetadata : CustomMetadata
    {
        public abstract class V<T> : HatMetadataElement
        {
            public int defaultCopyIndex;

            protected T _value;

            public Action postParseScript;

            protected V<T> _defaultCopy
            {
                get
                {
                    if (defaultCopyIndex != 0 && kCurrentMetadata != null && kCurrentMetadata._fieldMap.TryGetValue(defaultCopyIndex, out var m))
                    {
                        return m as V<T>;
                    }
                    return null;
                }
            }

            public virtual T value
            {
                get
                {
                    return _value;
                }
                set
                {
                    _value = value;
                }
            }

            public override void Parse(Color pColor)
            {
                randomizerX = Vec2.Zero;
                randomizerY = Vec2.Zero;
                set = true;
                OnParse(pColor);
                if (postParseScript != null)
                {
                    postParseScript();
                }
            }

            public abstract void OnParse(Color pColor);
        }

        public class MDVec2 : V<Vec2>
        {
            public bool allowNegative = true;

            public float range = 16f;

            public override Vec2 value
            {
                get
                {
                    Vec2 modified = _value;
                    if (_value == Vec2.MaxValue && base._defaultCopy != null)
                    {
                        modified = base._defaultCopy.value;
                    }
                    if (randomizerX != Vec2.Zero)
                    {
                        modified.x = Rando.Float(_value.x * randomizerX.x, _value.x * randomizerX.y);
                    }
                    if (randomizerY == Vec2.MaxValue)
                    {
                        modified.y = modified.x;
                    }
                    else if (randomizerY != Vec2.Zero)
                    {
                        modified.y = Rando.Float(_value.y * randomizerY.x, _value.y * randomizerY.y);
                    }
                    if (allowNegative)
                    {
                        modified.x = Maths.Clamp(modified.x, 0f - range, range);
                        modified.y = Maths.Clamp(modified.y, 0f - range, range);
                    }
                    else
                    {
                        modified.x = Maths.Clamp(modified.x, 0f, range);
                        modified.y = Maths.Clamp(modified.y, 0f, range);
                    }
                    return modified;
                }
                set
                {
                    _value = value;
                }
            }

            public override void OnParse(Color pColor)
            {
                float xOffset = Maths.Clamp(pColor.g - 128, 0f - range, range);
                float yOffset = Maths.Clamp(pColor.b - 128, 0f - range, range);
                _value = new Vec2(xOffset, yOffset);
            }
        }

        public class MDVec2Normalized : MDVec2
        {
            public MDVec2Normalized()
            {
                range = 1f;
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
                    _value = new Vec2((float)(int)pColor.g / 255f, (float)(int)pColor.b / 255f);
                }
                _value *= range;
            }
        }

        public class MDRandomizer : MDVec2Normalized
        {
            public bool randomizeY;

            public bool randomizeBoth;

            public override void OnParse(Color pColor)
            {
                base.OnParse(pColor);
                if (CustomMetadata.kPreviousParameter != null)
                {
                    if (!randomizeY)
                    {
                        CustomMetadata.kPreviousParameter.randomizerX = value;
                    }
                    else
                    {
                        CustomMetadata.kPreviousParameter.randomizerY = value;
                    }
                    if (randomizeBoth)
                    {
                        CustomMetadata.kPreviousParameter.randomizerY = Vec2.MaxValue;
                    }
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
            public float range = 1f;

            public bool allowNegative;

            public override float value
            {
                get
                {
                    float modified = _value;
                    if (_value == float.MaxValue && base._defaultCopy != null)
                    {
                        modified = base._defaultCopy.value;
                    }
                    if (randomizerX != Vec2.Zero)
                    {
                        modified = Rando.Float(_value * randomizerX.x, _value * randomizerX.y);
                    }
                    if (allowNegative)
                    {
                        return Maths.Clamp(modified, 0f - range, range);
                    }
                    return Maths.Clamp(modified, 0f, range);
                }
                set
                {
                    _value = value;
                }
            }

            public override void OnParse(Color pColor)
            {
                if (allowNegative)
                {
                    _value = (float)(pColor.g - 128) / 128f;
                }
                else
                {
                    _value = (float)(int)pColor.g / 255f;
                }
                _value *= range;
            }
        }

        public class MDInt : V<int>
        {
            public int range = 255;

            public bool allowNegative;

            public override int value
            {
                get
                {
                    float modified = _value;
                    if (_value == int.MaxValue && base._defaultCopy != null)
                    {
                        modified = base._defaultCopy.value;
                    }
                    if (randomizerX != Vec2.Zero)
                    {
                        modified = Rando.Float((float)_value * randomizerX.x, (float)_value * randomizerX.y);
                    }
                    return (int)Math.Round(modified);
                }
                set
                {
                    _value = value;
                }
            }

            public override void OnParse(Color pColor)
            {
                if (allowNegative)
                {
                    _value = pColor.g - 128;
                }
                else
                {
                    _value = pColor.g;
                }
                _value = Maths.Clamp(value, -range, range);
            }
        }

        public class MDIntPair : V<Vec2>
        {
            public int rangeX = 255;

            public int rangeY = 255;

            public bool allowNegative;

            public override Vec2 value
            {
                get
                {
                    Vec2 modified = _value;
                    if (_value == Vec2.MaxValue && base._defaultCopy != null)
                    {
                        modified = base._defaultCopy.value;
                    }
                    if (randomizerX != Vec2.Zero)
                    {
                        modified.x = (float)Math.Round(Rando.Float(_value.x * randomizerX.x, _value.x * randomizerX.y));
                    }
                    if (randomizerY == Vec2.MaxValue)
                    {
                        modified.y = modified.x;
                    }
                    else if (randomizerY != Vec2.Zero)
                    {
                        modified.y = (float)Math.Round(Rando.Float(_value.y * randomizerY.x, _value.y * randomizerY.y));
                    }
                    return modified;
                }
                set
                {
                    _value = value;
                }
            }

            public override void OnParse(Color pColor)
            {
                if (allowNegative)
                {
                    _value.x = pColor.g - 128;
                    _value.y = pColor.b - 128;
                }
                else
                {
                    _value.x = (int)pColor.g;
                    _value.y = (int)pColor.b;
                }
                _value.x = Maths.Clamp(value.x, -rangeX, rangeX);
                _value.y = Maths.Clamp(value.y, -rangeY, rangeY);
            }
        }

        public string hatPath;

        [Metapixel(1, "Hat Offset", "Hat offset position in pixels")]
        public MDVec2 HatOffset = new MDVec2
        {
            range = 16f
        };

        [Metapixel(2, "Use Duck Color", "If this metapixel exists, White (255, 255, 255) and Grey(157, 157, 157) will be recolored to duck colors.")]
        public MDBool UseDuckColor = new MDBool();

        [Metapixel(3, "Hat No-Flip", "If this metapixel exists, the hat will not be flipped with the direction of the duck.")]
        public MDBool HatNoFlip = new MDBool();

        [Metapixel(10, "Cape Offset", "Cape offset position in pixels")]
        public MDVec2 CapeOffset = new MDVec2
        {
            range = 16f
        };

        [Metapixel(11, "Cape Is Foreground", "If this metapixel exists, the cape will be drawn over the duck.")]
        public MDBool CapeForeground = new MDBool();

        [Metapixel(12, "Cape Sway Modifier", "Affects cape length, and left to right sway.")]
        public MDVec2Normalized CapeSwayModifier = new MDVec2Normalized
        {
            value = new Vec2(0.3f, 1f),
            allowNegative = true
        };

        [Metapixel(13, "Cape Wiggle Modifier", "Affects how much the cape wiggles in the wind.")]
        public MDVec2Normalized CapeWiggleModifier = new MDVec2Normalized
        {
            value = new Vec2(1f, 1f),
            allowNegative = true
        };

        [Metapixel(14, "Cape Taper Start", "Affects how narrow the cape/trail is at the top/beginning.")]
        public MDFloat CapeTaperStart = new MDFloat
        {
            value = 0.5f
        };

        [Metapixel(15, "Cape Taper End", "Affects how narrow the cape/trail is at the bottom/end.")]
        public MDFloat CapeTaperEnd = new MDFloat
        {
            value = 1f
        };

        [Metapixel(16, "Cape Alpha Start", "Affects how transparent the cape/trail is at the top/beginning.")]
        public MDFloat CapeAlphaStart = new MDFloat
        {
            value = 1f
        };

        [Metapixel(17, "Cape Alpha End", "Affects how transparent the cape/trail is at the bottom/end.")]
        public MDFloat CapeAlphaEnd = new MDFloat
        {
            value = 1f
        };

        [Metapixel(20, "Cape Is Trail", "If this metapixel exists, the cape will be a trail instead of a cape (think of the rainbow trail left by the TV object).")]
        public MDBool CapeIsTrail = new MDBool();

        [Metapixel(30, "Particle Emitter Offset", "The offset in pixels from the center of the hat where particles will be emitted.")]
        public MDVec2 ParticleEmitterOffset = new MDVec2
        {
            range = 16f
        };

        [Metapixel(31, "Particle Default Behavior", "B defines a particle behavior from a list of presets: 0 = No Behavior, 1 = Spit, 2 = Burst, 3 = Halo, 4 = Exclamation")]
        public MDInt ParticleDefaultBehavior = new MDInt
        {
            range = 4,
            postParseScript = ApplyDefaultParticleBehavior
        };

        [Metapixel(32, "Particle Emit Shape", "G: 0 = Point, 1 = Circle, 2 = Box   B: 0 = Emit Around Shape Border Randomly, 1 = Fill Shape Randomly, 2 = Emit Around Shape Border Uniformly")]
        public MDIntPair ParticleEmitShape = new MDIntPair
        {
            rangeX = 2,
            rangeY = 2
        };

        [Metapixel(33, "Particle Emit Shape Size", "X and Y size of the particle emitter (in pixels)")]
        public MDVec2 ParticleEmitShapeSize = new MDVec2
        {
            range = 32f,
            value = new Vec2(24f, 24f)
        };

        [Metapixel(34, "Particle Count", "The number of particles to emit.")]
        public MDInt ParticleCount = new MDInt
        {
            range = 8,
            value = 4
        };

        [Metapixel(35, "Particle Lifespan", "Life span of the particle, in seconds.")]
        public MDFloat ParticleLifespan = new MDFloat
        {
            range = 2f,
            value = 1f
        };

        [Metapixel(36, "Particle Velocity", "Initial velocity of the particle.")]
        public MDVec2Normalized ParticleVelocity = new MDVec2Normalized
        {
            range = 2f,
            allowNegative = true
        };

        [Metapixel(37, "Particle Gravity", "Gravity applied to the particle.")]
        public MDVec2Normalized ParticleGravity = new MDVec2Normalized
        {
            range = 2f,
            allowNegative = true,
            value = new Vec2(0f, PhysicsObject.gravity)
        };

        [Metapixel(38, "Particle Friction", "Friction applied to the particle (The value it's velocity is multiplied by every frame).")]
        public MDVec2Normalized ParticleFriction = new MDVec2Normalized
        {
            range = 1f,
            allowNegative = false,
            value = new Vec2(1f, 1f)
        };

        [Metapixel(39, "Particle Alpha", "G = Start alpha, B = End alpha")]
        public MDVec2Normalized ParticleAlpha = new MDVec2Normalized
        {
            range = 1f,
            allowNegative = false,
            value = new Vec2(1f, 1f)
        };

        [Metapixel(40, "Particle Scale", "G = Start scale, B = End scale")]
        public MDVec2Normalized ParticleScale = new MDVec2Normalized
        {
            range = 2f,
            allowNegative = false,
            value = new Vec2(1f, 0f)
        };

        [Metapixel(41, "Particle Rotation", "G = Start rotation, B = End rotation")]
        public MDVec2Normalized ParticleRotation = new MDVec2Normalized
        {
            range = 36f,
            value = new Vec2(0f, 0f)
        };

        [Metapixel(42, "Particle Offset", "Additional X Y offset of particle.")]
        public MDVec2 ParticleOffset = new MDVec2
        {
            range = 16f
        };

        [Metapixel(43, "Particle Background", "If this metapixel exists, particles will be rendered behind the duck.")]
        public MDBool ParticleBackground = new MDBool();

        [Metapixel(44, "Particle Anchor", "If this metapixel exists, particles will stay anchored around the hat position when it's moving.")]
        public MDBool ParticleAnchor = new MDBool();

        [Metapixel(45, "Particle Animated", "If this metapixel exists, particles will animate through their frames. Otherwise, a frame will be picked randomly.")]
        public MDBool ParticleAnimated = new MDBool();

        [Metapixel(46, "Particle Animation Loop", "If this metapixel exists, the particle animation will loop.")]
        public MDBool ParticleAnimationLoop = new MDBool();

        [Metapixel(47, "Particle Animation Random Frame", "If this metapixel exists, the particle animation will start on a random frame.")]
        public MDBool ParticleAnimationRandomFrame = new MDBool();

        [Metapixel(48, "Particle Animation Speed", "How quickly the particle animates.")]
        public MDFloat ParticleAnimationSpeed = new MDFloat
        {
            range = 1f,
            value = 0.1f
        };

        [Metapixel(49, "Particle Anchor Orientation", "If this metapixel exists, particles will flip and rotate to orient with the hat.")]
        public MDBool ParticleAnchorOrientation = new MDBool();

        [Metapixel(60, "Quack Delay", "Amount of time in between pressing the quack button and the quack frame appearing.")]
        public MDFloat QuackDelay = new MDFloat
        {
            range = 2f,
            value = 0f
        };

        [Metapixel(61, "Quack Hold", "Minimum amount of time to keep the quack frame held, even if the quack button is released.")]
        public MDFloat QuackHold = new MDFloat
        {
            range = 2f,
            value = 0f
        };

        [Metapixel(62, "Quack Suppress Requack", "If this metapixel exists, a new quack will not be allowed to begin until Quack Delay and Quack Hold are finished.")]
        public MDBool QuackSuppressRequack = new MDBool();

        [Metapixel(70, "Wet Lips", "If this metapixel exists, the hat will have 'wet lips'.")]
        public MDBool WetLips = new MDBool();

        [Metapixel(71, "Mechanical Lips", "If this metapixel exists, the hat will have 'mechanical lips'.")]
        public MDBool MechanicalLips = new MDBool();

        [Metapixel(100, "Randomize Parameter X", "If present, the previously defined metapixel value will have it's X value multiplied by a random normalized number between G and B each time it's used. This will generally only work with particles..")]
        public MDRandomizer RandomizeParameterX = new MDRandomizer
        {
            range = 1f,
            allowNegative = true
        };

        [Metapixel(101, "Randomize Parameter Y", "If present, the previously defined metapixel value will have it's Y value multiplied by a random normalized number between G and B each time it's used. This will generally only work with particles..")]
        public MDRandomizer RandomizeParameterY = new MDRandomizer
        {
            range = 1f,
            allowNegative = true,
            randomizeY = true
        };

        [Metapixel(102, "Randomize Parameter", "If present, the previously defined metapixel value will have a random number between G and B applied to its X and Y values each time it's used. This will generally only work with particles..")]
        public MDRandomizer RandomizeParameter = new MDRandomizer
        {
            range = 1f,
            allowNegative = true,
            randomizeBoth = true
        };

        public Team team;

        private static CustomHatMetadata kCurrentMetadata;

        private static Dictionary<Func<object, object>, Metapixel> kParameterAttributes;

        private static void ApplyDefaultParticleBehavior()
        {
            int value = kCurrentMetadata.ParticleDefaultBehavior.value;
            if (value == 1)
            {
                kCurrentMetadata.ParticleEmitShape.value = new Vec2(0f, 0f);
                kCurrentMetadata.ParticleOffset.value = new Vec2(2f, 2f);
                kCurrentMetadata.ParticleOffset.randomizerX = new Vec2(-1f, 1f);
                kCurrentMetadata.ParticleOffset.randomizerY = new Vec2(-1f, 1f);
                kCurrentMetadata.ParticleVelocity.value = new Vec2(3f, 1.5f);
                kCurrentMetadata.ParticleVelocity.randomizerX = new Vec2(0.3f, 1f);
                kCurrentMetadata.ParticleVelocity.randomizerY = new Vec2(-1f, 0.3f);
                kCurrentMetadata.ParticleScale.value = new Vec2(1f, 1f);
                kCurrentMetadata.ParticleScale.randomizerX = new Vec2(0.7f, 1f);
                kCurrentMetadata.ParticleScale.randomizerY = Vec2.MaxValue;
                kCurrentMetadata.ParticleCount.value = 5;
                kCurrentMetadata.ParticleCount.randomizerX = new Vec2(0.3f, 1f);
                kCurrentMetadata.ParticleBackground.value = false;
            }
            if (value == 2)
            {
                kCurrentMetadata.ParticleEmitShape.value = new Vec2(0f, 0f);
                kCurrentMetadata.ParticleOffset.value = new Vec2(2f, 2f);
                kCurrentMetadata.ParticleOffset.randomizerX = new Vec2(-1f, 1f);
                kCurrentMetadata.ParticleOffset.randomizerY = new Vec2(-1f, 1f);
                kCurrentMetadata.ParticleVelocity.value = new Vec2(1.5f, 2.5f);
                kCurrentMetadata.ParticleVelocity.randomizerX = new Vec2(-1f, 1f);
                kCurrentMetadata.ParticleVelocity.randomizerY = new Vec2(-1f, 1f);
                kCurrentMetadata.ParticleScale.value = new Vec2(1f, 0f);
                kCurrentMetadata.ParticleScale.randomizerX = new Vec2(0.7f, 1f);
                kCurrentMetadata.ParticleCount.value = 8;
                kCurrentMetadata.ParticleCount.randomizerX = new Vec2(0.5f, 1f);
                kCurrentMetadata.ParticleBackground.value = false;
            }
            if (value == 3)
            {
                kCurrentMetadata.ParticleEmitShape.value = new Vec2(1f, 2f);
                kCurrentMetadata.ParticleAlpha.value = new Vec2(1f, 0f);
                kCurrentMetadata.ParticleCount.value = 8;
                kCurrentMetadata.ParticleBackground.value = true;
                kCurrentMetadata.ParticleGravity.value = new Vec2(0f, 0f);
                kCurrentMetadata.ParticleAnchor.value = true;
            }
            if (value == 4)
            {
                kCurrentMetadata.ParticleEmitShape.value = new Vec2(0f, 0f);
                kCurrentMetadata.ParticleScale.value = new Vec2(0.3f, 1.5f);
                kCurrentMetadata.ParticleCount.value = 1;
                kCurrentMetadata.ParticleBackground.value = false;
                kCurrentMetadata.ParticleGravity.value = new Vec2(0f, 0f);
                kCurrentMetadata.ParticleAnchor.value = true;
                kCurrentMetadata.ParticleVelocity.value = new Vec2(1.4f, -1.2f);
                kCurrentMetadata.ParticleFriction.value = new Vec2(0.92f, 0.9f);
                kCurrentMetadata.ParticleLifespan.value = 0.8f;
            }
        }

        public CustomHatMetadata(Team pTeam)
        {
            team = pTeam;
            kCurrentMetadata = this;
            if (kParameterAttributes == null)
            {
                kParameterAttributes = CustomMetadata.PrepareParameterAttributes(GetType());
            }
            _fieldMap = CustomMetadata.PrepareFieldMap(kParameterAttributes, this);
            ApplyDefaultParticleBehavior();
        }

        public override bool Deserialize(Color pColor)
        {
            if (!base.Deserialize(pColor))
            {
                DevConsole.Log(DCSection.General, "Metapixel with invalid ID value (" + pColor.r + ") found in custom hat.");
                return false;
            }
            return true;
        }
    }

    public NetworkConnection customConnection;

    private Dictionary<DuckPersona, SpriteMap> _recolors = new Dictionary<DuckPersona, SpriteMap>();

    public string customHatPath;

    private static readonly long kPngHatKey = 630430737023345L;

    public static bool networkDeserialize = false;

    public static List<string> hatSearchPaths = new List<string>
    {
        Directory.GetCurrentDirectory() + "/Hats",
        DuckFile.saveDirectory + "/Hats",
        DuckFile.saveDirectory + "/Custom/Hats"
    };

    public static List<Team> deserializedTeams = new List<Team>();

    public static int currentLoadHat = 0;

    public static int totalLoadHats = 0;

    public static Team deserializeInto;

    public Texture2D _capeTexture;

    public List<Texture2D> _customParticles = new List<Texture2D>();

    public Texture2D _rockTexture;

    public bool capeRequestSuccess = true;

    private string _name = "";

    private string _description = "";

    private SpriteMap _default;

    private SpriteMap _hat;

    private int _score;

    private int _rockScore;

    private int _wins;

    private int _prevScoreboardScore;

    private Vec2 _hatOffset;

    public bool inDemo;

    private List<Profile> _activeProfiles = new List<Profile>();

    public CustomHatMetadata _basicMetadata;

    public CustomHatMetadata _metadata;

    private string _hatID;

    private byte[] _customData;

    public Vec2 prevTreeDraw = Vec2.Zero;

    private bool _locked;

    public bool isFolder;

    public Team folder;

    public string fullFolderPath;

    public Tex2D folderTexture;

    public bool defaultTeam;

    public Profile owner;

    public bool isTemporaryTeam;

    public bool isHair;

    public bool noCrouchOffset;

    public bool filter
    {
        get
        {
            if (customConnection != null)
            {
                Profile p = customConnection.profile;
                if (p != null && p.muteHat)
                {
                    return true;
                }
                bool filter = Options.Data.hatFilter == 2;
                if (Options.Data.hatFilter == 1 && customConnection.data is User && (customConnection.data as User).relationship != FriendRelationship.Friend)
                {
                    filter = true;
                }
                return filter;
            }
            return false;
        }
    }

    public Texture2D capeTexture
    {
        get
        {
            if (filter)
            {
                return null;
            }
            return GetFacadeTeam()._capeTexture;
        }
    }

    public List<Texture2D> customParticles
    {
        get
        {
            if (filter)
            {
                return new List<Texture2D>();
            }
            return GetFacadeTeam()._customParticles;
        }
    }

    public Texture2D rockTexture
    {
        get
        {
            if (filter)
            {
                return null;
            }
            return GetFacadeTeam()._rockTexture;
        }
    }

    public string name => _name;

    public string description => _description;

    public string currentDisplayName
    {
        get
        {
            string n = "";
            if (activeProfiles != null && activeProfiles.Count > 0)
            {
                n = ((activeProfiles.Count > 1) ? GetNameForDisplay() : ((!Profiles.IsDefault(activeProfiles[0]) || Network.isActive) ? activeProfiles[0].nameUI : GetNameForDisplay()));
            }
            return n;
        }
    }

    public SpriteMap hat
    {
        get
        {
            if (filter)
            {
                if (_default == null)
                {
                    _default = new SpriteMap("hats/default", 32, 32);
                }
                return _default;
            }
            return GetFacadeTeam()._hat;
        }
    }

    public bool hasHat
    {
        get
        {
            if (hat != null)
            {
                return hat.texture.textureName != "hats/noHat";
            }
            return false;
        }
    }

    public int score
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
        }
    }

    public int rockScore
    {
        get
        {
            return _rockScore;
        }
        set
        {
            _rockScore = value;
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

    public int prevScoreboardScore
    {
        get
        {
            return _prevScoreboardScore;
        }
        set
        {
            _prevScoreboardScore = value;
        }
    }

    public Vec2 hatOffset
    {
        get
        {
            if (filter)
            {
                return Vec2.Zero;
            }
            return GetFacadeTeam()._hatOffset;
        }
        set
        {
            _hatOffset = value;
        }
    }

    public List<Profile> activeProfiles => _activeProfiles;

    public int numMembers => _activeProfiles.Count;

    public CustomHatMetadata metadata
    {
        get
        {
            if (filter)
            {
                if (_basicMetadata == null)
                {
                    _basicMetadata = new CustomHatMetadata(this);
                    _basicMetadata.UseDuckColor.value = true;
                }
                return _basicMetadata;
            }
            return GetFacadeTeam()._metadata;
        }
        set
        {
            _metadata = value;
        }
    }

    public string hatID
    {
        get
        {
            return GetFacadeTeam()._hatID;
        }
        set
        {
            _hatID = value;
        }
    }

    public Team facade
    {
        get
        {
            Team t = GetFacadeTeam();
            if (t == this)
            {
                return null;
            }
            return t;
        }
    }

    public byte[] customData
    {
        get
        {
            return GetFacadeTeam()._customData;
        }
        set
        {
            _customData = value;
        }
    }

    public bool locked
    {
        get
        {
            if (NetworkDebugger.enabled && NetworkDebugger.currentIndex == 1)
            {
                if (Teams.all.IndexOf(this) > 15)
                {
                    return Teams.all.IndexOf(this) < 35;
                }
                return false;
            }
            return _locked;
        }
        set
        {
            _locked = value;
        }
    }

    public SpriteMap GetHat(DuckPersona pPersona)
    {
        if (metadata == null || !metadata.UseDuckColor.value)
        {
            return hat;
        }
        SpriteMap s = null;
        if (!_recolors.TryGetValue(pPersona, out s))
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

    private static byte[] ReadByteArray(Stream s)
    {
        byte[] rawLength = new byte[4];
        if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
        {
            throw new SystemException("Stream did not contain properly formatted byte array");
        }
        byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
        if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
        {
            throw new SystemException("Did not read byte array properly");
        }
        return buffer;
    }

    public static Team Deserialize(string file)
    {
        if (DuckFile.FileExists(file))
        {
            if (file.EndsWith(".png"))
            {
                return DeserializeFromPNG(file);
            }
            return Deserialize(File.ReadAllBytes(file), file);
        }
        return null;
    }

    public static Team DeserializeFromPNG(string pFile)
    {
        try
        {
            if (pFile.EndsWith("folder_preview.png"))
            {
                return null;
            }
            byte[] pData = File.ReadAllBytes(pFile);
            string name = Path.GetFileNameWithoutExtension(pFile);
            return DeserializeFromPNG(pData, name, pFile);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static Team DeserializeFromPNG(byte[] pData, string pName, string pPath, bool pIgnoreSizeRestriction = false)
    {
        try
        {
            Texture2D tex = TextureConverter.LoadPNGWithPinkAwesomeness(Graphics.device, new Bitmap(new MemoryStream(pData)), process: true);
            _ = (float)tex.Width / 32f % 1f;
            Team newTeam = deserializeInto;
            if (newTeam == null)
            {
                newTeam = new Team(pName, tex);
            }
            else
            {
                newTeam.Construct(pName, tex);
            }
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
                tex.GetData(0, new Microsoft.Xna.Framework.Rectangle(64, 0, 32, 32), rawData, 0, 1024);
                newTeam._capeTexture = new Texture2D(Graphics.device, 32, 32);
                newTeam._capeTexture.SetData(rawData);
                newTeam.capeRequestSuccess = true;
            }
            if (tex.Height >= 56)
            {
                Color[] rockData = new Color[576];
                tex.GetData(0, new Microsoft.Xna.Framework.Rectangle(0, 32, 24, 24), rockData, 0, 576);
                if (CheckForPixelData(rockData, 16))
                {
                    newTeam._rockTexture = new Texture2D(Graphics.device, 24, 24);
                    newTeam._rockTexture.SetData(rockData);
                }
                if (tex.Width > 32)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Color[] particleData = new Color[144];
                        tex.GetData(0, new Microsoft.Xna.Framework.Rectangle(24 + 12 * (i % 2), 32 + 12 * (i / 2), 12, 12), particleData, 0, 144);
                        if (CheckForPixelData(particleData))
                        {
                            Texture2D particleTex = new Texture2D(Graphics.device, 12, 12);
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
        catch (Exception)
        {
            return null;
        }
    }

    public static void LoadCustomHatsFromFolder(string pFolder, string pExtension)
    {
        string[] files = DuckFile.GetFiles(pFolder, "*." + pExtension);
        foreach (string f in files)
        {
            totalLoadHats++;
            MonoMain.currentActionQueue.Enqueue(new LoadingAction(delegate
            {
                //MonoMain.loadMessage = "Loading Custom Hats (" + currentLoadHat + "/" + totalLoadHats + ")";
                currentLoadHat++;
                Team team = null;
                if (pExtension == "png")
                {
                    byte[] pData = File.ReadAllBytes(f);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(f);
                    team = DeserializeFromPNG(pData, fileNameWithoutExtension, f);
                }
                else
                {
                    team = Deserialize(f);
                }
                if (team != null)
                {
                    deserializedTeams.Add(team);
                }
            }));
        }
    }

    public static void DeserializeCustomHats()
    {
        LoadCustomHatsFromFolder(Directory.GetCurrentDirectory(), "hat");
        foreach (string hatSearchPath in hatSearchPaths)
        {
            LoadCustomHatsFromFolder(hatSearchPath, "hat");
            LoadCustomHatsFromFolder(hatSearchPath, "png");
        }
    }

    public static bool CheckForPixelData(Color[] pColors, int pMinimumNumberOfPixels = 1)
    {
        int numValid = 0;
        for (int i = 0; i < pColors.Length; i++)
        {
            if ((pColors[i].r != byte.MaxValue || pColors[i].g != 0 || pColors[i].b != byte.MaxValue) && !(pColors[i] == Colors.Transparent))
            {
                numValid++;
            }
        }
        return numValid >= pMinimumNumberOfPixels;
    }

    public static void ProcessMetadata(Texture2D pTex, Team pTeam)
    {
        int metadataWidth = pTex.Width % 32;
        if (pTex.Width > 100 || metadataWidth <= 0)
        {
            return;
        }
        pTeam.metadata = new CustomHatMetadata(pTeam);
        Color[] meta = new Color[metadataWidth * Math.Min(pTex.Height, 56)];
        pTex.GetData(0, new Microsoft.Xna.Framework.Rectangle(pTex.Width - metadataWidth, 0, metadataWidth, Math.Min(pTex.Height, 56)), meta, 0, meta.Length);
        for (int i = 0; i < meta.Length; i++)
        {
            Color c = meta[i];
            if ((c.r != byte.MaxValue || c.g != 0 || c.b != byte.MaxValue) && !(c == Colors.Transparent))
            {
                pTeam.metadata.Deserialize(c);
            }
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
            {
                return null;
            }
            MemoryStream baseStream = new MemoryStream(teamData);
            if (new BinaryReader(baseStream).ReadInt64() == kPngHatKey)
            {
                BitBuffer bitBuffer = new BitBuffer(teamData);
                bitBuffer.ReadLong();
                return DeserializeFromPNG(pName: bitBuffer.ReadString(), pData: bitBuffer.ReadBitBuffer().buffer, pPath: pPath);
            }
            baseStream.Seek(0L, SeekOrigin.Begin);
            RijndaelManaged RMCrypto = new RijndaelManaged();
            byte[] Key = new byte[16]
            {
                243, 22, 152, 32, 1, 244, 122, 111, 97, 42,
                13, 2, 19, 15, 45, 230
            };
            RMCrypto.Key = Key;
            RMCrypto.IV = ReadByteArray(baseStream);
            BinaryReader reader = new BinaryReader(new CryptoStream(baseStream, RMCrypto.CreateDecryptor(RMCrypto.Key, RMCrypto.IV), CryptoStreamMode.Read));
            long key = reader.ReadInt64();
            if (key == 402965919293045L || key == 630430777029345L || key == 630449177029345L || key == 465665919293045L)
            {
                if (key == 630449177029345L || key == 465665919293045L)
                {
                    reader.ReadString();
                }
                string name = reader.ReadString();
                int length = reader.ReadInt32();
                return DeserializeFromPNG(reader.ReadBytes(length), name, pPath, pIgnoreSizeRestriction: true);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    private Team GetFacadeTeam()
    {
        if (Network.isActive || HostTable.loop)
        {
            foreach (Profile p in DuckNetwork.profiles)
            {
                if (p.team == this && p.fixedGhostIndex < 8)
                {
                    Team t = null;
                    if (p.connection != null && Teams.core._facadeMap.TryGetValue(p, out t))
                    {
                        return t;
                    }
                }
            }
        }
        return this;
    }

    public string GetNameForDisplay()
    {
        return name.ToUpperInvariant();
    }

    public void SetHatSprite(SpriteMap pSprite)
    {
        _hat = pSprite;
    }

    public void Join(Profile prof, bool set = true)
    {
        if (!_activeProfiles.Contains(prof))
        {
            if (prof.team != null)
            {
                prof.team.Leave(prof, set);
            }
            _activeProfiles.Add(prof);
            if (set)
            {
                prof.team = this;
            }
        }
    }

    public void Leave(Profile prof, bool set = true)
    {
        _activeProfiles.Remove(prof);
        if (set)
        {
            prof.team = null;
        }
    }

    public void ClearProfiles()
    {
        foreach (Profile p in new List<Profile>(_activeProfiles))
        {
            Leave(p);
        }
        _activeProfiles.Clear();
    }

    public void ResetTeam()
    {
        _score = 0;
    }

    public Team(string varName, string hatTexture, bool demo = false, bool lockd = false, Vec2 hatOff = default(Vec2), string desc = "", Texture2D capeTex = null)
    {
        _name = varName;
        _hat = new SpriteMap(hatTexture, 32, 32);
        _hatOffset = hatOff;
        inDemo = demo;
        _locked = lockd;
        _description = desc;
        _capeTexture = capeTex;
    }

    public Team(bool varHair, string varName, string hatTexture, bool demo = false, bool lockd = false, Vec2 hatOff = default(Vec2), string desc = "", Texture2D capeTex = null)
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

    public Team(string varName, string hatTexture, bool demo, bool lockd, Vec2 hatOff)
    {
        _name = varName;
        _hat = new SpriteMap(hatTexture, 32, 32);
        _hatOffset = hatOff;
        inDemo = demo;
        _locked = lockd;
    }

    public Team(string varName, Texture2D hatTexture, bool demo = false, bool lockd = false, Vec2 hatOff = default(Vec2), string desc = "")
    {
        Construct(varName, hatTexture, demo, lockd, hatOff, desc);
    }

    public Team(string varName, Texture2D hatTexture, bool demo, bool lockd, Vec2 hatOff)
    {
        _name = varName;
        _hat = new SpriteMap(hatTexture, 32, 32);
        _hatOffset = hatOff;
        inDemo = demo;
        _locked = lockd;
    }

    public void Construct(string varName, Texture2D hatTexture, bool demo = false, bool lockd = false, Vec2 hatOff = default(Vec2), string desc = "")
    {
        _name = varName;
        _hat = new SpriteMap(hatTexture, 32, 32);
        _hatOffset = hatOff;
        inDemo = demo;
        _locked = lockd;
        _description = desc;
    }
}
