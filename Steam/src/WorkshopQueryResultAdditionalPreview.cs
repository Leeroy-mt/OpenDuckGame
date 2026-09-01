namespace Steam;

public class WorkshopQueryResultAdditionalPreview
{
    #region Public Fields

    public bool IsImage;

    public string UrlOrVideoID;

    #endregion

    public WorkshopQueryResultAdditionalPreview(bool isImage, string urlOrVideoID)
    {
        this.IsImage = isImage;
        this.UrlOrVideoID = urlOrVideoID;
    }
}
