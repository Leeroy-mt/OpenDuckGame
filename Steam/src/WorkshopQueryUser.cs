using Steamworks;

public class WorkshopQueryUser : WorkshopQueryUGC
{
    #region Internal Fields

    internal EUserUGCList _listType;
    internal EUGCMatchingUGCType _type;
    internal EUserUGCListSortOrder _sortOrder;

    internal AccountID_t _accountID;

    #endregion

    public string? CloudNameFileFilter { get; set; }

    internal WorkshopQueryUser(uint unAccountID, EUserUGCList eListType, EUGCMatchingUGCType eMatchingUGCType, EUserUGCListSortOrder eSortOrder)
    {
        _accountID = new AccountID_t(unAccountID);
        _listType = eListType;
        _type = eMatchingUGCType;
        _sortOrder = eSortOrder;
    }

    internal override void Create()
    {
        handle = SteamUGC.CreateQueryUserUGCRequest(_accountID, _listType, _type, _sortOrder, (AppId_t)312530, (AppId_t)312530, _page); //! TODO: appid constant
    }

    internal override void SetQueryData()
    {
        base.SetQueryData();

        SteamUGC.SetCloudFileNameFilter(handle, CloudNameFileFilter);
    }
}