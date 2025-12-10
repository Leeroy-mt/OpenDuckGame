namespace DuckGame;

public class CustomTileDataChunk : BinaryClassChunk
{
    public string path;

    public string textureData;

    public int verticalWidthThick;

    public int verticalWidth;

    public int horizontalHeight;

    public bool leftNubber;

    public bool rightNubber;

    public uint textureChecksum;

    public CustomTileData GetTileData()
    {
        CustomTileData dat = new CustomTileData();
        if (textureData == null)
        {
            return dat;
        }
        dat.path = path;
        dat.texture = Editor.StringToTexture(textureData);
        dat.verticalWidthThick = verticalWidthThick;
        dat.verticalWidth = verticalWidth;
        dat.horizontalHeight = horizontalHeight;
        dat.leftNubber = leftNubber;
        dat.rightNubber = rightNubber;
        dat.checksum = textureChecksum;
        return dat;
    }
}
