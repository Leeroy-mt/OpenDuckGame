namespace DuckGame;

[EditorGroup("Spawns")]
[BaggedProperty("isInDemo", true)]
[BaggedProperty("previewPriority", true)]
public class FreeSpawn : SpawnPoint
{
	public EditorProperty<int> spawnType = new EditorProperty<int>(0, null, 0f, 2f, 1f);

	public EditorProperty<bool> secondSpawn = new EditorProperty<bool>(val: false);

	public EditorProperty<bool> eightPlayerOnly = new EditorProperty<bool>(val: false);

	private SpriteMap _eight;

	public FreeSpawn(float xpos = 0f, float ypos = 0f)
		: base(xpos, ypos)
	{
		graphic = new SpriteMap("duckSpawn", 32, 32)
		{
			depth = 0.9f
		};
		_editorName = "Spawn Point";
		center = new Vec2(16f, 23f);
		collisionSize = new Vec2(16f, 16f);
		collisionOffset = new Vec2(-8f, -16f);
		_visibleInGame = false;
		editorTooltip = "Basic spawn point for a single Duck. Every level needs at least one.";
		secondSpawn._tooltip = "If set, this duck will be the alternate duck in a 1V1 pair.";
	}

	public override void Draw()
	{
		frame = spawnType;
		if (secondSpawn.value)
		{
			frame = 3;
		}
		if (eightPlayerOnly.value)
		{
			if (_eight == null)
			{
				_eight = new SpriteMap("redEight", 10, 10);
				_eight.CenterOrigin();
			}
			Graphics.Draw(_eight, base.x - 5f, base.y + 7f, 1f);
		}
		base.Draw();
	}
}
