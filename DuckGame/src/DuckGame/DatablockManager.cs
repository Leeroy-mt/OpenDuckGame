namespace DuckGame;

public class DatablockManager
{
	private NetworkConnection _connection;

	private StreamManager _manager;

	public DatablockManager(NetworkConnection connection, StreamManager streamManager)
	{
		_connection = connection;
		_manager = streamManager;
	}

	public void OnMessage(NetMessage m)
	{
	}

	public void Update()
	{
	}

	public static void BuildLevelInitializerBlock()
	{
	}
}
