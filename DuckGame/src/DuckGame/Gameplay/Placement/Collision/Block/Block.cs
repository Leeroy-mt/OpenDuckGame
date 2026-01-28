using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class Block : MaterialThing, IPlatform
{
    public bool shouldWreck;

    public bool skipWreck;

    protected bool _groupedWithNeighbors;

    public BlockGroup group;

    protected bool _neighborsInitialized;

    protected Block _leftBlock;

    protected Block _rightBlock;

    protected Block _upBlock;

    protected Block _downBlock;

    private BlockStructure _structure;

    private List<ConcaveLine> _concaveLines;

    private bool _hit;

    private bool _pathed;

    private ConcaveLine nullLine = new ConcaveLine(Vec2.Zero, Vec2.Zero);

    private List<ConcaveLine> G1;

    private List<ConcaveLine> G2;

    private Dictionary<ConcaveLine, ConcaveLine> pairG1val = new Dictionary<ConcaveLine, ConcaveLine>();

    private Dictionary<ConcaveLine, ConcaveLine> pairG2val = new Dictionary<ConcaveLine, ConcaveLine>();

    private List<KeyValuePair<ConcaveLine, ConcaveLine>> matchingSet = new List<KeyValuePair<ConcaveLine, ConcaveLine>>();

    private List<ConcaveLine> minimumVertex = new List<ConcaveLine>();

    private int _initializeWait = 20;

    public bool groupedWithNeighbors
    {
        get
        {
            return _groupedWithNeighbors;
        }
        set
        {
            _groupedWithNeighbors = value;
        }
    }

    public bool neighborsInitialized
    {
        get
        {
            return _neighborsInitialized;
        }
        set
        {
            _neighborsInitialized = value;
        }
    }

    public Block leftBlock
    {
        get
        {
            return _leftBlock;
        }
        set
        {
            _leftBlock = value;
        }
    }

    public Block rightBlock
    {
        get
        {
            return _rightBlock;
        }
        set
        {
            _rightBlock = value;
        }
    }

    public Block upBlock
    {
        get
        {
            return _upBlock;
        }
        set
        {
            _upBlock = value;
        }
    }

    public Block downBlock
    {
        get
        {
            return _downBlock;
        }
        set
        {
            _downBlock = value;
        }
    }

    public BlockStructure structure
    {
        get
        {
            return _structure;
        }
        set
        {
            _structure = value;
        }
    }

    public bool pathed
    {
        get
        {
            return _pathed;
        }
        set
        {
            _pathed = value;
        }
    }

    public bool BFS(Dictionary<ConcaveLine, int> dist, Dictionary<ConcaveLine, ConcaveLine> pairG1, Dictionary<ConcaveLine, ConcaveLine> pairG2)
    {
        Queue<ConcaveLine> q = new Queue<ConcaveLine>();
        using (List<ConcaveLine>.Enumerator enumerator = G1.GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                ConcaveLine v = enumerator.Current;
                if (pairG1[v] == nullLine)
                {
                    dist[v] = 0;
                    q.Enqueue(v);
                }
                else
                {
                    dist[v] = int.MaxValue;
                }
            }
        }
        dist[nullLine] = int.MaxValue;
        while (q.Count > 0)
        {
            ConcaveLine v2 = q.Dequeue();
            if (dist[v2] >= dist[nullLine])
            {
                continue;
            }
            foreach (ConcaveLine u in v2.intersects)
            {
                if (dist[pairG2[u]] == int.MaxValue)
                {
                    dist[pairG2[u]] = dist[v2] + 1;
                    q.Enqueue(pairG2[u]);
                }
            }
        }
        return dist[nullLine] != int.MaxValue;
    }

    public bool DFS(Dictionary<ConcaveLine, int> dist, Dictionary<ConcaveLine, ConcaveLine> pairG1, Dictionary<ConcaveLine, ConcaveLine> pairG2, ConcaveLine v)
    {
        if (v != nullLine)
        {
            foreach (ConcaveLine u in v.intersects)
            {
                if (dist[pairG2[u]] == dist[v] + 1 && DFS(dist, pairG1, pairG2, pairG2[u]))
                {
                    pairG2[u] = v;
                    pairG1[v] = u;
                    return true;
                }
            }
            dist[v] = int.MaxValue;
            return false;
        }
        return true;
    }

    public void Calculate(List<ConcaveLine> G1val, List<ConcaveLine> G2val)
    {
        G1 = G1val;
        G2 = G2val;
        Dictionary<ConcaveLine, int> dist = new Dictionary<ConcaveLine, int>();
        foreach (ConcaveLine v in G1)
        {
            dist[v] = 0;
            pairG1val[v] = nullLine;
        }
        foreach (ConcaveLine v2 in G2)
        {
            dist[v2] = 0;
            pairG2val[v2] = nullLine;
        }
        int matching = 0;
        while (BFS(dist, pairG1val, pairG2val))
        {
            foreach (ConcaveLine v3 in G1)
            {
                if (pairG1val[v3] == nullLine && DFS(dist, pairG1val, pairG2val, v3))
                {
                    matching++;
                    matchingSet.Add(new KeyValuePair<ConcaveLine, ConcaveLine>(v3, pairG1val[v3]));
                }
            }
        }
        List<ConcaveLine> minimumVertexCover = new List<ConcaveLine>();
        minimumVertexCover.AddRange(_concaveLines);
        foreach (KeyValuePair<ConcaveLine, ConcaveLine> pair in matchingSet)
        {
            minimumVertexCover.Remove(pair.Key);
            minimumVertexCover.Remove(pair.Value);
        }
    }

    public BlockCorner GetNearestCorner(Vec2 to)
    {
        List<BlockCorner> groupCorners = GetGroupCorners();
        float near = 9999999f;
        float dist = 0f;
        BlockCorner nearCorner = null;
        foreach (BlockCorner corner in groupCorners)
        {
            dist = (corner.corner - to).Length();
            if (dist < near)
            {
                near = dist;
                nearCorner = corner;
            }
        }
        return nearCorner;
    }

    public void CalculateConcaveLines()
    {
        _concaveLines = new List<ConcaveLine>();
        List<ConcaveLine> horizontalLines = new List<ConcaveLine>();
        List<ConcaveLine> verticalLines = new List<ConcaveLine>();
        List<BlockCorner> innerCorners = _structure.corners.Where((BlockCorner v) => v.wallCorner).ToList();
        foreach (BlockCorner c1 in innerCorners)
        {
            foreach (BlockCorner c2 in innerCorners)
            {
                if (c1 == c2 || c1.testedCorners.Contains(c2) || c2.testedCorners.Contains(c1))
                {
                    continue;
                }
                c1.testedCorners.Add(c2);
                c2.testedCorners.Add(c1);
                if (c1.corner.X == c2.corner.X)
                {
                    int off = 8;
                    if (c1.corner.Y > c2.corner.Y)
                    {
                        off = -8;
                    }
                    if (Level.CheckPoint<AutoBlock>(new Vec2(c1.corner.X + 8f, c1.corner.Y + (float)off)) != null && Level.CheckPoint<AutoBlock>(new Vec2(c1.corner.X - 8f, c1.corner.Y + (float)off)) != null && Level.CheckPoint<AutoBlock>(new Vec2(c2.corner.X + 8f, c2.corner.Y - (float)off)) != null && Level.CheckPoint<AutoBlock>(new Vec2(c2.corner.X - 8f, c2.corner.Y - (float)off)) != null)
                    {
                        verticalLines.Add(new ConcaveLine(c1.corner, c2.corner));
                    }
                }
                else if (c1.corner.Y == c2.corner.Y)
                {
                    int off2 = 8;
                    if (c1.corner.X > c2.corner.X)
                    {
                        off2 = -8;
                    }
                    if (Level.CheckPoint<AutoBlock>(new Vec2(c1.corner.X + (float)off2, c1.corner.Y - 8f)) != null && Level.CheckPoint<AutoBlock>(new Vec2(c1.corner.X + (float)off2, c1.corner.Y + 8f)) != null && Level.CheckPoint<AutoBlock>(new Vec2(c2.corner.X - (float)off2, c2.corner.Y - 8f)) != null && Level.CheckPoint<AutoBlock>(new Vec2(c2.corner.X - (float)off2, c2.corner.Y + 8f)) != null)
                    {
                        horizontalLines.Add(new ConcaveLine(c1.corner, c2.corner));
                    }
                }
            }
        }
        foreach (ConcaveLine line1 in horizontalLines)
        {
            foreach (ConcaveLine line2 in verticalLines)
            {
                if (Collision.LineIntersect(line1.p1, line1.p2, line2.p1, line2.p2))
                {
                    line1.intersects.Add(line2);
                    line2.intersects.Add(line1);
                }
            }
        }
        if (verticalLines.Count == 4 && horizontalLines.Count == 4)
        {
            ConcaveLine l = verticalLines[2];
            verticalLines[2] = verticalLines[3];
            verticalLines[3] = l;
            l = horizontalLines[0];
            horizontalLines[0] = horizontalLines[1];
            horizontalLines[1] = l;
            l = horizontalLines[1];
            horizontalLines[1] = horizontalLines[2];
            horizontalLines[2] = l;
            _concaveLines.Add(verticalLines[0]);
            _concaveLines.Add(verticalLines[1]);
            _concaveLines.Add(horizontalLines[1]);
            _concaveLines.Add(horizontalLines[0]);
            _concaveLines.Add(horizontalLines[2]);
            _concaveLines.Add(verticalLines[2]);
            _concaveLines.Add(verticalLines[3]);
            _concaveLines.Add(horizontalLines[3]);
            int index = 1;
            foreach (ConcaveLine concaveLine in _concaveLines)
            {
                concaveLine.index = index;
                index++;
            }
        }
        else
        {
            _concaveLines.AddRange(verticalLines);
            _concaveLines.AddRange(horizontalLines);
        }
        Calculate(verticalLines, horizontalLines);
    }

    public List<ConcaveLine> GetConcaveLines()
    {
        if (_concaveLines == null)
        {
            CalculateConcaveLines();
        }
        return _concaveLines;
    }

    public virtual List<BlockCorner> GetGroupCorners()
    {
        if (_structure == null)
        {
            _structure = new BlockStructure();
            Stack<Block> stack = new Stack<Block>();
            stack.Push(this);
            _hit = true;
            while (stack.Count > 0)
            {
                Block block = stack.Pop();
                block._structure = _structure;
                _structure.blocks.Add(block);
                if (block.leftBlock == null)
                {
                    if (block.upBlock == null)
                    {
                        _structure.corners.Add(new BlockCorner(block.topLeft, block));
                    }
                    if (block.downBlock == null)
                    {
                        _structure.corners.Add(new BlockCorner(block.bottomLeft, block));
                    }
                }
                else if (!block.leftBlock._hit)
                {
                    block.leftBlock._hit = true;
                    stack.Push(block.leftBlock);
                }
                if (block.rightBlock == null)
                {
                    if (block.upBlock == null)
                    {
                        _structure.corners.Add(new BlockCorner(block.topRight, block));
                    }
                    if (block.downBlock == null)
                    {
                        _structure.corners.Add(new BlockCorner(block.bottomRight, block));
                    }
                }
                else if (!block.rightBlock._hit)
                {
                    block.rightBlock._hit = true;
                    stack.Push(block.rightBlock);
                }
                if (block.upBlock != null && !block.upBlock._hit)
                {
                    block.upBlock._hit = true;
                    stack.Push(block.upBlock);
                }
                if (block.downBlock != null && !block.downBlock._hit)
                {
                    block.downBlock._hit = true;
                    stack.Push(block.downBlock);
                }
                if (block.upBlock != null && block.leftBlock != null && block.upBlock.leftBlock == null)
                {
                    _structure.corners.Add(new BlockCorner(block.upBlock.bottomLeft, block, wall: true));
                }
                if (block.upBlock != null && block.rightBlock != null && block.upBlock.rightBlock == null)
                {
                    _structure.corners.Add(new BlockCorner(block.upBlock.bottomRight, block, wall: true));
                }
                if (block.downBlock != null && block.leftBlock != null && block.downBlock.leftBlock == null)
                {
                    _structure.corners.Add(new BlockCorner(block.downBlock.topLeft, block, wall: true));
                }
                if (block.downBlock != null && block.rightBlock != null && block.downBlock.rightBlock == null)
                {
                    _structure.corners.Add(new BlockCorner(block.downBlock.topRight, block, wall: true));
                }
            }
            return _structure.corners;
        }
        return _structure.corners;
    }

    public Block(float x, float y)
        : base(x, y)
    {
        collisionSize = new Vec2(16f, 16f);
        thickness = 10f;
    }

    public Block(float x, float y, float wid, float hi, PhysicsMaterial mat = PhysicsMaterial.Default)
        : base(x, y)
    {
        collisionSize = new Vec2(wid, hi);
        thickness = 10f;
        physicsMaterial = mat;
    }

    public override void Update()
    {
        InitializeNeighbors();
        _hit = false;
    }

    public override void DoInitialize()
    {
        base.DoInitialize();
    }

    public virtual void InitializeNeighbors()
    {
    }

    public override void Draw()
    {
        base.Draw();
    }
}
