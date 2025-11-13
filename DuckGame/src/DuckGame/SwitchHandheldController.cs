using System.Runtime.CompilerServices;

namespace DuckGame;

public class SwitchHandheldController : SwitchJoyConDual
{
	public override extern bool isConnected
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public SwitchHandheldController()
		: base(0)
	{
	}
}
