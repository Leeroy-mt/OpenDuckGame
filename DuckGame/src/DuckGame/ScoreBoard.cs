namespace DuckGame;

public class ScoreBoard : Thing
{
	public override void Initialize()
	{
		int index = 0;
		foreach (Team t in Teams.all)
		{
			if (t.activeProfiles.Count > 0)
			{
				PlayerCard c = new PlayerCard((float)index * 1f, new Vec2(-400f, 140 * index + 120), new Vec2(Graphics.width / 2 - 200, 140 * index + 120), t);
				Level.current.AddThing(c);
				index++;
			}
		}
	}
}
