namespace DuckGame;

public struct ResultData
{
	public string name;

	public bool multi;

	public object data;

	public int score;

	public BitmapFont font;

	public ResultData(Team t)
	{
		font = Profiles.EnvironmentProfile.font;
		if (t.activeProfiles.Count > 1)
		{
			name = t.GetNameForDisplay();
			multi = true;
		}
		else
		{
			if (Profiles.IsDefault(t.activeProfiles[0]))
			{
				name = t.GetNameForDisplay();
			}
			else
			{
				name = t.activeProfiles[0].name;
			}
			font = t.activeProfiles[0].font;
			multi = false;
		}
		data = t;
		score = t.score;
	}
}
