namespace DuckGame;

public class ATHighCalMachinegun : AmmoType
{
	public ATHighCalMachinegun()
	{
		range = 200f;
		penetration = 2f;
		combustable = true;
		accuracy = 0.85f;
	}

	public override void PopShell(float x, float y, int dir)
	{
		Level.Add(new SniperShell(x, y)
		{
			hSpeed = (float)dir * (1.5f + Rando.Float(1f))
		});
	}
}
