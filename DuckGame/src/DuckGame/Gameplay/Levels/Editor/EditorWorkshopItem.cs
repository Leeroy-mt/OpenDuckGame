using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace DuckGame;

public class EditorWorkshopItem
{
    public bool deathmatchTestSuccess;

    public bool challengeTestSuccess;

    private Tex2D _preview;

    private WorkshopItem _item;

    private LevelData _level;

    private Mod _mod;

    private List<EditorWorkshopItem> _subItems = new List<EditorWorkshopItem>();

    private EditorWorkshopItem _parent;

    public Tex2D preview
    {
        get
        {
            if (_preview == null)
            {
                if (_mod != null)
                {
                    string path = _mod.generateAndGetPathToScreenshot;
                    if (!File.Exists(path))
                    {
                        return null;
                    }
                    using FileStream s = File.Open(path, FileMode.Open);
                    _preview = Texture2D.FromStream(Graphics.device, s);
                }
                else
                {
                    RenderTarget2D previewTarget;
                    if (_level.metaData.type == LevelType.Arcade_Machine)
                    {
                        previewTarget = new RenderTarget2D(512, 512);
                        Content.customPreviewWidth = 128;
                        Content.customPreviewHeight = 128;
                        Content.customPreviewCenter = (Level.current as Editor).levelThings[0].Position;
                    }
                    else
                    {
                        previewTarget = new RenderTarget2D(1280, 720);
                    }
                    Content.GeneratePreview(_level, pRefresh: true, previewTarget);
                    Content.customPreviewWidth = 0;
                    Content.customPreviewHeight = 0;
                    Content.customPreviewCenter = Vector2.Zero;
                    _preview = new Texture2D(Graphics.device, previewTarget.width, previewTarget.height);
                    Color[] colors = new Color[previewTarget.width * previewTarget.height];
                    previewTarget.GetData(colors);
                    ((Tex2DBase)_preview).SetData<Color>(colors);
                }
            }
            return _preview;
        }
    }

    public IEnumerable<EditorWorkshopItem> subItems => _subItems;

    public int subIndex
    {
        get
        {
            if (_parent == null)
            {
                return -1;
            }
            return _parent._subItems.IndexOf(this);
        }
    }

    public EditorWorkshopItem parent => _parent;

    public LevelType levelType => _level.metaData.type;

    public LevelSize levelSize => _level.metaData.size;

    public IEnumerable<string> tags => workshopData.tags;

    private WorkshopMetaData workshopData
    {
        get
        {
            if (_mod == null)
            {
                return _level.workshopData;
            }
            return _mod.workshopData;
        }
    }

    public string name
    {
        get
        {
            return workshopData.name;
        }
        set
        {
            workshopData.name = value;
        }
    }

    public string description
    {
        get
        {
            return workshopData.description;
        }
        set
        {
            workshopData.description = value;
        }
    }

    public SteamResult result => _item.result;

    public bool finishedProcessing => _item.finishedProcessing;

    public WorkshopItem item => _item;

    public void AddTag(string pTag)
    {
        if (!workshopData.tags.Contains(pTag))
        {
            workshopData.tags.Add(pTag);
        }
    }

    public void RemoveTag(string pTag)
    {
        workshopData.tags.Remove(pTag);
    }

    public EditorWorkshopItem(LevelData pLevel, EditorWorkshopItem pParent = null)
    {
        _parent = pParent;
        _level = pLevel;
        if (_level.metaData.workshopID != 0L)
        {
            _item = WorkshopItem.GetItem(_level.metaData.workshopID);
            Steam.RequestWorkshopInfo(new List<WorkshopItem> { _item });
            Wait();
            _level.workshopData.name = _item.data.name;
            _level.workshopData.description = _item.data.description;
            _level.workshopData.tags = new List<string>(_item.data.tags);
        }
        if (_level.workshopData.name == "")
        {
            _level.workshopData.name = Path.GetFileNameWithoutExtension(_level.GetPath());
        }
        if (_level.metaData.type == LevelType.Arcade_Machine)
        {
            if (((Level.current as Editor).levelThings[0] as ArcadeMachine).challenge01Data != null)
            {
                _subItems.Add(new EditorWorkshopItem(((Level.current as Editor).levelThings[0] as ArcadeMachine).challenge01Data, this));
            }
            if (((Level.current as Editor).levelThings[0] as ArcadeMachine).challenge02Data != null)
            {
                _subItems.Add(new EditorWorkshopItem(((Level.current as Editor).levelThings[0] as ArcadeMachine).challenge02Data, this));
            }
            if (((Level.current as Editor).levelThings[0] as ArcadeMachine).challenge03Data != null)
            {
                _subItems.Add(new EditorWorkshopItem(((Level.current as Editor).levelThings[0] as ArcadeMachine).challenge03Data, this));
            }
        }
    }

    public EditorWorkshopItem(Mod pMod, EditorWorkshopItem pParent = null)
    {
        _parent = pParent;
        _mod = pMod;
        if (_mod.configuration.workshopID != 0L)
        {
            _item = WorkshopItem.GetItem(_mod.configuration.workshopID);
            Steam.RequestWorkshopInfo(new List<WorkshopItem> { _item });
            Wait();
            _mod.workshopData.name = _item.data.name;
            _mod.workshopData.description = _item.data.description;
            _mod.workshopData.tags = new List<string>(_item.data.tags);
        }
        _mod.workshopData.name = _mod.configuration.displayName;
        if (!workshopData.tags.Contains("Mod"))
        {
            AddTag("Mod");
        }
    }

    public SteamResult PrepareItem()
    {
        if (_item == null)
        {
            _item = Steam.CreateItem();
            Wait();
            _level.metaData.workshopID = _item.id;
            _item.SetDetails(workshopData.name, new WorkshopItemData());
            if (_parent != null && _parent._level.metaData.type == LevelType.Arcade_Machine)
            {
                _level.workshopData.name = _parent._item.name + " Sub Challenge " + subIndex;
                _level.workshopData.description = "One of the challenges in the \"" + _parent._item.name + "\" Arcade Machine.";
            }
        }
        if (result != SteamResult.OK)
        {
            return result;
        }
        _item.data.name = workshopData.name;
        _item.data.description = workshopData.description;
        workshopData.tags.RemoveAll((string x) => !SteamUploadDialog.possibleTags.Contains(x));
        if (_level.metaData.type != LevelType.Arcade_Machine)
        {
            AddTag("Map");
            AddTag(_level.metaData.size.ToString());
        }
        if (_level.metaData.type != LevelType.Deathmatch)
        {
            AddTag(_level.metaData.type.ToString().Replace("_", " "));
        }
        if (deathmatchTestSuccess)
        {
            AddTag("Deathmatch");
        }
        if (_level.metaData.eightPlayer)
        {
            AddTag("EightPlayer");
        }
        if (_level.metaData.eightPlayerRestricted)
        {
            AddTag("EightPlayerOnly");
        }
        else if (_level.metaData.type == LevelType.Arcade_Machine)
        {
            if (_subItems.Count == 3)
            {
                bool passed = true;
                foreach (EditorWorkshopItem subItem in _subItems)
                {
                    if (!subItem.challengeTestSuccess)
                    {
                        passed = false;
                        break;
                    }
                }
                if (passed)
                {
                    AddTag("Tested Machine");
                }
            }
        }
        else if (_level.metaData.type == LevelType.Challenge && challengeTestSuccess)
        {
            AddTag("Tested Challenge");
        }
        else if (_level.metaData.type == LevelType.Deathmatch)
        {
            AddTag("Strange");
        }
        if ((Level.current as Editor).levelThings.Exists((Thing x) => x is CustomCamera))
        {
            AddTag("Fixed Camera");
        }
        if (_level.metaData.hasCustomArt)
        {
            AddTag("Custom Art");
        }
        _item.data.tags = new List<string>(workshopData.tags);
        foreach (ulong u in _level.workshopData.dependencies)
        {
            Steam.WorkshopRemoveDependency(_item, WorkshopItem.GetItem(u));
        }
        _level.workshopData.dependencies.Clear();
        foreach (EditorWorkshopItem i in subItems)
        {
            if (i.PrepareItem() != SteamResult.OK)
            {
                return i.result;
            }
            _level.workshopData.dependencies.Add(i.item.id);
            Steam.WorkshopAddDependency(_item, i.item);
        }
        CopyFiles();
        return SteamResult.OK;
    }

    private void CopyFiles()
    {
        DuckFile.SaveChunk(_level, _level.GetPath());
        string folderPath = DuckFile.workshopDirectory + _level.metaData.workshopID + "/";
        string text = DuckFile.workshopDirectory + _level.metaData.workshopID + "-preview/";
        DuckFile.CreatePath(folderPath);
        DuckFile.CreatePath(text);
        string loneName = Path.GetFileNameWithoutExtension(_level.GetPath());
        string fileName = folderPath + Path.GetFileName(_level.GetPath());
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
        File.Copy(_level.GetPath(), fileName);
        File.SetAttributes(_level.GetPath(), FileAttributes.Normal);
        _item.data.contentFolder = folderPath;
        string previewName = text + loneName + ".png";
        if (File.Exists(previewName))
        {
            File.Delete(previewName);
        }
        Stream stream = DuckFile.Create(previewName);
        ((Texture2D)preview.nativeObject).SaveAsPng(stream, preview.width, preview.height);
        stream.Dispose();
        _item.data.previewPath = previewName;
    }

    public void Upload()
    {
        _item.ResetProcessing();
        _item.ApplyWorkshopData(_item.data);
    }

    public void FinishUpload()
    {
        if (_item.needsLegal)
        {
            Steam.ShowWorkshopLegalAgreement(_item.id.ToString());
        }
    }

    private void Wait()
    {
        while (!_item.finishedProcessing)
        {
            Steam.Update();
        }
    }
}
