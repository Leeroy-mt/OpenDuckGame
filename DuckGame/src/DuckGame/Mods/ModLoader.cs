using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;

namespace DuckGame;

/// <summary>
/// The class that handles mods to load, and allows mods to retrieve Mod objects.
/// </summary>
public static class ModLoader
{
    private static readonly Dictionary<string, Mod> _loadedMods = new Dictionary<string, Mod>();

    internal static readonly Dictionary<string, Mod> _modAssemblyNames = new Dictionary<string, Mod>();

    internal static readonly Dictionary<Assembly, Mod> _modAssemblies = new Dictionary<Assembly, Mod>();

    internal static readonly Dictionary<uint, Mod> _modsByHash = new Dictionary<uint, Mod>();

    internal static readonly Dictionary<ulong, Mod> _modsByWorkshopID = new Dictionary<ulong, Mod>();

    private static Assembly[] _modAssemblyArray;

    internal static readonly Dictionary<Type, Mod> _modTypes = new Dictionary<Type, Mod>();

    private static IList<Mod> _sortedMods = new List<Mod>();

    private static IList<Mod> _sortedAccessibleMods = new List<Mod>();

    private static readonly List<Tuple<string, Exception>> _modLoadErrors = new List<Tuple<string, Exception>>();

    public static readonly Dictionary<string, Type> _typesByName = new Dictionary<string, Type>();

    public static readonly Dictionary<string, Type> _typesByNameUnprocessed = new Dictionary<string, Type>();

    private static readonly Dictionary<Type, string> _namesByType = new Dictionary<Type, string>();

    /// <summary>
    /// Returns whether or not any mods are present and not disabled.
    /// </summary>
    private static int _numModsEnabled = 0;

    private static int _numModsTotal = 0;

    private static bool _preloading = false;

    internal static string _modString;

    private static List<ulong> brokenclientsidemods = new List<ulong>
    {
        2291455300uL, 2285058623uL, 1704010547uL, 1422187117uL, 1217536842uL, 1337905266uL, 1653126591uL, 1220996500uL, 804666647uL, 1198406315uL,
        1356415641uL
    };

    private static CSharpCodeProvider _provider = null;

    private static CompilerParameters _parameters = null;

    private static string _buildErrorText = null;

    private static string _buildErrorFile = null;

    internal static HashSet<string> disabledMods;

    internal static HashSet<string> forceLegacyLoad;

    internal static bool forceRecompilation = false;

    internal static string modDirectory;

    private static Dictionary<string, ModConfiguration> loadableMods;

    private static List<Mod> removeFromAccessible = new List<Mod>();

    public static bool runningModloadCode;

    private static List<Mod> initializationFailures = new List<Mod>();

    public static bool ignoreLegacyLoad = false;

    public static Mod loadingOldMod = null;

    public static string currentModLoadString = "";

    public static Assembly[] modAssemblyArray => _modAssemblyArray;

    internal static List<Tuple<string, Exception>> modLoadErrors => _modLoadErrors;

    /// <summary>
    /// Get an iterable list of Mods
    /// </summary>
    public static IList<Mod> accessibleMods => _sortedAccessibleMods;

    public static IList<Mod> allMods => _sortedMods;

    public static int numModsEnabled => _numModsEnabled;

    public static bool modsEnabled => _numModsEnabled != 0;

    public static int numModsTotal => _numModsTotal;

    internal static string modHash { get; private set; }

    internal static string modConfigFile => modDirectory + "/mods.conf";

    public static void InitializeAssemblyArray()
    {
        if (_modAssemblyArray != null)
        {
            return;
        }
        HashSet<Assembly> assemblyList = new HashSet<Assembly>();
        assemblyList.Add(Assembly.GetExecutingAssembly());
        foreach (KeyValuePair<Assembly, Mod> modAssembly in _modAssemblies)
        {
            assemblyList.Add(modAssembly.Key);
        }
        _modAssemblyArray = assemblyList.ToArray();
    }

    internal static void AddMod(object p)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a loaded Mod instance from its unique name.
    /// </summary>
    /// <typeparam name="T">The special Mod subclass to cast to.</typeparam>
    /// <returns>The Mod instance, or null.</returns>
    public static T GetMod<T>() where T : Mod
    {
        Mod mod = null;
        _modTypes.TryGetValue(typeof(T), out mod);
        return (T)mod;
    }

    /// <summary>
    /// Get a loaded Mod instance from its unique name.
    /// </summary>
    /// <param name="name">The name of the mod.</param>
    /// <returns>The Mod instance, or null.</returns>
    public static Mod GetMod(string name)
    {
        if (_loadedMods.TryGetValue(name, out var m))
        {
            return m;
        }
        return null;
    }

    public static void DisableModsAndRestart()
    {
        foreach (Mod allMod in allMods)
        {
            allMod.configuration.Disable();
        }
        RestartGame();
    }

    public static void RestartGame()
    {
        Process.Start(Environment.ProcessPath, Program.commandLine);
        MonoMain.instance.Exit();
    }

    internal static void AddMod(Mod mod)
    {
        if (_loadedMods.ContainsKey(mod.configuration.uniqueID))
        {
            return;
        }
        _loadedMods.Add(mod.configuration.uniqueID, mod);
        if (!mod.configuration.disabled && !(mod is ErrorMod) && !_modAssemblies.ContainsKey(mod.configuration.assembly))
        {
            _modAssemblyNames.Add(mod.configuration.assembly.FullName, mod);
            _modAssemblies.Add(mod.configuration.assembly, mod);
            _modsByHash.Add(mod.identifierHash, mod);
            if (mod.configuration.workshopID != 0L)
            {
                _modsByWorkshopID[mod.configuration.workshopID] = mod;
            }
            _modTypes.Add(mod.GetType(), mod);
        }
    }

    private static ModConfiguration GetDependency(string pDependency, ref Dictionary<string, ModConfiguration> pLoadableMods)
    {
        if (!pLoadableMods.TryGetValue(pDependency, out var modDependencyConfig))
        {
            foreach (KeyValuePair<string, ModConfiguration> pair in pLoadableMods)
            {
                if (pair.Key.StartsWith(pDependency))
                {
                    return pair.Value;
                }
            }
        }
        return modDependencyConfig;
    }

    private static Mod GetOrLoad(ModConfiguration modConfig, ref Stack<string> modLoadStack, ref Dictionary<string, ModConfiguration> loadableMods)
    {
        if (modLoadStack.Contains(modConfig.uniqueID))
        {
            throw new ModCircularDependencyException(modLoadStack);
        }
        modLoadStack.Push(modConfig.uniqueID);
        try
        {
            if (_loadedMods.TryGetValue(modConfig.uniqueID, out var mod))
            {
                return mod;
            }
            if (_preloading)
            {
                if (DGSave.upgradingFromVanilla)
                {
                    modConfig.Disable();
                    DGSave.showModsDisabledMessage = true;
                }
                if (modConfig.error != null)
                {
                    mod = new ErrorMod();
                }
                else if (MonoMain.nomodsMode)
                {
                    mod = new ErrorMod();
                    modConfig.error = "-nomods command line enabled";
                }
                else if (modConfig.majorSupportedRevision != 1)
                {
                    if (modConfig.workshopID == 1198406315 || modConfig.workshopID == 1354346379)
                    {
                        modConfig.Disable();
                        modConfig.error = "!This mod has been officially implemented, Thanks EIM64!";
                        mod = new DisabledMod();
                    }
                    if (modConfig.workshopID == 1820667892)
                    {
                        modConfig.Disable();
                        modConfig.error = "!This mod has been officially implemented, Thanks Yupdaniel!";
                        mod = new DisabledMod();
                    }
                    if (modConfig.workshopID == 1603886916)
                    {
                        modConfig.Disable();
                        modConfig.error = "!This mod has been officially implemented, Thanks Yupdaniel || Mr. Potatooh!";
                        mod = new DisabledMod();
                    }
                    if (modConfig.workshopID == 796033146)
                    {
                        modConfig.Disable();
                        modConfig.error = "!This mod has been officially implemented, Thanks TheSpicyChef!";
                        mod = new DisabledMod();
                    }
                    if (modConfig.workshopID == 1425615438)
                    {
                        modConfig.Disable();
                        modConfig.error = "!This mod has been officially implemented, Thanks EIM64 || Killer-Fackur!";
                        mod = new DisabledMod();
                    }
                    if (modConfig.workshopID == 1704010547)
                    {
                        modConfig.Disable();
                        modConfig.error = "!Regrettably, this version of QOL is incompatible with Duck Game 2020!";
                        mod = new DisabledMod();
                    }
                }
                else if (modConfig.workshopID == 1657985708)
                {
                    modConfig.Disable();
                    modConfig.error = "!This mod has been officially implemented, Thanks Yupdaniel!";
                    mod = new DisabledMod();
                }
                if (Program.isLinux && !modConfig.linuxFix && modConfig.workshopID == 1439906266)
                {
                    modConfig.Disable();
                    modConfig.error = "!This mod does not currently work on Linux!";
                    mod = new DisabledMod();
                }
            }
            if (mod == null)
            {
                if (modConfig.disabled)
                {
                    mod = new DisabledMod();
                }
                else if (modConfig.modType == ModConfiguration.Type.Reskin)
                {
                    //MonoMain.loadMessage = "LOADING RESKIN " + currentModLoadString;
                    mod = ReskinPack.LoadReskin(modConfig.directory, null, modConfig);
                }
                else if (modConfig.modType == ModConfiguration.Type.MapPack)
                {
                    //MonoMain.loadMessage = "LOADING MAPPACK " + currentModLoadString;
                    mod = MapPack.LoadMapPack(modConfig.directory, null, modConfig);
                }
                else if (!_preloading)
                {
                    //MonoMain.loadMessage = "LOADING MOD " + currentModLoadString;
                    try
                    {
                        foreach (string dependency in modConfig.hardDependencies)
                        {
                            ModConfiguration obj = GetDependency(dependency, ref loadableMods) ?? throw new ModDependencyNotFoundException(modConfig.uniqueID, dependency);
                            if (obj.disabled)
                            {
                                throw new ModDependencyNotFoundException(modConfig.uniqueID, dependency);
                            }
                            GetOrLoad(obj, ref modLoadStack, ref loadableMods);
                        }
                        foreach (string softDependency in modConfig.softDependencies)
                        {
                            ModConfiguration modDependencyConfig = GetDependency(softDependency, ref loadableMods);
                            if (modDependencyConfig != null && !modDependencyConfig.disabled)
                            {
                                GetOrLoad(modDependencyConfig, ref modLoadStack, ref loadableMods);
                            }
                        }
                        modConfig.assembly = Assembly.Load(File.ReadAllBytes(modConfig.isDynamic ? modConfig.tempAssemblyPath : modConfig.assemblyPath));
                        MonoMain.loadedModsWithAssemblies.Add(modConfig);
                        Type[] contentManagerType = (from type in modConfig.assembly.GetExportedTypes()
                                                     where type.IsSubclassOf(typeof(IManageContent)) && type.IsPublic && type.IsClass && !type.IsAbstract
                                                     select type).ToArray();
                        if (contentManagerType.Length > 1)
                        {
                            throw new ModTypeMissingException(modConfig.uniqueID + " has more than one content manager class");
                        }
                        modConfig.contentManager = ContentManagers.GetContentManager((contentManagerType.Length == 1) ? contentManagerType[0] : null);
                        Type[] array = (from type in modConfig.assembly.GetExportedTypes()
                                        where type.IsSubclassOf(typeof(Mod)) && !type.IsAbstract
                                        select type).ToArray();
                        if (array.Length != 1)
                        {
                            throw new ModTypeMissingException(modConfig.uniqueID + " is missing or has more than one Mod subclass");
                        }
                        if (MonoMain.preloadModContent && modConfig.preloadContent)
                        {
                            modConfig.content.PreloadContent();
                        }
                        else
                        {
                            modConfig.content.PreloadContentPaths();
                        }
                        mod = (Mod)Activator.CreateInstance(array[0]);
                        if (mod is DisabledMod || mod is CoreMod || mod is ErrorMod)
                        {
                            mod.clientMod = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        mod = new ErrorMod();
                        modConfig.error = ex.ToString();
                    }
                }
            }
            if (mod == null)
            {
                return null;
            }
            mod.configuration = modConfig;
            AddMod(mod);
            return mod;
        }
        finally
        {
            modLoadStack.Pop();
        }
    }

    private static string GetModHash()
    {
        if (!modsEnabled)
        {
            return "nomods";
        }
        using SHA256 sha = SHA256.Create();
        IEnumerable<string> parts = from a in _sortedAccessibleMods
                                    where !(a is CoreMod) && !(a is DisabledMod) && !brokenclientsidemods.Contains(a.configuration.workshopID)
                                    select a.configuration.uniqueID + "_" + a.configuration.version;
        _modString = string.Join("|", parts.OrderBy((string x) => x));
        if (string.IsNullOrEmpty(_modString))
        {
            return "nomods";
        }
        return Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(_modString)));
    }

    public static string SmallTypeName(string fullName)
    {
        int index = fullName.IndexOf(',', fullName.IndexOf(',') + 1);
        if (index == -1)
        {
            return null;
        }
        return fullName.Substring(0, index);
    }

    internal static string SmallTypeName(Type type)
    {
        if (!MonoMain.moddingEnabled)
        {
            return SmallTypeName(type.AssemblyQualifiedName);
        }
        string s = null;
        _namesByType.TryGetValue(type, out s);
        return s;
    }

    private static bool AttemptCompile(ModConfiguration config)
    {
        _buildErrorText = null;
        _buildErrorFile = null;
        if (config.noCompilation)
        {
            return false;
        }
        List<string> csFiles = DuckFile.GetFilesNoCloud(config.directory, "*.cs", SearchOption.AllDirectories);
        if (csFiles.Count == 0)
        {
            return false;
        }
        config.isDynamic = true;
        CRC32 newCrcProcessor = new CRC32();
        byte[] buffer = new byte[2048];
        foreach (string item in csFiles)
        {
            using FileStream fileStream = File.Open(item, FileMode.Open);
            while (fileStream.Position != fileStream.Length)
            {
                int len = fileStream.Read(buffer, 0, buffer.Length);
                newCrcProcessor.ProcessBlock(buffer, len);
            }
        }
        uint newCrc = newCrcProcessor.Finalize();
        if (!forceRecompilation && File.Exists(config.hashPath) && File.Exists(config.tempAssemblyPath))
        {
            try
            {
                if (BitConverter.ToUInt32(File.ReadAllBytes(config.hashPath), 0) == newCrc)
                {
                    return true;
                }
            }
            catch
            {
            }
        }
        File.WriteAllBytes(config.hashPath, BitConverter.GetBytes(newCrc));
        if (_provider == null)
        {
            _provider = new CSharpCodeProvider();
            _parameters = new CompilerParameters((from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                                  select assembly.Location).ToArray());
            CompilerParameters parameters = _parameters;
            bool generateExecutable = (_parameters.GenerateInMemory = false);
            parameters.GenerateExecutable = generateExecutable;
        }
        if (File.Exists(config.buildLogPath))
        {
            File.SetAttributes(config.buildLogPath, FileAttributes.Normal);
            File.Delete(config.buildLogPath);
        }
        _parameters.OutputAssembly = config.tempAssemblyPath;
        CompilerResults results = _provider.CompileAssemblyFromFile(_parameters, csFiles.ToArray());
        if (results.Errors.Count != 0)
        {
            bool foundNonWarningError = false;
            foreach (CompilerError e in results.Errors)
            {
                if (!e.IsWarning)
                {
                    foundNonWarningError = true;
                    _buildErrorFile = DuckFile.PreparePath(e.FileName);
                    _buildErrorText = e.ErrorText;
                    break;
                }
            }
            File.WriteAllLines(config.buildLogPath, results.Output.OfType<string>());
            if (foundNonWarningError)
            {
                return false;
            }
        }
        return true;
    }

    private static ModConfiguration AttemptModLoad(string folder)
    {
        ModConfiguration config = new ModConfiguration();
        config.directory = folder;
        config.contentDirectory = config.directory + "/content/";
        if (File.Exists(config.contentDirectory + "/New Text Document.tpconf"))
        {
            config.isExistingReskinMod = true;
        }
        else if (File.Exists(folder + "/info.txt") && DuckFile.GetFiles(folder, "*.dll").Count() == 0)
        {
            config.SetModType(ModConfiguration.Type.Reskin);
        }
        else if (File.Exists(folder + "/mappack_info.txt") && DuckFile.GetFiles(folder, "*.dll").Count() == 0)
        {
            config.SetModType(ModConfiguration.Type.MapPack);
        }
        else if (File.Exists(folder + "/hatpack_info.txt") && DuckFile.GetFiles(folder, "*.dll").Count() == 0)
        {
            config.SetModType(ModConfiguration.Type.HatPack);
        }
        config.name = Path.GetFileNameWithoutExtension(folder);
        config.content = new ContentPack(config);
        try
        {
            if (config.name == "DuckGame")
            {
                return null;
            }
            currentModLoadString = config.name;
            config.LoadConfiguration();
            if (!config.disabled && config.modType == ModConfiguration.Type.Mod)
            {
                if (!File.Exists(config.assemblyPath) && !MonoMain.nomodsMode)
                {
                    //MonoMain.loadMessage = "COMPILING MOD " + currentModLoadString;
                    if (!AttemptCompile(config))
                    {
                        config.error = _buildErrorText + "\n\nFile: " + _buildErrorFile + "\nNote: Assembly (" + Path.GetFileName(config.assemblyPath) + ") was not found, and a compile was attempted.";
                    }
                }
                if (config.error == null)
                {
                    _numModsEnabled++;
                }
            }
            _numModsTotal++;
            return config;
        }
        catch (Exception item)
        {
            _modLoadErrors.Add(Tuple.Create(config.uniqueID, item));
        }
        return null;
    }

    internal static void LoadConfig()
    {
        XmlDocument doc = null;
        XmlElement root = null;
        bool fileExists = File.Exists(modConfigFile);
        if (fileExists)
        {
            try
            {
                doc = DuckFile.LoadSharpXML(modConfigFile);
                root = doc["Mods"];
            }
            catch (Exception)
            {
                LogModFailure("Failure loading main mod config file. Recreating file.");
                fileExists = false;
            }
        }
        if (!fileExists)
        {
            doc = new XmlDocument();
            root = doc.CreateElement("Mods");
            root.AppendChild(doc.CreateElement("Disabled"));
            root.AppendChild(doc.CreateElement("ForceLegacyLoad"));
            root.AppendChild(doc.CreateElement("CompiledFor"));
            root["CompiledFor"].InnerText = DG.version;
            doc.AppendChild(root);
            DuckFile.SaveSharpXML(doc, modConfigFile);
        }
        if (root["Disabled"] != null)
        {
            disabledMods = new HashSet<string>(from a in root["Disabled"].InnerText.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                                               select a.Trim());
        }
        else
        {
            disabledMods = new HashSet<string>();
        }
        if (root["ForceLegacyLoad"] != null)
        {
            forceLegacyLoad = new HashSet<string>(from a in root["ForceLegacyLoad"].InnerText.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                                                  select a.Trim());
        }
        else
        {
            forceLegacyLoad = new HashSet<string>();
        }
        if (root["CompiledFor"] == null)
        {
            root.AppendChild(doc.CreateElement("CompiledFor"));
        }
        if (root["CompiledFor"].InnerText != DG.version)
        {
            forceRecompilation = true;
            root["CompiledFor"].InnerText = DG.version;
            DuckFile.SaveSharpXML(doc, modConfigFile);
        }
    }

    internal static void SetModDisabled(ModConfiguration pMod, bool pDisabled)
    {
        XmlDocument doc = DuckFile.LoadSharpXML(modConfigFile);
        XmlElement root = doc["Mods"];
        if (root["Disabled"] == null)
        {
            root.AppendChild(doc.CreateElement("Disabled"));
        }
        string modID = pMod.uniqueID;
        List<string> disabledMods = root["Disabled"].InnerText.Split('|').ToList();
        if (!pDisabled)
        {
            string[] array = modID.Split('|');
            foreach (string s in array)
            {
                disabledMods.Remove(s);
            }
        }
        else
        {
            string[] array = modID.Split('|');
            foreach (string s2 in array)
            {
                if (!disabledMods.Contains(s2))
                {
                    disabledMods.Add(s2);
                }
            }
        }
        root["Disabled"].InnerText = string.Join("|", disabledMods);
        if (root["ForceLegacyLoad"] == null)
        {
            root.AppendChild(doc.CreateElement("ForceLegacyLoad"));
        }
        root["ForceLegacyLoad"].InnerText = string.Join("|", from a in _sortedMods
                                                             where a.configuration.forceHarmonyLegacyLoad
                                                             select a.configuration.uniqueID);
        DuckFile.SaveSharpXML(doc, modConfigFile);
    }

    internal static void DisabledModsChanged()
    {
        XmlDocument doc = DuckFile.LoadSharpXML(modConfigFile);
        XmlElement root = doc["Mods"];
        if (root["Disabled"] == null)
        {
            root.AppendChild(doc.CreateElement("Disabled"));
        }
        root["Disabled"].InnerText = string.Join("|", from a in _sortedMods
                                                      where a.configuration.disabled
                                                      select a.configuration.uniqueID);
        if (root["ForceLegacyLoad"] == null)
        {
            root.AppendChild(doc.CreateElement("ForceLegacyLoad"));
        }
        root["ForceLegacyLoad"].InnerText = string.Join("|", from a in _sortedMods
                                                             where a.configuration.forceHarmonyLegacyLoad
                                                             select a.configuration.uniqueID);
        DuckFile.SaveSharpXML(doc, modConfigFile);
    }

    private static void ResultFetched(object value0, WorkshopQueryResult result)
    {
        if (result == null || result.details == null)
        {
            return;
        }
        WorkshopItem item = result.details.publishedFile;
        if (item == null)
        {
            return;
        }
        try
        {
            if ((item.stateFlags & WorkshopItemState.Installed) == 0 || !Directory.Exists(item.path))
            {
                return;
            }
            foreach (string item2 in DuckFile.GetDirectoriesNoCloud(item.path))
            {
                ModConfiguration config = AttemptModLoad(item2);
                if (config != null)
                {
                    try
                    {
                        config.isWorkshop = true;
                        loadableMods.Add(config.uniqueID, config);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        catch (Exception)
        {
        }
    }

    public static void FailWithHarmonyException()
    {
        if (loadingOldMod != null)
        {
            if (loadingOldMod.configuration.forceHarmonyLegacyLoad && !ignoreLegacyLoad)
            {
                return;
            }
            throw new OldModUsesHarmonyException("Mod is for an old version of Duck Game, and appears to use Harmony patching. This could be risky! Use 'Force Legacy Load' and restart to load it anyway.");
        }
        throw new OldModUsesHarmonyException("A Mod for the old version of Duck Game uses Harmony. This could be risky! Use 'Force Legacy Load' and restart to load it anyway.");
    }

    public static void PreLoadMods(string dir)
    {
        modDirectory = dir;
        LoadConfig();
        loadableMods = new Dictionary<string, ModConfiguration>();
        if (Directory.Exists(modDirectory))
        {
            if (Steam.IsInitialized())
            {
                LoadingAction steamLoad = new LoadingAction();
                steamLoad.action = delegate
                {
                    runningModloadCode = true;
                    WorkshopQueryUser workshopQueryUser = Steam.CreateQueryUser(Steam.user.id, WorkshopList.Subscribed, WorkshopType.UsableInGame, WorkshopSortOrder.TitleAsc);
                    workshopQueryUser.requiredTags.Add("Mod");
                    workshopQueryUser.onlyQueryIDs = true;
                    workshopQueryUser.QueryFinished += delegate
                    {
                        steamLoad.flag = true;
                    };
                    workshopQueryUser.ResultFetched += ResultFetched;
                    workshopQueryUser.Request();
                    Steam.Update();
                };
                steamLoad.waitAction = delegate
                {
                    Steam.Update();
                    return steamLoad.flag;
                };
                MonoMain.currentActionQueue.Enqueue(steamLoad);
            }
            LoadingAction attemptLoadMods = new LoadingAction();
            MonoMain.currentActionQueue.Enqueue(attemptLoadMods);
            attemptLoadMods.action = delegate
            {
                runningModloadCode = true;
                List<string> directoriesNoCloud = DuckFile.GetDirectoriesNoCloud(modDirectory);
                directoriesNoCloud.AddRange(DuckFile.GetDirectoriesNoCloud(DuckFile.globalModsDirectory));
                foreach (string folder in directoriesNoCloud)
                {
                    if (!folder.ToLowerInvariant().EndsWith("/texpacks") && !folder.ToLowerInvariant().EndsWith("/mappacks") && !folder.ToLowerInvariant().EndsWith("/hatpacks"))
                    {
                        attemptLoadMods.actions.Enqueue(new LoadingAction(delegate
                        {
                            ModConfiguration modConfiguration = AttemptModLoad(folder);
                            if (modConfiguration != null)
                            {
                                if (loadableMods.ContainsKey(modConfiguration.uniqueID))
                                {
                                    if (!loadableMods[modConfiguration.uniqueID].disabled || modConfiguration.disabled)
                                    {
                                        return;
                                    }
                                    loadableMods.Remove(modConfiguration.uniqueID);
                                }
                                loadableMods.Add(modConfiguration.uniqueID, modConfiguration);
                            }
                        }));
                    }
                }
            };
        }
        MonoMain.currentActionQueue.Enqueue(new LoadingAction(delegate
        {
            ReskinPack.InitializeReskins();
        }));
        MonoMain.currentActionQueue.Enqueue(new LoadingAction(delegate
        {
            MapPack.InitializeMapPacks();
        }));
        GetOrLoadMods(pPreload: true);
    }

    private static void GetOrLoadMods(bool pPreload)
    {
        Stack<string> modLoadStack = new Stack<string>();
        LoadingAction getOrLoadMods = new LoadingAction();
        MonoMain.currentActionQueue.Enqueue(getOrLoadMods);
        getOrLoadMods.action = delegate
        {
            _preloading = pPreload;
            int cluster = 0;
            _ = ReskinPack.active;
            foreach (ModConfiguration loadable in loadableMods.Values)
            {
                getOrLoadMods.actions.Enqueue(new LoadingAction(delegate
                {
                    try
                    {
                        currentModLoadString = loadable.name;
                        Mod orLoad = GetOrLoad(loadable, ref modLoadStack, ref loadableMods);
                        if (orLoad != null && loadable.isExistingReskinMod && !loadable.disabled && !(orLoad is ErrorMod))
                        {
                            ReskinPack.LoadReskin(loadable.contentDirectory + "tp/", orLoad);
                        }
                        cluster++;
                        if (cluster == 10)
                        {
                            cluster = 0;
                            Thread.Sleep(50);
                        }
                    }
                    catch (Exception)
                    {
                        if (Options.Data.disableModOnLoadFailure)
                        {
                            loadable.Disable();
                        }
                    }
                }));
            }
        };
    }

    internal static void LoadMods(string dir)
    {
        GetOrLoadMods(pPreload: false);
        MonoMain.currentActionQueue.Enqueue(new LoadingAction(delegate
        {
            InitializeAssemblyArray();
            ReskinPack.FinalizeReskins();
            _sortedMods = _loadedMods.Values.OrderBy((Mod mod) => (int)(mod.priority + ((!mod.configuration.disabled || mod is ErrorMod) ? (-1000) : 0))).ToList();
            _sortedAccessibleMods = _sortedMods.Where((Mod mod) => !mod.configuration.disabled && !(mod is ErrorMod)).ToList();
            foreach (Mod current in _sortedMods.Where((Mod a) => a.configuration.disabled || a is ErrorMod))
            {
                if (current != null && current.configuration != null)
                {
                    _loadedMods.Remove(current.configuration.uniqueID);
                }
            }
        }));
        LoadingAction preInitializeMods = new LoadingAction();
        MonoMain.currentActionQueue.Enqueue(preInitializeMods);
        preInitializeMods.action = delegate
        {
            foreach (Mod mod in _sortedAccessibleMods)
            {
                preInitializeMods.actions.Enqueue(new LoadingAction(delegate
                {
                    try
                    {
                        AssemblyName assemblyName = mod.GetType().Assembly.GetReferencedAssemblies().FirstOrDefault((AssemblyName x) => x.Name == "DuckGame");
                        if (assemblyName != null && assemblyName.Version.Minor != DG.versionMajor)
                        {
                            loadingOldMod = mod;
                            if (Directory.GetFiles(mod.configuration.directory, "0Harmony.dll", SearchOption.AllDirectories).FirstOrDefault() != null)
                            {
                                FailWithHarmonyException();
                            }
                        }
                        mod.InvokeOnPreInitialize();
                    }
                    catch (Exception ex)
                    {
                        mod.configuration.error = ex.ToString();
                        if (Options.Data.disableModOnLoadFailure)
                        {
                            mod.configuration.Disable();
                        }
                        if (MonoMain.modDebugging)
                        {
                            throw new ModException(mod.configuration.name + " OnPreInitialize failed with exception:", mod.configuration, ex);
                        }
                    }
                    loadingOldMod = null;
                }));
            }
        };
        MonoMain.currentActionQueue.Enqueue(new LoadingAction(delegate
        {
            foreach (Mod current in initializationFailures)
            {
                _sortedAccessibleMods.Remove(current);
            }
            modHash = GetModHash();
            foreach (Mod current2 in _sortedAccessibleMods)
            {
                string[] array = null;
                if (current2.namespaceFacade != null)
                {
                    try
                    {
                        array = current2.namespaceFacade.Split(':');
                        array[0] = array[0].Trim();
                        array[1] = array[1].Trim();
                    }
                    catch (Exception)
                    {
                        array = null;
                    }
                }
                try
                {
                    Type[] types = current2.configuration.assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        string text = "";
                        text = SmallTypeName(type.AssemblyQualifiedName);
                        _typesByNameUnprocessed[text] = type;
                        if (Program.isLinux && !text.Contains("\""))
                        {
                            string text2 = text.Insert(text.IndexOf(", ") + 2, "\"");
                            text2 += "\"";
                            _typesByNameUnprocessed[text2] = type;
                        }
                        if ((current2.assemblyNameFacade != null || current2.namespaceFacade != null) && type.Namespace != null)
                        {
                            int num = text.IndexOf(',');
                            string text3 = text.Substring(num + 2, text.Length - (num + 2));
                            string text4 = "";
                            if (array != null)
                            {
                                text4 = type.Namespace.Replace(array[0], array[1]) + "." + type.Name;
                            }
                            if (current2.assemblyNameFacade != null)
                            {
                                text3 = current2.assemblyNameFacade;
                            }
                            text = text4 + ", " + text3;
                        }
                        _typesByName[text] = type;
                        if (Program.isLinux && !text.Contains("\""))
                        {
                            string text5 = text.Insert(text.IndexOf(", ") + 2, "\"");
                            text5 += "\"";
                            _typesByName[text5] = type;
                        }
                        _namesByType[type] = text;
                    }
                }
                catch (Exception ex2)
                {
                    if (ex2 is ReflectionTypeLoadException)
                    {
                        DevConsole.Log(DCSection.General, "ModLoader.Load Assembly.GetAssemblies crashed with above exception-");
                        throw (ex2 as ReflectionTypeLoadException).LoaderExceptions.FirstOrDefault();
                    }
                }
            }
            runningModloadCode = false;
        }));
    }

    private static void LogModFailure(string s)
    {
        try
        {
            Program.LogLine("Mod Load Failure (Did not cause crash)\n================================================\n " + s + "\n================================================\n");
        }
        catch (Exception)
        {
        }
    }

    internal static void PostLoadMods()
    {
        runningModloadCode = true;
        foreach (Mod mod in _sortedAccessibleMods)
        {
            try
            {
                mod.InvokeOnPostInitialize();
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException && (ex as FileNotFoundException).FileName.StartsWith("Steam,"))
                {
                    continue;
                }
                mod.configuration.error = ex.ToString();
                if (Options.Data.disableModOnLoadFailure)
                {
                    mod.configuration.Disable();
                }
                throw new ModException(mod.configuration.name + " Mod.OnPostInitialize failed with exception:", mod.configuration, ex);
            }
        }
        foreach (Mod m in removeFromAccessible)
        {
            _sortedAccessibleMods.Remove(m);
        }
        if (!modsEnabled)
        {
            modHash = "nomods";
        }
        runningModloadCode = false;
    }

    internal static void Start()
    {
        List<Mod> removeFromAccessible = new List<Mod>();
        foreach (Mod mod in _sortedAccessibleMods)
        {
            try
            {
                mod.InvokeStart();
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException && (ex as FileNotFoundException).FileName.StartsWith("Steam,"))
                {
                    continue;
                }
                mod.configuration.error = ex.ToString();
                if (Options.Data.disableModOnLoadFailure)
                {
                    mod.configuration.Disable();
                }
                throw new ModException(mod.configuration.name + " Mod.InvokeStart failed with exception:", mod.configuration, ex);
            }
        }
        foreach (Mod m in removeFromAccessible)
        {
            _sortedAccessibleMods.Remove(m);
        }
        if (!modsEnabled)
        {
            modHash = "nomods";
        }
    }

    /// <summary>
    /// Searches core and mods for a fully qualified or short type name.
    /// </summary>
    /// <param name="typeName">Fully qualified, or short, name of the type.</param>
    /// <returns>The type, or null.</returns>
    internal static Type GetType(string typeName)
    {
        if (typeName == null)
        {
            return null;
        }
        if (typeName.LastIndexOf(',') != typeName.IndexOf(','))
        {
            typeName = SmallTypeName(typeName);
        }
        if (typeName == null)
        {
            return null;
        }
        if (_typesByName.TryGetValue(typeName, out var type))
        {
            return type;
        }
        if (_typesByNameUnprocessed.TryGetValue(typeName, out type))
        {
            return type;
        }
        return Type.GetType(typeName);
    }

    /// <summary>
    /// Gets a mod from a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The mod</returns>
    public static Mod GetModFromType(Type type)
    {
        if (type == null)
        {
            return null;
        }
        if (_modAssemblies.TryGetValue(type.Assembly, out var mod))
        {
            return mod;
        }
        return null;
    }

    /// <summary>
    /// Gets a mod from a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The mod</returns>
    public static Mod GetModFromTypeIgnoreCore(Type type)
    {
        if (type == null)
        {
            return null;
        }
        if (_modAssemblies.TryGetValue(type.Assembly, out var mod))
        {
            if (mod is CoreMod)
            {
                return null;
            }
            return mod;
        }
        return null;
    }

    /// <summary>
    /// Gets a mod from a hash value.
    /// </summary>
    /// <param name="pHash">A 'Mod.identifierHash' value.</param>
    /// <returns>The mod</returns>
    public static Mod GetModFromHash(uint pHash)
    {
        _modsByHash.TryGetValue(pHash, out var mod);
        return mod;
    }

    /// <summary>
    /// Gets a mod from a workshopID value.
    /// </summary>
    /// <param name="pHash">A 'Mod.configuration.workshopID' value.</param>
    /// <returns>The mod</returns>
    public static Mod GetModFromWorkshopID(ulong pID)
    {
        _modsByWorkshopID.TryGetValue(pID, out var mod);
        return mod;
    }
}
