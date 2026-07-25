using Steamworks;

public delegate void WorkshopQueryFinished(object sender);
public delegate void WorkshopQueryResultFetched(object sender, WorkshopQueryResult result);

public abstract class WorkshopQueryBase : IDisposable
{
    #region Public Fields

    public uint _numResultsFetched;

    public uint _numResultsTotal;

    public uint _maxCacheTime;

    public uint _page;

    public WorkshopQueryData _dataToFetch;

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
        _dataToFetch = WorkshopQueryData.Details;
        completedCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnSteamUGCQueryCompleted);
        _page = 1;
        handle = new UGCQueryHandle_t();
    }

    ~WorkshopQueryBase()
    {
        Dispose(true);
    }

    #endregion

    #region Public Fields

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
        if (!Steam.Initialized)
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
        if (_dataToFetch == WorkshopQueryData.TotalOnly)
            SteamUGC.SetReturnTotalOnly(handle, true);
        else
        {
            {
                var returnLongDescription = (_dataToFetch & WorkshopQueryData.LongDescription) != 0;
                SteamUGC.SetReturnLongDescription(handle, returnLongDescription);
            }

            {
                var returnMetadata = (_dataToFetch & WorkshopQueryData.Metadata) != 0;
                SteamUGC.SetReturnMetadata(handle, returnMetadata);
            }

            {
                var returnChildren = (_dataToFetch & WorkshopQueryData.Children) != 0;
                SteamUGC.SetReturnChildren(handle, returnChildren);
            }

            {
                var returnAdditionalPreviews = (_dataToFetch & WorkshopQueryData.AdditionalPreviews) != 0;
                SteamUGC.SetReturnAdditionalPreviews(handle, returnAdditionalPreviews);
            }
        }

        if (_maxCacheTime != 0)
            SteamUGC.SetAllowCachedResponse(handle, _maxCacheTime);

        if (OnlyQueryIDs)
            SteamUGC.SetReturnOnlyIDs(handle, true);
    }

    internal void SetQueryData_SetReturnOnlyIDs(bool bReturnOnlyIDs) //! possible method remove
    {
        SteamUGC.SetReturnOnlyIDs(handle, bReturnOnlyIDs);
    }

    #endregion

    #region Protected Methods

    protected virtual void RequestImpl()
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
        _numResultsTotal = queryCompleted.m_unTotalMatchingResults;

        if (queryCompleted.m_unNumResultsReturned == 0 || _dataToFetch == WorkshopQueryData.TotalOnly)
        {
            QueryFinished?.Invoke(this);
            return;
        }
        _numResultsFetched += queryCompleted.m_unNumResultsReturned;
        for (uint i = 0; i < queryCompleted.m_unNumResultsReturned; i++)
        {
            WorkshopQueryResult result = new();
            SteamUGC.GetQueryUGCPreviewURL(queryCompleted.m_handle, i, out result.previewURL, 260);

            SteamUGC.GetQueryUGCResult(queryCompleted.m_handle, i, out SteamUGCDetails_t ugcDetails);
            WorkshopQueryResultDetails resultDetails = result.details = new WorkshopQueryResultDetails();

            resultDetails.acceptedForUse = ugcDetails.m_bAcceptedForUse;
            resultDetails.banned = ugcDetails.m_bBanned;
            resultDetails.description = ugcDetails.m_rgchDescription;

            resultDetails.file = ugcDetails.m_hFile.m_UGCHandle;
            resultDetails.fileName = ugcDetails.m_pchFileName;
            resultDetails.fileSize = ugcDetails.m_nFileSize;
            resultDetails.fileType = ugcDetails.m_eFileType;
            resultDetails.numChildren = ugcDetails.m_unNumChildren;
            resultDetails.previewFile = ugcDetails.m_hPreviewFile.m_UGCHandle;
            resultDetails.previewFileSize = ugcDetails.m_nPreviewFileSize;
            resultDetails.publishedFile = WorkshopItem.GetItem(ugcDetails.m_nPublishedFileId);

            resultDetails.result = ugcDetails.m_eResult;
            resultDetails.score = ugcDetails.m_flScore;
            resultDetails.steamIDOwner = ugcDetails.m_ulSteamIDOwner;
            resultDetails.tags = ugcDetails.m_rgchTags.Split(',');
            resultDetails.tagsTruncated = ugcDetails.m_bTagsTruncated;
            resultDetails.timeAddedToUserList = ugcDetails.m_rtimeAddedToUserList;
            resultDetails.timeCreated = ugcDetails.m_rtimeCreated;
            resultDetails.timeUpdated = ugcDetails.m_rtimeUpdated;
            resultDetails.title = ugcDetails.m_rgchTitle;
            resultDetails.URL = ugcDetails.m_rgchURL;
            resultDetails.visibility = ugcDetails.m_eVisibility;
            resultDetails.votesDown = ugcDetails.m_unVotesDown;
            resultDetails.votesUp = ugcDetails.m_unVotesUp;

            if ((_dataToFetch & WorkshopQueryData.Children) != 0)
            {
                PublishedFileId_t[] children = new PublishedFileId_t[resultDetails.numChildren];
                if (SteamUGC.GetQueryUGCChildren(queryCompleted.m_handle, i, children, (uint)children.Length))
                    result.fileList = SteamHelper.GetArray(children, id => WorkshopItem.GetItem(id)!);
            }

            if ((_dataToFetch & WorkshopQueryData.Metadata) != 0)
                SteamUGC.GetQueryUGCMetadata(queryCompleted.m_handle, i, out result.metadata, 260);

            if ((_dataToFetch & WorkshopQueryData.AdditionalPreviews) != 0)
            {
                WorkshopQueryResultAdditionalPreview[] previews = result.additionalPreviews = new WorkshopQueryResultAdditionalPreview[SteamUGC.GetQueryUGCNumAdditionalPreviews(queryCompleted.m_handle, i)];
                for (uint previewi = 0; previewi < previews.Length; previewi++)
                {
                    if (SteamUGC.GetQueryUGCAdditionalPreview(queryCompleted.m_handle, i, previewi, out string url, 260, out string name, 260, out EItemPreviewType type))
                        previews[previewi] = new WorkshopQueryResultAdditionalPreview(type == EItemPreviewType.k_EItemPreviewType_Image, url);
                }
            }

            if ((_dataToFetch & WorkshopQueryData.Statistics) != 0)
            {
                uint[] stats = result.statistics = new uint[8];
                for (WorkshopResultStatistic stat = WorkshopResultStatistic.NumSubscriptions; (int)stat < stats.Length; stat++)
                {
                    if (SteamUGC.GetQueryUGCStatistic(queryCompleted.m_handle, i, (EItemStatistic)stat, out ulong val))
                        stats[(int)stat] = (uint)val;
                }
            }

            ResultFetched?.Invoke(this, result);
        }

        if (_numResultsFetched == _numResultsTotal || JustOnePage)
        {
            QueryFinished?.Invoke(this);
        }
        else
        {
            Destroy();
            _page++;
            Create();
            Request();
        }
    }
}