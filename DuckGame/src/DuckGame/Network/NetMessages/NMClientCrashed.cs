namespace DuckGame;

public class NMClientCrashed : NMEvent, IConnectionMessage
{
	public override void Activate()
	{
		Network.activeNetwork.core.DisconnectClient(base.connection, new DuckNetErrorInfo(DuckNetError.ClientCrashed, "CRASH"));
		base.Activate();
	}
}
