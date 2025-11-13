namespace DuckGame;

public class NMLogRequestIncoming : NMEvent
{
	public int numChunks;

	public NMLogRequestIncoming(int pNumChunks)
	{
		numChunks = pNumChunks;
	}

	public NMLogRequestIncoming()
	{
	}

	public override void Activate()
	{
		if (DevConsole.core.requestingLogs.Contains(base.connection))
		{
			base.connection.logTransferSize = numChunks;
			base.connection.logTransferProgress = 0;
		}
	}
}
