using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public static class RandomLevelDownloader
{
    #region Public Fields

    public static bool ready;

    public static int numToHaveReady = 10;

    public static int numSinceLowRating;

    public static List<WorkshopItem> _downloadingItems = [];

    #endregion

    #region Private Fields

    static int _toFetchIndex = -1;
    static int _numFetch;
    static int _totalMaps;

    static float _fetchDelay;

    static object _currentWorkshopLevelQuery;

    static WorkshopQueryFilterOrder _orderMode = WorkshopQueryFilterOrder.RankedByVote;

    static List<LevelData> _readyLevels = [];

    #endregion

    #region Public Methods

    public static void ProcessWorkshopItem(WorkshopItem pItem)
    {
        ProcessLevel(pItem.path);
    }

    public static void DownloadRandomMap()
    {
        int page = Rando.Int(_totalMaps / 50) + 1;
        if (numSinceLowRating > 3)
        {
            numSinceLowRating = 0;
            if (Rando.Float(1) > 0.8f)
                page %= 100;
        }
        else
        {
            page %= 12;
            if (Rando.Float(1) > 0.8f)
                page %= 30;
        }
        if (numSinceLowRating == 2)
            _orderMode = WorkshopQueryFilterOrder.RankedByTrend;
        else
            _orderMode = WorkshopQueryFilterOrder.RankedByVote;
        if (Rando.Float(1) > 0.7f)
        {
            _orderMode = Rando.Int(5) switch
            {
                0 => WorkshopQueryFilterOrder.FavoritedByFriendsRankedByPublicationDate,
                1 => WorkshopQueryFilterOrder.CreatedByFriendsRankedByPublicationDate,
                _ => WorkshopQueryFilterOrder.RankedByTotalUniqueSubscriptions,
            };
        }
        if (page == 0)
            page = 1;
        numSinceLowRating++;
        WorkshopQueryAll workshopQueryAll = Steam.CreateQueryAll(_orderMode, WorkshopType.Items);
        workshopQueryAll.requiredTags.Add("Deathmatch");
        workshopQueryAll.excludedTags.Add("Exclude From Random");
        workshopQueryAll.ResultFetched += Fetched;
        workshopQueryAll._page = (uint)page;
        workshopQueryAll.justOnePage = true;
        workshopQueryAll.Request();
        _fetchDelay = 5;
    }

    public static void Update()
    {
        _fetchDelay = Lerp.Float(_fetchDelay, 0, Maths.IncFrameTimer());
        if (!Steam.IsInitialized() || !Network.isServer || TeamSelect2.GetSettingInt("workshopmaps") <= 0)
            return;
        if (_downloadingItems.Count > 0)
        {
            for (int i = 0; i < _downloadingItems.Count; i++)
            {
                if (_downloadingItems[i].finishedProcessing)
                {
                    if (_downloadingItems[i].downloadResult == SteamResult.OK)
                    {
                        List<string> levelList = GetLevelList(_downloadingItems[i].path);
                        ProcessLevel(levelList[Rando.Int(levelList.Count - 1)]);
                    }
                    _downloadingItems.RemoveAt(i);
                    i--;
                }
            }
        }
        else if (_readyLevels.Count < numToHaveReady)
        {
            if (_totalMaps == 0)
            {
                _toFetchIndex = -1;
                _numFetch = 0;
                WorkshopQueryAll workshopQueryAll = Steam.CreateQueryAll(_orderMode, WorkshopType.Items);
                workshopQueryAll.requiredTags.Add("Deathmatch");
                workshopQueryAll.excludedTags.Add("Exclude From Random");
                workshopQueryAll.QueryFinished += FinishedTotalQuery;
                workshopQueryAll._dataToFetch = WorkshopQueryData.TotalOnly;
                workshopQueryAll.Request();
                _totalMaps = -1;
                DevConsole.Log(DCSection.Steam, "Querying for random levels.");
            }
            else if (_totalMaps != -1 && _fetchDelay <= 0f)
                DownloadRandomMap();
        }
    }

    public static LevelData GetNextLevel()
    {
        if (_readyLevels.Count == 0)
            return null;
        LevelData result = _readyLevels.First();
        _readyLevels.RemoveAt(0);
        return result;
    }

    public static LevelData PeekNextLevel()
    {
        if (_readyLevels.Count == 0)
            return null;
        return _readyLevels.First();
    }

    #endregion

    #region Private Fields

    static void Fetched(object sender, WorkshopQueryResult result)
    {
        _fetchDelay = 0;
        if (_currentWorkshopLevelQuery == null || _currentWorkshopLevelQuery != sender)
        {
            _numFetch = 0;
            _toFetchIndex = Rando.Int((int)((sender as WorkshopQueryAll)._numResultsFetched - 1));
            _currentWorkshopLevelQuery = sender;
        }
        if (_toFetchIndex == _numFetch)
        {
            if (Global.data.blacklist.Contains(result.details.publishedFile.id))
            {
                if (_numFetch < 49)
                    _numFetch++;
            }
            else if (Steam.DownloadWorkshopItem(result.details.publishedFile))
                _downloadingItems.Add(result.details.publishedFile);
            else
                ProcessWorkshopItem(result.details.publishedFile);
        }
        _numFetch++;
    }

    static void FinishedTotalQuery(object sender)
    {
        WorkshopQueryAll fin = sender as WorkshopQueryAll;
        if (fin._numResultsTotal != 0)
            _totalMaps = (int)fin._numResultsTotal;
    }

    static void ProcessLevel(string path)
    {
        Main.SpecialCode = ($"Loading Level {path}" != null) ? path : "null"; //schizo
        try
        {
            if (!path.EndsWith(".lev"))
                return;
            path = path.Replace('\\', '/');
            LevelData dat = null;
            dat = DuckFile.LoadLevel(path);
            dat.SetPath(path);
            path = path[..^4];
            bool canLoad = true;
            if (dat.modData.workshopIDs.Count != 0)
            {
                foreach (ulong id in dat.modData.workshopIDs)
                {
                    bool found = false;
                    foreach (Mod m in ModLoader.accessibleMods)
                    {
                        if (m.configuration != null && m.configuration.workshopID == id)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        canLoad = false;
                        break;
                    }
                }
            }
            if (canLoad && !dat.modData.hasLocalMods)
            {
                _readyLevels.Add(dat);
                DevConsole.Log(DCSection.Steam, $"Downloaded random level {_readyLevels.Count}/{numToHaveReady}");
            }
            else
                DevConsole.Log(DCSection.Steam, "Downloaded level had incompatible mods, and was ignored!");
        }
        catch (Exception)
        {
        }
    }

    static List<string> GetLevelList(string pItemPath, List<string> pLevels = null)
    {
        pLevels ??= [];
        string[] files = DuckFile.GetFiles(pItemPath, "*.lev");
        foreach (string f in files)
            pLevels.Add(f);
        files = DuckFile.GetDirectories(pItemPath);
        for (int i = 0; i < files.Length; i++)
        {
            _ = files[i];
            GetLevelList(pItemPath, pLevels);
        }
        return pLevels;
    }

    #endregion
}
