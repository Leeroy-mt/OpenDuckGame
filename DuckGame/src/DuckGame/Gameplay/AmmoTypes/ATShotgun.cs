namespace DuckGame;

public class ATShotgun : AmmoType
{
    public ATShotgun()
    {
        accuracy = 0.6f;
        range = 115f;
        penetration = 1f;
        rangeVariation = 10f;
        combustable = true;
    }

    public override void PopShell(float x, float y, int dir)
    {
        Level.Add(new ShotgunShell(x, y)
        {
            hSpeed = (float)dir * (1.5f + Rando.Float(1f))
        });
    }
}
