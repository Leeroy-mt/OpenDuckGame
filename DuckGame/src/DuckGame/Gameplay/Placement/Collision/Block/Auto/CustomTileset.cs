namespace DuckGame;

[EditorGroup("Blocks|custom", EditorItemType.Custom)]
[BaggedProperty("isInDemo", false)]
public class CustomTileset : AutoBlock
{
    private static CustomType _customType;

    public int customIndex;

    private string _currentTileset = "";

    public static string customTileset01
    {
        get
        {
            return Custom.data[_customType][0];
        }
        set
        {
            Custom.data[_customType][0] = value;
            Custom.Clear(CustomType.Block, value);
        }
    }

    public CustomTileset(float x, float y, string tset = "CUSTOM01")
        : base(x, y, "")
    {
        _tileset = tset;
        customIndex = 0;
        _editorName = "Custom Block 01";
        physicsMaterial = PhysicsMaterial.Metal;
        verticalWidthThick = 16f;
        verticalWidth = 14f;
        horizontalHeight = 16f;
        UpdateCurrentTileset();
    }

    public void UpdateCurrentTileset()
    {
        CustomTileData tileData = Custom.GetData(customIndex, _customType);
        int fr = 0;
        if (_sprite != null)
        {
            fr = _sprite.frame;
        }
        if (tileData != null && tileData.texture != null)
        {
            _sprite = new SpriteMap(tileData.texture, 16, 16);
            horizontalHeight = tileData.horizontalHeight;
            verticalWidth = tileData.verticalWidth;
            verticalWidthThick = tileData.verticalWidthThick;
            _hasLeftNub = tileData.leftNubber;
            _hasRightNub = tileData.rightNubber;
        }
        else
        {
            _sprite = new SpriteMap("blueprintTileset", 16, 16);
            verticalWidthThick = 16f;
            verticalWidth = 14f;
            horizontalHeight = 16f;
        }
        if (horizontalHeight == 0f)
        {
            horizontalHeight = 16f;
        }
        if (verticalWidth == 0f)
        {
            verticalWidth = 14f;
        }
        if (verticalWidthThick == 0f)
        {
            verticalWidthThick = 16f;
        }
        _sprite.frame = fr;
        _tileset = "CUSTOM0" + (customIndex + 1);
        _currentTileset = Custom.data[_customType][customIndex];
        graphic = _sprite;
        UpdateNubbers();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void EditorUpdate()
    {
        if (Level.current is Editor && _currentTileset != Custom.data[_customType][customIndex])
        {
            UpdateCurrentTileset();
        }
    }

    public override void Draw()
    {
        base.Draw();
    }

    public override ContextMenu GetContextMenu()
    {
        EditorGroupMenu editorGroupMenu = new EditorGroupMenu(null, root: true);
        editorGroupMenu.AddItem(new ContextFile("style", null, new FieldBinding(this, "customTileset0" + (customIndex + 1)), ContextFileType.Block));
        return editorGroupMenu;
    }
}
