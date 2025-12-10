using System.Collections.Generic;

namespace DuckGame;

public class LSFilterLevelType : IFilterLSItems
{
    private static Dictionary<string, LevelType> _types = new Dictionary<string, LevelType>();

    private LevelType _type;

    private bool _needsDeathmatchTag;

    public LSFilterLevelType(LevelType type, bool needsDeathmatchTag = false)
    {
        _type = type;
        _needsDeathmatchTag = needsDeathmatchTag;
    }

    public bool Filter(string lev, LevelLocation location = LevelLocation.Any)
    {
        try
        {
            LevelType levType = LevelType.Invalid;
            if (_types.TryGetValue(lev, out levType))
            {
                return levType == _type;
            }
            LevelData dat = DuckFile.LoadLevelHeaderCached(lev);
            if (dat == null)
            {
                _types[lev] = LevelType.Invalid;
                return false;
            }
            levType = dat.metaData.type;
            _types[lev] = levType;
            return levType == _type;
        }
        catch
        {
            _types[lev] = LevelType.Invalid;
            return false;
        }
    }
}
