using Steamworks;

namespace Steam;

public class WorkshopQueryResultDetails
{
    #region Public Fields

    public bool Banned;
    public bool AcceptedForUse;
    public bool TagsTruncated;

    public int FileSize;
    public int PreviewFileSize;

    public uint TimeCreated;
    public uint TimeUpdated;
    public uint TimeAddedToUserList;
    public uint VotesUp;
    public uint VotesDown;
    public uint NumChildren;

    public float Score;

    public ulong SteamIDOwner;
    public ulong File;
    public ulong PreviewFile;

    public string? Title;
    public string? Description;
    public string? FileName;
    public string? URL;
    public EResult Result;
    public EWorkshopFileType FileType;
    public ERemoteStoragePublishedFileVisibility Visibility;

    public WorkshopItem? PublishedFile;

    public string[]? tags;

    #endregion
}