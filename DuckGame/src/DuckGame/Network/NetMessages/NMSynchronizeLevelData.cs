namespace DuckGame;

public class NMSynchronizeLevelData : NMDuckNetworkEvent
{
    public override void Activate()
    {
        string customLevel = Deathmatch.RandomLevelString("", "deathmatch", forceCustom: true);
        if (customLevel == "")
        {
            LevelData level = Content.GetLevel(customLevel);
            level.GetChecksum();
            XMLLevel.GetCompressedLevelData(level, customLevel.Substring(0, customLevel.Length - 7));
        }
    }
}
