using Microsoft.Xna.Framework;
using SDL3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

#if FACEPUNCH
using Steamworks;
#else
using Steam;
#endif

namespace DuckGame;

public class DevConsole
{
    class QueuedCommand
    {
        public int wait;

        public string command;

        public Func<bool> waitCommand;
    }

    #region Public Fields

    public static bool showFPS;
    public static bool fancyMode;
    public static bool debugOrigin;
    public static bool debugBounds;
    public static bool wagnusDebug;
    public static bool fuckUpPacketOrder;

    public static CMD lastCommand;
    public static Sprite _tray;
    public static Sprite _scan;

    public static List<string> startupCommands = [];
    public static Dictionary<string, List<CMD>> commands = [];
    public static List<DCLine> debuggerLines = [];

    #endregion

    #region Private Fields

    static bool _enableNetworkDebugging;
    static bool _oldConsole;

    static string _dataSubmissionMessage;

    static DevConsoleCore _core = new();
    static RasterFont _raster;

    static List<ulong> lostSaveIDs = [76561198035257896uL];
    static Queue<QueuedCommand> _pendingCommandQueue = new();

    #endregion

    #region Public Properties

    public static bool open => _core.open;
    public static bool enableNetworkDebugging
    {
        get => _enableNetworkDebugging;
        set => _enableNetworkDebugging = value;
    }
    public static bool splitScreen
    {
        get => _core.splitScreen;
        set => _core.splitScreen = value;
    }
    public static bool rhythmMode
    {
        get => _core.rhythmMode;
        set => _core.rhythmMode = value;
    }
    public static bool qwopMode
    {
        get => _core.qwopMode;
        set => _core.qwopMode = value;
    }
    public static bool showIslands
    {
        get => _core.showIslands;
        set => _core.showIslands = value;
    }
    public static bool showCollision
    {
        get
        {
            if (_core.showCollision)
                return !Network.isActive;
            return false;
        }
        set => _core.showCollision = value;
    }
    public static bool shieldMode
    {
        get => _core.shieldMode;
        set => _core.shieldMode = value;
    }

    public static int consoleScale => Options.Data.consoleScale;
    public static int fontPoints => Options.Data.consoleFontSize;

    public static string fontName => Options.Data.consoleFont;

    public static Vector2 size => new(1280, 1280 / Resolution.current.aspect);

    public static Vector2 dimensions => new(Options.Data.consoleWidth / 100F, Options.Data.consoleHeight / 100F);

    public static DevConsoleCore core
    {
        get => _core;
        set => _core = value;
    }

    #endregion

    #region Public Methods

    public static void SuppressDevConsole()
    {
        _oldConsole = _enableNetworkDebugging;
        _enableNetworkDebugging = false;
    }

    public static void RestoreDevConsole()
    {
        _enableNetworkDebugging = _oldConsole;
    }

    public static void DrawLine(Vector2 pos, DCLine line, bool times, bool section)
    {
        var timeString = "";
        timeString += line.timestamp.Minute;

        if (timeString.Length == 1)
            timeString = $" {timeString}";

        timeString += ":";

        if (line.timestamp.Second < 10)
            timeString += "0";

        timeString += line.timestamp.Second;
        core.font.Scale = new Vector2(1);
        core.font.Draw($"{(times ? $"|GRAY|{timeString} " : "")}{(section ? line.SectionString(colored: true, small: true) : "")}{line.line}", pos.X, pos.Y, line.color * 0.8f, 0.9f);
        core.font.Scale = new Vector2(2);
    }

    public static void InitializeFont()
    {
        if (Options.Data.consoleFont == "" || Options.Data.consoleFont == null)
            _raster = null;
        else
            _raster ??= new RasterFont(Options.Data.consoleFont, Options.Data.consoleFontSize);
    }

    public static void Draw()
    {
        if (Layer.core._console != null)
        {
            Layer.core._console.camera.width = Resolution.current.x / 2;
            Layer.core._console.camera.height = Resolution.current.y / 2;
        }

        if (_core.font == null)
        {
            _core.font = new BitmapFont("biosFont", 8)
            {
                Scale = new Vector2(2)
            };
            _core.fancyFont = new FancyBitmapFont("smallFont")
            {
                Scale = new Vector2(2)
            };
        }

        if (!(_core.alpha > 0.01f))
            return;

        InitializeFont();

        if (_tray == null)
            return;

        _tray.Alpha = _core.alpha;
        _tray.Scale = new Vector2((float)(float.Round(Resolution.current.x / 1280F * 2) / 2) * 2) * (consoleScale + 1) / 2f;
        _tray.Depth = 0.75f;

        var numSectionsVert = (int)(Layer.core._console.camera.height * dimensions.Y / (16 * _tray.Scale.Y)) - 2;
        var numSectionsHor = (int)(Layer.core._console.camera.width * dimensions.X / (16 * _tray.Scale.X)) - 2;

        Graphics.Draw(_tray, 0, 0, new RectangleF(0, 0, 18, 18));
        Graphics.Draw(_tray, 0, 18 * _tray.Scale.Y + numSectionsVert * (16 * _tray.Scale.Y), new RectangleF(0, _tray.height - 18, 18, 18));
        Graphics.Draw(_tray, 18 * _tray.Scale.X + (numSectionsHor - 6) * (16 * _tray.Scale.X), 18 * _tray.Scale.Y + numSectionsVert * (16 * _tray.Scale.Y), new RectangleF(_tray.width - 114, _tray.height - 18, 114, 18));

        for (int i = 0; i < numSectionsHor; i++)
        {
            Graphics.Draw(_tray, 18 * _tray.Scale.X + 16 * _tray.Scale.X * i, 0, new RectangleF(16, 0, 16, 18));
            if (i < numSectionsHor - 6)
                Graphics.Draw(_tray, 18 * _tray.Scale.X + 16 * _tray.Scale.X * i, 18 * _tray.Scale.Y + numSectionsVert * (16 * _tray.Scale.Y), new RectangleF(16, _tray.height - 18, 16, 18));
        }

        Graphics.Draw(_tray, 18 * _tray.Scale.X + numSectionsHor * (16 * _tray.Scale.X), 0, new RectangleF(_tray.width - 18, 0, 18, 18));

        for (int j = 0; j < numSectionsVert; j++)
        {
            Graphics.Draw(_tray, 0, 18 * _tray.Scale.Y + 16 * _tray.Scale.Y * j, new RectangleF(0, 18, 18, 16));
            Graphics.Draw(_tray, 18 * _tray.Scale.X + numSectionsHor * (16 * _tray.Scale.X), 18 * _tray.Scale.Y + 16 * _tray.Scale.Y * j, new RectangleF(_tray.width - 18, 18, 18, 16));
        }

        Graphics.DrawRect(Vector2.Zero, new Vector2(18 * _tray.Scale.X + numSectionsHor * (16 * _tray.Scale.X) + _tray.Scale.Y * 4, (numSectionsVert + 2) * (16 * _tray.Scale.Y)), Color.Black * 0.8f * _core.alpha, 0.7f);

        _core.fancyFont.Scale = new Vector2(_tray.Scale.X / 2);
        _core.fancyFont.Depth = 0.98f;
        _core.fancyFont.Alpha = _core.alpha;

        var height = (numSectionsVert + 1) * 16 * _tray.Scale.Y + 5 * _tray.Scale.Y;
        var width = (numSectionsHor + 2) * (16 * _tray.Scale.X);
        var ver = DG.version;

        _core.fancyFont.Draw(ver, new Vector2(82 * _tray.Scale.X + (numSectionsHor - 6) * (16 * _tray.Scale.X), height + 7 * _tray.Scale.Y), new Color(62, 114, 122), 0.98f);
        _core.cursorPosition = Math.Min(Math.Max(_core.cursorPosition, 0), _core.typing.Length);

        if (_raster != null)
        {
            _raster.Scale = new Vector2(0.5f);
            _raster.Alpha = _core.alpha;
            _raster.Draw(_core.typing, 4 * _tray.Scale.X, height + _tray.Scale.Y * 8 - _raster.characterHeight * _raster.Scale.Y / 2, Color.White, 0.9f);
            Vector2 vec = new(_raster.GetWidth(_core.typing[.._core.cursorPosition]) + 4 * _tray.Scale.X + 1, height + 6 * _tray.Scale.Y);
            Graphics.DrawLine(vec, vec + new Vector2(0, 4 * _tray.Scale.X), Color.White, 1, 1);
        }
        else
        {
            _core.font.Scale = new Vector2(_tray.Scale.X / 2);
            _core.font.Alpha = _core.alpha;
            _core.font.Draw(_core.typing, 4 * _tray.Scale.X, height + 6 * _tray.Scale.Y, Color.White, 0.9f);
            Vector2 vec2 = new(_core.font.GetWidth(_core.typing[.._core.cursorPosition]) + 4 * _tray.Scale.X, height + 6 * _tray.Scale.Y);
            Graphics.DrawLine(vec2, vec2 + new Vector2(0, 4 * _tray.Scale.X), Color.White, 2, 1);
        }

        var index = _core.lines.Count - 1 - _core.viewOffset;
        var vOffset = 0f;
        _core.font.Scale = new Vector2(float.Max(float.Round(_tray.Scale.X / 4), 1));
        var mul = _core.font.Scale.X / 2;
        var lineHeight = 18 * mul;
        var numWidth = 20 * (_core.font.Scale.X * 2);

        if (_raster != null)
        {
            lineHeight = (_raster.characterHeight - 2) * _raster.Scale.Y;
            vOffset = lineHeight;
            numWidth = _raster.GetWidth("0000  ");
        }

        for (int k = 0; k < (height - 2 * _tray.Scale.Y) / lineHeight - 1; k++)
        {
            if (index < 0)
                break;

            var line = _core.lines.ElementAt(index);
            var lineNumber = index.ToString();

            while (lineNumber.Length < 4)
                lineNumber = "0" + lineNumber;

            if (_raster != null)
            {
                _raster.maxWidth = (int)(width - 35 * _tray.Scale.X);
                _raster.singleLine = true;
                _raster.enforceWidthByWord = false;
                _raster.Draw(lineNumber, 4 * _tray.Scale.X, height - vOffset + 2, (index % 2 > 0) ? (Color.Gray * 0.4f) : (Color.Gray * 0.6f), 0.9f);
                _raster.Draw(line.SectionString() + line.line, 4 * _tray.Scale.X + numWidth, height - vOffset + 2, line.color, 0.9f);
                vOffset += lineHeight;
            }
            else
            {
                _core.font.maxWidth = (int)(width - 35 * _tray.Scale.X);
                _core.font.singleLine = true;
                _core.font.enforceWidthByWord = false;
                _core.font.Draw(lineNumber, 4 * _tray.Scale.X, height - 18 * mul - vOffset + 2, (index % 2 > 0) ? (Color.Gray * 0.4f) : (Color.Gray * 0.6f), 0.9f);
                _core.font.Draw(line.SectionString() + line.line, 4 * _tray.Scale.X + numWidth, height - 18 * mul - vOffset + 2, line.color * 0.8f, 0.9f);
                vOffset += 18 * mul;
            }
            index--;
        }
        _core.font.Scale = new Vector2(2);
    }

    public static Profile ProfileByName(string findName)
    {
        foreach (var p in Profiles.all)
        {
            if (p.team != null)
            {
                var name = p.name.ToLower();
                if (findName == "player1" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer1))
                    name = findName;
                else if (findName == "player2" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer2))
                    name = findName;
                else if (findName == "player3" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer3))
                    name = findName;
                else if (findName == "player4" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer4))
                    name = findName;
                else if (findName == "player5" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer5))
                    name = findName;
                else if (findName == "player6" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer6))
                    name = findName;
                else if (findName == "player7" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer7))
                    name = findName;
                else if (findName == "player8" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer8))
                    name = findName;

                if (name == findName)
                    return p;
            }
        }
        return null;
    }

    public static void AddCommand(CMD pCommand)
    {
        GetCommands(pCommand.keyword).Add(pCommand);
        if (pCommand.aliases == null)
            return;

        foreach (string alias in pCommand.aliases)
            GetCommands(alias).Add(pCommand);
    }

    public static List<CMD> GetCommands(string pKeyword)
    {
        if (!commands.TryGetValue(pKeyword, out var cmds))
            cmds = commands[pKeyword] = [];
        return cmds;
    }

    public static void RunCommand(string command)
    {
        if (DG.buildExpired)
            return;

        _core.logScores = -1;
        if (!(command != ""))
            return;

        var culture = CultureInfo.CurrentCulture;
        var isCommand = false;
        ConsoleCommand c = new(command);
        var commandName = c.NextWord();
        _core.lines.Enqueue(new DCLine
        {
            line = command,
            color = Color.White
        });
        string message = null;
        var lastMessagePriority = int.MinValue;
        var lastMessageCommandName = "";

        foreach (var command2 in GetCommands(commandName))
        {
            CMD cmd = command2;
            isCommand = true;
            ConsoleCommand c2 = new(c.Remainder());

            while (cmd.subcommand != null && c2.NextWord(toLower: true, peek: true) == cmd.subcommand.keyword)
            {
                c2.NextWord();
                cmd = cmd.subcommand;
            }

            if (/*cmd.cheat*/false && !NetworkDebugger.enabled)
            {
                var overrideCheats = false;
#if FACEPUNCH
                if (SteamClient.SteamId != 0
                    && (SteamClient.SteamId == 76561197996786074L
                    || SteamClient.SteamId == 76561198885030822L
                    || SteamClient.SteamId == 76561198416200652L
                    || SteamClient.SteamId == 76561198104352795L
                    || SteamClient.SteamId == 76561198114791325L)
                    )
#else
                if (DGSteam.User != null && (DGSteam.User.Id == 76561197996786074L || DGSteam.User.Id == 76561198885030822L || DGSteam.User.Id == 76561198416200652L || DGSteam.User.Id == 76561198104352795L || DGSteam.User.Id == 76561198114791325L))
#endif
                    overrideCheats = true;
                if (!overrideCheats && (Network.isActive || Level.current is ChallengeLevel || Level.current is ArcadeLevel))
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "You can't do that here!",
                        color = Color.Red
                    });
                    return;
                }
            }

            if (cmd.Run(c2.Remainder()))
            {
                lastCommand = cmd;
                message = cmd.logMessage;

                if (cmd.commandQueueWaitFunction != null && _pendingCommandQueue.Count > 0)
                    _pendingCommandQueue.Peek().waitCommand = cmd.commandQueueWaitFunction;

                if (cmd.commandQueueWait > 0 && _pendingCommandQueue.Count > 0)
                    _pendingCommandQueue.Peek().wait = cmd.commandQueueWait;

                break;
            }

            if (cmd.priority >= lastMessagePriority && (lastMessageCommandName == "" || cmd.fullCommandName.Length >= lastMessageCommandName.Length))
            {
                lastCommand = null;
                message = cmd.logMessage;
                lastMessagePriority = cmd.priority;
                lastMessageCommandName = cmd.fullCommandName;
            }
        }

        if (message != null)
        {
            var array = message.Split('\n');
            foreach (var str in array)
            {
                _core.lines.Enqueue(new DCLine
                {
                    line = str,
                    color = Color.White
                });
            }
            return;
        }

        if (!isCommand)
        {
            lastCommand = null;
            if (commandName == "spawn")
            {
                if (CheckCheats())
                    return;

                isCommand = true;
                var spawnItem = c.NextWord();
                float xpos = 0,
                      ypos = 0;
                try
                {
                    xpos = Change.ToSingle(c.NextWord());
                    ypos = Change.ToSingle(c.NextWord());
                }
                catch
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "Parameters in wrong format.",
                        color = Color.Red
                    });
                    return;
                }

                if (c.NextWord() != "")
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "Too many parameters!",
                        color = Color.Red
                    });

                    return;
                }

                Type t = null;
                foreach (var tp in Editor.ThingTypes)
                {
                    // probably tp.Name.Equals(spawnItem, StringComparison.CurrentCultureIgnoreCase) is better
                    if (tp.Name.ToLower(culture) == spawnItem)
                    {
                        t = tp;
                        break;
                    }
                }

                if (t == null)
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = $"The type {spawnItem} does not exist!",
                        color = Color.Red
                    });
                    return;
                }

                if (!Editor.HasConstructorParameter(t))
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = $"{spawnItem} can not be spawned this way.",
                        color = Color.Red
                    });
                    return;
                }

                if (Editor.CreateThing(t) is PhysicsObject newThing)
                {
                    newThing.X = xpos;
                    newThing.Y = ypos;
                    Level.Add(newThing);
                    SFX.Play("hitBox");
                }
            }

            if (commandName == "netdebug")
            {
                if (!CheckCheats())
                {
                    _enableNetworkDebugging = !_enableNetworkDebugging;
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "Network Debugging Enabled",
                        color = Color.Green
                    });
                }
                return;
            }

            if (commandName == "close")
                _core.open = !_core.open;

            if (commandName == "console")
            {
                isCommand = true;
                switch (c.NextWord().ToLower(culture))
                {
                    case "":
                        _core.lines.Enqueue(new DCLine
                        {
                            line = "Parameters in wrong format.",
                            color = Color.Red
                        });
                        return;
                    case "width":
                        {
                            var val = c.NextWord().ToLower(culture);
                            if (val == "")
                            {
                                _core.lines.Enqueue(new DCLine
                                {
                                    line = "You must provide a value.",
                                    color = Color.Red
                                });
                                return;
                            }

                            if (c.NextWord() != "")
                            {
                                _core.lines.Enqueue(new DCLine
                                {
                                    line = "Too many parameters!",
                                    color = Color.Red
                                });
                                return;
                            }

                            try
                            {
                                int wide = Convert.ToInt32(val);
                                Options.Data.consoleWidth = Math.Min(Math.Max(wide, 25), 100);
                                Options.Save();
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    float wide2 = Convert.ToSingle(val);
                                    Options.Data.consoleWidth = (int)Math.Min(Math.Max(wide2, 0.25f), 1f) * 100;
                                    Options.Save();
                                }
                                catch (Exception)
                                {
                                }
                            }
                            break;
                        }
                    case "height":
                        {
                            var val = c.NextWord().ToLower(culture);
                            if (val == "")
                            {
                                _core.lines.Enqueue(new DCLine
                                {
                                    line = "You must provide a value.",
                                    color = Color.Red
                                });
                                return;
                            }

                            if (c.NextWord() != "")
                            {
                                _core.lines.Enqueue(new DCLine
                                {
                                    line = "Too many parameters!",
                                    color = Color.Red
                                });
                                return;
                            }

                            try
                            {
                                int high = Convert.ToInt32(val);
                                Options.Data.consoleHeight = Math.Min(Math.Max(high, 25), 100);
                                Options.Save();
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    float high2 = Convert.ToSingle(val);
                                    Options.Data.consoleHeight = (int)Math.Min(Math.Max(high2, 0.25f), 1) * 100;
                                    Options.Save();
                                }
                                catch (Exception)
                                {
                                }
                            }
                            break;
                        }
                    case "scale":
                        {
                            var val = c.NextWord().ToLower(culture);
                            if (val == "")
                            {
                                _core.lines.Enqueue(new DCLine
                                {
                                    line = "You must provide a value.",
                                    color = Color.Red
                                });
                                return;
                            }

                            if (c.NextWord() != "")
                            {
                                _core.lines.Enqueue(new DCLine
                                {
                                    line = "Too many parameters!",
                                    color = Color.Red
                                });
                                return;
                            }

                            try
                            {
                                int scale = Convert.ToInt32(val);
                                Options.Data.consoleScale = Math.Min(Math.Max(scale, 1), 5);
                                Options.Save();
                            }
                            catch (Exception)
                            {
                            }
                            break;
                        }
                    case "font":
                        {
                            var val = c.NextWord();
                            if (val == "")
                            {
                                _core.lines.Enqueue(new DCLine
                                {
                                    line = "You must provide a value.",
                                    color = Color.Red
                                });
                                return;
                            }

                            try
                            {
                                if (val == "size")
                                {
                                    val = c.NextWord().ToLower(culture);
                                    if (val == "")
                                    {
                                        _core.lines.Enqueue(new DCLine
                                        {
                                            line = "You must provide a size value.",
                                            color = Color.Red
                                        });
                                        return;
                                    }

                                    if (c.NextWord() != "")
                                    {
                                        _core.lines.Enqueue(new DCLine
                                        {
                                            line = "Too many parameters!",
                                            color = Color.Red
                                        });
                                        return;
                                    }

                                    try
                                    {
                                        var pts = Convert.ToInt32(val);
                                        _raster = new RasterFont(fontName, pts);
                                        Options.Data.consoleFontSize = pts;
                                        _raster.Scale = new Vector2(0.5f);
                                        Options.Save();
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    break;
                                }
                                if (c.Remainder().Length > 0)
                                {
                                    val = $"{val} {c.Remainder()}";
                                }

                                switch (val)
                                {
                                    case "clear":
                                    case "default":
                                    case "none":
                                        Options.Data.consoleFont = "";
                                        Options.Save();
                                        Log(DCSection.General, "|DGGREEN|Console font reset.");
                                        goto end_IL_0903;
                                    case "comic sans":
                                        val = "comic sans ms";
                                        break;
                                }

                                if (RasterFont.GetName(val) != null)
                                {
                                    _raster = new RasterFont(val, fontPoints);
                                    Options.Data.consoleFont = val;
                                    _raster.Scale = new Vector2(0.5f);
                                    Options.Save();
                                    if (_raster.data.name == "Comic Sans MS")
                                    {
                                        Log(DCSection.General, $"|DGGREEN|Font is now {_raster.data.name}! What a laugh!");
                                    }
                                    else
                                    {
                                        Log(DCSection.General, $"|DGGREEN|Font is now {_raster.data.name}!");
                                    }
                                }
                                else
                                {
                                    Log(DCSection.General, $"|DGRED|Could not find font ({val})!");
                                }

                            end_IL_0903:;
                            }
                            catch (Exception)
                            {
                            }
                            break;
                        }
                }
            }

            if (NetworkDebugger.enabled && commandName == "record")
            {
                isCommand = true;
                var level = c.NextWord();

                if (level.Length < 3)
                {
                    try
                    {
                        NetworkDebugger.StartRecording(Convert.ToInt32(level));
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    NetworkDebugger.StartRecording(level);
                }
            }

            if (commandName == "team")
            {
                if (CheckCheats())
                    return;

                isCommand = true;
                string who = c.NextWord();
                var p = ProfileByName(who);
                if (p == null)
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = $"No profile named {who}.",
                        color = Color.Red
                    });
                    return;
                }

                var team = c.NextWord();
                if (team == "")
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "Parameters in wrong format.",
                        color = Color.Red
                    });
                    return;
                }

                if (c.NextWord() != "")
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "Too many parameters!",
                        color = Color.Red
                    });
                    return;
                }

                team = team.ToLower();
                var found = false;
                foreach (Team t2 in Teams.all)
                {
                    if (t2.name.ToLower() == team)
                    {
                        found = true;
                        p.team = t2;
                        break;
                    }
                }

                if (!found)
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = $"No team named {team}.",
                        color = Color.Red
                    });
                    return;
                }
            }

            if (commandName == "call")
            {
                if (CheckCheats())
                {
                    return;
                }
                isCommand = true;
                string who2 = c.NextWord();
                bool found2 = false;
                foreach (Profile p2 in Profiles.all)
                {
                    if (p2.name.ToLower(culture) != who2)
                        continue;

                    if (p2.duck != null)
                    {
                        found2 = true;
                        var function = c.NextWord();
                        if (function == "")
                        {
                            _core.lines.Enqueue(new DCLine
                            {
                                line = "Parameters in wrong format.",
                                color = Color.Red
                            });
                            return;
                        }

                        if (c.NextWord() != "")
                        {
                            _core.lines.Enqueue(new DCLine
                            {
                                line = "Too many parameters!",
                                color = Color.Red
                            });
                            return;
                        }

                        var methods = typeof(Duck).GetMethods();
                        var foundMethod = false;
                        var array2 = methods;
                        foreach (MethodInfo method in array2)
                        {
                            if (method.Name.ToLower(culture) == function)
                            {
                                foundMethod = true;
                                if (method.GetParameters().Count() > 0)
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "You can only call functions with no parameters.",
                                        color = Color.Red
                                    });
                                    return;
                                }

                                try
                                {
                                    method.Invoke(p2.duck, null);
                                }
                                catch
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "The function threw an exception.",
                                        color = Color.Red
                                    });
                                    return;
                                }
                            }
                        }

                        if (!foundMethod)
                        {
                            _core.lines.Enqueue(new DCLine
                            {
                                line = "Duck has no function called " + function + ".",
                                color = Color.Red
                            });
                            return;
                        }
                        continue;
                    }
                    _core.lines.Enqueue(new DCLine
                    {
                        line = who2 + " is not in the game!",
                        color = Color.Red
                    });
                    return;
                }

                if (!found2)
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "No profile named " + who2 + ".",
                        color = Color.Red
                    });
                    return;
                }
            }

            if (commandName == "set")
            {
                if (CheckCheats())
                    return;

                isCommand = true;
                var who3 = c.NextWord();
                var found3 = false;
                foreach (var p3 in Profiles.all)
                {
                    if (p3.name.ToLower(culture) != who3)
                        continue;

                    if (p3.duck != null)
                    {
                        found3 = true;
                        var variable = c.NextWord();
                        if (variable == "")
                        {
                            _core.lines.Enqueue(new DCLine
                            {
                                line = "Parameters in wrong format.",
                                color = Color.Red
                            });
                            return;
                        }

                        var duckType = typeof(Duck);
                        var properties = duckType.GetProperties();
                        var foundProperty = false;
                        var array3 = properties;
                        foreach (var property in array3)
                        {
                            if (property.Name.ToLower(culture) != variable)
                                continue;

                            foundProperty = true;
                            if (property.PropertyType == typeof(float))
                            {
                                var val2 = 0F;
                                try
                                {
                                    val2 = Change.ToSingle(c.NextWord());
                                }
                                catch
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Parameters in wrong format.",
                                        color = Color.Red
                                    });
                                    return;
                                }

                                if (c.NextWord() != "")
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Too many parameters!",
                                        color = Color.Red
                                    });
                                    return;
                                }

                                property.SetValue(p3.duck, val2, null);
                            }

                            if (property.PropertyType == typeof(bool))
                            {
                                var val3 = false;

                                try
                                {
                                    val3 = Convert.ToBoolean(c.NextWord());
                                }
                                catch
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Parameters in wrong format.",
                                        color = Color.Red
                                    });
                                    return;
                                }

                                if (c.NextWord() != "")
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Too many parameters!",
                                        color = Color.Red
                                    });
                                    return;
                                }
                                property.SetValue(p3.duck, val3, null);
                            }

                            if (property.PropertyType == typeof(int))
                            {
                                var val4 = 0;

                                try
                                {
                                    val4 = Convert.ToInt32(c.NextWord());
                                }
                                catch
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Parameters in wrong format.",
                                        color = Color.Red
                                    });
                                    return;
                                }

                                if (c.NextWord() != "")
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Too many parameters!",
                                        color = Color.Red
                                    });
                                    return;
                                }
                                property.SetValue(p3.duck, val4, null);
                            }

                            if (property.PropertyType == typeof(Vector2))
                            {
                                float xval = 0,
                                      yval = 0;
                                try
                                {
                                    xval = Change.ToSingle(c.NextWord());
                                    yval = Change.ToSingle(c.NextWord());
                                }
                                catch
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Parameters in wrong format.",
                                        color = Color.Red
                                    });
                                    return;

                                }
                                if (c.NextWord() != "")
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Too many parameters!",
                                        color = Color.Red
                                    });
                                    return;
                                }
                                property.SetValue(p3.duck, new Vector2(xval, yval), null);
                            }
                        }

                        if (foundProperty)
                            continue;

                        var fields = duckType.GetFields();
                        foreach (var field in fields)
                        {
                            if (field.Name.ToLower(culture) != variable)
                                continue;

                            foundProperty = true;
                            if (field.FieldType == typeof(float))
                            {
                                var val5 = 0F;

                                try
                                {
                                    val5 = Change.ToSingle(c.NextWord());
                                }
                                catch
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Parameters in wrong format.",
                                        color = Color.Red
                                    });
                                    return;
                                }

                                if (c.NextWord() != "")
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Too many parameters!",
                                        color = Color.Red
                                    });
                                    return;
                                }
                                field.SetValue(p3.duck, val5);
                            }

                            if (field.FieldType == typeof(bool))
                            {
                                var val6 = false;
                                try
                                {
                                    val6 = Convert.ToBoolean(c.NextWord());
                                }
                                catch
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Parameters in wrong format.",
                                        color = Color.Red
                                    });
                                    return;
                                }

                                if (c.NextWord() != "")
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Too many parameters!",
                                        color = Color.Red
                                    });
                                    return;
                                }
                                field.SetValue(p3.duck, val6);
                            }

                            if (field.FieldType == typeof(int))
                            {
                                var val7 = 0;
                                try
                                {
                                    val7 = Convert.ToInt32(c.NextWord());
                                }
                                catch
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Parameters in wrong format.",
                                        color = Color.Red
                                    });
                                    return;
                                }

                                if (c.NextWord() != "")
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Too many parameters!",
                                        color = Color.Red
                                    });
                                    return;
                                }
                                field.SetValue(p3.duck, val7);
                            }

                            if (field.FieldType == typeof(Vector2))
                            {
                                float xval2 = 0,
                                      yval2 = 0;
                                try
                                {
                                    xval2 = Change.ToSingle(c.NextWord());
                                    yval2 = Change.ToSingle(c.NextWord());
                                }
                                catch
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Parameters in wrong format.",
                                        color = Color.Red
                                    });
                                    return;
                                }

                                if (c.NextWord() != "")
                                {
                                    _core.lines.Enqueue(new DCLine
                                    {
                                        line = "Too many parameters!",
                                        color = Color.Red
                                    });
                                    return;
                                }
                                field.SetValue(p3.duck, new Vector2(xval2, yval2));
                            }
                        }

                        if (!foundProperty)
                        {
                            _core.lines.Enqueue(new DCLine
                            {
                                line = $"Duck has no variable called {variable}.",
                                color = Color.Red
                            });
                            return;
                        }
                        continue;
                    }

                    _core.lines.Enqueue(new DCLine
                    {
                        line = $"{who3} is not in the game!",
                        color = Color.Red
                    });
                    return;
                }

                if (!found3)
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = $"No profile named {who3}.",
                        color = Color.Red
                    });
                    return;
                }
            }

            if (commandName == "globalscores")
            {
                if (CheckCheats())
                    return;

                isCommand = true;
                using var enumerator5 = Profiles.active.GetEnumerator();
                if (enumerator5.MoveNext())
                {
                    Profile p4 = enumerator5.Current;
                    _core.lines.Enqueue(new DCLine
                    {
                        line = p4.name + ": " + p4.stats.CalculateProfileScore().ToString("0.000"),
                        color = Color.Red
                    });
                }
            }

            if (commandName == "scorelog")
            {
                if (CheckCheats())
                    return;

                isCommand = true;
                var who4 = c.NextWord();
                if (c.NextWord() != "")
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "Too many parameters!",
                        color = Color.Red
                    });
                    return;
                }

                if (who4 == "")
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "You need to provide a player number.",
                        color = Color.Red
                    });
                    return;
                }

                var num = 0;
                try
                {
                    num = Convert.ToInt32(who4);
                }
                catch
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "Parameters in wrong format.",
                        color = Color.Red
                    });
                    return;
                }
                _core.logScores = num;
            }
        }

        if (!isCommand)
        {
            _core.lines.Enqueue(new DCLine
            {
                line = $"{commandName} is not a valid command!",
                color = Color.Red
            });
        }
    }

    public static void LogComplexMessage(string text, Color c, float scale = 2, int index = -1)
    {
        if (text.Contains('\n'))
        {
            var array = text.Split('\n');
            for (int i = 0; i < array.Length; i++)
                Log(array[i], c, scale, index);
            return;
        }

        DCLine line = new()
        {
            line = text,
            color = c,
            threadIndex = index < 0 ? NetworkDebugger.currentIndex : index,
            timestamp = DateTime.Now
        };

        if (NetworkDebugger.enabled)
        {
            lock (debuggerLines)
            {
                debuggerLines.Add(line);
                return;
            }
        }

        lock (_core.pendingLines)
        {
            _core.pendingLines.Add(line);
        }
    }

    public static void Log(string text)
    {
        Log(DCSection.General, text);
    }

    public static void Log(string text, Color c, float scale = 2, int index = -1)
    {
        DCLine line = new()
        {
            line = text,
            color = c,
            threadIndex = index < 0 ? NetworkDebugger.currentIndex : index,
            timestamp = DateTime.Now
        };

        if (NetworkDebugger.enabled)
        {
            lock (debuggerLines)
            {
                debuggerLines.Add(line);
                return;
            }
        }

        lock (_core.pendingLines)
        {
            _core.pendingLines.Add(line);
        }
    }

    public static void RefreshConsoleFont()
    {
        _raster = null;
        InitializeFont();
    }

    public static void LogEvent(string pDescription, NetworkConnection pConnection)
    {
        pDescription ??= "No Description.";
        Log($"@LOGEVENT@|AQUA|LOGEVENT!-----------{pConnection}|AQUA|signalled a log event!-----------!LOGEVENT", Color.White);
        Log($"@LOGEVENT@|AQUA|LOGEVENT!---{pDescription}|AQUA|---!LOGEVENT", Color.White);

        if (Network.isActive && pConnection == DuckNetwork.localConnection)
            Send.Message(new NMLogEvent(pDescription));
    }

    public static void Log(DCSection section, string text, int netIndex = -1)
    {
        Log(section, Verbosity.Normal, text, netIndex);
    }

    public static void Log(DCSection section, string text, NetworkConnection context, int netIndex = -1)
    {
        if (context != null)
            text += context.ToString();

        Log(section, Verbosity.Normal, text, netIndex);
    }

    public static void Log(DCSection section, Verbosity verbose, string text, int netIndex = -1)
    {
        Console.WriteLine(Program.RemoveColorTags(text));

        DCLine line = new()
        {
            line = text,
            section = section,
            verbosity = verbose,
            color = Color.White,
            threadIndex = netIndex < 0 ? NetworkDebugger.currentIndex : netIndex,
            timestamp = DateTime.Now
        };

        if (NetworkDebugger.enabled)
        {
            lock (debuggerLines)
            {
                debuggerLines.Add(line);
                return;
            }
        }

        lock (_core.pendingLines)
        {
            _core.pendingLines.Add(line);
        }
    }

    public static void SaveNetLog(string pName = null)
    {
        FlushPendingLines();
        var currentPart = "";
        for (int i = Math.Max(core.lines.Count - 1500, 0); i < core.lines.Count; i++)
            currentPart += core.lines.ElementAt(i).ToSendString();

        if (pName == null)
#if FACEPUNCH
            pName = $"{DateTime.Now.ToShortDateString().Replace('/', '_')}_{DateTime.Now.ToLongTimeString().Replace(':', '_')}_{FacepunchSteam.Me.Name}_netlog.txt";
#else
            pName = $"{DateTime.Now.ToShortDateString().Replace('/', '_')}_{DateTime.Now.ToLongTimeString().Replace(':', '_')}_{DGSteam.User.Name}_netlog.txt";
#endif
        else if (!pName.EndsWith(".txt"))
            pName += ".txt";

        var output = DuckFile.FixInvalidPath($"{DuckFile.logDirectory}{pName}");
        if (File.Exists(output))
            File.Delete(output);

        File.WriteAllText(output, currentPart);
    }

    public static void LogTransferComplete(NetworkConnection pConnection)
    {
        var data = core.GetReceivedLogData(pConnection);
        if (data != null)
        {
            var output = DuckFile.FixInvalidPath($"{DuckFile.logDirectory}{DateTime.Now.ToShortDateString().Replace('/', '_')}_{DateTime.Now.ToLongTimeString().Replace(':', '_')}_{pConnection.name}_netlog.rtf");
            DuckFile.CreatePath(output);
            File.WriteAllText(output, data);
        }
        core.requestingLogs.Remove(pConnection);
        core.receivingLogs.Remove(pConnection);
        pConnection.logTransferProgress = 0;
        pConnection.logTransferSize = 0;
    }

    public static void LogSendingComplete(NetworkConnection pConnection)
    {
        core.transferRequestsPending.Remove(pConnection);
        DuckNetwork.core.logTransferProgress = 0;
        DuckNetwork.core.logTransferSize = 0;
    }

    public static void Chart(string chart, string section, double x, double y, Color c)
    {
        lock (_core.pendingChartValues)
        {
            _core.pendingChartValues.Add(new DCChartValue
            {
                chart = chart,
                section = section,
                x = x,
                y = y,
                color = c,
                threadIndex = NetworkDebugger.currentIndex
            });
        }
    }

    public static void UpdateGraph(int index, NetGraph target)
    {
    }

    public static void InitializeCommands()
    {
        AddCommand(new("vsync", () =>
        {
            Graphics._manager.SynchronizeWithVerticalRetrace = !Graphics._manager.SynchronizeWithVerticalRetrace;
            Graphics._manager.ApplyChanges();

            if (Graphics._manager.SynchronizeWithVerticalRetrace)
                Log("|DGGREEN|vsync enabled");
            else
                Log("|DGRED|vsync disabled");
        }));

        AddCommand(new CMD("level",
        [
            new CMD.Level("level")
        ], delegate (CMD cmd)
        {
            Level.current = cmd.Arg<Level>("level");
        })
        {
            cheat = true,
            aliases = ["lev"],
            commandQueueWaitFunction = () => Level.core.nextLevel == null
        });

        AddCommand(new CMD("give",
        [
            new CMD.Thing<Duck>("player"),
            new CMD.Thing<Holdable>("object"),
            new CMD.String("specialCode", pOptional: true)
        ], delegate (CMD cmd)
        {
            var duck = cmd.Arg<Duck>("player");
            var holdable = cmd.Arg<Holdable>("object");
            var text = cmd.Arg<string>("specialCode");
            Level.Add(holdable);

            if (text == "i" && holdable is Gun)
                (holdable as Gun).infinite.value = true;

            switch (text)
            {
                case "h":
                case "hp":
                case "ph":
                    {
                        if (duck.GetEquipment(typeof(Holster)) is not Holster holster)
                        {
                            holster = (text != "hp" && text != "ph") ? new Holster(0, 0) : new PowerHolster(0, 0);
                            Level.Add(holster);
                            duck.Equip(holster);
                        }

                        holster.SetContainedObject(holdable);
                        break;
                    }
                case "e":
                    if (holdable is Equipment)
                    {
                        duck.Equip(holdable as Equipment);
                        break;
                    }
                    goto default;
                default:
                    duck.GiveHoldable(holdable);
                    break;
            }
            SFX.Play("hitBox");
        })
        {
            description = "Gives a player an item by name.",
            cheat = true,
            priority = 1
        });

        AddCommand(new CMD("give",
        [
            new CMD.Thing<Duck>("duckName"),
            new CMD.Thing<TeamHat>("hat")
        ], delegate (CMD cmd)
        {
            var duck = cmd.Arg<Duck>("duckName");
            var teamHat = cmd.Arg<TeamHat>("hat");
            Level.Add(teamHat);
            duck.GiveHoldable(teamHat);
            SFX.Play("hitBox");
        })
        {
            cheat = true
        });

        AddCommand(new CMD("kill",
        [
            new CMD.Thing<Duck>("duckName")
        ], delegate (CMD cmd)
        {
            cmd.Arg<Duck>("duckName").Kill(new DTIncinerate(null));
        })
        {
            description = "",
            cheat = true
        });

        AddCommand(new CMD("modhash", (Action)delegate
        {
            _core.lines.Enqueue(new DCLine
            {
                line = ModLoader._modString,
                color = Color.Red
            });

            _core.lines.Enqueue(new DCLine
            {
                line = ModLoader.modHash,
                color = Color.Red
            });
        }));

        AddCommand(new CMD("wagnus", (Action)delegate
        {
            wagnusDebug = !wagnusDebug;
        })
        {
            description = "Toggles guides in editor for Wagnus teleport ranges."
        });

        AddCommand(new CMD("steamid", (Action)delegate
        {
            _core.lines.Enqueue(new DCLine
            {
                line = $"Your steam ID is: {Profiles.experienceProfile.steamID}",
                color = Colors.DGBlue
            });
        }));

        AddCommand(new CMD("localid", (Action)delegate
        {
            _core.lines.Enqueue(new DCLine
            {
                line = $"Your local ID is: {DG.localID}",
                color = Colors.DGBlue
            });
        }));

        AddCommand(new CMD("showcollision", (Action)delegate
        {
            _core.showCollision = !_core.showCollision;
        })
        {
            cheat = true
        });

        AddCommand(new CMD("showorigin", (Action)delegate
        {
            debugOrigin = !debugOrigin;
        })
        {
            hidden = true
        });

        AddCommand(new CMD("showbounds", (Action)delegate
        {
            debugBounds = !debugBounds;
        })
        {
            hidden = true
        });

        AddCommand(new CMD("fps", (Action)delegate
        {
            showFPS = !showFPS;
        })
        {
            cheat = false
        });

        AddCommand(new CMD("randomedit", (Action)delegate
        {
            Editor.miniMode = !Editor.miniMode;
        })
        {
            cheat = false
        });

        AddCommand(new CMD("mem", (Action)delegate
        {
            long num = GC.GetTotalMemory(forceFullCollection: true) / 1000;
            _core.lines.Enqueue(new DCLine
            {
                line = $"GC Has {num} KB Allocated ({num / 1000} MB)",
                color = Color.White
            });
        }));

        AddCommand(new CMD("log",
        [
            new CMD.String("description")
            {
                takesMultispaceString = true
            }
        ], delegate (CMD cmd)
        {
            LogEvent(cmd.Arg<string>("description"), DuckNetwork.localConnection);
        })
        {
            hidden = true
        });

        AddCommand(new CMD("requestlogs", (Action)delegate
        {
            Send.Message(new NMRequestLogs());
            foreach (NetworkConnection current in Network.connections)
            {
                core.requestingLogs.Add(current);
            }
            SaveNetLog();
        })
        {
            hidden = true
        });

        AddCommand(new CMD("accept",
        [
            new CMD.Integer("number")
        ], delegate (CMD cmd)
        {
            try
            {
                var index = cmd.Arg<int>("number");
                var profile = DuckNetwork.profiles[index];
                if (core.transferRequestsPending.Contains(profile.connection))
                {
                    core.transferRequestsPending.Remove(profile.connection);
                    SendNetLog(profile.connection);
                }
            }
            catch
            {
            }
        })
        {
            hidden = true
        });

        AddCommand(new CMD("eight", (Action)delegate
        {
            var num = 0;
            foreach (Profile defaultProfile in Profiles.defaultProfiles)
            {
                defaultProfile.team = null;
                defaultProfile.team = Teams.all[num];
                num++;
            }
        })
        {
            cheat = true
        });

        AddCommand(new CMD("chat", new CMD("font",
        [
            new CMD.Font("font", () => Options.Data.chatFontSize)
        ], delegate (CMD cmd)
        {
            var chatFont = cmd.Arg<string>("font");
            Options.Data.chatFont = chatFont;
            Options.Save();
            DuckNetwork.UpdateFont();
        })));

        AddCommand(new CMD("chat", new CMD("font", new CMD("size",
        [
            new CMD.Integer("size")
        ], delegate (CMD cmd)
        {
            var chatFontSize = cmd.Arg<int>("size");
            Options.Data.chatFontSize = chatFontSize;
            Options.Save();
            DuckNetwork.UpdateFont();
        }))));

        AddCommand(new CMD("fancymode", (Action)delegate
        {
            fancyMode = !fancyMode;
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("shieldmode", (Action)delegate
        {
            shieldMode = !shieldMode;
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("qwopmode", (Action)delegate
        {
            _core.qwopMode = !_core.qwopMode;
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("splitscreen", (Action)delegate
        {
            _core.splitScreen = !_core.splitScreen;
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("rhythmmode", (Action)delegate
        {
            if (!_core.rhythmMode)
                Music.Stop();

            _core.rhythmMode = !_core.rhythmMode;
            if (_core.rhythmMode)
                Music.Play(Music.RandomTrack("InGame"));
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("toggle",
        [
            new CMD.Layer("layer")
        ], delegate (CMD cmd)
        {
            cmd.Arg<Layer>("layer").visible = !cmd.Arg<Layer>("layer").visible;
        })
        {
            description = "Toggles whether or not a layer is visible. Some options include 'game', 'background', 'blocks' and 'parallax'.",
            cheat = true
        });

        AddCommand(new CMD("clearmainprofile", (Action)delegate
        {
            _core.lines.Enqueue(new DCLine
            {
                line = "Your main account has been R U I N E D !",
                color = Color.Red
            });
            Profile p = new(Profiles.experienceProfile.steamID.ToString(), null, null, null, network: false, Profiles.experienceProfile.steamID.ToString())
            {
                steamID = Profiles.experienceProfile.steamID
            };
            Profiles.Remove(Profiles.experienceProfile);
            Profiles.Add(p);
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("xpskip", (Action)delegate
        {
            if (Profiles.experienceProfile.GetNumFurnitures(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0)
            {
                _core.lines.Enqueue(new DCLine
                {
                    line = "Limit one Voodoo Vincent per customer, sorry!",
                    color = Color.Red
                });
            }
            else if (MonoMain.pauseMenu != null)
            {
                _core.lines.Enqueue(new DCLine
                {
                    line = "Please close any open menus.",
                    color = Color.Red
                });
            }
            else
            {
                HUD.CloseAllCorners();
                (MonoMain.pauseMenu = new UIPresentBox(RoomEditor.GetFurniture("VOODOO VINCENT"), Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 190)).Open();
                _core.open = !_core.open;
            }
        })
        {
            hidden = true,
            cheat = false
        });

        AddCommand(new CMD("johnnygrey", (Action)delegate
        {
            Global.data.typedJohnny = true;
            Global.Save();
            if (Unlockables.HasPendingUnlocks())
            {
                _core.open = false;
                MonoMain.pauseMenu = new UIUnlockBox([.. Unlockables.GetPendingUnlocks()], Layer.HUD.camera.width / 2, Layer.HUD.camera.height / 2, 190);
            }
        })
        {
            hidden = true
        });

        AddCommand(new CMD("constantsync", (Action)delegate
        {
            _core.constantSync = !_core.constantSync;
            _core.lines.Enqueue(new DCLine
            {
                line = $"Constant sync has been {(Options.Data.powerUser ? "enabled" : "disabled")}!",
                color = (core.constantSync ? Colors.DGGreen : Colors.DGRed)
            });
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("poweruser", (Action)delegate
        {
            Options.Data.powerUser = !Options.Data.powerUser;
            _core.lines.Enqueue(new DCLine
            {
                line = $"Power User mode has been {(Options.Data.powerUser ? "enabled" : "disabled")}!",
                color = (Options.Data.powerUser ? Colors.DGGreen : Colors.DGRed)
            });
            Editor.InitializePlaceableGroup();
            Main.editor.UpdateObjectMenu();
            Options.Save();
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("oldangles", (Action)delegate
        {
            Options.Data.oldAngleCode = !Options.Data.oldAngleCode;
            _core.lines.Enqueue(new DCLine
            {
                line = $"Oldschool Angles have been {(Options.Data.oldAngleCode ? "enabled" : "disabled")}!",
                color = Options.Data.oldAngleCode ? Colors.DGGreen : Colors.DGRed
            });
            Options.Save();

            if (Network.isActive && DuckNetwork.localProfile != null)
                Send.Message(new NMOldAngles(DuckNetwork.localProfile, Options.Data.oldAngleCode));
        })
        {
            hidden = true
        });

        AddCommand(new CMD("debugtypelist", (Action)delegate
        {
            foreach (var current in ModLoader._typesByName)
            {
                _core.lines.Enqueue(new DCLine
                {
                    line = current.Key,
                    color = Colors.DGPurple
                });
            }
        })
        {
            hidden = true
        });

        AddCommand(new CMD("debugtypelistraw", (Action)delegate
        {
            foreach (var current in ModLoader._typesByNameUnprocessed)
            {
                _core.lines.Enqueue(new DCLine
                {
                    line = current.Key,
                    color = Colors.DGPurple
                });
            }
        })
        {
            hidden = true
        });

        AddCommand(new CMD("sing",
        [
            new CMD.String("song")
        ], delegate (CMD cmd)
        {
            var text = cmd.Arg<string>("song");
            Music.Play(text);

            if (Network.isActive)
                Send.Message(new NMSwitchMusic(text));
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("downpour", (Action<CMD>)delegate
        {
            var num = Level.current.bottomRight.X - Level.current.topLeft.X + 128;
            var num2 = 10;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < num2; j++)
                {
                    var randomItem = ItemBoxRandom.GetRandomItem();
                    randomItem.Position = Level.current.topLeft + new Vector2(-64 + (num / num2 * j + Rando.Float(-128, 128)), Level.current.topLeft.Y - 2000 - (512 * i) + Rando.Float(-256, 256));
                    Level.Add(randomItem);
                }
            }
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("ruindatahash",
        [
            new CMD.String("workshopID", pOptional: true)
        ], delegate (CMD cmd)
        {
            var text = cmd.Arg<string>("workshopID");
            if (text == null)
            {
                Editor.thingTypesHash = (uint)Rando.Int(99999999);
                _core.lines.Enqueue(new DCLine
                {
                    line = "Ruined local datahash! Good luck playing online now!",
                    color = Colors.DGRed
                });
            }
            else
            {
                var flag = false;
                try
                {
                    Mod modFromWorkshopID = ModLoader.GetModFromWorkshopID(Convert.ToUInt64(text));
                    if (modFromWorkshopID != null)
                    {
                        modFromWorkshopID.System_RuinDatahash();
                        flag = true;
                        _core.lines.Enqueue(new DCLine
                        {
                            line = $"Ruined datahash for {modFromWorkshopID.configuration.displayName}! Good luck playing online now!",
                            color = Colors.DGRed
                        });
                    }
                }
                catch
                {
                }

                if (!flag)
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = $"Could not find mod with ID ({text})",
                        color = Colors.DGRed
                    });
                }
            }
        })
        {
            hidden = true,
            cheat = true
        });

#if FACEPUNCH
        if (!SteamClient.IsValid)
#else
        if (!DGSteam.IsInitialized())
#endif
            return;

        AddCommand(new CMD("zipcloud", (Action)delegate
        {
            string text = $"{DuckFile.saveDirectory}cloud_zip.zip";
            Cloud.ZipUpCloudData(text);
            _core.lines.Enqueue(new DCLine
            {
                line = $"Zipped up to: {text}",
                color = Colors.DGBlue
            });
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("clearsave", (Action)delegate
        {
            _core.lines.Enqueue(new DCLine
            {
                line = "ARE YOU SURE? ALL SAVE DATA WILL BE DELETED",
                color = Color.Red
            });
            _core.lines.Enqueue(new DCLine
            {
                line = "LOCALLY, AND FROM THE CLOUD.",
                color = Color.Red
            });
            _core.lines.Enqueue(new DCLine
            {
                line = "ENTER 'corptron' IF YOU WANT TO CONTINUE..",
                color = Color.Red
            });
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("copy", (Action)delegate
        {
            var currentPart = "";
            for (int i = Math.Max(core.lines.Count - 750, 0); i < core.lines.Count; i++)
                currentPart += core.lines.ElementAt(i).ToShortString();

            SDL.SDL_SetClipboardText(currentPart);
            _core.lines.Enqueue(new DCLine
            {
                line = "Log was copied to clipboard!",
                color = Color.White
            });
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("savedir", (Action)delegate
        {
            Process.Start(DuckFile.saveDirectory);
            _core.lines.Enqueue(new DCLine
            {
                line = "Save directory was opened.",
                color = Color.White
            });
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("userdir", (Action)delegate
        {
            Process.Start(DuckFile.userDirectory);
            _core.lines.Enqueue(new DCLine
            {
                line = "User directory was opened.",
                color = Color.White
            });
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("recover", (Action)delegate
        {
            _core.lines.Enqueue(new DCLine
            {
                line = "ARE YOU SURE? ALL NEW SAVE DATA",
                color = Color.Red
            });
            _core.lines.Enqueue(new DCLine
            {
                line = "WILL BE OVERWRITTEN BY PRE 1.5 DATA",
                color = Color.Red
            });
            _core.lines.Enqueue(new DCLine
            {
                line = "ENTER 'corptron' IF YOU WANT TO CONTINUE..",
                color = Color.Red
            });
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("managecloud", (Action)delegate
        {
            (MonoMain.pauseMenu = new UICloudManagement(null)).Open();
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("manageblocks", (Action)delegate
        {
            (MonoMain.pauseMenu = new UIBlockManagement(null)).Open();
        })
        {
            hidden = true,
            cheat = true
        });

        AddCommand(new CMD("corptron", (Action)delegate
        {
            if (lastCommand != null && lastCommand.keyword == "clearsave")
            {
                Cloud.DeleteAllCloudData(pNewDataOnly: false);
                DuckFile.DeleteAllSaveData();
                _core.lines.Enqueue(new DCLine
                {
                    line = "All save data has been deleted.",
                    color = Color.Red
                });
            }
            if (lastCommand != null && lastCommand.keyword == "recover")
            {
                DuckFile.DeleteFolder(DuckFile.userDirectory);
                while (Cloud.processing)
                {
                    Cloud.Update();
                }
                Program.crashed = true;
                Process.Start(Environment.ProcessPath, Program.commandLine + " -recoversave");
                MonoMain.instance.Exit();
            }
        })
        {
            hidden = true,
            cheat = true
        });
    }

    public static void FlushPendingLines()
    {
        lock (_core.pendingLines)
        {
            foreach (DCLine line in _core.pendingLines)
            {
                _core.lines.Enqueue(line);
                if (_core.viewOffset != 0)
                    _core.viewOffset++;
            }

            if (_core.lines.Count > 3000)
            {
                for (int i = 0; i < 500; i++)
                {
                    _core.lines.Dequeue();
                    if (_core.viewOffset > 0)
                        _core.viewOffset--;
                }
            }
            _core.pendingLines.Clear();
        }
    }

    public static void Update()
    {
        if (_core == null)
            return;

        FlushPendingLines();
        var shift = Keyboard.Down(Keys.LeftShift) || Keyboard.Down(Keys.RightShift);
        var num = Keyboard.Pressed(Keys.OemTilde) && !shift;
        if (core.pendingSends.Count > 0)
        {
            NetMessage netMessage = core.pendingSends.Dequeue();
            Send.Message(netMessage, netMessage.connection);
        }

        if (num && !DuckNetwork.core.enteringText && NetworkDebugger.hoveringInstance)
        {
            if (_tray == null)
            {
                _tray = new Sprite("devTray");
                _scan = new Sprite("devScan");
            }
            _core.open = !_core.open;
            Keyboard.keyString = "";
            _core.cursorPosition = _core.typing.Length;
            _core.lastCommandIndex = -1;
            _core.viewOffset = 0;
        }

        _core.alpha = Maths.LerpTowards(_core.alpha, _core.open ? 1f : 0, 0.1f);
        if (_pendingCommandQueue.Count > 0)
        {
            QueuedCommand c = _pendingCommandQueue.Peek();
            if (c.wait > 0)
                c.wait--;
            else if (c.waitCommand == null || c.waitCommand())
            {
                _pendingCommandQueue.Dequeue();
                if (c.command != null)
                    RunCommand(c.command);
            }
        }

        if (_core.open && NetworkDebugger.hoveringInstance)
        {
            Input._imeAllowed = true;
            if (_core.cursorPosition > _core.typing.Length)
                _core.cursorPosition = _core.typing.Length;

            if (!Keyboard.control)
                _core.typing = _core.typing.Insert(_core.cursorPosition, Keyboard.keyString);

            if (_core.typing != "" && _pendingCommandQueue.Count > 0)
            {
                _pendingCommandQueue.Clear();
                _core.lines.Enqueue(new DCLine
                {
                    line = "Pending commands cleared!",
                    color = Colors.DGOrange
                });
            }

            if (Keyboard.keyString.Length > 0)
            {
                _core.cursorPosition += Keyboard.keyString.Length;
                _core.lastCommandIndex = -1;
            }

            Keyboard.keyString = "";
            if (Keyboard.control)
            {
                if (Keyboard.Pressed(Keys.C))
                {
                    if (!string.IsNullOrWhiteSpace(_core.typing))
                    {
                        SDL.SDL_SetClipboardText(_core.typing);
                        HUD.AddPlayerChangeDisplay("@CLIPCOPY@Copied!");
                    }
                }
                else if (Keyboard.Pressed(Keys.V))
                {
                    var paste = "";
                    paste = SDL.SDL_GetClipboardText();
                    var array = paste.Replace('\r', '\n').Split('\n');
                    List<string> commands = [];
                    string[] array2 = array;
                    foreach (string line in array2)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            commands.Add(line);
                    }

                    if (commands.Count == 1)
                    {
                        paste = commands[0].Trim();
                        _core.typing = _core.typing.Insert(_core.cursorPosition - 1, paste);
                        _core.cursorPosition += paste.Length - 1;
                    }
                    else
                    {
                        _core.typing = "";
                        _core.cursorPosition = 0;
                        foreach (string item in commands)
                        {
                            var waitVal = 0;
                            var commandVal = item.Trim();
                            Func<bool> waitCommandVal = null;
                            if (commandVal.StartsWith("wait "))
                            {
                                var parts = commandVal.Split(' ');
                                if (parts.Length == 2)
                                {
                                    if (parts[1] == "level")
                                    {
                                        Level c2 = Level.current;
                                        waitCommandVal = () => Level.current == c2 && Level.core.nextLevel == null;
                                    }
                                    else if (Triggers.IsTrigger(parts[1].ToUpperInvariant()))
                                    {
                                        waitCommandVal = () => Input.Pressed(parts[1].ToUpperInvariant());
                                    }
                                    else
                                    {
                                        try
                                        {
                                            waitVal = Convert.ToInt32(parts[1]);
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                }
                                commandVal = null;
                            }

                            _pendingCommandQueue.Enqueue(new QueuedCommand
                            {
                                command = commandVal,
                                wait = waitVal,
                                waitCommand = waitCommandVal
                            });
                        }
                    }
                }
            }

            if (Keyboard.Pressed(Keys.Enter) && !string.IsNullOrWhiteSpace(_core.typing))
            {
                RunCommand(_core.typing);
                _core.previousLines.Add(_core.typing);
                _core.typing = "";
                Keyboard.keyString = "";
                _core.lastCommandIndex = -1;
                _core.viewOffset = 0;
            }
            else if (Keyboard.Pressed(Keys.Back))
            {
                if (_core.typing.Length > 0 && _core.cursorPosition > 0)
                {
                    _core.typing = _core.typing.Remove(_core.cursorPosition - 1, 1);
                    _core.cursorPosition--;
                }
                _core.lastCommandIndex = -1;
            }
            else if (Keyboard.Pressed(Keys.Delete))
            {
                if (_core.typing.Length > 0 && _core.cursorPosition < _core.typing.Length)
                    _core.typing = _core.typing.Remove(_core.cursorPosition, 1);
                _core.lastCommandIndex = -1;
            }
            else if (Keyboard.Pressed(Keys.Left))
            {
                _core.cursorPosition = Math.Max(0, _core.cursorPosition - 1);
            }
            else if (Keyboard.Pressed(Keys.Right))
            {
                _core.cursorPosition = Math.Min(_core.typing.Length, _core.cursorPosition + 1);
            }
            else if (Keyboard.Pressed(Keys.Home))
            {
                if (Keyboard.shift)
                    _core.viewOffset = core.lines.Count - 1;
                else
                    _core.cursorPosition = 0;
            }
            else if (Keyboard.Pressed(Keys.End))
            {
                if (Keyboard.shift)
                    _core.viewOffset = 0;
                else
                    _core.cursorPosition = _core.typing.Length;
            }

            if (Keyboard.Pressed(Keys.PageUp))
            {
                _core.viewOffset += ((!Keyboard.shift) ? 1 : 10);
                if (_core.viewOffset > core.lines.Count - 1)
                    _core.viewOffset = core.lines.Count - 1;
            }

            if (Keyboard.Pressed(Keys.PageDown))
            {
                _core.viewOffset -= ((!Keyboard.shift) ? 1 : 10);
                if (_core.viewOffset < 0)
                    _core.viewOffset = 0;
            }

            if (Keyboard.Pressed(Keys.Up) && _core.previousLines.Count > 0)
            {
                _core.lastCommandIndex++;
                if (_core.lastCommandIndex >= _core.previousLines.Count)
                    _core.lastCommandIndex = _core.previousLines.Count - 1;
                _core.typing = _core.previousLines[_core.previousLines.Count - 1 - _core.lastCommandIndex];
                _core.cursorPosition = _core.typing.Length;
            }

            if (Keyboard.Pressed(Keys.Down))
            {
                if (_core.previousLines.Count > 0 && _core.lastCommandIndex > 0)
                {
                    _core.lastCommandIndex--;
                    _core.typing = _core.previousLines[_core.previousLines.Count - 1 - _core.lastCommandIndex];
                    _core.cursorPosition = _core.typing.Length;
                }
                else if (_core.lastCommandIndex == 0)
                {
                    _core.lastCommandIndex--;
                    _core.cursorPosition = 0;
                    _core.typing = "";
                }
                else if (_core.lastCommandIndex == -1)
                {
                    _core.cursorPosition = 0;
                    _core.typing = "";
                }
            }
        }
        else
        {
            lastCommand = null;
        }
    }

    #endregion

    #region Private Methods

    static bool CheckCheats()
    {
        if (NetworkDebugger.enabled)
            return false;

        var overrideCheats = false;
#if FACEPUNCH
        if (SteamClient.SteamId != 0
            && (SteamClient.SteamId == 76561197996786074L
            || SteamClient.SteamId == 76561198885030822L
            || SteamClient.SteamId == 76561198416200652L
            || SteamClient.SteamId == 76561198104352795L
            || SteamClient.SteamId == 76561198114791325L)
            )
#else
        if (DGSteam.User != null && (DGSteam.User.Id == 76561197996786074L || DGSteam.User.Id == 76561198885030822L || DGSteam.User.Id == 76561198416200652L || DGSteam.User.Id == 76561198104352795L || DGSteam.User.Id == 76561198114791325L))
#endif
            overrideCheats = true;

        if (/*!overrideCheats*/false && (Network.isActive || Level.current is ChallengeLevel || Level.current is ArcadeLevel))
        {
            _core.lines.Enqueue(new DCLine
            {
                line = "You can't do that here!",
                color = Color.Red
            });
            return true;
        }

        return false;
    }

    static void SendNetLog(NetworkConnection pConnection)
    {
        List<string> parts = [];
        var currentPart = "";
        for (int i = Math.Max(core.lines.Count - 750, 0); i < core.lines.Count; i++)
        {
            currentPart += core.lines.ElementAt(i).ToSendString();
            if (currentPart.Length > 500)
            {
                parts.Add(currentPart);
                currentPart = "";
            }
        }

        DuckNetwork.core.logTransferSize = parts.Count;
        Send.Message(new NMLogRequestIncoming(parts.Count), pConnection);
        foreach (string p in parts)
        {
            _core.pendingSends.Enqueue(new NMLogRequestChunk(p)
            {
                connection = pConnection
            });
        }
    }

    #endregion
}