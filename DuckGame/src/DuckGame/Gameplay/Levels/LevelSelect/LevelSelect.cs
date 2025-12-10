using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DuckGame;

public class LevelSelect : Level
{
    private string _currentDirectory;

    private string _rootDirectory;

    private List<LSItem> _items = new List<LSItem>();

    private int _topIndex;

    private int _maxItems = 15;

    private int _selectedItem;

    private LSItem _selectedLevel;

    private float _leftPos = 12f;

    private float _topPos = 21f;

    private Texture2D _preview;

    private Sprite _previewSprite;

    private LSItem _previewItem;

    private LSItem _lastSelection;

    private BitmapFont _font;

    private Level _returnLevel;

    private bool _exiting;

    private TextEntryDialog _dialog;

    private UIMenu _returnMenu;

    private SpriteMap _iconSheet;

    private bool _onlineMode;

    private List<IFilterLSItems> _filters = new List<IFilterLSItems>();

    public string modRoot;

    public MapPack mapPack;

    public bool isInitialized;

    public static bool _skipCompanionOpening;

    public bool showPlaylistOption;

    public bool isClosed;

    private UIMenu _confirmMenu;

    private UIMenu _notOnlineMenu;

    private MenuBoolean _deleteFile = new MenuBoolean();

    public List<IFilterLSItems> filters => _filters;

    public LevelSelect(string root = "", Level returnLevel = null, UIMenu returnMenu = null, bool onlineMode = false)
    {
        _centeredView = true;
        if (root == "")
        {
            root = DuckFile.levelDirectory;
        }
        root = root.TrimEnd('/');
        _rootDirectory = root;
        _font = new BitmapFont("biosFont", 8);
        _returnLevel = returnLevel;
        _dialog = new TextEntryDialog();
        _dialog.filename = true;
        _returnMenu = returnMenu;
        _iconSheet = new SpriteMap("iconSheet", 16, 16);
        _onlineMode = onlineMode;
        _filters.Add(new LSFilterLevelType(LevelType.Deathmatch, needsDeathmatchTag: true));
        _filters.Add(new LSFilterMods(isOnline: true));
    }

    public void SetCurrentFolder(string folder)
    {
        SetCurrentFolder(folder, isModPath: false, isModRoot: false);
    }

    public void SetCurrentFolder(string folder, bool isModPath, bool isModRoot, MapPack pMapPack = null)
    {
        if (isModRoot)
        {
            modRoot = folder;
        }
        _currentDirectory = folder;
        if (_currentDirectory == _rootDirectory)
        {
            _selectedItem = 0;
            mapPack = null;
        }
        else
        {
            _selectedItem = 1;
        }
        if (pMapPack != null)
        {
            mapPack = pMapPack;
        }
        HUDTopRightSetup();
        _topIndex = 0;
        _items.Clear();
        if (_currentDirectory != _rootDirectory)
        {
            AddItem(new LSItem(0f, 0f, this, "../"));
        }
        else
        {
            AddItem(new LSItem(0f, 0f, this, "@VANILLA@")
            {
                itemType = LSItemType.Vanilla
            });
            if (Steam.GetNumWorkshopItems() > 0)
            {
                AddItem(new LSItem(0f, 0f, this, "@WORKSHOP@", isWorkshop: true));
            }
        }
        if (folder.EndsWith(".play") || folder == "@WORKSHOP@" || folder == "@VANILLA@")
        {
            List<string> levelsInside = LSItem.GetLevelsInside(this, folder);
            levelsInside.Sort();
            foreach (string lev in levelsInside)
            {
                AddItem(new LSItem(0f, 0f, this, lev));
            }
            _items = _items.OrderBy((LSItem x) => x.data != null && x.data.metaData.eightPlayer).ToList();
            PositionItems();
            return;
        }
        string[] dirs = DuckFile.GetDirectories(folder);
        string[] files = DuckFile.GetFiles(folder);
        Array.Sort(dirs);
        Array.Sort(files);
        if (_currentDirectory == _rootDirectory)
        {
            foreach (Mod m in ModLoader.accessibleMods)
            {
                if (m.configuration != null && m.configuration.content != null && m.configuration.content.levels.Count > 0)
                {
                    AddItem(new LSItem(0f, 0f, this, m.configuration.contentDirectory + "/Levels", isWorkshop: false, pIsModPath: true, pIsModRoot: true)
                    {
                        _name = m.configuration.name
                    });
                }
            }
            foreach (MapPack pack in MapPack.active)
            {
                AddItem(new LSItem(0f, 0f, this, pack.path, isWorkshop: false, pIsModPath: true, pIsModRoot: true, pIsMapPack: true)
                {
                    _name = pack.name,
                    _customIcon = pack.icon,
                    mapPack = pack
                });
            }
        }
        string[] array = dirs;
        foreach (string dir in array)
        {
            if (DuckFile.GetFiles(dir, "*.lev", SearchOption.AllDirectories).Count() > 0)
            {
                AddItem(new LSItem(0f, 0f, this, dir, isWorkshop: false, isModPath));
            }
        }
        List<string> levels = new List<string>();
        array = files;
        foreach (string file in array)
        {
            if (Path.GetExtension(file) == ".lev" && _filters.TrueForAll((IFilterLSItems a) => a.Filter(file)))
            {
                levels.Add(file);
            }
            else if (Path.GetExtension(file) == ".play")
            {
                AddItem(new LSItem(0f, 0f, this, file, isWorkshop: false, isModPath));
            }
        }
        foreach (string file2 in levels)
        {
            AddItem(new LSItem(0f, 0f, this, file2, isWorkshop: false, isModPath));
        }
        PositionItems();
    }

    public override void Initialize()
    {
        InputProfile.repeat = true;
        Keyboard.repeat = true;
        SetCurrentFolder(_rootDirectory);
        isInitialized = true;
        _dialog.DoInitialize();
        float wide = 320f;
        float high = 180f;
        _confirmMenu = new UIMenu("DELETE FILE!?", wide / 2f, high / 2f, 160f, -1f, "@CANCEL@CANCEL @SELECT@SELECT");
        if (_returnMenu != null)
        {
            _confirmMenu.Add(new UIMenuItem("WHAT? NO!", new UIMenuActionOpenMenu(_confirmMenu, _returnMenu), UIAlign.Center, default(Color), backButton: true));
            _confirmMenu.Add(new UIMenuItem("YEAH!", new UIMenuActionOpenMenuSetBoolean(_confirmMenu, _returnMenu, _deleteFile)));
            _notOnlineMenu = new UIMenu("NO WAY", wide / 2f, high / 2f, 160f, -1f, "@SELECT@OH :(");
            BitmapFont littleFont = new BitmapFont("smallBiosFontUI", 7, 5);
            UIText t = new UIText("THIS LEVEL CONTAINS", Color.White);
            t.SetFont(littleFont);
            _notOnlineMenu.Add(t);
            t = new UIText("OFFLINE ONLY STUFF.", Color.White);
            t.SetFont(littleFont);
            _notOnlineMenu.Add(t);
            t = new UIText(" ", Color.White);
            t.SetFont(littleFont);
            _notOnlineMenu.Add(t);
            _notOnlineMenu.Add(new UIMenuItem("OH", new UIMenuActionOpenMenu(_confirmMenu, _returnMenu), UIAlign.Center, default(Color), backButton: true));
        }
        else
        {
            _confirmMenu.Add(new UIMenuItem("WHAT? NO!", new UIMenuActionCloseMenu(_confirmMenu), UIAlign.Center, default(Color), backButton: true));
            _confirmMenu.Add(new UIMenuItem("YEAH!", new UIMenuActionCloseMenuSetBoolean(_confirmMenu, _deleteFile)));
        }
        _confirmMenu.Close();
        Level.Add(_confirmMenu);
    }

    public void AddItem(LSItem item)
    {
        _items.Add(item);
    }

    public void PositionItems()
    {
        int index = 0;
        int drawIndex = 0;
        foreach (LSItem item in _items)
        {
            if (index >= _topIndex + _maxItems || index < _topIndex)
            {
                item.visible = false;
                index++;
                continue;
            }
            item.visible = true;
            item.x = _leftPos;
            item.y = _topPos + (float)(drawIndex * 10);
            if (index == _selectedItem)
            {
                item.selected = true;
                _selectedLevel = item;
            }
            else
            {
                item.selected = false;
            }
            index++;
            drawIndex++;
        }
    }

    public void FolderUp()
    {
        FolderUp(pIsModPath: false);
    }

    public void FolderUp(bool pIsModPath)
    {
        if (_currentDirectory == "@WORKSHOP@" || (modRoot != null && modRoot.Contains(_currentDirectory)) || _currentDirectory == "@VANILLA@")
        {
            SetCurrentFolder(_rootDirectory);
            modRoot = null;
        }
        else
        {
            int lastSlash = _currentDirectory.LastIndexOf('/');
            string newDirectory = _currentDirectory.Substring(0, lastSlash);
            SetCurrentFolder(newDirectory, pIsModPath, isModRoot: false);
        }
    }

    public void HUDRefresh()
    {
        showPlaylistOption = false;
        HUDBottomRightSetup();
        HUDTopRightSetup();
    }

    private void HUDBottomRightSetup()
    {
        HUD.CloseCorner(HUDCorner.BottomRight);
        if (_selectedLevel.itemType == LSItemType.UpFolder)
        {
            HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@RETURN");
        }
        else if (_selectedLevel.itemType == LSItemType.Folder || _selectedLevel.itemType == LSItemType.Playlist || _selectedLevel.itemType == LSItemType.Workshop || _selectedLevel.itemType == LSItemType.Vanilla)
        {
            HUD.AddCornerControl(HUDCorner.BottomRight, "@MENU1@TOGGLE");
            HUD.AddCornerControl(HUDCorner.BottomRight, "@SELECT@OPEN", null, allowStacking: true);
        }
        else
        {
            HUD.AddCornerControl(HUDCorner.BottomRight, "@MENU1@TOGGLE");
        }
    }

    private void HUDTopRightSetup()
    {
        HUD.CloseCorner(HUDCorner.TopRight);
        if (_currentDirectory == _rootDirectory)
        {
            HUD.AddCornerControl(HUDCorner.TopRight, "@CANCEL@DONE");
            HUD.AddCornerControl(HUDCorner.TopRight, "@MENU2@DELETE", null, allowStacking: true);
            return;
        }
        HUD.AddCornerControl(HUDCorner.TopRight, "@CANCEL@RETURN");
        if (modRoot == null && _currentDirectory != "@VANILLA@")
        {
            HUD.AddCornerControl(HUDCorner.TopRight, "@MENU2@DELETE", null, allowStacking: true);
        }
    }

    public override void Update()
    {
        HUD.CloseCorner(HUDCorner.TopLeft);
        _dialog.DoUpdate();
        if (_dialog.opened)
        {
            return;
        }
        Editor.lockInput = null;
        if (_dialog.result != null && _dialog.result != "")
        {
            string result = _dialog.result;
            LevelPlaylist newPlaylist = new LevelPlaylist();
            newPlaylist.levels.AddRange(Editor.activatedLevels);
            DuckXML duckXML = new DuckXML();
            duckXML.Add(newPlaylist.Serialize());
            DuckFile.SaveDuckXML(duckXML, DuckFile.levelDirectory + result + ".play");
            SetCurrentFolder(_rootDirectory);
            _dialog.result = null;
            return;
        }
        if (_selectedLevel == null)
        {
            _exiting = true;
        }
        if (Editor.activatedLevels.Count > 0)
        {
            if (!showPlaylistOption)
            {
                showPlaylistOption = true;
                HUD.AddCornerControl(HUDCorner.BottomLeft, "@RAGDOLL@NEW PLAYLIST");
            }
        }
        else if (showPlaylistOption)
        {
            showPlaylistOption = false;
            HUD.CloseCorner(HUDCorner.BottomLeft);
        }
        if (_deleteFile.value)
        {
            foreach (string l in _selectedLevel.levelsInside)
            {
                Editor.activatedLevels.Remove(l);
            }
            Editor.activatedLevels.Remove(_selectedLevel.path);
            if (_selectedLevel.itemType == LSItemType.Folder)
            {
                DuckFile.DeleteFolder(DuckFile.levelDirectory + _selectedLevel.path);
            }
            else if (_selectedLevel.itemType == LSItemType.Playlist)
            {
                DuckFile.Delete(DuckFile.levelDirectory + _selectedLevel.path);
            }
            else
            {
                Editor.Delete(_selectedLevel.path);
            }
            Thread.Sleep(100);
            SetCurrentFolder(_currentDirectory);
            _deleteFile.value = false;
        }
        if (_exiting)
        {
            HUD.CloseAllCorners();
            Graphics.fade = Lerp.Float(Graphics.fade, 0f, 0.04f);
            if (Graphics.fade < 0.01f)
            {
                isClosed = true;
            }
            return;
        }
        Graphics.fade = Lerp.Float(Graphics.fade, 1f, 0.04f);
        if (Input.Pressed("MENUUP"))
        {
            if (_selectedItem > 0)
            {
                _selectedItem--;
            }
            else
            {
                _selectedItem = _items.Count() - 1;
            }
            if (_selectedItem < _topIndex)
            {
                _topIndex = _selectedItem;
            }
            if (_selectedItem >= _topIndex + _maxItems)
            {
                _topIndex = _selectedItem + 1 - _maxItems;
            }
        }
        else if (Input.Pressed("MENUDOWN"))
        {
            if (_selectedItem < _items.Count() - 1)
            {
                _selectedItem++;
            }
            else
            {
                _selectedItem = 0;
            }
            if (_selectedItem < _topIndex)
            {
                _topIndex = _selectedItem;
            }
            if (_selectedItem >= _topIndex + _maxItems)
            {
                _topIndex = _selectedItem + 1 - _maxItems;
            }
        }
        else if (Input.Pressed("MENULEFT"))
        {
            _selectedItem -= _maxItems - 1;
            if (_selectedItem < 0)
            {
                _selectedItem = 0;
            }
            if (_selectedItem < _topIndex)
            {
                _topIndex = _selectedItem;
            }
        }
        else if (Input.Pressed("MENURIGHT"))
        {
            _selectedItem += _maxItems - 1;
            if (_selectedItem > _items.Count() - 1)
            {
                _selectedItem = _items.Count() - 1;
            }
            if (_selectedItem >= _topIndex + _maxItems)
            {
                _topIndex = _selectedItem + 1 - _maxItems;
            }
        }
        else if (Input.Pressed("MENU1"))
        {
            if (_selectedLevel.itemType != LSItemType.UpFolder)
            {
                if (_selectedLevel.isFolder || _selectedLevel.itemType == LSItemType.Playlist || _selectedLevel.itemType == LSItemType.Workshop || _selectedLevel.itemType == LSItemType.Vanilla)
                {
                    if (!_selectedLevel.enabled)
                    {
                        _selectedLevel.enabled = true;
                        _selectedLevel.partiallyEnabled = false;
                        Editor.activatedLevels.AddRange(_selectedLevel.levelsInside);
                    }
                    else
                    {
                        _selectedLevel.enabled = false;
                        _selectedLevel.partiallyEnabled = false;
                        foreach (string level in _selectedLevel.levelsInside)
                        {
                            Editor.activatedLevels.Remove(level);
                        }
                    }
                }
                else if (Editor.activatedLevels.Contains(_selectedLevel.path))
                {
                    Editor.activatedLevels.Remove(_selectedLevel.path);
                }
                else
                {
                    Editor.activatedLevels.Add(_selectedLevel.path);
                }
            }
        }
        else if (Input.Pressed("SELECT"))
        {
            if (_selectedLevel.itemType == LSItemType.Workshop || _selectedLevel.itemType == LSItemType.Vanilla)
            {
                SetCurrentFolder(_selectedLevel.path);
            }
            else if (_selectedLevel.isFolder || _selectedLevel.itemType == LSItemType.Playlist)
            {
                if (_selectedLevel.isModRoot || _selectedLevel.isModPath)
                {
                    SetCurrentFolder(_selectedLevel.path, _selectedLevel.isModPath, _selectedLevel.isModRoot, _selectedLevel.mapPack);
                }
                else
                {
                    SetCurrentFolder(_rootDirectory + _selectedLevel.path);
                }
            }
            else if (_selectedLevel.itemType == LSItemType.UpFolder)
            {
                FolderUp(_selectedLevel.isModPath);
            }
        }
        else if (Input.Pressed("CANCEL"))
        {
            if (_currentDirectory != _rootDirectory)
            {
                FolderUp(_selectedLevel.isModPath);
            }
            else
            {
                _exiting = true;
            }
        }
        else if (Input.Pressed("RAGDOLL"))
        {
            _dialog.Open("New Playlist...");
            Editor.lockInput = _dialog;
        }
        else if (Input.Pressed("MENU2") && modRoot == null && _currentDirectory != "@VANILLA@" && _selectedLevel.path != "@VANILLA@" && _currentDirectory != "@WORKSHOP@" && mapPack == null && MonoMain.pauseMenu != _confirmMenu && _selectedLevel.itemType != LSItemType.UpFolder && _selectedLevel.itemType != LSItemType.Workshop && _selectedLevel.itemType != LSItemType.MapPack)
        {
            _skipCompanionOpening = true;
            MonoMain.pauseMenu = _confirmMenu;
            HUD.CloseAllCorners();
            _confirmMenu.Open();
            SFX.Play("pause", 0.6f);
        }
        PositionItems();
        if (_selectedLevel != _lastSelection)
        {
            if (_lastSelection == null || _selectedLevel.itemType != _lastSelection.itemType)
            {
                HUDBottomRightSetup();
            }
            _lastSelection = _selectedLevel;
        }
        if (_selectedLevel != _previewItem)
        {
            if (_selectedLevel.itemType == LSItemType.Level)
            {
                LevelMetaData.PreviewPair pair = Content.GeneratePreview(_selectedLevel.path);
                if (pair != null)
                {
                    _preview = pair.preview;
                    if (_preview != null)
                    {
                        _previewSprite = new Sprite(_preview);
                    }
                    else
                    {
                        _previewSprite = null;
                    }
                }
                else
                {
                    _previewSprite = null;
                }
            }
            else
            {
                _previewSprite = null;
            }
            _previewItem = _selectedLevel;
        }
        foreach (LSItem item in _items)
        {
            item.Update();
        }
    }

    public void DrawThings(bool drawBack = false)
    {
        if (drawBack)
        {
            Graphics.DrawRect(new Vec2(0f, 0f), new Vec2(Layer.HUD.camera.width, Layer.HUD.camera.height), Color.Black, -0.8f);
        }
        foreach (LSItem item in _items)
        {
            if (item.visible)
            {
                item.Draw();
            }
        }
        Depth deep = _font.depth;
        if (_previewSprite != null)
        {
            _previewSprite.scale = new Vec2(0.5f, 0.5f);
            _previewSprite.depth = 0.9f;
            Graphics.Draw(_previewSprite, 150f, 45f);
        }
        else if (_selectedLevel.mapPack != null && _selectedLevel.mapPack.preview != null)
        {
            Tex2D tex = _selectedLevel.mapPack.preview;
            Vec2 scale = new Vec2(320f / (float)tex.width, 180f / (float)tex.height) * 0.5f;
            Graphics.Draw(tex, 150f, 45f, scale.x, scale.y);
        }
        _font.depth = deep;
        string path = "";
        path = ((mapPack != null) ? mapPack.name : ((_currentDirectory == "@WORKSHOP@") ? "Levels/Workshop" : ((!(_currentDirectory == "@VANILLA@")) ? ("Levels" + _currentDirectory.Substring(_rootDirectory.Length, _currentDirectory.Length - _rootDirectory.Length)) : "Levels/Deathmatch")));
        _font.Draw(path, _leftPos, _topPos - 10f, Color.LimeGreen);
        _dialog.DoDraw();
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.HUD)
        {
            DrawThings();
        }
        base.PostDrawLayer(layer);
    }

    public override void Terminate()
    {
        _items.Clear();
        InputProfile.repeat = false;
        Keyboard.repeat = false;
    }
}
