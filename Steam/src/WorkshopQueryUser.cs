using Steamworks;

namespace Steam;

public class WorkshopQueryUser : WorkshopQueryUGC
{
    #region Internal Fields

    internal WorkshopList listType;
    internal WorkshopType type;
    internal WorkshopSortOrder sortOrder;

    internal AccountID_t accountID;

    #endregion

    public string? CloudNameFileFilter { get; set; }

    internal WorkshopQueryUser(uint unAccountID, WorkshopList eListType, WorkshopType eMatchingUGCType, WorkshopSortOrder eSortOrder)
    {
        accountID = new(unAccountID);
        listType = eListType;
        type = eMatchingUGCType;
        sortOrder = eSortOrder;
    }

    internal override void Create()
    {
        handle = SteamUGC.CreateQueryUserUGCRequest(
            accountID,
            (EUserUGCList)listType,
            (EUGCMatchingUGCType)type,
            (EUserUGCListSortOrder)sortOrder,
            (AppId_t)DGSteam.AppId,
            (AppId_t)DGSteam.AppId,
            Page
            );
    }

    internal override void SetQueryData()
    {
        base.SetQueryData();
        SteamUGC.SetCloudFileNameFilter(handle, CloudNameFileFilter);
    }
}