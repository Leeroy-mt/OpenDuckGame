namespace DuckGame;

public class PlacementMenu : EditorGroupMenu
{
    private ContextMenu _noneMenu;

    private ContextMenu _searchMenu;

    public PlacementMenu(float xpos, float ypos)
        : base(null)
    {
        _alwaysDrawLast = true;
        base.X = xpos;
        base.Y = ypos;
        _root = true;
        willOnlineGrayout = false;
        _noneMenu = new ContextMenu(this);
        _noneMenu.text = "None";
        AddItem(_noneMenu);
        fancy = true;
        isPinnable = true;
        InitializeGroups(Editor.Placeables, null, null, isPinnable);
        _searchMenu = new ContextSearch(this);
        AddItem(_searchMenu);
        AddItem(new ContextToolbarItem(this));
    }

    public override void Selected(ContextMenu item)
    {
        if (item == _noneMenu && item.scrollButtonDirection == 0)
        {
            if (Level.current is Editor editor)
            {
                editor.placementType = null;
                editor.CloseMenu();
            }
        }
        else
        {
            base.Selected(item);
        }
    }

    public override void Initialize()
    {
    }
}
