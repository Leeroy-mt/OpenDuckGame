namespace DuckGame;

public class NMPipeOut : NMEvent
{
	public Vec2 position;

	public byte direction;

	public NMPipeOut(Vec2 pPosition, byte pDirection)
	{
		direction = pDirection;
		position = pPosition;
	}

	public NMPipeOut()
	{
	}

	public override void Activate()
	{
		for (int i = 0; i < 6; i++)
		{
			SmallSmoke s = SmallSmoke.New(position.x + Rando.Float(-4f, 4f), position.y + Rando.Float(-4f, 4f));
			if (direction == 0)
			{
				s.velocity = new Vec2(Rando.Float(-0.5f, 0.5f), Rando.Float(0f, -0.5f));
				Level.current.CollisionPoint<PipeTileset>(position)?.FlapPipe();
			}
			else if (direction == 1)
			{
				s.velocity = new Vec2(Rando.Float(0.2f, 0.7f), Rando.Float(-0.5f, 0.5f));
				Level.current.CollisionPoint<PipeTileset>(position + new Vec2(-10f, 0f))?.FlapPipe();
			}
			else if (direction == 3)
			{
				s.velocity = new Vec2(Rando.Float(-0.7f, -0.2f), Rando.Float(-0.5f, 0.5f));
				Level.current.CollisionPoint<PipeTileset>(position + new Vec2(10f, 0f))?.FlapPipe();
			}
			else if (direction == 2)
			{
				s.velocity = new Vec2(Rando.Float(-0.5f, 0.5f), Rando.Float(0.2f, 0.7f));
				Level.current.CollisionPoint<PipeTileset>(position + new Vec2(0f, -10f))?.FlapPipe();
			}
			Level.Add(s);
		}
		SFX.Play("pipeOut", 1f, Rando.Float(-0.1f, 0.1f));
		base.Activate();
	}
}
