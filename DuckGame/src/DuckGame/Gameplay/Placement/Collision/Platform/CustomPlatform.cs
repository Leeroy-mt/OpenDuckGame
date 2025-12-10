namespace DuckGame;

[EditorGroup("Blocks|custom", EditorItemType.Custom)]
[BaggedProperty("isInDemo", false)]
public class CustomPlatform : AutoPlatform
{
    private static CustomType _customType = CustomType.Platform;

    public int customIndex;

    private string _currentTileset = "";

    public static string customPlatform01
    {
        get
        {
            return Custom.data[_customType][0];
        }
        set
        {
            Custom.data[_customType][0] = value;
            Custom.Clear(CustomType.Platform, value);
        }
    }

    public CustomPlatform(float x, float y, string t = "CUSTOMPLAT01")
        : base(x, y, "")
    {
        _tileset = t;
        customIndex = 0;
        _editorName = "Custom Platform 01";
        physicsMaterial = PhysicsMaterial.Metal;
        verticalWidth = 14f;
        verticalWidthThick = 15f;
        horizontalHeight = 8f;
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
            _sprite = new SpriteMap("scaffolding", 16, 16);
            verticalWidth = 14f;
            verticalWidthThick = 15f;
            horizontalHeight = 8f;
        }
        if (horizontalHeight == 0f)
        {
            horizontalHeight = 8f;
        }
        if (verticalWidth == 0f)
        {
            verticalWidth = 14f;
        }
        if (verticalWidthThick == 0f)
        {
            verticalWidthThick = 15f;
        }
        _sprite.frame = fr;
        _tileset = "CUSTOMPLAT0" + (customIndex + 1);
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

    public override ContextMenu GetContextMenu()
    {
        EditorGroupMenu editorGroupMenu = new EditorGroupMenu(null, root: true);
        editorGroupMenu.AddItem(new ContextFile("style", null, new FieldBinding(this, "customPlatform0" + (customIndex + 1)), ContextFileType.Platform));
        return editorGroupMenu;
    }
}
