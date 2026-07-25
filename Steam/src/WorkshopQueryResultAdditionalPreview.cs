public class WorkshopQueryResultAdditionalPreview
{
    #region Public Fields

    public bool isImage;

    public string urlOrVideoID;

    #endregion

    public WorkshopQueryResultAdditionalPreview(bool isImage, string urlOrVideoID)
    {
        this.isImage = isImage;
        this.urlOrVideoID = urlOrVideoID;
    }
}
