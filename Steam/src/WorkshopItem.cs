using Steamworks;
using System.Data;

namespace Steam;

public class WorkshopItem
{
    #region Public Fields

    public List<object>? subItems;
    public List<WorkshopItem?>? dependencies;

    #endregion

    #region Public Properties

    public bool FinishedProcessing { get; set; }
    public bool NeedsLegal { get; private set; }

    public uint Timestamp
    {
        get => SteamUGC.GetItemInstallInfo(_id, out _, out _, 256, out uint punTimeStamp)
            ? punTimeStamp
            : 0;
    }

    public ulong Id => _id.m_PublishedFileId;
    public ulong UpdateHandle => _currentUpdateHandle.m_UGCUpdateHandle;

    public string? Name { get; private set; }
    public string Path
    {
        get => SteamUGC.GetItemInstallInfo(_id, out _, out string folder, 256, out _)
            ? folder
            : "";
    }

    public SteamResult Result { get; private set; }
    public SteamResult DownloadResult { get; private set; }
    public WorkshopItemState StateFlags => (WorkshopItemState)SteamUGC.GetItemState(_id);

    public WorkshopItemData? Data { get; private set; }

    #endregion

    #region Private Fields

    PublishedFileId_t _id;
    UGCUpdateHandle_t _currentUpdateHandle;

    static Dictionary<ulong, WorkshopItem>? items;

    #endregion

    #region Constructors

    public WorkshopItem(ulong id)
        : this(new PublishedFileId_t(id)) { }

    internal WorkshopItem(PublishedFileId_t id)
    {
        _id = id;
        FinishedProcessing = true;
        Result = SteamResult.OK;
    }

    public WorkshopItem()
    {
    }

    #endregion

    #region Public Methods

    public static WorkshopItem? GetItem(ulong id)
    {
        if (id == 0)
            return null;

        items ??= [];
        using Lock _lock = new(items);
        if (!items.TryGetValue(id, out WorkshopItem? item))
        {
            item = new WorkshopItem(id);
            items[id] = item;
        }
        return item;
    }

    public void ApplyResult(SteamResult r, bool legal, ulong id)
    {
        Result = r;
        NeedsLegal = legal;
        _id = new PublishedFileId_t(id);
        FinishedProcessing = true;
    }

    public void ApplyDownloadResult(SteamResult r)
    {
        DownloadResult = r;
        FinishedProcessing = true;
    }

    public bool ApplyWorkshopData(WorkshopItemData data)
    {
        UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), _id);
        if (handle.m_UGCUpdateHandle == 0)
            return false;

        Data = data;
        if (data.name != null && data.name != "")
        {
            SteamUGC.SetItemTitle(handle, data.name);
            SteamUGC.SetItemVisibility(handle, (ERemoteStoragePublishedFileVisibility)data.visibility);
        }

        if (data.description != null && data.description != "")
            SteamUGC.SetItemDescription(handle, data.description);

        var tags = data.tags;
        if (tags != null && tags.Count > 0)
            SteamUGC.SetItemTags(handle, data.tags);

        SteamUGC.SetItemPreview(handle, data.previewPath);
        SteamUGC.SetItemContent(handle, data.contentFolder);
        _currentUpdateHandle = handle;
        DGSteam.StartUpload(this);
        return true;
    }

    public TransferProgress GetUploadProgress()
    {
        var status = SteamUGC.GetItemUpdateProgress(_currentUpdateHandle, out ulong bytesDownloaded, out ulong bytesTotal);
        return new()
        {
            status = (ItemUpdateStatus)(int)status,
            bytesDownloaded = bytesDownloaded,
            bytesTotal = bytesTotal
        };
    }

    public TransferProgress GetDownloadProgress()
    {
        if (!SteamUGC.GetItemDownloadInfo(_id, out ulong bytesDownloaded, out ulong bytesTotal))
            bytesDownloaded = bytesTotal = 0;

        return new()
        {
            status = ItemUpdateStatus.Invalid,
            bytesDownloaded = bytesDownloaded,
            bytesTotal = bytesTotal
        };
    }

    public void ResetProcessing()
    {
        FinishedProcessing = false;
        NeedsLegal = false;
    }

    public void SkipProcessing()
    {
        FinishedProcessing = true;
        NeedsLegal = false;
        Result = SteamResult.OK;
    }

    public void SetDetails(string name, WorkshopItemData data)
    {
        Name = name;
        Data = data;
    }

    public void Subscribe()
    {
        SteamUGC.SubscribeItem(_id);
    }

    #endregion

    internal static WorkshopItem? GetItem(PublishedFileId_t id)
    {
        return GetItem(id.m_PublishedFileId);
    }
}