public class SessionState
{
    #region Public Fields

    public byte m_bConnectionActive;
    public byte m_bConnecting;
    public byte m_eP2PSessionError;
    public byte m_bUsingRelay;

    public ushort m_nRemotePort;

    public int m_nBytesQueuedForSend;
    public int m_nPacketsQueuedForSend;

    public uint m_nRemoteIP;

    #endregion
}
