namespace DuckGame;

public class NMObjectNeedsInitialize : NMDuckNetworkEvent
{
	public Thing thing;

	public NMObjectNeedsInitialize(Thing t)
	{
		thing = t;
	}

	public NMObjectNeedsInitialize()
	{
	}

	public override void Activate()
	{
		if (thing != null && thing.ghostObject != null)
		{
			if (thing.connection == base.connection)
			{
				Thing.Fondle(thing, DuckNetwork.localConnection);
			}
			if (thing.isServerForObject)
			{
				thing.ghostObject.DirtyStateMask(long.MaxValue, base.connection);
			}
		}
	}
}
