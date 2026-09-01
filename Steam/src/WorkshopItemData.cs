namespace Steam;

public class WorkshopItemData
{
    #region Public Fields

    public int votesUp;

    public string? name;
    public string? description;
    public string? contentFolder;
    public string? previewPath;
    public string? changeNotes;

    public RemoteStoragePublishedFileVisibility visibility;

    public List<string>? tags;

    #endregion
}
