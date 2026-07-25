using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using XnaRenderTarget2D = Microsoft.Xna.Framework.Graphics.RenderTarget2D;

namespace DuckGame;

public class Content
{
    #region Public Fields

    public static volatile bool renderingPreview;
    public static volatile bool readyToRenderPreview;
    public static volatile bool cancelPreview;
    public static bool doingTempSave;
    public static bool renderingToTarget;

    public static short _currentTextureIndex;

    public static int customPreviewWidth;
    public static int customPreviewHeight;

    public static Vector2 customPreviewCenter;

#if NO_TEX2D
    public static Texture2D invalidTexture;
#else
    public static Tex2D invalidTexture;
#endif
    public static XMLLevel previewLevel;
    public static Exception lastException;

    public static object _loadLock = new();

    public static byte[] generatePreviewBytes;

#endregion

    #region Private Fields

    static bool _previewBackground;

    static short _currentEffectIndex;

    static string _previewPath;
    static string _path = "";

    static Camera _previewCamera;
    static LevelData _previewLevelData;
    static LayerCore _previewLayerCore;
#if !MODERN_BATCH
    static MTSpriteBatch _previewBatch;
#else
    static TriangleBatch _previewBatch;
#endif
    static Thread _previewThread;
#if NO_TEX2D
    static XnaRenderTarget2D _currentPreviewTarget;
#else
    static RenderTarget2D _currentPreviewTarget;
#endif
    static LevelMetaData.PreviewPair _currentPreviewPair;
    static ContentManager _base;

    static MultiMap<string, LevelData> _levels = [];
    static Dictionary<string, Effect> _effects = [];
    static List<Effect> _effectList = [];
    static Dictionary<string, SoundEffect> _sounds = [];
#if NO_TEX2D
    static Dictionary<string, Texture2D> _textures = [];
    static List<Texture2D> _textureList = [];
#else
    static Dictionary<string, Tex2D> _textures = [];
    static Dictionary<object, Tex2D> _texture2DMap = [];
    static List<Tex2D> _textureList = [];
#endif
    static Dictionary<Type, string> _extensionList = new()
    {
        {
            typeof(Tex2D),
            "*.png"
        },
        {
            typeof(Texture2D),
            "*.png"
        },
        {
            typeof(SoundEffect),
            "*.wav"
        },
        {
            typeof(Song),
            "*.ogg"
        },
        {
            typeof(Level),
            "*.lev"
        },
        {
            typeof(Effect),
            "*.xnb"
        }
    };
    static Dictionary<string, Vector2> _spriteSizeDirectory = [];
    static Dictionary<string, ParallaxBackground.Definition> _parallaxDefinitions = [];

#endregion

    #region Public Properties

    public static string path => _path;

    public static Thread previewThread => _previewThread;

    public static List<Effect> effectList => _effectList;
#if NO_TEX2D
    public static Dictionary<string, Texture2D> textures => _textures;
    public static List<Texture2D> textureList => _textureList;
#else
    public static Dictionary<string, Tex2D> textures => _textures;
    public static List<Tex2D> textureList => _textureList;
#endif

    #endregion

    #region Public Methods

    public static LevelData GetLevel(string guid, LevelLocation location = LevelLocation.Any)
    {
        if (guid != null && _levels.TryGetValue(guid, out var levs))
        {
            foreach (LevelData lev in levs)
                if (lev.GetLocation() == location || location == LevelLocation.Any)
                    return lev;
        }
        return null;
    }

    public static List<LevelData> GetAllLevels(string guid)
    {
        if (_levels.TryGetValue(guid, out var levs))
            return levs;

        return [];
    }

    public static void MapLevel(string lev, LevelData dat, LevelLocation location)
    {
        lock (_levels)
        {
            if (_levels.TryGetValue(lev, out var levs))
            {
                LevelData found = null;
                foreach (LevelData l in levs)
                    if (l.GetLocation() == location)
                    {
                        found = l;
                        break;
                    }

                if (found != null)
                    levs.Remove(found);
            }
            dat.SetLocation(location);
            _levels.Add(lev, dat);
        }
    }

#if NO_TEX2D
    public static LevelMetaData.PreviewPair GeneratePreview(LevelData levelData, bool pRefresh = false, XnaRenderTarget2D pCustomPreviewTarget = null)
#else
    public static LevelMetaData.PreviewPair GeneratePreview(LevelData levelData, bool pRefresh = false, RenderTarget2D pCustomPreviewTarget = null)
#endif
    {
        _previewLevelData = levelData;
        return GeneratePreview((string)null, pRefresh, pCustomPreviewTarget);
    }

#if NO_TEX2D
    public static LevelMetaData.PreviewPair GeneratePreview(string levelPath, bool pRefresh = false, XnaRenderTarget2D pCustomPreviewTarget = null)
#else
    public static LevelMetaData.PreviewPair GeneratePreview(string levelPath, bool pRefresh = false, RenderTarget2D pCustomPreviewTarget = null)
#endif
    {
        if (generatePreviewBytes != null)
        {
            _previewLevelData = DuckFile.LoadLevel(generatePreviewBytes);
            generatePreviewBytes = null;
        }

        bool background = false;

        if (pCustomPreviewTarget != null)
        {
            background = true;
        }
        else if (!pRefresh && levelPath != null)
        {
            LevelMetaData d = (_previewLevelData == null) ? Editor.ReadLevelMetadata(levelPath) : Editor.ReadLevelMetadata(_previewLevelData);
            if (d != null)
            {
                LevelMetaData.PreviewPair previewPair = d.LoadPreview();
                if (previewPair != null)
                    return previewPair;
            }
        }

        DevConsole.Log(DCSection.General, $"Generating preview data for ({levelPath})...");
        _previewBackground = background;
        readyToRenderPreview = false;

        if (_previewThread != null && _previewThread.IsAlive)
        {
            cancelPreview = true;
            int wait = 250;
            while (_previewThread.IsAlive)
            {
                Tasker.RunTasks();
                Thread.Sleep(2);
                wait--;
            }
            readyToRenderPreview = false;
        }

        _previewThread = null;
        cancelPreview = false;
        Thing.skipLayerAdding = false;
        Level.skipInitialize = false;
        _previewBatch ??= new(Graphics.device);
        _previewPath = levelPath;
#if NO_TEX2D
        _currentPreviewTarget = pCustomPreviewTarget ?? XnaRenderTarget2D.CreateSetUpTarget(320, 200);
#else
        _currentPreviewTarget = pCustomPreviewTarget ?? new RenderTarget2D(320, 200);
#endif
        renderingToTarget = true;
        renderingPreview = true;
        readyToRenderPreview = true;
        PreviewThread();
        DoPreviewRender(pCustomPreviewTarget == null);
        renderingPreview = false;
        readyToRenderPreview = false;
        renderingToTarget = false;
        return _currentPreviewPair;
    }

#if NO_TEX2D
    public static void SetTextureAtIndex(short index, Texture2D tex)
#else
    public static void SetTextureAtIndex(short index, Tex2D tex)
#endif
    {
        while (index >= _textureList.Count)
        {
            _textureList.Add(null);
            _currentTextureIndex++;
        }
        _textureList[index] = tex;
#if NO_TEX2D
        _textures[tex.Name] = tex;
#else
        _texture2DMap[tex.nativeObject] = tex;
        _textures[tex.textureName] = tex;
#endif
        tex.SetTextureIndex(index);
    }

#if NO_TEX2D
    public static Texture2D AssignTextureIndex(Texture2D tex)
    {
        var index = tex.GetTextureIndex();
        if (index is -1)
        {
            tex.SetTextureIndex(_currentTextureIndex);
            _currentTextureIndex++;
            _textureList.Add(tex);
        }
        return tex;
    }
#else
    public static Tex2D AssignTextureIndex(Tex2D tex)
    {

        _texture2DMap.TryGetValue(tex, out Tex2D val);
        if (val == null)
        {
            tex.SetTextureIndex(_currentTextureIndex);
            _currentTextureIndex++;
            _textureList.Add(tex);
            _texture2DMap[tex] = tex;
        }
        return val;
    }
#endif

#if !NO_TEX2D
    public static Tex2D GetTex2D(object tex)
    {
        return GetTex2D((Texture2D)tex);
    }
#endif

    public static Tex2D GetTex2D(Texture2D tex)
    {
        if (tex == null)
            return null;
#if NO_TEX2D
        return tex;
#else
        _texture2DMap.TryGetValue(tex, out Tex2D val);
        if (val == null)
        {
            val = new Tex2D(tex, "", _currentTextureIndex);
            _currentTextureIndex++;
            _textureList.Add(val);
            _texture2DMap[tex] = val;
        }
        return val;
#endif
    }

    public static void SetEffectAtIndex(short index, Effect e)
    {
        while (index > _effectList.Count)
        {
            _effectList.Add(null);
            _currentEffectIndex++;
        }
        _effectList[index] = e;
        _effects[e.Name] = e;
    }

#if NO_TEX2D
    public static Texture2D GetTex2DFromIndex(short index)
    {
        return _textureList[index];
    }
#else
    public static Tex2D GetTex2DFromIndex(short index)
    {
        return _textureList[index];
    }
#endif

    public static Effect GetMTEffectFromIndex(short index)
    {
        if (index < 0)
            return null;

        return _effectList[index];
    }

    public static List<string> GetFiles<T>(string path)
    {
        List<string> files = [];

        if (_extensionList.TryGetValue(typeof(T), out string ext))
            GetFilesInternal<T>(path, files, ext);

        return files;
    }

    public static List<string> GetFilesInternal<T>(string path, List<string> files, string ext)
    {
        string[] files2 = DuckFile.GetFiles(path, ext);
        foreach (string f in files2)
            files.Add(f);
        files2 = GetDirectories(path);
        for (int i = 0; i < files2.Length; i++)
            GetFilesInternal<T>(files2[i], files, ext);

        return files;
    }

    public static void ReloadLevels(string s)
    {
        SearchDirLevels($"Content/levels/{s}", LevelLocation.Content);
    }

    public static string GetLevelID(string path, LevelLocation loc = LevelLocation.Content)
    {
        if (!path.EndsWith(".lev"))
            path += ".lev";

        foreach (KeyValuePair<string, List<LevelData>> level in _levels)
        {
            foreach (LevelData dat in level.Value)
                if ((dat.GetLocation() == loc || loc == LevelLocation.Any) && dat.GetPath().EndsWith($"/{path}"))
                    return dat.metaData.guid;
        }

        LevelData lev = DuckFile.LoadLevel($"{Content.path}/levels/{path}");
        if (lev != null)
        {
            MapLevel(lev.metaData.guid, lev, loc);
            return lev.metaData.guid;
        }

        return "";
    }

    public static List<string> GetLevels(string dir, LevelLocation location)
    {
        return GetLevels(dir, location, pRecursive: true, pOnline: false, pEightPlayer: false);
    }

    public static List<string> GetLevels(string dir, LevelLocation location, bool pRecursive, bool pOnline, bool pEightPlayer, bool pAllowNonRestrictedEightPlayer = false, bool pSkipFilters = false)
    {
        List<string> levels = [];
        foreach (KeyValuePair<string, List<LevelData>> levDat in _levels)
        {
            foreach (LevelData dat in levDat.Value)
            {
                if ((dat.GetLocation() == location || location == LevelLocation.Any) && (pSkipFilters || ((!pOnline || dat.metaData.online) && (!pEightPlayer || dat.metaData.eightPlayer) && (pEightPlayer || !dat.metaData.eightPlayer || (pAllowNonRestrictedEightPlayer && !dat.metaData.eightPlayerRestricted)))))
                {
                    string path = dat.GetPath();
                    int index = path.IndexOf($"{dir}/");
                    if (index >= 0 && (pRecursive || path.LastIndexOf('/') == index + dir.Length))
                        levels.Add(levDat.Key);
                }
            }
        }
        return levels;
    }

    public static void ProcessLevel(string path, LevelLocation location)
    {
        try
        {
            /*💀*/
            /*====================The Code========================\
            |                                                     |
            |  ($"Loading Level {path}" != null) ? path : "null"  |
            |                                                     |
            \====================================================*/

            Main.SpecialCode = path;
            if (path.EndsWith(".lev"))
                LoadLevelData(path, location);
        }
        catch (Exception ex)
        {
            LogLevelFailure(ex.ToString());
        }
    }

    public static void InitializeBase(ContentManager manager)
    {
        _base = manager;
#if NO_TEX2D
        invalidTexture = Load<Texture2D>("notexture");
#else
        invalidTexture = Load<Tex2D>("notexture");
#endif
        _path = $"{Directory.GetCurrentDirectory()}/Content/";
    }

    public static void InitializeLevels()
    {
        SearchDirLevels("Content/levels", LevelLocation.Content);
        if (!Steam.IsInitialized())
            return;

        WorkshopQueryUser workshopQueryUser = Steam.CreateQueryUser(Steam.User.Id, WorkshopList.Subscribed, WorkshopType.UsableInGame, WorkshopSortOrder.TitleAsc);
        workshopQueryUser.RequiredTags.Add("Map");
        workshopQueryUser.OnlyQueryIDs = true;
        workshopQueryUser.ResultFetched += delegate (object sender, WorkshopQueryResult result)
        {
            WorkshopItem publishedFile = result.details.publishedFile;
            if ((publishedFile.StateFlags & WorkshopItemState.Installed) != WorkshopItemState.None)
                SearchDirLevels(publishedFile.Path, LevelLocation.Workshop);
        };
        workshopQueryUser.Request();
        Steam.Update();
    }

    public static Vector2 GetTextureSize(string pName)
    {
        if (_spriteSizeDirectory.TryGetValue(pName, out Vector2 size))
            return size;

        return Vector2.Zero;
    }

    public static void InitializeTextureSizeDictionary()
    {
        try
        {
            if (File.Exists($"{DuckFile.contentDirectory}texture_size_directory.dat"))
            {
                string[] array = File.ReadAllLines($"{DuckFile.contentDirectory}texture_size_directory.dat");
                for (int i = 0; i < array.Length; i++)
                {
                    string[] subParts = array[i].Split(',');
                    _spriteSizeDirectory[subParts[0].Trim().Replace('\\', '/')] = new Vector2(Convert.ToSingle(subParts[1]), Convert.ToSingle(subParts[2]));
                }
            }
        }
        catch (Exception ex)
        {
            DevConsole.Log(DCSection.General, "|DGRED|Error initializing texture_size_directory.dat:");
            DevConsole.Log(DCSection.General, $"|DGRED|{ex.Message}");
        }
    }

    public static void Initialize(bool reverse)
    {
        SearchDirTextures("Content/", reverse);
    }

    public static void Initialize()
    {
        Initialize(reverse: false);
    }

    public static void InitializeEffects()
    {
        SearchDirEffects("Content/Shaders");
    }

    public static string[] GetFiles(string path, string filter = "*.*")
    {
        path = path.Replace('\\', '/');
        path = path.Trim('/');
        string cur = $"{Directory.GetCurrentDirectory()}/";
        cur = cur.Replace('\\', '/');
        List<string> dirs = [];
        foreach (string d in DuckFile.GetFilesNoCloud(path, filter))
        {
            if (!Path.GetFileName(d).Contains("._"))
            {
                string fix = d.Replace('\\', '/');
                int index = fix.IndexOf(cur);
                if (index != -1)
                    fix = fix.Remove(index, cur.Length);
                dirs.Add(fix);
            }
        }
        return [.. dirs];
    }

    public static string[] GetDirectories(string path, string filter = "*.*")
    {
        path = path.Replace('\\', '/');
        path = path.Trim('/');
        List<string> dirs = [];
        foreach (string d in DuckFile.GetDirectoriesNoCloud(path))
        {
            if (!Path.GetFileName(d).Contains("._"))
                dirs.Add(d);
        }
        return [.. dirs];
    }

    public static ParallaxBackground.Definition LoadParallaxDefinition(string pName)
    {
        try
        {
            if (!pName.EndsWith(".txt"))
                pName += ".txt";

            if (_parallaxDefinitions.TryGetValue(pName, out ParallaxBackground.Definition def))
                return def;

            string fullPath = pName;

            if (!pName.Contains(':'))
                fullPath = DuckFile.contentDirectory + pName;

            string[] parts = null;
            if (ReskinPack.active.Count > 0)
                parts = ReskinPack.LoadAsset<string[]>(pName);
            if (parts == null && File.Exists(fullPath))
                parts = File.ReadAllLines(fullPath);

            if (parts != null)
            {
                try
                {
                    def = new ParallaxBackground.Definition();
                    string[] array = parts;
                    foreach (string s in array)
                    {
                        if (s.StartsWith('[') || string.IsNullOrWhiteSpace(s))
                            continue;

                        string[] p = s.Split(',');
                        ParallaxBackground.Definition.Zone zone = new()
                        {
                            index = Convert.ToInt32(p[0].Trim()),
                            distance = Convert.ToSingle(p[1].Trim()),
                            speed = Convert.ToSingle(p[2].Trim()),
                            moving = Convert.ToBoolean(p[3].Trim())
                        };

                        if (p.Length > 4)
                        {
                            zone.sprite = new Sprite(p[4].Trim());
                            if (p.Length > 6)
                                zone.sprite.Position = new Vector2(Convert.ToSingle(p[5].Trim()), Convert.ToSingle(p[6].Trim()));
                            if (p.Length > 7)
                                zone.sprite.Depth = Convert.ToSingle(p[7].Trim());
                        }

                        if (zone.sprite != null)
                            def.sprites.Add(zone);
                        else
                            def.zones.Add(zone);
                    }

                    return def;
                }
                catch (Exception ex)
                {
                    DevConsole.Log(DCSection.General, $"|DGRED|LoadParallaxDefinition error ({pName}):");
                    DevConsole.Log(DCSection.General, $"|DGRED|{ex.Message}");
                }
            }
        }
        catch (Exception)
        {
        }
        return null;
    }

    public static T Load<T>(string name)
    {
#if !NO_TEX2D
        if (ReskinPack.active.Count > 0)
        {
            try
            {
                if (typeof(T) == typeof(Tex2D))
                {
                    Texture2D tex = ReskinPack.LoadAsset<Texture2D>(name);
                    if (tex != null)
                    {
                        lock (_loadLock) //! TODO: implement texture resize, based on texture resolution for Texture2D
                        {
                            Vector2 originalSize = GetTextureSize(name);
                            Tex2D t = ((!(originalSize != Vector2.Zero) || ((float)tex.Width == originalSize.X && (float)tex.Height == originalSize.Y)) ? new Tex2D(tex, name, _currentTextureIndex) : new BigBoyTex2D(tex, name, _currentTextureIndex)
                            {
                                scaleFactor = originalSize.X / (float)tex.Width
                            });
                            _currentTextureIndex++;
                            _textureList.Add(t);
                            _textures[name] = t;
                            _texture2DMap[tex] = t;
                            return (T)(object)t;
                        }
                    }
                }
                else
                {
                    T skinret = ReskinPack.LoadAsset<T>(name);
                    if (skinret != null)
                        return skinret;
                }
            }
            catch (Exception)
            {
            }
        }
#endif
#if NO_TEX2D
        if (typeof(T) == typeof(Texture2D))
        {
            Texture2D t2 = null;
#else
        if (typeof(T) == typeof(Tex2D))
        {
            Tex2D t2 = null;
#endif
            lock (_textures)
            {
                _textures.TryGetValue(name, out t2);
            }

            if (t2 == null)
            {
                Texture2D t2d = null;
                bool modLoad = false;
                if (MonoMain.moddingEnabled && ModLoader.accessibleMods.Count > 1 && name.Length > 1 && name[1] == ':')
                    modLoad = true;

                if (!modLoad)
                {
                    try
                    {
                        t2d = _base.Load<Texture2D>(name);
                    }
                    catch (Exception ex2)
                    {
                        modLoad = MonoMain.moddingEnabled && ModLoader.modsEnabled;
                        lastException = ex2;
                    }
                }

                if (modLoad)
                {
                    foreach (Mod mod in ModLoader.accessibleMods)
                    {
                        if (mod.configuration != null && mod.configuration.content != null)
                            t2d = mod.configuration.content.Load<Texture2D>(name);

                        if (t2d != null)
                            break;
                    }
                }
                else if (t2d == null)
                {
                    try
                    {
                        t2d = ContentPack.LoadTexture2D(name);
                    }
                    catch (Exception ex3)
                    {
                        lastException = ex3;
                    }
                }

                if (t2d == null)
                {
                    t2d = invalidTexture;
                    Main.SpecialCode = $"Couldn't load texture {name}";
                }

                lock (_loadLock)
                {
#if NO_TEX2D
                    t2 = t2d;
                    t2.Name = name;
                    t2.SetTextureIndex(_currentTextureIndex);
                    _currentTextureIndex++;
                    _textureList.Add(t2);
                    _textures[name] = t2;
#else
                    t2 = new Tex2D(t2d, name, _currentTextureIndex);
                    _currentTextureIndex++;
                    _textureList.Add(t2);
                    _textures[name] = t2;
                    _texture2DMap[t2d] = t2;
#endif
                }
            }
            return (T)(object)t2;
        }

        if (typeof(T) == typeof(Effect))
        {
            Effect t3 = null;
            lock (_effects)
            {
                _effects.TryGetValue(name, out t3);
            }

            if (t3 == null)
            {
                lock (_loadLock)
                {
                    t3 = _base.Load<Effect>(name);
                }
                lock (_loadLock)
                {
                    _effectList.Add(t3);
                    _effects[name] = t3;
                }
            }
            return (T)(object)t3;
        }

        if (typeof(T) == typeof(SoundEffect))
        {
            SoundEffect sound = null;
            lock (_sounds)
            {
                _sounds.TryGetValue(name, out sound);
            }
            if (sound == null)
            {
                if (!name.Contains(':') && !name.EndsWith(".wav"))
                {
                    lock (_loadLock)
                    {
                        try
                        {
                            string fullName = $"{DuckFile.contentDirectory}{name}.wav";
                            sound = SoundEffect.FromStream(new MemoryStream(File.ReadAllBytes(fullName)));
                            sound?.file = fullName;
                        }
                        catch (Exception ex4)
                        {
                            lastException = ex4;
                        }
                    }
                }

                if (sound == null && MonoMain.moddingEnabled && ModLoader.modsEnabled)
                {
                    foreach (Mod mod2 in ModLoader.accessibleMods)
                    {
                        if (mod2.configuration != null && mod2.configuration.content != null)
                            sound = mod2.configuration.content.Load<SoundEffect>(name);

                        if (sound != null)
                            break;
                    }
                }
            }

            if (sound == null)
                Main.SpecialCode = $"Couldn't load sound ({sound?.ToString()})";
            else
                _sounds[name] = sound;

            return (T)(object)sound;
        }

        if (typeof(T) == typeof(Song))
        {
            if (MonoMain.moddingEnabled && ModLoader.modsEnabled)
            {
                foreach (Mod mod3 in ModLoader.accessibleMods)
                    if (mod3.configuration != null && mod3.configuration.content != null)
                    {
                        Song song = mod3.configuration.content.Load<Song>(name);
                        if (song != null)
                            return (T)(object)song;
                    }
            }
            return default;
        }

        if (typeof(T) == typeof(Microsoft.Xna.Framework.Media.Song))
            return (T)(object)_base.Load<Microsoft.Xna.Framework.Media.Song>(name);

        return _base.Load<T>(name);
    }

    public static short GetTextureIndex(Texture2D tex)
    {
#if NO_TEX2D
        return tex?.GetTextureIndex() ?? -1;
#else
        if (tex is null)
            return -1;
        var tex2d = GetTex2D(tex);
        var index = tex2d?.textureIndex ?? -1;
        return index;
#endif
    }

#endregion

    #region Private Methods

    static void PreviewThread()
    {
        Level l = Level.activeLevel;
        Level l2 = Level.core.currentLevel;
        LayerCore oldLayerCore = Layer.core;
        try
        {
            renderingPreview = true;
            if (!_previewBackground)
                Thing.skipLayerAdding = true;
            XMLLevel lev = null;

            if (_previewLevelData == null)
            {
                lev = new XMLLevel(_previewPath);
            }
            else
            {
                lev = new XMLLevel(_previewLevelData);
                _previewLevelData = null;
            }

            if (cancelPreview)
                return;

            previewLevel = lev;
            previewLevel.ignoreVisibility = true;
            Level.skipInitialize = !_previewBackground;

            if (!_previewBackground)
                previewLevel.isPreview = true;

            _previewLayerCore = null;
            if (_previewBackground)
            {
                Layer.core = _previewLayerCore = new LayerCore();
                Layer.core.InitializeLayers();
            }
            Level.core.currentLevel = previewLevel;
            Level.activeLevel = previewLevel;
            previewLevel.Initialize();
            Level.activeLevel = l;
            Level.core.currentLevel = l2;

            if (cancelPreview)
                return;

            Thing.skipLayerAdding = false;
            Level.skipInitialize = false;
            previewLevel.CalculateBounds();

            if (customPreviewWidth != 0)
                _previewCamera = new Camera(0, 0, customPreviewWidth, customPreviewHeight);
            else
                _previewCamera = new Camera(0, 0, 1280, 1280 * Graphics.aspect);

            Vector2 topLeft = previewLevel.topLeft;
            Vector2 br = previewLevel.bottomRight;
            Vector2 center = (topLeft + br) / 2;

            if (cancelPreview)
                return;

            _previewCamera.width /= 2;
            _previewCamera.height /= 2;

            if (customPreviewCenter != Vector2.Zero)
                _previewCamera.center = customPreviewCenter;
            else
                _previewCamera.center = center;

            readyToRenderPreview = true;
            if (_previewThread != null)
            {
                while (readyToRenderPreview)
                    if (cancelPreview)
                        return;
            }
            renderingPreview = false;
        }
        catch (Exception ex)
        {
            Program.LogLine(ex.ToString());
            renderingPreview = false;
            Thing.skipLayerAdding = false;
            Level.skipInitialize = false;
        }

        if (_previewBackground)
        {
            Level.activeLevel = l;
            Level.core.currentLevel = l2;
            Layer.core = oldLayerCore;
        }
    }

    static void DoPreviewRender(bool pSaveMetadata)
    {
        var cur = Graphics.screen;
        Graphics.screen = _previewBatch;

        var curTarget = Graphics.currentRenderTarget;
        Graphics.SetRenderTarget(_currentPreviewTarget);
#if NO_TEX2D
        Graphics.viewport = new Viewport(0, 0, _currentPreviewTarget.Width, _currentPreviewTarget.Height);
#else
        Graphics.viewport = new Viewport(0, 0, _currentPreviewTarget.width, _currentPreviewTarget.height);
#endif

        string curTileset0 = Custom.data[CustomType.Block][0];
        if (Custom.previewData[CustomType.Block][0] != null)
            Custom.ApplyCustomData(Custom.previewData[CustomType.Block][0].GetTileData(), 0, CustomType.Block);

        string curTileset1 = Custom.data[CustomType.Block][1];
        if (Custom.previewData[CustomType.Block][1] != null)
            Custom.ApplyCustomData(Custom.previewData[CustomType.Block][1].GetTileData(), 1, CustomType.Block);

        string curTileset2 = Custom.data[CustomType.Block][2];
        if (Custom.previewData[CustomType.Block][2] != null)
            Custom.ApplyCustomData(Custom.previewData[CustomType.Block][2].GetTileData(), 2, CustomType.Block);

        string curBackground0 = Custom.data[CustomType.Background][0];
        if (Custom.previewData[CustomType.Background][0] != null)
            Custom.ApplyCustomData(Custom.previewData[CustomType.Background][0].GetTileData(), 0, CustomType.Background);

        string curBackground1 = Custom.data[CustomType.Background][1];
        if (Custom.previewData[CustomType.Background][1] != null)
            Custom.ApplyCustomData(Custom.previewData[CustomType.Background][1].GetTileData(), 1, CustomType.Background);

        string curBackground2 = Custom.data[CustomType.Background][2];
        if (Custom.previewData[CustomType.Background][2] != null)
            Custom.ApplyCustomData(Custom.previewData[CustomType.Background][2].GetTileData(), 2, CustomType.Background);

        string curPlatform0 = Custom.data[CustomType.Platform][0];
        if (Custom.previewData[CustomType.Platform][0] != null)
            Custom.ApplyCustomData(Custom.previewData[CustomType.Platform][0].GetTileData(), 0, CustomType.Platform);

        string curPlatform1 = Custom.data[CustomType.Platform][1];
        if (Custom.previewData[CustomType.Platform][1] != null)
            Custom.ApplyCustomData(Custom.previewData[CustomType.Platform][1].GetTileData(), 1, CustomType.Platform);

        string curPlatform2 = Custom.data[CustomType.Platform][2];
        if (Custom.previewData[CustomType.Platform][2] != null)
            Custom.ApplyCustomData(Custom.previewData[CustomType.Platform][2].GetTileData(), 2, CustomType.Platform);

        bool challenge = false;
        bool strange = true;
        bool arcade = false;
        Dictionary<string, int> invalidList = [];

        if (_previewBackground)
        {
            Level l = Level.activeLevel;
            Level l2 = Level.core.currentLevel;
            LayerCore oldLayerCore = Layer.core;
            if (_previewLayerCore != null)
                Layer.core = _previewLayerCore;
            Level.activeLevel = previewLevel;
            Level.core.currentLevel = previewLevel;

            try
            {
                Graphics.defaultRenderTarget = _currentPreviewTarget;
                Layer.HUD.visible = false;
                previewLevel.camera = _previewCamera;
                previewLevel.simulatePhysics = false;
                previewLevel.DoUpdate();
                previewLevel.DoUpdate();
                previewLevel.DoDraw();
                Layer.HUD.visible = true;
                Graphics.defaultRenderTarget = null;
                Level.activeLevel = l;
                Level.core.currentLevel = l2;
                Layer.core = oldLayerCore;
            }
            catch (Exception ex)
            {
                Layer.HUD.visible = true;
                Graphics.defaultRenderTarget = null;
                Level.activeLevel = l;
                Level.core.currentLevel = l2;
                Layer.core = oldLayerCore;
                throw ex;
            }
        }
        else
        {
            Graphics.Clear(Color.Black);
            Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, _previewCamera.getMatrix());

            foreach (Thing t in previewLevel.things)
            {
                if (t.layer == Layer.Game || t.layer == Layer.Blocks || t.layer == null)
                    t.Draw();

                if (pSaveMetadata)
                {
                    if (t is ChallengeMode)
                        challenge = true;
                    else if (t is SpawnPoint)
                        strange = false;
                    else if (t is ArcadeMode)
                        arcade = true;

                    if (!ContentProperties.GetBag(t.GetType()).GetOrDefault("isOnlineCapable", defaultValue: true))
                    {
                        if (!invalidList.TryGetValue(t.editorName, out int value))
                            invalidList[t.editorName] = 1;
                        else
                            invalidList[t.editorName] = ++value;
                    }
                }

                Graphics.material = null;
            }

            Graphics.screen.End();
        }

        Graphics.screen = cur;
        Graphics.SetRenderTarget(curTarget);
        Custom.data[CustomType.Block][0] = curTileset0;
        Custom.data[CustomType.Block][1] = curTileset1;
        Custom.data[CustomType.Block][2] = curTileset2;
        Custom.data[CustomType.Background][0] = curBackground0;
        Custom.data[CustomType.Background][1] = curBackground1;
        Custom.data[CustomType.Background][2] = curBackground2;
        Custom.data[CustomType.Platform][0] = curPlatform0;
        Custom.data[CustomType.Platform][1] = curPlatform1;
        Custom.data[CustomType.Platform][2] = curPlatform2;

        if (pSaveMetadata && !doingTempSave)
        {
            LevelMetaData metaData = Editor.ReadLevelMetadata(previewLevel.data);
            if (metaData != null && metaData.guid != null)
                _currentPreviewPair = metaData.SavePreview(_currentPreviewTarget, invalidList, strange, challenge, arcade);
        }
    }

    static void SearchDirLevels(string dir, LevelLocation location)
    {
        string[] array = (location == LevelLocation.Content) ? GetFiles(dir) : DuckFile.GetFiles(dir, "*.*");
        for (int i = 0; i < array.Length; i++)
            ProcessLevel(array[i], location);

        array = (location == LevelLocation.Content) ? GetDirectories(dir) : DuckFile.GetDirectories(dir);
        for (int i = 0; i < array.Length; i++)
            SearchDirLevels(array[i], location);
    }

    static void SearchDirTextures(string dir, bool reverse = false)
    {
        if (reverse)
        {
            foreach (string item in DG.Reverse(GetFiles(dir)))
                ProcessTexture(item);

            foreach (string d in DG.Reverse(GetDirectories(dir)))
            {
                if (!d.EndsWith("Audio") && !d.EndsWith("Shaders"))
                    SearchDirTextures(d, reverse);
            }

            return;
        }

        string[] files = GetFiles(dir);
        for (int i = 0; i < files.Length; i++)
            ProcessTexture(files[i]);

        files = GetDirectories(dir);
        foreach (string d2 in files)
        {
            if (!d2.EndsWith("Audio") && !d2.EndsWith("Shaders"))
                SearchDirTextures(d2);
        }
    }

    static void SearchDirEffects(string dir)
    {
        string[] files = GetFiles(dir);
        for (int i = 0; i < files.Length; i++)
            ProcessEffect(files[i]);

        files = GetDirectories(dir);
        for (int i = 0; i < files.Length; i++)
            SearchDirEffects(files[i]);
    }

    static LevelData LoadLevelData(string pPath, LevelLocation pLocation)
    {
        pPath = pPath.Replace('\\', '/');
        LevelData dat = (pLocation != LevelLocation.Content) ? DuckFile.LoadLevel(pPath) : DuckFile.LoadLevel(DuckFile.ReadEntireStream(DuckFile.OpenStream(pPath)));
        if (dat != null)
        {
            dat.SetPath(pPath);
            if (dat.metaData.guid != null)
                MapLevel(dat.metaData.guid, dat, pLocation);

            return dat;
        }
        return null;
    }

    static void LogLevelFailure(string s)
    {
        try
        {
            Program.LogLine($"Level Load Failure (Did not cause crash)\n================================================\n {s}\n================================================\n");
        }
        catch (Exception)
        {
        }
    }

    static void ProcessTexture(string path)
    {
        if (path.EndsWith(".xnb"))
        {
            path = path.Replace('\\', '/');
            if (path.StartsWith("Content/"))
                path = path[8..];
            path = path[..^4];
#if NO_TEX2D
            Load<Texture2D>(path);
#else
            Load<Tex2D>(path);
#endif
        }
    }

    static void ProcessEffect(string path)
    {
        try
        {
            if (path.EndsWith(".xnb"))
            {
                path = path.Replace('\\', '/');
                if (path.StartsWith("Content/"))
                    path = path[8..];
                path = path[..^4];
                Load<Effect>(path);
            }
        }
        catch (Exception ex)
        {
            DevConsole.Log(DCSection.General, $"|DGRED|Failed to load shader ({path}):");
            DevConsole.Log(DCSection.General, $"|DGRED|{ex.Message}");
        }
    }

#endregion
}
