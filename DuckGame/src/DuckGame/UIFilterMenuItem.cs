namespace DuckGame;

public class UIFilterMenuItem : UIMenuItem
{
	public UIFilterMenuItem(UIMenuAction action = null, UIAlign al = UIAlign.Center, Color c = default(Color), bool backButton = false)
		: base("AAAAAAAAAAAAAAAAAA", action, al, c, backButton)
	{
	}

	public override void Update()
	{
		string prevText = _textElement.text;
		int filters = 0;
		foreach (MatchSetting matchSetting in TeamSelect2.matchSettings)
		{
			if (matchSetting.filtered)
			{
				filters++;
			}
		}
		foreach (UnlockData dat in Unlocks.GetUnlocks(UnlockType.Any))
		{
			if (dat.enabled && dat.filtered)
			{
				filters++;
			}
		}
		if (filters == 0)
		{
			_textElement.text = "|DGBLUE|NO FILTERS";
		}
		else
		{
			_textElement.text = "|DGYELLOW|FILTERS: " + filters;
		}
		if (_textElement.text != prevText)
		{
			_textElement.Resize();
			_dirty = true;
			base.rightSection.Resize();
		}
		base.Update();
	}
}
