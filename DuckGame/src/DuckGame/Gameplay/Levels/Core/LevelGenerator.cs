using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class LevelGenerator
{
    public const int tileWidth = 12;

    public const int tileHeight = 9;

    private static List<RandomLevelData> _tiles = new List<RandomLevelData>();

    private static MultiMap<TileConnection, RandomLevelData> _connections = new MultiMap<TileConnection, RandomLevelData>();

    private static Dictionary<string, int> _used = new Dictionary<string, int>();

    public static bool openAirMode = false;

    public static int complexity = 0;

    public static List<RandomLevelData> tiles => _tiles;

    public static List<RandomLevelData> GetTiles(TileConnection requirement, TileConnection filter, bool mirror)
    {
        if (requirement == TileConnection.None && filter == TileConnection.None)
        {
            return new List<RandomLevelData>(_tiles);
        }
        bool needsLeft = (requirement & TileConnection.Left) != 0;
        bool needsRight = (requirement & TileConnection.Right) != 0;
        bool needsUp = (requirement & TileConnection.Up) != 0;
        bool needsDown = (requirement & TileConnection.Down) != 0;
        bool filterLeft = (filter & TileConnection.Left) != 0;
        bool filterRight = (filter & TileConnection.Right) != 0;
        bool filterUp = (filter & TileConnection.Up) != 0;
        bool filterDown = (filter & TileConnection.Down) != 0;
        List<RandomLevelData> allowed = new List<RandomLevelData>();
        foreach (RandomLevelData c in _tiles)
        {
            if ((!mirror || c.isMirrored) && (c.left || !needsLeft) && (c.right || !needsRight) && (c.up || !needsUp) && (c.down || !needsDown) && !(c.left && filterLeft) && !(c.right && filterRight) && !(c.up && filterUp) && !(c.down && filterDown))
            {
                allowed.Add(c);
            }
        }
        return allowed;
    }

    public static RandomLevelData GetTile(TileConnection requirement, RandomLevelData current, bool canBeNull = true, LevGenType type = LevGenType.Any, Func<RandomLevelData, bool> lambdaReq = null, TileConnection filter = TileConnection.None, bool requiresSpawns = false, bool mirror = false, int numLinksRequired = -1)
    {
        List<RandomLevelData> toTry = GetTiles(requirement, filter, mirror);
        RandomLevelData best = new RandomLevelData();
        bool hasBest = false;
        int commonSkips = 0;
        bool needsMult = RandomLevelNode.firstGetRequiresMultiplePaths;
        RandomLevelNode.firstGetRequiresMultiplePaths = false;
        while (true)
        {
            if (toTry.Count == 0)
            {
                if (hasBest)
                {
                    return best;
                }
                return null;
            }
            RandomLevelData dat = toTry[Rando.Int(toTry.Count - 1)];
            if (dat.numSpawns <= 0 && requiresSpawns)
            {
                toTry.Remove(dat);
                continue;
            }
            if (lambdaReq != null && !lambdaReq(dat))
            {
                toTry.Remove(dat);
                continue;
            }
            if (dat.numLinkDirections == 0 || (needsMult && dat.numLinkDirections == 1))
            {
                toTry.Remove(dat);
                continue;
            }
            int used = 0;
            if (_used.TryGetValue(dat.file, out used) && used >= dat.max)
            {
                toTry.Remove(dat);
                continue;
            }
            float chance = dat.chance;
            if (dat.numLinkDirections == 1)
            {
                chance *= 0.65f;
            }
            if (toTry.Count == 1 && !canBeNull)
            {
                if (hasBest)
                {
                    return best;
                }
                dat = toTry.First();
            }
            else if (dat.chance != 1f && Rando.Float(1f) > chance)
            {
                best = dat;
                toTry.Remove(dat);
                hasBest = true;
                dat = null;
            }
            if (dat != null)
            {
                if (dat.chance != 1f || !(Rando.Float(1f) < 0.3f) || commonSkips >= 4)
                {
                    if (_used.ContainsKey(dat.file))
                    {
                        _used[dat.file]++;
                    }
                    else
                    {
                        _used[dat.file] = 1;
                    }
                    return dat;
                }
                best = dat;
                commonSkips++;
            }
            else if (toTry.Count == 0)
            {
                break;
            }
        }
        if (hasBest)
        {
            return best;
        }
        return null;
    }

    public static void ReInitialize()
    {
        _tiles.Clear();
        _connections.Clear();
        Content.ReloadLevels("pyramid");
        Initialize();
    }

    public static RandomLevelData LoadInTile(string tile, string realName = null)
    {
        RandomLevelData dat = new RandomLevelData();
        dat.file = tile;
        if (realName != null)
        {
            dat.file = realName;
        }
        LevelData doc = Content.GetLevel(tile);
        if (doc == null)
        {
            doc = DuckFile.LoadLevel(tile);
        }
        int mask = doc.proceduralData.sideMask;
        if (mask != 0)
        {
            if ((mask & 1) != 0)
            {
                dat.up = true;
            }
            if ((mask & 2) != 0)
            {
                dat.right = true;
            }
            if ((mask & 4) != 0)
            {
                dat.down = true;
            }
            if ((mask & 8) != 0)
            {
                dat.left = true;
            }
        }
        dat.chance = doc.proceduralData.chance;
        dat.max = doc.proceduralData.maxPerLevel;
        dat.single = doc.proceduralData.enableSingle;
        dat.multi = doc.proceduralData.enableMulti;
        dat.ApplyWeaponData(doc.proceduralData.weaponConfig);
        dat.ApplySpawnerData(doc.proceduralData.spawnerConfig);
        dat.numArmor = doc.proceduralData.numArmor;
        dat.numEquipment = doc.proceduralData.numEquipment;
        dat.numKeys = doc.proceduralData.numKeys;
        dat.numLockedDoors = doc.proceduralData.numLockedDoors;
        dat.numSpawns = doc.proceduralData.numSpawns;
        dat.numTeamSpawns = doc.proceduralData.numTeamSpawns;
        dat.canMirror = doc.proceduralData.canMirror;
        dat.isMirrored = doc.proceduralData.isMirrored;
        dat.data = doc.objects.objects;
        dat.alternateData = doc.proceduralData.openAirAlternateObjects.objects;
        _tiles.Add(dat);
        if (dat.up)
        {
            _connections.Add(TileConnection.Up, dat);
        }
        if (dat.down)
        {
            _connections.Add(TileConnection.Down, dat);
        }
        if (dat.left)
        {
            _connections.Add(TileConnection.Left, dat);
            _connections.Add(TileConnection.Right, dat.Flipped());
        }
        if (dat.right)
        {
            _connections.Add(TileConnection.Right, dat);
            _connections.Add(TileConnection.Left, dat.Flipped());
        }
        _tiles.Add(dat.Flipped());
        if (dat.canMirror)
        {
            _tiles.Add(dat.Symmetric());
        }
        return dat;
    }

    public static void Initialize()
    {
        foreach (string level in Content.GetLevels("pyramid", LevelLocation.Content))
        {
            LoadInTile(level);
        }
    }

    public static RandomLevelNode MakeLevel(RandomLevelData tile = null, bool allowSymmetry = true, int seed = 0, LevGenType type = LevGenType.Any, int varwide = 0, int varhigh = 0, int genX = 1, int genY = 1)
    {
        Random oldGen = Rando.generator;
        if (seed == 0)
        {
            seed = Rando.Int(2147483646);
        }
        Rando.generator = new Random(seed);
        varwide = 0;
        varhigh = 0;
        openAirMode = Rando.Float(1f) > 0.75f;
        bool forceSize = false;
        _used.Clear();
        int wide = varwide;
        int high = varhigh;
        if (varwide == 0)
        {
            wide = ((!(Rando.Float(1f) > 0.6f)) ? 3 : 2);
        }
        if (varhigh == 0)
        {
            float num = Rando.Float(1f);
            if (num > 0.85f && wide == 3)
            {
                high = 1;
            }
            high = ((!(num > 0.45f)) ? 3 : 2);
        }
        if (forceSize)
        {
            wide = (high = 3);
        }
        genX = Rando.Int(wide - 2);
        genY = Rando.Int(high - 2);
        if (Rando.Float(1f) > 0.75f)
        {
            genX++;
        }
        if (Rando.Float(1f) > 0.75f)
        {
            genY++;
        }
        bool symm = true;
        if (symm)
        {
            switch (wide)
            {
                case 3:
                    genX = 1;
                    break;
                case 2:
                    genX = 0;
                    break;
            }
            switch (high)
            {
                case 3:
                    genY = 1;
                    break;
                case 2:
                    genY = 0;
                    break;
            }
        }
        else
        {
            if (wide == 3 && Rando.Float(1f) < 0.5f)
            {
                genX = 1;
            }
            if (high == 3 && Rando.Float(1f) < 0.5f)
            {
                genY = 1;
            }
        }
        if (wide == 1)
        {
            wide = ((Rando.Float(1f) > 0.5f) ? 2 : 3);
            genX = 0;
            genY = 1;
        }
        RandomLevelNode[,] tiles = new RandomLevelNode[wide, high];
        for (int xpos = 0; xpos < wide; xpos++)
        {
            for (int ypos = 0; ypos < high; ypos++)
            {
                tiles[xpos, ypos] = new RandomLevelNode(xpos, ypos);
                tiles[xpos, ypos].map = tiles;
            }
        }
        for (int i = 0; i < wide; i++)
        {
            for (int j = 0; j < high; j++)
            {
                RandomLevelNode t = tiles[i, j];
                if (i > 0)
                {
                    t.left = tiles[i - 1, j];
                }
                if (i < wide - 1)
                {
                    t.right = tiles[i + 1, j];
                }
                if (j > 0)
                {
                    t.up = tiles[i, j - 1];
                }
                if (j < high - 1)
                {
                    t.down = tiles[i, j + 1];
                }
            }
        }
        if (tile != null)
        {
            _used[tile.file] = 1;
        }
        RandomLevel.currentComplexityDepth = 0;
        tiles[genX, genY].tilesWide = wide;
        tiles[genX, genY].tilesHigh = high;
        tiles[genX, genY].kingTile = true;
        tiles[genX, genY].GenerateTiles(tile, type, symm);
        List<RandomLevelNode> availableNodes = new List<RandomLevelNode>();
        for (int k = 0; k < wide; k++)
        {
            for (int l = 0; l < high; l++)
            {
                RandomLevelNode t2 = tiles[k, l];
                if (t2.data != null)
                {
                    availableNodes.Add(t2);
                }
            }
        }
        Rando.generator = oldGen;
        tiles[genX, genY].seed = seed;
        tiles[genX, genY].tiles = tiles;
        return tiles[genX, genY];
    }
}
