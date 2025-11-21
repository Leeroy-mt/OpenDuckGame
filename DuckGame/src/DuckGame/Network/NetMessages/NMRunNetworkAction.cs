using System.Reflection;

namespace DuckGame;

public class NMRunNetworkAction : NMEvent
{
	public PhysicsObject target;

	public byte actionIndex;

	public NMRunNetworkAction(PhysicsObject pTarget, byte pNetworkActionIndex)
	{
		target = pTarget;
		actionIndex = pNetworkActionIndex;
	}

	public NMRunNetworkAction()
	{
	}

	public override void Activate()
	{
		if (target != null && actionIndex != byte.MaxValue)
		{
			MethodInfo method = Editor.MethodFromNetworkActionIndex(target.GetType(), actionIndex);
			if (method != null)
			{
				method.Invoke(target, null);
			}
		}
	}
}
