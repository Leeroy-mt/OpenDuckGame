using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class CMD
{
    public abstract class Argument
    {
        public Type type;

        public object value;

        public string name;

        public bool optional;

        public bool takesMultispaceString;

        protected string _parseFailMessage = "Argument was in wrong format. ";

        public Argument(string pName, bool pOptional)
        {
            name = pName;
            optional = pOptional;
        }

        public virtual string GetParseFailedMessage()
        {
            return _parseFailMessage;
        }

        public abstract object Parse(string pValue);

        protected object Error(string pError = null)
        {
            if (pError != null)
            {
                _parseFailMessage = pError;
            }
            return null;
        }
    }

    public class String : Argument
    {
        public String(string pName, bool pOptional = false)
            : base(pName, pOptional)
        {
            type = typeof(string);
        }

        public override object Parse(string pValue)
        {
            return pValue;
        }
    }

    public class Integer : Argument
    {
        public Integer(string pName, bool pOptional = false)
            : base(pName, pOptional)
        {
            type = typeof(int);
        }

        public override object Parse(string pValue)
        {
            try
            {
                return Convert.ToInt32(pValue);
            }
            catch (Exception)
            {
            }
            return Error("Argument value must be an integer.");
        }
    }

    public class Font : Argument
    {
        private Func<int> defaultFontSize;

        public Font(string pName, Func<int> pDefaultFontSize = null, bool pOptional = false)
            : base(pName, pOptional)
        {
            type = typeof(string);
            takesMultispaceString = true;
            defaultFontSize = pDefaultFontSize;
        }

        public override object Parse(string pValue)
        {
            switch (pValue)
            {
                case "clear":
                case "default":
                case "none":
                    return "";
                default:
                    {
                        bool bold = false;
                        bool italic = false;
                        if (pValue.EndsWith(" bold"))
                        {
                            bold = true;
                            pValue = pValue.Substring(0, pValue.Length - 5);
                        }
                        if (pValue.EndsWith(" italic"))
                        {
                            italic = true;
                            pValue = pValue.Substring(0, pValue.Length - 7);
                        }
                        if (pValue == "comic sans")
                        {
                            pValue = "comic sans ms";
                        }
                        string name = RasterFont.GetName(pValue);
                        if (name != null)
                        {
                            if (bold)
                            {
                                name += "@BOLD@";
                            }
                            if (italic)
                            {
                                name += "@ITALIC@";
                            }
                            return name;
                        }
                        return Error("Font (" + pValue + ") was not found.");
                    }
            }
        }
    }

    public class Layer : Argument
    {
        public Layer(string pName, bool pOptional = false)
            : base(pName, pOptional)
        {
            type = typeof(Layer);
        }

        public override object Parse(string pValue)
        {
            DuckGame.Layer l = DuckGame.Layer.core._layers.FirstOrDefault((DuckGame.Layer x) => x.name.ToLower() == pValue);
            if (l == null)
            {
                return Error("Layer named (" + pValue + ") was not found.");
            }
            return l;
        }
    }

    public class Level : Argument
    {
        public Level(string pName, bool pOptional = false)
            : base(pName, pOptional)
        {
            type = typeof(Level);
            takesMultispaceString = true;
        }

        public override object Parse(string pValue)
        {
            if (pValue == "pyramid" || (pValue.StartsWith("pyramid") && pValue.Contains('|')))
            {
                int seed = 0;
                if (pValue.Contains('|'))
                {
                    try
                    {
                        seed = Convert.ToInt32(pValue.Split('|')[1]);
                    }
                    catch (Exception)
                    {
                    }
                }
                return new GameLevel("RANDOM", seed);
            }
            switch (pValue)
            {
                case "title":
                    return new TitleScreen();
                case "rockintro":
                    return new RockIntro(new GameLevel(Deathmatch.RandomLevelString(GameMode.previousLevel)));
                case "rockthrow":
                    return new RockScoreboard();
                case "finishscreen":
                    return new RockScoreboard(null, ScoreBoardMode.ShowWinner, afterHighlights: true);
                case "highlights":
                    return new HighlightLevel();
                case "next":
                    return new GameLevel(Deathmatch.RandomLevelString(GameMode.previousLevel));
                case "editor":
                    return Main.editor;
                case "arcade":
                    return new ArcadeLevel(Content.GetLevelID("arcade"));
                default:
                    {
                        if (!pValue.EndsWith(".lev"))
                        {
                            pValue += ".lev";
                        }
                        string addString = "deathmatch/" + pValue;
                        LevelData lev = DuckFile.LoadLevel(Content.path + "/levels/" + addString);
                        if (lev != null)
                        {
                            return new GameLevel(lev.metaData.guid);
                        }
                        lev = DuckFile.LoadLevel(DuckFile.levelDirectory + pValue);
                        if (lev != null)
                        {
                            return new GameLevel(pValue);
                        }
                        lev = DuckFile.LoadLevel(Content.path + "/levels/" + pValue);
                        if (lev != null)
                        {
                            return new GameLevel(lev.metaData.guid);
                        }
                        foreach (Mod m in ModLoader.accessibleMods)
                        {
                            if (m.configuration.content == null)
                            {
                                continue;
                            }
                            foreach (string s in m.configuration.content.levels)
                            {
                                if (s.ToLowerInvariant().EndsWith(pValue))
                                {
                                    return new GameLevel(s);
                                }
                            }
                        }
                        return Error("Level (" + pValue + ") was not found.");
                    }
            }
        }
    }

    public class Thing<T> : String where T : Thing
    {
        public Thing(string pName, bool pOptional = false)
            : base(pName, pOptional)
        {
            type = typeof(T);
        }

        public override object Parse(string pValue)
        {
            pValue = pValue.ToLowerInvariant();
            if (pValue == "gun")
            {
                return ItemBoxRandom.GetRandomItem();
            }
            if (typeof(T) == typeof(Duck))
            {
                Profile p = DevConsole.ProfileByName(pValue);
                if (p == null)
                {
                    return Error("Argument (" + pValue + ") should be the name of a player.");
                }
                if (p.duck == null)
                {
                    return Error("Player (" + pValue + ") is not present in the game.");
                }
                return p.duck;
            }
            if (typeof(T) == typeof(TeamHat))
            {
                Team tm = Teams.all.FirstOrDefault((Team x) => x.name.ToLower() == pValue);
                if (tm != null)
                {
                    return new TeamHat(0f, 0f, tm);
                }
                return Error("Argument (" + pValue + ") should be the name of a team");
            }
            foreach (Type tp in Editor.ThingTypes)
            {
                if (tp.Name.ToLowerInvariant() == pValue)
                {
                    if (!Editor.HasConstructorParameter(tp))
                    {
                        return Error(tp.Name + " can not be spawned this way.");
                    }
                    if (!typeof(T).IsAssignableFrom(tp))
                    {
                        return Error("Wrong object type (requires " + typeof(T).Name + ").");
                    }
                    return Editor.CreateThing(tp) as T;
                }
            }
            return Error(typeof(T).Name + " of type (" + pValue + ") was not found.");
        }
    }

    public int commandQueueWait;

    public Func<bool> commandQueueWaitFunction;

    public bool hidden;

    public bool cheat;

    public string keyword;

    public List<string> aliases;

    public Argument[] arguments;

    public Action<CMD> action;

    public Action alternateAction;

    public CMD subcommand;

    public CMD parent;

    public int priority;

    public string description = "";

    public string logMessage;

    public string fullCommandName
    {
        get
        {
            if (parent != null)
            {
                return parent.fullCommandName + " " + keyword;
            }
            return keyword;
        }
    }

    public string info
    {
        get
        {
            string ret = keyword + "(";
            if (arguments != null)
            {
                bool first = true;
                Argument[] array = arguments;
                foreach (Argument arg in array)
                {
                    if (!first)
                    {
                        ret += ", ";
                    }
                    first = false;
                    ret = ret + arg.type.Name + " " + arg.name;
                }
            }
            ret += ")";
            if (description != "")
            {
                ret = ret + "\n|DGBLUE|" + description;
            }
            return ret;
        }
    }

    public bool HasArg<T>(string pName)
    {
        Argument arg = arguments.FirstOrDefault((Argument x) => x.name == pName);
        if (arg == null || arg.value == null)
        {
            return false;
        }
        return true;
    }

    public T Arg<T>(string pName)
    {
        Argument arg = arguments.FirstOrDefault((Argument x) => x.name == pName);
        if (arg != null && arg.value != null)
        {
            return (T)arg.value;
        }
        return default(T);
    }

    public CMD(string pKeyword, Argument[] pArguments, Action<CMD> pAction)
    {
        keyword = pKeyword.ToLowerInvariant();
        arguments = pArguments;
        action = pAction;
    }

    public CMD(string pKeyword, Action<CMD> pAction)
    {
        keyword = pKeyword.ToLowerInvariant();
        action = pAction;
    }

    public CMD(string pKeyword, Action pAction)
    {
        keyword = pKeyword.ToLowerInvariant();
        alternateAction = pAction;
    }

    public CMD(string pKeyword, CMD pSubCommand)
    {
        keyword = pKeyword.ToLowerInvariant();
        subcommand = pSubCommand;
        pSubCommand.parent = this;
        priority = -1;
    }

    public bool Run(string pArguments)
    {
        logMessage = null;
        string[] args = pArguments.Split(' ');
        if (subcommand != null)
        {
            return Error("|DGRED|Command (" + keyword + ") requires a sub command.");
        }
        if (args.Count() > 0 && args[0] == "?")
        {
            return Help(info);
        }
        if (args.Count() > 0 && args[0].Trim().Length > 0 && arguments == null)
        {
            return Error("|DGRED|Command (" + keyword + ") takes no arguments.");
        }
        if (arguments != null)
        {
            int parsingArgument = 0;
            int optionalIndex = -1;
            Argument[] array = arguments;
            foreach (Argument arg in array)
            {
                if (arg.optional)
                {
                    if (optionalIndex < 0)
                    {
                        optionalIndex = parsingArgument;
                    }
                }
                else if (optionalIndex >= 0)
                {
                    return Error("|DGRED|Command implementation error: 'optional' arguments must appear last.");
                }
                if (args.Count() > parsingArgument && !string.IsNullOrWhiteSpace(args[parsingArgument]))
                {
                    try
                    {
                        if (arguments[parsingArgument].takesMultispaceString)
                        {
                            string multiArg = "";
                            for (int j = parsingArgument; j < args.Length; j++)
                            {
                                multiArg = multiArg + args[j] + " ";
                            }
                            arg.value = arguments[parsingArgument].Parse(multiArg.Trim());
                            parsingArgument = args.Length;
                        }
                        else
                        {
                            arg.value = arguments[parsingArgument].Parse(args[parsingArgument]);
                        }
                        if (arg.value == null)
                        {
                            return Error("|DGRED|" + arg.GetParseFailedMessage() + " |GRAY|(" + arg.name + ")");
                        }
                    }
                    catch (Exception ex)
                    {
                        return Error("|DGRED|Error parsing argument (" + arg.name + "): " + ex.Message);
                    }
                }
                else if (!arg.optional)
                {
                    return Error("|DGRED|Missing argument (" + arg.name + ")");
                }
                parsingArgument++;
            }
        }
        try
        {
            if (action != null)
            {
                action(this);
            }
            else if (alternateAction != null)
            {
                alternateAction();
            }
        }
        catch (Exception ex2)
        {
            FinishExecution();
            return Error("|DGRED|Error running command: " + ex2.Message);
        }
        FinishExecution();
        return true;
    }

    private void FinishExecution()
    {
        if (arguments != null)
        {
            Argument[] array = arguments;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].value = null;
            }
        }
    }

    protected bool Error(string pError = null)
    {
        logMessage = "|DGRED|" + pError;
        return false;
    }

    protected bool Help(string pMessage = null)
    {
        logMessage = "|DGBLUE|" + pMessage;
        return true;
    }

    protected bool Message(string pMessage = null)
    {
        logMessage = pMessage;
        return true;
    }
}
