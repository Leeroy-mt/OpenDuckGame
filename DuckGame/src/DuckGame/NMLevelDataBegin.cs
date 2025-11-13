namespace DuckGame;

public class NMLevelDataBegin : NMConditionalEvent
{
	public new byte levelIndex;

	public NMLevelDataBegin()
	{
	}

	public NMLevelDataBegin(byte pLevelIndex)
	{
		levelIndex = pLevelIndex;
	}

	public override bool Update()
	{
		if (Level.current.networkIndex == levelIndex)
		{
			return Level.current.initializeFunctionHasBeenRun;
		}
		return false;
	}

	public override void Activate()
	{
	}
}
