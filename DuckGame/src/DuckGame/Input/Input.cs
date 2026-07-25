using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DuckGame;

public class Input
{
    #region Public Fields

    public static bool debuggerInputOverride;
    public static bool _imeAllowed;
    public static bool _prevImeAllowed;
    public static bool _dinputEnabled;
    public static bool enumeratingGamepads;
    public static bool mightHavePlaystationController;
    public static bool uiDevicesHaveChanged;
    public static volatile bool devicesChanged;

    public static int _suppressInputChangeMessages;
    public static int timesToEnumerateGamepads;

    public static InputCode konamiCode = new()
    {
        triggers =
        [
            Triggers.Up,
            Triggers.Up,
            Triggers.Down,
            Triggers.Down,
            Triggers.Left,
            Triggers.Right,
            Triggers.Left,
            Triggers.Right,
            Triggers.Quack,
            Triggers.Jump
        ]
    };
    public static InputCode konamiCodeAlternate = new()
    {
        triggers =
        [
            $"{Triggers.Up}|{Triggers.Jump}",
            $"{Triggers.Up}|{Triggers.Jump}",
            Triggers.Down,
            Triggers.Down,
            Triggers.Left,
            Triggers.Right,
            Triggers.Left,
            Triggers.Right,
            Triggers.Quack,
            $"{Triggers.Up}|{Triggers.Jump}",
        ]
    };
    public static InputCode hookCode = new()
    {
        triggers =
        [
            Triggers.Jump,
            Triggers.Quack,
            Triggers.Ragdoll,
            Triggers.Ragdoll,
            Triggers.Grab
        ],
        breakSpeed = 0.06f
    };
    public static InputProfile lastActiveProfile = new();

    public static List<DeviceInputMapping> _defaultInputMappingPresets =
    [
        new DeviceInputMapping
        {
            deviceName = "KEYBOARD P1",
            deviceGUID = "",
            map =
            {
                { Triggers.Left, (int)Keys.A },
                { Triggers.Right, (int)Keys.D },
                { Triggers.Up, (int)Keys.W },
                { Triggers.Down, (int)Keys.S },
                { Triggers.Jump, (int)Keys.Space },
                { Triggers.Shoot, (int)Keys.H },
                { Triggers.Grab, (int)Keys.G },
                { Triggers.Start, (int)Keys.Escape },
                { Triggers.Ragdoll, (int)Keys.F },
                { Triggers.Strafe, (int)Keys.LeftShift },
                { Triggers.Quack, (int)Keys.E },
                { Triggers.Select, (int)Keys.Space },
                { Triggers.Chat, (int)Keys.Enter },
                { Triggers.Cancel, (int)Keys.E },
                { Triggers.Menu1, (int)Keys.H },
                { Triggers.Menu2, (int)Keys.Q },
                { Triggers.MenuLeft, (int)Keys.A },
                { Triggers.MenuRight, (int)Keys.D },
                { Triggers.MenuUp, (int)Keys.W },
                { Triggers.MenuDown, (int)Keys.S },
                { Triggers.RightStick, (int)Keys.Tab },
                { Triggers.VoiceRegister, (int)Keys.Home },
                { Triggers.KeyboardF, (int)Keys.F }
            }
        },
        new DeviceInputMapping
        {
            deviceName = "KEYBOARD P2",
            deviceGUID = "",
            map =
            {
                { Triggers.Left, (int)Keys.Left },
                { Triggers.Right, (int)Keys.Right },
                { Triggers.Up, (int)Keys.Up },
                { Triggers.Down, (int)Keys.Down },
                { Triggers.Jump, (int)Keys.RightControl },
                { Triggers.Shoot, (int)Keys.OemQuotes },
                { Triggers.Grab, (int)Keys.OemSemicolon },
                { Triggers.Start, (int)Keys.OemPlus },
                { Triggers.Ragdoll, (int)Keys.O },
                { Triggers.Strafe, (int)Keys.L },
                { Triggers.Quack, (int)Keys.P },
                { Triggers.Select, (int)Keys.RightShift },
                { Triggers.Cancel, (int)Keys.P },
                { Triggers.Menu1, (int)Keys.OemQuotes },
                { Triggers.Menu2, (int)Keys.OemSemicolon },
                { Triggers.MenuLeft, (int)Keys.Left },
                { Triggers.MenuRight, (int)Keys.Right },
                { Triggers.MenuUp, (int)Keys.Up },
                { Triggers.MenuDown, (int)Keys.Down },
                { Triggers.RightStick, (int)Keys.Tab }
            }
        },
        new DeviceInputMapping
        {
            deviceName = "XBOX GAMEPAD",
            deviceGUID = "",
            map =
            {
                { Triggers.Left, (int)PadButton.DPadLeft },
                { Triggers.Right, (int)PadButton.DPadRight },
                { Triggers.Up, (int)PadButton.DPadUp },
                { Triggers.Down, (int)PadButton.DPadDown },
                { Triggers.Jump, (int)PadButton.A },
                { Triggers.Shoot, (int)PadButton.X },
                { Triggers.Grab, (int)PadButton.Y },
                { Triggers.Start, (int)PadButton.Start },
                { Triggers.Ragdoll, (int)PadButton.RightShoulder },
                { Triggers.Strafe, (int)PadButton.LeftShoulder },
                { Triggers.Quack, (int)PadButton.B },
                { Triggers.Select, (int)PadButton.A },

                { Triggers.LeftTrigger, (int)PadButton.LeftTrigger },
                { Triggers.RightTrigger, (int)PadButton.RightTrigger },
                { Triggers.LeftBumper, (int)PadButton.LeftShoulder },
                { Triggers.RightBumper, (int)PadButton.RightShoulder },
                { Triggers.LeftStick, (int)PadButton.LeftStick },
                { Triggers.RightStick, (int)PadButton.RightStick },
                { Triggers.Cancel, (int)PadButton.B },
                { Triggers.LeftOptionButton, (int)PadButton.Back },
                { Triggers.Menu1, (int)PadButton.X },
                { Triggers.Menu2, (int)PadButton.Y },
                { Triggers.MenuLeft, (int)PadButton.DPadLeft },
                { Triggers.MenuRight, (int)PadButton.DPadRight },
                { Triggers.MenuUp, (int)PadButton.DPadUp },
                { Triggers.MenuDown, (int)PadButton.DPadDown }
            }
        },
        new DeviceInputMapping
        {
            deviceName = "GENERIC GAMEPAD",
            deviceGUID = "",
            map =
            {
                { Triggers.Left, (int)PadButton.DPadLeft },
                { Triggers.Right, (int)PadButton.DPadRight },
                { Triggers.Up, (int)PadButton.DPadUp },
                { Triggers.Down, (int)PadButton.DPadDown },
                { Triggers.Jump, (int)PadButton.A },
                { Triggers.Shoot, (int)PadButton.X },
                { Triggers.Grab, (int)PadButton.Y },
                { Triggers.Start, (int)PadButton.Start },
                { Triggers.Ragdoll, (int)PadButton.RightShoulder },
                { Triggers.Strafe, (int)PadButton.LeftShoulder },
                { Triggers.Quack, (int)PadButton.B },
                { Triggers.Select, (int)PadButton.A },

                { Triggers.LeftTrigger, (int)PadButton.LeftTrigger },
                { Triggers.RightTrigger, (int)PadButton.RightTrigger },
                { Triggers.LeftBumper, (int)PadButton.LeftShoulder },
                { Triggers.RightBumper, (int)PadButton.RightShoulder },
                { Triggers.LeftStick, (int)PadButton.LeftStick },
                { Triggers.RightStick, (int)PadButton.RightStick },
                { Triggers.Cancel, (int)PadButton.B },
                { Triggers.LeftOptionButton, (int)PadButton.Back },
                { Triggers.Menu1, (int)PadButton.X },
                { Triggers.Menu2, (int)PadButton.Y },

                { Triggers.MenuLeft, (int)PadButton.DPadLeft },
                { Triggers.MenuRight, (int)PadButton.DPadRight },
                { Triggers.MenuUp, (int)PadButton.DPadUp },
                { Triggers.MenuDown, (int)PadButton.DPadDown }
            }
        },
    ];
    public static Dictionary<Keys, char> keyToChar = [];
    public static Dictionary<string, Sprite> _triggerImageMap = [];

    #endregion

    #region Private Fields

    static bool _ignoreInput;
    static bool _ignoreFirstInputChange = true;
    static bool _initializedIME;
    static bool _initializedMessageHook;
    static volatile bool _gamepadsChanged;
    static volatile bool _padConnectionChange;
    static volatile bool _changePluggedIn;

    static int _updateWaitFrames;
    static int _deviceUpdateWait = 60;

    static volatile string _changeName = "";

    static Thread _gamepadThread;

    static Array _keys = Enum.GetValues<Keys>();
    static List<Sprite> _buttonStyles = [];
    static List<InputDevice> _devices = [];
    static List<GenericController> _gamePads = [];
    static List<DeviceInputMapping> _defaultInputMapping = [];
    static List<DeviceInputMapping> _oldInputDefaults =
    [
        new DeviceInputMapping
        {
            deviceName = "KEYBOARD P1",
            deviceGUID = "",
            map =
            {
                { Triggers.Left, (int)Keys.A },
                { Triggers.Right, (int)Keys.D },
                { Triggers.Up, (int)Keys.W },
                { Triggers.Down, (int)Keys.S },
                { Triggers.Jump, (int)Keys.Space },
                { Triggers.Shoot, (int)Keys.H },
                { Triggers.Grab, (int)Keys.G },
                { Triggers.Start, (int)Keys.Escape },
                { Triggers.Ragdoll, (int)Keys.F },
                { Triggers.Strafe, (int)Keys.LeftShift },
                { Triggers.Quack, (int)Keys.E },
                { Triggers.Select, (int)Keys.Space },
                { Triggers.Chat, (int)Keys.Enter }
            }
        },
        new DeviceInputMapping
        {
            deviceName = "KEYBOARD P2",
            deviceGUID = "",
            map =
            {
                { Triggers.Left, (int)Keys.Left },
                { Triggers.Right, (int)Keys.Right },
                { Triggers.Up, (int)Keys.Up },
                { Triggers.Down, (int)Keys.Down },
                { Triggers.Jump, (int)Keys.RightControl },
                { Triggers.Shoot, (int)Keys.OemQuotes },
                { Triggers.Grab, (int)Keys.OemSemicolon },
                { Triggers.Start, (int)Keys.OemPlus },
                { Triggers.Ragdoll, (int)Keys.O },
                { Triggers.Strafe, (int)Keys.L },
                { Triggers.Quack, (int)Keys.P },
                { Triggers.Select, (int)Keys.RightShift },
            }
        }
    ];
    static Dictionary<string, InputProfile> _profiles = [];

    #endregion

    #region Public Properties

    public static bool ignoreInput
    {
        get => !debuggerInputOverride && (!Graphics.inFocus || _ignoreInput);
        set => _ignoreInput = value;
    }

    public static List<Sprite> buttonStyles => _buttonStyles;

    public static List<DeviceInputMapping> defaultInputMappingPresets => _defaultInputMappingPresets.ConvertAll(p => p.Clone());

    #endregion

    #region Public Methods

    public static void Save()
    {
        DevConsole.Log(DCSection.General, "Input.Save()...");

        DuckXML doc = new();
        DXMLNode node = new("Mappings");

        foreach (DeviceInputMapping deviceInputMapping in _defaultInputMapping)
            node.Add(deviceInputMapping.Serialize());
        doc.Add(node);

        DuckFile.SaveDuckXML(doc, $"{DuckFile.optionsDirectory}/input.dat");
    }

    public static void TryFallback(
        string pTrigger,
        string pFallback,
        DeviceInputMapping pMap
        )
    {
        if (pMap.map.ContainsKey(pTrigger) || !pMap.map.TryGetValue(pFallback, out int value))
            return;

        pMap.map[pTrigger] = value;
    }

    public static void SetDefaultMapping(DeviceInputMapping mapping, Profile overrideProfile = null)
    {
        if (overrideProfile != null && MappingIsDefault(mapping))
            return;

        TryFallback(Triggers.MenuLeft, Triggers.Left, mapping);
        TryFallback(Triggers.MenuRight, Triggers.Right, mapping);
        TryFallback(Triggers.MenuUp, Triggers.Up, mapping);
        TryFallback(Triggers.MenuDown, Triggers.Down, mapping);
        TryFallback(Triggers.Menu1, Triggers.Shoot, mapping);
        TryFallback(Triggers.Menu2, Triggers.Grab, mapping);
        TryFallback(Triggers.Cancel, Triggers.Quack, mapping);

        DeviceInputMapping defaultMapping = GetDefaultMapping(mapping.deviceName, mapping.deviceGUID);
        if (defaultMapping != null)
        {
            int num = 0;
            bool flag;
            do
            {
                flag = true;
                foreach (var keyValuePair1 in mapping.map)
                {
                    foreach (var keyValuePair2 in mapping.map)
                    {
                        if (keyValuePair1.Key != keyValuePair2.Key
                         && keyValuePair1.Value == keyValuePair2.Value
                         && Triggers.IsUITrigger(keyValuePair1.Key)
                         && Triggers.IsUITrigger(keyValuePair2.Key)
                         )
                        {
                            if (defaultMapping.map.TryGetValue(keyValuePair1.Key, out int value))
                            {
                                mapping.map[keyValuePair1.Key] = value;
                                flag = false;
                            }

                            if (defaultMapping.map.TryGetValue(keyValuePair2.Key, out value))
                            {
                                mapping.map[keyValuePair2.Key] = value;
                                flag = false;
                            }

                            if (!flag)
                                break;
                        }
                    }
                    if (!flag)
                        break;
                }
                ++num;
            }
            while (!flag && num <= 100);
        }

        List<DeviceInputMapping> source = _defaultInputMapping;

        if (overrideProfile != null)
            source = overrideProfile.inputMappingOverrides;

        DeviceInputMapping deviceInputMapping1 = source.FirstOrDefault(x => x.deviceName == mapping.deviceName && x.deviceGUID == mapping.deviceGUID);
        DeviceInputMapping deviceInputMapping2 = defaultInputMappingPresets.FirstOrDefault(x => x.deviceName == mapping.deviceName && x.deviceGUID == mapping.deviceGUID);

        if (deviceInputMapping1 != null)
        {
            DevConsole.Log(DCSection.General, $"SetDefaultMapping() Found existing map for ({mapping.deviceName})...");
            deviceInputMapping1.map = mapping.map;
            deviceInputMapping1.graphicMap = mapping.graphicMap;

            if (deviceInputMapping2 == null)
                return;

            foreach (var keyValuePair in deviceInputMapping2.map)
                if (!deviceInputMapping1.map.ContainsKey(keyValuePair.Key))
                    deviceInputMapping1.MapInput(keyValuePair.Key, keyValuePair.Value);
        }
        else
        {
            DeviceInputMapping compare = _defaultInputMapping.FirstOrDefault(x => x.deviceName == mapping.deviceName && x.deviceGUID == mapping.deviceGUID);

            if (compare != null)
            {
                if (!mapping.IsEqual(compare))
                {
                    source.Add(mapping);

                    DevConsole.Log(DCSection.General, $"Added input mapping for ({mapping.deviceName})...");
                }
                else
                    DevConsole.Log(DCSection.General, $"Skipped duplicate mapping for ({mapping.deviceName})...");
            }
            else
            {
                DevConsole.Log(DCSection.General, $"Found default settings for ({mapping.deviceName})...");

                if (_defaultInputMapping.FirstOrDefault(x => x.deviceName == "GENERIC GAMEPAD").IsEqual(mapping))
                    return;

                source.Add(mapping);
            }
        }
    }

    public static void SetDefaultMappings(List<DeviceInputMapping> mappings) => _defaultInputMapping = mappings;

    public static void ApplyDefaultMappings()
    {
        foreach (InputProfile defaultProfile in InputProfile.defaultProfiles)
        {
            bool flag = false;

            if (Profiles.all != null)
                foreach (Profile duckProfile in Profiles.active)
                    if (duckProfile.inputProfile == defaultProfile)
                    {
                        ApplyDefaultMapping(defaultProfile, duckProfile);
                        flag = true;
                        break;
                    }

            if (!flag)
                ApplyDefaultMapping(defaultProfile);
        }
    }

    /// <summary>
    /// This function resets an InputProfile to it's default control settings. If duckProfile is defined, it will use the profile's mapping.
    /// Otherwise, it will use the global mapping. If none exist, it will use the built in defaults.
    /// </summary>
    /// <param name="p">InputProfile to reset</param>
    /// <param name="duckProfile">Optional duck profile to take controls from</param>
    public static void ApplyDefaultMapping(InputProfile p = null, Profile duckProfile = null)
    {
        if (p == null)
            DevConsole.Log(DCSection.General, "ApplyDefaultMapping() had a null argument, for some reason?");
        else
        {
            p.ClearMappings();

            if (p.GetDevice(typeof(GenericController)) is GenericController device)
            {
                if (device.device != null)
                {
                    Profile p1 = duckProfile;
                    if (p1 == null && Profiles.all != null)
                        foreach (Profile profile in Profiles.active)
                            if (profile.inputProfile == p)
                            {
                                p1 = profile;
                                break;
                            }

                    DeviceInputMapping deviceInputMapping = GetDefaultMapping(device.device.productName, device.device.productGUID, p: p1)
                                                         ?? GetDefaultMapping(device.device.productName, device.device.productGUID);
                    if (deviceInputMapping != null)
                        foreach (var keyValuePair in deviceInputMapping.map)
                            p.Map(device, keyValuePair.Key, keyValuePair.Value);
                }
                else
                    p.Map(device, "", 0);
            }

            if (p == InputProfile.defaultProfiles[Options.Data.keyboard1PlayerIndex])
            {
                DeviceInputMapping deviceInputMapping = GetDefaultMapping("KEYBOARD P1", "", p: duckProfile)
                                                     ?? GetDefaultMapping("KEYBOARD P1", "");

                if (deviceInputMapping == null)
                    return;

                foreach (var keyValuePair in deviceInputMapping.map)
                    p.Map(GetDevice<Keyboard>(), keyValuePair.Key, keyValuePair.Value);
            }
            else
            {
                if (p != InputProfile.defaultProfiles[Options.Data.keyboard2PlayerIndex])
                    return;

                DeviceInputMapping deviceInputMapping = GetDefaultMapping("KEYBOARD P2", "", p: duckProfile)
                                                     ?? GetDefaultMapping("KEYBOARD P2", "");

                if (deviceInputMapping == null)
                    return;

                foreach (var keyValuePair in deviceInputMapping.map)
                    p.Map(GetDevice<Keyboard>(1), keyValuePair.Key, keyValuePair.Value);
            }
        }
    }

    public static void InitializeGraphics()
    {
        foreach (Keys key in _keys)
        {
            char ch = KeyHelper.KeyToChar(key);
            if (ch > ' ' && ch < '\u007F')
                keyToChar[key] = ch;
        }

        _triggerImageMap.Add("MOUSEWHEEL", new Sprite("buttons/mousewheel"));
        _triggerImageMap.Add("PLANET", new Sprite("smallEarth"));
        _triggerImageMap.Add("ARENA", new Sprite("smallArena"));
        _triggerImageMap.Add("MOON", new Sprite("smallMoon"));
        _triggerImageMap.Add("PLUG", new Sprite("plugRect"));
        _triggerImageMap.Add("UNPLUG", new Sprite("unplugRect"));
        _triggerImageMap.Add("CLIPCOPY", new Sprite("clipcopy"));
        _triggerImageMap.Add("SKIPICON", new Sprite("skipIcon"));
        _triggerImageMap.Add("SETTINGSCHANGED", new Sprite("wrenchRect"));
        _triggerImageMap.Add("NORMALICON", new Sprite("normalIcon"));
        _triggerImageMap.Add("RAINBOWICON", new Sprite("rainbowIcon"));
        _triggerImageMap.Add("CUSTOMICON", new Sprite("customIcon"));
        _triggerImageMap.Add("RANDOMICON", new Sprite("randomIcons"));
        _triggerImageMap.Add("ESCAPE", new Sprite("buttons/keyboard/escape"));
        _triggerImageMap.Add("CONSOLE", new Sprite("buttons/keyboard/tilde"));
        _triggerImageMap.Add("TINYLOCK", new Sprite("tinyLock"));
        _triggerImageMap.Add("RETICULE", new Sprite("challenge/reticule"));
        _triggerImageMap.Add("TICKET", new Sprite("arcade/ticket"));
        _triggerImageMap.Add("CHECK", new Sprite("checkIcon"));
        _triggerImageMap.Add("F1", new Sprite("buttons/keyboard/f1"));
        _triggerImageMap.Add("ALT", new Sprite("buttons/keyboard/alt"));
        _triggerImageMap.Add("COMMA", new KeyImage(','));
        _triggerImageMap.Add("SPEEDCLOCK", new Sprite("speedrunClock"));
        _triggerImageMap.Add("STARGOODY", new Sprite("challenge/star"));
        _triggerImageMap.Add("SUITCASEGOODY", new Sprite("challenge/suitcase"));
        _triggerImageMap.Add("LAPGOODY", new Sprite("challenge/goal"));
        _triggerImageMap.Add("EDITORCURRENCY", new Sprite("editorCurrency"));
        _triggerImageMap.Add("LWING", new Sprite("arcade/titleWing"));

        {

            Sprite sprite = new("arcade/titleWing")
            {
                flipH = true
            };
            sprite.CenterX = sprite.width;
            _triggerImageMap.Add("RWING", sprite);
        }

        {
            Sprite sprite = new("arcade/titleWing")
            {
                color = new Color(96, 119, 124)
            };
            ++sprite.CenterY;
            _triggerImageMap.Add("LWINGGRAY", sprite);
        }

        {
            Sprite sprite = new("arcade/titleWing")
            {
                flipH = true
            };
            sprite.CenterX = sprite.width;
            ++sprite.CenterY;
            sprite.color = new Color(96, 119, 124);
            _triggerImageMap.Add("RWINGGRAY", sprite);
        }

        _triggerImageMap.Add("WRENCH", new Sprite("titleWrench"));
        _triggerImageMap.Add("SCREWDRIVER", new Sprite("titleScrewdriver"));
        _triggerImageMap.Add("BASELINE", new SpriteMap("challengeTrophyIcons", 16, 16)
        {
            frame = 0
        });
        _triggerImageMap.Add("BRONZE", new SpriteMap("challengeTrophyIcons", 16, 16)
        {
            frame = 1
        });
        _triggerImageMap.Add("SILVER", new SpriteMap("challengeTrophyIcons", 16, 16)
        {
            frame = 2
        });
        _triggerImageMap.Add("GOLD", new SpriteMap("challengeTrophyIcons", 16, 16)
        {
            frame = 3
        });
        _triggerImageMap.Add("PLATINUM", new SpriteMap("challengeTrophyIcons", 16, 16)
        {
            frame = 4
        });
        _triggerImageMap.Add("DEVELOPER", new SpriteMap("challengeTrophyIcons", 16, 16)
        {
            frame = 5
        });
        _triggerImageMap.Add("ONLINEBAD", new SpriteMap("onlineStatusIcons", 7, 7)
        {
            frame = 0
        });
        _triggerImageMap.Add("ONLINENEUTRAL", new SpriteMap("onlineStatusIcons", 7, 7)
        {
            frame = 1
        });
        _triggerImageMap.Add("ONLINEGOOD", new SpriteMap("onlineStatusIcons", 7, 7)
        {
            frame = 2
        });

        {
            Sprite sprite = new("crownIcon")
            {
                Scale = new Vector2(0.5f, 0.5f)
            };
            sprite.CenterY -= 6;
            _triggerImageMap.Add("HOSTCROWN", sprite);
        }

        _triggerImageMap.Add("SUBPLUS", new Sprite("subPlus"));

        {
            Sprite sprite = new("steamIcon")
            {
                Scale = new Vector2(0.25f)
            };
            sprite.CenterY -= 48;
            _triggerImageMap.Add("STEAMICON", sprite);
        }

        {
            Sprite sprite = new("steamIcon")
            {
                Scale = new Vector2(0.5f)
            };
            sprite.CenterY -= 16;
            _triggerImageMap.Add("STEAMICONMED", sprite);
        }

        {
            Sprite sprite = new("accessIcon")
            {
                Scale = new Vector2(0.5f)
            };
            sprite.CenterY -= 8;
            _triggerImageMap.Add("ACCESSICON", sprite);
        }

        {
            Sprite sprite = new("vanillaIcon")
            {
                Scale = new Vector2(0.5f)
            };
            sprite.CenterY -= 8;
            _triggerImageMap.Add("VANILLAICON", sprite);
        }

        _triggerImageMap.Add("SPECTATOR", new Sprite("spectatorIcon"));
        _triggerImageMap.Add("SPECTATORBIG", new Sprite("spectatorIcon")
        {
            Scale = new Vector2(2f)
        });

        {
            Sprite sprite = new("discordIcon")
            {
                Scale = new Vector2(0.25f, 0.25f)
            };
            sprite.CenterY -= 48;
            _triggerImageMap.Add("DISCORDICON", sprite);
        }

        _triggerImageMap.Add("_!DUCKSPAWN", new Sprite("singleDuck")
        {
            Scale = new Vector2(1f, 1f)
        });
        _triggerImageMap.Add("SKIPSPIN", new Sprite("skipSpin"));
        _triggerImageMap.Add("error", new Sprite("exclamationMoji"));
        _triggerImageMap.Add("LOGEVENT", new Sprite("logEvent"));
        _triggerImageMap.Add("sent", new Sprite("networkSent"));
        _triggerImageMap.Add("received", new Sprite("networkReceived"));
        _triggerImageMap.Add("disconnect", new Sprite("networkDisconnect"));
        _triggerImageMap.Add("netdrop", new Sprite("networkDrop"));
        _triggerImageMap.Add("blacklist", new Sprite("blacklistX"));
        _triggerImageMap.Add("SIGNALDEAD", new SpriteMap("signal", 8, 5)
        {
            frame = 0
        });
        _triggerImageMap.Add("SIGNALBAD", new SpriteMap("signal", 8, 5)
        {
            frame = 1
        });
        _triggerImageMap.Add("SIGNALNORMAL", new SpriteMap("signal", 8, 5)
        {
            frame = 2
        });
        _triggerImageMap.Add("SIGNALGOOD", new SpriteMap("signal", 8, 5)
        {
            frame = 3
        });
        _triggerImageMap.Add("PLUSKEY", new KeyImage('+'));
        _triggerImageMap.Add("ENTERKEY", new Sprite("buttons/keyboard/enter"));
        _triggerImageMap.Add("ESCAPEKEY", new Sprite("buttons/keyboard/escape"));
        _triggerImageMap.Add("ICONGRADIENT", new Sprite("iconGradient"));
        _triggerImageMap.Add("CHANCEICON", new SpriteMap("chanceIcon", 10, 9)
        {
            frame = 0,
            CenterY = 1f
        });
        _triggerImageMap.Add("ICONEIGHT", new SpriteMap("iconEight", 8, 8));
        _triggerImageMap.Add("LEFTMOUSE", new SpriteMap("buttons/mouse", 12, 15)
        {
            frame = 0
        });
        _triggerImageMap.Add("MIDDLEMOUSE", new SpriteMap("buttons/mouse", 12, 15)
        {
            frame = 1
        });
        _triggerImageMap.Add("RIGHTMOUSE", new SpriteMap("buttons/mouse", 12, 15)
        {
            frame = 2
        });
        _triggerImageMap.Add("LOADICON", new SpriteMap("iconSheet", 16, 16)
        {
            frame = 1
        });
        _triggerImageMap.Add("SAVEICON", new SpriteMap("iconSheet", 16, 16)
        {
            frame = 2
        });
        _triggerImageMap.Add("LOADICONTINY", new SpriteMap("iconSheet", 16, 16)
        {
            Scale = new Vector2(0.5f, 0.5f),
            CenterY = -6f,
            frame = 1
        });
        _triggerImageMap.Add("LOCKEDFOLDERICON", new SpriteMap("iconSheet", 16, 16)
        {
            frame = 14
        });
        _triggerImageMap.Add("FOLDERICON", new SpriteMap("tinyIcons", 8, 8)
        {
            frame = 2
        });
        _triggerImageMap.Add("FOLDERDELETEICON", new SpriteMap("tinyIcons", 8, 8)
        {
            frame = 8
        });
        _triggerImageMap.Add("SELECTICON", new SpriteMap("tinyIcons", 8, 8)
        {
            frame = 3
        });
        _triggerImageMap.Add("DELETEFLAG_OFF", new SpriteMap("deleteFlag", 8, 8)
        {
            frame = 0
        });
        _triggerImageMap.Add("DELETEFLAG_ON", new SpriteMap("deleteFlag", 8, 8)
        {
            frame = 1
        });

        {
            Sprite sprite = new("muteIcon")
            {
                Scale = new Vector2(0.5f, 0.5f)
            };
            sprite.CenterY -= 6;
            _triggerImageMap.Add("MUTEICON", sprite);
        }

        {
            Sprite sprite = new("blockIcon")
            {
                Scale = new Vector2(0.5f, 0.5f)
            };
            sprite.CenterY -= 6;
            _triggerImageMap.Add("BLOCKICON", sprite);
        }

        {
            Sprite sprite = new("blockIcon")
            {
                Scale = new Vector2(0.25f, 0.25f)
            };
            sprite.CenterY -= 9;
            _triggerImageMap.Add("BLOCKICONSMALL", sprite);
        }

        _triggerImageMap.Add("SAVEICONTINY", new SpriteMap("iconSheet", 16, 16)
        {
            Scale = new Vector2(0.5f, 0.5f),
            CenterY = -6f,
            frame = 2
        });
        _triggerImageMap.Add("NEWICONTINY", new SpriteMap("iconSheet", 16, 16)
        {
            Scale = new Vector2(0.5f, 0.5f),
            CenterY = -6f,
            frame = 0
        });
        _triggerImageMap.Add("RAINBOWTINY", new SpriteMap("tinyIcons", 8, 8)
        {
            frame = 6
        });
        _triggerImageMap.Add("happyface", new SpriteMap("moji", 11, 11)
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f,
            frame = 0
        });
        _triggerImageMap.Add("sadface", new SpriteMap("moji", 11, 11)
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f,
            frame = 1
        });
        _triggerImageMap.Add("puffyface", new SpriteMap("moji", 11, 11)
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f,
            frame = 2
        });
        _triggerImageMap.Add("angryface", new SpriteMap("moji", 11, 11)
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f,
            frame = 3
        });
        _triggerImageMap.Add("yayface", new SpriteMap("moji", 11, 11)
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f,
            frame = 4
        });
        _triggerImageMap.Add("shrug", new SpriteMap("shrug", 78, 24)
        {
            Scale = new Vector2(1f, 1f),
            CenterY = -1f
        });
        _triggerImageMap.Add("wowface", new SpriteMap("moji", 11, 11)
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f,
            frame = 5
        });
        _triggerImageMap.Add("wtfface", new SpriteMap("moji", 11, 11)
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f,
            frame = 6
        });
        _triggerImageMap.Add("straightface", new SpriteMap("moji", 11, 11)
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f,
            frame = 7
        });
        _triggerImageMap.Add("oiface", new SpriteMap("moji", 11, 11)
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f,
            frame = 8
        });
        _triggerImageMap.Add("blankface", new SpriteMap("moji", 11, 11)
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f,
            frame = 9
        });
        _triggerImageMap.Add("sweatface", new SpriteMap("moji", 11, 11)
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f,
            frame = 10
        });
        _triggerImageMap.Add("cryface", new SpriteMap("moji", 11, 11)
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f,
            frame = 11
        });
        _triggerImageMap.Add("cooked", new Sprite("cookedDuck")
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 2f
        });
        _triggerImageMap.Add("rip", new Sprite("grave")
        {
            Scale = new Vector2(2f, 2f),
            CenterY = 0f
        });
        _triggerImageMap.Add("searchicon", new SpriteMap("searchicon", 16, 16)
        {
            Scale = new Vector2(0.5f, 0.5f),
            CenterY = -6f,
            CenterX = -4f
        });
        _triggerImageMap.Add("languageFilterOn", new Sprite("filterOn", 16f, 16f)
        {
            Scale = new Vector2(0.5f, 0.5f),
            Center = new Vector2(0f, -9f)
        });
        _triggerImageMap.Add("languageFilterOff", new Sprite("filterOff", 16f, 16f)
        {
            Scale = new Vector2(0.5f, 0.5f),
            Center = new Vector2(0f, -9f)
        });
        _triggerImageMap.Add("searchiconwhite", new SpriteMap("searchiconwhite", 16, 16)
        {
            Scale = new Vector2(0.5f, 0.5f),
            CenterY = -6f,
            CenterX = -4f
        });
        _triggerImageMap.Add("searchiconwhitebig", new SpriteMap("searchiconwhite", 16, 16)
        {
            Scale = new Vector2(1f, 1f),
            CenterY = -6f,
            CenterX = -4f
        });
        _triggerImageMap.Add("cloudicon", new SpriteMap("cloudIcon", 16, 16)
        {
            Scale = new Vector2(1f, 1f),
            CenterY = 0f,
            CenterX = 0f
        });

        {
            SpriteMap spriteMap = new("exBox", 10, 10)
            {
                frame = 0,
                Scale = new Vector2(0.5f)
            };
            spriteMap.CenterY -= 6;
            _triggerImageMap.Add("ITEMBOX", spriteMap);
        }

        {
            SpriteMap spriteMap = new("exBox", 10, 10)
            {
                frame = 1,
                Scale = new Vector2(0.5f, 0.5f)
            };
            spriteMap.CenterY -= 6;
            _triggerImageMap.Add("USERONLINE", spriteMap);
        }

        {
            SpriteMap spriteMap = new("exBox", 10, 10)
            {
                frame = 2,
                Scale = new Vector2(0.5f, 0.5f)
            };
            spriteMap.CenterY -= 6;
            _triggerImageMap.Add("USERAWAY", spriteMap);
        }

        {
            SpriteMap spriteMap = new("exBox", 10, 10)
            {
                frame = 3,
                Scale = new Vector2(0.5f, 0.5f)
            };
            spriteMap.CenterY -= 6;
            _triggerImageMap.Add("USERBUSY", spriteMap);
        }

        {
            SpriteMap spriteMap = new("exBox", 10, 10)
            {
                frame = 4,
                Scale = new Vector2(0.5f, 0.5f)
            };
            spriteMap.CenterY -= 6;
            _triggerImageMap.Add("USEROFFLINE", spriteMap);
        }

        _triggerImageMap.Add("KBDSHIFT", new Sprite("buttons/keyboard/shift"));
        _triggerImageMap.Add("KBDARROWS", new Sprite("buttons/keyboard/arrows"));
        _buttonStyles.Add(new Sprite("buttons/xbox/oButton"));
        _buttonStyles.Add(new Sprite("buttons/xbox/aButton"));
        _buttonStyles.Add(new Sprite("buttons/xbox/uButton"));
        _buttonStyles.Add(new Sprite("buttons/xbox/yButton"));
        _buttonStyles.Add(new Sprite("buttons/xbox/startButton"));
        _buttonStyles.Add(new Sprite("buttons/xbox/selectButton"));
        _buttonStyles.Add(new Sprite("buttons/xbox/dPadLeft"));
        _buttonStyles.Add(new Sprite("buttons/xbox/dPadRight"));
        _buttonStyles.Add(new Sprite("buttons/xbox/dPadUp"));
        _buttonStyles.Add(new Sprite("buttons/xbox/dPadDown"));
        _buttonStyles.Add(new Sprite("buttons/xbox/leftBumper"));
        _buttonStyles.Add(new Sprite("buttons/xbox/rightBumper"));
        _buttonStyles.Add(new Sprite("buttons/xbox/leftTrigger"));
        _buttonStyles.Add(new Sprite("buttons/xbox/rightTrigger"));
        _buttonStyles.Add(new Sprite("buttons/xbox/leftStick"));
        _buttonStyles.Add(new Sprite("buttons/xbox/rightStick"));
        _buttonStyles.Add(new Sprite("buttons/playstation/o"));
        _buttonStyles.Add(new Sprite("buttons/playstation/square"));
        _buttonStyles.Add(new Sprite("buttons/playstation/triangle"));
        _buttonStyles.Add(new Sprite("buttons/playstation/x"));
        _buttonStyles.Add(new Sprite("buttons/playstation/startButton"));
        _buttonStyles.Add(new Sprite("buttons/playstation/selectButton"));
        _buttonStyles.Add(new Sprite("buttons/playstation/leftBumper"));
        _buttonStyles.Add(new Sprite("buttons/playstation/rightBumper"));
        _buttonStyles.Add(new Sprite("buttons/playstation/leftTrigger"));
        _buttonStyles.Add(new Sprite("buttons/playstation/rightTrigger"));
        _buttonStyles.Add(new Sprite("buttons/SNES/a"));
        _buttonStyles.Add(new Sprite("buttons/SNES/b"));
        _buttonStyles.Add(new Sprite("buttons/SNES/x"));
        _buttonStyles.Add(new Sprite("buttons/SNES/y"));
        _buttonStyles.Add(new Sprite("buttons/SNES/aFami"));
        _buttonStyles.Add(new Sprite("buttons/SNES/bFami"));
        _buttonStyles.Add(new Sprite("buttons/SNES/xFami"));
        _buttonStyles.Add(new Sprite("buttons/SNES/yFami"));
        _buttonStyles.Add(new Sprite("buttons/SNES/startButton"));
        _buttonStyles.Add(new Sprite("buttons/SNES/selectButton"));
        _buttonStyles.Add(new Sprite("buttons/SNES/leftTrigger"));
        _buttonStyles.Add(new Sprite("buttons/SNES/rightTrigger"));
        _buttonStyles.Add(new Sprite("buttons/genesis/a"));
        _buttonStyles.Add(new Sprite("buttons/genesis/b"));
        _buttonStyles.Add(new Sprite("buttons/genesis/c"));
        _buttonStyles.Add(new Sprite("buttons/genesis/start"));
        _buttonStyles.Add(new Sprite("buttons/playstation/blank"));
        _buttonStyles.Add(new Sprite("buttons/genericButton"));
    }

    public static void InitDefaultProfiles()
    {
        string[] keys = [.. InputProfile.profiles.Keys];
        int MPPlayerCount = 0;

        for (int i = 0; i < keys.Length; i++)
            if (keys[i].StartsWith("MPPlayer"))
                MPPlayerCount += 1;

        for (int index = MPPlayerCount; index < DG.MaxPlayers; ++index)
        {
            InputProfile inputProfile = InputProfile.Add($"MPPlayer{index + 1}");
            if (inputProfile.mpIndex != -1)
                continue;

            inputProfile.mpIndex = index;
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Left, 4);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Right, 8);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Up, 1);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Down, 2);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Jump, 4096);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Shoot, 16384);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Grab, 32768);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Quack, 8192);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Start, 16);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Strafe, 256);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Ragdoll, 512);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.LeftTrigger, 8388608);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.RightTrigger, 4194304);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Select, 4096);
            inputProfile.Map(GetDevice<GenericController>(index), Triggers.Cancel, 8192);

            if (index == 0)
                InputProfile.active = inputProfile;
        }

        ApplyDefaultMappings();
        InputProfile.Add("Blank");
    }

    public static void ReInitialize()
    {
        DevConsole.Log(DCSection.General, "ReInitializing Input...");

        List<InputDevice> XInputPads = new(MonoMain.MaximumGamepadCount);
        List<GenericController> genericControllers = new(DG.MaxPlayers);

        _gamePads.Clear();

        int index = 0;
        while (index < _devices.Count)
        {
            InputDevice device = _devices[index];
            if (device is XInputPad)
            {
                if (XInputPads.Count < MonoMain.MaximumGamepadCount)
                    XInputPads.Add(device);
                _devices.RemoveAt(index);
            }
            else if (device is GenericController)
            {
                if (genericControllers.Count < DG.MaxPlayers)
                    genericControllers.Add(device as GenericController);
                _devices.RemoveAt(index);
            }
            else
                index++;
        }

        for (int i = XInputPads.Count; i < MonoMain.MaximumGamepadCount; i++)
        {
            XInputPad XInputDevice = new(i);
            XInputPads.Add(XInputDevice);
            XInputDevice.InitializeState();
        }

        _devices.AddRange(XInputPads);
        _devices.AddRange(genericControllers);
        _gamePads.AddRange(genericControllers);

        for (int i = genericControllers.Count; i < DG.MaxPlayers; i++)
        {
            GenericController genericController = new(i);
            _gamePads.Add(genericController);
            _devices.Add(genericController);
        }

        EnumerateGamepads();
        InitDefaultProfiles();
    }

    public static void Initialize()
    {
        DevConsole.Log(DCSection.General, "Initializing Input...");

        foreach (DeviceInputMapping inputMappingPreset in _defaultInputMappingPresets)
            _defaultInputMapping.Add(inputMappingPreset.Clone());

        InputDevice device = new Keyboard("KEYBOARD P1", 0);

        _devices.Add(device);
        _devices.Add(new Keyboard("KEYBOARD P2", 1));
        _devices.Add(new Mouse());

        for (int index = 0; index < MonoMain.MaximumGamepadCount; index++)
        {
            XInputPad XInputDevice = new XInputPad(index);
            _devices.Add(XInputDevice);
            XInputDevice.InitializeState();
        }

        InputProfile.Default = new InputProfile("Default");

        for (int i = 0; i < DG.MaxPlayers; i++)
        {
            GenericController genericController = new GenericController(i);
            _gamePads.Add(genericController);
            _devices.Add(genericController);
        }

        InputProfile.Default.Map(device, Triggers.Left, 37);
        InputProfile.Default.Map(device, Triggers.Right, 39);
        InputProfile.Default.Map(device, Triggers.Up, 38);
        InputProfile.Default.Map(device, Triggers.Down, 40);
        InputProfile.Default.Map(GetDevice<XInputPad>(), Triggers.Left, 4);
        InputProfile.Default.Map(GetDevice<XInputPad>(), Triggers.Right, 8);
        InputProfile.Default.Map(GetDevice<XInputPad>(), Triggers.Up, 1);
        InputProfile.Default.Map(GetDevice<XInputPad>(), Triggers.Down, 2);

        _profiles[InputProfile.Default.name] = InputProfile.Default;

        EnumerateGamepads();
        InitDefaultProfiles();

        string str = DuckFile.optionsDirectory + "/input.dat";
        if (MonoMain.defaultControls)
        {
            DevConsole.Log(DCSection.General, "Clearing input settings (MonoMain.defaultControls == true)");
            DuckFile.Delete(str);
        }
        else
        {
            if (!DuckFile.FileExists(str) && DGSave.upgradingFromVanilla || MonoMain.oldDefaultControls)
            {
                DevConsole.Log(DCSection.General, "Saving old input defaults...");

                foreach (DeviceInputMapping oldInputDefault in _oldInputDefaults)
                    SetDefaultMapping(oldInputDefault);

                Save();
            }

            DuckXML duckXml = DuckFile.LoadDuckXML(str);
            if (duckXml == null)
                return;

            IEnumerable<DXMLNode> source = duckXml.Elements("Mappings");
            if (source == null)
                return;

            foreach (DXMLNode element in source.Elements())
                if (element.Name == "InputMapping")
                {
                    DeviceInputMapping mapping = new();
                    mapping.Deserialize(element);
                    SetDefaultMapping(mapping);
                }
        }
    }

    public static void EnumerateGamepads()
    {
        foreach (GenericController gamePad in _gamePads)
        {
            AnalogGamePad device1 = gamePad.device;

            if (device1 != null && !device1.isConnected)
            {
                _changePluggedIn = false;
                _changeName = device1.productName;
                _padConnectionChange = true;
                _gamepadsChanged = true;
                gamePad.device = null;
            }

            if (gamePad.device == null)
                foreach (InputDevice device2 in _devices)
                    if (device2 is not GenericController && device2.isConnected && device2.genericController == null)
                        if (device2 is XInputPad)
                        {
                            gamePad.device = device2 as AnalogGamePad;
                            _gamepadsChanged = true;
                            _changePluggedIn = true;
                            _changeName = gamePad.device.productName;
                            _padConnectionChange = true;
                            break;
                        }
        }
    }

    public static void Update()
    {
        try
        {
            bool notlinux = !Program.isLinux;

            if (notlinux && !_initializedMessageHook)
            {
                InputSystem.Initialize(MonoMain.instance.Window);
                _initializedMessageHook = true;
            }
            if (notlinux && Options.Data.imeSupport && !_initializedIME)
            {
                InputSystem.InitializeIme(MonoMain.instance.Window);
                InputSystem.IMECharEntered += new CharEnteredHandler(Keyboard.IMECharEnteredHandler);
                _initializedIME = true;
            }

            bool flag = Options.Data.imeSupport && _imeAllowed;
            if (notlinux && flag != _prevImeAllowed)
            {
                if (flag)
                    InputSystem.StartIME();
                else
                    InputSystem.EndIME();
            }

            _prevImeAllowed = flag;
            _imeAllowed = false;

            if (devicesChanged)
            {
                ++_deviceUpdateWait;
                if (_deviceUpdateWait > 90) // 1.5 second instead of the old 2 seconds
                {
                    _deviceUpdateWait = 0;
                    devicesChanged = false;
                    ++timesToEnumerateGamepads;
                    EnumerateGamepads();
                }
            }

            if (_gamepadsChanged)
            {
                ApplyDefaultMappings();
                TeamSelect2.ControllerLayoutsChanged();
                _gamepadsChanged = false;
                uiDevicesHaveChanged = true;
            }

            if (_updateWaitFrames > 0)
                --_updateWaitFrames;
            else
            {
                if (_padConnectionChange)
                {
                    _padConnectionChange = false;
                    if (MonoMain.started && !_ignoreFirstInputChange && _suppressInputChangeMessages <= 0)
                    {
                        _changeName = _changeName.Trim();

                        if (_changeName.Length > 25)
                            _changeName = _changeName[..25] + "...";

                        string str = "@PLUG@|LIME|";
                        if (!_changePluggedIn)
                            str = "@UNPLUG@|RED|";

                        HUD.AddInputChangeDisplay(str + _changeName);
                    }
                }

                foreach (InputDevice device in _devices)
                    device.Update();
            }

            if (MonoMain.started)
                _ignoreFirstInputChange = false;

            if (_suppressInputChangeMessages <= 0)
                return;

            --_suppressInputChangeMessages;
        }
        catch (Exception e)
        {
            DevConsole.Log(e.Message);
        }
    }

    public static void Terminate()
    {
        _gamepadThread?.Abort();
        _gamepadThread = null;

        InputSystem.Terminate();
        for (int index = 0; index < MonoMain.MaximumGamepadCount; index++)
        {
            var playerIndex = (PlayerIndex)index; //new
            GamePadState state = GamePad.GetState(playerIndex, GamePadDeadZone.IndependentAxes); //new
            //GamePadState state = GamePad.GetState(index, GamePadDeadZone.IndependentAxes); old

            if (state.IsConnected)
                GamePad.SetVibration(playerIndex, 0f, 0f); //new
                                                           //GamePad.SetVibration(index, 0f, 0f); old
        }
    }

    public static bool MappingIsDefault(DeviceInputMapping pMapping)
    {
        DeviceInputMapping defaultMapping = GetDefaultMapping(pMapping.deviceName, pMapping.deviceGUID);
        return defaultMapping != null && defaultMapping.IsEqual(pMapping);
    }

    public static bool CheckCode(InputCode code)
    {
        foreach (var profile in InputProfile.profiles)
            if (profile.Value.virtualDevice == null && profile.Value.CheckCode(code))
                return true;
        return false;
    }

    public static bool Pressed(string trigger, string profile = "Any")
    {
        if (profile == "Any")
        {
            foreach (var profile1 in InputProfile.profiles)
                if (profile1.Value.virtualDevice == null && profile1.Value.Pressed(trigger))
                    return true;
            return false;
        }
        return _profiles.TryGetValue(profile, out InputProfile inputProfile) && inputProfile.Pressed(trigger);
    }

    public static bool Released(string trigger, string profile = "Any")
    {
        if (profile == "Any")
        {
            foreach (var profile1 in InputProfile.profiles)
                if (profile1.Value.Released(trigger))
                    return true;
            return false;
        }
        return _profiles.TryGetValue(profile, out InputProfile inputProfile) && inputProfile.Released(trigger);
    }

    public static bool Down(string trigger, string profile = "Any")
    {
        if (profile == "Any")
        {
            foreach (var profile1 in InputProfile.profiles)
                if (profile1.Value.Down(trigger))
                    return true;
            return false;
        }
        return _profiles.TryGetValue(profile, out InputProfile inputProfile) && inputProfile.Down(trigger);
    }

    public static T GetDevice<T>(int index = 0) where T : InputDevice
    {
        Type type = typeof(T);

        foreach (InputDevice device in _devices)
            if (type.IsAssignableFrom(device.GetType()) && device.index == index)
                return device as T;

        return default;
    }

    public static Sprite GetTriggerSprite(string trigger)
    {
        _triggerImageMap.TryGetValue(trigger, out var triggerSprite);
        return triggerSprite;
    }

    public static DeviceInputMapping GetDefaultMapping(
        string productName,
        string productGUID,
        bool presets = false,
        bool makeClone = true,
        Profile p = null
        )
    {
        if (p != null && p.linkedProfile != null)
            return GetDefaultMapping(productName, productGUID, presets, makeClone, p.linkedProfile);

        List<DeviceInputMapping> source = _defaultInputMapping;

        if (p != null && p.inputMappingOverrides.FirstOrDefault(x => x.deviceGUID == productGUID && x.deviceName == productName) == null)
            p = null;
        if (presets)
            source = defaultInputMappingPresets;
        if (p != null)
            source = p.inputMappingOverrides;

        foreach (DeviceInputMapping defaultMapping in source)
            if (defaultMapping.deviceName == productName && defaultMapping.deviceGUID == productGUID)
                return defaultMapping;

        if (p != null)
            return null;

        DeviceInputMapping defaultMapping1 = source.FirstOrDefault(x => x.deviceName == "GENERIC GAMEPAD");
        if (!makeClone)
            return defaultMapping1;
        if (defaultMapping1 == null)
            return new DeviceInputMapping();

        DeviceInputMapping defaultMapping2 = defaultMapping1.Clone();
        defaultMapping2.deviceName = productName;
        defaultMapping2.deviceGUID = productGUID;
        return defaultMapping2;
    }

    public static InputDevice GetDevice(string name)
    {
        foreach (InputDevice device in _devices)
            if (device.name == name)
                return device;

        return null;
    }

    public static List<InputDevice> GetInputDevices() => _devices;

    public static List<DeviceInputMapping> CloneDefaultMappings()
    {
        List<DeviceInputMapping> deviceInputMappingList = [];
        foreach (DeviceInputMapping deviceInputMapping in _defaultInputMapping)
            deviceInputMappingList.Add(deviceInputMapping.Clone());
        return deviceInputMappingList;
    }

    #endregion
}