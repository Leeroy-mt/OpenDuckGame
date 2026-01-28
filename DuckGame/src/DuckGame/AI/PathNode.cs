using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class PathNode : Thing, IPathNodeBlocker
{
    private Thing _thing;

    private new bool _initialized;

    public float cost;

    public float heuristic;

    public bool fallLeft;

    public bool fallRight;

    private PathNode _parent;

    private List<PathNodeLink> _links = new List<PathNodeLink>();

    private Dictionary<PathNode, AIPath> _paths = new Dictionary<PathNode, AIPath>();

    public bool specialNode;

    private bool _fallInit;

    public Thing thing => _thing;

    public PathNode parent
    {
        get
        {
            return _parent;
        }
        set
        {
            _parent = value;
        }
    }

    public List<PathNodeLink> links => _links;

    public PathNode(float xpos = 0f, float ypos = 0f, Thing t = null)
        : base(xpos, ypos)
    {
        _thing = t;
        graphic = new Sprite("ball");
        Center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-8f, -8f);
        collisionSize = new Vec2(16f, 16f);
        base.Scale = new Vec2(0.5f, 0.5f);
        base.editorOffset = new Vec2(0f, -8f);
    }

    public override void Update()
    {
        if (!_fallInit)
        {
            InitializeLinks();
            _fallInit = true;
        }
        else if (!_initialized)
        {
            InitializePaths();
            _initialized = true;
        }
    }

    public AIPath GetPath(PathNode to)
    {
        if (_paths.ContainsKey(to))
        {
            return _paths[to];
        }
        return null;
    }

    public PathNodeLink GetLink(PathNode with)
    {
        return _links.FirstOrDefault((PathNodeLink node) => node.link == with);
    }

    public void UninitializeLinks()
    {
        _initialized = false;
        _links.Clear();
    }

    public static bool LineIsClear(Vec2 from, Vec2 to, Thing ignore = null)
    {
        IEnumerable<IPathNodeBlocker> blockers = Level.current.CollisionLineAll<IPathNodeBlocker>(from, to);
        if (to.Y - from.Y < -64f)
        {
            return false;
        }
        bool blocked = false;
        foreach (IPathNodeBlocker blocker in blockers)
        {
            if (!(blocker is PathNode) && blocker != ignore)
            {
                Thing t = blocker as Thing;
                if (t is Block || !(t is IPlatform))
                {
                    blocked = true;
                    break;
                }
                if ((!(t is AutoPlatform) || !(t as AutoPlatform).HasNoCollision()) && !(t is AutoPlatform) && t.Y > from.Y && to.Y > t.Y)
                {
                    blocked = true;
                    break;
                }
            }
        }
        return !blocked;
    }

    public void InitializePaths()
    {
        _paths.Clear();
        foreach (PathNode node in Level.current.things[typeof(PathNode)])
        {
            List<PathNode> path = AI.GetPath(this, node);
            if (path != null)
            {
                float dist = 0f;
                Vec2 prevPos = Vec2.Zero;
                foreach (PathNode n in path)
                {
                    if (prevPos != Vec2.Zero)
                    {
                        dist += (n.Position - prevPos).Length();
                    }
                    prevPos = n.Position;
                }
                AIPath aiPath = new AIPath();
                aiPath.length = dist;
                aiPath.nodes = path;
                _paths[node] = aiPath;
            }
            else
            {
                _paths[node] = null;
            }
        }
    }

    public static bool CheckTraversalLimits(Vec2 from, Vec2 to)
    {
        if (from.Y - to.Y > 64f || Math.Abs(from.X - to.X) > 128f || (from.Y - to.Y > 8f && Math.Abs(from.X - to.X) > 64f))
        {
            return false;
        }
        return true;
    }

    public static bool CanTraverse(Vec2 from, Vec2 to, Thing ignore)
    {
        if (!CheckTraversalLimits(from, to))
        {
            return false;
        }
        if (PathPhysicallyBlocked(from, to, ignore))
        {
            return false;
        }
        return true;
    }

    public bool PathBlocked(PathNode to)
    {
        IEnumerable<IPathNodeBlocker> enumerable = Level.current.CollisionLineAll<IPathNodeBlocker>(Position, to.Position);
        bool blocked = false;
        foreach (IPathNodeBlocker blocker in enumerable)
        {
            if (blocker != this && blocker != to)
            {
                Thing t = blocker as Thing;
                if (t is Block || !(t is IPlatform))
                {
                    blocked = true;
                    break;
                }
                if ((!(t is AutoPlatform) || !(t as AutoPlatform).HasNoCollision()) && t.Y > base.Y)
                {
                    blocked = true;
                    break;
                }
            }
        }
        return blocked;
    }

    public static bool PathPhysicallyBlocked(Vec2 from, Vec2 to, Thing ignore = null)
    {
        IEnumerable<IPathNodeBlocker> enumerable = Level.current.CollisionLineAll<IPathNodeBlocker>(from, to);
        bool blocked = false;
        foreach (IPathNodeBlocker blocker in enumerable)
        {
            if (!(blocker is PathNode) && blocker != ignore)
            {
                Thing t = blocker as Thing;
                if (t is Block || !(t is IPlatform))
                {
                    blocked = true;
                    break;
                }
                if ((!(t is AutoPlatform) || !(t as AutoPlatform).HasNoCollision()) && t.Y > from.Y)
                {
                    blocked = true;
                    break;
                }
            }
        }
        return blocked;
    }

    public void InitializeLinks()
    {
        foreach (PathNode node in Level.current.things[typeof(PathNode)])
        {
            if (node == this || !CheckTraversalLimits(Position, node.Position) || PathBlocked(node))
            {
                continue;
            }
            PathNodeLink link = new PathNodeLink();
            link.owner = this;
            link.link = node;
            link.distance = (node.Position - Position).Length();
            if (Math.Abs(base.Y - node.Y) < 8f)
            {
                Vec2 vec = (Position + node.Position) / 2f;
                if (Level.CheckLine<IPathNodeBlocker>(vec, vec + new Vec2(0f, 18f)) == null)
                {
                    link.gap = true;
                }
            }
            PathNodeLink otherLink = node.GetLink(this);
            if (otherLink != null)
            {
                otherLink.oneWay = false;
                link.oneWay = false;
            }
            else
            {
                link.oneWay = true;
            }
            _links.Add(link);
        }
    }

    public void Reset()
    {
        cost = 0f;
        heuristic = 0f;
        _parent = null;
    }

    public static float CalculateCost(PathNode who, PathNode parent)
    {
        float dx = who.X - parent.X;
        float dy = who.Y - parent.Y;
        if (parent.Y > who.Y && Math.Abs(dy) > 48f)
        {
            dy *= 100f;
        }
        dy *= 2f;
        return parent.cost + (dx * dx + dy * dy);
    }

    public static float CalculateHeuristic(PathNode who, PathNode end)
    {
        float num = who.X - end.X;
        float dy = who.Y - end.Y;
        return Math.Abs(num + dy);
    }

    public static void CalculateNode(PathNode who, PathNode parent, PathNode end)
    {
        who.cost = CalculateCost(who, parent);
        who.heuristic = CalculateHeuristic(who, end);
    }

    public override void Draw()
    {
        foreach (PathNodeLink l in _links)
        {
            Color c = Color.LimeGreen;
            if (l.oneWay)
            {
                c = Color.Red;
            }
            if (l.gap)
            {
                c = Color.Blue;
            }
            if (l.oneWay && l.gap)
            {
                c = Color.LightBlue;
            }
            Graphics.DrawLine(Position, l.link.Position, c * 0.2f, 1f, 0.9f);
            float length = (l.link.Position - Position).Length();
            Vec2 dir = (l.link.Position - Position).Normalized;
            Vec2 dirLeft = dir;
            Vec2 dirRight = dir;
            dirLeft = -dirLeft.Rotate(1f, Vec2.Zero);
            dirRight = -dirRight.Rotate(-1f, Vec2.Zero);
            Vec2 vec = Position + dir * (length / 1.5f);
            Graphics.DrawLine(vec, vec + dirLeft * 4f, c * 0.2f, 1f, 0.9f);
            Graphics.DrawLine(vec, vec + dirRight * 4f, c * 0.2f, 1f, 0.9f);
        }
        base.Draw();
    }
}
