namespace DuckGame;

public class NMCurrentLevel : NMDuckNetwork
{
	public new byte levelIndex;

	public NMCurrentLevel()
	{
	}

	public NMCurrentLevel(byte idx)
	{
		levelIndex = idx;
	}

	public override string ToString()
	{
		return base.ToString() + "(index = " + levelIndex + ")";
	}
}
