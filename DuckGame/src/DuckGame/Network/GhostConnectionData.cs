namespace DuckGame;

public class GhostConnectionData
{
    private long _connectionStateMask = long.MaxValue;

    public ushort prevInputState;

    public uint latestCommandTickReceived;

    public NetIndex16 lastTickSent = 1;

    public NetIndex16 lastTickReceived = 0;

    public NetIndex8 authority = 1;

    public long connectionStateMask
    {
        get
        {
            return _connectionStateMask;
        }
        set
        {
            _connectionStateMask = value;
        }
    }
}
