namespace DuckGame;

public class ATSniper : AmmoType
{
	public ATSniper()
	{
		accuracy = 0.75f;
		range = 250f;
		penetration = 1f;
		combustable = true;
		range = 1000f;
		accuracy = 1f;
		penetration = 2f;
		bulletSpeed = 48f;
	}

	public override void PopShell(float x, float y, int dir)
	{
		Level.Add(new SniperShell(x, y)
		{
			hSpeed = (float)dir * (1.5f + Rando.Float(1f))
		});
	}
}
