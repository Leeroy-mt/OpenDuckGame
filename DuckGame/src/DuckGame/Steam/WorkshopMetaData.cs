using System.Collections.Generic;

namespace DuckGame;

public class WorkshopMetaData : BinaryClassChunk
{
    public string name;

    public string description;

    public string author;

    public RemoteStoragePublishedFileVisibility visibility;

    public List<string> tags;

    public List<ulong> dependencies;

    public WorkshopMetaData()
    {
        if (Steam.User != null)
        {
            author = Steam.User.Name;
        }
        Reset();
    }

    public void Reset()
    {
        name = "";
        description = "";
        author = "";
        visibility = RemoteStoragePublishedFileVisibility.Public;
        tags = new List<string>();
        dependencies = new List<ulong>();
    }
}
