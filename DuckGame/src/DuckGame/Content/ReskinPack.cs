using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace DuckGame;

public class ReskinPack : ContentPack
{
    public string name;

    public string path;

    public string contentPath;

    public bool hasIngameMusic;

    public bool isHD;

    public Dictionary<string, Color> recolors = new Dictionary<string, Color>();

    public static List<ReskinPack> active = new List<ReskinPack>();

    public static List<ReskinPack> _reskins = new List<ReskinPack>();

    public static ReskinPack context;

    private static bool _loadingMusic;

    private SoundEffect _currentMusic;

    public ReskinPack()
        : base(null)
    {
    }

    public static Mod LoadReskin(string pDir, Mod pExistingMod = null, ModConfiguration pExistingConfig = null)
    {
        ReskinPack pack = new ReskinPack
        {
            name = Path.GetFileName(pDir),
            path = pDir
        };
        pack.Initialize();
        _reskins.Add(pack);
        if (pExistingMod == null && pExistingConfig == null)
        {
            if (!DuckFile.FileExists(pDir + "/screenshot.png"))
            {
                File.Copy(DuckFile.contentDirectory + "/reskin_screenshot.pngfile", pDir + "/screenshot.png");
            }
            if (!DuckFile.FileExists(pDir + "/preview.png"))
            {
                File.Copy(DuckFile.contentDirectory + "/reskin_preview.pngfile", pDir + "/preview.png");
            }
            if (!DuckFile.FileExists(pDir + "/info.txt"))
            {
                DuckFile.SaveString("My DG Textures(" + pack.name + ")\nDan Rando\nEdit info.txt to change this information!\n<replace this line with the two letters 'hd' to tell DG that this is a high definition texture pack>", pDir + "/info.txt");
            }
            if (!DuckFile.DirectoryExists(pDir + "/Content"))
            {
                DuckFile.CreatePath(pDir + "/Content");
            }
        }
        Mod m = pExistingMod;
        if (m == null)
        {
            pack.contentPath = pack.path + "/Content";
            m = new ClientMod(pDir + "/", pExistingConfig);
            m.configuration.LoadOrCreateConfig();
            m.configuration.SetModType(ModConfiguration.Type.Reskin);
            pack.isHD = m.configuration.isHighResReskin;
            ModLoader.AddMod(m);
            string colorFile = pDir + "/colors.txt";
            if (DuckFile.FileExists(colorFile))
            {
                string[] array = DuckFile.ReadAllLines(colorFile);
                for (int i = 0; i < array.Length; i++)
                {
                    string[] split = array[i].Split('=');
                    if (split.Length == 2)
                    {
                        string colorName = split[0].Trim().ToLowerInvariant();
                        string colorVal = split[1].Trim();
                        try
                        {
                            pack.recolors[colorName] = Color.FromHexString(colorVal);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }
        else if (m.configuration.isExistingReskinMod)
        {
            pack.contentPath = pack.path;
        }
        m.SetPriority(Priority.Reskin);
        if (!m.configuration.disabled)
        {
            active.Add(pack);
        }
        if (m.configuration.content == null)
        {
            m.configuration.content = new ContentPack(m.configuration);
        }
        return m;
    }

    public static void InitializeReskins()
    {
        string[] directories = DuckFile.GetDirectories(DuckFile.skinsDirectory);
        for (int i = 0; i < directories.Length; i++)
        {
            LoadReskin(directories[i]);
        }
        if (Steam.user != null)
        {
            directories = DuckFile.GetDirectories(DuckFile.globalSkinsDirectory);
            for (int i = 0; i < directories.Length; i++)
            {
                LoadReskin(directories[i]);
            }
        }
    }

    public static void FinalizeReskins()
    {
        List<ClassMember> staticMembers = Editor.GetStaticMembers(typeof(Color));
        staticMembers.AddRange(Editor.GetStaticMembers(typeof(Colors)));
        Dictionary<string, ClassMember> existing = new Dictionary<string, ClassMember>();
        foreach (ClassMember c in staticMembers)
        {
            if (c.type == typeof(Color))
            {
                string name = c.name.ToLowerInvariant();
                existing[name] = c;
            }
        }
        for (int i = active.Count - 1; i >= 0; i--)
        {
            foreach (KeyValuePair<string, Color> pair in active[i].recolors)
            {
                ClassMember c2 = null;
                if (existing.TryGetValue(pair.Key, out c2))
                {
                    c2.SetValue(null, pair.Value);
                }
            }
        }
    }

    public void Initialize()
    {
        hasIngameMusic = DuckFile.GetFiles(contentPath + "/Audio/Music/InGame").Length != 0;
    }

    public static T LoadAsset<T>(string pName, bool pMusic = false)
    {
        _loadingMusic = pMusic;
        foreach (ReskinPack item in active)
        {
            T load = item.Load<T>(pName);
            if (load != null)
            {
                return load;
            }
        }
        return default(T);
    }

    public override T Load<T>(string name)
    {
        if (name.Contains(":"))
        {
            return default(T);
        }
        if (typeof(T) == typeof(string[]))
        {
            if (File.Exists(contentPath + "/" + name))
            {
                return (T)(object)File.ReadAllLines(contentPath + "/" + name);
            }
        }
        else
        {
            if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Tex2D))
            {
                Texture2D t = null;
                if (_textures.TryGetValue(name, out t))
                {
                    return (T)(object)t;
                }
                Texture2D tex = ContentPack.LoadTexture2D(contentPath + "/" + name, _modConfig == null || _modConfig.processPinkTransparency);
                _textures[name] = tex;
                return (T)(object)tex;
            }
            if (typeof(T) == typeof(SoundEffect))
            {
                SoundEffect s = null;
                if (!_loadingMusic && _sounds.TryGetValue(name, out s))
                {
                    return (T)(object)s;
                }
                if (_loadingMusic && _currentMusic != null)
                {
                    _currentMusic.Dispose();
                }
                SoundEffect snd = LoadSoundEffect(contentPath + "/" + name);
                if (_loadingMusic)
                {
                    _currentMusic = snd;
                }
                else
                {
                    _sounds[name] = snd;
                }
                return (T)(object)snd;
            }
            if (typeof(T) == typeof(Song))
            {
                Song s2 = null;
                if (_songs.TryGetValue(name, out s2))
                {
                    return (T)(object)s2;
                }
                Song song = LoadSong(contentPath + "/" + name);
                _songs[name] = song;
                return (T)(object)song;
            }
        }
        return default(T);
    }
}
