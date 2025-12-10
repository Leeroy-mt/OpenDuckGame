using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuckGame;

public class LSItem : Thing
{
    private bool _selected;

    private bool _enabled;

    private bool _partiallyEnabled;

    private string _path = "";

    public string _name;

    private LSItemType _itemType;

    public Sprite _customIcon;

    private SpriteMap _icons;

    private BitmapFont _font;

    private Sprite _steamIcon;

    private List<string> _levelsInside = new List<string>();

    private LevelSelect _select;

    public bool isModPath;

    public bool isModRoot;

    public bool isCloudFolder;

    public MapPack mapPack;

    public LevelData data;

    public bool selected
    {
        get
        {
            return _selected;
        }
        set
        {
            _selected = value;
        }
    }

    public bool enabled
    {
        get
        {
            return _enabled;
        }
        set
        {
            _enabled = value;
        }
    }

    public bool partiallyEnabled
    {
        get
        {
            return _partiallyEnabled;
        }
        set
        {
            _partiallyEnabled = value;
        }
    }

    public string path
    {
        get
        {
            return _path;
        }
        set
        {
            _path = value;
        }
    }

    public LSItemType itemType
    {
        get
        {
            return _itemType;
        }
        set
        {
            _itemType = value;
        }
    }

    public bool isFolder
    {
        get
        {
            if (_itemType != LSItemType.Folder)
            {
                return _itemType == LSItemType.MapPack;
            }
            return true;
        }
    }

    public bool isPlaylist => _itemType == LSItemType.Playlist;

    public List<string> levelsInside => _levelsInside;

    public LSItem(float xpos, float ypos, LevelSelect select, string PATH, bool isWorkshop = false, bool pIsModPath = false, bool pIsModRoot = false, bool pIsMapPack = false)
        : base(xpos, ypos)
    {
        _select = select;
        _icons = new SpriteMap("tinyIcons", 8, 8);
        _font = new BitmapFont("biosFont", 8);
        isModPath = pIsModPath;
        isModRoot = pIsModRoot;
        _path = PATH;
        if (_path == "../")
        {
            _name = "../";
            _itemType = LSItemType.UpFolder;
            return;
        }
        string ext = Path.GetExtension(_path);
        if (ext == ".lev")
        {
            _itemType = LSItemType.Level;
        }
        else if (ext == ".play")
        {
            _itemType = LSItemType.Playlist;
        }
        else
        {
            _itemType = LSItemType.Folder;
        }
        if (isWorkshop)
        {
            _itemType = LSItemType.Workshop;
        }
        if (pIsMapPack)
        {
            _itemType = LSItemType.MapPack;
        }
        _name = Path.GetFileNameWithoutExtension(_path);
        string path = _path.Replace('\\', '/');
        if (isWorkshop)
        {
            _path = "@WORKSHOP@";
            _levelsInside = GetLevelsInside(_select, "@WORKSHOP@");
        }
        else if (path == "@VANILLA@")
        {
            _path = "@VANILLA@";
            _levelsInside = GetLevelsInside(_select, "@VANILLA@");
        }
        else
        {
            if (!isFolder && !isPlaylist)
            {
                path = path.Substring(0, path.Length - 4);
            }
            string shortName = path.Substring(path.LastIndexOf("/levels/", StringComparison.InvariantCultureIgnoreCase) + 8);
            if (isFolder || isPlaylist)
            {
                _levelsInside = GetLevelsInside(_select, _path);
                if (!isModPath)
                {
                    _path = "/" + shortName;
                }
            }
            else
            {
                _path = path + ".lev";
            }
        }
        bool found = false;
        bool all = true;
        foreach (string lev in _levelsInside)
        {
            if (Editor.activatedLevels.Contains(lev))
            {
                found = true;
            }
            else
            {
                all = false;
            }
        }
        enabled = found;
        _partiallyEnabled = found && !all;
        data = DuckFile.LoadLevelHeaderCached(_path);
    }

    public static List<string> GetLevelsInside(LevelSelect selector, string path)
    {
        List<string> levels = new List<string>();
        if (path == "@WORKSHOP@")
        {
            foreach (WorkshopItem s in Steam.GetAllWorkshopItems())
            {
                if ((s.stateFlags & WorkshopItemState.Installed) == 0 || s.path == null || !Directory.Exists(s.path))
                {
                    continue;
                }
                string[] files = DuckFile.GetFiles(s.path);
                foreach (string file in files)
                {
                    string lName = file;
                    if (lName.EndsWith(".lev") && selector.filters.TrueForAll((IFilterLSItems a) => a.Filter(lName, LevelLocation.Workshop)))
                    {
                        levels.Add(lName);
                    }
                }
            }
        }
        else if (path == "@VANILLA@")
        {
            string vanillaPath = DuckFile.contentDirectory + "Levels/deathmatch/";
            string[] files = DuckFile.GetDirectories(vanillaPath);
            foreach (string dir in files)
            {
                levels.AddRange(from x in GetLevelsInside(selector, dir)
                                where !x.Contains("online") && !x.Contains("holiday")
                                select x);
            }
            files = Content.GetFiles(vanillaPath);
            foreach (string file2 in files)
            {
                if (!file2.Contains("online") && !file2.Contains("holiday") && file2.EndsWith(".lev") && selector.filters.TrueForAll((IFilterLSItems a) => a.Filter(file2)))
                {
                    string filename = file2.Replace('\\', '/');
                    levels.Add(filename);
                }
            }
        }
        else if (path.EndsWith(".play"))
        {
            DXMLNode root = DuckFile.LoadDuckXML(path).Element("playlist");
            if (root != null)
            {
                LevelPlaylist levelPlaylist = new LevelPlaylist();
                levelPlaylist.Deserialize(root);
                foreach (string l in levelPlaylist.levels)
                {
                    string lName2 = l;
                    if (selector.filters.TrueForAll((IFilterLSItems a) => a.Filter(lName2)))
                    {
                        levels.Add(lName2);
                    }
                }
            }
        }
        else
        {
            string[] files = DuckFile.GetDirectories(path);
            foreach (string dir2 in files)
            {
                levels.AddRange(GetLevelsInside(selector, dir2));
            }
            files = Content.GetFiles(path);
            foreach (string file3 in files)
            {
                if (file3.EndsWith(".lev") && selector.filters.TrueForAll((IFilterLSItems a) => a.Filter(file3)))
                {
                    string filename2 = file3.Replace('\\', '/');
                    levels.Add(filename2);
                }
            }
        }
        return levels;
    }

    public override void Update()
    {
        if (_itemType != LSItemType.UpFolder && !isFolder && _itemType != LSItemType.Playlist && _itemType != LSItemType.Workshop && _itemType != LSItemType.Vanilla)
        {
            enabled = Editor.activatedLevels.Contains(path);
        }
    }

    public override void Draw()
    {
        float xDraw = base.x;
        if (_selected)
        {
            _icons.frame = 3;
            Graphics.Draw(_icons, xDraw - 8f, base.y);
        }
        string text = _name;
        if (text.Length > 15)
        {
            text = text.Substring(0, 14) + ".";
        }
        if (_itemType != LSItemType.UpFolder)
        {
            _icons.frame = (_partiallyEnabled ? 4 : (_enabled ? 1 : 0));
            Graphics.Draw(_icons, xDraw, base.y);
            xDraw += 10f;
        }
        bool makeBlue = false;
        bool makeVanilla = false;
        if (_itemType == LSItemType.Folder || _itemType == LSItemType.UpFolder)
        {
            _icons.frame = 2;
            if (isModRoot)
            {
                _icons.frame = 6;
                makeBlue = true;
            }
            if (isCloudFolder)
            {
                _icons.frame = 7;
                makeBlue = true;
            }
            Graphics.Draw(_icons, xDraw, base.y);
            xDraw += 10f;
        }
        if (_itemType == LSItemType.Playlist)
        {
            _icons.frame = 5;
            Graphics.Draw(_icons, xDraw, base.y);
            xDraw += 10f;
            makeBlue = true;
        }
        if (_itemType == LSItemType.Workshop)
        {
            if (_steamIcon == null)
            {
                _steamIcon = new Sprite("steamIcon");
            }
            _steamIcon.scale = new Vec2(0.25f, 0.25f);
            Graphics.Draw(_steamIcon, xDraw, base.y);
            xDraw += 10f;
            text = "Workshop";
        }
        if (_itemType == LSItemType.Vanilla)
        {
            text = "@VANILLAICON@Vanilla";
            makeVanilla = true;
        }
        if (_itemType == LSItemType.MapPack)
        {
            Graphics.Draw(_customIcon, xDraw, base.y);
            xDraw += 10f;
            makeBlue = true;
        }
        if (data != null && data.metaData.eightPlayer)
        {
            text = "|DGPURPLE|(8)|PREV|" + text;
        }
        if (text.EndsWith("_8"))
        {
            text = text.Substring(0, text.Length - 2);
        }
        if (makeVanilla)
        {
            _font.Draw(text, xDraw, base.y, _selected ? Colors.DGVanilla : (Colors.DGVanilla * 0.75f), 0.8f);
        }
        else if (makeBlue)
        {
            _font.Draw(text, xDraw, base.y, _selected ? Colors.DGBlue : (Colors.DGBlue * 0.75f), 0.8f);
        }
        else
        {
            _font.Draw(text, xDraw, base.y, _selected ? Color.White : Color.Gray, 0.8f);
        }
    }
}
