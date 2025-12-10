using System.Collections.Generic;

namespace DuckGame;

public class DevConsoleCore
{
    public HashSet<NetworkConnection> requestingLogs = new HashSet<NetworkConnection>();

    public HashSet<NetworkConnection> transferRequestsPending = new HashSet<NetworkConnection>();

    public Dictionary<NetworkConnection, string> receivingLogs = new Dictionary<NetworkConnection, string>();

    public Queue<NetMessage> pendingSends = new Queue<NetMessage>();

    public bool constantSync;

    public int viewOffset;

    public int logScores = -1;

    public Queue<DCLine> lines = new Queue<DCLine>();

    public List<DCLine> pendingLines = new List<DCLine>();

    public List<DCChartValue> pendingChartValues = new List<DCChartValue>();

    public BitmapFont font;

    public FancyBitmapFont fancyFont;

    public float alpha;

    public bool open;

    public string typing = "";

    public List<string> previousLines = new List<string>();

    public bool splitScreen;

    public bool rhythmMode;

    public bool qwopMode;

    public bool showIslands;

    public bool showCollision;

    public bool shieldMode;

    public int cursorPosition;

    public int lastCommandIndex;

    public string lastLine = "";

    public void ReceiveLogData(string pData, NetworkConnection pConnection)
    {
        if (requestingLogs.Contains(pConnection))
        {
            string dat = null;
            if (!receivingLogs.TryGetValue(pConnection, out dat))
            {
                dat = (receivingLogs[pConnection] = "");
            }
            receivingLogs[pConnection] += pData;
        }
    }

    public string GetReceivedLogData(NetworkConnection pConnection)
    {
        if (receivingLogs.ContainsKey(pConnection))
        {
            return receivingLogs[pConnection];
        }
        return null;
    }
}
