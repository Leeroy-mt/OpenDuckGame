using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;


#if FACEPUNCH
using Steamworks;
using Steamworks.Ugc;
#else
using Steam;
#endif

namespace DuckGame;

public class EditorWorkshopItem
{
    public bool deathmatchTestSuccess;

    public bool challengeTestSuccess;

    Texture2D _preview;

    private WorkshopItem _item;

    private LevelData _level;

    private Mod _mod;

    private List<EditorWorkshopItem> _subItems = new List<EditorWorkshopItem>();

    private EditorWorkshopItem _parent;

    public Texture2D preview
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
                        previewTarget = RenderTarget2D.CreateSetUpTarget(512, 512);
                        Content.customPreviewWidth = 128;
                        Content.customPreviewHeight = 128;
                        Content.customPreviewCenter = (Level.current as Editor).levelThings[0].Position;
                    }
                    else
                    {
                        previewTarget = RenderTarget2D.CreateSetUpTarget(1280, 720);
                    }
                    Content.GeneratePreview(_level, pRefresh: true, previewTarget);
                    Content.customPreviewWidth = 0;
                    Content.customPreviewHeight = 0;
                    Content.customPreviewCenter = Vector2.Zero;
                    _preview = new Texture2D(Graphics.device, previewTarget.Width, previewTarget.Height);
                    Color[] colors = new Color[previewTarget.Width * previewTarget.Height];
                    previewTarget.GetData(colors);
                    _preview.SetData<Color>(colors);
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

#if FACEPUNCH
    public Result result => _item.Result;
#else
    public SteamResult result => _item.Result;
#endif

    public bool finishedProcessing => _item.FinishedProcessing;

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

        if (_level.metaData.workshopID != 0)
        {
            _item = WorkshopItem.GetItem(_level.metaData.workshopID);
#if FACEPUNCH
            _item.Request();
            Wait();
            _level.workshopData.name = _item.Data.Name;
            _level.workshopData.description = _item.Data.Description;
            _level.workshopData.tags = [.. _item.Data.Tags];
#else
            DGSteam.RequestWorkshopInfo(new List<WorkshopItem> { _item });
            Wait();
            _level.workshopData.name = _item.Data.name;
            _level.workshopData.description = _item.Data.description;
            _level.workshopData.tags = new List<string>(_item.Data.tags);
#endif
        }

        if (_level.workshopData.name == "")
            _level.workshopData.name = Path.GetFileNameWithoutExtension(_level.GetPath());

        if (_level.metaData.type == LevelType.Arcade_Machine)
        {
            if (((Level.current as Editor).levelThings[0] as ArcadeMachine).challenge01Data != null)
                _subItems.Add(new EditorWorkshopItem(((Level.current as Editor).levelThings[0] as ArcadeMachine).challenge01Data, this));

            if (((Level.current as Editor).levelThings[0] as ArcadeMachine).challenge02Data != null)
                _subItems.Add(new EditorWorkshopItem(((Level.current as Editor).levelThings[0] as ArcadeMachine).challenge02Data, this));

            if (((Level.current as Editor).levelThings[0] as ArcadeMachine).challenge03Data != null)
                _subItems.Add(new EditorWorkshopItem(((Level.current as Editor).levelThings[0] as ArcadeMachine).challenge03Data, this));
        }
    }

    public EditorWorkshopItem(Mod pMod, EditorWorkshopItem pParent = null)
    {
        _parent = pParent;
        _mod = pMod;
        if (_mod.configuration.workshopID != 0L)
        {
            _item = WorkshopItem.GetItem(_mod.configuration.workshopID);
#if FACEPUNCH
            _item.Request();
            Wait();
            _mod.workshopData.name = _item.Data.Name;
            _mod.workshopData.description = _item.Data.Description;
            _mod.workshopData.tags = [.. _item.Data.Tags];
#else
            DGSteam.RequestWorkshopInfo(new List<WorkshopItem> { _item });
            Wait();
            _mod.workshopData.name = _item.Data.name;
            _mod.workshopData.description = _item.Data.description;
            _mod.workshopData.tags = new List<string>(_item.Data.tags);
#endif
        }
        _mod.workshopData.name = _mod.configuration.displayName;

        if (!workshopData.tags.Contains("Mod"))
            AddTag("Mod");
    }

#if FACEPUNCH
    public Result PrepareItem()
    {
        if (_item == null)
        {
            _item = new(0);
            _item.Publish();
            Wait();
            _level.metaData.workshopID = _item.Id;
            _item.Name = workshopData.name;
            _item.Data = new();
            if (_parent != null && _parent._level.metaData.type == LevelType.Arcade_Machine)
            {
                _level.workshopData.name = $"{_parent._item.Data.Name} Sub Challenge {subIndex}";
                _level.workshopData.description = $"One of the challenges in the \"{_parent._item.Name}\" Arcade Machine.";
            }
        }

        if (result != Result.OK)
            return result;

        _item.Data.Name = workshopData.name;
        _item.Data.Description = workshopData.description;
        workshopData.tags.RemoveAll(x => !SteamUploadDialog.possibleTags.Contains(x));

        if (_level.metaData.type != LevelType.Arcade_Machine)
        {
            AddTag("Map");
            AddTag(_level.metaData.size.ToString());
        }

        if (_level.metaData.type != LevelType.Deathmatch)
            AddTag(_level.metaData.type.ToString().Replace("_", " "));

        if (deathmatchTestSuccess)
            AddTag("Deathmatch");

        if (_level.metaData.eightPlayer)
            AddTag("EightPlayer");

        if (_level.metaData.eightPlayerRestricted)
            AddTag("EightPlayerOnly");
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
                    AddTag("Tested Machine");
            }
        }
        else if (_level.metaData.type == LevelType.Challenge && challengeTestSuccess)
            AddTag("Tested Challenge");
        else if (_level.metaData.type == LevelType.Deathmatch)
            AddTag("Strange");

        if ((Level.current as Editor).levelThings.Exists(x => x is CustomCamera))
            AddTag("Fixed Camera");

        if (_level.metaData.hasCustomArt)
            AddTag("Custom Art");

        _item.Data.Tags = [.. workshopData.tags];

        foreach (ulong u in _level.workshopData.dependencies)
            _item.RemoveDependency(WorkshopItem.GetItem(u));
        _level.workshopData.dependencies.Clear();

        foreach (EditorWorkshopItem i in subItems)
        {
            if (i.PrepareItem() != Result.OK)
                return i.result;

            _level.workshopData.dependencies.Add(i.item.Id);
            _item.AddDependency(i.item);
        }

        CopyFiles();
        return Result.OK;
    }
#else
    public SteamResult PrepareItem()
    {
        if (_item == null)
        {
            _item = DGSteam.CreateItem();
            Wait();
            _level.metaData.workshopID = _item.Id;
            _item.SetDetails(workshopData.name, new WorkshopItemData());
            if (_parent != null && _parent._level.metaData.type == LevelType.Arcade_Machine)
            {
                _level.workshopData.name = _parent._item.Name + " Sub Challenge " + subIndex;
                _level.workshopData.description = "One of the challenges in the \"" + _parent._item.Name + "\" Arcade Machine.";
            }
        }
        if (result != SteamResult.OK)
        {
            return result;
        }
        _item.Data.name = workshopData.name;
        _item.Data.description = workshopData.description;
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
        _item.Data.tags = [.. workshopData.tags];
        foreach (ulong u in _level.workshopData.dependencies)
        {
            DGSteam.WorkshopRemoveDependency(_item, WorkshopItem.GetItem(u));
        }
        _level.workshopData.dependencies.Clear();
        foreach (EditorWorkshopItem i in subItems)
        {
            if (i.PrepareItem() != SteamResult.OK)
            {
                return i.result;
            }
            _level.workshopData.dependencies.Add(i.item.Id);
            DGSteam.WorkshopAddDependency(_item, i.item);
        }
        CopyFiles();
        return SteamResult.OK;
    }
#endif

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
#if FACEPUNCH
        _item.Data.ContentFolder = folderPath;
#else
        _item.Data.contentFolder = folderPath;
#endif
        string previewName = text + loneName + ".png";
        if (File.Exists(previewName))
        {
            File.Delete(previewName);
        }
        Stream stream = DuckFile.Create(previewName);
        preview.SaveAsPng(stream, preview.Width, preview.Height);
        stream.Dispose();
#if FACEPUNCH
        _item.Data.PreviewPath = previewName;
#else
        _item.Data.previewPath = previewName;
#endif
    }

    public void Upload()
    {
#if FACEPUNCH
        _item.ResetProcessing();
        _item.ApplyWorkshopData(_item.Data);
#else
        _item.ResetProcessing();
        _item.ApplyWorkshopData(_item.Data);
#endif
    }

    public void FinishUpload()
    {
        if (_item.NeedsLegal)
        {
#if FACEPUNCH
            SteamFriends.OpenWebOverlay($"steam://url/CommunityFilePage/{_item.Id}");
#else
            DGSteam.ShowWorkshopLegalAgreement(_item.Id.ToString());
#endif
        }
    }

    private void Wait()
    {
        while (!_item.FinishedProcessing)
        {
#if FACEPUNCH
            SteamClient.RunCallbacks();
#else
            DGSteam.Update();
#endif
        }
    }
}
