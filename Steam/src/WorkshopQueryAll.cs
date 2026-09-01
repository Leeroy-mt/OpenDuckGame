using Steamworks;

namespace Steam;

public class WorkshopQueryAll : WorkshopQueryUGC
{
    #region Internal Fields

    internal WorkshopQueryFilterOrder queryType;

    internal WorkshopType fileType;

    #endregion

    #region Public Properties

    public bool MatchAnyTag { get; set; }

    public uint TrendRankDays { get; set; }

    public string? SearchText { get; set; }

    #endregion

    internal WorkshopQueryAll(WorkshopQueryFilterOrder eQueryType, WorkshopType eMatchingUGCTypeFileType)
    {
        queryType = eQueryType;
        fileType = eMatchingUGCTypeFileType;
    }

    #region Public Fields

    internal override void Create()
    {
        handle = SteamUGC.CreateQueryAllUGCRequest((EUGCQuery)queryType, (EUGCMatchingUGCType)fileType, (AppId_t)DGSteam.AppId, (AppId_t)DGSteam.AppId, Page);
    }

    internal override void SetQueryData()
    {
        base.SetQueryData();
        SteamUGC.SetMatchAnyTag(handle, MatchAnyTag);
        SteamUGC.SetSearchText(handle, SearchText);

        if (TrendRankDays != 0)
            SteamUGC.SetRankedByTrendDays(handle, TrendRankDays);
    }

    #endregion
}