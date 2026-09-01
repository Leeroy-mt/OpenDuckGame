namespace Steam;

public class SessionState
{
    #region Public Fields

    public byte ConnectionActive;
    public byte Connecting;
    public byte P2PSessionError;
    public byte UsingRelay;

    public ushort RemotePort;

    public int BytesQueuedForSend;
    public int PacketsQueuedForSend;

    public uint RemoteIP;

    #endregion
}
