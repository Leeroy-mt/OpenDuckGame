namespace DuckGame;

public class ATRCShrapnel : AmmoType
{
	public ATRCShrapnel()
	{
		accuracy = 0.75f;
		range = 250f;
		penetration = 0.4f;
		bulletSpeed = 18f;
		combustable = true;
	}

	public override void MakeNetEffect(Vec2 pos, bool fromNetwork = false)
	{
		int i;
		for (i = 0; i < 1; i++)
		{
			Level.Add(new ExplosionPart(pos.x - 20f + Rando.Float(40f), pos.y - 20f + Rando.Float(40f)));
			i++;
		}
		SFX.Play("explode");
	}
}
