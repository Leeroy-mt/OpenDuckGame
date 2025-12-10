using System.Collections.Generic;

namespace DuckGame;

public class LSFilterMods : IFilterLSItems
{
    private bool _isOnline;

    private static Dictionary<string, Dictionary<bool, bool>> _filters = new Dictionary<string, Dictionary<bool, bool>>();

    public LSFilterMods(bool isOnline)
    {
        _isOnline = isOnline;
    }

    private bool Cache(string lev, bool result)
    {
        Dictionary<bool, bool> filts = null;
        if (!_filters.TryGetValue(lev, out filts))
        {
            filts = (_filters[lev] = new Dictionary<bool, bool>());
        }
        filts[_isOnline] = result;
        return result;
    }

    public bool Filter(string lev, LevelLocation location = LevelLocation.Any)
    {
        try
        {
            Dictionary<bool, bool> filts = null;
            if (_filters.TryGetValue(lev, out filts) && filts.TryGetValue(_isOnline, out var val))
            {
                return val;
            }
            LevelData dat = DuckFile.LoadLevelHeaderCached(lev);
            if (dat == null)
            {
                return Cache(lev, result: false);
            }
            ModMetaData modData = dat.modData;
            if (_isOnline)
            {
                if (modData.hasLocalMods && !MonoMain.modDebugging)
                {
                    return Cache(lev, result: false);
                }
                HashSet<ulong> workshops = new HashSet<ulong>();
                foreach (Mod mod in ModLoader.accessibleMods)
                {
                    if (mod.configuration.isWorkshop || mod.configuration.assignedWorkshopID != 0L)
                    {
                        workshops.Add(mod.configuration.assignedWorkshopID);
                    }
                    if (mod.workshopIDFacade != 0L)
                    {
                        workshops.Add(mod.workshopIDFacade);
                    }
                }
                if (!modData.workshopIDs.IsSubsetOf(workshops))
                {
                    return Cache(lev, result: false);
                }
            }
            return Cache(lev, result: true);
        }
        catch
        {
            return Cache(lev, result: false);
        }
    }
}
