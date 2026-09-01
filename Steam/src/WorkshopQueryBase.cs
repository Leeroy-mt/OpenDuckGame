using Steamworks;

namespace Steam;

public delegate void WorkshopQueryFinished(object sender);

public delegate void WorkshopQueryResultFetched(object sender, WorkshopQueryResult result);

public abstract class WorkshopQueryBase : IDisposable
{
    #region Public Fields

    public uint NumResultsFetched;

    public uint NumResultsTotal;

    public uint MaxCacheTime;

    public uint Page;

    public WorkshopQueryData DataToFetch;

    #endregion

    #region Internal Fields

    internal static readonly bool hasSetReturnOnlyIDs = typeof(SteamUGC).GetMethod("SetReturnOnlyIDs") != null;

    internal UGCQueryHandle_t handle;

    internal CallResult<SteamUGCQueryCompleted_t>? completedCallResult;

    #endregion

    #region Events

    public event WorkshopQueryFinished? QueryFinished;
    public event WorkshopQueryResultFetched? ResultFetched;

    #endregion

    #region Public Properties

    public bool JustOnePage { get; set; }
    public bool OnlyQueryIDs { get; set; }

    #endregion

    internal ulong Handle => handle.m_UGCQueryHandle;

    #region Constructor & Finalizer

    internal WorkshopQueryBase()
    {
        DataToFetch = WorkshopQueryData.Details;
        completedCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnSteamUGCQueryCompleted);
        Page = 1;
        handle = new UGCQueryHandle_t();
    }

    ~WorkshopQueryBase()
    {
        Dispose(true);
    }

    #endregion

    #region Public Methods

    public void Request()
    {
        try
        {
            RequestImpl();
        }
        catch (TypeLoadException)
        {
            QueryFinished?.Invoke(this);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Internal Methods

    internal abstract void Create();

    internal virtual void Destroy()
    {
        if (!DGSteam.Initialized)
            return;

        try
        {
            SteamUGC.ReleaseQueryUGCRequest(handle);
            handle = new UGCQueryHandle_t();
            completedCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnSteamUGCQueryCompleted);
        }
        catch { }
    }

    internal virtual void SetQueryData()
    {
        if (DataToFetch == WorkshopQueryData.TotalOnly)
            SteamUGC.SetReturnTotalOnly(handle, true);
        else
        {
            {
                var returnLongDescription = (DataToFetch & WorkshopQueryData.LongDescription) != 0;
                SteamUGC.SetReturnLongDescription(handle, returnLongDescription);
            }

            {
                var returnMetadata = (DataToFetch & WorkshopQueryData.Metadata) != 0;
                SteamUGC.SetReturnMetadata(handle, returnMetadata);
            }

            {
                var returnChildren = (DataToFetch & WorkshopQueryData.Children) != 0;
                SteamUGC.SetReturnChildren(handle, returnChildren);
            }

            {
                var returnAdditionalPreviews = (DataToFetch & WorkshopQueryData.AdditionalPreviews) != 0;
                SteamUGC.SetReturnAdditionalPreviews(handle, returnAdditionalPreviews);
            }
        }

        if (MaxCacheTime != 0)
            SteamUGC.SetAllowCachedResponse(handle, MaxCacheTime);

        if (OnlyQueryIDs)
            SteamUGC.SetReturnOnlyIDs(handle, true);
    }

    internal void SetQueryData_SetReturnOnlyIDs(bool bReturnOnlyIDs) //! possible method remove
    {
        SteamUGC.SetReturnOnlyIDs(handle, bReturnOnlyIDs);
    }

    #endregion

    #region Protected Methods

    protected virtual async void RequestImpl()
    {
        if (Handle == 0)
            Create();

        SetQueryData();
        SteamAPICall_t hSteamAPICall = SteamUGC.SendQueryUGCRequest(handle);

        if (hSteamAPICall.m_SteamAPICall != 0)
            completedCallResult?.Set(hSteamAPICall);
        else
            OnSteamUGCQueryCompleted(new SteamUGCQueryCompleted_t(), false);
    }

    protected virtual void Dispose(bool flag)
    {
        Destroy();
        completedCallResult?.Cancel();
        (completedCallResult as IDisposable)?.Dispose();
        completedCallResult = null;
    }

    #endregion

    void OnSteamUGCQueryCompleted(SteamUGCQueryCompleted_t queryCompleted, bool ioFailure)
    {
        NumResultsTotal = queryCompleted.m_unTotalMatchingResults;

        if (queryCompleted.m_unNumResultsReturned == 0 || DataToFetch == WorkshopQueryData.TotalOnly)
        {
            QueryFinished?.Invoke(this);
            return;
        }

        NumResultsFetched += queryCompleted.m_unNumResultsReturned;
        for (uint i = 0; i < queryCompleted.m_unNumResultsReturned; i++)
        {
            WorkshopQueryResult result = new();
            SteamUGC.GetQueryUGCPreviewURL(queryCompleted.m_handle, i, out result.PreviewURL, 260);

            SteamUGC.GetQueryUGCResult(queryCompleted.m_handle, i, out SteamUGCDetails_t ugcDetails);
            WorkshopQueryResultDetails resultDetails = result.Details = new WorkshopQueryResultDetails();

            resultDetails.AcceptedForUse = ugcDetails.m_bAcceptedForUse;
            resultDetails.Banned = ugcDetails.m_bBanned;
            resultDetails.Description = ugcDetails.m_rgchDescription;

            resultDetails.File = ugcDetails.m_hFile.m_UGCHandle;
            resultDetails.FileName = ugcDetails.m_pchFileName;
            resultDetails.FileSize = ugcDetails.m_nFileSize;
            resultDetails.FileType = ugcDetails.m_eFileType;
            resultDetails.NumChildren = ugcDetails.m_unNumChildren;
            resultDetails.PreviewFile = ugcDetails.m_hPreviewFile.m_UGCHandle;
            resultDetails.PreviewFileSize = ugcDetails.m_nPreviewFileSize;
            resultDetails.PublishedFile = WorkshopItem.GetItem(ugcDetails.m_nPublishedFileId);

            resultDetails.Result = ugcDetails.m_eResult;
            resultDetails.Score = ugcDetails.m_flScore;
            resultDetails.SteamIDOwner = ugcDetails.m_ulSteamIDOwner;
            resultDetails.tags = ugcDetails.m_rgchTags.Split(',');
            resultDetails.TagsTruncated = ugcDetails.m_bTagsTruncated;
            resultDetails.TimeAddedToUserList = ugcDetails.m_rtimeAddedToUserList;
            resultDetails.TimeCreated = ugcDetails.m_rtimeCreated;
            resultDetails.TimeUpdated = ugcDetails.m_rtimeUpdated;
            resultDetails.Title = ugcDetails.m_rgchTitle;
            resultDetails.URL = ugcDetails.m_rgchURL;
            resultDetails.Visibility = ugcDetails.m_eVisibility;
            resultDetails.VotesDown = ugcDetails.m_unVotesDown;
            resultDetails.VotesUp = ugcDetails.m_unVotesUp;

            if ((DataToFetch & WorkshopQueryData.Children) != 0)
            {
                PublishedFileId_t[] children = new PublishedFileId_t[resultDetails.NumChildren];
                if (SteamUGC.GetQueryUGCChildren(queryCompleted.m_handle, i, children, (uint)children.Length))
                    result.FileList = SteamHelper.GetArray(children, id => WorkshopItem.GetItem(id)!);
            }

            if ((DataToFetch & WorkshopQueryData.Metadata) != 0)
                SteamUGC.GetQueryUGCMetadata(queryCompleted.m_handle, i, out result.Metadata, 260);

            if ((DataToFetch & WorkshopQueryData.AdditionalPreviews) != 0)
            {
                WorkshopQueryResultAdditionalPreview[] previews = result.AdditionalPreviews = new WorkshopQueryResultAdditionalPreview[SteamUGC.GetQueryUGCNumAdditionalPreviews(queryCompleted.m_handle, i)];
                for (uint previewi = 0; previewi < previews.Length; previewi++)
                {
                    if (SteamUGC.GetQueryUGCAdditionalPreview(queryCompleted.m_handle, i, previewi, out string url, 260, out string name, 260, out EItemPreviewType type))
                        previews[previewi] = new WorkshopQueryResultAdditionalPreview(type == EItemPreviewType.k_EItemPreviewType_Image, url);
                }
            }

            if ((DataToFetch & WorkshopQueryData.Statistics) != 0)
            {
                uint[] stats = result.Statistics = new uint[8];
                for (WorkshopResultStatistic stat = WorkshopResultStatistic.NumSubscriptions; (int)stat < stats.Length; stat++)
                {
                    if (SteamUGC.GetQueryUGCStatistic(queryCompleted.m_handle, i, (EItemStatistic)stat, out ulong val))
                        stats[(int)stat] = (uint)val;
                }
            }

            ResultFetched?.Invoke(this, result);
        }

        if (NumResultsFetched == NumResultsTotal || JustOnePage)
        {
            QueryFinished?.Invoke(this);
        }
        else
        {
            Destroy();
            Page++;
            Create();
            Request();
        }
    }
}