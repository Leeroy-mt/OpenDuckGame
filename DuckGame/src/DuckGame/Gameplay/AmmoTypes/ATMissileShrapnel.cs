namespace DuckGame;

public class ATMissileShrapnel : AmmoType
{
	public ATMissileShrapnel()
	{
		accuracy = 0.75f;
		range = 250f;
		penetration = 0.4f;
		bulletSpeed = 18f;
		combustable = true;
	}

	public override void MakeNetEffect(Vec2 pos, bool fromNetwork = false)
	{
		Level.Add(new ExplosionPart(pos.x + Rando.Float(-2f, 2f), pos.y + Rando.Float(-2f, 2f), doWait: false));
		for (int repeat = 0; repeat < 4; repeat++)
		{
			Level.Add(new ExplosionPart(pos.x + Rando.Float(-11f, 11f), pos.y + Rando.Float(-11f, 11f), doWait: false));
		}
		if (fromNetwork)
		{
			foreach (PhysicsObject p in Level.CheckCircleAll<PhysicsObject>(pos, 70f))
			{
				if (p.isServerForObject)
				{
					p.sleeping = false;
					p.vSpeed = -2f;
				}
			}
		}
		SFX.Play("explode");
	}
}
