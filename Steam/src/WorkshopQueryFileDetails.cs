using Steamworks;

namespace Steam;

public class WorkshopQueryFileDetails : WorkshopQueryBase
{
    public IList<ulong> Files { get; internal set; }

    public WorkshopQueryFileDetails()
    {
        Files = [];
    }

    internal override void Create()
    {
        handle = SteamUGC.CreateQueryUGCDetailsRequest(SteamHelper.GetArray(Files, id => new PublishedFileId_t(id)), (uint)Files.Count);
    }
}