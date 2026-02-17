using Microsoft.Xna.Framework;
using SDL3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace DuckGame;

public class DevConsole
{
    private class QueuedCommand
    {
        public Func<bool> waitCommand;

        public string command;

        public int wait;
    }

    public static bool showFPS = false;

    public static List<string> startupCommands = new List<string>();

    public static bool fancyMode = false;

    private static DevConsoleCore _core = new DevConsoleCore();

    private static bool _enableNetworkDebugging = false;

    private static bool _oldConsole;

    public static bool debugOrigin;

    public static bool debugBounds;

    private static RasterFont _raster;

    public static Dictionary<string, List<CMD>> commands = new Dictionary<string, List<CMD>>();

    public static CMD lastCommand;

    public static bool wagnusDebug;

    public static bool fuckUpPacketOrder = false;

    public static List<DCLine> debuggerLines = new List<DCLine>();

    private static string _dataSubmissionMessage = null;

    private static List<ulong> lostSaveIDs = new List<ulong> { 76561198035257896uL };

    public static Sprite _tray;

    public static Sprite _scan;

    private static Queue<QueuedCommand> _pendingCommandQueue = new Queue<QueuedCommand>();

    public static DevConsoleCore core
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

    public static bool open => _core.open;

    public static bool enableNetworkDebugging
    {
        get
        {
            return _enableNetworkDebugging;
        }
        set
        {
            _enableNetworkDebugging = value;
        }
    }

    public static bool splitScreen
    {
        get
        {
            return _core.splitScreen;
        }
        set
        {
            _core.splitScreen = value;
        }
    }

    public static bool rhythmMode
    {
        get
        {
            return _core.rhythmMode;
        }
        set
        {
            _core.rhythmMode = value;
        }
    }

    public static bool qwopMode
    {
        get
        {
            return _core.qwopMode;
        }
        set
        {
            _core.qwopMode = value;
        }
    }

    public static bool showIslands
    {
        get
        {
            return _core.showIslands;
        }
        set
        {
            _core.showIslands = value;
        }
    }

    public static bool showCollision
    {
        get
        {
            if (_core.showCollision)
            {
                return !Network.isActive;
            }
            return false;
        }
        set
        {
            _core.showCollision = value;
        }
    }

    public static bool shieldMode
    {
        get
        {
            return _core.shieldMode;
        }
        set
        {
            _core.shieldMode = value;
        }
    }

    public static Vector2 size => new Vector2(1280f, 1280f / Resolution.current.aspect);

    public static Vector2 dimensions => new Vector2((float)Options.Data.consoleWidth / 100f, (float)Options.Data.consoleHeight / 100f);

    public static int consoleScale => Options.Data.consoleScale;

    public static int fontPoints => Options.Data.consoleFontSize;

    public static string fontName => Options.Data.consoleFont;

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
        string timeString = "";
        timeString += line.timestamp.Minute;
        if (timeString.Length == 1)
        {
            timeString = " " + timeString;
        }
        timeString += ":";
        if (line.timestamp.Second < 10)
        {
            timeString += "0";
        }
        timeString += line.timestamp.Second;
        core.font.Scale = new Vector2(1f);
        core.font.Draw((times ? ("|GRAY|" + timeString + " ") : "") + (section ? line.SectionString(colored: true, small: true) : "") + line.line, pos.X, pos.Y, line.color * 0.8f, 0.9f);
        core.font.Scale = new Vector2(2f);
    }

    public static void InitializeFont()
    {
        if (Options.Data.consoleFont == "" || Options.Data.consoleFont == null)
        {
            _raster = null;
        }
        else if (_raster == null)
        {
            _raster = new RasterFont(Options.Data.consoleFont, Options.Data.consoleFontSize);
        }
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
            _core.font = new BitmapFont("biosFont", 8);
            _core.font.Scale = new Vector2(2f, 2f);
            _core.fancyFont = new FancyBitmapFont("smallFont");
            _core.fancyFont.Scale = new Vector2(2f, 2f);
        }
        if (!(_core.alpha > 0.01f))
        {
            return;
        }
        InitializeFont();
        if (_tray == null)
        {
            return;
        }
        _tray.Alpha = _core.alpha;
        _tray.Scale = new Vector2((float)(Math.Round((float)Resolution.current.x / 1280f * 2f) / 2.0) * 2f) * (consoleScale + 1) / 2f;
        _tray.Depth = 0.75f;
        int numSectionsVert = (int)(Layer.core._console.camera.height * dimensions.Y / (16f * _tray.Scale.Y)) - 2;
        int numSectionsHor = (int)(Layer.core._console.camera.width * dimensions.X / (16f * _tray.Scale.X)) - 2;
        Graphics.Draw(_tray, 0f, 0f, new Rectangle(0f, 0f, 18f, 18f));
        Graphics.Draw(_tray, 0f, 18f * _tray.Scale.Y + (float)numSectionsVert * (16f * _tray.Scale.Y), new Rectangle(0f, _tray.height - 18, 18f, 18f));
        Graphics.Draw(_tray, 18f * _tray.Scale.X + (float)(numSectionsHor - 6) * (16f * _tray.Scale.X), 18f * _tray.Scale.Y + (float)numSectionsVert * (16f * _tray.Scale.Y), new Rectangle(_tray.width - 114, _tray.height - 18, 114f, 18f));
        for (int i = 0; i < numSectionsHor; i++)
        {
            Graphics.Draw(_tray, 18f * _tray.Scale.X + 16f * _tray.Scale.X * (float)i, 0f, new Rectangle(16f, 0f, 16f, 18f));
            if (i < numSectionsHor - 6)
            {
                Graphics.Draw(_tray, 18f * _tray.Scale.X + 16f * _tray.Scale.X * (float)i, 18f * _tray.Scale.Y + (float)numSectionsVert * (16f * _tray.Scale.Y), new Rectangle(16f, _tray.height - 18, 16f, 18f));
            }
        }
        Graphics.Draw(_tray, 18f * _tray.Scale.X + (float)numSectionsHor * (16f * _tray.Scale.X), 0f, new Rectangle(_tray.width - 18, 0f, 18f, 18f));
        for (int j = 0; j < numSectionsVert; j++)
        {
            Graphics.Draw(_tray, 0f, 18f * _tray.Scale.Y + 16f * _tray.Scale.Y * (float)j, new Rectangle(0f, 18f, 18f, 16f));
            Graphics.Draw(_tray, 18f * _tray.Scale.X + (float)numSectionsHor * (16f * _tray.Scale.X), 18f * _tray.Scale.Y + 16f * _tray.Scale.Y * (float)j, new Rectangle(_tray.width - 18, 18f, 18f, 16f));
        }
        Graphics.DrawRect(Vector2.Zero, new Vector2(18f * _tray.Scale.X + (float)numSectionsHor * (16f * _tray.Scale.X) + _tray.Scale.Y * 4f, (float)(numSectionsVert + 2) * (16f * _tray.Scale.Y)), Color.Black * 0.8f * _core.alpha, 0.7f);
        _core.fancyFont.Scale = new Vector2(_tray.Scale.X / 2f);
        _core.fancyFont.Depth = 0.98f;
        _core.fancyFont.Alpha = _core.alpha;
        float height = (float)((numSectionsVert + 1) * 16) * _tray.Scale.Y + 5f * _tray.Scale.Y;
        float width = (float)(numSectionsHor + 2) * (16f * _tray.Scale.X);
        string ver = DG.version;
        _core.fancyFont.Draw(ver, new Vector2(82f * _tray.Scale.X + (float)(numSectionsHor - 6) * (16f * _tray.Scale.X), height + 7f * _tray.Scale.Y), new Color(62, 114, 122), 0.98f);
        _core.cursorPosition = Math.Min(Math.Max(_core.cursorPosition, 0), _core.typing.Length);
        if (_raster != null)
        {
            _raster.Scale = new Vector2(0.5f);
            _raster.Alpha = _core.alpha;
            _raster.Draw(_core.typing, 4f * _tray.Scale.X, height + _tray.Scale.Y * 8f - (float)_raster.characterHeight * _raster.Scale.Y / 2f, Color.White, 0.9f);
            Vector2 vec = new Vector2(_raster.GetWidth(_core.typing.Substring(0, _core.cursorPosition)) + 4f * _tray.Scale.X + 1f, height + 6f * _tray.Scale.Y);
            Graphics.DrawLine(vec, vec + new Vector2(0f, 4f * _tray.Scale.X), Color.White, 1f, 1f);
        }
        else
        {
            _core.font.Scale = new Vector2(_tray.Scale.X / 2f);
            _core.font.Alpha = _core.alpha;
            _core.font.Draw(_core.typing, 4f * _tray.Scale.X, height + 6f * _tray.Scale.Y, Color.White, 0.9f);
            Vector2 vec2 = new Vector2(_core.font.GetWidth(_core.typing.Substring(0, _core.cursorPosition)) + 4f * _tray.Scale.X, height + 6f * _tray.Scale.Y);
            Graphics.DrawLine(vec2, vec2 + new Vector2(0f, 4f * _tray.Scale.X), Color.White, 2f, 1f);
        }
        int index = _core.lines.Count - 1 - _core.viewOffset;
        float vOffset = 0f;
        _core.font.Scale = new Vector2((float)Math.Max(Math.Round(_tray.Scale.X / 4f), 1.0));
        float mul = _core.font.Scale.X / 2f;
        float lineHeight = 18f * mul;
        float numWidth = 20f * (_core.font.Scale.X * 2f);
        if (_raster != null)
        {
            lineHeight = (float)(_raster.characterHeight - 2) * _raster.Scale.Y;
            vOffset = lineHeight;
            numWidth = _raster.GetWidth("0000  ");
        }
        for (int k = 0; (float)k < (height - 2f * _tray.Scale.Y) / lineHeight - 1f; k++)
        {
            if (index < 0)
            {
                break;
            }
            DCLine line = _core.lines.ElementAt(index);
            string lineNumber = index.ToString();
            while (lineNumber.Length < 4)
            {
                lineNumber = "0" + lineNumber;
            }
            if (_raster != null)
            {
                _raster.maxWidth = (int)(width - 35f * _tray.Scale.X);
                _raster.singleLine = true;
                _raster.enforceWidthByWord = false;
                _raster.Draw(lineNumber, 4f * _tray.Scale.X, height - vOffset + 2f, (index % 2 > 0) ? (Color.Gray * 0.4f) : (Color.Gray * 0.6f), 0.9f);
                _raster.Draw(line.SectionString() + line.line, 4f * _tray.Scale.X + numWidth, height - vOffset + 2f, line.color, 0.9f);
                vOffset += lineHeight;
            }
            else
            {
                _core.font.maxWidth = (int)(width - 35f * _tray.Scale.X);
                _core.font.singleLine = true;
                _core.font.enforceWidthByWord = false;
                _core.font.Draw(lineNumber, 4f * _tray.Scale.X, height - 18f * mul - vOffset + 2f, (index % 2 > 0) ? (Color.Gray * 0.4f) : (Color.Gray * 0.6f), 0.9f);
                _core.font.Draw(line.SectionString() + line.line, 4f * _tray.Scale.X + numWidth, height - 18f * mul - vOffset + 2f, line.color * 0.8f, 0.9f);
                vOffset += 18f * mul;
            }
            index--;
        }
        _core.font.Scale = new Vector2(2f);
    }
    public static Profile ProfileByName(string findName)
    {
        foreach (Profile p in Profiles.all)
        {
            if (p.team != null)
            {
                string name = p.name.ToLower();
                if (findName == "player1" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer1))
                {
                    name = findName;
                }
                else if (findName == "player2" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer2))
                {
                    name = findName;
                }
                else if (findName == "player3" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer3))
                {
                    name = findName;
                }
                else if (findName == "player4" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer4))
                {
                    name = findName;
                }
                else if (findName == "player5" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer5))
                {
                    name = findName;
                }
                else if (findName == "player6" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer6))
                {
                    name = findName;
                }
                else if (findName == "player7" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer7))
                {
                    name = findName;
                }
                else if (findName == "player8" && p.inputProfile == InputProfile.Get(InputProfile.MPPlayer8))
                {
                    name = findName;
                }
                if (name == findName)
                {
                    return p;
                }
            }
        }
        return null;
    }

    public static void AddCommand(CMD pCommand)
    {
        GetCommands(pCommand.keyword).Add(pCommand);
        if (pCommand.aliases == null)
        {
            return;
        }
        foreach (string alias in pCommand.aliases)
        {
            GetCommands(alias).Add(pCommand);
        }
    }

    public static List<CMD> GetCommands(string pKeyword)
    {
        if (!commands.TryGetValue(pKeyword, out var cmds))
        {
            cmds = (commands[pKeyword] = new List<CMD>());
        }
        return cmds;
    }

    public static void RunCommand(string command)
    {
        if (DG.buildExpired)
        {
            return;
        }
        _core.logScores = -1;
        if (!(command != ""))
        {
            return;
        }
        CultureInfo culture = CultureInfo.CurrentCulture;
        bool isCommand = false;
        ConsoleCommand c = new ConsoleCommand(command);
        string commandName = c.NextWord();
        _core.lines.Enqueue(new DCLine
        {
            line = command,
            color = Color.White
        });
        string message = null;
        int lastMessagePriority = int.MinValue;
        string lastMessageCommandName = "";
        foreach (CMD command2 in GetCommands(commandName))
        {
            CMD cmd = command2;
            isCommand = true;
            ConsoleCommand c2 = new ConsoleCommand(c.Remainder());
            while (cmd.subcommand != null && c2.NextWord(toLower: true, peek: true) == cmd.subcommand.keyword)
            {
                c2.NextWord();
                cmd = cmd.subcommand;
            }
            if (/*cmd.cheat*/false && !NetworkDebugger.enabled)
            {
                bool overrideCheats = false;
                if (Steam.user != null && (Steam.user.id == 76561197996786074L || Steam.user.id == 76561198885030822L || Steam.user.id == 76561198416200652L || Steam.user.id == 76561198104352795L || Steam.user.id == 76561198114791325L))
                {
                    overrideCheats = true;
                }
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
                {
                    _pendingCommandQueue.Peek().waitCommand = cmd.commandQueueWaitFunction;
                }
                if (cmd.commandQueueWait > 0 && _pendingCommandQueue.Count > 0)
                {
                    _pendingCommandQueue.Peek().wait = cmd.commandQueueWait;
                }
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
            string[] array = message.Split('\n');
            foreach (string s in array)
            {
                _core.lines.Enqueue(new DCLine
                {
                    line = s,
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
                {
                    return;
                }
                isCommand = true;
                string spawnItem = c.NextWord();
                float xpos = 0f;
                float ypos = 0f;
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
                foreach (Type tp in Editor.ThingTypes)
                {
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
                        line = "The type " + spawnItem + " does not exist!",
                        color = Color.Red
                    });
                    return;
                }
                if (!Editor.HasConstructorParameter(t))
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = spawnItem + " can not be spawned this way.",
                        color = Color.Red
                    });
                    return;
                }
                Thing newThing = Editor.CreateThing(t) as PhysicsObject;
                if (newThing != null)
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
            {
                _core.open = !_core.open;
            }
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
                            string val = c.NextWord().ToLower(culture);
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
                            string val = c.NextWord().ToLower(culture);
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
                                    Options.Data.consoleHeight = (int)Math.Min(Math.Max(high2, 0.25f), 1f) * 100;
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
                            string val = c.NextWord().ToLower(culture);
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
                            string val = c.NextWord();
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
                                        int pts = Convert.ToInt32(val);
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
                                if (c.Remainder().Count() > 0)
                                {
                                    val = val + " " + c.Remainder();
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
                                        Log(DCSection.General, "|DGGREEN|Font is now " + _raster.data.name + "! What a laugh!");
                                    }
                                    else
                                    {
                                        Log(DCSection.General, "|DGGREEN|Font is now " + _raster.data.name + "!");
                                    }
                                }
                                else
                                {
                                    Log(DCSection.General, "|DGRED|Could not find font (" + val + ")!");
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
                string level = c.NextWord();
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
                {
                    return;
                }
                isCommand = true;
                string who = c.NextWord();
                Profile p = ProfileByName(who);
                if (p == null)
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "No profile named " + who + ".",
                        color = Color.Red
                    });
                    return;
                }
                string team = c.NextWord();
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
                bool found = false;
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
                        line = "No team named " + team + ".",
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
                    if (!(p2.name.ToLower(culture) == who2))
                    {
                        continue;
                    }
                    if (p2.duck != null)
                    {
                        found2 = true;
                        string function = c.NextWord();
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
                        MethodInfo[] methods = typeof(Duck).GetMethods();
                        bool foundMethod = false;
                        MethodInfo[] array2 = methods;
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
                {
                    return;
                }
                isCommand = true;
                string who3 = c.NextWord();
                bool found3 = false;
                foreach (Profile p3 in Profiles.all)
                {
                    if (!(p3.name.ToLower(culture) == who3))
                    {
                        continue;
                    }
                    if (p3.duck != null)
                    {
                        found3 = true;
                        string variable = c.NextWord();
                        if (variable == "")
                        {
                            _core.lines.Enqueue(new DCLine
                            {
                                line = "Parameters in wrong format.",
                                color = Color.Red
                            });
                            return;
                        }
                        Type duckType = typeof(Duck);
                        PropertyInfo[] properties = duckType.GetProperties();
                        bool foundProperty = false;
                        PropertyInfo[] array3 = properties;
                        foreach (PropertyInfo property in array3)
                        {
                            if (!(property.Name.ToLower(culture) == variable))
                            {
                                continue;
                            }
                            foundProperty = true;
                            if (property.PropertyType == typeof(float))
                            {
                                float val2 = 0f;
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
                                bool val3 = false;
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
                                int val4 = 0;
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
                                float xval = 0f;
                                float yval = 0f;
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
                        {
                            continue;
                        }
                        FieldInfo[] fields = duckType.GetFields();
                        foreach (FieldInfo field in fields)
                        {
                            if (!(field.Name.ToLower(culture) == variable))
                            {
                                continue;
                            }
                            foundProperty = true;
                            if (field.FieldType == typeof(float))
                            {
                                float val5 = 0f;
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
                                bool val6 = false;
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
                                int val7 = 0;
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
                                float xval2 = 0f;
                                float yval2 = 0f;
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
                                line = "Duck has no variable called " + variable + ".",
                                color = Color.Red
                            });
                            return;
                        }
                        continue;
                    }
                    _core.lines.Enqueue(new DCLine
                    {
                        line = who3 + " is not in the game!",
                        color = Color.Red
                    });
                    return;
                }
                if (!found3)
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "No profile named " + who3 + ".",
                        color = Color.Red
                    });
                    return;
                }
            }
            if (commandName == "globalscores")
            {
                if (CheckCheats())
                {
                    return;
                }
                isCommand = true;
                using List<Profile>.Enumerator enumerator5 = Profiles.active.GetEnumerator();
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
                {
                    return;
                }
                isCommand = true;
                string who4 = c.NextWord();
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
                int num = 0;
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
                line = commandName + " is not a valid command!",
                color = Color.Red
            });
        }
    }

    private static bool CheckCheats()
    {
        return false;
        if (NetworkDebugger.enabled)
        {
            return false;
        }
        bool overrideCheats = false;
        if (Steam.user != null && (Steam.user.id == 76561197996786074L || Steam.user.id == 76561198885030822L || Steam.user.id == 76561198416200652L || Steam.user.id == 76561198104352795L || Steam.user.id == 76561198114791325L))
        {
            overrideCheats = true;
        }
        if (!overrideCheats && (Network.isActive || Level.current is ChallengeLevel || Level.current is ArcadeLevel))
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

    public static void LogComplexMessage(string text, Color c, float scale = 2f, int index = -1)
    {
        if (text.Contains('\n'))
        {
            string[] array = text.Split('\n');
            for (int i = 0; i < array.Length; i++)
            {
                Log(array[i], c, scale, index);
            }
            return;
        }
        DCLine line = new DCLine
        {
            line = text,
            color = c,
            threadIndex = ((index < 0) ? NetworkDebugger.currentIndex : index),
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

    public static void Log(string text, Color c, float scale = 2f, int index = -1)
    {
        DCLine line = new DCLine
        {
            line = text,
            color = c,
            threadIndex = ((index < 0) ? NetworkDebugger.currentIndex : index),
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
        if (pDescription == null)
        {
            pDescription = "No Description.";
        }
        Log("@LOGEVENT@|AQUA|LOGEVENT!-----------" + pConnection.ToString() + "|AQUA|signalled a log event!-----------!LOGEVENT", Color.White);
        Log("@LOGEVENT@|AQUA|LOGEVENT!---" + pDescription + "|AQUA|---!LOGEVENT", Color.White);
        if (Network.isActive && pConnection == DuckNetwork.localConnection)
        {
            Send.Message(new NMLogEvent(pDescription));
        }
    }

    public static void Log(DCSection section, string text, int netIndex = -1)
    {
        Log(section, Verbosity.Normal, text, netIndex);
    }

    public static void Log(DCSection section, string text, NetworkConnection context, int netIndex = -1)
    {
        if (context != null)
        {
            text += context.ToString();
        }
        Log(section, Verbosity.Normal, text, netIndex);
    }

    public static void Log(DCSection section, Verbosity verbose, string text, int netIndex = -1)
    {
        DCLine line = new DCLine
        {
            line = text,
            section = section,
            verbosity = verbose,
            color = Color.White,
            threadIndex = ((netIndex < 0) ? NetworkDebugger.currentIndex : netIndex),
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

    private static void SendNetLog(NetworkConnection pConnection)
    {
        List<string> parts = new List<string>();
        string currentPart = "";
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

    public static void SaveNetLog(string pName = null)
    {
        FlushPendingLines();
        string currentPart = "";
        for (int i = Math.Max(core.lines.Count - 1500, 0); i < core.lines.Count; i++)
        {
            currentPart += core.lines.ElementAt(i).ToSendString();
        }
        if (pName == null)
        {
            pName = DateTime.Now.ToShortDateString().Replace('/', '_') + "_" + DateTime.Now.ToLongTimeString().Replace(':', '_') + "_" + Steam.user.name + "_netlog.rtf";
        }
        else if (!pName.EndsWith(".rtf"))
        {
            pName += ".rtf";
        }
        string output = DuckFile.FixInvalidPath(DuckFile.logDirectory + pName);
        if (File.Exists(output))
        {
            File.Delete(output);
        }
        RichTextBox richTextBox = new FancyBitmapFont().MakeRTF(currentPart);
        DuckFile.CreatePath(output);
        richTextBox.SaveFile(output);
    }

    public static void LogTransferComplete(NetworkConnection pConnection)
    {
        string data = core.GetReceivedLogData(pConnection);
        if (data != null)
        {
            string output = DuckFile.FixInvalidPath(DuckFile.logDirectory + DateTime.Now.ToShortDateString().Replace('/', '_') + "_" + DateTime.Now.ToLongTimeString().Replace(':', '_') + "_" + pConnection.name + "_netlog.rtf");
            RichTextBox richTextBox = new FancyBitmapFont().MakeRTF(data);
            DuckFile.CreatePath(output);
            richTextBox.SaveFile(output);
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

        AddCommand(new CMD("level", new CMD.Argument[1]
        {
            new CMD.Level("level")
        }, delegate (CMD cmd)
        {
            Level.current = cmd.Arg<Level>("level");
        })
        {
            cheat = true,
            aliases = new List<string> { "lev" },
            commandQueueWaitFunction = () => Level.core.nextLevel == null
        });
        AddCommand(new CMD("give",
        [
            new CMD.Thing<Duck>("player"),
            new CMD.Thing<Holdable>("object"),
            new CMD.String("specialCode", pOptional: true)
        ], delegate (CMD cmd)
        {
            Duck duck = cmd.Arg<Duck>("player");
            Holdable holdable = cmd.Arg<Holdable>("object");
            string text = cmd.Arg<string>("specialCode");
            Level.Add(holdable);
            if (text == "i" && holdable is Gun)
            {
                (holdable as Gun).infinite.value = true;
            }
            switch (text)
            {
                case "h":
                case "hp":
                case "ph":
                    {
                        Holster holster = duck.GetEquipment(typeof(Holster)) as Holster;
                        if (holster == null)
                        {
                            holster = ((!(text == "hp") && !(text == "ph")) ? new Holster(0f, 0f) : new PowerHolster(0f, 0f));
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
        AddCommand(new CMD("give", new CMD.Argument[2]
        {
            new CMD.Thing<Duck>("duckName"),
            new CMD.Thing<TeamHat>("hat")
        }, delegate (CMD cmd)
        {
            Duck duck = cmd.Arg<Duck>("duckName");
            TeamHat teamHat = cmd.Arg<TeamHat>("hat");
            Level.Add(teamHat);
            duck.GiveHoldable(teamHat);
            SFX.Play("hitBox");
        })
        {
            cheat = true
        });
        AddCommand(new CMD("kill", new CMD.Argument[1]
        {
            new CMD.Thing<Duck>("duckName")
        }, delegate (CMD cmd)
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
                line = "Your steam ID is: " + Profiles.experienceProfile.steamID,
                color = Colors.DGBlue
            });
        }));
        AddCommand(new CMD("localid", (Action)delegate
        {
            _core.lines.Enqueue(new DCLine
            {
                line = "Your local ID is: " + DG.localID,
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
                line = "GC Has " + num + " KB Allocated (" + num / 1000 + " MB)",
                color = Color.White
            });
        }));
        AddCommand(new CMD("log", new CMD.Argument[1]
        {
            new CMD.String("description")
            {
                takesMultispaceString = true
            }
        }, delegate (CMD cmd)
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
        AddCommand(new CMD("accept", new CMD.Argument[1]
        {
            new CMD.Integer("number")
        }, delegate (CMD cmd)
        {
            try
            {
                int index = cmd.Arg<int>("number");
                Profile profile = DuckNetwork.profiles[index];
                if (core.transferRequestsPending.Contains(profile.connection))
                {
                    core.transferRequestsPending.Remove(profile.connection);
                    SendNetLog(profile.connection);
                }
            }
            catch (Exception)
            {
            }
        })
        {
            hidden = true
        });
        AddCommand(new CMD("eight", (Action)delegate
        {
            int num = 0;
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
        AddCommand(new CMD("chat", new CMD("font", new CMD.Argument[1]
        {
            new CMD.Font("font", () => Options.Data.chatFontSize)
        }, delegate (CMD cmd)
        {
            string chatFont = cmd.Arg<string>("font");
            Options.Data.chatFont = chatFont;
            Options.Save();
            DuckNetwork.UpdateFont();
        })));
        AddCommand(new CMD("chat", new CMD("font", new CMD("size", new CMD.Argument[1]
        {
            new CMD.Integer("size")
        }, delegate (CMD cmd)
        {
            int chatFontSize = cmd.Arg<int>("size");
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
            {
                Music.Stop();
            }
            _core.rhythmMode = !_core.rhythmMode;
            if (_core.rhythmMode)
            {
                Music.Play(Music.RandomTrack("InGame"));
            }
        })
        {
            hidden = true,
            cheat = true
        });
        AddCommand(new CMD("toggle", new CMD.Argument[1]
        {
            new CMD.Layer("layer")
        }, delegate (CMD cmd)
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
            Profile p = new Profile(Profiles.experienceProfile.steamID.ToString(), null, null, null, network: false, Profiles.experienceProfile.steamID.ToString())
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
                (MonoMain.pauseMenu = new UIPresentBox(RoomEditor.GetFurniture("VOODOO VINCENT"), Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f)).Open();
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
                MonoMain.pauseMenu = new UIUnlockBox(Unlockables.GetPendingUnlocks().ToList(), Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f);
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
                line = "Constant sync has been " + (Options.Data.powerUser ? "enabled" : "disabled") + "!",
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
                line = "Power User mode has been " + (Options.Data.powerUser ? "enabled" : "disabled") + "!",
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
                line = "Oldschool Angles have been " + (Options.Data.oldAngleCode ? "enabled" : "disabled") + "!",
                color = (Options.Data.oldAngleCode ? Colors.DGGreen : Colors.DGRed)
            });
            Options.Save();
            if (Network.isActive && DuckNetwork.localProfile != null)
            {
                Send.Message(new NMOldAngles(DuckNetwork.localProfile, Options.Data.oldAngleCode));
            }
        })
        {
            hidden = true
        });
        AddCommand(new CMD("debugtypelist", (Action)delegate
        {
            foreach (KeyValuePair<string, Type> current in ModLoader._typesByName)
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
            foreach (KeyValuePair<string, Type> current in ModLoader._typesByNameUnprocessed)
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
        AddCommand(new CMD("sing", new CMD.Argument[1]
        {
            new CMD.String("song")
        }, delegate (CMD cmd)
        {
            string text = cmd.Arg<string>("song");
            Music.Play(text);
            if (Network.isActive)
            {
                Send.Message(new NMSwitchMusic(text));
            }
        })
        {
            hidden = true,
            cheat = true
        });
        AddCommand(new CMD("downpour", (Action<CMD>)delegate
        {
            float num = Level.current.bottomRight.X - Level.current.topLeft.X + 128f;
            int num2 = 10;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < num2; j++)
                {
                    PhysicsObject randomItem = ItemBoxRandom.GetRandomItem();
                    randomItem.Position = Level.current.topLeft + new Vector2(-64f + (num / (float)num2 * (float)j + Rando.Float(-128f, 128f)), Level.current.topLeft.Y - 2000f - (float)(512 * i) + Rando.Float(-256f, 256f));
                    Level.Add(randomItem);
                }
            }
        })
        {
            hidden = true,
            cheat = true
        });
        AddCommand(new CMD("ruindatahash", new CMD.Argument[1]
        {
            new CMD.String("workshopID", pOptional: true)
        }, delegate (CMD cmd)
        {
            string text = cmd.Arg<string>("workshopID");
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
                bool flag = false;
                try
                {
                    Mod modFromWorkshopID = ModLoader.GetModFromWorkshopID(Convert.ToUInt64(text));
                    if (modFromWorkshopID != null)
                    {
                        modFromWorkshopID.System_RuinDatahash();
                        flag = true;
                        _core.lines.Enqueue(new DCLine
                        {
                            line = "Ruined datahash for " + modFromWorkshopID.configuration.displayName + "! Good luck playing online now!",
                            color = Colors.DGRed
                        });
                    }
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    _core.lines.Enqueue(new DCLine
                    {
                        line = "Could not find mod with ID (" + text + ")",
                        color = Colors.DGRed
                    });
                }
            }
        })
        {
            hidden = true,
            cheat = true
        });
        if (!Steam.IsInitialized())
        {
            return;
        }
        AddCommand(new CMD("zipcloud", (Action)delegate
        {
            string text = DuckFile.saveDirectory + "cloud_zip.zip";
            Cloud.ZipUpCloudData(text);
            _core.lines.Enqueue(new DCLine
            {
                line = "Zipped up to: " + text,
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
            string currentPart = "";
            for (int i = Math.Max(core.lines.Count - 750, 0); i < core.lines.Count; i++)
            {
                currentPart += core.lines.ElementAt(i).ToShortString();
            }
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
        AddCommand(new CMD("savetool", (Action)delegate
        {
            if (File.Exists("SaveTool.dll"))
            {
                MonoMain.showingSaveTool = true;
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
                {
                    _core.viewOffset++;
                }
            }
            if (_core.lines.Count > 3000)
            {
                for (int i = 0; i < 500; i++)
                {
                    _core.lines.Dequeue();
                    if (_core.viewOffset > 0)
                    {
                        _core.viewOffset--;
                    }
                }
            }
            _core.pendingLines.Clear();
        }
    }

    public static void Update()
    {
        if (_core == null)
        {
            return;
        }
        FlushPendingLines();
        bool shift = Keyboard.Down(Keys.LeftShift) || Keyboard.Down(Keys.RightShift);
        bool num = Keyboard.Pressed(Keys.OemTilde) && !shift;
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
        _core.alpha = Maths.LerpTowards(_core.alpha, _core.open ? 1f : 0f, 0.1f);
        if (_pendingCommandQueue.Count > 0)
        {
            QueuedCommand c = _pendingCommandQueue.Peek();
            if (c.wait > 0)
            {
                c.wait--;
            }
            else if (c.waitCommand == null || c.waitCommand())
            {
                _pendingCommandQueue.Dequeue();
                if (c.command != null)
                {
                    RunCommand(c.command);
                }
            }
        }
        if (_core.open && NetworkDebugger.hoveringInstance)
        {
            Input._imeAllowed = true;
            if (_core.cursorPosition > _core.typing.Length)
            {
                _core.cursorPosition = _core.typing.Length;
            }
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
                    string paste = "";
                    paste = SDL.SDL_GetClipboardText();
                    string[] array = paste.Replace('\r', '\n').Split('\n');
                    List<string> commands = new List<string>();
                    string[] array2 = array;
                    foreach (string line in array2)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            commands.Add(line);
                        }
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
                            int waitVal = 0;
                            string commandVal = item.Trim();
                            Func<bool> waitCommandVal = null;
                            if (commandVal.StartsWith("wait "))
                            {
                                string[] parts = commandVal.Split(' ');
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
                {
                    _core.typing = _core.typing.Remove(_core.cursorPosition, 1);
                }
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
                {
                    _core.viewOffset = core.lines.Count - 1;
                }
                else
                {
                    _core.cursorPosition = 0;
                }
            }
            else if (Keyboard.Pressed(Keys.End))
            {
                if (Keyboard.shift)
                {
                    _core.viewOffset = 0;
                }
                else
                {
                    _core.cursorPosition = _core.typing.Length;
                }
            }
            if (Keyboard.Pressed(Keys.PageUp))
            {
                _core.viewOffset += ((!Keyboard.shift) ? 1 : 10);
                if (_core.viewOffset > core.lines.Count - 1)
                {
                    _core.viewOffset = core.lines.Count - 1;
                }
            }
            if (Keyboard.Pressed(Keys.PageDown))
            {
                _core.viewOffset -= ((!Keyboard.shift) ? 1 : 10);
                if (_core.viewOffset < 0)
                {
                    _core.viewOffset = 0;
                }
            }
            if (Keyboard.Pressed(Keys.Up) && _core.previousLines.Count > 0)
            {
                _core.lastCommandIndex++;
                if (_core.lastCommandIndex >= _core.previousLines.Count)
                {
                    _core.lastCommandIndex = _core.previousLines.Count - 1;
                }
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
}
