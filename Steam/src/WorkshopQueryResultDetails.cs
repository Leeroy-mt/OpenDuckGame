using Steamworks;

public class WorkshopQueryResultDetails
{
    #region Public Fields

    public bool banned;
    public bool acceptedForUse;
    public bool tagsTruncated;

    public int fileSize;
    public int previewFileSize;

    public uint timeCreated;
    public uint timeUpdated;
    public uint timeAddedToUserList;
    public uint votesUp;
    public uint votesDown;
    public uint numChildren;

    public float score;

    public ulong steamIDOwner;
    public ulong file;
    public ulong previewFile;

    public string? title;
    public string? description;
    public string? fileName;
    public string? URL;

    public EResult result;
    public EWorkshopFileType fileType;
    public ERemoteStoragePublishedFileVisibility visibility;

    public WorkshopItem? publishedFile;

    public string[]? tags;

    #endregion
}