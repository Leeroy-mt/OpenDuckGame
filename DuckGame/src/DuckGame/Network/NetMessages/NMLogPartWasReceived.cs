namespace DuckGame;

public class NMLogPartWasReceived : NMEvent
{
	public override void Activate()
	{
		DuckNetwork.core.logTransferProgress++;
		if (DuckNetwork.core.logTransferProgress == DuckNetwork.core.logTransferSize)
		{
			DevConsole.LogSendingComplete(base.connection);
		}
	}
}
