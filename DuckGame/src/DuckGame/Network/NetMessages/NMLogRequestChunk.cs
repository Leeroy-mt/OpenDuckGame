namespace DuckGame;

public class NMLogRequestChunk : NMEvent
{
    public string data;

    public NMLogRequestChunk(string pData)
    {
        data = pData;
    }

    public NMLogRequestChunk()
    {
    }

    public override void Activate()
    {
        if (DevConsole.core.requestingLogs.Contains(base.connection))
        {
            DevConsole.core.ReceiveLogData(data, base.connection);
            base.connection.logTransferProgress++;
            Send.Message(new NMLogPartWasReceived(), base.connection);
            if (base.connection.logTransferProgress == base.connection.logTransferSize)
            {
                DevConsole.LogTransferComplete(base.connection);
            }
        }
    }
}
