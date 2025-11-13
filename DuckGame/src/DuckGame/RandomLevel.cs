namespace DuckGame;

public class RandomLevel : DeathmatchLevel
{
	public static int currentComplexityDepth;

	private new RandomLevelNode _level;

	public RandomLevel()
		: base("RANDOM")
	{
		_level = LevelGenerator.MakeLevel();
	}

	public override void Initialize()
	{
		_level.LoadParts(0f, 0f, this);
		Level.Add(new OfficeBackground(0f, 0f)
		{
			visible = false
		});
		base.Initialize();
	}

	public override void Update()
	{
		base.Update();
	}
}
