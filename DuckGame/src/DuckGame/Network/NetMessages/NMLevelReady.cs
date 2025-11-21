namespace DuckGame;

public class NMLevelReady : NMDuckNetworkEvent
{
	public new byte levelIndex;

	public NMLevelReady()
	{
	}

	public NMLevelReady(byte pLevelIndex)
	{
		levelIndex = pLevelIndex;
	}

	public override void Activate()
	{
		DevConsole.Log(DCSection.DuckNet, "|DGORANGE|Level ready message(" + base.connection.levelIndex + " -> " + levelIndex + ")", base.connection);
		base.connection.levelIndex = levelIndex;
		if (Network.isServer && levelIndex == DuckNetwork.levelIndex)
		{
			Level.current.ClientReady(base.connection);
		}
	}
}
