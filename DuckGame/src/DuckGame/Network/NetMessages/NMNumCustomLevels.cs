namespace DuckGame;

public class NMNumCustomLevels : NMDuckNetworkEvent
{
    public int customLevels;

    public NMNumCustomLevels(int pCustomLevels)
    {
        customLevels = pCustomLevels;
    }

    public NMNumCustomLevels()
    {
    }

    public override void Activate()
    {
        foreach (Profile p in DuckNetwork.profiles)
        {
            if (p.connection == base.connection)
            {
                p.numClientCustomLevels = customLevels;
            }
        }
        TeamSelect2.UpdateModifierStatus();
    }
}
