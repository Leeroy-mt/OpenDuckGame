namespace DuckGame;

public class NMClientClosedGame : NMEvent, IConnectionMessage
{
	public override void Activate()
	{
		Network.activeNetwork.core.DisconnectClient(base.connection, new DuckNetErrorInfo(DuckNetError.ClientCrashed, "CLOSED"));
		base.Activate();
	}
}
