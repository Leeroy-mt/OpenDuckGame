using System.Collections.Generic;

#if FACEPUNCH
using Steamworks;
#else
using Steam;
#endif

namespace DuckGame;

public class WorkshopMetaData : BinaryClassChunk
{
    public string name;

    public string description;

    public string author;

#if FACEPUNCH
    public Visibility visibility;
#else
    public RemoteStoragePublishedFileVisibility visibility;
#endif

    public List<string> tags;

    public List<ulong> dependencies;

    public WorkshopMetaData()
    {
#if FACEPUNCH
        if (FacepunchSteam.SteamId != 0)
            author = FacepunchSteam.Me.Name;
#else
        if (DGSteam.User != null)
            author = DGSteam.User.Name;
#endif
        Reset();
    }

    public void Reset()
    {
        name = "";
        description = "";
        author = "";
#if FACEPUNCH
        visibility = Visibility.Public;
#else
        visibility = RemoteStoragePublishedFileVisibility.Public;
#endif
        tags = new List<string>();
        dependencies = new List<ulong>();
    }
}

#if FACEPUNCH
public enum Visibility
{
    Public,
    FriendsOnly,
    Private
}
#endif