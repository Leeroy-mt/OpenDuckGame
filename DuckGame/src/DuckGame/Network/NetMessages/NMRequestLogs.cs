namespace DuckGame;

public class NMRequestLogs : NMEvent
{
    public override void Activate()
    {
        if (base.connection.profile != null)
        {
            DevConsole.Log("@error@" + base.connection.ToString() + " is requesting your Netlog!", Color.Red);
            DevConsole.Log("@error@Only accept this request if it's from someone you trust!", Color.Red);
            DevConsole.Log("@error@type |WHITE|accept " + base.connection.profile.networkIndex + " |RED|to begin transfer.", Color.Red);
            DevConsole.core.transferRequestsPending.Add(base.connection);
        }
    }
}
