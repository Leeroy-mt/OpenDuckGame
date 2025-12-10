using System.Collections.Generic;

namespace DuckGame;

public class LevelCustomData : BinaryClassChunk
{
    public List<string> scriptPackages = new List<string>();

    public CustomTileDataChunk customTileset01Data => GetChunk<CustomTileDataChunk>("customTileset01Data");

    public CustomTileDataChunk customTileset02Data => GetChunk<CustomTileDataChunk>("customTileset02Data");

    public CustomTileDataChunk customTileset03Data => GetChunk<CustomTileDataChunk>("customTileset03Data");

    public CustomTileDataChunk customBackground01Data => GetChunk<CustomTileDataChunk>("customBackground01Data");

    public CustomTileDataChunk customBackground02Data => GetChunk<CustomTileDataChunk>("customBackground02Data");

    public CustomTileDataChunk customBackground03Data => GetChunk<CustomTileDataChunk>("customBackground03Data");

    public CustomTileDataChunk customPlatform01Data => GetChunk<CustomTileDataChunk>("customPlatform01Data");

    public CustomTileDataChunk customPlatform02Data => GetChunk<CustomTileDataChunk>("customPlatform02Data");

    public CustomTileDataChunk customPlatform03Data => GetChunk<CustomTileDataChunk>("customPlatform03Data");

    public CustomTileDataChunk customParallaxData => GetChunk<CustomTileDataChunk>("customParallaxData");
}
