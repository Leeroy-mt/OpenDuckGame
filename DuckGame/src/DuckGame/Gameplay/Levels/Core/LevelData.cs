namespace DuckGame;

[ChunkVersion(2)]
[MagicNumber(5033950674723417L)]
public class LevelData : BinaryClassChunk
{
    private LevelMetaData _rerouteMetadata;

    private string _path;

    private LevelLocation _location;

    private WorkshopItem _publishingWorkshopItem;

    public LevelMetaData metaData
    {
        get
        {
            if (_rerouteMetadata == null)
            {
                return GetChunk<LevelMetaData>("metaData");
            }
            return _rerouteMetadata;
        }
    }

    public LevelCustomData customData => GetChunk<LevelCustomData>("customData");

    public WorkshopMetaData workshopData => GetChunk<WorkshopMetaData>("workshopData", pPartialDeserialize: false, pForceCreation: true);

    public ModMetaData modData => GetChunk<ModMetaData>("modData", pPartialDeserialize: false, pForceCreation: true);

    public ProceduralChunkData proceduralData => GetChunk<ProceduralChunkData>("proceduralData");

    public PreviewData previewData => GetChunk<PreviewData>("previewData");

    public LevelObjects objects => GetChunk<LevelObjects>("objects");

    public void RerouteMetadata(LevelMetaData data)
    {
        _rerouteMetadata = data;
    }

    public void SetPath(string path)
    {
        _path = path;
    }

    public string GetPath()
    {
        return _path;
    }

    public void SetLocation(LevelLocation loc)
    {
        _location = loc;
    }

    public LevelLocation GetLocation()
    {
        return _location;
    }

    public WorkshopItem GetPublishingWorkshopItem()
    {
        return _publishingWorkshopItem;
    }

    public void SetPublishingWorkshopItem(WorkshopItem pItem)
    {
        _publishingWorkshopItem = pItem;
    }

    public LevelData Clone()
    {
        BinaryClassChunk.fullDeserializeMode = true;
        LevelData levelData = DuckFile.LoadLevel(Serialize().buffer);
        BinaryClassChunk.fullDeserializeMode = false;
        levelData._path = _path;
        levelData._location = _location;
        return levelData;
    }
}
