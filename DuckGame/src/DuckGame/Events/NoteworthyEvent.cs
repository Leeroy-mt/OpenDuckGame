namespace DuckGame;

public class NoteworthyEvent
{
    public static string GoodKillDeathRatio = "GoodKillDeathRatio";

    public static string BadKillDeathRatio = "BadKillDeathRatio";

    public static string ManyFallDeaths = "ManyFallDeaths";

    public string eventTag;

    public Profile who;

    public float quality;

    public NoteworthyEvent(string tag, Profile owner, float q)
    {
        eventTag = tag;
        who = owner;
        quality = q;
    }
}
