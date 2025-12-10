using System.Collections.Generic;

namespace DuckGame;

public class EventManager
{
    private NetworkConnection _connection;

    private StreamManager _manager;

    private Dictionary<Thing, GhostObject> _ghosts = new Dictionary<Thing, GhostObject>();

    public EventManager(NetworkConnection connection, StreamManager streamManager)
    {
        _connection = connection;
        _manager = streamManager;
    }

    public void OnMessage(NetMessage m)
    {
        if (m is NMEvent)
        {
            (m as NMEvent).Activate();
        }
        else if (m is NMSynchronizedEvent)
        {
            (m as NMSynchronizedEvent).Activate();
        }
        else if (m is NMConditionalEvent)
        {
            (m as NMConditionalEvent).Activate();
        }
        Level.current.OnMessage(m);
    }

    public void Update()
    {
    }
}
