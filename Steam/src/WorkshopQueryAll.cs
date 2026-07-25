using Steamworks;

public class WorkshopQueryAll : WorkshopQueryUGC
{
    #region Internal Fields

    internal EUGCQuery queryType;

    internal EUGCMatchingUGCType fileType;

    #endregion

    #region Public Properties

    public bool MatchAnyTag { get; set; }

    public uint TrendRankDays { get; set; }

    public string? SearchText { get; set; }

    #endregion

    internal WorkshopQueryAll(EUGCQuery eQueryType, EUGCMatchingUGCType eMatchingUGCTypeFileType)
    {
        queryType = eQueryType;
        fileType = eMatchingUGCTypeFileType;
    }

    #region Public Fields

    internal override void Create()
    {
        handle = SteamUGC.CreateQueryAllUGCRequest(queryType, fileType, (AppId_t)312530, (AppId_t)312530, _page); //! TODO: appid constant
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