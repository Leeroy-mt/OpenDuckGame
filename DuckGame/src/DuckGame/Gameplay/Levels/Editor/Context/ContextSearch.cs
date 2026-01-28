namespace DuckGame;

public class ContextSearch : ContextMenu
{
    private bool _searching;

    public ContextSearch(IContextListener owner)
        : base(owner)
    {
        itemSize.Y = 16f;
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
            Graphics.DrawRect(Position, Position + itemSize, new Color(70, 70, 70), base.Depth + 1);
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
        Graphics.DrawFancyString(_text, Position + new Vec2(0f, 4f), c, base.Depth + 2);
    }
}
