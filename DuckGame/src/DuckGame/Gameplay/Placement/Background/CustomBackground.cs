namespace DuckGame;

[EditorGroup("Background|custom", EditorItemType.Custom)]
public class CustomBackground : BackgroundTile
{
    private static CustomType _customType = CustomType.Background;

    public int customIndex;

    private string _currentTileset = "";

    public static string customBackground01
    {
        get
        {
            return Custom.data[_customType][0];
        }
        set
        {
            Custom.data[_customType][0] = value;
            Custom.Clear(CustomType.Background, value);
        }
    }

    public CustomBackground(float xpos, float ypos)
        : base(xpos, ypos)
    {
        customIndex = 0;
        graphic = new SpriteMap("arcadeBackground", 16, 16, calculateTransparency: true);
        _opacityFromGraphic = true;
        Center = new Vec2(8f, 8f);
        collisionSize = new Vec2(16f, 16f);
        collisionOffset = new Vec2(-8f, -8f);
        _editorName = "01";
        UpdateCurrentTileset();
    }

    public void UpdateCurrentTileset()
    {
        CustomTileData tileData = Custom.GetData(customIndex, _customType);
        int fr = 0;
        if (graphic is SpriteMap)
        {
            fr = _frame;
        }
        if (tileData != null && tileData.texture != null)
        {
            graphic = new SpriteMap(tileData.texture, 16, 16);
        }
        else
        {
            graphic = new SpriteMap("blueprintTileset", 16, 16);
        }
        (graphic as SpriteMap).frame = fr;
        _currentTileset = Custom.data[_customType][customIndex];
    }

    public override void Draw()
    {
        if (Level.current is Editor && _currentTileset != Custom.data[_customType][customIndex])
        {
            UpdateCurrentTileset();
        }
        base.Draw();
    }
}
