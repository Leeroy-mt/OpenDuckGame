namespace DuckGame;

public class NMGhostState : NetMessage
{
    public GhostObjectHeader header;

    public BitBuffer data;

    public long mask;

    public GhostObject ghost;

    public NetIndex16 id => header.id;

    public new byte levelIndex => header.levelIndex;

    public ushort classID => header.classID;

    public NetIndex8 authority => header.authority;

    public NetIndex16 tick
    {
        get
        {
            return header.tick;
        }
        set
        {
            header.tick = value;
        }
    }

    public bool minimalState { get; set; }

    public NMGhostState(BitBuffer dat)
    {
        data = dat;
        manager = BelongsToManager.GhostManager;
        header = new GhostObjectHeader(pClean: true);
    }

    public NMGhostState()
    {
        manager = BelongsToManager.GhostManager;
        header = new GhostObjectHeader(pClean: true);
    }

    protected override void OnSerialize()
    {
        _serializedData.WriteBufferData(data);
    }

    public override void OnDeserialize(BitBuffer d)
    {
        header = GhostObjectHeader.Deserialize(d, minimalState);
        data = d.ReadBitBuffer();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
