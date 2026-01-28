using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace DuckGame;

public class DuckFile
{
    private static string _saveRoot;

    private static string _saveDirectory;

    private static string _levelDirectory;

    private static string _editorPreviewDirectory;

    private static string _workshopDirectory;

    private static string _onlineLevelDirectory;

    private static string _albumDirectory;

    private static string _profileDirectory;

    private static string _optionsDirectory;

    private static string _modsDirectory;

    private static string _challengeDirectory;

    private static string _scriptsDirectory;

    private static string _skinsDirectory;

    private static string _mappackDirectory;

    private static string _hatpackDirectory;

    private static string _customBlockDirectory;

    private static string _downloadedBlockDirectory;

    private static string _customBackgroundDirectory;

    private static string _downloadedBackgroundDirectory;

    private static string _customPlatformDirectory;

    private static string _downloadedPlatformDirectory;

    private static string _customParallaxDirectory;

    private static string _downloadedParallaxDirectory;

    private static string _customArcadeDirectory;

    private static string _customMojiDirectory;

    private static string _logDirectory;

    private static string _contentDirectory = "Content/";

    private static string _musicDirectory;

    private static List<string> _allPaths = new List<string>();

    private static Dictionary<char, string> _invalidPathCharConversions = new Dictionary<char, string>
    {
        { '/', "!53029662!" },
        { '\\', "!52024921!" },
        { '?', "!54030923!" },
        { '%', "!50395932!" },
        { '*', "!31040256!" },
        { ':', "!40205341!" },
        { '|', "!95302943!" },
        { '"', "!41302950!" },
        { '<', "!21493928!" },
        { '>', "!95828381!" },
        { '.', "!34910294!" }
    };

    public static bool freshInstall;

    public static bool appdataSave = false;

    public static bool _flaggedForBackup = false;

    public static bool mojimode = true;

    private static Dictionary<string, Sprite> _mojis = new Dictionary<string, Sprite>();

    private static Dictionary<string, Dictionary<string, Sprite>> _profileMojis = new Dictionary<string, Dictionary<string, Sprite>>();

    private static int waitTry = 0;

    private static bool _suppressCommit = false;

    private static Dictionary<string, LevelData> _levelCache = new Dictionary<string, LevelData>();

    private static Dictionary<uint, string> _conversionGUIDMap = new Dictionary<uint, string>();

    public static volatile bool LegacyLoadLock = false;

    public static string saveRoot => _saveRoot;

    public static string saveDirectory => _saveRoot + _saveDirectory;

    public static string levelDirectory => saveDirectory + _levelDirectory;

    public static string cloudLevelDirectory => userDirectory + _levelDirectory;

    public static string editorPreviewDirectory => saveDirectory + _editorPreviewDirectory;

    public static string workshopDirectory => saveDirectory + _workshopDirectory;

    public static string onlineLevelDirectory => saveDirectory + _onlineLevelDirectory;

    public static string albumDirectory => saveDirectory + _albumDirectory;

    public static string profileDirectory => userDirectory + _profileDirectory;

    public static string optionsDirectory
    {
        get
        {
            string ret = userDirectory + _optionsDirectory;
            if (!DirectoryExists(ret) && !MonoMain.atPostCloudLogic)
            {
                return globalOptionsDirectory;
            }
            return ret;
        }
    }

    public static string modsDirectory => userDirectory + _modsDirectory;

    public static string globalProfileDirectory => saveDirectory + _profileDirectory;

    public static string globalOptionsDirectory => saveDirectory + _optionsDirectory;

    public static string globalModsDirectory => saveDirectory + _modsDirectory;

    public static string globalSkinsDirectory => globalModsDirectory + _skinsDirectory;

    public static string globalMappackDirectory => globalModsDirectory + _mappackDirectory;

    public static string globalHatpackDirectory => globalModsDirectory + _hatpackDirectory;

    public static string challengeDirectory => saveDirectory + _challengeDirectory;

    public static string scriptsDirectory => saveDirectory + _scriptsDirectory;

    public static string skinsDirectory => modsDirectory + _skinsDirectory;

    public static string mappackDirectory => modsDirectory + _mappackDirectory;

    public static string hatpackDirectory => modsDirectory + _hatpackDirectory;

    public static string userDirectory
    {
        get
        {
            if (Steam.user != null)
            {
                return saveDirectory + Steam.user.id + "/";
            }
            return saveDirectory;
        }
    }

    public static string customBlockDirectory => saveDirectory + _customBlockDirectory;

    public static string downloadedBlockDirectory => saveDirectory + _downloadedBlockDirectory;

    public static string customBackgroundDirectory => saveDirectory + _customBackgroundDirectory;

    public static string downloadedBackgroundDirectory => saveDirectory + _downloadedBackgroundDirectory;

    public static string customPlatformDirectory => saveDirectory + _customPlatformDirectory;

    public static string downloadedPlatformDirectory => saveDirectory + _downloadedPlatformDirectory;

    public static string customParallaxDirectory => saveDirectory + _customParallaxDirectory;

    public static string downloadedParallaxDirectory => saveDirectory + _downloadedParallaxDirectory;

    public static string customArcadeDirectory => saveDirectory + _customArcadeDirectory;

    public static string customMojiDirectory => saveDirectory + _customMojiDirectory;

    public static string logDirectory => saveDirectory + _logDirectory;

    public static string contentDirectory => _contentDirectory;

    public static string musicDirectory => contentDirectory + _musicDirectory;

    public static string oldSaveLocation => (Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/").Replace('\\', '/');

    public static string newSaveLocation => (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/").Replace('\\', '/');

    public static Dictionary<string, Sprite> mojis => _mojis;

    public static string GetCustomDownloadDirectory(CustomType t)
    {
        return t switch
        {
            CustomType.Block => downloadedBlockDirectory,
            CustomType.Background => downloadedBackgroundDirectory,
            CustomType.Platform => downloadedPlatformDirectory,
            CustomType.Parallax => downloadedParallaxDirectory,
            _ => "",
        };
    }

    public static string GetCustomDirectory(CustomType t)
    {
        return t switch
        {
            CustomType.Block => customBlockDirectory,
            CustomType.Background => customBackgroundDirectory,
            CustomType.Platform => customPlatformDirectory,
            CustomType.Parallax => customParallaxDirectory,
            _ => "",
        };
    }

    public static void Initialize()
    {
        _saveDirectory = "DuckGame/";
        bool alternate = true;
        string oldSaveFolder = oldSaveLocation;
        _saveRoot = newSaveLocation;
        if (!DirectoryExists(_saveRoot + _saveDirectory) && !Program.alternateSaveLocation && DirectoryExists(oldSaveFolder + _saveDirectory))
        {
            _saveRoot = oldSaveFolder;
            alternate = false;
            appdataSave = false;
        }
        if (alternate)
        {
            appdataSave = true;
            try
            {
                oldSaveFolder += _saveDirectory;
                if (Program.alternateSaveLocation && DirectoryExists(oldSaveFolder) && !DirectoryExists(saveDirectory))
                {
                    DirectoryCopy(oldSaveFolder, saveDirectory, copySubDirs: true);
                }
                string saveInfoFile = oldSaveFolder + "where_is_my_save.txt";
                if (!File.Exists(saveInfoFile))
                {
                    CreatePath(oldSaveFolder);
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    using (StreamWriter writer = new StreamWriter(oldSaveFolder + "Save Data.url"))
                    {
                        string location = Assembly.GetExecutingAssembly().Location;
                        writer.WriteLine("[InternetShortcut]");
                        writer.WriteLine("URL=file:///" + saveDirectory);
                        writer.WriteLine("IconIndex=0");
                        string icon = location.Replace('\\', '/');
                        writer.WriteLine("IconFile=" + icon);
                    }
                    File.WriteAllText(saveInfoFile, "Hey! Keeping save data in the Documents folder was causing all kinds\nof issues for people, and it's with great sadness that I had to move your data.\nDon't worry, it still exists- your data is now located here:\n\n" + saveDirectory + "\n\nAny save data still located in this folder is for the old version (pre-2020) of Duck Game.");
                }
            }
            catch (Exception)
            {
            }
        }
        DevConsole.Log(DCSection.General, "DuckFile.Initialize().. " + (_saveRoot.Contains("OneDrive/") ? "Ah, a |DGBLUE|OneDrive|WHITE| user, I see.." : ""));
        if (!DirectoryExists(saveDirectory))
        {
            freshInstall = true;
        }
        _levelDirectory = "Levels/";
        _allPaths.Add(_levelDirectory);
        _editorPreviewDirectory = "EditorPreviews/";
        _allPaths.Add(_editorPreviewDirectory);
        _onlineLevelDirectory = "Online/Levels/";
        _allPaths.Add(_onlineLevelDirectory);
        _optionsDirectory = "Options/";
        _allPaths.Add(_optionsDirectory);
        _albumDirectory = "Album/";
        _allPaths.Add(_albumDirectory);
        _profileDirectory = "Profiles/";
        _allPaths.Add(_profileDirectory);
        _challengeDirectory = "ChallengeData/";
        _allPaths.Add(_challengeDirectory);
        _modsDirectory = "Mods/";
        _allPaths.Add(_modsDirectory);
        _skinsDirectory = "Texpacks/";
        _allPaths.Add(_skinsDirectory);
        _mappackDirectory = "Mappacks/";
        _allPaths.Add(_mappackDirectory);
        _hatpackDirectory = "Hatpacks/";
        _allPaths.Add(_hatpackDirectory);
        _scriptsDirectory = "Scripts/";
        _allPaths.Add(_scriptsDirectory);
        _workshopDirectory = "Workshop/";
        _allPaths.Add(_workshopDirectory);
        _customBlockDirectory = "Custom/Blocks/";
        _allPaths.Add(_customBlockDirectory);
        CreatePath(customBlockDirectory);
        _downloadedBlockDirectory = "Custom/Blocks/Downloaded/";
        _allPaths.Add(_downloadedBlockDirectory);
        _customBackgroundDirectory = "Custom/Background/";
        _allPaths.Add(_customBackgroundDirectory);
        CreatePath(customBackgroundDirectory);
        _downloadedBackgroundDirectory = "Custom/Background/Downloaded/";
        _allPaths.Add(_downloadedBackgroundDirectory);
        _customPlatformDirectory = "Custom/Platform/";
        _allPaths.Add(_customPlatformDirectory);
        CreatePath(customPlatformDirectory);
        _downloadedPlatformDirectory = "Custom/Platform/Downloaded/";
        _allPaths.Add(_downloadedPlatformDirectory);
        _customParallaxDirectory = "Custom/Parallax/";
        _allPaths.Add(_customParallaxDirectory);
        CreatePath(customParallaxDirectory);
        _downloadedParallaxDirectory = "Custom/Parallax/Downloaded/";
        _allPaths.Add(_downloadedParallaxDirectory);
        _customArcadeDirectory = "Custom/Arcade/";
        _allPaths.Add(customArcadeDirectory);
        CreatePath(customArcadeDirectory);
        try
        {
            _customMojiDirectory = "Custom/Moji/";
            _allPaths.Add(customMojiDirectory);
            CreatePath(customMojiDirectory);
            CreatePath(saveDirectory + "Custom/Hats/");
        }
        catch (Exception)
        {
            DevConsole.Log(DCSection.General, "|DGRED|Could not create moji path, disabling custom mojis :(");
            mojimode = false;
        }
        _logDirectory = "Logs/";
        _allPaths.Add(logDirectory);
        _musicDirectory = "Audio/Music/";
    }

    public static void FlagForBackup()
    {
        if (MonoMain.started)
        {
            _flaggedForBackup = true;
        }
    }

    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
        if (!directoryInfo.Exists)
        {
            throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
        }
        DirectoryInfo[] dirs = directoryInfo.GetDirectories();
        Directory.CreateDirectory(destDirName);
        FileInfo[] files = directoryInfo.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, overwrite: false);
        }
        if (copySubDirs)
        {
            DirectoryInfo[] array = dirs;
            foreach (DirectoryInfo subdir in array)
            {
                string tempPath2 = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath2, copySubDirs);
            }
        }
    }

    public static Sprite GetMoji(string moji, NetworkConnection pConnection = null)
    {
        if (!mojimode)
        {
            return null;
        }
        if (Options.Data.mojiFilter == 1 && pConnection != null && pConnection.data is User && (pConnection.data as User).relationship != FriendRelationship.Friend)
        {
            return null;
        }
        Sprite s = null;
        if (pConnection != null)
        {
            Dictionary<string, Sprite> profMap = null;
            if (_profileMojis.TryGetValue(pConnection.identifier, out profMap))
            {
                profMap.TryGetValue(moji, out s);
            }
        }
        else
        {
            _mojis.TryGetValue(moji, out s);
        }
        if ((pConnection == DuckNetwork.localConnection || pConnection == null) && Network.isActive && s == null)
        {
            try
            {
                foreach (NetworkConnection c in Network.connections)
                {
                    if (c != DuckNetwork.localConnection)
                    {
                        s = GetMoji(moji, c);
                        if (s != null && pConnection == DuckNetwork.localConnection)
                        {
                            SaveString(Editor.TextureToString(s.texture), customMojiDirectory + moji + ".moj");
                            RegisterMoji(moji, s);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        if (s == null && waitTry <= 0)
        {
            waitTry = 60;
            try
            {
                string fileName = customMojiDirectory + moji + ".png";
                if (!File.Exists(fileName))
                {
                    fileName = customMojiDirectory + moji + ".jpg";
                }
                if (!File.Exists(fileName))
                {
                    fileName = customMojiDirectory + moji + ".jpeg";
                }
                if (!File.Exists(fileName))
                {
                    fileName = customMojiDirectory + moji + ".bmp";
                }
                if (File.Exists(fileName))
                {
                    Texture2D t = TextureConverter.LoadPNGWithPinkAwesomenessAndMaxDimensions(Graphics.device, fileName, process: true, new Vec2(28f, 28f));
                    if (t != null)
                    {
                        if (t.Width <= 28 && t.Height <= 28)
                        {
                            Sprite spr = new Sprite(t);
                            RegisterMoji(moji, spr);
                            if (TextureConverter.lastLoadResultedInResize)
                            {
                                try
                                {
                                    TryFileOperation(delegate
                                    {
                                        string text = fileName;
                                        Delete(text);
                                        if (text.EndsWith(".jpg"))
                                        {
                                            text = text.Replace(".jpg", ".png");
                                        }
                                        if (text.EndsWith(".bmp"))
                                        {
                                            text = text.Replace(".bmp", ".png");
                                        }
                                        if (text.EndsWith(".jpeg"))
                                        {
                                            text = text.Replace(".jpeg", ".png");
                                        }
                                        FileStream stream = File.Create(text);
                                        t.SaveAsPng(stream, t.Width, t.Height);
                                    }, "InitializeMojis.Resize");
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                        else
                        {
                            DevConsole.Log("Error loading " + fileName + " MOJI (must be smaller than 28x28)", Color.Red);
                        }
                    }
                }
                else
                {
                    fileName = customMojiDirectory + moji + ".moj";
                    if (File.Exists(fileName))
                    {
                        Texture2D t2 = Editor.StringToTexture(ReadAllText(fileName));
                        if (t2 != null)
                        {
                            if (t2.Width <= 28 && t2.Height <= 28)
                            {
                                Sprite spr2 = new Sprite(t2);
                                RegisterMoji(moji, spr2);
                            }
                            else
                            {
                                DevConsole.Log("Error loading " + fileName + " MOJI (must be smaller than 28x28)", Color.Red);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        if (waitTry > 0)
        {
            waitTry--;
        }
        return s;
    }

    public static void StealMoji(string moji)
    {
        foreach (NetworkConnection c in Network.connections)
        {
            if (c != DuckNetwork.localConnection)
            {
                Sprite s = GetMoji(moji, c);
                if (s != null)
                {
                    SaveString(Editor.TextureToString(s.texture), customMojiDirectory + moji + ".moj");
                    RegisterMoji(moji, s);
                }
            }
        }
    }

    public static void RegisterMoji(string moji, Sprite s, NetworkConnection pConnection = null)
    {
        if (!mojimode)
        {
            return;
        }
        if (s.width <= 16)
        {
            s.Scale = new Vec2(2f, 2f);
        }
        else
        {
            s.Scale = new Vec2(1f, 1f);
        }
        if (s.width > 28 || s.height > 28)
        {
            return;
        }
        s.moji = true;
        if (pConnection != null)
        {
            Dictionary<string, Sprite> profMap = null;
            if (!_profileMojis.TryGetValue(pConnection.identifier, out profMap))
            {
                Dictionary<string, Sprite> dictionary = (_profileMojis[pConnection.identifier] = new Dictionary<string, Sprite>());
                profMap = dictionary;
            }
            profMap[moji] = s;
        }
        else
        {
            _mojis[moji] = s;
        }
    }

    public static void InitializeMojis()
    {
        if (!mojimode)
        {
            return;
        }
        List<string> list = GetFiles(customMojiDirectory, "*.png").ToList();
        list.AddRange(GetFiles(customMojiDirectory, "*.jpg"));
        list.AddRange(GetFiles(customMojiDirectory, "*.jpeg"));
        list.AddRange(GetFiles(customMojiDirectory, "*.bmp"));
        foreach (string s in list)
        {
            try
            {
                Texture2D t = TextureConverter.LoadPNGWithPinkAwesomenessAndMaxDimensions(Graphics.device, s, process: true, new Vec2(28f, 28f));
                if (t == null)
                {
                    continue;
                }
                if (t.Width <= 28 && t.Height <= 28)
                {
                    Sprite spr = new Sprite(t);
                    RegisterMoji(Path.GetFileNameWithoutExtension(s), spr);
                    if (!TextureConverter.lastLoadResultedInResize)
                    {
                        continue;
                    }
                    try
                    {
                        TryFileOperation(delegate
                        {
                            string text = s;
                            Delete(text);
                            if (text.EndsWith(".jpg"))
                            {
                                text = text.Replace(".jpg", ".png");
                            }
                            if (text.EndsWith(".bmp"))
                            {
                                text = text.Replace(".bmp", ".png");
                            }
                            if (text.EndsWith(".jpeg"))
                            {
                                text = text.Replace(".jpeg", ".png");
                            }
                            FileStream stream = File.Create(text);
                            t.SaveAsPng(stream, t.Width, t.Height);
                        }, "InitializeMojis.Resize");
                    }
                    catch (Exception)
                    {
                    }
                    continue;
                }
                DevConsole.Log("Error loading " + Path.GetFileName(s) + " MOJI (must be smaller than 28x28)", Color.Red);
            }
            catch (Exception)
            {
            }
        }
        foreach (string s2 in GetFiles(customMojiDirectory, "*.moj").ToList())
        {
            try
            {
                Texture2D t2 = Editor.StringToTexture(ReadAllText(s2));
                if (t2 != null)
                {
                    if (t2.Width <= 28 && t2.Height <= 28)
                    {
                        Sprite spr2 = new Sprite(t2);
                        RegisterMoji(Path.GetFileNameWithoutExtension(s2), spr2);
                    }
                    else
                    {
                        DevConsole.Log("Error loading " + Path.GetFileName(s2) + " MOJI (must be smaller than 28x28)", Color.Red);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        _mojis = _mojis.OrderByDescending((KeyValuePair<string, Sprite> x) => x.Key).ToDictionary((KeyValuePair<string, Sprite> pair) => pair.Key, (KeyValuePair<string, Sprite> pair) => pair.Value);
    }

    public static void BeginDataCommit()
    {
        _suppressCommit = true;
    }

    public static void EndDataCommit()
    {
        _suppressCommit = false;
    }

    private static void Commit(string pPath, bool pDelete = false)
    {
        _ = _suppressCommit;
        if (pPath != null)
        {
            if (pDelete)
            {
                Cloud.Delete(pPath);
            }
            else
            {
                Cloud.Write(pPath);
            }
        }
    }

    public static void DeleteAllSaveData()
    {
        Directory.Delete(userDirectory, recursive: true);
    }

    public static void CreatePath(string pathString)
    {
        CreatePath(pathString, ignoreLast: false);
    }

    public static void CreatePath(string pathString, bool ignoreLast)
    {
        pathString = pathString.Replace('\\', '/');
        string[] path = pathString.Split('/');
        string fullPath = "";
        for (int i = 0; i < path.Count(); i++)
        {
            if (path[i] == "" || path[i] == "/" || ((path[i].Contains('.') || ignoreLast) && i == path.Count() - 1))
            {
                continue;
            }
            fullPath += path[i];
            if (!Directory.Exists(fullPath))
            {
                if (MonoMain.logFileOperations)
                {
                    DevConsole.Log(DCSection.General, "DuckFile.CreatePath(" + fullPath + ")");
                }
                Directory.CreateDirectory(fullPath);
                Commit(null);
            }
            fullPath += "/";
        }
    }

    public static FileStream Create(string path)
    {
        CreatePath(path);
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.Create(" + path + ")");
        }
        return File.Create(path);
    }

    public static string ReadAllText(string pPath)
    {
        pPath = PreparePath(pPath);
        TryClearAttributes(pPath);
        string text = "";
        TryFileOperation(delegate
        {
            text = File.ReadAllText(pPath);
        }, "ReadAllText(" + pPath + ")");
        return text;
    }

    public static string[] ReadAllLines(string path)
    {
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.ReadAllLines(" + path + ")");
        }
        return File.ReadAllLines(path);
    }

    public static string GetShortPath(string path)
    {
        path.Replace('\\', '/');
        string saveDir = saveDirectory;
        int idx = path.IndexOf(saveDir);
        if (idx != -1)
        {
            return path.Substring(idx + saveDir.Length, path.Length - idx - saveDir.Length);
        }
        return path;
    }

    public static string FixInvalidPath(string pPath, bool pRemoveDirectoryCharacters = false)
    {
        string text = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (pRemoveDirectoryCharacters || (c != '\\' && c != '/' && c != ':'))
            {
                pPath = pPath.Replace(c.ToString(), "");
            }
        }
        return pPath;
    }

    public static bool IsUserPath(string path)
    {
        try
        {
            return Steam.user != null && Path.GetDirectoryName(path).Contains(Steam.user.id.ToString());
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static string GetLocalSavePath(string path)
    {
        path.Replace('\\', '/');
        if (IsUserPath(path))
        {
            string saveDir = userDirectory;
            int idx = path.IndexOf(saveDir);
            if (idx != -1)
            {
                return path.Substring(idx + saveDir.Length, path.Length - idx - saveDir.Length);
            }
        }
        else
        {
            string saveDir2 = saveDirectory;
            int idx2 = path.IndexOf(saveDir2);
            if (idx2 != -1)
            {
                return path.Substring(idx2 + saveDir2.Length, path.Length - idx2 - saveDir2.Length);
            }
        }
        return null;
    }

    public static string GetShortDirectory(string path)
    {
        path.Replace('\\', '/');
        string saveDir = saveRoot;
        int idx = path.IndexOf(saveDir);
        if (idx != -1)
        {
            return path.Substring(idx + saveDir.Length, path.Length - idx - saveDir.Length);
        }
        return path;
    }

    public static Stream OpenStream(string path)
    {
        if (MonoMain.logLevelOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.OpenStream(" + path + ")");
        }
        return TitleContainer.OpenStream(path);
    }

    public static List<string> GetFilesNoCloud(string path, string filter = "*.*", SearchOption so = SearchOption.TopDirectoryOnly)
    {
        List<string> files = new List<string>();
        try
        {
            string[] files2 = Directory.GetFiles(path, filter, SearchOption.TopDirectoryOnly);
            foreach (string d in files2)
            {
                files.Add(d);
            }
        }
        catch (Exception)
        {
        }
        if (so == SearchOption.AllDirectories)
        {
            try
            {
                string[] files2 = Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files2.Length; i++)
                {
                    List<string> f = GetFilesNoCloud(files2[i], filter, so);
                    files.AddRange(f);
                }
            }
            catch (Exception)
            {
            }
        }
        return files;
    }

    public static List<string> GetAllSavegameFiles(List<string> pFolderFilters, List<string> pRet = null, string pSubFolder = null, bool pRecurseFiltered = false, bool pDontAddFilteredFolders = false)
    {
        bool filtered = pRecurseFiltered;
        if (!filtered && pFolderFilters != null && pSubFolder != null)
        {
            string shortName = pSubFolder.Replace(userDirectory, "").Replace('\\', '/');
            foreach (string folderFilter in pFolderFilters)
            {
                if (shortName.StartsWith(folderFilter))
                {
                    if (pDontAddFilteredFolders)
                    {
                        return pRet;
                    }
                    filtered = true;
                }
            }
        }
        string path = ((pSubFolder != null) ? pSubFolder : userDirectory);
        List<string> files = ((pRet != null) ? pRet : new List<string>());
        string[] files2;
        try
        {
            files2 = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files2.Length; i++)
            {
                string r = files2[i];
                if (filtered)
                {
                    r += "?";
                }
                files.Add(r);
            }
        }
        catch (Exception)
        {
        }
        files2 = Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
        foreach (string d in files2)
        {
            try
            {
                GetAllSavegameFiles(pFolderFilters, files, d, filtered, pDontAddFilteredFolders);
            }
            catch (Exception)
            {
            }
        }
        return files;
    }

    public static List<string> GetDirectoriesNoCloud(string path, string filter = "*.*")
    {
        List<string> directories = new List<string>();
        try
        {
            string[] directories2 = Directory.GetDirectories(path, filter, SearchOption.TopDirectoryOnly);
            foreach (string d in directories2)
            {
                directories.Add(d);
            }
        }
        catch (Exception)
        {
        }
        return directories;
    }

    public static string[] GetFiles(string path, string filter)
    {
        return GetFiles(path, filter, SearchOption.TopDirectoryOnly);
    }

    public static string[] GetFiles(string path, string filter = "*.*", SearchOption option = SearchOption.TopDirectoryOnly)
    {
        List<string> files = new List<string>();
        if (Directory.Exists(path))
        {
            files = GetFilesNoCloud(path, filter, option);
            for (int i = 0; i < files.Count; i++)
            {
                files[i] = files[i].Replace('\\', '/');
            }
        }
        return files.ToArray();
    }

    public static string[] GetDirectories(string path)
    {
        path = path.Replace('\\', '/');
        List<string> dirs = new List<string>();
        path = path.Trim('/');
        if (Directory.Exists(path))
        {
            foreach (string d in GetDirectoriesNoCloud(path))
            {
                if (!Path.GetFileName(d).Contains("._"))
                {
                    string real = d.Replace('\\', '/');
                    if (!dirs.Contains(real))
                    {
                        dirs.Add(real);
                    }
                }
            }
        }
        return dirs.ToArray();
    }

    public static string ReplaceInvalidCharacters(string path)
    {
        string newPath = "";
        for (int i = 0; i < path.Length; i++)
        {
            char c = path[i];
            string inv = "";
            newPath = ((!_invalidPathCharConversions.TryGetValue(c, out inv)) ? (newPath + c) : (newPath + inv));
        }
        return newPath;
    }

    public static string RestoreInvalidCharacters(string path)
    {
        foreach (KeyValuePair<char, string> pair in _invalidPathCharConversions)
        {
            path = path.Replace(pair.Value, pair.Key.ToString() ?? "");
        }
        return path;
    }

    public static LevelData LoadLevelHeaderCached(string path)
    {
        LevelData d = null;
        if (!_levelCache.TryGetValue(path, out d))
        {
            LevelData levelData = (_levelCache[path] = LoadLevel(path, pHeaderOnly: true));
            d = levelData;
        }
        return d;
    }

    public static LevelData LoadLevel(string path)
    {
        return LoadLevel(path, pHeaderOnly: false);
    }

    public static bool FileExists(string pPath)
    {
        return File.Exists(pPath);
    }

    public static bool DirectoryExists(string pPath)
    {
        return Directory.Exists(pPath);
    }

    public static LevelData LoadLevel(string path, bool pHeaderOnly)
    {
        Cloud.ReplaceLocalFileWithCloudFile(path);
        if (!File.Exists(path))
        {
            return null;
        }
        if (MonoMain.logLevelOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.LoadLevel(" + path + ")");
        }
        LevelData l = LoadLevel(File.ReadAllBytes(path), pHeaderOnly);
        l?.SetPath(path);
        return l;
    }

    private static LevelData ConvertLevel(byte[] data)
    {
        if (MonoMain.logLevelOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.ConvertLevel()");
        }
        LevelData dat = null;
        Editor e = new Editor();
        bool skipInit = Level.skipInitialize;
        Level.skipInitialize = true;
        Level curLevel = Level.core.currentLevel;
        Level.core.currentLevel = e;
        try
        {
            e.minimalConversionLoad = true;
            DuckXML doc = DuckXML.Load(data);
            e.LegacyLoadLevelParts(doc);
            e.things.RefreshState();
            dat = e.CreateSaveData();
            if (!e.hadGUID)
            {
                uint checksum = Editor.Checksum(data);
                string getVal = null;
                if (!_conversionGUIDMap.TryGetValue(checksum, out getVal))
                {
                    getVal = dat.metaData.guid;
                    _conversionGUIDMap[checksum] = getVal;
                }
                dat.metaData.guid = getVal;
            }
        }
        catch
        {
        }
        Level.core.currentLevel = curLevel;
        Level.skipInitialize = skipInit;
        return dat;
    }

    public static LevelData LoadLevel(byte[] data)
    {
        return LoadLevel(data, pHeaderOnly: false);
    }

    public static LevelData LoadLevel(byte[] data, bool pHeaderOnly)
    {
        LevelData dat = BinaryClassChunk.FromData<LevelData>(new BitBuffer(data, copyData: false), pHeaderOnly);
        if ((!pHeaderOnly && dat == null) || (dat != null && dat.GetResult() == DeserializeResult.InvalidMagicNumber))
        {
            Promise<LevelData> promise = Tasker.Task(() => ConvertLevel(data));
            promise.WaitForComplete();
            return promise.Result;
        }
        if (dat != null && dat.GetExtraHeaderInfo() is LevelMetaData)
        {
            dat.RerouteMetadata(dat.GetExtraHeaderInfo() as LevelMetaData);
        }
        else
        {
            _ = dat.metaData;
        }
        return dat;
    }

    public static void WriteAllTextSafe(string path, string contents)
    {
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.WriteAllTextSafe(" + path + ")");
        }
        string tempPath = Path.GetTempFileName();
        byte[] data = Encoding.UTF8.GetBytes(contents);
        using (FileStream tempFile = File.Create(tempPath, 4096, FileOptions.WriteThrough))
        {
            tempFile.Write(data, 0, data.Length);
        }
        File.Replace(tempPath, path, null);
    }

    public static void WriteAllText(string pPath, string pContents)
    {
        pPath = PreparePath(pPath, pCreatePath: true);
        TryClearAttributes(pPath);
        TryFileOperation(delegate
        {
            File.WriteAllText(pPath, pContents);
        }, "WriteAllText(" + pPath + ")");
        Commit(pPath);
    }

    public static DuckXML LoadDuckXML(string path)
    {
        Cloud.ReplaceLocalFileWithCloudFile(path);
        if (!File.Exists(path))
        {
            return null;
        }
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.LoadDuckXML(" + path + ")");
        }
        DuckXML ret = null;
        try
        {
            ret = DuckXML.Load(path);
        }
        catch
        {
        }
        return ret;
    }

    public static void SaveDuckXML(DuckXML doc, string path)
    {
        path = PreparePath(path, pCreatePath: true);
        string docString = doc.ToString();
        if (string.IsNullOrWhiteSpace(docString))
        {
            throw new Exception("Blank XML (" + path + ")");
        }
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.SaveDuckXML(" + path + ")");
        }
        TryClearAttributes(path);
        TryFileOperation(delegate
        {
            File.WriteAllText(path, docString);
        }, "SaveDuckXML(" + path + ")");
        Commit(path);
    }

    public static void TryClearAttributes(string pFilename)
    {
        try
        {
            if (File.Exists(pFilename))
            {
                File.SetAttributes(pFilename, FileAttributes.Normal);
            }
        }
        catch (Exception)
        {
        }
    }

    public static bool TryFileOperation(Action pAction, string pActionName)
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                pAction();
                return true;
            }
            catch (Exception ex)
            {
                if (i == 2)
                {
                    throw ex;
                }
                DevConsole.Log(DCSection.General, ex.Message);
                DevConsole.Log(DCSection.General, "Exception running " + pActionName + ", retrying...");
                Thread.Sleep(50);
            }
        }
        return false;
    }

    public static XDocument LoadXDocument(string path)
    {
        CreatePath(Path.GetDirectoryName(path));
        if (!File.Exists(path))
        {
            return null;
        }
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.LoadXDocument(" + path + ")");
        }
        try
        {
            return XDocument.Load(path);
        }
        catch
        {
            return null;
        }
    }

    public static void SaveXDocument(XDocument doc, string path)
    {
        path = PreparePath(path, pCreatePath: true);
        string docString = doc.ToString();
        if (string.IsNullOrWhiteSpace(docString))
        {
            throw new Exception("Blank XML (" + path + ")");
        }
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.SaveXDocument(" + path + ")");
        }
        TryClearAttributes(path);
        TryFileOperation(delegate
        {
            File.WriteAllText(path, docString);
        }, "SaveXDocument(" + path + ")");
        Commit(path);
    }

    public static string LoadString(string pPath)
    {
        CreatePath(Path.GetDirectoryName(pPath));
        if (!File.Exists(pPath))
        {
            return null;
        }
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.LoadString(" + pPath + ")");
        }
        try
        {
            return File.ReadAllText(pPath);
        }
        catch
        {
            return null;
        }
    }

    public static void SaveString(string pString, string pPath)
    {
        pPath = PreparePath(pPath, pCreatePath: true);
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.SaveString(" + pPath + ")");
        }
        TryFileOperation(delegate
        {
            TryClearAttributes(pPath);
            File.WriteAllText(pPath, pString);
        }, "SaveString(" + pPath + ")");
        Commit(pPath);
    }

    public static string PreparePath(string pPath, bool pCreatePath = false)
    {
        pPath = pPath.Replace("//", "/");
        pPath = pPath.Replace('\\', '/');
        if (pPath.Length > 1 && pPath[1] == ':')
        {
            pPath = char.ToUpper(pPath[0]) + pPath.Substring(1, pPath.Length - 1);
        }
        if (pCreatePath)
        {
            CreatePath(Path.GetDirectoryName(pPath));
        }
        try
        {
            if (File.Exists(pPath))
            {
                File.SetAttributes(pPath, FileAttributes.Normal);
            }
        }
        catch (Exception)
        {
        }
        return pPath;
    }

    public static XmlDocument LoadSharpXML(string pPath)
    {
        Cloud.ReplaceLocalFileWithCloudFile(pPath);
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.LoadSharpXML(" + pPath + ")");
        }
        pPath = PreparePath(pPath);
        if (File.Exists(pPath))
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(pPath);
            return xmlDocument;
        }
        return null;
    }

    public static void SaveSharpXML(XmlDocument pDoc, string pPath)
    {
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.SaveSharpXML(" + pPath + ")");
        }
        TryFileOperation(delegate
        {
            pPath = PreparePath(pPath, pCreatePath: true);
            TryClearAttributes(pPath);
            pDoc.Save(pPath);
        }, "SaveSharpXML(" + pPath + ")");
        Commit(pPath);
    }

    public static T LoadChunk<T>(string path) where T : BinaryClassChunk
    {
        CreatePath(Path.GetDirectoryName(path));
        if (!File.Exists(path))
        {
            return null;
        }
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.LoadChunk(" + path + ")");
        }
        return BinaryClassChunk.FromData<T>(new BitBuffer(File.ReadAllBytes(path), 0, allowPacking: false));
    }

    public static bool GetLevelSpacePercentUsed(ref float percent)
    {
        return false;
    }

    public static void EnsureDownloadFileSpaceAvailable()
    {
    }

    public static bool SaveChunk(BinaryClassChunk doc, string path)
    {
        path = PreparePath(path, pCreatePath: true);
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.SaveChunk(" + path + ")");
        }
        BitBuffer data = null;
        data = doc.Serialize();
        TryFileOperation(delegate
        {
            FileStream fileStream = File.Create(path);
            fileStream.Write(data.buffer, 0, data.lengthInBytes);
            fileStream.Close();
        }, "SaveChunk(" + path + ")");
        Commit(path);
        return true;
    }

    public static void DeleteFolder(string folder)
    {
        if (Directory.Exists(folder))
        {
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "DuckFile.DeleteFolder(" + folder + ")");
            }
            string[] directories = GetDirectories(folder);
            for (int i = 0; i < directories.Length; i++)
            {
                DeleteFolder(directories[i]);
            }
            directories = GetFiles(folder);
            foreach (string f in directories)
            {
                if (f.EndsWith(".lev"))
                {
                    Editor.Delete(f);
                }
                else
                {
                    Delete(f);
                }
            }
            Directory.Delete(folder);
        }
        Commit(null);
    }

    public static byte[] ReadAllBytes(BinaryReader reader)
    {
        using MemoryStream ms = new MemoryStream();
        byte[] buffer = new byte[4096];
        int count;
        while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
        {
            ms.Write(buffer, 0, count);
        }
        return ms.ToArray();
    }

    public static byte[] ReadEntireStream(Stream stream)
    {
        long originalPosition = 0L;
        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
            stream.Position = 0L;
        }
        try
        {
            byte[] readBuffer = new byte[4096];
            int totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;
                if (totalBytesRead == readBuffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte != -1)
                    {
                        byte[] temp = new byte[readBuffer.Length * 2];
                        Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                        Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                        readBuffer = temp;
                        totalBytesRead++;
                    }
                }
            }
            byte[] buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead)
            {
                buffer = new byte[totalBytesRead];
                Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }
            return buffer;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }

    public static void Delete(string file)
    {
        file = PreparePath(file);
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "DuckFile.Delete(" + file + ")");
        }
        TryFileOperation(delegate
        {
            if (File.Exists(file))
            {
                TryClearAttributes(file);
                File.Delete(file);
            }
        }, "DuckFile.Delete(" + file + ")");
        Commit(file, pDelete: true);
    }
}
