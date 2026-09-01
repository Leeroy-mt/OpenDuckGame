namespace Steam;

public class WorkshopQueryResult
{
    #region Public Fields

    public string? PreviewURL;
    public string? Metadata;

    public WorkshopQueryResultDetails? Details;
    public WorkshopItem[]? FileList;
    public WorkshopQueryResultAdditionalPreview[]? AdditionalPreviews;

    public uint[]? Statistics;

    #endregion
}