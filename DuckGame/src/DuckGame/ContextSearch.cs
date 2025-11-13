namespace DuckGame;

public class ContextSearch : ContextMenu
{
	private bool _searching;

	public ContextSearch(IContextListener owner)
		: base(owner)
	{
		itemSize.y = 16f;
		_text = "@searchicon@ search...";
		tooltip = "Search for an object!";
	}

	public override void Selected()
	{
		(Level.current as Editor).searching = true;
	}

	public override void Draw()
	{
		if (_hover && !greyOut)
		{
			Graphics.DrawRect(position, position + itemSize, new Color(70, 70, 70), base.depth + 1);
		}
		Color c = Color.White;
		if (greyOut)
		{
			c = Color.White * 0.3f;
		}
		if (base.hover)
		{
			_text = "@searchiconwhite@ search...";
		}
		else
		{
			_text = "@searchicon@ |GRAY|search...";
		}
		Graphics.DrawFancyString(_text, position + new Vec2(0f, 4f), c, base.depth + 2);
	}
}
