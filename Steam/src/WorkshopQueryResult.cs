public class WorkshopQueryResult
{
    #region Public Fields

    public string? previewURL;
    public string? metadata;

    public WorkshopQueryResultDetails? details;
    public WorkshopItem[]? fileList;
    public WorkshopQueryResultAdditionalPreview[]? additionalPreviews;

    public uint[]? statistics;

    #endregion
}