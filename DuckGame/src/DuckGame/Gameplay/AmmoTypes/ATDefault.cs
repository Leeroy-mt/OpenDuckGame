namespace DuckGame;

public class ATDefault : AmmoType
{
    public ATDefault()
    {
        accuracy = 0.75f;
        range = 250f;
        penetration = 1f;
        combustable = true;
    }

    public override void PopShell(float x, float y, int dir)
    {
        Level.Add(new PistolShell(x, y)
        {
            hSpeed = (float)dir * (1.5f + Rando.Float(1f))
        });
    }
}
