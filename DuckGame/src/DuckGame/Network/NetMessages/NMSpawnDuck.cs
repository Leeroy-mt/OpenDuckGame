namespace DuckGame;

public class NMSpawnDuck : NMEvent
{
	public byte index;

	public NMSpawnDuck(byte idx)
	{
		index = idx;
	}

	public NMSpawnDuck()
	{
	}

	public override void Activate()
	{
		if (index < 0 || index >= DuckNetwork.profiles.Count)
		{
			return;
		}
		Profile p = DuckNetwork.profiles[index];
		if (p != null && p.duck != null && p.persona != null)
		{
			if (p.localPlayer)
			{
				p.duck.connection = DuckNetwork.localConnection;
			}
			else
			{
				p.duck.connection = p.connection;
			}
			p.duck.visible = true;
			Vec3 col = p.persona.color;
			Level.Add(new SpawnLine(p.duck.x, p.duck.y, 0, 0f, new Color((int)col.x, (int)col.y, (int)col.z), 32f));
			Level.Add(new SpawnLine(p.duck.x, p.duck.y, 0, -4f, new Color((int)col.x, (int)col.y, (int)col.z), 4f));
			Level.Add(new SpawnLine(p.duck.x, p.duck.y, 0, 4f, new Color((int)col.x, (int)col.y, (int)col.z), 4f));
			SFX.Play("pullPin", 0.7f);
		}
	}
}
