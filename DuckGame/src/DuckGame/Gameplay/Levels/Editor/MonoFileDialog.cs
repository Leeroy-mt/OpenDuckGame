using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;

namespace DuckGame;

public class MonoFileDialog : ContextMenu
{
    private bool _save;

    private string _currentDirectory;

    private string _rootFolder;

    private bool _scrollBar;

    private float _scrollPosition;

    private float _scrollLerp;

    private int _maxItems = 15;

    private float _scrollWait;

    private bool _doDeleteDialog;

    private bool _doOverwriteDialog;

    private new ContextFileType _type;

    private Sprite _badTileset;

    private Sprite _badParallax;

    private Sprite _badArcade;

    private FancyBitmapFont _fancyFont;

    private TextEntryDialog _dialog;

    private MessageDialogue _deleteDialog;

    private MessageDialogue _overwriteDialog;

    private string _overwriteName = "";

    private bool _selectLevels;

    private bool _loadLevel;

    public string result;

    private float hOffset = -86f;

    private float _percentStorageUsed;

    private float _fdHeight = 262f;

    private string modRootPath;

    private string prevDirectory;

    private ZipArchive _openedArchive;

    private int _framesSinceSelected = 999;

    private ContextMenu _lastItemSelected;

    private string _prevPreviewPath = "";

    private string _previewName = "";

    private LevelMetaData.PreviewPair _previewPair;

    private Tex2D _preview;

    private Sprite _previewSprite;

    private byte[] _currentPreviewZipData;

    public bool drag;

    public string rootFolder => _rootFolder;

    public MonoFileDialog()
        : base(null)
    {
    }

    public override void Initialize()
    {
        base.layer = Layer.HUD;
        base.depth = 0.9f;
        _showBackground = false;
        _fancyFont = new FancyBitmapFont("smallFont");
        itemSize = new Vec2(390f, 16f);
        _root = true;
        _dialog = new TextEntryDialog();
        _dialog.filename = true;
        Level.Add(_dialog);
        _deleteDialog = new MessageDialogue();
        Level.Add(_deleteDialog);
        _overwriteDialog = new MessageDialogue();
        Level.Add(_overwriteDialog);
        drawControls = false;
    }

    public void Open(string rootFolder, string currentFolder, bool save, bool selectLevels = false, bool loadLevel = true, ContextFileType type = ContextFileType.Level)
    {
        _type = type;
        _selectLevels = selectLevels;
        _loadLevel = loadLevel;
        if (_type == ContextFileType.Block || _type == ContextFileType.Background || _type == ContextFileType.Platform)
        {
            _badTileset = new Sprite("badTileset");
        }
        if (_type == ContextFileType.Parallax)
        {
            _badParallax = new Sprite("badParallax");
        }
        if (_type == ContextFileType.ArcadeStyle)
        {
            _badArcade = new Sprite("badArcade");
        }
        _preview = null;
        _previewSprite = null;
        float windowWidth = 350f;
        float windowHeight = 350f;
        Vec2 topLeft = new Vec2(base.layer.width / 2f - windowWidth / 2f + hOffset, base.layer.height / 2f - windowHeight / 2f);
        new Vec2(base.layer.width / 2f + windowWidth / 2f + hOffset, base.layer.height / 2f + windowHeight / 2f);
        position = topLeft + new Vec2(4f, 40f);
        _save = save;
        rootFolder = rootFolder.Replace('\\', '/');
        currentFolder = currentFolder.Replace('\\', '/');
        if (currentFolder == "")
        {
            _currentDirectory = rootFolder;
        }
        else
        {
            _currentDirectory = currentFolder;
        }
        _rootFolder = rootFolder;
        if (prevDirectory != null)
        {
            _currentDirectory = prevDirectory;
        }
        SetDirectory(_currentDirectory);
        Editor.lockInput = this;
        ComputeAvailableStorageSpace();
        SFX.Play("openClick", 0.4f);
        base.opened = true;
    }

    private void ComputeAvailableStorageSpace()
    {
        float percentUsed = 0f;
        if (DuckFile.GetLevelSpacePercentUsed(ref percentUsed))
        {
            _percentStorageUsed = ((percentUsed > 100f) ? 100f : percentUsed);
        }
    }

    public void Close()
    {
        Editor.lockInput = null;
        base.opened = false;
        ClearItems();
    }

    public string TypeExtension()
    {
        if (_type == ContextFileType.Level)
        {
            return ".lev";
        }
        if (_type == ContextFileType.Block || _type == ContextFileType.Background || _type == ContextFileType.Platform || _type == ContextFileType.Parallax || _type == ContextFileType.ArcadeAnimation || _type == ContextFileType.ArcadeStyle)
        {
            return ".png";
        }
        return "";
    }

    public void SetDirectory(string dir)
    {
        SetDirectory(dir, pIsModPath: false);
    }

    private string FixPath(string pPath)
    {
        string path = Path.GetFullPath(pPath);
        path = path.Replace('\\', '/');
        while (path.StartsWith("/"))
        {
            path = path.Substring(1);
        }
        if (path.EndsWith("/"))
        {
            path = path.Substring(0, path.Length - 1);
        }
        return path;
    }

    public void SetDirectory(string dir, bool pIsModPath)
    {
        if (_openedArchive != null)
        {
            _openedArchive.Dispose();
            _openedArchive = null;
        }
        DevConsole.Log("MonoFileDialog.SetDirectory(" + dir + ")");
        if (!pIsModPath && dir.Contains("Mods") && (dir.Contains(DuckFile.globalModsDirectory) || dir.Contains(DuckFile.modsDirectory)))
        {
            pIsModPath = true;
        }
        isModPath = pIsModPath;
        dir = FixPath(dir);
        _drawIndex = 0;
        DevConsole.Log("MonoFileDialog.SetDirectory(postfix)(" + dir + ")");
        if (modRootPath != null && modRootPath == dir)
        {
            dir = _rootFolder;
            modRootPath = null;
            isModPath = false;
        }
        if (dir.Length < _rootFolder.Length && !pIsModPath)
        {
            dir = _rootFolder;
        }
        int numItems = 0;
        _currentDirectory = dir;
        if (_currentDirectory != _rootFolder)
        {
            numItems++;
        }
        if (_save)
        {
            numItems++;
        }
        prevDirectory = dir;
        string[] dirs = null;
        string[] files = null;
        dirs = DuckFile.GetDirectories(_currentDirectory);
        files = DuckFile.GetFiles(_currentDirectory);
        numItems += dirs.Length + files.Length;
        Array.Sort(dirs);
        Array.Sort(files);
        float itemWidth = 338f;
        _scrollBar = false;
        _scrollPosition = 0f;
        if (numItems > _maxItems)
        {
            itemWidth = 326f;
            _scrollBar = true;
        }
        if (_save)
        {
            ContextMenu dirItem = new ContextMenu(this);
            dirItem.layer = base.layer;
            dirItem.text = "@NEWICONTINY@New File...";
            dirItem.data = "New File...";
            dirItem.itemSize = new Vec2(itemWidth, 16f);
            AddItem(dirItem);
        }
        if (_currentDirectory != _rootFolder)
        {
            ContextMenu dirItem2 = new ContextMenu(this);
            dirItem2.layer = base.layer;
            bool modPath = false;
            dirItem2.text = "@LOADICON@../";
            dirItem2.data = "../";
            dirItem2.itemSize = new Vec2(itemWidth, 16f);
            dirItem2.isModPath = modPath;
            AddItem(dirItem2);
        }
        else
        {
            foreach (Mod m in ModLoader.accessibleMods)
            {
                if (m.configuration != null && m.configuration.content != null && m.configuration.content.levels.Count > 0 && m.configuration.contentDirectory.Contains(DuckFile.saveDirectory))
                {
                    ContextMenu dirItem3 = new ContextMenu(this);
                    dirItem3.layer = base.layer;
                    dirItem3.fancy = true;
                    dirItem3.text = "@RAINBOWTINY@|DGBLUE|" + m.configuration.name;
                    dirItem3.data = m.configuration.contentDirectory + "/Levels";
                    dirItem3.itemSize = new Vec2(itemWidth, 16f);
                    dirItem3.mod = m;
                    dirItem3.isModPath = true;
                    dirItem3.isModRoot = true;
                    AddItem(dirItem3);
                }
            }
            foreach (MapPack m2 in MapPack.active)
            {
                if (m2.mod != null && m2.mod.configuration.directory.Contains(DuckFile.saveDirectory))
                {
                    ContextMenu dirItem4 = new ContextMenu(this);
                    dirItem4.layer = base.layer;
                    dirItem4.fancy = true;
                    dirItem4.text = "|DGBLUE|" + m2.mod.configuration.name;
                    dirItem4.data = m2.mod.configuration.directory;
                    dirItem4.itemSize = new Vec2(itemWidth, 16f);
                    dirItem4.mod = m2.mod;
                    dirItem4.isModPath = true;
                    dirItem4.isModRoot = true;
                    dirItem4.customIcon = m2.mod.configuration.mapPack.icon;
                    AddItem(dirItem4);
                }
            }
        }
        string[] array = dirs;
        foreach (string d in array)
        {
            string name = Path.GetFileName(d);
            ContextMenu dirItem5 = new ContextMenu(this);
            dirItem5.layer = base.layer;
            dirItem5.fancy = true;
            dirItem5.text = "@LOADICON@" + name;
            if (pIsModPath)
            {
                dirItem5.data = d;
            }
            else
            {
                dirItem5.data = name;
            }
            dirItem5.isModPath = pIsModPath;
            dirItem5.itemSize = new Vec2(itemWidth, 16f);
            AddItem(dirItem5);
        }
        int idx = 0;
        array = files;
        for (int i = 0; i < array.Length; i++)
        {
            string d2 = array[i];
            if (d2.StartsWith("|"))
            {
                d2 = d2.Substring(1, d2.Length - 1);
            }
            string name2 = Path.GetFileName(d2);
            if (!_selectLevels)
            {
                if (name2.EndsWith(TypeExtension()))
                {
                    ContextMenu dirItem6 = new ContextMenu(this);
                    dirItem6.layer = base.layer;
                    dirItem6.fancy = true;
                    dirItem6.text = name2;
                    dirItem6.text = name2.Substring(0, name2.Length - 4);
                    if (pIsModPath)
                    {
                        dirItem6.data = d2;
                    }
                    else
                    {
                        dirItem6.data = name2;
                    }
                    dirItem6.itemSize = new Vec2(itemWidth, 16f);
                    dirItem6.isModPath = pIsModPath;
                    AddItem(dirItem6);
                }
            }
            else
            {
                string path = d2.Replace('\\', '/');
                path = path.Substring(0, path.Length - 4);
                string shortName = path.Substring(path.IndexOf("/levels/", StringComparison.InvariantCultureIgnoreCase) + 8);
                ContextCheckBox dirItem7 = new ContextCheckBox(name2, this);
                dirItem7.layer = base.layer;
                dirItem7.fancy = true;
                dirItem7.path = shortName;
                dirItem7.isChecked = Editor.activatedLevels.Contains(shortName);
                dirItem7.itemSize = new Vec2(itemWidth, 16f);
                AddItem(dirItem7);
            }
            idx++;
        }
        int offset = (int)Math.Round((float)(_items.Count - 1 - _maxItems) * _scrollPosition);
        int index = 0;
        for (int j = 0; j < _items.Count; j++)
        {
            if (j < offset || j > offset + _maxItems)
            {
                _items[j].visible = false;
                continue;
            }
            _items[j].visible = true;
            _items[j].position = new Vec2(_items[j].position.x, base.y + 3f + (float)index * (_items[j].itemSize.y + 1f));
            index++;
        }
        menuSize.y = _fdHeight;
    }

    private Tex2D GetArcadeSizeTex2D(string pTex, Tex2D pOriginalTex)
    {
        if (pOriginalTex.width != 48 || pOriginalTex.height != 48)
        {
            Image image = Image.FromFile(_currentDirectory + "/" + Path.GetFileName(pTex));
            System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(0, 0, 48, 48);
            Bitmap destImage = new Bitmap(48, 48);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using ImageAttributes wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
            MemoryStream stream = new MemoryStream();
            destImage.Save(stream, ImageFormat.Png);
            return Texture2D.FromStream(Graphics.device, stream);
        }
        return pOriginalTex;
    }

    public override void Selected(ContextMenu item)
    {
        bool force = false;
        if (_framesSinceSelected < 20 && _lastItemSelected == item)
        {
            force = true;
        }
        _lastItemSelected = item;
        _framesSinceSelected = 0;
        if (!force && Editor.inputMode == EditorInput.Touch)
        {
            return;
        }
        if (_percentStorageUsed >= 100f && (_save || item.text == "@NEWICONTINY@New File..."))
        {
            SFX.Play("consoleError");
            return;
        }
        SFX.Play("highClick", 0.3f, 0.2f);
        if (item.text == "@NEWICONTINY@New File...")
        {
            _dialog.Open("Save File As...");
            Editor.lockInput = _dialog;
        }
        else if (item.data.EndsWith(TypeExtension()) && _type != ContextFileType.All)
        {
            if (!_selectLevels)
            {
                if (!_save)
                {
                    Close();
                    string t = _currentDirectory + "/" + item.data;
                    if (item.isModPath)
                    {
                        t = item.data;
                    }
                    if (_loadLevel)
                    {
                        (Level.current as Editor).LoadLevel(t);
                    }
                    else if (_type == ContextFileType.ArcadeStyle)
                    {
                        result = Editor.TextureToString(GetArcadeSizeTex2D(t, Content.Load<Tex2D>(t)));
                    }
                    else
                    {
                        result = t.Replace(_rootFolder, "");
                    }
                }
                else
                {
                    _overwriteDialog.Open("OVERWRITE " + item.data + "?");
                    Editor.lockInput = _overwriteDialog;
                    _doOverwriteDialog = true;
                    _overwriteDialog.result = false;
                    _overwriteName = item.data;
                    Editor.tookInput = true;
                }
            }
            else if (item is ContextCheckBox box)
            {
                box.isChecked = !box.isChecked;
                if (!box.isChecked)
                {
                    Editor.activatedLevels.Remove(box.path);
                }
                else
                {
                    Editor.activatedLevels.Add(box.path);
                }
            }
        }
        else
        {
            ClearItems();
            if (item.isModPath)
            {
                if (item.isModRoot)
                {
                    modRootPath = FixPath(item.data + "/../");
                }
                SetDirectory(item.data, item.isModPath);
            }
            else
            {
                SetDirectory(_currentDirectory + "/" + item.data);
            }
        }
        ComputeAvailableStorageSpace();
    }

    public override void Toggle(ContextMenu item)
    {
    }

    public override void Update()
    {
        _framesSinceSelected++;
        if (!base.opened || _dialog.opened || _deleteDialog.opened || _overwriteDialog.opened || _opening)
        {
            _opening = false;
            {
                foreach (ContextMenu item2 in _items)
                {
                    item2.disabled = true;
                }
                return;
            }
        }
        bool didHover = false;
        foreach (ContextMenu menu in _items)
        {
            menu.disabled = false;
            if (didHover || !menu.hover)
            {
                continue;
            }
            didHover = true;
            string path = "";
            int rootIndex = _currentDirectory.IndexOf(_rootFolder);
            if (rootIndex != -1)
            {
                path = _currentDirectory.Remove(rootIndex, _rootFolder.Length);
            }
            if (path != "" && !path.EndsWith("/"))
            {
                path += "/";
            }
            if (path.StartsWith("/"))
            {
                path = path.Substring(1, path.Length - 1);
            }
            path = _rootFolder + "/" + path + menu.data;
            if (menu.isModPath)
            {
                path = menu.data;
            }
            bool needsPreview = true;
            if (!(_prevPreviewPath != path))
            {
                continue;
            }
            if (path.EndsWith(".lev"))
            {
                _previewName = menu.data;
                try
                {
                    if (menu.zipItem != null)
                    {
                        ZipArchiveEntry zip = menu.zipItem as ZipArchiveEntry;
                        _currentPreviewZipData = new byte[zip.Length];
                        zip.Open().Read(_currentPreviewZipData, 0, (int)zip.Length);
                        Content.generatePreviewBytes = _currentPreviewZipData;
                    }
                    if (_previewPair != null && _previewPair.preview != null)
                    {
                        _previewPair.preview.Dispose();
                    }
                    _previewPair = Content.GeneratePreview(path);
                }
                catch (Exception)
                {
                    _previewPair = null;
                    needsPreview = false;
                    _previewSprite = null;
                }
                if (_previewPair != null)
                {
                    _preview = _previewPair.preview;
                }
            }
            else if (path.EndsWith(".png"))
            {
                string previewPath = _currentDirectory + "/" + Path.GetFileName(path);
                Texture2D tex = ContentPack.LoadTexture2D(previewPath);
                if (tex != null)
                {
                    if ((_type == ContextFileType.Block || _type == ContextFileType.Background || _type == ContextFileType.Platform) && (tex.Width != 128 || tex.Height != 128))
                    {
                        _preview = _badTileset.texture;
                    }
                    else if (_type == ContextFileType.Parallax && (tex.Width != 320 || tex.Height != 240))
                    {
                        _preview = _badParallax.texture;
                    }
                    else if (_type == ContextFileType.ArcadeStyle)
                    {
                        _preview = GetArcadeSizeTex2D(previewPath, tex);
                    }
                    else
                    {
                        _preview = tex;
                    }
                }
                else
                {
                    _preview = Content.invalidTexture;
                }
            }
            else
            {
                _prevPreviewPath = null;
                needsPreview = false;
            }
            if (needsPreview)
            {
                _previewSprite = new Sprite(_preview);
                if (_type == ContextFileType.Block || _type == ContextFileType.Background || _type == ContextFileType.Platform || _type == ContextFileType.Parallax)
                {
                    _previewSprite.scale = new Vec2(2f, 2f);
                }
            }
            else
            {
                _previewSprite = null;
            }
            _prevPreviewPath = path;
        }
        if (!didHover && _type == ContextFileType.ArcadeStyle)
        {
            _preview = _badArcade.texture;
            _previewSprite = new Sprite(_preview);
            _prevPreviewPath = null;
        }
        Editor.lockInput = this;
        base.Update();
        _scrollWait = Lerp.Float(_scrollWait, 0f, 0.2f);
        if (_dialog.result != null && _dialog.result != "")
        {
            string[] matchingFiles = DuckFile.GetFiles(_currentDirectory, _dialog.result + ".lev");
            if (matchingFiles != null && matchingFiles.Length != 0)
            {
                _overwriteDialog.Open("OVERWRITE " + _dialog.result + "?");
                Editor.lockInput = _overwriteDialog;
                _doOverwriteDialog = true;
                _overwriteDialog.result = false;
                _overwriteName = _dialog.result;
                _dialog.result = "";
            }
            else
            {
                Editor obj = Level.current as Editor;
                Editor._currentLevelData.metaData.guid = Guid.NewGuid().ToString();
                Editor._currentLevelData.workshopData.Reset();
                obj.DoSave(_currentDirectory + "/" + _dialog.result);
                _dialog.result = "";
                Close();
            }
        }
        if (!_overwriteDialog.opened && _doOverwriteDialog)
        {
            _doOverwriteDialog = false;
            if (_overwriteDialog.result)
            {
                Editor e = Level.current as Editor;
                try
                {
                    LevelData overwriting = DuckFile.LoadLevel(_currentDirectory + "/" + _overwriteName);
                    if (overwriting == null)
                    {
                        throw new Exception();
                    }
                    Editor._currentLevelData.metaData.guid = overwriting.metaData.guid;
                }
                catch (Exception)
                {
                    if (string.IsNullOrEmpty(Editor._currentLevelData.metaData.guid))
                    {
                        Editor._currentLevelData.metaData.guid = Guid.NewGuid().ToString();
                    }
                }
                e.DoSave(_currentDirectory + "/" + _overwriteName);
                _overwriteDialog.result = false;
                _overwriteName = "";
                Close();
            }
        }
        if (!_deleteDialog.opened && _doDeleteDialog)
        {
            _doDeleteDialog = false;
            if (_deleteDialog.result)
            {
                foreach (ContextMenu item in _items)
                {
                    if (item.hover)
                    {
                        Editor.Delete(_currentDirectory + "/" + item.text + ".lev");
                        ComputeAvailableStorageSpace();
                        break;
                    }
                }
                ClearItems();
                SetDirectory(_currentDirectory);
            }
        }
        if (Keyboard.Pressed(Keys.Escape) || Mouse.right == InputState.Pressed || Input.Pressed("CANCEL"))
        {
            Close();
        }
        if (Input.Down("STRAFE") && Input.Pressed("RAGDOLL"))
        {
            try
            {
                Process.Start(Path.GetFullPath(_currentDirectory));
            }
            catch (Exception ex3)
            {
                try
                {
                    Process.Start(_currentDirectory);
                }
                catch (Exception)
                {
                    DevConsole.Log("|DGRED|Could not open directory '" + Path.GetFullPath(_currentDirectory) + "' (" + ex3.Message + ")");
                }
            }
        }
        if (!_selectLevels && Input.Pressed("MENU2"))
        {
            _deleteDialog.Open("CONFIRM DELETE");
            Editor.lockInput = _deleteDialog;
            _doDeleteDialog = true;
            _deleteDialog.result = false;
            return;
        }
        if (Input.Pressed("MENULEFT"))
        {
            _selectedIndex -= _maxItems;
        }
        else if (Input.Pressed("MENURIGHT"))
        {
            _selectedIndex += _maxItems;
        }
        _selectedIndex = Maths.Clamp(_selectedIndex, 0, _items.Count - 1);
        float scrollAmount = 1f / (float)(_items.Count - _maxItems);
        if (Mouse.scroll != 0f)
        {
            _scrollPosition += (float)Math.Sign(Mouse.scroll) * scrollAmount;
            if (_scrollPosition > 1f)
            {
                _scrollPosition = 1f;
            }
            if (_scrollPosition < 0f)
            {
                _scrollPosition = 0f;
            }
        }
        bool keyScroll = false;
        int offset = (int)Math.Round(((float)(_items.Count - _maxItems) - 1f) * _scrollPosition);
        int index = 0;
        int hoverIndex = 0;
        for (int i = 0; i < _items.Count; i++)
        {
            if (keyScroll)
            {
                _items[i].hover = false;
            }
            if (_items[i].hover)
            {
                hoverIndex = i;
                break;
            }
        }
        if (Editor.inputMode == EditorInput.Gamepad && !keyScroll)
        {
            if (hoverIndex > offset + _maxItems)
            {
                _scrollPosition += (float)(hoverIndex - (offset + _maxItems)) * scrollAmount;
            }
            else if (hoverIndex < offset)
            {
                _scrollPosition -= (float)(offset - hoverIndex) * scrollAmount;
            }
        }
        for (int j = 0; j < _items.Count; j++)
        {
            _items[j].disabled = false;
            if (j < offset || j > offset + _maxItems)
            {
                _items[j].visible = false;
                _items[j].hover = false;
                continue;
            }
            _ = _items[j];
            _items[j].visible = true;
            _items[j].position = new Vec2(_items[j].position.x, base.y + 3f + (float)index * _items[j].itemSize.y);
            index++;
        }
    }

    public override void Draw()
    {
        menuSize.y = _fdHeight;
        if (!base.opened)
        {
            return;
        }
        base.Draw();
        float width = 350f;
        float height = _fdHeight + 22f;
        Vec2 topLeft = new Vec2(base.layer.width / 2f - width / 2f + hOffset - 1f, base.layer.height / 2f - height / 2f - 15f);
        Vec2 bottomRight = new Vec2(base.layer.width / 2f + width / 2f + hOffset + 1f, base.layer.height / 2f + height / 2f - 12f);
        Graphics.DrawRect(topLeft, bottomRight, new Color(70, 70, 70), base.depth, filled: false);
        Graphics.DrawRect(topLeft, bottomRight, new Color(30, 30, 30), base.depth - 8);
        Graphics.DrawRect(topLeft + new Vec2(3f, 23f), bottomRight + new Vec2(-18f, -4f), new Color(10, 10, 10), base.depth - 4);
        Vec2 scrollBarTL = new Vec2(bottomRight.x - 16f, topLeft.y + 23f);
        Vec2 scrollBarBR = bottomRight + new Vec2(-3f, -4f);
        Graphics.DrawRect(scrollBarTL, scrollBarBR, new Color(10, 10, 10), base.depth - 4);
        Graphics.DrawRect(topLeft + new Vec2(3f, 3f), new Vec2(bottomRight.x - 3f, topLeft.y + 19f), new Color(70, 70, 70), base.depth - 4);
        if (_scrollBar)
        {
            _scrollLerp = Lerp.Float(_scrollLerp, _scrollPosition, 0.05f);
            Vec2 barTL = new Vec2(bottomRight.x - 14f, base.topRight.y + 7f + (240f * _scrollLerp - 4f));
            Vec2 barBR = new Vec2(bottomRight.x - 5f, base.topRight.y + 11f + (240f * _scrollLerp + 8f));
            bool hover = false;
            if (Mouse.x > barTL.x && Mouse.x < barBR.x && Mouse.y > barTL.y && Mouse.y < barBR.y)
            {
                hover = true;
                if (Mouse.left == InputState.Pressed)
                {
                    drag = true;
                }
            }
            if (Mouse.left == InputState.None)
            {
                drag = false;
            }
            if (drag)
            {
                _scrollPosition = (Mouse.y - scrollBarTL.y - 10f) / (scrollBarBR.y - scrollBarTL.y - 20f);
                if (_scrollPosition < 0f)
                {
                    _scrollPosition = 0f;
                }
                if (_scrollPosition > 1f)
                {
                    _scrollPosition = 1f;
                }
                _scrollLerp = _scrollPosition;
            }
            Graphics.DrawRect(barTL, barBR, drag ? new Color(190, 190, 190) : (hover ? new Color(120, 120, 120) : new Color(70, 70, 70)), base.depth + 4);
        }
        string path = _currentDirectory;
        int rootIndex = _currentDirectory.IndexOf(_rootFolder);
        if (rootIndex != -1)
        {
            path = _currentDirectory.Remove(rootIndex, _rootFolder.Length);
        }
        path = Path.GetFileName(_rootFolder) + path;
        if (isModPath)
        {
            path = _currentDirectory.Replace(DuckFile.modsDirectory, "");
            path = path.Replace(DuckFile.globalModsDirectory, "");
        }
        if (path == "")
        {
            path = ((_type == ContextFileType.Block) ? "Custom/Blocks" : ((_type == ContextFileType.Platform) ? "Custom/Platform" : ((_type == ContextFileType.Background) ? "Custom/Background" : ((_type == ContextFileType.Parallax) ? "Custom/Parallax" : ((_type != ContextFileType.ArcadeStyle) ? "LEVELS" : "Custom/Arcade")))));
        }
        string headingString = "";
        headingString = (_save ? "@SAVEICON@Save Level" : (_selectLevels ? "Select Active Levels" : ((_type == ContextFileType.Block) ? "@LOADICON@Custom" : ((_type == ContextFileType.Platform) ? "@LOADICON@Custom" : ((_type == ContextFileType.Background) ? "@LOADICON@Custom" : ((_type == ContextFileType.Parallax) ? "@LOADICON@Custom" : ((_type != ContextFileType.ArcadeStyle) ? "@LOADICON@Load Level" : "@LOADICON@Custom")))))));
        string drawPath = path;
        Graphics.DrawString(headingString + ((drawPath == "") ? "" : (" - " + drawPath)), topLeft + new Vec2(5f, 7f), Color.White, base.depth + 8);
        Vec2 part2TL = new Vec2(bottomRight.x + 2f, topLeft.y);
        Vec2 part2BR = part2TL + new Vec2(164f, 120f);
        if (_previewSprite != null && _previewSprite.texture != null && (_type == ContextFileType.Block || _type == ContextFileType.Background || _type == ContextFileType.Platform || _type == ContextFileType.Parallax || _type == ContextFileType.ArcadeStyle || _type == ContextFileType.ArcadeAnimation))
        {
            part2BR = ((_type != ContextFileType.Parallax) ? (part2TL + new Vec2(_previewSprite.width + 4, _previewSprite.height + 4)) : (part2TL + new Vec2(_previewSprite.width / 2 + 4, _previewSprite.height / 2 + 4)));
        }
        Graphics.DrawRect(part2TL, part2BR, new Color(70, 70, 70), base.depth, filled: false);
        Graphics.DrawRect(part2TL, part2BR, new Color(30, 30, 30), base.depth - 8);
        if (_previewSprite == null || _previewSprite.texture == null)
        {
            return;
        }
        _previewSprite.depth = 0.95f;
        _previewSprite.scale = new Vec2(0.5f);
        if (_type == ContextFileType.Block || _type == ContextFileType.Background || _type == ContextFileType.Platform)
        {
            _previewSprite.scale = new Vec2(1f);
        }
        Graphics.Draw(_previewSprite, part2TL.x + 2f, part2TL.y + 2f);
        if (_previewPair == null)
        {
            return;
        }
        string levName = _previewName;
        int idx = levName.LastIndexOf("/");
        if (idx != -1)
        {
            levName = levName.Substring(idx, levName.Length - idx);
        }
        if (levName.Length > 19)
        {
            levName = levName.Substring(0, 18) + ".";
        }
        _fancyFont.maxWidth = 160;
        string prepend = "";
        if (_previewPair.strange)
        {
            Graphics.DrawString(prepend + "STRANGE LEVEL", part2TL + new Vec2(5f, 107f), Colors.DGPurple, base.depth + 8);
            part2TL += new Vec2(0f, 122f);
            part2BR = part2TL + new Vec2(166f, 36f);
            Graphics.DrawRect(part2TL, part2BR, new Color(70, 70, 70), base.depth, filled: false);
            Graphics.DrawRect(part2TL, part2BR, new Color(30, 30, 30), base.depth - 8);
            _fancyFont.Draw("Must place at least one Duck Spawn Point to make a valid level.", part2TL.x + 4f, part2TL.y + 4f, Color.White, base.depth + 8);
            return;
        }
        if (_previewPair.arcade)
        {
            Graphics.DrawString(prepend + "ARCADE LAYOUT", part2TL + new Vec2(5f, 107f), Colors.DGYellow, base.depth + 8);
            return;
        }
        if (_previewPair.challenge)
        {
            Graphics.DrawString(prepend + "CHALLENGE LEVEL", part2TL + new Vec2(5f, 107f), Colors.DGRed, base.depth + 8);
            return;
        }
        if (_previewPair.invalid == null || _previewPair.invalid.Count == 0)
        {
            Graphics.DrawString(prepend + "ONLINE LEVEL", part2TL + new Vec2(5f, 107f), Colors.DGGreen, base.depth + 8);
            return;
        }
        Graphics.DrawString(prepend + "LOCAL LEVEL", part2TL + new Vec2(5f, 107f), Colors.DGBlue, base.depth + 8);
        part2TL += new Vec2(0f, 122f);
        _fancyFont.Draw("Contains the following Local-Only objects:", part2TL.x + 4f, part2TL.y + 4f, Color.White, base.depth + 8);
        int i = 22;
        if (_previewPair.invalid != null)
        {
            foreach (KeyValuePair<string, int> s in _previewPair.invalid)
            {
                string displayString = "- " + s.Key + ((s.Value > 1) ? (" (x" + s.Value + ")") : "");
                _fancyFont.Draw(displayString, part2TL.x + 4f, part2TL.y + 4f + (float)i, Color.White, base.depth + 8);
                if (_fancyFont.GetWidth(displayString) > 160f)
                {
                    i += 12;
                }
                i += 12;
            }
        }
        part2BR = part2TL + new Vec2(166f, 6 + i);
        Graphics.DrawRect(part2TL, part2BR, new Color(70, 70, 70), base.depth, filled: false);
        Graphics.DrawRect(part2TL, part2BR, new Color(30, 30, 30), base.depth - 8);
    }
}
