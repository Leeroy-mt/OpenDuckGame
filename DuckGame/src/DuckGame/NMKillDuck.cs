namespace DuckGame;

public class NMKillDuck : NMEvent
{
	public byte index;

	public bool crush;

	public bool cook;

	public bool fall;

	public byte lifeChange;

	private byte _levelIndex;

	public NMKillDuck(byte idx, bool wasCrush, bool wasCook, bool wasFall, byte pLifeChange)
	{
		index = idx;
		crush = wasCrush;
		cook = wasCook;
		fall = wasFall;
		lifeChange = pLifeChange;
	}

	public NMKillDuck(byte idx, bool wasCrush, bool wasCook)
	{
		index = idx;
		crush = wasCrush;
		cook = wasCook;
	}

	public NMKillDuck()
	{
	}

	public override void Activate()
	{
		if (DuckNetwork.levelIndex != _levelIndex || index >= DuckNetwork.profiles.Count)
		{
			return;
		}
		Profile p = DuckNetwork.profiles[index];
		if (p.duck == null || !p.duck.WillAcceptLifeChange(lifeChange))
		{
			return;
		}
		DestroyType t = null;
		t = (crush ? new DTCrush(null) : ((!fall) ? ((DestroyType)new DTImpact(null)) : ((DestroyType)new DTFall())));
		p.duck.isKillMessage = true;
		if (p.duck.Kill(t))
		{
			if (!cook)
			{
				p.duck.GoRagdoll();
			}
			Thing.Fondle(p.duck, base.connection);
			if (p.duck._ragdollInstance != null)
			{
				Thing.Fondle(p.duck._ragdollInstance, base.connection);
			}
			if (p.duck._trappedInstance != null)
			{
				Thing.Fondle(p.duck._trappedInstance, base.connection);
			}
			if (p.duck._cookedInstance != null)
			{
				Thing.Fondle(p.duck._cookedInstance, base.connection);
			}
		}
		p.duck.isKillMessage = false;
	}

	protected override void OnSerialize()
	{
		base.OnSerialize();
		_serializedData.Write(DuckNetwork.levelIndex);
	}

	public override void OnDeserialize(BitBuffer d)
	{
		base.OnDeserialize(d);
		_levelIndex = d.ReadByte();
	}
}
