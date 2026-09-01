using Steamworks;

namespace Steam;

public abstract class WorkshopQueryUGC : WorkshopQueryBase
{
    #region Public Properties

    public IList<string> RequiredTags { get; internal set; }
    public IList<string> ExcludedTags { get; internal set; }

    #endregion

    internal WorkshopQueryUGC()
    {
        RequiredTags = [];
        ExcludedTags = [];
    }

    internal override void SetQueryData()
    {
        base.SetQueryData();

        foreach (string tag in RequiredTags)
            SteamUGC.AddRequiredTag(handle, tag);

        foreach (string tag in ExcludedTags)
            SteamUGC.AddExcludedTag(handle, tag);
    }
}