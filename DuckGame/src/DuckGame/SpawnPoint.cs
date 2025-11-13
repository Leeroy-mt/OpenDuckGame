namespace DuckGame;

public abstract class SpawnPoint : Thing
{
	public SpawnPoint(float xpos, float ypos)
		: base(xpos, ypos)
	{
	}

	public override void Draw()
	{
		graphic.flipH = flipHorizontal;
		base.Draw();
	}
}
