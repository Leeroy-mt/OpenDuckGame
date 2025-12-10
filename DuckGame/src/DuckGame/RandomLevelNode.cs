using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class RandomLevelNode
{
    private const float _width = 192f;

    private const float _height = 144f;

    public int gridX;

    public int gridY;

    public RandomLevelNode[,] map;

    public int seed;

    public RandomLevelNode up;

    public RandomLevelNode down;

    public RandomLevelNode left;

    public RandomLevelNode right;

    public bool isCentered;

    public bool symmetric;

    public bool mirror;

    public RandomLevelNode symmetricalPartner;

    private RandomLevelData _combinedData;

    public RandomLevelNode[,] tiles;

    public int tilesWide;

    public int tilesHigh;

    public static bool editorLoad = false;

    public static bool processing = false;

    public List<RandomLevelData.PreparedThing> _preparedThings;

    public static HashSet<Thing> _allPreparedThings;

    public static Vec2 topLeft = new Vec2(0f, 0f);

    public static bool firstGetRequiresMultiplePaths = false;

    public bool kingTile;

    private bool leftSymmetric;

    private bool rightSymmetric;

    private bool connectionUp;

    private bool connectionDown;

    private bool connectionLeft;

    private bool connectionRight;

    private bool removeLeft;

    private bool removeRight;

    public RandomLevelData data;

    public bool visited;

    public List<RandomLevelNode> nodes => new List<RandomLevelNode> { up, down, left, right };

    public RandomLevelData totalData
    {
        get
        {
            _combinedData = Combine();
            ClearFlags();
            return _combinedData;
        }
    }

    public RandomLevelNode(int pX, int pY)
    {
        gridX = pX;
        gridY = pY;
    }

    private RandomLevelData Combine()
    {
        visited = true;
        RandomLevelData dat = new RandomLevelData();
        if (left != null && left.data != null && !left.visited)
        {
            dat = left.data.Combine(dat);
        }
        if (right != null && right.data != null && !right.visited)
        {
            dat = right.data.Combine(dat);
        }
        if (up != null && up.data != null && !up.visited)
        {
            dat = up.data.Combine(dat);
        }
        if (down != null && down.data != null && !down.visited)
        {
            dat = down.data.Combine(dat);
        }
        return dat.Combine(data);
    }

    public void ClearFlags()
    {
        visited = false;
        if (up != null && up.visited)
        {
            up.ClearFlags();
        }
        if (down != null && down.visited)
        {
            down.ClearFlags();
        }
        if (left != null && left.visited)
        {
            left.ClearFlags();
        }
        if (right != null && right.visited)
        {
            right.ClearFlags();
        }
    }

    public bool LoadParts(float x, float y, Level level, int seed = 0)
    {
        Random generator = Rando.generator;
        if (seed != 0)
        {
            Rando.generator = new Random(seed);
        }
        Level.InitChanceGroups();
        processing = true;
        topLeft = new Vec2(-gridX * 192, -gridY * 144);
        _allPreparedThings = new HashSet<Thing>();
        PreparePartsRecurse(x, y, level);
        processing = false;
        ClearFlags();
        if (null == null)
        {
            new List<NGeneratorRule>
            {
                new NGeneratorRule(delegate
                {
                    bool result = false;
                    for (int i = 0; i < 4; i++)
                    {
                        if (NGeneratorRule.Count(_allPreparedThings, delegate(Thing thing)
                        {
                            if (thing is IContainAThing)
                            {
                                Type contains = (thing as IContainAThing).contains;
                                if (contains != null)
                                {
                                    Thing thing2 = Editor.GetThing(contains);
                                    if (thing2 is Gun)
                                    {
                                        return (thing2 as Gun).isFatal;
                                    }
                                    return false;
                                }
                            }
                            return false;
                        }) != 0)
                        {
                            result = true;
                            break;
                        }
                        foreach (Thing current in from thing in _allPreparedThings.AsEnumerable()
                            orderby Rando.Float(1f)
                            select thing)
                        {
                            if (current is IContainPossibleThings)
                            {
                                (current as IContainPossibleThings).PreparePossibilities();
                            }
                        }
                    }
                    return result;
                }),
                new NGeneratorRule(delegate
                {
                    int num = NGeneratorRule.Count(_allPreparedThings, (Thing thing) => thing is Door && (thing as Door).locked);
                    int num2 = NGeneratorRule.Count(_allPreparedThings, (Thing thing) => thing is Key);
                    if (num > 0 && num2 == 0)
                    {
                        _allPreparedThings.RemoveWhere((Thing v) => v is Door && (v as Door).locked);
                    }
                    return true;
                }),
                new NGeneratorRule(() => NGeneratorRule.Count(_allPreparedThings, (Thing thing) => thing is Warpgun) > 0)
            };
        }
        LoadPartsRecurse(x, y, level);
        ClearFlags();
        Rando.generator = generator;
        if (!LevelGenerator.openAirMode)
        {
            for (int xpos = -1; xpos < tilesWide + 1; xpos++)
            {
                for (int ypos = -1; ypos < tilesHigh + 1; ypos++)
                {
                    RandomLevelNode t = null;
                    if (xpos >= 0 && xpos < tilesWide && ypos >= 0 && ypos < tilesHigh)
                    {
                        t = tiles[xpos, ypos];
                    }
                    if (t == null || t.data == null)
                    {
                        Vec2 pyramidWallPos = new Vec2(xpos * 192 - 8, ypos * 144 - 8) + topLeft;
                        level.AddThing(new PyramidWall(pyramidWallPos.x, pyramidWallPos.y));
                    }
                }
            }
        }
        level.things.RefreshState();
        for (int xpos2 = 0; xpos2 < tilesWide; xpos2++)
        {
            for (int ypos2 = 0; ypos2 < tilesHigh; ypos2++)
            {
                RandomLevelNode t2 = null;
                if (xpos2 >= 0 && xpos2 < tilesWide && ypos2 >= 0 && ypos2 < tilesHigh)
                {
                    t2 = tiles[xpos2, ypos2];
                }
                if (t2 != null && t2.data != null)
                {
                    Vec2 vec = new Vec2(xpos2 * 192 + 96, ypos2 * 144 + 72) + topLeft;
                    PyramidWall w = Level.CheckPoint<PyramidWall>(vec + new Vec2(-192f, 0f));
                    if (w != null)
                    {
                        w.hasRight = true;
                    }
                    w = Level.CheckPoint<PyramidWall>(vec + new Vec2(192f, 0f));
                    if (w != null)
                    {
                        w.hasLeft = true;
                    }
                    w = Level.CheckPoint<PyramidWall>(vec + new Vec2(0f, -144f));
                    if (w != null)
                    {
                        w.hasDown = true;
                    }
                    w = Level.CheckPoint<PyramidWall>(vec + new Vec2(0f, 144f));
                    if (w != null)
                    {
                        w.hasUp = true;
                    }
                }
            }
        }
        foreach (PyramidWall item in level.things[typeof(PyramidWall)])
        {
            item.AddExtraWalls();
        }
        foreach (PyramidDoor d in level.things[typeof(PyramidDoor)])
        {
            Block b = level.CollisionPoint<Block>(d.position + new Vec2(-16f, 0f), d);
            if (b == null)
            {
                b = level.CollisionPoint<Block>(d.position + new Vec2(16f, 0f), d);
            }
            if (b != null && !(b is PyramidDoor) && !(b is Door))
            {
                level.RemoveThing(d);
                Level.Add(new PyramidTileset(d.x, d.y - 16f));
                Level.Add(new PyramidTileset(d.x, d.y));
                continue;
            }
            Block blog = level.CollisionPoint<Block>(d.x, d.y, d);
            if (blog != null)
            {
                level.RemoveThing(blog);
            }
            blog = level.CollisionPoint<Block>(d.x, d.y - 16f, d);
            if (blog != null)
            {
                level.RemoveThing(blog);
            }
        }
        foreach (Door d2 in level.things[typeof(Door)])
        {
            Block b2 = level.CollisionLine<Block>(d2.position + new Vec2(-16f, 0f), d2.position + new Vec2(16f, 0f), d2);
            if (b2 != null && !(b2 is PyramidDoor) && !(b2 is Door))
            {
                level.RemoveThing(d2);
                Level.Add(new PyramidTileset(d2.x, d2.y - 16f));
                Level.Add(new PyramidTileset(d2.x, d2.y));
            }
        }
        foreach (Teleporter t3 in level.things[typeof(Teleporter)])
        {
            t3.InitLinks();
            if (t3._link == null)
            {
                if (t3.direction == 2)
                {
                    t3.direction = 3;
                }
                else if (t3.direction == 3)
                {
                    t3.direction = 2;
                }
                else if (t3.direction == 0)
                {
                    t3.direction = 1;
                }
                else if (t3.direction == 1)
                {
                    t3.direction = 0;
                }
                t3.InitLinks();
                if (t3._link == null)
                {
                    level.RemoveThing(t3);
                    Level.Add(new PyramidTileset(t3.x, t3.y - 16f));
                    Level.Add(new PyramidTileset(t3.x, t3.y));
                }
            }
        }
        if (editorLoad)
        {
            level.things.RefreshState();
            foreach (AutoBlock b3 in level.things[typeof(AutoBlock)])
            {
                b3.DoPositioning();
                if (!(b3 is BlockGroup))
                {
                    b3.FindFrame();
                }
            }
            foreach (AutoPlatform item2 in level.things[typeof(AutoPlatform)])
            {
                item2.DoPositioning();
                item2.FindFrame();
            }
        }
        LightingTwoPointOH l = new LightingTwoPointOH();
        l.visible = false;
        level.AddThing(l);
        return true;
    }

    private void PreparePartsRecurse(float x, float y, Level level)
    {
        visited = true;
        if (data != null)
        {
            _preparedThings = data.PrepareThings(mirror, x, y);
            foreach (RandomLevelData.PreparedThing t in _preparedThings)
            {
                _allPreparedThings.Add(t.thing);
            }
        }
        if (up != null && !up.visited)
        {
            up.PreparePartsRecurse(x, y - 144f, level);
        }
        if (down != null && !down.visited)
        {
            down.PreparePartsRecurse(x, y + 144f, level);
        }
        if (left != null && !left.visited)
        {
            left.PreparePartsRecurse(x - 192f, y, level);
        }
        if (right != null && !right.visited)
        {
            right.PreparePartsRecurse(x + 192f, y, level);
        }
    }

    private void LoadPartsRecurse(float x, float y, Level level)
    {
        visited = true;
        if (data != null)
        {
            data.Load(x, y, level, mirror, _preparedThings);
        }
        if (up != null && !up.visited)
        {
            up.LoadPartsRecurse(x, y - 144f, level);
        }
        if (down != null && !down.visited)
        {
            down.LoadPartsRecurse(x, y + 144f, level);
        }
        if (left != null && !left.visited)
        {
            left.LoadPartsRecurse(x - 192f, y, level);
        }
        if (right != null && !right.visited)
        {
            right.LoadPartsRecurse(x + 192f, y, level);
        }
    }

    public void GenerateTiles(RandomLevelData tile = null, LevGenType type = LevGenType.Any, bool symmetricVal = false)
    {
        firstGetRequiresMultiplePaths = false;
        TileConnection req = TileConnection.None;
        if (symmetricVal && tilesWide == 3 && gridX == 1)
        {
            req = TileConnection.Left | TileConnection.Right;
        }
        if (symmetricVal && tilesWide == 2 && gridX == 0)
        {
            req = TileConnection.Right;
        }
        else if (symmetricVal && tilesWide == 2 && gridX == 1)
        {
            req = TileConnection.Left;
        }
        if (tile == null)
        {
            tile = LevelGenerator.GetTile(req, tile, canBeNull: false, type, null, GetFilter(), requiresSpawns: true);
        }
        if (tile == null)
        {
            DevConsole.Log("|DGRED|RandomLevel.GenerateTiles had a null tile! This should never happen!");
            return;
        }
        int sides = 0;
        if (tile.left)
        {
            sides++;
        }
        if (tile.right)
        {
            sides++;
        }
        if (tile.up)
        {
            sides++;
        }
        if (tile.down)
        {
            sides++;
        }
        if (sides <= 1)
        {
            symmetricVal = false;
            firstGetRequiresMultiplePaths = true;
        }
        if (symmetricVal && !LevelGenerator.openAirMode)
        {
            if (sides == 2 && Rando.Float(1f) < 0.3f)
            {
                symmetricVal = false;
            }
            else if (sides == 1 && Rando.Float(1f) < 0.8f)
            {
                symmetricVal = false;
            }
            else if (sides == 1 && tile.right && (right == null || right.right == null))
            {
                symmetricVal = false;
            }
            else if (sides == 1 && tile.left && (left == null || left.left == null))
            {
                symmetricVal = false;
            }
        }
        symmetric = symmetricVal;
        GenerateTilesRecurse(tile, type);
        ClearFlags();
    }

    private TileConnection GetFilter()
    {
        TileConnection filter = TileConnection.None;
        if (left == null)
        {
            filter |= TileConnection.Left;
        }
        if (right == null)
        {
            filter |= TileConnection.Right;
        }
        if (up == null)
        {
            filter |= TileConnection.Up;
        }
        return filter;
    }

    private void GenerateTilesRecurse(RandomLevelData tile, LevGenType type = LevGenType.Any)
    {
        RandomLevel.currentComplexityDepth++;
        if (LevelGenerator.complexity > 0 && RandomLevel.currentComplexityDepth >= LevelGenerator.complexity)
        {
            return;
        }
        visited = true;
        if (tile == null)
        {
            return;
        }
        data = tile;
        connectionUp = data.up;
        connectionDown = data.down;
        connectionLeft = data.left;
        connectionRight = data.right;
        if (symmetric)
        {
            if (kingTile)
            {
                if ((connectionLeft && connectionRight) || (!connectionLeft && !connectionRight))
                {
                    mirror = true;
                }
                else
                {
                    if (!connectionLeft)
                    {
                        if (up != null)
                        {
                            up.left = null;
                            up.removeRight = true;
                        }
                        if (down != null)
                        {
                            down.left = null;
                            down.removeRight = true;
                        }
                        removeRight = true;
                        left = null;
                    }
                    if (!connectionRight)
                    {
                        if (up != null)
                        {
                            up.right = null;
                            up.removeLeft = true;
                        }
                        if (down != null)
                        {
                            down.right = null;
                            down.removeLeft = true;
                        }
                        removeLeft = true;
                        right = null;
                    }
                }
            }
            if (mirror)
            {
                connectionRight = data.left;
            }
            if (up != null)
            {
                up.mirror = mirror;
            }
            if (down != null)
            {
                down.mirror = mirror;
            }
        }
        List<TileConnection> dirs = new List<TileConnection>
        {
            TileConnection.Right,
            TileConnection.Left,
            TileConnection.Up,
            TileConnection.Down
        };
        if (removeLeft)
        {
            dirs.Remove(TileConnection.Left);
        }
        if (removeRight)
        {
            dirs.Remove(TileConnection.Right);
        }
        using (List<TileConnection>.Enumerator enumerator = Utils.Shuffle(dirs).GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                switch (enumerator.Current)
                {
                    case TileConnection.Up:
                        if (connectionUp && up != null && up.data == null)
                        {
                            up.leftSymmetric = leftSymmetric;
                            up.rightSymmetric = rightSymmetric;
                            up.symmetric = symmetric;
                            up.GenerateTilesRecurse(LevelGenerator.GetTile(TileConnection.Down, tile, canBeNull: true, type, null, up.GetFilter(), requiresSpawns: false, mirror), type);
                        }
                        break;
                    case TileConnection.Down:
                        if (connectionDown && down != null && down.data == null)
                        {
                            down.leftSymmetric = leftSymmetric;
                            down.rightSymmetric = rightSymmetric;
                            down.symmetric = symmetric;
                            down.GenerateTilesRecurse(LevelGenerator.GetTile(TileConnection.Up, tile, canBeNull: true, type, null, down.GetFilter(), requiresSpawns: false, mirror), type);
                        }
                        break;
                    case TileConnection.Left:
                        if (!connectionLeft || left == null || left.data != null || (mirror && symmetric && rightSymmetric))
                        {
                            break;
                        }
                        if (mirror && symmetric)
                        {
                            leftSymmetric = true;
                            if (down != null)
                            {
                                down.leftSymmetric = leftSymmetric;
                                if (down.down != null)
                                {
                                    down.down.leftSymmetric = leftSymmetric;
                                }
                            }
                            if (up != null)
                            {
                                up.leftSymmetric = leftSymmetric;
                                if (up.up != null)
                                {
                                    up.up.leftSymmetric = leftSymmetric;
                                }
                            }
                        }
                        left.leftSymmetric = leftSymmetric;
                        left.rightSymmetric = rightSymmetric;
                        left.symmetric = symmetric;
                        left.GenerateTilesRecurse(LevelGenerator.GetTile(TileConnection.Right, tile, canBeNull: true, type, null, left.GetFilter(), requiresSpawns: false, left.mirror), type);
                        break;
                    case TileConnection.Right:
                        if (!connectionRight || right == null || right.data != null || (mirror && symmetric && leftSymmetric))
                        {
                            break;
                        }
                        if (mirror && symmetric)
                        {
                            rightSymmetric = true;
                            if (down != null)
                            {
                                down.rightSymmetric = rightSymmetric;
                                if (down.down != null)
                                {
                                    down.down.rightSymmetric = rightSymmetric;
                                }
                            }
                            if (up != null)
                            {
                                up.rightSymmetric = rightSymmetric;
                                if (up.up != null)
                                {
                                    up.up.rightSymmetric = rightSymmetric;
                                }
                            }
                        }
                        right.leftSymmetric = leftSymmetric;
                        right.rightSymmetric = rightSymmetric;
                        right.symmetric = symmetric;
                        right.GenerateTilesRecurse(LevelGenerator.GetTile(TileConnection.Left, tile, canBeNull: true, type, null, right.GetFilter(), requiresSpawns: false, right.mirror), type);
                        break;
                }
            }
        }
        if (kingTile && symmetric)
        {
            SolveSymmetry();
            if (up != null)
            {
                up.SolveSymmetry();
            }
            if (down != null)
            {
                down.SolveSymmetry();
            }
        }
    }

    public void SolveSymmetry()
    {
        if (mirror)
        {
            if (leftSymmetric)
            {
                if (left != null && left.data != null && right != null)
                {
                    if (left.data.isMirrored)
                    {
                        right.data = left.data;
                    }
                    else
                    {
                        right.data = left.data.Flipped();
                    }
                    right.symmetricalPartner = left;
                    left.symmetricalPartner = right;
                    right.mirror = left.mirror;
                }
            }
            else if (right != null && right.data != null && left != null)
            {
                if (right.data.isMirrored)
                {
                    left.data = right.data;
                }
                else
                {
                    left.data = right.data.Flipped();
                }
                right.symmetricalPartner = left;
                left.symmetricalPartner = right;
                left.mirror = right.mirror;
            }
        }
        else if (data != null)
        {
            if (removeRight && right != null)
            {
                right.data = data.Flipped();
                right.symmetricalPartner = this;
                symmetricalPartner = right;
            }
            if (removeLeft && left != null)
            {
                left.data = data.Flipped();
                left.symmetricalPartner = this;
                symmetricalPartner = left;
            }
        }
    }
}
