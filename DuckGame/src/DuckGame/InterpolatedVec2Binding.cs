namespace DuckGame;

public class InterpolatedVec2Binding : CompressedVec2Binding
{
	public InterpolatedVec2Binding(string field, int range = int.MaxValue, bool real = true)
		: base(field, range)
	{
		_priority = GhostPriority.High;
	}
}
